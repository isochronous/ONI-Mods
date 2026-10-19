# Unit 08 report: S5 and S6 tests (written and run)

**Status:** The tests were committed as `6944e9b`. The fixes from the runs are `2362df7`, `4d99e15` and `d49434f`, all on OM-007-smooth-borders and staged by path. Phase 2 has run. The harness check passed, with the config SHA1 unchanged. The SmoothBorders suite stands at 14 pass, 2 fail, 3 skipped (the base-game coordinates, in Spaced Out). Every worldgen test passes. Both failures are mod defects (Phase 2 results, section 3): the options dialog does not bind its four settings, and the 3-line intro area cuts off the last line.

## Files (all in `mods/smooth-borders/src/SmoothBorders.Tests/`)

| File | Class | Step |
|---|---|---|
| `AsteroidKinds.cs` | `AsteroidKinds` (4 NoColony tests) | S5 T2 |
| `Dialog.cs` | `Dialog` (3 NoColony tests) | S5 T1 |
| `Flatness.cs` | `Flatness` (3 tests, 2 worlds) | S6 T4a, T4b |
| `Identity.cs` | `Identity` (9 default tests and 5 Explicit; 4 worlds in Spaced Out) | S6 T3, T5 |
| `Support.cs` | `Support` (internal helpers) | shared |

**File and class names.** The plan named `Options.cs`, `Mapping.cs` and `Looks.cs` ("names free"). Discover runs classes in ordinal order of their full names. Under these names the two no-colony classes come before the worldgen classes, so a full run loads the fixture once instead of twice. `Mapping` became `AsteroidKinds` for that reason. `Options` and `Groups` could not be test class names, because inside `SmoothBorders.Tests` they would hide the mod's own types.

## What each test proves

### S5: no colony (`NoColony = true`)

**AsteroidKinds**
- **EveryWorldInTheTable.** Every world in `SettingsCache.worlds` that draws a border has a (WorldBorderThickness, WorldBorderRange) pair from the design 2.1 table, which the test holds as data. `Groups.ForWorld` gives the table's kind for each one. A world with a pair outside the table fails the test, so a game update that adds a new pair would show up here. T and R are read the way `WorldGenSettings.GetSetting` reads them: the world's `defaultsOverrides` first, then `SettingsCache.defaults`.
- **DefaultsAreTheGamesThinnest.** At the default config, every loaded world's flat thickness equals T−R, the game's thinnest wall. This is the design's claim that each default is vanilla's floor.
- **PairsOutsideTheTable.** With every setting at its maximum:
  - (12,3) is no kind and gives 9.
  - (2,5) gives 1 (never less than 1).
  - (0,0) and (−1,0) give 0.
  - (10,6) takes the Primary setting (16).
- **ConfigLimitedOnRead.** The test writes `{"Primary":99,"Small":0,"Moonlets":-5,"TinyMoonlets":2}` by hand to `mods\config\SmoothBorders\config.json`. `Options.Load()` reads it back as 16, 1, 1 and 2: out-of-range values are clamped and an in-range value is kept.

**Dialog.** Each test deletes the config file first, so the dialog shows what a new player sees. The harness puts the player's config back at the end of the run. Each test then opens `POptions.ShowDialog(typeof(Options))`, waits 1.5 s of real time, and finds the parts by PLib's object names: "Intro" for the text block and "Label" for the rows. The dialog is closed at cleanup.
- **LabelsAndTooltips.** Four rows, top to bottom: "Primary asteroids (1-16)", "Small asteroids (1-7)", "Moonlets (1-6)", "Tiny moonlets (1-3)". Their tooltips are exactly "Game default: 4." / "3." / "2." / "1.". The tooltip is read through `ToolTip.GetMultiString(0)`, because `toolTip` has no getter.
- **IntroTopLeft.** Takes the screenshot `Dialog.IntroTopLeft-dialog.png` for the user. It then checks that:
  - the intro text equals `ModStrings…INTRO`
  - it sits above the first row
  - it wraps (more than one line)
  - **every line of it starts where the rows' labels start** (within 4 px), measured from TMP's line and character info
