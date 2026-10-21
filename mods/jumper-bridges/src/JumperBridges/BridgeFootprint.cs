#pragma warning disable 649 // [MyCmpReq] fields are populated by the game via reflection
using System.Collections.Generic;
using UnityEngine;

namespace JumperBridges
{
	/// <summary>
	/// Where every bridge is, by cell, for the tools that sweep Grid.Objects. The game only puts a
	/// bridge on its kind's end layer (LogicGate, WireConnectors) at its two end cells, and this mod
	/// stops a bridge from taking that layer slot away from a port or an earlier bridge
	/// (BuildingDef.MarkArea prefix), so a bridge whose end cells are both taken is on no layer at all. The deconstruct, cancel and
	/// prioritize tools then find it here instead, by its end cells. On the under-construction
	/// and the complete prefab.
	/// </summary>
	public sealed class BridgeFootprint : KMonoBehaviour
	{
		private static readonly Dictionary<int, List<BridgeFootprint>> byCell = new Dictionary<int, List<BridgeFootprint>>();

		[MyCmpReq] private UtilityNetworkLink link;

		private readonly List<int> cells = new List<int>(3);
		private int end1 = Grid.InvalidCell, end2 = Grid.InvalidCell;

		public BridgeKind Kind { get; private set; }

		public bool IsEnd(int cell) => cell == end1 || cell == end2;

		/// <summary>The bridges with an end on the cell; a fresh list.</summary>
		public static List<GameObject> EndsAt(int cell)
		{
			List<GameObject> result = new List<GameObject>();
			if (byCell.TryGetValue(cell, out List<BridgeFootprint> list))
				foreach (BridgeFootprint footprint in list)
					if (footprint != null && footprint.IsEnd(cell))
						result.Add(footprint.gameObject);
			return result;
		}

		/// <summary>Whether a power bridge other than self, of the joint plate kind asked for, has an end on the cell; allocates nothing.</summary>
		public static bool AnyPowerEndAt(int cell, GameObject self, bool jointPlate)
		{
			if (!byCell.TryGetValue(cell, out List<BridgeFootprint> list))
				return false;
			foreach (BridgeFootprint footprint in list)
				if (footprint != null && footprint.gameObject != self && footprint.IsEnd(cell) && footprint.Kind == BridgeKind.Power && BridgeKind.IsJointPlate(footprint.gameObject) == jointPlate)
					return true;
			return false;
		}

		/// <summary>The bridges with an end or their middle on the cell; a fresh list.</summary>
		public static List<GameObject> At(int cell)
		{
			List<GameObject> result = new List<GameObject>();
			if (byCell.TryGetValue(cell, out List<BridgeFootprint> list))
				foreach (BridgeFootprint footprint in list)
					if (footprint != null)
						result.Add(footprint.gameObject);
			return result;
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			Kind = BridgeKind.OfBridge(gameObject);
			link.GetCells(out end1, out end2);
			Add(end1);
			Add(Grid.PosToCell(this));
			Add(end2);
		}

		protected override void OnCleanUp()
		{
			foreach (int cell in cells)
				if (byCell.TryGetValue(cell, out List<BridgeFootprint> list))
				{
					list.Remove(this);
					if (list.Count == 0)
						byCell.Remove(cell);
				}
			cells.Clear();
			base.OnCleanUp();
		}

		private void Add(int cell)
		{
			if (!Grid.IsValidCell(cell))
				return;
			if (!byCell.TryGetValue(cell, out List<BridgeFootprint> list))
				byCell[cell] = list = new List<BridgeFootprint>();
			list.Add(this);
			cells.Add(cell);
		}
	}
}
