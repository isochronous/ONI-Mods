# Smooth Borders

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod that draws the neutronium wall around every asteroid flat: one thickness along each whole edge, with no spikes, notches or stepped mounds.

## What it does

`WorldGen.DrawWorldBorder` reads two values from the world's settings, `WorldBorderThickness` (T) and `WorldBorderRange` (R), and walks each edge column by column (or row by row), stepping the depth by -2 to +1 each time and keeping it between T-R and T+R. That walk is what makes the jagged outline. The mod leaves the method as the game wrote it and only answers those two reads differently: the configured thickness for T, and 0 for R. Each edge then comes out at one depth from end to end.

- **A setting per kind of asteroid.** The game gives each kind of asteroid its own (T, R), so the options screen has one setting per kind, with the game's thinnest wall (T-R) as the default:

  | Setting | Range | Default |
  |---|---|---|
  | Primary asteroids | 1-16 | 4 |
  | Small asteroids | 1-7 | 3 |
  | Moonlets | 1-6 | 2 |
  | Tiny moonlets | 1-3 | 1 |

  The upper end of each range is the game's thickest wall for that kind (T+R). A world belongs to the kind whose (T, R) it has; kinds whose thinnest wall is the same share a setting.
- **Same layout at the defaults.** The border also adds one rectangle per edge, as deep as the deepest cell it drew, and points of interest, geysers and story traits are kept out of those rectangles. The mod runs the game's own walk on a copy of the cells to get the game's rectangles, and keeps the larger of the game's and the flat one on each edge. At or below the game's thickness the rectangles are the game's, so a seed places its points of interest and geysers where it does without the mod. A thicker wall gets a deeper rectangle, so nothing is placed inside the extra rows, and the layout changes.
- **New worlds only.** The border exists only in world generation. Existing colonies keep theirs, and the mod can be added or removed at any time.
- **Open edges stay open.** Where the game skips the wall over vacuum terrain (`RemoveWorldBorderOverVacuum`), it still skips it, and worlds with no wall (T of 0) are left alone.
- **Fail-safe.** The transpiler finds each of the two reads by its key and replaces it only when each key appears exactly once. If a game update changes the method, the mod logs `DrawWorldBorder has changed; borders stay as the game draws them` and the game draws its usual border. If the probe of the game's rectangles fails for a world, that world keeps the game's border and the log says why.

## Other worldgen mods

- Worlds from other mods follow the setting for the kind whose (T, R) they have. A world with a (T, R) no game asteroid uses gets a flat wall at its own thinnest, max(1, T-R), and has no setting of its own.
- Tested in the game with Empty Worlds, Baator, Omnificia Asteroid II, Voidria and MiniBase, each alone and all five together, in the base game and Spaced Out:
  - MiniBase builds its own asteroid and never calls `DrawWorldBorder`, so its walls are its own.
  - Voidria's asteroids have vacuum edges that the game leaves unwalled, and they stay unwalled.
  - Empty Worlds draws from the border's random before the game does; the mod copies that random at the point the game's walk starts, so its rectangles match the game's.
  - Baator's large primary uses the border values of a moonlet, so it follows the Moonlets setting.

## Installing

As a local mod:

1. Download `SmoothBorders-<version>.zip` from the [releases](https://github.com/isochronous/ONI-Mods/releases) (tags `smooth-borders-v<version>`).
2. Extract it into a new folder named `SmoothBorders` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\SmoothBorders`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/SmoothBorders`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/ONI-Mods.git
dotnet build ONI-Mods/mods/smooth-borders/src/SmoothBorders -c Release
```

PLib is merged into the DLL by the shared build, so the mod ships as a single file.

## Notes for modders

- `WorldGen_DrawWorldBorder_Patch.cs` patches `WorldGen.DrawWorldBorder` three ways:
  - a transpiler that swaps the two `GetIntSetting` calls for `Border.Setting`, which answers the configured thickness and range 0 while a world is drawn flat;
  - a prefix at `Priority.Last` that runs the original method (a reverse patch in `WorldGen_DrawWorldBorder_Vanilla.cs`) on a copy of the cells with a copy of the method's `SeededRandom`, and takes the four rectangles it adds back out of `POIBounds`;
  - a postfix that keeps, per edge, the deeper of the game's rectangle and the flat one.
- The flat state is per thread and cleared at the start and end of every call.
- Kinds and ranges are in `Groups.cs`, built from the (T, R) pairs in the game's worldgen files.
- Settings are PLib options (`Options.cs`), stored in `mods/config/SmoothBorders/config.json`.

## Tests

`src/SmoothBorders.Tests` runs in the game through the shared TestRunner (`common/tools/TestRunner`): the options dialog, flatness at the defaults and the maxima, the same placements as the game at the defaults, the fail-safe, and opt-in tests for more clusters and the worldgen mods above.
