# Handoff — OM-012-u60-readiness

**Scope:** `mods/smooth-borders`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/07-implement.md`

```
# Unit 07 — implement + test: Smooth Borders on U60

## Objective
Unit 06's smoke test found Smooth Borders logging "DrawWorldBorder has changed; borders stay as the game draws them" on U60 762230: its patch detects the change and no-ops, so border smoothing is off on U60 while its tests still pass. Unit 01's IL/patch check missed it. Make Smooth Borders smooth borders on U60, and close the test gap so a no-op patch fails a test.

## Scope
- IN: mods/smooth-borders (mod and test project). Compare vanilla `DrawWorldBorder` (and whatever it calls) between U59 (F:\ONI-Frozen\APP) and U60 (F:\SteamLibrary\steamapps\common\OxygenNotIncluded); update the patch. Prefer a change that works on both builds; if that isn't reasonably possible, say so and make it U60-only (then raise `minimumSupportedBuild` to 762230 in the source mod_info.yaml and tell the coordinator — that change belongs on the held branch). Add or strengthen a test in its test project so that the suite fails when the patch is inactive (follow the existing tests' patterns).
- Run the Smooth Borders suite in game on U60: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders` (one call; never -KeepGame; don't stop a run mid-run). Also confirm the new test fails with the patch disabled if that's cheap to show (e.g. against the unfixed build), otherwise explain why it would.
- Save safety: unit 06 confirmed TestRunner never writes over Jeremy's saves; don't change that.
- OUT: other mods; commits; releases; Steam branch switches.
- Acceptance: build output with 0 errors; the in-game suite output showing pass counts; Player.log no longer carrying the "DrawWorldBorder has changed" line. Your report must include the actual output — do not claim success without it.

## Context the packet does not carry
- Branch: OM-012-u60-only checked out (the both-builds lane is OM-012-u60-readiness; the coordinator will move commits). Don't switch branches.
- Unit 06 report: .context/tasks/OM-012-u60-readiness/units/06-test-report.md. Unit 01 report (IL diff method): units/01-explore-u60-breakage.md; its tooling is in the scratchpad.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad, never the repo root. Keep files' existing line endings.
- If a test fails for a reason you can't explain, stop and screenshot the setup and the failure state; don't loop.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/07-implement-report.md
- Must contain: what changed in vanilla; files changed and why; BOTH-BUILDS or U60-ONLY; build + in-game output; why unit 01's check missed it (one paragraph, so the method can be fixed).
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 2 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:domains/game-interfaces.md, mods:overview.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
