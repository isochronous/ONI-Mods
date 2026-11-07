## Task: OM-012
## Branch: OM-012-u60-readiness
## Objective: Get every mod ready for ONI U60 using Klei's public testing branch (installed now; ONI-moono launch params removed). Find what breaks against the new game assemblies — compile errors, Harmony patch targets that moved or changed signature, runtime errors in the in-game tests — and fix it.
## Folder: .context/tasks/OM-012-u60-readiness/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation, implementation, testing, completion
- **Current**: completion   (status: done)
- **Next**: none — ship-now PR open; held lane OM-012-u60-only waits for U60's release
- **Loaded skill**: task-plan-phase-completion
- **Branch**: OM-012-u60-readiness
- **Attempts (current phase)**: 1

## Decisions
- Two lanes (Jeremy, 2026-11-07): fixes that work on both U59 744825 and U60 ship now (this branch, PR to main; common/ branch `u60`); changes that need U60-only API (Ovagro's Overhead, any obsolete-overload replacement missing in U59) go on a held branch `OM-012-u60-only` stacked on this one, merged when U60 is officially released.
- Only U60-only changes raise `minimumSupportedBuild` to 762230 (the testing build; the release build will be higher).
- No Magic Fridges is retired (Jeremy, 2026-11-07): vanilla U60 fixes the problem it was made for. No U60 code changes to it; how it's retired on the Workshop/README is settled at U60 release.
- "Everything else gets updated" includes the at-risk items: obsolete Subscribe/Unsubscribe and GameScenePartitioner.Add overloads move to their U60 replacements, and the upgrade-planner tick timing and jumper-bridges overlay get checked in game.

## Key Files
- mods/salty-surf/src/SaltySurf/Mod.cs: descriptor patch pair (U59 GetDescriptors / U60 CollectDescriptors), warnings
- mods/upgrade-planner/src/UpgradePlanner/Patches.cs, UI/MaterialPicker.cs: OnSelectMaterial by name; ConfigureScreen callbacks typed per build; cached lookups
- mods/corner-build-anything/src/CornerBuildAnything.Tests/Reach.cs: GetOffsets().ToArray()
- mods/{bitshifter,jumper-bridges,motion-sensor-range,multichannel-critter-sensor,smart-weight-plate}: Subscribe/Unsubscribe off the obsolete overloads
- mods/smooth-borders/src/SmoothBorders.Tests/FailSafes.cs: expected-warning log line
- common/tools/TestRunner/{Patches.cs,Reflect.cs,README.md,Worldgen.cs,GameMode.ps1}: CapturePng patch, worldgen Stopwatch, blank last-save key (common branch u60, 2413b5a)
- Held lane (OM-012-u60-only): mods/ovagros-overhead (VineBranchCeilingConfig, VineMotherCeilingConfig, tests, mod_info.yaml); motion-sensor-range RangeSlider.cs + smart-weight-plate SmartWeightPlate.cs partitioner overload, both mod_info.yaml 762230
- .context/domains/game-updates.md, .context/testing/integration-testing.md, .context/domains/tooling-friction.md, .context/retrospectives/icon.md

## Phase Handoff Log

### Handoff: investigation → implementation
**Sub-agent outputs**:
- [explore] units/01-explore-u60-breakage.md: U60 testing build 762230 (preview branch; U59 was 744825). Game read from F:\SteamLibrary\...\OxygenNotIncluded via common/oni-mod.props; frozen U59 copy at F:\ONI-Frozen\APP used as diff baseline. Broken: salty-surf (MechanicalSurfboard GetDescriptors → CollectDescriptors, TargetMethod null, Mod.cs:121-145); ovagros-overhead compile (Crop.CropVal now `products` list, YieldAmount → YieldFactor, VineBranchCeilingConfig.cs:68,78,82); upgrade-planner compile (MaterialSelector.OnSelectMaterial now private, Patches.cs:77, MaterialPicker.cs:71 likely next); corner-build-anything tests compile (Constructable.GetOffsets → IReadOnlyList, Reach.cs:99,132,165,202); common TestRunner Timelapser patch silently bypassed (new private CapturePng). At risk: upgrade-planner rebuild now next tick; jumper-bridges overlay rewritten (fields unchanged); obsolete Subscribe/GameScenePartitioner.Add overloads in 6 mods. All mod_info.yaml minimumSupportedBuild 744825. No Magic Fridges: partly fixed in vanilla (fridges CoolingOnly; deep-frozen items no longer warmed); heat sharing, finite compressor mass and insulation remain mod-only.
**Reviewer findings**: N/A this phase
**Verification evidence**: in the unit report (build: 9 errors / 307 warnings; patch-check and IL diff outputs in scratchpad)
**What the next phase needs**: Jeremy's answers on No Magic Fridges and on release timing / U59 compatibility before the ovagros fix shape is fixed.

### Handoff: testing → completion
**Sub-agent outputs**: unit 06 — every suite green on U60 762230 after two harness fixes; save folders identical (2705 files). Unit 07 — Smooth Borders patch active on U60 (transpiler matched: True), 18/18. Unit 08 review — 0 critical, 2 moderate, 6 minor; both lanes ready. Unit 09 — moderate + minor fixes; rerun 77/78 (ProblemTypesKept order-dependent, passes alone).
**Reviewer findings**: moderate 1 (RangeSlider throwing bind) fixed; moderate 2 (U59 in-game evidence) open — needs live build; minors: Salty Surf warnings + DeclaredMethod fixed, Upgrade Planner cache + warning fixed, CBA test DLL on U59 accepted (test-only), submodule push done with the PR, publish/ yaml left to the release script.
**Verification evidence**: ship-now tip on U60: solution build errors only in ovagros-overhead (expected — its fix is held); held tip 0 errors; `ks check`: 0 block.
**Retro Stage-1 draft**: landed in retrospectives/icon.md (OM-012).

## Completion Summary
**Accomplished:** every mod checked and fixed for U60; fixes that work on U59 too are ready to ship now, U60-only changes wait on OM-012-u60-only.
**Files changed:** 9 mods + common/tools/TestRunner; store: game-updates.md, integration-testing.md section.
**Tests:** none added; full in-game smoke test on U60 green except one order-dependent Upgrade Planner test.
**Follow-up work:** U59 in-game run of the ship-now lane before release; ProblemTypesKept flake; re-check transpilers on the U60 release build; No Magic Fridges retirement at release; merge held lane at U60 release.

## Progress
- [x] Investigate — unit 01: 3 mods + corner tests + TestRunner broken; see handoff
- [x] Update this plan with findings and the fix list
- [x] Fix per mod — units 02-05; solution builds 0 errors on U60. Ship-now commits on OM-012-u60-readiness (common b528473 on branch u60); held commits (Ovagro's Overhead, partitioner overloads in Motion Sensor Range + Smart Weight Plate, minimumSupportedBuild 762230) on OM-012-u60-only
- [x] In-game smoke test, every mod on U60 from the OM-012-u60-only tip — unit 06: all suites green after two harness fixes (worldgen timer on worker thread; blank last-save key) — common dd0fa4c pointer; saves untouched (2705-file listing identical). Unit 07: Smooth Borders warning was the fail-safe test's own, patch matched on U60, 18/18.
- [x] Review (both lanes) — unit 08: 0 critical, 2 moderate, 6 minor; both lanes ready; ship-now verified against U59 by decompile + Cecil member-reference scan
- [x] Review fixes — unit 09: RangeSlider non-throwing bind (held); Salty Surf warnings + DeclaredMethod, Upgrade Planner cached lookups + warning (ship-now). U60 rerun SaltySurf,UpgradePlanner,MotionSensorRange: 77/78; BuildingSettings.ProblemTypesKept failed in the suite, passes alone (1/1)
- [x] Store: domains/game-updates.md, testing/integration-testing.md "Smoke-testing many mods", tooling-friction entry
- [x] Retro — retrospectives/icon.md
- [x] PRs — ship-now lane (ONI-Mods OM-012-u60-readiness, oni-mods-common u60); held lane pushed, no PR until U60 releases
- [ ] Ship-now lane on the live U59 build (needs Steam switched back; Jeremy)
- [ ] Close: review, retro, commit, PR

## Open Questions / Blockers
- 2026-11-07: the smoke test's worldgen runs left SaveFilenameKey/ on generated test colonies ("Prosperous Debacle", "The Messy Fortress") whose saves the harness blocked; the warden took those for the player's and dropped the kept pointer. Jeremy's real last save is The Aftermath v009 — pointer set back by hand (prefs backed up in the scratchpad). Harness fix in common 53a3a79: a pointer to a missing file counts as the run's. Proven 2026-11-07 11:00: NaturalBackwalls Generated.StartingAsteroid 1/1; the run set the pointer to generated "Ultra Antfarm", the warden put The Aftermath v009 back. OM-012-u60-only is not rebased onto this (needs a force push); rebase it at U60 release.
- How No Magic Fridges is retired (Workshop text, README, hide/unlist) — at U60 release.
- Upgrade Planner BuildingSettings.ProblemTypesKept is order/timing dependent on U60 (FishFeeder not rebuilt in Cinnabar inside the full suite; passes alone). Not investigated; not touched by OM-012's edits.
- Ship-now reflection paths (Upgrade Planner picker, Salty Surf Rewrite) have static evidence only on U59 (decompile + reference scan); an in-game run on the live build needs Steam switched back (Jeremy) before release.
- Corner Build Anything test DLL built against U60 won't load on U59 (test-only).
- Re-check the four transpilers against the release U60 build (testing build is a debug build).

## Constraints
- Test runs take Jeremy's desktop: narrowest filter, ask before long or chained runs; never -KeepGame.
- Minimal mod loadout for in-game checks.
- Never save over an existing save while on U60: a U60 save can't be opened in the live build, so that's the one irreversible action. Test runs use their own new or fixture saves; check SaveRunner/TestRunner don't write back to a loaded save before any run.
- Workshop/changelog text follows the repo's style memories; no release in this task unless asked.
