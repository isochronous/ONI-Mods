# Unit 08 report: S5 and S6 tests (phase 1: written, built, committed; not run)

**Status:** Phase 1 is done. The tests are committed as **`6944e9b`** "OM-007: Smooth Borders tests" on OM-007-smooth-borders, with both trailers. Only the five test files were staged, by path. Nothing was run in the game: the player's game was up throughout, and I neither launched it nor closed it. Phase 2 (running) is still to do; the commands are at the end.

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
