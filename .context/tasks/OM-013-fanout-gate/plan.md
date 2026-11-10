## Task: OM-013
## Branch: OM-013-fanout-gate
## Objective: Add the Fan-out Gate to the Bitshifter ribbon toolkit (user, 2026-11-10). One normal (1-bit) automation input, one ribbon output. When the input is on, the gate sends the configured bits on the ribbon; when it is off, the ribbon is 0. Side screen: like the Ribbon Reader/Writer bit selection panel, but several bits can be selected. Art from 3GuB, ready to use: mods/bitshifter/src/Bitshifter/anim/assets/fanout_gate/ (logic_fanout_0.png, logic_fanout_anim.bytes, logic_fanout_build.bytes; untracked, delivered by the user).
## Folder: .context/tasks/OM-013-fanout-gate/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-013-fanout-gate
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-013: `ks resolve` gives node mods/bitshifter; root .context/tasks holds up to OM-012 (OM-012-u60-readiness).
- Art is 3GuB's and approved for use (user, 2026-11-10); it ships, not a prototype.
- Behaviour (user, 2026-11-10): input on -> output = configured bit mask; input off -> output 0 (assumed off = 0; the user said "signal on input makes the gate send out the configured bits").
- UI (user, 2026-11-10): the vanilla Reader/Writer bit list, multiple selection allowed.

## Key Files
- mods/bitshifter/src/Bitshifter/anim/assets/fanout_gate/: 3GuB's kanim (anim name logic_fanout)
- mods/bitshifter/src/Bitshifter/Toolkit/: the toolkit buildings (Ribbon Diagnostic Panel from OM-008) — where the gate goes
- mods/bitshifter/src/Bitshifter/SideScreen.cs: Reader/Writer bit selector the gate's panel follows

## Phase Handoff Log

## Progress
- [ ] Investigate: the kanim (symbols, anims, size, ports), the toolkit building pattern from OM-008, the bit-selector side screen and whether it can do multi-select ← IN PROGRESS
- [ ] Update this plan with findings and the open questions for the user
- [ ] Implement the building, logic, side screen, strings, translations
- [ ] Build, in-game tests (narrow filter), screenshots for the user
- [ ] Completion: review, retro, commit, PR

## Open Questions / Blockers
- Building id, player name, ports, menu position, tech: to propose after investigation.

## Constraints
- In-game tests at human speed, narrow filter, never -KeepGame; screenshots to the user via SendUserFile.
- Changelog and commit messages terse; Workshop/README text without hedging.
- Don't release; the user releases.
