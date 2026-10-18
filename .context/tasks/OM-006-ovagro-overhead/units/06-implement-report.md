# Unit 06 report: S2, hidden ceiling plant and CeilingForm

Commit `fbd4703` on OM-006-ovagro-overhead, message `OM-006: hidden ceiling Ovagro plant` with both trailers. `git status --short` was empty after it (this report is written after the commit and is not committed). The game was not launched.

## Diff summary

Created, `mods/ovagro-overhead/src/OvagroOverhead/` (2 files, 219 insertions):
- `VineMotherCeilingConfig.cs`: `sealed class VineMotherCeilingConfig : IEntityConfig, IHasDlcRestrictions`, required DLC `DlcManager.DLC4`, forbidden null.
  - Plant `VineMotherCeiling`: `CreatePlacedEntity` with the vanilla arguments (mass 2, `vine_mother_kanim`, initial anim "object", BuildingFront, 1x2, `DECOR.BONUS.TIER1`, Creature, 308.15 K) and the vanilla name/description `STRINGS.CREATURES.SPECIES.VINEMOTHER.*`; tags `Hanging`, `HideFromSpawnTool`, `HideFromCodex` passed as `additionalTags` (VineBranchConfig's way).
  - `MakeHangingOffsets(plant, 1, 2)` before `ExtendEntityToBasicPlant` (SaltPlantConfig's order); `ExtendEntityToBasicPlant` with vanilla `TEMPERATURE_*` and `ALLOWED_ELEMENTS` constants and the vanilla flags, base trait id `VineMotherCeilingOriginal`; `UprootedMonitor.monitorCells = {(0,1)}`.
  - Wilt delays 0, `RegisterWithOverlay(HarvestableIDs, "VineMotherCeiling")`, `Traits` + `initialTraits.Add(baseTraitId)` (as vanilla), `VineMother.Def` (`VineBranch`, 24), `HarvestDesignatable`, `ExtendPlantToIrrigated` (Water, `IRRIGATION_RATE` 0.15).
  - `SeedProducer.Configure("VineMotherSeed", Hidden)`, `CodexEntryRedirector.CodexID = "VineMother"`.
  - Preview `VineMotherCeiling_preview`: `CreateAndRegisterPreview(id, vine_mother_kanim, "place", ObjectLayer.Building, 1, 2)` + `MakeHangingOffsets(preview, 1, 2)`.
  - Art on plant and preview (`HangFromCeiling`): `Rotation = 180`, `FlipX = true`, `Pivot = (0, 0.5, 0)`.
  - Neither `CreateAndRegisterSeedForPlant` nor `CreateAndRegisterPreviewForPlant` is called.
- `CeilingForm.cs`: static class with
  - `PlantId` / `PreviewId` constants and `PlantTag`, `PreviewTag`, `VanillaPlantTag`, `SeedTag` (`TagManager.Create`), for patches and tests to share.
  - `struct Swap { Active, Direction, PlantID, PreviewID }`; `Enter(seed)` saves the three fields and sets Bottom / `VineMotherCeiling` / `VineMotherCeiling_preview`; `Exit(seed, swap)` restores, no-op when `!Active` (so a finalizer can always call it with the `__state` default).
  - `IsOvagro(seed)` (PlantID `VineMother` and direction Top), `WantsCeiling(cell)` (`Grid.Solid[CellAbove(cell)]`, both cells validity-checked), `FacesDown(plot)` (`rotatable != null && Direction == Bottom`).

No Harmony patches, no strings, no `ModStrings`, no csproj change.

## Build output (verbatim tail)

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none --no-incremental` (exit 0):

```
    291 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.53
```
```
  OvagroOverhead -> C:\dev\ONI-Mods\mods\ovagro-overhead\src\OvagroOverhead\bin\Release\OvagroOverhead.dll
  OvagroOverhead.Tests -> C:\dev\ONI-Mods\mods\ovagro-overhead\src\OvagroOverhead.Tests\bin\Release\OvagroOverhead.Tests.dll
```

The new code adds no warnings: 291 is the same total S1 recorded, and `grep -i ovagro <log> | grep -ic warning` gives 0. The plain (incremental) form of the acceptance command also exited 0 (5 warnings, all in other mods' test projects).

Also run: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead -BuildOnly` exit 0:
```
Checking OvagroOverhead.Tests.dll against the game assemblies
Built and checked against the game assemblies (-BuildOnly: no game session).
```

## Entity loader and DLC gating

- Discovery: the class is public, non-abstract and implements `IEntityConfig`, so `EntityConfigManager.LoadGeneratedEntities` (EntityConfigManager.cs 42-81) picks it up from the type scan.
- Gating: `GetDlcIds()` returns null, so the loader takes the `IHasDlcRestrictions` branch, reads `DLC4` / null and registers only when `IsCorrectDlcSubscribed` passes, the same path as the vanilla `VineMotherConfig`. `RegisterEntity` stamps those ids on the plant's `KPrefabID`. The preview is registered inside `CreatePrefab` (as vanilla's is), so it exists only when the plant does.

## Departures from the design, and what the game code contradicted

1. **Two extra interface methods (compiler forced).** The design names `IEntityConfig, IHasDlcRestrictions` only. The first build failed with CS8702: `IEntityConfig.GetDlcIds()` and `IHasDlcRestrictions.GetAnyRequiredDlcIds()` have default bodies in the game's assemblies, and a net48 target cannot use default interface implementations. Both are written out returning null, the defaults' values. `GetDlcIds` must stay null; a non-null value would send the loader down the obsolete branch with a `DevLogError`. Any later entity config in a net48 mod needs the same two methods.
2. **`Db.Get().traits.Get(text)` omitted.** Vanilla line 53 is a lookup whose result is discarded; it does nothing.
3. **Vanilla double-adds its base trait, kept.** With plant mutations enabled (Spaced Out), `ExtendEntityToBasicPlant` already puts the base trait in `initialTraits` (its MaxRadiationThreshold modifier makes `SelfModifiers` non-empty), and `VineMotherConfig` adds it again (line 54). The ceiling config does the same, for parity with the vanilla prefab that T1 compares against. A comment marks it.
4. **`SeedTag`, `VanillaPlantTag` and the `Swap.Active` flag** are additions inside the helper the plan left open ("a small state struct"). `Active` lets a finalizer call `Exit` unconditionally with the default `__state`.
5. Nothing else in the design was contradicted. Checked against the decomp: `KAnimControllerBase.Rotation`/`FlipX`/`Pivot` setters only call `layering?.Dirty()` and `SetDirty()`, which is a no-op while `batch` is null, so setting them on a prefab is safe; `GetTransformMatrix` applies the rotation about `Pivot` in world units after scaling, so (0, 0.5) is the centre of the origin cell as the design says. `PlantableSeed.direction`/`PlantID`/`PreviewID` and `SingleEntityReceptacle.rotatable` are public. The only game code naming the `"VineMother"` id is `VineBranchConfig` (codex redirect) and `VineMotherConfig` itself, so no other system keys on the vanilla id.

## Store reads

Runs (cwd `C:/dev/ONI-Mods`, `node c:/dev/ai/icon/bin/ks.mjs`):
1. `read blk-5b62e7507604 blk-76e79f89b3c4 blk-7d0c65d332f3 blk-bcc50465e110` (4 rows, 4116 B): just-fall-already's imports example, variable naming, the FallPoke entity, Harmony patch conventions.
2. `tags --store C:/dev/ONI-Mods`: looked for an entity/prefab/DLC subject term; there is none.
3. `brief --at C:/dev/ONI-Mods --for mods/ovagro-overhead/src/OvagroOverhead --budget 6000`: nothing matched (the new node's three records declare no coverage or tags).
4. `map mods/just-fall-already/.context/standards/code-style.md` refused (no positional argument); then `map --document mods/just-fall-already/.context/standards/code-style.md` (22 records).
5. `read "mods/just-fall-already:standards/code-style.md#formatting" "...#language-and-toolchain" "...#comments" "...#game-components" "...#control-structures/strings"` (5 rows, 4457 B).

Applied: tabs/Allman/one-statement bodies without braces, trailing commas in multi-line initialisers, LF in the repo (autocrlf), C# 9 namespace blocks, `<summary>` saying what the game does and what the code changes, no string interpolation, camelCase locals / PascalCase constants, imports order. `blk-bcc50465e110` (patch conventions) shaped the `Swap`/`Exit` shape for S3/S4's prefix + finalizer use.

Read as files: handoff-6.md, units/06-implement.md, units/03-design-report.md, units/04-plan-report.md, units/05-implement-report.md, the scaffold's `Mod.cs` and csproj, `mods/just-fall-already/src/JustFallAlready/FallPoke.cs` (first 60 lines), `common/oni-mod.props`; decomp `VineMotherConfig`, `SaltPlantConfig`, `VineBranchConfig`, `EntityTemplates` (170-330, 885-1030), `EntityConfigManager` (30-140), `IEntityConfig`, `CodexEntryRedirector`, `KAnimControllerBase`, `KBatchedAnimController` (transform matrix, `SetDirty`), `PlantableSeed` (fields), `SingleEntityReceptacle`/`Grid` (members); fp `IHasDlcRestrictions`, `DlcManager`.

Packet report-back:
- **Did not apply:** all of the ~45 other mods' "Example"/"Loops"/"Variables" copies; `blk-7d0c65d332f3` (FallPoke) said nothing about entity configs.
- **Missed:** the store has no record on entity configs (`IEntityConfig`, `IHasDlcRestrictions`) or on net48's lack of default interface implementations; the CS8702 finding is worth a record in the new node. The formatting, toolchain and comments records that did govern this code were not in the packet (only "Example", "Variables" and "Loops" were); I reached them by `ks map --document`.
- **Reason lines:** accurate but uninformative ("names FallPoke.cs").

## ICON friction

- `ks map <document path>` refused with "takes no positional arguments"; the error named `--document` and `--applies-to`, and the second call worked. A positional path on `map` was the obvious first spelling.
- The packet's path brief again spent its budget on per-mod copies of one standard and left out the template's formatting/comments/toolchain sections; the brief over the new mod's own path returned nothing because the new node's records carry no coverage.
- Not ICON: CS8702 from the game's default interface methods (project toolchain).
