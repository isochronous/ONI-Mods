# OM-011 test report: Fridge Thresholds Extended in-game tests

**Result: the suite is written and builds, but it did not run.** The game never started. The approved run ended at launch with "The game did not start within 120 s." No test executed: 0 passed, 0 failed, 5 not run. Nothing was committed and nothing under common/ was changed.

## Why the game did not start (verified)

Steam's launch options for Oxygen Not Included now run the game through oni-moono. In `C:\Program Files (x86)\Steam\userdata\...\config\localconfig.vdf` the option is:

```
"LaunchOptions"		"\"C:\\Users\\thegr\\AppData\\Local\\oni-moono\\player\\OniMoono.exe\" -- %command%"
```

1. Steam started `OniMoono.exe -- ...\OxygenNotIncluded.exe "--onitest-dir=..." "--onitest-save=..." ...` as pid 19200 at 13:01:41. Steam removed that process at 13:01:44 (Steam `logs\console_log.txt`).
2. The process wrote no Player.log. The last write to Player.log was 12:51:09, from Jeremy's own session that ended before the run. The Windows Application log has no crash entry.
3. The moono host's `Options.ParseInto` (decompiled from `oni-moono\player\OniMoono.dll`) handles `--` by jumping to the game executable and then carries on parsing the arguments that follow it. `--onitest-dir=...` is neither a moono option nor a Unity option. It hits `Console.Error.WriteLine("Unknown option " + args[i])`, then `Parse` returns null and `Main` returns 2. That happens before the engine log is opened, which is why nothing was written.
4. On top of that, the launcher only looks for a process named `OxygenNotIncluded` (Invoke-OniTests.ps1:154, 215-232, 542, 576). Under moono the game runs as `OniMoono`, so the launcher would not find it even if moono accepted the arguments.

The last good TestRunner run in the warden log was 2026-10-22. That was before the launch option pointed at moono.

Cleanup after the failed launch was verified. The player's `mods.json` is byte-identical to the run's `mods.json.real` backup (SHA-256 350F34FA...6592). No warden process is left running. The mode was not changed, because the run was in the player's own Spaced Out mode.

## How to run it (Jeremy's call; I changed nothing)

- **A. One run on the vanilla player.** Clear the game's Steam launch options, run the command below, then put the options back. This tests the mod on Mono, not on moono.
- **B. Make the harness work under moono.** Moono would need to pass arguments that follow the game executable through to the game, instead of rejecting them. The launcher would also need to accept an `OniMoono` process, and its warden and pid file would have to follow it. That is a moono change plus a TestRunner change, outside this unit.

Either way the suite needs a fresh approval for its run.

## Files

Created:
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/FridgeThresholdsExtended.Tests.csproj`: a copy of the VentFreezeFix.Tests csproj shape. It references the mod and TestRunner, `ModDeployFolder none`.
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/Fridges.cs`: five tests, described below.

Changed:
- `ONI-Mods.sln`: project `FridgeThresholdsExtended.Tests` {4E1C7B2A-93D5-4F86-A0B7-6D2E58C9F314} under the `fridge-thresholds-extended` folder, with Debug/Release configs. 7 lines added, CRLF and BOM kept.

The other mods' types are reached only by name: `AccessTools.TypeByName` for `StorageRefrigeratorThresholds.RefrigeratorThresholds` and `Psyko.Freezer.Freezer`, and `AccessTools.Property` for `SendGreenOnLow`, `ActivateValue`, `DeactivateValue` and `LastSetFlag`.

## The tests (one per behaviour)

Every rig follows NoMagicFridges: a `t.Dupes.Box()`, a copper wire dragged with the real tool, and a `DevGenerator` for power. Fills store real MushBar. Emptying goes through `Storage.ConsumeIgnoringDisease`, the call a Duplicant's eating uses, so the hysteresis never passes through an empty fridge. The port is read with `LogicPorts.GetOutputValue(FilteredStorage.FULL_PORT_ID)`. Steps are a game second apart.

