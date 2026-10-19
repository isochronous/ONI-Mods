## Task: OM-007
## Branch: OM-007-smooth-borders
## Objective: A new mod, "Smooth Borders", that makes the neutronium world border perfectly flat: constant thickness along every edge instead of the vanilla jagged, random-walk outline (1-tile spikes, notches, stepped mounds; see before-border.png in this folder). Applies at worldgen only.
## Folder: .context/tasks/OM-007-smooth-borders/

## Phase State
- **Phase plan**: investigation → architecture → implementation → testing → completion
- **Completed**: investigation
- **Current**: architecture   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-architecture
- **Branch**: OM-007-smooth-borders
- **Attempts (current phase)**: 1

## Decisions
- Shape: perfectly flat, constant thickness (user, 2026-10-19). Rolling curves and a settings toggle were offered and not chosen.
- Scope: new worlds only (user, 2026-10-19). Existing saves keep their borders; nothing is added to or removed from a running colony.
- Approach is discussed with the user before building (user, 2026-10-19: "we can talk about the cleanest way to accomplish this, since worldgen is complicated"). Investigation reports first; options go to the user; no implement unit before the user picks.
- Candidate approach from the user (2026-10-19): skip worldgen and reshape the border on a new world's first load (replace neutronium past a flat line with another element, fill notches). "Clunky but it works"; offered as an option, not chosen. Sent to unit 01 to cover alongside the worldgen hooks.
- Runs once, at the end of worldgen (user, 2026-10-19): even a tile-replacement approach hooks the end of the worldgen pipeline, runs once on generation and never again; first-load replacement stays only as the fallback in the comparison. Sent to unit 01.
- Approach A (user, 2026-10-19): draw the border flat inside `WorldGen.DrawWorldBorder` (unit 01 option A/A′), keeping vanilla POIBounds so geysers, POIs and story traits match a vanilla seed; critter/plant placement may differ. Chosen over P2 (exact spawn identity, unsettled edge liquids, deeper hook) and P1 (same result as A, more parts). Runs once at generation.
- Thickness is configurable per asteroid type (user, 2026-10-19): separate settings, defaults = vanilla values (the walk's floor T-R; 4 on the primary), primary range 1-10 as the user's first guide; each setting's label shows its valid range; a top-left-aligned text block above the fields explains that vanilla gives different asteroid types different wall thicknesses (user's wording was a draft: "Different asteroid type have different ranges in vanilla so that moonlets, small asteroids, and the primary get different wall thicknesses").
- Task id OM-007: OM-001..OM-006 are taken across this store and the common/ submodule store.

## Key Files
- .context/tasks/OM-007-smooth-borders/before-border.png: the user's screenshot of the vanilla bottom border

## Phase Handoff Log

### Handoff: investigation → architecture   (commit: Phase-Handoff: investigation)
**Sub-agent outputs**:
- explore unit 01 (units/01-explore-report.md), game build U59-744825: "`WorldGen.DrawWorldBorder` (WorldGen.cs 1354-1439) draws a random walk on each edge: thickness = T + offset, the offset moves by {-2,-1,0,+1} per row or column and is kept within [-R, R]." Defaults T=10, R=6 (worldgen/defaults.yaml 35-38); 54 world files override. Over open space the border is skipped. "Its random source is always `new SeededRandom(0)` (945), used by nothing else" — the border is the same for every seed of a given world size/settings; the screenshot's mound is the walk (reproduced by scratchpad border_sim.py, unconfirmed in game). The border still moves later draws through its output: POIBounds feed template/geyser/story-trait placement (`myRandom` shuffle, TemplateSpawning.cs ~397; traits carry across asteroids); its cells feed mob/plant placement (one shared terrain-seed RNG per world, MobSpawning.cs 109-174). "The cells written to the save are the sim's state inside `SaveSim`, not `simData`" (WorldGenSimUtil.cs 111, 138); a pass after `DoSettleSim` returns changes nothing. Options (report Q5 table): A transpiler on the four `RandomRange` calls making each step constant; A′ = A plus vanilla POIBounds from a replayed walk ("Templates, geysers and story traits stay identical to vanilla for the seed"); B full replacement; C/C′/C″ settings only (C has a Mono inlining risk on the 20-byte `GetIntSetting`); D post-process (can only add); P1 prefix on `DoSettleSim` editing `simData` with a `DrawWorldBorder` prefix snapshot of the strip; P2 inject after the tick-498 template stamp, `ModifyCell` into the live worldgen sim, `simData` left vanilla ("the whole spawn list is identical to vanilla"; restored liquids unsettled; reaching `Sim.Save` unverified); F first-load fallback (`WorldGenSpawner.OnSpawn` while `hasPlacedTemplates` false; fill is a guess). Flat thickness: "T-R is the natural choice" (4 on default worlds; the walk's floor, so only removal, no fill); TinyMagma T=0 stays 0. Removing the mod later leaves the flat border; adding it to an old save does nothing. Decompiled code: scratchpad\decomp\ (acs, fp).
**Reviewer findings**: N/A this phase.
**Verification evidence**: none run in game; code reading plus border_sim.py reproduction of the walk.
**Decisions delta**: none yet; options go to the user.
**Key files delta**: none.
**What the next phase needs**: the user's choice of approach (A′ / P1 / P2) and thickness (T-R / T); then a plan unit for the scaffold and patch. Packet note from unit 01: the generated-worlds testing records (blk-87272da04dee, blk-c8a18ea4088d) were reachable only by --tag generated-worlds.

## Progress
- [x] Investigate — unit 01 reported; mechanism, RNG effects and the hook table above
- [x] Bring the approach options to the user; record the choice — A, per-asteroid-type thickness settings
- [ ] Design unit 02: settings grouping, ranges, labels, text block, flat draw + POIBounds with thickness above vanilla ← IN PROGRESS
- [ ] Show the user the grouping and labels; record
- [ ] Update this plan with the implementation steps

## Open Questions / Blockers
- Thickness of the flat border: vanilla's minimum, its average, or something else.
- Whether things other than the border walk reach the edge (POI/template bounds the border skips, subworld features, space-surface tops) and how a flat border meets them.
- Determinism: the border walk consumes the worldgen RNG; changing it may shift every later random draw and so change the seed's layout. Must be checked.

## Constraints
- Worldgen only; no change to loaded saves.
- Build: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`; in-game tests through `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <Mod>` (worldgen tests via [OniTest(Worldgen = ...)]).
