# Unit 12 findings — review: TestRunner fixture clear (common/ fb3b432)

Model tier: default. Passes run: 1-5 (code-quality-rules). Read-only; no game launch.
Game types checked by decompiling `F:/SteamLibrary/.../Managed/Assembly-CSharp.dll` with ilspycmd
(`Building`, `BuildingPreview`, `BuildingUnderConstruction`, `BuildTool`).

Verdict: sound, approve with the fixes below. No critical findings.

## Moderate

### M1. A throw from one Building aborts the whole destroy step (Pass 1)
`common/tools/TestRunner/FixtureArea.cs:121-147` (new pass at 135-137).
The new `FindObjectsByType<Building>` loop runs inside the same `try` as the `Grid.Objects` collection and
the `KDestroyGameObject` loop. `Building.PlacementCells` calls `RefreshCells()` lazily, which dereferences
`Def.PlacementOffsets`. Any scene `Building` with a null `Def` or a broken state (a mod's own component, an
object mid-teardown) throws there, the catch at 144 logs a warning, and nothing is destroyed: not the
off-layer buildings, and not the objects the old `Grid.Objects` pass had already collected. Before this
commit the try could only fail on grid reads. Failure scenario: one odd Building anywhere in the world, even
far from the area, and every clear in the session leaves the previous test's buildings standing (the new
warning then names them, so it is visible, but the run goes on with dirty areas). Likelihood is low; blast
radius is every mod's suite. Fix: guard the scene scan per building (or in its own try) so the layer pass
always reaches the destroy loop.

### M2. Not verified in game (Pass 5)
`.context/tasks/OM-009-jumper-heavy-wire/units/11-implement-report.md` says so itself: build only, exit 0;
the expected destroy counts (WireBridgeOnPlateEnd 12, BatteryBridgeLamp 10) are predictions. No test covers
the off-layer case. Needs the narrowest in-game run (the two jumper-bridges tests from unit 10) before the
fix is called done, plus one other mod's suite with boxed tests (ovagros-overhead `Plots`) to show
`ClearInside` still keeps the life support.

## Minor

### m1. Build-tool previews are Buildings too (Pass 1 / Pass 4)
`FixtureArea.cs:135`, class summary `FixtureArea.cs:13-15`, `README.md` (new lines), unit 11 report.
`BuildingPreview : Building` in the game, so the scan also destroys active previews whose cells touch the
rect: the `BuildTool` visualizer and the utility tool's path previews. The summary, README and report say
"built or under construction", which is not the whole set. Between tests this is harmless:
`Harness.ResetTools()` (`TestBehaviour.cs:321`) switches to the select tool before the clear at
`TestBehaviour.cs:184/186`, and that destroys the visualizers. Inside a test, `Dupes.ClearBox`
(`Kit/Dupes.cs:96`) calls `ClearInside` with whatever tool is active; destroying `BuildTool.visualizer`
under it leads to missing-reference errors on its next update, which fail the test. No mod calls `ClearBox`
today, so it is latent. A preview's `PlacementCells` are also stale: they are computed once in
`Building.OnSpawn` and `BuildTool` moves the visualizer with `transform.SetPosition`, so `Touches` tests
where it spawned, not where it is. Fix: skip `BuildingPreview` in the scan (`if (building is BuildingPreview) continue;`).
(`BuildTool` already writes its visualizer into `Grid.Objects[cell, TileLayer]` for tiles, so the old layer
pass had a narrow version of this exposure.)

### m2. Dead fallback in Touches (Pass 4)
`FixtureArea.cs:205-207`. `Building.PlacementCells` never returns null: the getter runs `RefreshCells()`
when the field is null, and that always assigns an array. The `cells == null` branch never runs. When
`Grid.PosToCell` is negative, `RefreshCells` leaves a zero-filled array (cell 0); cell 0 is never in a
fixture rect, so that is harmless. Drop the branch or keep it with a note; it currently reads as a real case.

### m3. WarnStanding misses replaced objects (Pass 1, by design)
`FixtureArea.cs:215-236`, `Kit/World.cs:37`. Only the GameObject `World.Build` returned is tracked. If a
build is replaced by another GameObject (an under-construction build completing with instant build off, a
tile replaced), the survivor is untracked and goes unreported. Not wrong, just narrower than the README line
"Anything `t.World.Build` made" suggests. No change needed beyond wording, if any.

### m4. Store record stale (Pass 4)
`tools/TestRunner:domains/entities.md` FixtureArea (blk-633dd6387bca) still lists "objects, loose items,
solid cells". Unit 11 already handed this to the coordinator as a store note; it should land with the fix.

## Checked, no finding

- **Multi-cell buildings partly outside the rect**: destroyed, as before. `BuildingComplete` registers every
  placement cell on its object layer, so the old `Grid.Objects` pass already destroyed any building with a
  cell in the rect. The new pass only adds buildings that hold no layer slot in the rect. Nothing that
  survived before and has a layer slot changes.
- **Box and life support**: `ClearInside` uses `Inside` (inset by one), so the 1x1 wall tiles on the edge
  never touch it; `keep` (`FixtureArea.LifeSupport`) is skipped in both the scan and `WarnStanding`. A full
  `Clear` nulls `keep`, so the life support goes, as before.
- **Under construction**: `BuildingUnderConstruction : Building`, included; same destroy path as before.
- **Double destroy**: the scan adds to the same `HashSet<GameObject>`, so a building found by both passes is
  destroyed once. Destroy is deferred to frame end, so a cleanup that destroys a set member early is the same
  risk as before.
- **Cost**: one `FindObjectsByType<Building>` (inactive excluded, so prefab templates are not returned) plus a
  placement-cell loop per clear, once per test. Small next to the four or more frames each clear waits.
- **Warning false positives**: `WarnStanding` runs after at least four `yield return null`s past the
  destroys; `RemoveAll(go => go == null)` uses Unity's overloaded equality on a `GameObject`-typed lambda, so
  destroyed objects drop out. It is a `LogWarning`, and between tests it runs after `capture.End()`, so it
  cannot fail a test either way.
- **API**: `FindObjectsByType(FindObjectsSortMode.None)` is already used by sweep-zones, smooth-borders and
  ovagros-overhead tests on this game build; `Object` resolves to `UnityEngine.Object` (no `using System;` in
  the file).
- **Other suites**: no mod test references `FixtureArea` or depends on a building outside the layer grid
  surviving a clear. KeepSave tests (1 in mods) skip the clear entirely (`TestBehaviour.cs:181-182`);
  generated-world tests never clear (`TestBehaviour.cs:177-178`). No mod calls `Dupes.ClearBox`.
- **Style** (`tools/TestRunner:standards/code-style.md`): tabs, Allman braces, unbraced one-statement
  bodies, `///` summaries on the new members, `Session.Tag` warnings matching the existing "threw" pattern,
  fully qualified `Isochronous.TestRunner.FixtureArea` from the kit as in `Dupes.cs`. `Track` is `internal`,
  same assembly. README updated with the behaviour.
- **Security** (Pass 2): no input, paths or secrets touched.
