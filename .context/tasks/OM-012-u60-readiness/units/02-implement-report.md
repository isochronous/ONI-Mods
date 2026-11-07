# Unit 02 report: Salty Surf, Upgrade Planner, Corner Build Anything on U60

All three mods and their test projects build with 0 errors. Every change is BOTH-BUILDS (U59 744825 and U60 762230). No `minimumSupportedBuild` change, no version bump, no commit, game not launched.

## Files changed

| File | Why | Builds |
|---|---|---|
| `mods/salty-surf/src/SaltySurf/Mod.cs` | The one patch on the explicit `IGameObjectEffectDescriptor.GetDescriptors` is split in two classes, each with `Prepare()`/`TargetMethod()` that only apply when their method exists: `MechanicalSurfboard_GetDescriptors_Patch` (explicit method, U59) and new `MechanicalSurfboard_CollectDescriptors_Patch` (public `CollectDescriptors(GameObject, List<Descriptor>)`, U60). Both call new `SurfDescriptors.Rewrite(list)`. | BOTH |
| `mods/upgrade-planner/src/UpgradePlanner/Patches.cs` | `[HarmonyPatch(typeof(MaterialSelector), "OnSelectMaterial")]` by name (public on U59, private on U60). | BOTH |
| `mods/upgrade-planner/src/UpgradePlanner/UI/MaterialPicker.cs` | (a) preselect calls `OnSelectMaterial` through a cached `AccessTools.MethodDelegate` instead of the compile-time call; I did not use U60's `TrySelectMaterial` because it skips the pick when stock is short, and preselect must pick regardless. (b) A second break that unit 01 had hidden behind the first error: `MaterialSelectionPanel.ConfigureScreen`'s first callback returns `bool` on U59 but `MaterialSelectionPanel.BuildableState` on U60 (the PlanScreen answer is `IsDefBuildable` on U59, `IsDefBuildableForMaterialSelectionPanel` on U60). New `Configure(...)` reads the parameter types from `ConfigureScreen` and builds the delegates to match, with a small generic `Always<T>` holder for the "always allowed" case and `NoTooltip` for the empty tooltip. | BOTH |
| `mods/corner-build-anything/src/CornerBuildAnything.Tests/Reach.cs` | Four `CellOffset[] x = constructable.GetOffsets(..)` lines (99, 132, 165, 202) get `.ToArray()`; works for the array (U59) and the `IReadOnlyList` (U60). | BOTH |

Salty Surf description text: `Rewrite` finds the Water line by the template `WATER_REQUIREMENT` ("{element}: {amount}") with the water name filled in, matching head and tail around `{amount}` and keeping whatever amount string the game wrote. So it no longer depends on `GameUtil.GetFormattedMass` equalling U60's `MassFormatter`. It then writes the mod's wording (element list) into text and tooltip, as before. In U60 the descriptor comes from a memoizer as a struct; the list entry is replaced, the memoized original is untouched.

Not changed: `SaltySurf.Tests/Intake.cs:48` still calls the obsolete `GameUtil.GetAllDescriptors` (warning CS0618, exists on both builds and works).

## Build output

```
mods/salty-surf: dotnet build src/SaltySurf/SaltySurf.csproj -c Release -p:ModDeployFolder=none      -> Build succeeded. 0 Warning(s) 0 Error(s)
                 dotnet build src/SaltySurf.Tests/...                                                -> Build succeeded. 2 Warning(s) (CS0618 GetAllDescriptors, FindObjectsOfType) 0 Error(s)
mods/upgrade-planner: UpgradePlanner.csproj       -> 0 Warning(s) 0 Error(s), Merge succeeded, Build succeeded.
                      UpgradePlanner.Tests.csproj -> 0 Warning(s) 0 Error(s), Merge succeeded, Build succeeded.
mods/corner-build-anything: CornerBuildAnything.csproj       -> 0 Error(s), Build succeeded.
                            CornerBuildAnything.Tests.csproj -> 0 Error(s), Build succeeded. (1 pre-existing FindObjectsOfType CS0618)
```
(All built against the U60 DLLs at F:\SteamLibrary\...; all with `-c Release -p:ModDeployFolder=none`, one project at a time.)

## Patch-resolution evidence (scratch PatchCheck, `...\scratchpad\PatchCheck\out\PatchCheck.exe patch <Managed> <res> <dll>`)

Salty Surf, U60 (762230) then U59 (F:\ONI-Frozen\APP):
```
U60  SaltySurf.MechanicalSurfboard_CollectDescriptors_Patch  OK -> MechanicalSurfboard.CollectDescriptors(GameObject, List`1)
     SaltySurf.MechanicalSurfboard_GetDescriptors_Patch      SKIPPED (Prepare false)
     -- 6 patch classes, 5 originals patched   TOTAL FAILURES 0
U59  SaltySurf.MechanicalSurfboard_GetDescriptors_Patch      OK -> MechanicalSurfboard.IGameObjectEffectDescriptor.GetDescriptors(GameObject)
     SaltySurf.MechanicalSurfboard_CollectDescriptors_Patch  SKIPPED (Prepare false)
     -- 6 patch classes, 5 originals patched   TOTAL FAILURES 0
```
(Other four patch classes OK on both.)

Upgrade Planner, both builds identical:
```
UpgradePlanner.Patches+MaterialSelector_OnSelectMaterial_Patch  OK -> MaterialSelector.OnSelectMaterial(Tag, Recipe, Boolean)
-- 6 patch classes, 6 originals patched   TOTAL FAILURES 0
```

Corner Build Anything: no mod change; unit 01 showed both transpilers apply on U60.

Compat checks done by reading decompiled DLLs (ilspycmd) of both builds: `OnSelectMaterial(Tag, Recipe, bool)` same signature on both; `ConfigureScreen(Recipe, GetBuildableStateDelegate, GetBuildableTooltipDelegate)` on both; `PlanScreen.GetTooltipForBuildable(BuildingDef)` on both; `BuildableState.Allowed` exists on U60.

## Upgrade Planner's PLib

`lib/PLib/PLib.dll` is PLib 4.25.0 plus the PDialog.Draggable commit (README). Unit 01 checked PLib 4.25.0's 24 game targets against U60: all resolve except `EquipmentTemplates.CreateEquipmentDef` (2 overloads now), used only by `PBuildingManager`, which this mod does not use. The modified copy's own patches were not separately run through PatchCheck (Draggable adds no game patch).

## Needs an in-game check

- The `Configure` delegate construction in `MaterialPicker.Attach` and the preselect pick are reflection and are not covered by PatchCheck or the build: run the Upgrade Planner tests (change-material tab opens, preselect works, building picker greys unresearched/unaffordable correctly) on U60 and, ideally, once on U59.
- Salty Surf info panel on U60: the Water line should read "Water or Salt Water: <amount>" and the Spillage line stay (test `Descriptors` in SaltySurf.Tests covers it; it uses the obsolete `GetAllDescriptors`).
- Unit 01's note stands: `Reconstructable.TryCommenceReconstruct` now schedules on the next game tick (Upgrade Planner `Orders.FollowRebuild`); test paused and unpaused.

## Friction (for whoever records it)

- `ks read` pointers in the handoff were all generic error-handling standards copied per mod; none needed opening for this work, so none were read. Nothing governing the work was missing from the brief.
- Repo files are LF in git but CRLF in the working copy for some files; Python edits with `newline=''` produced mixed endings, fixed by normalising to LF (git warns about LF-to-CRLF on touch, as it does for other files).
