using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace CornerBuildAnything.Tests
{
	/// <summary>
	/// Where a Duplicant may stand to build or take down a building, asked of the game's own
	/// workables: the order's Constructable, its delivery Storage, and the finished building's
	/// Deconstructable. Each case is a corner pocket (see <see cref="Pockets"/>) where only the
	/// corner table gives a standing place, and each is checked against the same question put to the
	/// game's standard table, which must say no, so the case could not pass without the mod.
	/// </summary>
	public static class Reach
	{
		[OniTest("Corner Build Anything: both OnSpawn methods load the corner table and no longer the standard one", Timeout = 20), Layer(0)]
		public static IEnumerator PatchesInPlace(TestContext t)
		{
			foreach (var method in new[] { AccessTools.Method(typeof(Constructable), "OnSpawn"), AccessTools.Method(typeof(Deconstructable), "OnSpawn") })
			{
				var code = PatchProcessor.GetCurrentInstructions(method).ToList();
				int standard = code.Count(c => c.opcode == System.Reflection.Emit.OpCodes.Ldsfld && Equals(c.operand, AccessTools.Field(typeof(OffsetGroups), nameof(OffsetGroups.InvertedStandardTable))));
				int corners = code.Count(c => c.opcode == System.Reflection.Emit.OpCodes.Ldsfld && Equals(c.operand, AccessTools.Field(typeof(OffsetGroups), nameof(OffsetGroups.InvertedStandardTableWithCorners))));
				t.Log(method.DeclaringType.Name + ".OnSpawn loads the standard table " + standard + " time(s) and the corner table " + corners + " time(s)");
				t.Assert.Eq(0, standard, method.DeclaringType.Name + ".OnSpawn no longer loads the standard table");
				t.Assert.True(corners >= 2, method.DeclaringType.Name + ".OnSpawn loads the corner table where it loaded either (once for the tile case, once for the rest)");
				var info = Harmony.GetPatchInfo(method);
				t.Assert.True(info != null && info.Transpilers.Any(p => p.owner.Contains("CornerBuildAnything") || p.PatchMethod.DeclaringType.Namespace == "CornerBuildAnything"),
					"the mod's transpiler is on " + method.DeclaringType.Name + ".OnSpawn");
			}
			yield break;
		}

		/// <summary>
		/// The base game gives diagonal reach to tile pieces only (a building whose def has a TileLayer:
		/// tiles, ladders, wires, pipes, doors) and the standard table to everything else, so each case
		/// below is a def that is not a tile piece. Order, delivery storage and the finished building's
		/// Deconstructable are asked where a Duplicant may stand; the game's standard table, the one it
		/// uses for that def, is asked too and must refuse the corner.
		/// </summary>
		private static IEnumerator Pocketed(TestContext t, string id, int w, int h)
		{
			t.InstantBuild(false);
			CellRect r = t.Rect(20, 8);
			BuildingDef def = Assets.GetBuildingDef(id);
			t.Assert.False(def.IsTilePiece, id + " is not a tile piece, so the base game gives it no diagonal reach");
			int[] x0 = { 3, 12 };
			var corners = new int[2];
			for (int i = 0; i < 2; i++)
				corners[i] = Pockets.BuildRing(t, r, x0[i], 2, w, h, +1, stair: false);
			yield return Pockets.Settle(t);

			int origin = r.Cell(x0[0], 2);
			CellOffset corner = Pockets.Offset(origin, corners[0]);
			GameObject order = Pockets.Order(t, id, origin);
			yield return t.Frames(3);
			Constructable constructable = order.GetComponent<Constructable>();
			Pockets.Has(t, corner, constructable.GetOffsets(origin), "a Duplicant may stand in the corner to build the " + id);
			Pockets.Lacks(t, corner, Pockets.StandardOffsets(constructable, def, Orientation.Neutral, origin, true), "the base game's table does not reach the " + id + " from there");
			Pockets.Has(t, corner, order.GetComponent<Storage>().GetOffsets(origin), "and may deliver its material from there");

			int built = r.Cell(x0[1], 2);
			CellOffset cornerBuilt = Pockets.Offset(built, corners[1]);
			t.World.Build(id, built);
			yield return t.WaitGameSeconds(1);
			Deconstructable deconstructable = Pockets.Finished(built, def).GetComponent<Deconstructable>();
			Pockets.Has(t, cornerBuilt, deconstructable.GetOffsets(built), "a Duplicant may stand in the corner to take the built " + id + " down");
			Pockets.Lacks(t, cornerBuilt, Pockets.StandardOffsets(deconstructable, def, Orientation.Neutral, built, false), "the base game's table would leave it stuck");
		}

		[OniTest("Corner Build Anything: a one-cell building that is not a tile piece (automation switch) is reachable from a corner", Timeout = 40), Layer(0)]
		public static IEnumerator OneCellBuilding(TestContext t)
		{
			return Pocketed(t, "LogicSwitch", 1, 1);
		}

		[OniTest("Corner Build Anything: a multi-cell building (2x2 hand generator) is reachable from a corner of its footprint", Timeout = 40), Layer(0)]
		public static IEnumerator MultiCellBuilding(TestContext t)
		{
			return Pocketed(t, "ManualGenerator", 2, 2);
		}

		[OniTest("Corner Build Anything: (control) a ladder, a tile piece, was reachable from a corner without the mod and still is", Timeout = 40), Layer(0)]
		public static IEnumerator LadderControl(TestContext t)
		{
			t.InstantBuild(false);
			CellRect r = t.Rect(8, 6);
			int corner = Pockets.BuildRing(t, r, 3, 2, 1, 1, +1, stair: false);
			yield return Pockets.Settle(t);
			BuildingDef def = Assets.GetBuildingDef("Ladder");
			t.Assert.True(def.IsTilePiece, "a ladder is a tile piece");
			int origin = r.Cell(3, 2);
			GameObject order = Pockets.Order(t, "Ladder", origin);
			yield return t.Frames(3);
			Constructable constructable = order.GetComponent<Constructable>();
			CellOffset[] now = constructable.GetOffsets(origin);
			Pockets.Has(t, Pockets.Offset(origin, corner), now, "the corner reaches the ladder");
			// What the game itself would do for a tile piece: the corner table, with the def's filter.
			var table = OffsetGroups.BuildReachabilityTable(def.PlacementOffsets, OffsetGroups.InvertedStandardTableWithCorners, def.ConstructionOffsetFilter);
			var tracker = new OffsetTableTracker(table, constructable);
			CellOffset[] game;
			try
			{
				game = tracker.GetOffsets(origin).ToArray();
			}
			finally
			{
				tracker.Clear();
			}
			t.Assert.Eq(Pockets.Describe(game), Pockets.Describe(now), "the ladder's reach is exactly the corner table's, as in the base game");
		}

		[OniTest("Corner Build Anything: a bridge with only its end in the pocket is reachable from the corner", Timeout = 40), Layer(0)]
		public static IEnumerator BridgeEndInPocket(TestContext t)
		{
			t.InstantBuild(false);
			CellRect r = t.Rect(20, 5);
			// The pocket holds the bridge's right end; its middle and left end are open cells.
			Pockets.Build(t, r, 6, 2, +1, far: false, stair: false);
			Pockets.Build(t, r, 16, 2, +1, far: false, stair: false);
			yield return Pockets.Settle(t);
			BuildingDef def = Assets.GetBuildingDef("LogicWireBridge");

			int middle = r.Cell(5, 2);
			CellOffset corner = Pockets.Offset(middle, Pockets.Corner(r, 6, 2, +1));
			GameObject order = Pockets.Order(t, "LogicWireBridge", middle);
			yield return t.Frames(3);
			Constructable constructable = order.GetComponent<Constructable>();
			CellOffset[] build = constructable.GetOffsets(middle);
			t.Log("build offsets " + Pockets.Describe(build));
			Pockets.Has(t, corner, build, "a Duplicant may stand in the corner to build the bridge from its end");
			Pockets.Lacks(t, corner, Pockets.StandardOffsets(constructable, def, Orientation.Neutral, middle, true), "the standard table fails for a bridge whose end alone is in the pocket");
			Pockets.Has(t, corner, order.GetComponent<Storage>().GetOffsets(middle), "and to deliver its material");

			int built = r.Cell(15, 2);
			t.World.Build("LogicWireBridge", built);
			yield return t.WaitGameSeconds(1);
			Deconstructable deconstructable = Pockets.Finished(built, def).GetComponent<Deconstructable>();
			CellOffset cornerBuilt = Pockets.Offset(built, Pockets.Corner(r, 16, 2, +1));
			Pockets.Has(t, cornerBuilt, deconstructable.GetOffsets(built), "a Duplicant may stand in the corner to take the built bridge down from its end");
			Pockets.Lacks(t, cornerBuilt, Pockets.StandardOffsets(deconstructable, def, Orientation.Neutral, built, false), "the standard table would have left it stuck");
		}

		[OniTest("Corner Build Anything: a bridge turned upright is reachable through the corner at its upper end", Timeout = 40), Layer(0)]
		public static IEnumerator RotatedBridge(TestContext t)
		{
			t.InstantBuild(false);
			CellRect r = t.Rect(8, 8);
			// Upright bridge on column 3, rows 3-5; its upper end (3, 5) is walled left, right and above,
			// and the open cell diagonally above-right, (4, 6), stands on the tile at (4, 5).
			int middle = r.Cell(3, 4);
			Pockets.Tile(t, r.Cell(2, 5));
			Pockets.Tile(t, r.Cell(4, 5));
			Pockets.Tile(t, r.Cell(3, 6));
			yield return Pockets.Settle(t);
			BuildingDef def = Assets.GetBuildingDef("LogicWireBridge");
			CellOffset corner = Pockets.Offset(middle, r.Cell(4, 6));

			GameObject order = Pockets.Order(t, "LogicWireBridge", middle, Orientation.R90);
			yield return t.Frames(3);
			Constructable constructable = order.GetComponent<Constructable>();
			CellOffset[] build = constructable.GetOffsets(middle);
			t.Log("build offsets " + Pockets.Describe(build));
			Pockets.Has(t, corner, build, "a Duplicant may stand in the corner above the upper end");
			Pockets.Lacks(t, corner, Pockets.StandardOffsets(constructable, def, Orientation.R90, middle, true), "the standard table would not have reached it");
		}

		[OniTest("Corner Build Anything: (control) a pressure plate's own offset filter still keeps Duplicants off the cell it forbids", Timeout = 40), Layer(0)]
		public static IEnumerator OwnFilterKept(TestContext t)
		{
			t.InstantBuild(false);
			CellRect r = t.Rect(7, 5);
			// A pressure plate (filter: never from the cell directly below) in mid-air at (3, 2); the cell
			// below it, (3, 1), is a place to stand, and so is (2, 2) on the tile at (2, 1).
			Pockets.Tile(t, r.Cell(3, 0));
			Pockets.Tile(t, r.Cell(2, 1));
			yield return Pockets.Settle(t);
			BuildingDef def = Assets.GetBuildingDef("FloorSwitch");
			t.Assert.NotNull(def.ConstructionOffsetFilter, "the pressure plate carries an offset filter");
			int cell = r.Cell(3, 2);
			var below = new CellOffset(0, -1);
			var left = new CellOffset(-1, 0);

			GameObject order = Pockets.Order(t, "FloorSwitch", cell);
			yield return t.Frames(3);
			Constructable constructable = order.GetComponent<Constructable>();
			var unfiltered = OffsetGroups.BuildReachabilityTable(def.PlacementOffsets, OffsetGroups.InvertedStandardTableWithCorners, null);
			var probe = new OffsetTableTracker(unfiltered, constructable);
			CellOffset[] open;
			try
			{
				open = probe.GetOffsets(cell).ToArray();
			}
			finally
			{
				probe.Clear();
			}
			Pockets.Has(t, below, open, "the cell below is a place to stand when no filter is applied");
			CellOffset[] build = constructable.GetOffsets(cell);
			t.Log("build offsets " + Pockets.Describe(build));
			Pockets.Lacks(t, below, build, "but the plate's filter still forbids it");
			Pockets.Has(t, left, build, "the cell beside it, on a tile, is still allowed");
		}

		[OniTest("Corner Build Anything: the corner table reaches every standing place the standard table does, by the same or a shorter row", Timeout = 20), Layer(0)]
		public static IEnumerator CornerTableIsSuperset(TestContext t)
		{
			bool shorter = false;
			foreach (string id in new[] { "LogicSwitch", "LogicWireBridge", "ManualGenerator", "Refrigerator", "Generator" })
			{
				BuildingDef def = Assets.GetBuildingDef(id);
				var standard = Rows(OffsetGroups.BuildReachabilityTable(def.PlacementOffsets, OffsetGroups.InvertedStandardTable, def.ConstructionOffsetFilter));
				var corners = Rows(OffsetGroups.BuildReachabilityTable(def.PlacementOffsets, OffsetGroups.InvertedStandardTableWithCorners, def.ConstructionOffsetFilter));
				t.Log(id + ": " + standard.Count + " places in the standard table, " + corners.Count + " with corners");
				foreach (var place in standard)
				{
					t.Assert.True(corners.TryGetValue(place.Key, out int length), id + ": the corner table reaches " + Pockets.Describe(new[] { place.Key }));
					t.Assert.True(length <= place.Value, id + ": the corner row to " + Pockets.Describe(new[] { place.Key }) + " is no longer than the standard one");
					if (length < place.Value)
						shorter = true;
				}
			}
			t.Assert.True(shorter, "some place needs fewer open cells on the way with the corner table");
			yield break;
		}

		/// <summary>For each standing place, the length of its shortest row (1: no open cell needed on the way).</summary>
		private static Dictionary<CellOffset, int> Rows(CellOffset[][] table)
		{
			var shortest = new Dictionary<CellOffset, int>();
			foreach (CellOffset[] row in table)
				if (!shortest.TryGetValue(row[0], out int known) || row.Length < known)
					shortest[row[0]] = row.Length;
			return shortest;
		}

		[OniTest("Corner Build Anything: what may be placed where is unchanged", Timeout = 30), Layer(0)]
		public static IEnumerator PlacementUnchanged(TestContext t)
		{
			CellRect r = t.Rect(10, 5);
			Pockets.Build(t, r, 3, 2, +1, far: true, stair: false);
			yield return Pockets.Settle(t);
			BuildingDef ladder = Assets.GetBuildingDef("Ladder");
			BuildingDef generator = Assets.GetBuildingDef("ManualGenerator");
			string reason;
			t.Assert.True(ladder.IsValidBuildLocation(null, r.Cell(3, 2), Orientation.Neutral, false, out reason), "a ladder fits the pocket, as before: " + reason);
			t.Assert.False(ladder.IsValidPlaceLocation(null, r.Cell(3, 1), Orientation.Neutral, out reason), "a ladder does not go into a tile");
			t.Log("refusal: " + reason);
			t.Assert.False(generator.IsValidBuildLocation(null, r.Cell(8, 3), Orientation.Neutral, false, out reason), "a generator that needs a floor is refused in mid-air");
			t.Log("refusal: " + reason);
		}
	}
}
