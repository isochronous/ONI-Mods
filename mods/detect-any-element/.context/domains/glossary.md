---
witness_digest: "sha256:9688b74ec6e9cba440970a13381f458489d3ef2313d82d57c9e72a7b1d5a4ab8"
witness: loose
applies_to:
  - "src/DetectAnyElement/**"
doc_id: doc-9501e487a27b
description: "Glossary for detect-any-element: the Any choice, the sensors it patches and the asterisk art"
---

# Domain Glossary

Terms used in the Detect Any Element mod (`mods/detect-any-element`). For the classes and patches behind them, see the entities document.

## Abbreviations and Acronyms
**Id:** blk-dd341ecb4ff1

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod is for |
| kanim | Klei animation | The game's sprite-animation asset; the mod ships `detect_any_asterisk_kanim` |
| .po / .pot | Gettext translation file / template | `translations/<locale>.po` and `DetectAnyElement.pot` |

## Terms to Avoid
**Id:** blk-567dd4463017

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Wildcard, All | Any | The side screen row, the tag and the README all say Any |
| Element (for the Any choice) | Choice | Any is a tag in the filter, not an element; "no element selected" must not show for it |

## Cross-Domain Terms
**Id:** blk-872dc2eafe6c

### Any
**Id:** blk-c8ba999de59d  
**Definition**: The extra choice under None in an element sensor's side screen; the sensor is on while anything is present.  
**Used in**: `AnyElement`, `FilterSideScreen_SetTarget_Patch`

### Element sensor
**Id:** blk-e193fc931720  
**Definition**: One of five automation sensors that switch on when a chosen element is present: Liquid Pipe, Gas Pipe and Conveyor Rail Element Sensors (`ConduitElementSensor`), and the in-world Gas and Liquid Element Sensors (`LogicElementSensor`).  
**Used in**: all patches

### Filterable
**Id:** blk-ba979d320f69  
**Definition**: The component that holds a building's chosen tag (`SelectedTag`) and saves it. The Any tag is stored here like an element tag.  
**Used in**: `AnyElement`, `AnyGlyph`, the sensor patches

### Tag
**Id:** blk-4a8dea407a48  
**Definition**: The game's `Tag` identifier, used for elements, categories and filter choices. `DetectAnyElement_Any` is the mod's tag, with a proper name "Any".  
**Used in**: `AnyElement`, `ModStrings`

### FilterSideScreen
**Id:** blk-d8ac2b48b1c5  
**Definition**: The details-panel screen listing None and element categories for a filtering building. `isLogicFilter` is true for the sensors; the conveyor and pipe filters keep only None.  
**Used in**: `FilterSideScreen_SetTarget_Patch`

### Conduit
**Id:** blk-691ec378450d  
**Definition**: A pipe or rail cell that carries a packet; the pipe sensors read the contents at their own cell.  
**Used in**: `ConduitElementSensor_ConduitUpdate_Patch`

### Steadiness window
**Id:** blk-ada7d73ad87a  
**Definition**: The in-world sensors' eight samples (every 200 ms) that must all agree before the switch changes, which stops flicker. Kept on Any.  
**Used in**: `LogicElementSensor_Sim200ms_Patch`

### Symbol override
**Id:** blk-8b1e59874671  
**Definition**: Replacing one named symbol of an animation at runtime (`SymbolOverrideController`). The sensor's `elements` symbol, the flashing icon, is replaced by the asterisk.  
**Used in**: `AnyGlyph`

## Related
**Id:** blk-aa8fba331ade

- See also: [entities](entities.md)
