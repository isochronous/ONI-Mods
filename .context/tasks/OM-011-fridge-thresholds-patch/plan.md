## Task: OM-011
## Branch: OM-011-fridge-thresholds-patch
## Objective: A new patch mod, Fridge Thresholds Extended (mods/fridge-thresholds-extended), that makes the High/Low hysteresis sliders added by Storage Refrigerator Thresholds (llunak, Workshop 2959340381) actually drive the logic output of Psyko's Freezer (Workshop 2618339179) and of the fridges in Ronivan's Industrial Revolution. Today the sliders appear and save on those buildings, but their logic port only reports "full".
## Folder: .context/tasks/OM-011-fridge-thresholds-patch/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation, implementation, testing, completion
- **Current**: completion   (status: done)
- **Next**: none — task complete
- **Loaded skill**: task-plan-phase-completion
- **Branch**: OM-011-fridge-thresholds-patch
- **Attempts (current phase)**: 0

## Decisions
- IR fridges: target the five DupesRefrigeration fridge configs by class name, prefix on DoPostConfigureComplete (Jeremy, over the generic any-Refrigerator form).
- CabinetFrozen out of scope; SpaceBox skipped (RationBox sends no signal).
- Freezer: prefix UpdateLogicCircuit + postfix OnCopySettings; skip if a llunak prefix is already present.
- No compile-time reference to the three mods; all reflection, bound in OnAllModsLoaded.
- Name: "Fridge Thresholds Extended", folder mods/fridge-thresholds-extended (Jeremy's choice, 2026-11-04).
- Ship as a separate patch mod in this repo rather than waiting on upstream: the thresholds mod's own source says Freezer support is pending a Freezer PR that has not landed (Storage.cs:304-305 in Workshop 2959340381).

## Key Files
- C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs: Storage Refrigerator Thresholds source. `RefrigeratorThresholds` component (369-378), serialized fields (23-38), `UpdateLogicState` (105-119), Refrigerator prefix (346-366), `OtherMods_Patch` adds the component to Psyko's FreezerConfig (385-412), reflection fallback that calls the modded building's own UpdateLogicCircuit (304-313).
- C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2618339179\Freezer.dll: Psyko's Freezer. `Psyko.Freezer.Freezer.UpdateLogicCircuit` sends `filteredStorage.IsFull() & operational.IsOperational` with no thresholds.
- Decompiled sources from the earlier investigation: scratchpad\freezer-src and scratchpad\srt-src (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad).

## Phase Handoff Log

### Handoff: investigation → implementation
**Sub-agent outputs**:
- [research] units/01-research-report.md: IR is Workshop 3557584850 (assembly RonivansLegacy_ChemicalProcessing, v1.20.1). Its five fridges (`RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesRefrigeration.{FridgeSmall,FridgeLarge,FridgePod,HightechBig,HightechSmall}Config`) use vanilla `Refrigerator`, so "add the component only": prefix each config's `DoPostConfigureComplete` with `go.AddComponent(RefrigeratorThresholds)` when absent (prefix so the component spawns before Refrigerator). Thresholds' OtherMods_Patch only lists the old `Advanced_Refrigeration` types. SpaceBox (RationBox, no signal) skipped. Freezer: manual prefix on private `Psyko.Freezer.Freezer.UpdateLogicCircuit` with `Component __instance, FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports`; body mirrors Storage.cs:351-365 using the public ThresholdsBase API (`Get`, `UpdateLogicState`, `IsActuallyOperational`, `LastSetFlag`) via reflection and FilteredStorage's private `GetAmountStored` / `GetMaxCapacityMinusStorageMargin` via `AccessTools.MethodDelegate`. Optional postfix on `Freezer.OnCopySettings` copying SendGreenOnLow/ActivateValue/DeactivateValue. Skip the Freezer prefix if a llunak prefix already exists on it. All lookups in `OnAllModsLoaded` via `AccessTools.TypeByName`, try/catch per group, warn on missing members; no compile-time reference to any of the three mods and no foreign type in any patch signature. Moono: patch at load time (D34), use TypeByName (D49).
**Reviewer findings**: N/A this phase
**Verification evidence**: code reading only (decompiles in scratchpad\ir-src, freezer-src, srt-src, vanilla); nothing run.
**Decisions delta**: IR fridges targeted by name (Jeremy). CabinetFrozen out of scope (Jeremy). SpaceBox skipped. Freezer OnCopySettings postfix included, to match what the thresholds mod does for the vanilla fridge.
**Key files delta**: mods/fridge-thresholds-extended/** (new); pattern sources mods/vent-freeze-fix (skeleton), mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs:22-58 (optional-mod compat shape).
**What the next phase needs**: the research report; the repo skeleton pattern; the decisions above. Implementation must build with `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`.

### Handoff: implementation/testing → completion
**Sub-agent outputs**: units/02-implement-report.md (mod + Fixes), units/04-test-report.md (runs 1-4, review 2 fixes).
**Reviewer findings**: review 1 (units/03-review-report.md): M1 fixed, M2 tests added, minors fixed or accepted. Review 2 (units/05-review-report.md): moderate test-name fixed; same-frame builds and debug logs fixed; green-on-low assertion and Patch-line order left as follow-ups.
**Verification evidence**: run-20261104-131755: 5 passed, 0 failed, 0 errors (before the review-2 test edits). `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`: Build succeeded, 0 Error(s), no warnings from FridgeThresholdsExtended projects. `ks check --store .`: 0 block.
**Decisions delta**: Jeremy clears Steam launch options himself for test runs; RoundTripLoad discovers its food before the filter.
**Key files delta**: mods/fridge-thresholds-extended/** ; ONI-Mods.sln ; README.md ; .github/ISSUE_TEMPLATE/{bug_report,feature_request}.yml ; .github/workflows/label-mod.yml ; .context/testing/integration-testing.md ; .context/retrospectives/icon.md
**Retro Stage-1 draft**: landed as the OM-011 entry in retrospectives/icon.md (Avoid: check the launch path before a run, don't hand Jeremy guesses as causes; Repeat: read both mods' code to settle mod-vs-environment, vanilla control next to modded building, reflection-only compat bound in OnAllModsLoaded; Updated: two integration-testing sections).

## Completion Summary
**Accomplished:** New mod Fridge Thresholds Extended makes Storage Refrigerator Thresholds' High/Low sliders drive the logic port of Psyko's Freezer and of the five Industrial Revolution fridges, with no build-time reference to any of the three mods.
**Files changed:** new mods/fridge-thresholds-extended (mod + Tests project), sln, root README, .github templates/workflow, two store records.
**Tests:** 5 [OniTest] added; 5/5 passed in game (stock runtime) before the review-2 test edits, which are built but not run.
**Follow-up work:** see Open Questions.

## Progress
- [x] Earlier session investigation: the Freezer slider is a mod gap, not an oni-moono fault (reasoning above in Key Files)
- [x] Investigate — units/01-research-report.md; design in the investigation handoff block
- [x] Update this plan with findings — Jeremy chose named IR fridges, no CabinetFrozen
- [x] Implementation: new mod skeleton + Compat patches — units/02-implement-report.md; build 0 warnings 0 errors (re-run by manager); also added to sln, root README, .github templates and label-mod.yml (GitHub label to create at release)
- [x] Review of the new mod — units/03-review-report.md: no blockers. M1 real (Freezer component spawns after Freezer, stale first signal on load); M2 no Tests project; minors: wrong warning text for a missing IR class (ThresholdsCompat.cs:89-93), Invoke allocation (accepted: runs only on storage/power events), README last line explains mechanics.
- [x] Fix M1, the warning text, the README line — build 0/0 (02-implement-report.md "Fixes")
- [ ] Testing phase: Tests project + one in-game run with the three Workshop mods (units/04-test.md); Jeremy approved the run 2026-11-04 ← IN PROGRESS
  - [x] Tests project FridgeThresholdsExtended.Tests (5 tests in Fridges.cs), -BuildOnly 0/0
  - [x] Run 1 (run-20261104-130136): 0 run, game never started. Steam launch options go through oni-moono (OniMoono.exe -- %command%); moono rejects the --onitest-* args and exits; TestRunner only watches for an OxygenNotIncluded process. mods.json restored byte-identical.
  - [x] Jeremy cleared the launch options himself. Run 2 (run-20261104-130820) and rerun (run-20261104-131031): 4 passed, 1 failed. Verified: Freezer hysteresis matches the vanilla Refrigerator control, red without power; IR SimpleFridge thresholds; Freezer copy settings; 3 patch-applied log lines, no exceptions. RoundTripLoad fails in test setup (food not in the Freezer's filter, dropped on load). M1 partly evidenced: component order correct, LastSetFlag already set on first post-load update.
  - [x] Run 3 (run-20261104-131318): filter set but still 4/1 — real cause: MushBar never discovered (storing does not discover), TreeFilterable.OnSpawn strips undiscovered tags on load and drops the food.
  - [x] Run 4 (run-20261104-131755) with DiscoveredResources.Discover before the filter: 5 passed, 0 failed. After load Freezer holds 35%, sends green once powered; thresholds component ahead of Freezer (34 < 37). M1 evidenced by order + LastSetFlag False at first update, not by the green itself (Freezer unpowered at first update).
- [x] Second review (units/05-review-report.md): no blockers. Fixed in Fridges.cs: RoundTripLoad renamed LoadKeepsOrderFillAndGreen (component order is what proves M1), a game second between back-to-back builds, debug logs removed; -BuildOnly 0/0, ModCheck clean. These last test edits are not run in game.
- [x] Store: testing/integration-testing.md gains "Launching with oni-moono" and "Save round trips and undiscovered items"; retro appended to retrospectives/icon.md; `ks check` 0 block. Brief-duplication friction already recorded (tooling-friction.md 2026-10-21, 2026-10-22), not re-added.
- [x] Completion: PR opened
- [ ] In-game test with the three Workshop mods loaded
- [ ] Completion: review, retro, commit, PR

## Open Questions / Blockers
- Follow-ups (not in this task): (1) not run under oni-moono; TestRunner support for Moono launches would be its own task. (2) The review-2 test edits are built but not run in game; the next run of the suite covers them. (3) Minor, left: green-on-low never asserted against the port; Patch-line order in PatchFreezerLogic; Invoke allocation. (4) The GitHub label fridge-thresholds-extended is created at release.

## Constraints
- Patch mod must load cleanly when any of the three target mods is missing.
- Don't redistribute or modify the other authors' code; patch at runtime only.
- Jeremy runs the game on oni-moono (.NET 10 host); Harmony inlining caveats apply (Moono rebuilds dependents of patched methods).
