using System.Collections;
using System.IO;
using System.Linq;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace DetectAnyElement.Tests
{
	/// <summary>
	/// Where the choice lives: in the sensor's Filterable, so it is saved with the building and
	/// copied with its settings like an element.
	/// </summary>
	public static class Persistence
	{
		[OniTest("DAE: copying settings from a sensor on Any makes the target Any, and from an element makes it that element", Timeout = 40), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			GameObject source = t.World.Build("LogicElementSensorGas", r.Cell(1, 0));
			GameObject target = t.World.Build("LogicElementSensorGas", r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			Sensors.PickAny(source);
			Sensors.Pick(target, SimHashes.Oxygen);
			target.Trigger(-905833192, source);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(AnyElement.Is(target.GetComponent<Filterable>().SelectedTag), "the target took Any");
			t.Assert.NotNull(target.GetComponent<SymbolOverrideController>().GetSymbolOverrides.FirstOrDefault(e => e.targetSymbol == "elements" && e.priority == 0).sourceSymbol, "and shows the asterisk");

			Sensors.Pick(source, SimHashes.Hydrogen);
			target.Trigger(-905833192, source);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(SimHashes.Hydrogen.CreateTag(), target.GetComponent<Filterable>().SelectedTag, "the target took the element");
			t.Assert.False(AnyElement.Is(target.GetComponent<Filterable>().SelectedTag), "and is off Any");
		}

		private static string RoundTripPath => Path.Combine(Session.Dir, "dae-roundtrip.sav");

		/// <summary>
		/// Half of a save round trip: three sensors are set up and the colony saved into the run
		/// folder. RoundTripLoad, the next test, starts from that save instead of the fixture.
		/// </summary>
		[OniTest("DAE: (round trip 1/2) sensors on Any, an element and None are saved to the run folder", Timeout = 60), Layer(0)]
		public static IEnumerator WriteRoundTrip(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(10, 5);
			GameObject gas = Sensors.InChamber(t, r, "LogicElementSensorGas", out int gasCell);
			t.World.Build("LiquidConduit", r.Cell(7, 0));
			GameObject pipe = t.World.Build("LiquidConduitElementSensor", r.Cell(7, 0));
			GameObject element = t.World.Build("LogicElementSensorLiquid", r.Cell(9, 0));
			yield return Sensors.Ready(t, gasCell);
			Sensors.Fill(gasCell, SimHashes.Hydrogen, 0.5f);
			Sensors.PickAny(gas);
			Sensors.PickAny(pipe);
			Sensors.Pick(element, SimHashes.CrudeOil);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(gas), "the gas sensor is on before saving");
			string path = RoundTripPath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (FreshSave) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
		}

		[OniTest("DAE: (round trip 2/2) after loading the save, Any is still Any, the element still the element, and Any still works", Timeout = 60, FreshSave = true), Layer(0)]
		public static IEnumerator RoundTripLoad(TestContext t)
		{
			string fixture = Session.SavePath;
			t.Cleanup(() =>
			{
				// Fixture back for whoever loads next; the run folder's copy of the original is the one in mods.json's run args.
				string original = Directory.GetFiles(Session.Dir, "fixture.sav").FirstOrDefault();
				if (original != null)
					Session.SavePath = original;
				string path = RoundTripPath;
				if (File.Exists(path))
					File.Delete(path);
			});
			t.Speed(3);
			t.Assert.True(fixture.EndsWith("dae-roundtrip.sav"), "this test started from the round trip save, not the fixture (" + fixture + ")");
			yield return t.WaitGameSeconds(1);
			Filterable[] all = Object.FindObjectsOfType<Filterable>();
			Filterable Find(string prefab) => all.FirstOrDefault(f => f.PrefabID() == prefab);
			Filterable gas = Find("LogicElementSensorGas");
			Filterable pipe = Find("LiquidConduitElementSensor");
			Filterable element = Find("LogicElementSensorLiquid");
			t.Assert.NotNull(gas, "the gas sensor was loaded");
			t.Assert.NotNull(pipe, "the pipe sensor was loaded");
			t.Assert.NotNull(element, "the liquid sensor was loaded");
			t.Assert.True(AnyElement.Is(gas.SelectedTag), "the gas sensor is still on Any");
			t.Assert.True(AnyElement.Is(pipe.SelectedTag), "the pipe sensor is still on Any");
			t.Assert.Eq(SimHashes.CrudeOil.CreateTag(), element.SelectedTag, "the liquid sensor is still on crude oil");
			t.Assert.Eq(SimHashes.Hydrogen.ToString(), Grid.Element[Grid.PosToCell(gas)].id.ToString(), "the gas is still in the chamber");
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(gas.gameObject), "the loaded gas sensor on Any is on for the hydrogen");
			var overrides = gas.GetComponent<SymbolOverrideController>().GetSymbolOverrides;
			t.Assert.True(overrides.Any(e => e.targetSymbol == "elements" && e.priority == 0), "the asterisk is back on the loaded sensor");
			var plain = element.GetComponent<SymbolOverrideController>().GetSymbolOverrides;
			t.Assert.False(plain.Any(e => e.targetSymbol == "elements" && e.priority == 0), "and not on the one set to an element");
			Sensors.Pick(gas.gameObject, SimHashes.Oxygen);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(gas.gameObject), "a loaded sensor still leaves Any when an element is picked");
		}
	}
}
