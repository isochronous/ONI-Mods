using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace VentFreezeFix.Tests
{
	/// <summary>
	/// "Repair tiles after loading": the watcher's verdict on a natural tile in the vent cell or the
	/// landing cell during its first second. A load cannot be made to form the tile at the right
	/// instant, so each test puts the tile in the cell and gives the watcher the snapshot of the
	/// tick before that a load would have restored from the save.
	/// </summary>
	public static class Repair
	{
		private const float Cold = 253.15f;
		/// <summary>For the small tiles a test expects to stay: 1.5 kg of ice at Cold melts within seconds in a warm room.
		/// The watcher's decision does not depend on temperature, only on what the cell held before.</summary>
		private const float DeepCold = 100f;

		[OniTest("VFF: a tile out of a small packet in the landing cell under the vent is dug up and left as debris at full mass", Timeout = 40), Layer(0)]
		public static IEnumerator TileInLandingCell(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 6);
			int floor = r.Cell(2, 0);
			Vents.Put(floor, SimHashes.Granite, 800f, Cold);
			int ventCell = r.Cell(2, 4);
			int landing = r.Cell(2, 1);
			GameObject vent = Vents.Build(t, ventCell);
			yield return Vents.WindowExpires(t);
			Vents.Put(landing, SimHashes.Ice, 1.2f, Cold);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(Grid.Solid[landing], "the puddle froze into a tile above the floor: " + Grid.Element[landing].id + " " + Grid.Mass[landing] + " " + Grid.Element[floor].id);

			Vents.Rearm(Vents.Watcher(vent), (landing, SimHashes.Water, 1.2f));
			yield return t.WaitGameSeconds(3);
			t.Assert.False(Grid.Solid[landing], "the tile is gone (" + Grid.Element[landing].id + ")");
			float debris = Vents.Debris(landing, SimHashes.Ice);
			t.Assert.True(Mathf.Abs(debris - 1.2f) < 0.1f, "the whole 1.2 kg lies there as ice debris (" + debris + " kg)");
			t.Assert.True(Grid.Solid[floor], "the floor under it is untouched");
		}

		[OniTest("VFF: a tile from a real pool, an old tile, a tile with no history and a built tile are all left alone", Timeout = 60), Layer(0)]
		public static IEnumerator LeavesOtherTiles(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(14, 5);
			int pool = r.Cell(1, 3), old = r.Cell(4, 3), fresh = r.Cell(7, 3), built = r.Cell(10, 3);
			GameObject poolVent = Vents.Build(t, pool + Grid.WidthInCells);
			GameObject oldVent = Vents.Build(t, old + Grid.WidthInCells);
			GameObject freshVent = Vents.Build(t, fresh + Grid.WidthInCells);
			GameObject builtVent = Vents.Build(t, built + Grid.WidthInCells);
			yield return Vents.WindowExpires(t);

			Vents.Put(pool, SimHashes.Ice, 900f, Cold);
			Vents.Put(old, SimHashes.Ice, 1.5f, DeepCold);
			Vents.Put(fresh, SimHashes.Ice, 1.5f, DeepCold);
			yield return t.WaitGameSeconds(1);
			// Each vent sits on top of its tile, so its landing cell is its own; the snapshot names the cell under it, which the watcher still checks.
			t.World.Build("Tile", built);
			yield return t.WaitGameSeconds(1);
			foreach (int cell in new[] { pool, old, fresh })
				t.Assert.True(Grid.Solid[cell], "the tile at " + Cells.Describe(cell) + " formed");

			Vents.Rearm(Vents.Watcher(poolVent), (pool, SimHashes.Water, 900f));
			Vents.Rearm(Vents.Watcher(oldVent), (old, SimHashes.Ice, 1.5f));
			Vents.Rearm(Vents.Watcher(freshVent));
			Vents.Rearm(Vents.Watcher(builtVent), (built, SimHashes.Water, 1.5f));
			yield return t.WaitGameSeconds(3);

			t.Assert.True(Grid.Solid[pool], "a tile out of 900 kg of water (a real pool) stays");
			t.Assert.Eq(0f, Vents.Debris(pool, SimHashes.Ice), "and made no debris");
			t.Assert.True(Grid.Solid[old], "a tile that was already a tile stays");
			t.Assert.True(Grid.Solid[fresh], "a tile with no earlier state stays");
			t.Assert.NotNull(t.World.At(built, ObjectLayer.FoundationTile), "a built tile stays");
			t.Assert.Eq(0f, Vents.Debris(built, SimHashes.Ice) + Vents.Debris(built, SimHashes.Copper), "and was not turned into debris");
		}

		[OniTest("VFF: the watcher stops after its first second: a tile that turns up later is left alone", Timeout = 40), Layer(0)]
		public static IEnumerator WindowCloses(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 6);
			Vents.Put(r.Cell(2, 0), SimHashes.Granite, 800f, Cold);
			int cell = r.Cell(2, 1);
			GameObject vent = Vents.Build(t, r.Cell(2, 4));
			VentFreezeWatcher watcher = Vents.Watcher(vent);
			t.Assert.NotNull(watcher, "a liquid vent carries the watcher");
			yield return Vents.WindowExpires(t);
			t.Assert.True(Vents.Ticks(watcher) > 5, "the watcher ran its five ticks and stopped (" + Vents.Ticks(watcher) + ")");
			int ticksBefore = Vents.Ticks(watcher);

			Vents.Put(cell, SimHashes.Ice, 1.5f, Cold);
			yield return t.WaitGameSeconds(3);
			t.Assert.True(Grid.Solid[cell], "the tile stays");
			t.Assert.Eq(0f, Vents.Debris(cell, SimHashes.Ice), "no debris");
			t.Assert.Eq(ticksBefore, Vents.Ticks(watcher), "and the watcher is not ticking any more");
		}

		[OniTest("VFF: with Repair tiles after loading off, the watcher leaves the tile and stops", Timeout = 40), Layer(0)]
		public static IEnumerator OptionOff(TestContext t)
		{
			t.Speed(3);
			bool original = VentFreezeWatcher.RepairEnabled;
			t.Cleanup(() => VentFreezeWatcher.RepairEnabled = original);
			CellRect r = t.Rect(5, 6);
			Vents.Put(r.Cell(2, 0), SimHashes.Granite, 800f, Cold);
			int cell = r.Cell(2, 1);
			GameObject vent = Vents.Build(t, r.Cell(2, 4));
			yield return Vents.WindowExpires(t);
			Vents.Put(cell, SimHashes.Ice, 1.5f, Cold);
			yield return t.WaitGameSeconds(1);
			VentFreezeWatcher.RepairEnabled = false;
			Vents.Rearm(Vents.Watcher(vent), (cell, SimHashes.Water, 1.5f));
			yield return t.WaitGameSeconds(3);
			t.Assert.True(Grid.Solid[cell], "the tile stays with the option off");
			t.Assert.Eq(0f, Vents.Debris(cell, SimHashes.Ice), "no debris");
		}
	}
}
