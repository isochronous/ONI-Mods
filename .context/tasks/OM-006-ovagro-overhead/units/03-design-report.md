# Unit 03 report: design for Ovagro Overhead (ceiling-planted Ovagro)

Game build 744825 (release). Game code was read in the decompiled sources (`scratchpad\decomp`, `scratchpad\fp`). File:line references below point into those files (file name = type name). Nothing was run in the game.

## Recommendation

**Approve with modifications.**

The feature can be built. Planting and spawning go through a **second, hidden plant prefab** (`VineMotherCeiling`, with its preview). The **vanilla seed** (`VineMotherSeed`) stays the only seed. The game picks the form where the seed is planted, from the cell's geometry or the plot's facing. Nine small Harmony patches route the seed to the ceiling form. One more patch makes the first vine branch hug the ceiling.

The modifications to the change as proposed:
1. **Wild ceiling planting is done by Pips walking on the ceiling.** Pips walk floors, walls and ceilings. Vanilla Dasha Saltvine already gets planted this way, so no navigation change is needed. This corrects unit 01, which assumed Pips walk floors only (user's correction, see "Corrections to unit 01").
2. **World-gen Ovagro nodes stay on floors.** The feature covers planting by Pips and Duplicants. It adds no ceiling nodes to world gen.
3. **The mod declares `requiredDlcIds: [DLC4_ID]`.** No mod in the repo uses this field yet, so it is a new pattern. The open decision D3 covers it.
4. Three findings from unit 01 are overturned, and they change what must be patched (see "Corrections to unit 01"):
   - The planter side screen blocks a Top seed on a downward plot.
   - The Top-seed preview sits above a downward tile, not inside it.
   - Several VineMother line numbers were wrong.

## Recommended design

### Data: one new entity, no new seed

`VineMotherCeilingConfig : IEntityConfig, IHasDlcRestrictions` (required DLC `DlcManager.DLC4`, the same as vanilla) builds:

- **Plant `VineMotherCeiling`.** It is built with the same calls as `VineMotherConfig.CreatePrefab` (VineMotherConfig.cs 44-66), with these changes:
  - It reads the vanilla public constants: `ALLOWED_ELEMENTS`, `TEMPERATURE_*`, `IRRIGATION_RATE`.
  - It uses the vanilla name and description strings `STRINGS.CREATURES.SPECIES.VINEMOTHER.*`, so it adds no new text.
  - It reuses the `vine_mother_kanim` anim.
  - Hanging changes, as SaltPlantConfig.cs 22-50 does: `additionalTags = {GameTags.Hanging}`, `EntityTemplates.MakeHangingOffsets(go, 1, 2)` (origin = top cell, occupies (0,0),(0,-1)), and `UprootedMonitor.monitorCells = {(0,1)}`.
  - Its own base trait id (`VineMotherCeilingOriginal`). `ExtendEntityToBasicPlant` creates a trait under that id (EntityTemplates.cs ~224), and the id must not collide with the vanilla one.
  - `VineMother.Def` with `BRANCH_PREFAB_NAME = "VineBranch"` and `MAX_BRANCH_COUNT = 24`. The vanilla branch prefab is reused.
  - `HarvestDesignatable`, the overlay registration, and `ExtendPlantToIrrigated` (water 0.15 kg/s).
  - `plant.AddOrGet<SeedProducer>().Configure("VineMotherSeed", Hidden)`. This is what `CreateAndRegisterSeedForPlant` would add (EntityTemplates.cs ~978). Do **not** call `CreateAndRegisterSeedForPlant`: it would register a second `VineMotherSeed` prefab.
  - Tags `HideFromSpawnTool` and `HideFromCodex`, plus `CodexEntryRedirector.CodexID = "VineMother"`. VineBranchConfig.cs 49 sets up the branch the same way.
  - Art: on its `KBatchedAnimController`, `Rotation = 180`, `FlipX = true`, `Pivot = (0, 0.5, 0)`. See "Mother art".
- **Preview `VineMotherCeiling_preview`.** Built with `EntityTemplates.CreateAndRegisterPreview(id, vine_mother_kanim, "place", ObjectLayer.Building, 1, 2)`, then `MakeHangingOffsets(preview, 1, 2)` and the same art transform. Do **not** call `CreateAndRegisterPreviewForPlant`: it would overwrite the vanilla seed's `PreviewID` (EntityTemplates.cs 1009-1013).

### Routing: "ceiling form" swaps on the vanilla seed

One static helper, `CeilingForm`, holds the routing:

- `IsOvagro(PlantableSeed s)` is true when `s.PlantID == "VineMother"` and `s.direction == Top`.
- `Enter(PlantableSeed s)` sets `direction = Bottom`, `PlantID = VineMotherCeiling` and `PreviewID = VineMotherCeiling_preview`. `Exit(s)` restores them.
- `WantsCeiling(int cell)` is true when `Grid.Solid[Grid.CellAbove(cell)]`.

The three `PlantableSeed` fields are plain public instance fields, and none of them is serialised (PlantableSeed.cs 9-20; only `timeUntilSelfPlant` is `[Serialize]`). Every swap is synchronous on the main thread. A prefix enters the swap and sets `__state`; a **finalizer** exits it, so an exception cannot leave a seed swapped. The repo already uses `__state` (vent-freeze-fix DebugSave.cs 91) and `Finalizer` (natural-backwalls WorldgenThreadPatch.cs 22).

**The geometry rule is exact, not a preference.**
- The standing form needs the cell below to be solid and the cell above to be free of solid. `EntombVulnerable` covers (0,1).
- The hanging form needs the cell above to be solid and the cell below to be free.
- No cell can host both forms, so `WantsCeiling(cell)` decides the form without ambiguity.
- Plots decide by `plot.rotatable != null && plot.Direction == Bottom`, which is exactly a FlipV Farm Tile or Hydroponic Farm (FarmTileConfig.cs 71-95). WideFarmTile is `Unrotatable` (WideFarmTileConfig.cs 30), and the hanging flower vases keep direction Top.

### Game methods patched (10)

| # | Method | Kind | Why |
|---|---|---|---|
| 1 | `PlantableCellQuery.CheckValidPlotCell(PlantableSeed, int)` (private, PlantableCellQuery.cs 33-90) | prefix + finalizer, swap when `IsOvagro && WantsCeiling(plant_cell)` | The Pip's search for a planting cell. With the swap, its `Grid.Solid[CellAbove]`, plot-direction and `TestSuitableGround` checks all see the hanging form. |
| 2 | `SeedPlantingStates.CheckValidPlotCell(Instance, PlantableSeed, int, out PlantablePlot)` (private static, SeedPlantingStates.cs 221-244) | same | The Pip's re-check at the moment of planting (`PlantComplete`, line 175). |
| 3 | `PlantableSeed.TryPlant(bool)` (PlantableSeed.cs 45-74) | same, cell = `Grid.PosToCell(seed)` (the Pip's cell while the seed is in its storage) | Spawns `PlantID`, which is `VineMotherCeiling` during the swap. `TestSuitableGround` itself needs no patch: its only callers are #1-#3. |
| 4 | `PlantablePlot.SpawnOccupyingObject(GameObject)` (protected override, PlantablePlot.cs 239-257) | prefix + finalizer, swap the deposited seed when the plot is a downward rotatable plot | The Duplicant fetch/deposit path and the Pip's `ForceDeposit`. It spawns the ceiling prefab. `PositionOccupyingObject` already moves the plant to the cell below (FlipV `GetRotatedOffset` gives (0,-1), SingleEntityReceptacle.cs 420-434). |
| 5 | `PlantablePlot.SetPreview(Tag, bool)` (public override, PlantablePlot.cs 372-433) | prefix + finalizer, swap the **seed prefab's** `PlantableSeed` when the plot faces down and the tag is `VineMotherSeed` | The preview becomes the hanging preview, placed with the R180 offset (line 417). Its cache check (line 388) compares against the swapped `PreviewID`, so it stays consistent. `EntityPreview.UpdateValidity` then tests the correct cells below the tile. Covers every caller: side screen, `CreateOrder`, plot `OnSpawn`, copy settings. |
| 6 | `ReceptacleSideScreen.Initialize(SingleEntityReceptacle)` (ReceptacleSideScreen.cs 161-285) | prefix + finalizer, swap the seed prefab while the toggles are built, for a downward plot | `SelectableEntity.direction` is read from the prefab here (line 231/257). `ValidRotationForDeposit` (666-672) otherwise disables the Ovagro toggle on a downward plot. |
| 7 | `PlanterSideScreen.SetResultDescriptions(GameObject)` (protected override, PlanterSideScreen.cs 302-345) | same | Removes the "rotate to floor" text (ROTATION_NEED_FLOOR, line 324). The vanilla "Requires ceiling" descriptor (PlantableSeed.cs 131-135, already translated) shows instead. |
| 8 | `PlantablePlot.OnCopySettings(object)` (private, PlantablePlot.cs ~144-170) | same, for this plot | Copy Settings between downward tiles otherwise skips Ovagro (line 168 compares the seed's direction with the plot's). |
| 9 | `VineBranch.Instance.SetupRootInformation(VineMother.Instance)` (VineBranch.cs 461-473; overload, needs `new[] { typeof(VineMother.Instance) }`) | postfix: when `mother.PrefabID() == VineMotherCeiling`, `__instance.sm.RootShape.Set((int)VineBranch.Shape.Top, __instance)` | Line 466 hard-codes `RootShape = 1` (Bottom). Later branches copy their parent's shape (line 480), so fixing the first branch fixes the whole chain. |
| (10) | entity registration | not a patch | `VineMotherCeilingConfig` is found by `LegacyModMain.LoadEntities` → `EntityConfigManager.LoadGeneratedEntities` over `App.GetCurrentDomainTypes()` (LegacyModMain.cs 43/112, EntityConfigManager.cs 42-81). It is registered only when DLC4 is subscribed (line 79). |

Patches #5 to #8 swap a **prefab's** fields for the duration of one call. That is the one unusual move in this design. Each call is synchronous, the finalizer restores the fields, and nothing reads the prefab in between. The alternative is a hidden "ceiling seed" prefab plus a tag rewrite in every one of those methods. It costs more code and registers a seed prefab that the game would never otherwise know.

### Vines (question 4)

- `VineMother.Instance.AttemptToSpawnBranches` (VineMother.cs 103-136) spawns branches LEFT and RIGHT of `Grid.PosToCell(mother)`, which is the origin cell. A hanging origin is the top cell, so the first branches sit in the row under the ceiling. No change is needed.
- `SpawnBranchOnCell` (150-157) calls `SetActive(true)` and then `SetupRootInformation`. The branch's state machine starts on `KMonoBehaviour.Start → Spawn → OnSpawn` (fp KMonoBehaviour.cs 108-135; StateMachineController.StartSMIS 120-136), which runs after `SetActive` returns. So the parameters set by patch #9 are in place when `undevelopedBranch` enters and calls `RecalculateMyShape` (VineBranch.cs 1191). This is inferred from the code; test T5 confirms it.
- Confirmed symmetric:
  - `RecalculateMyShape` (680-804) handles Left/Right root directions with RootShape Top (`{RightEnd, InCornerTopRight, OutCornerTopLeft, Top}`) exactly as with Bottom.
  - `ChooseCompatibleShape`'s Top check is `IsCellFoundation(above) && (left || right free)` (943-945).
  - `GetCellToSpawnBranch` (558-628), `ResetUprootMonitor` (970-1056; Top monitors `CellOffset.up`) and `SetAnimOrientation` (806-881) all have Top cases.
  - `IsCellFoundation` / `IsCellAvailable` (1164-1182) do not depend on direction.
  - `IsMotherCellFoundation` (519-526) applies only to `IsOffGround` planter boxes, which never face down.
- The Top-shape branch art is already used in vanilla, when a vine wraps under an overhang. Branch meters and fruit drops follow the branch's own anim and transform (unit 01, Q3.9-10).

### Mother art (question 5)

I inspected the art.
- `kanim.py extract` reads only `sharedassets0.assets`, which does not hold DLC4 art. The Ovagro kanim is in `OxygenNotIncluded_Data/StreamingAssets/dlc4_bundle` (`vine_mother_0`, `vine_mother_build`, `vine_mother_anim`).
- I pulled it out with a scratch UnityPy script and rendered `idle_full`, `grow`, `grow_pst` (last frame), `wilt3` and `place` upright and flipped. The sheet is at `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\kanim\sheet.png` (Klei art, not for the repo).
- Flipped, the node reads as a pineapple-like caudex hanging from the ceiling with its leaves below. The seedling, wilt and place outline flip cleanly. The art is lit from the top, so the flipped node is shaded from below. That is the only visual cost.
- The kanim has no `object` anim, although `initialAnim` names it. This is a vanilla quirk, shared by both forms.

How to flip. `KBatchedAnimController.GetTransformMatrix` (KBatchedAnimController.cs ~570-590) applies `FlipY` about the anim origin before position and `Offset`. Rotation is applied about `Pivot` in world units.
- **FlipY + Offset (0,1)** is VineBranch's method (line 878). It does **not** survive in a farm plot: `PlantablePlot.PositionOccupyingObject` sets `Offset = rotated occupyingObjectVisualOffset` = (0,0) on every placement and load (PlantablePlot.cs 292-298). It would need an extra patch.
- **Recommended: `Rotation = 180`, `FlipX = true`, `Pivot = (0, 0.5, 0)`.** This is a vertical mirror about the origin cell's centre (y' = 1 − y), so the art spans from the ceiling surface downward. Nothing in the plant or plot code touches Rotation, FlipX or Pivot. All three are `[SerializeField]` on `KAnimControllerBase` (lines 40-80), so values set on the prefab carry to instances.
- Status and harvest icons: `HarvestDesignatable` places its icon at the top of the extents for `Hanging` plants (HarvestDesignatable.cs 123), and `PollinationVFXMonitor` uses `Hanging` too. The selection box follows `KBoxCollider2D`, which `MakeHangingOffsets` sets. The flipped grow/wilt anims are plain sprites with no direction-dependent symbols the code reads.

### Pips (question 2, reworked after the user's correction)

**Pip navigation.** `BaseSquirrelConfig` (BaseSquirrelConfig.cs 14-19) uses nav grid `SquirrelNavGrid` (adult; babies use `DreckoBabyNavGrid`). `NavType.Floor` there is only the *spawn* nav type. `GameNavGrids.CreateSquirrelNavigation` (GameNavGrids.cs 2111-2203) gives Pips:
- Floor, RightWall, LeftWall and Ceiling nav types.
- Ceiling↔Ceiling walking, and corner transitions Floor→Wall→Ceiling→Wall→Floor, mirrored.
- Validators `FloorValidator`, `WallValidator` and `CeilingValidator`.
- `CeilingValidator` makes a cell walkable when `Grid.Solid[CellAbove(cell)]` (GameNavGrids.cs ~276-288). A Pip on the ceiling stands in the open cell directly under the solid tile, drawn upside down (`rotation = -π`, `animControllerOffset (0,-1)`).

**What the planting code assumes.**
- `PlantableCellQuery` is a `PathFinderQuery`, and `IsMatch(cell, …)` (24-31) runs for every cell the Pip can reach, whatever the nav type. `plant_cell` is the cell the Pip will stand in. For a Bottom seed, the ground is `CellAbove(plant_cell)` (line 39).
- `SeedPlantingStates.FindDirtPlot` (209-219) picks one of up to 20 results at random. `moveToDirt` moves the Pip to that cell (line 117), and the Pip arrives there in Ceiling nav. `planting` plays "plant". `PlantComplete` (172-192) re-checks the same cell and calls `TryPlant(allow_plant_from_storage: true)`. The seed is in the Pip's storage, so `Grid.PosToCell(seed)` is the Pip's cell.
- The hanging plant occupies the Pip's cell and the one below. `OccupyArea.CanOccupyArea(cell, Building)` ignores creatures.
- So the code assumes that **the Pip's own cell is the plant's origin cell**. That holds on a ceiling exactly as on a floor.

**How vanilla Dasha Saltvine gets Pip-planted.** By exactly this path. `SaltPlantSeed` has direction Bottom and `CropSeed` (SaltPlantConfig.cs 56-58), so `FindSeed` (253-272) accepts it. `PlantableCellQuery` then finds cells with solid above, and a Pip reaches them by walking on the ceiling. Floor-nav cells in a 1-high gap also have solid above, but the 1x2 hanging plant fails there: its lower cell is solid.

**What must change for Ovagro.** Only the seed's direction, per cell, through patches #1 to #3. Pathfinding and reachability need nothing.

**Floor or ceiling: does the Pip need a rule?** With the swaps, one query returns floor and ceiling cells together, in path-cost order, and the Pip picks among the 20 nearest at random. The vanilla limits apply to both: radius 6, at most 2 nearby plants, no Foundation (built tiles), hardness < 150. Open decision D1.

**Pips and downward plots.** A Pip with a Bottom seed already plants into a downward Farm Tile or Hydroponic Farm in vanilla (plot branch of PlantableCellQuery 56-77, `ForceDeposit` at PlantComplete 181). With patches #1, #2 and #4, the same holds for Ovagro.

### Downward farm plots (question 3)

- **Duplicant ordering.** The side screen lists the Ovagro seed and enables it for a FlipV plot (patch #6). Its description shows "Requires ceiling" instead of "rotate to floor" (patch #7).
- **Preview.** `SetPreview` (patch #5) places the hanging preview with the R180 offset at the cell below the tile, covering that cell and the next one down. `EntityPreview.UpdateValidity` (EntityPreview.cs 71-105) turns it red when those cells are solid or hold a building. `CreateOrder` (PlantablePlot.cs 189-200) creates the fetch only while the preview is valid.
- **Delivery.** `SpawnOccupyingObject` (patch #4) instantiates `VineMotherCeiling`. `PositionOccupyingObject` moves it one cell down. `ConfigureOccupyingObject` sets `canBeUprooted = false`, as for every plot plant (258-268).
- **Hydroponic irrigation.**
  - `RegisterWithPlant` (300-316) reads the plant's `IrrigationMonitor.Def`, so the ceiling prefab must call `ExtendPlantToIrrigated` exactly as vanilla does.
  - It then sets `RequireInputs.RequiresInputConduit`.
  - The liquid input is on the farm tile's own cell (`UtilityInputOffset (0,0)`), and the plant takes water from the plot's storage.
  - None of this depends on the plant's orientation.
- **On load.** The plot's `OnSpawn` repositions its occupant (SingleEntityReceptacle.cs 159-165). The prefab id is the saved one, so the plant reloads in its ceiling form.

### DLC gating (question 6)

- **Field name and type.** `requiredDlcIds` is a `string[]` on `KMod.Mod.PackagedModInfo` (KMod/Mod.cs 33-58), parsed by `YamlIO.Parse<PackagedModInfo>` (Mod.cs ~436).
- **Value checks.** Each value is upper-cased and checked with `DlcManager.IsDlcId` (482-491). The value is `DLC4_ID` (fp DlcManager.cs 70 and 82).
- **`supportedContent` is deprecated.** When `requiredDlcIds`/`forbiddenDlcIds` are absent and `supportedContent` is present, the loader converts `supportedContent` and logs a deprecation warning (442-480). With `requiredDlcIds` set, `supportedContent` is ignored. Every repo mod still writes `supportedContent: ALL` and triggers that warning.
- **Enforcement.** `DlcManager.IsCorrectDlcSubscribed` (Mod.cs 354/365/389) checks what the player owns and has installed, not what the save has active. Without DLC4 the mod is marked incompatible and not loaded.
- **Proposed `mod_info.yaml`:**
  ```yaml
  requiredDlcIds:
    - DLC4_ID
  minimumSupportedBuild: 744825
  version: 0.1.0
  APIVersion: 2
  ```
- **Patch targets exist everywhere.** `VineMother`, `VineBranch` and `VineMotherConfig` are in the base `Assembly-CSharp.dll`, so the patches would load even without gating. The gating is about what the player sees (D3).
- **Saves.** In a save without DLC4 content, the vanilla seed never exists (it is DLC-restricted), so no patch ever fires.

### Compatibility (question 6)

- **In this repo:** no mod patches any of the 9 methods. The only other mention is a test, `UpgradePlanner.Tests/SettingKinds.cs`.
- **Likely overlap elsewhere:**
  - Farming and planter mods: `PlantablePlot.SpawnOccupyingObject`/`SetPreview`, `ReceptacleSideScreen`, `PlanterSideScreen`.
  - Pip and seed-planting mods: `PlantableCellQuery`, `SeedPlantingStates`, `PlantableSeed.TryPlant`.
  - Ovagro-specific mods: `VineBranch`.
- **Why the patch style suits overlap:** prefix plus finalizer, no transpilers, and no skipped originals. Another mod's prefix or postfix on the same method still runs, and it sees the ceiling form during the swap.
- The repo's architecture review trigger names "a Harmony patch on a game method another mod is known to patch" (`blk-c8ecd5229a93`). The store records no known conflict for these methods. The harness's `-Steam <id>` option (`blk-4ed03be05da6`) can test next to a specific Workshop mod if the user names one (D6).

### Testability (question 7)

The tests live in `src/OvagroOverhead.Tests/` and follow the `Doors.cs` style: `[OniTest]` with `Layer(0)`, waits on conditions, and game time between steps (`blk-33fb80177cba`, `blk-db81d810e051`). The save round trip follows salty-surf's `Persistence.cs`. Test helpers:
- `t.Dupes.Box()` walls the area with **built** Tiles. Built tiles are `Grid.Foundation`, which `TestSuitableGround` rejects (PlantableSeed.cs 87). Wild-planting tests must therefore lay a row of natural ceiling with `SimMessages.ReplaceElement(cell, SimHashes.SandStone, …)`, the same call `Dupes.Box` uses for air (Dupes.cs 78).
- Keep that row clear of the DevLifeSupport in the box's top-left corner.

| Test | Proves | How |
|---|---|---|
| T1 Registration (`NoColony` or first in a run) | Prefabs exist and vanilla is untouched | `Assets.GetPrefab("VineMotherCeiling")`: `Hanging` tag, OccupyArea (0,0),(0,-1), `UprootedMonitor` (0,1), `SeedProducer.seedInfo.seedId == "VineMotherSeed"`, irrigation and temperature values equal the vanilla prefab's, `HideFromCodex`. The vanilla `VineMother` has no `Hanging` tag. `VineMotherSeed` direction is still Top and `PreviewID` is still `VineMother_preview`. Catches drift after a game update. |
| T2 Pip query | A Pip finds ceiling cells | In a box with a natural ceiling and a built-tile floor: spawn a `Squirrel` and a `VineMotherSeed`, and run `PathFinderQueries.plantableCellQuery.Reset(seed, 20)` through the Pip's `Navigator.RunQuery`. Every result has solid above, and the seed's fields are back to Top afterwards. |
| T3 Wild plant | `TryPlant` spawns the ceiling form | Seed at a ceiling cell, `TryPlant(true)` → a `VineMotherCeiling` whose origin is that cell and which occupies the cell below. Also, a seed on a natural floor still spawns `VineMother` (regression). |
| T4 Pip end to end (`Timeout` ~180, `Explicit` if too slow) | A real Pip plants on the ceiling | Same box, Pip plus seed, `GameTags.Creatures.WantsToPlantSeed` added to the Pip. Wait until a `VineMotherCeiling` exists. The floor is built tiles, so the ceiling is the only valid place. |
| T5 Branches along the ceiling | Vines grow as on a floor | Wild ceiling mother under a flat natural ceiling. Set its maturity amount to max, then each new branch's. Wait until ≥3 branches per side. Each branch is in the ceiling row, with `MyShape == Top` and the first branch's `RootShape == Top`. After a few game seconds none is uprooted. One end of the ceiling stops short, so the vine shows `OutCornerTop*`. |
| T6 Hydroponic Farm FlipV | Duplicant path, preview, irrigation | `t.World.Build("HydroponicFarm", cell, Orientation.FlipV)`. `plot.Direction == Bottom`. `plot.CreateOrder(VineMotherSeed)` gives a preview prefab of `VineMotherCeiling_preview` one cell below with `ValidPlant`. `ForceDeposit(seed)` gives an occupant of `VineMotherCeiling` occupying the 2 cells below, with `RequiresInputConduit` true. The seed prefab's fields are restored. |
| T7 Farm Tile FlipV, and upright regression | Same without irrigation; floor unchanged | As T6 on `FarmTile`. A Neutral farm tile still grows an upright `VineMother`. |
| T8 Side screen (`Layer(1)`) | The player can order it | Select a FlipV farm tile. The Ovagro toggle is enabled, not greyed out, and the description has no ROTATION_NEED_FLOOR text. |
| T9/T10 Round trip (two tests, `FreshSave` on the second) | Save/load | Save with a grown ceiling mother plus branches and a FlipV hydroponic occupant. After loading: prefab ids, positions, branch shapes, plot occupant link, and the mother's anim `Rotation == 180`. |

Not testable by the harness: loading a save **without** the mod. The harness runs with the mod enabled. That check stays manual (see Risks).

## Rationale

- **The game's own pattern.** Hanging plants in this build are separate prefabs with data-level differences: SaltPlant, DewDripper, FlyTrap, Kelp, SpiceVine and IceCavesForage all use the `Hanging` tag, `MakeHangingOffsets` and an up-facing `UprootedMonitor`. Every vanilla component already reads those offsets and tags: entomb, drowning, pressure, uproot, box collider, harvest icon, pollination VFX and preview validity. A second prefab gets all of them without a patch.
- **One seed, chosen at planting time.** The user sees one Ovagro seed, and branches keep dropping it. The form is decided where the game already decides placement: the Pip's cell query, the planting spawn, the plot spawn and preview, and the side screen.
- **The geometry rule removes the hard question.** A cell can host only one form, so there is no state to save about the form other than the prefab id itself.
- **Branch logic needs a single constant changed.** All four orientations already exist in VineBranch.

## Alternatives rejected

- **Per-instance orientation on the vanilla `VineMother` prefab** (a serialised flag flips offsets, uproot cells, tag and art at spawn).
  - **What the player sees:** the same as the recommended design.
  - **Patch count:** larger. Every seed-side patch is still needed, because the seed is still Top. On top of that:
    - `TestSuitableGround`: it reads the *prefab's* `UprootedMonitor` and `OccupyArea` (PlantableSeed.cs 115-123).
    - The preview: `VineMother_preview` has floor offsets.
    - `OccupyArea` cell registration timing: the offsets must change before `OnSpawn` registers cells.
    - `UprootedMonitor` setup.
    - `KBoxCollider2D`.
    - The `Hanging` tag at runtime.
  - **Save:** adds a mod component to a vanilla prefab.
  - **Mod removed:** the plant reloads upright in a ceiling cell, entombed in the tile above. A wild one is uprooted at once.
  - **Why rejected:** more patches on hotter code, and the failure on mod removal is no better.
- **A second seed item (`VineMotherCeilingSeed`)**.
  - **What the player sees:** two Ovagro seeds.
  - **Seed drops:** branches can drop only one seed type (`SeedProducer.Configure`, VineBranchConfig.cs 75), so the ceiling seed would need its own source or a conversion recipe.
  - **Why rejected:** it changes the game's economy and adds strings and translations. It also contradicts "one seed".
- **Cloning the vanilla plant prefab with `Object.Instantiate`**, as the game does for compost prefabs (EntityTemplates.cs ~924-940).
  - **Pro:** it would follow vanilla tuning automatically.
  - **Con:** `StateMachineController` defs are held through a shared handle (StateMachineController.cs 108-136), and a clone of a plant with several state machines has no precedent in the game.
  - **Why rejected:** an explicit factory, as SaltPlantConfig is, is predictable. T1 catches drift.
- **Art via `FlipY` + `Offset (0,1)`.** `PlantablePlot.PositionOccupyingObject` overwrites `Offset` (PlantablePlot.cs 292-298), so plot plants would render one cell off unless one more method were patched.
- **New hanging art.** Not needed. The flipped vanilla art reads correctly (rendered, see above), and drawing new Klei-style art is out of scope.
- **Ceiling Ovagro nodes in world gen.** Out of the stated feature ("planted by Pips … and by Duplicants").

## What the player sees (question 1)

- **Seed item:** unchanged (`VineMotherSeed`). Its tooltip shows no direction requirement, as in vanilla.
- **Codex:** unchanged. The ceiling form redirects to the Ovagro Node entry and is hidden from the codex and the sandbox spawner.
- **Planter side screen, downward Farm Tile or Hydroponic Farm:** Ovagro is selectable, the preview hangs below the tile, and the description lists "Requires ceiling".
- **Upward plots:** exactly vanilla.
- **Wild:** Pips plant Ovagro on natural floors as before, and now on natural ceilings too.
- **Selecting a ceiling plant:** shows "Ovagro Node", the same name and description.
- **Branch seed drops:** unchanged. Branches drop `VineMotherSeed`, and the ceiling mother's `SeedProducer` names the same seed.
- **Wild world-gen nodes:** unchanged (floor).

## Risks

1. **Mod removed from a save with ceiling Ovagro.**
   - `SaveManager.Load` skips the unknown prefab with the warning "Could not find prefab 'VineMotherCeiling'" (SaveManager.cs 318-322), so the ceiling mothers are gone.
   - Their branches hold a null `Mother` and should die through `ParamTransition(Mother, dead, IsNull)` (VineBranch.cs 1188). That they actually die on load is not verified.
   - A downward plot that held one keeps `requestedEntityTag = VineMotherSeed`. On load it re-orders the seed with the vanilla Top preview, which sits **above** the downward tile. If that space is open, a Duplicant can plant an upright Ovagro that renders under the tile, with its upper cell inside it.
   - Mitigation: Workshop and README text telling players to clear Ovagro from downward tiles before removing the mod. Nothing can run once the mod is gone.
2. **Prefab-field swaps (#5-#8).** These rely on the calls being synchronous and on nothing caching the seed's direction or ids during the call. Code read: true for all four methods. A future game change that caches would show up as a wrong preview or toggle. T6-T8 cover it.
3. **First-branch timing (#9).** This relies on the branch state machine starting after `SetupRootInformation` (inferred from `KMonoBehaviour.Start`). T5 covers it.
4. **Rendering of the rotated mother:** culling bounds and layering with a 180° rotation on the BuildingFront layer. VineBranch already uses rotation on the same layer. T9/T10 assert the rotation, and a `t.Screenshot` in T5 gives a visual check.
5. **Game updates.** Two of the patched methods are private: `PlantableCellQuery.CheckValidPlotCell` and `PlantablePlot.OnCopySettings`. One is private static: `SeedPlantingStates.CheckValidPlotCell`. ModCheck and the harness's patch-time failure report a renamed member, and the patch class should log when its target is missing (repo error-handling convention, per unit 02).
6. **DLC4-only Workshop audience.** With `requiredDlcIds`, players without DLC4 see the mod as incompatible. That is accurate, but it is the first repo mod to declare it, and the release and ModTest tooling has not been exercised with it.

## Open decisions for the user

- **D1. The Pip's choice between floor and ceiling.**
  - (a) No preference: the Pip takes one of the 20 nearest valid cells at random, floor or ceiling, as vanilla does among floor cells. No extra patch.
  - (b) Prefer floor; ceiling only when no floor cell is valid. One more patch, on `SeedPlantingStates.FindDirtPlot`.
  - (c) Prefer ceiling.
  - **Pick (a):** it is the game's own rule extended to the new cells, and it needs no extra code.
- **D2. Seed text.**
  - (a) No new text. The planter side screen shows the vanilla "Requires ceiling" on a downward plot, and the Workshop description explains the rest.
  - (b) Add a descriptor such as "Can also grow on ceilings" to the seed and the codex. This needs `ModStrings`, `UseTranslations`, a `.pot` and zh/ko/ru `.po` files (unit 02 §7).
  - **Pick (a):** the only feature is placement, and (a) keeps the mod textless like just-fall-already.
- **D3. DLC gating.**
  - (a) `requiredDlcIds: [DLC4_ID]`, dropping `supportedContent`. New in this repo.
  - (b) `supportedContent: ALL` like every other mod. It loads everywhere and does nothing without DLC4, and it keeps the deprecation warning.
  - **Pick (a):** it is the loader's current field, and it tells players the truth.
- **D4. Id of the ceiling prefab.** It is saved into saves and cannot be renamed after release.
  - (a) `VineMotherCeiling` / `VineMotherCeiling_preview`.
  - (b) A mod-prefixed id such as `OvagroOverhead_VineMother`.
  - **Pick (a):** it is readable in logs and matches the game's `<Plant><Variant>` ids (`FlowerVaseHanging`, `IceCavesForagePlantPlanted`). The collision risk is small.
- **D5. Copy Settings between downward tiles (patch #8).**
  - (a) Include it.
  - (b) Leave it out; Copy Settings then skips Ovagro on downward tiles.
  - **Pick (a):** it is one small patch, and it keeps the downward tile behaving like the upward one.
- **D6. Compatibility runs.**
  - (a) Run the suite once with `-Steam <id>` next to any farming or Pip mod the user plays with.
  - (b) None.
  - **Pick (a)** if the user names one; otherwise (b).

## Corrections to unit 01

- **Pips walk on walls and ceilings** (user's correction). Unit 01 read only the spawn nav type, `NavType.Floor` in `BaseSquirrelConfig`. The Pip's `SquirrelNavGrid` (GameNavGrids.cs 2111-2203) has Floor, LeftWall, RightWall and Ceiling nav types with ceiling walking and corner transitions. The open question "Pip reachability of ceiling cells" is closed: a Pip reaches every ceiling cell it can path to, and vanilla Saltvine planting depends on that.
- **A Top seed cannot be ordered into a downward farm tile through the UI.** `SingleEntityReceptacle.IsValidEntity` does skip the direction check for rotatable plots (unit 01 was right there). But `ReceptacleSideScreen.ValidRotationForDeposit` (ReceptacleSideScreen.cs 666-672) disables the toggle and `CanDepositEntity` (410-420) when the seed's direction differs from a rotatable plot's. That is why patch #6 is needed.
- **The Top-seed preview on a downward tile sits above the tile, not inside it.** `PlantablePlot.SetPreview` positions a Top seed's preview at the unrotated `(0,1)` (PlantablePlot.cs 405-409). `Rotatable` rotates only the anim (Rotatable.cs 148-154), not the transform. So the preview's validity depends on the cells above a downward tile.
- **VineMother line numbers.** VineMother.cs has 269 lines. `AttemptToSpawnBranches` is at 103-136, `SpawnBranchOnCell` at 150-157, and the 4000 ms update at line 207. Unit 01 gave 186-219, 233-240 and 290.

## Store reads

`ks` runs, all with `node c:/dev/ai/icon/bin/ks.mjs`, store `C:/dev/ONI-Mods`:

1. `read blk-9d52ccfdb3df blk-7d0c65d332f3 blk-5b62e7507604 blk-76e79f89b3c4` (packet pointers)
2. `tags --store C:/dev/ONI-Mods`
3. `brief --at C:/dev/ONI-Mods --for .github/workflows/label-mod.yml mods/just-fall-already/src --decisions --budget 14914` (the packet's full selection; no decision records came back)
4. `brief --at C:/dev/ONI-Mods --tag 'in-game-testing || test-pacing || test-assertions || test-cleanup' --decisions`
5. `read blk-33fb80177cba blk-db81d810e051 blk-fb116707eda5 blk-a66da6f9aaa2 blk-4ed03be05da6 blk-46b28a0040e8 blk-5e77d0d9ce90 blk-4224fccba710 blk-66e912fbb652`
6. `brief --at C:/dev/ONI-Mods --tag 'mod-compatibility || save-files || game-update-compatibility' --decisions`
7. `read blk-c8ecd5229a93 blk-c8a18ea4088d blk-e1cba092696e blk-b6ca8e2d3bd3`

Pointers that informed the design:
- `blk-c8ecd5229a93` (architecture review triggers): a new mod needs the solution entry, the issue-label map in `.github/workflows/label-mod.yml`, the README table and the release plan. A patch on a method other mods patch, or a change to what a save stores, is a review trigger. This shaped the Compatibility and Risks sections. The issue templates `bug_report.yml` and `feature_request.yml` also list mod names and need the new one (read as files).
- `blk-33fb80177cba`, `blk-db81d810e051` (pace tests to game time; wait on conditions), `blk-fb116707eda5` (`AllowLogError` only where provoked), `blk-4224fccba710` (assert what the game's API shows): the test plan.
- `blk-a66da6f9aaa2`, `blk-66e912fbb652` (fixture: testbed-so.sav, every DLC; area cleared between tests): DLC4 is active in the fixture, and tests build their own natural ceiling.
- `blk-c8a18ea4088d` (`NoColony`, `FreshSave`, `Explicit`, `Timeout`): T1, T4 and T9/T10.
- `blk-4ed03be05da6` (`-Steam <id>` for other authors' mods): D6.
- `blk-46b28a0040e8` (test error paths, including a save made without the mod): the risk 1 note and the manual check.
- `blk-5e77d0d9ce90` (ModCheck after game updates): risk 5.
- `blk-e1cba092696e` (Layer 0/1): T8 is the one Layer 1 test.
- `blk-b6ca8e2d3bd3` (verification commands): the test plan's run commands.
- `blk-9d52ccfdb3df`, `blk-7d0c65d332f3` (just-fall-already's structure, `Doors.cs` as the test template), `blk-76e79f89b3c4` (naming), `blk-5b62e7507604` (imports): code conventions for the patch class.

Read as files, not through the store: plan.md, units/01-explore-report.md, units/02-explore-report.md, `.github/workflows/label-mod.yml`, `.github/ISSUE_TEMPLATE/*.yml`, `mods/salty-surf/src/SaltySurf.Tests/Persistence.cs`, `common/tools/TestRunner/Kit/Dupes.cs`, `Kit/World.cs` and `Kit/TestContext.cs` (signatures), and `common/tools/Kanim/kanim.py`. Repo greps covered `IEntityConfig`, `Finalizer`/`__state`, string-named `HarmonyPatch` targets, and overlapping patches.

Report-back on the packet:
- **Did not apply:** the ~50 code-style "Example", "Loops" and "Variables" records copied across 15 other mods. Only just-fall-already's own copies mattered.
- **Missed by the path brief:** everything that governs this work. That is the architecture review trigger `blk-c8ecd5229a93`, the testing records and the save round-trip pattern. They were reached only through tag briefs. The scope (`mods/just-fall-already/src`, a template for a mod that does not exist yet) cannot reach subject records.
- **Reason lines:** none was wrong. The code-style pointers' reason ("names …/FallPoke.cs") is true but says nothing about relevance.

## ICON friction

- **Token-shared tag matching in OR queries.** `ks brief --tag 'mod-compatibility || save-files || game-update-compatibility'` listed nearly every `in-game-testing` record as "tagged in-game-testing → game-update-compatibility". It also cross-mapped records like "tagged mod-build → mod-compatibility". A term is matched on any shared token ("game", "mod"), so the OR query returned the same broad testing set as the previous query and did not narrow it. I expected an exact match on the term. Instead I read the few records whose own tags fit and ignored the rest.
- **The packet's path brief was 8 KB of duplicate per-mod code-style records** (the same issue as unit 02's G9). `--decisions` returned no decision records. Everything relevant came from tag briefs I ran myself.
- No skill, hook or `ks read` failures. Multi-id `ks read` worked on the first try each time.
- Not ICON (repo tooling, for the record): `common/tools/Kanim/kanim.py extract` reads only `sharedassets0.assets`, so it cannot extract DLC art. DLC4 kanims are in `StreamingAssets/dlc4_bundle`.
