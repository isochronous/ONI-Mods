# Unit 08 — review: OM-012 U60 readiness, both lanes

## Objective
Review every code change of OM-012 before PRs open. Two lanes:
- Ship-now (works on live U59 744825 and U60 762230): `git diff main..OM-012-u60-readiness -- mods common` and inside the submodule `git -C common diff main..u60`.
- Held until U60 release: `git diff OM-012-u60-readiness..OM-012-u60-only -- mods`.

## Scope
- IN: Salty Surf descriptor patch pair (Mod.cs); Upgrade Planner Patches.cs + UI/MaterialPicker.cs (reflection delegates, BuildableState vs bool callback types); Corner Build Anything tests Reach.cs; Subscribe/Unsubscribe conversions in bitshifter, jumper-bridges (handles), motion-sensor-range, multichannel-critter-sensor, smart-weight-plate; Smooth Borders FailSafes.cs; common/tools/TestRunner Patches.cs, Reflect.cs, README.md, Worldgen.cs, GameMode.ps1; held lane: Ovagro's Overhead VineBranchCeilingConfig/VineMotherCeilingConfig/tests/mod_info.yaml, GameScenePartitioner.Add conversions + mod_info.yaml in motion-sensor-range and smart-weight-plate.
- Focus:
  1. Lane correctness: does every ship-now change actually work on U59 744825 (check against F:\ONI-Frozen\APP DLLs), and does every change on the held lane belong there? Anything U60-only that slipped into ship-now is critical.
  2. Event subscription conversions: leaks (unsubscribe actually removes the handler, handles kept), handler signature/semantics differences, static vs instance callbacks receiving the right context.
  3. Reflection fallbacks: what happens if a target isn't found (null delegates, silent no-ops vs logged warnings), exceptions at patch time.
  4. GameMode.ps1: the blank-key change — can it ever misclassify the player's real last-save pointer as the test run's and overwrite/restore it wrongly? Saves and the save pointer are the one irreversible area on U60.
  5. Repo conventions (the packet's standards pointers).
- OUT: re-running in-game tests (unit 06/07 did, reports in units/); editing code.
- Acceptance: findings each with severity (critical/moderate/minor), file:line, and the fix expected; list the passes run. "No findings" counts only with the passes listed.

## Context the packet does not carry
- Decision: two lanes as above (Jeremy, 2026-11-07). No Magic Fridges retired — its obsolete calls are deliberately unchanged.
- Reports: units/01-explore-u60-breakage.md, 02..05-implement-report.md, 06-test-report.md, 07-implement-report.md.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/08-review-report.md
- Must contain: findings table; passes run; a verdict per lane.
- Model tier: complex
