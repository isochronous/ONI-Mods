using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace MotionSensorRange.Tests
{
	/// <summary>The range setting: its limits, what it does to the sensor's detection area and overlay, and what a Duplicant makes of it.</summary>
	public static class Range
	{
		internal static GameObject Sensor(TestContext t, CellRect r, int x, int y = 1)
		{
			for (int dx = -1; dx <= 1; dx++)
				if (x + dx >= 0 && x + dx < r.W && !(t.World.At(r.Cell(x + dx, y - 1), ObjectLayer.FoundationTile) != null))
					t.World.Build("Tile", r.Cell(x + dx, y - 1));
			return t.World.Build(LogicDuplicantSensorConfig.ID, r.Cell(x, y));
		}

		internal static Extents ExtentsOf(GameObject sensor) => Traverse.Create(sensor.GetComponent<LogicDuplicantSensor>()).Field("pickupableExtents").GetValue<Extents>();

		[OniTest("MSR: the sensor has a range slider from 2 to 16 at the vanilla 4, and copy settings", Timeout = 30), Layer(0)]
		public static IEnumerator Defaults(TestContext t)
		{
			CellRect r = t.Rect(5, 3);
			GameObject go = Sensor(t, r, 2);
			yield return t.WaitGameSeconds(1);
			var slider = go.GetComponent<RangeSlider>();
			t.Assert.NotNull(slider, "the sensor has the slider");
			t.Assert.NotNull(go.GetComponent<CopyBuildingSettings>(), "and takes part in copy settings");
			t.Assert.Eq(2f, slider.GetSliderMin(0), "minimum");
			t.Assert.Eq(16f, slider.GetSliderMax(0), "maximum");
			t.Assert.Eq(4f, slider.GetSliderValue(0), "the default is the vanilla range");
			t.Assert.Eq(4, go.GetComponent<LogicDuplicantSensor>().pickupRange, "and so is the sensor's");
			t.Assert.Eq(0, slider.SliderDecimalPlaces(0), "whole tiles");
			t.Assert.False(string.IsNullOrEmpty(Strings.Get(RangeSlider.TitleKey).String), "the title is in the string table");
			t.Assert.False(string.IsNullOrEmpty(slider.GetSliderTooltip(0)), "the tooltip has text");
			t.Assert.True(Assets.GetBuildingDef(LogicDuplicantSensorConfig.ID).BuildingUnderConstruction.GetComponent<RangeSlider>() != null, "a building under construction has it too");
		}

		[OniTest("MSR: values snap to even tiles within 2 to 16", Timeout = 30), Layer(0)]
		public static IEnumerator Snapping(TestContext t)
		{
			CellRect r = t.Rect(5, 3);
			GameObject go = Sensor(t, r, 2);
			yield return t.WaitGameSeconds(1);
			var slider = go.GetComponent<RangeSlider>();
			var cases = new Dictionary<float, int>
			{
				{ 5.2f, 6 }, { 6.9f, 6 }, { 7.2f, 8 }, { 9.8f, 10 }, { 15.1f, 16 }, { 1f, 2 }, { -4f, 2 }, { 17f, 16 }, { 100f, 16 }, { 2f, 2 }, { 16f, 16 },
			};
			foreach (KeyValuePair<float, int> c in cases)
			{
				slider.SetSliderValue(c.Key, 0);
				t.Assert.Eq((float)c.Value, slider.GetSliderValue(0), "the slider set to " + c.Key);
				t.Assert.Eq(c.Value, go.GetComponent<LogicDuplicantSensor>().pickupRange, "the sensor's range after " + c.Key);
			}
			slider.SetSliderValue(5f, 0);
			t.Assert.True((int)slider.GetSliderValue(0) % 2 == 0, "an odd value lands on an even one");
			yield return t.WaitGameSeconds(0.5);
		}

		[OniTest("MSR: a new range moves the sensor's detection area and its overlay", Timeout = 30), Layer(0)]
		public static IEnumerator DetectionArea(TestContext t)
		{
			CellRect r = t.Rect(25, 3);
			GameObject go = Sensor(t, r, 12);
			yield return t.WaitGameSeconds(1);
			Grid.CellToXY(r.Cell(12, 1), out int sx, out int sy);
			Extents before = ExtentsOf(go);
			t.Assert.Eq(sx - 2, before.x, "the default area starts two tiles left");
			t.Assert.Eq(5, before.width, "and is five wide");
			var slider = go.GetComponent<RangeSlider>();
			slider.SetSliderValue(12f, 0);
			yield return t.WaitGameSeconds(0.5);
			Extents e = ExtentsOf(go);
			t.Assert.Eq(sx - 6, e.x, "six tiles left of the sensor at range 12");
			t.Assert.Eq(13, e.width, "thirteen wide");
			t.Assert.Eq(sy, e.y, "from the sensor's row");
			t.Assert.Eq(13, e.height, "and thirteen high");
			RangeVisualizer visualizer = go.GetComponent<RangeVisualizer>();
			t.Assert.Eq(new Vector2I(-6, 0), visualizer.RangeMin, "the overlay's low corner");
			t.Assert.Eq(new Vector2I(6, 12), visualizer.RangeMax, "the overlay's high corner");
			slider.SetSliderValue(2f, 0);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(3, ExtentsOf(go).width, "range 2 is three wide");
			t.Assert.Eq(new Vector2I(1, 2), visualizer.RangeMax, "and the overlay shrinks with it");
		}

		[OniTest("MSR: a sensor facing sideways rotates its detection area", Timeout = 30), Layer(0)]
		public static IEnumerator Rotated(TestContext t)
		{
			CellRect r = t.Rect(5, 7);
			// Mounted on a wall to its left, the sensor faces right.
			for (int y = 0; y < 7; y++)
				t.World.Build("Tile", r.Cell(0, y));
			BuildingDef def = Assets.GetBuildingDef(LogicDuplicantSensorConfig.ID);
			Orientation facing = def.IsValidPlaceLocation(null, r.Cell(1, 3), Orientation.R90, out _) ? Orientation.R90 : Orientation.R270;
			GameObject go = t.World.Build(LogicDuplicantSensorConfig.ID, r.Cell(1, 3), facing);
			yield return t.WaitGameSeconds(1);
			go.GetComponent<RangeSlider>().SetSliderValue(8f, 0);
			yield return t.WaitGameSeconds(0.5);
			Grid.CellToXY(r.Cell(1, 3), out int sx, out int sy);
			Extents e = ExtentsOf(go);
			t.Assert.Eq(9, e.width, "nine wide whichever way it faces");
			t.Assert.Eq(sy - 4, e.y, "centred on the sensor's row when it faces sideways, not four rows above it");
			t.Assert.True(e.x == sx || e.x == sx - 8, "starting at the sensor or eight tiles left, " + e.x + " for a sensor at " + sx);
		}

		[OniTest("MSR: copy settings gives another sensor the range", Timeout = 30), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(30, 3);
			GameObject a = Sensor(t, r, 5);
			GameObject b = Sensor(t, r, 20);
			yield return t.WaitGameSeconds(1);
			a.GetComponent<RangeSlider>().SetSliderValue(14f, 0);
			yield return t.WaitGameSeconds(0.5);
			b.Trigger((int)GameHashes.CopySettings, a);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(14f, b.GetComponent<RangeSlider>().GetSliderValue(0), "the copy's slider");
			t.Assert.Eq(14, b.GetComponent<LogicDuplicantSensor>().pickupRange, "the copy's sensor");
			t.Assert.Eq(15, ExtentsOf(b).width, "the copy's area is 15 wide");
			t.Assert.Eq(new Vector2I(7, 14), b.GetComponent<RangeVisualizer>().RangeMax, "the copy's overlay");
		}
	}
}
