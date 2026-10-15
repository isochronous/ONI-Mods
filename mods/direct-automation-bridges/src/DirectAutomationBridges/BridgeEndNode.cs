using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DirectAutomationBridges
{
	/// <summary>
	/// The stand-in for a wire under a bridge end. The automation network only asks three things
	/// of a cell before it carries a signal across it: something on the LogicWireTile layer
	/// (UtilityNetworkManager.QueueCellForVisit), a physical node with connection bits
	/// (SetConnections), and an item at the cell (AddToNetworks) so the port there is told it is
	/// connected. A wire building answers all three through its KAnimGraphTileVisualizer and
	/// LogicWire; this node answers them with no building at all, so there is no cost, no yield,
	/// nothing to deconstruct and nothing to save. It plays the wire kanim through the inherited
	/// Refresh(), which is how a real wire picks its "L", "LR", "LRU"... art, so a wire dragged up
	/// to a bridge end gets the matching segment drawn back at it.
	///
	/// The rule: a bridge end behaves exactly like a wire cell. It connects only to cells a wire
	/// drag explicitly joined it to, never because a wire merely sits next to it. The node has
	/// no connections of its own; it mirrors the bits its neighbours point at it, and a neighbour
	/// only points at a cell because a drag paired the two (ApplyPathToConduitSystem), because its
	/// saved connections did, or because the manager's Reconnect echoed a bit the other side had.
	/// </summary>
	public sealed class BridgeEndNode : KAnimGraphTileVisualizer
	{
		public const int TileLayer = (int)ObjectLayer.LogicWireTile;

		private const string WireAnim = "logic_wires_kanim";
		private const string RibbonAnim = "logic_ribbon_kanim";

		/// <summary>Live, registered nodes by cell; at most one per cell because the manager keeps one item per cell.</summary>
		private static readonly Dictionary<int, BridgeEndNode> byCell = new Dictionary<int, BridgeEndNode>();

		/// <summary>
		/// The manager's item table is private and has no getter. A node must only unregister
		/// its own item: when a real wire has already taken the cell (LogicWire.OnSpawn stomps
		/// the item first), removing "the item at this cell" would remove the wire's. Read by
		/// FieldInfo rather than FieldRefAccess: it is a cold path, and a FieldRef's dynamic
		/// method cannot be built on the CoreCLR host that ModCheck runs on.
		/// </summary>
		private static readonly FieldInfo ItemsField =
			AccessTools.Field(typeof(UtilityNetworkManager<LogicCircuitNetwork, LogicWire>), "items");

		/// <summary>
		/// The bridges whose end is this cell. Two bridge ends may share a cell; the cell gets one
		/// node (the manager keeps one item per cell), the later bridge adopts it, and the node
		/// goes when the last owner does.
		/// </summary>
		private readonly List<DirectBridge> owners = new List<DirectBridge>();

		public int Cell { get; private set; }
		public bool Ribbon { get; private set; }

		private KBatchedAnimController controller;
		private bool registered;
		private bool released;

		public static IEnumerable<BridgeEndNode> All => byCell.Values;

		public static int Count => byCell.Count;

		public static bool TryGet(int cell, out BridgeEndNode node) => byCell.TryGetValue(cell, out node);

		/// <summary>
		/// A standalone GameObject at the cell, on the wire scene layer so the bridge art (LogicGates,
		/// one layer up) draws over it, built the way the game builds its own runtime controllers
		/// (AutoMiner's arm): inactive, controller configured, then activated.
		/// </summary>
		public static BridgeEndNode Create(DirectBridge bridge, int cell, bool ribbon)
		{
			GameObject go = new GameObject("DirectBridgeEnd");
			go.SetActive(false);
			go.transform.SetPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.LogicWires));
			KBatchedAnimController controller = go.AddComponent<KBatchedAnimController>();
			controller.AnimFiles = new KAnimFile[] { Assets.GetAnim(ribbon ? RibbonAnim : WireAnim) };
			controller.initialAnim = "None";
			controller.sceneLayer = Grid.SceneLayer.LogicWires;
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
			node.Cell = cell;
			node.Ribbon = ribbon;
			node.connectionSource = ConnectionSource.Logic;
			node.isPhysicalBuilding = true;
			// In the registry from the start, not from OnSpawn (which runs at the next Start phase):
			// two bridges sharing this cell create in the same scheduler tick, and the second must
			// find and adopt this node rather than make its own. Everything that reads the
			// registry checks that the node is registered with the manager before acting on it.
			byCell[cell] = node;
			go.SetActive(true);
			return node;
		}

		/// <summary>
		/// Not the base OnSpawn: that one ends in TileVisualizer.RefreshCell through
		/// GetComponent&lt;Building&gt;().Def, and the node has no Building. The three
		/// registrations are the ones a wire makes (BuildingDef.Build marks the tile layer,
		/// KAnimGraphTileVisualizer.OnSpawn sets the node, LogicWire.OnSpawn adds the item).
		/// </summary>
		protected override void OnSpawn()
		{
			connectionManager = ConnectionManager;
			controller = GetComponent<KBatchedAnimController>();
			if (Grid.Objects[Cell, TileLayer] != null)
			{
				// Something took the cell between the bridge's check and this frame's Start.
				Release();
				Util.KDestroyGameObject(gameObject);
				return;
			}
			Grid.Objects[Cell, TileLayer] = gameObject;
			Game.Instance.logicCircuitSystem.AddToNetworks(Cell, this, is_endpoint: false);
			registered = true;
			RefreshFromNeighbours();
			UtilityConnections inherited = DirectBridge.TakeInherited(Cell);
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
				if (byCell.TryGetValue(neighbour, out BridgeEndNode node) && node != null && node.registered)
					other = node;
				else if (Grid.Objects[neighbour, (int)ObjectLayer.LogicWire] != null)
					other = Grid.Objects[neighbour, (int)ObjectLayer.LogicWire].GetComponent<KAnimGraphTileVisualizer>();
				if (other != null && (other.Connections & direction.InverseDirection()) != 0)
					other.UpdateConnections(other.Connections);
			}
			SetBits(connections);
			Debug.Log(DirectAutomationBridgesMod.LogPrefix + "end node at " + Cell + " inherited " + connections + " from the wire that left");
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
			if (byCell.TryGetValue(Cell, out BridgeEndNode current) && current == this)
				byCell.Remove(Cell);
			foreach (DirectBridge owner in owners.ToArray())
				if (owner != null)
					owner.Forget(this);
			owners.Clear();
			if (!registered || Game.IsQuitting() || App.IsExiting)
				return;
			if (Grid.Objects[Cell, TileLayer] == gameObject)
				Grid.Objects[Cell, TileLayer] = null;
			UtilityNetworkManager<LogicCircuitNetwork, LogicWire> system = Game.Instance.logicCircuitSystem;
			Dictionary<int, object> items = (Dictionary<int, object>)ItemsField.GetValue(system);
			if (items.TryGetValue(Cell, out object item) && ReferenceEquals(item, this))
			{
				// RemoveFromNetworks sends the port at this cell its disconnect, as a wire's removal does.
				system.RemoveFromNetworks(Cell, this, is_endpoint: false);
				system.ClearCell(Cell, is_physical_building: true);
			}
			// ClearCell strips the neighbours' grid bits toward this cell but not a neighbour wire's own
			// stored Connections; a wire's removal fixes that through this call, which has every
			// neighbour Refresh() from the grid. Without it the stale bit would be saved, and would
			// reconnect a node recreated here on the next load without any drag.
			TileVisualizer.RefreshCell(Cell, ObjectLayer.LogicWireTile, ObjectLayer.ReplacementLogicWire);
			RefreshNeighbours(Cell);
		}

		/// <summary>A second bridge with an end on this cell shares the node.</summary>
		public void AddOwner(DirectBridge bridge)
		{
			if (!owners.Contains(bridge))
				owners.Add(bridge);
		}

		/// <summary>An owner is going; the node stays for the others and leaves with the last.</summary>
		public void Disown(DirectBridge bridge)
		{
			owners.Remove(bridge);
			if (owners.Count > 0 || released)
				return;
			Release();
			Util.KDestroyGameObject(gameObject);
		}

		/// <summary>Sets this node's bits outright, for the disconnect tool; mirrors UpdateConnections + Refresh on a wire.</summary>
		public void SetBits(UtilityConnections bits)
		{
			if (released || !registered)
				return;
			Game.Instance.logicCircuitSystem.SetConnections(bits, Cell, is_physical_building: true);
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
			TileVisualizer.RefreshCell(Cell, ObjectLayer.LogicWireTile, ObjectLayer.ReplacementLogicWire);
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
			UtilityNetworkManager<LogicCircuitNetwork, LogicWire> system = Game.Instance.logicCircuitSystem;
			foreach (UtilityConnections direction in Directions)
			{
				if ((remove & direction) == 0)
					continue;
				int neighbour = direction.CellInDirection(Cell);
				if (Grid.IsValidCell(neighbour) && byCell.TryGetValue(neighbour, out BridgeEndNode other) && other != null)
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
			UtilityNetworkManager<LogicCircuitNetwork, LogicWire> system = Game.Instance.logicCircuitSystem;
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
		public static void RefreshNeighbours(int cell)
		{
			Vector2I xy = Grid.CellToXY(cell);
			if (xy.x > 0)
				RefreshAt(Grid.CellLeft(cell));
			if (xy.x < Grid.WidthInCells - 1)
				RefreshAt(Grid.CellRight(cell));
			if (xy.y > 0)
				RefreshAt(Grid.CellBelow(cell));
			if (xy.y < Grid.HeightInCells - 1)
				RefreshAt(Grid.CellAbove(cell));
		}

		private static void RefreshAt(int cell)
		{
			if (byCell.TryGetValue(cell, out BridgeEndNode node) && node != null)
				node.RefreshFromNeighbours();
		}

		/// <summary>After a wire drag: the drag writes display bits into node cells it passed through.</summary>
		public static void RefreshAll()
		{
			if (byCell.Count == 0)
				return;
			foreach (BridgeEndNode node in new List<BridgeEndNode>(byCell.Values))
				if (node != null)
					node.RefreshFromNeighbours();
		}

		/// <summary>A real wire is spawning on the cell: the node steps aside before the wire registers.</summary>
		public static void Evict(int cell)
		{
			if (!byCell.TryGetValue(cell, out BridgeEndNode node) || node == null)
				return;
			node.Release();
			Util.KDestroyGameObject(node.gameObject);
		}

		/// <summary>
		/// The automation overlay colours wires by the network at their cell and lifts them onto its
		/// masked camera layer; it finds them through their SaveLoadRoot and prefab tag, which the
		/// node has neither of, so the same colouring is applied here from the overlay's Update.
		/// </summary>
		public void ApplyOverlay(int layer, Color32 on, Color32 off, HashSet<UtilityNetwork> highlighted, float highlightScale)
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
			if (scale)
			{
				tint.r = (byte)(tint.r * highlightScale);
				tint.g = (byte)(tint.g * highlightScale);
				tint.b = (byte)(tint.b * highlightScale);
			}
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
