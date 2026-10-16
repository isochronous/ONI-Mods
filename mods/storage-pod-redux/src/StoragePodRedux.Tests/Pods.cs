using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace StoragePodRedux.Tests
{
	/// <summary>The two buildings as a player places and uses them.</summary>
	public static class Pods
	{
		[OniTest("Storage Pod Redux: a Storage Pod placed with the build tool in open air holds what a Storage Bin holds", Timeout = 40), Layer(1)]
		public static IEnumerator StoragePod(TestContext t)
		{
			CellRect r = t.Rect(6, 4);
			int cell = r.Cell(3, 2);
			BuildingDef def = Assets.GetBuildingDef(StoragePodConfig.ID);
			t.Assert.Eq(1, def.WidthInCells * def.HeightInCells, "one cell");
			t.Assert.Eq(BuildLocationRule.Anywhere, def.BuildLocationRule, "built anywhere, no floor needed");
			t.Assert.Eq("RefinedMetal", def.CraftRecipe.Ingredients[0].tag.ToString(), "built of refined metal");
			t.Assert.Eq(TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0], def.CraftRecipe.Ingredients[0].amount, "of the Storage Bin's tier of mass");

			yield return Helpers.PlaceWithTool(t, StoragePodConfig.ID, cell);
			GameObject pod = Helpers.Finished(cell, StoragePodConfig.ID);
			t.Assert.NotNull(pod, "the pod stands in open air, above nothing");
			Storage storage = pod.GetComponent<Storage>();
			t.Assert.Eq(Options.Instance.StoragePodCapacity, storage.capacityKg, "capacity follows the option");
			t.Assert.Null(pod.GetComponent<EnergyConsumer>(), "it needs no power");
			t.Assert.NotNull(pod.GetComponent<StorageLocker>(), "it is a storage locker, filterable like the bin");

			Storage bin = Assets.GetBuildingDef(StorageLockerConfig.ID).BuildingComplete.GetComponent<Storage>();
			var expected = new HashSet<Tag>(bin.storageFilters);
			if (Options.Instance.StoragePodStoresFood)
				expected.UnionWith(TUNING.STORAGEFILTERS.FOOD);
			t.Assert.True(expected.SetEquals(storage.storageFilters), "the pod accepts the bin's categories" + (Options.Instance.StoragePodStoresFood ? " and food" : ", no food"));

			GameObject sand = t.Dupes.Drop(SimHashes.SandStone, 100f, r.Cell(1, 0));
			storage.Store(sand);
			t.Assert.Eq(100f, storage.MassStored(), "100 kg of sandstone goes in");
			t.Assert.Eq(storage.capacityKg - 100f, storage.RemainingCapacity(), "and the capacity shrinks by that");
		}

		[OniTest("Storage Pod Redux: a Cool Pod placed with the build tool completes and is a one-cell fridge", Timeout = 40), Layer(1)]
		public static IEnumerator CoolPod(TestContext t)
		{
			CellRect r = t.Rect(6, 4);
			int cell = r.Cell(3, 2);
			BuildingDef def = Assets.GetBuildingDef(CoolPodConfig.ID);
			t.Assert.Eq(1, def.WidthInCells * def.HeightInCells, "one cell");
			t.Assert.Eq(BuildLocationRule.Anywhere, def.BuildLocationRule, "built anywhere");
			t.Assert.Eq(60f, def.EnergyConsumptionWhenActive, "60 W");
			t.Assert.Eq("RefinedMetal", def.CraftRecipe.Ingredients[0].tag.ToString(), "built of refined metal");

			yield return Helpers.PlaceWithTool(t, CoolPodConfig.ID, cell);
			GameObject pod = Helpers.Finished(cell, CoolPodConfig.ID);
			t.Assert.NotNull(pod, "the Cool Pod stands (the original mod's crashed on completion)");
			yield return t.WaitGameSeconds(2);
			t.Assert.NotNull(pod.GetComponent<FoodStorage>(), "it has the food storage the fridge code needs");
			t.Assert.NotNull(pod.GetComponent<Refrigerator>(), "it is a Refrigerator");
			t.Assert.NotNull(pod.GetSMI<RefrigeratorController.StatesInstance>(), "with the Mini Fridge's controller");
			t.Assert.True(pod.GetComponent<KPrefabID>().HasTag(RoomConstraints.ConstraintTags.KitchenRefrigerator), "it counts as the Kitchen's refrigerator");
			t.Assert.Eq(Options.Instance.CoolPodCapacity, pod.GetComponent<Storage>().capacityKg, "capacity follows the option");
			t.Assert.Eq(cell, pod.GetComponent<LogicPorts>().GetPortCell(FilteredStorage.FULL_PORT_ID), "the Full automation output sits on its cell");
			RocketUsageRestriction.Def restriction = pod.GetDef<RocketUsageRestriction.Def>();
			t.Assert.NotNull(restriction, "it has a rocket usage restriction");
			t.Assert.False(restriction.restrictOperational, "which does not stop it working in flight");
		}

		/// <summary>A dev generator, the pod and (optionally) a vanilla Mini Fridge on one wire, powered.</summary>
		private static IEnumerator Powered(TestContext t, CellRect r, bool withFridge, GameObject[] made)
		{
			int generatorCell = r.Cell(1, 1);
			int podCell = r.Cell(3, 1);
			int fridgeCell = r.Cell(5, 1);
			t.World.Build("DevGenerator", generatorCell);
			t.World.Build(CoolPodConfig.ID, podCell);
			if (withFridge)
				t.World.Build("Refrigerator", fridgeCell);
			yield return t.WaitGameSeconds(1);
			UtilityDragSession wire = t.Tools.UtilityTool("Wire", SimHashes.Copper);
			yield return wire.Path(Enumerable.Range(1, withFridge ? 5 : 3).Select(x => r.Cell(x, 1)).ToArray());
			wire.Dispose();
			made[0] = Helpers.Finished(podCell, CoolPodConfig.ID);
			made[1] = withFridge ? Helpers.Finished(fridgeCell, "Refrigerator") : null;
			yield return t.WaitUntil(() => made[0].GetComponent<EnergyConsumer>().IsPowered && (made[1] == null || made[1].GetComponent<EnergyConsumer>().IsPowered), 15, "the fridges to be powered by the generator");
		}

		private static bool Is(GameObject fridge, bool cooling)
		{
			var smi = fridge.GetSMI<RefrigeratorController.StatesInstance>();
			return smi.IsInsideState(cooling ? smi.sm.operational.cooling : smi.sm.operational.steady);
		}

		private static void Hold(GameObject fridge, float kelvin)
		{
			foreach (GameObject item in fridge.GetComponent<Storage>().items)
				item.GetComponent<PrimaryElement>().Temperature = kelvin;
		}

		private static void Warm(TestContext t, GameObject fridge, float kelvin, int cell)
		{
			GameObject meal = t.Dupes.Drop(new Tag("BasicPlantFood"), 1f, cell);
			meal.GetComponent<PrimaryElement>().Temperature = kelvin;
			fridge.GetComponent<Storage>().Store(meal);
		}

		[OniTest("Storage Pod Redux: a powered Cool Pod idles at 10 W when empty, cools warm food and idles again when it is cold", Timeout = 90), Layer(1)]
		public static IEnumerator CoolPodCools(TestContext t)
		{
			CellRect r = t.Rect(8, 3);
			var made = new GameObject[2];
			yield return Powered(t, r, false, made);
			t.Speed(3);
			GameObject pod = made[0];
			EnergyConsumer power = pod.GetComponent<EnergyConsumer>();
			yield return t.WaitUntil(() => Is(pod, false), 15, "the empty pod to idle");
			t.Assert.Eq(10f, power.BaseWattageRating, "the pod idles at 10 W");
			t.Assert.True(pod.GetComponent<Operational>().IsActive, "and is running");

			Warm(t, pod, 300f, r.Cell(2, 0));
			// The pod's own thermal model pulls the food down within moments, so the food is held at
			// the temperature under test until the controller has looked.
			yield return t.WaitUntil(() => { Hold(pod, 300f); return Is(pod, true); }, 20, "the pod to start cooling the warm food");
			t.Log("cooling: " + power.BaseWattageRating + " W");
			yield return t.WaitUntil(() => { Hold(pod, 270f); return Is(pod, false); }, 20, "the pod to go back to idle once the food is cold");
			t.Assert.Eq(10f, power.BaseWattageRating, "at 10 W");
		}

		[OniTest("Storage Pod Redux: a Cool Pod's Full output goes high at capacity and low again when emptied", Timeout = 60), Layer(1)]
		public static IEnumerator CoolPodFull(TestContext t)
		{
			CellRect r = t.Rect(6, 3);
			var made = new GameObject[2];
			yield return Powered(t, r, false, made);
			GameObject pod = made[0];
			Storage storage = pod.GetComponent<Storage>();
			LogicPorts ports = pod.GetComponent<LogicPorts>();
			t.Assert.Eq(0, ports.GetOutputValue(FilteredStorage.FULL_PORT_ID), "empty: Full is low");
			GameObject food = t.Dupes.Drop(new Tag("BasicPlantFood"), storage.capacityKg, r.Cell(2, 0));
			food.GetComponent<PrimaryElement>().Temperature = 270f;
			storage.Store(food);
			yield return t.WaitUntil(() => ports.GetOutputValue(FilteredStorage.FULL_PORT_ID) == 1, 10, "the Full output to go high");
			storage.Drop(food);
			yield return t.WaitUntil(() => ports.GetOutputValue(FilteredStorage.FULL_PORT_ID) == 0, 10, "the Full output to go low again");
		}
	}
}
