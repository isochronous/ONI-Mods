using HarmonyLib;

namespace StoragePodRedux
{
	public static class Patches
	{
		// A building turned off in the options (or both, while the original mod is also
		// enabled) is never registered, so it is absent from the build menu, the tech tree
		// and the codex rather than merely hidden. IBuildingConfig.ForbidFromLoading looks
		// made for this but nothing in the game calls it.
		[HarmonyPatch(typeof(BuildingConfigManager), nameof(BuildingConfigManager.RegisterBuilding))]
		public static class BuildingConfigManager_RegisterBuilding_Patch
		{
			public static bool Prefix(IBuildingConfig config)
			{
				return Buildings.IsEnabled(config);
			}
		}

		// Build menu: Base > Storage right after the Storage Bin, Food > Storage right after
		// the Refrigerator.
		[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
		{
			public static void Prefix()
			{
				if (Buildings.IsEnabled(StoragePodConfig.ID))
					ModUtil.AddBuildingToPlanScreen("Base", StoragePodConfig.ID, "storage", StorageLockerConfig.ID, ModUtil.BuildingOrdering.After);
				if (Buildings.IsEnabled(CoolPodConfig.ID))
					ModUtil.AddBuildingToPlanScreen("Food", CoolPodConfig.ID, "storage", RefrigeratorConfig.ID, ModUtil.BuildingOrdering.After);
			}
		}

		// Research: Refined Renovations for the Storage Pod (with the Smart Storage Bin's
		// prerequisites), Agriculture for the Cool Pod (with the Refrigerator).
		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Postfix()
			{
				if (Buildings.IsEnabled(StoragePodConfig.ID))
					Unlock("RefinedObjects", StoragePodConfig.ID);
				if (Buildings.IsEnabled(CoolPodConfig.ID))
					Unlock("Agriculture", CoolPodConfig.ID);
			}

			private static void Unlock(string techId, string buildingId)
			{
				Tech tech = Db.Get().Techs.TryGet(techId);
				if (tech == null)
				{
					Debug.LogWarning("[StoragePodRedux] Tech '" + techId + "' not found; " + buildingId + " will be unlocked from the start");
					return;
				}
				if (!tech.unlockedItemIDs.Contains(buildingId))
					tech.unlockedItemIDs.Add(buildingId);
			}
		}
	}
}
