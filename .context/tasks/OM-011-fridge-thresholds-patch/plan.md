## Task: OM-011
## Branch: OM-011-fridge-thresholds-patch
## Objective: A new patch mod, Fridge Thresholds Extended (mods/fridge-thresholds-extended), that makes the High/Low hysteresis sliders added by Storage Refrigerator Thresholds (llunak, Workshop 2959340381) actually drive the logic output of Psyko's Freezer (Workshop 2618339179) and of the fridges in Ronivan's Industrial Revolution. Today the sliders appear and save on those buildings, but their logic port only reports "full".
## Folder: .context/tasks/OM-011-fridge-thresholds-patch/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
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

## Progress
- [x] Earlier session investigation: the Freezer slider is a mod gap, not an oni-moono fault (reasoning above in Key Files)
- [x] Investigate — units/01-research-report.md; design in the investigation handoff block
- [x] Update this plan with findings — Jeremy chose named IR fridges, no CabinetFrozen
- [x] Implementation: new mod skeleton + Compat patches — units/02-implement-report.md; build 0 warnings 0 errors (re-run by manager); also added to sln, root README, .github templates and label-mod.yml (GitHub label to create at release)
- [ ] Review of the new mod ← IN PROGRESS
- [ ] In-game test with the three Workshop mods loaded
- [ ] Completion: review, retro, commit, PR

## Open Questions / Blockers
- To check in game: Harmony accepts `Component __instance` on the manual Freezer prefix; sliders drive the port on Freezer and IR fridges; first signal after load.
- ICON friction to record at close: the research brief repeated the same two sections once per mod node (17 copies each).

## Constraints
- Patch mod must load cleanly when any of the three target mods is missing.
- Don't redistribute or modify the other authors' code; patch at runtime only.
- Jeremy runs the game on oni-moono (.NET 10 host); Harmony inlining caveats apply (Moono rebuilds dependents of patched methods).
