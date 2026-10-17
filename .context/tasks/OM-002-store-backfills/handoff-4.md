# Handoff — OM-002-store-backfills

**Scope:** `mods/smart-weight-plate`, `mods/storage-pod-redux`, `mods/sweep-zones`, `mods/upgrade-planner`, `mods/vent-freeze-fix`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/04-implement-phaseA.md`

```
# Unit 04 — implement: Phase A, mods group C

## Objective
Draft change-sets replacing the scaffold placeholders in the named documents with real content. Plan step: Phase A.

## Scope
- IN: the nodes `mods/<name>` for: smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner, vent-freeze-fix. Documents per node: `overview.md` (both sections), `domains/README.md`, `standards/README.md`.
- OUT: every other node.
- Acceptance: one `phaseA-<mod>.json` per listed mod, each accepted by `write plan` (output shown).

## Context the packet does not carry
- Read `.context/tasks/OM-002-store-backfills/units/common-rules.md` first; it binds this unit.

## Report
- File: .context/tasks/OM-002-store-backfills/units/04-implement-phaseA-report.md
- Must contain: per node, one line on what went in; TODO(owner)s left; the `write plan` summary per file; friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs → mods/storage-pod-redux · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 166 further candidate(s) matched and are not listed above (7723 of 8192 bytes; the whole selection is 49293). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/smart-weight-plate mods/storage-pod-redux mods/sweep-zones mods/upgrade-planner mods/vent-freeze-fix --budget 49293

⚠ 15 record(s) in these 5 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/smart-weight-plate:domains/README.md, mods/smart-weight-plate:overview.md, mods/smart-weight-plate:standards/README.md, mods/storage-pod-redux:domains/README.md, mods/storage-pod-redux:overview.md and 10 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
