# OM-011 research report: thresholds patch for Freezer and Industrial Revolution

Scratch abbreviations: `SP` = `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad`,
`WS` = `C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam`. Decompiles: `SP\ir-src` (new), `SP\freezer-src`,
`SP\srt-src`, `SP\vanilla\{Refrigerator,FilteredStorage,StorageLockerSmart,RationBox}.cs` (new, from the installed game).

## Versions checked
- Game: U59-744825 (Player.log "Build: U59-744825-SCRPAN"); Harmony in Managed has `AccessTools.TypeByName`, `FieldRefAccess`, `MethodDelegate(..., delegateArgs)`.
- Storage Refrigerator Thresholds: WS\2959340381, mod_info version 2025.1130.1, staticID `llunak.StorageRefrigeratorThresholds`, assembly `StorageRefrigeratorThresholds`. Shipped DLL matches `Source\Storage.cs` (SP\srt-src\...\OtherMods_Patch.cs has the same 11-entry list).
- Freezer: WS\2618339179, mod_info version 1.0.10, staticID `Freezer`, assembly `Freezer`.
- Industrial Revolution: WS\3557584850 "Ronivans Legacy - Industrial Revolution", version 1.20.1, staticID `RonivansLegacy_ChemicalProcessing`, assembly `RonivansLegacy_ChemicalProcessing.dll` (author line: "Ronivan, Sgt_Imalas", LauncherMetadata.json). It is the only Ronivan mod installed (grep of all 147 Steam mod.yaml files).
- mods.json: all three enabled (`enabledForDlc` ['', 'EXPANSION1_ID']); load order index Freezer 16, IR 84, Thresholds 140 (FastTrack 0).

## Q1. Industrial Revolution fridges (verified from decompile)
All in `RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesRefrigeration` (internal classes), registered in `BuildingDatabase.RegisterBuildings_DupesRefrigeration` (BuildingDatabase.cs:471-486) with no config toggle (always on).

| Config class | Building ID | Port | Component driving the port |
|---|---|---|---|
| FridgeSmallConfig | `SimpleFridge` | FULL (0,1) | vanilla `Refrigerator` (FridgeSmallConfig.cs:51,81) |
| FridgeLargeConfig | `AIO_FridgeLarge` | FULL (0,1) | vanilla `Refrigerator` (FridgeLargeConfig.cs:18,51,81) |
| FridgePodConfig | `FridgePod` | FULL (0,0) | vanilla `Refrigerator` (FridgePodConfig.cs:18,51,81) |
| HightechBigFridgeConfig | `HightechSmallFridge` (sic) | FULL (0,1) | vanilla `Refrigerator` (HightechBigFridgeConfig.cs:21,60,88) |
| HightechSmallFridgeConfig | `HightechBigFridge` (sic) | FULL (0,1) | vanilla `Refrigerator` (HightechSmallFridgeConfig.cs:21,62,102) |
| SpaceBoxConfig (DLC1 only) | `SpaceBox` | FULL (0,0) | vanilla `RationBox`, which never sends a signal (SP\vanilla\RationBox.cs has no port code) |

The two Hightech configs have their IDs swapped relative to class names; target by class, not by ID.
Related, not a fridge: `DupesLogistics.CabinetFrozenConfig` (`CabinetFrozen`) uses vanilla `StorageLockerSmart` with FULL as port 0 and a second energy-saver port (CabinetFrozenConfig.cs:45-48,72; EnergySaverLogicOutput.cs:52).
UpdateLogicCircuit equivalent: for the five fridges it is vanilla `Refrigerator.UpdateLogicCircuit` itself (SP\vanilla\Refrigerator.cs:101-108, private), which the thresholds mod already prefixes (Storage.cs:346-366). IR also calls `ModIntegrationHelper.AllStorageIntegations` (PreciseStorageControl, HysteresisStorage, NoManualDelivery by reflection; none refer to the thresholds mod).

## Q2. Does the thresholds mod list IR? (verified)
No. `OtherMods_Patch` (Storage.cs:385-400) lists `Psyko.Freezer.FreezerConfig, Freezer` and ten `Advanced_Refrigeration.*, Advanced Refrigeration` types: the old standalone Dupes Refrigeration mod. IR moved those buildings to the `RonivansLegacy_ChemicalProcessing...` namespace and assembly, so `Type.GetType` returns null and nothing is patched. Nothing else in the thresholds source names IR. IR source has no reference to the thresholds mod either.

## Q3. Patch per building
Thresholds API (all public, Storage.cs): `ThresholdsBase.Get(GameObject)` static (153), `UpdateLogicState(float)` (105), `IsActuallyOperational(Operational)` (164), `LastSetFlag` bool? property (40); `RefrigeratorThresholds` public class (285); `StorageThresholds` (189). Side screen shows for any object with `RefrigeratorThresholds` or `StorageThresholds` (SP\srt-src\...\ThresholdsSideScreen.cs:54-58), so adding the component is all the side screen needs.

