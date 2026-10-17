using System.Collections.Generic;

namespace JumperBridges
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
	/// belonging to another bridge is repaired at once. The generic manager is not patched. Groups
	/// are per kind: an automation bridge and a power bridge sharing a cell are on two managers.
	/// </summary>
	internal static class BridgeLinks
	{
		private static readonly Dictionary<UtilityNetworkLink, BridgeKind> connected = new Dictionary<UtilityNetworkLink, BridgeKind>();

		public static void OnConnect(UtilityNetworkLink link, BridgeKind kind)
		{
			if (Game.IsQuitting())
				return;
			connected[link] = kind;
			link.GetCells(out int cell1, out _);
			RewriteGroupAt(kind, cell1);
		}

		public static void OnDisconnect(UtilityNetworkLink link, BridgeKind kind, int cell1, int cell2)
		{
			connected.Remove(link);
			if (Game.IsQuitting())
			{
				connected.Clear();
				return;
			}
			RewriteGroupAt(kind, cell1);
			RewriteGroupAt(kind, cell2);
		}

		/// <summary>Rewrites the ring of the group that has a bridge end on the cell; nothing if none has.</summary>
		private static void RewriteGroupAt(BridgeKind kind, int cell)
		{
			List<int> cells = GroupCells(kind, cell);
			if (cells.Count < 2)
				return;
			cells.Sort();
			Dictionary<int, int> links = kind.Links;
			for (int i = 0; i < cells.Count; i++)
				links[cells[i]] = cells[(i + 1) % cells.Count];
			kind.Manager.ForceRebuildNetworks();
		}

		/// <summary>The distinct end cells of every connected bridge of the kind reachable from the cell through shared end cells.</summary>
		private static List<int> GroupCells(BridgeKind kind, int start)
		{
			foreach (UtilityNetworkLink gone in new List<UtilityNetworkLink>(connected.Keys))
				if (gone == null)
					connected.Remove(gone);
			Dictionary<int, List<UtilityNetworkLink>> byCell = new Dictionary<int, List<UtilityNetworkLink>>();
			Dictionary<UtilityNetworkLink, (int, int)> ends = new Dictionary<UtilityNetworkLink, (int, int)>();
			foreach (KeyValuePair<UtilityNetworkLink, BridgeKind> entry in connected)
			{
				if (entry.Value != kind)
					continue;
				entry.Key.GetCells(out int cell1, out int cell2);
				ends[entry.Key] = (cell1, cell2);
				Index(byCell, cell1, entry.Key);
				Index(byCell, cell2, entry.Key);
			}
			List<int> cells = new List<int>();
			HashSet<int> seenCells = new HashSet<int>();
			HashSet<UtilityNetworkLink> seenLinks = new HashSet<UtilityNetworkLink>();
			Queue<int> queue = new Queue<int>();
			if (!byCell.ContainsKey(start))
				return cells;
			queue.Enqueue(start);
			seenCells.Add(start);
			while (queue.Count > 0)
			{
				int cell = queue.Dequeue();
				cells.Add(cell);
				foreach (UtilityNetworkLink link in byCell[cell])
				{
					if (!seenLinks.Add(link))
						continue;
					(int cell1, int cell2) = ends[link];
					int other = cell1 == cell ? cell2 : cell1;
					if (seenCells.Add(other))
						queue.Enqueue(other);
				}
			}
			return cells;
		}

		private static void Index(Dictionary<int, List<UtilityNetworkLink>> byCell, int cell, UtilityNetworkLink link)
		{
			if (!byCell.TryGetValue(cell, out List<UtilityNetworkLink> list))
				byCell[cell] = list = new List<UtilityNetworkLink>();
			list.Add(link);
		}
	}
}
