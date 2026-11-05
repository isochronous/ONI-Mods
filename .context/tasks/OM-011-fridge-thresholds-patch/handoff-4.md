# Handoff — OM-011-fridge-thresholds-patch

**Scope:** `mods/fridge-thresholds-extended`, `common/tools/TestRunner`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-011-fridge-thresholds-patch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-011-fridge-thresholds-patch/units/04-test.md`

```
Task: OM-011 — in-game tests for Fridge Thresholds Extended
Lens: test. Branch OM-011-fridge-thresholds-patch, C:\dev\ONI-Mods. Do not commit. Jeremy has approved ONE in-game run of this suite (2026-11-04); ask before any second run beyond one fix-and-rerun of your own test code.

Build:
1. Add mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests following the TestRunner kit pattern used by other mods (see common/tools/TestRunner/README.md and an existing *.Tests project, e.g. the one added in commit 88214fb). Add it to ONI-Mods.sln like the others.
2. Representative tests only (one per behaviour/code path):
   a. Freezer: build a Freezer (Psyko.Freezer building), set the thresholds component to High 50 / Low 20, put food in to ~60% of capacity -> FULL port is green (1); drop to ~10% -> red (0); back to ~35% -> still matches hysteresis state. Compare with a vanilla Refrigerator under the same settings as a control.
   b. One IR fridge (e.g. SimpleFridge): the thresholds component exists on the built building and the same High/Low behaviour drives its port.
   c. Copy settings Freezer -> Freezer carries SendGreenOnLow / ActivateValue / DeactivateValue.
   d. First signal after load for the Freezer (review finding M1): if the harness supports save/reload (see the harness README), cover it; otherwise note it as untested.
   Reach other mods' types by reflection only in the test project too.
   Pace steps in game time (memory: tests at human speed; never build/destroy frames apart).
3. Run: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850` (check the exact -Steam parameter syntax in the script's help). Never pass -KeepGame. Never stop the runner mid-run. If a test fails for an unclear reason, stop, take screenshots of setup and failure as the harness allows, and report rather than retrying blindly.
4. If a harness change is needed, make it in common/ and report it (do not commit inside the submodule).

Report: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md — files created, the exact command, the runner's pass/fail output copied verbatim, relevant Player.log lines (patch-applied log lines from FridgeThresholdsExtended, any exceptions), and what remains unverified.
Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/fridge-thresholds-extended`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

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
