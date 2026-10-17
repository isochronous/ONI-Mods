---
tags: storage-building, building-registration
doc_id: doc-4b5791f2e9a8
description: The Storage Pod and Cool Pod buildings of Storage Pod Redux, with their property tables, options, mod detection, registration gate, Harmony patches, business rules and the source file each lives in
applies_to:
  - "src/StoragePodRedux/**"
---

# Storage Pod Redux domain

## Overview
**Id:** blk-157f709a0f36  
**Tags:** mod-compatibility

Storage Pod Redux adds two compact 1x1 storage buildings that can be built anywhere, rockets included: the Storage Pod (solids) and the Cool Pod (a powered fridge). It is a code rewrite of Skyrunner's Storage Pod, keeping that mod's prefab IDs and art. The mod is `mods/storage-pod-redux`, static ID `Isochronous.StoragePodRedux`, source in `src/StoragePodRedux/`.

## Key Entities
**Id:** blk-bd15ff5c5a22

### Storage Pod (building)
**Id:** blk-f1d8d33bbdb4  
**Tags:** building-config, building-unlock, kanim-art  
**Description**: A Storage Bin in one cell, mirroring the vanilla `StorageLockerConfig`. Costs 100 kg of Refined Metal (`CONSTRUCTION_MASS_KG.TIER2`), needs no power, build rule `Anywhere`. Found in Base > Storage after the Storage Bin; unlocked by the `RefinedObjects` tech (Refined Renovations).  
**Location**: `src/StoragePodRedux/StoragePodConfig.cs`; art `src/StoragePodRedux/anim/assets/` (`storage_pod_kanim`).

| Property | Type | Description |
|----------|------|-------------|
| ID | string | `StoragePodConfig`, the original mod's prefab ID, so saves keep their pods |
| Capacity | float | `Options.StoragePodCapacity`, default 5000 kg |
| Filters | List<Tag> | `STORAGEFILTERS.STORAGE_LOCKERS_STANDARD`, plus `STORAGEFILTERS.FOOD` when `StoragePodStoresFood` is on |

**Relationships**: components `Storage`, `StorageLocker`, `CopyBuildingSettings` (group `GameTags.StorageLocker`), `UserNameable`, `RocketUsageRestriction.Def`, `StorageController.Def`.

### Cool Pod (building)
**Id:** blk-0e2ce168150c  
**Tags:** building-config, building-unlock, refrigerator, kanim-art  
**Description**: A fridge in one cell, built the way the game builds its Mini Fridge (`MiniFridgeConfig`). Costs 100 kg of Refined Metal, uses 60 W and 10 W once its contents are cold, build rule `Anywhere`, no DLC requirement. Found in Food > Storage after the Refrigerator; unlocked by the `Agriculture` tech.  
**Location**: `src/StoragePodRedux/CoolPodConfig.cs`; art `cool_pod_kanim`.

| Property | Type | Description |
|----------|------|-------------|
| ID | string | `CoolPodConfig`, the original mod's prefab ID |
| Capacity | float | `Options.CoolPodCapacity`, default 50 kg, food only (`STORAGEFILTERS.FOOD`) |
| Power | float | 60 W active (`PowerW`), 10 W saver (`EnergySaverW`) |
| Logic port | LogicPorts.Port | `FilteredStorage.FULL_PORT_ID`, the refrigerator's Full output, with the vanilla Refrigerator strings |

**Relationships**: components `Storage`, `FoodStorage`, `Refrigerator`, `RefrigeratorController.Def`, `TreeFilterable`, `UserNameable`, `DropAllWorkable`, `RocketUsageRestriction.Def` (`restrictOperational = false`, so it works in flight), `StorageController.Def`; tagged `RoomConstraints.ConstraintTags.KitchenRefrigerator` so it satisfies the Kitchen room.

### Options
**Id:** blk-96dad730c3ce  
**Tags:** mod-options, mod-strings  
**Description**: Saved with PLib `SingletonOptions<Options>`, shared config location, restart required.  
**Location**: `src/StoragePodRedux/Options.cs`

| Property | Type | Description |
|----------|------|-------------|
| StoragePodEnabled | bool | Default true |
| StoragePodCapacity | float | 100 to 50000 kg, default 5000 |
| StoragePodStoresFood | bool | Default false; the pod is not refrigerated |
| CoolPod | CoolPodMode | `Auto` (default), `Enabled`, `Disabled` |
| CoolPodCapacity | float | 10 to 5000 kg, default 50 |

Option titles and tooltips are keys into `ModStrings` (`STRINGS.STORAGEPODREDUX.OPTIONS...`), so the options screen follows the translation.

