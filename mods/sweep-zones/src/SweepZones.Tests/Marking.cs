using System.Collections;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace SweepZones.Tests
{
	/// <summary>
	/// What the zones do once painted: once a second the store marks debris in sweep zones for
	/// sweeping and puts mop errands on puddles in mop zones, at the zone's priority. The zones are
	/// set straight in the store here (the painting is covered in <see cref="Painting"/>), so these
	/// check the once-a-second pass on its own.
	/// </summary>
	public static class Marking
	{
		private static bool Marked(GameObject go)
		{
			return go.GetComponent<KPrefabID>().HasTag(GameTags.Garbage);
		}

		[OniTest("Sweep Zones: debris landing in a sweep zone is marked for sweeping at the zone's priority, debris outside is not", Timeout = 60), Layer(0)]
		public static IEnumerator DebrisMarked(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(10, 3);
			Zones.Floor(t, r, 0, 9);
			yield return t.WaitGameSeconds(1);
			foreach (int x in Enumerable.Range(1, 4))
				store.SetSweep(r.Cell(x, 1), Zones.Basic(6));

			GameObject inside = t.Dupes.Drop(SimHashes.SandStone, 40f, r.Cell(2, 1));
			GameObject outside = t.Dupes.Drop(SimHashes.SandStone, 40f, r.Cell(7, 1));
			yield return t.WaitUntil(() => Marked(inside), 6, "the debris in the zone to be marked");
			Zones.Same(t, Zones.Basic(6), inside.GetComponent<Prioritizable>().GetMasterPriority(), "marked at the zone's priority");
			yield return t.WaitGameSeconds(2);
			t.Assert.False(Marked(outside), "debris outside every zone is left alone");
		}

		[OniTest("Sweep Zones: a priority changed by hand sticks, sweeping cancelled by code (not the player) comes back while the zone stands, and erasing the zone keeps errands but stops new ones", Timeout = 90), Layer(0)]
		public static IEnumerator ZoneLifecycle(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(8, 3);
			Zones.Floor(t, r, 0, 7);
			yield return t.WaitGameSeconds(1);
			int cell = r.Cell(3, 1);
			store.SetSweep(cell, Zones.Basic(2));
			GameObject debris = t.Dupes.Drop(SimHashes.SandStone, 40f, cell);
			yield return t.WaitUntil(() => Marked(debris), 6, "the debris to be marked");
			Zones.Same(t, Zones.Basic(2), debris.GetComponent<Prioritizable>().GetMasterPriority(), "at the zone's priority");

			debris.GetComponent<Prioritizable>().SetMasterPriority(Zones.Basic(9));
			yield return t.WaitGameSeconds(3);
			Zones.Same(t, Zones.Basic(9), debris.GetComponent<Prioritizable>().GetMasterPriority(), "a manual adjustment is not overridden");

			debris.GetComponent<Clearable>().CancelClearing();
			t.Assert.False(Marked(debris), "cancelled by code, as the game does when an item is stored");
			yield return t.WaitUntil(() => Marked(debris), 6, "the cancelled debris to be marked again, the zone still standing");

			store.RemoveSweep(cell);
			yield return t.WaitGameSeconds(2);
			t.Assert.True(Marked(debris), "erasing the zone keeps the errand that was made");
			debris.GetComponent<Clearable>().CancelClearing();
			yield return t.WaitGameSeconds(3);
			t.Assert.False(Marked(debris), "and with the zone gone, cancelled debris stays cancelled");
		}

		[OniTest("Sweep Zones: debris the player cancels stays cancelled in a sweep zone until it is marked again", Timeout = 90), Layer(0)]
		public static IEnumerator PlayerCancelSticks(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(8, 3);
			Zones.Floor(t, r, 0, 7);
			yield return t.WaitGameSeconds(1);
			int cell = r.Cell(3, 1);
			store.SetSweep(cell, Zones.Basic(5));
			GameObject debris = t.Dupes.Drop(SimHashes.SandStone, 40f, cell);
			yield return t.WaitUntil(() => Marked(debris), 6, "the debris to be marked");

			// What the Cancel tool does to the first item in a dragged cell.
			Grid.Objects[cell, (int)ObjectLayer.Pickupables].Trigger((int)GameHashes.Cancel);
			t.Assert.False(Marked(debris), "the Cancel tool cancels the sweep");
			yield return t.WaitGameSeconds(3);
			t.Assert.False(Marked(debris), "and it stays cancelled with the zone standing");

			// Marked again by the player (Sweep tool or the item's button), then cancelled with the
			// item's own button.
			debris.GetComponent<Clearable>().MarkForClear();
			t.Assert.True(Marked(debris), "marked again");
			t.Assert.False(debris.GetComponent<KPrefabID>().HasTag(ZoneStore.PlayerCancelled), "marking lifts the player's cancel");
			Traverse.Create(debris.GetComponent<Clearable>()).Method("OnClickCancel").GetValue();
			t.Assert.False(Marked(debris), "the item's Cancel Sweep button cancels it");
			yield return t.WaitGameSeconds(3);
			t.Assert.False(Marked(debris), "and that stays cancelled too");
		}

		[OniTest("Sweep Zones: puddles in a mop zone get mop errands at the zone's priority, by vanilla's rules", Timeout = 90), Layer(0)]
		public static IEnumerator PuddlesMopped(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(11, 3);
			Zones.Floor(t, r, 0, 10);
			// Walls of tile between the cells keep each puddle where it is: cell 1 a puddle, 3 too much
			// water, 5 dry, 7 a puddle in a sweep zone only.
			foreach (int x in new[] { 0, 2, 4, 6, 8, 10 })
				Zones.Pillar(t, r.Cell(x, 1));
			yield return t.WaitGameSeconds(1);
			int puddle = r.Cell(1, 1), heavy = r.Cell(3, 1), dry = r.Cell(5, 1), sweepOnly = r.Cell(7, 1);
			var wet = new[] { puddle, heavy, sweepOnly };
			t.Cleanup(() =>
			{
				foreach (int c in wet)
					SimMessages.ReplaceElement(c, SimHashes.Vacuum, CellEventLogger.Instance.SandBoxTool, 0f, 0f);
			});
			SimMessages.ReplaceElement(puddle, SimHashes.Water, CellEventLogger.Instance.SandBoxTool, 50f, 300f);
			SimMessages.ReplaceElement(heavy, SimHashes.Water, CellEventLogger.Instance.SandBoxTool, MopTool.maxMopAmt + 50f, 300f);
			SimMessages.ReplaceElement(sweepOnly, SimHashes.Water, CellEventLogger.Instance.SandBoxTool, 50f, 300f);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(Grid.Element[puddle].IsLiquid && Grid.Element[heavy].IsLiquid, "the puddles are in the cells");

			store.SetMop(puddle, Zones.Basic(4));
			store.SetMop(heavy, Zones.Basic(4));
			store.SetMop(dry, Zones.Basic(4));
			store.SetSweep(sweepOnly, Zones.Basic(4));
			yield return t.WaitUntil(() => t.World.At(puddle, ObjectLayer.MopPlacer) != null, 6, "a mop errand on the puddle");
			Zones.Same(t, Zones.Basic(4), t.World.At(puddle, ObjectLayer.MopPlacer).GetComponent<Prioritizable>().GetMasterPriority(), "at the zone's priority");
			yield return t.WaitGameSeconds(2);
			t.Assert.Null(t.World.At(heavy, ObjectLayer.MopPlacer), "no errand on more water than the Mop tool takes");
			t.Assert.Null(t.World.At(dry, ObjectLayer.MopPlacer), "none on a dry cell");
			t.Assert.Null(t.World.At(sweepOnly, ObjectLayer.MopPlacer), "none in a sweep-only zone");

			// More passes make no second errand, and a priority changed by hand is kept.
			GameObject placer = t.World.At(puddle, ObjectLayer.MopPlacer);
			placer.GetComponent<Prioritizable>().SetMasterPriority(Zones.Basic(8));
			yield return t.WaitGameSeconds(3);
			t.Assert.True(ReferenceEquals(placer, t.World.At(puddle, ObjectLayer.MopPlacer)), "the same errand is still there");
			Zones.Same(t, Zones.Basic(8), placer.GetComponent<Prioritizable>().GetMasterPriority(), "with the adjusted priority");
			int placers = Object.FindObjectsByType<KPrefabID>(FindObjectsSortMode.None).Count(id => id.PrefabTag == new Tag("MopPlacer"));
			t.Assert.Eq(1, placers, "one mop errand in the world");
		}

		[OniTest("Sweep Zones: the zones are written into the save and read back", Timeout = 30), Layer(0)]
		public static IEnumerator ZonesSerialize(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(8, 3);
			Zones.Floor(t, r, 0, 7);
			yield return t.WaitGameSeconds(1);
			store.SetSweep(r.Cell(1, 1), Zones.Basic(3));
			store.SetSweep(r.Cell(2, 1), Zones.YellowAlert);
			store.SetMop(r.Cell(5, 1), Zones.Basic(8));

			byte[] bytes;
			using (var stream = new System.IO.MemoryStream())
			using (var writer = new System.IO.BinaryWriter(stream))
			{
				KSerialization.Serializer.SerializeTypeless(store, writer);
				writer.Flush();
				bytes = stream.ToArray();
			}
			t.Log("serialized " + bytes.Length + " bytes");
			store.RemoveSweep(r.Cell(1, 1));
			store.RemoveSweep(r.Cell(2, 1));
			store.RemoveMop(r.Cell(5, 1));
			t.Assert.Eq(0, store.SweepCells.Count + store.MopCells.Count, "the store is empty before loading");

			KSerialization.Deserializer.DeserializeTypeless(store, new FastReader(bytes));
			t.Assert.Eq(2, store.SweepCells.Count, "two sweep cells come back");
			t.Assert.Eq(1, store.MopCells.Count, "and one mop cell");
			Zones.Same(t, Zones.Basic(3), store.SweepCells[r.Cell(1, 1)], "with their priorities");
			Zones.Same(t, Zones.YellowAlert, store.SweepCells[r.Cell(2, 1)], "yellow alert included");
			Zones.Same(t, Zones.Basic(8), store.MopCells[r.Cell(5, 1)], "and the mop cell's");
		}
	}
}
