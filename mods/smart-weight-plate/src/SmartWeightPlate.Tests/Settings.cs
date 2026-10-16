using System.Collections;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace SmartWeightPlate.Tests
{
	/// <summary>The plate's stored settings: copy settings, saving, and the plates saved by versions before 0.2.0.</summary>
	public static class Settings
	{
		[OniTest("SWP: copy settings gives another plate both thresholds and Invert, and its port text", Timeout = 40), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(4, 2);
			SmartWeightPlate a = Kit.Plate(t, r.Cell(0, 0), 7, 77);
			SmartWeightPlate b = Kit.Plate(t, r.Cell(2, 0), 10, 100);
			yield return Kit.Settle(t);
			a.Invert = true;
			string invertedText = a.GetComponent<LogicPorts>().outputPortInfo[0].activeDescription;
			b.gameObject.Trigger((int)GameHashes.CopySettings, a.gameObject);
			yield return Kit.Settle(t);
			t.Assert.Eq(77f, b.ActivateValue, "the high threshold");
			t.Assert.Eq(7f, b.DeactivateValue, "the low threshold");
			t.Assert.True(b.Invert, "Invert");
			t.Assert.Eq(invertedText, b.GetComponent<LogicPorts>().outputPortInfo[0].activeDescription, "the port text follows");
			t.Assert.Eq(1, Kit.Port(b), "an empty inverted plate sends green");
		}

		[OniTest("SWP: thresholds, Invert and the latch are saved with the building", Timeout = 60), Layer(1)]
		public static IEnumerator SavedAndLoaded(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 15, 40);
			plate.Invert = true;
			// A tile above weighs 200 kg and does not fall when the plate goes.
			t.World.Build("Tile", r.Cell(1, 1));
			yield return Kit.Settle(t);
			t.Assert.Eq(0, Kit.Port(plate), "heavy and inverted: red");
			// Raise the high threshold past the weight: between the thresholds, a heavy plate stays heavy.
			plate.ActivateValue = 300;
			yield return Kit.Settle(t);
			t.Assert.Eq(0, Kit.Port(plate), "still red between the thresholds");

			Tag tag = plate.gameObject.PrefabID();
			byte[] file = SaveFile.Capture(plate.gameObject);
			t.World.Destroy(plate.gameObject);
			yield return t.WaitGameSeconds(2);
			SmartWeightPlate back = SaveFile.Restore(tag, file).GetComponent<SmartWeightPlate>();
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(300f, back.ActivateValue, "the high threshold");
			t.Assert.Eq(15f, back.DeactivateValue, "the low threshold");
			t.Assert.True(back.Invert, "Invert is kept, not flipped by the migration of old saves");
			t.Assert.Eq(Grid.Mass[r.Cell(1, 1)], back.CurrentValue, "the tile is weighed again");
			t.Assert.Eq(0, Kit.Port(back), "the latch was saved: still heavy, so still red between the thresholds");
		}

		[OniTest("SWP: a plate saved before 0.2.0 keeps the signal it sent", Timeout = 60), Layer(1)]
		public static IEnumerator OldSaveKeepsItsSignal(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 10, 100);
			yield return Kit.Settle(t);
			// An old save has no settings version. Those plates sent green while light.
			Traverse.Create(plate).Field("settingsVersion").SetValue(-1);
			Tag tag = plate.gameObject.PrefabID();
			byte[] file = SaveFile.Capture(plate.gameObject);
			t.World.Destroy(plate.gameObject);
			yield return t.WaitGameSeconds(2);
			SmartWeightPlate back = SaveFile.Restore(tag, file).GetComponent<SmartWeightPlate>();
			yield return t.WaitGameSeconds(2);
			t.Assert.True(back.Invert, "the old plate's Invert is flipped once, so it still sends green while light");
			t.Assert.Eq(1, Kit.Port(back), "and it sends green when empty, as it did");
			t.Assert.Eq(1, Traverse.Create(back).Field("settingsVersion").GetValue<int>(), "it now carries the current version");
		}

		[OniTest("SWP: a plate built new is not migrated", Timeout = 30), Layer(1)]
		public static IEnumerator NewPlateIsNotMigrated(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 10, 100);
			yield return Kit.Settle(t);
			t.Assert.False(plate.Invert, "a new plate is not inverted");
			t.Assert.Eq(0, Kit.Port(plate), "and sends red when empty, like the vanilla plate");
			t.Assert.Eq(1, Traverse.Create(plate).Field("settingsVersion").GetValue<int>(), "at the current version");
		}
	}
}
