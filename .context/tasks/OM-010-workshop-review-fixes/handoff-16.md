# Handoff — OM-010-workshop-review-fixes

**Scope:** `mods/upgrade-planner/src`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-010-workshop-review-fixes/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-010-workshop-review-fixes/units/16-debug.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
mods/upgrade-planner:domains/entities.md · reference
  The planner tool, plan classes, replacement rules, order queue, presets, dialog, Blueprints Exp…
  mods/upgrade-planner/.context/domains/entities.md · src/UpgradePlanner/** → mods/upgrade-planner/src · linked from domains/glossary.md
mods/upgrade-planner:domains/glossary.md · reference
  Glossary for Upgrade Planner: abbreviations (BE, ABM, PLib, def), terms to avoid such as upgrad…
  mods/upgrade-planner/.context/domains/glossary.md · src/UpgradePlanner/** → mods/upgrade-planner/src · linked from domains/entities.md
mods/upgrade-planner:standards/code-style.md · reference
  C# code style for the Upgrade Planner mod: formatting, imports, comments, Harmony patch and gam…
  mods/upgrade-planner/.context/standards/code-style.md · src/** → mods/upgrade-planner/src · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/upgrade-planner:standards/error-handling.md · reference
  Error handling and logging rules for the Upgrade Planner mod: guarding patches, try/catch use,…
  mods/upgrade-planner/.context/standards/error-handling.md · src/** → mods/upgrade-planner/src · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/upgrade-planner:standards/naming-conventions.md · reference
  Naming rules for the Upgrade Planner mod: files, namespaces, patch classes, variables, ModStrin…
  mods/upgrade-planner/.context/standards/naming-conventions.md · src/** → mods/upgrade-planner/src · linked from standards/code-style.md · linked from standards/error-handling.md
blk-4ed03be05da6 · reference
  Other mods
  .context/testing/integration-testing.md#test-environment/other-mods · names mods/upgrade-planner/src/UpgradePlanner.Tests/DialogDrag.cs → mods/upgrade-planner/src
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/upgrade-planner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
