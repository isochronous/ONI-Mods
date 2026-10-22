# Changelog

## Unreleased

- Heavi-Watt Joint Plates and Heavi-Watt Conductive Joint Plates too: a plate end can sit directly on a power port or another plate's end. The centre stays a solid tile.
- Fixed an error when placing a tile over a bridge end node.

## 0.2.0 - 2026-10-17

- Renamed to Jumper Bridges, since it now covers power bridges too.
- Power bridges too: a Wire Bridge, Conductive Wire Bridge or Insulated Wire Bridge end can sit directly on a power port or another bridge's end. Heavy-watt joint plates are unchanged.
- Options to turn automation bridges and power bridges on or off separately.
- The deconstruct, cancel and prioritize tools reach a bridge by its ends only, as in the base game.
- The disconnect tool's filter now applies to bridge ends.
- A bridge end joined to a neighbour no longer shows a white band behind its wire piece.

## 0.1.1 - 2026-10-15

- A bridge built while the game is paused now connects its ends right away, instead of only once the game is unpaused.

## 0.1.0 - 2026-10-13

- First version: an Automation Wire Bridge or Automation Ribbon Bridge can be placed with either end directly on any automation port cell (a machine's port, a logic gate's port, another bridge's end), and that port is connected. A bridge end with no wire under it draws as a piece of wire of the bridge's kind, coloured by its network in the automation overlay, costs nothing and goes with the bridge. A bridge end connects only to cells a wire drag joined it to, and the disconnect tool cuts at bridge ends. Bridge ends on real wires are unchanged.
- A bridge end sitting on a wire keeps that wire's connections if the wire melts or breaks, so the bridge stays joined to what the wire joined it to.
