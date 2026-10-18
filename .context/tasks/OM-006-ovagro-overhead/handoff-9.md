# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead/src/OvagroOverhead.Tests`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/09-test.md`

```
# Unit 09 — test: Step S5, Pip and vine tests T2-T5

Step S5: in-game tests proving that Pips find and plant ceiling cells for Ovagro and that a ceiling Ovagro's vines grow along the ceiling. This is the user's core requirement ("make sure their vines will grow as expected").
Ticket: OM-006

## Files to create/modify
mods/ovagro-overhead/src/OvagroOverhead.Tests/WildPlanting.cs (T2 query, T3 TryPlant with the floor regression, T4 a real Pip end to end) and Vines.cs (T5), as the S5 section of units/04-plan-report.md and the design's Testability table (units/03-design-report.md) describe; extend Kit.cs where needed. Mod source is out of scope: a test that shows a mod bug is reported with evidence (results line, Player.log text, screenshot), not fixed.

## Pattern to follow
The suite unit 08 built (Kit.cs, Registration.cs; units/08-test-report.md) and mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs.

## Context the packet does not carry
- Pips walk floors, walls and ceilings (user); the design's "Pips" section explains why a Pip on the ceiling plants into its own cell.
- Run with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead` (and `-Filter` while iterating). The user closed their game for these runs; if the harness says the game is running, stop and report it, never close a game you did not start. Never rebuild during a run. The harness restores mods.json and closes the game; leave it that way.
- Pace every step to game time (wait game seconds after ReplaceElement, spawning, planting and maturity changes; wait on conditions), never frames apart.
- T5 must leave visual evidence: take screenshots of the grown ceiling vine (and of a T3/T4 ceiling plant), crop them with Pillow and look at them yourself; describe in the report whether the mother hangs right side out and the vines hug the ceiling's underside.
- T4 (real Pip) gets about 180 s; mark it `Explicit` only if it cannot pass reliably inside that, and say so.
- Watch Player.log for exceptions during runs and quote any.
- Commit trailers exactly: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9`.

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds before runs.
- The full suite run (`-Mods OvagroOverhead`) exits 0 with T1-T5 passing (T4 may be Explicit with its own filtered run passing); the report pastes the results.jsonl status lines and the run folder.
- Screenshots exist for T5 and a ceiling plant, with their paths in the report.
- Commit all created/modified test files in one or more commits on OM-006-ovagro-overhead, message `OM-006: Pip and vine tests` (one terse line). Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/09-test-report.md
- Must contain: pass/fail counts, the status lines, what was written, screenshot paths with what each shows, mod bugs found (with evidence), Player.log findings, and a "Store reads" section.
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: nothing in these nodes carries any of `in-game-testing`, `screenshots`, `test-assertions`, `test-cleanup` and `test-pacing`. The vocabulary here is: ks tags --store C:/dev/ONI-Mods --node mods/ovagro-overhead

⚠ 3 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
