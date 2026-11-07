# Unit 01: what breaks against the U60 testing branch

## Game build

| | Value | Source |
|---|---|---|
| Install the build reads | `F:\SteamLibrary\steamapps\common\OxygenNotIncluded` | `common/oni-mod.props`: the first match is `F:\SteamLibrary\...`. `C:\Program Files (x86)\Steam\...\OxygenNotIncluded` does not exist. `ONI_GAME_FOLDER` is unset. |
| Steam branch | `BetaKey "public_testing"`, Steam buildid `26492567` | `F:\SteamLibrary\steamapps\appmanifest_457140.acf` |
| Klei build | **762230**, `BuildBranch = "preview"` | `KleiVersion.ChangeList` in the installed `Assembly-CSharp.dll` (DLLs dated 2026-11-06 23:36) |
| Previous build | 744825 (U59, `release`) | `F:\ONI-Frozen\APP` (the oni-freeze copy, `KleiVersion` 744825); the last Player.log line `Build: U59-744825-SCRPAN` was written before the DLLs updated |
| Unity | 6000.3.5 → 6000.3.17 | UnityPlayer.dll file versions |
| Harmony | 2.4.2.0 in both builds | `0Harmony.dll` |

The build reads the Steam install directly. No local copy of the game DLLs is used.

The testing-branch `Assembly-CSharp.dll` is a **debug (unoptimised) build**: it ships with its `.pdb`, it is 15.3 MB against 14.1 MB, and its IL is full of `nop`s and extra temporaries. All four transpilers match it (see below). The release U60 build will produce different IL, so the transpilers need checking again when it ships.

## Per-mod status

