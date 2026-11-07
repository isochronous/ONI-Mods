# Unit 06 report: in-game suites on U60 (762230), branch OM-012-u60-only

Result: every suite passes on U60. Two TestRunner harness bugs surfaced on U60 and were fixed in common/ (below); the failures they caused (16 tests in the first shared-loadout run) went green after the fixes. No mod code changed, no commit.

## Save safety (step 0)
Gate passed, nothing was launched before it was checked.
- `Patches.cs` `SaveLoader_Save_Patch` blocks `SaveLoader.Save(string, bool, bool)` for any path outside the run folder while a session is active (`Session.InRunFolder`). Reflection over U60 `SaveLoader` shows that is the only method that writes a save file (`Save(BinaryWriter)` is a stream writer called from it; `InitialSave` and `SaveAndQuitRoutine` go through it).
- Autosave: `Game_StartDelayedSave_Patch` skips every autosave while a session is active. Save on quit goes through the blocked `Save`.
- The fixture (`fixtures/testbed-so.sav`) is copied to `runs/<label>-<stamp>/fixture.sav` and only the copy is loaded. NoColony and Worldgen tests load no save.
- `RetireColonyUtility.SaveColonySummaryData` is skipped (no `RetiredColonies` writes). The run script lists save_files, cloud_save_files and RetiredColonies before and after and prints "nothing new" (it did in every run).
- Own check: listing of save_files, cloud_save_files, RetiredColonies (2705 files, path|size|mtime UTC) taken before the first run and after the last: identical except `cloud_save_files\76561197969666927\steam_autocloud.vdf` (50 bytes both times, mtime 2026-11-06 23:15 -> 2026-11-07 05:55). That file is Steam's cloud-sync marker, written by the Steam client on game launch/exit, not a save and not a test write. No `.sav` changed. Lists: scratchpad `before.txt` / `after.txt`.
- Side effect found: `kplayerprefs.yaml` `SaveFilenameKey/` (the "last save" pointer, a pref) was blanked by a worldgen run and the runner crashed on the next start (harness bug 2). Restored: it now points at `...\cloud_save_files\76561197969666927\Prosperous Debacle\Prosperous Debacle.sav` again and no `.onitest-savekey` is left over.

## Step 1: ONI-moono launch parameters gone
Runs launch as `-applaunch 457140 --onitest-...` and find the game process as `OxygenNotIncluded` (`GameProcess.ps1` looks for that name first, `OniMoono` only as a fallback). All 10 launches worked; no change needed.

## Step 2: CapturePng patch
- The patch resolves on U60 (Harmony applied it; no patch error in Player.log; unit 04 PatchCheck showed `Timelapser_CapturePng_Patch` OK / `WriteToPng` skipped).
- Folder check: RetiredColonies listing identical before/after all runs (see above). Player.log shows `Timelapse.OnNewDay but worldsToScreenshot is not empty` during runs but no `skipped a timelapse image` line (0 occurrences), so the game never reached `CapturePng` in these runs; the "image is nulled" path itself was not exercised. Only the negative (no image written) is shown.
- Colony summary skip works: `skipped the colony summary of Mod Test` in Player.log.

## Step 3: suite results

Relaunches: 10 game launches in total. Before the coordinator's change of method, SaltySurf, UpgradePlanner and CornerBuildAnything each ran with their own minimal loadout (3 launches; a fourth, Bitshifter, was killed during its build when I removed my sequential driver, so it never launched and ran in the shared run instead). Then one shared run of all 20 test projects at once (1 launch, 1183 s, loadout = TestRunner + every mod of ours and nothing else; this is also the cross-mod check). Then isolation reruns of the suites that failed there or were skipped (6 launches, 3 of them after the fixes). Reasons for relaunch: each differing loadout needs a new game, per the runner.

Minimal-loadout runs (mod under test only):

| Suite | Pass | Fail | Skip | Runner excerpt |
|---|---|---|---|---|
| SaltySurf | 11 | 0 | 0 | `Session done: 11 passed, 0 failed ... 55.1 s` |
| UpgradePlanner | 41 | 0 | 17 | 17 skipped: "Blueprints Expanded is not loaded" |
| CornerBuildAnything | 10 | 0 | 0 | `Session done: 10 passed ... 46.8 s` |