### Buildings (decision state)
**Id:** blk-a270aff56acd  
**Tags:** mod-compatibility, logging  
**Description**: Which of the two buildings this game gets, decided once all mods are loaded and read by the registration patch. `StoragePodEnabled`, `CoolPodEnabled`, `CoolPodReason` (logged), `IsEnabled(config|id)`; `IsEnabled` is false for both when the original mod is active.  
**Location**: `src/StoragePodRedux/Buildings.cs`

### Mod detection
**Id:** blk-ae5a3fd77fc3  
**Tags:** mod-compatibility  
**Description**: `ModDetection` finds an enabled mod by static ID and reads another mod's JSON config. `RonivansLegacy.FridgePodActive` decides whether Ronivan's Legacy - Industrial Revolution (`RonivansLegacy_ChemicalProcessing`) has its Fridge Pod active: module `DupesRefrigeration_Enabled` in `config.json` (default true) and `BuildingConfigurations.FridgePod.BuildingEnabled` (default true) in `RonivanAIO_BuildingConfig.json`, or `BuildingEnabledForce`. A missing file or key means the default.  
**Location**: `src/StoragePodRedux/ModDetection.cs`. The original mod's static ID is `Storage Pod` (`StoragePodReduxMod.OriginalStaticID`).

### Harmony patches
**Id:** blk-55d1a13d9ef6  
**Tags:** harmony-patching, building-unlock  
**Location**: `src/StoragePodRedux/Patches.cs`

| Target | Kind | Purpose |
|--------|------|---------|
| `BuildingConfigManager.RegisterBuilding` | Prefix | Returns false for a pod that is turned off, so it is absent from the build menu, tech tree and codex |
| `GeneratedBuildings.LoadGeneratedBuildings` | Prefix | Adds enabled pods to the plan screen (Base/storage after `StorageLockerConfig`, Food/storage after `RefrigeratorConfig`) |
| `Db.Initialize` | Postfix | Adds enabled pods to the `RefinedObjects` and `Agriculture` techs; a missing tech logs a warning |

## Business Rules
**Id:** blk-3f72d6cebb5f  
**Tags:** saved-state, mod-compatibility

- Both pods use the original mod's prefab IDs (`StoragePodConfig`, `CoolPodConfig`), so a save made with the original keeps its pods.
- If the original mod is enabled alongside, Redux registers no building and logs a warning; the user must disable one.
- A building turned off in the options is never registered; `IBuildingConfig.ForbidFromLoading` is not used because the game never calls it.
- Cool Pod Auto adds the pod unless Ronivan's Legacy is enabled with its Fridge Pod active; Enabled and Disabled force it.
- The Storage Pod holds the standard locker solids, and food only when the option is on. The Cool Pod holds food only.
- The Cool Pod drops to 10 W once everything inside is cold, cools with `coolingHeatKW` 0.1875 and self-heats 0.125 kW when active.
- Option changes take effect after a restart.

## Domain Terminology
**Id:** blk-4a9d0a5cfeeb  
**Tags:** refrigerator, mod-compatibility

| Term | Definition |
|------|-----------|
| Pod | Either building of this mod |
| Auto | The Cool Pod mode that defers to another mod's Fridge Pod |
| Original mod | Skyrunner's Storage Pod (`Storage Pod`), whose IDs and art this mod reuses |
| Energy saver | The refrigerator's lower power draw once its contents are cold |

## Key Code Paths
**Id:** blk-344beb8e3371  
**Tags:** mod-entry-point, harmony-patching

- **Startup**: `StoragePodReduxMod.OnLoad` inits PLib, registers `ModStrings` translations and the options; `OnAllModsLoaded` checks for the original mod and calls `Buildings.Decide`. See `src/StoragePodRedux/Mod.cs`.
- **Registration gate**: `RegisterBuilding` prefix -> `Buildings.IsEnabled(config)`. See `src/StoragePodRedux/Patches.cs`, `src/StoragePodRedux/Buildings.cs`.
- **Cool Pod mode**: `Buildings.Decide` -> `RonivansLegacy.FridgePodActive`. See `src/StoragePodRedux/Buildings.cs`, `src/StoragePodRedux/ModDetection.cs`.
- **Prefab setup**: `CreateBuildingDef`, `ConfigureBuildingTemplate`, `DoPostConfigureComplete` in `StoragePodConfig.cs` and `CoolPodConfig.cs`; capacities and filters read from `Options.Instance` at configure time.
- **Tests**: in-game `[OniTest]` suites in `src/StoragePodRedux.Tests/` (`Registration.cs`, `Pods.cs`, `Decisions.cs`, `Helpers.cs`).

## Related
**Id:** blk-5cc270c3cf27

- See also: [glossary](glossary.md)
