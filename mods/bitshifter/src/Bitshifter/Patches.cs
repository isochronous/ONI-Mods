using HarmonyLib;
using UnityEngine;

namespace Bitshifter
{
	// The bit selector side screen builds one row per GetBitDepth(), and both buildings
	// already shift by selectedBit with no upper bound, so widening the depth is the whole
	// change. The reader also passes the shifted value straight through to a ribbon output,
	// which is what makes shift chains work.
	[HarmonyPatch(typeof(LogicRibbonReader), nameof(LogicRibbonReader.GetBitDepth))]
	internal static class LogicRibbonReader_GetBitDepth_Patch
	{
		private static void Postfix(ref int __result)
		{
			__result = BitshifterMod.Bits;
		}
	}

	[HarmonyPatch(typeof(LogicRibbonWriter), nameof(LogicRibbonWriter.GetBitDepth))]
	internal static class LogicRibbonWriter_GetBitDepth_Patch
	{
		private static void Postfix(ref int __result)
		{
			__result = BitshifterMod.Bits;
		}
	}

	/// <summary>
	/// The buildings' own kanims (anim/assets): the vanilla art with the four bit lights replaced
	/// by a two-digit seven-segment display of the selected bit and one light for that bit's state.
	/// The poses net0, net1, net4 and net5 are the vanilla "&lt;ports&gt;_&lt;bit&gt;" poses without the
	/// bit lights; the number counts the wired ports the way the vanilla code does (reader: input 4,
	/// output 1; writer: input 1, output 4), which picks the port lights' art. Each segment is a
	/// symbol, seg_a_1 .. seg_g_2, shown or hidden per digit; signal_bloom is the bit's light, tinted
	/// with the vanilla on and off colours like the port lights.
	/// </summary>
	internal static class Display
	{
		public const string ReaderAnim = "bitshifter_ribbon_reader_kanim";
		public const string WriterAnim = "bitshifter_ribbon_writer_kanim";

		public static readonly KAnimHashedString Signal = "signal_bloom";
		public static readonly KAnimHashedString OutputLight = "output_light_bloom";
		public static readonly KAnimHashedString InputLight = "input_light_bloom";

		/// <summary>The segments lit for each digit, a to g clockwise from the top with g in the middle.</summary>
		private static readonly string[] Digits =
		{
			"abcdef", "bc", "abdeg", "abcdg", "bcfg", "acdfg", "acdefg", "abc", "abcdefg", "abcdfg",
		};

		private static readonly KAnimHashedString[,] Segments = new KAnimHashedString[2, 7];

		static Display()
		{
			for (int position = 0; position < 2; position++)
				for (int segment = 0; segment < 7; segment++)
					Segments[position, segment] = "seg_" + (char)('a' + segment) + "_" + (position + 1);
		}

		public static void UseAnim(BuildingDef def, string name)
		{
			KAnimFile anim = Assets.GetAnim(name);
			if (anim != null)
				def.AnimFiles = new[] { anim };
			else
				Debug.LogWarning("[Bitshifter] " + name + " is missing; " + def.PrefabID + " keeps the vanilla art");
		}

		/// <summary>The logic network on one of the building's ports, or null when nothing is wired to it.</summary>
		private static LogicCircuitNetwork Network(LogicPorts ports, HashedString port)
		{
			if (ports == null)
				return null;
			int cell = ports.GetPortCell(port);
			return cell < 0 ? null : Game.Instance.logicCircuitManager.GetNetworkForCell(cell);
		}