| Mod | Status | What | Where | Suggested fix (one line) |
|---|---|---|---|---|
| bitshifter | OK | 11/11 patch classes apply. The `LogicGate.UpdateState` transpiler finds its single `15 & ` mask (no warning logged). Every FieldRef (`colorOn`/`colorOff`) and `DetailsScreen` field resolves. One new U60 obsolete warning. | warning at `src/Bitshifter/Toolkit/RibbonDiagnosticPanel.cs:132,139` (`Subscribe<T>(IntraObjectHandler)`) | Optional: move to `Subscribe(int, Action<object,object>, object)` |
| corner-build-anything | OK (mod), tests BROKEN | Both transpilers apply and log "corner reach table in place". **The test project does not compile**: `Constructable.GetOffsets` now returns `IReadOnlyList<CellOffset>`. | `src/CornerBuildAnything.Tests/Reach.cs:99,132,165,202` (CS0266) | Declare `IReadOnlyList<CellOffset> now` (or `.ToArray()`), and update `Pockets.Has` to take it |
| detect-any-element | OK | 5 classes / 9 originals apply. All FieldRefs resolve (`targetFilterable`, `icon`, `samples`, `sampleIdx`) and `Switch.Toggle` resolves. Changes in the patched methods are only debug-compile rewrites. | none | none |
| dont-count-eggs-on-rails | OK | `OvercrowdingMonitor.UpdateRegion` applies. Its decompiled source is identical in U59 and U60. | none | none |
| fridge-thresholds-extended | OK | No attribute patches. `FilteredStorage.GetAmountStored` and `GetMaxCapacityMinusStorageMargin` resolve. The other targets belong to a third-party mod (`TypeByName`) and are not covered by this check. | `src/FridgeThresholdsExtended/ThresholdsCompat.cs` | none |
| jumper-bridges | OK, AT RISK (low) | 31 classes. 21 apply as they are. The other 10 skip only because `Prepare()` reads options that are false outside the game; resolved without `Prepare`, every target exists. All reflected fields resolve with the expected types (`HashSet<UtilityNetwork> connectedNetworks`, `float[] previousTilePower`, `conduitTargetLayer`, `items`/`links` on both `UtilityNetworkManager<…>`). U60 rewrote `OverlayModes.Logic.Update`/`Power.Update` (pooled sets, `CachedNetwork`), but the power tint formula the mod copies is unchanged. `LogicPorts.Create/DestroyPhysicalPorts` became public and Destroy now resets `cell = -1`; the mod only skips them for bridges. New obsolete warning. | warning at `src/JumperBridges/JumperBridge.cs:39,40` (`Unsubscribe(int, Action<object>)`) | Optional: keep the handle `Subscribe` returns and `Unsubscribe(handle)` |
| just-fall-already | OK | `Pathfinding.AddNavGrid` and `Door.SetWorldState` apply. Their source is unchanged. | none | none |
| motion-sensor-range | OK | 3/3 apply. All five `LogicDuplicantSensor` members resolve. U60 adds `IReconstructableState` (save and restore on rebuild) in `Constructable.FinishConstruction` and `Reconstructable`; the mod's own copy postfix still runs. New obsolete warnings. | warnings at `src/MotionSensorRange/RangeSlider.cs:93` (`Subscribe<T>`) and `:167` (`GameScenePartitioner.Add(...Action<object>)`) | Optional: move to the context-object overloads; consider `IReconstructableState` for the range copy |
| multichannel-critter-sensor | OK | 3/3 apply. All 12 side-screen fields resolve. New obsolete warning. | warning at `src/MultichannelCritterSensor/MultichannelCritterSensor.cs:133` | Optional: `Subscribe` context overload |
| natural-backwalls-everywhere | OK | 7 classes. 6 apply directly. `Game_OnSpawn_Census` fails only in the harness (ECall), and its target resolves. The `DoSettleSim` transpiler finds the `BackwallManager.UpdateFromSim` call (no warning). `BackwallManager` is unchanged apart from one expression-bodied property, so the vanilla settle NRE is still there and the guard is still needed. **`Cluster.BeginGeneration` now catches every exception itself and sets `Status = Failed`**, so the `Cluster_BeginGeneration_Patch` finalizer never sees one; it is redundant but harmless. | `src/NaturalBackwalls/WorldgenThreadPatch.cs` | Optional: drop or keep the finalizer as a no-op on U60 |
| no-magic-fridges | OK, AT RISK (low) | 2/2 apply. `active`/`storage` fields resolve, and so do `SimTemperatureTransfer.SimRegister`/`SimUnregister`. U60 adds `SimulatedTemperatureAdjuster.CoolingOnly` and `RefreshItems()`; `RefreshItems` calls the patched `OnItemSimRegistered` again, which the prefix already handles. Nothing seen sets `CoolingOnly` (uncertain: only the adjuster was decompiled). New obsolete warnings. | warnings at `src/NoMagicFridges/FridgeThermostat.cs:101,106` | Optional: context overloads; check in game whether vanilla fridges now set `CoolingOnly` |
| ovagros-overhead | **BROKEN** (compile) | `Crop.CropVal` is now a class with `List<ProductInfo> products`; `cropId` and `numProduced` are gone. `PlantAttributes.YieldAmount` is now `YieldFactor`. The obsolete `ExtendEntityToBasicPlant(..., string crop_id, ...)` overload remains. The runtime patches (stale 2026-10-19 DLL) all apply, 9/9. Tests were not built (they depend on the mod). | `src/OvagrosOverhead/VineBranchCeilingConfig.cs:68,78,82`, warnings `:59` and `VineMotherCeilingConfig.cs:72` | Follow vanilla `VineBranchConfig`: `crop = VineBranchConfig.HARVEST`, `YieldFactor` with modifier `1f`, and the `ExtendEntityToBasicPlant(go, CropVal, …)` overload |
| refined-building | OK | `ElementLoader.Load` applies. Only a debug rewrite in the method. | none | none |
| salty-surf | **BROKEN** (runtime) | `IGameObjectEffectDescriptor` changed: `GetDescriptors` is now an obsolete default method, and `MechanicalSurfboard` implements `CollectDescriptors(GameObject, List<Descriptor>)` with no explicit `IGameObjectEffectDescriptor.GetDescriptors`. `TargetMethod()` returns null, Harmony throws, and `base.OnLoad`'s `PatchAll` aborts, so the mod's patches do not load. The descriptor text is also now built through `KleiStringBuilder` with `MassFormatter`. | `src/SaltySurf/Mod.cs:121-145` | Postfix `MechanicalSurfboard.CollectDescriptors(GameObject, List<Descriptor> descriptors)` and edit `descriptors`; do not rely on matching the `GetFormattedMass` text (uncertain whether it equals `MassFormatter`); test `Intake.cs:48` uses the obsolete `GameUtil.GetAllDescriptors` |
| smart-weight-plate | OK | 4/4 apply. `DetailsScreen.sideScreens` resolves. New obsolete warnings. | warnings at `src/SmartWeightPlate/SmartWeightPlate.cs:132,152,154,156` | Optional: context overloads |
| smooth-borders | OK | The `DrawWorldBorder` transpiler finds both `GetIntSetting` reads (no "has changed" warning). The reverse patch builds. `GetIntSetting(string)` and `Border.Setting` resolve. | none | none |
| storage-pod-redux | OK | 3/3 apply. The `RegisterBuilding` source is unchanged. | none | none |
| sweep-zones | OK | 10/10 apply. All `DragTool`/`InterfaceTool` fields resolve. | none | none |
| upgrade-planner | **BROKEN** (compile), AT RISK | `MaterialSelector.OnSelectMaterial` went from **public to private** (public `TrySelectMaterial(Tag, Recipe, bool)` added, which checks mass first). The string-named patch still resolves: the stale 2026-10-22 DLL patches `OnSelectMaterial(Tag,Recipe,Boolean)` OK. `Reconstructable.TryCommenceReconstruct` now schedules on the **next game tick** (was next frame) and carries `IReconstructableState`, which `Orders.FollowRebuild` rides on (timing changed; check in game). | `src/UpgradePlanner/Patches.cs:77` (CS0117), and next (expected, hidden behind the first error) `src/UpgradePlanner/UI/MaterialPicker.cs:71` | Patch by string `"OnSelectMaterial"`; call it through `AccessTools`/`TrySelectMaterial` in MaterialPicker; test the rebuild flow paused and unpaused |
| vent-freeze-fix | OK | 7 classes. 4 apply directly. The 3 `Game.OnSpawn`/`LateUpdate` postfixes fail only in the harness (ECall), and their targets resolve. `WorldDamage.OnDigComplete` changed only its popup string building. | none | none |

