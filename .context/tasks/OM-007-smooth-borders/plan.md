## Task: OM-007
## Branch: OM-007-smooth-borders
## Objective: A new mod, "Smooth Borders", that makes the neutronium world border perfectly flat: constant thickness along every edge instead of the vanilla jagged, random-walk outline (1-tile spikes, notches, stepped mounds; see before-border.png in this folder). Applies at worldgen only.
## Folder: .context/tasks/OM-007-smooth-borders/

## Phase State
- **Phase plan**: investigation → architecture → implementation → testing → completion
- **Completed**: none
- **Current**: investigation   (status: in-progress)
- **Next**: architecture
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-007-smooth-borders
- **Attempts (current phase)**: 1

## Decisions
- Shape: perfectly flat, constant thickness (user, 2026-10-19). Rolling curves and a settings toggle were offered and not chosen.
- Scope: new worlds only (user, 2026-10-19). Existing saves keep their borders; nothing is added to or removed from a running colony.
- Approach is discussed with the user before building (user, 2026-10-19: "we can talk about the cleanest way to accomplish this, since worldgen is complicated"). Investigation reports first; options go to the user; no implement unit before the user picks.
- Task id OM-007: OM-001..OM-006 are taken across this store and the common/ submodule store.

## Key Files
- .context/tasks/OM-007-smooth-borders/before-border.png: the user's screenshot of the vanilla bottom border

## Phase Handoff Log

## Progress
- [ ] Investigate: how vanilla worldgen draws the neutronium border, what else shapes the edge, where a mod can hook it ← IN PROGRESS
- [ ] Bring the approach options to the user; record the choice
- [ ] Update this plan with the implementation steps

## Open Questions / Blockers
- Thickness of the flat border: vanilla's minimum, its average, or something else.
- Whether things other than the border walk reach the edge (POI/template bounds the border skips, subworld features, space-surface tops) and how a flat border meets them.
- Determinism: the border walk consumes the worldgen RNG; changing it may shift every later random draw and so change the seed's layout. Must be checked.

## Constraints
- Worldgen only; no change to loaded saves.
- Build: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`; in-game tests through `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <Mod>` (worldgen tests via [OniTest(Worldgen = ...)]).
