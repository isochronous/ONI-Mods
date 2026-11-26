using System;
using System.Collections;
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
	/// A Smart Storage Locker with Storage Refrigerator Thresholds (Workshop 2959340381) loaded, run
	/// with -Steam 2959340381. On every spawn the locker sends the game's own full signal before the
	/// thresholds component has registered; the round trip saves a locker whose thresholds answer
	/// (green) differs from the game's (red) and checks the port after the load, with nothing stored,
	/// removed or changed in between. The locker takes no power, so nothing else recomputes the port.
	///
	/// The fix-off proof: run WriteRoundTripFixOff instead of WriteRoundTrip. It removes Fridge
	/// Thresholds Extended's two spawn postfixes before saving, so the load runs without them and
	/// LoadSendsThresholds fails. LoadSendsThresholds puts them back when it ends, and WriteRoundTrip
	/// puts them back before it starts, so a reused game never keeps the fix off.
	/// </summary>
	public static class SmartStorage
	{
		private const string StorageThresholdsType = "StorageRefrigeratorThresholds.StorageThresholds";
		private const string ThresholdsBaseType = "StorageRefrigeratorThresholds.ThresholdsBase";
		private const string CompatType = "FridgeThresholdsExtended.ThresholdsCompat";
		private const string ModHarmonyId = "Isochronous.FridgeThresholdsExtended";
		private const float UserCapacity = 1000f;

		private static string RoundTripPath => Path.Combine(Session.Dir, "fte-smart-storage.sav");
		private static string RoundTripCellPath => Path.Combine(Session.Dir, "fte-smart-storage-cell.txt");

		/// <summary>The two methods the fix postfixes, each with the name of its postfix in ThresholdsCompat.</summary>
		private static (MethodBase original, string postfix)[] FixPatches(TestContext t)
		{
			Type thresholdsBase = AccessTools.TypeByName(ThresholdsBaseType);
			if (thresholdsBase == null)
				return new (MethodBase, string)[0];
			return new (MethodBase, string)[]
			{
				(AccessTools.DeclaredMethod(typeof(StorageLockerSmart), "OnSpawn"), "LockerOnSpawnPostfix"),
				(AccessTools.DeclaredMethod(thresholdsBase, "OnSpawn"), "ThresholdsOnSpawnPostfix"),
			};
		}

		private static bool HasFix(MethodBase original)
		{
			Patches info = Harmony.GetPatchInfo(original);
			return info != null && info.Postfixes.Any(patch => patch.owner == ModHarmonyId);
		}

		/// <summary>Puts back any of the fix's postfixes that WriteRoundTripFixOff removed.</summary>
		private static void RestoreFix(TestContext t)
		{
			Type compat = AccessTools.TypeByName(CompatType);
			if (compat == null)
				return;
			var harmony = new Harmony(ModHarmonyId);
			foreach (var (original, postfix) in FixPatches(t))
			{
				if (HasFix(original))
					continue;
				harmony.Patch(original, postfix: new HarmonyMethod(AccessTools.Method(compat, postfix)));
				t.Log("put back Fridge Thresholds Extended's postfix on " + original.DeclaringType.Name + "." + original.Name);
			}
		}

		/// <summary>The locker's thresholds component; with a null locker, only checks that the thresholds mod is loaded.</summary>
		private static Component Thresholds(TestContext t, GameObject locker)
		{
			Type type = AccessTools.TypeByName(StorageThresholdsType);
			t.Assert.NotNull(type, "Storage Refrigerator Thresholds is loaded (" + StorageThresholdsType + ")");
			if (locker == null)
				return null;
			Component thresholds = locker.GetComponent(type);
			t.Assert.NotNull(thresholds, "the Smart Storage Locker has the thresholds component");
			return thresholds;
		}

		private static object Setting(Component thresholds, string name)
		{
			return AccessTools.Property(thresholds.GetType(), name).GetValue(thresholds);
		}

		/// <summary>What the fill fraction is measured against: the capacity the player set, less the storage margin.</summary>
		private static float Capacity(GameObject locker)
		{
			Storage storage = locker.GetComponent<Storage>();
			return Mathf.Min(storage.capacityKg, locker.GetComponent<IUserControlledCapacity>().UserMaxCapacity) - storage.storageFullMargin;
		}

		/// <summary>Stores sandstone or takes it out until the locker is at the fraction of its capacity.</summary>
		private static void Fill(TestContext t, GameObject locker, float fraction)
		{
			Storage storage = locker.GetComponent<Storage>();
			float target = fraction * Capacity(locker);
			float now = storage.MassStored();
			if (target > now + 0.001f)
			{
				Vector3 pos = Grid.CellToPosCCC(Grid.PosToCell(locker), Grid.SceneLayer.Ore);
				GameObject ore = ElementLoader.FindElementByHash(SimHashes.SandStone).substance.SpawnResource(pos, target - now, 293.15f, byte.MaxValue, 0);
				storage.Store(ore);
			}
			else if (target < now - 0.001f)
				storage.ConsumeIgnoringDisease(SimHashes.SandStone.CreateTag(), now - target);
			t.Log("Smart Storage Locker filled to " + (storage.MassStored() / Capacity(locker) * 100f).ToString("0.0") + "% (" + storage.MassStored().ToString("0.0") + " kg)");
		}

		private static int Port(GameObject locker)
		{
			return locker.GetComponent<LogicPorts>().GetOutputValue(FilteredStorage.FULL_PORT_ID);
		}

		[OniTest("FTE: (Smart Storage round trip 1/2) a Smart Storage Locker at 35% on its way down from 60% (green, High 50 / Low 20) is saved to the run folder", Timeout = 90), Layer(0)]
		public static IEnumerator WriteRoundTrip(TestContext t)
		{
			Thresholds(t, null);
			RestoreFix(t);
			foreach (var (original, _) in FixPatches(t))
				t.Assert.True(HasFix(original), "Fridge Thresholds Extended patches " + original.DeclaringType.Name + "." + original.Name);
			yield return Write(t);
		}

		[OniTest("FTE: (Smart Storage round trip 1/2, fix off) the same save with Fridge Thresholds Extended's Smart Storage spawn postfixes removed, so round trip 2/2 fails", Timeout = 90, Explicit = true), Layer(0)]
		public static IEnumerator WriteRoundTripFixOff(TestContext t)
		{
			Thresholds(t, null);
			var harmony = new Harmony(ModHarmonyId);
			foreach (var (original, _) in FixPatches(t))
			{
				t.Assert.True(HasFix(original), "Fridge Thresholds Extended patches " + original.DeclaringType.Name + "." + original.Name);
				harmony.Unpatch(original, HarmonyPatchType.Postfix, ModHarmonyId);
				t.Assert.False(HasFix(original), "the postfix on " + original.DeclaringType.Name + "." + original.Name + " is removed");
			}
			t.Log("Fridge Thresholds Extended's Smart Storage spawn postfixes are off until round trip 2/2 ends");
			yield return Write(t);
		}

		private static IEnumerator Write(TestContext t)
		{
			t.Speed(2);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			int cell = inside.Cell(5, 0);
			GameObject locker = t.World.Build("StorageLockerSmart", cell);
			yield return t.WaitGameSeconds(1);
			Component thresholds = Thresholds(t, locker);
			locker.GetComponent<IUserControlledCapacity>().UserMaxCapacity = UserCapacity;
			// High 50 / Low 20, green on high, through the properties the side screen sets.
			Type type = thresholds.GetType();
			AccessTools.Property(type, "SendGreenOnLow").SetValue(thresholds, false);
			AccessTools.Property(type, "ActivateValue").SetValue(thresholds, 50f);
			AccessTools.Property(type, "DeactivateValue").SetValue(thresholds, 20f);
			// TreeFilterable.OnSpawn drops, on a load, stored items that are not a discovered resource
			// of the storage's categories: discover sandstone under its own category and tick it.
			Tag sandstone = SimHashes.SandStone.CreateTag();
			Tag category = DiscoveredResources.GetCategoryForEntity(Assets.GetPrefab(sandstone).GetComponent<KPrefabID>());
			Storage storage = locker.GetComponent<Storage>();
			DiscoveredResources.Instance.Discover(sandstone, category);
			t.Assert.True(storage.storageFilters.Any(f => DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(f).Contains(sandstone)), "sandstone is a discovered resource of one of the locker's storage categories");
			TreeFilterable filter = locker.GetComponent<TreeFilterable>();
			filter.AddTagToFilter(sandstone);
			t.Assert.True(filter.ContainsTag(sandstone), "the locker's filter allows sandstone");
			yield return t.WaitGameSeconds(1);

			Fill(t, locker, 0.60f);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1, Port(locker), "the locker at 60% with High 50 / Low 20 sends green");
			Fill(t, locker, 0.35f);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1, Port(locker), "the locker at 35% on its way down stays green");

			File.WriteAllText(RoundTripCellPath, cell.ToString());
			string path = RoundTripPath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (LoadSendsThresholds) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
		}

		[OniTest("FTE: (Smart Storage round trip 2/2) after the load the Smart Storage Locker at 35% sends the thresholds' green, not the game's own red, with nothing stored or changed", Timeout = 60, FreshSave = true), Layer(0)]
		public static IEnumerator LoadSendsThresholds(TestContext t)
		{
			string loaded = Session.SavePath;
			t.Cleanup(() =>
			{
				RestoreFix(t);
				string original = Directory.GetFiles(Session.Dir, "fixture.sav").FirstOrDefault();
				if (original != null)
					Session.SavePath = original;
				if (File.Exists(RoundTripPath))
					File.Delete(RoundTripPath);
				if (File.Exists(RoundTripCellPath))
					File.Delete(RoundTripCellPath);
			});
			t.Assert.True(loaded.EndsWith("fte-smart-storage.sav"), "this test started from the round trip save (" + loaded + ")");
			int cell = int.Parse(File.ReadAllText(RoundTripCellPath));
			GameObject locker = t.World.At(cell, ObjectLayer.Building);
			t.Assert.NotNull(locker, "a building is at " + Cells.Describe(cell));
			t.Assert.NotNull(locker.GetComponent<StorageLockerSmart>(), "the building at " + Cells.Describe(cell) + " is a Smart Storage Locker");
			Component thresholds = Thresholds(t, locker);
			t.Assert.Eq(50f, Setting(thresholds, "ActivateValue"), "High is still 50");
			t.Assert.Eq(20f, Setting(thresholds, "DeactivateValue"), "Low is still 20");
			float fill = locker.GetComponent<Storage>().MassStored() / Capacity(locker);
			t.Assert.True(Mathf.Abs(fill - 0.35f) < 0.01f, "the loaded locker holds the 35% it was saved with (" + (fill * 100f).ToString("0.0") + "%)");
			// The game's own answer, which it sends at spawn: below its own 100% bar and never activated, so red.
			StorageLockerSmart smart = locker.GetComponent<StorageLockerSmart>();
			t.Assert.Eq(false, AccessTools.Field(typeof(StorageLockerSmart), "activated").GetValue(smart), "the game's own state for this locker is not full (red), so it differs from the thresholds' green");

			yield return t.WaitGameSeconds(1);
			yield return t.LookAt(cell, 8f);
			yield return t.Screenshot("smart-storage-after-load");
			t.Assert.Eq(1, Port(locker), "the loaded locker at 35%, saved green on its way down, sends green with no storage change");
			t.Assert.Eq(true, Setting(thresholds, "LastSetFlag"), "the thresholds recorded the green as sent");
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(1, Port(locker), "and still sends green two game seconds later");
		}
	}
}
