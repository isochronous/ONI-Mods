# Unit 13 — review: the Ovagro Overhead change since unit 12

Feature: Ovagro Overhead (Ovagro grows from ceilings). Unit 12 (units/12-review-report.md) reviewed the change up to 896238b. This unit reviews what changed after it.
Ticket: OM-006

## Changed files
In this repository, `git diff 896238b..HEAD -- . ':!.context' ':!mods/ovagro-overhead/.context'`:
- mods/ovagro-overhead/src/OvagroOverhead/VineBranchCeilingConfig.cs (new): the vanilla vine (VineBranchConfig) under its own id VineBranchCeiling; it and the ceiling mother name it as BRANCH_PREFAB_NAME, so removing the mod drops the plant and its vines together (verified in game: 0 errors on removal).
- CeilingForm.cs (BranchId/BranchTag, `var` removed), VineMotherCeilingConfig.cs (branch prefab, `var` removed).
- Tests: Registration.cs (vine prefab and vanilla-value checks), Vines.cs (uses Kit's grow helpers; vine prefab checks), WildPlanting.cs and Kit.cs (one seed check in Kit), Plots.cs (new VinesFromDownwardFarm), Preview.cs (Explicit Workshop preview scene), Removal.cs (Explicit save for the removal check).
- README.md, publish/workshop-description.txt (user-approved text), publish/preview.png.
In the common/ submodule, `git -C common diff 93d2890..a0fe078`:
- tools/TestRunner: new [OniTest(KeepSave = true)] (Kit/Attributes.cs, Session.cs, TestBehaviour.cs, README.md): the fixture-area clear is skipped before such a test and after the test before it.
- tools/MakePreview/title_preview.py: split titles keep the bottom line in place and give the top line the same ink gap.

## Review focus
- VineBranchCeilingConfig against the decompiled VineBranchConfig (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\): anything vanilla sets that the copy misses or sets differently; any game code that finds vines by the "VineBranch" id or VineBranchConfig.ID (critters that eat vines, harvest, overlays, codex) and would now miss ceiling vines.
- BranchPatch.cs (#9) still applies to ceiling vines (the postfix keys on the mother's prefab id).
- KeepSave in TestBehaviour.RunAll: every path (first test, between tests, FreshSave, boxed tests, next mod) behaves as its doc says, and nothing else changes for tests that do not set it.
- title_preview.py split placement for one-line titles and other fonts.
- Tests assert behaviour (not just that code ran); Explicit tests do not run in the normal suite.
- Standards per the packet and unit 12's passes; approved decisions in plan.md `## Decisions` are not re-litigated.

## Context the packet does not carry
- Evidence: full suite 17/17 (common/tools/TestRunner/runs/run-20261018-013757); removal check run-20261018-014051 (Player.log there).
- Do not launch the game or change files other than the report.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/13-review-report.md
- Must contain: findings with severity (block / fix / nit), file:line and the fix each expects; the passes run and what each examined; a "Store reads" section.
- Model tier: default
