# Handoff — OM-008-ribbon-monitor

**Scope:** `common/tools/TestRunner`, `mods/bitshifter/src/Bitshifter.Tests`
**Stores:** `C:/dev/ONI-Mods-OM-008` (the task's own store), `C:/dev/ONI-Mods-OM-008/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-008-ribbon-monitor/units/06-implement.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods-OM-008`

Scope here: `mods/bitshifter/src/Bitshifter.Tests`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods-OM-008`: an id here is this store's own, and the same id can name a different record in another.

mods/bitshifter:standards/code-style.md · reference
  C# code style for the Bitshifter mod: formatting, toolchain, imports, comments, Harmony patch a…
  mods/bitshifter/.context/standards/code-style.md · src/** → mods/bitshifter/src/Bitshifter.Tests · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/bitshifter:standards/error-handling.md · reference
  Error handling conventions for the Bitshifter mod: guarding Harmony patches and transpilers, tr…
  mods/bitshifter/.context/standards/error-handling.md · src/** → mods/bitshifter/src/Bitshifter.Tests · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/bitshifter:standards/naming-conventions.md · reference
  Naming conventions for the Bitshifter mod: files, namespaces, variables, methods, strings and i…
  mods/bitshifter/.context/standards/naming-conventions.md · src/** → mods/bitshifter/src/Bitshifter.Tests · linked from standards/code-style.md · linked from standards/error-handling.md
blk-4ed03be05da6 · reference
  Other mods
  .context/testing/integration-testing.md#test-environment/other-mods · names mods/bitshifter/src/Bitshifter.Tests/Compatibility.cs → mods/bitshifter/src/Bitshifter.Tests
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods-OM-008

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/bitshifter:tags.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods-OM-008/common`

Scope here: `common/tools/TestRunner`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods-OM-008/common`: an id here is this store's own, and the same id can name a different record in another.

blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj → tools/TestRunner · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods-OM-008/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

3 checkpoints, set by the coordinator for THIS work. There is no universal number: the bound is sized to the work.

Write each one directly — task scratch is exempt from the write path, so no change-set, no dispatch:

- `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/checkpoint-6-1.md`
- `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/checkpoint-6-2.md`
- `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/checkpoint-6-3.md`

This is where checkpoints go, whatever else a spec narrows for the lane's other writes.

A checkpoint says what is done, what is not, and what you would do next. Stop at the bound and report.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