Shared loadout, all mods together (run `all-20261107-002413`; first pass before the harness fixes: 255 passed, 16 failed, 20 skipped of 291):

| Suite | Pass | Fail | Note |
|---|---|---|---|
| Bitshifter | 30 | 0 | |
| CornerBuildAnything | 10 | 0 | |
| DetectAnyElement | 15 | 0 | |
| DontCountEggsOnRails | 3 | 0 | |
| FridgeThresholdsExtended | 0 | 5 | needs third-party mods, see below |
| JumperBridges | 21 | 0 | |
| JustFallAlready | 4 | 0 | |
| MotionSensorRange | 9 | 0 | |
| MultichannelCritterSensor | 8 | 0 | |
| NaturalBackwalls | 15 | 2 | worldgen thread bug (harness) |
| NoMagicFridges (retired) | 8 | 0 | |
| OvagrosOverhead | 17 | 0 | |
| RefinedBuilding | 7 | 0 | |
| SaltySurf | 11 | 0 | |
| SmartWeightPlate | 11 | 0 | |
| SmoothBorders | 9 | 9 | worldgen thread bug (harness); 3 skipped (base-game cluster in Spaced Out mode) |
| StoragePodRedux | 11 | 0 | |
| SweepZones | 17 | 0 | |
| UpgradePlanner | 41 | 0 | 17 skipped (Blueprints Expanded) |
| VentFreezeFix | 8 | 0 | |

Isolation reruns after the fixes:

| Run | Result |
|---|---|
| NaturalBackwalls + SmoothBorders (one loadout) | `35 passed, 0 failed, 0 errors, 0 timeouts, 3 skipped of 38; 218.6 s` (the 3 skips are SmoothBorders base-game-cluster tests, correct in Spaced Out mode) |
| FridgeThresholdsExtended with `-Steam 2959340381,2618339179,3557584850` (the mods its tests document) | `5 passed, 0 failed of 5` |
| UpgradePlanner with `-Steam 3468585385` (Blueprints Expanded) | `58 passed, 0 failed, 0 skipped of 58; 413.5 s` |

