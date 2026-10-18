## Task: OM-006
## Branch: OM-006-ovagro-overhead
## Objective: Scaffold a new mod, mods/ovagro-overhead ("Ovagro Overhead"), whose only feature is letting Ovagro plants grow from the ceiling: planted by Pips in natural tiles (wild planting on the underside of a tile), and by Duplicants in downward-facing Hydroponic Farms and Farm Tiles (both can face down in the current game). Their vines must grow as they do on the floor, along the ceiling's underside instead. Floor planting keeps working as in vanilla.
## Folder: .context/tasks/OM-006-ovagro-overhead/

## Phase State
- **Phase plan**: investigation → architecture → implementation → testing → completion
- **Completed**: investigation, architecture
- **Current**: implementation   (status: pending)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-006-ovagro-overhead
- **Attempts (current phase)**: 0

## Decisions
- Task id OM-006, not OM-003: `ks resolve --explain` reported highest OM-002 for this node, but the common/ submodule store already holds OM-003..OM-005 under the same prefix.
- Wild ceiling planting is the Pips' path, as the user wrote; the first plan text said Duplicants, which was the manager's error (unit 01, Q5).
- Pips walk on walls and ceilings (user, 2026-10-17). This overrides the investigation handoff's "a Pip stands on a floor" premise in question (2); unit 01 read only the default `NavType.Floor`.
- Farm Tiles are in scope: both Farm Tile and Hydroponic Farm already rotate FlipV and set receptacle direction Bottom (unit 01, Q4), which meets the user's "if they can face down" condition.
- Architecture (design unit 03, approved by the user 2026-10-17): a hidden second plant prefab `VineMotherCeiling` (+ `VineMotherCeiling_preview`) built like SaltPlant (Hanging tag, MakeHangingOffsets, UprootedMonitor (0,1)); the vanilla `VineMotherSeed` stays the only seed and is routed to the ceiling form at planting time (cell with solid above, or a FlipV Farm Tile / Hydroponic Farm). Chosen over per-instance flipping, which needs more patches and leaves broken upright plants if the mod is removed.
- D1 (user): a Pip picks among its nearest valid cells as vanilla does; no floor/ceiling preference patch.
- D2: no new player-visible text; the ceiling plant reuses the vanilla Ovagro strings and the vanilla "Requires ceiling" descriptor, so the mod has no ModStrings or translations.
- D3 (user): mod_info.yaml declares `requiredDlcIds: [DLC4_ID]`; a new pattern for the repo (other mods use the deprecated `supportedContent: ALL`).
- D4 (user): plant id `VineMotherCeiling`; it is written into saves and is not renamed after release.
- D5 (user): include the Copy Settings patch (#8).
- D6 (user, 2026-10-17): no farming or Pip mods in the user's loadout would affect this; tests run on Ovagro Overhead alone, no -Steam compatibility run.
- World-gen Ovagro nodes stay on floors.
- S3 and S4 run as one unit (07), not two parallel units: parallel units would commit on one branch at once, and the nine patches share the swap pattern.
- To record at completion (store): an IEntityConfig in these net48 mods must write out GetDlcIds and GetAnyRequiredDlcIds (CS8702 on the game's default interface methods); GetDlcIds returns null so the loader takes the IHasDlcRestrictions branch (unit 06).
- The design lands as a decision record in the mod's own store node once it exists (icon-integrate after the scaffold), through adr-discipline.
- Naming: folder mods/ovagro-overhead, assembly/namespace/project OvagroOverhead, staticID Isochronous.OvagroOverhead, first version 0.1.0, tag ovagro-overhead-v0.1.0 (unit 02).

## Key Files
- mods/ovagro-overhead/: the new mod (scaffolded in bea076e: src/OvagroOverhead/{OvagroOverhead.csproj, Mod.cs, mod.yaml, mod_info.yaml}, src/OvagroOverhead.Tests/OvagroOverhead.Tests.csproj, .gitignore, .gitattributes; store node .context/; template mods/just-fall-already, tests modelled on its Doors.cs)
- ONI-Mods.sln: solution folder, mod project and Tests project entries (unit 02 gives the blocks)
- README.md (root): one table row for the new mod
- .github/workflows/label-mod.yml, .github/ISSUE_TEMPLATE/bug_report.yml, feature_request.yml: new mod registered (S1)
- mods/.context/projects.md: new node row (M1)
- Game code (decompiled, outside the repo): C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\ (VineMotherConfig, VineMother, VineBranch, PlantableSeed, PlantableCellQuery, SeedPlantingStates, FarmTileConfig, SaltPlantConfig)

## Phase Handoff Log

### Handoff: investigation → architecture   (commit: Phase-Handoff: investigation)
**Sub-agent outputs**:
- explore unit 01 (units/01-explore-report.md), game build 744825 release: Ovagro is `VineMother` (1x2 plant, DLC4 Prehistoric Planet Pack), seed `VineMotherSeed` (direction Top), branches `VineBranch` (1x1, bear the Ovagro Figs), fruit `VineFruit`. Branches are not vertical vines: the mother spawns branches in the cells left and right of its origin cell; each branch hugs a solid surface and walks around it up to 12 deep, with shape and anim logic already symmetric for all four sides. Load-bearing levers, quoted: "Prefab data: tag `Hanging`, `MakeHangingOffsets` on plant and preview, `UprootedMonitor.monitorCells {(0,1)}`, seed `PlantableSeed.direction = Bottom` ... as SaltPlantConfig does." "`VineBranch.cs 466` hard-coded `RootShape = 1 (Bottom)`; the only code constant tying branch geometry to a floor-standing mother. Patch point is `VineBranch.Instance.SetupRootInformation(VineMother.Instance)`." "Mother art: needs `FlipY` (or hanging kanim) on the mother's `KBatchedAnimController`; none exists in the game." "A single prefab can hold only one direction (`PlantableSeed.direction` is per seed); supporting both floor and ceiling needs either a second plant+seed+preview prefab or runtime flipping per placement. `IsValidEntity` already ignores direction for rotatable plots." Farm plots: a vanilla Ovagro seed can already be ordered into a downward farm tile, but the plant spawns upright with its 1x2 area running up into the tile itself. Pips walk `NavType.Floor` and plant through `PlantableCellQuery`/`SeedPlantingStates.CheckValidPlotCell`/`PlantableSeed.TestSuitableGround`, all keyed on `seed.direction`.
- explore unit 02 (units/02-explore-report.md): template mods/just-fall-already (single-feature Harmony mod, no strings, Doors.cs tests); csproj imports `$(OniCommon)oni-mod.props`/`oni-mod.targets`; Directory.Build.props unchanged; sln needs 3 GUIDs, 8 config lines, 2 NestedProjects lines; tests in `src/OvagroOverhead.Tests/` with `[OniTest]`, found by `Invoke-OniTests.ps1 -Mods OvagroOverhead`; no mod yet declares requiredDlcIds (syntax unverified); release needs CHANGELOG `## Unreleased`, publish/preview 512x512, workshop-description.txt, workshop-tags.txt; workshop-id.txt is written by the release script; no scaffolding script exists; only testbed-so.sav exists (testbed-base.sav missing); 9 doc gaps G1-G9 listed in the report.
**Reviewer findings**: N/A this phase.
**Verification evidence**: none run; both units read code only. Unit 01: nothing run in the game, kanim art not opened.
**Decisions delta**: Pips not Duplicants; Farm Tiles in scope; naming.
**Key files delta**: as listed in Key Files.
**What the next phase needs**: a design choosing (1) how one seed supports both floor and ceiling: a second hanging prefab chosen at planting time versus per-instance flipping, and how that survives save/load; (2) how Pips find and reach ceiling cells for a 1x2 hanging plant (a Pip stands on a floor, and a hanging 1x2 needs two free cells under the ceiling); (3) mother art: FlipY on `vine_mother_kanim` versus other art; (4) how downward farm plots spawn the ceiling form; (5) the first branch's root shape; (6) DLC4 gating in mod_info.yaml; (7) how in-game tests prove vines grow along the ceiling. The design unit gets --decisions in its packet and the decompiled sources path.

### Handoff: architecture → implementation   (commit: Phase-Handoff: architecture)
**Sub-agent outputs**:
- design unit 03 (units/03-design-report.md), tier complex: "**Approve with modifications.** The feature can be built. Planting and spawning go through a **second, hidden plant prefab** (`VineMotherCeiling`, with its preview). The **vanilla seed** (`VineMotherSeed`) stays the only seed. The game picks the form where the seed is planted, from the cell's geometry or the plot's facing. Nine small Harmony patches route the seed to the ceiling form. One more patch makes the first vine branch hug the ceiling." Modifications: (1) wild ceiling planting is by Pips walking the ceiling; their nav grid has floor, wall and ceiling nav types and vanilla Dasha Saltvine is Pip-planted this way, so no navigation change; (2) world-gen nodes stay on floors; (3) `requiredDlcIds: [DLC4_ID]`; (4) unit 01 overturned: the planter side screen blocks a Top seed on a downward plot, the Top-seed preview sits above a downward tile, VineMother line numbers were off. Patch table (report lines 57-68): #1 `PlantableCellQuery.CheckValidPlotCell`, #2 `SeedPlantingStates.CheckValidPlotCell`, #3 `PlantableSeed.TryPlant`, #4 `PlantablePlot.SpawnOccupyingObject`, #5 `PlantablePlot.SetPreview`, #6 `ReceptacleSideScreen.Initialize`, #7 `PlanterSideScreen.SetResultDescriptions`, #8 `PlantablePlot.OnCopySettings`, #9 `VineBranch.Instance.SetupRootInformation` postfix (RootShape Top for a ceiling mother), (10) entity registration through `IEntityConfig`. #1-#8 are prefix + finalizer pairs that swap the seed's (or seed prefab's) `direction`/`PlantID`/`PreviewID` for one synchronous call; the finalizer restores them. Art: the mother's `KBatchedAnimController` gets `Rotation = 180`, `FlipX = true`, `Pivot = (0, 0.5, 0)` (a plain FlipY + offset is reset by plots on place and reload); rendered sheet at scratchpad\kanim\sheet.png, sent to the user. Tests T1-T10 (report "Testability"): registration and drift, the Pip's cell search, wild planting, a full Pip run, vines along the ceiling, FlipV Hydroponic Farm and Farm Tile plus an upright regression, the side screen, a save/load round trip; wild tests need a natural ceiling made in code, because the test box's built tiles cannot be planted under.
**Reviewer findings**: N/A this phase.
**Verification evidence**: none run; code reading and a kanim render only. Nothing launched in the game.
**Decisions delta**: architecture, D1-D5, world-gen nodes on floors, decision record after integrate (all in ## Decisions).
**Key files delta**: design names the new files (VineMotherCeilingConfig, CeilingForm helper, patches, tests); the plan unit fixes the file list.
**What the next phase needs**: a plan unit turns units/02-explore-report.md (scaffold checklist) and units/03-design-report.md (patches, art, tests) into sequenced implement steps; risks to carry into tests: mod-removal behaviour (ceiling plants dropped on load; their branches' fate unconfirmed), the first-branch fix relying on the branch state machine starting after the patched method (T5 checks it), and the prefab field swap in #5-#8. Open: D6, which of the user's farming or Pip mods to run the tests beside (asked).

## Progress
- [x] Investigate — units 01 and 02 reported; findings above
- [x] Update this plan with findings
- [x] Architecture: design unit 03 — Approve with modifications; user chose D1-D5 (all as recommended)
- [x] Plan unit 04 (units/04-plan-report.md): steps below; order S1 → S2 → {S3, S4, S4a} → S5 (needs S3, S4a), S6 (needs S4, S4a) → S9; S7, S8 any time after S1; test steps queue on one game session; each step ends in the solution build and a commit
- [x] S1 scaffold (unit 05, run at default not basic: nine files incl. the sln and a new DLC field) — bea076e; 12 files; requiredDlcIds confirmed in KMod/Mod.cs 38/436/442-491; manager rebuild: OvagroOverhead and OvagroOverhead.Tests built, 0 errors, 288 warnings all in other mods
- [x] M1 store node: `ks integrate --at .` wrote mods/ovagro-overhead/.context/ and a mods/projects.md row (7bad16c); its unrequested write to common/.context/projects.md was restored from git and logged as friction; node placeholders (overview, META, domains/standards README) are filled at completion
- [x] S2 (unit 06) — fbd4703: VineMotherCeilingConfig.cs, CeilingForm.cs (219 lines); departures: net48 cannot use the game's default interface methods (CS8702), so GetDlcIds (null) and GetAnyRequiredDlcIds are written out; vanilla's no-op traits.Get dropped; base trait added twice as vanilla does. Manager build of the mod: 0 warnings, 0 errors
- [ ] S3+S4 patches #1-#9 in one unit (07) ← IN PROGRESS
- [ ] S4a test kit (natural-ceiling helper via SimMessages.ReplaceElement) + T1 registration (default)
- [ ] S5 tests T2-T5 Pips and vines (default)
- [ ] S6 tests T6-T8 plots and side screen (default)
- [ ] S9 save round trip T9/T10 (default)
- [ ] S7 mod README + CHANGELOG (basic); S8 Workshop description + tags (basic)
- [ ] Preview image (manager, shown to the user before commit)
- [ ] Manual checks: load a save without the mod; look of the flipped mother (T5 screenshot)
- [ ] Store: icon-integrate the new mod's node; land the design as a decision record (adr-discipline)
- [ ] Completion: review, retrospective, commit, PR

## Open Questions / Blockers
- Pip reachability of ceiling cells for a 1x2 hanging plant. Correction (user, 2026-10-17): Pips walk on walls and ceilings; unit 01 read only the default `NavType.Floor` and missed the nav grid. Passed to design unit 03.
- Whether `vine_mother_kanim` looks right flipped (art not opened).
- Unverified API details the implementers read from source: ReplaceElement arguments (Kit Dupes.cs), plant maturity API, how T2 drives the Pip's plantableCellQuery, requiredDlcIds list syntax in a real load (unit 04).
- Not in scope, noted: `common/tools/Kanim/kanim.py extract` reads only sharedassets0.assets and cannot extract DLC art (unit 03).
- testbed-base.sav is missing from the test kit (unit 02, gap).

## Constraints
- Only feature is ceiling planting for Ovagro; nothing else (user).
- Vines must grow as expected when planted on the ceiling (user).
- Farm Tiles: support only if they can face downward in the game (user) — they can.
- New mod follows the existing mods/<name> layout, build and release conventions of this repo.
- Store reads: units and the manager read packet pointers with `ks read`; packets are scoped to the folders a unit touches (user, 2026-10-17).
