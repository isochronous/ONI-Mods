using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProcGen;
using ProcGenGame;
using UnityEngine;

namespace SmoothBorders
{
	/// <summary>
	/// Draws the world border flat. DrawWorldBorder reads WorldBorderThickness (T) and
	/// WorldBorderRange (R) once and walks every edge between T-R and T+R; the transpiler swaps
	/// those two reads for <see cref="Border.Setting"/>, which answers the configured thickness
	/// and range 0 while the world is drawn flat. Everything else in the method stays the game's.
	///
	/// The method also adds one POI rectangle per edge, as deep as the deepest cell it drew there;
	/// templates, geysers and story traits keep out of them. The prefix runs the untouched method
	/// on a copy of the cells to get the game's own rectangles, and the postfix keeps, per edge,
	/// the larger of the game's and the flat one: at or below the game's thickness the list is the
	/// game's (same placements as the seed without the mod), and a thicker wall still keeps
	/// templates off it.
	/// </summary>
	[HarmonyPatch(typeof(WorldGen), "DrawWorldBorder")]
	public static class WorldGen_DrawWorldBorder_Patch
	{
		private const int EdgeCount = 4;
		private static readonly string[] EdgeNames = { "left", "right", "bottom", "top" };

		private static readonly MethodInfo GetIntSetting =
			AccessTools.Method(typeof(WorldGenSettings), nameof(WorldGenSettings.GetIntSetting), new[] { typeof(string) });
		private static readonly MethodInfo FlatSetting =
			AccessTools.Method(typeof(Border), nameof(Border.Setting));

		private static readonly WorldGen.OfflineCallbackFunction NoProgress = (key, percent, stage) => true;

		/// <summary>What the prefix hands the postfix for one world.</summary>
		public sealed class State
		{
			public string World;
			public string Group;
			public int Thickness;
			/// <summary>Count of POIBounds before the original ran.</summary>
			public int Before;
			/// <summary>The game's four rectangles: left, right, bottom, top.</summary>
			public List<RectInt> Vanilla;
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> code = new List<CodeInstruction>(instructions);
			Border.Patched = false;
			int thickness = FindRead(code, Border.ThicknessKey);
			int range = FindRead(code, Border.RangeKey);
			if (GetIntSetting == null || FlatSetting == null || thickness < 0 || range < 0)
			{
				Debug.LogWarning("[SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them");
				return code;
			}
			// Same stack shape: (WorldGenSettings, string) in, int out. Labels and blocks stay on the instruction.
			code[thickness].opcode = OpCodes.Call;
			code[thickness].operand = FlatSetting;
			code[range].opcode = OpCodes.Call;
			code[range].operand = FlatSetting;
			Border.Patched = true;
			return code;
		}

		/// <summary>
		/// The index of the one GetIntSetting call whose argument is the string <paramref name="key"/>
		/// loaded right before it, or -1 when the string is loaded anywhere else too, more than once,
		/// or not at all.
		/// </summary>
		private static int FindRead(List<CodeInstruction> code, string key)
		{
			int found = -1;
			int loads = 0;
			for (int i = 0; i < code.Count; i++)
			{
				if (code[i].opcode != OpCodes.Ldstr || !(code[i].operand is string text) || text != key)
					continue;
				loads++;
				if (i + 1 < code.Count && GetIntSetting != null && code[i + 1].Calls(GetIntSetting))
					found = i + 1;
			}
			return loads == 1 ? found : -1;
		}

		public static void Prefix(WorldGen __instance, ref WorldgenSimData simData, Chunk world, out State __state)
		{
			__state = null;
			Border.End();
			if (!Border.Patched)
				return;
			List<RectInt> bounds = __instance.POIBounds;
			int before = bounds != null ? bounds.Count : 0;
			try
			{
				__state = Probe(__instance, ref simData, world);
			}
			catch (Exception e)
			{
				// The world gets the game's border: never flat without the game's rectangles to merge with.
				Border.End();
				__state = null;
				if (bounds != null && bounds.Count > before)
					bounds.RemoveRange(before, bounds.Count - before);
				Debug.LogWarning("[SmoothBorders] Flat border skipped, this world keeps the game's border: " + e.Message);
			}
		}

