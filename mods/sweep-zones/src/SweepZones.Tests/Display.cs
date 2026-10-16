using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace SweepZones.Tests
{
	/// <summary>What the tool shows while it is open: cell tints, priority labels, the outline, and nothing in the overlay system.</summary>
	public static class Display
	{
		private static List<ToolMenu.CellColorData> Colors()
		{
			Zones.Tool.GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors);
			return colors.ToList();
		}

		private static int Labels()
		{
			return ((List<System.Guid>)Zones.Field(Zones.Tool, "priorityLabels")).Count;
		}

		[OniTest("Sweep Zones: the tint follows the mode (sweep warm, mop cool), shows only that kind, and grows with priority", Timeout = 60), Layer(1)]
		public static IEnumerator Tints(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(10, 3);
			Zones.Floor(t, r, 0, 9);
			yield return t.WaitGameSeconds(1);
			store.SetSweep(r.Cell(1, 1), Zones.Basic(1));
			store.SetSweep(r.Cell(2, 1), Zones.Basic(9));
			store.SetSweep(r.Cell(3, 1), Zones.YellowAlert);
			store.SetMop(r.Cell(6, 1), Zones.Basic(1));
			store.SetMop(r.Cell(7, 1), Zones.Basic(9));
			yield return Zones.Open(t);

			var sweep = Colors();
			t.Assert.Eq(3, sweep.Count, "sweep mode tints the three sweep cells and no mop cell");
			Color low = sweep.First(c => c.cell == r.Cell(1, 1)).color, high = sweep.First(c => c.cell == r.Cell(2, 1)).color, alert = sweep.First(c => c.cell == r.Cell(3, 1)).color;
			t.Assert.True(low.r >= low.b && high.r >= high.b, "sweep tints are warm (red over blue)");
			t.Assert.NotEq(low, high, "priority 9 tints differently from priority 1");
			t.Assert.True(high.g < low.g, "toward red as the priority rises");
			t.Assert.NotEq(high, alert, "yellow alert has its own colour");

			yield return Zones.Mode(t, ModStrings.FilterMop);
			var mop = Colors();
			t.Assert.Eq(2, mop.Count, "mop mode tints the two mop cells and no sweep cell");
			t.Assert.True(mop.All(c => c.color.b > c.color.r), "mop tints are cool (blue over red)");
			t.Assert.True(mop.Select(c => c.color).Distinct().Count() == 2, "and also differ by priority");

			yield return Zones.Mode(t, ModStrings.FilterClearSweep);
			t.Assert.Eq(3, Colors().Count, "the erase sweep mode shows the sweep zones again");
		}

		[OniTest("Sweep Zones: labels are drawn per same-priority region, merge when painted together and go when the tool closes", Timeout = 60), Layer(1)]
		public static IEnumerator LabelsPerRegion(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(12, 3);
			Zones.Floor(t, r, 0, 11);
			yield return t.WaitGameSeconds(1);
			yield return Zones.Open(t);
			t.Assert.Eq(0, Labels(), "no zones, no labels");
			yield return Zones.Priority(t, Zones.Basic(5));
			yield return Zones.Drag(t, r.Cell(1, 1), r.Cell(3, 1));
			yield return Zones.Drag(t, r.Cell(7, 1), r.Cell(9, 1));
			yield return t.Frames(3);
			t.Assert.Eq(2, Labels(), "two separate regions of one priority: two labels");

			yield return Zones.Drag(t, r.Cell(4, 1), r.Cell(6, 1));
			yield return t.Frames(3);
			t.Assert.Eq(1, Labels(), "painting between them joins them: one label");

			yield return Zones.Priority(t, Zones.Basic(2));
			yield return Zones.Drag(t, r.Cell(5, 1), r.Cell(5, 1));
			yield return t.Frames(3);
			t.Assert.Eq(3, Labels(), "a cell of another priority in the middle splits it: three labels");
			GameObject outline = (GameObject)Zones.Field(Zones.Tool, "outlineObject");
			t.Assert.True(outline != null && outline.activeSelf, "and the zone outline is drawn");

			t.Assert.True(OverlayScreen.Instance.GetMode() == OverlayModes.None.ID, "no overlay was switched on for any of it");
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			yield return t.Frames(3);
			t.Assert.Eq(0, Labels(), "closing the tool removes the labels");
			t.Assert.False(outline.activeSelf, "and the outline");
			t.Assert.Eq(9, store.SweepCells.Count, "the zones themselves stay");
		}
	}
}
