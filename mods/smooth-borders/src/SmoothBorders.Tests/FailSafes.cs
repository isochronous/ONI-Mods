using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Isochronous.TestKit;
using ProcGen;
using ProcGenGame;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// The mod's guard against a game update that changes DrawWorldBorder: unless the method reads
	/// each border setting exactly once, as a string load right before GetIntSetting, the transpiler
	/// hands the code back unchanged and turns the mod off (Border.Patched false), so the game draws
	/// its own border. Run on copies of the method's original code; the running game's patch is not
	/// touched, and Border.Patched is put back at cleanup.
	/// </summary>
	public static class FailSafes
	{
		private static MethodBase DrawWorldBorder => AccessTools.Method(typeof(WorldGen), "DrawWorldBorder");

		private static readonly MethodInfo GetIntSetting =
			AccessTools.Method(typeof(WorldGenSettings), nameof(WorldGenSettings.GetIntSetting), new[] { typeof(string) });

		private static readonly MethodInfo FlatSetting = AccessTools.Method(typeof(Border), nameof(Border.Setting));

		/// <summary>One line per instruction, "opcode operand", to compare code before and after.</summary>
		private static List<string> Listing(IEnumerable<CodeInstruction> code)
		{
			return code.Select(c => c.opcode + " " + (c.operand is MemberInfo member ? member.DeclaringType + "::" + member.Name : c.operand)).ToList();
		}

		private static int ThicknessLoad(List<CodeInstruction> code)
		{
			return code.FindIndex(c => c.opcode == OpCodes.Ldstr && c.operand as string == Border.ThicknessKey);
		}

		/// <summary>Runs the transpiler on the code; the code must come back as it went in and the mod must be off.</summary>
		private static void ExpectLeftAlone(TestContext t, List<CodeInstruction> code, string what)
		{
			List<string> before = Listing(code);
			Border.Patched = true;
			List<CodeInstruction> result = WorldGen_DrawWorldBorder_Patch.Transpiler(code).ToList();
			t.Assert.True(!Border.Patched, what + ": the mod turned itself off (Border.Patched false)");
			List<string> after = Listing(result);
			int firstDiff = Enumerable.Range(0, System.Math.Min(before.Count, after.Count)).Where(i => before[i] != after[i]).DefaultIfEmpty(-1).First();
			t.Assert.True(before.Count == after.Count && firstDiff < 0, what + ": the code came back unchanged ("
				+ before.Count + " instructions in, " + after.Count + " out" + (firstDiff >= 0 ? "; first change at " + firstDiff + ": " + before[firstDiff] + " became " + after[firstDiff] : "") + ")");
		}

		[OniTest("SB: when DrawWorldBorder reads the thickness setting twice or not at all, the transpiler leaves its code unchanged and the mod off", NoColony = true, Timeout = 30), Layer(0)]
		public static IEnumerator ChangedMethodLeftAlone(TestContext t)
		{
			bool patched = Border.Patched;
			t.Cleanup(() => Border.Patched = patched);
			t.Assert.NotNull(DrawWorldBorder, "WorldGen.DrawWorldBorder exists");
			t.Assert.NotNull(GetIntSetting, "WorldGenSettings.GetIntSetting(string) exists");
			t.Assert.True(patched, "the transpiler matched when the game patched DrawWorldBorder at load");

			// Control: the method's own code is matched and only the two setting reads change.
			List<CodeInstruction> original = PatchProcessor.GetOriginalInstructions(DrawWorldBorder);
			List<string> originalListing = Listing(original);
			int load = ThicknessLoad(original);
			t.Assert.True(load >= 0 && load + 1 < original.Count && original[load + 1].Calls(GetIntSetting),
				"the original code loads \"" + Border.ThicknessKey + "\" right before GetIntSetting");
			Border.Patched = false;
			List<string> patchedListing = Listing(WorldGen_DrawWorldBorder_Patch.Transpiler(original).ToList());
			t.Assert.True(Border.Patched, "on the original code the transpiler turns the mod on");
			t.Assert.Eq(originalListing.Count, patchedListing.Count, "the transpiler adds and removes no instruction");
			List<int> changed = Enumerable.Range(0, originalListing.Count).Where(i => originalListing[i] != patchedListing[i]).ToList();
			t.Log(original.Count + " instructions; changed: " + string.Join(", ", changed.Select(i => i + " " + originalListing[i] + " -> " + patchedListing[i])));
			t.Assert.Eq(2, changed.Count, "exactly the two setting reads change");
			string flat = Listing(new[] { new CodeInstruction(OpCodes.Call, FlatSetting) })[0];
			t.Assert.True(changed.All(i => patchedListing[i] == flat), "both now call Border.Setting");

			// The thickness read twice: the load and the call duplicated right after the first pair.
			List<CodeInstruction> twice = PatchProcessor.GetOriginalInstructions(DrawWorldBorder);
			load = ThicknessLoad(twice);
			twice.InsertRange(load + 2, new[] { new CodeInstruction(OpCodes.Ldstr, Border.ThicknessKey), new CodeInstruction(twice[load + 1].opcode, twice[load + 1].operand) });
			ExpectLeftAlone(t, twice, "thickness read twice");

			// The thickness read gone: the load and the call removed.
			List<CodeInstruction> gone = PatchProcessor.GetOriginalInstructions(DrawWorldBorder);
			load = ThicknessLoad(gone);
			gone.RemoveRange(load, 2);
			ExpectLeftAlone(t, gone, "thickness read removed");
			yield break;
		}
	}
}
