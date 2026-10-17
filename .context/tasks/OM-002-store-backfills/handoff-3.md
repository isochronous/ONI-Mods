# Handoff — OM-002-store-backfills

**Scope:** `mods/motion-sensor-range`, `mods/multichannel-critter-sensor`, `mods/natural-backwalls-everywhere`, `mods/no-magic-fridges`, `mods/refined-building`, `mods/salty-surf`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/03-implement-phaseA.md`

```
# Unit 03 — implement: Phase A, mods group B

## Objective
Draft change-sets replacing the scaffold placeholders in the named documents with real content. Plan step: Phase A.

## Scope
- IN: the nodes `mods/<name>` for: motion-sensor-range, multichannel-critter-sensor, natural-backwalls-everywhere, no-magic-fridges, refined-building, salty-surf. Documents per node: `overview.md` (both sections), `domains/README.md`, `standards/README.md`.
- OUT: every other node.
- Acceptance: one `phaseA-<mod>.json` per listed mod, each accepted by `write plan` (output shown).

## Context the packet does not carry
- Read `.context/tasks/OM-002-store-backfills/units/common-rules.md` first; it binds this unit.

## Report
- File: .context/tasks/OM-002-store-backfills/units/03-implement-phaseA-report.md
- Must contain: per node, one line on what went in; TODO(owner)s left; the `write plan` summary per file; friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

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
blk-5b2181c3b848 · reference
  Null checks on game objects
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-77e727b81a5c · reference
  Null checks on game objects
  mods/vent-freeze-fix/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-ae3db55a76d7 · reference
  Null checks on game objects
  mods/jumper-bridges/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-d6d515fe70fe · reference
  Null checks on game objects
  mods/no-magic-fridges/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-da9dc4418506 · reference
  Null checks on game objects
  mods/storage-pod-redux/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-de388d2e3d6e · reference
  Null checks on game objects
  mods/salty-surf/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-e3e6ea0506fe · reference
  Null checks on game objects
  mods/just-fall-already/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-fcfa7d5a5167 · reference
  Null checks on game objects
  mods/corner-build-anything/.context/standards/error-handling.md#guarding-patches/null-checks-on-game-objects · names mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs → mods/motion-sensor-range
blk-0ec0eff52bb1 · reference
  Missing data: warn, continue
  mods/motion-sensor-range/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-101c18a7fdbd · reference
  Missing data: warn, continue
  mods/bitshifter/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-1ce6ea29b82a · reference
  Missing data: warn, continue
  mods/salty-surf/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-2862c2be79f7 · reference
  Missing data: warn, continue
  mods/no-magic-fridges/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-3930951b591e · reference
  Missing data: warn, continue
  mods/vent-freeze-fix/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-52d2797f1664 · reference
  Missing data: warn, continue
  mods/sweep-zones/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-9a05a76074e8 · reference
  Missing data: warn, continue
  mods/upgrade-planner/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-a4c34b089222 · reference
  Missing data: warn, continue
  mods/smart-weight-plate/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-af94c6ab1d6f · reference
  Missing data: warn, continue
  mods/jumper-bridges/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-bfbab614ef56 · reference
  Missing data: warn, continue
  mods/detect-any-element/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
blk-c5d951f306e9 · reference
  Missing data: warn, continue
  mods/just-fall-already/.context/standards/error-handling.md#guarding-patches/missing-data-warn-continue · names mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs → mods/multichannel-critter-sensor
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 174 further candidate(s) matched and are not listed above (7987 of 8192 bytes; the whole selection is 53978). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/motion-sensor-range mods/multichannel-critter-sensor mods/natural-backwalls-everywhere mods/no-magic-fridges mods/refined-building mods/salty-surf --budget 53978

⚠ 18 record(s) in these 6 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/motion-sensor-range:domains/README.md, mods/motion-sensor-range:overview.md, mods/motion-sensor-range:standards/README.md, mods/multichannel-critter-sensor:domains/README.md, mods/multichannel-critter-sensor:overview.md and 13 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
