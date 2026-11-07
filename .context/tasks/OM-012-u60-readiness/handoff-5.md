# Handoff — OM-012-u60-readiness

**Scope:** `mods/motion-sensor-range`, `mods/smart-weight-plate`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/05-implement.md`

```
# Unit 05 — implement: GameScenePartitioner.Add context-object overload (U60-only)

## Objective
Plan step "Fix per mod" of OM-012, held lane. Move the 4 obsolete `GameScenePartitioner.Add(..., Action<object>)` calls (motion-sensor-range RangeSlider.cs:164; smart-weight-plate SmartWeightPlate.cs:152,154,156) to the U60 context-object overload. The current branch is `OM-012-u60-only` (held until U60 releases), so U60-only API is fine here.

## Scope
- IN: those two files; `minimumSupportedBuild: 762230` in mods/motion-sensor-range/src/MotionSensorRange/mod_info.yaml and mods/smart-weight-plate/src/SmartWeightPlate/mod_info.yaml (wherever each mod's source mod_info.yaml lives — not publish/).
- OUT: everything else; launching the game; commits.
- Acceptance: both mods and their test projects build with 0 errors and no CS0618 for GameScenePartitioner.Add. Read the new overload's implementation first and say whether callback behaviour differs (e.g. what the context object and data arguments carry). Your report must include the actual build output.

## Context the packet does not carry
- Unit 04's report has the proposed fix: .context/tasks/OM-012-u60-readiness/units/04-implement-report.md.
- Game DLLs: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (via common/oni-mod.props). Build with `-p:ModDeployFolder=none`. Write scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad, never the repo root. Keep the files' existing line endings.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/05-implement-report.md
- Must contain: files changed and why; build output; behaviour differences to check in game.
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
blk-00af2e4182a4 · reference
  When to Comment
  mods/just-fall-already/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-15a99617bbd0 · reference
  When to Comment
  mods/refined-building/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-29493d7a7b8a · reference
  When to Comment
  mods/upgrade-planner/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-3d37bcce99a4 · reference
  When to Comment
  mods/storage-pod-redux/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-40ac288e819d · reference
  When to Comment
  mods/motion-sensor-range/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-430105ebb58b · reference
  When to Comment
  mods/vent-freeze-fix/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-4351aee3774b · reference
  When to Comment
  mods/multichannel-critter-sensor/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-5b85e9aaa8d8 · reference
  When to Comment
  mods/smart-weight-plate/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-6090624fb2d7 · reference
  When to Comment
  mods/sweep-zones/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-6e6c7ccc6bba · reference
  When to Comment
  mods/detect-any-element/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 61 further candidate(s) matched and are not listed above (8174 of 8192 bytes; the whole selection is 23747). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/motion-sensor-range mods/smart-weight-plate --budget 23747

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/motion-sensor-range:tags.md, mods/smart-weight-plate:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
