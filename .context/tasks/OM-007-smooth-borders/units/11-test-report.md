# Unit 11 report: vanilla coverage across modes, DLCs and unusual asteroids

## Status

Done. Every world in scope passes on the final code: base game, all three worldgen DLCs in both modes, the five Spaced Out mini clusters (all three upside-down asteroids), and the Skewed Asteroid in both modes, each at defaults plus maxima on the upside-down and skewed asteroids. The WorldgenDriver sweep covers all 43 selectable vanilla clusters at defaults and at maxima: 43/43 complete each time, and every bordered asteroid got a logged flat wall at the configured thickness.

No mod defect was found. Three runs failed before the final code, and all three were test expectations, not mod behaviour. Section 4 has the evidence:
- the game's fixed-position templates
- the game's own vacuum skip
- magma-bed neutronium on a 64-wide moonlet

The fourth thing section 4 covers is that templates move at the maxima.

**Commits, by path:**
- `common`, branch OM-007-smooth-borders: `d73e603` "TestRunner: -Mode base|spacedout switches the game's mode for a run; no fixture needed for worldgen-only runs" (GameMode.ps1 new; Invoke-OniTests.ps1, Watch-OniGame.ps1, README.md). **Manager: bump the `common` pointer** in ONI-Mods. I did not stage it.
- ONI-Mods, branch OM-007-smooth-borders: `d2ebddf` "OM-007: Clusters tests for DLC, mini, upside-down and skewed asteroids; inner neutronium check" (`SmoothBorders.Tests/Clusters.cs` new; `Identity.cs`, `Support.cs`).
- Not mine, left alone: the untracked `.context/tasks/OM-007-smooth-borders/handoff-13.md`.

## 1. Harness: base-game mode (`-Mode`)

The harness could not switch modes. The launcher read the player's mode from `kplayerprefs.yaml`, and a coordinate of the other mode was skipped. It also required `fixtures/testbed-base.sav`, which does not exist.

Added to `Invoke-OniTests.ps1`: `-Mode auto|base|spacedout`. The default is `auto`, the player's mode.

**Switching (`GameMode.ps1`):**
- The player's `EXPANSION1_ID.ENABLED` value (`1`, `0`, or `none` when the line is missing) is kept in `kplayerprefs.yaml.onitest-mode`.
- Only that line is rewritten, with the file's CRLF line ends preserved.
- The value is put back after the game exits:
  - by the warden (new `-Prefs` argument)
  - by the launcher's `finally`, when there is no warden
  - by the next launcher, after a run that died
- `auto` reads the kept value while a switch is live, so a later `auto` run sees the player's mode and not the switched one.
- The mode was already part of the loadout key, so a run in the other mode ends the live game and launches a new one.

