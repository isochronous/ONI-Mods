# Unit 07 — implement: Steps S3 + S4, Harmony patches #1-#9

Steps S3 and S4: the seed-side patches (#1 `PlantableCellQuery.CheckValidPlotCell`, #2 `SeedPlantingStates.CheckValidPlotCell`, #3 `PlantableSeed.TryPlant`, #9 `VineBranch.Instance.SetupRootInformation`) and the plot-side patches (#4 `PlantablePlot.SpawnOccupyingObject`, #5 `PlantablePlot.SetPreview`, #6 `ReceptacleSideScreen.Initialize`, #7 `PlanterSideScreen.SetResultDescriptions`, #8 `PlantablePlot.OnCopySettings`).
Ticket: OM-006

## Files to create/modify
As listed under S3 and S4 in units/04-plan-report.md, in mods/ovagro-overhead/src/OvagroOverhead/ (and Mod.cs if patching is applied there). The design is units/03-design-report.md, "Game methods patched (10)" and "Routing": prefix + finalizer pairs around `CeilingForm.Enter`/`Exit` for #1-#8, a postfix for #9. Use `CeilingForm` as unit 06 built it (units/06-implement-report.md).

## Pattern to follow
The repo's Harmony patch conventions per the packet's pointers and the template mod; `__state` and `Finalizer` precedents named in the design (vent-freeze-fix DebugSave.cs, natural-backwalls-everywhere WorldgenThreadPatch.cs). Confirm every target's exact signature, access and overloads in the decompiled source (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\) before writing its patch.

## Prior reports
units/03-design-report.md, units/04-plan-report.md (S3, S4), units/06-implement-report.md.

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds with no new warnings; the report shows the output tail. Do not claim success without showing it.
- Every patch target resolves at patch time: a target that is private, an overload or a nested type is named so Harmony binds it, and the report lists each target with the signature it binds.
- Every swap is restored on every path, exceptions included.
- No player-visible strings added. Do not launch the game; in-game tests are S4a-S9.
- Commit all created/modified files in one commit on OM-006-ovagro-overhead: `OM-006: route Ovagro seeds to the ceiling form`, with trailers
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
  Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/07-implement-report.md
- Must contain: the diff summary, the target/signature table, the build output tail verbatim, the commit hash, every departure from the design and why, anything the game code contradicted, and a "Store reads" section.
- Model tier: complex
