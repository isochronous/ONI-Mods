# Unit 07 report: S3 + S4, Harmony patches #1-#9

Commit `b79be6a` on OM-006-ovagro-overhead, message `OM-006: route Ovagro seeds to the ceiling form` with both trailers. `git status --short` was empty after it (this report is written after the commit and is not committed). The game was not launched.

## Diff summary

`mods/ovagro-overhead/src/OvagroOverhead/`, 6 files, 236 insertions, 1 deletion:
- **`PlantingPatches.cs`** (new): #1, #2, #3. Each one is a prefix plus a finalizer. The prefix enters the swap when `IsOvagro(seed) && WantsCeiling(cell)` and stores the `CeilingForm.Swap` in `__state`. The finalizer calls `CeilingForm.Exit(seed, __state)`. Where the cell comes from:
  - #1: the `plant_cell` argument.
  - #2: the `cell` argument.
  - #3: `Grid.PosToCell(__instance)`.
- **`BranchPatch.cs`** (new): #9. A postfix. When `mother.gameObject.PrefabID() == CeilingForm.PlantTag`, it calls `__instance.sm.RootShape.Set((int)VineBranch.Shape.Top, __instance)`.
- **`PlotPatches.cs`** (new): #4, #5, #8.
  - #4 swaps the deposited seed's own `PlantableSeed` when `FacesDown(plot)`.
  - #5 swaps the seed prefab when `entityTag == SeedTag && FacesDown(plot)`.
  - #8 swaps the seed prefab when `FacesDown(plot)`.
- **`SideScreenPatches.cs`** (new): #6, #7. They swap the seed prefab when the side screen's target is a downward plot. #6 reads its `target` argument. #7 reads the protected `targetReceptacle` field through Harmony's `___targetReceptacle` injection.
- **`CeilingForm.cs`** (modified): adds `EnterPrefab()` / `ExitPrefab(Swap)` and a private `PrefabSeed()` (`Assets.TryGetPrefab(SeedTag)`), plus `using UnityEngine`. `EnterPrefab` swaps only when the prefab is still in its vanilla form. So when one swapped call is nested in another, the inner call does nothing and the outer finalizer restores the prefab:
  - #8 → `CreateOrder` → #5.
  - #6 → `ToggleClicked` → #7.
- **`Mod.cs`** (modified): `base.OnLoad(harmony)` is wrapped so patching is all or nothing. On an exception it calls `harmony.UnpatchAll(harmony.Id)` and logs `[OvagroOverhead] Could not patch the game, Ovagro stays floor-only: <message>`. See departure 1.

No player-visible strings, no `ModStrings`, no csproj change.

## Targets and the signatures they bind

The binding was checked against the real game assemblies, not just compiled. A scratch script (`scratchpad\resolve-targets.ps1`) loads the game's `0Harmony.dll` and `Assembly-CSharp.dll` and the built mod DLL. It reads each `[HarmonyPatch]` attribute and resolves the target with `AccessTools.DeclaredMethod(type, name, argTypes)`, which is the call Harmony's `PatchTools.GetOriginalMethod` makes for a normal method (read in the decompiled 0Harmony). `Out` variations are turned into by-ref types. The output, verbatim:

```
OK   VineBranch_SetupRootInformation_Patch -> public Void VineBranch+Instance.SetupRootInformation(VineMother+Instance mother)
OK   PlantableCellQuery_CheckValidPlotCell_Patch -> private Boolean PlantableCellQuery.CheckValidPlotCell(PlantableSeed seed, System.Int32 plant_cell)
OK   SeedPlantingStates_CheckValidPlotCell_Patch -> private static Boolean SeedPlantingStates.CheckValidPlotCell(SeedPlantingStates+Instance smi, PlantableSeed seed, System.Int32 cell, PlantablePlot& plot)
OK   PlantableSeed_TryPlant_Patch -> public Void PlantableSeed.TryPlant(System.Boolean allow_plant_from_storage)
OK   PlantablePlot_SpawnOccupyingObject_Patch -> protected override GameObject PlantablePlot.SpawnOccupyingObject(UnityEngine.GameObject depositedEntity)
OK   PlantablePlot_SetPreview_Patch -> public override Void PlantablePlot.SetPreview(Tag entityTag, System.Boolean solid)
OK   PlantablePlot_OnCopySettings_Patch -> private Void PlantablePlot.OnCopySettings(System.Object data)
OK   ReceptacleSideScreen_Initialize_Patch -> public Void ReceptacleSideScreen.Initialize(SingleEntityReceptacle target)
OK   PlanterSideScreen_SetResultDescriptions_Patch -> protected override Void PlanterSideScreen.SetResultDescriptions(UnityEngine.GameObject seed_or_plant)
0 failure(s)
```

