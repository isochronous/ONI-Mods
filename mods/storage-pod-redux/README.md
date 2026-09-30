# Storage Pod Redux

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod that adds two compact 1×1 storage buildings that can be built anywhere, including inside rockets:

- **Storage Pod**: a Storage Bin in one cell. Holds 5,000 kg of solids (configurable), needs no power, costs 100 kg of Refined Metal, and unlocks with Refined Renovations. Found under Base > Storage, next to the Storage Bin.
- **Cool Pod**: a fridge in one cell. Holds 50 kg of food (configurable), uses 60 W, dropping to 10 W once everything inside is cold, costs 100 kg of Refined Metal, and unlocks with Agriculture. Found under Food > Storage, next to the Refrigerator. Satisfies the Kitchen room's refrigerator requirement and has the same Full automation output.

It is a from-scratch rewrite of Skyrunner's [Storage Pod](https://steamcommunity.com/sharedfiles/filedetails/?id=1873476551), whose Cool Pod has crashed on completion since the Sweet Dreams update (`Refrigerator.OnSpawn` needs a `FoodStorage` component that mod never added). The Cool Pod here is built the way the game builds its own Mini Fridge, so it cools, saves power, and keeps working in flight. Prefab IDs are the original mod's (`StoragePodConfig`, `CoolPodConfig`), so a save made with the original keeps its pods when you switch; do not run both mods at once (if you do, this one steps aside and logs a warning).

## Options

In the mod's options (main menu > Mods > Storage Pod Redux > Options; changes need a restart):

- **Storage Pod**: enabled, capacity (kg), and whether it may hold food (unrefrigerated).
- **Cool Pod**: Auto / Enabled / Disabled, and capacity (kg). *Auto* adds the Cool Pod unless [Ronivan's Legacy - Industrial Revolution](https://steamcommunity.com/sharedfiles/filedetails/?id=3557584850) is enabled with its own Fridge Pod turned on (its `DupesRefrigeration_Enabled` module and `FridgePod` building settings are read from that mod's config files).

A building that is turned off is not registered at all, so it does not appear in the build menu, tech tree, or codex.

## Installing

As a local mod:

1. Download `StoragePodRedux-<version>.zip` from the [latest release](https://github.com/isochronous/storage-pod-redux/releases/latest).
2. Extract it into a new folder named `StoragePodRedux` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\StoragePodRedux`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/StoragePodRedux`
   - macOS: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/local/StoragePodRedux`
3. Start the game, enable the mod under **Mods** in the main menu, and let the game restart.

## Building

Requires the .NET SDK (8+). Shared build configuration lives in the [oni-mods-common](https://github.com/isochronous/oni-mods-common) submodule, so clone with `--recurse-submodules` (or run `git submodule update --init`). The game DLLs are referenced directly from the game install; override the path if yours differs:

```
dotnet build src/StoragePodRedux -c Release -p:GameFolder="<path-to>\OxygenNotIncluded"
```

A successful build merges [PLib](https://github.com/peterhaneve/ONIMods/tree/main/PLib) into the DLL and deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\StoragePodRedux` (disable with `-p:ModDeployFolder=none`).

Art: `tools/make_art.py` builds both kanims from `publish/sprite-storage.png` and `publish/sprite-cool.png`, drawing simple placeholder pods when those files are absent. Each kanim carries the `off`/`on`/`working` states the game's `StorageController` plays, a white-outline `place` ghost, the build-menu `ui` icon, and the 16-frame `meter` fill gauge (plus, for the Cool Pod, the 2-frame `logicmeter`) that `FilteredStorage` drives.

## Implementation notes

- `StoragePodConfig` mirrors the vanilla `StorageLockerConfig`; `CoolPodConfig` mirrors `MiniFridgeConfig` (`FoodStorage`, `Refrigerator`, `RefrigeratorController.Def`, `TreeFilterable`, an unrestricted `RocketUsageRestriction`, the kitchen-refrigerator room tag, and the Full logic port), with `BuildLocationRule.Anywhere` and no DLC requirement.
- Turning a building off is a Harmony prefix on `BuildingConfigManager.RegisterBuilding` that returns false for that config. (`IBuildingConfig.ForbidFromLoading` exists but nothing in the game calls it.)
- The Cool Pod's Auto decision and the original-mod check run in `OnAllModsLoaded`, where the enabled mod list is known; the mods are identified by staticID (`RonivansLegacy_ChemicalProcessing`, `Storage Pod`).
- Other patch points: `GeneratedBuildings.LoadGeneratedBuildings` (plan screen) and `Db.Initialize` (tech).
