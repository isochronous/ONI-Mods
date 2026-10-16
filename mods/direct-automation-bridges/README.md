# Direct Automation Bridges

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: an Automation Wire Bridge or Automation Ribbon Bridge can be placed with either end directly on any automation port cell, and that port counts as connected.

## What it does

- A bridge end may sit on any port cell: a machine's input or output, a logic gate's port, or another bridge's end. The port is on the bridge's network with no wire laid under the end first. Chains of bridges end-to-end work, and several bridges may meet at one cell.
- A bridge end behaves exactly like a wire cell: it connects only to cells a wire drag explicitly joined it to, never because a wire merely sits next to it.
- Each bridge end that has no wire under it draws as a piece of wire of the bridge's kind (wire or ribbon), coloured by its network in the automation overlay like any wire. Dragging the wire tool from or to a bridge end joins the two cells, visually and logically, without queuing a wire order on the end cell. The disconnect tool cuts connections at bridge ends like at wires, including between two adjacent bridge ends.
- There is nothing extra to build: the piece of wire at the end costs nothing, yields nothing and has no deconstruct order of its own. Deconstructing the bridge takes it away, and wires that led into the end redraw as dead ends at once.
- A bridge end on a real wire works exactly as before.
- A bridge whose end is on a gate does not put the gate out of order, and the gate's own ports keep working.
- Overload is unchanged: a wire bridge carrying a ribbon signal is damaged as in the base game.

The bridges' own end ports no longer show in the automation overlay; the bridge art shows its ends.

## How it works

The automation network is a flood fill over cells that hold a conductor. Each bridge end that has no wire under it gets a bridge end node: a GameObject with no building that makes the three registrations a wire makes (the wire tile layer, a physical network node, a network item), so the game's own rules carry the signal into the port at that cell. The node plays the wire kanim through the game's own `KAnimGraphTileVisualizer`, reading its connection bits from the wires pointing at it; a cell shared by several bridge ends has one node that all of them own. The bridges' two input ports are kept as geometry (the game finds a bridge's end cells through them) but stopped from ever registering as ports, which also removes the base game's silent port displacement when two ports share a cell. Bridges that share end cells have their network links rewritten as a ring over the group's cells, because the network manager keeps one link partner per cell. A bridge never takes the LogicGate layer slot away from a gate or an earlier bridge; the tools that sweep layers find it through the mod's own cell registry instead. The design and the code evidence behind it are in the ONI-Mods workspace's incubator notes, `incubator/direct-automation-bridges/INVESTIGATION.md`, section 6 (sections 1 to 5 are the decompiled evidence it rests on).

