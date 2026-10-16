#pragma warning disable 649 // [MyCmpReq] fields are populated by the game via reflection
using System.Collections.Generic;
using UnityEngine;

namespace DirectAutomationBridges
{
	/// <summary>
	/// Where every bridge is, by cell, for the tools that sweep Grid.Objects. The game only puts a
	/// bridge on the LogicGate layer at its two end cells, and this mod stops a bridge from taking
	/// that layer slot away from a gate or an earlier bridge (BuildingDef.MarkArea prefix), so a
	/// bridge whose end cells are both taken is on no layer at all. The deconstruct, cancel and
	/// prioritize tools then find it here instead, by its end cells. On the under-construction
	/// and the complete prefab.
	/// </summary>
	public sealed class BridgeFootprint : KMonoBehaviour
	{
		private static readonly Dictionary<int, List<BridgeFootprint>> byCell = new Dictionary<int, List<BridgeFootprint>>();

		[MyCmpReq] private LogicUtilityNetworkLink link;

		private readonly List<int> cells = new List<int>(3);
		private int end1 = Grid.InvalidCell, end2 = Grid.InvalidCell;

		/// <summary>The bridges with an end on the cell; a fresh list.</summary>
		public static List<GameObject> EndsAt(int cell)
		{
			List<GameObject> result = new List<GameObject>();
			if (byCell.TryGetValue(cell, out List<BridgeFootprint> list))
				foreach (BridgeFootprint footprint in list)
					if (footprint != null && (footprint.end1 == cell || footprint.end2 == cell))
						result.Add(footprint.gameObject);
			return result;
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
