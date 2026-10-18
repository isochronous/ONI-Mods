# Unit 04 report: sequenced plan for Ovagro Overhead

Inputs: plan.md, units/02-explore-report.md (scaffold), units/03-design-report.md (design, patches T1-T10). Nothing was run and no repo file was written except this report. Paths are from the repo root `C:\dev\ONI-Mods`. Decompiled game source for implementers: `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\` (design report cites file:line into it).

## 1. Dependency map

```
S1 scaffold ──┬─> S2 prefab + CeilingForm ──┬─> S3 seed-side patches (#1-#3, #9) ──> S5 Pip tests (T2-T5)
              │                             │
              │                             └─> S4 plot-side patches (#4-#8) ──────> S6 plot tests (T6-T8)
              │                  S2 ──> S4a Kit + T1 (registration)
              ├─> S7 repo-facing docs (README, CHANGELOG)             (parallel with S2 onward)
              └─> S8 publish material (description, tags; preview art)  (parallel; preview shown to user)
S3 + S4 ──> S9 save round trip (T9/T10)
All ──> M1 integrate node, M2 decision record (manager)  ──> completion
```

Sequential: S1 first; S2 before S3/S4 (they use `CeilingForm` and the ids); every test step after the patches it exercises. Parallel candidates once S2 is committed: S3 and S4 (different files, no shared edits), S4a, S7, S8. Because they share one branch and one `.sln`, run parallel steps either in separate worktrees merged by the manager or one after another; S3 and S4 touch disjoint files, so a merge is trivial. The game runs one test session at a time (`blk-960faf85db39`: one run at a time, others queue), so test steps queue on the game even when written in parallel.

Every step ends with `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeding (paste the tail of the output) and a commit of all created and modified files, one terse line `OM-006: <what>` (commit-message-style memory; Co-Authored-By trailer). Test steps also end with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead` exit code 0 (`blk-b6ca8e2d3bd3`). Always check the destination before writing (look-before-overwriting memory); write scripts and files with Write, not heredocs.

## 2. Steps

### S1 - Scaffold the mod (tier: basic)
Mechanical copy of an existing mod plus the repo-wide registrations. A new mod is an architecture trigger (`blk-c8ecd5229a93`: sln entry, label map, README table, release plan); the design unit already covered it.

Creates (copy from `mods/just-fall-already/`, per unit 02 section 2):
- `mods/ovagro-overhead/src/OvagroOverhead/OvagroOverhead.csproj` (AssemblyName/RootNamespace `OvagroOverhead`, `<Version>0.1.0</Version>`, keep the `$(OniCommon)oni-mod.props`/`.targets` imports)
- `.../OvagroOverhead/mod.yaml` (`title: "Ovagro Overhead"`, one-sentence description, `staticID: "Isochronous.OvagroOverhead"`)
- `.../OvagroOverhead/mod_info.yaml` (version 0.1.0, `minimumSupportedBuild: 744825`, `APIVersion: 2`, and `requiredDlcIds:` list with `DLC4_ID` instead of `supportedContent: ALL`; D3)
- `.../OvagroOverhead/Mod.cs` (`OvagroOverheadMod : UserMod2`, `OnLoad` calls `base.OnLoad(harmony)` and logs `[OvagroOverhead] Loaded version ...`; analogue `mods/just-fall-already/src/JustFallAlready/Mod.cs`)
- `mods/ovagro-overhead/src/OvagroOverhead.Tests/OvagroOverhead.Tests.csproj` (copy `JustFallAlready.Tests.csproj`; `ModDeployFolder` none, TestRunner reference `Private="false"`) plus one placeholder-free first test file is NOT created here (S4a adds tests; an empty test project builds).
- `mods/ovagro-overhead/.gitignore`, `.gitattributes` (unchanged copies)

Modifies:
- `ONI-Mods.sln`: three new GUIDs (`[guid]::NewGuid()`), project entries after the `vent-freeze-fix` pair, the `.Tests` entry after `NaturalBackwalls.Tests`, 8 ProjectConfigurationPlatforms lines, 2 NestedProjects lines (unit 02 section 3.2). File is UTF-8 BOM + CRLF; preserve it (check bytes after the edit).
- `README.md` (root): row `| Ovagro Overhead | [mods/ovagro-overhead](mods/ovagro-overhead) |` after "No Magic Fridges".
- `.github/workflows/label-mod.yml`: add `"Ovagro Overhead": "ovagro-overhead",` in the alphabetical `labels` map (after "No Magic Fridges").
- `.github/ISSUE_TEMPLATE/bug_report.yml` and `.github/ISSUE_TEMPLATE/feature_request.yml`: add "Ovagro Overhead" to the Mod dropdown options (same position). The task scope lists only label-mod.yml, but the form answer must match a map key for the label to apply (see section 5, A1).

Acceptance:
- Solution build command exits 0 and `dotnet sln ONI-Mods.sln list` shows both new projects.
- The built DLL deploys to `Documents\Klei\OxygenNotIncluded\mods\local\OvagroOverhead` only when `ModDeployFolder` is not none; with `none` nothing deploys (expected).
- `Invoke-OniTests.ps1 -Mods OvagroOverhead -BuildOnly` exits 0 (build and ModCheck, no game).
- Commit all created and modified files.

### S2 - Ceiling prefab and routing helper (tier: complex)
Hand-built entity with many exact vanilla calls and the art transform; the design's riskiest data work. Copy `VineMotherConfig.CreatePrefab` (VineMotherConfig.cs 44-66) for the plant and `SaltPlantConfig.cs` 22-50 for the Hanging parts (decomp).

Creates:
- `mods/ovagro-overhead/src/OvagroOverhead/VineMotherCeilingConfig.cs`: `IEntityConfig, IHasDlcRestrictions` (required DLC `DlcManager.DLC4`); plant `VineMotherCeiling` and `VineMotherCeiling_preview` exactly as the design's "Data" section: vanilla constants and strings, own trait id `VineMotherCeilingOriginal`, `Hanging` tag, `MakeHangingOffsets(go, 1, 2)`, `UprootedMonitor.monitorCells {(0,1)}`, `VineMother.Def` (`VineBranch`, 24), `HarvestDesignatable`, overlay registration, `ExtendPlantToIrrigated` 0.15, `SeedProducer.Configure("VineMotherSeed", Hidden)`, tags `HideFromSpawnTool`/`HideFromCodex`, `CodexEntryRedirector.CodexID = "VineMother"`, art `Rotation = 180`, `FlipX = true`, `Pivot = (0, 0.5, 0)` on plant and preview. Do not call `CreateAndRegisterSeedForPlant` or `CreateAndRegisterPreviewForPlant` (design lines 34, 37). The id `VineMotherCeiling` is permanent (D4).
- `mods/ovagro-overhead/src/OvagroOverhead/CeilingForm.cs`: static helper `IsOvagro(PlantableSeed)`, `Enter(seed)` / `Exit(seed)` with a small state struct for the saved `direction`, `PlantID`, `PreviewID`, and `WantsCeiling(int cell)` (`Grid.Solid[Grid.CellAbove(cell)]`). Also a plot rule `FacesDown(PlantablePlot)` (`rotatable != null && Direction == Bottom`). Constants for the three ids live here so patches and tests share them.

Pattern: static classes, namespace block, tabs, Allman, `[OvagroOverhead]` log prefix (`mods/just-fall-already:standards/code-style.md`, `naming-conventions.md`).

Acceptance: build exits 0; `-BuildOnly` exits 0; commit. The in-game proof is S4a.

### S4a - Test kit and registration test T1 (tier: default)
After S2. Establishes the test project's helpers so later steps reuse them (the per-mod `Kit.cs` pattern, `mods/detect-any-element/src/DetectAnyElement.Tests/Kit.cs`; test project layout `blk-41ffba520973`).

Creates:
- `mods/ovagro-overhead/src/OvagroOverhead.Tests/Kit.cs`: helpers `NaturalCeiling(t, inside, x0, x1, y)` and `NaturalFloor(...)` (see section 4), `Spawn(prefabId, cell)` (copy `mods/multichannel-critter-sensor/.../Kit.cs` Spawn, or the `Doors.cs` Critter pattern), `Seed(t, cell)` for `VineMotherSeed`, `ForceMature(plant)`.
- `mods/ovagro-overhead/src/OvagroOverhead.Tests/Registration.cs`: T1 as `[OniTest("REGISTRATION: ...", ...), Layer(0)]` with `NoColony` (`blk-c8a18ea4088d` per the design). Asserts per design table T1, including the vanilla `VineMother` has no `Hanging` tag and `VineMotherSeed` is still direction Top with `PreviewID` `VineMother_preview`.

Acceptance: build exit 0; `Invoke-OniTests.ps1 -Mods OvagroOverhead -Filter "*REGISTRATION*"` exit 0 (output tail pasted); commit.

### S3 - Seed-side patches: Pip planting and first branch (tier: complex)
After S2. Harmony prefix+finalizer swaps on private/static game methods plus a postfix with an overload type array. Analogues: `__state` in `mods/vent-freeze-fix/src/.../DebugSave.cs` (~line 91), `Finalizer` in `mods/natural-backwalls-everywhere/src/NaturalBackwalls/WorldgenThreadPatch.cs` (~line 22), patch convention `blk-bcc50465e110`, and the repo's error-handling rule: log when a target is missing.

Creates:
- `mods/ovagro-overhead/src/OvagroOverhead/PlantingPatches.cs`: patches #1 `PlantableCellQuery.CheckValidPlotCell` (private), #2 `SeedPlantingStates.CheckValidPlotCell` (private static), #3 `PlantableSeed.TryPlant`. Each prefix: `if (CeilingForm.IsOvagro(seed) && CeilingForm.WantsCeiling(cell)) state = CeilingForm.Enter(seed)`; finalizer restores. Cell source: #1/#2 the `plant_cell` argument, #3 `Grid.PosToCell(seed)`.
- `mods/ovagro-overhead/src/OvagroOverhead/BranchPatch.cs`: patch #9 postfix on `VineBranch.Instance.SetupRootInformation(VineMother.Instance)` using `new[] { typeof(VineMother.Instance) }`; when `mother.PrefabID() == VineMotherCeiling` set `RootShape` to `(int)VineBranch.Shape.Top`.

Modifies: nothing else (Harmony applies every `[HarmonyPatch]` class in the assembly via `base.OnLoad`).

Acceptance: build exit 0; `-BuildOnly` exit 0 (ModCheck confirms the private targets resolve); commit. Proof is S5.

### S4 - Plot-side patches: Duplicant planting, preview, side screen (tier: complex)
After S2, parallel with S3. The unusual part is swapping the seed prefab's fields for one synchronous call (#5-#8); the design calls this the single unusual move.

Creates:
- `mods/ovagro-overhead/src/OvagroOverhead/PlotPatches.cs`: #4 `PlantablePlot.SpawnOccupyingObject` (swap the deposited seed when `FacesDown(plot)`), #5 `PlantablePlot.SetPreview(Tag, bool)` (swap the prefab's `PlantableSeed` when the tag is `VineMotherSeed`), #8 `PlantablePlot.OnCopySettings(object)` (private).
- `mods/ovagro-overhead/src/OvagroOverhead/SideScreenPatches.cs`: #6 `ReceptacleSideScreen.Initialize(SingleEntityReceptacle)`, #7 `PlanterSideScreen.SetResultDescriptions(GameObject)` (protected override: target the declaring type's override, check with ModCheck).

Prefab swap helper: a second method pair in `CeilingForm.cs` for "enter/exit on the seed prefab" if S2 did not include it (touching S2's file; if S3 runs in parallel, put the prefab helper in `PlotPatches.cs` to avoid conflicts).

Acceptance: build exit 0; `-BuildOnly` exit 0; commit.

### S5 - Pip-side in-game tests T2-T5 (tier: default; T5 and T4 complex)
After S3 and S4a. Pace every step to game time (`blk-33fb80177cba`, `blk-db81d810e051`): build or change, `WaitGameSeconds` 1-2 s, then act; never `yield return null` between build and destroy.

Creates:
- `mods/ovagro-overhead/src/OvagroOverhead.Tests/WildPlanting.cs`: T2 (query: spawn a `Squirrel` and seed, run `PathFinderQueries.plantableCellQuery.Reset(seed, 20)` through `Navigator.RunQuery`; every result has solid above; seed fields back to Top), T3 (`TryPlant(true)` on a ceiling cell gives a `VineMotherCeiling` occupying the cell below; seed on a natural floor still gives `VineMother`), T4 (a real Pip, `GameTags.Creatures.WantsToPlantSeed`, `Timeout` ~180, mark `Explicit` if too slow for a normal run; floor is built tiles so the ceiling is the only valid cell).
- `mods/ovagro-overhead/src/OvagroOverhead.Tests/Vines.cs`: T5 (wild ceiling mother under a flat natural ceiling, force maturity, wait for 3 or more branches per side; each in the ceiling row with `MyShape == Top`, first branch `RootShape == Top`; none uprooted after a few game seconds; one ceiling end stops short so `OutCornerTop*` shows; `t.Screenshot` for a visual check). Use `ForceMature` from the kit and wait on branch counts with `t.WaitUntil`.

Acceptance: build exit 0; `Invoke-OniTests.ps1 -Mods OvagroOverhead -Filter "*WILD*"` and `"*VINES*"` exit 0 (names chosen to match the `[OniTest("PREFIX: ...")]` ids); Player.log shows no repeating exception; commit. T4 passes or is documented `Explicit` with its filtered run exit 0.

### S6 - Plot in-game tests T6-T8 (tier: default)
After S4 and S4a (parallel with S5, queued on the game).

Creates:
- `mods/ovagro-overhead/src/OvagroOverhead.Tests/Plots.cs`: T6 `t.World.Build("HydroponicFarm", cell, Orientation.FlipV)` (direction Bottom; `plot.CreateOrder(VineMotherSeed)` yields the `VineMotherCeiling_preview` one cell below, valid; `ForceDeposit` gives a `VineMotherCeiling` occupying two cells, `RequiresInputConduit` true; seed prefab fields restored); T7 Farm Tile FlipV and an upright regression (Neutral farm tile still grows `VineMother`), plus Copy Settings (#8) between two downward tiles; T8 `Layer(1)` side screen test (select a FlipV farm tile; Ovagro toggle enabled; no `ROTATION_NEED_FLOOR` text).

Acceptance: build exit 0; `Invoke-OniTests.ps1 -Mods OvagroOverhead -Filter "*PLOT*"` exit 0; commit.

### S9 - Save round trip T9/T10 (tier: default)
After S5 and S6. Copy `mods/salty-surf/src/SaltySurf.Tests/Persistence.cs` (two tests, second `FreshSave`).

Creates: `mods/ovagro-overhead/src/OvagroOverhead.Tests/Persistence.cs`: save with a grown ceiling mother plus branches and a FlipV hydroponic occupant; after load assert prefab ids, positions, branch shapes, plot occupant link, mother anim `Rotation == 180`.

Acceptance: build exit 0; full suite `Invoke-OniTests.ps1 -Mods OvagroOverhead` exit 0 (output tail pasted, per `blk-960faf85db39` exit codes); commit. Copy `Player.log` before any later launch if a crash occurs (after-a-crash memory).

### S7 - Mod README and CHANGELOG (tier: basic)
After S1, any time.
Creates `mods/ovagro-overhead/README.md` (structure of `mods/just-fall-already/README.md`; Installing and Building text from `mods/jumper-bridges/README.md`, which uses the monorepo links; unit 02 gap G2) and `mods/ovagro-overhead/CHANGELOG.md` (`# Changelog`, `## Unreleased`, a few short plain bullets, `mods/corner-build-anything/CHANGELOG.md` as model). README states what the mod does, that it needs the Prehistoric Planet Pack, and tells players to clear Ovagro from downward tiles before removing the mod (design Risk 1); no hedging language (memory).
Acceptance: build exit 0 (unchanged), commit.

### S8 - Publish material (tier: basic; preview art is a manager and user step)
After S1, any time.
Creates `mods/ovagro-overhead/publish/workshop-description.txt` (BBCode; copy structure of `mods/just-fall-already/publish/workshop-description.txt`; headline, `[list]`, compatibility note on the patched methods, credit line, `Source and issues: https://github.com/isochronous/ONI-Mods`) and `mods/ovagro-overhead/publish/workshop-tags.txt` (line 1 comment, line 2 `tweaks`, then only `The Prehistoric Planet Pack`). Does not create `workshop-id.txt`. Does not create `preview.png`.
Preview: 512x512 from `common/tools/MakePreview/title_preview.py` (Graystroke font, memory preview-title-tool); the manager produces it, sends it with SendUserFile for approval, then commits `publish/preview.png`. No release runs in this task; the first Workshop upload is a deliberate `WorkshopUpload publish` (`blk-319d2b9dd5c7`; `release.py` makes only a GitHub release for a mod without an item).
Acceptance: files exist and are committed; preview shown to the user before commit.

### Manager steps (not unit steps)
- M1: after S1 is merged to the branch, run icon-integrate for `mods/ovagro-overhead` (creates `.context/` node; also adds `| ovagro-overhead | ovagro-overhead |` to `mods/.context/projects.md`; `projects.md` lists `OvagroOverhead -> src/OvagroOverhead` and `OvagroOverhead.Tests -> src/OvagroOverhead.Tests`).
- M2: land the design as a decision record in the new node through adr-discipline (record D1-D5, the prefab id permanence, the patch table, risks).
- M3: completion review, retrospective, one PR for the repo (one-PR-per-repo memory).

## 3. Test coverage matrix (automated and manual)

| Test | Automated | Where | Notes |
|---|---|---|---|
| T1 registration/drift | yes, `NoColony` | S4a | catches game-update drift |
| T2 Pip query | yes | S5 | |
| T3 wild TryPlant + floor regression | yes | S5 | |
| T4 real Pip end to end | yes, `Explicit` if slower than the default timeout | S5 | Timeout ~180 s |
| T5 branches along ceiling | yes | S5 | plus a screenshot for the visual check |
| T6 Hydroponic Farm FlipV | yes | S6 | |
| T7 Farm Tile FlipV, upright regression, Copy Settings | yes | S6 | |
| T8 side screen | yes, `Layer(1)` | S6 | |
| T9/T10 save round trip | yes, two tests | S9 | |
| Load a save without the mod | no (harness runs with the mod enabled) | manual by the user | design Risk 1; branch death on load unverified |
| Visual check of the flipped mother (lighting from below) | partly: T5 screenshot | manual look by the user | sent through SendUserFile |
| Beside the user's farming/Pip mods (D6) | `-Steam <id>` run if a mod is named | S9 follow-up | D6 still open |

Fixture: `common/tools/TestRunner/fixtures/testbed-so.sav` (Spaced Out, every DLC, so DLC4 is active) is the only fixture and is enough for every test. `testbed-base.sav` is missing and is not needed: the mod requires DLC4, and DLC4 content exists only in the Spaced Out fixture. Base-game runs are not applicable (launcher throws in base mode without `-Fixture`; none is attempted).

## 4. How wild-planting tests make a natural ceiling

`t.Dupes.Box()` walls the area with built Tiles, which are `Grid.Foundation`; `PlantableSeed.TestSuitableGround` rejects Foundation (PlantableSeed.cs 87), so wild tests cannot plant under or on the box. The kit helper `NaturalCeiling` therefore writes natural sand stone into cells inside the box with the same call `Dupes.Box` uses for air, `SimMessages.ReplaceElement(cell, SimHashes.SandStone, ...)` (`common/tools/TestRunner/Kit/Dupes.cs` line ~78: read its exact argument list there; do not guess). Rules for the helper:
- Lay a row one or two cells inside the top wall, leaving at least two free cells below it for the 1x2 plant and one free column at one end for T5's corner.
- Keep the row clear of the DevLifeSupport in the box's top-left corner.
- After `ReplaceElement`, wait on `t.WaitUntil(() => Grid.Solid[cell] && !Grid.Foundation[cell], 5, "natural tile")` and a further `WaitGameSeconds(1)` before spawning anything (sim message is asynchronous; pace to game time).
- `NaturalFloor` is the same on a floor row, for the upright regression in T3. The harness clears the 40x10 area before each test, and `t.Cleanup` removes spawned objects.
- Not a worldgen test (`[OniTest(Worldgen = ...)]`, `blk-87272da04dee`): generated worlds share one coordinate across tests and are not cleared, and the natural row in the fixture is enough; no test here needs a generated world.

## 5. Assumptions and open questions

- A1. The issue templates need the new mod name in their dropdowns (`.github/ISSUE_TEMPLATE/bug_report.yml` line ~21 area, `feature_request.yml` line ~17 area), beyond the three files the task scope names. S1 includes them; the manager should widen the S1 scope if the packet enforces it.
- A2. `ReplaceElement`'s exact signature, the `ForceMature`/maturity API (`Growing`/`Maturity` amount handle), and `Navigator.RunQuery` use in T2 are not verified here; S4a and S5 implementers read `Dupes.cs`, `World.cs`, and the decomp.
- A3. `requiredDlcIds` syntax comes from the design (KMod/Mod.cs read); the list form in `mod_info.yaml` is unproven in a real load. S1's acceptance includes the build; the first test run (S4a) proves the mod loads and is not marked incompatible. If T1 finds the prefab missing, check the mod's log line `[OvagroOverhead] Loaded version`.
- A4. Test names must carry stable prefixes (`REGISTRATION`, `WILD`, `VINES`, `PLOT`, `PERSIST`) so `-Filter` globs work per step.
- A5. Parallel steps share one branch; the manager decides worktrees versus sequence (`ICON:start-worktree`). No two steps edit the same file except `ONI-Mods.sln` (only S1) and `CeilingForm.cs` (S2, with S4 writing its own helper in `PlotPatches.cs`).
- Q1. D6 remains open: which farming or Pip mod of the user's to run beside. Not blocking; becomes a final `-Steam <id>` run after S9.
- Q2. Whether branches of a ceiling mother die on load without the mod is unverified (design Risk 1); only a manual check settles it, and the README warning covers the player.
- Q3. A `testbed-base.sav` does not exist; no step creates it (out of scope).
- Q4. The preview image needs the user's approval before commit; release is out of this task.
- Tiers follow the design's own sizing (Harmony on private game methods and prefab-field swaps are complex; copy-and-rename scaffolding and text files are basic; the rest default). The routing guide itself (`manager-routing-guide`) was not read here, so the tiers are inferred from the spec's wording; the manager should adjust if the guide's signals differ.

## 6. Store reads

Runs (all `node c:/dev/ai/icon/bin/ks.mjs`, store `C:/dev/ONI-Mods`, cwd `C:/dev/ONI-Mods`):
1. `read blk-960faf85db39 blk-33fb80177cba blk-db81d810e051 blk-41ffba520973 blk-87272da04dee blk-319d2b9dd5c7 blk-c8ecd5229a93 blk-b6ca8e2d3bd3` - all eight resolved in one call (8 rows, 7222 B).
2. `ks brief`: none run. The packet's own path brief (handoff-4) was read as a file.

Pointers that informed the plan:
- `blk-960faf85db39` (running tests): the exact test commands, `-BuildOnly`, `-Filter`, `-Steam`, exit codes, one session at a time.
- `blk-33fb80177cba` (waiting) and `blk-db81d810e051` (frames-apart anti-pattern): the pacing rules in S5/S6/S9 and the natural-ceiling helper.
- `blk-41ffba520973` (test file location): `<Mod>.Tests` project layout and csproj copy source (S1).
- `blk-87272da04dee` (generated worlds): why no worldgen test is used (section 4).
- `blk-319d2b9dd5c7` (first Workshop upload): S8 and the no-release boundary.
- `blk-c8ecd5229a93` (architecture triggers): sln, label map, README table, release plan for a new mod (S1); the compatibility trigger noted in S3/S4.
- `blk-b6ca8e2d3bd3` (repository specifics): commit-line format, verification commands, key-file paths.
- Packet pointers from the brief seen but not opened: the code-style/naming copies in other mods (`blk-5b62e7507604`, `blk-76e79f89b3c4`, `blk-dffe3109ac36` are just-fall-already's own and were already summarised by units 02 and 03); the other mods' copies do not apply.

Read as files, not by `ks read` (task folder is write-path exempt): plan.md, units/02-explore-report.md, units/03-design-report.md, `.github/workflows/label-mod.yml`, `.github/ISSUE_TEMPLATE/*.yml` (grep for the mod dropdown), `README.md` (grep), `mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs` (first 60 lines), directory listings of `common/tools/TestRunner/Kit` and `mods/just-fall-already`.

Report-back on the packet:
- Did not apply: the ~45 code-style "Example/Loops/Variables" copies from 15 other mods in the path brief.
- Missed: none beyond what the explicit `ks read` supplied; the testing and release records the plan depends on were reached only through the ids the coordinator named.
- Reason lines: all accurate; the code-style ones only say "names FallPoke.cs", which says nothing about relevance.

## 7. ICON friction

- The packet's path brief again spent about 8 KB of its budget on per-mod copies of one code-style standard (same as units 02 and 03, G9); the pointers that governed this plan (testing, release, repository specifics) were absent from it and came from the ids in the task message. The truncation notice's suggested command (`ks brief --budget 26338`) would only list more of the same.
- `ks read` with eight ids worked on the first call; no refusals.
- Not ICON: none.
