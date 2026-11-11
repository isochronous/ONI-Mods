# Unit 03 report - review: Fan-out Gate (994e525 + common 4130779)

## Verdict

**Approve with fixes.** Nothing is blocking. One should-fix: the claim that Reader/Writer behaviour is unchanged while Bitshifter yields to Digital CPU is false once a gate's screen has been shown. The rest are nits.

I read every changed source file in full: FanoutGate.cs, FanoutGateConfig.cs, Toolkit.cs, ToolkitPatches.cs, SideScreen.cs, ModStrings.cs, Rig.cs, FanoutGate tests, make_art.py, Anims.cs, the po/pot entries, CHANGELOG, README and the Workshop text. I also decompiled the vanilla `LogicBitSelectorSideScreen` (ilspycmd, current install) to check the patches against it.

## Findings

### Should-fix

1. **Under yield, the gate's wrap stays on the shared screen, so it also changes the Reader's and Writer's screens** (Pass 3, integration).
   - Where: `mods/bitshifter/src/Bitshifter/Toolkit/ToolkitPatches.cs:79-95` and its doc at `:82-84`; `SideScreen.cs:15-17`; plan.md Decisions line 22 ("so Reader/Writer behaviour under yield is unchanged").
   - Why: `DetailsScreen` has one `LogicBitSelectorSideScreen` instance for the gate, the Reader and the Writer. `WrapRows` moves the instance's row container into a `BitshifterScroll` KScrollRect. It is never undone, and `IsWrapped` keeps it wrapped from then on.
   - Scenario: Digital CPU is enabled, so Bitshifter yields and the uncategorized wrap patch is not applied. The player selects a Fan-out Gate, then a Digital CPU 32-bit Ribbon Reader. The Reader's 32 rows now sit in Bitshifter's scroll view, 6 rows high, with `FitUnderHeader` width clamping. Before this change that list was vanilla.
   - Possible harm: the list does scroll, but Bitshifter is now restyling a screen it says it leaves to Digital CPU. If Digital CPU (or a future version) lays out that screen itself, the two collide. The doc comment at ToolkitPatches.cs:82-84 states the opposite of what happens.
   - Fix: pick one.
     - (a) Accept it: move `LogicBitSelectorSideScreen_SetTarget_Patch` into `Toolkit.PatchCategory`, so the behaviour is at least consistent, and correct the decision and the two doc comments.
     - (b) Keep the claim: unwrap the screen, or hide the scroll view, in a toolkit-category SetTarget postfix when the target is not a gate and the mod has yielded.
   - The explore report (01, section 3, point 2) flagged this as a decision to make. The plan's decision went with "gate only", but the code cannot deliver that.
   - Not testable here: Digital CPU is not installed (`workshop/content/457140/3244925649` is absent), and no installed Workshop dll references `LogicBitSelectorSideScreen`.

### Nits

2. **Magic event hashes** (Pass 4). `FanoutGate.cs:93` `Subscribe(-905833192, ...)` and `:99` `Subscribe(-801688580, ...)`. The tests use `(int)GameHashes.CopySettings`. `(int)GameHashes.CopySettings` and `(int)GameHashes.LogicEvent` read better. The panel uses the same literals, so this is precedent, not drift.

3. **Two names for the 31-bit mask** (Pass 4). `FanoutGate.cs:29` `Mask = int.MaxValue` and `Patches.cs:181` `Mask = (int)((1L << BitshifterMod.Bits) - 1)` are the same value written two ways. If `Bits` ever changes, only one of them follows. `FanoutGate.Mask` could be defined from `BitshifterMod.Bits` as the Patches one is.

4. **Untested anim branch** (Pass 5). `Anim()` at `FanoutGate.cs:83-88` returns `on_0_0000` when only the output is wired. That is the implementer's documented deviation, and no test covers it: AnimFollowsInput only covers nothing wired and both wired. Fixing it would take one more assert in AnimFollowsInput, by building the rig with `wireInput: false`.

5. **Side screen title not asserted** (Pass 5). Nothing checks that `SideScreenTitle` ("STRINGS.BITSHIFTER.FANOUTGATE.SIDE_SCREEN_TITLE") resolves. `Translations.cs:31` uses the `STRINGS.` root, so the key is right, and the unit 02 screenshot shows "Bits to send". A typo in that string would still pass every test. One `Strings.Get(...)` assert in SideScreenMultiSelect would close it.

6. **Name clash forces qualified names** (Pass 4). The test class `Bitshifter.Tests.FanoutGate` shadows the component, so Rig.cs and the tests write `Bitshifter.FanoutGate` throughout. The panel precedent avoids this: test class `DiagnosticPanel`, component `RibbonDiagnosticPanel`. This is cosmetic; the test ids read well as they are.

7. **EFFECT text skips the unwired case** (`ModStrings.cs` FANOUTGATE.EFFECT). It says a Red input gives a Red ribbon; an unwired input gives the same. That is fine for players, and I mention it only for completeness.

