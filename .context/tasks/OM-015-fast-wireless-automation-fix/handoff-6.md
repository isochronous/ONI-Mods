# Handoff — OM-015-fast-wireless-automation-fix

**Scope:** `mods/fast-wireless-automation-fix`, `common/tools/TestRunner`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-015-fast-wireless-automation-fix/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-015-fast-wireless-automation-fix/units/06-implement.md`

```
# Unit 06 — implement: review fixes

## Objective
Fix the review findings in units/05-review-report.md (findings 1, 2, 3, 4, 5, 6; 7 at your discretion; 8 is no action).

## Scope
- Main repo (branch OM-015-fast-wireless-automation-fix):
  - Finding 1: `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix.Tests/SecondLoad.cs` — the test must not pass vacuously: the reflection lookups the injection needs are asserted in a test body, and `LoadAgain` fails unless the stale registration was actually saved/left (a marker the cleanup sets, as the reviewer suggests, or a better way).
  - Finding 2: `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/Mod.cs` — drop the `Game.DestroyInstances` postfix; keep only the `Game.OnPrefabInit` prefix. Update the class doc comment and log line to match. (Jeremy approved this.)
- common submodule, currently on its branch OM-015-fast-wireless-automation-fix at b7844e1:
  - Finding 3: end the load capture in `Session.Begin` and `Session.Finish` (Session.cs ~462) so errors don't carry into the next run in a reused game.
  - Finding 4: the `LoadErrors` doc comment says what it covers.
  - Finding 5: `SetLoadErrors` copies under the lock.
  - Finding 6: add `LoadErrors` to the TestRunner README. `entities.md:114` is a `.context/` record — do NOT edit it; put the exact sentence it should gain in your report and the manager lands it.
- OUT: no game launches, no commits, no edits under `.context/` other than your report.
- Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — include the summary lines; then build `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix` with the normal deploy so the local mods folder has the one-patch build, and show its output.

## Report
- File: .context/tasks/OM-015-fast-wireless-automation-fix/units/06-implement-report.md
- Must contain: per-finding change with file:line, build outputs, the proposed entities.md sentence.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/fast-wireless-automation-fix`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

domains/game-updates.md · reference
  How ONI-Mods gets ready for an Oxygen Not Included update: where the new and previous game buil…
  .context/domains/game-updates.md · mods/** → mods/fast-wireless-automation-fix
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 2 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:domains/game-interfaces.md, mods:overview.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/tools/TestRunner`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

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
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
