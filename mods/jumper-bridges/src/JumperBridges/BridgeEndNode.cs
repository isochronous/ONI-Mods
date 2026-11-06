using System.Collections.Generic;
using UnityEngine;

namespace JumperBridges
{
	/// <summary>
	/// The stand-in for a wire under a bridge end. The network only asks three things of a cell
	/// before it carries a signal or a current across it: something on the wire tile layer
	/// (UtilityNetworkManager.QueueCellForVisit), a physical node with connection bits
	/// (SetConnections), and an item at the cell (AddToNetworks) so the port there is told it is
	/// connected. A wire building answers all three through its KAnimGraphTileVisualizer and its
	/// LogicWire or Wire; this node answers them with no building at all, so there is no cost, no
	/// yield, nothing to deconstruct and nothing to save. It plays the wire kanim through the
	/// inherited Refresh(), which is how a real wire picks its "L", "LR", "LRU"... art, so a wire
	/// dragged up to a bridge end gets the matching segment drawn back at it. Automation and power
	/// work the same way; the kind says which manager, layers and art.
	///
	/// The rule: a bridge end behaves exactly like a wire cell. It connects only to cells a wire
	/// drag explicitly joined it to, never because a wire merely sits next to it. The node has
	/// no connections of its own; it mirrors the bits its neighbours point at it, and a neighbour
	/// only points at a cell because a drag paired the two (ApplyPathToConduitSystem), because its
	/// saved connections did, or because the manager's Reconnect echoed a bit the other side had.
	///
	/// On a power network the node is not a Wire, so ElectricalUtilityNetwork.AddItem leaves it out
	/// of the wattage groups: the circuit's limit is the bridge's own rating, as it is for a bridge
	/// whose end sits on a wire.
	/// </summary>
	public sealed class BridgeEndNode : KAnimGraphTileVisualizer
	{
		/// <summary>
		/// The bridges whose end is this cell. Two bridge ends may share a cell; the cell gets one
		/// node (the manager keeps one item per cell), the later bridge adopts it, and the node
		/// goes when the last owner does.
		/// </summary>
		private readonly List<JumperBridge> owners = new List<JumperBridge>();

		public BridgeKind Kind { get; private set; }
		public int Cell { get; private set; }
		public bool Ribbon { get; private set; }

		private KBatchedAnimController controller;
		/// <summary>The owner whose wire art the node plays.</summary>
		private JumperBridge artOwner;
		private bool registered;
		private bool released;

		private int TileLayer => (int)Kind.TileLayer;

		public static IEnumerable<BridgeEndNode> All(BridgeKind kind) => kind.Nodes.Values;

		public static int Count(BridgeKind kind) => kind.Nodes.Count;

		public static bool TryGet(BridgeKind kind, int cell, out BridgeEndNode node) => kind.Nodes.TryGetValue(cell, out node);

