---
tags: bridge-end
witness_digest: "sha256:ef46729018139a2309bdcd5cdc1cb45c601d5d52cc19e58efd8b43fca4df915d"
witness: loose
applies_to:
  - "src/JumperBridges/**"
doc_id: doc-dfca545db3a4
description: "The classes, options, Harmony patches and saved state of Jumper Bridges (`JumperBridge`, `BridgeEndNode`, `BridgeKind`, `BridgeLinks`, `BridgeFootprint`), with the bridge end rules, key code paths and tests"
---

# Jumper Bridges domain

## Overview
**Id:** blk-934528d428c9  
**Tags:** utility-network

Jumper Bridges lets an Automation Wire Bridge or Automation Ribbon Bridge, and a Wire Bridge, Conductive Wire Bridge or Insulated Wire Bridge, be placed with either end directly on a port cell, with that port counting as connected. It adds no buildings. A bridge end with no wire under it gets a stand-in node that makes the game's own network rules carry the signal or current into the port. Source is in `mods/jumper-bridges/src/JumperBridges/`.

## Key Entities
**Id:** blk-dbb08ca2d7f3

### JumperBridgesMod
**Id:** blk-45611b9a4ffc  
**Tags:** mod-entry-point, mod-options, mod-strings  
**Description**: The `UserMod2` entry point. Loads `Options` before `base.OnLoad` (which applies the patches) and sets each `BridgeKind.Enabled`, so a kind that is off has its patches skipped through `Prepare()`. Then initializes PLib, registers `ModStrings` with `Isochronous.Translations` and registers the options screen.  
**Location**: `mods/jumper-bridges/src/JumperBridges/Mod.cs`

### Options
**Id:** blk-f92ca7ddeb23  
**Tags:** mod-options, mod-strings  
**Description**: PLib options saved in the shared config location, both restart-required.  
**Location**: `mods/jumper-bridges/src/JumperBridges/Options.cs`

| Property | Type | Description |
|----------|------|-------------|
| AutomationBridges | bool (default true) | Direct automation bridges; off leaves automation bridges to the base game |
| PowerBridges | bool (default true) | Direct power bridges; off leaves power bridges to the base game |

**Relationships**: option names and tooltips are string keys under `STRINGS.JUMPERBRIDGES.OPTIONS` defined in `ModStrings.cs`; translations load from `translations/<locale>.po`.

### BridgeKind
**Id:** blk-e978d080073f  
**Tags:** utility-network

**Description**: What differs between automation and power: the network manager, layers and wire art. Two instances, `Logic` and `Power`, in `BridgeKind.All`. Each keeps its own live node table, so an automation and a power bridge end on one cell are on two separate networks. `Of(BuildingDef)` maps `BuildLocationRule.LogicBridge` to Logic, and both `BuildLocationRule.WireBridge` and `BuildLocationRule.HighWattBridgeTile` (the two heavy-watt joint plates) to Power. The plates join Power rather than a kind of their own because one network manager allows one node and one link partner per cell.  
**Location**: `mods/jumper-bridges/src/JumperBridges/BridgeKind.cs`

| Property | Type | Description |
|----------|------|-------------|
| Source | ConnectionSource | `Logic` or `Electrical` |
| TileLayer / WireLayer / ReplacementLayer / EndLayer | ObjectLayer | Logic: LogicWireTile, LogicWire, ReplacementLogicWire, LogicGate. Power: WireTile, Wire, ReplacementWire, WireConnectors |
| SceneLayer | Grid.SceneLayer | LogicWires or Wires |
| Enabled | bool | Set from the options at load |
| Nodes | Dictionary<int, BridgeEndNode> | Live nodes by cell, one per cell |
| Manager, Items, Links | game network manager and its private `items` and `links` tables | Read by reflection on cold paths |

