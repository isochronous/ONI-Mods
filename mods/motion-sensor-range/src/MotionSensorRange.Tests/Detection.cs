using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace MotionSensorRange.Tests
{
	/// <summary>A Duplicant walking about in a corridor, and what sensors of different ranges make of it.</summary>
	public static class Detection
	{
		private static bool On(GameObject sensor) => sensor.GetComponent<Switch>().IsSwitchedOn;

		[OniTest("MSR: the range decides how far from the sensor a Duplicant switches it on", Timeout = 90, Boxed = true), Layer(1)]
		public static IEnumerator DuplicantAtDistance(TestContext t)
		{
			CellRect room = t.Dupes.Box(t.Rect(25, 4));
			yield return t.WaitGameSeconds(1.5);
			// Two sensors at the left end of the corridor, side by side; the first keeps the vanilla range, the second is widened.
			GameObject narrow = t.World.Build(LogicDuplicantSensorConfig.ID, room.Cell(2, 0));
			GameObject wide = t.World.Build(LogicDuplicantSensorConfig.ID, room.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			wide.GetComponent<RangeSlider>().SetSliderValue(16f, 0);
			narrow.GetComponent<RangeSlider>().SetSliderValue(4f, 0);
			yield return t.WaitGameSeconds(0.5);
			MinionIdentity dupe = t.Dupes.Spawn(1, room.Cell(10, 0))[0];
			yield return t.WaitGameSeconds(2.5);

			Grid.CellToXY(Grid.PosToCell(dupe), out int dx, out int dy);
			Grid.CellToXY(room.Cell(3, 0), out int wx, out int wy);
			int distanceFromWide = Mathf.Abs(dx - wx);
			int distanceFromNarrow = Mathf.Abs(dx - (wx - 1));
			t.Log("the Duplicant is " + distanceFromNarrow + " tiles from the narrow sensor and " + distanceFromWide + " from the wide one");
			t.Assert.True(distanceFromNarrow > 3 && distanceFromWide <= 8, "the Duplicant stands between the two ranges (" + distanceFromNarrow + ", " + distanceFromWide + ")");
			t.Assert.True(On(wide), "the sensor at range 16 sees a Duplicant " + distanceFromWide + " tiles away");
			t.Assert.False(On(narrow), "the sensor at the vanilla range does not");

			// The same sensor, narrowed: it lets go.
			wide.GetComponent<RangeSlider>().SetSliderValue(2f, 0);
			yield return t.WaitGameSeconds(2);
			Grid.CellToXY(Grid.PosToCell(dupe), out dx, out dy);
			if (Mathf.Abs(dx - wx) > 2)
				t.Assert.False(On(wide), "narrowed to 2, the sensor lets go of a Duplicant " + Mathf.Abs(dx - wx) + " tiles away");

			// And widened again: it sees them again.
			wide.GetComponent<RangeSlider>().SetSliderValue(16f, 0);
			yield return t.WaitGameSeconds(2);
			Grid.CellToXY(Grid.PosToCell(dupe), out dx, out dy);
			if (Mathf.Abs(dx - wx) <= 7)
				t.Assert.True(On(wide), "widened to 16 again, the sensor sees the Duplicant " + Mathf.Abs(dx - wx) + " tiles away");
		}
	}
}
