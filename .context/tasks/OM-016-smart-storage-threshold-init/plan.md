## Task: OM-016
## Branch: OM-016-smart-storage-threshold-init
## Objective: Smart Storage Lockers using the High/Low threshold (hysteresis) settings from Storage Refrigerator Thresholds (Workshop 2959340381) start with a wrong automation output after a save loads and correct themselves only after something refreshes them. Find the cause (that mod alone, or another of Jeremy's mods interfering) and fix or work around it, the way the earlier fridge init-state bug was handled.
## Folder: .context/tasks/OM-016-smart-storage-threshold-init/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-016-smart-storage-threshold-init
- **Attempts (current phase)**: 1

## Decisions
- Mod id: Jeremy wrote 295340381; no such mod is installed, so this is taken as 2959340381 (Storage Refrigerator Thresholds), told to Jeremy 2026-11-26.

## Key Files
- (to be found) Storage Refrigerator Thresholds: %USERPROFILE%/Documents/Klei/OxygenNotIncluded/mods/Steam/2959340381
- (candidate interferer) Storage Quotas: mods/Steam/3781549526
- mods/fridge-thresholds-extended/: our mod that drives fridge ports from the same thresholds; its earlier init-state fix is the precedent (OM-011)

## Phase Handoff Log

## Progress
- [ ] Investigate: Smart Storage threshold output wrong after load, fixes itself after a refresh ← IN PROGRESS
- [ ] Update this plan with findings and next steps from investigation

## Open Questions / Blockers
- What "refresh" means in game (re-opening the side screen, moving a slider, an item stored/removed) — find from code which path sets the port correctly.

## Constraints
- Jeremy plays on oni-moono; a fix for his bug is also checked with -Runtime Moono.
- Bug may come from his other mods: check their patches on Smart Storage / StorageLocker / LogicPorts (test-with-player-mods).
- In-game test runs hold his desktop: narrowest filter, ask before long runs.
