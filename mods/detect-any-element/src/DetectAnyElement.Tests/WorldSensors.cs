using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace DetectAnyElement.Tests
{
	/// <summary>
	/// The Gas and Liquid Element Sensors (the in-world ones) on Any. They differ from each other
	/// only in the state they look for, so each gets one test of "empty, any of its state, other
	/// state, an element picked instead".
	/// </summary>
	public static class WorldSensors
	{
		[OniTest("DAE: a Gas Element Sensor on Any is on for any gas, off for vacuum and for liquid", Timeout = 40), Layer(0)]
		public static IEnumerator GasSensor(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 5);
			GameObject sensor = Sensors.InChamber(t, r, "LogicElementSensorGas", out int cell);
			yield return Sensors.Ready(t, cell);
			Sensors.PickAny(sensor);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(sensor), "vacuum counts as nothing (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Fill(cell, SimHashes.Oxygen, 0.5f);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "oxygen is a gas (cell: " + Sensors.Describe(cell) + ")");
			t.Assert.Eq("1", Sensors.PortSignal(sensor), "the output port carries the signal");

			Sensors.Fill(cell, SimHashes.Hydrogen, 0.5f);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "hydrogen is a gas too, whichever it is (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Empty(cell);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(sensor), "back to vacuum, off again (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Fill(cell, SimHashes.Water, 50f);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(sensor), "a liquid is not a gas (cell: " + Sensors.Describe(cell) + ")");
		}

		[OniTest("DAE: a Liquid Element Sensor on Any is on for any liquid, off for gas", Timeout = 40), Layer(0)]
		public static IEnumerator LiquidSensor(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 5);
			GameObject sensor = Sensors.InChamber(t, r, "LogicElementSensorLiquid", out int cell);
			yield return Sensors.Ready(t, cell);
			Sensors.PickAny(sensor);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(sensor), "vacuum counts as nothing (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Fill(cell, SimHashes.Water, 50f);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "water is a liquid (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Fill(cell, SimHashes.CrudeOil, 50f);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "crude oil is a liquid too, whichever it is (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Fill(cell, SimHashes.Oxygen, 0.5f);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(sensor), "a gas is not a liquid (cell: " + Sensors.Describe(cell) + ")");
		}

		[OniTest("DAE: picking an element on a sensor that was on Any switches Any off", Timeout = 40), Layer(0)]
		public static IEnumerator ElementReplacesAny(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 5);
			GameObject sensor = Sensors.InChamber(t, r, "LogicElementSensorGas", out int cell);
			yield return Sensors.Ready(t, cell);
			Sensors.Fill(cell, SimHashes.Hydrogen, 0.5f);
			Sensors.PickAny(sensor);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "Any matches the hydrogen (cell: " + Sensors.Describe(cell) + ")");

			Sensors.Pick(sensor, SimHashes.Oxygen);
			yield return Sensors.Settle(t);
			t.Assert.False(Sensors.IsOn(sensor), "oxygen chosen: the hydrogen no longer matches (cell: " + Sensors.Describe(cell) + ")");
			t.Assert.False(AnyElement.Is(sensor.GetComponent<Filterable>().SelectedTag), "the choice is oxygen, not Any");

			Sensors.Pick(sensor, SimHashes.Hydrogen);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "hydrogen chosen: the game's own element test still works (cell: " + Sensors.Describe(cell) + ")");

			Sensors.PickAny(sensor);
			yield return Sensors.Settle(t);
			t.Assert.True(Sensors.IsOn(sensor), "and back to Any (cell: " + Sensors.Describe(cell) + ")");
		}

		[OniTest("DAE: a sensor on Any does not show the 'no element selected' warning; an empty one does", Timeout = 30), Layer(0)]
		public static IEnumerator NoElementWarning(TestContext t)
		{
			CellRect r = t.Rect(5, 5);
			GameObject sensor = Sensors.InChamber(t, r, "LogicElementSensorGas", out _);
			yield return Sensors.Ready(t);
			KSelectable selectable = sensor.GetComponent<KSelectable>();
			StatusItem warning = Db.Get().BuildingStatusItems.NoFilterElementSelected;
			t.Assert.True(selectable.HasStatusItem(warning), "a fresh sensor warns that no element is selected");

			Sensors.PickAny(sensor);
			yield return t.WaitGameSeconds(1);
			t.Assert.False(selectable.HasStatusItem(warning), "Any is a choice");

			sensor.GetComponent<Filterable>().SelectedTag = GameTags.Void;
			yield return t.WaitGameSeconds(1);
			t.Assert.True(selectable.HasStatusItem(warning), "None warns again");
		}
	}
}
