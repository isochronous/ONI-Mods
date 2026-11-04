# OM-011 implement report: Fridge Thresholds Extended 0.1.0

Nothing committed, game not launched, nothing under common/ or other mods touched.

## Files

Created, `mods/fridge-thresholds-extended/`:
- `.gitignore`, `.gitattributes`: copied from mods/smooth-borders (the most recent scaffold, same content as vent-freeze-fix).
- `README.md`: what it does (Freezer, IR fridges), Compatibility (requires Storage Refrigerator Thresholds; works with either or both of Freezer and IR; build 744825), Installing, Building.
- `CHANGELOG.md`: 0.1.0 - 2026-11-04, one bullet.
- `src/FridgeThresholdsExtended/FridgeThresholdsExtended.csproj`: imports `$(OniCommon)` props/targets, version 0.1.0, no PLib, no translations (the no-magic-fridges csproj shape).
- `src/FridgeThresholdsExtended/mod.yaml` (staticID `Isochronous.FridgeThresholdsExtended`), `mod_info.yaml` (ALL, 744825, 0.1.0, APIVersion 2).
- `src/FridgeThresholdsExtended/Mod.cs`: `UserMod2.OnLoad` logs the version; `OnAllModsLoaded` calls `ThresholdsCompat.Apply(harmony)`.
- `src/FridgeThresholdsExtended/ThresholdsCompat.cs`: the compat class.

Changed (repo root, as the OM-007 Smooth Borders scaffold commit 88214fb did):
- `ONI-Mods.sln`: solution folder `fridge-thresholds-extended` + project `FridgeThresholdsExtended` (GUIDs 9A160FD4-..., 775B88F8-...), configs, NestedProjects.
- `README.md`: row in the mod table.
- `.github/ISSUE_TEMPLATE/bug_report.yml`, `feature_request.yml`, `.github/workflows/label-mod.yml`: "Fridge Thresholds Extended" option and its `fridge-thresholds-extended` label mapping. The GitHub label itself does not exist yet; create it at release.

All new files are CRLF like their neighbours.

## What ThresholdsCompat does

`Apply` resolves `StorageRefrigeratorThresholds.ThresholdsBase` and `.RefrigeratorThresholds` with `AccessTools.TypeByName`; if either is missing it logs "Storage Refrigerator Thresholds not found; nothing to do" once and returns. Then three parts, each in its own try/catch (`e.GetBaseException().Message`), each warning and leaving its target alone on a missing member:
- **IR fridges**: the five `RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesRefrigeration.{FridgeSmall,FridgeLarge,FridgePod,HightechBig,HightechSmall}Config` types. None found: silent (IR not installed). Otherwise each declared `DoPostConfigureComplete` gets a prefix `AddThresholdsPrefix(GameObject go)` that adds `RefrigeratorThresholds` when `go.GetComponent(type) == null`; a missing one is warned by class name. Logs how many were patched.
- **Freezer logic port**: only when `Psyko.Freezer.Freezer` resolves. Checks `UpdateLogicCircuit`, the fields `filteredStorage`/`operational`/`ports` by name and type, `ThresholdsBase.Get(GameObject)`, `UpdateLogicState(float)` and `IsActuallyOperational(Operational)` (both returning bool), `LastSetFlag` (bool?), and FilteredStorage's private `GetAmountStored`/`GetMaxCapacityMinusStorageMargin`. Skips with a log line if `Harmony.GetPatchInfo` shows a prefix whose owner contains "llunak" (verified: the game's Harmony id for a mod is its staticID, `KMod.Mod` -> `DLLLoader.LoadDLLs(this, staticID, ...)`, so the thresholds mod's id is `llunak.StorageRefrigeratorThresholds`). Binds `Get` as `Func<GameObject, object>` via `Delegate.CreateDelegate`, the FilteredStorage methods via `AccessTools.MethodDelegate<Func<FilteredStorage, float>>`, and keeps the instance members as `MethodInfo`/`PropertyInfo` (called with `Invoke`; the method only runs on storage and operational events). Prefix `UpdateLogicCircuitPrefix(Component __instance, FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports)` is the thresholds mod's Refrigerator prefix (Storage.cs:346-366) line for line; returns true when `Get` gives null.
- **Freezer copy settings**: postfix on `Freezer.OnCopySettings(object)` copying `SendGreenOnLow`, `ActivateValue`, `DeactivateValue` (read/write properties on ThresholdsBase, Storage.cs:42-79, the same three Storage.cs:338-340 copies) from the source object's `RefrigeratorThresholds` to this one's, in that order.

No compile-time reference: the built DLL names the three mods only in string constants (checked with ilspycmd on bin/Release/FridgeThresholdsExtended.dll). Fields are `Type`, `MethodInfo`, `PropertyInfo` and delegates over `GameObject`/`object`/`FilteredStorage`; patch signatures use `GameObject`, `Component`, `object` and vanilla types only.

## Build

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`, exit 0. Full log: C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\build-om011.txt.

```
  Restored C:\dev\ONI-Mods\mods\fridge-thresholds-extended\src\FridgeThresholdsExtended\FridgeThresholdsExtended.csproj (in 387 ms).
  FridgeThresholdsExtended -> C:\dev\ONI-Mods\mods\fridge-thresholds-extended\src\FridgeThresholdsExtended\bin\Release\FridgeThresholdsExtended.dll