		/// <summary>
		/// A standalone GameObject at the cell, on the kind's wire scene layer so the bridge art (one
		/// layer up) draws over it, built the way the game builds its own runtime controllers
		/// (AutoMiner's arm): inactive, controller configured, then activated.
		/// </summary>
		public static BridgeEndNode Create(JumperBridge bridge, BridgeKind kind, int cell)
		{
			GameObject go = new GameObject("JumperBridgeEnd");
			go.SetActive(false);
			go.transform.SetPosition(Grid.CellToPosCBC(cell, kind.SceneLayer));
			KBatchedAnimController controller = go.AddComponent<KBatchedAnimController>();
			controller.AnimFiles = new KAnimFile[] { Assets.GetAnim(kind.NodeAnim(bridge.gameObject)) };
			controller.initialAnim = "None";
			controller.sceneLayer = kind.SceneLayer;
			controller.isMovable = false;
			// The wire tool's BuildPath, with instant build off, reads the element of whatever sits on
			// the wire tile layer at each cell of the drag to decide whether an order replaces it
			// (gameObject2.GetComponent<PrimaryElement>().Element.tag); a node without one throws
			// there, mid-drag, with the UI event system still switched off. The bridge's own element
			// is the honest answer, and nothing else reads it: the node has no mass and no building.
			PrimaryElement bridgeElement = bridge.GetComponent<PrimaryElement>();
			PrimaryElement element = go.AddComponent<PrimaryElement>();
			element.ElementID = bridgeElement != null ? bridgeElement.ElementID : SimHashes.Copper;
			BridgeEndNode node = go.AddComponent<BridgeEndNode>();
			node.owners.Add(bridge);
			node.artOwner = bridge;
			node.Kind = kind;
			node.Cell = cell;
			LogicUtilityNetworkLink logicLink = bridge.GetComponent<LogicUtilityNetworkLink>();
			node.Ribbon = logicLink != null && logicLink.bitDepth == LogicWire.BitDepth.FourBit;
			node.connectionSource = kind.Source;
			node.isPhysicalBuilding = true;
			// In the registry from the start, not from OnSpawn (which runs at the next Start phase):
			// two bridges sharing this cell create in the same scheduler tick, and the second must
			// find and adopt this node rather than make its own. Everything that reads the
			// registry checks that the node is registered with the manager before acting on it.
			kind.Nodes[cell] = node;
			go.SetActive(true);
			return node;
		}

		/// <summary>
		/// Not the base OnSpawn: that one ends in TileVisualizer.RefreshCell through
		/// GetComponent&lt;Building&gt;().Def, and the node has no Building. The three
		/// registrations are the ones a wire makes (BuildingDef.Build marks the tile layer,
		/// KAnimGraphTileVisualizer.OnSpawn sets the node, LogicWire.OnSpawn or Wire.OnSpawn adds
		/// the item).
		/// </summary>
		protected override void OnSpawn()
		{
			connectionManager = ConnectionManager;
			controller = GetComponent<KBatchedAnimController>();
			// The wire art has an "outline" symbol, a white band behind any segment, which a real
			// wire hides as it spawns (Wire.OnSpawn, LogicWire.OnSpawn); same name in both.
			controller.SetSymbolVisiblity(Wire.OutlineSymbol, is_visible: false);
			if (Grid.Objects[Cell, TileLayer] != null)
			{
				// Something took the cell between the bridge's check and this frame's Start.
				Release();
				Util.KDestroyGameObject(gameObject);
				return;
			}
			Grid.Objects[Cell, TileLayer] = gameObject;
			Kind.Manager.AddToNetworks(Cell, this, is_endpoint: false);
			registered = true;
			RefreshFromNeighbours();
			UpdateArt();
			UtilityConnections inherited = JumperBridge.TakeInherited(Kind, Cell);
			if (inherited != 0)
				InheritFromWire(inherited);
		}

		/// <summary>
		/// A wire that stood here and left (melted, broke) had these connections; the node keeps
		/// them, so the bridge stays joined to what the wire joined it to. The manager's ClearCell
		/// has stripped the neighbours' grid bits toward this cell but not their own stored
		/// Connections, so a neighbour still pointing here has its bits written back first, then
		/// this cell's are set; SetConnections keeps only bits toward physical neighbours, which
		/// is what a wire's would be.
		/// </summary>
		private void InheritFromWire(UtilityConnections connections)
		{
			foreach (UtilityConnections direction in Directions)
			{
				if ((connections & direction) == 0)
					continue;
				int neighbour = direction.CellInDirection(Cell);
				if (!Grid.IsValidCell(neighbour))
					continue;
				KAnimGraphTileVisualizer other = null;
				if (Kind.Nodes.TryGetValue(neighbour, out BridgeEndNode node) && node != null && node.registered)
					other = node;
				else if (Grid.Objects[neighbour, (int)Kind.WireLayer] != null)
					other = Grid.Objects[neighbour, (int)Kind.WireLayer].GetComponent<KAnimGraphTileVisualizer>();
				if (other != null && (other.Connections & direction.InverseDirection()) != 0)
					other.UpdateConnections(other.Connections);
			}
			SetBits(connections);
			Debug.Log(JumperBridgesMod.LogPrefix + Kind + " end node at " + Cell + " inherited " + connections + " from the wire that left");
		}

