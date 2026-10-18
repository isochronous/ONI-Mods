# Unit 10 report: S6 + S9 farm plot tests T6-T8 and save round trip T9/T10 (PASSED in game)

## Result
- Full suite `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead`: **exit 0, 16 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped** (127.4 s real, fixture loaded again 2 times).
- Run folder: `C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261018-003145`
- Build first: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`, 0 errors. The 5 warnings are all in other mods' test projects (NaturalBackwalls, StoragePodRedux, VentFreezeFix); none in Ovagro Overhead.
- Commit: `30c075c OM-006: farm plot and save tests` (Plots.cs and Persistence.cs new; Kit.cs and OvagroOverhead.Tests.csproj modified), with the Opus 5.5 and session trailers.
- After the run the game process was gone; Documents `mods.json` hash equals the run's `mods.json.real` (1158ABF2...). No game was running before any of my runs.

results.jsonl (status lines; notes trimmed to the informative ones):
```
{"id": "OvagroOverhead.Tests/Persistence.WriteRoundTrip", "status": "pass", "message": null, "durationSeconds": 6.65, "gameSeconds": 18.33, "logErrors": []}
    left: (120,191) Top grown, (119,191) Top grown, (118,191) Top; right: (122,191) Top grown, (123,191) Top grown, (124,191) Top
{"id": "OvagroOverhead.Tests/Persistence.RoundTripLoad", "status": "pass", "message": null, "durationSeconds": 4.07, "gameSeconds": 8.19, "logErrors": []}
    fixture area after the load: 40x10 at (100, 175)
{"id": "OvagroOverhead.Tests/Plots.HydroponicFarmDownward", "status": "pass", "message": null, "durationSeconds": 5.35, "gameSeconds": 16.03, "logErrors": []}
    grown; wilting False; plant status: none; plot status: Normal
{"id": "OvagroOverhead.Tests/Plots.FarmTileDownward", "status": "pass", "message": null, "durationSeconds": 3.39, "gameSeconds": 10.17, "logErrors": []}
    grown; wilting False; plant status: none; plot status: Normal, MaterialsUnavailable
{"id": "OvagroOverhead.Tests/Plots.FarmTileUpright", "status": "pass", "message": null, "durationSeconds": 7.01, "gameSeconds": 7.01, "logErrors": []}
{"id": "OvagroOverhead.Tests/Plots.FarmTileNoRoomBelow", "status": "pass", "message": null, "durationSeconds": 5.01, "gameSeconds": 5.01, "logErrors": []}
{"id": "OvagroOverhead.Tests/Plots.CopySettings", "status": "pass", "message": null, "durationSeconds": 11.01, "gameSeconds": 11.01, "logErrors": []}
{"id": "OvagroOverhead.Tests/Plots.SideScreen", "status": "pass", "message": null, "durationSeconds": 7.94, "gameSeconds": 7.94, "logErrors": []}
    toggle states: Ovagro 0, Mealwood 2 (0 enabled, 1 selected, 2 greyed out, 3 greyed out and selected)
{"id": "OvagroOverhead.Tests/Registration.Prefabs", "status": "pass", ...}
{"id": "OvagroOverhead.Tests/Registration.NaturalCeilingHelper", "status": "pass", ...}
{"id": "OvagroOverhead.Tests/Vines.AlongTheCeiling", "status": "pass", "durationSeconds": 9.62, "gameSeconds": 28.86, "logErrors": []}
{"id": "OvagroOverhead.Tests/Vines.AlongTheFloor", "status": "pass", ...}
{"id": "OvagroOverhead.Tests/WildPlanting.QueryFindsCeilingCells", "status": "pass", ...}
{"id": "OvagroOverhead.Tests/WildPlanting.PlantUnderCeiling", "status": "pass", ...}
{"id": "OvagroOverhead.Tests/WildPlanting.PlantOnFloor", "status": "pass", ...}
{"id": "OvagroOverhead.Tests/WildPlanting.PipPlantsOnCeiling", "status": "pass", "durationSeconds": 11.19, "gameSeconds": 33.54, "logErrors": []}
results.jsonl.status: {"status":"done","detail":"16 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped",...,"reloads":2,"realSeconds":127.4}
```

Earlier runs while iterating:
- `run-20261018-002330` (`-Filter "*PLOT*"`, exit 1): 3 passed, 2 errors, 1 failure. The two errors were a NullReferenceException in `VineMother.Instance.IsGrown`: that property reads a `Growing` component the Ovagro Node does not have (vanilla, both forms). The fix was the new `Kit.IsGrown`, which reads the state machine's own `IsGrown` flag. The failure was in the side screen test, which waited for the colony to count the seeds (see the next item).
- `run-20261018-002526` (`*PLOT*`, exit 1): 5 passed. The side screen test still failed. The world inventory counts an item only once a Duplicant can reach it, and the fixture has no Duplicants. The test now spawns one Duplicant. It cannot reach the tile under the ceiling, so the order stays pending.
- `run-20261018-002714` (`*SideScreen*`, exit 0) and `run-20261018-002913` (`*PERSIST*;*PLOT*`, exit 0, 8 passed): after these I added status logging and the scroll-to-toggle step for the picture.

## What was written (mods/ovagro-overhead/src/OvagroOverhead.Tests/)
- `Plots.cs`, prefix `PLOT:`. Every test is Boxed. A downward plot sits in the box's top row, against the built ceiling, unless a Duplicant must reach it.
  - T6 `HydroponicFarmDownward` (Speed 3, `FreshSave`, see below). A FlipV Hydroponic Farm at inside (10,3) with a liquid pipe on its cell. `CreateOrder(VineMotherSeed)`. Checks:
    - The order exists, and the preview is `VineMotherCeiling_preview`, valid, in the cell below.
    - A **real Duplicant** fetches the seed from the floor and delivers it.
    - The occupant is `VineMotherCeiling`, with its origin in the cell below and registered in both cells below. It is the plot's `plant`, its `ReceptacleMonitor.Replanted` is true, its anim is `Rotation` 180 with `FlipX`, and it is rooted.
    - The seed is used up and the preview is gone. `RequiresInputConduit` is true. The seed prefab is back to vanilla.
    - With water in the farm's storage it grows: no wilting, no plant status. Screenshot.
  - T7a `FarmTileDownward`. A FlipV Farm Tile under the box ceiling, so the cell above it is solid. The vanilla preview would sit there and be refused. The order gives the hanging preview below, valid. `ForceDeposit` gives the hanging occupant, with the same checks as T6 and the errand done. It grows. Screenshot.
  - T7b `FarmTileUpright` (regression). A Neutral Farm Tile: the vanilla preview `VineMother_preview` above the tile, then a `VineMother` above that occupies the next cell up, with `Rotation` 0.
  - T7c `FarmTileNoRoomBelow` (counter path of the preview). A built tile two cells below a FlipV tile: no errand, no requested seed, no preview left behind.
  - T7d `CopySettings`. Uses `CopyBuildingSettings.ApplyCopy`, the call the Copy Settings tool makes for each target. Four copies are checked:
    - planted downward tile to empty downward tile: hanging order;
    - ordered downward tile to empty downward tile: hanging order;
    - upright tile ordered with Ovagro to downward tile: hanging order;
    - counter path, planted downward tile to upright tile: standing order, vanilla preview above.

    The seed prefab is checked back to vanilla after the copies.
  - T8 `SideScreen` (Layer 2, UI, the kit's numbering; the plan said Layer 1). It selects the FlipV tile through `SelectTool.Select` and finds the open `PlanterSideScreen`. It reads each toggle's seed from the screen's `depositObjectMap` (Traverse), with an Ovagro seed and a Mealwood seed in the colony. Checks:
    - The Ovagro toggle state is 0 (enabled).
    - The Mealwood toggle state is 2 (greyed out): the counter path, a floor seed that is not Ovagro on a downward tile.
    - Clicking the Ovagro toggle (an `ExecuteEvents` pointer click, as in upgrade-planner's MaterialChange tests) selects it (state 1) and makes the order button interactable.
    - The description has no "Requires upward plot orientation". The requirements list "Plot Orientation: Downward". The preview is the hanging one below the tile.
    - Screenshot.
    - `SignalClick` on the Plant button places the hanging order. The seed prefab is back to vanilla.
- `Persistence.cs`, prefix `PERSIST:`. Salty Surf's two-test pattern:
  - T9 `WriteRoundTrip` (Boxed, Speed 3):
    - Setup: a natural sandstone strip (x 14..26) with a wild `VineMotherCeiling` under it at (20, strip-1), grown to 3 branches each side. Also a FlipV Hydroponic Farm at (6,3), with a pipe, planted (order then `ForceDeposit`), watered, and grown.
    - Before saving it asserts every branch is `Top`.
    - It writes the mother, farm, farm plant and branch cells and shapes to `ovagro-roundtrip.txt` in the run folder. It saves the colony to `ovagro-roundtrip.sav` and sets `Session.SavePath`.
  - T10 `RoundTripLoad` (`FreshSave`) checks the loaded save:
    - The wild mother is at its cell, hanging into the cell below, `Rotation` 180 with `FlipX`, rooted and grown.
    - Both chains have the same count, cells and shapes, are rooted, and the first branch has `RootShape` Top.
    - The farm faces down and its occupant is the ceiling Ovagro at the saved cell, in both cells below. The farm's `plant` link is restored, `Replanted` is true, `RequiresInputConduit` is true, and the art is turned over.
    - The seed prefab is vanilla.
    - After 6 more game seconds it checks again: nothing uprooted, the chains unchanged, the farm still holding its plant. Screenshot.
    - Cleanup restores the fixture path and deletes both files.
- `Kit.cs` additions:
  - `GrowMother` and `GrowChains`: the same logic as Vines.cs's private copies, which I left untouched because Vines.cs was out of this unit's file list.
  - `IsGrown(VineMother.Instance)`, `AssertSeedPrefabIsVanilla`, `Plot(t, id, cell, orientation)`, `PreviewOf(plot)` (the plot's child `EntityPreview`, since `plantPreview` is private), `PreviewName`, `Water(plot, kg)` and `Statuses(go)`.
- `OvagroOverhead.Tests.csproj`: `<ReferenceUnityUI>true</ReferenceUnityUI>` for `PointerEventData`/`ExecuteEvents`, as upgrade-planner's test project has.
- Suite ordering: classes run by full name, so Persistence runs first and leaves the round-trip save loaded. The first Plots test is `FreshSave = true`, which reloads the fixture so every later test finds the colony as before. A filtered run that starts at Plots reloads nothing. The full run's log confirms it: the area after the round-trip load was (100,175); after the Plots reload it was back at (100,185).

Coverage evidence block:
- PlotPatches #4 `SpawnOccupyingObject` (swap when FacesDown) -> T6 (Duplicant delivery), T7a, T7d, T9 (ForceDeposit). Counter (upright, no swap) -> T7b.
- PlotPatches #5 `SetPreview` (seed tag on a FacesDown plot) -> T6, T7a, T8, plus every hanging-order assert in T7d. Validity tests the cells below -> T7c. The tile under a solid ceiling still gets the order -> T7a. Upright unchanged -> T7b and the T7d counter.
- PlotPatches #8 `OnCopySettings` (FacesDown target) -> T7d (three sources). Counter (upright target) -> T7d.
- SideScreenPatches #6 `ReceptacleSideScreen.Initialize` -> T8 (Ovagro state 0). Counter (Mealwood stays 2) -> T8.
- SideScreenPatches #7 `SetResultDescriptions` -> T8 (no ROTATION_NEED_FLOOR; "Plot Orientation: Downward" listed).
- Save/load of the ceiling prefab, branch shapes, the plot link and the art -> T9/T10.
- Intentionally untested: an upright tile's side screen with Ovagro (vanilla path, no swap), and loading a save without the mod (the harness cannot; manual, per the design).

## Screenshots
Originals in `C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261018-003145\artifacts\` (2560x1440). Crops (Pillow) are in `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\u10\`.
- `OvagroOverhead.Tests.Plots.HydroponicFarmDownward-hydroponic-downward.png` (crop `u10\final-hydroponic.png`):
  - The Hydroponic Farm is turned over: its yellow arrow points down.
  - The Ovagro's pineapple-like caudex sits directly under it, with the green leaf tendrils at its top and the pink leaves hanging below. It is a clean mirror of the floor plant.
  - There is no status icon: grown, irrigated, not wilting.
  - The delivering Duplicant stands in the bottom right corner.
- `OvagroOverhead.Tests.Plots.FarmTileDownward-farm-tile-downward.png` (crop `u10\final-farm-tile.png`):
  - The flipped Farm Tile is tucked under the box's white tile ceiling, with the same hanging Ovagro below it.
  - A red icon sits over the caudex. That is the farm tile's own `MaterialsUnavailable` status (logged on the plot, not the plant): the tile's water delivery errand finds no water in the colony, because the test put the water straight into the storage. It is vanilla Farm Tile behaviour, not a mod issue. The plant itself has no status.
- `OvagroOverhead.Tests.Plots.SideScreen-side-screen.png` (crop `u10\final-side-screen.png`):
  - The Farm Tile Seeds screen is open on the downward tile. The Ovagro Node toggle is highlighted (selected) with count 1, beside Dasha Saltvine, Oxyfern and others at 0. The list was scrolled to it.
  - The header reads "PLANT: Ovagro Node Seed", with "This plant produces Ovagro Fig." and the growth requirements below it. "Plot Orientation: Downward" is below the fold of that panel; the test asserts it in the panel's text. The Plant button is shown.
  - In the world, the white outline of the hanging preview hangs below the downward tile, with the awaiting-delivery icon.
- `OvagroOverhead.Tests.Persistence.RoundTripLoad-loaded.png` (crop `u10\final-loaded.png`): the loaded save.
  - Left: the flipped Hydroponic Farm with its grown hanging Ovagro.
  - Right: the sandstone strip with the wild mother hanging from its underside, and green vine segments along the underside on both sides.
  - The box floor shows at the bottom. Nothing is uprooted or displaced.

## Mod bugs found
None. Every assertion on the mod's behaviour passed the first time it was reached. The three failures while iterating were test-side: the vanilla `IsGrown` property, which needs a component the Ovagro mother lacks; the world inventory needing a Duplicant to count items; and the missing UnityUI reference.

## Player.log findings (run-20261018-003145\Player.log)
- `[04:32:03.628] [1] [INFO] [OvagroOverhead] Loaded version 0.1.0.0`
- 0 lines contain "Exception".
- No warning mentions Ovagro Overhead. Every test's `logErrors` is `[]`.
- In the earlier failing run the NullReferenceException was raised inside the test's wait condition (results.jsonl stack), not in game code paths running on their own.

## Store reads
Runs (cwd C:/dev/ONI-Mods, `node c:/dev/ai/icon/bin/ks.mjs`):
1. `read blk-960faf85db39 blk-33fb80177cba blk-db81d810e051 blk-41ffba520973 blk-3753681f851b blk-976d76b5a752 blk-a66da6f9aaa2 blk-46b28a0040e8 blk-0194e02d0fd6`: all 9 resolved in one call (7637 B).
2. `brief --store common --for common/tools/TestRunner`: 8 pointers. None opened further; I read the kit sources directly (TestContext, World, Tools, Dupes, SaveFile, Attributes, FixtureArea, Session, TestBehaviour).

What informed the work:
- blk-960faf85db39: run commands, `-Filter` (including `;`-separated globs), exit codes.
- blk-33fb80177cba and blk-db81d810e051: game seconds after every build, deposit, copy and click; waits on conditions.
- blk-a66da6f9aaa2, blk-976d76b5a752 and blk-3753681f851b: the box, area clearing, `FreshSave` and `t.Cleanup`. This is why the first Plots test reloads the fixture.
- blk-0194e02d0fd6: the Assert API.
- blk-46b28a0040e8: check Player.log.

blk-41ffba520973 was read and added nothing.

Report-back on the packet:
- Did not apply: the three salty-surf standards pointers (code style, error handling, naming). The scope named salty-surf's test folder only as the round-trip example, and its Persistence.cs was what mattered.
- Missed: nothing governing; the testing records came through the caller's ids.
- Reason lines: accurate.

Read as files (task scratch, code): handoff-10.md, units/10-test.md, units/04-plan-report.md, units/03-design-report.md, units/08-test-report.md and units/09-test-report.md. Also the mod source (CeilingForm, PlotPatches, SideScreenPatches, VineMotherCeilingConfig), the existing tests, salty-surf Persistence.cs, detect-any-element SideScreen.cs, the upgrade-planner README and csproj (plus a grep of its click helpers), and decompiled game sources in the scratchpad.

## ICON friction
- None blocking. `ks read` with 9 ids and `ks brief --store common` worked first time.
- The packet's path brief again pointed at another mod's code-style records (salty-surf's) because salty-surf's test folder was in scope as an example. The records governing a test unit came only from the ids in the task message.

## Not done / next
- Nothing open for S6/S9.
- Vines.cs could switch to `Kit.GrowMother`/`Kit.GrowChains` (an identical copy now lives in Kit). It was left alone as out of this unit's file list.
