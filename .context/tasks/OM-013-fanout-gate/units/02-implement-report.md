# Unit 02 report - implement: Fan-out Gate

Status: done. Everything builds and the 9 in-game tests pass. Nothing is committed in the main repo. `common/` has one commit, 4130779, on branch OM-013-fanout-gate, not pushed.

## Files changed

- `mods/bitshifter/src/Bitshifter/Toolkit/FanoutGateConfig.cs` (new): 2x1 building, id `FanoutGate`, kanim `fanout_gate_kanim`. Values copied from the Ribbon Writer, R360. 1-bit input at (0,0), ribbon output at (1,0).
- `mods/bitshifter/src/Bitshifter/Toolkit/FanoutGate.cs` (new): the component.
  - Saves a mask of bits 0-30. Output is the mask while the input is on, else 0. An unwired input counts as off.
  - Sends the output again on LogicValueChanged, on spawn, on a mask change, and from a Render200ms poll.
  - Copy settings copies the mask.
  - Plays `off`, `on_1_1111` or `on_0_0000`, only when the state changes.
  - Implements ILogicRibbonBitSelector: a click toggles a bit, bit 31 is ignored, both description containers are hidden, and the title key is `STRINGS.BITSHIFTER.FANOUTGATE.SIDE_SCREEN_TITLE`.
- `mods/bitshifter/src/Bitshifter/Toolkit/Toolkit.cs`: one row, `wires` after RibbonDiagnosticPanel, tech ParallelAutomation.
- `mods/bitshifter/src/Bitshifter/Toolkit/ToolkitPatches.cs`: two patches in the toolkit category.
  - A `RefreshToggles` postfix highlights every picked row when the target is the gate (state 0 picked, 1 plain).
  - A `SetTarget` postfix wraps the rows in the scroll view, for the gate only.
- `mods/bitshifter/src/Bitshifter/SideScreen.cs`: the existing patch's body moved into `internal static WrapRows(screen)` so the gate patch can reuse it. The existing patch is still uncategorized and behaves as before.
- `mods/bitshifter/src/Bitshifter/ModStrings.cs`: `BUILDINGS.PREFABS.FANOUTGATE` (NAME, DESC, EFFECT, 6 port strings) and `BITSHIFTER.FANOUTGATE.SIDE_SCREEN_TITLE` ("Bits to send").
- `mods/bitshifter/src/Bitshifter/translations/Bitshifter.pot` (regenerated with `--build`) and `ko.po`, `ru.po`, `zh.po`: 10 new entries each, added with the Edit tool. Line endings stay CRLF.
- Art:
  - 3GuB's three files moved unchanged (sha256 recorded) to `mods/bitshifter/anim-original/fanout_gate/logic_fanout_*`.
  - `mods/bitshifter/tools/make_art.py`: `build_panel` became a general `build_export(name, original, base)` that runs for the panel and the gate (`EXPORTS`). It copies the texture byte for byte and sets the frame bounds. It writes the build file back with the name `fanout_gate` only when the name differs, so the panel's output stays byte-identical.
  - Output written to `mods/bitshifter/src/Bitshifter/anim/assets/fanout_gate/fanout_gate_{0.png,anim.bytes,build.bytes}`. The png is identical to the original (`cmp` passes).
- `mods/bitshifter/src/Bitshifter.Tests/Rig.cs`: `GateRig` added after `PanelRig`. Layout: switch at 0, gate at 1-2, Reader at 3-4, alarm at 5. It has `WireInput` and `CutInput`.
- `mods/bitshifter/src/Bitshifter.Tests/FanoutGate.cs` (new): 9 tests.
- `common/tools/TestRunner/Kit/Anims.cs`: `Anims.Current(kbac)` returns the name of `GetCurrentAnim()`. Committed in common as 4130779 "TestKit: Anims.Current reads the anim a controller plays".
- `mods/bitshifter/CHANGELOG.md`: `## Unreleased` with one bullet.
- `mods/bitshifter/README.md`: art credit is now "Diagnostic panel and Fan-out Gate art by 3GuB."
- `mods/bitshifter/publish/workshop-description.txt`: one gate bullet in the list, and the same art credit.