		/// <summary>Not the base OnCleanUp either (same Building dereference); Release is idempotent.</summary>
		protected override void OnCleanUp()
		{
			Release();
		}

		/// <summary>
		/// Undoes the registrations, each only if still this node's: the layer mark may already be a
		/// wire's, and the item may already be a wire's (then its connections are the wire's too, so
		/// ClearCell must not touch them). Safe to call more than once and before destruction.
		/// </summary>
		public void Release()
		{
			if (released)
				return;
			released = true;
			if (Kind.Nodes.TryGetValue(Cell, out BridgeEndNode current) && current == this)
				Kind.Nodes.Remove(Cell);
			foreach (JumperBridge owner in owners.ToArray())
				if (owner != null)
					owner.Forget(this);
			owners.Clear();
			if (!registered || Game.IsQuitting() || App.IsExiting)
				return;
			if (Grid.Objects[Cell, TileLayer] == gameObject)
				Grid.Objects[Cell, TileLayer] = null;
			IUtilityNetworkMgr system = Kind.Manager;
			if (Kind.Items.TryGetValue(Cell, out object item) && ReferenceEquals(item, this))
			{
				// RemoveFromNetworks sends the port at this cell its disconnect, as a wire's removal does.
				system.RemoveFromNetworks(Cell, this, is_endpoint: false);
				system.ClearCell(Cell, is_physical_building: true);
			}
			// ClearCell strips the neighbours' grid bits toward this cell but not a neighbour wire's own
			// stored Connections; a wire's removal fixes that through this call, which has every
			// neighbour Refresh() from the grid. Without it the stale bit would be saved, and would
			// reconnect a node recreated here on the next load without any drag.
			TileVisualizer.RefreshCell(Cell, Kind.TileLayer, Kind.ReplacementLayer);
			RefreshNeighbours(Kind, Cell);
		}

		/// <summary>A second bridge with an end on this cell shares the node.</summary>
		public void AddOwner(JumperBridge bridge)
		{
			if (!owners.Contains(bridge))
				owners.Add(bridge);
			UpdateArt();
		}

		/// <summary>An owner is going; the node stays for the others and leaves with the last.</summary>
		public void Disown(JumperBridge bridge)
		{
			owners.Remove(bridge);
			if (released)
				return;
			if (owners.Count > 0)
			{
				UpdateArt();
				return;
			}
			Release();
			Util.KDestroyGameObject(gameObject);
		}

		/// <summary>
		/// A power end shared by a joint plate and a wire bridge draws as the plate's heavi-watt
		/// wire while any plate has an end here, whichever was built first; otherwise it keeps the
		/// art it has while that owner stays, and takes the first remaining owner's when it goes.
		/// The kanim is swapped on the existing controller, and Refresh replays the segment.
		/// </summary>
		private void UpdateArt()
		{
			if (Kind != BridgeKind.Power || released || !registered || owners.Count == 0)
				return;
			bool plateHere = owners.Exists(owner => owner != null && BridgeKind.IsJointPlate(owner.gameObject));
			if (artOwner != null && owners.Contains(artOwner) && (!plateHere || BridgeKind.IsJointPlate(artOwner.gameObject)))
				return;
			JumperBridge chosen = plateHere
				? owners.Find(owner => owner != null && BridgeKind.IsJointPlate(owner.gameObject))
				: owners.Find(owner => owner != null);
			if (chosen == null)
				return;
			artOwner = chosen;
			KAnimFile anim = Assets.GetAnim(Kind.NodeAnim(chosen.gameObject));
			if (controller.AnimFiles[0] == anim)
				return;
			controller.SwapAnims(new KAnimFile[] { anim });
			controller.SetSymbolVisiblity(Wire.OutlineSymbol, is_visible: false);
			Refresh();
		}

