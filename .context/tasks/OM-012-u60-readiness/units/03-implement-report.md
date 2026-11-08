# Unit 03 report — ovagros-overhead on U60 (build 762230)

Both projects build with zero errors. Nothing launched, committed or versioned.

## Files changed
- `src/OvagrosOverhead/VineBranchCeilingConfig.cs`: the four CS1061 errors and the obsolete-overload warning.
  - `ExtendEntityToBasicPlant(branch, harvest: null, ...)`, with `crop_id` dropped. This is the new overload vanilla calls with `null`.
  - `EntityTemplates.ConfigureCrop(branch, VineBranchConfig.HARVEST)` replaces the `CROP_TYPES.Find` plus `Crop.Configure` lookup. It also registers a `prefabInitFn` that reconfigures the crop on spawn, as vanilla does.
  - `PlantAttributes.YieldAmount` is now `YieldFactor`, with modifier `1f` (was `crop.numProduced`).
- `src/OvagrosOverhead/VineMotherCeilingConfig.cs`:
  - Moved to the new overload (`harvest: null`, no `crop_id`), which removes the CS0618 warning.
  - Added `AddOrGet<InfoDescription>().subtitle = VINEMOTHER.SUBTITLE`, a new U60 line in `VineMotherConfig`.
- `src/OvagrosOverhead/mod_info.yaml`: `minimumSupportedBuild: 762230`.
- `src/OvagrosOverhead.Tests/Registration.cs:68`: `Crop.cropId` is gone, so the test uses `primaryCropId`.
- `src/OvagrosOverhead.Tests/WildPlanting.cs:59`: `PlantableCellQuery.Reset(seed, max_results)` is now `Reset(seed, can_use_backwall, max_results)`. The test passes `false`, which keeps the old behaviour (ceiling cells only).
- Not touched: `publish/content/mod_info.yaml` still says 744825. It is Workshop material, so I left it. It needs updating at release.

## Vanilla U59 to U60 changes (decompiled with ilspycmd, both builds)

| Vanilla change | In the ceiling vine |
|---|---|
| `VineBranchConfig.HARVEST` (new static `Crop.CropVal(VineFruitConfig.ID, 1800f)`) | Reused directly as `VineBranchConfig.HARVEST` |
| `CropVal` is now `products` (a list) plus `cropDuration`, `renewable`; `cropId` and `numProduced` removed; `Crop.primaryCropId` added | The code no longer reads these. Only the test needed `primaryCropId` |
| `ExtendEntityToBasicPlant` gets a `CropVal harvest` parameter in 2nd position; vanilla passes `null` | `harvest: null` |
| `AddOrGet<Crop>().Configure(cropval)` becomes `EntityTemplates.ConfigureCrop(go, HARVEST)` | Same call |
| `PlantAttributes.YieldAmount` becomes `YieldFactor`, modifier `1f` (was `numProduced`, also 1) | `YieldFactor`, `1f` |
| Cosmetic: local variables extracted (`plantFiberProducer`, `component2`, `def`, and so on) | No behaviour change |
| `VineMotherConfig`: `InfoDescription.subtitle = VINEMOTHER.SUBTITLE` | Carried to `VineMotherCeilingConfig` |
| `VineMotherConfig`: seed and preview split into two statements, `Traits` local | Cosmetic, no change |
| `VineBranch`: `AnimSet` reworked (wilt animation names are now arrays of 3, indexed from 0 instead of 1-based; suffix becomes prefix), plus formatting | Not in the config. The ceiling branch uses `vine_kanim` and the vanilla `VineBranch.Def`, so `VineBranch` itself needs no change. The animation names are checked in game (see below) |
| `VineFruitConfig`: no diff | None |

## Build output (`-c Release -p:ModDeployFolder=none --no-incremental`)

```
OvagrosOverhead.csproj:
Build succeeded.
    0 Warning(s)
    0 Error(s)

OvagrosOverhead.Tests.csproj:
  OvagrosOverhead.Tests -> ...\OvagrosOverhead.Tests\bin\Release\OvagrosOverhead.Tests.dll
Build succeeded.
    3 Warning(s)   (all CS0618 in common/tools/TestRunner: Worldgen.cs:259,292 and TestBehaviour.cs:219, Unity FindObjectOfType, already present on U59)
    0 Error(s)
```

Before the fixes the first test-project build failed on WildPlanting.cs(59,68) CS7036. That error was hidden behind the mod's own errors in unit 01.

## Needs an in-game check
- Yield: one Ovagro Fig per harvest from a ceiling vine. `YieldFactor` is now a multiplier (the vanilla value is `1f`); it used to be an amount.
- Growth: the ceiling vine matures in 3 cycles (`GROWING_DURATION_CYCLES`) and fruit takes 1800 s, as vanilla. `ExtendEntityToBasicPlant` with `harvest: null` skips the harvest-driven trait block, so check that the Maturity max still comes from the explicit modifier lines.
- Harvest and seeds: `HarvestDesignatable` and `Harvestable` still work, and `PlantFiberProducer` is unchanged. The Registration test compares crop ids against the vanilla vine.
- Wilt animations, if a ceiling vine wilts: U60 `VineBranch.AnimSet` indexes `wilted1..3` from 0 into arrays instead of building the name with `level`. The ceiling uses the same `vine_kanim`, so it should match, but it was not run.
- Wild planting: `Reset(seed, false, 20)` passes only ceiling cells, as before. U60 adds a backwall planting path (`CheckValidBackWallPlotCell`) that the `CheckValidPlotCell` patch does not cover. It matters only for seeds with `requireBackWallPlantable`, which the Ovagro seed is not as far as was checked.
- Run the mod's own test suite (WILD, Registration, Persistence, Removal) on U60.

## Store feedback (for the coordinator)
- The one pointer (`blk-9a58d8528c61`, removal testing) was not needed for this work. No governing record was missing that I could see.
- I read the pointer and the spec but did not run `ks read`; the task needed only the decompiled game sources.
- Friction: `ks read` was named in the prompt but nothing in the packet needed resolving.
