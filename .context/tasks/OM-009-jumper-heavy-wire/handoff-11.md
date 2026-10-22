# Handoff — OM-009-jumper-heavy-wire

**Scope:** `common/tools/TestRunner`
**Store:** `C:/dev/ONI-Mods/common` — the store the brief below was computed from.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-009-jumper-heavy-wire/` — tracked, write-path exempt, never authoritative ([IF-1.C]). ⚠ It is in the TASK's own store, which is not the store above: this scope reached across checkouts. The coordinator chose it with `--store`, or by the checkout it was standing in.

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-009-jumper-heavy-wire/units/11-implement.md`

```
# Unit 11 — implement: FixtureArea.Clear misses buildings on no layer

Ticket: OM-009. Work in the `common/` submodule (its own git repo, already on branch OM-009-jumper-heavy-wire). Jeremy's standing rule: fix the TestRunner harness when a test needs it; no workarounds in test code.

Root cause from units/10-debug-findings.md (read it): `FixtureArea.Clear` (common/tools/TestRunner/.../FixtureArea.cs:112-124) collects only objects on `Grid.Objects`. A bridge whose ends were given up to other owners and whose middle sits on no layer survives the clear and leaks into the next test (Jumper Bridges' BridgeLinks rings it into the next bridge's group).

Do:
- Make Clear also destroy every building (complete and under construction) whose placement cells touch the rect, found generically — the harness must not know about any mod's types.
- After the clear, log a warning naming anything built through World.Build in the cleared area that is still alive.
- Keep existing behaviour for everything else; follow the harness's existing code style and logging.
- Update the TestRunner README/docs if they describe Clear.

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` from C:\dev\ONI-Mods succeeds — output tail in report.
  - Do NOT launch the game.
  - Commit inside common/ with one terse line, e.g. `TestRunner: fixture clear removes buildings on no layer`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`. Do not commit in the outer repo; do not push.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/11-implement-report.md (outer repo; leave it uncommitted, the manager commits it)
  - Must contain: diff summary, the common/ commit hash, build output verbatim.
  - Model tier: default
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
