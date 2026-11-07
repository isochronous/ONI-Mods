# Unit 03 — implement: Ovagro's Overhead on U60

## Objective
Plan step "Fix per mod" of OM-012. Make mods/ovagros-overhead build and behave correctly against the U60 testing branch (build 762230).

## Scope
- IN: mods/ovagros-overhead (mod and test project). U60 replaced `Crop.CropVal`'s `cropId`/`numProduced` with a `products` list and renamed `PlantAttributes.YieldAmount` to `YieldFactor` (VineBranchCeilingConfig.cs:68,78,82). The ceiling vine should follow vanilla U60 `VineBranchConfig` (the floor vine it mirrors) — compare vanilla's U59 vs U60 VineBranchConfig/VineMotherConfig and carry every relevant change over, not just enough to compile. Then fix any further compile errors in the mod and its tests. Set `minimumSupportedBuild: 762230` in its mod_info.yaml.
- OUT: other mods; common/; launching the game; commits; version/CHANGELOG.
- Acceptance: `dotnet build` of the mod and its test project with zero errors; a short table of vanilla VineBranchConfig U59→U60 changes and how each is reflected in the ceiling vine. Your report must include the actual command output — do not claim success without it.

## Context the packet does not carry
- Decision: U60-only, no U59 compatibility — the crop types changed shape, so this change waits on the held branch until U60 releases (other mods' both-builds fixes ship now).
- Unit 01 report: .context/tasks/OM-012-u60-readiness/units/01-explore-u60-breakage.md (ovagros-overhead section).
- Game DLLs: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (via common/oni-mod.props). Frozen U59 copy: F:\ONI-Frozen\APP. Unit 01's decompile/IL diff tooling and outputs are in the scratchpad: C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad (ILDiff\). Write scripts with the Write tool.
- Build only your projects with `-p:ModDeployFolder=none`; other units edit other mods concurrently.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/03-implement-report.md
- Must contain: files changed with why; the vanilla-change table; build output; what needs an in-game check (yield, harvest, growth).
- Model tier: default
