# Unit 12 — review report: Ovagro Overhead (OM-006)

Reviewed: `git diff main...HEAD -- . ':!.context'` on OM-006-ovagro-overhead (HEAD 896238b), 38 files. Read every changed file under `mods/ovagro-overhead/src/OvagroOverhead/**` and `mods/ovagro-overhead/src/OvagroOverhead.Tests/**`, plus the README, CHANGELOG, `publish/*`, `ONI-Mods.sln`, the root README and `.github/`. The decompiled game source in the scratchpad `decomp\` folder was the reference for every game claim. Changed no file except this report.

**Verdict: approve after the two fixes.** No block findings. 2 fix, 5 nit.

## Findings

| # | Sev | Where | Finding | Expected fix | Pass |
|---|-----|-------|---------|--------------|------|
| 1 | fix | `OvagroOverhead.Tests/Plots.cs:72-105`, `:111-140`; claim at `README.md:8` | No committed test checks that the vines of an Ovagro planted in a downward plot grow along the ceiling. `README.md:8` says "its vines grow along the ceiling", and the task objective covers plots as well as wild planting. `HydroponicFarmDownward` and `FarmTileDownward` only wait for the mother's `IsGrown` flag. The game spawns the first branches in the cells left and right of the mother's origin, which is the cell under the plot. A branch there needs solid ground above it (`VineBranch.IsCellFoundation`: solid or door), or the plot cell itself as a corner (`IsMotherCellFoundation` → `PlanterboxCell`). In both tests the plot has no neighbouring tiles, so this path never runs. The untracked `Preview.cs` grows 8+8 branches along glass tiles, but it is Explicit, not committed, and checks no shapes. | In `FarmTileDownward` (or the Hydroponic Farm test), build tiles on both sides of the downward plot in its row. Water it, then call `Kit.GrowMother` and `Kit.GrowChains(t, smi, 2, 2, 90)`. Assert that `left[0].RootShape` and `right[0].RootShape` are `Top`, and that both branches sit beside the origin with `MyShape == Top`. | 5, 6 |
| 2 | fix | `OvagroOverhead.Tests/Vines.cs:17-49` vs `Kit.cs:155-184` | The private `GrowChains` and `GrowMother` in Vines.cs copy `Kit.GrowChains` and `Kit.GrowMother`. Vines' `GrowMother` adds only an elapsed-time log line. Unit 10 left this open; the plan asked the review to rule on it. | Delete both private copies from Vines.cs and call `Kit.GrowMother` and `Kit.GrowChains` at Vines.cs:85, :87 and :132. If the elapsed-time line is wanted, move it into `Kit.GrowMother`. | 4, 5 |
| 3 | nit | `OvagroOverhead.Tests/WildPlanting.cs:30-35` vs `Kit.cs:196-202` | `AssertSeedIsVanilla` repeats the three asserts in `Kit.AssertSeedPrefabIsVanilla`. The only difference is that it takes a seed instance. | Give the Kit helper a `PlantableSeed` parameter that defaults to the prefab's, then use it in both places. | 4 |
| 4 | nit | `OvagroOverhead.Tests/Plots.cs:260`, `:262`, `:342`, `:344` | `Traverse` reads four private side-screen fields ad hoc (`depositObjectMap`, `tag`, `requestSelectedEntityBtn`, `descriptionLabel`). If a game update renames one, the test fails with a NullReferenceException that does not name the member. That is the case the testing standard's "Asserting on a private field" rule (blk-4224fccba710) points at `Reflect.cs` for. Bitshifter and smart-weight-plate tests do the same, so this is existing drift and not new to this mod. | Smallest fix: `t.Assert.NotNull` on each Traverse result, with the member named in the message. Standard fix: add the members to `common/tools/TestRunner/Reflect.cs` with its self-check. That is a common/ change and an architecture trigger (blk-c8ecd5229a93), so it is out of scope for this task. | 5 |
| 5 | nit | `OvagroOverhead/CeilingForm.cs:49`, `OvagroOverhead/VineMotherCeilingConfig.cs:61` | Both use `var` in mod code, while the rest of each file writes types out (`GameObject plant`, `WiltCondition wilt`, `VineMother.Def mother`). The Loops section of the code-style standard (blk-dffe3109ac36) says to follow the surrounding file. | `Swap swap = new Swap { ... }` and `List<Tag> additionalTags = new List<Tag> { ... }`. | 4 |
| 6 | nit | `OvagroOverhead/VineMotherCeilingConfig.cs:67-79`, `:91-92`; drift check at `OvagroOverhead.Tests/Registration.cs:42-54` | The config copies vanilla literals: mass 2f, default temperature 308.15f, `pressure_warning_low` 0.15f, `max_age` 2400f, `max_radiation` 2200f, decor TIER1, `MAX_BRANCH_COUNT` 24 and `BRANCH_PREFAB_NAME`. Vanilla has no constants for these. The Registration drift test compares only temperatures and irrigation, so if Klei retunes any other value, the ceiling form keeps the old one and no test fails. | Extend `Registration.Prefabs` to compare these against the vanilla prefab: `VineMother.Def` fields, `PrimaryElement` mass and temperature, `DecorProvider` values, and the radiation and pressure bounds. | 1, 5 |
| 7 | nit | `README.md:15`, `publish/workshop-description.txt:9` | The removal advice covers only planted ceiling Ovagro. A downward plot with an open Ovagro order keeps `requestedEntityTag` in the save. Without the mod, `SingleEntityReceptacle.OnSpawn` creates that order again, and the vanilla `PlantablePlot.SetPreview` puts the floor preview above the plot, since a Top seed is not rotated. Where that cell is open, the order stands, and a delivery spawns the upright Ovagro running up into the plot (the vanilla behaviour unit 01 found). Reasoned from source, not tried in the game. | Add "and cancel Ovagro orders on downward-facing farm tiles" to both texts, or confirm the case during the plan's pending "load a save without the mod" check. | 3, 6 |

## Passes run

### Pass 1 — Harmony patches against the decompiled game (correctness)

What I checked, patch by patch against `decomp\`:

- **Targets and signatures.** All nine patches match their targets.
  - `PlantableCellQuery.CheckValidPlotCell(PlantableSeed, int)`: private instance method, argument names `seed` and `plant_cell`.
  - `SeedPlantingStates.CheckValidPlotCell(Instance, PlantableSeed, int, out PlantablePlot)`: private static method; the `ArgumentType.Out` array matches; names `seed` and `cell`.
  - `PlantableSeed.TryPlant(bool)`: public.
  - `PlantablePlot.SpawnOccupyingObject(GameObject)`: protected override; name `depositedEntity`.
  - `PlantablePlot.SetPreview(Tag, bool)`: public override, reached virtually from `SingleEntityReceptacle.CreateOrder`.
  - `PlantablePlot.OnCopySettings(object)`: private.
  - `ReceptacleSideScreen.Initialize(SingleEntityReceptacle)`: public; name `target`.
  - `PlanterSideScreen.SetResultDescriptions(GameObject)`: protected override. `___targetReceptacle` is a protected field on the base class, and Harmony's field lookup walks base types.
  - `VineBranch.Instance.SetupRootInformation(VineMother.Instance)`: public, with the `VineMother.Instance` overload picked.
  - Public targets use `nameof` and private or protected ones use strings, as the Harmony section of the code-style standard (blk-bcc50465e110) requires.
- **The swap is restored on every path.**
  - Normal path: the finalizer calls `Exit` with `__state`. Harmony 2 shares `__state` between the prefix and the finalizer of one patch class.
  - Exception paths: Harmony wraps prefixes, the original and postfixes in the try that the finalizer guards. If the prefix itself throws, `__state` is `default`, so `Exit` does nothing. The finalizers take no `__exception`, so the exception is rethrown after the restore.
  - Nested calls: an instance swap is entered only while `IsOvagro` sees direction Top, so an inner call on a seed that is already swapped does nothing and the outer call restores it. I traced every nested chain the game has: `CreateOrder` → `SetPreview` → `base.CreateOrder` → `SetPreview(solid)`; `OnCopySettings` → `CreateOrder` → `SetPreview` ×2; `Initialize` → `ToggleClicked` → `SetResultDescriptions`; `PlantComplete`, which calls the #2 check and #3 `TryPlant` one after the other. All restore correctly. `EnterPrefab` does nothing on a prefab that is already swapped.
  - `SpawnOccupyingObject`: the seed is destroyed only after the method returns (`OnDepositObject` → `destroyEntityOnDeposit`), so the finalizer's `GetComponent` still finds it.
- **Leak paths.**
  - A seed instantiated while the prefab is swapped would keep the swapped fields. `EntitySplitter.Split` clones from the prefab, and none of the prefab-swap windows (`SetPreview`, `OnCopySettings`, the two side-screen calls) splits or spawns a seed.
  - `TryPlant`'s `TakeUnit` splits from the prefab, which is not swapped there; only the instance is. The routing fields carry no `[Serialize]` (`PlantableSeed.cs:9-21`), so a swap can never be saved.
  - Threading: `Navigator.RunQuery` runs synchronously on the main thread, so the #1 swap cannot race.
- **Readers the patches do not cover.** I grepped every reader of `PlantID`, `PreviewID` and `direction`. Those left unpatched only display things: `BasePacuConfig`, `PrinterceptorScreen`, `PlanterSideScreen` lines 182, 281 and 291, and the codex receptacle list. The side screen saves each toggle's direction at `Initialize` and uses it at lines 412 and 609, which #6 already covers.
- **Routing rule.** `WantsCeiling` checks only for solid ground above, and that loses nothing. The floor form's `EntombVulnerable.IsCellSafe` rejects any solid cell in its 1x2 area, so a cell with solid ground above can only ever take the ceiling form. Foundation, doors and hard rock are rejected by `TestSuitableGround`, as on floors.
- **#9 timing.** `VineMother.SpawnBranchOnCell` calls `SetActive(true)` and then `SetupRootInformation`. The postfix runs right after vanilla's own `RootShape.Set(1)` inside the same call. `VineBranch` has no `ParamTransition` on `RootShape`, and the statements left in that call (BranchNumber, WildPlanted, Mother, `SetHarvestWhenReady`) do not read it. Any reader that would see vanilla's value therefore sees the postfix's value instead. The timing assumption is the one vanilla already relies on. Later branches copy `root.MyShape` (line 480). `RootShape` is a saved state-machine parameter (`SerializeType.ParamsOnly`), so it survives a reload.

Result: no findings.

### Pass 2 — hidden prefab against VineMotherConfig and SaltPlantConfig

`VineMotherCeilingConfig.CreatePrefab` makes the same calls as vanilla with the same values. These are the differences, all expected:

- Extra tags: `Hanging` (as SaltPlant, SpiceVine, Kelp and FlyTrap have), plus `HideFromSpawnTool` and `HideFromCodex`.
- `MakeHangingOffsets` on the plant and on the preview.
- `UprootedMonitor.monitorCells` set to {(0,1)}.
- `CodexEntryRedirector` → VineMother, as `VineBranchConfig` does.
- Its own base trait id. Nothing in the game references `VineMotherOriginal`.
- The no-op `traits.Get` is dropped.

Two things vanilla gets from `CreateAndRegisterSeedForPlant` and `CreateAndRegisterPreviewForPlant` are made by hand here: `SeedProducer.Configure(SEED_ID, Hidden)` (the same default of 1 seed), and the preview through `CreateAndRegisterPreview(..., ObjectLayer.Building, 1, 2)`. The DLC restriction comes from the loader through `IHasDlcRestrictions`. `GetDlcIds` returns null, so `EntityConfigManager.cs:69-81` takes the restrictions branch.

Nothing vanilla sets is missing. The copied literals are finding 6.

### Pass 3 — save/load and mod removal

Saving and loading:

- The ceiling plant saves under the id `VineMotherCeiling` (D4).
- The anim `Rotation`, `FlipX` and `Pivot` come from the prefab when the plant is re-instantiated.
- Branch `RootShape` is a saved parameter.
- A plot's link to its plant is restored through `plantRef`.
- An Ovagro order on a downward plot is re-created at `OnSpawn` → `CreateOrder` → the patched `SetPreview`.
- Persistence.cs checks all of this, and it passed in run-20261018-003145.

Removing the mod:

- `SaveManager.Load:318-322` logs "Could not find prefab" and skips the bytes, so ceiling Ovagro are dropped, as the README says.
- Orphaned branches go to `dead` through `ParamTransition(Mother, dead, IsNull)` and `OnTargetLost`.
- A plot whose plant is gone loads empty.
- Pending orders: finding 7.

### Pass 4 — standards adherence

I checked the code-style standard sections Formatting, Language and Toolchain, Imports, Comments, Control Structures, Harmony Patches, Game Components and Project-Specific Rules, and the error-handling standard sections Principles, Guarding Patches, try/catch, Logging and Anti-Patterns.

What conforms:

- Tabs throughout, Allman braces, and trailing commas in initialisers.
- Usings sorted with `System` first.
- One namespace per project.
- No string interpolation.
- XML summaries on every patch class and on the config, each explaining what the game does and what the patch changes.
- The load line `[OvagroOverhead] Loaded version` is present.
- No PLib, options or strings (D2).

**Fifth try/catch case (Mod.cs:14-22).** It matches the user's ruling: it unpatches everything with `UnpatchAll(harmony.Id)`, warns once with the `[OvagroOverhead] <what was skipped>: ` + `e.GetBaseException().Message` form, and still logs the load line. A renamed member, whether a string target or `___targetReceptacle`, makes `PatchAll` throw and lands in this catch, so nothing reflects ad hoc without a guard.

Two things for the user, raised only as notes because the ruling covers them:

- The catch also unpatches #9. #9 serves ceiling Ovagro already in a save, so after a failed patch their new branches would be laid as floor branches.
- The warning text "Ovagro stays floor-only" does not mention that case.

The case still has to be written into the node's own error-handling standard at completion; the plan already lists this.

Findings: 2, 3, 5.

### Pass 5 — tests

All 16 tests in run-20261018-003145 passed (I read `summary.txt`). The run's Player.log has the load line and no exception, `[ERROR]`, VineMother or missing-prefab line.

- **Assertions check behaviour.** They check cells, shapes, prefab ids, preview cells and validity, toggle states, descriptor text, and that the seed prefab is back to vanilla after every swap. Each test has a counter path: floor planting, an upright plot, a plot with no room below, Mealwood greyed out, and copying onto an upright tile. `QueryFindsCeilingCells` would fail without #1, because the query would return the row above the strip. I found no `!= null` "it worked" asserts.
- **Pacing.** Game seconds pass between building, ordering, delivering and growing. `t.Speed` restores itself through `Cleanup` (`TestContext.cs:151-157`).
- **Cleanup.** `Kit.Spawn` and `HeldSeed` register cleanup, plants spawned during a test are cleaned up, the side screen is deselected, and the round-trip save is removed and `SavePath` restored, the same way detect-any-element, salty-surf and vent-freeze-fix do it.
- **Order dependence.** The Persistence pair depends on running in order. The first Plots test (FreshSave) puts the fixture back, as its summary says.
- **Exception paths.** No test drives the finalizers' exception path; Pass 1 covers it by reasoning from Harmony's semantics. `SeedPlantingStates.CheckValidPlotCell` (#2) is covered only end to end, by `PipPlantsOnCeiling`.

Findings: 1, 2, 4, 6.

### Pass 6 — player-facing text

I read `mod.yaml`, `README.md`, `CHANGELOG.md` and `publish/workshop-description.txt`. There is no hedging, and the bullets are short and plain, matching the changelog style.

The claims check out against the code and the tests: natural ceilings only (built tiles are Foundation and the seed rejects them), flipped Farm Tile and Hydroponic Farm, floor planting unchanged, and the Prehistoric Planet Pack required. The one untested claim is finding 1. The removal advice is incomplete, which is finding 7.

`workshop-tags.txt` lists every DLC, per the manager's fix. The credit line ("Made by ... Claude Code") is one of the variants already used across the repo.

### Pass 7 — registration files

- `ONI-Mods.sln`: a solution folder plus the mod and Tests projects, each with 4 config lines and 2 NestedProjects lines. GUIDs are unique and the paths are correct. The folder sits at the end of the mods list, as vent-freeze-fix's did.
- `label-mod.yml`, `bug_report.yml` and `feature_request.yml`: "Ovagro Overhead" is in alphabetical order, and the label `ovagro-overhead` matches the folder name.
- Root README row: correct and in alphabetical order.
- `mods/ovagro-overhead/.gitignore` is identical to just-fall-already's.
- `mod_info.yaml`: `requiredDlcIds: [DLC4_ID]` (D3), `minimumSupportedBuild` 744825, version 0.1.0 (matches the csproj `Version`), `APIVersion` 2.
- The test csproj sets `ModDeployFolder` none and `ReferenceUnityUI` for the side screen test.

Result: no findings.

### Pass 8 — security and trust boundaries

The mod code does no file I/O, takes no player input outside the game's own UI paths, holds no secrets and writes no logs beyond the load and failure lines. The tests write and delete files only under `Session.Dir`, the run folder.

Result: no findings.

### Build evidence

`dotnet build mods/ovagro-overhead/src/OvagroOverhead.Tests/OvagroOverhead.Tests.csproj -c Debug -p:ModDeployFolder=none` → Build succeeded, 0 errors. The 3 warnings are all CS0618 in `common/tools/TestRunner` (Worldgen.cs, TestBehaviour.cs), none in this mod. Deploy was off, so the user's mods folder was not touched. I did not run the game or the suite (spec).

## Outside the diff, for the manager

- `mods/ovagro-overhead/src/OvagroOverhead.Tests/Preview.cs` is untracked: the Explicit Workshop-preview scene. It compiles into the test DLL. If it gets committed, review it then.
- Still open in the plan, not code findings: the preview image (release needs `publish/preview`), the manual check of loading a save without the mod (finding 7 can be confirmed there), the node's standards with the fifth try/catch case, and the design decision record.

## Store reads

- Handoff packet `handoff-12.md`: the brief was empty for `mods/ovagro-overhead/src` because the node has no records yet.
- `ks map --document mods/just-fall-already/.context/standards/code-style.md` and `--document .../error-handling.md`: section maps (22 and 23 records).
- `ks read` on the code-style sections #formatting (blk-6b50367224f0, with blk-e0ff5de8ea26, blk-9f56da6c281c and blk-60755d5ff761), #language-and-toolchain (blk-ab0660690485), #imports-includes (blk-45ca80d394c7), #comments (blk-9edc9a83a58d), #control-structures (blk-2b2f52d515f0, including Loops blk-dffe3109ac36 and Strings blk-786743155fe7), #harmony-patches (blk-bcc50465e110), #game-components (blk-9fead247823f) and #project-specific-rules (blk-9d74aee10321). Applied in Passes 1 and 4 and in finding 5.
- `ks read` on the error-handling sections #principles (blk-f75b46be2845), #guarding-patches (blk-dc52b95d72e4), #try-catch (blk-5938243ee768), #yielding-to-other-mods (blk-b684bb2fb3e0), #logging (blk-0f4cc25567d7) and #anti-patterns-to-avoid (blk-88d3fdd9130e). Applied in Pass 4, including the Mod.cs fifth case.
- `ks read` on the root testing records: blk-960faf85db39 (running tests), blk-33fb80177cba (pace to the game), blk-db81d810e051 (frames-apart anti-pattern), blk-3753681f851b (setup and teardown), blk-976d76b5a752 (cleanup strategies), blk-46b28a0040e8 (best practices), blk-0194e02d0fd6 (assertion patterns), blk-4224fccba710 (private-field assertions, used for finding 4), blk-f054250ed484 (meaningless assertions) and blk-c8ecd5229a93 (architecture triggers, which put finding 4's standard fix out of scope). Applied in Passes 5 and 7.
- No `ks brief --tag` widening was needed. Decisions came from plan.md `## Decisions` as the spec directed.
