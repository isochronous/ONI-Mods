# Unit 06 — implement: rewrite the lost TestRunner kit methods

Lens: implement. Worktree `C:\dev\ONI-Mods-OM-008`. The kit lives in the `common` submodule (`common/tools/TestRunner`), which the coordinator has put on a new branch `OM-008-ribbon-panel` cut from common's current `origin/main` (357073f). Work in `common` on that branch.

## Why

Common commit 7beecd3 ("TestRunner: Anims symbol tint and visibility, Screens select/side screen/click, World.StatusItems"; files `Kit/Anims.cs`, `Kit/Screens.cs`, `Kit/TestContext.cs`, `Kit/World.cs`, `Reflect.cs`, `README.md`) and its follow-up 549ff6f were never pushed and are gone. The Bitshifter panel tests (`mods/bitshifter/src/Bitshifter.Tests/DiagnosticPanel.cs`, `DiagnosticPanelLook.cs`, `Rig.cs`, `Compatibility.cs`) were written against them and no longer compile.

## Sources for the API

1. The call sites in `mods/bitshifter/src/Bitshifter.Tests/` are the contract: `t.Anims.SymbolVisible`, `t.Anims.SymbolTint`, `t.Screens.Select / Deselect / SideScreen<T> / Click / Find`, `t.World.StatusItems`. Match their signatures and return types exactly; do not change the tests to fit a different API unless a call cannot be implemented as written (then say so in the report).
2. `units/02-design-report.md` (harness section) and `units/04-implement-report.md` describe what each method does and the pitfalls hit (e.g. the wire drag deselecting the panel).
3. The existing kit (`Kit/World.cs`, `Kit/Tools.cs`, `Kit/TestContext.cs`, `Reflect.cs`, README) for style and conventions; check whether common main already has any of these (e.g. `t.Screens.Find`, `t.Press`, `t.Screenshot` landed on main) and reuse rather than duplicate.
4. The game's own API for symbol tint/visibility (KBatchedAnimController), DetailsScreen selection and side screens, status items (KSelectable / StatusItemGroup). Decompiled game assemblies are referenced by the build; use them.

Tests run at human speed (memory: build/destroy frames apart broke the game) — Click/Select should go through the game's own screens the way a player would, not poke private state where a public path exists.

## Steps

1. Implement the methods in common (new `Kit/Anims.cs`, `Kit/Screens.cs` or extend existing files as main's layout suggests), wire them on `TestContext`, add needed `Reflect.cs` fields, document them in the TestRunner README in its existing style.
2. Build common's TestRunner and `mods/bitshifter/src/Bitshifter.Tests` (and `src/Bitshifter`) — 0 errors. Build the rest of the solution's test projects too if the TestRunner change could affect them (common main moved from 7687b3c to 357073f for this worktree; note anything that broke from that move).
3. Do NOT launch the game or run any in-game tests; the coordinator asks the user first.
4. Commits: in `common` on `OM-008-ribbon-panel`, terse one-line messages, ending with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Then in the worktree (ONI-Mods, branch OM-008-ribbon-monitor) commit the `common` pointer bump plus any test fixes: "OM-008: common with the rewritten panel kit". Push nothing.

## Report

`units/06-implement-report.md`: each method with its signature and how it works, files changed, build output, commit hashes in both repos, any test call you could not honour as written, and anything from the common main move that needed attention.
