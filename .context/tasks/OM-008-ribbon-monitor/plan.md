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
- **Completed**: investigation, architecture
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-008-ribbon-monitor
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-008: `ks resolve --explain` reports highest OM-007 (OM-007-smooth-borders, the other agent's branch); common/ holds OM-003..OM-005.
- Scope is issue #5 (user, 2026-10-19), not the other Bitshifter companion issues #2-#4.
- Placement (user, 2026-10-19): not its own mod. Bitshifter gains a companion toolkit of buildings for 31-bit ribbons; the Ribbon Monitor is its first building, #2-#4 follow in the same toolkit.
- Art (user, 2026-10-19): 3gub's prototype kanim is C:/temp/diagnostic_panel_anim.bytes + diagnostic_panel_build.bytes and C:/tmp/diagnostic_panel_0.png (texture: 8 × 4 panel with 31 cells, lamps white because the game tints them at runtime, a "temp" placeholder UI icon); free to rename when moving them into the mod. Working copies in C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/57fa2d13-c310-4f59-90ad-9f226f41d92c/scratchpad/art-3gub/; originals left in place until the move.
- Behaviour (user, 2026-10-19): ribbon input and ribbon output. Output connected: a 31-channel switch, the player sets each bit. Both connected: output = input OR switches. Lamp colours: green = incoming bit on, purple = switch on; purple wins when both are on; output-only lamps show switches in purple. Input only: a monitor (green lamps).
- Switch UI (user, 2026-10-19): a side-screen grid of toggles matching the 8 × 4 face (PLib), only if it works well, is intuitive, looks good and needs no heavy patching; otherwise the vanilla Reader/Writer-style bit list with several bits allowed on. Grid only, not both.
- Output unconnected (user, 2026-10-19): lamps show the input only; the switch controls are disabled, their saved state is kept but ignored until the output is wired.
- Name (user, 2026-10-19): the building is the "Ribbon Diagnostic Panel" (player-visible name), not "Ribbon Monitor" ("display panel sounds like it's read-only"; it is also a switch). The kanim keeps its name `diagnostic_panel`, no rename. The task folder and branch keep the ribbon-monitor name as history.
- Design unit 02 (units/02-design-report.md): Approve with modifications. Toolkit in src/Bitshifter/Toolkit/, toolkit patches under [HarmonyPatchCategory("BitshifterToolkit")] applied in OnLoad with PLib init and Translations.Register; Reader/Writer patches via PatchAllUncategorized after the yield check. Output always sends input OR switches (no wrong pulse on wiring; the player sees switches ignored while unwired). Off lamps hidden. PLib grid side screen, one DetailsScreen.OnPrefabInit postfix; the list fallback is rejected (breaks on stand-down, shared rows, no disabled state).
- Building id `RibbonDiagnosticPanel` (user, 2026-10-19).
- Ports (user, 2026-10-19): input top-left (0,1), output bottom-left (0,0); overrides the design's bottom-row pick.
- Release waits for 3gub's fixed art; the prototype is for building and testing only (user, 2026-10-19).
- Grid palette C, panel mirror (user, 2026-10-19): "most closely resembles the way the existing ribbon readers/writers work in their config panels". The user expects the game's custom-UI support to be weaker than the design assumes, so the side screen is built and screenshotted before anything depends on it; if it fights the game, stop and bring screenshots, do not pile on patches.
- Taken from the design without a user question: purple #9B59D0 (seen in game before tests), tooltip lists on bits with ranges (`Input: 1, 3, 9-12`) plus the ribbon value, build icon unlit per the icon style (screenshot), LogicGate layer like the Reader/Writer, keep the Workshop preview.
- Yield (user, 2026-10-19): the monitor stays available when Bitshifter stands down for Digital CPU or another Reader/Writer mod; its patches apply apart from the Reader/Writer PatchAll.
- Menu and tech (user, 2026-10-19): Automation > wires next to the Ribbon Writer, unlocked by ParallelAutomation.

## Key Files
- mods/bitshifter/: the Bitshifter mod the Ribbon Monitor is a companion to (src/Bitshifter, src/Bitshifter.Tests, tools/make_art.py)
- mods/bitshifter/src/Bitshifter/Mod.cs: load and yield logic (PatchAll skipped when yielding, lines 45-56)
- mods/bitshifter/src/Bitshifter/Patches.cs, SideScreen.cs: Reader/Writer patches, bit-selector scroll view
- mods/bitshifter/src/Bitshifter/Bitshifter.csproj, mod_info.yaml: UseTranslations/UsePLib, version 1.2.0
- mods/bitshifter/src/Bitshifter/anim/assets/<name>/: where the monitor kanim lands
- mods/bitshifter/src/Bitshifter.Tests/Rig.cs, Readout.cs: test rig and symbol-visibility oracle
- mods/multichannel-critter-sensor/src/MultichannelCritterSensor/: building config, plan/tech patches, ModStrings, PLib side screen patterns
- common/tools/Kanim/kanim.py: kanim read/write/rename/render
- common/tools/TestRunner/: harness (needs a tint getter and maybe a side-screen click helper)

## Phase Handoff Log

### Handoff: investigation → architecture   (commit: Phase-Handoff: investigation)
**Sub-agent outputs**:
- explore unit 01 (units/01-explore-report.md), game build 744825. Prototype kanim: build `diagnostic_panel`, 1024 x 512 atlas matching the UVs; symbols `body` (426 x 426 units, so a 2 x 2 building), `place` (white outline), `ui` (placeholder), `TileSize`, `numbers` (2 frames: small labels, large digits), and 31 white bloom lamps `bit_bloom_1..31`, one symbol per bit, laid out 8 x 4 in reading order with a nibble gap, spare 32nd cell empty; anims `off` (no lamps), `on` (all lamps), `place`, `ui`, one frame each. "Each lamp is its own symbol, so `SetSymbolVisiblity(bit_bloom_N, lit)` and `SetSymbolTint(bit_bloom_N, colour)` work per lamp, which is what vanilla does with `bit1_bloom`..`bit4_bloom` (`LogicRibbonReader.UpdateVisuals`)." Body has a dark socket under bits 1-31, so hiding a lamp is a natural off state. Art defects: the large digit under lamp 31 reads 32; small label 29 is about 14 px off its socket; no socket or ribbon mark in the spare cell; `ui` unchecked against the repo icon style.
- Bitshifter: widens Reader/Writer to 31 bits (`Bits = 31`, B/Mod.cs:12), adds no building, no strings, no PLib. "OnAllModsLoaded ... if it returns a reason it logs 'Disabled: ...' and returns without PatchAll (B/Mod.cs:45-56)"; it yields to Digital CPU (Workshop 3244925649 / OniSmallCpu) and to any mod already patching the Reader/Writer. "An IBuildingConfig class is found by reflection ... so it registers even while the mod stands down; anything done with [HarmonyPatch] (plan screen, tech tree, side screen) does not apply when PatchAll is skipped."
- Ribbon value: one int, `LogicPorts.GetInputValue(portId)`; `LogicCircuitNetwork.IsBitActive` 0-based; `LogicPorts.IsPortConnected(portId)` for wiring; `SendSignal(OUTPUT_ID, value)` sets output with or without a network; output values saved by LogicPorts. Refresh on `LogicValueChanged` and `Render200ms` as the Reader does. Tints are not saved; reapply after spawn.
- Hosting a building in Bitshifter: `IBuildingConfig` (pattern MultichannelCritterSensorConfig.cs:13-47), component, plan-screen + tech patches (vanilla Reader/Writer: subcategory `wires`, tech `ParallelAutomation`), `ModStrings` + `<UseTranslations>` + `Translations.Register`, `translations/` pot + zh/ko/ru po, kanim folder `src/Bitshifter/anim/assets/<name>/` loaded as `<name>_kanim`, minor version bump (1.2.0 now), README/CHANGELOG/mod.yaml/Workshop text, store records that say "adds no buildings". Two ribbon ports: `LogicInputPorts`/`LogicOutputPorts` with `RibbonInputPort`/`RibbonOutputPort` at distinct cells; unrotatable (labels do not rotate).
- Switch UI: grid side screen feasible with PLib 4.25.0 (`PGridPanel`, `PToggle`; repo precedents SmartWeightPlateSideScreen, MultichannelCritterSensorSideScreen; needs `<UsePLib>`, `PUtil.InitLibrary`, `PUIUtils.AddSideScreenContent<T>()` in a `DetailsScreen.OnPrefabInit` postfix; ~30 px per toggle in 280 px). Fallback: implement `ILogicRibbonBitSelector` (GetBitDepth 31, SetBitSelection toggles) so vanilla `LogicBitSelectorSideScreen` plus Bitshifter's scroll-view postfix (B/SideScreen.cs:16) serves it, no PLib; one row stays highlighted.
- Tooltip: no vanilla ribbon-value text; a custom `StatusItem` with a resolve callback is the cheapest route; no custom StatusItem in the repo yet.
- Tests: Rig.cs builds switch/writer/reader/alarm; `GetSymbolVisiblity` is readable (Readout.AssertDigits precedent); harness gaps: no symbol-tint getter (colour lives in `SymbolInstanceGpuData`), no side-screen click helper, no hover-text reader.
**Reviewer findings**: N/A this phase.
**Verification evidence**: read-only unit; kanim parsed with common/tools/Kanim/kanim.py and rendered (scratchpad proto-render/off.png, on.png); nothing run in game.
**Decisions delta**: placement in Bitshifter, behaviour and lamp colours, switch UI (user; see Decisions).
**Key files delta**: see Key Files.
**What the next phase needs**: user answers on the open questions below, then a design unit for: building id and kanim name, port cells, component state and update flow (input, switches, output, tints, visibility), gating against Bitshifter's yield, strings and status-item tooltip format, side screen (grid via PLib, fallback ILogicRibbonBitSelector), harness additions (tint getter, side-screen click), test plan, and the art fixes the repo makes vs the artist.

### Handoff: architecture → implementation   (commit: Phase-Handoff: architecture)
**Sub-agent outputs**:
- design unit 02 (units/02-design-report.md): "**Approve with modifications.** 1. Switch UI: the PLib grid, not the list. ... 2. Split the patches with Harmony categories (`[HarmonyPatchCategory]`, present in the game's 0Harmony 2.4.2 ...). The toolkit patches apply in `OnLoad` ... The Reader/Writer patches keep their late, conditional `OnAllModsLoaded` application. 3. The output always carries input OR switches, wired or not. Only the lamps and the controls ignore the switches while the output is unwired. 4. Off lamps are hidden, not tinted dark." Files: `B/Toolkit/{Toolkit.cs, ToolkitPatches.cs, RibbonDiagnosticPanelConfig.cs, RibbonDiagnosticPanel.cs, RibbonDiagnosticPanelSideScreen.cs}`, `B/ModStrings.cs`, `B/Mod.cs` changed, translations, `B/anim/assets/diagnostic_panel/`, `mods/bitshifter/anim-original/diagnostic_panel/`, make_art.py ui step. Component state: one `[Serialize] int switches` (bits 0-30); refresh on input LogicValueChanged and Render200ms (wiring changes raise no event); lamp per bit: switch (output wired) → purple, else input → green #57B95E, else hidden. Two StatusItems (input always, switches while wired), list-of-bits format. Grid: PButton cells 28 x 24 in four PPanel rows with a 16 px nibble gap, ColorStyleSetting per state, `isInteractable = false` when unwired, "All off" button. Harness: `Kit/Anims.cs` (SymbolTint, SymbolVisible), `Kit/Screens.cs` (Select, SideScreen<T>, Click, Deselect), `World.StatusItems`, Reflect.cs fields. 12 tests listed (section 7). Release: 1.3.0, UsePLib + UseTranslations, zh/ko/ru po, README/CHANGELOG/mod.yaml/Workshop, store records, decision record draft (Bitshifter node decisions/001).
**Reviewer findings**: N/A this phase.
**Verification evidence**: design only; Harmony 2.4.2 PatchCategory/PatchAllUncategorized checked by reflection on the game's 0Harmony.dll; id collision checked against 201 installed Workshop mods; palette mock rendered (scratchpad/om008-design/grid-directions.png).
**Decisions delta**: design approved with its modifications; id RibbonDiagnosticPanel; ports input (0,1) top-left, output (0,0) bottom-left; palette C; release waits for fixed art.
**Key files delta**: the Toolkit/ files, ModStrings.cs, anim folders, make_art.py, TestRunner Kit/Anims.cs, Kit/Screens.cs, Kit/World.cs, Reflect.cs.
**What the next phase needs**: design steps 1-6 first (art in, build setup, strings, building, patches + Mod.cs, side screen), then in-game screenshots for the user (grid wired and unwired, lit face, hover card) and a stop for the user's call on the look. Steps 7-11 (harness, tests, translations, docs, store) only after that.

## Progress
- [x] Worktree and task branch created
- [x] Investigate — unit 01 report; prototype kanim fits per-lamp tinting; Bitshifter needs strings, building patches and a gating choice
- [x] Update this plan with findings and next steps from investigation
- [x] Architecture — design unit 02 approved with modifications; user picked id, ports, palette C
- [x] Implementation 1-6 — unit 03 (units/03-implement-report.md), commits 57b43b8..7639641; build 0 errors, make_art --check passes; PLib grid laid out as designed, clicks set switches; deviations: 1024 x 1024 atlas (write_kanim packs square), no missing-wire icon on the ports, two-bit runs listed not ranged, output resent on Render200ms after a cut input wire, "All off" ButtonBlueStyle
- [ ] Screenshots to the user (common/tools/TestRunner/runs/om008-look2-20261019-162616/artifacts/); open: face lamps wash to pastel under bloom and cover their digits, overlay port icons cover lamps 2-3 and 18-19, build icon busy; stop for the look decision ← IN PROGRESS
- [ ] Implementation 7: TestRunner Kit additions (common/ first, then the bump)
- [ ] In-game tests
- [ ] Completion: review, retro, PR

## Open Questions / Blockers
- Art defects for 3gub: large digit under lamp 31 reads 32; small label 29 offset; spare 32nd cell has no ribbon mark; `ui` icon is a placeholder. The repo renames the kanim itself.

## Constraints
- Work only in the worktree C:/dev/ONI-Mods-OM-008.
- Tests at human speed; in-game tests via common/tools/TestRunner.
- No hedging language in player-visible text; build icon style rules apply to the build menu icon.
