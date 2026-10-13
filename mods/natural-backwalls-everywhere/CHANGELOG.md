# Changelog

## Unreleased

- Chinese (Simplified) translation. Translations load from translations/<locale>.po; the template for other languages is translations/NaturalBackwalls.pot.

## 0.1.4 - 2026-10-11

- Feature rooms that reach past the world edge no longer log "Feature-room backwalls skipped for one cell: Index was outside the bounds of the array" (seen on Skewed Asteroid); the out-of-world points are skipped.
- Backwall phase changes during the settle sim no longer run the game's in-game code, which needs objects that do not exist before a game is loaded and would stop the worldgen thread without a message. The mod applies them itself during the settle (same result: a backwall past its melting point becomes the liquid in the cell) and logs how many it handled. This is a guard; whether it is the cause of the generation hang reported on Blasted Ceres, RelicAAAAAAAGHH, Skewed Asteroid and Flipped Asteroid is not confirmed.
- An error that escapes the worldgen thread now shows the game's normal "not all seeds germinate" message, with the exception in Player.log, instead of a loading screen that never finishes.

## 0.1.3 - 2026-10-09

- The mod's internal id is now Isochronous.NaturalBackwallsEverywhere. The game sees it as a new mod: enable it again in the Mods menu and check its settings.

## 0.1.2 - 2026-10-09

- Renamed to Natural Backwalls Everywhere.

## 0.1.1 - 2026-10-09

- Added a preview image, so the mod shows one in the game's mod list.

## 0.1.0 - 2026-10-09

- First version: natural backwall tiles in every biome, built from the Aquatic Planet Pack's own worldgen data (its backwall noise plus a per-biome element band). New worlds only.
- One options row per biome family: a material dropdown offering that biome's solids, and a coverage box (default 0.4, or 0.15 for the denser kelp forest pattern). The Aquatic Planet Pack's own biomes keep their vanilla values as defaults.
- A Patterns section with a dropdown per subworld zone: the pack's reef, kelp forest, abyss, and beach patterns ship with the mod, plus the base game's Strange.
- One starting-biome coverage (default 1) that applies to whatever biome the colony starts in.
- Backwalls on Spaced Out's other planetoids are optional and off by default.
- Feature rooms (carved caves, lakes, geodes), which the game's own pass leaves bare, get backwalls too; optional, on by default.
- Biomes from other worldgen mods get their most common solid (optional). Every patch is guarded, so a failure leaves worldgen untouched. Surface and space subworlds stay open.
- A per-zone backwall census is logged at every world load, and an options button exports the settings as a worldgen YAML overlay for external renderers.
