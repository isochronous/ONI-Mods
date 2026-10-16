using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace MultichannelCritterSensor.Tests
{
	/// <summary>The sensor's registration and its stored settings: copy settings, saving, the threshold adapter the side screen drives.</summary>
	public static class Config
	{
		/// <summary>Every setting changed from its default, with a species list that is not All.</summary>
		private static void Customise(MultichannelCritterSensor s)
		{
			s.separateThresholds = true;
			s.countCritters = true;
			s.countEggs = false;
			s.allCritters = false;
			s.allEggs = false;
			s.critterTags = new List<Tag> { Kit.Hatch, Kit.Puft };
			s.eggTags = new List<Tag> { Kit.PuftEgg };
			s.combinedThreshold = 7;
			s.combinedAbove = false;
			s.critterThreshold = 3;
			s.critterAbove = false;
			s.eggThreshold = 9;
			s.eggAbove = false;
			s.NotifyConfigChanged();
		}

		private static void AssertCustomised(TestContext t, MultichannelCritterSensor s, string who)
		{
			t.Assert.True(s.separateThresholds, who + ": separate thresholds");
			t.Assert.True(s.countCritters, who + ": counts critters");
			t.Assert.False(s.countEggs, who + ": does not count eggs");
			t.Assert.False(s.allCritters, who + ": not all critters");
			t.Assert.False(s.allEggs, who + ": not all eggs");
			t.Assert.True(s.critterTags.SequenceEqual(new[] { Kit.Hatch, Kit.Puft }), who + ": critter species");
			t.Assert.True(s.eggTags.SequenceEqual(new[] { Kit.PuftEgg }), who + ": egg species");
			t.Assert.Eq(7, s.combinedThreshold, who + ": combined threshold");
			t.Assert.False(s.combinedAbove, who + ": combined below");
			t.Assert.Eq(3, s.critterThreshold, who + ": critter threshold");
			t.Assert.False(s.critterAbove, who + ": critter below");
			t.Assert.Eq(9, s.eggThreshold, who + ": egg threshold");
			t.Assert.False(s.eggAbove, who + ": egg below");
		}

		[OniTest("MCS: is a buildable of Multiplexing under Sensors, with a four-bit ribbon output", Timeout = 20), Layer(0)]
		public static IEnumerator Registration(TestContext t)
		{
			BuildingDef def = Assets.GetBuildingDef(MultichannelCritterSensorConfig.ID);
			t.Assert.NotNull(def, "the building exists");
			t.Assert.Eq(2, def.Mass.Length, "two materials");
			t.Assert.Eq(50f, def.Mass[0], "50 kg of refined metal");
			t.Assert.Eq(50f, def.Mass[1], "50 kg of plastic");
			t.Assert.True(Db.Get().Techs.Get("Multiplexing").unlockedItemIDs.Contains(MultichannelCritterSensorConfig.ID), "unlocked by Multiplexing");
			t.Assert.Eq(1, def.LogicOutputPorts.Count, "one output port");
			t.Assert.Eq(LogicPortSpriteType.RibbonOutput, def.LogicOutputPorts[0].spriteType, "and it is a ribbon port");
			bool found = false;
			foreach (PlanScreen.PlanInfo info in TUNING.BUILDINGS.PLANORDER)
				if (info.category == (HashedString)"Automation")
				{
					int sensor = -1, vanilla = -1;
					for (int i = 0; i < info.buildingAndSubcategoryData.Count; i++)
					{
						if (info.buildingAndSubcategoryData[i].Key == MultichannelCritterSensorConfig.ID) sensor = i;
						if (info.buildingAndSubcategoryData[i].Key == LogicCritterCountSensorConfig.ID) vanilla = i;
					}
					found = sensor >= 0 && vanilla >= 0 && sensor == vanilla + 1 && info.buildingAndSubcategoryData[sensor].Value == "sensors";
				}
			t.Assert.True(found, "right after the Critter Sensor under Automation > Sensors");
			yield return t.Frames(1);
		}

		[OniTest("MCS: copy settings gives another sensor every setting, with its own lists", Timeout = 40), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(4, 1);
			var a = t.World.Build("MultichannelCritterSensor", r.Cell(0, 0)).GetComponent<MultichannelCritterSensor>();
			var b = t.World.Build("MultichannelCritterSensor", r.Cell(2, 0)).GetComponent<MultichannelCritterSensor>();
			yield return t.WaitGameSeconds(1);
			Customise(a);
			b.gameObject.Trigger((int)GameHashes.CopySettings, a.gameObject);
			yield return t.WaitGameSeconds(0.5);
			AssertCustomised(t, b, "the copy");
			a.critterTags.Clear();
			t.Assert.Eq(2, b.critterTags.Count, "the copy keeps its own list when the source's changes");
		}

		[OniTest("MCS: every setting is saved with the building and restored on load", Timeout = 60), Layer(1)]
		public static IEnumerator SavedAndLoaded(TestContext t)
		{
			CellRect r = t.Rect(2, 1);
			GameObject go = t.World.Build("MultichannelCritterSensor", r.Cell(0, 0));
			yield return t.WaitGameSeconds(1);
			Customise(go.GetComponent<MultichannelCritterSensor>());
			Tag tag = go.PrefabID();
			byte[] file = SaveFile.Capture(go);
			t.World.Destroy(go);
			yield return t.WaitGameSeconds(2);
			GameObject back = SaveFile.Restore(tag, file);
			yield return t.WaitGameSeconds(2);
			AssertCustomised(t, back.GetComponent<MultichannelCritterSensor>(), "the loaded sensor");
			t.Assert.NotNull(back.GetComponent<LogicPorts>(), "with its ports");
		}

		[OniTest("MCS: the threshold editors write the right threshold, clamped to 0..64", Timeout = 30), Layer(0)]
		public static IEnumerator ThresholdAdapters(TestContext t)
		{
			CellRect r = t.Rect(2, 1);
			var s = t.World.Build("MultichannelCritterSensor", r.Cell(0, 0)).GetComponent<MultichannelCritterSensor>();
			yield return t.WaitGameSeconds(1);
			System.Type adapterType = typeof(MultichannelCritterSensor).Assembly.GetType("MultichannelCritterSensor.ThresholdAdapter");
			System.Type kindType = adapterType.GetNestedType("Kind");
			var host = new GameObject("test adapters");
			t.Cleanup(() => Object.Destroy(host));
			foreach (string kind in new[] { "Combined", "Critters", "Eggs" })
			{
				Component adapter = host.AddComponent(adapterType);
				adapterType.GetField("kind").SetValue(adapter, System.Enum.Parse(kindType, kind));
				adapterType.GetField("sensor").SetValue(adapter, s);
				var sw = (IThresholdSwitch)adapter;
				sw.Threshold = 100f;
				t.Assert.Eq(64f, sw.Threshold, kind + ": above 64 is 64");
				sw.Threshold = -5f;
				t.Assert.Eq(0f, sw.Threshold, kind + ": below 0 is 0");
				sw.Threshold = 12.4f;
				sw.ActivateAboveThreshold = false;
				t.Assert.Eq(12f, sw.Threshold, kind + ": whole numbers");
				t.Assert.False(sw.ActivateAboveThreshold, kind + ": below");
			}
			t.Assert.Eq(12, s.combinedThreshold, "the combined adapter wrote the combined threshold");
			t.Assert.Eq(12, s.critterThreshold, "the critter adapter wrote the critter threshold");
			t.Assert.Eq(12, s.eggThreshold, "the egg adapter wrote the egg threshold");
			t.Assert.False(s.combinedAbove && s.critterAbove && s.eggAbove, "and the directions");
			t.Assert.False(s.combinedAbove, "combined below");
			t.Assert.False(s.critterAbove, "critters below");
			t.Assert.False(s.eggAbove, "eggs below");
		}
	}
}
