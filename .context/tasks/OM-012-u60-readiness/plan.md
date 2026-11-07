## Task: OM-012
## Branch: OM-012-u60-readiness
## Objective: Get every mod ready for ONI U60 using Klei's public testing branch (installed now; ONI-moono launch params removed). Find what breaks against the new game assemblies — compile errors, Harmony patch targets that moved or changed signature, runtime errors in the in-game tests — and fix it.
## Folder: .context/tasks/OM-012-u60-readiness/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-012-u60-readiness
- **Attempts (current phase)**: 1

## Decisions
- Two lanes (Jeremy, 2026-11-07): fixes that work on both U59 744825 and U60 ship now (this branch, PR to main; common/ branch `u60`); changes that need U60-only API (Ovagro's Overhead, any obsolete-overload replacement missing in U59) go on a held branch `OM-012-u60-only` stacked on this one, merged when U60 is officially released.
- Only U60-only changes raise `minimumSupportedBuild` to 762230 (the testing build; the release build will be higher).
- No Magic Fridges is retired (Jeremy, 2026-11-07): vanilla U60 fixes the problem it was made for. No U60 code changes to it; how it's retired on the Workshop/README is settled at U60 release.
- "Everything else gets updated" includes the at-risk items: obsolete Subscribe/Unsubscribe and GameScenePartitioner.Add overloads move to their U60 replacements, and the upgrade-planner tick timing and jumper-bridges overlay get checked in game.

## Key Files
- ONI-Mods.sln: build of every mod against the testing-branch assemblies
- common/oni-mod.props: where the game's managed folder is referenced

## Phase Handoff Log

### Handoff: investigation → implementation
**Sub-agent outputs**:
- [explore] units/01-explore-u60-breakage.md: U60 testing build 762230 (preview branch; U59 was 744825). Game read from F:\SteamLibrary\...\OxygenNotIncluded via common/oni-mod.props; frozen U59 copy at F:\ONI-Frozen\APP used as diff baseline. Broken: salty-surf (MechanicalSurfboard GetDescriptors → CollectDescriptors, TargetMethod null, Mod.cs:121-145); ovagros-overhead compile (Crop.CropVal now `products` list, YieldAmount → YieldFactor, VineBranchCeilingConfig.cs:68,78,82); upgrade-planner compile (MaterialSelector.OnSelectMaterial now private, Patches.cs:77, MaterialPicker.cs:71 likely next); corner-build-anything tests compile (Constructable.GetOffsets → IReadOnlyList, Reach.cs:99,132,165,202); common TestRunner Timelapser patch silently bypassed (new private CapturePng). At risk: upgrade-planner rebuild now next tick; jumper-bridges overlay rewritten (fields unchanged); obsolete Subscribe/GameScenePartitioner.Add overloads in 6 mods. All mod_info.yaml minimumSupportedBuild 744825. No Magic Fridges: partly fixed in vanilla (fridges CoolingOnly; deep-frozen items no longer warmed); heat sharing, finite compressor mass and insulation remain mod-only.
**Reviewer findings**: N/A this phase
**Verification evidence**: in the unit report (build: 9 errors / 307 warnings; patch-check and IL diff outputs in scratchpad)
**What the next phase needs**: Jeremy's answers on No Magic Fridges and on release timing / U59 compatibility before the ovagros fix shape is fixed.

## Progress
- [x] Investigate — unit 01: 3 mods + corner tests + TestRunner broken; see handoff
- [x] Update this plan with findings and the fix list
- [ ] Fix per mod — units 02 (salty-surf, upgrade-planner, corner tests), 03 (ovagros-overhead), 04 (common TestRunner + obsolete overloads) ← IN PROGRESS
- [ ] In-game tests per touched mod (narrow filters; ask before long runs)
- [ ] Close: review, retro, commit, PR

## Open Questions / Blockers
- How No Magic Fridges is retired (Workshop text, README, hide/unlist) — at U60 release.
- Re-check the four transpilers against the release U60 build (testing build is a debug build).

## Constraints
- Test runs take Jeremy's desktop: narrowest filter, ask before long or chained runs; never -KeepGame.
- Minimal mod loadout for in-game checks.
- Never save over an existing save while on U60: a U60 save can't be opened in the live build, so that's the one irreversible action. Test runs use their own new or fixture saves; check SaveRunner/TestRunner don't write back to a loaded save before any run.
- Workshop/changelog text follows the repo's style memories; no release in this task unless asked.