**No fixture:** with no fixture for the mode, the launcher now warns and continues. Worldgen and NoColony tests need none. A test that does need one ends the run as `no-save` (Session.Continue's existing path). An explicit `-Fixture` that is missing still throws.

**Checks:**
- **Offline:** `GameMode.ps1` was exercised on copies of the prefs file in four cases: 1→0→1, 0→1→0, a missing line →1→missing, and a repeated switch and restore. Each copy came back byte-identical (`cmp`).
- **First regex bug:** the first version missed the line because .NET's multiline `$` stops before `\n`, not `\r`. It was fixed before any game run.
- **In the game:**
  - The warden logged `put the game's mode back (EXPANSION1_ID.ENABLED 1)`.
  - After every run, `kplayerprefs.yaml` holds `EXPANSION1_ID.ENABLED: 1` and no `.onitest-mode` file is left.
  - `mods.json` SHA1 is `4779bb7d13cd14fb2d029378ae4a089f9b144f57`, as before the first run.
  - `mods\config\SmoothBorders` is absent before and after.

## 2. Test changes

**`Clusters.cs`:** 61 `Explicit` tests, using Identity's three-test pattern (Vanilla control, Flat, Same) for each world, plus `AtMaxima` on the upside-down and skewed clusters. Run them with `-Filter '*Clusters.*'`, adding `-Mode base` for the base-game ones. They are Explicit so the default run stays at 6 worlds.

**Flat tests:** the Flat test also asserts that the cluster has the named asteroid (`MiniFlippedStart`, `MiniFlipped`, `MiniFlippedWarp`, `StrangeAsteroidKleiFest2023[Cluster]`, and the DLC primaries).

**"No neutronium removed outside the border strip" (`Support.InnerNeutronium`, `ExpectInnerNeutroniumKept`):**
- **Baseline:** the Vanilla control now also writes every neutronium cell deeper than the strip. The strip on each edge is `max(F, T+R)`, where T+R is the deepest wall the game draws. Coordinates are counted from the asteroid's corner.
- **At defaults:** `Identity.SameAndNoGrowth`, now shared with `Clusters`, requires the same cells, none gone and none new. This also runs on Terrania, classic Terra and base Terra.
- **At maxima:** the compare is asserted only when every building and geyser sits where the vanilla world has it. Otherwise it is logged (section 4.4).

**`ExpectFlat`, gaps the game makes:** two kinds of gap are now excused, each logged as a note with its cells:
- cells stamped by a template the world file places with `overridePlacement` (`FixedTemplateCells`, read from `worldTemplateRules` and `TemplateCache`)
- vacuum cells on lines whose edge point lies in terrain tagged `RemoveWorldBorderOverVacuum` (`EdgePointSkipsVacuum`, read from `SaveLoader.clusterDetailSave.overworldCells`)

Both follow `WorldGen.DrawWorldBorder` and `TemplateSpawning` (decompiled). A gap message now names its first four cells and their elements.

**Magma allowance:** `Edge.MagmaAllowed` changed from lines/10 to lines/4 (section 4.3).

**Build:** `Invoke-OniTests.ps1 -BuildOnly` printed `Build succeeded. 0 Warning(s) 0 Error(s)` and `Built and checked against the game assemblies`.

## 3. Runs (verbatim, every run, in order)

The full per-test lines are in each run's `summary.txt` under `common\tools\TestRunner\runs\`. SKIPPED lines are left out below; they are coordinates of the other mode.

**`sb-base-20261019-131516`**, `-Mode base -Filter '*Identity.BaseTerra*;*Clusters.*'`, first base-game run:
```
FAIL     SmoothBorders.Tests/Clusters.SkewedBaseAtMaxima (0.6 s): every wall is flat at the configured thickness: worlds/StrangeAsteroidKleiFest2023 left: 17 cell(s) nearer the edge than 16 are not neutronium (expected 0, got 1)
Session done: 21 passed, 1 failed, 0 errors, 0 timeouts, 43 skipped; 21 passed, 1 failed, 0 errors, 0 timeouts, 43 skipped of 65; 311 s real time; fixture loaded again 14 time(s)
Game live after 325 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-base-20261019-131516
```
The other 21 passed: BaseTerra ×3 and every base-game Clusters test except SkewedBaseAtMaxima.

**`sb-base-skew-20261019-132324`**, `-Filter '*Clusters.SkewedBase*'`, after the fixed-template excuse:
```
Session done: 4 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 4 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 4; 61.5 s real time; fixture loaded again 2 time(s)
```

**`sb-so-dlc-20261019-132454`**, `-Mode spacedout -Filter '*Clusters.Ceres*;*Clusters.Prehistoric*;*Clusters.Aquatic*'`:
```
FAIL     SmoothBorders.Tests/Clusters.AquaticClassicFlat (0.6 s): every wall is flat at the configured thickness: dlc5::worlds/AquaticClassicAsteroid left: 1 cell(s) nearer the edge than 4 are not neutronium ((3, 321) Vacuum) (expected 0, got 1)
Session done: 20 passed, 1 failed, 0 errors, 0 timeouts, 15 skipped; 20 passed, 1 failed, 0 errors, 0 timeouts, 15 skipped of 36; 414.2 s real time; fixture loaded again 13 time(s)
```

**`sb-so-aquc-20261019-133323`**, `-Filter '*Clusters.AquaticClassic*'`, after the vacuum excuse:
```
Session done: 3 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 3 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 3; 72.5 s real time; fixture loaded again 1 time(s)
```

**`sb-so-mini-20261019-133502`**, `-Filter '*Clusters.Skewed*;*Clusters.Mini*'`:
```
FAIL     SmoothBorders.Tests/Clusters.SkewedFlat (0.7 s): every wall is flat at the configured thickness: expansion1::worlds/MarshyMoonlet bottom: neutronium at depth 1 on 0 of 62 lines outside magma and 13 in magma (zones: MagmaCore, MagmaCore, MagmaCore, MagmaCore, MagmaCore) (expected 0, got 1)
FAIL     SmoothBorders.Tests/Clusters.SkewedAtMaxima (0.0 s): no neutronium inside the border strips is gone: MiniRegolithMoonlet 26 10, MiniRegolithMoonlet 27 10, MiniRegolithMoonlet 28 10, MiniRegolithMoonlet 29 10, WaterMoonlet 40 71, WaterMoonlet 41 71, WaterMoonlet 42 71, WaterMoonlet 43 71 (expected true)
FAIL     SmoothBorders.Tests/Clusters.MiniBadlandsAtMaxima (0.0 s): no neutronium inside the border strips is gone: MiniBadlandsStart 13 60, MiniBadlandsStart 14 60, MiniBadlandsStart 14 90, MiniBadlandsStart 15 60, MiniBadlandsStart 15 90, MiniBadlandsStart 16 60, MiniBadlandsStart 16 90, MiniBadlandsStart 17 90 (expected true)
FAIL     SmoothBorders.Tests/Clusters.MiniFlippedAtMaxima (0.0 s): no neutronium inside the border strips is gone: MiniBadlandsWarp 114 54, MiniBadlandsWarp 115 54, MiniBadlandsWarp 116 54, MiniBadlandsWarp 117 54, MiniBadlandsWarp 46 96, MiniBadlandsWarp 47 96, MiniBadlandsWarp 48 22, MiniBadlandsWarp 48 96 (expected true)
FAIL     SmoothBorders.Tests/Clusters.MiniRadioactiveAtMaxima (0.0 s): no neutronium inside the border strips is gone: MiniBadlands 102 89, MiniBadlands 103 89, MiniBadlands 104 89, MiniBadlands 105 89, MiniBadlands 30 90, MiniBadlands 31 90, MiniBadlands 32 90, MiniBadlands 33 90 (expected true)
Session done: 17 passed, 5 failed, 0 errors, 0 timeouts, 4 skipped; 17 passed, 5 failed, 0 errors, 0 timeouts, 4 skipped of 26; 454.1 s real time; fixture loaded again 15 time(s)
Game reused after 454 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-so-mini-20261019-133502
```

**Final runs on the committed code** (d2ebddf / d73e603):

`sb-base2-20261019-135652`:
```
PASS     SmoothBorders.Tests/Clusters.CeresBaseVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresBaseFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresBaseSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresShatteredBaseVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresShatteredBaseFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresShatteredBaseSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricBaseVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricBaseFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricBaseSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricShatteredBaseVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricShatteredBaseFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricShatteredBaseSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticBaseVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticBaseFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticBaseSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedBaseVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedBaseFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedBaseSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedBaseAtMaxima (0.1 s)
PASS     SmoothBorders.Tests/Identity.BaseTerraVanilla (0.0 s)
PASS     SmoothBorders.Tests/Identity.BaseTerraFlat (0.0 s)
PASS     SmoothBorders.Tests/Identity.BaseTerraSame (0.0 s)
Session done: 22 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 22 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 22; 308.3 s real time; fixture loaded again 14 time(s)
Game live after 323 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-base2-20261019-135652
```
`sb-so-dlc2-20261019-140226`:
```
PASS     SmoothBorders.Tests/Clusters.CeresVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresClassicVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresClassicFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresClassicSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresMiniVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresMiniFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.CeresMiniSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricClassicVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricClassicFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.PrehistoricClassicSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticClassicVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticClassicFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.AquaticClassicSame (0.0 s)
Session done: 21 passed, 0 failed, 0 errors, 0 timeouts, 15 skipped; 21 passed, 0 failed, 0 errors, 0 timeouts, 15 skipped of 36; 409.3 s real time; fixture loaded again 13 time(s)
Game live after 427 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-so-dlc2-20261019-140226
```
`sb-so-mini2-20261019-134439`:
```
PASS     SmoothBorders.Tests/Clusters.SkewedVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.SkewedAtMaxima (0.1 s)
PASS     SmoothBorders.Tests/Clusters.MiniBadlandsVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniBadlandsFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniBadlandsSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniBadlandsAtMaxima (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFlippedVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFlippedFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFlippedSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFlippedAtMaxima (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFrozenVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFrozenFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniFrozenSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniSwampyVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniSwampyFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniSwampySame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniRadioactiveVanilla (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniRadioactiveFlat (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniRadioactiveSame (0.0 s)
PASS     SmoothBorders.Tests/Clusters.MiniRadioactiveAtMaxima (0.0 s)
Session done: 22 passed, 0 failed, 0 errors, 0 timeouts, 4 skipped; 22 passed, 0 failed, 0 errors, 0 timeouts, 4 skipped of 26; 465.5 s real time; fixture loaded again 15 time(s)
Game live after 482 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-so-mini2-20261019-134439
```
`sb-so-default-20261019-135312`:
```
PASS     SmoothBorders.Tests/AsteroidKinds.EveryWorldInTheTable (0.0 s)
PASS     SmoothBorders.Tests/AsteroidKinds.DefaultsAreTheGamesThinnest (0.0 s)
PASS     SmoothBorders.Tests/AsteroidKinds.PairsOutsideTheTable (0.0 s)
PASS     SmoothBorders.Tests/AsteroidKinds.ConfigLimitedOnRead (0.0 s)
PASS     SmoothBorders.Tests/Dialog.LabelsAndTooltips (1.6 s)
PASS     SmoothBorders.Tests/Dialog.IntroTopLeft (1.6 s)
PASS     SmoothBorders.Tests/Dialog.SlidersAndDefaults (1.5 s)
PASS     SmoothBorders.Tests/Dialog.SavesAndResets (5.1 s)
PASS     SmoothBorders.Tests/Flatness.AtMaxima (0.0 s)
PASS     SmoothBorders.Tests/Flatness.RectanglesGrowAtMaxima (0.0 s)
PASS     SmoothBorders.Tests/Flatness.AtMixedValues (0.0 s)
PASS     SmoothBorders.Tests/Identity.TerraniaVanilla (0.0 s)
PASS     SmoothBorders.Tests/Identity.TerraniaFlat (0.0 s)
PASS     SmoothBorders.Tests/Identity.TerraniaSame (0.0 s)
PASS     SmoothBorders.Tests/Identity.ClassicTerraVanilla (0.0 s)
PASS     SmoothBorders.Tests/Identity.ClassicTerraFlat (0.0 s)
PASS     SmoothBorders.Tests/Identity.ClassicTerraSame (0.0 s)
Session done: 17 passed, 0 failed, 0 errors, 0 timeouts, 3 skipped; 17 passed, 0 failed, 0 errors, 0 timeouts, 3 skipped of 20; 188.9 s real time; fixture loaded again 7 time(s)
Game live after 205 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-so-default-20261019-135312
```

**WorldgenDriver sweep:** 43 coordinates (all selectable vanilla clusters, `all.txt` below), `-Mods SmoothBorders`, run once with `-ModConfig SmoothBorders=defaults.json` (4/3/2/1) and once with `maxima.json` (16/7/6/3):
```
sb-sweep-defaults-20261019-141003:  Results: C:\dev\ONI-Mods\common\tools\WorldgenDriver\runs\sb-sweep-defaults-20261019-141003.jsonl
  complete     43
sb-sweep-maxima-20261019-142159:    Results: C:\dev\ONI-Mods\common\tools\WorldgenDriver\runs\sb-sweep-maxima-20261019-142159.jsonl
  complete     43
```

A script (`scratchpad\sweep\analyze.py`, not committed) reads every session's Player.log copy. For each coordinate it splits off the mod's `[SmoothBorders]` lines at the driver's `finished` lines. It then checks every bordered world of the cluster file:
- the mod logged a flat wall for it
- F equals the thickness the config gives for its (T, R)

Result: `43 coordinate(s), 0 with a problem` for both configs, and no worldgen errors in any result.
- **Grew lines at the defaults:** 0 everywhere.
- **Grew lines at the maxima:**
  - 3 per base-game world
  - 9–13 per Spaced Out cluster
  - 8 for M-CERS-C

The sweep proves generation completes and that the mod drew each asteroid at the right F. It does not measure flatness; the in-game tests do that.

## 4. Findings (the earlier failures, with evidence)

### 4.1 Fixed-position templates cut into a thick wall (game behaviour, not a mod defect)

**What failed:** KF23-A at the maxima had 17 non-neutronium cells within 16 tiles of the left edge.

**Cause:** `StrangeAsteroidKleiFest2023.yaml` places three `geysers/volcanohole` templates with `overridePlacement`, at (20,161), (22,111) and a third that lands at (15,52) (vanilla placements file: `GeyserGeneric_big_volcano 15 52`). The template is 6 wide from x−2, so it covers x 13..18. `TemplateSpawning` places an `overridePlacement` template at that position without consulting the border or the POI bounds (decompiled `TemplateSpawning.cs` ~285). The game would do the same wherever its own walk reached past 13.

**Evidence:**
- The 17 cells are Magma: (14,49), (15,49), (13,50), and so on. The nearest is 13 from the edge, so 13 tiles of neutronium still seal the edge.
- Screenshot: `common\tools\TestRunner\runs\sb-base-20261019-131516\artifacts\SmoothBorders.Tests.Clusters.SkewedBaseAtMaxima-StrangeAsteroidKleiFest2023-left-gap.png`. It shows magma jutting into the wall and the template's own neutronium bar.
- The same 17 cells appear on KF23-C's Skewed asteroid at the maxima.

**Status:** the test now excuses `overridePlacement` template cells and logs them. This matches design 2.x, "except overridePlacement, same as vanilla". **Manager's call** whether a Primary setting above 13 on the Skewed Asteroid deserves a note for players.

### 4.2 The game's vacuum skip leaves a wall cell open (game behaviour, the mod keeps it)

**What failed:** V-AQU-C-100 left edge had one Vacuum cell at (3,321), depth 3, in a zone that is not Space.

**Cause:** `DrawWorldBorder` skips vacuum on any line whose edge point lies in terrain tagged `RemoveWorldBorderOverVacuum`, whatever the zone. The test only excused Space zones.

**Evidence:** the vanilla control of the same coordinate has the same gap: `left 1 gap(s)` in `AquaticClassicVanilla`. The new excuse matched exactly this cell (`1 vacuum cell(s) ... (3, 321) Vacuum`).

### 4.3 Magma-bed neutronium on a small moonlet (test tolerance)

**What failed:** MarshyMoonlet's bottom edge in KF23-C-7 (F = 1) had neutronium at depth 1 on 13 of 62 lines, all in MagmaCore. The old allowance was max(2, 62/10) = 6.

**Evidence:**
- The mod logged `flat wall 1` for it.
- The screenshot (`sb-so-mini-20261019-133502\artifacts\SmoothBorders.Tests.Clusters.SkewedFlat-MarshyMoonlet-bottom-deep.png`) shows the one-tile wall and the magma bed's own neutronium blobs touching it.

**Measured shares of magma lines with neutronium at depth F:**
- the mod's worlds: at most 21% (this edge); every other edge in this unit's runs is at 8% or less
- the game's own wall on the same seeds: 32–53%

**Change:** the allowance is now lines/4. The non-magma allowance (lines/50) is unchanged. It is what tells the game's uneven wall from a flat one: vanilla shows 80–157 deeper lines per edge there, the mod 0.

### 4.4 At the maxima, templates move, and their neutronium moves with them (by design)

The grown POI rectangles change where templates land: 58 to 1791 building and geyser placements differ from vanilla. Templates carry neutronium of their own, such as the 4-wide bars under some geysers, so that neutronium moves with them. The counts match that:

| Cluster | Gone | New |
|---|---|---|
| KF23-C | 12 | 8 |
| M-BAD-C | 85 | 81 |
| M-FLIP-C | 101 | 101 |
| M-RAD-C | 64 | 60 |

So at the maxima the cell compare is logged, not asserted, whenever placements differ. On the base-game Skewed Asteroid only one placement moved, and every cell was kept: KF23-A-7 at the maxima, `201 ... 201 in the baseline; 0 gone, 0 new` (asserted in sb-base-skew, where no placement differed; logged in sb-base2).

**At the defaults the compare is exact on every world in this unit** (table below).

## 5. World × setting × result

Final runs:
- base game: `sb-base2-20261019-135652`
- Spaced Out DLC clusters: `sb-so-dlc2-20261019-140226`
- skewed and mini clusters: `sb-so-mini2-20261019-134439`
- default suite: `sb-so-default-20261019-135312`

Column meanings:
- **Asteroids:** asteroids measured.
- **Flat at defaults:** 0 gaps on every edge, and 0 deeper lines outside magma on every edge.
- **Same:** buildings and geysers equal to the vanilla control, plus inner neutronium equal.

| Mode | Coordinate | Asteroids | Flat at defaults | Same: placements | Same: inner neutronium | Maxima |
|---|---|---|---|---|---|---|
| base | SNDST-A-100 (Terra) | 1 | pass | 1450 = 1450 | 232 = 232 | sweep only |
| base | CER-A-100 (Ceres) | 1 | pass | 776 = 776 | 66 = 66 | sweep only |
| base | CERS-A-100 (Blasted Ceres) | 1 | pass | 619 = 619 | 110 = 110 | sweep only |
| base | PRE-A-100 (Relica) | 1 | pass | 396 = 396 | 60 = 60 | sweep only |
| base | PRES-A-100 (shattered Relica) | 1 | pass | 412 = 412 | 130 = 130 | sweep only |
| base | AQU-A-100 (Marinea) | 1 | pass | 1233 = 1233 | 262 = 262 | sweep only |
| base | KF23-A-7 (Skewed) | 1 | pass | 1046 = 1046 | 201 = 201 | pass: flat at 16; 17 template cells excused (4.1); 201/201 kept |
| SO | CER-C-100 | 9 | pass | 2492 = 2492 | 174 = 174 | sweep only |
| SO | V-CER-C-100 | 8 | pass | 2331 = 2331 | 310 = 310 | sweep only |
| SO | M-CERS-C-100 (Ceres mini) | 10 | pass | 2217 = 2217 | 163 = 163 | sweep only |
| SO | PRE-C-100 | 9 | pass | 1896 = 1896 | 144 = 144 | sweep only |
| SO | V-PRE-C-100 | 8 | pass | 2207 = 2207 | 307 = 307 | sweep only |
| SO | AQU-C-100 | 9 | pass | 1691 = 1691 | 199 = 199 | sweep only |
| SO | V-AQU-C-100 | 8 | pass (1 vacuum cell excused, 4.2) | 1831 = 1831 | 377 = 377 | sweep only |
| SO | KF23-C-7 (Skewed cluster) | 8 | pass | 2448 = 2448 | 336 = 336 | pass: flat at every maximum; 17 template cells excused; templates moved (4.4) |
| SO | M-BAD-C-100 (MiniFlipped) | 11 | pass | 1548 = 1548 | 210 = 210 | pass: flat; templates moved |
| SO | M-FLIP-C-100 (MiniFlippedStart) | 11 | pass | 1766 = 1766 | 198 = 198 | pass: flat; templates moved |
| SO | M-FRZ-C-100 (MiniFlipped) | 11 | pass | 1762 = 1762 | 226 = 226 | sweep only |
| SO | M-SWMP-C-100 (MiniFlipped) | 11 | pass | 1698 = 1698 | 227 = 227 | sweep only |
| SO | M-RAD-C-100 (MiniFlippedWarp) | 11 | pass | 1680 = 1680 | 210 = 210 | pass: flat; templates moved |
| SO | SNDST-C-100 (Terrania) | 9 | pass | 1759 = 1759 | 188 = 188 | pass (Flatness.AtMaxima, AtMixedValues) |
| SO | V-SNDST-C-100 (classic Terra) | 8 | pass | 2327 = 2327 | 257 = 257 | sweep only |
| both | the other 22 selectable clusters | — | sweep: logged F correct | — | — | sweep: complete, logged F correct |

The 22 sweep-only clusters:
- base game: BAD-A, FRST-A, HTFST-A, LUSH-A, OASIS-A, OCAN-A, S-FRZ, VOLCA
- Spaced Out: SWMP-C, FRST-C, V-CERS-C, V-PRES-C, V-FRST-C, V-HTFST-C, V-BAD-C, V-LUSH-C, V-OASIS-C, V-OCAN-C, V-SFRZ-C, V-SWMP-C, V-VOLCA-C
- every SO cluster above, at the maxima

Every Vanilla control passed: the measure found the game's own wall deeper than the default on some edge, and placements and inner neutronium were written.

## 6. Not covered, and why

- **TheRock:**
  - `worldgen/worlds/TheRock.yaml` has `skip: true` and `DrawWorldBorder: false`.
  - No cluster file names it.
  - Covered by the mapping tests only (`AsteroidKinds.PairsOutsideTheTable`: (0,0) → 0).
- **TinyMagma:**
  - Its pair is (0,0), and no cluster file names it.
  - Covered by the mapping tests only: (0,0) and (−1,0) → 0.
  - `Probe` with T ≤ 0 stays untested in the game, as in unit 08.
- **Editor-only clusters** (`skip: EditorOnly`): BIG-E-A/C, TNY-E-A/C, TNY-SURF-A/C, TNY-C, TWOWORLD. They are not selectable, so they were not run.
- **Full flat and identity tests** ran on the 22 clusters in the table. The other 22 selectable clusters had only the sweep: generation completes and the mod logs the right F. That includes V-CERS-C and V-PRES-C, the classic shattered Ceres and Relica. Adding them is three lines each in `Clusters.cs`.
- **One seed per cluster:** 100, or 7 for KF23, the fixed seed its destination screen forces.
- **Story traits and mixing:** off in every coordinate (`-0-0`). The sweep tool supports `-Stories all` and `-Mixing all` and was not run with them.
- **Maxima in full tests:** only on the upside-down and skewed clusters, plus Terrania (unit 08). Every cluster at the maxima is in the sweep only. Mixed values (7/1/5/2) were run only on Terrania.
- **Worldgen mods** (MiniBase and others) are out of scope per the spec.

## 7. Colonies and saves (manager's request)

Every generated world embarks, so the game named a colony. Names come from the run's Player.log: the harness's `blocked a save to ...\<name>\<name>.sav` lines and the `RetiredColonies\<name>` screenshot lines. Each name below has a `Documents\Klei\OxygenNotIncluded\RetiredColonies\<name>\` folder now (checked by script). Nothing was deleted.

| Run | Names |
|---|---|
| sb-base-20261019-131516 | Data Spacejunk; The Luckless Dystopia; Spacerock; Cool Hovel; The Temporal Antfarm; Settlement; The Needy League; Antigravity Dwelling; Ill Eclipse; Clone Sewer; Quarantined Committee; The Clone Moonbase; Pit; Hovel; The Chaotic Colony |
| sb-base-skew-20261019-132324 | Spacerock; The Bustling Brigade; Inescapable Abyss |
| sb-so-dlc-20261019-132454 | Esoteric Debacle; Committee; The Antigravity Planet; Colony; Septic Bunker; The Duplicant Brigade; Rustic Homestead; Eclipse; Quarantined Spacejunk; Radical Spacejunk; Facility; The Coughing League; Leaky Phase; Cool Asteroid |
| sb-so-aquc-20261019-133323 | Temporal Cantaloupe; Impenetrable Moonbase |
| sb-so-mini-20261019-133502 | Panopticon; Antfarm; Impenetrable Eclipse; Dump; The Spacefaring Trainwreck; Deadly Fortress; The Optimistic Facility; Luxury Pit; The Radical Rat's Nest; The Esoteric Sewer; The Rustic Brigade; Galaxy; Gaggle; Beautiful Homestead; Plucky Crater; Underground Spaceprison |
| sb-so-mini2-20261019-134439 | The Galactic Paradise; The Intergalactic Antfarm; Outpost; Space Hut; The Merry Paradise; The Dangerous Galaxy; Burrow; The Sneezing Gang; The Ramshackle Space Hut; The Outrageous Gang; Antfarm; Grimy Utopia; The Doomed Burrow; Planet; Home; The Lunar Eclipse |
| sb-so-default-20261019-135312 | The Burgeoning Orbit; Dangerous Pigpen; Lunar Planet; The Radical Moonbase; The Immune Hospital; Debacle |
| sb-base2-20261019-135652 | Ultra Abyss; Cute Laboratory; Phase; Spacecamp; Regrettable Pothole; The Uncomfortable Laboratory; Festering Factory; The Rustic Home; The Gobsmacked Dreamland; The Plagued Committee; Moonbase; Quarantined Depot; Rosy Dystopia; Outpost |
| sb-so-dlc2-20261019-140226 | Swanky Timeline; Super Depot; Happy Colony; Suave Abode; The Unstoppable Pigpen; Clinical Dump; Clone Citadel; The Death Hospital; Dreamland; Unstoppable Base; Debacle; Factory; Grimy Trainwreck; The Quarantined Eclipse |

**Caution for the clean-up unit:** names repeat across runs (Spacerock, Antfarm, Outpost, Debacle). Generic names such as Colony, Committee, Eclipse, Facility, Home, Planet and Factory could also be the player's own retired colonies. Check folder dates (today, 13:15–14:10 EDT) before deleting.

**The last name in some runs has no screenshot line:** for example "The Chaotic Colony", "Inescapable Abyss" and "The Quarantined Eclipse". It comes from a blocked save as the run left its last colony, yet its RetiredColonies folder exists too.

**Saves:**
- No `.sav` file under `Documents\Klei\OxygenNotIncluded` was written after 13:00 today (script scan: 0). Every save to `cloud_save_files\...` was blocked by the harness.
- The WorldgenDriver sweeps do not embark (no `-Load`), so they made no colonies.

## 8. Harness observations (not fixed)

- **Misleading counter:** the status line's "fixture loaded again N time(s)" counts generated worlds in a worldgen-only run, where no fixture is loaded at all. For example, sb-base2 says 14 and loaded none.
- **Store records lag the README:** `-Mode` is in the README, but the TestRunner store records and the `testing/integration-testing.md#generated-worlds` block (`blk-87272da04dee`) still say "a coordinate of the other mode skips the test". They need a write-path update; I have no write path.
- **Unknown local mod in mods.json:** every launch printed `Registered new local mod 'SmoothBorders'`. The player's `mods.json` does not list SmoothBorders, so the launcher adds it each time, and the restore removes it again. Harmless.

## 9. Report back on the packet

- **Did not apply:** the common brief's ModCheck and tools overview blocks (`blk-3ca89d57dfde`, `blk-647210c6a78a`, `blk-6c2bbf052519`) and the WorldgenDriver code-style, naming and error-handling records. I worked from `Invoke-OniTests.ps1`, `Session.cs`, `Worldgen.cs`, the WorldgenDriver README and `Invoke-Worldgen.ps1` directly.
- **Missing:** as in unit 08, the main store's brief matched nothing for `mods/smooth-borders/src`. The testing records that govern this work (`blk-87272da04dee` generated worlds, `blk-c8a18ea4088d` per-test options, `blk-7316ec9fd58f` naming, `blk-33fb80177cba` pacing) are reachable only by tag. Also missing: nothing in the store records how the harness chooses the mode (now `-Mode`), or that `kplayerprefs.yaml` holds it.
- **Mismatched reasons:** none.

## 10. ICON tooling friction

None. `ks read --store C:/dev/ONI-Mods blk-87272da04dee blk-c8a18ea4088d blk-7316ec9fd58f blk-33fb80177cba` worked on the first try.

## 11. Other friction (not ICON)

- **Shell quoting:** a sed expression with backslashes broke in Git Bash, as `bash-heredoc-backslashes` warns. I moved the colony scan into a Python file written with Write.
- **Python `-I`:** it hides the user site-packages, so PIL was missing under `-I`. Screenshot crops ran without `-I`; they read only the harness's own PNGs.
- **Foreground sleep:** a `sleep 30` before a wait loop was refused by the tool. I waited with until-loops and background-task notifications instead.