- **SlidersAndDefaults.** Top to bottom, the four number fields read 4, 3, 2, 1. The sliders run from 1 to 16, 7, 6 and 3 and stand at 4, 3, 2, 1.

### S6: worldgen

**Prepare methods** (public static in each class, returning `System.Action`):
- `Identity.Defaults`, `Flatness.Maxima` and `Flatness.Mixed` write the config as JSON by hand. PLib's own writer only logs a failure; this one throws. They then start the log capture.
- `Identity.Vanilla` and `Identity.VanillaAgain` call `Support.VanillaDraw`, which works in three steps:
  1. **Find the patch owner.** It finds the Harmony id that owns the mod's `DrawWorldBorder` prefix, postfix and transpiler. It does this through `Harmony.GetPatchInfo`, matching `PatchMethod.DeclaringType == typeof(WorldGen_DrawWorldBorder_Patch)`, so nothing is hard-coded.
  2. **Unpatch.** It calls `Unpatch(DrawWorldBorder, HarmonyPatchType.All, id)` and checks that none of those patches remain.
  3. **Return the undo.** The undo stops the capture and re-applies the patches with `harmony.CreateClassProcessor(typeof(WorldGen_DrawWorldBorder_Patch)).Patch()`, which re-runs the transpiler on the original IL.

  The reverse-patch stub is a separate method, so the unpatch does not touch it.

**Log capture.** `Application.logMessageReceivedThreaded` catches the lines from the worldgen thread. The capture keeps every line containing `[SmoothBorders] `, under a lock, and is cleared when the next capture starts. The test code's own log lines use `[SmoothBorders.Tests]`, so they are never captured. Lines are parsed against unit 07's formats: `<world>: group <Id>, flat wall F tiles` and `<world>: <edge> POI rectangle grew from V to F tiles`.

**Flatness measure (`Support.Measure`).** For each asteroid that is not a rocket interior, and for every row (left and right edges) and column (bottom and top):
- **Gaps:** every cell nearer the edge than F must be neutronium. Cells in a Space zone are excused, because the game skips vacuum there (`RemoveWorldBorderOverVacuum`). Any gap fails the test. Gaps are also how template holes would show.
- **Deep:** the cell at depth F must not be neutronium. Corner lines, where the neighbouring edge's wall runs, are left out. A few cells are tolerated per edge, `max(2, lines/50)`, for neutronium the game places elsewhere; above that the test fails.
- **Photos:** up to 4 problem cells are photographed before the test fails. The fog is lifted around each one first, and the camera switches to its asteroid.

**ExpectFlat.** Used by T3, T4a and T4b. It checks that:
- the worldgen reported no errors
- for every asteroid with a wall, the mod logged a flat line with the thickness that `Groups.Thickness` gives for the config the test wrote (so the mod both drew the world and read this config)
- every edge measures flat at that thickness
- the cluster held every expected kind: all four for Terrania and classic Terra, Primary for base-game Terra

| Test | World | What it proves |
|---|---|---|
| `Identity.TerraniaVanilla`, `ClassicTerraVanilla`, `BaseTerraVanilla` (`control:`) | coordinate + `Vanilla` | The mod logged no flat wall, so the baseline really is the game's draw. The measure at the defaults finds the game's wall deeper than the default on at least one edge, which shows the flatness measure can tell. The buildings and geysers are written to `<run>\placements-vanilla-<coord>.txt`. |
| `Identity.TerraniaFlat`, `ClassicTerraFlat`, `BaseTerraFlat` (T3) | coordinate + `Defaults` | ExpectFlat at the defaults. It runs after a Vanilla world, so it also shows that the undo re-applied the patches: the mod's lines are back and the walls are flat. |
| `Identity.TerraniaSame`, `ClassicTerraSame`, `BaseTerraSame` (T5) | the same world as Flat | The mod drew this world (its flat lines are present), no "rectangle grew" line appeared at the defaults, and the buildings and geysers match the baseline file line for line. The list is spawnables of type Building, plus `GeyserGeneric*` and prefabs with a `Geyser` component, as "world id x y". Critters and plants are left out (design 2.4). The compared list is written beside the baseline file. |
| `Flatness.AtMaxima` (T4a) | Terrania + `Maxima` (16/7/6/3) | ExpectFlat at the maxima. Any template cell in the wall would be a gap. |
| `Flatness.RectanglesGrowAtMaxima` (T4a) | the same world | At least one "grew" line names TerraMoonlet. The test records every grew line. This is the counter path to "no growth at the defaults". |
| `Flatness.AtMixedValues` (T4b) | Terrania + `Mixed` (7/1/5/2) | ExpectFlat at a mixed set, with values both above and below the defaults. It is generated after the Maxima world in the same session, so the logged F values also show that the config was read again. |
| `Identity.TerraniaVanillaAgain` (Explicit) | Terrania + `VanillaAgain` | The self-check from the plan: a second vanilla generation of one coordinate places its buildings and geysers where the first did. Run it together with `TerraniaVanilla`. |
| `Identity.Terrania200*`, `Terrania300*` (Explicit) | seeds 200 and 300 | Baseline and Same for two more seeds. |

