using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CornerBuildAnything
{
	/// <summary>
	/// Where a duplicant may stand to work on a building is a table of reach offsets. The game
	/// keeps two: the standard one, in which a diagonal cell can be reached only by way of one of
	/// its two orthogonal neighbours, and one "with corners", in which the diagonal cells are
	/// reachable outright. Construction and deconstruction pick the corner table for tiles and
	/// the standard one for everything else; these transpilers make both pick the corner table.
	///
	/// Multi-cell buildings are covered by the same swap: the game builds their reach table from
	/// every cell of the footprint, taking the rows of whichever footprint cell is nearest the
	/// standing cell, so a bridge is reachable through a corner from either end as well as from
	/// its middle. Rotation, each building's own offset filter, and the delivery storage that
	/// shares the construction table are left exactly as the game sets them.
	/// </summary>
	internal static class ReachTablePatches
	{
		private static readonly FieldInfo Standard =
			AccessTools.Field(typeof(OffsetGroups), nameof(OffsetGroups.InvertedStandardTable));
		private static readonly FieldInfo WithCorners =
			AccessTools.Field(typeof(OffsetGroups), nameof(OffsetGroups.InvertedStandardTableWithCorners));

		[HarmonyPatch(typeof(Constructable), "OnSpawn")]
		internal static class Constructable_OnSpawn
		{
			internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
			{
				return SwapTable(instructions, original);
			}
		}

		[HarmonyPatch(typeof(Deconstructable), "OnSpawn")]
		internal static class Deconstructable_OnSpawn
		{
			internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
			{
				return SwapTable(instructions, original);
			}
		}

		/// <summary>Every load of the standard table becomes a load of the corner table. If the method
		/// no longer loads the standard table (a game update moved the choice elsewhere), it is left
		/// unchanged and a warning says so, rather than failing the patch.</summary>
		internal static IEnumerable<CodeInstruction> SwapTable(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			var code = new List<CodeInstruction>(instructions);
			int swapped = 0;
			foreach (CodeInstruction instruction in code)
			{
				if (instruction.opcode == OpCodes.Ldsfld && Equals(instruction.operand, Standard))
				{
					instruction.operand = WithCorners;
					swapped++;
				}
			}
			string where = original.DeclaringType.Name + "." + original.Name;
			if (swapped == 0)
				Debug.LogWarning("[CornerBuildAnything] " + where + " does not load OffsetGroups.InvertedStandardTable; corner reach not applied there");
			else
				Debug.Log("[CornerBuildAnything] " + where + ": corner reach table in place");
			return code;
		}
	}
}
