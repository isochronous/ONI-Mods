---
tags: bridge-end
witness_digest: "sha256:ef46729018139a2309bdcd5cdc1cb45c601d5d52cc19e58efd8b43fca4df915d"
witness: loose
applies_to:
  - "src/JumperBridges/**"
doc_id: doc-59647738de3c
description: "Glossary for Jumper Bridges: abbreviations, terms to avoid, and definitions of bridge end and end node, port, ribbon, utility network, link ring, object layer, joint plate, wire drag, overload and overlay"
---

# Domain Glossary

Terms used in the Jumper Bridges mod (`mods/jumper-bridges`). For the classes and patches behind them, see the entities document.

## Abbreviations and Acronyms
**Id:** blk-331987027e34

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod is for |
| kanim | Klei animation | The wire art a node plays, for example `logic_wires_kanim` |
| PLib | Peter Han's Library | Options screen and library init in `Mod.cs` and `Options.cs` |
| Logic | Automation (code name) | `BridgeKind.Logic`, `LogicPorts`, `LogicWire`: the game's code says "logic" for automation |
| Power / Electrical | Power network | `BridgeKind.Power`; `ConnectionSource.Electrical` |

## Terms to Avoid
**Id:** blk-b1e1a23dc016  
**Tags:** mod-strings

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Invisible wire, hidden wire | Bridge end node | The node is not a wire building; it has no cost, yield or deconstruct order |
| Logic bridge (in player text) | Automation bridge | The game and the README call the buildings Automation Wire Bridge and Automation Ribbon Bridge |
| Wire under the end (for the node) | Bridge end node | A real wire under an end means no node is made |

## Cross-Domain Terms
**Id:** blk-48cc95762b31

### Bridge end
**Id:** blk-fa52bd2fd391  
**Tags:** utility-network  
**Definition**: One of a bridge's two end cells, which the game finds through the bridge's input ports.  
**Used in**: all files

### Bridge end node
**Id:** blk-2b5366f53fe2  
**Tags:** utility-network, game-component  
**Definition**: The stand-in for a wire under a bridge end: a building-less GameObject that registers the wire tile layer, a physical node and a network item so the port at that cell is connected.  
**Used in**: `BridgeEndNode`, `JumperBridge`

### Port
**Id:** blk-a6f1b2b01703  
**Tags:** utility-network  
**Definition**: A building's automation input or output, or a power input or output, at a cell. A port counts as connected when its cell is on a network.  
**Used in**: placement patches, `LogicPorts` patches

### Automation, ribbon, bit depth
**Id:** blk-a466b94b963a  
**Tags:** automation-ribbon  
**Definition**: Automation is the game's logic wiring. A ribbon carries four signals on one wire (`LogicWire.BitDepth.FourBit`); the ribbon bridge's node plays `logic_ribbon_kanim`.  
**Used in**: `BridgeKind.NodeAnim`

### Utility network
**Id:** blk-8f0976689c96  
**Tags:** utility-network  
**Definition**: The game's generic flood-filled network over cells that hold a conductor (`UtilityNetworkManager`); both automation and power networks are one. Bridges join two cells through the manager's link table.  
**Used in**: `BridgeKind`, `BridgeLinks`

### Link and link ring
**Id:** blk-b6960c2ccb0d  
**Tags:** utility-network  
**Definition**: The manager keeps one link partner per cell. For bridges that share cells, the mod writes a directed ring over the group's cells so the fill reaches all of them.  
**Used in**: `BridgeLinks`

### Object layer
**Id:** blk-251bdb713081  
**Tags:** utility-network  
**Definition**: `ObjectLayer`, the game's per-cell slot lists. A wire tile layer cell is what a network flood fill requires; an end layer (LogicGate, WireConnectors) is where the game marks a bridge's end cells.  
**Used in**: `BridgeKind`, `BridgeFootprint`

### Joint plate (heavy-watt)
**Id:** blk-279f9dc39e51  
**Tags:** utility-network

**Definition**: The Heavi-Watt Joint Plate or Heavi-Watt Conductive Joint Plate, a power bridge whose centre cell is a solid tile. The mod makes it a jumper bridge of the Power kind: the centre stays solid, the ends may sit on ports and on other plate ends, never on a wire bridge end.  
**Used in**: `BridgeKind.Of`, `MarkArea`, `IsValidHighWattBridgeLocation` and `IsValidTileLocation` patches

### Wire drag
**Id:** blk-d0010eca3d9e  
**Tags:** wire-tools  
**Definition**: Dragging the wire tool across cells; it joins consecutive cells by connection bits. A bridge end connects to a neighbour only through a drag.  
**Used in**: `BaseUtilityBuildTool.BuildPath` patch, `WireDrags.cs` tests

### Disconnect tool
**Id:** blk-8e5c50d5554a  
**Tags:** wire-tools  
**Definition**: The tool that cuts connections between adjacent wire cells; the mod makes it cut at bridge ends too.  
**Used in**: `DisconnectTool` patches

### Overload
**Id:** blk-6b907dab8e6f  
**Tags:** automation-ribbon  
**Definition**: A wire bridge carrying a ribbon signal is damaged as in the base game and breaks, which disconnects its link until repaired.  
**Used in**: `JumperBridge.OnBroken`, `OnRepaired`

### Overlay
**Id:** blk-3955244df226  
**Tags:** utility-network, harmony-patching  
**Definition**: The automation or power view that colours wires by network; the mod colours node pieces the same way.  
**Used in**: `OverlayModes` patches

## Related
**Id:** blk-3f15e924a8f5

- See also: [entities](entities.md)
