## Task: OM-013
## Branch: OM-013-fanout-gate
## Objective: Add the Fan-out Gate to the Bitshifter ribbon toolkit (user, 2026-11-10). One normal (1-bit) automation input, one ribbon output. When the input is on, the gate sends the configured bits on the ribbon; when it is off, the ribbon is 0. Side screen: like the Ribbon Reader/Writer bit selection panel, but several bits can be selected. Art from 3GuB, ready to use: mods/bitshifter/src/Bitshifter/anim/assets/fanout_gate/ (logic_fanout_0.png, logic_fanout_anim.bytes, logic_fanout_build.bytes; untracked, delivered by the user).
## Folder: .context/tasks/OM-013-fanout-gate/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-013-fanout-gate
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-013: `ks resolve` gives node mods/bitshifter; root .context/tasks holds up to OM-012 (OM-012-u60-readiness).
- Art is 3GuB's and approved for use (user, 2026-11-10); it ships, not a prototype.
- Behaviour (user, 2026-11-10): input on -> output = configured bit mask; input off -> output 0 (assumed off = 0; the user said "signal on input makes the gate send out the configured bits").
- UI (user, 2026-11-10): the vanilla Reader/Writer bit list, multiple selection allowed. Rows only, no Clear all or extra text (user, 2026-11-10).
- Footprint (user, 2026-11-10): 2 x 1 "like every other two-port gate": input CellOffset(0,0), ribbon output CellOffset(1,0), as the Ribbon Writer. The art body (528 x 236) is wider than the Ribbon Writer's place outline (364 x 223), so it overhangs the 400-unit footprint by ~64 units each side; the user asked for that comparison.
- Lamps (user, 2026-11-10): "for now we'll just turn them all on when we get an input signal": input on -> `on_1_1111`; input off (wired) -> `on_0_0000`; nothing wired -> `off`.
- Taken without a user question (precedent OM-008 / Ribbon Writer): player name "Fan-out Gate", id `FanoutGate`; Automation > wires after the Ribbon Diagnostic Panel, tech ParallelAutomation; rotatable R360 like the Ribbon Writer; unwired input = off; art goes through make_art.py from anim-original/fanout_gate/ with bounds fixed and files renamed fanout_gate_*; the scroll-view wrap for the gate's list is applied in the toolkit category for the gate only, so Reader/Writer behaviour under yield is unchanged; bit 32 rows ignored.

## Key Files
- mods/bitshifter/src/Bitshifter/anim/assets/fanout_gate/: 3GuB's kanim (anim name logic_fanout)
- mods/bitshifter/src/Bitshifter/Toolkit/: the toolkit buildings (Ribbon Diagnostic Panel from OM-008) — where the gate goes
- mods/bitshifter/src/Bitshifter/SideScreen.cs: Reader/Writer bit selector the gate's panel follows

## Phase Handoff Log

### Investigation findings (unit 01, units/01-explore-report.md; renders units/01-art/)
- Art: 35 one-frame anims `off`, `place`, `ui`, `on_<input 0|1>_<bbbb>` (input lamp green/red + 4 output lamps green/red, colours baked, no tint). Body 528 x 236 centred on x=0: reads 3 x 1 (lamps sit at x -105 / +117, like a 2-wide's cell centres). Frames have NaN bounds; make_art.py fixes that for the panel. Kanim name comes from the folder: `fanout_gate_kanim`; files/build are named logic_fanout.
- Pattern: copy RibbonDiagnosticPanelConfig.cs / RibbonDiagnosticPanel.cs, one row in Toolkit.Buildings (Toolkit.cs:37), ModStrings entries, po files.
- Side screen: implement ILogicRibbonBitSelector; SetBitSelection toggles a bit in a saved mask; postfix on private LogicBitSelectorSideScreen.RefreshToggles sets every selected row's highlight, in the toolkit patch category. Ignore bit 32 (shared row list may hold 32 rows after a Digital CPU reader).
- Output: SendSignal(Output, inputOn ? mask : 0) on LogicValueChanged + Render200ms poll for a cut wire (as the panel).
- Tests: GateRig after PanelRig; add Anims.Current to the TestKit (harness change).

## Progress
- [x] Investigate: unit 01 — art, toolkit pattern, side-screen route, tests, docs
- [x] Update this plan with findings and the open questions for the user — answered 2026-11-10 (Decisions)
- [ ] Implement the building, logic, side screen, strings, translations, art pipeline, tests — unit 02 ← IN PROGRESS
- [ ] Build, in-game tests (narrow filter), screenshots for the user
- [ ] Completion: review, retro, commit, PR

## Open Questions / Blockers
- Building id, player name, ports, menu position, tech: to propose after investigation.

## Constraints
- In-game tests at human speed, narrow filter, never -KeepGame; screenshots to the user via SendUserFile.
- Changelog and commit messages terse; Workshop/README text without hedging.
- Don't release; the user releases.
