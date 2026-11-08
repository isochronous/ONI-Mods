# Unit 04 — implement: TestRunner timelapse patch and obsolete overloads on U60

## Objective
Plan step "Fix per mod" of OM-012, two parts:
1. common/tools/TestRunner: the Timelapser patch no longer takes effect — U60 writes timelapse/preview images through a new private `CapturePng` that bypasses `WriteToPng`, so test runs would write timelapse images into the player's folder. Retarget it so no image reaches the player's folder.
2. The `Subscribe`/`Unsubscribe` and `GameScenePartitioner.Add` overloads that U60 marks obsolete: move every use in bitshifter, jumper-bridges, motion-sensor-range, multichannel-critter-sensor and smart-weight-plate to the replacement U60 recommends (the Obsolete attribute message names it). Set `minimumSupportedBuild: 762230` in each of those mods' mod_info.yaml if the replacement doesn't exist in U59 (check F:\ONI-Frozen\APP).

## Scope
- IN: common/tools/TestRunner (the submodule is on branch `u60`); the five mods listed.
- OUT: no-magic-fridges (retired — leave its obsolete calls alone); other mods; launching the game; commits (the manager commits, inside common/ first); version/CHANGELOG.
- Acceptance: TestRunner and the five mods (and their test projects) build with zero errors and no remaining CS0618 obsolete warnings for those APIs; show the Timelapser patch's target resolves against the U60 DLLs. Your report must include the actual command output — do not claim success without it.

## Context the packet does not carry
- Decision (revised by Jeremy mid-task): fixes that can work on both the live U59 build (744825) and U60 ship now; only changes that need U60-only API wait on the held branch. So write every fix here to work on both builds where that's possible (e.g. resolve a patch target by name, pick whichever method exists, avoid U60-only types), and say per change in the report whether it is BOTH-BUILDS or U60-ONLY. Verify both-builds claims against the frozen U59 DLLs at F:\ONI-Frozen\APP as well as the U60 ones. Don't raise minimumSupportedBuild for a both-builds change.
- Unit 01 report: .context/tasks/OM-012-u60-readiness/units/01-explore-u60-breakage.md (common/ and obsolete-warnings sections).
- Game DLLs: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (via common/oni-mod.props). Scratchpad with unit 01's PatchCheck host: C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad. Write scripts with the Write tool.
- Changing the obsolete overloads changes event subscription semantics if the new API differs (e.g. handler signatures, returned handles that must be kept for unsubscribe) — read the new overloads' implementations before converting, and say in the report where behaviour could differ.
- Build only your projects with `-p:ModDeployFolder=none`; other units edit salty-surf, upgrade-planner, corner-build-anything and ovagros-overhead concurrently.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/04-implement-report.md
- Must contain: files changed with why; build output; patch resolution evidence; behaviour differences to check in game.
- Model tier: default
