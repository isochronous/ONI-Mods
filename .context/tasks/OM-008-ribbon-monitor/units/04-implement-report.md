# Unit 04 report: TestRunner kit additions and the Ribbon Diagnostic Panel test suite

Steps 7 and 8 are done. The panel suite is green in game (13 of 13). The full Bitshifter run was 29 of 30 before one test fix; the fixed test then passed in the narrower rerun. The Digital CPU stand-down test was not run: the mod is not installed.

## Commits

common (submodule, branch `OM-008-ribbon-panel`, from 7687b3c, not pushed):

| Hash | Message | Files |
|---|---|---|
| 7beecd3 | TestRunner: Anims symbol tint and visibility, Screens select/side screen/click, World.StatusItems | `Kit/Anims.cs`, `Kit/Screens.cs`, `Kit/TestContext.cs`, `Kit/World.cs`, `Reflect.cs`, `README.md` |

ONI-Mods (branch OM-008-ribbon-monitor, not pushed):

| Hash | Message | Files |
|---|---|---|
| 2c14327 | OM-008: brighter, less yellow lamp green | `Toolkit/RibbonDiagnosticPanel.cs` (LampGreen = 40/255, 1, 80/255, the coordinator's request) |
| 8f0c9f0 | OM-008: panel test suite, PanelRig, stand-down test; common bumped to the screens kit | `Bitshifter.Tests/DiagnosticPanel.cs`, `Rig.cs`, `Compatibility.cs`, `DiagnosticPanelLook.cs`, `common` pointer |
| 38c3461 | OM-008: unit 04 code done, suite run waiting on the user's game | coordinator's commit |
| 8984e5e | OM-008: grid test selects the panel again after dragging the output ribbon | `Bitshifter.Tests/DiagnosticPanel.cs` |

## The kit's new public surface

- `Anims` (static, `Isochronous.TestKit`):
  - `Color SymbolTint(KBatchedAnimController, KAnimHashedString)`: reads the colour through `Reflect.SymbolInstanceGpuDataConverter`. Fails the test naming the symbol if the build has none.
  - `bool SymbolVisible(KBatchedAnimController, KAnimHashedString)`.
- `TestContext.Screens` (class `Screens`):
  - `IEnumerator Select(GameObject)`: selects, waits until the details screen shows it, then 3 frames.
  - `void Deselect()`.
  - `T SideScreen<T>() where T : SideScreenContent`.
  - `GameObject Find(GameObject root, string name)`.
  - `void Click(GameObject)`: pointer down, up, click.
- `World.StatusItems(GameObject)`: the resolved status item names.
- `Reflect.SymbolInstanceGpuDataConverter` and `Reflect.DetailsScreenSideScreens`, both in `SelfCheck`.
- The README documents all of it, and its intro now names layer 2.

## Tests (all in `Bitshifter.Tests/DiagnosticPanel.cs` unless noted)

Final run om008-panel-20261019-201748, filter `*DiagnosticPanel.*`:

```
Session done: 13 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 13 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 13; 98.1 s real time
```

| Test | Result |
|---|---|
| LampsFollowInput (Writer-fed; all red, bit green, all red) | PASS |
| FullValue (31-bit value; status item "Input: 1, 3, 9-12, 31") | PASS |
| SwitchesReachOutput (Reader on bit 17; output-only shows all red) | PASS |
| PurpleWins | PASS |
| InputOnlyRed (input wired, value 0, output unwired: every lamp red) | PASS |
| NeitherWired (all hidden, switches or not) | PASS |
| OutputUnwired (wire, unwire, status item gone, switches kept) | PASS |
| SavedAndLoaded | PASS |
| CopySettings | PASS |
| BuildMenuAndResearch | PASS |
| SwitchGrid (rows 8, 8, 8, 7; click on and off; cell colour) | PASS |
| GridDisabled | PASS |
| AllOff | PASS |
| Compatibility.PanelStaysWhenStoodDown (Explicit, `-Steam 3244925649`) | NOT RUN, Digital CPU not installed (its Workshop folder `F:/SteamLibrary/steamapps/workshop/content/457140/3244925649` is empty) |

The earlier full run, om008-suite-20261019-201324:

```
Session done: 29 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped ... of 30; 185.1 s real time
```

The one failure was GridDisabled ("wiring the output enables the cells (expected true)"). That run included the existing Bitshifter tests; they all passed. The GridDisabled fix is below. I did not rerun the whole Bitshifter set after the fix. The 13-test rerun covers the changed test.

DiagnosticPanelLook (Explicit), run om008-look-20261019-201956: `Session done: 1 passed ... of 1; 39.3 s real time`.

## Face screenshots (LampGreen 40, 255, 80)

Folder: `C:/dev/ONI-Mods-OM-008/common/tools/TestRunner/runs/om008-look-20261019-201956/artifacts/`
- Face lit: `C:/dev/ONI-Mods-OM-008/common/tools/TestRunner/runs/om008-look-20261019-201956/artifacts/Bitshifter.Tests.DiagnosticPanelLook.Screenshots-3-face-lit.png`
- Grid wired: `.../Bitshifter.Tests.DiagnosticPanelLook.Screenshots-1-grid-wired.png`
- Grid unwired: `.../Bitshifter.Tests.DiagnosticPanelLook.Screenshots-2-grid-unwired.png`
- Hover: `.../Bitshifter.Tests.DiagnosticPanelLook.Screenshots-4a-hover.png`
- Hover in the automation overlay: `.../Bitshifter.Tests.DiagnosticPanelLook.Screenshots-4b-hover-automation-overlay.png`
- Build menu: `.../Bitshifter.Tests.DiagnosticPanelLook.Screenshots-5-build-menu.png`

I did not look at the pictures myself.

## SelfCheck

`Reflect.SelfCheck()` throws "game members not found" at the start of `TestBehaviour.Setup`. It logs nothing when it passes. In the Look run's Player.log, Setup carried on to the next line:
`[00:20:36.768] [1] [INFO] [TestRunner] fixture area 40x10 at (100, 185), focus True, instant build True, ...`
That means both new members resolved in game.

## Mod-code fixes

None. The GridDisabled failure was a test fault. A utility build tool activation clears the selection, so the open side screen was closed and its target was null when the ribbon was dragged. The test now selects the panel again after `WireOutput`, and it passes. The side screen's own refresh on a wiring change (Render200ms) is covered through OutputUnwired and the screenshot test, not through an open-screen test, because no player can wire while selecting.

## Deviations from the design

- `Screens.Select` returns `IEnumerator`, not `Wait` (it needs a wait and then 3 frames). I added `Screens.Find`.
- Tests use the red Inactive state, plus the three tests the spec adds (InputOnlyRed, output-only red inside SwitchesReachOutput, NeitherWired).
- OutputUnwired asserts `OutputValue` before wiring, right after the wire and after 1 s. It cannot assert on the first frame that the downstream Reader never sees input only.
- Test 1 uses a vanilla switch and Writer (`Feed.Writer`) and the others a second panel (`Feed.Panel`) as the source.
- The stand-down test uses a Digital CPU Writer on bit 32 (index 31) to make a 32-bit input. It is untested.
- The kit doc's layer list: the kit's `LayerAttribute` doc already names layer 2, so I only changed the README. I wrote nothing under `.context/`.

## Collision with the other checkout

The other checkout's unpushed common commits (8a320ef and earlier) also change `tools/TestRunner/README.md`, `Reflect.cs`, `Kit/World.cs` and `Kit/Attributes.cs`. My commit touches README.md, Reflect.cs, World.cs and TestContext.cs. Expect merge conflicts there.

## Durable facts for the store

- Build tools (utility drag) clear the selection, so a test that watches a side screen across a drag must select again.
- `SymbolInstanceGpuData` has no colour getter, so tint comes from the private `symbolInstancesConverter` field, now in Reflect.
- `Reflect.SelfCheck` logs nothing on success. The line to look for is "fixture area ..." in Player.log.
- The harness reuses the running game for 15 s, so back-to-back narrow filters ran in about 40 s.

## ICON tooling friction

None from ICON's engine, skills or hooks. A background `&` in a Bash command makes the tool report "completed" at once, so the task notification does not mean the run ended. I used run_in_background without `&` afterwards. The background wait loop was killed once by the system for low memory.
