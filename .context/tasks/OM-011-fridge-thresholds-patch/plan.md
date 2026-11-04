## Task: OM-011
## Branch: OM-011-fridge-thresholds-patch
## Objective: A new patch mod (mods/<name>, name to be confirmed) that makes the High/Low hysteresis sliders added by Storage Refrigerator Thresholds (llunak, Workshop 2959340381) actually drive the logic output of Psyko's Freezer (Workshop 2618339179) and of the fridges in Ronivan's Industrial Revolution. Today the sliders appear and save on those buildings, but their logic port only reports "full".
## Folder: .context/tasks/OM-011-fridge-thresholds-patch/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-011-fridge-thresholds-patch
- **Attempts (current phase)**: 1

## Decisions
- Ship as a separate patch mod in this repo rather than waiting on upstream: the thresholds mod's own source says Freezer support is pending a Freezer PR that has not landed (Storage.cs:304-305 in Workshop 2959340381).

## Key Files
- C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs: Storage Refrigerator Thresholds source. `RefrigeratorThresholds` component (369-378), serialized fields (23-38), `UpdateLogicState` (105-119), Refrigerator prefix (346-366), `OtherMods_Patch` adds the component to Psyko's FreezerConfig (385-412), reflection fallback that calls the modded building's own UpdateLogicCircuit (304-313).
- C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2618339179\Freezer.dll: Psyko's Freezer. `Psyko.Freezer.Freezer.UpdateLogicCircuit` sends `filteredStorage.IsFull() & operational.IsOperational` with no thresholds.
- Decompiled sources from the earlier investigation: scratchpad\freezer-src and scratchpad\srt-src (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad).

## Phase Handoff Log

## Progress
- [x] Earlier session investigation: the Freezer slider is a mod gap, not an oni-moono fault (reasoning above in Key Files)
- [ ] Investigate: identify Ronivan's Industrial Revolution fridge buildings and their classes; decide how the patch mod reaches the thresholds mod's component and UpdateLogicState (reflection vs. reference), and how it behaves when either target mod is absent ← IN PROGRESS
- [ ] Update this plan with findings and next steps from investigation
- [ ] Implementation
- [ ] In-game test with the three Workshop mods loaded
- [ ] Completion: review, retro, commit, PR

## Open Questions / Blockers
- Mod folder / display name not chosen yet.
- Which Industrial Revolution buildings count as fridges, and whether the thresholds mod already adds its component to them.

## Constraints
- Patch mod must load cleanly when any of the three target mods is missing.
- Don't redistribute or modify the other authors' code; patch at runtime only.
- Jeremy runs the game on oni-moono (.NET 10 host); Harmony inlining caveats apply (Moono rebuilds dependents of patched methods).
