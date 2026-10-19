# Unit 02 report: Smooth Borders design (settings, flat draw, POI bounds, tests)

Game build U59-744825, as in unit 01. "WorldGen.cs" means the decompiled `acs\ProcGenGame\WorldGen.cs` in the scratchpad `decomp\` folder. "DrawWorldBorder.il" means `scratchpad\DrawWorldBorder.il`, the method's IL taken from Assembly-CSharp with `ilspycmd -il`. World data comes from the world files under `StreamingAssets\...\worldgen\worlds\`, and asteroid names come from `acs\STRINGS\WORLDS.cs`.

---

## 1. Settings (for the player)

### What the options screen shows

The text block comes first. It is aligned top-left and sits above the fields:

> The game gives each kind of asteroid its own neutronium wall thickness: the primary asteroid gets the thickest wall, small asteroids and moonlets get thinner ones. Set the wall thickness in tiles for each kind below. Each default is the thinnest point of the game's own wall for that kind. Changes apply to newly generated worlds.

Four number fields follow, each with a slider:

| Label | Default | Asteroids it covers |
|---|---|---|
| **Primary asteroids (1-10)** | 4 | **Base game:** Terra, Arboria, Aridio, Verdante, Oceania, Oasisse, Rime, Volcanea, The Badlands, Helicona, Skewed Asteroid, Ceres, Blasted Ceres, Relica, RelicAAAAAAAGHH, Marinea. **Spaced Out classic clusters:** the starting asteroids (Terra, Arboria, Aridio, Verdante, Oceania, Oasisse, Rime, Volcanea, The Badlands, Squelchy, Ceres, Relica and Marinea Asteroid, Skewed Asteroid). **Spaced Out standard clusters:** the starting asteroids (Terrania, Folia, Quagmiris, Ceres Minor, Relica Minor, Marinea Minor). |
| **Small asteroids (1-5)** | 3 | The mini-cluster asteroids (The Desolands, Flipped, Frozen Forest, Metallic Swampy and Radioactive Ocean Asteroids, each as start, teleporter and plain version; Ceres Mantle, Ceres Core and Sweet Ceres Asteroids). The Irradiated Forest, Irradiated Marsh and Irradiated Swampy Asteroids (rocket destinations of standard clusters). The Rusty Oil Asteroid (Folia's teleporter destination). The Regolith Asteroid of classic clusters. |
| **Moonlets (1-4)** | 2 | Tundra, Water and Moo Asteroids. The Oily Swamp and Oily Sandy Swamp Asteroids (teleporter destinations of standard clusters). The classic clusters' second asteroids (Radioactive Swamp, Radioactive Forest, Radioactive Terra, Radioactive Terrabog, Glowood Wasteland, Stinko Swamp). The Ceres, Relica and Marinea Fragments (asteroid remix). |
| **Tiny moonlets (1-2)** | 1 | Marshy, Superconductive and Regolith Asteroids (the Regolith Asteroid of standard clusters). |

Each field's tooltip lists the asteroids in its row.

- **Defaults:** every default is the game's thinnest wall for that kind. The game's jagged wall sits at that thickness most of the time. At the defaults the flat wall keeps every tile the game guarantees and removes only the bumps.
- **Ranges:** every range runs from 1 tile up to the thickness the game's wall wanders around for that kind. That centre is 10 on the primary, the range the user gave.
- **Geysers, POIs and story traits:** at or below the defaults they land exactly where the same seed puts them without the mod. Above the defaults they still do on every edge where the game's own wall was at least that thick. Critter and plant positions near the edge can differ at any setting.
- **Asteroids from other mods:** they get a flat wall at their own thinnest thickness.
- **Asteroids with no wall** (The Rock, Tiny Magma) keep none.

---

## 2. Technical design

### 2.1 Which group a world belongs to

**Mapping rule.** A world's group is decided at worldgen from that world's own (T, R): `WorldBorderThickness` and `WorldBorderRange`, read through `WorldGenSettings.GetIntSetting`. That method looks in the world's `defaultsOverrides` first, then in `worldgen/defaults.yaml` (WorldGenSettings.cs 207-222, 245-248). The mapping uses no world names, so it keeps working when Klei renames or adds worlds, and when another mod adds worlds with vanilla-style values.

| (T, R) | Vanilla thickness T-R .. T+R | Group | Worlds (file: in-game name) |
|---|---|---|---|
| 10, 6 (defaults.yaml 35-38) | 4 .. 16 | Primary | **Base game** `worldgen/worlds/`: SandstoneDefault (Terra), ForestDefault (Arboria), ForestHot (Aridio), ForestLush (Verdante), Oceania, Oasis (Oasisse), SandstoneFrozen (Rime), Volcanic (Volcanea), Badlands (The Badlands), Helicona, StrangeAsteroidKleiFest2023 (Skewed Asteroid). **dlc2/4/5 base-game and classic:** CeresBaseGame(Shattered)Asteroid, CeresClassic(Shattered)Asteroid (Ceres, Blasted Ceres, Ceres Asteroid), PrehistoricBaseGame/Classic/Shattered* (Relica, Relica Asteroid, RelicAAAAAAAGHH), AquaticBaseGame/ClassicAsteroid (Marinea, Marinea Asteroid). **Spaced Out classic 240x380:** VanillaArboria, VanillaAridio, VanillaBadlands, VanillaForestDefault (Verdante Asteroid), VanillaOasis, VanillaOceania, VanillaSandstoneDefault (Terra Asteroid), VanillaSandstoneFrozen (Rime Asteroid), VanillaSwampDefault (Squelchy Asteroid), VanillaVolcanic, StrangeAsteroidKleiFest2023Cluster. **Spaced Out 160x274:** TerraMoonlet (Terrania), ForestMoonlet (Folia), SwampMoonlet (Quagmiris), CeresSpacedOutAsteroid (Ceres Minor), PrehistoricSpacedOutAsteroid (Relica Minor), AquaticSpacedOutAsteroid (Marinea Minor). Moon_Barren (Barren Moon, 96x96) is in no cluster. |
| 5, 2 | 3 .. 7 | Small | Spaced Out 128x153: MiniBadlands(+Start, +Warp) (The Desolands), MiniFlipped(+Start, +Warp), MiniForestFrozen(+Start, +Warp), MiniMetallicSwampy(+Start, +Warp), MiniRadioactiveOcean(+Start, +Warp), IdealLandingSite (Irradiated Forest), MetalHeavyLandingSite (Irradiated Marsh), SwampyLandingSite (Irradiated Swampy); dlc2 MiniShatteredStart/Geo/WarpAsteroid (Ceres Mantle, Ceres Core, Sweet Ceres) |
| 4, 1 | 3 .. 5 | Small | OilRichWarpTarget (Rusty Oil, 128x153), MiniRegolithMoonlet (Regolith Asteroid, 96x96, in the 16 classic clusters) |
| 4, 2 | 2 .. 6 | Moonlets | TundraMoonlet, WaterMoonlet, MooMoonlet, WarpOilySwamp (Oily Swamp, 128x153), WarpOilySandySwamp (128x153), MixingCeres/Prehistoric/AquaticAsteroid (Fragments, used by `worldMixing/*MixingSettings.yaml`); OilyMoonlet (Viscosia) is in no cluster |
| 3, 1 | 2 .. 4 | Moonlets | MediumRadioactiveVanillaWarpPlanet (Radioactive Swamp), MediumSandyRadioactiveVanillaWarpPlanet (Radioactive Terra), MediumForestyRadioactiveVanillaWarpPlanet (Radioactive Forest), MediumSandySwamp (Radioactive Terrabog), MediumForestyWasteland (Glowood Wasteland), MediumSwampy (Stinko Swamp), all 160x176; SmallRadioactiveLandingSite and SmallWarpOilySwamp (96x128) are in no cluster |
| 2, 1 | 1 .. 3 | Tiny moonlets | MarshyMoonlet, NiobiumMoonlet (Superconductive), RegolithMoonlet (Regolith Asteroid, 160x96, in the 12 standard and mini clusters) |
| 1, 0 | 1 | Tiny moonlets | TinyStart, TinySurface (dev clusters TNY-C and TinySurface); TinyForest, TinyIce, TwinMoonlet, SulfurMoonlet (Brimstonia) in no cluster; force flag on: BigEmpty, TinyEmpty (both "FOR DEVS") |
| 0, 0 | none | none | TinyMagma: T=0 gives thickness 0 (no border) |
| — | none | none | TheRock: `DrawWorldBorder: false`, so `DrawWorldBorder` is never called (WorldGen.cs 943) |

**Other cases:**
- **Unknown pair** (a modded world with its own T and R): the world is flattened at its own floor, `max(1, T-R)`, and no setting applies to it. With T ≤ 0 it gets 0. This gives the vanilla-faithful thickness without guessing which group the author meant.
- **Rocket interiors** (SpaceshipInterior, 1, 0) never pass through `DrawWorldBorder`; its only caller is WorldGen.cs 946.

**Asteroids that share a name across groups:**
- "Regolith Asteroid" is MiniRegolithMoonlet (4,1, Small) in classic clusters and RegolithMoonlet (2,1, Tiny) in standard clusters.
- "Irradiated Forest Asteroid" is IdealLandingSite (5,2) and the unused SmallRadioactiveLandingSite (3,1).
- "Oily Swamp Asteroid" is WarpOilySwamp (4,2) and the unused SmallWarpOilySwamp (3,1).

The tooltips name the cluster type where it matters.

### 2.2 Groups, defaults and ranges

**Merging.** Groups are merged only when they have the same vanilla floor (T-R). The default is that floor, so a merged group's default is exactly vanilla for every world in it. Merging therefore costs no fidelity at the default.
- (5,2) and (4,1) both have floor 3 and form Small.
- (4,2) and (3,1) both have floor 2 and form Moonlets.
- (2,1) and (1,0) both have floor 1 and form Tiny moonlets.

Floors 4, 3, 2 and 1 are distinct, so four is the smallest number of groups with vanilla defaults. The user's three words (primary, small asteroids, moonlets) map to the first three, plus Tiny moonlets.

**Range principle: 1 to T.** T is the thickness the vanilla walk is centred on. For a merged group it is the larger T in the group.
- **Minimum 1.** Every asteroid that has a border keeps at least one tile of it. 0 would remove the border; Klei does that only on a developer world (TinyMagma).
- **Maximum T.** It matches the user's 1-10 for the primary. It also stays within the vanilla walk on every edge: the reproduced walk reaches at least T on every edge of every world size (border_sim.py: 256x384 left/right/bottom/top maxima 15/11/10/12; 240x380 15/11/10/11; 160x274 15/11/10/13; 128x153 at 5,2: 7/6/7/7; at 4,1: 5/5/5/5; 160x176 at 3,1: 4/4/4/4; 64x96 at 2,1: 3/3/3/3). So at the top of its range a group's flat border is never thicker than the thickest wall vanilla draws somewhere on that edge (the reproduction is unverified in game; unit 01 gap 1).
- **Ranges:** Primary 1-10, Small 1-5, Moonlets 1-4, Tiny moonlets 1-2.
- **Exception:** Tiny moonlets at 2 is thicker than anything vanilla gives the (1,0) dev worlds.

**Rejected principle: 1 to T+R** (vanilla's thickest point). It gives Primary 1-16, which contradicts the user's 1-10.

### 2.3 The flat draw: swap the two setting reads inside `DrawWorldBorder`

**The method.** Every edge's depth is the loop bound `intSetting + num`, where `num` is the walk offset clamped to [-R, R] (WorldGen.cs 1375-1377, 1387-1389, 1411-1413, 1423-1425). With R = 0 the clamp `Max(-0, Min(num + step, 0))` is always 0, so every row and column is exactly `intSetting` cells deep. T and R are read once at the top of the method:

```
DrawWorldBorder.il 67-73 (WorldGen.cs 1357-1358):
IL_0024: ldstr "WorldBorderThickness"
IL_0029: callvirt instance int32 ProcGen.WorldGenSettings::GetIntSetting(string)
IL_0035: ldstr "WorldBorderRange"
IL_003a: callvirt instance int32 ProcGen.WorldGenSettings::GetIntSetting(string)
```

**The lever.** A transpiler on `WorldGen.DrawWorldBorder` (private; `[HarmonyPatch(typeof(WorldGen), "DrawWorldBorder")]`) replaces each of those two `callvirt`s, identified by the `ldstr` before it, with `call int SmoothBorders.Border.Setting(WorldGenSettings, string)`. The stack shape is the same (instance and string in, int out), so nothing else in the method changes. The helper:
- reads the real T and R through `settings.GetIntSetting`. The method itself is not patched, so there is no recursion.
- for `"WorldBorderThickness"` returns the configured thickness F for the world's group (2.1), or the world's own floor for an unknown pair, or 0 when T ≤ 0.
- for `"WorldBorderRange"` returns 0.
- returns the vanilla value for both names when this world's draw is not flat (the probe failed, see 2.7).

**What stays vanilla.**
- The vacuum skip over space-tagged terrain (1370, 1376, 1380 and the three twins) and the `DrawWorldBorderForce` flag (1356) are untouched.
- The progress callbacks are untouched.
- `borderCells` is untouched, so the finalisation at RenderOffline 476-481 (re-stamp, vacuum backwall, `claimedPOICells`) applies to the flat set.
- Cells deeper than F are never overwritten, so they keep the biome content and backwalls (Natural Backwalls' `ApplyBackground` included) without any snapshot.
- The four `RandomRange` draws still happen. They come from the private `SeededRandom(0)` (WorldGen.cs 945), which nothing else reads (unit 01 Q3).

**Worlds with no border stay without one.**
- With `DrawWorldBorder: false` the method is not called (943).
- With T = 0 the helper returns 0 and the loops draw nothing.

**Settings are read per world.** The prefix calls `Options.Load()` once per `DrawWorldBorder` call on the worldgen thread. Natural Backwalls does the same (`Patches.cs` 66). A changed setting therefore needs no restart, and a test can set options before a generation.

### 2.4 POI bounds: per edge, the larger of the vanilla and the flat rectangle

**How vanilla computes them.** After each pair of edges, vanilla adds one rectangle per edge to the `WorldGen.POIBounds` field (1401-1402, 1437-1438). Each rectangle is as deep as the deepest cell actually written on that edge (`num4`, `num5`, `num13`, `num14`, updated only inside the vacuum-skip condition). The rectangles therefore depend on the cell contents, not only on the walk. `POIBounds` is a new, empty list for each world (466) and nothing adds to it before the border, so the border's four rectangles come first.

**The rule.**
1. **Prefix.** Copy `simData` with `cells` cloned (`WorldgenSimData` is a struct of three arrays; `DrawWorldBorder` writes only `cells`, WorldgenSimData.cs). Run the **unpatched** method on the copy through a Harmony reverse patch (`[HarmonyReversePatch]`, the original IL), with a fresh `new SeededRandom(0)` as vanilla passes (945), a throwaway `borderCells`, and a no-op progress callback. This adds the exact vanilla rectangles to `POIBounds`. Take them out and keep them.
2. **Original (transpiled).** The flat draw adds its four rectangles.
3. **Postfix.** Replace each flat rectangle with the larger of it and its vanilla twin. All four are anchored to their edge and span its whole length, so "larger" is one number per edge:
   - left: the wider of the two
   - right: the smaller x
   - bottom: the taller of the two
   - top: the smaller y

   The list ends up with four rectangles in vanilla order.

**Why the larger, not vanilla's alone.** A thicker-than-vanilla border inside a vanilla rectangle would let a template sit over it. At settle tick 498 template cells are stamped with `ReplaceType.Replace` and no claimed-cell check (WorldGenSimUtil.cs 35-69, unit 01 Q4), which would punch holes in the border. Growing the rectangle where the flat border is deeper prevents that, just as vanilla's own rectangles do.

**When seed identity holds.** Template, geyser, POI and story-trait placement reads only `POIBounds`, the terrain cells' polygons and tags, and `myRandom`:
- the filters are tag based (TerrainCell.cs 1046-1058)
- the overlap test is rectangles only (TemplateSpawning.cs 401-410, 438-461)
- the shuffle is `myRandom` (TemplateSpawning.cs ~397)

None of these reads cell contents. So an asteroid gets **exactly vanilla placements whenever its four rectangles equal vanilla's**, which happens when, on each edge, the flat border's deepest drawn cell is no deeper than vanilla's.

- **Guaranteed at or below the group default (F ≤ T-R) on every asteroid.** Vanilla's thickness is at least T-R on every row and column, so every cell the flat draw writes is a cell vanilla writes too. The vacuum condition sees the same cell. A cell the flat draw finds non-vacuum because an earlier edge wrote it was also written by that edge in vanilla. The flat extents are therefore at most vanilla's.
- **Above the default:** it holds edge by edge while F is at most vanilla's deepest **drawn** cell on that edge. On sides and bottoms away from space that is the walk maximum (2.2). On tops, and on sides next to space, the vacuum skip leaves vanilla's band shallower, so the top rectangle can grow and placements on that asteroid can change.
- **Spaced Out:** story traits that do not fit on one asteroid carry on to the next (Cluster.cs 307-332), so cluster-wide identity needs every asteroid's rectangles unchanged.
- **Logging:** the mod logs one line per world, plus a line naming each edge whose rectangle grew. The tests and a player's Player.log both show when identity was given up.

**What differs at any setting.**
- **Edge cells.** The cells in the edge band and their settled liquids and gases differ.
- **Critter and plant placement.** Mob placement shuffles the candidate cells of each terrain cell with one shared RNG per world (MobSpawning.cs 109-174, WorldGen.cs 545-557), so a changed border can shift critters and plants across the whole asteroid (unit 01 Q3).
- **overridePlacement templates.** These templates skip the bounds (TemplateSpawning.cs 285-293). When F is below vanilla they can keep template objects that vanilla dropped on claimed border cells (GameSpawnData.cs 49, 59). Vanilla has the same exposure for its own border.

### 2.5 Options screen (PLib)

**Library.** The repo's mods use PLib Options 4.25.0, merged into each mod (`common/oni-mod.targets` 74-81, `<UsePLib>true`). Every options class has `[JsonObject(MemberSerialization.OptIn)]`, `[ConfigFile(SharedConfigLocation: true)]` and `Options.Load() => POptions.ReadSettings<Options>() ?? new Options()`. Examples: `mods/natural-backwalls-everywhere/src/NaturalBackwalls/Options.cs` 13-15 and 112-115, and `vent-freeze-fix`, `jumper-bridges` and `storage-pod-redux` `Options.cs`. Registration: `new POptions().RegisterOptions(this, typeof(Options))` after `PUtil.InitLibrary(false)` (NaturalBackwalls `Mod.cs` 14-16).

**PLib's text-only entry is centred, not top-left.** `TextBlockOptionsEntry`, used for a `LocText` property (OptionsHandlers.cs 146), lays its label out with `Alignment = TextAnchor.MiddleCenter` and the default PLabel text alignment, which is MiddleCenter (PTextComponent.cs 240):
- `incubator/plib/ONIMods/PLibOptions/TextBlockOptionsEntry.cs` 62-69
- the same in the 4.25.0 package (`ilspycmd` of `~/.nuget/packages/plib/4.25.0/lib/net48/PLib.dll`: `Alignment = (TextAnchor)4`)

Natural Backwalls' help blocks use it as is (Options.cs 96, 100), so they are centred.

**Smallest way to a top-left block:** a 15-line subclass in the mod, `TopLeftTextOptionsEntry : TextBlockOptionsEntry`.
- The class is public and `CreateUIEntry` is virtual (OptionsEntry 4.25.0).
- It overrides `CreateUIEntry` to add a `PLabel` with:
  - `TextAlignment = TextAnchor.UpperLeft`
  - `FlexSize = Vector2.right`, so it takes the panel's width and wraps there
  - a word-wrapping style (`PUITuning.Fonts.TextLightStyle.DeriveStyle()` with `enableWordWrapping = true`, as PLib's own static constructor does)
  - `GridComponentSpec(row, 0) { Alignment = TextAnchor.UpperLeft, ColumnSpan = 2, Margin = CONTROL_MARGIN }` (`CONTROL_MARGIN` is protected, so it is reachable)
- No PLib change is needed.

**Order: everything is yielded from `IOptions.CreateOptions()`.**
- Static `[Option]` entries are grouped by category in a `SortedList` (OptionsEntry.cs 68-78). Inside a category they keep reflection order, which the CLR does not promise.
- Dynamic entries are appended after them (OptionsDialog.cs 283-291).
- With no static `[Option]` properties at all, the dialog shows exactly the yield order. The category `""` gets no header and fills the panel (OptionsDialog.cs 205-208), so the text block is the first thing at the top-left.

The yield order:
1. `new TopLeftTextOptionsEntry("Intro", new OptionAttribute(OPTIONS.INTRO))`
2. for each group, `new IntOptionsEntry(nameof(Primary), new OptionAttribute(OPTIONS.PRIMARY.NAME + " (1-10)", OPTIONS.PRIMARY.TOOLTIP), new LimitAttribute(1, 10))`

`IntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)` is public in 4.25.0. With finite limits it shows a slider and clamps typed values. The properties `Primary`, `Small`, `Moonlets` and `TinyMoonlets` are `[JsonProperty] public int` with the defaults 4, 3, 2 and 1. This is the pattern Natural Backwalls already uses for its dynamic help blocks and its `LogCensus` checkbox (Options.cs 94-105).

**Other details:**
- **Ranges.** They live in one group table in code. The table supplies both the `LimitAttribute` and the label suffix " (min-max)", so the label cannot disagree with the limit.
- **Values read from disk.** `Options.Load()` clamps each value to its range, because a hand-edited `config.json` bypasses the dialog.
- **Restart.** None needed: settings are read at each worldgen (2.3), so there is no `[RestartRequired]`.
- **Screenshot check.** The 4.25.0 layout of a flexing, wrapping PLabel in a two-column grid is not verified. A NoColony screenshot test checks it (section 4).

### 2.6 Strings

The strings follow the repo pattern:
- **ModStrings tree.** A `ModStrings` class tree holds `LocString` fields and is registered with `Translations.Register(harmony, typeof(ModStrings))` (`common/src/Translations.cs` 45; NaturalBackwalls `Mod.cs` 15). `ModStrings.SMOOTHBORDERS.X` is the key `STRINGS.SMOOTHBORDERS.X`.
- **Translation files.** They go in `translations/`; `SmoothBorders.pot` is written by `common/tools/Translations/translations.py`, which runs StringsTemplate for every csproj with `<UseTranslations>true</UseTranslations>` (store record blk-e125ed2b68c6).

Keys:
- `SMOOTHBORDERS.OPTIONS.INTRO`: the text block in section 1
- `SMOOTHBORDERS.OPTIONS.PRIMARY.NAME = "Primary asteroids"`, `.TOOLTIP` = that row's asteroid list plus "Game default: 4."
- `SMALL` ("Small asteroids"), `MOONLETS` ("Moonlets") and `TINY_MOONLETS` ("Tiny moonlets"), each with `.NAME` and `.TOOLTIP` in the same form

How the strings reach the dialog:
- **Range suffix.** It is appended in code, not inside the translatable text, so translators never handle placeholders.
- **Compile-checked fields.** The dialog reads these fields directly (dynamic entries), so a missing key is a compile error. StringsTemplate's `[Option]`-key check has nothing to check here.
- **Language.** Text is read when the dialog is built, so the current language shows, as in Natural Backwalls (Options.cs 91-93).

Player-visible text follows the no-hedging rule: it states what the setting does.

### 2.7 Failure handling and other mods

The repo standard applies: a worldgen patch catches, logs `"[SmoothBorders] <what was skipped>: " + e.Message`, and leaves vanilla behaviour (store record blk-f04d3902a855, case 3).

- **Transpiler.** It requires exactly one match for each of the two `ldstr`+`GetIntSetting` pairs. Any other count returns the IL unchanged, logs "DrawWorldBorder has changed; borders stay as the game draws them", and clears a static `Patched` flag. With the flag cleared, the prefix and postfix do nothing.
- **Probe failure.** If the vanilla probe throws in the prefix, the world is drawn vanilla: a `[ThreadStatic]` "flat this world" flag stays false, and the helper then returns the real T and R. The draw is never flat without vanilla bounds to merge with, so the mod can never produce a flat border with unsafe bounds.
- **Postfix guard.** The postfix merges only if exactly four rectangles were added since the prefix. Otherwise it logs and leaves the list alone, which covers another mod's prefix skipping the original.
- **Other mods.** The two-call swap composes with other transpilers that leave those calls in place. A reverse patch always gives the original IL, so the vanilla bounds stay vanilla even when another mod also patches the method. Worlds that other mods add or retune are handled by the (T, R) mapping (2.1).
- **Thread.** All of this runs on the worldgen thread (unit 01 Q5) and touches no Unity objects.

---

## 3. Alternatives considered and rejected

| Alternative | Rejected because |
|---|---|
| Unit 01's A: the four `RandomRange` calls (DrawWorldBorder.il 153, 243, 430, 520) return a constant | The clamp (WorldGen.cs 1375 etc.) only reaches T-R, T or T+R, so most configured values (primary 1-3, 5-9) are impossible |
| Transpiling the four loop bounds `intSetting + num` | Four sites matched by local slot numbers (`ldloc.2`, `ldloc.s 9` …) instead of two by string constant; more fragile, same result |
| B: prefix that returns false and redraws flat | Copies about 85 lines including the vacuum lookup; drifts silently when Klei edits the method; disables other mods' patches of it |
| C: postfix on `WorldGenSettings.GetIntSetting` | The 20-byte method may be inlined by Mono (unit 01 gap 3) and the postfix would then never run; it also runs for every setting read in the game |
| C′: rewrite the settings data after load | Mutates shared caches (`GetSetting` caches parsed values, WorldGenSettings.cs 195-203) and must chase world files other mods load later |
| D, P1, P2, F | Rejected by the user in favour of A (plan.md Decisions) |
| POIBounds = vanilla only (A′ as first stated) | A configured border thicker than vanilla's band could be stamped over by a template at tick 498 (WorldGenSimUtil.cs 35-69) |
| POIBounds = flat only (unit 01's A) | Changes every seed's geysers and POIs even at the default, because vanilla's rectangles are as deep as the walk's maximum (e.g. 15/11/10/12 on 256x384) while the flat default is 4 |
| Vanilla bounds by replaying the walk in mod code | About 40 copied lines including the vacuum condition and the cross-edge interplay at corners; the reverse patch runs Klei's own IL instead |
| Appending the flat rectangles next to vanilla's (8 rectangles) | Same overlap result, but the list differs from vanilla's (`RemoveTemplate` matches rectangles by centre, TemplateSpawning.cs 179); the per-edge merge keeps the list vanilla when nothing grew |
| Static `[Option]` properties with a `LocText` text block | Order inside a category is reflection order, and the stock text block is centred |
| Six groups, one per (T, R) | Same defaults as four groups, but two more fields whose only difference is the slider's top end |
| Three groups (primary, small, moonlets) | Moonlet floors 2 and 1 cannot share a vanilla default; one of them would change at default |
| Range 1 to T+R | Primary 1-16 contradicts the user's 1-10 |
| Allowing 0 | Removes the border; only a developer world does that in vanilla |

---

## 4. Tests

**Coordinates.**
- `V-SNDST-C-<seed>-0-0-0` (classic Terra) contains all four groups:
  - VanillaSandstoneDefault (Primary)
  - MiniRegolithMoonlet (Small, 4,1)
  - MediumRadioactiveVanillaWarpPlanet (Moonlets, 3,1), plus Tundra, Moo and Water (4,2)
  - Marshy and Niobium (Tiny, 2,1)
- `SNDST-C-<seed>-0-0-0` (Terrania) contains:
  - TerraMoonlet (Primary)
  - IdealLandingSite (Small, 5,2)
  - WarpOilySwamp and the 4,2 moonlets (Moonlets)
  - Marshy, Niobium and RegolithMoonlet (Tiny)
- `SNDST-A-<seed>-0-0-0` (base-game Terra) contains Primary only.

All prefixes come from the cluster files' `coordinatePrefix`.

**Harness additions** (TestRunner; harness changes are welcome per the repo rule):
1. `[OniTest(Worldgen = "...", Prepare = nameof(M))]` runs a static method at the main menu before the coordinate is applied. Tests with different `Prepare` never share a generated world.
2. The run snapshots the deployed mods' shared config folders (`mods/config/<staticID>`) at start and restores them at the end. Without this, a test's options would replace the player's options and change the loadout key (Invoke-OniTests.ps1 274-293).

**Mod-side test switch.** The vanilla baseline needs the mod's draw off in a run where the mod is loaded, since the test assembly references the mod. So `Border.Active` is a public static bool (default true) that only tests set from `Prepare`. It is not an option.

| # | Test | Kind | Proves |
|---|---|---|---|
| 1 | Options dialog: the text block is first and top-left; four fields with labels "Primary asteroids (1-10)", "Small asteroids (1-5)", "Moonlets (1-4)", "Tiny moonlets (1-2)" and defaults 4/3/2/1; screenshot | `NoColony` (Natural Backwalls' `Looks.cs` precedent) | Labels show ranges; layout as the user asked |
| 2 | Mapping: every world in `SettingsCache.worlds` maps per the 2.1 table; unknown pair (12,3) → 9; (0,0) → 0; out-of-range config values are clamped on load | `NoColony` | Groups and the borderless rule (no cluster in the game uses TinyMagma or TheRock, so borderless is checked at the helper) |
| 3 | Flat at defaults on `V-SNDST-C`, `SNDST-C`, `SNDST-A` (see below) | `Worldgen` | Flatness per edge per group at the defaults |
| 4 | The same at non-defaults: `Prepare` sets Primary 7, Small 5, Moonlets 1, Tiny moonlets 2 on `SNDST-C` | `Worldgen` | Arbitrary values in both directions; no template holes when thicker (depth < F is all neutronium) |
| 5 | Vanilla identity at defaults: `Prepare` turns `Border.Active` off and records the `WorldGenSpawner` spawnables of type Building plus geyser prefabs (id, cell) per world to the session folder; a second test with the same coordinate at defaults records the same and compares; also compares the log's "rectangle grew" lines (must be none) | `Worldgen`, one seed per cluster in the default run, two more seeds `Explicit` | Geysers, POI buildings and story-trait buildings identical to a vanilla seed |
| 6 | WorldgenDriver sweep: every cluster coordinate prefix, `--worldgen-stories=all --worldgen-mixing=all`, once with defaults and once with every group at its maximum, compared with a vanilla sweep | `Invoke-Worldgen.ps1 -Mods SmoothBorders` | No new worldgen failures (start-template overlap, TemplateSpawning.cs 75-82; unplaceable guaranteed templates) when rectangles grow |

**How test 3 checks flatness.**
- Walk every `WorldContainer` that is not a module interior.
- Get F from the mod's own mapping, fed with the world's settings (`SettingsCache.worlds.GetWorldData(world.worldName)`).
- For every row (left and right) and column (bottom and top):
  - every cell at depth < F is Unobtanium, except cells in a Space zone (the vacuum skip)
  - count the cells at depth F that are Unobtanium
- The count must be zero outside natural neutronium. Unit 01 Q2: MagmaBed bands, override noise and templates can put neutronium there. So fail on a count above a small threshold, and photograph each one.
- The discriminating check: vanilla's bottom edge on 256x384 has 99 of 256 columns thicker than 4 (border_sim.py), and the flat border has none from the walk. This also confirms unit 01's reproduction once.

---

## 5. Scaffold notes

Naming follows Ovagros Overhead (OM-006 plan.md Decisions, lines 37-41: kebab-case folder, PascalCase assembly, namespace and projects, `Isochronous.<Pascal>` staticID, first version 0.1.0, tag `<folder>-v<ver>`).

**Names:**
- Folder `mods/smooth-borders`
- Assembly, namespace and project `SmoothBorders` (`src/SmoothBorders/SmoothBorders.csproj`); tests `src/SmoothBorders.Tests/SmoothBorders.Tests.csproj`
- `mod.yaml`: `title: "Smooth Borders"`, `staticID: "Isochronous.SmoothBorders"`, plus a one-line description, e.g. "Draws the neutronium world border flat, at a set thickness per kind of asteroid. New worlds only."
- `mod_info.yaml`: `supportedContent: ALL` (base game and Spaced Out; no `requiredDlcIds`), `minimumSupportedBuild: 744825`, `version: 0.1.0`, `APIVersion: 2`
- Tag `smooth-borders-v0.1.0`

**csproj properties.** Natural Backwalls' are the model:
- `<UsePLib>true</UsePLib>` and `<UseTranslations>true</UseTranslations>`
- `<ReferenceUnityUI>true</ReferenceUnityUI>` for `TextAnchor` and PLib UI types
- the `Newtonsoft.Json` reference with `Private=false`
- no worldgen folder

**Mod class.** `SmoothBordersMod : UserMod2`:
- `base.OnLoad(harmony)`
- `PUtil.InitLibrary(false)`
- `Translations.Register(harmony, typeof(ModStrings))`
- `new POptions().RegisterOptions(this, typeof(Options))`
- a "Loaded version" log line, as in both reference mods

**Patch class names** follow `<Type>_<Method>_Patch`, e.g. `WorldGen_DrawWorldBorder_Patch` (transpiler, prefix, postfix). The reverse-patch stub goes in its own class, e.g. `WorldGen_DrawWorldBorder_Vanilla`.

**Target.** It stays net48 (OM-006 decision, plan.md line 38). The CS8702 default-interface note concerns entity configs, which this mod has none of.

**Repo entries,** as for Ovagros Overhead:
- `ONI-Mods.sln` solution folder plus the two projects
- `README.md` mod table row
- `.github/ISSUE_TEMPLATE/bug_report.yml` and `feature_request.yml` option "Smooth Borders"
- `.github/workflows/label-mod.yml` map `"Smooth Borders": "smooth-borders"`
- store node through icon-integrate, then `mods/.context/projects.md` row; the decision record lands there through adr-discipline (OM-006 plan.md line 40)

**Translations.** Run `translations.py` after the strings exist, to write `translations/SmoothBorders.pot`.

---

## 6. Open questions for the user

1. **Four groups and their names.** I recommend Primary asteroids / Small asteroids / Moonlets / Tiny moonlets, merged by equal vanilla floor. Alternatives:
   - six groups, one per vanilla (T, R)
   - your three words, at the cost of one group's default not matching vanilla
2. **"Moonlets" holds some bigger asteroids.** The classic clusters' 160x176 radioactive second asteroids and the Oily Swamp teleporter asteroids are in it, because vanilla gives them the moonlet thickness (floor 2). They are larger than the "Small asteroids" (128x153, floor 3). Keep that grouping and naming, or rename the groups by thickness rather than size?
3. **Ranges 1 to T** (Primary 1-10, Small 1-5, Moonlets 1-4, Tiny moonlets 1-2), or 1 to T+R (vanilla's thickest: 1-16, 1-7, 1-6, 1-3)?
4. **POI bounds refinement.** Your choice was vanilla POI bounds. The design keeps them vanilla whenever the flat border fits inside them, and enlarges an edge's rectangle only where the flat border is deeper, so no POI can be stamped over the border. Geysers then match a vanilla seed at or below the defaults, and above them on every edge where vanilla's wall was at least as thick. Is that the rule you want?
5. **Asteroids from other mods** with their own wall values: flat at their own thinnest (recommended), or follow the nearest group's setting, or left jagged?
6. **Natural neutronium touching the border** (the magma core's neutronium band, unit 01 Q2) stays. It is not part of the border walk. Keep it?
7. **Base game.** All base-game asteroids are Primary. Show all four fields in both modes (recommended, one shared config), or only the Primary field when Spaced Out is off?
8. **The text block wording** in section 1, and the tooltip wording ("Game default: 4.").

---

## 7. Report back on the packet

- **Pointers that did not apply:** the comment, braces, indentation, harmony-patches and key-code-paths records, which govern writing code (this unit wrote none), and 16 of the 17 copies of the try/catch record (one copy is enough).
- **Pointers that applied:**
  - the try/catch record (worldgen patches, case 3) and "Yielding to Other Mods", both read for 2.7
  - in `common`: StringsTemplate's overview (blk-e125ed2b68c6), WorldgenDriver's entities, TestRunner's entities and README
- **Missing from the brief, as unit 01 also found:** the `generated-worlds` testing records blk-87272da04dee and blk-c8a18ea4088d, reached by id from unit 01's report.
- **Reasons that did not match:** none.

## 8. ICON tooling friction

None in this unit. `ks read --store <store> <id>` resolved every id used.

The only observation is the one unit 01 recorded. The `mods` brief again spent its budget on 17 identical copies of one standards record (one per mod node), so the tag-reachable testing records stayed out of it.
