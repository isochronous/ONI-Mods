using System.Collections;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace SweepZones.Tests
{
	/// <summary>
	/// Duplicants doing what the zones order: a crew in a sealed box, a zone painted with the real
	/// tool, debris or a puddle put in it. The marking is checked through the API first (a second or
	/// two); the crew is sent only when the order exists and a Duplicant can walk to the cell.
	/// </summary>
	public static class ByHand
	{
		[OniTest("Sweep Zones: Duplicants sweep debris that lands in a painted zone into a storage bin", Timeout = 120, Boxed = true), Layer(1)]
		public static IEnumerator SweepsIntoBin(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			GameObject bin = t.World.Build("StorageLocker", inside.Cell(30, 0));
			// A new bin takes only what its filter allows; let it take sandstone.
			bin.GetComponent<TreeFilterable>().AddTagToFilter(SimHashes.SandStone.CreateTag());
			yield return t.WaitGameSeconds(1);
			var crew = t.Dupes.Spawn(1, inside.Cell(20, 0));
			t.Speed(3);
			yield return t.WaitGameSeconds(2);
			int debrisCell = inside.Cell(8, 0);
			t.Assert.True(crew[0].GetComponent<Navigator>().GetNavigationCost(debrisCell) != -1, "the Duplicant can walk to the debris cell");
			t.Assert.True(crew[0].GetComponent<Navigator>().GetNavigationCost(inside.Cell(30, 0)) != -1, "and to the bin");

			yield return Zones.Open(t);
			yield return Zones.Priority(t, Zones.Basic(5));
			yield return Zones.Drag(t, inside.Cell(5, 0), inside.Cell(12, 0));
			t.Assert.Eq(8, store.SweepCells.Count, "the zone is painted");
			PlayerController.Instance.ActivateTool(SelectTool.Instance);

			GameObject debris = t.Dupes.Drop(SimHashes.SandStone, 30f, debrisCell);
			yield return t.WaitUntil(() => debris.GetComponent<KPrefabID>().HasTag(GameTags.Garbage), 8, "the debris to be marked for sweeping");
			Storage storage = bin.GetComponent<Storage>();
			yield return t.WaitUntil(() => storage.MassStored() >= 29f, 45, "the Duplicant to carry the debris into the bin");
		}

		[OniTest("Sweep Zones: Duplicants mop a puddle that forms in a painted mop zone", Timeout = 120, Boxed = true), Layer(1)]
		public static IEnumerator MopsPuddle(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			var crew = t.Dupes.Spawn(1, inside.Cell(20, 0));
			t.Speed(3);
			yield return t.WaitGameSeconds(2);
			int puddle = inside.Cell(10, 0);
			// Two tiles either side keep the water in its one cell, so the job is one puddle.
			Zones.Pillar(t, inside.Cell(9, 0));
			Zones.Pillar(t, inside.Cell(11, 0));
			yield return t.WaitGameSeconds(1);
			t.Assert.True(crew[0].GetComponent<Navigator>().GetNavigationCost(puddle) != -1, "the Duplicant can walk to the puddle");
			t.Cleanup(() =>
			{
				foreach (int x in Enumerable.Range(0, inside.W))
					if (Grid.Element[inside.Cell(x, 0)].IsLiquid)
						SimMessages.ReplaceElement(inside.Cell(x, 0), SimHashes.Vacuum, CellEventLogger.Instance.SandBoxTool, 0f, 0f);
			});

			yield return Zones.Open(t);
			yield return Zones.Mode(t, ModStrings.FilterMop);
			yield return Zones.Priority(t, Zones.Basic(5));
			yield return Zones.Drag(t, inside.Cell(8, 0), inside.Cell(12, 0));
			t.Assert.Eq(3, store.MopCells.Count, "the mop zone is painted on the three open cells; the two tiles are not ground");
			PlayerController.Instance.ActivateTool(SelectTool.Instance);

			SimMessages.ReplaceElement(puddle, SimHashes.Water, CellEventLogger.Instance.SandBoxTool, 20f, 300f);
			yield return t.WaitUntil(() => t.World.At(puddle, ObjectLayer.MopPlacer) != null, 8, "a mop errand on the puddle");
			Zones.Same(t, Zones.Basic(5), t.World.At(puddle, ObjectLayer.MopPlacer).GetComponent<Prioritizable>().GetMasterPriority(), "at the zone's priority");
			t.Assert.True(crew[0].GetComponent<Navigator>().GetNavigationCost(inside.Cell(8, 0)) != -1, "the Duplicant can walk to the tile's side of the puddle");
			yield return t.WaitUntil(() => t.World.At(puddle, ObjectLayer.MopPlacer) == null && Grid.Mass[puddle] < 1f, 40, "the Duplicant to mop the puddle away");
		}
	}
}
