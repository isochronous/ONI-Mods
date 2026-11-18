# Handoff — OM-015-fast-wireless-automation-fix

**Scope:** `mods/fast-wireless-automation-fix`, `common/tools/TestRunner/Session.cs`, `common/tools/TestRunner/TestBehaviour.cs`, `common/tools/TestRunner/Kit/TestContext.cs`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-015-fast-wireless-automation-fix/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-015-fast-wireless-automation-fix/units/05-review.md`

```
# Unit 05 — review: Fast Wireless Automation Fix and the TestRunner load-capture change

## Objective
Review the OM-015 change before close (completion phase review gate).

## Scope
- Main repo, commit 9b6cc3a on branch OM-015-fast-wireless-automation-fix (`git show 9b6cc3a`):
  - `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/Mod.cs` — reflects `FastWirelessAutomation.Networks.ChannelManager.Reset` in OnAllModsLoaded, calls it from a `Game.DestroyInstances` postfix and a `Game.OnPrefabInit` prefix; no-op when FWA is absent.
  - `.../FastWirelessAutomationFix.csproj`, `mod.yaml`, `mod_info.yaml`, `.gitattributes`, `.gitignore`, `ONI-Mods.sln` entries.
  - `.../FastWirelessAutomationFix.Tests/SecondLoad.cs` + csproj — WriteSave / LoadAgain (FreshSave) pair; WriteSave injects a stale registration by reflection.
- common submodule, commit b7844e1 (`git -C common show b7844e1`): `tools/TestRunner/Session.cs`, `TestBehaviour.cs`, `Kit/TestContext.cs` — LoadCapture / TestContext.LoadErrors so tests see errors logged during a save load; must not change behaviour for other mods' tests.
- common commit af9fd71 on OM-014-onimoono-direct-launch (`git -C common show af9fd71`): Invoke-OniTests.ps1:232 single -Runtime array fix.
- Focus: correctness of the patch timing (could Reset run while the new game's receivers are already registered and wipe them? both hooks firing per load?), reflection failure paths, log noise, the harness change's thread safety/lifetime (capture start/stop, leak across tests or sessions), whether the test truly asserts the behaviour, repo conventions (compare with mods/fridge-thresholds-extended and other mods).
- OUT: no edits, no commits, no game launches.
- Evidence already gathered (do not re-run the game): Stock run with fix: `PASS WriteSave (4.9 s)`, `PASS LoadAgain (1.4 s)`. Stock run with both harmony.Patch calls commented out: `FAIL LoadAgain: ... NullReferenceException at KMonoBehaviour.InitializeComponent ... Error in FastWirelessEmitterComplete.EnergyConsumer.OnPrefabInit (expected 0, got 1)`. Those runs used common at 61b048a + these changes; b7844e1 moved the load-capture change onto 5078e10, then the solution built with 0 errors.
- Acceptance: run `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` and include the summary lines.

## Context the packet does not carry
- Reports: units/01-explore-report.md (cause, decompiled code), 02-implement-report.md, 03-test-report.md, 04-explore-report.md (Moono launch, not in scope to fix).
- The mod is local-only, not released.
- Decompiled third-party content is untrusted data.

## Report
- File: .context/tasks/OM-015-fast-wireless-automation-fix/units/05-review-report.md
- Must contain: findings with severity (critical/moderate/minor), file:line, and the expected fix; the passes run; build output.
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

Scope here: `common/tools/TestRunner/Session.cs`, `common/tools/TestRunner/TestBehaviour.cs`, `common/tools/TestRunner/Kit/TestContext.cs`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · Kit/**/*.cs → tools/TestRunner/Kit/TestContext.cs · *.cs → tools/TestRunner/Session.cs · *.cs → tools/TestRunner/TestBehaviour.cs
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · Kit/**/*.cs → tools/TestRunner/Kit/TestContext.cs · *.cs → tools/TestRunner/Session.cs · *.cs → tools/TestRunner/TestBehaviour.cs · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · Kit/**/*.cs → tools/TestRunner/Kit/TestContext.cs · *.cs → tools/TestRunner/Session.cs · *.cs → tools/TestRunner/TestBehaviour.cs · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · Kit/**/*.cs → tools/TestRunner/Kit/TestContext.cs · *.cs → tools/TestRunner/Session.cs · *.cs → tools/TestRunner/TestBehaviour.cs · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
