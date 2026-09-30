using System.Collections.Generic;
using System.Linq;
using STRINGS;
using TUNING;
using UnityEngine;

namespace StoragePodRedux
{
	/// <summary>
	/// A 1x1 Storage Bin that can be built anywhere (no floor needed, rockets included). The
	/// component set is the vanilla StorageLockerConfig's; the prefab ID is the one Skyrunner's
	/// Storage Pod used, so saves keep their pods when switching mods.
	/// </summary>
	public sealed class StoragePodConfig : IBuildingConfig
	{
		public const string ID = "StoragePodConfig";
		public const string Anim = "storage_pod_kanim";

		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, Anim, 30, 10f,
				TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere,
				noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
			def.Floodable = false;
			def.Overheatable = false;
			def.AudioCategory = "Metal";
			def.AudioSize = "small";
			def.AddSearchTerms(SEARCH_TERMS.STORAGE);
			return def;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			SoundEventVolumeCache.instance.AddVolume(Anim, "StorageLocker_Hit_metallic_low", NOISE_POLLUTION.NOISY.TIER1);
			Prioritizable.AddRef(go);
			Storage storage = go.AddOrGet<Storage>();
			storage.showInUI = true;
			storage.allowItemRemoval = true;
			storage.showDescriptor = true;
			storage.storageFilters = Options.Instance.StoragePodStoresFood
				? STORAGEFILTERS.STORAGE_LOCKERS_STANDARD.Concat(STORAGEFILTERS.FOOD).ToList()
				: new List<Tag>(STORAGEFILTERS.STORAGE_LOCKERS_STANDARD);
			storage.capacityKg = Options.Instance.StoragePodCapacity;
			storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
			storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
			storage.showCapacityStatusItem = true;
			storage.showCapacityAsMainStatus = true;
			go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.StorageLocker;
			go.AddOrGet<StorageLocker>();
			go.AddOrGet<UserNameable>();
			go.AddOrGetDef<RocketUsageRestriction.Def>();
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGetDef<StorageController.Def>();
		}
	}
}