| # | Attribute | Binds |
|---|---|---|
| 1 | `(typeof(PlantableCellQuery), "CheckValidPlotCell", new[] { typeof(PlantableSeed), typeof(int) })` | private `bool CheckValidPlotCell(PlantableSeed seed, int plant_cell)` |
| 2 | `(typeof(SeedPlantingStates), "CheckValidPlotCell", new[] { Instance, PlantableSeed, int, PlantablePlot }, new[] { Normal, Normal, Normal, Out })` | private static `bool CheckValidPlotCell(SeedPlantingStates.Instance smi, PlantableSeed seed, int cell, out PlantablePlot plot)` |
| 3 | `(typeof(PlantableSeed), nameof(TryPlant), new[] { typeof(bool) })` | public `void TryPlant(bool allow_plant_from_storage = false)` |
| 4 | `(typeof(PlantablePlot), "SpawnOccupyingObject", new[] { typeof(GameObject) })` | protected override `GameObject SpawnOccupyingObject(GameObject depositedEntity)` |
| 5 | `(typeof(PlantablePlot), nameof(SetPreview), new[] { typeof(Tag), typeof(bool) })` | public override `void SetPreview(Tag entityTag, bool solid = false)` |
| 6 | `(typeof(ReceptacleSideScreen), nameof(Initialize), new[] { typeof(SingleEntityReceptacle) })` | public `void Initialize(SingleEntityReceptacle target)` |
| 7 | `(typeof(PlanterSideScreen), "SetResultDescriptions", new[] { typeof(GameObject) })` | protected override `void SetResultDescriptions(GameObject seed_or_plant)` |
| 8 | `(typeof(PlantablePlot), "OnCopySettings", new[] { typeof(object) })` | private `void OnCopySettings(object data)` |
| 9 | `(typeof(VineBranch.Instance), nameof(SetupRootInformation), new[] { typeof(VineMother.Instance) })` | public `void SetupRootInformation(VineMother.Instance mother)`, the first of the two overloads, on the nested type |

Every target lists its argument types, so none depends on there being a single overload. The patch parameters use the originals' names: `seed`, `plant_cell`, `cell`, `depositedEntity`, `entityTag`, `target` and `mother`. #7's `___targetReceptacle` resolves through `AccessTools.Field`, which walks base types; the field is declared `protected` on `ReceptacleSideScreen` (line 107).

## Every swap is restored on every path

- The game's Harmony is read from its decompiled `MethodCreator`. It declares each patch class's `__state` local once, for prefix, postfix and finalizer alike, and initialises it before the try block. Prefixes run inside the try.
- On a normal return, finalizers run once (`AddFinalizers(catchExceptions: false)`). On an exception they run in the catch block, unless they already ran. A void finalizer leaves the exception as it was.
- So `Exit` runs exactly once on both paths. A swap that was never entered is the default `Swap` (`Active == false`), and `Exit` ignores it.
- Swaps are synchronous. Each one is entered and exited within one game call on the main thread.
- #3 and stack splitting: `TryPlant` calls `pickupable.TakeUnit(1f)`. When the stack holds more than one seed, `EntitySplitter.Split` instantiates the new unit from `Assets.GetPrefab(...)`, not from the swapped live object (EntitySplitter.cs 127-143). So no copy of a swapped seed escapes the call.

## Build output (verbatim tail)

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` (exit 0):

```
  OvagroOverhead -> C:\dev\ONI-Mods\mods\ovagro-overhead\src\OvagroOverhead\bin\Release\OvagroOverhead.dll
  OvagroOverhead.Tests -> C:\dev\ONI-Mods\mods\ovagro-overhead\src\OvagroOverhead.Tests\bin\Release\OvagroOverhead.Tests.dll
