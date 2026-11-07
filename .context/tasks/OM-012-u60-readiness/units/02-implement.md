# Unit 02 — implement: Salty Surf, Upgrade Planner, Corner Build Anything tests on U60

## Objective
Plan step "Fix per mod" of OM-012. Make these build and patch correctly against the U60 testing branch (build 762230).

## Scope
- IN:
  - mods/salty-surf: `MechanicalSurfboard` now implements descriptors through `CollectDescriptors(GameObject, List<Descriptor>)`; the patch targeting the explicit `IGameObjectEffectDescriptor.GetDescriptors` gets a null TargetMethod (Mod.cs:121-145). Retarget it. Check the produced description text still makes sense with vanilla's new text.
  - mods/upgrade-planner: `MaterialSelector.OnSelectMaterial` is now private (Patches.cs:77); MaterialPicker.cs:71 is the expected next error. Fix until the mod and its test project build. Its own copy of PLib, if it has one: confirm it resolves on U60.
  - mods/corner-build-anything tests: `Constructable.GetOffsets` returns `IReadOnlyList<CellOffset>` (Reach.cs:99,132,165,202).
  - `minimumSupportedBuild` stays unless a change can only be U60-only.
- OUT: other mods; common/; launching the game; commits; version bumps / CHANGELOG (done at release).
- Acceptance: `dotnet build` of each touched mod's project and test project succeeds with zero errors; for the retargeted patches, show they resolve against the U60 DLLs (the scratch PatchCheck host from unit 01 at the scratchpad path below can do this). Your report must include the actual command output — do not claim success without it.

## Context the packet does not carry
- Decision (revised by Jeremy mid-task): fixes that can work on both the live U59 build (744825) and U60 ship now; only changes that need U60-only API wait on the held branch. So write every fix here to work on both builds where that's possible (e.g. resolve a patch target by name, pick whichever method exists, avoid U60-only types), and say per change in the report whether it is BOTH-BUILDS or U60-ONLY. Verify both-builds claims against the frozen U59 DLLs at F:\ONI-Frozen\APP as well as the U60 ones. Don't raise minimumSupportedBuild for a both-builds change.
- Unit 01 report: .context/tasks/OM-012-u60-readiness/units/01-explore-u60-breakage.md (read its sections for your three mods only).
- Game DLLs: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (via common/oni-mod.props). Frozen U59 copy for comparison: F:\ONI-Frozen\APP.
- Scratchpad (scratch scripts, PatchCheck host): C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad. Write scripts with the Write tool; no long Bash heredocs.
- Build per project with `-p:ModDeployFolder=none`; other units are editing other mods in the same tree at the same time, so build only your projects, not the whole solution.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/02-implement-report.md
- Must contain: files changed with a one-line why each; build output; patch-resolution evidence; anything that needs an in-game check.
- Model tier: default
