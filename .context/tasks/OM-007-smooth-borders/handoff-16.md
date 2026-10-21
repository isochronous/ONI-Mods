# Handoff — OM-007-smooth-borders

**Scope:** `common/tools/TestRunner`
**Store:** `C:/dev/ONI-Mods/common` — the store the brief below was computed from.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-007-smooth-borders/` — tracked, write-path exempt, never authoritative ([IF-1.C]). ⚠ It is in the TASK's own store, which is not the store above: this scope reached across checkouts. The coordinator chose it with `--store`, or by the checkout it was standing in.

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-007-smooth-borders/units/13-implement.md`

```
# Unit 13 — implement: tests delete every save file they create

## Objective
The user: "Make sure these tests are deleting the save files they create." Today's worldgen test runs left 61 colony-summary folders in `Documents\Klei\OxygenNotIncluded\RetiredColonies\<colony name>\` (each a `<colony>.json` and `00000\<dupe>_cycle_0000.png`). No test `.sav` was found under `Documents\Klei\OxygenNotIncluded` (only the user's own "The Aftermath" saves), but confirm that for every path the harness uses.

## What to do
1. **Find everything a test run writes into the player's game folders**: saves (`save_files`, `cloud_save_files`, autosaves), colony summaries (`RetiredColonies`), timelapse images, worldgen caches, anything else — for both worldgen tests (`[OniTest(Worldgen = ...)]`, which embark a new colony) and fixture tests. Read the harness (common/tools/TestRunner) and the game code that writes them (decompiled code path in units/07-implement-report.md).
2. **Make the harness delete them**, in the harness (not in each test): record exactly what each run created (by name/path from the run itself, not by date or pattern guessing), delete it when the world is done or at the end of the run, and have the warden or the next launcher delete leftovers from a crashed run, the same way the config backup is recovered (unit 05). Never delete anything the run did not create; never touch the user's own colonies or saves. If the game must not write some of these at all (e.g. a setting or patch that skips colony summaries during tests), that is acceptable if simpler and as reliable — say which you chose and why.
3. **Leftovers from today:** units/11-test-report.md §7 lists each of its runs with the colony names its worlds got (it notes some names repeat or could match the player's own colonies — so match by more than the name: creation time inside a run's window, the run's Player.log, the summary's cluster/seed). Earlier runs today (units 05, 08, 10) need the same from their run folders. Build the list of RetiredColonies folders that test runs created, from evidence (run logs, Player.log, run artifacts with colony names), and put it in your report with the evidence per folder. **Do not delete them** — the manager shows the list to the user first.
4. Prove it: run one short worldgen test (e.g. `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -Filter '*AtDefaults*'` or the smallest worldgen test) and show the before/after listing of RetiredColonies and save folders: nothing new remains.

## Scope
- IN: common/tools/TestRunner/** (branch OM-007-smooth-borders inside common/; commit there; the manager bumps the pointer), its README and the facts for the store records it changes (in the report).
- OUT: mod code; deleting any existing folder.
- Acceptance: the proof run's summary line verbatim with run id, before/after listings, the leftovers list with evidence. The game must be free (check; never close a game you did not start); tests at human speed. Commit inside common/ with one terse subject line, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

## Report
- File: .context/tasks/OM-007-smooth-borders/units/13-implement-report.md (do not commit it)
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

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
