using System.Collections;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace MultichannelCritterSensor.Tests
{
	/// <summary>The sensor's side screen, driven the way a player clicks it.</summary>
	public static class SideScreen
	{
		[OniTest("MCS: the side screen switches mode, kinds and species, and edits a threshold", Timeout = 60), Layer(2)]
		public static IEnumerator Controls(TestContext t)
		{
			CellRect r = t.Rect(2, 1);
			GameObject go = t.World.Build("MultichannelCritterSensor", r.Cell(0, 0));
			var s = go.GetComponent<MultichannelCritterSensor>();
			yield return t.WaitGameSeconds(1);
			MultichannelCritterSensorSideScreen screen = null;
			yield return Kit.Show(t, go, x => screen = x);

			// Mode.
			Kit.Click(Kit.Find<KButton>(screen, "Separate").gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(s.separateThresholds, "Separate switches the mode");
			Kit.Click(Kit.Find<KButton>(screen, "Combined").gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(s.separateThresholds, "Combined switches it back");

			// Kinds.
			Kit.Click(Kit.Find<MultiToggle>(screen, "CountCritter").gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(s.countCritters, "Count Critters unticked");
			Kit.Click(Kit.Find<MultiToggle>(screen, "CountCritter").gameObject);
			Kit.Click(Kit.Find<MultiToggle>(screen, "CountEgg").gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(s.countCritters, "Count Critters ticked again");
			t.Assert.False(s.countEggs, "Count Eggs unticked");
			Kit.Click(Kit.Find<MultiToggle>(screen, "CountEgg").gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(s.countEggs, "Count Eggs ticked again");

			// Species: expand the critter list, untick one row, then the All row.
			Kit.Click(Kit.Find<MultiToggle>(screen, "ArrowToggle", "HorizontalContent").gameObject);
			yield return t.WaitGameSeconds(0.5);
			MultiToggle hatch = Kit.Find<MultiToggle>(screen, "CheckBox", "Hatch");
			t.Assert.NotNull(hatch, "the Hatch row");
			t.Assert.True(hatch.gameObject.activeInHierarchy, "the list is open");
			Kit.Click(hatch.gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(s.allCritters, "unticking one row leaves All");
			t.Assert.False(s.IsSpeciesSelected(true, Kit.Hatch), "Hatch is off");
			t.Assert.True(s.IsSpeciesSelected(true, Kit.Puft), "and the others stay on");
			MultiToggle all = screen.GetComponentsInChildren<MultiToggle>(true).First(m => m.name == "CheckBox" && m.transform.parent.name == "HorizontalContent");
			Kit.Click(all.gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(s.allCritters, "the All row ticks everything again");
			t.Assert.True(s.IsSpeciesSelected(true, Kit.Hatch), "Hatch is back");

			// A threshold: +10 on the combined editor.
			MultiToggle plusTen = screen.GetComponentsInChildren<MultiToggle>(false).First(m => m.name == "Inc Major");
			Kit.Press(plusTen.gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(10, s.combinedThreshold, "+10 on the combined threshold (combined " + s.combinedThreshold + ", critters " + s.critterThreshold + ", eggs " + s.eggThreshold + ", editor under " + plusTen.transform.parent.parent.parent.name + ")");
			yield return t.Screenshot("screen");
		}
	}
}
