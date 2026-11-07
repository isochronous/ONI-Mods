# Handoff — OM-012-u60-readiness

**Scope:** `mods/salty-surf`, `mods/upgrade-planner`, `mods/corner-build-anything`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/02-implement.md`

```
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
blk-b1cf9481a19b · reference
  The `UserMod2` entry point. Calls `base.OnLoad` (which applies all patches) and logs the versio…
  mods/corner-build-anything/.context/domains/entities.md#key-entities/cornerbuildanythingmod · names mods/corner-build-anything/src/CornerBuildAnything/Mod.cs → mods/corner-build-anything
blk-090e1e892c99 · reference
  Transpiler: count the match
  mods/smart-weight-plate/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
blk-0b1f765c56ad · reference
  Transpiler: count the match
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
blk-101f4f9d4623 · reference
  Transpiler: count the match
  mods/corner-build-anything/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
blk-1987e6421398 · reference
  Transpiler: count the match
  mods/no-magic-fridges/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
blk-1b351a36e13a · reference
  The shared instruction rewrite. Counts the swapped loads, logs `corner reach table in place` pe…
  mods/corner-build-anything/.context/domains/entities.md#key-entities/swaptable · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
blk-21e2796e605b · reference
  Transpiler: count the match
  mods/jumper-bridges/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
blk-21f0d61743aa · reference
  The game keeps two tables of offsets that say where a duplicant may stand to work on a building…
  mods/corner-build-anything/.context/domains/entities.md#key-entities/offsetgroups-reach-tables · names mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs → mods/corner-build-anything
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 65 further candidate(s) matched and are not listed above (8104 of 8192 bytes; the whole selection is 24511). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/corner-build-anything mods/salty-surf mods/upgrade-planner --budget 24511

⚠ 3 record(s) in these 3 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/corner-build-anything:tags.md, mods/salty-surf:tags.md, mods/upgrade-planner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
