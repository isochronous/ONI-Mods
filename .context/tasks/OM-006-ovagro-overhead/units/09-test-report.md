# Unit 09 report: S5 Pip and vine tests T2-T5 (PASSED in game)

## Result
- Full suite `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead`: **exit 0, 8 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped** (64.7 s real).
- Run folder: `C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261018-000941`
- T4 (real Pip) is **not** Explicit. It planted in 7 s real (8 s in the first run), with a 180 s budget.
- Build first: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`, 0 errors. The 5 warnings come from other mods' test projects (NaturalBackwalls, StoragePodRedux, VentFreezeFix); none from Ovagro Overhead.
- Commit: `971d9f3 OM-006: Pip and vine tests` (Kit.cs modified; WildPlanting.cs and Vines.cs new), with the Opus 5.5 and session trailers.
- After the run the game process was gone. Documents `mods.json` hashes equal to the run's `mods.json.real`, and no `.onitest-backup` was left.

results.jsonl, status lines (notes trimmed to the informative ones):
```
{"id": "OvagroOverhead.Tests/Registration.Prefabs", "status": "pass", "message": null, "durationSeconds": 0.0, "gameSeconds": 0.0, "logErrors": []}
{"id": "OvagroOverhead.Tests/Registration.NaturalCeilingHelper", "status": "pass", "message": null, "durationSeconds": 3.09, "gameSeconds": 3.09, "logErrors": []}
{"id": "OvagroOverhead.Tests/Vines.AlongTheCeiling", "status": "pass", "message": null, "durationSeconds": 9.69, "gameSeconds": 29.07, "logErrors": []}
    left: (120,191) Top grown, (119,191) Top grown, (118,191) Top grown, (117,191) OutCornerTopRight
    right: (122,191) Top grown, (123,191) Top grown, (124,191) Top
    left after the corner grew: ... (117,191) OutCornerTopRight grown, (117,192) Right
{"id": "OvagroOverhead.Tests/Vines.AlongTheFloor", "status": "pass", "message": null, "durationSeconds": 2.71, "gameSeconds": 8.12, "logErrors": []}
    left: (120,187) Bottom; right: (122,187) Bottom
{"id": "OvagroOverhead.Tests/WildPlanting.QueryFindsCeilingCells", "status": "pass", "message": null, "durationSeconds": 5.03, "gameSeconds": 5.02, "logErrors": []}
    20 planting cells, all in row 191 (under the strip, x 105..134)
{"id": "OvagroOverhead.Tests/WildPlanting.PlantUnderCeiling", "status": "pass", "message": null, "durationSeconds": 9.07, "gameSeconds": 9.07, "logErrors": []}
{"id": "OvagroOverhead.Tests/WildPlanting.PlantOnFloor", "status": "pass", "message": null, "durationSeconds": 7.02, "gameSeconds": 7.02, "logErrors": []}
{"id": "OvagroOverhead.Tests/WildPlanting.PipPlantsOnCeiling", "status": "pass", "message": null, "durationSeconds": 9.17, "gameSeconds": 27.5, "logErrors": []}
    planted at 121608 (132, 191) after 7 s
