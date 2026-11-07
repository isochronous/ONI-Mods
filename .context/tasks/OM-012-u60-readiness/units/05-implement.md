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