**IR fridges (5 configs): add the component only.** Their component is vanilla `Refrigerator`, so the thresholds mod's own prefix and `OnCopySettings` postfix (Storage.cs:328-344) already handle them once `RefrigeratorThresholds` is on the prefab. Patch: for each type name `RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesRefrigeration.{FridgeSmall,FridgeLarge,FridgePod,HightechBig,HightechSmall}Config`, resolve with `AccessTools.TypeByName`, patch `DoPostConfigureComplete` with a **prefix** that does `go.AddComponent(rtType)` if `go.GetComponent(rtType) == null` (rtType = `TypeByName("StorageRefrigeratorThresholds.RefrigeratorThresholds")`). Prefix rather than postfix mirrors the thresholds mod's vanilla `RefrigeratorConfig` prefix (Storage.cs:369-377), so the component is ahead of `Refrigerator` and its `OnSpawn` fills `fastMap` before `Refrigerator.OnSpawn` calls `UpdateLogicCircuit` (Refrigerator.cs:67); with a postfix the first update after load takes the vanilla IsFull path until the next storage event (inferred from component spawn order, not tested). `OnSpawn` reads `LogicPorts.outputPortInfo[0]` (Storage.cs:137-143); port 0 is FULL on all five.
Alternative generic form (the no-magic-fridges pattern, mods/no-magic-fridges/src/NoMagicFridges/Patches.cs:38-56): a `GeneratedBuildings.LoadGeneratedBuildings` postfix adding `RefrigeratorThresholds` to every `BuildingComplete` that has `Refrigerator` and a FULL port. Covers any mod using vanilla `Refrigerator`, but always lands after `Refrigerator` (the first-update caveat above).
**SpaceBox: skip.** `RationBox` sends nothing; a thresholds component would show sliders that do nothing.
**CabinetFrozen (optional):** add `StorageThresholds` the same way; the thresholds `StorageLockerSmart.UpdateLogicAndActiveState` prefix (Storage.cs:243-264) then drives it, matching vanilla `SetActive` (SP\vanilla\StorageLockerSmart.cs:46-55). Ask whether it is in scope.