C:\dev\ONI-Mods\mods\storage-pod-redux\src\StoragePodRedux.Tests\Decisions.cs(70,53): warning CS0618: 'DlcManager.GetHighestActiveDlcId()' is obsolete: 'This should no longer be used.' [C:\dev\ONI-Mods\mods\storage-pod-redux\src\StoragePodRedux.Tests\StoragePodRedux.Tests.csproj]
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.26
```

The 5 warnings are all in other mods' test projects, the same as S2 recorded for the incremental form. Counting warning lines that mention `ovagro` in the full log gives 0. On the first, non-incremental pass the total was 288, with no Ovagro lines.

ModCheck against the mod DLL, `dotnet run --project common/tools/ModCheck -- <Managed> mods/ovagro-overhead/src/OvagroOverhead/bin/Release/OvagroOverhead.dll`: every type `OK`, `ALL CHECKS PASSED`.

`Invoke-OniTests.ps1 -Mods OvagroOverhead -BuildOnly`: exit 0, with "Built and checked against the game assemblies (-BuildOnly: no game session)."

## Departures from the design, and why

1. **Patching is all or nothing (`Mod.cs`).** The design asks that the patch class log when its target is missing (Risk 5). With attribute patches, a missing target makes `PatchAll` throw part-way through. `DLLLoader.LoadDLLs` (KMod/DLLLoader.cs ~118-133) then logs the exception, but it leaves applied whatever was patched before the failure. The ceiling prefab is registered regardless.
   - A half-patched mod is worse than none. For example, #5 could apply without #4, giving a hanging preview over a plot that still delivers the floor plant.
   - So `OnLoad` catches the exception, unpatches this mod's Harmony id and logs one warning naming the failure. This keeps the vanilla behaviour, as error-handling principle 1 (`blk-f75b46be2845`) requires.
   - It is a fifth `try/catch` situation beyond the four the standard lists (`blk-5938243ee768`). The manager may want it recorded as an allowed case.
2. **The prefab-swap helpers live in `CeilingForm.cs`, not `PlotPatches.cs`.** The plan put them in `PlotPatches.cs` only to avoid a merge conflict while S3 and S4 ran in parallel. Here they ran in one unit, so the helpers sit next to `Enter`/`Exit`.
3. **#8 swaps whenever the plot faces down.** It does not first work out which seed is being copied. Inside `OnCopySettings`, only the direction check (line 166) and the nested `SetPreview` read the Ovagro prefab, so swapping it while another seed is copied changes nothing. Recomputing the copied tag would duplicate lines 150-161.
4. **#7 uses `___targetReceptacle`, not an `AccessTools.FieldRefAccess` field.** My first version used `FieldRefAccess`, and ModCheck failed it ("FieldRefAccess<ReceptacleSideScreen, SingleEntityReceptacle> for targetReceptacle caused an exception"). The same check also fails the shipped detect-any-element and bitshifter DLLs for their `FieldRefAccess` fields, so it is a ModCheck host artifact (CoreCLR), not a game problem. The injected parameter is simpler and is one of the forms the patch convention (`blk-bcc50465e110`) lists.

## What the game code (or tooling) contradicted

- **ModCheck does not check string-named Harmony targets.** The plan's S3 acceptance says "`-BuildOnly` exit 0 (ModCheck confirms the private targets resolve)". That is not true:
  - ModCheck only forces type loads, class constructors and JIT (common/tools/ModCheck/Program.cs).
  - `-BuildOnly` checks only `OvagroOverhead.Tests.dll` ("Checking OvagroOverhead.Tests.dll against the game assemblies"), not the mod DLL.
  - So target resolution was proved by the scratch resolve script above, and the mod DLL was ModChecked by hand.
- **ModCheck fails every mod that uses `AccessTools.FieldRefAccess`** (detect-any-element 2 failures, bitshifter 2 failures). That is a false positive worth a note in ModCheck's record or code.
- **Nothing contradicted the design's reading of the game.** Re-read and confirmed in the decomp:
  - `TryPlant`'s only caller is `SeedPlantingStates.PlantComplete` (line 186).
  - `ReceptacleSideScreen.Initialize` stores each prefab's direction into `SelectableEntity` (line 257). Every later toggle check (`CanDepositEntity`, line 609) reads that stored value, so a swap during `Initialize` is enough.
  - `PlanterSideScreen.SetResultDescriptions` reads `component.direction` and `PlantID` (320-336), and `VineMother` has no `MutantPlant`: `crop_id` is null, and `MutantPlant` is only added on the crop path (EntityTemplates.cs 280-283). So the swapped `PlantID` changes no subspecies lookup.
  - Inside the #6 swap, `GetEntityName` and `GetEntityIcon` read the swapped `PlantID`. They resolve to the ceiling prefab, which has the same name strings and the same kanim, so the toggle looks identical.
  - Inside #2, `ForceDeposit` runs after the swap has ended, so the downward-plot Pip path is routed by #4 alone.

## Not done here

In-game proof is S4a-S9 (the game was not launched, per the spec).

## Store reads

Runs (cwd `C:/dev/ONI-Mods`, `node c:/dev/ai/icon/bin/ks.mjs`):
1. `read blk-5938243ee768 blk-f04d3902a855 blk-5dac2e3d8156 blk-1de4d1f2707d blk-bcc50465e110` (5 rows, 5894 B). One copy of each distinct packet pointer (the try/catch, transpiler-count and conditionals copies are identical across mods), plus the patch conventions.
2. `map --document mods/just-fall-already/.context/standards/code-style.md` (22 records).
3. `read` on `code-style.md#formatting`, `#language-and-toolchain`, `#comments`, `#imports-includes`, `#control-structures/strings` and `#project-specific-rules` (6 rows, 6213 B).
4. `map --document mods/just-fall-already/.context/standards/error-handling.md` (23 records).
5. `read` on `error-handling.md#principles`, `#guarding-patches/missing-member-warn-leave-vanilla`, `#guarding-patches/null-checks-on-game-objects`, `#anti-patterns-to-avoid` and `#logging` (5 rows, 3976 B).

