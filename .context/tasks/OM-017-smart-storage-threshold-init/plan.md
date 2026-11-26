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
- [ ] Get the diagnostic log: Jeremy restarts his game (loads the deployed build), loads The Aftermath v009, unpauses ~25 s, nudges one slider on an affected locker, quits; then grep Player.log for [FTE-DIAG] ← IN PROGRESS
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