**Relationships**: `NodeAnim` picks the wire kanim a node plays: `logic_wires_kanim` or `logic_ribbon_kanim` (by the link's bit depth), and for power `utilities_electric_kanim`, `utilities_electric_conduct_kanim` or `utilities_electric_rubber_kanim`, or for the joint plates `utilities_electric_insulated_kanim` (heavi-watt wire) or `utilities_electric_conduct_hiwatt_kanim` (conductive heavi-watt wire).

### JumperBridge
**Id:** blk-c5d470758241  
**Tags:** game-component  
**Description**: `KMonoBehaviour` on every complete bridge of an enabled kind. Gives each end that has no wire under it a `BridgeEndNode` (creation deferred a frame so a loaded save's wires have marked their cells first), removes the nodes when the bridge is deconstructed or broken (`BuildingBroken`) and recreates them on repair (`BuildingFullyRepaired`).  
**Location**: `mods/jumper-bridges/src/JumperBridges/JumperBridge.cs`

**Relationships**: requires `UtilityNetworkLink`; owns up to two `BridgeEndNode`s; `Inherit` passes a leaving wire's connections to the node that replaces it.

### BridgeEndNode
**Id:** blk-a0f2a41a1467  
**Tags:** game-component, utility-network  
**Description**: A `KAnimGraphTileVisualizer` on a standalone GameObject (no building) that stands in for a wire under a bridge end. It makes the three registrations a wire makes: the wire tile layer, a physical network node with connection bits and a network item at the cell, so the port there is told it is connected. It plays the wire kanim and mirrors the connection bits its neighbours point at it. Several bridge ends on one cell share one node, which goes with its last owner. It costs nothing, yields nothing, has no deconstruct order and is not saved. On a power network it is not a `Wire`, so the circuit's wattage limit is the bridge's own.  
**Location**: `mods/jumper-bridges/src/JumperBridges/BridgeEndNode.cs`

**Relationships**: owned by one or more `JumperBridge`s; registered in `BridgeKind.Nodes`.

### BridgeFootprint
**Id:** blk-f00f2e956606  
**Tags:** game-component, wire-tools  
**Description**: Registry of where every bridge is, by cell, on the complete and the under-construction prefab. The game puts a bridge on its end layer only at its two end cells and this mod stops a bridge taking that slot from a port or earlier bridge, so a bridge with both ends taken is on no layer. The deconstruct, cancel and prioritize tools find it here by its end cells.  
**Location**: `mods/jumper-bridges/src/JumperBridges/BridgeFootprint.cs`

### BridgeLinks
**Id:** blk-67d61b61d321  
**Tags:** utility-network  
**Description**: Owns the network manager's link table for groups of bridges that share end cells. The manager keeps one link partner per cell, so after any bridge connects or disconnects, the distinct cells of its group are rewritten as a directed ring (c0 to c1 to ... to c0), sorted so the write is idempotent, then the networks are rebuilt.  
**Location**: `mods/jumper-bridges/src/JumperBridges/BridgeLinks.cs`

### Harmony patches
**Id:** blk-a88b0d39bc11  
**Tags:** harmony-patching, wire-tools

**Description**: The game classes and methods patched, grouped by purpose, all in `mods/jumper-bridges/src/JumperBridges/Patches.cs`. A group is skipped when its kind is off.
- Bridge setup (automation): `LogicWireBridgeConfig` and `LogicRibbonBridgeConfig` `DoPostConfigureComplete` and `DoPostConfigureUnderConstruction` add `JumperBridge` and `BridgeFootprint`.
- Bridge setup (power): `WireBridgeConfig` and `WireBridgeHighWattageConfig` `DoPostConfigureComplete` and `DoPostConfigureUnderConstruction`; the other wire bridges and the conductive joint plate inherit them.
- Placement: `BuildingDef.MarkArea` (a joint plate keeps the vanilla footprint marks, so its solid centre stays on Building and FoundationTile), `AreLogicPortsInValidPositions`, `IsValidWireBridgeLocation`, `IsValidHighWattBridgeLocation` (replaced: ends may sit on ports and other plate ends, vanilla's off-map, in-tile and door refusals kept), `IsValidTileLocation` (no tile on a plate end; vanilla's checks done by the mod on bridge end node cells, where vanilla would throw), `ArePowerPortsInValidPositions`, `MarkOverlappingPorts`.
- Ports: `LogicPorts.CreatePhysicalPorts`, `DestroyPhysicalPorts`, `CreateVisualizers` (skipped for bridges).
- Links: `LogicUtilityNetworkLink` and `WireUtilityNetworkLink` `OnConnect` and `OnDisconnect` call `BridgeLinks`.
- Wire art and drags: `KAnimGraphTileVisualizer.OnSpawn`, `UpdateConnections`, `OnCleanUp` (a leaving wire's connections are kept only for a cell where some bridge has an end); `BaseUtilityBuildTool.BuildPath`.
- Tools: `DisconnectTool.OnDragComplete` and `OnMouseMove`; `DeconstructTool.DeconstructCell`; `CancelTool.OnDragTool`; `PrioritizeTool.OnDragTool`.
- Overlays: `OverlayModes.Logic` and `OverlayModes.Power` `Update` and `Disable`.

## Business Rules
**Id:** blk-4bb5a42c53b7  
**Tags:** wire-tools, vanilla-fallback, mod-options

- A bridge end may sit on any port cell (a machine's input or output, a gate's port, another bridge's end) and the port is on the bridge's network with no wire laid first. Chains work, and several bridges may meet at one cell.
- A bridge end behaves like a wire cell: it connects only to cells a wire drag explicitly joined it to, never because a wire sits next to it.
- A bridge end on a real wire works as in the base game and gets no node.
- A bridge end on a gate does not put the gate out of order.
- Deconstructing the bridge removes the node; wires that led into the end redraw as dead ends at once.
- Overload is unchanged: a wire bridge carrying a ribbon signal is damaged as in the base game, and a broken bridge's ends stop conducting until repaired.
- The heavy-watt joint plates are jumper bridges too. Their centre stays a solid tile as in the base game; only the ends gain jumper placement. A plate end may share a cell with a port or another plate's end, never with a wire bridge's end (refused both ways), and no tile may be built on a plate end. A plate is reached for deconstruct at its centre as well as its ends.
- A building may be placed with its power port on an existing bridge end.
- Each kind can be turned off in the options (restart required); a kind that is off is left to the base game. The joint plates come under the power option.
- The bridges' own two end ports are kept as geometry but never register as ports, which also removes the base game's silent port displacement when two ports share a cell. They no longer show in the automation overlay.

## Key Code Paths
**Id:** blk-852815eed994  
**Tags:** harmony-patching, wire-tools, in-game-testing

- **Startup**: options read, kinds enabled, patches applied (with `Prepare` skipping disabled kinds) — see `mods/jumper-bridges/src/JumperBridges/Mod.cs`
- **Bridge spawns**: `JumperBridge.OnSpawn` schedules node creation, `BridgeEndNode.Create` builds the node, `BridgeLinks.OnConnect` rewrites the ring — see `JumperBridge.cs`, `BridgeEndNode.cs`, `BridgeLinks.cs`
- **Wire drag onto a bridge end**: `BaseUtilityBuildTool.BuildPath` patch reads the drag's bits into the node — see `Patches.cs`
- **Deconstruct by ends**: `DeconstructTool.DeconstructCell` and `BridgeFootprint.EndsAt` — see `Patches.cs`, `BridgeFootprint.cs`
- **Design record**: the investigation notes (section 6 covers the design) are kept outside this repository
- **Tests**: in-game tests in `mods/jumper-bridges/src/JumperBridges.Tests/` (`Signals.cs`, `Gates.cs`, `WireDrags.cs`, `WireUnderEnd.cs`, `Deconstruct.cs`, `Power.cs`, `JointPlates.cs`, plus explicit `Preview.cs`); `README.md` carries the manual in-game checklist for what is not yet automated

## Related
**Id:** blk-02f47981e0e0

- See also: [glossary](glossary.md)