**Shared worlds.** The plan had T3 in `Flatness.cs`, in worlds of its own. Here T3 and T5 share one world per cluster: same coordinate, same `Defaults` Prepare, consecutive in one class. That saves three generations. A default run in Spaced Out generates 6 worlds; the 2 base-game Terra worlds are skipped. Unit 07's "Left for S6" items map as follows:
1. flat draw: T3, T4
2. reverse patch sees the original IL: T5, plus grew lines at the maxima
3. merge gives vanilla placements at the defaults: T5
4. no template holes: the T4a gaps check
5. `Options.Load` re-reads the config: T4b
6. probe side effects: covered indirectly by T5
7. log line formats: parsed by the capture

## Coverage evidence

- **Changed decision points:**
  - `Groups.ForWorld`, each table pair and unknown → `AsteroidKinds.EveryWorldInTheTable`, `PairsOutsideTheTable`
  - `Groups.Thickness`: t ≤ 0 → `PairsOutsideTheTable`; unknown → `PairsOutsideTheTable`; group value → `PairsOutsideTheTable`, `DefaultsAreTheGamesThinnest`
  - `Group.Clamp` / `Options.Load` above, below and in range → `ConfigLimitedOnRead`
  - `Options.CreateOptions` (labels, ranges, tooltips, limits, order) → `Dialog.*`
  - `TopLeftTextOptionsEntry.CreateUIEntry` → `Dialog.IntroTopLeft`
  - Transpiler `Patched` true path and the `Border.Setting` flat branch → `ExpectFlat` (T3, T4a, T4b)
  - Prefix probe of the 4 vanilla rectangles → T5 (identity) and `RectanglesGrowAtMaxima`
  - Postfix merge, both branches: tie keeps vanilla → T5 (`*Same`, no grew lines); flat deeper → `RectanglesGrowAtMaxima`
  - `Options.Load` per world → T4b after T4a
  - Mod off (unpatched) → `*Vanilla` controls
- **Intentionally untested branches:**
  - Transpiler IL-mismatch fallback, and the prefix's "added != 4" / exception paths: these need a changed game method. Unit 07 covered them offline.
  - `Probe` with T ≤ 0 (TinyMagma, TheRock): no cluster in the game uses them. The rule is covered at the helper by `PairsOutsideTheTable`.

## Build output (verbatim tails)

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none -nologo --no-incremental`:
```
  SmoothBorders -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders\bin\Release\SmoothBorders.dll
  SmoothBorders.Tests -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders.Tests\bin\Release\SmoothBorders.Tests.dll
    291 Warning(s)
    0 Error(s)
Time Elapsed 00:00:04.93
```
`grep smooth-borders | grep -c "warning|error"` on that log gives 0. All 291 warnings belong to other projects, as in units 04 and 05. The first build of the tests had one CS0618 (`FindObjectsOfType`); it was replaced with `FindObjectsByType`.

`pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -BuildOnly -Label buildonly`:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Checking SmoothBorders.Tests.dll against the game assemblies
Built and checked against the game assemblies (-BuildOnly: no game session).
```
**Note:** `-BuildOnly` builds the test project with the default deploy, so it **deployed `SmoothBorders` into `Documents\Klei\OxygenNotIncluded\mods\local\SmoothBorders`** for the first time while the player's game was running. TestRunner was redeployed unchanged. The running game does not read that folder until it restarts. At its next start it will list Smooth Borders as a new local mod, disabled. Phase 2 needs it deployed anyway.

