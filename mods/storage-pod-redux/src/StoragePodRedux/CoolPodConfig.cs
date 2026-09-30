using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

namespace StoragePodRedux
{
	/// <summary>
	/// A 1x1 fridge that can be built anywhere. The component set is the vanilla Mini Fridge's
	/// (MiniFridgeConfig, the rocket fridge): FoodStorage + Refrigerator + RefrigeratorController
	/// for the cooling and energy-saver logic, and an unrestricted RocketUsageRestriction so it
	/// keeps working in flight. The prefab ID is the one Skyrunner's Cool Pod used.
	/// </summary>
	public sealed class CoolPodConfig : IBuildingConfig
	{
		public const string ID = "CoolPodConfig";
		public const string Anim = "cool_pod_kanim";

		private const float PowerW = 60f;
		private const float EnergySaverW = 10f;

		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, Anim, 30, 10f,
				TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere,
				noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1);
			def.RequiresPowerInput = true;
			def.AddLogicPowerPort = false;
			def.EnergyConsumptionWhenActive = PowerW;
			def.SelfHeatKilowattsWhenActive = 0.125f;
			def.ExhaustKilowattsWhenActive = 0f;
			def.LogicOutputPorts = new List<LogicPorts.Port>
			{
				LogicPorts.Port.OutputPort(FilteredStorage.FULL_PORT_ID, new CellOffset(0, 0),
					STRINGS.BUILDINGS.PREFABS.REFRIGERATOR.LOGIC_PORT,
					STRINGS.BUILDINGS.PREFABS.REFRIGERATOR.LOGIC_PORT_ACTIVE,
					STRINGS.BUILDINGS.PREFABS.REFRIGERATOR.LOGIC_PORT_INACTIVE)
			};
			def.Floodable = false;
			def.ViewMode = OverlayModes.Power.ID;
			def.AudioCategory = "Metal";
			def.AudioSize = "small";
			def.AddSearchTerms(SEARCH_TERMS.FRIDGE);
			return def;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			SoundEventVolumeCache.instance.AddVolume(Anim, "Refrigerator_open", NOISE_POLLUTION.NOISY.TIER1);
			SoundEventVolumeCache.instance.AddVolume(Anim, "Refrigerator_close", NOISE_POLLUTION.NOISY.TIER1);
			go.AddTag(RoomConstraints.ConstraintTags.KitchenRefrigerator);
			Storage storage = go.AddOrGet<Storage>();
			storage.showInUI = true;
			storage.showDescriptor = true;
			storage.storageFilters = STORAGEFILTERS.FOOD;
			storage.allowItemRemoval = true;
			storage.capacityKg = Options.Instance.CoolPodCapacity;
			storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
			storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
			storage.showCapacityStatusItem = true;
			Prioritizable.AddRef(go);
			go.AddOrGet<TreeFilterable>().allResourceFilterLabelString = UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON_EDIBLES;
			go.AddOrGet<FoodStorage>();
			go.AddOrGet<Refrigerator>();
			RefrigeratorController.Def controller = go.AddOrGetDef<RefrigeratorController.Def>();
			controller.powerSaverEnergyUsage = EnergySaverW;
			controller.coolingHeatKW = 0.1875f;
			controller.steadyHeatKW = 0f;
			go.AddOrGet<UserNameable>();
			go.AddOrGet<DropAllWorkable>();
			go.AddOrGetDef<RocketUsageRestriction.Def>().restrictOperational = false;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGetDef<StorageController.Def>();
		}
	}
}
