# Unit 01 report: Ovagro planting and vine growth in the game code

## Build and sources
- Game: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (resolved by common/oni-mod.props; no GameFolder override found). KleiVersion.ChangeList = 744825, branch "release". Assembly-CSharp.dll dated Aug 1.
- Decompiled with the installed `ilspycmd` 9.1 (`ilspycmd -p -o <dir> <dll>`), one .cs per type. Written outside the repo:
  - `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\` (Assembly-CSharp, 4409 files)
  - `...\scratchpad\fp\` (Assembly-CSharp-firstpass, holds DlcManager)
- Line numbers below are lines in those files (file name = type name).

## Store reads
No `ks brief` or `ks read` was run by this unit, and no packet pointer was read. The packet's pointers (error-handling and code-style standards of other mods) did not bear on a decompiled-game question. Nothing in this report came from the store.

## Q1. What Ovagro is
- Display names: "Ovagro Node" (plant), "Ovagro Vine" (branch), "Ovagro Node Seed", "Ovagro Fig" (fruit); STRINGS/CREATURES.cs 1935-2085, STRINGS/ITEMS.cs 739.
- Prefab ids (VineMotherConfig.cs 9-11, VineBranchConfig.cs 361, VineFruitConfig):
  - plant: `VineMother` (class `VineMotherConfig`), 1 wide x 2 tall, anim `vine_mother_kanim`, `CreatePlacedEntity(... width 1, height 2 ...)` line 44
  - seed: `VineMotherSeed` (`seed_vine_kanim`), preview `VineMother_preview` (line 72)
  - branch: `VineBranch` (class `VineBranchConfig`, `vine_kanim`, 1x1, tags HideFromSpawnTool/HideFromCodex/PlantBranch), the part that bears fruit
  - fruit: `VineFruitConfig.ID` (Ovagro Fig); crop def in TUNING/CROPS.cs
- State machines: `VineMother` (VineMother.cs, extends PlantBranchGrowerBase) and `VineBranch` (VineBranch.cs).
- DLC gate: both configs implement `IHasDlcRestrictions`; `GetRequiredDlcIds()` returns `DlcManager.DLC4` (= `{"DLC4_ID"}`, firstpass DlcManager.cs 82; DLC4 is the Prehistoric Planet Pack, line 112). Seed gets requiredDlcIds from the same object (EntityTemplates.cs 965).
- The seed is `SeedProducer.ProductionType.Hidden` (VineMotherConfig line 72), so the mother never drops it. Branches drop it: `seedProducer.Configure("VineMotherSeed", HarvestOnly)`, chance multiplier 1/6 (VineBranchConfig 426-428). Wild nodes come from digging buried objects (seed DESC) and `VineMother.Instance.OnSpawnedByDiscovered` (event 1119167081).
- Other consumers: Lumb critter eats the vines (STRINGS only checked).

## Q2. Placement rules, and how ceiling plants differ
Ovagro (VineMotherConfig):
- Seed direction `ReceptacleDirection.Top` (line 72), no `replantGroundTag`.
- No `GameTags.Hanging` tag, no `MakeHangingOffsets`; occupies `GenerateOffsets(1,2)` = (0,0),(0,1) (EntityTemplates.cs 1016-1023, set via CreatePlacedEntity line 171).
- `ExtendEntityToBasicPlant(... require_solid_tile: true ...)` (line 46) adds `UprootedMonitor` with default `monitorCells = {(0,-1)}` (UprootedMonitor.cs 17-20): the cell below must be solid.
- No custom kanim handling.

Ceiling plant pattern (SaltPlantConfig.cs, the vanilla hanging example):
1. `additionalTags = {GameTags.Hanging}` (line 22)
2. `EntityTemplates.MakeHangingOffsets(plant, w, h)` (line 24) and the same on the preview (line 58): OccupyArea offsets become `GenerateHangingOffsets` = y from -(h-1) to 0 (EntityTemplates.cs 199-214, 1042-1049); box collider offset moved to match. Origin cell is the TOP cell.
3. `UprootedMonitor.monitorCells = {(0,1)}` (line 47-50): cell above must be solid.
4. Seed built with `ReceptacleDirection.Bottom` (line 58).
5. The art is a dedicated hanging anim; plants carry no `Rotatable`, so nothing flips them automatically.

Where the game reads `seed.direction`:
- `PlantableSeed.TestSuitableGround(cell)` (PlantableSeed.cs 76-126): ground cell is `CellAbove(cell)` for Bottom, else `CellBelow(cell)` (line 82); then rejects Foundation tiles (87), `Element.hardness >= 150` (91), wrong `replantGroundTag` (95), and tests the prefab's `EntombVulnerable.IsCellSafe` (100), `DrowningMonitor.IsCellSafe` (105), `TemperatureVulnerable.IsCellSafe` (110), `UprootedMonitor.IsSuitableFoundation` (115), `OccupyArea.CanOccupyArea` (120). All of these read the prefab's own offsets, so a hanging Ovagro prefab passes them with no change to them.
- `PlantableSeed.GetDescriptors` (128-142): Bottom shows "needs ceiling", Side "needs wall".
- `PlantableCellQuery.CheckValidPlotCell` (PlantableCellQuery.cs 176-233) and `SeedPlantingStates.CheckValidPlotCell` (221-244): same Above/Below choice; ground must be `Grid.Solid`; if the ground cell holds a building it must be a `PlantablePlot` whose `Direction == seed.Direction` and empty; else wild checks plus `CountNearbyPlants(... radius 6) <= 2`.
- Hydroponic/farm tiles: see Q4.

## Q3. How vines grow, and every "up" assumption
Mother (VineMother.cs):
- `Def.BRANCH_PREFAB_NAME="VineBranch"`, `MAX_BRANCH_COUNT=24` (VineMotherConfig 55-57; branch chain cap 12, `MaxBranchNumberReached` VineBranch 150).
- Once grown, `grown.growingBranches` calls `AttemptToSpawnBranches` every 4000 ms (VineMother 290, 186-219): spawns a branch in the cell directly LEFT and RIGHT of the mother's origin cell (`Grid.OffsetCell(cell, CellOffset.left/right)`, lines 191, 204) if `VineBranch.IsCellAvailable`. Only the origin cell row is used; origin is the bottom cell of a standing mother and would be the top cell of a hanging one (offsets above), so the first branch row is the row touching the floor/ceiling in both cases.
- `SpawnBranchOnCell` (233-240) instantiates at `Grid.CellToPosCBC(cell, BuildingFront)` and calls `VineBranch.Instance.SetupRootInformation(mother)`.

Branches (VineBranch.cs) are NOT vertical vines. Each is a 1x1 piece that hugs a solid surface and walks around it: shapes Top, Bottom, Left, Right, 4 InCorner, 4 OutCorner, 4 End (lines 69-95). Each new branch spawns in the next cell along the surface (`GetCellToSpawnBranch` 558-628), up to 12 deep. So "grows as on the floor" means: branches travel left/right along the surface the mother stands on and turn corners; on a ceiling they must travel along the underside. The shape logic is already symmetric for all four sides.

Up/down assumptions (the real blockers):
1. **VineBranch.cs 466** `SetupRootInformation(VineMother.Instance mother)` sets `RootShape.Set(1)`; enum value 1 = `Shape.Bottom`. This tells the first branch its parent lies on a floor. For a hanging mother it must be `Shape.Top` (0). With Bottom, `RecalculateMyShape` (680-804, case RootDirection Left/Right with RootShape Bottom) picks among `{End, InCornerBottom*, OutCornerBottom*, Bottom}` and `Bottom` needs `IsCellFoundation(cell below branch)` (line 947); a ceiling mother has no floor there, so the first branch becomes a dead end or wrong corner. This is the single mother-to-branch hard-coded orientation. Later branches inherit `RootShape` from the parent (line 480), so fixing the first fixes the chain.
2. **VineBranch.cs 463-464 and 477-478** `RootDirection` is derived from branch-to-parent offset; only left/right occurs for the first branch. No up assumption.
3. **VineBranch.IsCellAvailable** (1173-1182) and `IsCellFoundation` (1164-1171) are direction-neutral (solid or door; not liquid; no building on layer 1; no plant on layer 5). Fine.
4. **VineBranch.IsMotherCellFoundation** (519-526): only for `IsOnPlanterBox` (mother's receptacle is a `PlantablePlot` with `IsOffGround`: PlanterBox and FlowerVase only; FarmTile and Hydroponic do not set it, FarmTileConfig/HydroponicFarmConfig have no IsOffGround). Farm tiles are solid tiles, so they count as foundation through `Grid.IsSolidCell` already.
5. **VineMother Instance.IsOnPlanterBox/PlanterboxCell** (147-169): not direction dependent.
6. **UprootedMonitor on the mother**: default `monitorCells {(0,-1)}` (UprootedMonitor.cs 17). A hanging mother needs `{(0,1)}`, as SaltPlant does. Without it a ceiling mother is uprooted at once (when wild; plot plants get `canBeUprooted=false`, PlantablePlot.cs 264-268) and `PlantableSeed.TestSuitableGround` (line 115) rejects ceiling cells.
7. **Branch UprootedMonitor** is customised per shape (`ResetUprootMonitor` 970-1056, `customFoundationCheckFn = IsCellFoundation` line 230): shape-based, no change needed.
8. **Anim orientation**: `SetAnimOrientation(shape, clockwise)` (806-881) drives `FlipX/FlipY/Rotation/Pivot/Offset` per branch from shape, symmetric. `Rotation *= -1` (877), `Offset = (0, FlipY?1:0)`, pivot (0, 0.5 or -0.5). Branches need no change. The MOTHER anim `vine_mother_kanim` (2 cells tall, "object"/"idle_full"/"grow"/"wilt3") has no flip handling; nothing sets `FlipY`, so a hanging mother renders upright and hanging from nothing unless the mod sets `KBatchedAnimController.FlipY` (KAnimControllerBase.cs 295) plus offset/pivot, or ships hanging art. (Whether the art is vertically symmetric enough was not checked; the kanim files were not inspected.)
9. **Fruit meters**: `MeterController(AnimController, Anims.meter_target, Anims.meter, Meter.Offset.NoChange, Building)` (VineBranch 344), positioned by `SetPositionPercent` (435-445, 1359-1362); follows the controller transform, so it flips with the branch.
10. **Fruit drop**: `Crop.SpawnAndGetSomeFruit` (Crop.cs 114-117) spawns at `transform.position + cropSpawnOffset` on the Ore layer; branch-driven, falls normally. No change required.
11. **Harvest overlay icon**: `HarvestDesignatable.cs 123` positions the icon at the top of the extents when the plant has `GameTags.Hanging`, at the bottom otherwise. The branch uses `iconOffset (0, 0.75)` (VineBranchConfig 435). Mother with `Hanging` tag gets the hanging placement.
12. **PollinationVFXMonitor.cs 18, 29, 77** uses `Hanging` for its effect cell offset; `ExtendEntityToBasicPlant` adds it to every plant.

Anything else vertical? `VineMotherConfig` mother has `HarvestDesignatable` but no `Crop`, so the harvest chain is on branches. `SetAutoHarvestInChainReaction` is graph, not geometry.

## Q4. Farm tiles
- Plain Farm Tile (`FarmTileConfig`): `PermittedRotations.FlipV` (line 22), anim `farmtilerotating_kanim`. Hydroponic Farm (`HydroponicFarmConfig`): `PermittedRotations.FlipV` (line 22), `farmtilehydroponicrotating_kanim`. Both can face down in this build.
- `FarmTileConfig.SetUpFarmPlotTags` (71-95, also called from the Hydroponic `DoPostConfigureComplete` line 70) runs in `prefabSpawnFn` and maps orientation to receptacle direction: Neutral/FlipH -> Top; R180/FlipV -> Bottom; R90/R270 -> Side, via `PlantablePlot.SetReceptacleDirection` (SingleEntityReceptacle.cs 190-193). Field: `SingleEntityReceptacle.direction` (line 65), `Direction` property (113).
- Farm tile `occupyingObjectRelativePosition (0,1,0)` (FarmTileConfig 41); hydroponic `.y = 1` (line 49). `PositionOccupyingObject` (SingleEntityReceptacle 420-434) adds `rotatable.GetRotatedOffset(offset)`; `Rotatable.GetRotatedOffset` for FlipV returns `(x, -y)` (Rotatable.cs 223/246), so the plant lands in the cell below.
- Seed acceptance: `SingleEntityReceptacle.IsValidEntity` (120-135) is true when `Direction == Any || rotatable != null || seedDirection == Direction`. Farm tiles have a `Rotatable`, so the direction test is bypassed and ANY crop seed is accepted at deposit time. `PlanterSideScreen.SetResultDescriptions` (lines 320-335) only adds the text ROTATION_NEED_FLOOR/WALL/CEILING when `targetReceptacle.Direction != seed.direction`; it does not block. `PlantablePlot.SetPreview` (372-433) positions the preview with `Rotatable.GetRotatedOffset` using the seed's own direction (R180 offset for Bottom), and `EntityPreview.UpdateValidity` (EntityPreview.cs 71-105) tests the preview's OccupyArea cells for not-solid and an empty Building layer; that is the gate that turns a mismatched Top-seed preview red.
- Deposit path: `PlantablePlot.SpawnOccupyingObject` (239-257) instantiates `component.PlantID` at `CellToPosCBC(PosToCell(plot), plantLayer)` then `PositionOccupyingObject` moves it by the rotated offset. The spawned plant is not flipped. For a hanging plant the OccupyArea offsets (from MakeHangingOffsets) make it extend downward from the placed cell.
- Allowed tags for Farm/Hydro: `GameTags.CropSeed` and `GameTags.WaterSeed`; `ForbiddenTags` rejects `LargeSeed` and `BackwallSeed` (FarmTileConfig 57-61). The Ovagro seed has `CropSeed` (VineMotherConfig 70), so it is accepted.
- Practical consequence: a current-vanilla Ovagro seed (Top) can already be ordered into a downward farm tile (the direction filter is bypassed), but the plant that spawns is upright, its OccupyArea (0,0),(0,1) runs upward into the farm tile itself (cell above the origin = the tile), and `canBeUprooted=false` hides the foundation check. Whether the preview shows red is decided by `EntityPreview` on those offsets (the (0,1) cell is the solid tile, so it would be invalid). Not runtime-verified.
- Hydroponic Farm: `ConduitConsumer` and `PlanterBox` component; liquid input at `UtilityInputOffset (0,0)`; `InputConduitType = Liquid`. The vine mother needs water: `ExtendPlantToIrrigated`, 0.15 kg/s (VineMotherConfig 59-66), so the hydroponic variant is the natural one.

## Q5. Planting a seed in a natural tile
- Duplicants do NOT plant seeds in natural tiles in this build. The only dupe path is `PlantablePlot` fetch/deposit (`SingleEntityReceptacle.CreateOrder`, choreType `FarmFetch`, PlantablePlot.cs 105; deposit via `ForceDeposit`).
- The natural-tile planter is the Pip (squirrel): `SeedPlantingMonitor` (SeedPlantingMonitor.cs; 60-300 s search timer) toggles `GameTags.Creatures.WantsToPlantSeed`; `SeedPlantingStates` (findSeed ... planting) runs it; registered in `BaseSquirrelConfig` lines 30 and 76; navigation `NavType.Floor` (line 19). Flow: `FindSeed` (253-272, any `PlantableSeed` tagged Seed/CropSeed within 25 units), `FindDirtPlot` (209-219) runs `PlantableCellQuery` (20 results), `PlantComplete` (172-192) either `plot.ForceDeposit(seed)` or `plantableSeed.TryPlant(allow_plant_from_storage: true)` (PlantableSeed.cs 45-74), which spawns `PlantID` at the cell via `GameUtil.KInstantiate` and consumes one unit.
- `PlantableSeed.timeUntilSelfPlant` is set and serialised but nothing else in Assembly-CSharp references it; no self-planting path exists.
- Valid underside target for a seed: `PlantableSeed.TestSuitableGround` and `PlantableCellQuery.CheckValidPlotCell`/`SeedPlantingStates.CheckValidPlotCell`, all keyed on `seed.Direction` (Bottom = ceiling). See Q2.
- This contradicts the task brief ("planted by Duplicants in natural tiles"). With the vanilla code, wild ceiling planting is done by Pips, and a Pip walks `NavType.Floor`, so it plants a Bottom seed only in a cell it can reach whose CellAbove is solid. Hydroponic/farm-tile planting is the Duplicant path.

## Q6. Other things that fight an upside-down Ovagro
- `EntombVulnerable` (EntityTemplates.cs 226; EntombVulnerable.cs 133-145): tests every OccupyArea cell not solid; offset-driven, fine with hanging offsets.
- `PressureVulnerable` with `pressure_sensitive:false` and allowed elements Oxygen/CO2/ContaminatedOxygen (VineMotherConfig 15-20, 46): checks occupied cells; offset-driven.
- `DrowningMonitor` (`can_drown: true`): offset-driven; hanging plant's lower cell is the exposed one.
- `Uprootable` (Uprootable.cs 136, 274): uses `OccupyArea.OccupiedCellsOffsets` for extents; offset-driven.
- `UprootedMonitor` on the branch: set to empty array in `VineBranchConfig.OnPrefabInit` (line 434) and per shape later (see Q3.7).
- Harvest of the vine: the harvest tool targets the branch (`Harvestable`, `HarvestDesignatable` on both); dupes reach the branch's own cell; no up assumption.
- Mother `decor`, `DecorProvider` offsets come from CreatePlacedEntity (offset-driven).
- Seed preview prefab `VineMother_preview` uses `CreateAndRegisterPreview` (1x2 offsets set by CreatePlacedEntity); for ceiling placement it needs `MakeHangingOffsets(preview,1,2)` exactly as SaltPlantConfig line 58.
- Save/load: `Hanging`-style prefab offsets are baked into the prefab, so a mod that adds a second prefab (or modifies the prefab at registration) is the data lever. `VineMother` serialisation is `ParamsOnly` (VineMother 281); branch is `ParamsOnly` (VineBranch 1186); the shape and root shape parameters persist.
- Existing repo code: no mod in `mods/` touches `PlantableSeed`, `ReceptacleDirection` or `GameTags.Hanging` (grep over `mods/**/*.cs`). The FlipV hits are building rotation code in other mods, not plants.

## Levers (facts, not a design)
- Prefab data: tag `Hanging`, `MakeHangingOffsets` on plant and preview, `UprootedMonitor.monitorCells {(0,1)}`, seed `PlantableSeed.direction = Bottom`; all are prefab fields that a postfix on `VineMotherConfig.CreatePrefab` (or a patch of the registered prefab) can set, as SaltPlantConfig does.
- Orientation: `VineBranch.cs 466` hard-coded `RootShape = 1 (Bottom)`; the only code constant tying branch geometry to a floor-standing mother. Patch point is `VineBranch.Instance.SetupRootInformation(VineMother.Instance)`.
- Mother art: needs `FlipY` (or hanging kanim) on the mother's `KBatchedAnimController`; none exists in the game.
- A single prefab can hold only one direction (`PlantableSeed.direction` is per seed); supporting both floor and ceiling needs either a second plant+seed+preview prefab or runtime flipping per placement. `IsValidEntity` already ignores direction for rotatable plots.
- Farm tile and Hydroponic Farm already rotate FlipV and set `Direction=Bottom` (FarmTileConfig 71-95).

## Gaps
- Kanim art for `vine_mother_kanim` was not opened (not tested whether it can be flipped without artefacts).
- Nothing was run in the game, so the preview/validity behaviour for a Top Ovagro in a downward farm tile (Q4) and Pip reachability of ceiling cells (Q5) are code-read conclusions only.
- Branch cell used by the mother when `IsNewGameSpawned` (world gen) and the `GetSpawnableInCell` check (VineBranch 540-543) were read but not traced further.
- `Growing`, `ReceptacleMonitor`, `IrrigationMonitor` internals were not read for direction logic (grep for Hanging/CellAbove/CellBelow in Uprootable, Growing, ReceptacleMonitor, StandardCropPlant returned nothing).

## ICON friction
- None hit; `ks` was not used (see Store reads).
