## Task: OM-012
## Branch: OM-012-u60-readiness
## Objective: Get every mod ready for ONI U60 using Klei's public testing branch (installed now; ONI-moono launch params removed). Find what breaks against the new game assemblies — compile errors, Harmony patch targets that moved or changed signature, runtime errors in the in-game tests — and fix it.
## Folder: .context/tasks/OM-012-u60-readiness/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-012-u60-readiness
- **Attempts (current phase)**: 1

## Decisions
- (none yet)

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
- [ ] Fix per mod
- [ ] In-game tests per touched mod (narrow filters; ask before long runs)
- [ ] Close: review, retro, commit, PR

## Open Questions / Blockers
- Jeremy thinks No Magic Fridges may be unnecessary in U60 (vanilla fix?). Added to unit 01's scope; if fixed in vanilla, deciding what happens to the mod (retire, mark as live-build-only, or keep) is Jeremy's call.
- Do mods declare a minimum/last-working build (mod_info.yaml `minimumSupportedBuild`) that needs bumping for U60?

## Constraints
- Test runs take Jeremy's desktop: narrowest filter, ask before long or chained runs; never -KeepGame.
- Minimal mod loadout for in-game checks.
- Never save over an existing save while on U60: a U60 save can't be opened in the live build, so that's the one irreversible action. Test runs use their own new or fixture saves; check SaveRunner/TestRunner don't write back to a loaded save before any run.
- Workshop/changelog text follows the repo's style memories; no release in this task unless asked.