		/// <summary>
		/// UpdateVisuals for either building. The ribbon port is the reader's input and the writer's
		/// output, the 1-bit port the other one. The pose counts a wired ribbon port as 4 and a wired
		/// 1-bit port as 1, as vanilla does; the 1-bit port's light shows bit 0 of that port's value
		/// and the signal light the selected bit of the ribbon, which shows as its 1-based number.
		/// </summary>
		public static void Update<T>(T building, KBatchedAnimController kbac, HashedString ribbonPort, HashedString bitPort,
			KAnimHashedString bitLight, int bitValue, Color on, Color off) where T : KMonoBehaviour, ILogicRibbonBitSelector
		{
			if (kbac == null)
				return;
			LogicPorts ports = building.GetComponent<LogicPorts>();
			bool ribbonWired = Network(ports, ribbonPort) != null, bitWired = Network(ports, bitPort) != null;
			if (bitWired)
				kbac.SetSymbolTint(bitLight, LogicCircuitNetwork.IsBitActive(0, bitValue) ? on : off);
			kbac.Play("net" + ((ribbonWired ? 4 : 0) + (bitWired ? 1 : 0)));
			int selected = building.GetBitSelection(), number = selected + 1;
			SetDigit(kbac, 0, number >= 10 ? number / 10 : -1);
			SetDigit(kbac, 1, number % 10);
			kbac.SetSymbolTint(Signal, ribbonWired && building.IsBitActive(selected) ? on : off);
		}

		private static void SetDigit(KBatchedAnimController kbac, int position, int digit)
		{
			string lit = digit < 0 ? "" : Digits[digit];
			for (int segment = 0; segment < 7; segment++)
				kbac.SetSymbolVisiblity(Segments[position, segment], lit.IndexOf((char)('a' + segment)) >= 0);
		}
	}

	[HarmonyPatch(typeof(LogicRibbonReaderConfig), nameof(LogicRibbonReaderConfig.CreateBuildingDef))]
	internal static class LogicRibbonReaderConfig_CreateBuildingDef_Patch
	{
		private static void Postfix(BuildingDef __result)
		{
			Display.UseAnim(__result, Display.ReaderAnim);
		}
	}

	[HarmonyPatch(typeof(LogicRibbonWriterConfig), nameof(LogicRibbonWriterConfig.CreateBuildingDef))]
	internal static class LogicRibbonWriterConfig_CreateBuildingDef_Patch
	{
		private static void Postfix(BuildingDef __result)
		{
			Display.UseAnim(__result, Display.WriterAnim);
		}
	}

	// Both buildings end UpdateVisuals with Play("<ports>_<bit+1>") into the vanilla kanim, which
	// has those poses for bits 1-4 only. These replace the method: the same port lights, tinted
	// with the building's own colours, then the display.
	[HarmonyPatch(typeof(LogicRibbonReader), nameof(LogicRibbonReader.UpdateVisuals))]
	internal static class LogicRibbonReader_UpdateVisuals_Patch
	{
		private static readonly AccessTools.FieldRef<LogicRibbonReader, Color> ColorOn = AccessTools.FieldRefAccess<LogicRibbonReader, Color>("colorOn");
		private static readonly AccessTools.FieldRef<LogicRibbonReader, Color> ColorOff = AccessTools.FieldRefAccess<LogicRibbonReader, Color>("colorOff");

		private static bool Prefix(LogicRibbonReader __instance, KBatchedAnimController ___kbac)
		{
			Display.Update(__instance, ___kbac, LogicRibbonReader.INPUT_PORT_ID, LogicRibbonReader.OUTPUT_PORT_ID,
				Display.OutputLight, __instance.GetOutputValue(), ColorOn(__instance), ColorOff(__instance));
			return false;
		}
	}

	[HarmonyPatch(typeof(LogicRibbonWriter), nameof(LogicRibbonWriter.UpdateVisuals))]
	internal static class LogicRibbonWriter_UpdateVisuals_Patch
	{
		private static readonly AccessTools.FieldRef<LogicRibbonWriter, Color> ColorOn = AccessTools.FieldRefAccess<LogicRibbonWriter, Color>("colorOn");
		private static readonly AccessTools.FieldRef<LogicRibbonWriter, Color> ColorOff = AccessTools.FieldRefAccess<LogicRibbonWriter, Color>("colorOff");

		private static bool Prefix(LogicRibbonWriter __instance, KBatchedAnimController ___kbac)
		{
			Display.Update(__instance, ___kbac, LogicRibbonWriter.OUTPUT_PORT_ID, LogicRibbonWriter.INPUT_PORT_ID,
				Display.InputLight, __instance.GetInputValue(), ColorOn(__instance), ColorOff(__instance));
			return false;
		}
	}
}
