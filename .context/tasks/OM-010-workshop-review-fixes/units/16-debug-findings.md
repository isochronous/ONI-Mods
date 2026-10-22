# Unit 16 findings: UpgradePlanner ByHand.CancelledRebuild failure

## Verdict

OM-010's change did not cause this. The run was made without Blueprints Expanded, and `ByHand.CancelledRebuild` is the one default (non-Explicit) "All buildings" test that has no `RequireBlueprints` guard. The test has needed Blueprints Expanded since it was written in 52a0b43 (2026-10-15). It passed in mono-20261015-182351 only because that run loaded Blueprints Expanded. Confidence: high.

## Evidence

1. **Which mods each run loaded** (`mods.json.used`):
   - mono-20261015-182351 had No Manual Delivery, Suppress Notifications, Adaptive Building Menu, **Blueprints Expanded (3468585385)**, Upgrade Planner, Direct Automation Bridges and TestRunner. 0 skipped tests; CancelledRebuild passed.
   - om010-20261022-123342 had Sweep Zones, Just Fall Already, Salty Surf, Upgrade Planner and TestRunner. **No Blueprints Expanded.** Every guarded test skipped with "Blueprints Expanded is not loaded (run with -Steam 3468585385)": the AllBuildings.* tests, BuildingSettings.ProblemTypesKept, and ByHand.Rebuilt/ReplacedWithSettings/Doors/TwoIngredients.
2. **The test** (`mods/upgrade-planner/src/UpgradePlanner.Tests/ByHand.cs:321-341`): it builds a sandstone StorageLocker, then `SetAllBuildings(true)`, picks the StorageLocker family, and sets SandStone to Granite. It then asserts `plan.IsComplete` at line 341. Nothing before that calls `BuildingSettings.RequireBlueprints(t)`. Its siblings `Remade` (line 177) and `TwoIngredients` (line 222) do call it.
3. **Why the plan is incomplete without Blueprints Expanded:**
   - `Rules/MaterialSwap.cs:33` and `:48` only offer rebuild-only types when `allBuildings && BlueprintsCompat.Loaded`. Otherwise `Categories` returns `Families.ByLayer(Replaceable)`.
   - A StorageLocker cannot be built over, so it is not offered. `ChangeMaterialPlan.UsableTypes()` is then empty, `IsComplete` (`Plan/ChangeMaterialPlan.cs:40`) is false, and `Summary()` returns `MaterialPromptTypes`: "Pick at least one building that can be built in Granite." That is exactly the failure message.
   - `git blame` dates both gate lines to 70edf490 (2026-10-14). That is before the passing run, so this is not new behaviour.
4. **What OM-010 changed** (`git diff 154a196 HEAD -- mods/upgrade-planner/src/UpgradePlanner`): only `Tool/UpgradeTool.cs` changed. It adds a `colorsBoxVersion` assignment and a `colorsWorld` key to the overlay-colour cache in `GetOverlayColorData`. Nothing on the path from plan to `IsComplete` changed. No other Upgrade Planner source changed after 2026-10-15 except 6254c4f, a Suppress Notifications damage-bar null fix that does not touch plan logic. Material mode was already in the passing run: its MaterialChange.* tests pass there.
5. **Test leakage ruled out:** the test calls `plan.Clear()` first, and the failure follows deterministically from the gate. It failed after 3.0 s at the assertion, with no log errors.

## Fix I'd make (not made)

- Add `BuildingSettings.RequireBlueprints(t);` as the first line of `ByHand.CancelledRebuild` (`ByHand.cs:323`). This changes test code only. In BE-less runs the test then skips, as its siblings do.
- How far it reaches: I checked every other `SetAllBuildings(true)` call site in the tests. The AllBuildings.* tests, BuildingSettings, `Remade`/`PlanAndDrag` via TwoIngredients, DoorCrusher (line 30) and the ByHandAll tests (Explicit, guarded at 76/90) are already guarded or only reached from guarded tests. CancelledRebuild is the only unguarded one.
- To check that the code itself is fine rather than only skipping it: re-run with `-Steam 3468585385 -Filter "*CancelledRebuild*"`, which should pass as it did on 2026-10-15.

## Handoff pointer review

- 16 of the pointers are near-identical `try/catch` sections from other mods' error-handling standards. They matched only because they name Upgrade Planner files, and none of them applied. The upgrade-planner standards and domain docs did not govern a test-guard question either.
- The pointer that mattered was blk-4ed03be05da6 (integration-testing, Other mods / `-Steam`). It appeared, but it ranked last.
- Missing: nothing in the store says that tests needing Blueprints Expanded must call `RequireBlueprints`.
