# Unit 12 report: Smooth Borders with five Workshop worldgen mods

## Status

Done. Every run is listed in section 4.

**Goal 1, never crash:** Smooth Borders did not crash and logged no exception or warning in any run. The runs covered:
- each mod alone with Smooth Borders, in both modes where the mod has clusters
- all five mods enabled together

Every generated colony loaded and played at normal speed, and every asteroid was shown. The exceptions that do appear in Player.log are listed in 5.4; none of them comes from Smooth Borders.

**Goal 2, stand down:** none of the five mods needs a detection rule.
- **MiniBase:** builds its own asteroid, and the game's `DrawWorldBorder` never runs for it. Smooth Borders therefore never touches it: it is the same cell for cell, checked in both modes and at the maxima.
- **Voidria:** its asteroids have vacuum edges, which the game leaves unwalled. Smooth Borders keeps the game's own skip, so those edges stay unwalled too: 0 cells added or removed.
- **Empty Worlds, Baator and Omnificia:** they keep the game's rectangular border, and Smooth Borders flattens it as designed.

**One defect found (not fixed):** with Empty Worlds installed, the POI rectangles at the defaults can differ by 1–2 tiles from the world without Smooth Borders, so templates land elsewhere. The cause, the evidence and a small proposed fix are in 3.2.

**Goal 3:** section 6 describes what supporting the mods' own asteroid kinds in the settings would take. Nothing was built for it.

**Manager's requests during the unit:**
- **Fog and reveal:** both are handled (section 2.2).
- **The black camera:** it was fog of war on undiscovered asteroids; nothing was wrong with the worlds. The answer and the screenshots are in 5.1.
- **Representative sample:** after the user's note I stopped re-running full sets. The final runs are one pass per mod, not a second full sweep.

**Commits, by path:**
- **`common`**, branch OM-007-smooth-borders: `ba6ee41` "OM-007: TestRunner runs Workshop clusters as a player would: mod prefixes, unregistered Workshop items, destination screen with its category, and a test-only asteroid reveal". Files: `tools/TestRunner/Worldgen.cs`, `Reflect.cs`, `Invoke-OniTests.ps1`, `README.md`, `Kit/World.cs`. **Manager: bump the `common` pointer.** I did not stage it.
- **ONI-Mods**, branch OM-007-smooth-borders: `3138441` "OM-007: Compatibility tests with five Workshop worldgen mods; photos reveal the asteroid first". Files: `mods/smooth-borders/src/SmoothBorders.Tests/Compatibility.cs` (new) and `Support.cs`.
- No mod code was changed.

## 1. The five mods: how each builds its edges

