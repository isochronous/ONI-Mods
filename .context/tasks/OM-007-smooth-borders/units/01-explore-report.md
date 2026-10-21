# Unit 01 report: how vanilla worldgen draws the neutronium border, and where a mod can make it flat

## Sources read

- **Game build:** U59-744825 (`LaunchInitializer.cs` 10-12 `UPDATE_NUMBER = 59`; `KleiVersion.cs` 3 `ChangeList = 744825u`). Steam buildid 24423041 (`F:\SteamLibrary\steamapps\appmanifest_457140.acf`). The install is at `F:\SteamLibrary\steamapps\common\OxygenNotIncluded`, the first Steam library `common/oni-mod.props` lists.
- **Decompiled code:** `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\676240ed-29f0-48be-b5c6-6462cdcaf87f\scratchpad\decomp\`. `acs\` holds Assembly-CSharp and `fp\` holds Assembly-CSharp-firstpass. Both were decompiled with `ilspycmd -p` (`~/.dotnet/tools/ilspycmd`). The repo and store name no decompiler; OM-006 used the same scratchpad layout. Below, "WorldGen.cs" means `acs\ProcGenGame\WorldGen.cs`, and so on.
- **Settings data:** `...\OxygenNotIncluded_Data\StreamingAssets\worldgen\` and `...\StreamingAssets\dlc\*\worldgen\`.
- **Border reproduction:** `...\scratchpad\border_sim.py` reproduces `KRandom` (fp\KRandom.cs), `SeededRandom.RandomRange(int,int)` (fp\SeededRandom.cs 38-42) and the walk in `DrawWorldBorder`. It prints the thickness profile of each edge for any world size, T and R. It is a reimplementation and has not been checked against a generated world (see Gaps).

## Q1. What draws the border, step by step

**Entry.** `WorldGen.RenderToMap` (WorldGen.cs 906-969) first fills the world with Katairite (919-929). It then runs `ProcessByTerrainCell`, which places biomes, features and subworld borders (933). After that:
```
943  if (Settings.GetBoolSetting("DrawWorldBorder"))
945      SeededRandom rnd = new SeededRandom(0);
946      DrawWorldBorder(ref simData, data.world, rnd, ref borderCells, ref poiBounds, updateProgressFn);
```

**The method.** `private void WorldGen.DrawWorldBorder(ref WorldgenSimData simData, Chunk world, SeededRandom rnd, ref HashSet<int> borderCells, ref List<RectInt> poiBounds, OfflineCallbackFunction updateProgressFn)` (WorldGen.cs 1354-1439).
- **Settings read** (1356-1358): `DrawWorldBorderForce` (bool), `WorldBorderThickness` (int, T) and `WorldBorderRange` (int, R).
- **Left and right edges** (1372-1400) are drawn together. The loop runs over rows from y = height-1 down to 0. For each row:
  - Left edge: `num = clamp(num + rnd.RandomRange(-2, 2), -R, R)` (1375). Cells x = 0 .. T+num-1 become Unobtanium (1377-1386).
  - Right edge: the same with its own offset `num2` (1387-1399). Cells x = W-1 .. W-(T+num2) become Unobtanium.
- **Bottom and top edges** (1408-1436) are drawn together. The loop runs over columns from x = 0 to W-1:
  - Bottom edge: offset `num11` (1411). Cells y = 0 .. T+num11-1 (1413-1422).
  - Top edge: offset `num12` (1423). Cells y = H-1 downward (1425-1435).
- **Each cell written:** `simData.cells[c].SetValues(unobtanium idx, default temperature, default mass)` and `borderCells.Add(c)` (1382-1383, 1395-1396, 1418-1419, 1431-1432).
- **Step size.** `RandomRange(-2, 2)` returns `-2 + (int)(NextDouble()*4)` (SeededRandom.cs 38-42), so each step is one of {-2, -1, 0, +1}. The mean step is -0.5, so the walk drifts down and sits on the floor -R most of the time. Thickness on every edge ranges over **[T-R, T+R]**. Going along an edge, it can rise by at most 1 per column or row but can fall by 2. Each edge's offset starts at 0, so the first rows or columns start near T.
- **Exclusion rectangles (POI bounds).** After each pair of edges the method adds rectangles to the WorldGen field `POIBounds`:
  - left: `(0, 0, maxLeft+1, H)` (1401)
  - right: `(minRightX, 0, W-minRightX, H)` (1402)
  - bottom: `(0, 0, W, maxBottom+1)` (1437)
  - top: `(0, maxTopY…)` (1438)

  Each rectangle is as wide as that edge's **maximum** thickness and covers the whole edge. The method writes to the field, not to its `poiBounds` parameter. `RenderOffline` passes `ref POIBounds` (468), so they are the same list.
- **What the border skips.** Vacuum is skipped, but only in space-tagged regions. Line 1370 collects the terrain cells tagged `RemoveWorldBorderOverVacuum` (fp\ProcGen\WorldGenTags.cs 121). For each row or column, a flag records whether the edge point lies in such a terrain cell (1376, 1388, 1412, 1424). If it does and `DrawWorldBorderForce` is false, strip cells that are Vacuum or Void are left alone and solid cells there still get neutronium (condition at 1380/1393/1416/1429). The tag is set by the space subworlds: `Space`, `SpaceNoBorder`, `Surface`, `SurfaceCrags`, the Spaced Out `SpaceWith*Border` family, and the dlc2/4/5 space subworlds. In practice:
  - The top edge and the upper part of both side edges have no border over open space, but do have one wherever regolith or other solid material reaches the edge.
  - Within a space region the border follows the terrain, not the walk.
- **What the border does not skip.** Nothing else. There is no check for templates or POIs, because templates are placed after the border (Q4).
- **After the pass,** `RenderOffline` loops over `borderCells` (WorldGen.cs 476-481). It sets each cell to Unobtanium again (default values), sets its backwall to vacuum (`simData.backwallCells[item].SetValues(vacuumElement.idx, 0, 0)`), and marks it `claimedPOICells[item] = 1`.

**Where the settings come from.** `WorldGenSettings.GetSetting<T>` (fp\ProcGen\WorldGenSettings.cs 207-222) checks the world file's `defaultsOverrides.data` first and then `SettingsCache.defaults` (`worldgen/defaults.yaml`, loaded at fp\ProcGen\SettingsCache.cs 612). No world trait or cluster file sets these keys; a grep of all worldgen YAML finds them only in `defaults.yaml` and in world files.
- **`worldgen/defaults.yaml` lines 35-38:** `DrawWorldBorder: true`, `DrawWorldBorderForce: false`, `WorldBorderThickness: 10`, `WorldBorderRange: 6`. So the default thickness is **4 to 16**.
- **Worlds with no override** use T=10, R=6:
  - every base-game world (256x384, Helicona 384x384, KleiFest 380x240)
  - every Spaced Out "Vanilla*" classic world (240x380)
  - the Spaced Out start and moonlet worlds at 160x274
  - Ceres, Prehistoric and Aquatic base/classic/spaced-out asteroids
  - Moon_Barren
- **Overrides** (T, R):

  | Worlds | T, R | Notes |
  |---|---|---|
  | Spaced Out minis 128x153 (MiniBadlands*, MiniFlipped*, MiniForestFrozen*, MiniMetallicSwampy*, MiniRadioactiveOcean*), IdealLandingSite, MetalHeavyLandingSite, SwampyLandingSite, dlc2 MiniShattered* | 5, 2 | |
  | WarpOily*, TundraMoonlet, WaterMoonlet, MooMoonlet, OilyMoonlet, Mixing{Ceres, Prehistoric, Aquatic}Asteroid | 4, 2 | |
  | OilRichWarpTarget, MiniRegolithMoonlet | 4, 1 | |
  | Medium* worlds (160x176), SmallRadioactiveLandingSite, SmallWarpOilySwamp | 3, 1 | |
  | MarshyMoonlet, NiobiumMoonlet, RegolithMoonlet | 2, 1 | |
  | TinyStart, TinyForest, TinyIce, TwinMoonlet, SulfurMoonlet, SpaceshipInterior, TinySurface | 1, 0 | already flat |
  | BigEmpty, TinyEmpty | 1, 0 | Force true |
  | TinyMagma | 0, 0 | no border |
  | TheRock | | `DrawWorldBorder: false` |

**Base game compared with Spaced Out.** The code path is the same. In Spaced Out, `Cluster.BeginGeneration` (acs\ProcGenGame\Cluster.cs 250-334) runs `GenerateOffline` and then `RenderOffline` once per asteroid, each with its own grid (`GridSettings.Reset(size)`, 310) and its own settings. Only T, R and the world size differ.

**Every seed of a world gets the same border.** The RNG is always `new SeededRandom(0)` (945). `RandomRange` is called once per row or column on every edge, whatever the cells contain (1375, 1387, 1411, 1423). The vacuum skip changes which cells are written but never the walk. So for a given world size, T and R, every seed draws the same border shape. The left and right walks depend on H; the bottom and top walks depend on H and W, because their draws start at index 2H.

Reproduced statistics:

| World | T, R | left | right | bottom | top |
|---|---|---|---|---|---|
| 256x384 | 10, 6 | 4..15 (avg 5.5) | 4..11 (avg 4.7) | 4..10 (avg 4.7, 157 of 256 columns at 4) | 4..12 (avg 5.0) |
| 128x153 | 5, 2 | 3..7 | 3..6 | 3..7 | 3..7 |
| 160x176 | 3, 1 | 2..4 | 2..4 | 2..4 | 2..4 |

## Q2. The stepped mound at the bottom right of before-border.png

**It comes from the border walk, not from another source.** Evidence:
1. **The shape.** The mound rises gradually in single-tile steps from the left and drops steeply on its right side. The bottom walk runs left to right with steps of at most +1 and as low as -2 (Q1), which gives exactly that asymmetry. The 1-tile spikes and notches elsewhere are single +1 steps off the floor (T-R).
2. **The reproduced profile.** For a 256x384 world (T=10, R=6) the reproduced bottom profile has:
   - a notched plateau of thickness 5-6 at x≈180-186
   - a gap, then a mound that climbs 5,5,6,7,8,9,9,9,8,9,**10**,9,8,7 between x≈196 and x≈209
   - a drop back to the floor of 4

   That matches the plateau-then-mound sequence in the screenshot. The 240x380 profile has the same feature about 4 columns further left. The ASCII profile is in `border_sim.py 256 384 10 6 x`.
3. **The texture.** It is the same neutronium texture as the border band, with no break between them.

**Other sources of neutronium near an edge** exist, but none produces a stepped shape:
- **Magma biome bands.** `biomes/Magma.yaml` `MagmaBed` and `CooledMagmaBed` have an `Unobtanium` band at the top of their noise range (lines 28, 39). Base-game `subworlds/magma/Bottom.yaml` and Spaced Out `BottomSmall` and `MagmaSurface` use them. This makes irregular neutronium blobs inside the bottom magma core, sometimes touching the border.
- **Subworld override noise.** Values ≥200 become Unobtanium in `TerrainCell` (TerrainCell.cs 829-833, from `world.overrides`, written per subworld `overrideNoise` in `WriteOverWorldNoise`, WorldGen.cs 1546+).
- **Templates and geysers** (`templates/geysers/*.yaml` and many POI templates) contain Unobtanium cells.
- **Subworld borders.** `borders.yaml` defines an `impenetrable` (Unobtanium) border type, but no subworld uses `borderOverride: impenetrable`. The values actually used are rocky, hardToDig, NONE, and so on.
- **The `subworlds/Impenetrable.yaml` subworld.** All Unobtanium; no world file in this build references it.

A flat border removes only the walk-driven shape. Natural neutronium blobs from the magma core can still touch the border, so a "perfectly flat" border can look bumpy where a blob joins it. Whether the user wants those removed is a separate decision.

## Q3. RNG and determinism

- **The border's draws come from a private generator.** It is `new SeededRandom(0)`, created at WorldGen.cs 945 and passed only to `DrawWorldBorder`. **No other stage shares it.** Removing, adding or changing the border's draws shifts nothing else directly.
- **The other generators:**
  - `myRandom = new SeededRandom(worldSeed)` (409). Used in layout (`SwapNodesAround`, 1096) and in templates (`TemplateSpawning.DetermineTemplatesForWorld`, 486).
  - Mob and plant spawning: `new SeededRandom(data.globalTerrainSeed)` (545).
  - The settle sim: native SimDLL seeded with `simSeed` (WorldGenSimUtil.cs 21).
- **The border's output still feeds later stages that draw random numbers:**
  1. **POIBounds feeds templates (`myRandom`).** `FindTargetForTemplate` drops candidate terrain cells whose padded template bounds overlap any POI bound (TemplateSpawning.cs 438-461, 401-410). It then calls `filteredTerrainCells.ShuffleSeeded(myRandom.RandomSource())` (~397), which makes one draw per element (fp\WorldGenUtil.cs 5-16). If the border rectangles change size, the candidate counts change, the number of draws changes, and every later template, geyser and story-trait placement on that world can change. The starting template throws "overlaps world boundaries" if it hits a bound (TemplateSpawning.cs 75-82), which makes generation fail.
  2. **Story traits carry across worlds** (Spaced Out). A trait not placed on one world goes to the next (Cluster.cs 307-332). A change on one asteroid can therefore move traits on others.
  3. **Cell contents feed mob and plant placement** (terrain-seed RNG). `PlaceBiomeAmbientMobs` drops Unobtanium and claimed cells from the spawn candidates (MobSpawning.cs 109-115). `GetMobPossibleSpawnPoints` shuffles the suitable cells (MobSpawning.cs 161-174, shuffle at 172), one draw per cell. Spawn count = round(count × density) (153). One `rnd` is shared across all terrain cells in order (WorldGen.cs 545-557). If the border's cells change in one terrain cell, critter and plant placement can shift in **every later terrain cell of that world**, not just at the edge.
  4. **Cell contents feed the settle sim.** Liquids and gases next to the edge settle differently. Whether the native sim's internal RNG makes that spread further is unknown (see Gaps).
- **The only way to keep a seed identical to vanilla everywhere except the edge band:**
  - keep POIBounds exactly as vanilla adds them (this protects templates, geysers and story traits), and
  - accept that critters, plants and settle results can differ. Any worldgen hook that changes cells before the settle has this effect.
  - The exception is the post-settle pass P2 (Q5b). It changes only the native sim's cells and leaves `simData` as vanilla, so mob and plant placement also stays identical.
- **Keeping vanilla POIBounds is cheap.** The walk is fully deterministic (SeededRandom(0), depends only on H, W, T and R), so a mod can replay it without writing cells to get the vanilla rectangles.

## Q4. Stage order around the border

Per world, in `Cluster.BeginGeneration` (Cluster.cs 250-334) on the worldgen thread:
1. `WorldgenMixing.DoSubworldMixing` (256), then worlds are sorted by distance from the start (277-287).
2. `GridSettings.Reset` and `BackwallManager.Clear` (310-311), then `GenerateOffline` (312; layout and noise), then `FinalizeStartLocation` (317).
3. `RenderOffline` (WorldGen.cs 463-539):
   1. `WriteOverWorldNoise` (467).
   2. `RenderToMap` (468 → 906-969):
      1. clear to Katairite (919-929)
      2. `ProcessByTerrainCell` (933): biomes, features, subworld borders. Natural Backwalls' `TerrainCell.ApplyBackground` postfix runs inside this step.
      3. **`DrawWorldBorder` (943-948)**
      4. `baseStartPos` and `modifyLayoutTags` (949-967)
   3. **Border cells are finalised** (476-481): Unobtanium, vacuum backwall, `claimedPOICells`.
   4. `TemplateSpawning.DetermineTemplatesForWorld` (486). Uses **POIBounds**: the starting template (64-87), template rules (89-125, 210-312) and story traits (127-173). Templates with `overridePlacement` (285-293; 69 uses in YAML) **skip the bounds check**.
   5. `EnsureEnoughElementsInStartingBiome` (507), then start-world tags (509-520).
   6. `WorldGenSimUtil.DoSettleSim` (523; WorldGenSimUtil.cs 16+): 500 sim ticks. At tick 498 every template's cells are stamped with `ReplaceType.Replace` and **no claimed-cell check** (35-69). A template that overlapped the border would overwrite neutronium. The POI bounds are what prevent that.
   7. `PlaceTemplateSpawners` (527-530 → GameSpawnData.cs 37+): template buildings, pickupables and ores on **claimed cells** (border cells included) are dropped (49, 59).
   8. `SpawnMobsAndTemplates` (534, 541+): `DetectNaturalCavities`, then mob and plant placement with claimed cells and Unobtanium excluded.
4. Story-trait bookkeeping (Cluster.cs 325-332) and `AssignClusterLocations` (351).

**Users of `borderCells`:** only `RenderOffline` 476-481 (re-stamp, vacuum backwall, claim). Through `claimedPOICells` it also feeds template object placement and mob placement.

**Settings read again outside worldgen:**
- `WorldContainer.SetWorldDetails` (WorldContainer.cs 803) reads `DrawWorldBorder && DrawWorldBorderOverVacuum`. `DrawWorldBorderOverVacuum` is in no YAML, so it reads false. The result is `FullyEnclosedBorder`, used for surface rendering (ClusterCoverPostFX.cs 55, PropertyTextures.cs 558). Changing T or R does not touch it; switching `DrawWorldBorder` off would only matter if some world set `DrawWorldBorderOverVacuum`.
- In game, `Game.SanityCheckBoundsNextFrame` (Game.cs 857-877, started at 883) digs out any non-Unobtanium solid in the top `Grid.TopBorderHeight` (= 2, Grid.cs 632) rows of every discovered world.

**Nothing downstream needs the border's exact shape.** Later stages depend on which cells it covers and how big the POI bounds are, as described in Q3.

## Q5. Candidate hook points

All of these run in the real game's own worldgen. `Cluster.Generate` starts a `new Thread(ThreadMain)` inside the game process (Cluster.cs 240-242), and its only caller is the new-game worldgen screen (OfflineWorldGen.cs 349). Harmony patches apply process-wide, so they also affect that thread. Patch code must not touch Unity objects or main-thread state.

No worldgen runs for existing saves. A save stores its cells, and `RenderOffline` has no other caller (Cluster.cs 320). So every option below affects **new worlds only**.

| # | Hook | What the patch does | RNG and determinism | POIs and templates at the edge | Risks and fragility |
|---|---|---|---|---|---|
| A | **Transpiler on `WorldGen.DrawWorldBorder`** (private; `[HarmonyPatch(typeof(WorldGen), "DrawWorldBorder")]`). Replace the four `call SeededRandom::RandomRange(int32,int32)` (1375, 1387, 1411, 1423) with a helper that returns a constant step | The step is clamped to [-R, R]. Return **-1000** and every edge is exactly **T-R** thick from the first row (the vanilla floor). Return **0** for exactly **T**, or **+1000** for **T+R**. One constant, the rest of vanilla logic kept (vacuum skip, Force, POIBounds, progress) | The border RNG is private, so nothing shifts directly. POIBounds become the flat extents. With T-R they are smaller than vanilla, so template, geyser and story-trait placement change for the seed. Mob and plant placement and settle change too (Q3) | POIBounds always match the actual border, so no template overlaps it (except `overridePlacement`, same as vanilla). With T-R, templates may sit closer to the edge than in vanilla | Smallest code. Depends on IL shape: if Klei renames or inlines `RandomRange` or rewrites the loop, the match fails. The transpiler can detect 0 or ≠4 matches and log or skip. Patching a private method by name: Harmony errors at load if the method is gone |
| A′ | Same as A, plus a prefix or postfix that replays the vanilla walk (SeededRandom(0)) for extents only and puts the **vanilla** rectangles in POIBounds | Flat border, vanilla exclusion zones | **Templates, geysers and story traits stay identical to vanilla for the seed.** Only cells in the edge band differ, along with mob, plant and settle results | Templates stay as far from the edge as in vanilla | Copies about 20 lines of walk logic; drifts silently if Klei changes the walk |
| B | **Prefix on `DrawWorldBorder` that returns false** and reimplements it flat | Full control: per-edge thickness, POIBounds as flat or vanilla (A′), vacuum rule kept or changed | Same as A or A′ depending on the POIBounds choice | As A or A′ | Duplicates about 85 lines (vacuum-tag lookup, Force, progress callbacks, `POIBounds` field). Silent drift if Klei edits the method; loud failure only if the signature changes. Overrides any other mod's patch of this method |
| C | **Settings only, as a postfix on `ProcGen.WorldGenSettings.GetIntSetting(string target)`** (public, firstpass, fp\ProcGen\WorldGenSettings.cs 245-248). Return 0 for `"WorldBorderRange"` and optionally a new value for `"WorldBorderThickness"` | R=0 makes `clamp(...)` always 0, so thickness is exactly T (= 10 on default worlds, 2.5x vanilla's usual 4). For T-R the postfix must compute it from the unpatched values, read via the settings data, because a recursive call would hit the patch | As A (border RNG private). Flat at T is **thicker** than vanilla's maximum on some edges (for example the 256x384 bottom max is 10 and the right max is 11; at T=10 it is roughly equal). POIBounds may grow, so a guaranteed template could fail to place and the start-template-overlap exception becomes possible (TemplateSpawning.cs 75-82) | POIBounds match the border | **Inlining risk:** `GetIntSetting` IL is exactly 20 bytes (`ilspycmd --ilcode`), at Mono's default inline limit; if Mono inlines it into `DrawWorldBorder` the postfix never runs. Not verified. Other callers of `GetIntSetting` get a name check per call (cheap). Also respects other mods' world YAML because it derives from their T and R |
| C′ | Settings only, by **rewriting the data**: after settings load (for example a postfix on `SettingsCache.LoadFiles`, or on `WorldGen.LoadSettings`), set `defaults.data["WorldBorderRange"]=0` and the same in each world's `defaultsOverrides.data` | Same result as C | Same as C | Same as C | Mutates shared caches (`GetSetting` already caches parsed values back, WorldGenSettings.cs 202). Must also catch world files that mods load later. No inlining risk |
| C″ | Settings only, **by shipped YAML** (the mod overrides `worldgen/defaults.yaml` and the world files) | Same | Same | Same | World overrides win over defaults (WorldGenSettings.cs 212-214), so about 54 world files would need copying. Conflicts with other worldgen mods and goes stale with every game update. **Not advisable** |
| D | **Post-process: postfix on `DrawWorldBorder`, or on `public bool WorldGen.RenderToMap(OfflineCallbackFunction, ref WorldgenSimData, ref HashSet<int> borderCells, ref List<RectInt> poiBounds)`**. Edit `simData.cells` and `borderCells` before `RenderOffline` 476-481 applies them | **Can only add** neutronium, for example fill every edge up to its vanilla maximum, or up to T+R. Thinning needs a prefix that snapshots the strip cells first, because `DrawWorldBorder` has already overwritten the biome content (`SetValues` at 1383 and elsewhere), and then a postfix that restores them | Flattening to the per-edge vanilla max leaves POIBounds identical to vanilla, so templates are unchanged. Mob, plant and settle results still change | Filling to the per-edge max never touches a template (vanilla rectangles already cover it), except `overridePlacement` templates, whose cells are stamped over it at settle tick 498 as in vanilla | Thick borders (up to 15 on the left of default worlds), so it removes the most playable space. Snapshot and restore is the most moving parts |

| P1 | **End-of-worldgen tile pass, before the settle**: prefix on `public static bool WorldGenSimUtil.DoSettleSim(WorldGenSettings settings, BinaryWriter writer, uint simSeed, ref WorldgenSimData simData, WorldGen.OfflineCallbackFunction updateProgressFn, Data data, List<TemplateSpawning.TemplateSpawner> templateSpawnTargets, Action<OfflineWorldGen.ErrorInfo> error_cb, int baseId)` (WorldGenSimUtil.cs 16). It edits `simData` before `SimDataInitializeFromCells` (21) | Removes border neutronium past the flat line, restoring the pre-border biome cells from a snapshot taken in a `DrawWorldBorder` prefix. Fills notches (only when flat > T-R) with Unobtanium. Details in Q5b | Border, layout and template RNG untouched. **Templates, geysers, POIs and story traits are identical to vanilla** (POIBounds untouched; templates chosen at 486, before the pass). Mob and plant placement can shift, because it reads the settled `simData` (Q3 item 3) | Template cells are stamped at settle tick 498, after the pass, so template neutronium is never touched | Runs once per world at generation. Two private patch points (`DrawWorldBorder` for capture). Liquids and gases settle into the reshaped space naturally |
| P2 | **End-of-worldgen tile pass, after the settle**: inject at tick 498 of `DoSettleSim` (transpiler after the template-stamp block, 37-69), or prefix on `private static bool WorldGenSimUtil.SaveSim(BinaryWriter, Data, int, Action<OfflineWorldGen.ErrorInfo>)` (116) plus one flush frame. Uses `SimMessages.ModifyCell(..., ReplaceType.Replace)` and `SimMessages.SetBackwallData`, as vanilla template stamping does (63-64) | Same cell changes, applied to the native sim just before `Sim.Save` (138). Leaves `simData` as vanilla | **Everything in the spawn data is identical to vanilla** (templates, geysers, mobs, plants), because `simData`, which mob placement reads, stays vanilla. Only the cells in the edge band differ | Must skip template cells explicitly (already stamped at 498): rebuild their set from `templateSpawnTargets` the way 42-66 does | The saved world is what the pass wrote, plus 1-2 sim frames. Restored liquids are not settled. Same private-patch fragility as P1 |
| F | **Fallback, first load (live sim)**: prefix on `WorldGenSpawner.OnSpawn` while the serialized `hasPlacedTemplates` is false (WorldGenSpawner.cs 162-163, 207-215) | Recomputes border cells by replaying the walk per `WorldContainer`, then `SimMessages.ModifyCell` or `ReplaceElement` (SimMessages.cs 1245, 1387) | Worldgen untouched, so the seed is identical (like P2) | Template neutronium must be excluded by footprint. `borderCells` is not saved, and the vacuum rule must be inferred | Runs in the live game. The pre-border biome content is lost (no snapshot possible), so it fills with a guess. Details in Q5b |

Notes for every option:
- **Natural Backwalls** (repo mod) patches `TerrainCell.ApplyBackground`, `WorldGen.GenerateOffline`, `WorldGenSimUtil.DoSettleSim` and `Cluster.BeginGeneration`. None of these conflicts with a `DrawWorldBorder` patch. Cells that stop being border keep the biome backwall that `ApplyBackground` gave them; cells that stay border get a vacuum backwall at 479.
- **The vacuum skip** in space regions means even a "flat" border has no neutronium over open space at the top. Keeping it keeps surfaces open to space. Removing it is the `Force` behaviour, which Klei only uses on the empty test worlds.
- **Fragility across game updates.** Klei could change the step range, the clamp, the shared POIBounds field or the 476-481 finalisation. A and C react to that differently: A's IL match fails loudly if the call disappears, while C keeps working as long as the setting names are read.

## Q5b. Tile pass at the end of worldgen (user preference), with the first-load pass as fallback

### Where "all cells are final" actually is

`RenderOffline` (WorldGen.cs 463-539) runs, per world: border finalisation (476-481), then template choice (486), then `DoSettleSim` (523). `DoSettleSim` runs 500 ticks; at tick 498 it stamps template cells (WorldGenSimUtil.cs 37-69). It then calls **`SaveSim`, which serialises the world from the native sim** (`Sim.Save`, 111 and 138), and `Sim.Shutdown` (112). After that, `PlaceTemplateSpawners` (527) and `SpawnMobsAndTemplates` (534) only write spawn lists (`data.gameSpawnData`).

**The cells written to the save are the sim's state inside `SaveSim`, not `simData`.** So:
- A pass that edits `simData` after `DoSettleSim` returns, or in `Cluster.PerWorldGenCompleteCallback` (Cluster.cs 54, invoked 325-328; no vanilla code sets it), **changes nothing in the save**.
- **The last point where cells can still change is inside `DoSettleSim`, before `SaveSim`.** Two usable spots:
  - **P1, before the settle.** Prefix on `DoSettleSim`, editing `ref simData` before `SimMessages.SimDataInitializeFromCells` (21). Every natural cell is final, the border is final (476-481 done), and template positions are chosen (`templateSpawnTargets` is a parameter). Template cells are not stamped yet; that happens at tick 498, after the pass, so templates always win and **template neutronium is never touched**.
  - **P2, after the settle.** Either:
    - (a) a transpiler injecting a call right after the tick-498 template block, so the pass's `ModifyCell` messages are applied in the same frame as the templates and get frames 498-499, or
    - (b) a prefix on the private `SaveSim` (116) that sends the messages and then runs one flush frame the way the loop does (`SimMessages.NewGameFrame(0.2f, regions)` plus `Sim.HandleMessage(SimMessageHashes.PrepareGameData, …)`, 70-71).

    (a) needs no copied frame code. Here templates are already in the sim, so the pass must skip template cells. Rebuild that set exactly as 42-66 does: `Grid.OffsetCell(Grid.XYToCell(pos), cell.location_x, cell.location_y)` for each `templateSpawnTargets` cell.

**Before or after the settle:**
- **Before (P1).**
  - Restored biome content (liquid pockets, gases) settles with the rest of the world for 500 ticks, so the result looks natural.
  - Mob and plant placement later reads the settled `simData` (MobSpawning.cs 109-115, 161-174) and **can shift its RNG** across the whole world (Q3 item 3).
  - The saved world is "pass plus settle", not exactly what the pass wrote.
- **After (P2).**
  - The saved world is what the pass wrote, plus 1-2 frames.
  - If the pass does **not** write back into `simData`, then `DetectNaturalCavities` and mob and plant placement (WorldGen.cs 541-557) see vanilla cells, and **the whole spawn list is identical to vanilla**.
  - Cost: restored cells are unsettled (a liquid pocket will flow once the game runs), and spawn data was computed against vanilla cells. With flat = T-R the pass only removes neutronium (the walk never goes below T-R), so no spawn can land inside new neutronium. Mobs or plants standing on removed neutronium can lose their floor if the fill is not solid (see "What removed cells become").
  - If flat > T-R, notches get filled. A vanilla spawn could then sit in a filled notch, and a postfix on the private `WorldGen.SpawnMobsAndTemplates` (541) could drop spawns on changed cells. Dropping a spawn does not shift anyone else's RNG.

### Spaced Out: per world

- `Cluster.BeginGeneration` generates **every asteroid at new-game time**, including ones revealed later (Cluster.cs 290-334). Each asteroid gets its own `GridSettings.Reset(size)` (310), `RenderOffline` and `DoSettleSim`/`SaveSim`, so P1 and P2 run once per asteroid automatically.
- Coordinates are the asteroid's **local grid** (0..W-1, 0..H-1; `Grid.XYToCell` is local during generation). The cluster offset is only recorded at save (`simSaveFileStructure.x/y = data.world.offset`, 132-133) and applied at load, so the pass needs no offset.
- Per-world settings: `WorldGen.Settings` for T and R. P1 can reach T and R through `settings` (the first parameter of `DoSettleSim`).
- Base game is the same path with one world.

### Which data must change together

| Structure | P1 (simData, before the settle) | P2 (live worldgen sim, after the settle) |
|---|---|---|
| Element, mass, temperature | `simData.cells[c].SetValues(...)` (`Sim.Cell[]`, WorldgenSimData.cs 5) | `SimMessages.ModifyCell(c, elementIdx, temperature, mass, diseaseIdx, diseaseCount, ReplaceType.Replace)` (SimMessages.cs 1245) |
| Disease | `simData.diseaseCells[c]`: idx 255 and count 0 for new neutronium; snapshot for restored cells | same values through the `ModifyCell` arguments |
| Backwall | `simData.backwallCells[c]`: vacuum for new neutronium (as 479 does); snapshot for restored cells (Natural Backwalls' `ApplyBackground` backwalls are inside the snapshot) | `SimMessages.SetBackwallData(c, idx, mass, temperature)` (as WorldGenSimUtil.cs 64) |
| `borderCells` | local to `RenderOffline` (465) and already applied at 476-481. Capture it in a `DrawWorldBorder` postfix; it is not needed afterwards | same |
| `claimedPOICells` | Leave removed cells claimed: template objects (GameSpawnData.cs 49, 59) and mobs stay excluded there, the same as vanilla. Add newly filled notch cells | Leave as is if `simData` stays vanilla |
| Zone and biome | Per-polygon only: `WorldDetailSave.OverworldCell(zoneType, terrain cell)` (WorldGenSimUtil.cs 122-125). No per-cell array to update | same |
| World bounds and `WorldDetail` | Unchanged (`WidthInCells/HeightInCells`, 130-131) | same |
| `Grid.*` mirrors, `BackwallManager` | Not used for saving; the sim is rebuilt from `simData` at 21 | Updated only by flush frames. `Sim.Save` reads the native state. Whether the native backwall data needs the `BackwallManager` round trip is a gap |

### Which cells the pass may change

- **Only cells the border pass placed.** Capture `borderCells` in a `DrawWorldBorder` postfix (exact: 1382, 1395, 1418, 1431). Clear the capture at the start of each world, for example in a `RenderOffline` prefix, because worlds with `DrawWorldBorder: false` never call it.
- **Remove:** cells in `borderCells` whose depth from their edge is ≥ flat.
- **Fill:** cells at depth < flat that are not in `borderCells`. Exclude:
  - cells the vacuum rule would skip (space-tagged rows or columns with vacuum or void), and
  - template cells (P2: the stamped set; P1: they cannot be touched anyway).
- **Never touch** natural neutronium (MagmaBed bands, override noise; Q2) or template neutronium. Natural neutronium is never in `borderCells`.
- Corners: a cell in two edges' strips counts as border if either edge placed it. Measure depth per edge and remove a cell only if it is beyond the flat line for every edge that wrote it.

### Keeping the RNG untouched

- The pass draws no random numbers.
- It runs after `DetermineTemplatesForWorld` (486) and does not touch `POIBounds`, so `myRandom` and every template, geyser, POI and story-trait placement stay identical (also across asteroids, Cluster.cs 307-332).
- The border's own `SeededRandom(0)` stays untouched, because `DrawWorldBorder` runs as vanilla.
- The only remaining RNG effect is mob and plant placement under P1. P2 avoids it by leaving `simData` vanilla.

### What removed cells become

| Fill | P1 | P2 and F | Effect |
|---|---|---|---|
| **Pre-border biome content** (snapshot of the strip, cells, diseases and backwalls, in a `DrawWorldBorder` prefix: the biome output `DrawWorldBorder` overwrites at 1383 and elsewhere) | natural; settles | available in P2, not in F | What the world would have had; may be liquid or gas |
| Copy of the nearest inward non-border cell | possible | possible | Visually continuous; can remove a floor under a plant or critter if that cell is gas |
| Nearest inward **solid** element, or the subworld's border-type element | possible | possible | Always a floor; looks like a crust |
| Vacuum | | | Vacuum pockets in non-space biomes. Not advisable (outside space zones) |

### F, the first-load fallback (for comparison)

- **Detection:** `WorldGenSpawner.OnSpawn` runs the reveal and template placement only while the serialized `hasPlacedTemplates` is false, then sets it true (WorldGenSpawner.cs 162-163, 207-215). That is exactly the first load of a freshly generated cluster, base game and Spaced Out alike.
  - All asteroids' cells are in the sim at that point.
  - Spawnables on undiscovered asteroids wait until discovery (`TrySpawn` checks `world.IsDiscovered`, 89-90), so their critters and geysers spawn later into the already-changed cells.
  - Old saves have the flag true, so F never fires on them.
- **API:** `SimMessages.ModifyCell` (1245), `ReplaceElement` or `ReplaceAndDisplaceElement` (1387, 1398). The cost is one message per cell, about 8k cells on a default world, which is trivial.
- **Drawbacks compared with P1 and P2:**
  - `borderCells` is not saved, so F must replay the walk per `WorldContainer`, with T and R from `SettingsCache` via `worldName`, and infer the vacuum rule.
  - The pre-border biome content is lost, so the fill is a guess.
  - It runs in the live game rather than at generation, which the user ruled out as the main approach.

### Removing the mod later (all variants)

The reshaped border is ordinary cell data in the save. The mod stores nothing of its own, so removing it leaves the world as written, and nothing breaks or reverts. Adding the mod to an existing save does nothing: P1 and P2 never run again, and F is guarded by `hasPlacedTemplates`.

## Q6. What the flat thickness could be

| World group | T, R | Vanilla min T-R | T | Vanilla max T+R | Reproduced average bottom thickness |
|---|---|---|---|---|---|
| Default (all base-game, classic, 160x274 SO start) | 10, 6 | **4** | 10 | 16 | 4.6-4.8 (left edge 5.5-6.0) |
| SO minis and landing sites | 5, 2 | 3 | 5 | 7 | 3.8 |
| Mixing and medium moonlets | 4, 2 | 2 | 4 | 6 | 3.4 |
| Medium and small worlds | 3, 1 | 2 | 3 | 4 | 2.6 |
| Small moonlets | 2, 1 | 1 | 2 | 3 | 1.7 |
| Tiny worlds | 1, 0 | 1 | 1 | 1 | already flat |

- **T-R is the natural choice.** It is the floor the walk sits on most of the time: 157 of 256 bottom columns, and 226 of 384 right-edge rows on default worlds. It is closest to what players see, keeps every vanilla-guaranteed cell of neutronium (the walk never goes below it), and takes no playable space.
- **T** is 2.5x thicker than usual on default worlds.
- **A fixed number,** for example "4 everywhere", ignores per-world tuning. Moonlets would get thicker borders than vanilla ever gives them.
- **TinyMagma** (T=0) has no border; any formula must leave it at 0.

## Q7. Repo tooling for checking results

- **TestRunner worldgen tests** (`common/tools/TestRunner/Worldgen.cs`, README.md 117-130; store records `blk-87272da04dee`, `blk-c8a18ea4088d`):
  - `[OniTest(Worldgen = "SNDST-C-<seed>-0-0-0")]` (Spaced Out) or `"SNDST-A-…"` (base game; the mode letter is the fifth part from the end) generates the world with the run's mods active, embarks and runs the test in it.
  - `t.Generated` gives the coordinate, cluster and worldgen errors.
  - Use `Grid.Reveal` before `t.Screenshot` and `t.LookAt(cell, size)`.
  - Example: `mods/natural-backwalls-everywhere/src/NaturalBackwalls.Tests/Generated.cs`. It walks `WorldContainer.WorldOffset/WorldSize` cells (26-35, 82-87) and reads `Grid.Element`.
  - **One Spaced Out cluster covers several (T, R) groups in one generation,** for example a mini, moonlets and the start world.
- **How a test could assert flatness.** For each `WorldContainer` in `ClusterManager.Instance.WorldContainers`, skipping `IsModuleInterior`:
  1. Get the expected thickness from `SettingsCache.worlds.GetWorldData(world.worldName)` overrides with `SettingsCache.defaults` as fallback (`worldName` = `Settings.world.filePath`, WorldContainer.cs ~808).
  2. For every column (bottom and top) and every row (left and right), cells at depth < expected must be Unobtanium. Skip cells in a Space zone: the vacuum skip leaves them open, and gas may have drifted in since worldgen.
  3. The cell at depth == expected should not be Unobtanium. Natural neutronium can sit there (Q2: MagmaBed bands, override noise, templates), so report those columns with a screenshot rather than failing on them blindly. Another option is to fail only if the run-length profile changes by more than 0 between neighbouring columns on stretches with no biome neutronium.
  4. To prove the test can tell flat from vanilla, compare the profile with the vanilla one replayed from SeededRandom(0) (H, W, T, R), which must differ on default worlds.
  5. Because the border is identical for every seed of a world, one seed per world size is enough for shape. Template and generation-failure effects need several seeds.
- **WorldgenDriver** (`common/tools/WorldgenDriver`; store `tools/WorldgenDriver:domains/entities.md`):
  - `./Invoke-Worldgen.ps1 <coords> -Mods <mod>` generates many coordinates unattended and reports `complete/failed/hang/crash` per coordinate, plus `Player.log`.
  - It does not inspect cells.
  - It is the right tool for checking that a change to POIBounds or thickness causes no worldgen failures across every cluster: start-template overlap and unplaceable guaranteed templates. The `runs/coverage-*.txt` lists are gitignored and are not in this checkout.
- **@onimaxxing/worldgen-node** (WASM emulation; memory note) could show vanilla borders offline. It is unverified and cannot run the mod's patch, so it is not proof.

## Gaps

1. **The border reproduction is unverified.** It matches the code as read, and the screenshot's plateau-then-mound shape, but no world has been checked cell by cell. Confirm with one in-game worldgen test that reads the bottom row profile of a default world and compares it with `border_sim.py 256 384 10 6 x`.
2. **The world in before-border.png is unknown,** so I cannot say exactly which x range it shows. The match is qualitative (shape, step asymmetry, size).
3. **The inlining risk for `GetIntSetting` (option C) is unverified.** I know the IL is 20 bytes, but not whether Unity's Mono, with its inline limit and the `ldftn`/`newobj` inside, inlines it. Only an in-game check would settle it.
4. **The settle sim is native.** I could not read SimDLL, so I don't know whether different edge cells shift the sim's RNG elsewhere in the world.
5. **`overridePlacement` templates near edges:** I did not check whether any of the 69 uses sits within 16 cells of an edge on a default world. The behaviour matches vanilla, but a thinner border would expose those cells differently.
6. **Rocket interiors** (`SpaceshipInterior.yaml`, T=1 R=0) are not created by `Cluster.Generate`. I did not trace how they are made at runtime. They are already flat in any case.
7. **P2 flush and backwalls.** I have not verified that `ModifyCell` and `SetBackwallData` sent after tick 498 (or in a `SaveSim` prefix with one flush frame) all land in `Sim.Save`'s output. I also have not checked whether the native backwall data needs a `BackwallManager` round trip (WorldGenSimUtil.cs 90; Natural Backwalls already transpiles that call). An in-game worldgen test should check a few restored cells.
8. **Fallback F timing.** I did not check whether sim messages sent in the `WorldGenSpawner.OnSpawn` prefix are applied before the first frame the player sees, or whether `DoReveal` and `PlaceTemplates` (same method) react to the changed cells.
9. **Other mods' YAML** (custom worlds with their own T and R) were not surveyed. Options A, C and C′ derive from whatever T and R a world declares.

## Report back on the packet

- **Pointers that did not apply.** All 28 pointers in the `C:/dev/ONI-Mods` brief: 17 copies of one try/catch record across mod nodes, plus comment, braces and indentation records. They govern writing code, which this unit does not do.
- **Records that govern this work but were missing from the brief:**
  - `testing/integration-testing.md#generated-worlds` (`blk-87272da04dee`) and `testing/unit-testing.md#…per-test-options-on-onitest` (`blk-c8a18ea4088d`), reached only through `--tag generated-worlds`.
  - The node `mods/natural-backwalls-everywhere` worldgen patches, relevant as a neighbouring patcher.
- **Pointers whose one-line reason did not match the record:** none. The WorldgenDriver entities record matched its line.

## ICON tooling friction

None worth recording. One minor point: the path brief over `mods` spent its whole budget on 17 identical copies of one standards record, one per mod node, and pushed out the tag-reachable testing records that actually applied.