results.jsonl.status: {"status":"done","detail":"8 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped",...,"realSeconds":64.7}
```

Earlier runs while iterating:
- `run-20261018-000602` (full suite, exit 1): 6 passed. Both Vines tests failed on `VineMotherCeiling has no maturity amount` / `VineMother has no maturity amount`. That was my test's fault: the Ovagro mother has no Maturity amount. Its state machine plays its grow animation once, marks itself grown, then puts out branches. The fix waits for the branches instead of setting maturity.
- `run-20261018-000738` (`-Filter "*VINES*"`, exit 0): 2 passed. After it I added the step that grows the corner branch for the picture.

## What was written (mods/ovagro-overhead/src/OvagroOverhead.Tests/)
- `Kit.cs` additions:
  - `HeldSeed(t, cell)`: a seed made with `Pickupable.handleFallerComponents = false`, so it stays in a cell under a ceiling the way a seed in a Pip's mouth does. `TryPlant` reads only the seed's cell.
  - `Pip(t, cell)`
  - `AssertReaches(t, pip, cell, what)`: waits up to 10 s for `Navigator.CanReach(cell)`, which is `GetNavigationCost != -1`, and fails at once with the Pip's cell and the target named.
  - `PlantAt(cell)`, `Is(go, tag)`, `Chain(firstBranch)`, `Describe(chain)`.
- `WildPlanting.cs`. Every test lays the natural strip one row under the box's built ceiling, x 4..33 with open ends. Pips spawn **on top of the strip**:
  - T2 `WILD: a Pip looking for a place for an Ovagro seed finds the cells under a natural ceiling`. Asserts the Pip reaches a cell under the strip, then runs `PathFinderQueries.plantableCellQuery.Reset(seed, 20)` through the Pip's `Navigator.RunQuery`. Checks: results are not empty; every result has natural (non-Foundation) solid above, sits in the row under the strip, and has open space below; the world seed and the seed prefab are back to Top / `VineMother` / `VineMother_preview`.
  - T3a `WILD: an Ovagro seed planted under a natural ceiling grows the hanging Ovagro`. A held seed under the strip, then `TryPlant(true)`. Checks: a `VineMotherCeiling` on the building layer at the seed's cell and the cell below; the seed used up; the prefab seed restored; anim `Rotation == 180` and `FlipX`; still rooted 3 game seconds later; screenshot.
  - T3b `WILD: an Ovagro seed planted on a natural floor still grows the standing Ovagro` (the floor regression). A seed lying on natural sandstone, then `TryPlant(true)`. Checks: a vanilla `VineMother` at the cell and the cell above; the prefab seed restored.
  - T4 `WILD: a Pip carries an Ovagro seed onto the ceiling and plants the hanging Ovagro` (Timeout 180, Speed 3). The Pip and the seed are both on top of the strip. Asserts the Pip reaches the underside and the seed. Then, each game second, it sets `SeedPlantingMonitor.Instance.nextSearchTime = 0` until a `VineMotherCeiling` exists, failing with the Pip's cell and nav type after 160 s. Checks: the plant is in the row under the strip and within its x range, and occupies the cell below; no standing `VineMother` anywhere in the box; the prefab seed restored; the plant is rooted; screenshot.
- `Vines.cs`:
  - T5 `VINES: a ceiling Ovagro's vines grow along the underside of the ceiling and turn its end` (Timeout 200, Speed 3). The strip runs from 3 cells left of the mother to x W-3. A `VineMotherCeiling` is spawned at (20, row under the strip). The test waits for both first branches, then sets full maturity on every branch of each side until the left side has 4 branches and the right side 3. After 5 game seconds it checks:
    - both first branches have `RootShape == Top`;
    - left 1-3 and right 1-3 are at the expected cells in the ceiling row with `MyShape == Top`;
    - left 4, one cell past the strip's end, is `OutCornerTopRight`;
    - the mother and all branches are not `Uprooted`.
    Then it grows the corner branch, waits 6 game s, re-asserts its shape and that the chain is rooted, and takes two screenshots.
  - Counter path for patch #9: `VINES: a floor Ovagro's vines still lie along the floor`. A vanilla mother on a natural floor; both first branches have `RootShape == Bottom` and `MyShape == Bottom` at (mx±1, 1).

Coverage evidence block:
- PlantingPatches #1 (`PlantableCellQuery.CheckValidPlotCell`, swap when solid above) -> T2 (cells found under the strip; seed restored). Counter: cells on top of the strip have the built ceiling (Foundation) above, are swapped and rejected, and none appears in T2's results.
- PlantingPatches #2 (`SeedPlantingStates.CheckValidPlotCell`) -> T4 (the Pip's re-check at PlantComplete passes, or no plant would spawn).
- PlantingPatches #3 (`TryPlant`, swap when solid above) -> T3a. No swap on a floor -> T3b. From a Pip's storage -> T4.
- BranchPatch #9 (Top for a ceiling mother) -> T5. Vanilla mother unchanged -> the floor vine test.
- Not covered here: the plot patches #4-#8 (S6) and the save round trip (S9).

## Change made at the user's request (coordinator message)
T2 and T4 were not yet written when the message came. They were written from the start with the Pip spawned on top of the strip, not on the box floor, and with the `AssertReaches` fast-fail before the query (T2) and before the wait (T4). No floor-spawn version ever existed or ran. The strip uses the helper's layout: one row below the built ceiling, open at both ends.

The second message (screenshots when a test misbehaves) did not come into play. The one failure (missing Maturity on the mother) had a clear cause in the stack trace and was fixed in one step.

