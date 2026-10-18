# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead/src/OvagroOverhead.Tests`, `mods/salty-surf/src/SaltySurf.Tests`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/10-test.md`

```
# Unit 10 — test: Steps S6 + S9, farm plot tests T6-T8 and the save round trip T9/T10

Steps S6 and S9: in-game tests proving that Duplicant-side planting works in downward-facing (FlipV) Hydroponic Farms and Farm Tiles (preview, deposit, irrigation, Copy Settings, the planter side screen), that upright plots are unchanged, and that ceiling Ovagro (wild with vines, and in a FlipV hydroponic farm) survive a save and load.
Ticket: OM-006

## Files to create/modify
mods/ovagro-overhead/src/OvagroOverhead.Tests/Plots.cs (T6 Hydroponic Farm FlipV, T7 Farm Tile FlipV + Neutral regression + Copy Settings between two downward tiles, T8 side screen) and Persistence.cs (T9/T10, second test FreshSave), as the S6 and S9 sections of units/04-plan-report.md and the design's Testability table (units/03-design-report.md) describe; extend Kit.cs where needed. Mod source is out of scope: report mod bugs with evidence, do not fix them.

## Pattern to follow
The suite as it stands (Kit.cs, Registration.cs, WildPlanting.cs, Vines.cs; units/08-test-report.md, units/09-test-report.md), mods/salty-surf/src/SaltySurf.Tests/Persistence.cs for the round trip, and the UI-test conventions of upgrade-planner's tests for T8 (widgets driven the way the input module does).

## Context the packet does not carry
- Run with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead` (and `-Filter` while iterating). If the harness says the game is running, stop and report it; never close a game you did not start. Never rebuild during a run. Leave the game closed and mods.json restored, as the harness does.
- Pace every step to game time; wait on conditions, never frames apart.
- If a test misbehaves and you cannot work out why after a reasonable look, stop iterating: screenshot the setup and the failing state (t.Screenshot, cropped with Pillow to the area that matters), and report the paths with what you expected versus what each shows. The user can often spot the cause from a picture.
- Take screenshots of a FlipV Hydroponic Farm and a FlipV Farm Tile each holding a grown ceiling Ovagro, of the planter side screen on a FlipV tile with Ovagro selectable, and of the loaded state after the round trip; look at them yourself and describe them.
- The whole suite (T1-T10) must pass together at the end, not only filtered runs.
- Commit trailers exactly: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9`.

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds before runs.
- The full suite run exits 0 with every test passing; the report pastes the status lines and the run folder.
- Screenshots as listed, paths in the report.
- Commit all created/modified test files on OM-006-ovagro-overhead, message `OM-006: farm plot and save tests` (one terse line). Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/10-test-report.md
- Must contain: pass/fail counts, status lines, what was written, screenshot paths with what each shows, mod bugs found (with evidence), Player.log findings, and a "Store reads" section.
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

mods/salty-surf:standards/code-style.md · reference
  C# code style for Salty Surf: indentation, braces, imports, comments, control structures, Harmo…
  mods/salty-surf/.context/standards/code-style.md · src/** → mods/salty-surf/src/SaltySurf.Tests · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/salty-surf:standards/error-handling.md · reference
  Error handling rules for Salty Surf: guarding Harmony patches and transpilers, try/catch, loggi…
  mods/salty-surf/.context/standards/error-handling.md · src/** → mods/salty-surf/src/SaltySurf.Tests · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/salty-surf:standards/naming-conventions.md · reference
  Naming conventions for Salty Surf: files, variables, methods, types, ModStrings keys, ids and t…
  mods/salty-surf/.context/standards/naming-conventions.md · src/** → mods/salty-surf/src/SaltySurf.Tests · linked from standards/code-style.md · linked from standards/error-handling.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 4 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md, mods/salty-surf:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
