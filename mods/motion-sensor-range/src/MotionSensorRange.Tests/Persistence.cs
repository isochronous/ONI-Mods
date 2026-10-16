using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace MotionSensorRange.Tests
{
	/// <summary>The range through a save and a load, through construction, and in the sensor's side screen.</summary>
	public static class Persistence
	{
		[OniTest("MSR: the range is saved with the building and applied when it loads", Timeout = 60), Layer(1)]
		public static IEnumerator SavedAndLoaded(TestContext t)
		{
			CellRect r = t.Rect(25, 3);
			GameObject go = Range.Sensor(t, r, 12);
			yield return t.WaitGameSeconds(1);
			go.GetComponent<RangeSlider>().SetSliderValue(10f, 0);
			yield return t.WaitGameSeconds(0.5);
			Tag tag = go.PrefabID();
			byte[] file = SaveFile.Capture(go);
			t.World.Destroy(go);
			yield return t.WaitGameSeconds(2);
			GameObject back = SaveFile.Restore(tag, file);
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(10f, back.GetComponent<RangeSlider>().GetSliderValue(0), "the slider");
			t.Assert.Eq(10, back.GetComponent<LogicDuplicantSensor>().pickupRange, "the sensor's range");
			Grid.CellToXY(r.Cell(12, 1), out int sx, out _);
			Extents e = Range.ExtentsOf(back);
			t.Assert.Eq(sx - 5, e.x, "the detection area starts five tiles left");
			t.Assert.Eq(11, e.width, "and is eleven wide");
			t.Assert.Eq(new Vector2I(5, 10), back.GetComponent<RangeVisualizer>().RangeMax, "the overlay is as wide");
		}

		[OniTest("MSR: a range set on the building under construction carries over when it is finished", Timeout = 60), Layer(1)]
		public static IEnumerator CarriedOverFromConstruction(TestContext t)
		{
			t.InstantBuild(false);
			CellRect r = t.Rect(5, 3);
			for (int x = 0; x < 5; x++)
				t.World.Build("Tile", r.Cell(x, 0));
			yield return t.WaitGameSeconds(1);
			BuildingDef def = Assets.GetBuildingDef(LogicDuplicantSensorConfig.ID);
			var elements = new List<Tag> { SimHashes.Copper.CreateTag() };
			GameObject site = def.TryPlace(null, Grid.CellToPosCBC(r.Cell(2, 1), def.SceneLayer), Orientation.Neutral, elements);
			t.Assert.NotNull(site, "the construction site is placed");
			yield return t.WaitGameSeconds(1);
			var slider = site.GetComponent<RangeSlider>();
			t.Assert.NotNull(slider, "the site has the slider");
			slider.SetSliderValue(14f, 0);
			Constructable constructable = site.GetComponent<Constructable>();
			Traverse.Create(constructable).Field("initialTemperature").SetValue(293.15f);
			// What the Duplicant's last work does, without the Duplicant.
			Traverse.Create(constructable).Method("FinishConstruction", new[] { typeof(UtilityConnections), typeof(WorkerBase) }, new object[] { (UtilityConnections)0, null }).GetValue();
			yield return t.WaitGameSeconds(1.5);
			GameObject built = t.World.At(r.Cell(2, 1), def.ObjectLayer);
			t.Assert.NotNull(built, "the sensor is built");
			t.Assert.True(built.GetComponent<LogicDuplicantSensor>() != null, "and it is the finished sensor");
			t.Assert.Eq(14f, built.GetComponent<RangeSlider>().GetSliderValue(0), "the finished sensor has the range set on the site");
			t.Assert.Eq(14, built.GetComponent<LogicDuplicantSensor>().pickupRange, "and applies it");
		}

		[OniTest("MSR: the side screen shows the range and a change in it moves the sensor", Timeout = 60), Layer(2)]
		public static IEnumerator SideScreen(TestContext t)
		{
			CellRect r = t.Rect(5, 3);
			GameObject go = Range.Sensor(t, r, 2);
			yield return t.WaitGameSeconds(1);
			SelectTool.Instance.Select(go.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
			SideScreenContent screen = null;
			foreach (DetailsScreen.SideScreenRef sr in Traverse.Create(DetailsScreen.Instance).Field("sideScreens").GetValue<List<DetailsScreen.SideScreenRef>>())
				if (sr.screenInstance != null && sr.screenInstance.gameObject.activeInHierarchy && sr.screenInstance.IsValidForTarget(go) && sr.screenInstance.GetType().Name.Contains("Slider"))
					screen = sr.screenInstance;
			t.Assert.NotNull(screen, "a slider screen shows for the sensor");
			KSlider slider = screen.GetComponentsInChildren<KSlider>(true).FirstOrDefault();
			t.Assert.NotNull(slider, "with a slider");
			t.Assert.Eq(2f, slider.minValue, "from 2");
			t.Assert.Eq(16f, slider.maxValue, "to 16");
			t.Assert.Eq(4f, slider.value, "at 4");
			slider.value = 12f;
			var fired = new List<string>();
			foreach (string name in new[] { "onDrag", "onMove", "onReleaseHandle", "onPointerDown" })
			{
				System.Delegate handler = Traverse.Create(slider).Field(name).GetValue<System.Delegate>();
				if (handler != null && name != "onPointerDown")
				{
					handler.DynamicInvoke();
					fired.Add(name);
				}
			}
			t.Log("the slider's handlers: " + string.Join(", ", fired));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(12f, go.GetComponent<RangeSlider>().GetSliderValue(0), "moving the slider sets the range");
			t.Assert.Eq(12, go.GetComponent<LogicDuplicantSensor>().pickupRange, "and the sensor's");
			yield return t.Screenshot("screen");
		}
	}
}
