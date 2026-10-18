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