## Screenshots
Originals in `C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261018-000941\artifacts\` (2560x1440). Crops made with Pillow and viewed in `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\final\`.
- `OvagroOverhead.Tests.Vines.AlongTheCeiling-ceiling-vine.png` (crops `final\vine-close.png`, `final\vine-corner.png`):
  - The mother hangs right side out: its pineapple-like caudex sits against the underside of the sandstone strip, and the pink leaves hang downward.
  - Three grown vine segments on the left and two on the right lie along the underside, hugging the strip's lower edge. The third right branch is ungrown, so its paused grow animation draws nothing.
  - At the strip's left end the grown corner piece curls from the underside up around the end face, with a fruit pod hanging below it.
  - Nothing is left floating and nothing lies on top of the strip.
- `OvagroOverhead.Tests.Vines.AlongTheCeiling-ceiling-vine-wide.png` (crop `final\vine-wide.png`): the same vine at zoom 9, under the sandstone strip. The built ceiling (white tile band) is above and the open box is below.
- `OvagroOverhead.Tests.WildPlanting.PlantUnderCeiling-ceiling-plant.png` (crop `final\ceiling-plant.png`): the T3a plant hangs from the strip, caudex up against the ceiling and leaves down. It is a cleanly mirrored node. As the design predicted, its lighting comes from below.
- `OvagroOverhead.Tests.WildPlanting.PipPlantsOnCeiling-pip-planted.png` (crop `final\pip-planted.png`): the Pip clings upside down to the strip's underside (Ceiling nav) in the cell where it planted. The hanging Ovagro's pink leaves show below it.
- Also from the earlier run: `run-20261018-000602\artifacts\` has the same T3a and T4 shots, with the plant at (124,191) and the Pip beside it.

## Mod bugs found
None. Every assertion on the mod's behaviour passed on the first run that reached it.

## Player.log findings (run-20261018-000941\Player.log)
- `[04:09:57.988] [1] [INFO] [OvagroOverhead] Loaded version 0.1.0.0`
- 0 lines contain "Exception", in this run and in run-20261018-000602.
- No warning or error mentions Ovagro Overhead. The warnings present are other content's: "Recoverable parse error" in Workshop mod.yaml files, "No supported content for mod", "baby_snail_iron_kanim ... placeSymbol is missing", and "world Asteroid has invalid season ...".
- Every test's `logErrors` is `[]`.

## Store reads
Runs (cwd C:/dev/ONI-Mods, `node c:/dev/ai/icon/bin/ks.mjs`):
1. `read blk-960faf85db39 blk-33fb80177cba blk-db81d810e051 blk-41ffba520973 blk-3753681f851b blk-976d76b5a752 blk-a66da6f9aaa2 blk-46b28a0040e8 blk-0194e02d0fd6`: all 9 resolved in one call (7637 B).
2. `brief --store common --for common/tools/TestRunner`: 8 pointers.
3. `read --store common tools/TestRunner:domains/entities.md tools/TestRunner:standards/error-handling.md`: both resolved.

What informed the work:
- blk-960faf85db39: run commands, `-Filter`, exit codes, one game at a time.
- blk-33fb80177cba and blk-db81d810e051: pacing (game seconds after the strip, spawns and planting; waits on conditions).
- blk-a66da6f9aaa2 and blk-976d76b5a752: box clearing between Boxed tests.
- blk-3753681f851b: `t.Cleanup`.
- blk-0194e02d0fd6: Assert API.
- entities.md (kit table: `Speed`, `LookAt`, `Screenshot`, `Boxed`).
- error-handling.md (a logged error fails a test, which is why `logErrors` was checked).

blk-41ffba520973 and blk-46b28a0040e8 were read but added nothing beyond the above.

Report-back on the packet:
- The handoff brief had no pointers ("Nothing matched this scope") because the new node has no records. The ids in the task message carried all the testing knowledge.
- Nothing governing was missing. The TestRunner brief's 3 overview pointers (ModCheck, tools overview) did not apply.

Read as files (task scratch, code): handoff-9.md, units/08-test-report.md, units/04-plan-report.md, units/03-design-report.md (sections up to Testability), the mod's source and test files, `Doors.cs`, the TestRunner Kit sources, and the decompiled game sources in the scratchpad.

## ICON friction
- None blocking. `ks read` with 9 ids, `ks brief --store common`, and `ks read --store common` with two node-qualified docs all worked first time.
- Minor: `ks brief --for common/tools/TestRunner` returned the two most useful TestRunner records (entities, error-handling) only as whole-file pointers (12.6 KB and 7.1 KB) with no block ids. Reading them meant reading all of both, where a block-level pointer to the Kit table and the Test Results table would have been enough.

## Not done / next
- S6 (plot tests T6-T8) and S9 (save round trip) are other units.
- The T4 Pip took a random ceiling cell (132,191 here; 124,191 before), as designed (decision D1). The test asserts only that the cell is under the strip.