...
    291 Warning(s)
    0 Error(s)

Time Elapsed 00:00:09.13
```

`grep -ci "warning.*fridge-thresholds-extended"` over the log: 0. The 291 warnings are all in other projects (CS0618 obsolete Unity/DLC APIs in test projects and TestRunner, CS0649 in NoMagicFridges.Tests, and so on).

## Deviations from the design

- **No PLib, no ModStrings, no translations/**: the mod has no player-visible strings beyond mod.yaml, so `PUtil.InitLibrary` and `Translations.Register` have nothing to do. The csproj and Mod.cs follow no-magic-fridges (the repo's no-PLib skeleton) rather than vent-freeze-fix's PLib lines. No `publish/` folder and no Tests project: neither was asked for; publish material comes at release.
- **Three try/catch blocks rather than two**: Freezer's logic port and its copy settings are separate targets, so a missing `OnCopySettings` does not cost the port fix and the reverse.
- **Freezer absent is silent**, as is IR absent; only the thresholds mod being absent is logged (the spec's "log once"). SmartWeightPlate's compat is silent when its mod is absent too.
- **Root README and .github lists** were updated beyond the sln, following the previous scaffold commit.

## Needs an in-game check

- Harmony accepts `Component __instance` and the `___` field injection on the manual Freezer prefix and postfix (SmartWeightPlate's manual postfix already uses `Component __instance`, so the first is likely fine).
- Freezer: the sliders drive the port (High/Low hysteresis, green-on-low option), red when unpowered; copy settings between two Freezers carries the thresholds.
- Freezer, first signal after load: the thresholds mod adds `RefrigeratorThresholds` in a postfix on FreezerConfig, so it sits after `Freezer` and may not be spawned when `Freezer.OnSpawn` calls `UpdateLogicCircuit`. Then the prefix returns true and the first signal is Freezer's own IsFull until the next storage event. Check whether a loaded Freezer between the thresholds shows the right colour right away; if not, adding the component in our own prefix on `Psyko.Freezer.FreezerConfig.DoPostConfigureComplete` would put it first.
- IR fridges (all five, including the two Hightech ones whose IDs are swapped): side screen sliders appear, the port follows them, copy settings carries them (handled by the thresholds mod's own Refrigerator postfix), and the first signal after load is right.
- Load order: if a future thresholds release prefixes `Freezer.UpdateLogicCircuit` itself in its own `OnAllModsLoaded` after ours runs, the llunak check cannot see it and both prefixes are installed. Both compute the same flag and the `LastSetFlag` guard prevents double sends, so the effect would be a redundant computation, not a wrong signal.
- Each mod missing in turn (thresholds alone, thresholds + Freezer, thresholds + IR, IR + Freezer without thresholds): loads cleanly, one log line each.

## Brief feedback

- Applied: blk-430105ebb58b (When to Comment: XML summary, other mods named with Workshop ids), blk-e2901713e50b (try/catch for optional integration, message form, GetBaseException), blk-ca37f1585a91 (missing member: warn, leave vanilla; reached by id from the research report, not the brief).
- The brief listed the same two sections once per mod node (17 copies each of When to Comment and try/catch); one copy of each was useful, the rest did not add anything. Same friction as the research unit reported.
- Missed: nothing in the brief covered the scaffold checklist (sln, root README table, .github issue templates and label workflow); that came from reading commit 88214fb. The `common` store brief (two ICON lessons) did not apply to this work.
- The Workshop writing style came from Jeremy's memory file, not the store.

## Fixes (after review 03-review-report.md)

Only mods/fridge-thresholds-extended changed. The Invoke allocations (m2) and the llunak check (m3) are unchanged, as instructed.

- **M1, Freezer first signal after load.** `PatchFreezerLogic` resolves `Psyko.Freezer.FreezerConfig.DoPostConfigureComplete` (missing: warn "FreezerConfig.DoPostConfigureComplete not found" and skip the whole Freezer logic part) and prefixes it with the existing null-checked `AddThresholdsPrefix`. `RefrigeratorThresholds` now sits ahead of `Freezer`, and the thresholds mod's postfix `AddOrGet` finds it. The prefix is applied after the llunak check, in the same try/catch, right before the `UpdateLogicCircuit` prefix. The comment on the `Get == null` path now reads "no thresholds on this Freezer: its own full signal", and the class summary says why the config prefix exists.
- **m1, IR warning.** A config class that does not resolve now warns "Industrial Revolution <Class> skipped: class not found". "DoPostConfigureComplete not found" is kept for the case where the class exists but the method does not.
- **README.** I replaced the last line ("Harmony only, no PLib. None of the three mods is referenced at build time; ...") with "The build does not need any of the three mods installed."

Rebuild: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`, exit 0. Full log: C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\build-om011-fix.txt.

```
  FridgeThresholdsExtended -> C:\dev\ONI-Mods\mods\fridge-thresholds-extended\src\FridgeThresholdsExtended\bin\Release\FridgeThresholdsExtended.dll
...
    288 Warning(s)
    0 Error(s)

Time Elapsed 00:00:06.23
```

Warnings naming fridge-thresholds-extended: 0. All 288 are in other projects (the first build reported 291; this build was incremental, and none of the warnings in either belongs to this mod).

The in-game check for the first signal after load still applies, now to confirm that the component order puts thresholds first on the Freezer.
