# Handoff — OM-007-smooth-borders

**Scope:** `common/tools/TestRunner`, `common/tools/WorldgenDriver`, `mods/smooth-borders/src/SmoothBorders.Tests`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-007-smooth-borders/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-007-smooth-borders/units/16-implement.md`

```
# Unit 16 — implement: fixes from the S7 review (code and build only, NO game launch)

## Objective
Resolve the review findings in units/15-review-report.md (read it; finding ids below are its ids).

- **M1** Invoke-OniTests.ps1:181-192: leftover config/mode/save-pointer restore runs before the "game already running" check and only knows the live pid file. Restore leftovers only when no OxygenNotIncluded process exists at all (as Invoke-Worldgen.ps1:68 does); start the warden before the game, so a launcher killed during the launch window still has a warden.
- **m1** a kept mode or save-pointer file that survives a restart must not overwrite the player's newer value: restore only when the current value is still the test's value (record what the run set, or compare against it).
- **M2** SmoothBorders.Tests/Support.cs:361: drop the blanket `!InSpace(cell)` excuse; rely on the existing `EdgePointSkipsVacuum` check, which follows the game's rule. If you find a case the game itself leaves unwalled that only the InSpace excuse covered, say so with the decompiled code; do not guess.
- **m2** add one NoColony test that runs the transpiler on altered IL (e.g. the GetIntSetting read for the thickness key removed or duplicated) and checks `Border.Patched` comes back false and the code is returned unchanged.
- **m3** cut redundant Explicit tests per the user's rule (2026-10-19): "comprehensive tests don't necessarily mean 'every single one', it means 'a representative sample'". Cut what the review lists (EmptyEmpteraCamera, the redundant Clusters tests, the extra Omnificia tests, duplicate "same world twice" controls) unless a test is the only cover for a distinct behaviour or code path; keep one case per asteroid kind, mode, mod mechanism and edge value. List every test cut and every test kept with the reason. The nightly WorldgenDriver sweep is the all-clusters check.
- **m4** Invoke-Worldgen.ps1:56: reject `.`, `..`, empty names and anything with a path separator in -ModConfig names.
- **m7** Clusters.cs:87, Compatibility.cs:479: a clear assertion message when the baseline is missing, not an exception.
- **Nits**: Flatness.cs:43 "15 at most" → the game's thickest is 16; remove the empty config-backup folder after a restore; warden-start argument quoting with a trailing backslash.

## Scope
- IN: common/tools/TestRunner/**, common/tools/WorldgenDriver/** (commit in common on branch OM-007-smooth-borders; the manager bumps the pointer), mods/smooth-borders/src/SmoothBorders.Tests/** (commit in ONI-Mods on branch OM-007-smooth-borders). READMEs where behaviour changes.
- OUT: mod code under mods/smooth-borders/src/SmoothBorders (no finding needs it); store records under .context (m5 lands at completion); launching the game or in-game tests — a game that is not ours is open; never touch it.
- Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` Warning(s)/Error(s) lines verbatim, TestRunner and WorldgenDriver builds, PowerShell parse check of each changed .ps1 (0 errors), dry checks outside the game for M1/m1/m4 (scratchpad copies, fake processes) with output. List the in-game proof runs the manager should schedule (narrowest filters, expected time). Commits: one terse subject line each, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

## Report
- File: .context/tasks/OM-007-smooth-borders/units/16-implement-report.md (do not commit). If the Write tool refuses it, put the full report in your final message.
- Must contain: per finding, the change and file:line or "not changed" with why; tests cut/kept list; build/parse output verbatim; dry checks; proposed in-game proofs; store facts affected; ICON tooling friction (or none).
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/smooth-borders/src/SmoothBorders.Tests`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 2 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:domains/game-interfaces.md, mods:overview.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/tools/TestRunner`, `common/tools/WorldgenDriver`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → tools/WorldgenDriver
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools/WorldgenDriver
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → tools/WorldgenDriver
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
tools/WorldgenDriver:domains/entities.md · reference
  WorldgenDriver's mod class in Mod.cs, the Driver state and --worldgen options in Driver.cs, the…
  tools/WorldgenDriver/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/error-handling.md
tools/WorldgenDriver:standards/code-style.md · reference
  WorldgenDriver's code style for the C# mod and Invoke-Worldgen.ps1: indentation and braces per…
  tools/WorldgenDriver/.context/standards/code-style.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/WorldgenDriver:standards/error-handling.md · reference
  WorldgenDriver's error handling: principles, the result status table with where each is raised,…
  tools/WorldgenDriver/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/code-style.md
tools/WorldgenDriver:standards/naming-conventions.md · reference
  WorldgenDriver's naming rules: file, run output and Klei-folder names, class, patch class and m…
  tools/WorldgenDriver/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
tools/WorldgenDriver:domains/glossary.md · reference
  WorldgenDriver glossary: abbreviations (ONI, DLC, JSONL, cluster prefixes), terms to avoid with…
  tools/WorldgenDriver/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools/WorldgenDriver
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md, tools/WorldgenDriver:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
