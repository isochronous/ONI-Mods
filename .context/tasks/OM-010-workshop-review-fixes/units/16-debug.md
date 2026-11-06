# Unit 16 — explore (debug): UpgradePlanner ByHand.CancelledRebuild failure

## Objective
Root-cause the one failure in run om010-20261022-123342 (common/tools/TestRunner/runs/om010-20261022-123342/: results.jsonl, Player.log):
`FAIL UpgradePlanner.Tests/ByHand.CancelledRebuild (3.0 s): the plan is complete: Pick at least one building that can be built in Granite. (expected true)`
Decide whether OM-010's Upgrade Planner change caused it, or whether it was already failing. That change is the GetOverlayColorData cache: the colorsBoxVersion assignment in aa2fae7, then the world-id key in 768d2ac. The only earlier run of this test is mono-20261015-182351 (pass). Upgrade Planner commits since then, such as material mode, may be the cause instead.

## Scope
- IN (read only): the run folders, mods/upgrade-planner/src/**, `git log`/`git diff` of mods/upgrade-planner since 2026-10-15, the test's own steps. Use the systematic-debugging skill. Test-state leakage from earlier tests in the same session is possible (the harness clear changed in OM-009).
- OUT: no code changes, no game launch, no commits.
- Acceptance: the cause traced to a change and line with evidence (log lines, code), and whether it is OM-010's. Name the fix you'd make and how far it reaches, but don't make it. Confidence level.

## Report
- File: .context/tasks/OM-010-workshop-review-fixes/units/16-debug-findings.md
- Model tier: complex
