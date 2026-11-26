## Task: OM-017
## Branch: OM-017-smart-storage-threshold-init
## Objective: Smart Storage Lockers using the High/Low threshold (hysteresis) settings from Storage Refrigerator Thresholds (Workshop 2959340381) start with a wrong automation output after a save loads and correct themselves only after something refreshes them. Find the cause (that mod alone, or another of Jeremy's mods interfering) and fix or work around it, the way the earlier fridge init-state bug was handled.
## Folder: .context/tasks/OM-017-smart-storage-threshold-init/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-017-smart-storage-threshold-init
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-017: OM-016 was already taken by common/.context/tasks/OM-016-saverunner-memory-probe (same store, same prefix); renamed 2026-11-26.
- Mod id: Jeremy wrote 295340381; no such mod is installed, so this is taken as 2959340381 (Storage Refrigerator Thresholds), told to Jeremy 2026-11-26.

- Fix home: Fridge Thresholds Extended (Jeremy, over a new mod); its scope, README and Workshop text widen to Smart Storage.
- Confirm the cause with a logging-only run in his save before fixing (Jeremy).
- Cause candidate 1 (vanilla signal at spawn, LastSetFlag null): Jeremy says any small slider nudge fixes it.

## Key Files
- %USERPROFILE%/Documents/Klei/OxygenNotIncluded/mods/Steam/2959340381/Source/Storage.cs: Storage Refrigerator Thresholds ships its source; StorageThresholds component and the StorageLockerSmart prefix are in it.
- (to be found) Storage Refrigerator Thresholds: %USERPROFILE%/Documents/Klei/OxygenNotIncluded/mods/Steam/2959340381
- (candidate interferer) Storage Quotas: mods/Steam/3781549526
- mods/fridge-thresholds-extended/: our mod that drives fridge ports from the same thresholds; its earlier init-state fix is the precedent (OM-011)

## Phase Handoff Log

## Progress
- [x] Investigate (code reading) — units/01-investigate-report.md: not the OM-011 order bug; no other enabled mod touches the Smart Storage port (Storage Quotas patches FilteredStorage.IsFull, which only Refrigerator calls). Only wrong window found: at spawn the mod sends 0 because power is not evaluated yet, then corrects on the first electrical tick (paused load → until unpause). The mod sends only on change, so a "refresh" fixes it only if an input changed (fill, power, sliders, checkbox, capacity, copy settings); opening the side screen writes nothing.
- [x] Ask Jeremy what the "refresh" was — moving a slider fixes it. He also says Smart Storage takes no power since U60, which contradicts the report's power-window finding (report read EnergyConsumer/powered); re-check which game build the decompiled DLL is.
- [x] Follow-up investigation (report "## Follow-up 2026-11-26"): build 764025 = Jeremy's; locker has no power, always operational. Only the replaced UpdateLogicAndActiveState writes the port. Slider runs the same calc, sends only on change. Two candidates: (1) vanilla ran at spawn instead of the mod's prefix (thresholds component missing/bypassed then), LastSetFlag left null, so any later call sends — matches "any nudge fixes"; (2) `activated` wrong after load with fill between Low and High — fixed only by dragging past the fill.
- [x] Ask Jeremy: does any small nudge fix it? — yes, any nudge → candidate 1 (vanilla signal at spawn, LastSetFlag null).
- [x] Decide with Jeremy: fix goes in Fridge Thresholds Extended; log first in his save (both his picks, 2026-11-26).
- [x] Unit 02: logging-only diagnostic in FTE — commit bc2e8f6 (revert before the fix), built and deployed to mods\local\FridgeThresholdsExtended; lines tagged [FTE-DIAG], mismatch sweeps at 2 s and 20 s. Save Runner run NOT done: it launches only through Steam (no Moono, per OM-014), and Jeremy's game was open.
- [x] Diagnostic run in his save on Moono (Save Runner -Runtime Moono, OM-018; autosave Cycle 993; log common/tools/SaveRunner/runs/om017-moono-20261126-150108/moono.log). CONFIRMED candidate 1, with the order reversed from the code-reading guess: each locker's OnSpawn runs before the thresholds component has registered ("Locker.OnSpawn enter ... comp=present spawned=True registered=False"; "Thresholds.OnSpawn exit ... registered=True" comes after). The spawn-time UpdateLogicAndActiveState ran the vanilla body ("originalRan=True ... last null->null"), so LastSetFlag stays null. Sweep 2 s: 16 lockers, 11 with the wrong port value (8 expected 0 got 1, 3 expected 1 got 0), 5 right by chance; sweep 20 s still wrong.
- [x] Unit 03 (units/03-fix-report.md): f70e300 reverts the diagnostic; 69c6de2 fix + tests; 48db95e README/CHANGELOG/Workshop/mod.yaml. Why the locker spawns first: StorageThresholds has [MyCmpGet] StorageLockerSmart, and KMonoBehaviour.Spawn spawns MyCmpGet targets before its own OnSpawn — same on stock and Moono. Fix: postfixes on StorageLockerSmart.OnSpawn and ThresholdsBase.OnSpawn; the second to run clears LastSetFlag and calls UpdateLogicAndActiveState through the thresholds prefix. Vanilla Refrigerator has the same order but self-corrects on its first power tick (LastSetFlag null); FTE's fridges already covered by ThresholdsRefresh. Release build 0 warnings, ModCheck passed, fixed build deployed to mods\local. No strings → no translations.
- [x] In-game tests 2026-11-26 (Jeremy: "go" covers all needed runs). Stock run-20261126-152859: PASS WriteRoundTrip, PASS LoadSendsThresholds. Fix-off run-20261126-153024 (fresh game): PASS WriteRoundTripFixOff, FAIL LoadSendsThresholds "expected 1, got 0" — as intended. Moono run-20261126-153125-moono: both PASS.
- [ ] Harness bug: fix-off run-20261126-152950 reused the live game from the previous run; after WriteRoundTripFixOff passed, loading fte-smart-storage.sav made the game quit ("Application wants to quit" ~0.2 s after the load, no status file). The same load in a fresh game works. Investigate and fix in TestRunner (harness changes welcome) ← IN PROGRESS
  — moved to OM-018 (common): not reproducible on demand; App.QuitCode/crash-dialog tracing added there (87a6ba0).