8. **Store record behind the code**. `mods/bitshifter/.context/domains/entities.md` (Key Code Paths, blk-2fe104dbf053) still lists neither the Toolkit folder nor the gate. Unit 02 already handed this to the manager.

9. **`debug.log` is untracked at the repo root.** Keep it out of the PR.

### Checked and fine

- **Signal logic**
  - Output is `InputOn ? mask : 0`, and `InputOn` is false when the input is unwired.
  - The output port's own LogicValueChanged is filtered out.
  - `SetBits` masks off the sign bit.
  - `TogglePicked`/`IsPicked`/`IsBitActive` bound bits to 0..30.
  - The Render200ms poll resends when the port value differs from the computed output, so a cut wire is caught. UnwiredInput exercises exactly that path, since cutting a wire raises no event.
- **Save/load and copy settings**
  - `[Serialize] int mask` with OptIn.
  - On spawn the gate resends and redraws. The poll fixes any value read before the network settles.
  - Copy settings goes through the CopySettings hash and a GameObject check.
- **Bit 32 guard, checked against vanilla**
  - `RefreshToggles` re-binds `onClick` only for `i < GetBitDepth()`.
  - A 32nd row left over from a 32-bit Reader still calls `SelectToggle(31)` on the current target. The gate ignores bit 31, and the postfix paints the row plain.
- **RefreshToggles postfix**
  - It acts only when `___target is FanoutGate` and runs after the vanilla single-highlight loop, so other targets are untouched. ReaderStillSingle covers this.
  - `toggles_by_int` is public.
- **SideScreen.cs refactor**
  - The body moved into `WrapRows` unchanged.
  - The uncategorized patch still runs for every target when the mod does not yield.
  - Double-application for the gate is idempotent through `IsWrapped`.
- **Anim states** match the plan: `off` when nothing is wired, `on_1_1111` / `on_0_0000` otherwise, played only on a change.
- **Config**: 2x1, input (0,0), ribbon output (1,0), R360, the Ribbon Writer's values, toolkit row after the panel with ParallelAutomation.
- **make_art.py**
  - The panel's build file is rewritten only when the name differs.
  - The diff touches no diagnostic_panel asset, and `--check` passes. Panel output is unchanged.
- **TestKit `Anims.Current`**: small, documented, fails loudly with no controller.
- **Tests**
  - One per behaviour, asserting port values, alarm state, the mask, anim names and row MultiToggle states, not calls.
  - The layers match the DiagnosticPanel precedent: CopySettings and BuildMenu at Layer 0.
- **Translations**: 10 entries in each po, all style tags and links kept, checker passes.
- **CHANGELOG, README, Workshop text**
  - Short, player-facing, no hedging.
  - `## Unreleased` is the form release.py expects (`common/tools/Release/release.py:36, 248`).
  - The credit line is updated in both places.
- **Security pass**: no input handling, secrets or I/O in the change. Nothing to report.

## Command output

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` (tail):
```
    291 Warning(s)
    0 Error(s)

Time Elapsed 00:00:06.86
  Bitshifter -> C:\dev\ONI-Mods\mods\bitshifter\src\Bitshifter\bin\Release\Bitshifter.dll
  Bitshifter.Tests -> C:\dev\ONI-Mods\mods\bitshifter\src\Bitshifter.Tests\bin\Release\Bitshifter.Tests.dll
```
The warnings all come from other mods' test projects (CS0618/CS0649). A grep for bitshifter warnings found none.

`python tools/make_art.py --check` (run from mods/bitshifter):
```
bitshifter_ribbon_reader: display centred at (-105.0, -100.1)
bitshifter_ribbon_writer: display centred at (119.2, -102.2)
diagnostic_panel: copied from anim-original\diagnostic_panel with its anim bounds set
fanout_gate: copied from anim-original\fanout_gate with its anim bounds set
the committed kanims match a fresh build
```

`python common/tools/Translations/translations.py mods/bitshifter`:
```
Wrote 35 strings to ...\translations\Bitshifter.pot
ko.po: 35/35 translated
ru.po: 35/35 translated
zh.po: 35/35 translated
ALL CHECKS PASSED
```
Afterwards, `git status` showed no modified tracked files. The game was not launched.

## Pointer feedback (handoff section 2)

- **Did not apply.** All 40 pointers shown were other mods' code-style and error-handling blocks. They matched only because they cite `Bitshifter/Mod.cs` as an example, and none of them bore on this review. This is the same noise unit 01 reported.
- **Missing.** What actually governed the review never appeared:
  - the task's plan.md decisions;
  - the unit 01 and unit 02 reports;
  - the writing-rules records (changelog and Workshop style);
  - the vanilla `LogicBitSelectorSideScreen` source.
- **Pointer reasons.** None contradicted its record. I opened none of them past the one-line reasons.

## Tooling friction

- **ICON:** none this unit (`ks read` was not needed).
- **Not ICON:** ilspycmd still prints its "not the latest version" banner on stdout.