**Freezer: prefix `Psyko.Freezer.Freezer.UpdateLogicCircuit`.** The component is already added by the thresholds mod (Storage.cs:388,409-412), and `RefrigeratorThresholds.UpdateLogicCircuit` already calls the Freezer method by reflection when sliders move (Storage.cs:304-313), but the Freezer method is a copy of vanilla that sends `IsFull() & IsOperational` (SP\freezer-src\Psyko.Freezer\Freezer.cs:112-117), so the sliders never reach the port. Members: `Freezer` public class, `UpdateLogicCircuit` private instance, fields `filteredStorage`, `operational`, `ports` private (Freezer.cs:9-20). Patch via `harmony.Patch(AccessTools.Method(freezerType,"UpdateLogicCircuit"), prefix: ...)` with signature
`static bool Prefix(Component __instance, FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports)` (all vanilla types, so no compile-time reference to Freezer; Harmony injects `___` fields of the original's declaring type on manual patches too; `__instance` typed as a base class is inferred to be accepted, check at build).
Body, the same computation as the thresholds vanilla path (Storage.cs:351-365): `t = Get(go)`; null -> `return true`; `stored = GetAmountStored(fs)`, `cap = GetMaxCapacityMinusStorageMargin(fs)` (both private on FilteredStorage, SP\vanilla\FilteredStorage.cs:174-185, take them with `AccessTools.MethodDelegate<Func<FilteredStorage,float>>`, which the thresholds mod also does); `op = IsActuallyOperational(operational)`; `flag = UpdateLogicState(stored/cap) && op`; if `flag != LastSetFlag` send FULL and set `LastSetFlag`; `SetLogicMeter(flag)`; `return false`. Thresholds members are reached by reflection, bound once in OnAllModsLoaded: `Get` as `Func<GameObject,object>` (return covariance is fine), instance members via `MethodInfo.Invoke` / `PropertyInfo` or `MethodDelegate` with `delegateArgs` (exists in this Harmony). The method runs only on storage/operational events, so Invoke cost is negligible. Optional: postfix `Freezer.OnCopySettings` (private, Freezer.cs:94) copying `SendGreenOnLow/ActivateValue/DeactivateValue` as Storage.cs:330-343 does for Refrigerator; without it copy-settings carries capacity but not thresholds.
Guard against double handling: if `Harmony.GetPatchInfo(method)` already has a prefix from owner `llunak...` (a future thresholds release fixing Freezer itself), skip ours.

## Q4. Absent mods and timing
- Do all target lookups in `UserMod2.OnAllModsLoaded` (every mod's assembly is loaded by then; the thresholds mod uses the same hook, Mod.cs:17-21). `GeneratedBuildings.LoadGeneratedBuildings` (where configs run) comes later, so config patches applied there are in time; the no-magic-fridges postfix runs even later.
- Presence: `AccessTools.TypeByName` (searches all loaded assemblies, returns null when missing) for `StorageRefrigeratorThresholds.ThresholdsBase`, `...RefrigeratorThresholds`, `Psyko.Freezer.Freezer`, and each IR config. Optionally confirm with `mods` (`mod.IsEnabledForActiveDlc() && mod.staticID == ...`), as UpgradePlanner's BlueprintsCompat does (mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs:38-50).
- Thresholds missing: do nothing (log once). Freezer or IR missing: skip that group. Any member missing: warn and leave the target as is (the store's "Missing member: warn, leave vanilla" rule, e.g. blk-ca37f1585a91). Wrap each group in try/catch as SmartWeightPlate's `BetterAutomationOverlayCompat.Apply` does (mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs:22-58).
- No mod_info `requires`/load-order field is needed: OnAllModsLoaded runs after every mod's OnLoad regardless of list order, and nothing here depends on the order of other mods' OnAllModsLoaded (the thresholds mod adds the Freezer component from its own config postfix; ours only prefixes a method and adds a component with a null check).
- Never let a patch method's signature or a field type name a type from another mod's assembly; the JIT would fail the method when that mod is absent. Use `Component`/`object` and reflection only. No compile-time reference to the three DLLs, no copied code.

## Q5. oni-moono (SP\oni-moono\docs\ARCHITECTURE.md)
- D21 (404-427): a method patched after an earlier replacement that inlined it is handled by the host (`PatchDependents` rebuilds). `Freezer.UpdateLogicCircuit` is small (inline candidate) and called from `OnSpawn`, `UserMaxCapacity` setter and `UpdateLogicCircuitCB`; if another mod patched one of those first, moono rebuilds it. Nothing for us to do beyond patching at load time.
- D34 (733-760): patches made after code tiered up miss inlined calls. Patching in OnAllModsLoaded, before any world exists and before Freezer code has run, keeps clear of it. Do not defer the Freezer patch to game start.
- D13 (271-297): concerns value-type overrides and shared generics; neither applies (class methods, no generics).
- D19 (357-380): mod patch methods are kept from inlining and out of tail position; the patch must not use `Assembly.GetCallingAssembly` or fixed stack indices. Ours uses neither.
- D49 (1201-1220) and Load context (71): use `AccessTools.TypeByName` or assembly-qualified names; a bare `Type.GetType("X")` from inside a patch searches 0Harmony on CoreCLR. (The thresholds mod's own `Type.GetType("Psyko.Freezer.Freezer, Freezer")` is assembly-qualified, so it works there.)
- MOD_COMPATIBILITY.md rows 76 (IR), 99 (Freezer), 1620 (Thresholds): all three pass on moono individually.

## Q6. Repo pattern (verified)
- Mod at `mods/<name>/src/<Name>/` with `<Name>.csproj`, `mod.yaml`, `mod_info.yaml`, `Mod.cs`, `ModStrings.cs`, `translations/`; tests in `src/<Name>.Tests/` (TestRunner kit); `README.md`, `CHANGELOG.md`, `publish/` at mod root (mods/vent-freeze-fix).
- csproj (mods/vent-freeze-fix/src/VentFreezeFix/VentFreezeFix.csproj:1-24): imports `$(OniCommon)oni-mod.props` / `oni-mod.targets`; sets AssemblyName, RootNamespace, Version, `UsePLib`, `UseTranslations`, `ReferenceUnityUI`; Newtonsoft reference only for PLib options. `OniCommon` comes from root `Directory.Build.props`; common/oni-mod.props sets net48, C# 9, game path F:\SteamLibrary..., deploy to `Documents\Klei\OxygenNotIncluded\mods\local\<ProjectName>` (oni-mod.targets:118-130).
- mod.yaml: title, description, `staticID: "Isochronous.<Name>"`. mod_info.yaml: `supportedContent: ALL`, `minimumSupportedBuild: 744825`, version, `APIVersion: 2`.
- Mod.cs: `UserMod2.OnLoad` -> `PUtil.InitLibrary(false)`, `Translations.Register(harmony, typeof(ModStrings))`, optional `POptions`, log line with version (VentFreezeFix/Mod.cs:12-19).
- Optional patching of other Workshop mods already exists: SmartWeightPlate (Better Automation Overlay, Mod.cs:22-58), Bitshifter (OverlayLabelGuard.cs:26, Mod.cs:38-45), UpgradePlanner Compat/ (OtherModSettings.cs, ModCompat.cs, BlueprintsCompat.cs), StoragePodRedux Mod.cs:27. Copy SmartWeightPlate's shape: OnAllModsLoaded -> static Compat.Apply(harmony) with TypeByName, null checks, try/catch, `harmony.Patch`.

## Verified vs inferred
Verified by reading decompiled/installed code: everything cited with file:line, versions, enabled state, IDs, that IR fridges use vanilla Refrigerator, that the thresholds list misses IR, the thresholds API's access levels, the Freezer method/field names.
Inferred, needs an in-game test: that the side screen and sliders then drive the port on IR fridges and Freezer; the component-order effect of prefix vs postfix on the first signal after load; Harmony accepting `Component __instance` for a manual patch; that IR's swapped Hightech IDs do not matter.

## Brief feedback for the coordinator
- The store brief listed the same two sections (Game Components, Missing member) once per mod node, 17 copies each; only one copy of each was useful. The no-magic-fridges generic `LoadGeneratedBuildings` pattern was the relevant precedent and was reached through the code, not the brief.