Patches: `LogicPorts.CreatePhysicalPorts`, `DestroyPhysicalPorts` and `CreateVisualizers` (skipped for bridges); `BuildingDef.AreLogicPortsInValidPositions` (no port conflict for bridges) and `MarkArea` (bridges mark only free LogicGate-layer cells and never tag); `LogicWireBridgeConfig` and `LogicRibbonBridgeConfig.DoPostConfigureComplete` and `DoPostConfigureUnderConstruction` (adds `DirectBridge` and `BridgeFootprint`); `LogicUtilityNetworkLink.OnConnect` and `OnDisconnect` (link rings for bridge groups); `KAnimGraphTileVisualizer.OnSpawn`, `UpdateConnections` and `OnCleanUp` (nodes step aside for wires and follow their neighbours); `BaseUtilityBuildTool.BuildPath` (nodes read a drag's bits); `DisconnectTool.OnDragComplete` and `OnMouseMove` (cuts and cut markers at node cells); `DeconstructTool.DeconstructCell`, `CancelTool.OnDragTool` and `PrioritizeTool.OnDragTool` (bridges on no layer); `OverlayModes.Logic.Update` and `Disable` (overlay colouring of nodes).

## Installing

As a local mod:

1. Download `DirectAutomationBridges-<version>.zip` from the [latest release](https://github.com/isochronous/direct-automation-bridges/releases/latest).
2. Extract it into a new folder named `DirectAutomationBridges` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\DirectAutomationBridges`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/DirectAutomationBridges`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/direct-automation-bridges.git
dotnet build direct-automation-bridges/src/DirectAutomationBridges -c Release
```

The build deploys the mod into the game's local mods folder. See `common/README.md` for the shared build and release tooling.

## Testing

Automated in-game tests live in `src/DirectAutomationBridges.Tests` and run through the harness in `common/tools/TestRunner` (see its README): the game loads a fixture save, the tests build in a cleared area and drive the real wire tool, and the results land in a run folder.

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods DirectAutomationBridges
```

So far: the sensor-bridge-machine signal check (API layer) and the "wire dragged from a bridge end to its neighbour" check (tool layer). The rest of the checklist below is still done by hand.

In-game checks, from the investigation's sections 5 and 6. Run them in sandbox mode with the automation overlay open; each line is one check.

Placement

- [ ] A wire bridge previews white with one end on a sensor's output port, and builds there (`BuildTool.UpdateVis` colours by `IsValidPlaceLocation`).
- [ ] Same with both ends on ports (sensor output on one end, machine input on the other), for the wire bridge and the ribbon bridge, in all four rotations.
- [ ] A bridge end on a NOT gate's input cell is allowed; the gate stays operational (no "overlapping ports" status) and inverts the signal the bridge brings.
- [ ] A bridge end on another bridge's end is allowed.
- [ ] A machine placed so that its port lands on an existing bridge end is allowed.
- [ ] A bridge end on a queued (under construction) machine's port is allowed.

Signal

- [ ] Sensor output, bridge end on it, bridge other end on a machine's input port: the machine follows the sensor (green and red both ways), with no wire anywhere.
- [ ] The automation overlay shows both end cells in the network's colour; hovering the bridge highlights the far side.
- [ ] The machine's port keeps working after the bridge is deconstructed (it reads disconnected, then follows a wire laid to it).
- [ ] Deconstructing the bridge does not leave a wire piece behind and refunds only the bridge.
- [ ] Deconstruct the bridge: wires that led into its ends redraw as dead ends at once.
- [ ] Load a save with a direct bridge built machine-first, and one built bridge-first: the signal still crosses after load, in both.

Chains

- [ ] Two bridges end-to-end across a gap (second bridge's end on the first's end, one shared cell): a signal entering the far end of the first reaches the far end of the second, with no wire anywhere; the shared cell is one piece of wire in the overlay.
- [ ] Three bridges meeting at one cell: a signal entering any of the three outer ends reaches the other two.
- [ ] Deconstruct each bridge of the chain by dragging the deconstruct tool over its two end cells in turn (shared end, own end): each end marks it, and the middle cell does not, as in the base game; the other bridges and the shared cell's wire piece stay and keep conducting.
- [ ] Cancel tool on a queued bridge whose ends are both on gates, by one of its end cells.
- [ ] Save and load with a chain: the chain still conducts; no "already has a utility network connector" warnings in the log.

Wires meeting bridge ends

- [ ] Build a wire on the cell beside a bridge end with a drag that does not enter the end cell: no connection, no segment drawn on either side.
- [ ] Drag the wire tool from the bridge end to that neighbour: the two connect, both draw the segment, no wire order is queued on the end cell.
- [ ] Drag a wire up to a bridge end that has no wire: the new wire draws its segment into the end and the end draws the matching segment back; the wire's network reaches across the bridge once the wire completes.
- [ ] Drag a wire path through a bridge end: no wire order is queued on the end cell; wires either side connect across it.
- [ ] Deconstruct a wire that was under a bridge end: the end gets its own piece of wire back, with no connections, and the port there stays connected.
- [ ] Disconnect box (or line) over the bridge end and its wire neighbour: the connection is cut and both redraw; the cut marker shows while dragging.
- [ ] Two adjacent bridge ends joined by a drag (both draw the segment, signal crosses), then cut by the disconnect tool.
- [ ] A bridge end on a real wire: no node is created (the log line names "(wire)" for that end), everything as in the base game.

Overload and damage

- [ ] A ribbon signal through a wire bridge overloads and damages the bridge, not the ports; when the bridge breaks, its ends stop conducting; after repair they conduct again.

Overlay and sorting

- [ ] Outside the overlay, the end piece draws under the bridge art, not over it, for the wire bridge and the ribbon bridge.
- [ ] Ribbon bridge ends show the four ribbon strands coloured per bit in the overlay.
- [ ] Closing the overlay returns the end pieces to the plain look.

Open

- [ ] The machine's port sprite in the overlay: with a bridge end on it and no wire, `LogicPorts.GetConnectedWireBitDepth` reads layer 31 and finds no wire, so the sprite is the no-wire variant; note what it shows.
- [ ] A wire bridge and a ribbon bridge sharing an end cell: the shared piece draws as whichever bridge arrived first; note whether that reads wrong.
- [ ] Log: one "end nodes at" line per bridge when its nodes are created, one "end nodes removed" line when they go, nothing per tick.