## Command output

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`:
```
    291 Warning(s)
    0 Error(s)
```
The 291 warnings were already there, in other mods' test projects (CS0618 FindObjectsOfType and similar). Bitshifter's own projects build with none:
```
  Bitshifter -> C:\dev\ONI-Mods\mods\bitshifter\src\Bitshifter\bin\Release\Bitshifter.dll
  Bitshifter.Tests -> C:\dev\ONI-Mods\mods\bitshifter\src\Bitshifter.Tests\bin\Release\Bitshifter.Tests.dll
    0 Warning(s)
    0 Error(s)
```

`python tools/make_art.py --check`:
```
bitshifter_ribbon_reader: display centred at (-105.0, -100.1)
bitshifter_ribbon_writer: display centred at (119.2, -102.2)
diagnostic_panel: copied from anim-original\diagnostic_panel with its anim bounds set
fanout_gate: copied from anim-original\fanout_gate with its anim bounds set
the committed kanims match a fresh build
```
The full run (without --check) left the reader, writer and panel assets unchanged; git showed no modification to them.

`python common/tools/Kanim/kanim.py dump .../assets/fanout_gate` now shows real bounds instead of NaN:
```
off frame 0 of 1 (30 fps), bounds 1.3, -100.3  528.0 x 236.0
on_1_0000 frame 0 of 1 (30 fps), bounds 1.3, -100.3  528.0 x 236.0
```
`kanim.py list` gives the build name `fanout_gate`.

`python common/tools/Translations/translations.py mods/bitshifter`:
```
Wrote 35 strings to ...\translations\Bitshifter.pot
ko.po: 35/35 translated
ru.po: 35/35 translated
zh.po: 35/35 translated
ALL CHECKS PASSED
```

In game, `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods Bitshifter -Filter "Bitshifter:?Fan-out*"` (run-20261110-191054, final run):
```
PASS     Bitshifter.Tests/FanoutGate.InputOnAndOff (4.1 s)
PASS     Bitshifter.Tests/FanoutGate.ChangeWhileOn (4.1 s)
PASS     Bitshifter.Tests/FanoutGate.UnwiredInput (5.0 s)
PASS     Bitshifter.Tests/FanoutGate.SavedAndLoaded (5.0 s)
PASS     Bitshifter.Tests/FanoutGate.CopySettings (2.0 s)
PASS     Bitshifter.Tests/FanoutGate.BuildMenuAndResearch (0.0 s)
PASS     Bitshifter.Tests/FanoutGate.AnimFollowsInput (5.3 s)
PASS     Bitshifter.Tests/FanoutGate.SideScreenMultiSelect (6.3 s)
PASS     Bitshifter.Tests/FanoutGate.ReaderStillSingle (2.0 s)
Session done: 9 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; ... of 9; 49.9 s real time
Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies
```
Player.log for that run has no exceptions (grep count 0).

There were three launches, one after another, none looped:
1. The spec's filter `"Bitshifter: Fan-out*"` matched 0 tests. The space splits the argument on the Steam `-applaunch` command line: the log says "matching Bitshifter:".
2. With `?` in place of the space, 8 tests passed and 1 failed: AnimFollowsInput called `t.Rect` twice, and both rectangles start at the same corner, so the second gate overlapped the first. That was a bug in my test. It now uses one rectangle.
3. All 9 pass. Before this run I also changed the side screen test to pick rows 2, 3, 5 and 17, so the screenshot shows several highlighted rows without scrolling.

## What each test covers
- Input on sends the mask and input off sends 0, checked at the port and at a Reader plus alarm on bit 17 (InputOnAndOff).
- Changing the mask while the input is on changes the output (ChangeWhileOn).
- An unwired input sends 0. Wiring it sends the bits, and cutting the wire sends 0 again while the mask is kept (UnwiredInput).
- A save and load keeps the mask, the output, the wiring and the anim (SavedAndLoaded).
- Copy settings (CopySettings).
- Build menu: after the Diagnostic Panel in "wires", unlocked by ParallelAutomation, listed once (BuildMenuAndResearch).
- Anim: `off` with nothing wired, `on_0_0000` when wired with the input off, `on_1_1111` with the input on (AnimFollowsInput, with screenshot).
- Side screen: no highlight at first and no reader/writer descriptions. Clicking rows 2, 3, 5 and 17 highlights all four and the alarm fires. Clicking row 5 again clears it. `SetBitSelection(31)` is ignored (SideScreenMultiSelect, with screenshot).
- After showing the gate, a Reader's screen still has exactly one highlighted row, and a click moves it (ReaderStillSingle).

## Screenshots
- `.context/tasks/OM-013-fanout-gate/units/02-shots/Bitshifter.Tests.FanoutGate.AnimFollowsInput-input-on.png`: the gate wired with the input on, all lamps green. A lone unwired gate sits above it showing `off` and the missing-wire icon.
- `.context/tasks/OM-013-fanout-gate/units/02-shots/Bitshifter.Tests.FanoutGate.SideScreenMultiSelect-several-bits.png`: the gate with the input on, alarm lit, and its "Bits to send" screen with Bits 2, 3 and 5 highlighted and reading Green (bit 17 is below the scroll).

## Deviations from the spec and decisions taken
- **Test filter:** I used `"Bitshifter:?Fan-out*"` instead of `"Bitshifter: Fan-out*"` because of the argument split above. It selects the same 9 tests.
- **Output port missing-wire icon:** it also uses `show_wire_missing_icon: true`, as the vanilla Ribbon Writer's output does. The spec named only the input; a gate with no output wired does nothing. This is a one-argument change if unwanted.
- **When `off` plays:** only when neither port is wired. When either port is wired, the anim follows the input (`on_0_0000` / `on_1_1111`). The plan's decision says "nothing wired -> off", and unit 01 proposed exactly this rule.
- **Row text:** `IsBitActive` reports the output ribbon's bit, as the Writer does. So a picked row reads "Red" while the input is off and "Green" while it is on. The highlight is what marks a row as picked.
- **`SideScreenDescription`** returns the title text; the vanilla screen does not read it.
- **Test class name:** the test class is named `FanoutGate`, like `DiagnosticPanel`, so test ids read `Bitshifter.Tests/FanoutGate.*`. Because that name clashes with the component, `Rig.cs` and the tests refer to the component as `Bitshifter.FanoutGate`.
- **Translations:** I wrote the ko/ru/zh entries myself, following the existing panel entries' terms. Names: 팬아웃 게이트, Вентиль-разветвитель, 扇出门. No native speaker has checked them.
- **Not done:** `mods/bitshifter/.context/domains/entities.md` does not list the Toolkit folder or the gate. It was out of scope (`.context/` writes) and is for the manager.

## Pointer feedback (handoff section 2)
- **Applied:** `mods/bitshifter:standards/code-style.md` (tabs, Allman braces, using order, summaries). The TestRunner records were not opened; the Kit code itself was enough.
- **Missing from the brief:**
  - `mods/bitshifter/tools/make_art.py` and the `anim-original/` convention.
  - The precedent tests (`DiagnosticPanel.cs`, `Rig.cs`).
  - The TestRunner's filter/argument behaviour: its Session.cs doc says GLOB matches "Assembly/Class.Method" or the display name, but nothing records that a space in -Filter is split by `-applaunch`.
- No pointer's stated reason contradicted its record.

## Tooling friction
- **ICON:** none this unit. `ks read` was not needed past the code-style record.
- **Not ICON, for the record:**
  - `Invoke-OniTests.ps1 -Filter` with a space silently matches a prefix and runs 0 tests with exit 1. A guard or quoting fix in the runner would help.
  - A Bash heredoc turned `"\\n"` in a Python replacement string into real newlines inside ModStrings.cs. I caught it and fixed it with Edit; the memory note on heredoc backslashes applies.