**Test list.** `-BuildOnly` does not list tests. A scratch console (`scratchpad\listtests\`, not committed) reflects over the built DLL with Discover's rules:
- order: class full name ordinal, then metadata token
- Explicit tests left out when there is no filter
- the Prepare shape check, and the world key per test

Output (one line per test, names omitted):
```
SmoothBorders.Tests/AsteroidKinds.EveryWorldInTheTable [NoColony]
SmoothBorders.Tests/AsteroidKinds.DefaultsAreTheGamesThinnest [NoColony]
SmoothBorders.Tests/AsteroidKinds.PairsOutsideTheTable [NoColony]
SmoothBorders.Tests/AsteroidKinds.ConfigLimitedOnRead [NoColony]
SmoothBorders.Tests/Dialog.LabelsAndTooltips [NoColony] [Timeout 30]
SmoothBorders.Tests/Dialog.IntroTopLeft [NoColony] [Timeout 30]
SmoothBorders.Tests/Dialog.SlidersAndDefaults [NoColony] [Timeout 30]
SmoothBorders.Tests/Flatness.AtMaxima [Timeout 90]
SmoothBorders.Tests/Flatness.RectanglesGrowAtMaxima [Timeout 30]
SmoothBorders.Tests/Flatness.AtMixedValues [Timeout 90]
SmoothBorders.Tests/Identity.TerraniaVanilla [Timeout 90]
SmoothBorders.Tests/Identity.TerraniaVanillaAgain [Explicit] [Timeout 90]
SmoothBorders.Tests/Identity.TerraniaFlat [Timeout 90]
SmoothBorders.Tests/Identity.TerraniaSame [Timeout 60]
SmoothBorders.Tests/Identity.ClassicTerraVanilla [Timeout 90]
SmoothBorders.Tests/Identity.ClassicTerraFlat [Timeout 90]
SmoothBorders.Tests/Identity.ClassicTerraSame [Timeout 60]
SmoothBorders.Tests/Identity.BaseTerraVanilla [Timeout 90]
SmoothBorders.Tests/Identity.BaseTerraFlat [Timeout 90]
SmoothBorders.Tests/Identity.BaseTerraSame [Timeout 60]
SmoothBorders.Tests/Identity.Terrania200Vanilla [Explicit] [Timeout 90]
SmoothBorders.Tests/Identity.Terrania200Same [Explicit] [Timeout 60]
SmoothBorders.Tests/Identity.Terrania300Vanilla [Explicit] [Timeout 90]
SmoothBorders.Tests/Identity.Terrania300Same [Explicit] [Timeout 60]

19 of 24 test(s) selected without a filter; 8 generated world(s) for them (base-game coordinates are skipped in Spaced Out)
```
World keys, in run order:
1. `SNDST-C-100|Flatness.Maxima`
2. `SNDST-C-100|Flatness.Mixed`
3. `SNDST-C-100|Identity.Vanilla`
4. `SNDST-C-100|Identity.Defaults`
5. `V-SNDST-C-100|Identity.Vanilla`
6. `V-SNDST-C-100|Identity.Defaults`
7. `SNDST-A-100|Identity.Vanilla` (skipped in Spaced Out)
8. `SNDST-A-100|Identity.Defaults` (skipped in Spaced Out)

No signature or Prepare errors were reported.

## Things found while writing (for the manager)

1. **Plan risk (b) is answered by reading the code: PLib does not cache settings.** `POptions.ReadSettings<T>` opens and deserialises the file on every call (decompiled PLib 4.25.0, `POptions.ReadSettings(string, Type)`). T4b still proves it in the game.
2. **Possible mod bug, which `Dialog.IntroTopLeft` will settle.** PLib's `PLabel` always gives its text child `TextAlignmentOptions.Center` (`PTextComponent.TextChildHelper`: `alignment = 514`). `TextAnchor.UpperLeft` on the PLabel only anchors that child to the left and top of its cell. If the text child is as wide as the dialog, wrapped lines would be centred inside it: the block would sit top-left, but each line would be centred. The test measures where each line actually starts. If it fails in phase 2, the fix is in `TopLeftTextOptionsEntry`: set the LocText's alignment to TopLeft in an OnRealize handler. That is a small change to the mod; I'll leave it to you unless it is that obvious.
3. **The Dialog tests run inside the fixture colony in a full run.** NoColony tests run at the main menu only when every selected test is NoColony. In a mixed run they run in the colony, after the one fixture load. `POptions.ShowDialog` in a colony has no precedent in this repo; Natural Backwalls only ever ran it at the main menu. If it misbehaves there, run S5 alone: `-Filter '*AsteroidKinds*;*Dialog*'`.
4. **Thresholds that phase 2 may need to tune.**
   - **Deep allowance:** `max(2, lines/50)` per edge.
   - **Gap excuse:** limited to Space zone cells. The first run will show whether natural neutronium, or a template with `overridePlacement`, trips either check. Each failure is photographed.
   - **Control assertion:** the Vanilla tests require at least one edge over the deep allowance, which makes sure the measure is not blind.

## Phase 2 results

### 1. Harness check owed by unit 05: passed

**Command:** `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods NaturalBackwalls -Filter '*Generated*' -Label nb-harness`

**Run id:** `nb-harness-20261019-121323`

**Runner lines (verbatim):**
```
PASS     NaturalBackwalls.Tests/Generated.StartingAsteroid (0.0 s)
PASS     NaturalBackwalls.Tests/Generated.OtherPlanetoids (0.0 s)
PASS     NaturalBackwalls.Tests/Generated.Overview (2.3 s)
Session done: 3 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 3 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 3; 41.4 s real time
Game live after 62 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\nb-harness-20261019-121323
```

**Player.log:**
```
[16:13:58.554] [1] [INFO] [TestRunner] backed up the config of NaturalBackwalls
[16:14:30.908] [1] [INFO] [TestRunner] put back the config of NaturalBackwalls
```

**warden.log:** `12:14:47 game exited` / `restored mods.json` / `live session closed`.

**SHA1 of `mods\config\NaturalBackwalls\config.json`:** before `8151c4e6f20b60cfc69236884ac7c2608b7dc066`, after `8151c4e6f20b60cfc69236884ac7c2608b7dc066`. Unchanged.

**Small harness leftover:** after the restore, `%LOCALAPPDATA%\IsochronousOniTests\config-backup\` remains as an empty folder. It holds no backup, so nothing is restored by mistake later.

### 2. SmoothBorders suite

**Final standing:** 14 pass, 2 fail, 3 skipped (the base-game coordinates, in Spaced Out). Both failures are mod defects, with the evidence in section 3. All worldgen tests pass. `mods\config\SmoothBorders` was absent before every run and absent after each one.

**Every test fix, committed by path on OM-007-smooth-borders:**
- `2362df7` Dialog tests:
  - The dialog is found as PLib's "ModOptions" screen. The intro's input field carries a KScreen of its own, so the nearest KScreen gave 0 rows.
  - The input field's trailing zero-width space is ignored in the text compare.
  - A new check: the whole intro fits its area.
- `4d99e15` Support:
  - The mod's log lines are read from Player.log, between offsets taken at Prepare and at undo. The game's `Debug.Log` is `Console.WriteLine` (decompiled `Debug.WriteTimeStamped`), so `Application.logMessageReceivedThreaded` never sees it.
  - Neutronium at depth F inside a MagmaCore zone is counted apart, with an allowance of lines/10. Elsewhere the allowance stays at lines/50.
- `d49434f` Dialog: the intro must be left-aligned (every line on one left edge, within 4 px) and indented 12 px or less from the rows. This allows e3af936's padding.

**Runs, in order:**

`sb-s5-20261019-121630`, S5 alone (`-Filter '*AsteroidKinds*;*Dialog*'`), before the fixes:
```
Session done: 4 passed, 3 failed, 0 errors, 0 timeouts, 0 skipped; 4 passed, 3 failed, 0 errors, 0 timeouts, 0 skipped of 7; 18.9 s real time
```
All 4 AsteroidKinds pass. The world table holds 62 loaded worlds with a border, in the six pairs `(2,1)` x3, `(3,1)` x6, `(4,1)` x2, `(4,2)` x8, `(5,2)` x20, `(10,6)` x23.

`sb-dialog-20261019-121911`, Dialog on c55d5cb plus fix 2362df7:
```
Session done: 1 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped; 1 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped of 3; 13.9 s real time
```

`sb-full-20261019-122006`, the first full run (`-TimeoutSec 1800`):
```
Session done: 7 passed, 9 failed, 0 errors, 0 timeouts, 3 skipped; 7 passed, 9 failed, 0 errors, 0 timeouts, 3 skipped of 19; 192.9 s real time; fixture loaded again 7 time(s)
```
- The worldgen failures were "the mod logged no flat wall": the log-capture bug, fixed in 4d99e15.
- Every wall still measured flat.
- One more failure: TerraMoonlet's bottom at the defaults had neutronium at depth 4 on 9 of 152 lines. It is the magma bed's own neutronium meeting the wall (screenshot `sb-full-20261019-122006\artifacts\SmoothBorders.Tests.Identity.TerraniaFlat-TerraMoonlet-bottom-deep.png`). Run 2 confirms all 9 are in MagmaCore.

`sb-full2-20261019-122424`, the second full run, on HEAD 4d99e15 with e3af936 included:
```
PASS     SmoothBorders.Tests/AsteroidKinds.EveryWorldInTheTable (0.0 s)
PASS     SmoothBorders.Tests/AsteroidKinds.DefaultsAreTheGamesThinnest (0.0 s)
PASS     SmoothBorders.Tests/AsteroidKinds.PairsOutsideTheTable (0.0 s)
PASS     SmoothBorders.Tests/AsteroidKinds.ConfigLimitedOnRead (0.0 s)
PASS     SmoothBorders.Tests/Dialog.LabelsAndTooltips (1.6 s)
FAIL     SmoothBorders.Tests/Dialog.IntroTopLeft (1.6 s): every line of the intro starts where the rows' labels start: line 1 starts 6 px from the rows' left edge; ... (test fixed by d49434f)
FAIL     SmoothBorders.Tests/Dialog.SlidersAndDefaults (1.5 s): row 1's field shows the default (expected 4, got 0)
PASS     SmoothBorders.Tests/Flatness.AtMaxima (0.0 s)
PASS     SmoothBorders.Tests/Flatness.RectanglesGrowAtMaxima (0.0 s)
PASS     SmoothBorders.Tests/Flatness.AtMixedValues (0.0 s)
PASS     SmoothBorders.Tests/Identity.TerraniaVanilla (0.0 s)
PASS     SmoothBorders.Tests/Identity.TerraniaFlat (0.0 s)
PASS     SmoothBorders.Tests/Identity.TerraniaSame (0.0 s)
PASS     SmoothBorders.Tests/Identity.ClassicTerraVanilla (0.0 s)
PASS     SmoothBorders.Tests/Identity.ClassicTerraFlat (0.0 s)
PASS     SmoothBorders.Tests/Identity.ClassicTerraSame (0.0 s)
SKIPPED  SmoothBorders.Tests/Identity.BaseTerraVanilla (0.0 s): SNDST-A-100-0-0-0 is a base game cluster and the game is in Spaced Out mode
SKIPPED  SmoothBorders.Tests/Identity.BaseTerraFlat (0.0 s): SNDST-A-100-0-0-0 is a base game cluster and the game is in Spaced Out mode
SKIPPED  SmoothBorders.Tests/Identity.BaseTerraSame (0.0 s): SNDST-A-100-0-0-0 is a base game cluster and the game is in Spaced Out mode
Session done: 14 passed, 2 failed, 0 errors, 0 timeouts, 3 skipped; 14 passed, 2 failed, 0 errors, 0 timeouts, 3 skipped of 19; 192.9 s real time; fixture loaded again 7 time(s)
Game live after 210 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-full2-20261019-122424
```
Player.log: `backed up the config of SmoothBorders (none yet)`, then `put back the config of SmoothBorders (none, as before the run)`.

`sb-dialog2-20261019-122821`, Dialog on HEAD d49434f (mod at e3af936):
```
PASS     SmoothBorders.Tests/Dialog.LabelsAndTooltips (1.6 s)
FAIL     SmoothBorders.Tests/Dialog.IntroTopLeft (1.5 s): the whole intro fits its area (last line 19 px below the bottom) (expected true)
FAIL     SmoothBorders.Tests/Dialog.SlidersAndDefaults (1.5 s): row 1's field shows the default (expected 4, got 0)
Session done: 1 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped; 1 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped of 3; 13.5 s real time
```
The intro is now left-aligned: 4 lines on one left edge, 6 px right of the row labels (the padding).

**What the worldgen results show (run 2):**
- **Flat at every setting tested:**
  - **Coverage:** defaults, maxima (16/7/6/3) and mixed (7/1/5/2), on all 9 Terrania asteroids and all 8 classic Terra asteroids. Every kind came up in both clusters.
  - **Thickness:** for every asteroid the mod logged the thickness the config gives.
  - **Gaps:** 0 gaps on every edge, so no template holes, even at 16.
  - **Depth F:** no neutronium at depth F outside magma. Inside magma the most is 9 of 152 (TerraMoonlet bottom at the defaults).
  - **Control:** the game's own wall on the same seeds is deeper than the default on most edges. TerraMoonlet's left edge has 106 of 266 outside magma plus 14 in magma; its bottom has 60 of 152 in magma. The measure can tell the two apart.
- **POI rectangles at the maxima:** 9 grew lines. TerraMoonlet's left grew 11->16, right 7->16, bottom 10->16; six moonlet edges grew by 1. At the defaults no rectangle grew, in either cluster.
- **Identity at the defaults:**
  - Terrania: 1759 buildings and geysers, 0 missing, 0 extra.
  - Classic Terra: 2327, 0 missing, 0 extra.
  - Both "Flat" tests ran in the world generated right after a Vanilla world. That shows the undo re-applied the patches, and that the reverse patch drew the vanilla rectangles.
- **Options re-read:** AtMixedValues came after AtMaxima in one session and logged the new thicknesses, so the options are read again for each world.
- **Not run:** the Explicit tests (VanillaAgain, seeds 200 and 300).

### 3. Mod defects found (evidence; not fixed)

**A. The options dialog neither shows nor saves the four settings. Severity: high.**

- **What the player sees:** with no config file, the four number fields read `0, 0, 0, 0` and the sliders stand at their minimum: `1-16 at 1, 1-7 at 1, 1-6 at 1, 1-3 at 1` (SlidersAndDefaults notes, every run; screenshot below).
- **Cause, in PLib 4.25.0's `OptionsDialog` (decompiled):**
  - `FillModOptions` adds the entries from `IOptions.CreateOptions()` only to a local copy of the categories.
  - `UpdateOptions()` (`item.ReadFrom(options)`) and `WriteOptions()` (`item.WriteTo(options)`) loop over `optionCategories`, which holds only the entries built from `[Option]` attributes.
  - So a value entry yielded from `CreateOptions` is never filled from the config, and is never written back on OK.
  - Natural Backwalls avoids this: its value options are `[Option]` properties, and its dynamic entries hold the options object themselves.
- **Effect:** the player sees 0s. A change made in the dialog is lost on OK: the file gets the unchanged object that was read. Worldgen itself is unaffected; it reads the file.
- **Why I did not fix it:** it is not a one-line change. It is the choice made in S3, "No static `[Option]` properties (keeps the yield order)". There are two ways out:
  1. Make the four ints `[Option]` properties with `[Limit]`. Then the intro text block, which stays dynamic, would come after them.
  2. Use an `IntOptionsEntry` subclass bound to the options object. It reads the value in its constructor (`ReadFrom(owner)`) and writes on every change by overriding the protected `Update()` and calling `WriteTo(owner)`. `CreateOptions` is called on the dialog's own options object, so `this` is the one PLib saves. "Reset to default" would still not refresh these rows: PLib's `OnResetConfig` swaps in a new object and calls the same `UpdateOptions`.

  This is the manager's call. `Dialog.SlidersAndDefaults` is the test for it.

**B. The 3-line intro area cuts off the last line. Severity: low, visual.**

At the current dialog width, the new shorter intro wraps to 4 lines, and the area shows 3. "Changes apply to newly generated worlds." is hidden. The test measures the last line ending 19 px below an area 57 px high. `LineCount = 4` would fit it at this width.

The white border is gone and the padding is in (e3af936). There is no large gap.

The `Dialog.IntroTopLeft` assertion is "the whole intro fits its area". `TopLeftTextOptionsEntry.cs` was left for the manager.

**Dialog screenshot for the user (current build, e3af936):**
`C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-dialog2-20261019-122821\artifacts\SmoothBorders.Tests.Dialog.IntroTopLeft-dialog.png`

Earlier states, for comparison:
- `sb-s5-20261019-121630` (6-line area, long text)
- `sb-dialog-20261019-121911` (3-line area, with border)

### 4. Config and leftovers

- **SmoothBorders config:** `mods\config\SmoothBorders` is absent now, as it was before the runs.
- **Natural Backwalls config:** SHA1 still `8151c4e6f20b60cfc69236884ac7c2608b7dc066`.
- **Warden:** its last lines are `game exited`, `restored mods.json`, `live session closed`. No game is running.
- **Leftover:** the empty `%LOCALAPPDATA%\IsochronousOniTests\config-backup\` folder is still there.

## Phase 2 commands (when the game is free)

1. Harness check owed by unit 05:
   - `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods NaturalBackwalls -Filter '*Generated*'`
   - Compare `mods\config\NaturalBackwalls\config.json` against SHA1 `8151C4E6F20B60CFC69236884AC7C2608B7DC066`.
2. S5 alone, no save loaded: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -Filter '*AsteroidKinds*;*Dialog*'`. Hash `mods\config\SmoothBorders` before and after: it is absent now, so it must still be absent after.
3. The full run: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -TimeoutSec 1800`. That is 6 worlds plus one fixture load; 1800 s leaves room above the plan's 1200.
4. Optional:
   - `-Filter '*Identity.TerraniaVanilla*;*Identity.TerraniaVanillaAgain*'` for the generation self-check
   - `-Filter '*Identity.Terrania200*;*Identity.Terrania300*'` for the extra seeds

## Report back on the packet

- **Did not apply:**
  - The common brief's `blk-3ca89d57dfde` (ModCheck overview) and `blk-647210c6a78a` / `blk-6c2bbf052519` (tools overview).
  - The TestRunner entity, naming and error-handling records. I worked from the TestRunner source (`Kit/Attributes.cs`, `Session.cs`, `Worldgen.cs`, `TestBehaviour.cs`) and unit 05's report, which carry the post-ef0115e facts. The records predate the Prepare hook.
- **Missing:** The main store's brief matched nothing for `mods/smooth-borders`, yet the testing records govern this work:
  - `blk-87272da04dee` (generated worlds)
  - `blk-c8a18ea4088d` (per-test options)
  - `blk-7316ec9fd58f` (naming: prefix, `control:`)
  - `blk-33fb80177cba` (waiting and pacing)
  - `blk-bac04f55a861` (screenshots: lift the fog with `Grid.Reveal`)
  - `blk-0194e02d0fd6`, `blk-f054250ed484` (assertions)

  They are reachable only by `--tag generated-worlds` / `--tag test-pacing`. The PLib facts in "Things found" (no settings cache, a PLabel's text always centred) are in no record.
- **Mismatched reasons:** none.

## ICON tooling friction

None. `ks read --store C:/dev/ONI-Mods blk-c8a18ea4088d blk-87272da04dee` and the two `ks brief --store C:/dev/ONI-Mods --tag generated-worlds` / `--tag test-pacing` calls each worked on the first try.

## Other friction (not ICON)

- **Decompiling.** ilspycmd 9.1 (it warns that 11.1 exists) decompiled `ToolTip`, `WorldGenSpawner` and the PLib types without trouble. The scratchpad's `decomp\acs` folder from units 01, 02 and 07 was still there and saved a full decompile.
- **Line endings.** A `sed -i` rename in Git Bash left `AsteroidKinds.cs` with LF line endings. I converted it back with `unix2dos` before committing (git normalises it anyway).
