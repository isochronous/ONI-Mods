using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace JumperBridges
{
	/// <summary>
	/// What differs between automation bridges and power bridges. Both networks are the game's
	/// generic UtilityNetworkManager, flood filled over cells that hold something on the wire tile
	/// layer, with bridge ends linked through its one-partner-per-cell link table, so everything
	/// else in the mod is shared and reads its layers, manager and art from here. Each kind keeps
	/// its own nodes and links: an automation bridge end and a power bridge end on the same cell
	/// are on two separate networks.
	/// </summary>
	public sealed class BridgeKind
	{
		/// <summary>The Automation Wire Bridge and the Automation Ribbon Bridge.</summary>
		public static readonly BridgeKind Logic = new BridgeKind(
			"automation",
			KAnimGraphTileVisualizer.ConnectionSource.Logic,
			ObjectLayer.LogicWireTile, ObjectLayer.LogicWire, ObjectLayer.ReplacementLogicWire, ObjectLayer.LogicGate,
			Grid.SceneLayer.LogicWires,
			typeof(UtilityNetworkManager<LogicCircuitNetwork, LogicWire>));

		/// <summary>
		/// The Wire Bridge, Conductive Wire Bridge and Insulated Wire Bridge: every building on the
		/// WireBridge placement rule. The heavy-watt joint plates have a rule of their own and are
		/// left alone.
		/// </summary>
		public static readonly BridgeKind Power = new BridgeKind(
			"power",
			KAnimGraphTileVisualizer.ConnectionSource.Electrical,
			ObjectLayer.WireTile, ObjectLayer.Wire, ObjectLayer.ReplacementWire, ObjectLayer.WireConnectors,
			Grid.SceneLayer.Wires,
			typeof(UtilityNetworkManager<ElectricalUtilityNetwork, Wire>));

		public static readonly BridgeKind[] All = { Logic, Power };

		public readonly string Name;
		public readonly KAnimGraphTileVisualizer.ConnectionSource Source;
		/// <summary>The layer the network's flood fill requires a cell to hold something on.</summary>
		public readonly ObjectLayer TileLayer;
		/// <summary>The layer the wire buildings themselves are on.</summary>
		public readonly ObjectLayer WireLayer;
		public readonly ObjectLayer ReplacementLayer;
		/// <summary>The layer the game marks a bridge on at its two end cells, which the tools sweep.</summary>
		public readonly ObjectLayer EndLayer;
		public readonly Grid.SceneLayer SceneLayer;

		/// <summary>Set from the options when the mod loads; a kind that is off is left entirely to the game.</summary>
		public bool Enabled { get; set; }

		/// <summary>Live nodes by cell; at most one per cell because the manager keeps one item per cell.</summary>
		internal readonly Dictionary<int, BridgeEndNode> Nodes = new Dictionary<int, BridgeEndNode>();

		/// <summary>The manager's item and link tables are private and have no getters; read by FieldInfo (cold paths).</summary>
		private readonly FieldInfo itemsField, linksField;

		private BridgeKind(string name, KAnimGraphTileVisualizer.ConnectionSource source, ObjectLayer tileLayer, ObjectLayer wireLayer,
			ObjectLayer replacementLayer, ObjectLayer endLayer, Grid.SceneLayer sceneLayer, System.Type managerType)
		{
			Name = name;
			Source = source;
			TileLayer = tileLayer;
			WireLayer = wireLayer;
			ReplacementLayer = replacementLayer;
			EndLayer = endLayer;
			SceneLayer = sceneLayer;
			itemsField = AccessTools.Field(managerType, "items");
			linksField = AccessTools.Field(managerType, "links");
		}

		public IUtilityNetworkMgr Manager => this == Logic ? Game.Instance.logicCircuitSystem : (IUtilityNetworkMgr)Game.Instance.electricalConduitSystem;

		public Dictionary<int, object> Items => (Dictionary<int, object>)itemsField.GetValue(Manager);

		public Dictionary<int, int> Links => (Dictionary<int, int>)linksField.GetValue(Manager);

		/// <summary>The tool filter the kind's wires and bridges fall under (ToolParameterMenu.FILTERLAYERS).</summary>
		public string FilterLayer => this == Logic ? ToolParameterMenu.FILTERLAYERS.LOGIC : ToolParameterMenu.FILTERLAYERS.WIRES;

		/// <summary>The kind a bridge's placement rule belongs to; null for every other building.</summary>
		public static BridgeKind Of(BuildingDef def)
		{
			switch (def.BuildLocationRule)
			{
				case BuildLocationRule.LogicBridge:
					return Logic;
				case BuildLocationRule.WireBridge:
					return Power;
				default:
					return null;
			}
		}

		public static BridgeKind Of(KAnimGraphTileVisualizer.ConnectionSource source)
		{
			if (source == KAnimGraphTileVisualizer.ConnectionSource.Logic)
				return Logic;
			if (source == KAnimGraphTileVisualizer.ConnectionSource.Electrical)
				return Power;
			return null;
		}

		/// <summary>The enabled kind of a bridge building or prefab; null for anything else, a heavy-watt joint plate included.</summary>
		public static BridgeKind OfBridge(GameObject go)
		{
			if (go == null)
				return null;
			Building building = go.GetComponent<Building>();
			BridgeKind kind = building != null ? Of(building.Def) : null;
			return kind != null && kind.Enabled ? kind : null;
		}

		/// <summary>
		/// The wire art an end node plays: the wire the bridge is the bridge of. Automation:
		/// wire or ribbon. Power: the plain, conductive or insulated wire, by the bridge.
		/// </summary>
		public string NodeAnim(GameObject bridge)
		{
			if (this == Logic)
			{
				LogicUtilityNetworkLink link = bridge.GetComponent<LogicUtilityNetworkLink>();
				return link != null && link.bitDepth == LogicWire.BitDepth.FourBit ? "logic_ribbon_kanim" : "logic_wires_kanim";
			}
			switch (bridge.PrefabID().Name)
			{
				case WireRefinedBridgeConfig.ID:
					return "utilities_electric_conduct_kanim";
				case WireRubberBridgeConfig.ID:
					return "utilities_electric_rubber_kanim";
				default:
					return "utilities_electric_kanim";
			}
		}

		public override string ToString() => Name;
	}
}
