## Task: OM-006
## Branch: OM-006-ovagro-overhead
## Objective: Scaffold a new mod, mods/ovagro-overhead ("Ovagro Overhead"), whose only feature is letting Ovagro plants grow from the ceiling: planted by Duplicants in natural tiles facing down, and in Hydroponic Farm tiles facing down. Their vines must grow as they do on the floor (downward from the ceiling). Plain Farm Tiles get the same support only if the game lets them be rotated to face down.
## Folder: .context/tasks/OM-006-ovagro-overhead/

## Phase State
- **Phase plan**: investigation → architecture → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: architecture
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-006-ovagro-overhead
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-006, not OM-003: `ks resolve --explain` reported highest OM-002 for this node, but the common/ submodule store already holds OM-003..OM-005 under the same prefix.

## Key Files
- mods/ovagro-overhead/: the new mod (to be scaffolded)

## Phase Handoff Log

## Progress
- [ ] Investigate: how the game defines the Ovagro plant, how it is planted (seed placement rules, PlantablePlot / farm tile orientation, natural-tile planting by Duplicants), how its vines grow, and how other ceiling-capable plants (e.g. Wheezewort-style "hanging" plants) are allowed on ceilings; whether Farm Tiles / Hydroponic Farms can be rotated; how existing mods in this repo are scaffolded ← IN PROGRESS
- [ ] Update this plan with findings and the design before any code is written

## Open Questions / Blockers
- Which DLC ships Ovagro, and what its internal ids are (investigation will name them).

## Constraints
- Only feature is ceiling planting for Ovagro; nothing else (user).
- Vines must grow as expected when planted on the ceiling (user).
- Farm Tiles: support only if they can face downward in the game (user).
- New mod follows the existing mods/<name> layout, build and release conventions of this repo.
