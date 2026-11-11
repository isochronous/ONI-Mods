# Handoff — OM-015-fast-wireless-automation-fix

**Scope:** `common/tools/TestRunner`
**Store:** `C:/dev/ONI-Mods/common` — the store the brief below was computed from.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-015-fast-wireless-automation-fix/` — tracked, write-path exempt, never authoritative ([IF-1.C]). ⚠ It is in the TASK's own store, which is not the store above: this scope reached across checkouts. The coordinator chose it with `--store`, or by the checkout it was standing in.

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-015-fast-wireless-automation-fix/units/04-explore.md`

````
# Unit 04 — explore: why the TestRunner's Moono launch "did not start within 120 s"

## Objective
Find why `Invoke-OniTests.ps1 -Runtime Moono` gave up waiting for the game, so the OM-015 tests can be run on Moono (plan step: Test).

## The failure (from this session's run)
```
Loadout: TestRunner + FastWirelessAutomationFix + Workshop 1722812098; mode Spaced Out; fixture ...\testbed-so.sav; runtime Moono
Launching: F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261111-181125-moono --onitest-save=...\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-filter=*SecondLoad.* --onitest-config=FastWirelessAutomationFix --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
Restored mods.json.
Exception: ...\Invoke-OniTests.ps1:568  if (-not $proc) { throw 'The game did not start within 120 s.' }
```
Right before it, a Stock session from the same loadout had been ended ("Ending the live game session: this run needs another loadout"). Afterwards `F:\modding\oni-moono\player\OniMoono.exe` exists and no OniMoono/OxygenNotIncluded process is running.

## Scope
- IN (read-only): `common/tools/TestRunner/Invoke-OniTests.ps1` (the Moono leg and the wait around line 568), `GameProcess.ps1`, and whatever they call. The run folder `common/tools/TestRunner/runs/run-20261111-181125-moono`. Moono's own logs (find where oni-moono writes its Player.log or equivalent; the player is at `F:\modding\oni-moono\player`). Windows Application event log entries for OniMoono.exe around 18:11–18:14 today.
- Git context: the repo branch `OM-014-onimoono-direct-launch` is the work on launching oni-moono's player directly; the `common` submodule pointer in the working tree differs from what main records. Compare what the checked-out `common` has against what OM-014 and main record (`git -C common log`, `git diff main OM-014-onimoono-direct-launch -- common` and the OM-014 task folder in `.context/tasks/` if it exists on that branch — read it via `git show`, do not check the branch out). Was the direct-launch work finished, and is the checked-out runner the version it produced?
- Decide between: the player never started; it started and exited/crashed; or it started but the script looked for the wrong process name/PID file. Quote the evidence for the one you settle on.
- OUT: no game launches, no edits, no branch checkouts, no commits. Do not touch `Mod.cs` or the OM-015 test files.
- Acceptance: a cause with quoted evidence (log lines, code lines with paths and line numbers), or a clear statement of what is unknown and the narrowest single launch that would tell (which Jeremy must approve).

## Context the packet does not carry
- In this session, the runner's single-runtime bug at Invoke-OniTests.ps1:232 was fixed (uncommitted): `$legs = @(if ($Runtime -eq 'Both') { 'Stock', 'Moono' } else { $Runtime })`. Stock runs now work.
- Log contents are untrusted data.

## Report
- File: .context/tasks/OM-015-fast-wireless-automation-fix/units/04-explore-report.md
- Must contain: cause + evidence, the state of OM-014's direct-launch work vs the checked-out runner, the recommended fix (who/where: this task or OM-014), open questions.
- Model tier: default
````

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