What was applied:
- `blk-bcc50465e110`: string-named targets for non-public methods, `__instance`/`___field` injection, prefixes that never skip the original.
- `blk-f75b46be2845`: "never break the game; leave vanilla in place and say so", which led to departure 1.
- `blk-5938243ee768` and `blk-f04d3902a855`: the message form `"[Name] what was skipped: " + e.GetBaseException().Message`.
- `blk-ca29ac4a9ee9`: no throwing from a patch, so prefixes and finalizers null-guard (`FacesDown(null)`, `IsOvagro(null)`, `depositedEntity == null`).
- `blk-1de4d1f2707d`: guard clauses.
- Formatting and toolchain: tabs, Allman, one-statement bodies without braces, C# 9 namespace blocks, `System.*` imports first, a `<summary>` saying what the game does and what each patch changes, no string interpolation.

Read as files, not through the store: handoff-7.md, units/03-design-report.md, units/04-plan-report.md (S3, S4, section 5), units/06-implement-report.md, the mod's `Mod.cs`, `CeilingForm.cs` and csproj, `mods/vent-freeze-fix/.../DebugSave.cs` (70-120), `mods/natural-backwalls-everywhere/.../WorldgenThreadPatch.cs`, `common/oni-mod.props` (game folder), `common/tools/ModCheck/Program.cs`.
- Decomp: `PlantableCellQuery`, `PlantableSeed`, `SeedPlantingStates` (165-250), `Pickupable.Take`, `EntitySplitter.Split`, `SingleEntityReceptacle` (deposit path), `PlantablePlot` (130-440), `ReceptacleSideScreen` (161-290, 666-672), `PlanterSideScreen`, `PlantSubSpeciesCatalog`, `EntityTemplates` (270-290), `VineBranch` (Shape, `SetupRootInformation` 455-485, parameters), `StateMachine` (`sm`), `Assets.GetPrefab`/`TryGetPrefab`, `KMod/UserMod2`, `KMod/DLLLoader`.
- Decompiled 0Harmony with ilspycmd: `MethodCreator`, `MethodCreatorTools`, `PatchTools`.

Packet report-back:
- **Did not apply:**
  - the 15 per-mod copies of the try/catch record and the 15 of "Transpiler: count the match". There are no transpilers here, and one copy of the try/catch record was enough.
  - the "Conditionals" copies beyond one.
- **Missed:** the records that governed this work most were not in the packet. I reached them by `ks map --document`:
  - `blk-bcc50465e110` (Harmony patch conventions)
  - `blk-f75b46be2845` (principles)
  - `blk-249b3eb5bb20` (missing member: leave vanilla)
  - `blk-ca29ac4a9ee9` (throwing from a patch)
  - just-fall-already's formatting and comment records

  The store also has no record that ModCheck does not check Harmony targets, or that it false-fails `FieldRefAccess`.
- **Reason lines:** accurate, but they only say "names FeaturePatch.cs" or "names SettleSimPatch.cs", which says nothing about why a record bears on a Harmony prefix/finalizer task.

## ICON friction

- **The packet's path brief was 8 KB of three records copied across about 15 mods.** That was the try/catch record, "Transpiler: count the match" and "Conditionals". The patch-convention and principles records that governed this work were not in it. The truncation notice suggests `ks brief ... --budget 30555`, which would list more of the same. I used `ks map --document` on the template mod's two standards instead, as the task message directed. This is the same issue units 02, 03, 04 and 06 reported.
- No `ks` refusals this time. Multi-id `ks read` and multi-address `ks read` worked on the first call. `map --document` worked first time, because unit 06's report named the flag.
- Not ICON (repo tooling): ModCheck checks neither Harmony targets nor the mod DLL under `-BuildOnly`, and it false-fails `FieldRefAccess`. Both are noted above.
