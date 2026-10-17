#pragma warning disable 649 // [MyCmpReq] fields are populated by the game via reflection
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace JumperBridges
{
	/// <summary>
	/// On every complete bridge of an enabled kind. Gives each end that has no wire under it a
	/// BridgeEndNode, so the bridge conducts into whatever port is at that cell, and takes the
	/// nodes away when the bridge goes, breaks, or a real wire arrives. Creation is deferred a
	/// frame so that on load every saved wire has marked its cell before the bridge looks.
	/// </summary>
	public sealed class JumperBridge : KMonoBehaviour
	{
		/// <summary>UtilityNetworkLink.connected is private; a broken bridge has no link and gets no nodes.</summary>
		private static readonly FieldInfo ConnectedField = AccessTools.Field(typeof(UtilityNetworkLink), "connected");

		[MyCmpReq] private UtilityNetworkLink link;

		private readonly BridgeEndNode[] nodes = new BridgeEndNode[2];
		private SchedulerHandle pending;

		public BridgeKind Kind { get; private set; }

		protected override void OnSpawn()
		{
			base.OnSpawn();
			Kind = BridgeKind.OfBridge(gameObject);
			Subscribe((int)GameHashes.BuildingBroken, OnBroken);
			Subscribe((int)GameHashes.BuildingFullyRepaired, OnRepaired);
			ScheduleCreate();
		}

		protected override void OnCleanUp()
		{
			pending.ClearScheduler();
			Unsubscribe((int)GameHashes.BuildingBroken, OnBroken);
			Unsubscribe((int)GameHashes.BuildingFullyRepaired, OnRepaired);
			if (!Game.IsQuitting() && !App.IsExiting)
				RemoveNodes();
			base.OnCleanUp();
		}

		/// <summary>The link disconnects on BuildingBroken (overload damage) and reconnects on repair.</summary>
		private void OnBroken(object data) => RemoveNodes();

		private void OnRepaired(object data) => ScheduleCreate();

		/// <summary>
		/// Connections a wire had when it left a cell a bridge end sits on, for the node that
		/// takes its place (see KAnimGraphTileVisualizer_OnCleanUp_Patch); consumed by the
		/// node when it spawns.
		/// </summary>
		private static readonly Dictionary<(BridgeKind, int), UtilityConnections> inherited = new Dictionary<(BridgeKind, int), UtilityConnections>();

		public static void Inherit(BridgeKind kind, int cell, UtilityConnections connections)
		{
			if (connections != 0)
				inherited[(kind, cell)] = connections;
		}

		/// <summary>What a node at the cell inherits, once; nothing when no wire just left it.</summary>
		public static UtilityConnections TakeInherited(BridgeKind kind, int cell)
		{
			if (!inherited.TryGetValue((kind, cell), out UtilityConnections connections))
				return 0;
			inherited.Remove((kind, cell));
			return connections;
		}

		/// <summary>
		/// Next frame, fill any end that has no wire and no node. Harmless when nothing is missing.
		/// On the UI scheduler, whose clock is real time: the game scheduler runs nothing while the
		/// game clock stands still, so a bridge built while paused would stay unconnected until
		/// the game was unpaused.
		/// </summary>
		public void ScheduleCreate()
		{
			if (pending.IsValid)
				return;
			pending = UIScheduler.Instance.ScheduleNextFrame("JumperBridge.CreateNodes", CreateNodes);
		}

		private void CreateNodes(object data)
		{
			pending = default;
			if (this == null || link == null || Kind == null || !(bool)ConnectedField.GetValue(link))
				return;
			link.GetCells(out int cell1, out int cell2);
			bool created = EnsureNode(0, cell1) | EnsureNode(1, cell2);
			if (created)
				Debug.Log(JumperBridgesMod.LogPrefix + name + " at " + Grid.PosToCell(this) + ": end nodes at " + Describe(0) + " and " + Describe(1));
		}

		/// <summary>
		/// A wire on the cell means the vanilla case, nothing to add. Another bridge's node on the
		/// cell is shared (one item per cell in the manager), so this bridge becomes an owner of it.
		/// </summary>
		private bool EnsureNode(int index, int cell)
		{
			if (nodes[index] != null || !Grid.IsValidCell(cell))
				return false;
			if (BridgeEndNode.TryGet(Kind, cell, out BridgeEndNode shared) && shared != null)
			{
				shared.AddOwner(this);
				nodes[index] = shared;
				return true;
			}
			if (Grid.Objects[cell, (int)Kind.TileLayer] != null)
				return false;
			nodes[index] = BridgeEndNode.Create(this, Kind, cell);
			return true;
		}

		private string Describe(int index) => nodes[index] != null ? nodes[index].Cell.ToString() : "(wire)";

		/// <summary>Every complete bridge of the kind with an end or its middle on the cell.</summary>
		public static List<JumperBridge> At(BridgeKind kind, int cell)
		{
			List<JumperBridge> result = new List<JumperBridge>();
			foreach (GameObject go in BridgeFootprint.At(cell))
			{
				JumperBridge bridge = go.GetComponent<JumperBridge>();
				if (bridge != null && bridge.Kind == kind)
					result.Add(bridge);
			}
			return result;
		}

		/// <summary>The node has released itself (evicted by a wire, or destroyed); the slot is free again.</summary>
		public void Forget(BridgeEndNode node)
		{
			for (int i = 0; i < nodes.Length; i++)
				if (nodes[i] == node)
					nodes[i] = null;
		}

		private void RemoveNodes()
		{
			bool removed = false;
			for (int i = 0; i < nodes.Length; i++)
			{
				BridgeEndNode node = nodes[i];
				nodes[i] = null;
				if (node == null)
					continue;
				node.Disown(this);
				removed = true;
			}
			if (removed)
				Debug.Log(JumperBridgesMod.LogPrefix + name + " at " + Grid.PosToCell(this) + ": end nodes removed");
		}
	}
}