Build pins: every `mod_info.yaml` (20 mods plus TestRunner, SaveRunner and WorldgenDriver) has `minimumSupportedBuild: 744825` and `APIVersion: 2`. They load on 762230 as they are. A fix that uses U60-only API (`CollectDescriptors`, `YieldFactor`, the `CropVal` list) would fail on U59, so raise `minimumSupportedBuild` to the U60 release build for those. Comments that cite 744825: `RangeSlider.cs:29`, `SweepZoneTool.cs:28`, `BiomeGroups.cs:53`, plus the READMEs of fridge-thresholds-extended and natural-backwalls. StreamingAssets `elements/` is identical in both builds. `worldgen/` differs only in `traits/MetalPoor.yaml` and `MetalRich.yaml`, so the BiomeGroups material lists still apply (DLC worldgen folders were not diffed).

## common/ (submodule) findings

| Item | Status | Evidence | Fix |
|---|---|---|---|
| `src/Translations.cs` | OK | `Localization.Initialize` resolves | none |
| TestRunner `Timelapser_WriteToPng_Patch` | **BROKEN (silent)** | U60 `Timelapser.RenderAndPrint` calls a new private `CapturePng(RenderTexture, string path, string secondaryPath, bool)` directly, and writes are queued on a thread (`QueueEncodeAndWrite`). `WriteToPng` is no longer on the timelapse or preview path, so the prefix that keeps test runs from writing images into the player's folder never runs. `previewScreenshot`/`previewSaveGamePath` still exist. | Patch `CapturePng` (or `RenderAndPrint`) by name and skip writes whose `path` is outside the run folder |
| TestRunner other 17 patches | OK | 15 apply, `App.LateUpdate`/`Game.OnSpawn`/`KInputManager.OnApplicationFocus` resolve (ECall harness limit, parameterless postfixes); `Reflect.cs` fields all resolve | none |
| TestRunner warnings | pre-existing | `FindObjectOfType`/`FindObjectsOfType` CS0618 (`Worldgen.cs:259,292`, `TestBehaviour.cs:219`) are a Unity 6 deprecation, already present on 6000.3.5 | none |
| SaveRunner, WorldgenDriver | OK | 8 and 7 classes, all targets resolve; changed methods differ only in debug rewrites | none |
| `oni-mod.props` | OK | resolves the testing-branch install on F: | none |
| mod_info pins | note | TestRunner, SaveRunner and WorldgenDriver are at 744825 | as above |

## PLib

