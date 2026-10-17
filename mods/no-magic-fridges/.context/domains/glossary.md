---
tags: refrigerator, heat-exchange
witness_digest: "sha256:be1c3bfba2f3ba0bebbb432de154b8fb12c3f13303a0e342b572bcf2cb622932"
witness: loose
applies_to:
  - "src/NoMagicFridges/**"
doc_id: doc-cf415c96c76b
description: "Glossary for No Magic Fridges: abbreviations such as DTU, terms to avoid, and definitions of the vanilla reservoir, thermostat, setpoint, compressor, interior temperature and Insulated Tile factor, with the code using each"
---

# No Magic Fridges glossary

Terms for the No Magic Fridges mod. For the code behind them, see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-3456ea7ca0c8

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod targets |
| NMF | No Magic Fridges | Prefix of the in-game test names in `src/NoMagicFridges.Tests/` |
| DTU | Duplicant Thermal Unit | The game's heat unit; 1 kDTU = 1 kJ. Heat capacities in the code are kDTU/K |
| kDTU/s/K | kilo-DTU per second per kelvin | Conductance; the def's 1000 (W/K) reads as 1 kDTU/s/K |
| C | Heat capacity | In the model, an item's mass x specific heat; the comments also write C for degrees Celsius |

## Terms to Avoid
**Id:** blk-f014faf2eaeb

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Magic (cooling) | Reservoir | The vanilla behaviour the mod removes is a simulated 1 degree C reservoir on every item |
| Freezer (for the building) | Refrigerator | The vanilla building is the Refrigerator; "Freezer" appears only in a code comment describing a -19 degree C cold box |
| Insulite (for the item insulation) | Insulated Tile factor | The 0.01 scale slows exchange like an Insulated Tile; Insulite stops it |

## Cross-Domain Terms
**Id:** blk-5331b50664aa

### Reservoir (simulated temperature adjuster)
**Id:** blk-f3e230c14424  
**Definition**: Vanilla's `SimulatedTemperatureAdjuster` attaches a sim heat reservoir at the fridge's `simulatedInternalTemperature` (1 degree C) to each stored item through `ModifyElementChunkTemperatureAdjuster`. It exchanges heat both ways, which is why vanilla warms frozen food.  
**Used in**: `Patches.cs`

### Thermostat
**Id:** blk-4f0bf4991185  
**Definition**: The mod's model of the fridge as an insulated box: a shared interior temperature, a compressor that only cools, and no heating.  
**Used in**: `FridgeThermostat.cs`

### Compressor on / off
**Id:** blk-cd0fd04ff662  
**Definition**: On while the mean temperature of the contents is above the setpoint; the fridge's own thermal mass (`BaseHeatCapacity`) at the setpoint is blended in and pulls the interior down. Off at or below the setpoint, when the contents only equalise among themselves.  
**Used in**: `FridgeThermostat.Exchange`

### Setpoint
**Id:** blk-23cf70bad61b  
**Definition**: The fridge's target temperature, `RefrigeratorController.Def.simulatedInternalTemperature`, 1 degree C in vanilla.  
**Used in**: `FridgeThermostat.Sim1000ms`

### Interior temperature
**Id:** blk-c63ad1f95c65  
**Definition**: The single temperature every sampled item moves toward in one step.  
**Used in**: `FridgeThermostat.Exchange`

### RefrigeratorController
**Id:** blk-5645b8195c9e  
**Definition**: The vanilla state machine (`RefrigeratorController.Def`) on the Refrigerator and Mini Fridge. Any building with this def gets a thermostat, including modded fridges.  
**Used in**: `Patches.cs`

### SimTemperatureTransfer
**Id:** blk-c77fc14f105f  
**Tags:** game-component  
**Definition**: The component that links an item to the sim for heat exchange with the cell it sits in; `SurfaceArea` and `GroundTransferScale` set how fast.  
**Used in**: `FridgeThermostat.Insulate`

### PrimaryElement
**Id:** blk-b54dd85bb2d1  
**Tags:** game-component  
**Definition**: The component holding an item's element, mass and temperature. Writing `Temperature` forwards to the sim chunk.  
**Used in**: `FridgeThermostat.Exchange`

### Insulated Tile factor
**Id:** blk-83c568d5faca  
**Definition**: 0.01, the thermal-conductivity scale of the Insulated Tile, applied to a cold item's exchange with a warmer room.  
**Used in**: `FridgeThermostat.InsulationFactor`

### Operational / powered
**Id:** blk-98383b78254f  
**Tags:** game-component  
**Definition**: `Operational.IsActive` on the fridge; true while it has power. The mod's model runs only then.  
**Used in**: `FridgeThermostat.Sim1000ms`

### Sim1000ms
**Id:** blk-0efeb721f740  
**Definition**: The game's `ISim1000ms` callback, once per second of game time; the thermostat's tick.  
**Used in**: `FridgeThermostat`

## Related
**Id:** blk-3a7256bf9c1b

- See also: [entities](entities.md)
