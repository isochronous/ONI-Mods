---
witness_digest: "sha256:9688b74ec6e9cba440970a13381f458489d3ef2313d82d57c9e72a7b1d5a4ab8"
witness: loose
applies_to:
  - "src/DetectAnyElement/**"
doc_id: doc-25efc9f67a81
description: "The tag, strings and Harmony patches of the Detect Any Element mod (src/DetectAnyElement): the Any tag, ModStrings, side screen row, pipe, rail and world sensor patches and the asterisk glyph, with locations"
---

# Detect Any Element domain

## Overview
**Id:** blk-3e95a3354643

Detect Any Element adds an Any choice, directly under None, to the side screen of the five element sensors. On Any a sensor is on while anything at all is present, and its screen flashes an asterisk. The choice is a tag in the sensor's `Filterable`, so it is saved and copied like an element. Source is in `mods/detect-any-element/src/DetectAnyElement/`.

## Key Entities
**Id:** blk-eae9be288da3

### AnyElement
**Id:** blk-e4bdbace6e5b  
**Description**: The Any choice. A `Tag` named `DetectAnyElement_Any`, created with the English name "Any" through `TagManager.Create`. It rides in the same saved `Filterable` field as element tags, so it is saved with the building and copied by copy-settings. Without the mod, a sensor saved on Any keeps the tag and matches nothing.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/Mod.cs`

| Member | Type | Description |
|--------|------|-------------|
| TagName | const string | `DetectAnyElement_Any` |
| Tag | Tag | The created tag |
| Is(tag) | bool | Whether a tag is Any |

**Relationships**: `DetectAnyElementMod.OnLoad` registers `ModStrings` with `Isochronous.Translations` and re-creates the tag with the translated name after every language pass, because the game keeps its own copy of a tag's proper name.

### ModStrings
**Id:** blk-f7cd1a1120d9  
**Description**: The mod's one string, `ModStrings.MISC.TAGS.DETECTANYELEMENT_ANY` ("Any"), registered under the game's `STRINGS.MISC.TAGS` and replaced from `translations/<locale>.po`. Chinese (Simplified), Korean and Russian ship; the template is `translations/DetectAnyElement.pot`.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/ModStrings.cs`

### FilterSideScreen.SetTarget patch
**Id:** blk-ee58ba77ae0e  
**Description**: Postfix that adds an Any row second in the list, under None, on screens with `isLogicFilter` (the pipe and rail element sensors and the in-world sensors; the conveyor and pipe filters keep only None). The row has its icon hidden, like None. The row is kept per screen in a `ConditionalWeakTable`, and selected when the target is already on Any.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/Patches.cs` (`FilterSideScreen_SetTarget_Patch`)

### ConduitElementSensor.ConduitUpdate patch
**Id:** blk-2c3758b48106  
**Description**: Prefix for the Liquid Pipe, Gas Pipe and Conveyor Rail Element Sensors. On Any the sensor is on while the conduit at its cell holds mass above zero (any packet, whatever the element or item); it toggles the switch to match and skips the game's element test.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/Patches.cs` (`ConduitElementSensor_ConduitUpdate_Patch`)

### LogicElementSensor.Sim200ms patch
**Id:** blk-1687ce1b77e2  
**Description**: Prefix for the in-world Gas and Liquid Element Sensors. On Any the sensor is on while its cell holds any element of its desired state (any gas, or any liquid; vacuum counts as nothing). It keeps the game's eight-sample steadiness window (`samples`, `sampleIdx`) and drops only the element test.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/Patches.cs` (`LogicElementSensor_Sim200ms_Patch`)

### LogicElementSensor.OnElementSelected patch
**Id:** blk-8499447d8b7f  
**Description**: Postfix that clears the "no element selected" status item when the choice is Any, since Any is a choice and not a lack of one.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/Patches.cs` (`LogicElementSensor_OnElementSelected_Patch`)

### AnyGlyph
**Id:** blk-e07d0d8f7399  
**Description**: `KMonoBehaviour` added to each of the five sensors. While the sensor is on Any it overrides the symbol `elements` (the icon the screen flashes when on) with an asterisk symbol from the mod's `detect_any_asterisk_kanim`, named `asterisk_<sensor kanim>`; each is in that icon's colour and offset to land mid-screen. Off or another choice removes the override. If the symbol is missing it logs a warning and the sensor keeps the vanilla icon.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/AnyGlyph.cs`

**Relationships**: needs `Filterable`, `SymbolOverrideController` and `KBatchedAnimController` on the same object; subscribes to `Filterable.onFilterChanged`.

### SensorConfigs_DoPostConfigureComplete_Patch
**Id:** blk-dd68a64821ad  
**Description**: Postfix on `DoPostConfigureComplete` of `LiquidConduitElementSensorConfig`, `GasConduitElementSensorConfig`, `SolidConduitElementSensorConfig`, `LogicElementSensorGasConfig` and `LogicElementSensorLiquidConfig`. Adds a `SymbolOverrideController` to the prefab when missing (it must exist before the anim controller is instantiated) and adds `AnyGlyph`.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/AnyGlyph.cs`

### Switches
**Id:** blk-014063748fa2  
**Description**: Delegate for the protected `Switch.Toggle`, which the sensors flip through.  
**Location**: `mods/detect-any-element/src/DetectAnyElement/Patches.cs`

## Business Rules
**Id:** blk-1a05232af13b

- Any appears right under None for the five element sensors only.
- A pipe or rail sensor on Any is green while any packet at all passes through it.
- An in-world Gas sensor on Any is green while its cell holds any gas; the Liquid sensor, any liquid. Vacuum counts as nothing.
- Picking an element switches Any off and picking Any switches the element off: a sensor has one choice at a time.
- The Any choice is saved with the building and copied with its settings.
- When a sensor on Any is on, its screen flashes an asterisk; when off, the screen looks as it always does.
- Everything else, including the eight-sample window, is the game's own logic with the element test removed.

## Key Code Paths
**Id:** blk-752306474b78

- **Choosing Any**: `FilterSideScreen.SetTarget` postfix adds the row; the filter change updates the `Filterable` tag and `AnyGlyph.OnFilterChanged` — see `mods/detect-any-element/src/DetectAnyElement/Patches.cs` and `AnyGlyph.cs`
- **Pipe and rail detection**: `ConduitElementSensor.ConduitUpdate` prefix — see `Patches.cs`
- **World detection**: `LogicElementSensor.Sim200ms` prefix — see `Patches.cs`
- **Asterisk art**: `tools/make_art.py` writes `src/DetectAnyElement/anim/assets/detect_any_asterisk/` using the Liberation Sans Bold asterisk (SIL Open Font License; font and licence are in `tools/`)
- **Tests**: `mods/detect-any-element/src/DetectAnyElement.Tests/` (`Glyph.cs`, `Kit.cs`, `Persistence.cs`, `PipeSensors.cs`, `SideScreen.cs`, `WorldSensors.cs`)

## Related
**Id:** blk-b067b1ae2cbb

- See also: [glossary](glossary.md)