| # | Test | Spec item | Oracle |
|---|---|---|---|
| 1 | `FreezerFollowsThresholds` | a | A Freezer and a vanilla Refrigerator (the control) sit on one circuit, both set to High 50 / Low 20. Fills go 60 → 35 → 10 → 35 → 60 and the expected port is 1, 1, 0, 0, 1 on both. Then the generator is removed and both send 0 at 60%. A screenshot is taken at 60%. Without this mod the Freezer sends 0 at 60%, because it is not full. |
| 2 | `IndustrialRevolutionFridge` | b | `SimpleFridge` is registered and has `RefrigeratorThresholds`. With High 50 / Low 20, fills 60 → 35 → 10 give 1, 1, 0. The component order is logged. |
| 3 | `FreezerCopySettings` | c | The source Freezer is set to green on low, High 70, Low 30. `CopyBuildingSettings.ApplyCopy` returns true (the tool's own call), and the target then reads true / 70 / 30. |
| 4 | `WriteRoundTrip` (1/2) | d | On the built Freezer, the thresholds component comes before Psyko's `Freezer` (this is the M1 mechanism). Fills 60 → 35 give 1, 1, so the hysteresis is active. The colony is then saved to the run folder. This follows the VentFreezeFix / Ovagro round-trip pattern. |
| 5 | `RoundTripLoad` (2/2, `FreshSave`) | d | The first look after the load, before the test touches anything: the port is 1 at 35%, High and Low are kept, and the thresholds component is still ahead of `Freezer`. The port is still 1 two game seconds later. Port, operational state, `LastSetFlag` and fill are logged. |

The harness supports save and reload (`SaveLoader.Instance.Save` under `Session.Dir`, then `Session.SavePath` and `FreshSave`), so item d is covered and no harness change was needed.

One limit of test 5: after a load the Freezer's power flag is probably set after `OnSpawn`. If so, the OperationalChanged event recomputes the port anyway, and the green-after-load check would pass even without the M1 fix. The component-order assertions in tests 4 and 5 are the checks that tell the fix apart from no fix. The first-look log line would show which case the game is in.

## Exact command and runner output

Build check, which passed:

```
pwsh -NoProfile -File common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850 -BuildOnly
...
Build succeeded.
    0 Warning(s)
    0 Error(s)
Checking FridgeThresholdsExtended.Tests.dll against the game assemblies
Built and checked against the game assemblies (-BuildOnly: no game session).
```

The approved run (no -KeepGame), verbatim:

```
pwsh -NoProfile -File common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850

Building C:\dev\ONI-Mods\mods\fridge-thresholds-extended\src\FridgeThresholdsExtended.Tests\FridgeThresholdsExtended.Tests.csproj

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.19
Checking FridgeThresholdsExtended.Tests.dll against the game assemblies
Registered new local mod 'FridgeThresholdsExtended' (Fridge Thresholds Extended)
Loadout: TestRunner + FridgeThresholdsExtended + Workshop 2959340381, 2618339179, 3557584850; mode Spaced Out; fixture C:\dev\ONI-Mods\common\tools\TestRunner\fixtures\testbed-so.sav
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130136 --onitest-save=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130136\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
Restored mods.json.
Exception: C:\dev\ONI-Mods\common\tools\TestRunner\Invoke-OniTests.ps1:543
Line |
 543 |  …      if (-not $proc) { throw 'The game did not start within 120 s.' }
     |                           ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
     | The game did not start within 120 s.
```

Exit code 1. Run folder: `common/tools/TestRunner/runs/run-20261104-130136` (no results.jsonl, no Player.log). Full console capture: `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\om011-run1.txt`.

## Player.log

None from this run: the game process exited before any log was opened. No FridgeThresholdsExtended patch-applied lines and no exceptions to report.

## What remains unverified

All of it in game:
- Harmony accepting `Component __instance` and the `___` field injection on the manual Freezer prefix and postfix.
- Freezer: the port following the thresholds, and red without power.
- The IR SimpleFridge port.
- Copy settings between Freezers.
- The first signal after a load, and the component order.
- The four other IR fridges, and each target mod missing in turn. These were not in this suite, which is representative by design.

The test code itself has not run, so a fix-and-rerun of it may still be needed once the game can start.

## Brief feedback

- The `common` store's TestRunner pointers (entities, error-handling, naming) applied in principle, but the TestRunner README held everything this work needed. The README was not among the pointers; it was reached through the spec.
- Missed: nothing in either brief records that the game now launches through oni-moono, or that the harness cannot launch through it. That is the fact that decided this unit's outcome. It belongs in the TestRunner node, for example as a known limit next to "Steam must be running".
- No ICON friction in this unit.

## Run 2 (Steam launch options cleared by Jeremy, no Moono)

Command (both runs): `pwsh -NoProfile -File common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850` (no -KeepGame).

**Result: 4 passed, 1 failed, in both the run and the one allowed rerun.** The failure is the round-trip load test, and it comes from the test's setup, not from the mod.

### Run 2a, verbatim

```
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130820 --onitest-save=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130820\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerFollowsThresholds (5.8 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.IndustrialRevolutionFridge (4.1 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerCopySettings (2.0 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.WriteRoundTrip (4.2 s)
FAIL     FridgeThresholdsExtended.Tests/Fridges.RoundTripLoad (0.0 s): the loaded Freezer at 35%, saved green on its way down, sends green (expected 1, got 0)
Session done: 4 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped; 4 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped of 5; 50.8 s real time; fixture loaded again 1 time(s)
Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies
Game live after 70 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130820
```

RoundTripLoad notes: `first look after the load: port 0, operational False, LastSetFlag False, stored 0.0%` and `loaded Freezer components: thresholds at 34, Freezer at 37`.

The test started on the game's first update after the load (`first update, time scale 0` in Player.log), before the saved generator had powered the circuit. Asserting at that moment was a test-code fault. I fixed it: the test now logs the first look, waits until the Freezer is operational and one game second has passed, takes a screenshot, and asserts that the fill is still 35% before checking the port.

### Run 2b (fix and rerun), verbatim

```
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131031 --onitest-save=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131031\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerFollowsThresholds (5.8 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.IndustrialRevolutionFridge (4.2 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerCopySettings (2.0 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.WriteRoundTrip (4.2 s)
FAIL     FridgeThresholdsExtended.Tests/Fridges.RoundTripLoad (1.3 s): the loaded Freezer holds the 35% it was saved with (0.0%) (expected true)
Session done: 4 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped; 4 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped of 5; 52 s real time; fixture loaded again 1 time(s)
Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies
Game live after 68 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131031
```

RoundTripLoad notes: `first look after the load: port 0, operational False, LastSetFlag False, stored 0.0% in 0 item(s)` and `once powered: port 0, operational True, LastSetFlag False, stored 0.0% in 0 item(s)`.

**Cause.** After the load the Freezer holds no items. Screenshot `runs/run-20261104-131031/artifacts/FridgeThresholdsExtended.Tests.Fridges.RoundTripLoad-after-load.png` shows the food in a pile on the floor at the Freezer's base. The test stores MushBar straight into storage and never adds it to the Freezer's food filter (TreeFilterable), which is empty on a new building. The likely mechanism, not confirmed in code, is that the load re-applies the empty filter and the game drops what it does not allow. Port 0 at 0% is the correct signal for an empty Freezer. **Fix for a further run:** before filling, set the Freezer's TreeFilterable accepted tags to include MushBar. That needs Jeremy's approval for another run; I did not run again.

### What the runs establish

- **Freezer (a): verified.** Port 1, 1, 0, 0, 1 at 60/35/10/35/60% with High 50 / Low 20, the same as the vanilla Refrigerator control, and red without power. Before this mod the Freezer sent red at 60%. Screenshot: `runs/run-20261104-130820/artifacts/...FreezerFollowsThresholds-green-at-60.png`.
- **IR SimpleFridge (b): verified.** It has RefrigeratorThresholds, at component index 32, ahead of Refrigerator at 36. Port 1, 1, 0 at 60/35/10%.
- **Copy settings (c): verified.** Green on low, 70 and 30 carried from one Freezer to another.
- **First signal after load (d): partly verified.** The thresholds component is ahead of Freezer on the built Freezer (34 < 37) and on the loaded one. `LastSetFlag` is `False`, not `null`, at the first update after the load. ThresholdsBase.OnSpawn resets it to null, so the Freezer's OnSpawn ran the prefix after the thresholds were registered, which is the M1 fix working. Still not verified: a green signal from a loaded, filled Freezer. That needs the filter fix above.
- Harmony accepted `Component __instance` and the `___` field injection on the manual Freezer patches. The Freezer and copy tests could not have passed otherwise.

### FridgeThresholdsExtended Player.log lines (identical in both runs)

```
[FridgeThresholdsExtended] Loaded version 0.1.0.0
[FridgeThresholdsExtended] Industrial Revolution found; thresholds added to 5 fridges
[FridgeThresholdsExtended] Freezer found; its logic port follows the thresholds
```

No exceptions in either Player.log. Each run's own `Player's folders` line above confirms the harness left nothing new in the player's folders. Run folders: `common/tools/TestRunner/runs/run-20261104-130820` and `run-20261104-131031`.

Test file changed since run 2a: `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/Fridges.cs` (RoundTripLoad waits for power before asserting, and asserts the fill). Not committed.

## Run 3 (food filter set before filling)

Change since run 2b, in `Fridges.cs` `WriteRoundTrip`: before power and filling, `TreeFilterable.AddTagToFilter(MushBar)`, with an assert that the filter now contains it. `RoundTripLoad` already asserts that the 35% fill survives the load. Same command, no -KeepGame. This was the last approved run; there were no reruns.

**Result: 4 passed, 1 failed, 0 errors.**

```
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131318 --onitest-save=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131318\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerFollowsThresholds (5.8 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.IndustrialRevolutionFridge (4.1 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerCopySettings (2.0 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.WriteRoundTrip (4.7 s)
FAIL     FridgeThresholdsExtended.Tests/Fridges.RoundTripLoad (1.3 s): the loaded Freezer holds the 35% it was saved with (0.0%) (expected true)
Session done: 4 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped; 4 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped of 5; 52.1 s real time; fixture loaded again 1 time(s)
Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies
Game live after 68 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131318
```

RoundTripLoad notes:

```
first look after the load: port 0, operational False, LastSetFlag False, stored 0.0% in 0 item(s)
loaded Freezer components: thresholds at 34, Freezer at 37
once powered: port 0, operational True, LastSetFlag False, stored 0.0% in 0 item(s)
```

WriteRoundTrip had the Freezer at 35.0% (69.8 kg) right before `Saved to [...\fte-roundtrip.sav]`. Screenshot: `runs/run-20261104-131318/artifacts/FridgeThresholdsExtended.Tests.Fridges.RoundTripLoad-after-load.png`. It shows the same green pile on the floor at the Freezer's base as in run 2b.

**The cause is unclear, and the filter explanation from Run 2 is ruled out.** With the filter allowing MushBar, the loaded Freezer still has no items, and the pile is still on the floor. What is known: the food was in storage when the save was written. After the load, storage is empty and something lies on the floor beside the Freezer. What is not known: whether the item is written to the save outside storage, dropped as the colony loads, or dropped by something in this mod loadout. The vanilla Refrigerator was not given the same round trip, so it is not known whether this is specific to the Freezer. Next step, which needs a new approval: add a vanilla Refrigerator to the round trip as a control, and log the pile's prefab and mass after the load. I did not investigate further, because no more runs are approved.

Unchanged from Run 2: Freezer (a), IR SimpleFridge (b) and copy settings (c) pass. For (d), the component order and `LastSetFlag False` at the first update after the load show the M1 order fix taking effect. A green signal from a filled Freezer after a load is still unverified.

FridgeThresholdsExtended Player.log lines (run 3):

```
[18:13:36.412] [1] [INFO] [FridgeThresholdsExtended] Loaded version 0.1.0.0
[18:13:36.571] [1] [INFO] [FridgeThresholdsExtended] Industrial Revolution found; thresholds added to 5 fridges
[18:13:36.576] [1] [INFO] [FridgeThresholdsExtended] Freezer found; its logic port follows the thresholds
```

No exceptions in Player.log. The only warnings are "Recoverable parse error" lines for other Workshop mods' mod.yaml files (2880433226, 3599672848, 1722760223), which are unrelated. Run folder: `common/tools/TestRunner/runs/run-20261104-131318`. Not committed.

### Run 3: why the filter does not hold (Jeremy: "filter is not getting set")

I worked this out from the game code and the screenshots, without another run.

**Which API the test calls.** `TreeFilterable.AddTagToFilter(new Tag("MushBar"))` on the built Freezer, before power and filling. That call is the same one the game's side screen makes: `TreeFilterableSideScreen` calls `targetFilterable.AddTagToFilter(tag)` / `RemoveTagFromFilter(tag)`. The test's assert `ContainsTag(MushBar)` passed, so the tag was in the filter for the rest of that session. The Freezer's `TreeFilterable` is vanilla (`go.AddOrGet<TreeFilterable>()` in FreezerConfig), with `storage.storageFilters = STORAGEFILTERS.FOOD`. Psyko's Freezer has no filter logic of its own. Its `FilteredStorage` only forbids `Compostable`.

**What differs from the side screen.** The side screen only offers what the colony has discovered. Its rows come from `DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(category)` for each storage filter category. A player can therefore only tick food the colony has discovered. The test's MushBar is made with `KInstantiate` and put straight into storage, so it is never discovered. That is why Jeremy saw no filter set in the side screen: an undiscovered item has no row to show ticked.

**Why it is undone on the load (vanilla `TreeFilterable.OnSpawn`, decompiled from the installed game):**
1. `autoSelectStoredOnLoad` unions the stored item IDs into `acceptedTagSet`, so MushBar is in it.
2. Then `filterByStorageCategoriesOnSpawn` runs `RemoveIncorrectAcceptedTags`. That removes every accepted tag not found in `DiscoveredResources.GetDiscoveredResourcesFromTag(storageFilter)` for any of the storage's categories. MushBar is not discovered, so it is removed.
3. `RemoveTagFromFilter` → `UpdateFilters` → `DropFilteredItemsFromTargetStorage` (`dropIncorrectOnFilterChange` is true). The food is dropped onto the floor. This is the green pile at the Freezer's base in the run 2b and run 3 screenshots.

This explains both runs. Without the filter (run 2b), step 1 added the tag and step 2 removed it again. With the filter (run 3), the tag came out of the save and step 2 removed it again. Neither this mod nor Psyko's Freezer is involved: a vanilla Refrigerator with undiscovered food would drop it the same way. That is inferred from the shared code; it has not been run.

Inferred: that MushBar is undiscovered in `testbed-so.sav`. It is consistent with the drop, and nothing else in OnSpawn drops a stored item, but I did not read the fixture's DiscoveredResources directly.

**Proposed fix (test code only, not applied, not run).** In `WriteRoundTrip`, before setting the filter, discover the food the way picking it up in a colony would: `DiscoveredResources.Instance.Discover(new Tag(Food), GameTags.Edible)`. Assert `DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(GameTags.Edible).Contains(MushBar)` so the test fails clearly if the category is wrong. Then `AddTagToFilter` as now. Discovery is saved with the colony, so after the load `RemoveIncorrectAcceptedTags` keeps MushBar and the food stays in storage. The round-trip save is a run-folder file and the fixture is reloaded afterwards, so the player's saves are untouched. A screenshot of the side screen before saving would show the tick Jeremy expected. Running it needs a new approval.

## Run 4 (discovery fix)

**Jeremy's question: aren't the earlier tests already putting MushBar into the Freezer?** Yes. All of them fill through `Fill` (Fridges.cs:113-118: `KInstantiate(Assets.GetPrefab("MushBar"))` then `storage.Store(go)`). `FreezerFollowsThresholds` does so at Fridges.cs:182, for the Freezer and the Refrigerator, and `IndustrialRevolutionFridge` at :208. All five tests share one loaded colony; the only reload is the FreshSave before `RoundTripLoad`. Any discovery from tests 1-2 would therefore have been in the round-trip save.

The run measured the outcome directly. When `WriteRoundTrip` started, after tests 1-2 had stored hundreds of kg of MushBar, the log read: `MushBar before discovery: IsDiscovered False, category Edible, listed under the Freezer's filters (Edible, CookingIngredient, Medicine) False`. So storing never discovers the food.

The only discovery path I found in the game is `WorldInventory.OnAddedFetchable`, which calls `DiscoveredResources.Instance.Discover` the first time a prefab appears in a world's inventory (decompiled WorldInventory.cs:310-328, subscribed to a Game event at :71). That event does not fire for an item made with `KInstantiate` and stored straight away. I did not trace exactly why, because I did not find the code that raises the event.

The drop happens only at load time, in `TreeFilterable.OnSpawn` → `RemoveIncorrectAcceptedTags` → `RemoveTagFromFilter` → `UpdateFilters` → `DropFilteredItemsFromTargetStorage`. That is why the earlier tests never showed it.

**Fix** (Fridges.cs, `WriteRoundTrip`): log the discovery state, call `DiscoveredResources.Instance.Discover(MushBar, GetCategoryForEntity(prefab))` (the category came out as `Edible`), assert that MushBar is now listed under one of the Freezer's storage categories (the same check `RemoveIncorrectAcceptedTags` makes), then `AddTagToFilter`. `RoundTripLoad` also logs the filter and the discovery state after the load. `-BuildOnly`: 0 warnings, 0 errors, ModCheck clean.

**Result: 5 passed, 0 failed, 0 errors.** Same command, no -KeepGame, run once.

```
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131755 --onitest-save=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131755\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerFollowsThresholds (5.8 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.IndustrialRevolutionFridge (4.2 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerCopySettings (2.0 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.WriteRoundTrip (4.7 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.RoundTripLoad (3.3 s)
Session done: 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 5; 54 s real time; fixture loaded again 1 time(s)
Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies
Game live after 70 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-131755
```

RoundTripLoad notes:

```
first look after the load: port 0, operational False, LastSetFlag False, stored 35.0% in 1 item(s)
after the load: filter MushBar, MushBar discovered True
loaded Freezer components: thresholds at 34, Freezer at 37
once powered: port 1, operational True, LastSetFlag True, stored 35.0% in 1 item(s)
```

FridgeThresholdsExtended Player.log lines:

```
[18:18:12.835] [1] [INFO] [FridgeThresholdsExtended] Loaded version 0.1.0.0
[18:18:13.000] [1] [INFO] [FridgeThresholdsExtended] Industrial Revolution found; thresholds added to 5 fridges
[18:18:13.006] [1] [INFO] [FridgeThresholdsExtended] Freezer found; its logic port follows the thresholds
```

No exceptions. Run folder: `common/tools/TestRunner/runs/run-20261104-131755`; screenshot `artifacts/FridgeThresholdsExtended.Tests.Fridges.RoundTripLoad-after-load.png`.

**What it shows for M1.** After the load the food and thresholds survive, the hysteresis state survives (green at 35% on the way down), and the thresholds component is ahead of the Freezer's own (34 < 37). At the first update after the load the Freezer was not yet powered (port 0, operational False), and it went green once the saved generator powered it. As predicted, that green comes from the OperationalChanged recompute, so the port alone does not tell the fix apart from no fix. The component order and `LastSetFlag False` (not null) at the first update are the evidence for the fix.

Still not covered by this suite: the other four IR fridges, and loading with each target mod missing in turn. Not committed.

## Review 2 fixes (05-review-report.md; Fridges.cs only, game not run)

- **M1, test name.** `RoundTripLoad` is now `LoadKeepsOrderFillAndGreen`. Its display name reads "(round trip 2/2) after the load the thresholds component is still ahead of the Freezer's own, the thresholds and the 35% fill are kept, and once powered it sends green". A summary on the test says that the green on its own would pass without the order fix (power-on recomputes the port), and that the component-order assertion is the oracle for that fix. The assertions are unchanged.
- **m1, pacing.** `yield return t.WaitGameSeconds(1)` now sits between the two `t.World.Build` calls in `FreezerFollowsThresholds` (Freezer, then Refrigerator) and in `FreezerCopySettings` (the two Freezers).
- **m2, logs.** I removed the "before discovery" line in `WriteRoundTrip`, and the `Look()` helper with its "first look", "after the load" and "once powered" lines in the load test (which also drops the now-unused `LastSetFlag` read). Kept: the fill log in `Fill` and the component-order logs (SimpleFridge, built Freezer, loaded Freezer). Those carry the values behind the order and fill assertions. Every remaining assertion message states its own value.
- Unchanged as instructed: green-on-low coverage (m3) and the Patch-line order in ThresholdsCompat.cs (m4).

Build (`-BuildOnly`, no game session):

```
Building C:\dev\ONI-Mods\mods\fridge-thresholds-extended\src\FridgeThresholdsExtended.Tests\FridgeThresholdsExtended.Tests.csproj

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.19
Checking FridgeThresholdsExtended.Tests.dll against the game assemblies
Built and checked against the game assemblies (-BuildOnly: no game session).
```

The renamed and paced tests have not been run in game since these edits. Not committed.