Net: all 19 mods under mods/ (20 test projects counting NaturalBackwalls' name) pass; every test that can run in Spaced Out mode ran. Not run in base-game mode (SmoothBorders `BaseTerra*`, 3 tests).

Cross-mod check (Player.log of the shared run, snapshot `Player-all-snapshot.log` in scratchpad logs): 36 lines with "Exception", all accounted for: 14 + 7 are the worldgen-thread `get_realtimeSinceStartup` error (harness bug 1, from TestRunner's own `Worldgen.Progress`, not a mod); 1 is NaturalBackwalls' intentional "test error" in its error-reporting test. No Harmony patch-failure lines from any mod of ours. Warnings worth knowing:
- `[SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them` (twice at load): SmoothBorders' patch finds the method changed on U60 and deliberately does nothing, so its border smoothing is likely inactive on U60. Its tests still pass (they test through the mod's own API). SmoothBorders was not in the touched list; needs a look.
- `Missing prefab: DetectAnyElement_Any` (once, game-side warning during a DetectAnyElement test); not a failure, not investigated.
- `Building ... has not been added to plan screen subcategory organization` for StoragePodConfig, SmartWeightPlate, MultichannelCritterSensor (vanilla warning for mods adding buildings; repeated 26 times each).
- `[UpgradePlanner] Blueprints Expanded is not enabled` (expected in the shared loadout).

## Step 4: in-game checks from the fix reports

| Check | Covered by | Result |
|---|---|---|
| Salty Surf info panel text (Water or Salt Water line, Spillage line) | `SaltySurf.Tests/Intake.Descriptors`, `Intake.PropertiesLine` | PASS |
| Upgrade Planner material picker preselect, `Configure` delegates | `MaterialChange.PickerSteady`, `MaterialChange.MaterialsLayout`, `MaterialChange.GreyedButton`, `MaterialChange.BuildMenuMemoryKept`, `Looks.MaterialWindow` | PASS (preselect itself is exercised through the picker tests; no test names the preselect alone) |
| Upgrade Planner rebuild on next tick, paused vs unpaused | `ByHand.Rebuilt`, `ByHand.CancelledRebuild`, `AllBuildings.RebuildOrder` / `RebuildInstant` / `RebuildInstantMany`, `ToolParity.JointPlates`, `DragTiming.Orders`. No test pauses the game. | Rebuild PASS in the (running) game; paused case UNTESTED |
| Jumper Bridges overlay tint | `JumperBridges.Tests/Preview.cs`, `Gates.cs` tests (no test names the tint on the overlay explicitly) | Suite PASS (21); tint UNTESTED as a named check |
| Jumper Bridges broken/repaired handlers (`Subscribe` by handle) | `Deconstruct.cs`, `Power.cs` tests exercise break/repair paths | Suite PASS; no test asserts the unsubscribe on cleanup specifically |
| Ovagro's Overhead yield, growth, harvest | `OvagrosOverhead.Tests/Vines.cs`, `Registration.cs` (crop ids vs vanilla vine), `Persistence.cs`, `Removal.cs`, `WildPlanting.cs`, `Plots.cs` | PASS (17) |
| Ovagro's Overhead wilt animations | none (`Plots.cs` logs `IsWilting` but asserts no animation names) | UNTESTED |
| Copy-settings subscription (Motion Sensor Range, Multichannel Critter Sensor, Smart Weight Plate) | their suites (9, 8, 11 pass); I did not find a test that names copy-settings for each | Suites PASS; copy-settings itself UNTESTED as a named check |
| Bitshifter `RibbonDiagnosticPanel` logic-value-changed event | `Bitshifter.Tests` (30 pass) | Suite PASS; not individually confirmed |
| Motion sensor range slider reaches the sensor / weight plate layers react (scene partitioner callbacks, unit 05) | `MotionSensorRange.Tests`, `SmartWeightPlate.Tests` | Suites PASS; per-layer reaction not individually confirmed |

No tests were added: nothing in the list was quick to add safely under the "pace to game time" and "tests at human speed" constraints without reading each suite in depth.

## Harness changes in common/tools/TestRunner (uncommitted, in the submodule)
1. `Worldgen.cs`: `Worldgen.Progress()` is called from `OfflineWorldGen.UpdateProgress` which on U60 runs on the worldgen worker thread; it read `Time.realtimeSinceStartup`, which Unity forbids off the main thread, so every generated-world test failed ("worldgen gave up ... get_realtimeSinceStartup can only be called from the main thread"). Now it stores `Stopwatch.GetTimestamp()` (`lastProgressTicks`), and `Watch()` compares against `Stopwatch.Frequency`. Verified: NaturalBackwalls and SmoothBorders worldgen tests green (35/38, 3 legit skips).
2. `GameMode.ps1`: `Test-TestSaveKey` crashed (`Split-Path` on an empty string -> "Cannot bind argument to parameter 'Path' because it is an empty string") when kplayerprefs.yaml held an empty `SaveFilenameKey/:`, which a worldgen run leaves. That stopped every later run at startup and left the player's pointer blank. A blank pointer now counts as the run's own, so the kept value is restored. Verified: after the final run the pointer is the player's real save again.
(Unit 04's earlier `Patches.cs` / `Reflect.cs` / README changes are also in that submodule, uncommitted.)

## Friction for whoever records it
- Running several mods through a shell driver loop relaunched the game for every mod; the runner only reuses the session for an identical loadout. A single `-Mods A,B,C` call is the way for a smoke run. Killing my loop script while one of its children was between build and launch killed that child (Bitshifter), harmlessly before launch.
- The runner prints results only when the session ends; progress is visible only in `runs/<label>-<stamp>/results.jsonl`.
- `ks read` was not used; the brief's pointers were per-mod error-handling/code-style standards and none governed running tests. Nothing that governs this work (the TestRunner/ModTest README and the testing memory notes) was in the brief.

## Files
- Raw outputs and Player.log snapshots: `C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad\logs\` (`SaltySurf.txt`, `UpgradePlanner.txt`, `CornerBuildAnything.txt`, `ALL.txt`, `iso-*.txt`, `iso2-*.txt`, `Player-all-snapshot.log`, `Player-UP-final.log`).
- Run folders: `common\tools\TestRunner\runs\all-20261107-002413` (shared run) and the `run-20261107-*` folders.
- No failure screenshots were taken: the only failures were the harness bugs above, which are explained by the log/stack trace; there was no stuck-test state to capture.
