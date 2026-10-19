## Task: OM-008
## Branch: OM-008-ribbon-monitor
## Objective: Build the Ribbon Monitor from GitHub issue #5 (isochronous/ONI-Mods; the user called it "issue #1", which was deleted on GitHub, and chose #5): a building for a Bitshifter companion mod that shows the state of all 31 bits on a ribbon at once. Each bit reads lit or unlit at a glance and in the hover tooltip; the point is debugging ribbon setups without a Ribbon Reader on every bit. Art: the user has a prototype png/kanim from 3gub to hand over once the code is ready for it. Recommended display layout in the issue: an 8 × 4 lamp grid (layouts: https://claude.ai/artifact/YVj66UEEudCaAQxPbfUEUu).
## Folder: .context/tasks/OM-008-ribbon-monitor/

## Worktree
Path: C:/dev/ONI-Mods-OM-008
Branch: OM-008-ribbon-monitor
Another agent works in C:/dev/ONI-Mods (branch OM-007-smooth-borders); all of this task's work happens in the worktree. Branched from origin/main d8b5d4e; upstream unset so nothing pushes to main by accident.

## Phase State
- **Phase plan**: investigation → architecture → implementation → testing → completion
- **Completed**: none
- **Current**: investigation   (status: in-progress)
- **Next**: architecture
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-008-ribbon-monitor
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-008: `ks resolve --explain` reports highest OM-007 (OM-007-smooth-borders, the other agent's branch); common/ holds OM-003..OM-005.
- Scope is issue #5 (user, 2026-10-19), not the other Bitshifter companion issues #2-#4.

## Key Files
- mods/bitshifter/: the Bitshifter mod the Ribbon Monitor is a companion to (src/Bitshifter, src/Bitshifter.Tests, tools/make_art.py)

## Phase Handoff Log

## Progress
- [x] Worktree and task branch created
- [ ] Investigate: Bitshifter's structure, ribbon ports and logic APIs, how building art/kanims are made and loaded in this repo, where a companion mod lives ← IN PROGRESS
- [ ] Update this plan with findings and next steps from investigation
- [ ] Architecture: placement (new mod vs inside Bitshifter), display layout, art hand-off format
- [ ] Implementation
- [ ] In-game tests
- [ ] Completion: review, retro, PR

## Open Questions / Blockers
- Is the Ribbon Monitor its own mod, one building in a single companion mod that will later hold #2-#4 too, or a building inside Bitshifter? The issue says "a Bitshifter companion mod".
- Art format from 3gub (png + kanim triplet?) and which states/frames it carries (lamp-per-bit overlays vs whole-building frames).

## Constraints
- Work only in the worktree C:/dev/ONI-Mods-OM-008.
- Tests at human speed; in-game tests via common/tools/TestRunner.
- No hedging language in player-visible text; build icon style rules apply to the build menu icon.
