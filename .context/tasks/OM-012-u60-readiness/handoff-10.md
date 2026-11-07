# Handoff — OM-012-u60-readiness

**Scope:** `mods/salty-surf`, `mods/upgrade-planner`, `mods/motion-sensor-range`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/09-implement.md`

```
# Unit 09 — implement: review findings from unit 08

## Objective
Address the review findings in .context/tasks/OM-012-u60-readiness/units/08-review-report.md before the PRs open, then rerun the touched suites in game on U60.

## Scope
- IN:
  - Finding 1 (moderate, held lane): motion-sensor-range RangeSlider.cs:36-41 — bind the open delegate without throwing (`throwOnBindFailure: false` or equivalent) and fold its null check into `SensorInternalsFound`, so a future signature change reaches the existing missing-internals warning. This file's change lives on branch OM-012-u60-only — **you are on OM-012-u60-only; make only this edit there first**, then tell the coordinator in your report (the coordinator commits it).
  - Minor findings, ship-now lane (salty-surf Mod.cs, upgrade-planner UI/MaterialPicker.cs): Salty Surf logs one warning when neither descriptor target exists and when a translation lacks `{amount}`; use a declared-only lookup for `CollectDescriptors`; Upgrade Planner caches its reflection lookups and logs one warning naming the mod on a miss. These must keep working on both U59 744825 and U60 762230 (check against F:\ONI-Frozen\APP).
  - Build the three mods + their test projects; then one in-game run on U60: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SaltySurf,UpgradePlanner,MotionSensorRange -Steam 3468585385` (UpgradePlanner's Blueprints Expanded tests need that Workshop mod; one call so the game isn't relaunched per mod; never -KeepGame; don't stop a run mid-run).
- OUT: other findings (CBA test DLL on U59, submodule push, publish/ yaml — coordinator's); commits; branch switches beyond what's stated; releases.
- Acceptance: build output 0 errors; in-game suite counts with the runner's actual output. Do not claim success without showing evidence.

## Context the packet does not carry
- The salty-surf and upgrade-planner edits are ship-now-lane files; making them on OM-012-u60-only is fine — the coordinator moves those commits to OM-012-u60-readiness. Keep each mod's edits confined to its own files so they split cleanly.
- Unit 06 confirmed TestRunner never writes over Jeremy's saves; don't change that.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad. Keep files' existing line endings.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/09-implement-report.md
- Must contain: each finding → what changed (file:line); build output; in-game output.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-00c50af2c97d · reference
  Null checks on game objects
  mods/motion-sensor-range/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-08ab5b72b198 · reference
  Null checks on game objects
  mods/upgrade-planner/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-10650aa02cc8 · reference
  Null checks on game objects
  mods/smart-weight-plate/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-1a260b7c2cae · reference
  Null checks on game objects
  mods/detect-any-element/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-2a8492ae16cb · reference
  Null checks on game objects
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-2af030a5b58e · reference
  Null checks on game objects
  mods/sweep-zones/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-379c7f4d853c · reference
  Null checks on game objects
  mods/refined-building/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-414d69d4cbc7 · reference
  Null checks on game objects
  mods/bitshifter/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-475f58d796c4 · reference
  Null checks on game objects
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 58 further candidate(s) matched and are not listed above (8131 of 8192 bytes; the whole selection is 22385). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/motion-sensor-range mods/salty-surf mods/upgrade-planner --budget 22385

⚠ 3 record(s) in these 3 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/motion-sensor-range:tags.md, mods/salty-surf:tags.md, mods/upgrade-planner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