		/// <summary>Sets this node's bits outright, for the disconnect tool; mirrors UpdateConnections + Refresh on a wire.</summary>
		public void SetBits(UtilityConnections bits)
		{
			if (released || !registered)
				return;
			Kind.Manager.SetConnections(bits, Cell, is_physical_building: true);
			RedrawWithNeighbours();
		}

		/// <summary>
		/// What a wire does after its bits change: this cell and its four neighbours re-read
		/// their connections from the grid and replay their anim (TileVisualizer.RefreshCell calls
		/// Refresh() on the KAnimGraphTileVisualizer of each). Without it a wire that led into this
		/// cell keeps its cached segment toward it after the node is gone.
		/// </summary>
		private void RedrawWithNeighbours()
		{
			Refresh();
			TileVisualizer.RefreshCell(Cell, Kind.TileLayer, Kind.ReplacementLayer);
		}

		/// <summary>
		/// The disconnect tool cutting the given directions at this cell. A wire on the far side
		/// drops its own bit through the tool's vanilla pass (both cells are inside the box, and
		/// its UpdateConnections makes this node recompute); a node on the far side has nothing
		/// vanilla touches, and since two nodes keep each other's bits alive by mirroring, both
		/// sides are cleared here before either could recompute.
		/// </summary>
		public void Cut(UtilityConnections remove)
		{
			if (released || !registered || remove == 0)
				return;
			IUtilityNetworkMgr system = Kind.Manager;
			foreach (UtilityConnections direction in Directions)
			{
				if ((remove & direction) == 0)
					continue;
				int neighbour = direction.CellInDirection(Cell);
				if (Grid.IsValidCell(neighbour) && Kind.Nodes.TryGetValue(neighbour, out BridgeEndNode other) && other != null)
					other.SetBits(system.GetConnections(neighbour, is_physical_building: false) & ~direction.InverseDirection());
			}
			SetBits(system.GetConnections(Cell, is_physical_building: false) & ~remove);
			system.ForceRebuildNetworks();
		}

		private static readonly UtilityConnections[] Directions =
			{ UtilityConnections.Left, UtilityConnections.Right, UtilityConnections.Up, UtilityConnections.Down };

		/// <summary>
		/// A node has no connections of its own; it points wherever a neighbour points at it. The
		/// drag tool never writes a node's bits (BaseUtilityBuildTool.BuildPath only calls
		/// UpdateConnections on objects with a BuildingComplete), so they are derived from the
		/// neighbours' display bits, which a wire under construction already has toward the node;
		/// SetConnections keeps the physical bits only toward neighbours that are physical nodes
		/// and its Reconnect writes the inverse bit back on them, as for a wire.
		/// </summary>
		public void RefreshFromNeighbours()
		{
			if (released || !registered)
				return;
			IUtilityNetworkMgr system = Kind.Manager;
			UtilityConnections bits = 0;
			Vector2I xy = Grid.CellToXY(Cell);
			if (xy.x > 0 && (system.GetDisplayConnections(Grid.CellLeft(Cell)) & UtilityConnections.Right) != 0)
				bits |= UtilityConnections.Left;
			if (xy.x < Grid.WidthInCells - 1 && (system.GetDisplayConnections(Grid.CellRight(Cell)) & UtilityConnections.Left) != 0)
				bits |= UtilityConnections.Right;
			if (xy.y > 0 && (system.GetDisplayConnections(Grid.CellBelow(Cell)) & UtilityConnections.Up) != 0)
				bits |= UtilityConnections.Down;
			if (xy.y < Grid.HeightInCells - 1 && (system.GetDisplayConnections(Grid.CellAbove(Cell)) & UtilityConnections.Down) != 0)
				bits |= UtilityConnections.Up;
			system.SetConnections(bits, Cell, is_physical_building: true);
			RedrawWithNeighbours();
		}

