using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace FridgeThresholdsExtended.Tests
{
	/// <summary>
	/// Fridges with Storage Refrigerator Thresholds (Workshop 2959340381), Freezer (2618339179) and
	/// Industrial Revolution (3557584850) loaded: powered by a dev generator on a copper wire, filled
	/// with real food and emptied the way a Duplicant eats from them (Storage.ConsumeIgnoringDisease),
	/// the full port read as the automation overlay shows it. Run with
	/// -Steam 2959340381,2618339179,3557584850. The other mods' types are reached by name only.
	/// </summary>
	public static class Fridges
	{
		private const string RefrigeratorThresholdsType = "StorageRefrigeratorThresholds.RefrigeratorThresholds";
		private const string FreezerType = "Psyko.Freezer.Freezer";
		private const string Food = "MushBar";
		private const float Cold = 255.15f;

		private static string RoundTripPath => Path.Combine(Session.Dir, "fte-roundtrip.sav");
		private static string RoundTripCellPath => Path.Combine(Session.Dir, "fte-roundtrip-cell.txt");

		private sealed class Rig
		{
			public CellRect Inside;
			public int Generator;
			public GameObject GeneratorObject;
		}

		/// <summary>The thresholds component type; fails the test when the thresholds mod is not loaded.</summary>
		private static Type ThresholdsType(TestContext t)
		{
			Type type = AccessTools.TypeByName(RefrigeratorThresholdsType);
			t.Assert.NotNull(type, "Storage Refrigerator Thresholds is loaded (" + RefrigeratorThresholdsType + ")");
			return type;
		}

		private static Type FreezerComponentType(TestContext t)
		{
			Type type = AccessTools.TypeByName(FreezerType);
			t.Assert.NotNull(type, "Freezer is loaded (" + FreezerType + ")");
			return type;
		}

		/// <summary>A boxed room with a copper wire dragged with the real tool along its floor from x = 2 to <paramref name="toX"/>.</summary>
		private static IEnumerator Box(TestContext t, Rig rig, int toX)
		{
			rig.Inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			rig.Generator = rig.Inside.Cell(2, 0);
			var cells = new List<int>();
			for (int x = 2; x <= toX; x++)
				cells.Add(rig.Inside.Cell(x, 0));
			UtilityDragSession wire = t.Tools.UtilityTool("Wire", SimHashes.Copper);
			yield return wire.Path(cells.ToArray());
			wire.Dispose();
			yield return t.WaitGameSeconds(2);
		}

		/// <summary>Builds the dev generator at the wire's end and waits until every fridge is powered and operational.</summary>
		private static IEnumerator Power(TestContext t, Rig rig, params GameObject[] fridges)
		{
			rig.GeneratorObject = t.World.Build("DevGenerator", rig.Generator);
			yield return t.WaitUntil(() => Array.TrueForAll(fridges, f => f.GetComponent<EnergyConsumer>().IsPowered && f.GetComponent<Operational>().IsOperational), 20, "the fridge(s) to be powered through the wire");
			yield return t.WaitGameSeconds(1);
		}

		private static Component Thresholds(TestContext t, GameObject fridge)
		{
			Component thresholds = fridge.GetComponent(ThresholdsType(t));
			t.Assert.NotNull(thresholds, fridge.PrefabID().Name + " has the thresholds component");
			return thresholds;
		}

		private static object Setting(Component thresholds, string name)
		{
			return AccessTools.Property(thresholds.GetType(), name).GetValue(thresholds);
		}

		/// <summary>The three settings the side screen sets, through the same properties (each setter updates the port).</summary>
		private static void SetThresholds(Component thresholds, bool greenOnLow, float high, float low)
		{
			Type type = thresholds.GetType();
			AccessTools.Property(type, "SendGreenOnLow").SetValue(thresholds, greenOnLow);
			AccessTools.Property(type, "ActivateValue").SetValue(thresholds, high);
			AccessTools.Property(type, "DeactivateValue").SetValue(thresholds, low);
		}

		/// <summary>What the fill fraction is measured against: the capacity the player set, less the storage margin.</summary>
		private static float Capacity(GameObject fridge)
		{
			Storage storage = fridge.GetComponent<Storage>();
			IUserControlledCapacity control = fridge.GetComponent<IUserControlledCapacity>();
			return Mathf.Min(storage.capacityKg, control.UserMaxCapacity) - storage.storageFullMargin;
		}

		/// <summary>Stores food or eats it until the fridge is at the fraction of its capacity.</summary>
		private static void Fill(TestContext t, GameObject fridge, float fraction)
		{
			Storage storage = fridge.GetComponent<Storage>();
			float target = fraction * Capacity(fridge);
			float now = storage.MassStored();
			if (target > now + 0.001f)
			{
				GameObject go = GameUtil.KInstantiate(Assets.GetPrefab(Food), Grid.CellToPosCCC(Grid.PosToCell(fridge), Grid.SceneLayer.Ore), Grid.SceneLayer.Ore);
				PrimaryElement element = go.GetComponent<PrimaryElement>();
				element.Mass = target - now;
				element.Temperature = Cold;
				go.SetActive(true);
				storage.Store(go);
			}
			else if (target < now - 0.001f)
			{
				PrimaryElement element = storage.items.First(item => item != null).GetComponent<PrimaryElement>();
				storage.ConsumeIgnoringDisease(element.gameObject.PrefabID(), (now - target) / element.MassPerUnit);
			}
			t.Log(fridge.PrefabID().Name + " filled to " + (storage.MassStored() / Capacity(fridge) * 100f).ToString("0.0") + "% (" + storage.MassStored().ToString("0.0") + " kg)");
		}

		private static int Port(GameObject fridge)
		{
			return fridge.GetComponent<LogicPorts>().GetOutputValue(FilteredStorage.FULL_PORT_ID);
		}

		/// <summary>Takes every fridge through the fills in turn, a game second apart, and checks the port after each.</summary>
		private static IEnumerator Steps(TestContext t, (float fill, int port)[] steps, params GameObject[] fridges)
		{
			foreach (var step in steps)
			{
				foreach (GameObject fridge in fridges)
					Fill(t, fridge, step.fill);
				yield return t.WaitGameSeconds(1);
				foreach (GameObject fridge in fridges)
					t.Assert.Eq(step.port, Port(fridge), fridge.PrefabID().Name + " at " + (step.fill * 100f).ToString("0") + "% with High 50 / Low 20 sends " + (step.port == 1 ? "green" : "red"));
			}
		}

		/// <summary>Where a component sits among the building's components.</summary>
		private static int IndexOf(GameObject go, Type type)
		{
			Component[] all = go.GetComponents<Component>();
			for (int i = 0; i < all.Length; i++)
				if (type.IsInstanceOfType(all[i]))
					return i;
			return -1;
		}

		// High 50 / Low 20, green on high: green from 50% up until the fill drops to 20%.
		private static readonly (float, int)[] Hysteresis =
		{
			(0.60f, 1), // above High: green
			(0.35f, 1), // between, coming down: stays green
			(0.10f, 0), // below Low: red
			(0.35f, 0), // between, coming up: stays red
			(0.60f, 1), // above High again: green
		};

		[OniTest("FTE: a Freezer's full port follows High 50 / Low 20 with hysteresis, as the vanilla Refrigerator's does, and is red without power", Timeout = 90), Layer(0)]
		public static IEnumerator FreezerFollowsThresholds(TestContext t)
		{
			t.Speed(2);
			var rig = new Rig();
			yield return Box(t, rig, 10);
			GameObject freezer = t.World.Build("Freezer", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			GameObject fridge = t.World.Build("Refrigerator", rig.Inside.Cell(9, 0));
			yield return t.WaitGameSeconds(1);
			t.Assert.NotNull(freezer.GetComponent(FreezerComponentType(t)), "the Freezer building runs Psyko's Freezer component");
			SetThresholds(Thresholds(t, freezer), false, 50, 20);
			SetThresholds(Thresholds(t, fridge), false, 50, 20);
			yield return Power(t, rig, freezer, fridge);
			t.Assert.Eq(0, Port(freezer), "the empty Freezer sends red");
			t.Assert.Eq(0, Port(fridge), "the empty Refrigerator sends red");

			yield return Steps(t, Hysteresis, freezer, fridge);
			yield return t.LookAt(Grid.PosToCell(freezer), 8f);
			yield return t.Screenshot("green-at-60");

			// Power goes: not operational, so red although it is above High.
			t.World.Destroy(rig.GeneratorObject);
			yield return t.WaitUntil(() => !freezer.GetComponent<Operational>().IsOperational, 20, "the Freezer to lose power");
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, Port(freezer), "the Freezer at 60% without power sends red");
			t.Assert.Eq(0, Port(fridge), "the Refrigerator at 60% without power sends red");
		}

		[OniTest("FTE: an Industrial Revolution Simple Fridge has the thresholds and its full port follows High 50 / Low 20", Timeout = 90), Layer(0)]
		public static IEnumerator IndustrialRevolutionFridge(TestContext t)
		{
			t.Speed(2);
			t.Assert.NotNull(Assets.GetBuildingDef("SimpleFridge"), "Industrial Revolution's SimpleFridge is registered");
			var rig = new Rig();
			yield return Box(t, rig, 6);
			GameObject fridge = t.World.Build("SimpleFridge", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			Component thresholds = Thresholds(t, fridge);
			t.Log("SimpleFridge components: thresholds at " + IndexOf(fridge, thresholds.GetType()) + ", Refrigerator at " + IndexOf(fridge, typeof(Refrigerator)));
			SetThresholds(thresholds, false, 50, 20);
			yield return Power(t, rig, fridge);
			t.Assert.Eq(0, Port(fridge), "the empty Simple Fridge sends red");
			yield return Steps(t, new[] { (0.60f, 1), (0.35f, 1), (0.10f, 0) }, fridge);
		}

		// Storage Pod Redux leaves its Cool Pod out while Industrial Revolution's Fridge Pod is on,
		// so this one runs in its own loadout: -With storage-pod-redux -Steam 2959340381.
		[OniTest("FTE: a Storage Pod Redux Cool Pod has the thresholds and its full port follows High 50 / Low 20", Timeout = 90), Layer(0)]
		public static IEnumerator StoragePodReduxCoolPod(TestContext t)
		{
			if (Assets.GetBuildingDef("CoolPodConfig") == null)
				t.Skip("Storage Pod Redux's Cool Pod is not registered (Storage Pod Redux not loaded, or left out for Industrial Revolution's Fridge Pod)");
			t.Speed(2);
			var rig = new Rig();
			yield return Box(t, rig, 6);
			GameObject pod = t.World.Build("CoolPodConfig", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			Component thresholds = Thresholds(t, pod);
			t.Log("CoolPodConfig components: thresholds at " + IndexOf(pod, thresholds.GetType()) + ", Refrigerator at " + IndexOf(pod, typeof(Refrigerator)));
			SetThresholds(thresholds, false, 50, 20);
			yield return Power(t, rig, pod);
			t.Assert.Eq(0, Port(pod), "the empty Cool Pod sends red");
			yield return Steps(t, Hysteresis, pod);
		}

		[OniTest("FTE: a vanilla Mini Fridge has the thresholds and its full port follows High 50 / Low 20", Timeout = 90), Layer(0)]
		public static IEnumerator MiniFridge(TestContext t)
		{
			if (Assets.GetBuildingDef("MiniFridge") == null)
				t.Skip("the Mini Fridge is not registered (it comes with The Aquatic Planet Pack)");
			t.Speed(2);
			var rig = new Rig();
			yield return Box(t, rig, 6);
			GameObject fridge = t.World.Build("MiniFridge", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			Component thresholds = Thresholds(t, fridge);
			t.Assert.True(IndexOf(fridge, thresholds.GetType()) < IndexOf(fridge, typeof(ThresholdsRefresh)), "the refresh comes after the thresholds component");
			SetThresholds(thresholds, false, 50, 20);
			yield return Power(t, rig, fridge);
			t.Assert.Eq(0, Port(fridge), "the empty Mini Fridge sends red");
			yield return Steps(t, Hysteresis, fridge);
		}

		// Run with -Steam 2959340381,3756115269 (needs The Aquatic Planet Pack).
		[OniTest("FTE: a Mini Freezer has the thresholds and its full port follows High 50 / Low 20", Timeout = 90), Layer(0)]
		public static IEnumerator MiniFreezer(TestContext t)
		{
			if (Assets.GetBuildingDef("MiniFreezer") == null)
				t.Skip("the Mini Freezer is not registered (the Mini Freezer mod, Workshop 3756115269, not loaded)");
			t.Speed(2);
			var rig = new Rig();
			yield return Box(t, rig, 6);
			GameObject freezer = t.World.Build("MiniFreezer", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			Component thresholds = Thresholds(t, freezer);
			t.Assert.True(IndexOf(freezer, thresholds.GetType()) < IndexOf(freezer, typeof(ThresholdsRefresh)), "the refresh comes after the thresholds component");
			SetThresholds(thresholds, false, 50, 20);
			yield return Power(t, rig, freezer);
			t.Assert.Eq(0, Port(freezer), "the empty Mini Freezer sends red");
			yield return Steps(t, Hysteresis, freezer);
		}

		/// <summary>
		/// What happens on a spawn: the fridge's own OnSpawn sends its plain full signal before the
		/// thresholds component has registered (LastSetFlag null, as ThresholdsBase.OnSpawn leaves
		/// it), and the refresh, spawning after both, has to send the thresholds' signal. On a load the
		/// power usually comes on later and recomputes the port anyway; the refresh is what fixes a
		/// fridge that is operational at spawn, so this sets up that state on a powered fridge and
		/// runs the refresh's own code.
		/// </summary>
		[OniTest("FTE: the refresh replaces a fridge's stale plain full signal with the thresholds' one (Freezer and Industrial Revolution Simple Fridge)", Timeout = 90), Layer(0)]
		public static IEnumerator RefreshResendsStalePort(TestContext t)
		{
			t.Speed(2);
			t.Assert.NotNull(Assets.GetBuildingDef("SimpleFridge"), "Industrial Revolution's SimpleFridge is registered");
			MethodInfo refresh = AccessTools.Method(AccessTools.TypeByName("FridgeThresholdsExtended.ThresholdsCompat"), "RefreshPort");
			t.Assert.NotNull(refresh, "ThresholdsCompat.RefreshPort, which ThresholdsRefresh.OnSpawn runs");
			var rig = new Rig();
			yield return Box(t, rig, 10);
			GameObject freezer = t.World.Build("Freezer", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			GameObject fridge = t.World.Build("SimpleFridge", rig.Inside.Cell(9, 0));
			yield return t.WaitGameSeconds(1);
			GameObject[] both = { freezer, fridge };
			foreach (GameObject go in both)
			{
				t.Assert.NotNull(go.GetComponent<ThresholdsRefresh>(), go.PrefabID().Name + " has the refresh");
				SetThresholds(Thresholds(t, go), false, 50, 20);
			}
			yield return Power(t, rig, both);
			yield return Steps(t, new[] { (0.60f, 1), (0.35f, 1) }, both);

			// The spawn state: the plain signal (35% is not full: red) and no flag sent yet.
			foreach (GameObject go in both)
			{
				Component thresholds = Thresholds(t, go);
				AccessTools.Property(thresholds.GetType(), "LastSetFlag").SetValue(thresholds, null);
				go.GetComponent<LogicPorts>().SendSignal(FilteredStorage.FULL_PORT_ID, 0);
			}
			yield return t.WaitGameSeconds(1);
			foreach (GameObject go in both)
				t.Assert.Eq(0, Port(go), go.PrefabID().Name + " sends the stale red with nothing to correct it");

			foreach (GameObject go in both)
				refresh.Invoke(null, new object[] { go });
			yield return t.WaitGameSeconds(1);
			foreach (GameObject go in both)
			{
				t.Assert.Eq(1, Port(go), go.PrefabID().Name + " at 35% on its way down sends green after the refresh");
				Component thresholds = Thresholds(t, go);
				t.Assert.Eq(true, AccessTools.Property(thresholds.GetType(), "LastSetFlag").GetValue(thresholds), go.PrefabID().Name + "'s thresholds recorded the green as sent");
			}
		}

		[OniTest("FTE: copy settings from one Freezer to another carries green on low, High and Low", Timeout = 60), Layer(0)]
		public static IEnumerator FreezerCopySettings(TestContext t)
		{
			t.Speed(2);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			GameObject source = t.World.Build("Freezer", inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			GameObject target = t.World.Build("Freezer", inside.Cell(9, 0));
			yield return t.WaitGameSeconds(1);
			Component from = Thresholds(t, source);
			Component to = Thresholds(t, target);
			SetThresholds(from, true, 70, 30);
			t.Assert.Eq(false, Setting(to, "SendGreenOnLow"), "the target starts with green on high");
			yield return t.WaitGameSeconds(1);

			t.Assert.NotNull(source.GetComponent<CopyBuildingSettings>(), "the Freezer offers copy settings");
			bool applied = CopyBuildingSettings.ApplyCopy(target.GetComponent<KPrefabID>(), source, source.GetComponent<KPrefabID>(), source.GetComponent<CopyBuildingSettings>());
			t.Assert.True(applied, "the game applied the copy");
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(true, Setting(to, "SendGreenOnLow"), "green on low was copied");
			t.Assert.Eq(70f, Setting(to, "ActivateValue"), "High was copied");
			t.Assert.Eq(30f, Setting(to, "DeactivateValue"), "Low was copied");
		}

		[OniTest("FTE: (round trip 1/2) a powered Freezer at 35% on its way down from 60% (green, High 50 / Low 20) is saved to the run folder", Timeout = 90), Layer(0)]
		public static IEnumerator WriteRoundTrip(TestContext t)
		{
			t.Speed(2);
			var rig = new Rig();
			yield return Box(t, rig, 6);
			int cell = rig.Inside.Cell(5, 0);
			GameObject freezer = t.World.Build("Freezer", cell);
			yield return t.WaitGameSeconds(1);
			Type freezerType = FreezerComponentType(t);
			Component thresholds = Thresholds(t, freezer);
			int thresholdsAt = IndexOf(freezer, thresholds.GetType()), freezerAt = IndexOf(freezer, freezerType);
			t.Log("built Freezer components: thresholds at " + thresholdsAt + ", Freezer at " + freezerAt);
			t.Assert.True(thresholdsAt >= 0 && thresholdsAt < IndexOf(freezer, typeof(ThresholdsRefresh)), "the refresh comes after the thresholds component, so it sends the Freezer's port again once the thresholds are registered");
			SetThresholds(thresholds, false, 50, 20);
			// TreeFilterable.OnSpawn drops, on a load, stored items whose tag is not a discovered resource
			// of the storage's categories; the side screen likewise lists only discovered food. Discover
			// the food under the game's own category for it, then tick it as the side screen does.
			Tag food = new Tag(Food);
			Tag category = DiscoveredResources.GetCategoryForEntity(Assets.GetPrefab(Food).GetComponent<KPrefabID>());
			Storage freezerStorage = freezer.GetComponent<Storage>();
			bool Listed() => freezerStorage.storageFilters.Any(f => DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(f).Contains(food));
			DiscoveredResources.Instance.Discover(food, category);
			t.Assert.True(Listed(), Food + " is a discovered resource of one of the Freezer's storage categories");
			TreeFilterable filter = freezer.GetComponent<TreeFilterable>();
			filter.AddTagToFilter(food);
			t.Assert.True(filter.ContainsTag(new Tag(Food)), "the Freezer's food filter allows " + Food);
			yield return t.WaitGameSeconds(1);
			yield return Power(t, rig, freezer);
			yield return Steps(t, new[] { (0.60f, 1), (0.35f, 1) }, freezer);

			File.WriteAllText(RoundTripCellPath, cell.ToString());
			string path = RoundTripPath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (FreshSave) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
		}

		/// <summary>
		/// The green is checked once the saved generator powers the Freezer, and power-on recomputes
		/// the port, so this passes with or without the refresh; RefreshResendsStalePort is the test
		/// of what the refresh sends.
		/// </summary>
		[OniTest("FTE: (round trip 2/2) after the load the refresh still follows the thresholds component, the thresholds and the 35% fill are kept, and once powered it sends green", Timeout = 60, FreshSave = true), Layer(0)]
		public static IEnumerator LoadKeepsOrderFillAndGreen(TestContext t)
		{
			string loaded = Session.SavePath;
			t.Cleanup(() =>
			{
				string original = Directory.GetFiles(Session.Dir, "fixture.sav").FirstOrDefault();
				if (original != null)
					Session.SavePath = original;
				if (File.Exists(RoundTripPath))
					File.Delete(RoundTripPath);
				if (File.Exists(RoundTripCellPath))
					File.Delete(RoundTripCellPath);
			});
			t.Assert.True(loaded.EndsWith("fte-roundtrip.sav"), "this test started from the round trip save (" + loaded + ")");
			int cell = int.Parse(File.ReadAllText(RoundTripCellPath));
			GameObject freezer = t.World.At(cell, ObjectLayer.Building);
			t.Assert.NotNull(freezer, "a building is at " + Cells.Describe(cell));
			t.Assert.NotNull(freezer.GetComponent(FreezerComponentType(t)), "the building at " + Cells.Describe(cell) + " is the Freezer");
			Component thresholds = Thresholds(t, freezer);

			Storage storage = freezer.GetComponent<Storage>();
			Operational operational = freezer.GetComponent<Operational>();
			int thresholdsAt = IndexOf(freezer, thresholds.GetType()), freezerAt = IndexOf(freezer, FreezerComponentType(t));
			t.Log("loaded Freezer components: thresholds at " + thresholdsAt + ", Freezer at " + freezerAt);
			t.Assert.True(thresholdsAt >= 0 && thresholdsAt < IndexOf(freezer, typeof(ThresholdsRefresh)), "the loaded Freezer has the refresh after the thresholds component");
			t.Assert.Eq(50f, Setting(thresholds, "ActivateValue"), "High is still 50");
			t.Assert.Eq(20f, Setting(thresholds, "DeactivateValue"), "Low is still 20");

			yield return t.WaitUntil(() => operational.IsOperational, 20, "the loaded Freezer to be powered by the saved generator");
			yield return t.WaitGameSeconds(1);
			yield return t.LookAt(Grid.PosToCell(freezer), 8f);
			yield return t.Screenshot("after-load");
			float fill = storage.MassStored() / Capacity(freezer);
			t.Assert.True(Mathf.Abs(fill - 0.35f) < 0.01f, "the loaded Freezer holds the 35% it was saved with (" + (fill * 100f).ToString("0.0") + "%)");
			t.Assert.Eq(1, Port(freezer), "the loaded Freezer at 35%, saved green on its way down, sends green with no storage change");
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(1, Port(freezer), "and still sends green two game seconds later");
		}
	}
}