- [x] Review (units/04-review-report.md): 0 critical, 2 moderate, 5 minor. Fixed: M1 ThresholdsBase.Get/LastSetFlag binding in a try/catch with an IsStatic check, so a changed signature skips the Smart Storage part with a warning instead of the whole mod; M2 the fix-off test sets a process marker and RestoreFix only restores when it is set, so WriteRoundTrip's presence assert checks what the mod installed; minor 3 WriteRoundTripFixOff restores the fix when it fails before saving, doc corrected; minor 4 unused parameter dropped; minor 5 Workshop line 1 back to Jeremy's wording, list item shortened; minor 6 README/CHANGELOG no longer say "only full". Left: minor 7 (commit 7646900 moved the common pointer without saying so; history not rewritten). Re-run after the fixes: stock PASS/PASS, fix-off (reused) PASS/FAIL "expected 1, got 0", stock again in the same reused game PASS/PASS (fix restored), Moono PASS/PASS.
- [ ] Completion: retro, PRs (oni-mods-common for OM-018 first, then ONI-Mods) ← IN PROGRESS
- [ ] Fix: resend once both StorageLockerSmart and the thresholds component have spawned, order-independent; README/Workshop text widened to Smart Storage
- [ ] In-game test (harness) that fails with the fix off; check on Moono
- [ ] Logging-only diagnostic build (postfixes on StorageLockerSmart.OnSpawn / UpdateLogicAndActiveState), run on his save on Moono
- [ ] Diagnostic logging postfix on StorageLockerSmart.UpdateLogicAndActiveState, reproduce in his save on Moono
- [ ] Update this plan with the fix (power-window guard prefix, or resend when the port differs from LastSetFlag)

## Open Questions / Blockers
- What "refresh" means in game (re-opening the side screen, moving a slider, an item stored/removed) — find from code which path sets the port correctly.

## Constraints
- Jeremy plays on oni-moono; a fix for his bug is also checked with -Runtime Moono.
- Bug may come from his other mods: check their patches on Smart Storage / StorageLocker / LogicPorts (test-with-player-mods).
- In-game test runs hold his desktop: narrowest filter, ask before long runs.