The mods use PLib 4.25.0 from NuGet, ILRepack-merged; upgrade-planner uses its own `lib/PLib/PLib.dll`. All 24 game targets in PLib 4.25.0's own patches resolve (`plib-member-check.txt`), with one exception: `EquipmentTemplates.CreateEquipmentDef` now has **2 overloads**. In U59 it had 1. `PBuildingManager.Initialize` therefore catches an `AmbiguousMatchException` and logs it, and its `CreateEquipmentDef_Postfix` is not applied. None of these mods uses `PBuildingManager` (`grep PBuilding` finds nothing), so it does not affect them. `LightGridEmitter` is nested `LightGridManager.LightGridEmitter` in both builds; my resolver missed it, so it is not a game change. upgrade-planner's modified PLib was not checked separately (uncertain).

## Method and evidence

1. Ran `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none --no-incremental`.
2. Built **PatchCheck**, a .NET 8 host with Lib.Harmony 2.4.2. A net48 host could not load the game's types: "Non-abstract, non-.cctor method in an interface", meaning U60 uses default interface methods. For every `[HarmonyPatch]` class in each built mod DLL, PatchCheck runs `harmony.CreateClassProcessor(type).Patch()`. That runs Prepare/TargetMethod(s), binds parameters and runs transpilers on the real U60 IL, with the game's `Debug`/`DebugUtil`/`UnityEngine.Debug` log calls captured. Classes that Prepare skips are resolved again without Prepare. The two mods that no longer compile were checked through their last deployed DLLs in `Documents\Klei\...\mods\local`. Patch-class counts match the current source (9 and 6).
3. Ran a member check: a regex over all `.cs` files in mods/, common/src and common/tools/TestRunner for `AccessTools.Field/Method/Property/Inner/...(typeof(T), "name"|nameof)`, `FieldRefAccess<T,F>("name")` and `typeof(T).GetMethod/GetField/GetProperty("name")`. Each hit is resolved with Harmony's own `AccessTools` against the U60 assemblies, with field types checked for FieldRefs. The few variable-name targets were added by hand.
4. Built **ILDiff** (Cecil and ICSharpCode.Decompiler). For each of the 119 patched game methods it compares the decompiled C# in U59 (`F:\ONI-Frozen`, 744825) against U60. Raw IL differed in every method because of the debug build; the decompiled source differs in 59, and each of those diffs was read.

Scratch tools and outputs are in `C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad\`: `PatchCheck\`, `ILDiff\`, `run-check.sh`, `rebuild.log`, `patch-check.txt`, `member-check.txt`, `il-diff.txt`, `patch-check-tools.txt`, `il-diff-tools.txt`, `plib-member-check.txt`, `targets.tsv`.

### Build output (trimmed)

```
dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none -nologo --no-incremental
mods\corner-build-anything\src\CornerBuildAnything.Tests\Reach.cs(99,23): error CS0266: Cannot implicitly convert type 'IReadOnlyList<CellOffset>' to 'CellOffset[]'   (also 132,25 / 165,25 / 202,25)
mods\ovagros-overhead\src\OvagrosOverhead\VineBranchCeilingConfig.cs(68,53): error CS1061: 'Crop.CropVal' does not contain a definition for 'cropId'
mods\ovagros-overhead\src\OvagrosOverhead\VineBranchCeilingConfig.cs(78,61): error CS1061: 'PlantAttributes' does not contain a definition for 'YieldAmount'
mods\ovagros-overhead\src\OvagrosOverhead\VineBranchCeilingConfig.cs(82,61): error CS1061: 'PlantAttributes' does not contain a definition for 'YieldAmount'
mods\ovagros-overhead\src\OvagrosOverhead\VineBranchCeilingConfig.cs(82,82): error CS1061: 'Crop.CropVal' does not contain a definition for 'numProduced'
mods\upgrade-planner\src\UpgradePlanner\Patches.cs(77,67): error CS0117: 'MaterialSelector' does not contain a definition for 'OnSelectMaterial'
    307 Warning(s)   (278 CA1416 in the PreviewGen/IconGen asset tools, 27 CS0618, 2 CS0649)
    9 Error(s)
