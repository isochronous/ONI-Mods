using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProcGenGame;

namespace NaturalBackwalls
{
	/// <summary>
	/// The offline settle sim (the "Infusing oxygen" stage) applies backwall phase changes with the
	/// code a running game uses, BackwallManager.UpdateFromSim. That code reaches into objects that
	/// only exist once a game is loaded (the cell event logger, debris spawning), so one backwall
	/// melting during the settle throws on the worldgen thread. Nothing catches it there: the thread
	/// dies, the cluster never reports Failed, and the loading screen waits forever. The game never
	/// met this because its only backwalls (the Aquatic pack's) sit in temperate biomes; this mod
	/// puts backwalls next to magma, so it has to handle it. The transpiler swaps the call for
	/// <see cref="SettleSimBackwalls.UpdateFromSim"/>, which applies the same transition without the
	/// game-only parts.
	/// </summary>
	[HarmonyPatch(typeof(WorldGenSimUtil), nameof(WorldGenSimUtil.DoSettleSim))]
	public static class WorldGenSimUtil_DoSettleSim_Patch
	{
		private static readonly MethodInfo GameUpdate = AccessTools.Method(typeof(BackwallManager), nameof(BackwallManager.UpdateFromSim));
		private static readonly MethodInfo SafeUpdate = AccessTools.Method(typeof(SettleSimBackwalls), nameof(SettleSimBackwalls.UpdateFromSim));

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool replaced = false;
			foreach (CodeInstruction instruction in instructions)
			{
				if (!replaced && GameUpdate != null && SafeUpdate != null && instruction.Calls(GameUpdate))
				{
					replaced = true;
					yield return new CodeInstruction(OpCodes.Call, SafeUpdate).WithLabels(instruction.labels).WithBlocks(instruction.blocks);
					continue;
				}
				yield return instruction;
			}
			if (!replaced)
				Debug.LogWarning("[NaturalBackwalls] BackwallManager.UpdateFromSim call not found in the settle sim; a backwall melting during worldgen would still stop it.");
		}

		public static void Postfix()
		{
			SettleSimBackwalls.LogSummary();
		}
	}

	/// <summary>
	/// Worldgen-safe twin of BackwallManager.UpdateFromSim: same thresholds, same results (a solid
	/// result replaces the backwall, a liquid or gas result removes it and is released into the
	/// cell), minus the event log entry and the debris object, which cannot exist before a game.
	/// </summary>
	public static class SettleSimBackwalls
	{
		private const int DetailLines = 3;

		private static int handled;
		private static int detailed;

		public static unsafe void UpdateFromSim(Sim.GameDataUpdate* data)
		{
			MirrorPointers(data);
			int count = data->numBackwallShouldTransitionInfos;
			if (count <= 0)
				return;
			List<Element> elements = ElementLoader.elements;
			ushort vacuum = ElementLoader.GetElementIndex(SimHashes.Vacuum);
			for (int i = 0; i < count; i++)
			{
				int cell = data->backwallShouldTransitionInfos[i].gameCell;
				if (!Grid.IsValidCell(cell))
					continue;
				ushort idx = data->backwallElement[cell];
				if (idx == ushort.MaxValue || idx >= elements.Count)
					continue;
				Element element = elements[idx];
				if (element == null || element.IsVacuum)
					continue;
				float temperature = data->backwallTemperature[cell];
				float mass = data->backwallMass[cell];
				Element target;
				float newTemperature;
				float conversion;
				SimHashes byproductId;
				if (temperature > element.highTemp)
				{
					target = element.highTempTransition;
					newTemperature = temperature - 1.5f;
					conversion = element.highTempTransitionOreMassConversion;
					byproductId = element.highTempTransitionOreID;
				}
				else if (temperature < element.lowTemp)
				{
					target = element.lowTempTransition;
					newTemperature = temperature + 1.5f;
					conversion = element.lowTempTransitionOreMassConversion;
					byproductId = element.lowTempTransitionOreID;
				}
				else
					continue;
				if (target == null)
					continue;
				float newMass = byproductId != (SimHashes)0 ? (1f - conversion) * mass : mass;
				if (target.IsSolid)
					SimMessages.SetBackwallData(cell, target.idx, newMass, newTemperature);
				else
				{
					SimMessages.SetBackwallData(cell, vacuum, 0f, 0f);
					SimMessages.AddRemoveSubstance(cell, target.idx, null, newMass, newTemperature, byte.MaxValue, 0);
				}
				// The game drops the byproduct as debris; there are no game objects during worldgen,
				// so a solid byproduct is lost (a few kilograms) and a fluid one goes into the cell.
				float byproductMass = mass - newMass;
				if (byproductId != (SimHashes)0 && byproductMass > 0.001f)
				{
					Element byproduct = ElementLoader.FindElementByHash(byproductId);
					if (byproduct != null && !byproduct.IsSolid)
						SimMessages.AddRemoveSubstance(cell, byproduct.idx, null, byproductMass, newTemperature, byte.MaxValue, 0);
				}
				handled++;
				if (detailed < DetailLines)
				{
					detailed++;
					Vector2I xy = Grid.CellToXY(cell);
					Debug.Log("[NaturalBackwalls] Settle sim: " + element.id + " backwall at " + xy.x + "," + xy.y + " is " + temperature.ToString("F0")
						+ " K and becomes " + target.id + (target.IsSolid ? "" : " (released into the cell)"));
				}
			}
		}

		/// <summary>
		/// Keeps BackwallManager's view of the sim buffers current, as the game's method does first
		/// thing. The game's method is safe to call when there is nothing to transition (it then only
		/// stores the three buffer pointers), so it gets a copy of the update with a zero count. Its
		/// pointer fields cannot be set through reflection on Mono.
		/// </summary>
		private static unsafe void MirrorPointers(Sim.GameDataUpdate* data)
		{
			Sim.GameDataUpdate copy = *data;
			copy.numBackwallShouldTransitionInfos = 0;
			BackwallManager.UpdateFromSim(&copy);
		}

		public static void LogSummary()
		{
			if (handled > 0)
				Debug.Log("[NaturalBackwalls] Settle sim: " + handled + " backwall phase change(s) applied during world generation");
			handled = 0;
			detailed = 0;
		}
	}
}
