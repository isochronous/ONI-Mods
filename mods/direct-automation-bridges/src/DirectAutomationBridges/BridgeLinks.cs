using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace DirectAutomationBridges
{
	/// <summary>
	/// The network manager keeps one link partner per cell (AddLink writes links[c1] = c2 and
	/// links[c2] = c1) and its flood fill follows exactly one links[cell] hop from each visited
	/// cell, so two bridge ends on one cell overwrite each other. This owns the link table for
	/// every group of bridges that share cells: after any bridge connects or disconnects, the
	/// distinct cells of its group are written as a directed ring (c0 -> c1 -> ... -> c0). The
	/// fill takes the hop from every cell it visits, so a ring reaches the whole group from any
	/// entry; a group of one bridge is the vanilla pair. The write is idempotent (cells sorted),
	/// and it runs after the game's own AddLink/RemoveLink, so a RemoveLink that took a ring entry
	/// belonging to another bridge is repaired at once. The generic manager is not patched.
	/// </summary>
	internal static class BridgeLinks
	{
		private static readonly FieldInfo LinksField =
			AccessTools.Field(typeof(UtilityNetworkManager<LogicCircuitNetwork, LogicWire>), "links");

		private static readonly HashSet<LogicUtilityNetworkLink> connected = new HashSet<LogicUtilityNetworkLink>();

		public static void OnConnect(LogicUtilityNetworkLink link)
		{
			if (Game.IsQuitting())
				return;
			connected.Add(link);
			RewriteGroupAt(link.cell_one);
		}

		public static void OnDisconnect(LogicUtilityNetworkLink link, int cell1, int cell2)
		{
			connected.Remove(link);
			if (Game.IsQuitting())
			{
				connected.Clear();
				return;
			}
			RewriteGroupAt(cell1);
			RewriteGroupAt(cell2);
		}

		/// <summary>Rewrites the ring of the group that has a bridge end on the cell; nothing if none has.</summary>
		private static void RewriteGroupAt(int cell)
		{
			List<int> cells = GroupCells(cell);
			if (cells.Count < 2)
				return;
			cells.Sort();
			UtilityNetworkManager<LogicCircuitNetwork, LogicWire> system = Game.Instance.logicCircuitSystem;
			Dictionary<int, int> links = (Dictionary<int, int>)LinksField.GetValue(system);
			for (int i = 0; i < cells.Count; i++)
				links[cells[i]] = cells[(i + 1) % cells.Count];
			system.ForceRebuildNetworks();
		}

		/// <summary>The distinct end cells of every connected bridge reachable from the cell through shared end cells.</summary>
		private static List<int> GroupCells(int start)
		{
			connected.RemoveWhere(link => link == null);
			Dictionary<int, List<LogicUtilityNetworkLink>> byCell = new Dictionary<int, List<LogicUtilityNetworkLink>>();
			foreach (LogicUtilityNetworkLink link in connected)
			{
				Index(byCell, link.cell_one, link);
				Index(byCell, link.cell_two, link);
			}
			List<int> cells = new List<int>();
			HashSet<int> seenCells = new HashSet<int>();
			HashSet<LogicUtilityNetworkLink> seenLinks = new HashSet<LogicUtilityNetworkLink>();
			Queue<int> queue = new Queue<int>();
			if (!byCell.ContainsKey(start))
				return cells;
			queue.Enqueue(start);
			seenCells.Add(start);
			while (queue.Count > 0)
			{
				int cell = queue.Dequeue();
				cells.Add(cell);
				foreach (LogicUtilityNetworkLink link in byCell[cell])
				{
					if (!seenLinks.Add(link))
						continue;
					int other = link.cell_one == cell ? link.cell_two : link.cell_one;
					if (seenCells.Add(other))
						queue.Enqueue(other);
				}
			}
			return cells;
		}

		private static void Index(Dictionary<int, List<LogicUtilityNetworkLink>> byCell, int cell, LogicUtilityNetworkLink link)
		{
			if (!byCell.TryGetValue(cell, out List<LogicUtilityNetworkLink> list))
				byCell[cell] = list = new List<LogicUtilityNetworkLink>();
			list.Add(link);
		}
	}
}
