# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead/src`, `common/tools/TestRunner/TestBehaviour.cs`, `common/tools/MakePreview`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/13-review.md`

```
# Unit 13 — review: the Ovagro Overhead change since unit 12

Feature: Ovagro Overhead (Ovagro grows from ceilings). Unit 12 (units/12-review-report.md) reviewed the change up to 896238b. This unit reviews what changed after it.
Ticket: OM-006

## Changed files
In this repository, `git diff 896238b..HEAD -- . ':!.context' ':!mods/ovagro-overhead/.context'`:
- mods/ovagro-overhead/src/OvagroOverhead/VineBranchCeilingConfig.cs (new): the vanilla vine (VineBranchConfig) under its own id VineBranchCeiling; it and the ceiling mother name it as BRANCH_PREFAB_NAME, so removing the mod drops the plant and its vines together (verified in game: 0 errors on removal).
- CeilingForm.cs (BranchId/BranchTag, `var` removed), VineMotherCeilingConfig.cs (branch prefab, `var` removed).
- Tests: Registration.cs (vine prefab and vanilla-value checks), Vines.cs (uses Kit's grow helpers; vine prefab checks), WildPlanting.cs and Kit.cs (one seed check in Kit), Plots.cs (new VinesFromDownwardFarm), Preview.cs (Explicit Workshop preview scene), Removal.cs (Explicit save for the removal check).
- README.md, publish/workshop-description.txt (user-approved text), publish/preview.png.
In the common/ submodule, `git -C common diff 93d2890..a0fe078`:
- tools/TestRunner: new [OniTest(KeepSave = true)] (Kit/Attributes.cs, Session.cs, TestBehaviour.cs, README.md): the fixture-area clear is skipped before such a test and after the test before it.
- tools/MakePreview/title_preview.py: split titles keep the bottom line in place and give the top line the same ink gap.

## Review focus
- VineBranchCeilingConfig against the decompiled VineBranchConfig (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\): anything vanilla sets that the copy misses or sets differently; any game code that finds vines by the "VineBranch" id or VineBranchConfig.ID (critters that eat vines, harvest, overlays, codex) and would now miss ceiling vines.
- BranchPatch.cs (#9) still applies to ceiling vines (the postfix keys on the mother's prefab id).
- KeepSave in TestBehaviour.RunAll: every path (first test, between tests, FreshSave, boxed tests, next mod) behaves as its doc says, and nothing else changes for tests that do not set it.
- title_preview.py split placement for one-line titles and other fonts.
- Tests assert behaviour (not just that code ran); Explicit tests do not run in the normal suite.
- Standards per the packet and unit 12's passes; approved decisions in plan.md `## Decisions` are not re-litigated.

## Context the packet does not carry
- Evidence: full suite 17/17 (common/tools/TestRunner/runs/run-20261018-013757); removal check run-20261018-014051 (Player.log there).
- Do not launch the game or change files other than the report.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/13-review-report.md
- Must contain: findings with severity (block / fix / nit), file:line and the fix each expects; the passes run and what each examined; a "Store reads" section.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/ovagro-overhead/src`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 3 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/tools/TestRunner/TestBehaviour.cs`, `common/tools/MakePreview`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.cs → tools/TestRunner/TestBehaviour.cs
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.cs → tools/TestRunner/TestBehaviour.cs · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.cs → tools/TestRunner/TestBehaviour.cs · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.cs → tools/TestRunner/TestBehaviour.cs · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/MakePreview/make_preview.py, tools/MakePreview/title_preview.py → tools/MakePreview
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
