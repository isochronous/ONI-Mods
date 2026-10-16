using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace SmartWeightPlate.Tests
{
	/// <summary>The plate's panel (current weight, Invert) above the game's two-slider screen, and the fix to that screen's text boxes.</summary>
	public static class SideScreen
	{
		private static T Find<T>(Component under, string name) where T : Component
		{
			return under.GetComponentsInChildren<T>(true).FirstOrDefault(c => c.name == name);
		}

		[OniTest("SWP: the panel shows the weight and an Invert box, and the range screen takes up to 2000", Timeout = 60), Layer(2)]
		public static IEnumerator Panel(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 10, 100);
			t.Dupes.Drop(SimHashes.Iron, 150f, r.Cell(1, 1));
			yield return Kit.Settle(t);
			var showing = new List<SideScreenContent>();
			yield return Kit.Show(t, plate.gameObject, s => showing = s);
			SmartWeightPlateSideScreen panel = showing.OfType<SmartWeightPlateSideScreen>().FirstOrDefault();
			ActiveRangeSideScreen range = showing.OfType<ActiveRangeSideScreen>().FirstOrDefault();
			t.Assert.NotNull(panel, "the plate's own panel is showing");
			t.Assert.NotNull(range, "and the game's two-slider screen");
			t.Assert.True(panel.GetSideScreenSortOrder() > range.GetSideScreenSortOrder(), "the panel sorts above the sliders");

			Transform weightObject = Find<Transform>(panel, "CurrentWeight");
			t.Assert.NotNull(weightObject, "the weight label");
			TMPro.TMP_Text weight = weightObject.GetComponentInChildren<TMPro.TMP_Text>(true);
			t.Assert.NotNull(weight, "text on the weight label (" + string.Join(", ", weightObject.GetComponentsInChildren<Component>(true).Select(c => c.GetType().Name)) + ")");
			t.Assert.True(weight.text.Contains("150"), "it reads 150 kg: " + weight.text);

			var high = Traverse.Create(range).Field("activateValueLabel").GetValue<KNumberInputField>();
			var low = Traverse.Create(range).Field("deactivateValueLabel").GetValue<KNumberInputField>();
			t.Assert.Eq(2000f, high.maxValue, "the high box allows 2000");
			t.Assert.Eq(2000f, low.maxValue, "and so does the low one");
			t.Assert.True(high.field.characterLimit >= 4, "with room for four digits");

			GameObject invert = Find<Transform>(panel, "Invert").gameObject;
			Kit.Click(invert);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(plate.Invert, "ticking the box inverts the plate (components on it: " + string.Join(", ", invert.GetComponents<Component>().Select(c => c.GetType().Name)) + ")");
			Kit.Click(invert);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(plate.Invert, "and unticking undoes it");
			yield return t.Screenshot("panel");
		}

		[OniTest("SWP: another building's range screen goes back to 0..100 after the plate's", Timeout = 60), Layer(2)]
		public static IEnumerator RangeBoxesFollowTheTarget(TestContext t)
		{
			CellRect r = t.Rect(5, 3);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(0, 0), 10, 100);
			GameObject battery = t.World.Build("BatterySmart", r.Cell(3, 1));
			yield return Kit.Settle(t);
			var showing = new List<SideScreenContent>();
			yield return Kit.Show(t, plate.gameObject, s => showing = s);
			ActiveRangeSideScreen range = showing.OfType<ActiveRangeSideScreen>().First();
			var high = Traverse.Create(range).Field("activateValueLabel").GetValue<KNumberInputField>();
			t.Assert.Eq(2000f, high.maxValue, "2000 for the plate");
			yield return Kit.Show(t, battery, s => showing = s);
			t.Assert.Eq(100f, high.maxValue, "100 for a Smart Battery shown next");
			yield return Kit.Show(t, plate.gameObject, s => showing = s);
			t.Assert.Eq(2000f, high.maxValue, "and 2000 again for the plate");
		}
	}
}