The decompiled DLLs are in `scratchpad\compat\dec-<id>\` (ilspycmd). The world data comes from each mod's `worldgen/` and `dlc/expansion1/worldgen/` folders.

Background, from the game's `WorldGen.cs` at lines 943 and 1354:
- The game reads only three keys: `DrawWorldBorder`, `DrawWorldBorderForce` and `WorldBorderThickness`/`WorldBorderRange`.
- `DrawWorldBorderTop`, `DrawWorldBorderBottom` and `DrawWorldBorderOverVacuum` are never read by `DrawWorldBorder`. `DrawWorldBorderOverVacuum` is read only by `WorldContainer.cs:803`, for "fully enclosed".

None of the five mods replaces or transpiles `DrawWorldBorder`. In every run:
- The transpiler matched: walls were drawn flat and logged.
- The fail-safe line "DrawWorldBorder has changed" never appeared.

| Mod | Patches that touch worldgen | Worlds (T, R) → Smooth Borders kind, F at defaults / maxima |
|---|---|---|
| **MiniBaseFE** 3382061617 | `WorldGen.RenderOffline` prefix returns false for `MiniBase`, `BabyOilyMoonlet`, `BabyMarshyMoonlet` and `BabyNiobiumMoonlet`; its postfix builds the world itself (`MiniBaseWorldGen.CreateWorld`). That code draws its own walls (`DrawCustomWorldBorders`: neutronium, abyssalite corners, a katairite roof) and calls `DoSettleSim` itself. It never calls `DrawWorldBorder`. It also patches `ColonyDestinationSelectScreen.OnSpawn`, where it sizes its asteroids and picks the moonlets from its options. | MiniBase (1, 0), built by the mod, **never reached**. The natural Baby moonlets are (3, 2), own pair, F 1/1, and go through the game's worldgen. They are off by default in MiniBase's options, and its config lives in its Workshop folder, which is outside what the harness restores, so they were not generated. |
| **Empty Worlds** 1825146346 | A void `DrawWorldBorder` **prefix**, on every world of every cluster. It draws a bottom band only when `DrawWorldBorderForce` is true, but it calls `rnd.RandomRange(-2, 2)` once per column either way (section 3.2). It also has a `TemplateCache.GetTemplate` postfix (bunker removal option, off) and Db/Localization patches. | Emptera / EmpteraDLC and Islands / IslandsDLC are (6, 4), own pair, F 2/2. Their YAML sets the legacy `DrawWorldBorderBottom` key, which the game ignores. The game's rectangular wall is drawn. The vanilla moonlets in its clusters are normal. |
| **Voidria** 3430682737 | A `MutatedWorldData` postfix (template rules), plus `ColonyDestinationSelectScreen.OnSpawn`, `RegisterTemporalTear` and `Db.Initialize`. Nothing touches borders. | Voidria (base game) is (10, 6), Primary, 4/16. VoidriaSO and VoidriaMini are (5, 2), Small, 3/7. TinyWarpSurface and TinyLandingZone are (1, 0), TinyMoonlets, 1/3. The edges are vacuum in terrain tagged RemoveWorldBorderOverVacuum (`voa_SpaceWithBg`, `SpaceNoBorder`), so the game draws almost no wall. |
| **Baator** 2582905897 | `Enum.ToString`/`Parse` for its temperature ranges, element loading, `Assets.OnPrefabInit`, meteor traits. Nothing touches borders. | Baator (256x416 primary) and Baator_moonlet are (4, 2), **Moonlets**, 2/6. Baator_coldterra is (2, 2), own pair, F max(1, 0) = 1. Baator_oilyswampy is (6, 2), own pair, 4/4. The vanilla worlds in its clusters are normal. |
| **Omnificia Asteroid II** 3743010929 | Seasons, colony-count achievements, the world cache, Localization, the impactor visualizer, `Assets`. Nothing touches borders. | All six worlds (352x448) have no border override, so they use the game's defaults (10, 6): **Primary**, 4/16. |

## 2. Harness and test changes

### 2.1 What the harness lacked (fixed in `common` ba6ee41)

**1. Workshop coordinate prefixes**
- **Problem:** `Worldgen.ModeMismatch` decided the mode by the letter before the seed (`A` or `C`). `MINI-B`, `EMP-PHO`, `BTOR-L`, `VOA-TG-M` and `V-MINI` were all taken for base-game clusters and skipped in Spaced Out.
- **Fix:** a prefix without the letter is now looked up in the clusters the game loaded for its mode. If the mode has no cluster with that prefix, the test is skipped with that reason.

**2. Workshop items not yet in mods.json**
- **Problem:** four of the five mods are downloaded but have never been registered (the player has not started the game since subscribing). The launcher threw "not installed".
- **Fix:** the launcher now registers such an item for the run:
  - `distribution_platform` is set to 1
  - title and staticID come from its mod.yaml
  - `label.version` is the item's `timeupdated` from Steam's `appworkshop_457140.acf`. This matches what the game stored for MiniBase: 1781352193.
- The player's mods.json is restored as before: SHA1 `4779bb7d…` before and after.

**3. The destination screen**
- **Problem:** MiniBase sets its cluster up in `ColonyDestinationSelectScreen.OnSpawn`. The harness never opened that screen, so MiniBase's worldgen failed with "Index was outside the bounds of the array", even with Smooth Borders' patches off (runs sb-compat-minibase-…150336 and …150646).
- **Fix:** the harness now opens and spawns that screen from the main menu's New Game flow before applying the coordinate, then closes it.
  - It first sets `DestinationSelectPanel.ChosenClusterCategorySetting` to the cluster's category, as the category screen does. Without that, Empty Worlds' "Special" clusters were swapped for Terra in base game (run sb-compat-empty-base-…154759 generated `SNDST-A`). The screen's `GetDefaultAsteroid` also threw "Sequence contains no elements" in the earlier runs.
  - On close it takes the screen's three handlers off `CustomGameSettings` at once. The mixing handler is one the game itself never removes.
  - The harness now fails the generation if the game took another coordinate than asked.
- **Check on vanilla worlds afterwards:** the default suite (17 passed) and the base-game BaseTerra + Skewed set (7 passed) both pass.

**4. Reveal, for the manager's request**
- `t.World.RevealAsteroid(world)` and `t.World.RevealActiveAsteroid()` run `Grid.Reveal` on every cell of the asteroid, as the game's debug reveal does (`Game.cs` ~1834). They are test-only and documented in the README.

### 2.2 Which tests reveal now, and how

- **`Support.Photograph`** (used by every flatness test before it fails, in Identity, Clusters, Flatness and Compatibility): it now reveals the whole asteroid and switches to it with the game's own `CameraController.ActiveWorldStarWipe` before `LookAt` and the screenshot. Before, it revealed only a 41x25 patch and switched with `ClusterManager.SetActiveWorld`.
- **`Compatibility.*Plays`:** for each asteroid it reveals the whole asteroid, switches the game's way, logs where the camera points and asserts that the point is inside the asteroid, then screenshots it and lets two game seconds pass.
- **`Compatibility.EmptyEmpteraCamera`:** the diagnosis in 5.1.
- **Cell checks** (flatness, placements, neutronium, solids) read `Grid` data, which fog does not affect. They do not reveal.

### 2.3 `Compatibility.cs`

All tests are Explicit, one filter per mod: `-Filter '*Compatibility.MiniBase*'` with `-Steam 3382061617`, and so on (the class summary lists them all). Each cluster has five tests:

| Test | What it does |
|---|---|
| **Alone** | Generates with Smooth Borders' border patches off, so the worldgen mod works alone. It records: placements, inner neutronium, the solid cells of the mod's own asteroids, the four POI rectangles of each asteroid (a test-only last postfix on DrawWorldBorder), and the neutronium near every edge. |
| **Flat** | At the defaults, every asteroid the game walls is flat. Smooth Borders logged nothing about the mod's own asteroids, and their solid cells equal Alone's. It also logs the order of the prefixes on `DrawWorldBorder`. |
| **Same** | No POI rectangle grew. Rectangles are compared (logged), placements are compared exactly, and inner neutronium is compared exactly. For MiniBase only, the geysers are logged rather than compared. MiniBase picks its random useful geyser with `Util.GetRandom` (UnityEngine.Random, unseeded), so it differs even with Smooth Borders off. Control: `MiniBaseAloneAgain`. |
| **Plays** | As in 2.2. Any error the game logs fails the test. |
| **AtMaxima** | Flat at every kind's maximum, and the mod's own asteroids untouched. Placements and inner neutronium are logged. |

Further tests:
- `EmptyEmpteraAloneAgain` and `EmptyIslandsAloneAgain`: controls showing Empty Worlds is deterministic on its own.
- `Omnificia{Beach,Forest,Garden,IceCaves,Swamp}{Flat,AtMaxima}`: generation succeeds and every wall is flat.
- `Together*`: one cluster per mod with all five mods enabled.

`Support.ExpectFlat` gained an overload that takes the mod-built asteroids to leave unmeasured.

I covered "generation succeeds" with these tests rather than a WorldgenDriver sweep. The driver has the same two gaps that items 1–3 in 2.1 fixed in the harness: it would register Workshop ids as local mods, and it does not open the destination screen.

## 3. Findings per mod

### 3.1 MiniBase: stands down by itself

**Spaced Out (`MINI-B-100`)** and **base game (`V-MINI-100`):** one asteroid, MiniBase 112x57 (MiniBase sizes it), at its defaults. Results:
- Smooth Borders logged nothing for it.
- 3249/3249 solid cells are equal to MiniBase alone, at the defaults and at the maxima.
- Buildings 12 = 12. Inner neutronium 984 = 984.
- `MiniBaseAloneAgain`, with the patches off both times: buildings are equal, and the geyser at (87, 23) was `hot_hydrogen` one time and `hot_steam` the other. That is MiniBase's own random.

**Stand-down mechanism:** MiniBase's `RenderOffline` prefix skips the game's worldgen, so `DrawWorldBorder`, and with it Smooth Borders, never runs for these asteroids.

**Not covered:** the optional natural moonlets. They would go through the game's `DrawWorldBorder` at (3, 2) and be flattened to 1, like any vanilla-walled world.

### 3.2 Empty Worlds: flat as designed; one identity defect

**At the defaults:** every asteroid is flat. EmpteraDLC and IslandsDLC (6, 4) get 2 tiles.

**`Same` fails on EMP-PHO-100 and ISD-PHO-100 in Spaced Out, and on ISD-PHO-100 in base game:**
- EMP-PHO, Spaced Out: 2024 buildings and geysers against 1941.
- ISD-PHO, base game: 763 against 800.
- Base-game EMP-PHO passed, with one bottom rectangle 7 instead of 8.

**Cause, measured with the rectangle probe:**
- EMP-PHO-100: EmpteraDLC's right rectangle is `165,0 5x240` against `164,0 6x240` alone, and its bottom is `170x7` against `170x8`. IslandsDLC and MooMoonlet differ in the same way.
- The other six asteroids have equal rectangles and equal placements.
- Empty Worlds alone is deterministic: `AloneAgain` passes, and the rectangles are equal on every asteroid in both modes.

**Mechanism:**
- Empty Worlds' prefix runs first (it logged as index 0, Smooth Borders as index 1, both priority 400). It draws `world.size.x` numbers from the game's `rnd` before the game's walk.
- Smooth Borders' probe walks with a fresh `new SeededRandom(0)`. It therefore computes the rectangles of the game without Empty Worlds, not those of the world Empty Worlds actually makes.
- Smooth Borders' own wall is flat (R = 0), so its flatness is unaffected. Only the "same placements as without Smooth Borders" promise breaks. With Empty Worlds installed, this affects every cluster the player generates, not only Empty Worlds' own.
- Nothing crashes. The border is correct.

**Proposed fix (not implemented; small, but it changes the identity behaviour, so it is the manager's and user's call):**
- The prefix already receives the game's `rnd`. Probe with a copy of it instead of `new SeededRandom(0)`: a new `SeededRandom(rnd.seed)` whose `KRandom` gets `inext`, `inextp` and a clone of `SeedArray` copied over by reflection (`fp/KRandom.cs`).
- Give Smooth Borders' prefix `[HarmonyPriority(Priority.Last)]`, so every other prefix has drawn from `rnd` first, whatever the load order.
- Test: `EmptyEmpteraSame` and `EmptyIslandsSame` should then pass, and the rectangle probe should log no difference.

### 3.3 Voidria: the game draws (almost) no wall, and Smooth Borders keeps it that way

Edge neutronium within the strip:

| Asteroid | Defaults: alone → Smooth Borders | Maxima: alone → Smooth Borders |
|---|---|---|
| VoidriaSO | 0 → 0 | 0 → 0 |
| VoidriaMini | 0 → 0 | 0 → 0 |
| Voidria (base game) | 0 → 0 | 0 → 0 (Smooth Borders logs "flat wall 16 tiles", but every cell it would set is skipped vacuum) |
| TinyWarpSurface | 25 → 25 | 25 → 75 (+50) |
| TinyLandingZone | 65 → 65 | 65 → 185 (+120) |

- At the maxima, Smooth Borders thickens the small wall pieces the game drew on non-vacuum lines from 1 to 3, which is the setting doing what it says.
- POI rectangles, placements and inner neutronium are equal to Voidria alone at the defaults.
- Stand-down mechanism: `DrawWorldBorder`'s own vacuum skip, which the transpiler leaves in place. No rule is needed.

### 3.4 Baator: flat as designed

- **Results:** BTOR-L-100 has 11 asteroids and BTOR-S-100 has 11. Every asteroid is flat at the defaults and the maxima. Rectangles are equal, buildings are 3089 = 3089 and 2519 = 2519, and inner neutronium is equal at the defaults.
- **Note for the settings:** Baator's 256x416 primary is (4, 2), so its wall follows the **Moonlets** slider, not Primary.
- **Note on coldterra:** Baator_coldterra (2, 2) gets F = 1. The game's wall there is 0–4 thick, and 0 on some lines.

### 3.5 Omnificia: flat as designed

- **XL-OM2-SND-C-100 (full set):**
  - At the defaults: flat at 4, and rectangles, 2653 placements and inner neutronium are equal to Omnificia alone.
  - At the maxima: flat at 16, and the rectangles grew from 11/8/10 to 16.
- **The other five clusters:** generation succeeds and every wall is flat at the defaults and at the maxima.

### 3.6 All five mods together

One cluster of each mod, with every one of the five mods enabled. All generate with no worldgen error. Each is flat where the game walls it, MiniBase's asteroid is untouched, and each colony plays. Result: 10/10.

## 4. Runs (verbatim summary lines, in order)

**First attempts, failing on harness gaps (fixed in 2.1):**
- `sb-compat-minibase-20261019-150336`: `Session done: 0 passed, 5 failed, 0 errors, 0 timeouts, 5 skipped; … of 10; 26.3 s real time`. MiniBase worldgen gave up: "Index was outside the bounds of the array".
- `sb-compat-minibase-20261019-150646`: `Session done: 0 passed, 5 failed, 0 errors, 0 timeouts, 5 skipped; … of 10; 26.6 s real time`. Same cause; the destination screen prefab was not found in ScreenPrefabs.
- `sb-compat-empty-20261019-151332`: the launcher threw "Workshop mod 1825146346 is not installed (not in mods.json)".

**Runs while the tests were developed:**
- `sb-compat-minibase-20261019-150745`: `Session done: 4 passed, 1 failed, 0 errors, 0 timeouts, 5 skipped; … of 10; 49.3 s real time; fixture loaded again 3 time(s)`. MiniBaseSame failed on MiniBase's random geyser.
- `sb-compat-minibase-20261019-150944`: `Session done: 4 passed, 2 failed, 0 errors, 0 timeouts, 5 skipped; … of 11; 61.6 s real time; fixture loaded again 4 time(s)`. The spot-only compare was wrong: the oil reservoir pick is not a geyser.
- `sb-compat-minibase-20261019-151153`: `Session done: 6 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped; 6 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped of 11; 62.8 s real time; fixture loaded again 4 time(s)`
- `sb-compat-empty-20261019-151443`: `Session done: 8 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped; … of 10; 233.3 s real time; fixture loaded again 5 time(s)`. EmptyEmpteraSame and EmptyIslandsSame failed.
- `sb-compat-empty-20261019-152002`: `Session done: 10 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped; … of 12; 283 s real time; fixture loaded again 7 time(s)`. The same two failed, with rectangle evidence; the AloneAgain controls passed.
- `sb-compat-voidria-20261019-152533`: `Session done: 8 passed, 2 failed, 0 errors, 0 timeouts, 5 skipped; … of 15; 137 s real time; fixture loaded again 6 time(s)`. VoidriaAlone and VoidriaMiniAlone failed on the "game's wall is uneven" assertion; Voidria has no wall, so the assertion is now a log line.
- `sb-compat-camera-20261019-153104` and `sb-compat-camera-20261019-153304`: the camera diagnosis (5.1). The second run: `Session done: 2 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; … of 2; 80.9 s real time`
- `sb-compat-voidria-20261019-153508`: `Session done: 10 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped; 10 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped of 15; 156.5 s real time; fixture loaded again 6 time(s)`
- `sb-compat-baator-20261019-153817`: `Session done: 10 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 10 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 10; 328.9 s real time; fixture loaded again 5 time(s)`
- `sb-compat-omnificia-20261019-154421`: `Session done: 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 5; 113.2 s real time; fixture loaded again 2 time(s)`
- `sb-compat-minibase-base-20261019-154650` (`-Mode base`): `Session done: 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 5; 41.5 s real time; fixture loaded again 2 time(s)`
- `sb-compat-empty-base-20261019-154759` (base): `Session done: 2 passed, 10 failed, …`. The game took Terra; this is the category fix in 2.1.
- `sb-compat-empty-base-20261019-155219` (base): `Session done: 11 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped; … of 12; 150.3 s real time; fixture loaded again 7 time(s)`. EmptyIslandsSame failed (3.2).
- `sb-compat-voidria-base-20261019-155516` (base): `Session done: 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 5; 67.6 s real time; fixture loaded again 2 time(s)`
- `sb-compat-omnificia-all-20261019-155734`: `Session done: 10 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 10 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 10; 361.9 s real time; fixture loaded again 9 time(s)`
- `sb-compat-together-20261019-160407` (`-Steam` all five): `Session done: 10 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 10 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 10; 258.1 s real time; fixture loaded again 4 time(s)`

**Final runs on the committed code:**
- `sb-final-default-20261019-160904` (default suite, no Workshop mod): `Session done: 17 passed, 0 failed, 0 errors, 0 timeouts, 3 skipped; 17 passed, 0 failed, 0 errors, 0 timeouts, 3 skipped of 20; 199.2 s real time; fixture loaded again 7 time(s)`
- `sb-final-base-vanilla-20261019-161403` (`-Mode base`, `*Identity.BaseTerra*;*Clusters.SkewedBase*`): `Session done: 7 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 7 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 7; 101.1 s real time; fixture loaded again 4 time(s)`
- `sb-final-minibase-20261019-161609`: `Session done: 6 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped; 6 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped of 11; 62.1 s real time; fixture loaded again 4 time(s)`
- `sb-final-empty-20261019-161733`: `Session done: 11 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped; 11 passed, 2 failed, 0 errors, 0 timeouts, 0 skipped of 13; 318.8 s real time; fixture loaded again 7 time(s)`. EmptyEmpteraSame and EmptyIslandsSame failed: the defect in 3.2.
- `sb-final-voidria-20261019-162315`: `Session done: 10 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped; 10 passed, 0 failed, 0 errors, 0 timeouts, 5 skipped of 15; 154 s real time; fixture loaded again 6 time(s)`
- **Not rerun after the last harness change:** Baator and Omnificia, because they passed already. The change only affects which screen is open while the coordinate is applied, and the vanilla checks above show it.

Every run printed `Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies`.

### Mod × world × setting × result

Columns: **Flat** = every asteroid the game walls is flat at the defaults (or untouched, for a mod-built asteroid); **Same** = placements and rectangles equal to the mod alone at the defaults; **Maxima** = flat at every kind's maximum; **Plays** = colony played with no error.

| Mod | Mode | Coordinate | Asteroids | Flat | Same | Maxima | Plays |
|---|---|---|---|---|---|---|---|
| MiniBase | SO | MINI-B-100 | 1 (built by the mod) | untouched, 3249/3249 cells | buildings equal; geyser random in MiniBase itself | untouched | pass |
| MiniBase | base | V-MINI-100 | 1 (built by the mod) | untouched | buildings equal | untouched | pass |
| Empty Worlds | SO | EMP-PHO-100 | 9 | pass | **fail** (3.2) | pass | pass |
| Empty Worlds | SO | ISD-PHO-100 | 9 | pass | **fail** (3.2) | pass | pass |
| Empty Worlds | base | EMP-PHO-100 | 1 | pass | pass (one rectangle differs, placements equal) | pass | pass |
| Empty Worlds | base | ISD-PHO-100 | 1 | pass | **fail** (3.2) | pass | pass |
| Voidria | SO | VOA-TG-C-100 | 3 | pass (no wall on VoidriaSO; 0 cells changed) | pass | pass | pass |
| Voidria | SO | VOA-TG-M-100 | 3 | pass (as above) | pass | pass | pass |
| Voidria | base | VOA-TG-B-100 | 1 | pass (0 cells changed) | pass | pass | pass |
| Baator | SO | BTOR-L-100 | 11 | pass | pass | pass | pass |
| Baator | SO | BTOR-S-100 | 11 | pass | pass | pass | pass |
| Omnificia | SO | XL-OM2-SND-C-100 | 1 | pass | pass | pass | pass |
| Omnificia | SO | BCH, FST, GDN, ICV, SWP -C-100 | 1 each | pass | — | pass | — |
| all five | SO | MINI-B, EMP-PHO, VOA-TG-C, BTOR-L, XL-OM2-SND-C | — | pass | — | — | pass |

## 5. Other findings

### 5.1 The black camera (the user's question)

**Which tests:** my first `Compatibility.*Plays` tests, in the MiniBase, Empty Worlds and Voidria runs up to 15:35. The flatness tests also show a mostly black start asteroid, because only the start area is revealed.

**What it was:** fog of war on asteroids not yet visited. Nothing was wrong with the worlds:
- The camera was inside the right asteroid.
- The cells were present.
- The active world was the right one.

**Evidence** (`EmptyEmpteraCamera`, Empty Worlds EMP-PHO-100, run `sb-compat-camera-20261019-153304`):
- **Start asteroid:** "camera at (320, 126) … active asteroid 0 EmpteraDLC at (232, 0) size 170x240; inside it; 2978 of 40800 of its cells out of the fog".
- **After `ClusterManager.SetActiveWorld(1)` alone**, as the old Plays did: "camera at (550, 76), cell 68798 of asteroid 1; active asteroid 1 SwampyLandingSite at (486, 0) size 128x153; inside it; **0 of 19584** of its cells out of the fog". The screen is black apart from the HUD.
- **After `RevealAsteroid`:** "19531 of 19584 … out of the fog". The map shows.

Screenshots (all under `C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-compat-camera-20261019-153304\artifacts\`):
- black: `SmoothBorders.Tests.Compatibility.EmptyEmpteraCamera-2-setactiveworld-only.png`
- revealed: `SmoothBorders.Tests.Compatibility.EmptyEmpteraCamera-3-setactiveworld-only-revealed.png`
- after the game's switch: `SmoothBorders.Tests.Compatibility.EmptyEmpteraCamera-4-game-switch-revealed.png`

The fix is in 2.2. This answer was also sent to main by SendMessage while the unit was running.

### 5.2 Player's folders: before and after

**Before** (15:03, script `scratchpad\compat\folders.py`): 3061 entries. save_files 154, cloud_save_files 237, RetiredColonies 2670.

**After** (16:27, the game exited): 2705 entries. save_files 154, cloud_save_files 237, RetiredColonies 2314.
- New entries: 0.
- Changed: 1, `cloud_save_files/…/steam_autocloud.vdf` (its date).
- Gone: 356, all under RetiredColonies.

**The 356 gone entries were not removed by this unit.** No command of mine deletes anything. They are the earlier test leftovers listed in unit 13: Antfarm, Spacerock, Data Abyss, Beyond, Tragic Sanctuary and so on. A clean-up elsewhere must have removed them during this session.

**What remains in RetiredColonies:** Citadel, Crashpad, Crew, Dangerous Spacecamp, Devland, Frigidaire, March 2023 Test Map, Meteor, Mod Test, ModTest, Spaced In, Swanky Lab, The Aftermath, The Perfect Dreamland. That is unit 13's "left alone" list without Tragic Sanctuary.

**Other state after the runs:**
- mods.json SHA1 is `4779bb7d13cd14fb2d029378ae4a089f9b144f57`, as before.
- `kplayerprefs.yaml` has `EXPANSION1_ID.ENABLED: 1` and no `.onitest-mode` file.
- `mods\config\SmoothBorders` is absent, as before.

### 5.3 Representative sample

After the user's note, I stopped re-running full sets: the final pass is one run per mod set on the final code (section 4). Baator and Omnificia were not repeated, for the reason given at the end of section 4.

### 5.4 Exceptions in the logs (none from Smooth Borders)

- **`NullReferenceException at OverlayScreen.get_mode` (`StatusItemRenderer`)**
  - When: during colony load.
  - Where: these runs, and unit 11's vanilla runs (`sb-base-…`, `sb-so-dlc…`, `sb-so-mini…`).
  - Status: present before this unit; not a Workshop mod and not Smooth Borders.
- **`ColonyDestinationScreen2 … Sequence contains no elements`**
  - Cause: my first version of the destination-screen step, with the category unset.
  - Status: gone since the category fix. 0 in every `sb-final-*` log.
- **One `NullReferenceException at BackwallManager.BackwallIndexer.get_Element`**
  - Where: Omnificia, `sb-compat-omnificia-all`.
  - Status: the known vanilla NRE (Klei report r61226).

## 6. Goal 3: supporting the mods' own asteroid kinds in the settings (report only)

**What the settings miss today:**
- Pairs outside the table take max(1, T−R) and ignore the sliders: Empty Worlds (6, 4) → 2, Baator_coldterra (2, 2) → 1, Baator_oilyswampy (6, 2) → 4, and MiniBase's natural moonlets (3, 2) → 1.
- Some pairs land in an unexpected kind: Baator's primary (4, 2) follows **Moonlets**, and Voidria's primaries (5, 2) follow **Small**.

**What it would take:**
1. **Find the pairs.** When the options dialog opens (the main menu, where `SettingsCache.worlds` is loaded), list every world whose (T, R) is not in the table and whose `DrawWorldBorder` is true. Group them by pair or by mod (`world.filePath` prefix, or `ModUtil`'s mod of origin).
2. **Options.** Add `Options.Extra`, a dictionary keyed by pair ("6,4") or by world file, serialised by PLib's JSON. `Groups.Thickness` consults it before max(1, T−R). The clamp range for a pair is 1…T+R.
3. **Dialog.** Add dynamic entries, one slider per extra pair, labelled with the asteroid names (the world's `name` string key via `Strings.Get`), under a "Worldgen mods" heading. This needs PLib's dynamic options: `IOptions.CreateOptions` returns extra `IOptionsEntry`s. It must be checked against the PLib 4.25 in use; unit 10 found `DynamicOptionAttribute` only in the incubator source.
4. **Translations:** the heading and the tooltip pattern only, since the asteroid names come from the mods.
5. **Optional:** a per-world override for mods whose primary sits under a moonlet pair (Baator).
6. **Tests:** a NoColony dialog test with a fake extra pair, and one Compatibility run with Baator at a custom value.

**Size:** about one implement unit plus one test unit. Nothing for it was built.

## 7. Report back on the packet

- **Pointers that did not apply:** the ModCheck and tools-overview blocks (`blk-3ca89d57dfde`, `blk-647210c6a78a`, `blk-6c2bbf052519`) and the TestRunner code-style, naming and glossary records. I worked from the TestRunner README, `Worldgen.cs`, `Session.cs` and the decompiled game.
- **Records that govern this work but did not appear:** as in units 08 and 11, the main store's brief matched nothing for `mods/smooth-borders/src`. The testing records (`blk-87272da04dee` generated worlds, `blk-c8a18ea4088d` per-test options, `blk-33fb80177cba` pacing) are reachable only by tag; I read them with `ks read`.
- **Stale record:** `blk-87272da04dee` still says "a coordinate of the other mode skips the test". It now needs to say:
  - Workshop prefixes are looked up in the loaded clusters
  - the destination screen is opened
  - Workshop items are registered
  - the reveal helper exists
- **Facts no store holds, to land through the write path:**
  - The game reads only `DrawWorldBorder`, `DrawWorldBorderForce` and T/R in `DrawWorldBorder`.
  - MiniBase sets its clusters up in `ColonyDestinationSelectScreen.OnSpawn`.
  - `ClusterManager.SetActiveWorld` alone leaves undiscovered asteroids fogged.
- **Mismatched reasons:** none.

## 8. ICON tooling friction

None. `ks read --store C:/dev/ONI-Mods blk-87272da04dee blk-c8a18ea4088d blk-33fb80177cba` worked on the first try.

## 9. Other friction (not ICON)

- **Long heredoc:** a long Python heredoc through Bash failed with "unexpected EOF". This is the known cut near 8k characters. I moved the script into a file written with Write.
- **Line endings:**
  - `sed -i` and the Write tool left LF in files that use CRLF. I normalised them before committing.
  - `Invoke-OniTests.ps1` was LF already and stays LF.
- **Waiting on long runs:** the PowerShell tool moved a 3-run loop to the background after 600 s, and a Monitor on `summary.txt` expired unused. Plain foreground runs, with the timeout raised, were simpler.
