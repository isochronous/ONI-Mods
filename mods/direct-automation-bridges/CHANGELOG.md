# Changelog

## Unreleased

- A wire under a bridge end that melts or breaks leaves its connections to the end: the node that takes its place inherits them, so the bridge stays joined to what the wire joined it to. Before, the bridge lost those links while standing.
- Fixed before release: with instant build off, dragging a wire from, to or through a bridge end threw inside the game's build path and left the interface unresponsive until the tool was cancelled. Found by the new automated tests.
- First version: an Automation Wire Bridge or Automation Ribbon Bridge can be placed with either end directly on any automation port cell (a machine's port, a logic gate's port, another bridge's end), and that port is connected. A bridge end with no wire under it draws as a piece of wire of the bridge's kind, coloured by its network in the automation overlay, costs nothing and goes with the bridge. A bridge end connects only to cells a wire drag joined it to, and the disconnect tool cuts at bridge ends. Bridge ends on real wires are unchanged.
