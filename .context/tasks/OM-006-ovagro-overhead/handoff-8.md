# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead/src/OvagroOverhead.Tests`, `mods/just-fall-already/src/JustFallAlready.Tests`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/08-test.md`

```
# Unit 08 — test: Step S4a, test kit helpers and T1 (registration)

Step S4a: the test project's shared helpers (a natural ceiling made in code, and whatever S5-S9 will reuse) and test T1, which proves the mod loads with its DLC requirement and registers `VineMotherCeiling` and its preview as the design specifies.
Ticket: OM-006

## Files to create/modify
Under mods/ovagro-overhead/src/OvagroOverhead.Tests/, as the S4a section of units/04-plan-report.md lists. Mod source is out of scope: if a test shows a mod bug, report it with the evidence; do not fix mod code.

## Pattern to follow
mods/just-fall-already/src/JustFallAlready.Tests (Doors.cs) and the TestRunner kit (common/tools/TestRunner, README and Kit/). The natural ceiling: the plan's helper laying natural sandstone with `SimMessages.ReplaceElement` (read the exact arguments from the kit's Dupes.cs), because built tiles are Foundation and the seed's ground check rejects them.

## Context the packet does not carry
- Tests run with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead` (fixture testbed-so.sav, all DLCs active). The user's request to make sure the vines grow covers launching the game for test runs. Never leave the game running or the user's mods.json changed afterwards; the harness restores both, so do not work around it. Never rebuild while a run is in progress.
- Pace every step to game time: wait game seconds (1-2 after building or clearing, more after sim changes such as ReplaceElement) and wait on conditions, never frames apart. The user has had to point this out several times.
- Screenshots: `yield return t.Screenshot("name")` writes a PNG into the run's artifacts folder; name each one and list the paths in the report.
- Prior reports: units/03-design-report.md (Testability, T1-T10), units/04-plan-report.md (S4a), units/06-implement-report.md and units/07-implement-report.md (what was built).
- Watch Player.log for repeating exceptions during runs; copy any exception text into the report.

## Acceptance criteria
- T1 passes in game; the report includes the results.jsonl lines for the run and the run folder path. Do not claim a pass without the output.
- The natural-ceiling helper is exercised once (a short test or part of T1) and a screenshot shows the sandstone where expected.
- Player.log shows the mod's "Loaded version" line and no OvagroOverhead warning; quote both checks.
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds before the run.
- Commit all created/modified test files in one commit on OM-006-ovagro-overhead: `OM-006: test kit and registration test`, with trailers
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
  Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/08-test-report.md
- Must contain: pass/fail counts, the results.jsonl lines, what was written, screenshot paths, any mod bug found (with evidence), harness friction, and a "Store reads" section.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-9d52ccfdb3df · reference
  Key Code Paths
  mods/just-fall-already/.context/domains/entities.md#key-code-paths · names mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs → mods/just-fall-already/src/JustFallAlready.Tests · tagged in-game-testing → in-game-testing
mods/just-fall-already:standards/code-style.md · reference
  Code style guide for Just Fall Already!: C# formatting, imports, comments, control structures,…
  mods/just-fall-already/.context/standards/code-style.md · src/** → mods/just-fall-already/src/JustFallAlready.Tests · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/just-fall-already:standards/error-handling.md · reference
  Error handling conventions for Just Fall Already!: guarding patches, try/catch cases, logging l…
  mods/just-fall-already/.context/standards/error-handling.md · src/** → mods/just-fall-already/src/JustFallAlready.Tests · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/just-fall-already:standards/naming-conventions.md · reference
  Naming conventions for Just Fall Already!: files, namespaces, variables, methods, patch classes…
  mods/just-fall-already/.context/standards/naming-conventions.md · src/** → mods/just-fall-already/src/JustFallAlready.Tests · linked from standards/code-style.md · linked from standards/error-handling.md
blk-9d74aee10321 · reference
  Project-Specific Rules
  mods/just-fall-already/.context/standards/code-style.md#project-specific-rules · tagged in-game-testing → in-game-testing
blk-f21fb042f2ec · reference
  Just Fall Already! specifics
  mods/just-fall-already/.context/standards/naming-conventions.md#project-specific-conventions/just-fall-already-specifics · tagged in-game-testing → in-game-testing
blk-58f12d9bf0cf · reference
  Just Fall Already! specifics
  mods/just-fall-already/.context/standards/code-style.md#project-specific-rules/just-fall-already-specifics · tagged in-game-testing → in-game-testing
tags: no record satisfies `(in-game-testing && test-pacing) || test-assertions || test-cleanup` (`&&` binds tighter than `||`); selected the 4 that carry any of its terms. This is the union, not the expression you asked for.

⚠ 4 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/just-fall-already:tags.md, mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
