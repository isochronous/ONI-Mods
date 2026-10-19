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
- Placement (user, 2026-10-19): not its own mod. Bitshifter gains a companion toolkit of buildings for 31-bit ribbons; the Ribbon Monitor is its first building, #2-#4 follow in the same toolkit.
- Art (user, 2026-10-19): 3gub's prototype kanim is C:/temp/diagnostic_panel_anim.bytes + diagnostic_panel_build.bytes and C:/tmp/diagnostic_panel_0.png (texture: 8 × 4 panel with 31 cells, lamps white because the game tints them at runtime, a "temp" placeholder UI icon); free to rename when moving them into the mod. Working copies in C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/57fa2d13-c310-4f59-90ad-9f226f41d92c/scratchpad/art-3gub/; originals left in place until the move.
- Behaviour (user, 2026-10-19): ribbon input and ribbon output. Output connected: a 31-channel switch, the player sets each bit. Both connected: output = input OR switches. Lamp colours: green = incoming bit on, purple = switch on; purple wins when both are on; output-only lamps show switches in purple. Input only: a monitor (green lamps).
- Switch UI (user, 2026-10-19): a side-screen grid of toggles matching the 8 × 4 face, with a fallback to the vanilla Ribbon Reader/Writer style bit selector extended to allow several bits on at once.

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
- "Fallback" for the switch UI: read as "use the Reader/Writer-style selector if the grid side screen proves impractical"; confirm with the user if the grid turns out feasible but the fallback is wanted as well.
- Behaviour with input only and no output connected: lamps show input (green), switches have no effect; confirm whether switch settings should still show (purple) when the output is unconnected.
- Which symbols, frames and animations 3gub's kanim carries, and whether they fit driving 31 lamps independently.

## Constraints
- Work only in the worktree C:/dev/ONI-Mods-OM-008.
- Tests at human speed; in-game tests via common/tools/TestRunner.
- No hedging language in player-visible text; build icon style rules apply to the build menu icon.
