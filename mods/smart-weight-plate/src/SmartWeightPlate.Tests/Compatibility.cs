using System.Collections;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace SmartWeightPlate.Tests
{
	/// <summary>
	/// With Better Automation Overlay the overlay's label for the plate reads in kilograms.
	/// Needs that Workshop mod, so it runs only when asked:
	/// Invoke-OniTests.ps1 -Mods SmartWeightPlate -Steam 1878896484 -Filter "*Overlay*"
	/// </summary>
	public static class Compatibility
	{
		[OniTest("SWP: Better Automation Overlay labels the plate with its thresholds in kilograms", Timeout = 40, Explicit = true), Layer(1)]
		public static IEnumerator OverlayLabelInKilograms(TestContext t)
		{
			System.Type labelType = AccessTools.TypeByName("BetterLogicOverlay.LogicSettingDisplay.ActivationRangeTargetSetting");
			if (labelType == null)
				t.Skip("Better Automation Overlay is not enabled; run with -Steam 1878896484");
			CellRect r = t.Rect(3, 2);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 7, 35);
			GameObject battery = t.World.Build("BatterySmart", r.Cell(0, 1));
			yield return Kit.Settle(t);
			string Label(GameObject go)
			{
				Component label = go.GetComponent(labelType) ?? go.AddComponent(labelType);
				return AccessTools.Method(labelType, "GetSetting").Invoke(label, null) as string;
			}
			string text = Label(plate.gameObject);
			t.Log("the plate's label reads '" + text + "'");
			t.Assert.True(text != null && text.Contains("7 - 35") && text.Contains("kg"), "the plate reads in kilograms: " + text);
			string other = Label(battery);
			t.Log("a Smart Battery's label reads '" + other + "'");
			t.Assert.True(other != null && !other.Contains("kg"), "other buildings keep the overlay's own text: " + other);
		}
	}
}