Not built (depend on a failed project): OvagrosOverhead.Tests, UpgradePlanner.Tests
```

CS0618 warnings in shipping code. All are new in U60: the frozen U59 build has no `[Obsolete]` on these members.

```
Bitshifter\Toolkit\RibbonDiagnosticPanel.cs(132,4),(139,4): KMonoBehaviour.Subscribe<T>(int, IntraObjectHandler<T>) obsolete
JumperBridges\JumperBridge.cs(39,4),(40,4): KMonoBehaviour.Unsubscribe(int, Action<object>) obsolete
MotionSensorRange\RangeSlider.cs(93,4): Subscribe<T> obsolete; (167,32): GameScenePartitioner.Add(string, object, Extents, layer, Action<object>) obsolete
MultichannelCritterSensor\MultichannelCritterSensor.cs(133,4): Subscribe<T> obsolete
NoMagicFridges\FridgeThermostat.cs(101,4): Subscribe<T>; (106,4): Unsubscribe<T>(int, IntraObjectHandler<T>, bool) obsolete
OvagrosOverhead\VineBranchCeilingConfig.cs(59,4), VineMotherCeilingConfig.cs(72,4): ExtendEntityToBasicPlant(... string crop_id ...) obsolete "Prefer CropVal type to string"
SmartWeightPlate\SmartWeightPlate.cs(132,4): Subscribe<T>; (152,24),(154,30),(156,28): GameScenePartitioner.Add(... int cell ..., Action<object>) obsolete
```

The obsolete overloads still work: each forwards to a `*_obsolete` twin, as the decompiled `KMonoBehaviour` shows. They are a removal risk, not a break. In test projects: `GameUtil.GetAllDescriptors` (SaltySurf.Tests `Intake.cs:48`) and `DlcManager.GetHighestActiveDlcId` (StoragePodRedux.Tests `Decisions.cs:70`), plus the Unity `FindObjectsOfType` warnings, which were already there.

### Patch-target check (PatchCheck patch, trimmed)

```
## SaltySurf
  SaltySurf.MechanicalSurfboard_GetDescriptors_Patch  FAIL HarmonyException: ... TargetMethod() returned an unexpected result: null
  (ElementLoader_Load, IsReady, MechanicalSurfboardConfig, OnWorkTick: OK)
