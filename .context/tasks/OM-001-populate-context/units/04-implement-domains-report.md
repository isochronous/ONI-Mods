# Unit 04 report — domains change-sets

All six files are in `.context/tasks/OM-001-populate-context/changesets/` as `domains-<mod>.json`. Each has 2 create ops (`domains/entities.md` with `applies_to: ["src/<Project>/**"]`, and `domains/glossary.md`). Nothing applied, nothing committed.

## Per mod

- motion-sensor-range: adds a 2-16 tile (even steps, default 4) range slider to the vanilla Duplicant Motion Sensor. entities covers `RangeSlider`, the three Harmony patches, saved range, rules (snapping, copy settings, construction carry-over), code paths and tests; glossary covers the sensor, pickupRange, scene partitioner, Extents, RangeVisualizer, IIntSliderControl, copy settings.
- multichannel-critter-sensor: new room critter/egg counter with a 4-bit ribbon output. entities covers the config, component (saved fields, bit layout), `ThresholdAdapter`, the side screen, 3 patches, rules (pulses, modes, visuals) and tests; glossary covers ribbon, logic tick, Sim200ms, thresholds, modes, prefab tags, Multiplexing.
- natural-backwalls-everywhere: worldgen pass adding backwalls to every biome. entities covers `CloneInToNewWorld_Patch`/`Plan`, `Options`, `BiomeGroups`, `Patterns`, option entries, noise trees, the five other patches, export overlay, BiomePreview tool, rules and tests; glossary covers backwall, subworld, biome group, zone, band table, coverage, feature room, settle sim, census, overlay.
- no-magic-fridges: replaces the vanilla 1 C fridge reservoir with a thermostat model. entities covers `FridgeThermostat`, 2 patches, tuning constants, rules (compressor, insulation) and tests; glossary covers reservoir, setpoint, compressor, DTU units, RefrigeratorController.
- refined-building: tags refined metals as `Metal`. entities covers the single `ElementLoader.Load` postfix, rules, tests, and notes the mod is retired on the Workshop (`publish/retired.txt`); glossary covers Metal/RefinedMetal tags, oreTags, material lists.
- salty-surf: Mechanical Surfboard accepts Salt Water. entities covers `SurfWater`, 5 patches, rules and tests; glossary covers the shared tag, capacity tag, readiness, spill, descriptor.

## TODO(owner) left

None. Every fact came from the mod's source, README or tests. The template's "State Transitions" section was cut (no mod has states), and "Domain Terminology" was folded into each glossary rather than repeated in entities.

## write plan summary (each file, from `C:/dev/ONI-Mods`)

All six accepted with the same result, node `mods/<mod>` [aggregator], branch OM-001-populate-context:

    create  .context/domains/entities.md   by the narrative writer of record
    create  .context/domains/glossary.md   by the narrative writer of record
    narrative  domains/entities.md created
    narrative  domains/glossary.md created
    plan only — nothing was written.

## Friction

- None with ks. `write plan` accepted every file first time.
- Minor: the Bash tool system note asks for heredocs, but the memory rule says heredocs mangle backslashes; I wrote the markdown with the Write tool and built the JSON with a node script (`scratchpad/build4.mjs`), which avoided it.
- Unverified in entities.md: none flagged; the `U59`/build 744825 note in the motion-sensor glossary is taken from a source comment.