		/// <summary>Recomputes the nodes in the four cells around a cell whose wire just changed.</summary>
		public static void RefreshNeighbours(BridgeKind kind, int cell)
		{
			Vector2I xy = Grid.CellToXY(cell);
			if (xy.x > 0)
				RefreshAt(kind, Grid.CellLeft(cell));
			if (xy.x < Grid.WidthInCells - 1)
				RefreshAt(kind, Grid.CellRight(cell));
			if (xy.y > 0)
				RefreshAt(kind, Grid.CellBelow(cell));
			if (xy.y < Grid.HeightInCells - 1)
				RefreshAt(kind, Grid.CellAbove(cell));
		}

		private static void RefreshAt(BridgeKind kind, int cell)
		{
			if (kind.Nodes.TryGetValue(cell, out BridgeEndNode node) && node != null)
				node.RefreshFromNeighbours();
		}

		/// <summary>After a wire drag: the drag writes display bits into node cells it passed through.</summary>
		public static void RefreshAll(BridgeKind kind)
		{
			if (kind.Nodes.Count == 0)
				return;
			foreach (BridgeEndNode node in new List<BridgeEndNode>(kind.Nodes.Values))
				if (node != null)
					node.RefreshFromNeighbours();
		}

		/// <summary>A real wire is spawning on the cell: the node steps aside before the wire registers.</summary>
		public static void Evict(BridgeKind kind, int cell)
		{
			if (!kind.Nodes.TryGetValue(cell, out BridgeEndNode node) || node == null)
				return;
			node.Release();
			Util.KDestroyGameObject(node.gameObject);
		}

		/// <summary>
		/// The automation overlay colours wires by the network at their cell and lifts them onto its
		/// masked camera layer; it finds them through their SaveLoadRoot and prefab tag, which the
		/// node has neither of, so the same colouring is applied here from the overlay's Update.
		/// </summary>
		public void ApplyLogicOverlay(int layer, Color32 on, Color32 off, HashSet<UtilityNetwork> highlighted, float highlightScale)
		{
			if (controller == null)
				return;
			controller.SetLayer(layer);
			LogicCircuitNetwork network = Game.Instance.logicCircuitManager.GetNetworkForCell(Cell);
			bool scale = network != null && highlighted.Count > 0 && highlighted.Contains(network);
			if (Ribbon)
			{
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_1_SYMBOL_NAME, Tint(network, 0, on, off, scale, highlightScale));
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_2_SYMBOL_NAME, Tint(network, 1, on, off, scale, highlightScale));
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_3_SYMBOL_NAME, Tint(network, 2, on, off, scale, highlightScale));
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_4_SYMBOL_NAME, Tint(network, 3, on, off, scale, highlightScale));
			}
			else
				controller.TintColour = Tint(network, 0, on, off, scale, highlightScale);
		}

		private static Color32 Tint(LogicCircuitNetwork network, int bit, Color32 on, Color32 off, bool scale, float highlightScale)
		{
			Color32 tint = network != null && network.IsBitActive(bit) ? on : off;
			return scale ? Scaled(tint, highlightScale) : tint;
		}

		/// <summary>The power overlay's colouring, worked out by the caller from the circuit at this cell.</summary>
		public void ApplyPowerOverlay(int layer, Color32 tint)
		{
			if (controller == null)
				return;
			controller.SetLayer(layer);
			controller.TintColour = tint;
		}

		public static Color32 Scaled(Color32 tint, float scale)
		{
			tint.r = (byte)(tint.r * scale);
			tint.g = (byte)(tint.g * scale);
			tint.b = (byte)(tint.b * scale);
			return tint;
		}

		/// <summary>Back to the plain look when the overlay closes, as Mode.ResetDisplayValues does for wires.</summary>
		public void ResetOverlay()
		{
			if (controller == null)
				return;
			controller.SetLayer(0);
			controller.TintColour = Color.white;
			if (Ribbon)
			{
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_1_SYMBOL_NAME, Color.white);
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_2_SYMBOL_NAME, Color.white);
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_3_SYMBOL_NAME, Color.white);
				controller.SetSymbolTint(OverlayModes.Logic.RIBBON_WIRE_4_SYMBOL_NAME, Color.white);
			}
		}
	}
}