## Bitshifter            -- 11 patch classes, 11 originals patched (LogicGate.UpdateState transpiler: no warning)
## CornerBuildAnything   -- 2/2, log: "[CornerBuildAnything] Constructable.OnSpawn: corner reach table in place" (same for Deconstructable)
## DetectAnyElement      -- 5 classes, 9 originals
## DontCountEggsOnRails  -- 1/1
## JumperBridges         -- 31 classes, 21 originals; 10 SKIPPED by Prepare, each "attribute target without Prepare -> <method>" resolved
## JustFallAlready 2/2   ## MotionSensorRange 3/3   ## MultichannelCritterSensor 3/3   ## NoMagicFridges 2/2   ## RefinedBuilding 1/1
## NaturalBackwalls      -- 7 classes, 6 originals; Game_OnSpawn_Census FAIL SecurityException: ECall methods must be packaged into a system module (harness only)
## SmartWeightPlate 4/4  ## SmoothBorders 1 + reverse patch  ## StoragePodRedux 3/3  ## SweepZones 10/10
## VentFreezeFix         -- 7 classes, 4 originals; Game.LateUpdate/Game.OnSpawn x3 FAIL ECall (harness only)
## TestRunner            -- 18 classes, 15 originals; App.LateUpdate, Game.OnSpawn, KInputManager.OnApplicationFocus FAIL ECall (harness only)
## OvagrosOverhead (stale 2026-10-19 DLL)  -- 9/9
## UpgradePlanner (stale 2026-10-22 DLL)   -- 6/6, incl. MaterialSelector.OnSelectMaterial(Tag, Recipe, Boolean)
## SaveRunner 8 classes, WorldgenDriver 7 classes: all targets resolve (ECall harness failures only)
```

"ECall methods must be packaged into a system module" comes from the .NET host failing to JIT Unity `InternalCall` code, not from the mod. Harmony had already resolved the original (the message names it), and every affected patch is a parameterless postfix. Their in-game behaviour is still unproven by this check.

### Member check (PatchCheck members, trimmed)

```
CHECKED 137 MISSING 3
MISSING salty-surf\src\SaltySurf\Mod.cs:125  GetMethod(MechanicalSurfboard, IGameObjectEffectDescriptor.GetDescriptors, NonPublic|Instance)  method not found
MISSING upgrade-planner\...\BlueprintsCompat.cs:134, :151  GetMethod(T, Invoke)  (false positive: generic delegate T)
OK  ... 134 others, e.g.
OK  jumper-bridges\...\Patches.cs:764  Field(OverlayModes.Power, previousTilePower)  non-public Single[]
OK  upgrade-planner\...\Variants.cs:37  Method(BuildTool, Activate, (BuildingDef, IList<Tag>))  public Void BuildTool.Activate(BuildingDef, IList`1)
OK  upgrade-planner\...\Orders.cs:18  FieldRefAccess(Reconstructable, selectedElementsTags, Tag[])  non-public Tag[]
OK  natural-backwalls\...\Patches.cs:24  GetProperty(SubWorld, backwallNoise).GetSetMethod(true)  set=True
OK  jumper-bridges BridgeKind.cs:71/72 (UtilityNetworkManager<LogicCircuitNetwork, LogicWire>/<ElectricalUtilityNetwork, Wire>, items/links)
OK  no-magic-fridges FridgeThermostat.cs:76 (SimTemperatureTransfer, SimRegister/SimUnregister)
```

### U59 → U60 source diff of patched methods (ILDiff, decompiled C#)

`SAME 60 CHANGED 59 MISSING 0`. Most of the 59 are debug-compile rewrites, such as `go.AddOrGet<X>().f = v` split into a local, or `foreach` written as `for`. The real changes are:

- `MaterialSelector.OnSelectMaterial`: `public` → `private` (upgrade-planner).
- `Timelapser.WriteToPng`: now a 1-line wrapper around `CapturePng`. The capture path no longer calls it (TestRunner).
- `Cluster.BeginGeneration`: the body moved to `GenerateClusterWorlds()`. It is now wrapped in `catch (Exception) { Status = Failed; ... ReportWorldGenError(ex); }` (natural-backwalls finalizer redundant).
- `Reconstructable.TryCommenceReconstruct`: `ScheduleNextFrame` → `ScheduleNextGameTick`, and it carries `IReconstructableState` (upgrade-planner, motion-sensor-range).
- `Constructable.FinishConstruction`: loads `IReconstructableState`. `Constructable.OnSpawn` uses `Subscribe(..., this)` and `GameScenePartitioner.Add_obsolete`; the transpiler is still OK.
- `LogicPorts.CreatePhysicalPorts`/`DestroyPhysicalPorts`: `private` → `public`. Destroy now resets `cell = -1` (jumper-bridges).
- `OverlayModes.Logic.Update`/`Power.Update`: rewritten with pooled sets, delegates and `CachedNetwork`. The fields and tint formula the mod reads are unchanged (jumper-bridges).
- `SimulatedTemperatureAdjuster.OnItemSimRegistered`: adds `|| (coolingOnly && stt.pe.Temperature <= temperature)` (no-magic-fridges).
- `PlanterSideScreen.SetResultDescriptions`: KleiStringBuilder and pooled descriptor lists, `component.CollectDescriptors` (ovagros; its prefix/finalizer swap is unaffected).
- `SeedPlantingStates.CheckValidPlotCell`: new `canPlantBackWall` branch (ovagros; prefix/finalizer swap unaffected).
- `BaseUtilityBuildTool.BuildPath`: adds a `SandboxModeActive` check. `Db.Initialize`: adds `SupportBuildingBoosts`. `MechanicalSurfboardWorkable.basePriority` TIER3 → TIER4. `Game.LateUpdate`: drops the Superluminal markers. All harmless.

## Does U60 make No Magic Fridges unnecessary?

**Verdict: partly fixed in vanilla.** Klei fixed the problem the mod was built for: a powered fridge no longer warms food that is colder than 1 °C. The rest of the mod's thermal model has no vanilla equivalent.

What the mod changes (README, `src/NoMagicFridges/Patches.cs`, `FridgeThermostat.cs`):

- A prefix on `SimulatedTemperatureAdjuster.OnItemSimRegistered` keeps the vanilla 1 °C reservoir off every item in a building with a `FridgeThermostat`.
- `FridgeThermostat.Sim1000ms` then runs its own model:
  - the contents share heat with each other;
  - a compressor with a fixed thermal mass (`BaseHeatCapacity`, about 25 kg of food) only cools, and only while the contents' average is above 1 °C;
  - an item colder than the room has its exchange with the room scaled by 0.01 (insulated tile).
- The README's stated problem is that the vanilla reservoir "runs both ways, so food you deep-froze … is dragged back up to 1 °C the moment the fridge gets power".

What changed in vanilla, U59 744825 against U60 762230 (decompiled):

`RefrigeratorController.StatesInstance` now sets the adjuster to cooling only. This is the only caller of `set_CoolingOnly`; a Cecil reference scan found only Compost and the fridge creating adjusters. The lines marked `+` are new in U60:

```csharp
temperatureAdjuster = new SimulatedTemperatureAdjuster(def.simulatedInternalTemperature, def.simulatedInternalHeatCapacity, def.simulatedThermalConductivity, storage);
+ temperatureAdjuster.CoolingOnly = true;
```

`SimulatedTemperatureAdjuster.OnItemSimRegistered` gives no reservoir to an item at or below the setpoint:

```csharp
// U59:
if (!active)
// U60:
if (!active || (coolingOnly && stt.pe.Temperature <= temperature))
{
    num = 0f; heat_capacity = 0f; thermal_conductivity = 0f;
}
SimMessages.ModifyElementChunkTemperatureAdjuster(stt.SimHandle, num, heat_capacity, thermal_conductivity);
```

That decision is re-made every 4 s while the fridge runs. The state machine is new in U60, and `RefreshTemperatureAdjuster()` calls `temperatureAdjuster.RefreshItems()`, which runs `OnItemSimRegistered` for each stored item:

```csharp
.Update("Refresh temperature adjuster", delegate(StatesInstance smi, float dt)
{
    smi.RefreshTemperatureAdjuster();
}, UpdateRate.SIM_4000ms, load_balance: true);
```

What this means:

- **Fixed in vanilla:** deep-frozen food is no longer pulled up to 1 °C when the fridge gets power. Items at or below 1 °C get a zero reservoir. Items above it get the 1 °C reservoir, which can only cool them toward 1 °C and never below.
- **Not in vanilla, still only from the mod:**
  - Contents share heat with each other. In vanilla each item is attached to its own reservoir.
  - The compressor has a finite thermal mass. Vanilla still gives every warm item the full per-item reservoir from the def, so a fridge full of warm food cools at the same per-item rate as one warm item.
  - Items colder than the room are insulated from it. In vanilla, an item below 1 °C now has no reservoir and exchanges with the room at the normal rate until it warms past 1 °C; from then on the reservoir holds it near 1 °C.
- **With the mod installed on U60:** its prefix still replaces vanilla for fridges, so players get the mod's model, and the new `CoolingOnly` branch never runs for them. `RefreshItems` calls the patched method every 4 s, and the prefix zeroes the reservoir each time, so the mod keeps working (see the per-mod row).
- **The README's "The problem" section is no longer true on U60.** If the mod stays, its pitch shifts to the shared-heat, thermal-mass and insulation model. Whether that is worth keeping is Jeremy's call; nothing in the code forces it either way.
- Uncertain: this is a static reading of the debug preview build. The 4 s refresh means an item that warms past 1 °C in the room can go up to 4 s without a reservoir. That was not measured in game.

## Uncertain

- The ECall-blocked postfixes (Game.OnSpawn, Game.LateUpdate, App.LateUpdate, KInputManager.OnApplicationFocus) have resolved targets but have not been proven in game.
- MaterialPicker.cs:71 is a predicted second compile error, not observed: the compiler stopped reporting on that project after the attribute error. The Ovagros and UpgradePlanner test projects may hold more errors once their mods compile.
- Whether `MassFormatter` text equals `GameUtil.GetFormattedMass`, which affects the SaltySurf fix.
- Whether anything sets `SimulatedTemperatureAdjuster.CoolingOnly`.
- upgrade-planner's own `lib/PLib/PLib.dll` was not checked separately. Third-party mod targets (`TypeByName`) were not checked.
- The release U60 build will be optimised, so the transpiler matches need checking again then.

## Report-back on the handoff brief

- Did not apply: the 17 copies of `try/catch` (one per mod's `error-handling.md`) and the code-style pointers (comments, braces, indentation). They govern writing code, not this read-only breakage check. The oni-moono lessons (`blk-062fcdde7835`, `blk-aa6fccf8d4a0`) are about process detection, not U60.
- Missing: nothing in either store records the frozen U59 copy at `F:\ONI-Frozen\APP` as the diff baseline for a game update, and nothing records that the build reads the game from F: rather than C:. Both were found by searching the disk. `mods:domains/game-interfaces.md` was flagged as unreachable and was not opened. It may list the game interfaces each mod touches, which is exactly this unit's subject.
- Mismatched reasons: none. Each pointer's one-line reason matched its record.
- Tooling friction: none. `ks read --store ...` worked first time.