		private static State Probe(WorldGen worldGen, ref WorldgenSimData simData, Chunk world)
		{
			WorldGenSettings settings = worldGen.Settings;
			string name = settings.world?.filePath ?? "world";
			int t = settings.GetIntSetting(Border.ThicknessKey);
			int r = settings.GetIntSetting(Border.RangeKey);
			if (t <= 0)
			{
				Debug.Log("[SmoothBorders] " + name + ": no wall in the game, left as it is");
				return null;
			}
			Options options = Options.Load();
			Group group = Groups.ForWorld(t, r);
			int tiles = Groups.Thickness(t, r, options);

			// The game's own draw on a copy of the cells, with the same fresh random the game passes,
			// adds the game's four rectangles; they are taken out again and kept for the postfix.
			List<RectInt> bounds = worldGen.POIBounds;
			int before = bounds.Count;
			WorldgenSimData copy = simData;
			copy.cells = (Sim.Cell[])simData.cells.Clone();
			HashSet<int> throwawayCells = new HashSet<int>();
			List<RectInt> poiBounds = bounds;
			WorldGen_DrawWorldBorder_Vanilla.DrawWorldBorder(worldGen, ref copy, world, new SeededRandom(0),
				ref throwawayCells, ref poiBounds, NoProgress);
			int added = bounds.Count - before;
			if (added != EdgeCount)
			{
				if (added > 0)
					bounds.RemoveRange(before, added);
				Debug.LogWarning("[SmoothBorders] Flat border skipped, the game's draw of " + name + " added " + added
					+ " POI rectangles instead of " + EdgeCount + "; this world keeps the game's border");
				return null;
			}
			List<RectInt> vanilla = bounds.GetRange(before, EdgeCount);
			bounds.RemoveRange(before, EdgeCount);

			Border.BeginFlat(tiles);
			return new State
			{
				World = name,
				Group = group != null ? group.Id : "own (" + t + ", " + r + ")",
				Thickness = tiles,
				Before = before,
				Vanilla = vanilla
			};
		}

		public static void Postfix(WorldGen __instance, State __state)
		{
			Border.End();
			if (__state == null)
				return;
			try
			{
				Merge(__instance.POIBounds, __state);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[SmoothBorders] POI rectangle merge skipped for " + __state.World + ": " + e.Message);
			}
		}

		/// <summary>
		/// Per edge, the larger of the game's rectangle and the flat one. All four span their whole
		/// edge, so "larger" is the width (left, right) or the height (bottom, top). A tie keeps the
		/// game's rectangle, so the list is exactly the game's when the flat wall is no deeper.
		/// </summary>
		private static void Merge(List<RectInt> bounds, State state)
		{
			int added = bounds.Count - state.Before;
			if (added != EdgeCount)
			{
				Debug.LogWarning("[SmoothBorders] " + state.World + ": DrawWorldBorder added " + added
					+ " POI rectangles instead of " + EdgeCount + "; left as they are");
				return;
			}
			Debug.Log("[SmoothBorders] " + state.World + ": group " + state.Group + ", flat wall " + state.Thickness + " tiles");
			for (int edge = 0; edge < EdgeCount; edge++)
			{
				int index = state.Before + edge;
				RectInt flatRect = bounds[index];
				RectInt vanillaRect = state.Vanilla[edge];
				int flatDepth = edge < 2 ? flatRect.width : flatRect.height;
				int vanillaDepth = edge < 2 ? vanillaRect.width : vanillaRect.height;
				if (flatDepth > vanillaDepth)
					Debug.Log("[SmoothBorders] " + state.World + ": " + EdgeNames[edge] + " POI rectangle grew from "
						+ vanillaDepth + " to " + flatDepth + " tiles");
				else
					bounds[index] = vanillaRect;
			}
		}
	}
}
