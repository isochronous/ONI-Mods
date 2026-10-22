using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using UnityEngine;

namespace JustFallAlready
{
	/// <summary>
	/// Makes a critter fall the moment a door takes its footing away, instead of standing there until
	/// it figures out it is supposed to. Whether a critter should fall is a behaviour precondition its
	/// brain evaluates on the brain scheduler's round-robin, so the game itself can leave it hanging
	/// for a while, as in a standard pez dropper (a pneumatic door closing on a critter that stands on
	/// another pneumatic door).
	///
	/// When a door finishes changing state (its state machine enters open, closed or locked after the
	/// animation), its cells are armed: the door cells themselves, which is the pez dropper, and, when
	/// Sgt_Imalas's Critters Fall Through Open Doors is installed and makes open doors open air, the
	/// cells around them. The door marks those cells dirty at the same moment, and every navigation
	/// grid raises an event once it has recomputed a batch of cells; the batches that hold an armed
	/// cell are the ones where the critters' footing has actually changed. A critter standing in such
	/// a cell whose own navigation type is no longer valid there has its brain updated right away,
	/// which runs the game's normal chore selection and with it the fall check. Once the last grid
	/// has had its batch, the cell is disarmed. Nothing else is ever inspected: a door starting to
	/// move, another door, or the stream of batches from digging and building cannot trigger a poke.
	///
	/// Telemetry: every poke is logged when the game was started by the test runner (any
	/// --onitest-* argument), which is what the tests read; any handler run over a few milliseconds is
	/// always logged with its numbers. The event fires on the main thread inside the game's own
	/// graph update, with and without Fast Track (its path cache keeps that call), and updating a critter brain directly from
	/// the main thread is what Fast Track itself does for non-duplicant brains it queues.
	/// </summary>
	[HarmonyPatch(typeof(Pathfinding), nameof(Pathfinding.AddNavGrid))]
	public static class FallPoke
	{
		/// <summary>Armed cells are dropped if no navigation batch touches them within this time (it always should).</summary>
		private const float ArmSeconds = 2f;
		private const double SlowMilliseconds = 2.0;
		/// <summary>Whether the game was started by the test runner (an --onitest-* argument); only then is each poke logged.</summary>
		private static readonly bool LogPokes = StartedByTestRunner();

		/// <summary>Armed cell -> the time it was armed.</summary>
		private static readonly Dictionary<int, float> armed = new Dictionary<int, float>();
		private static readonly List<int> scratch = new List<int>();
		/// <summary>The grid registered last; its batch for a set of dirty cells comes after every other grid's.</summary>
		private static NavGrid lastGrid;
		private static bool? openDoorsAreAir;
		private static int cacheFrame = -1;
		/// <summary>Critters in armed cells, gathered at most once per frame; arming a cell clears it.</summary>
		private static readonly Dictionary<int, List<Brain>> crittersByCell = new Dictionary<int, List<Brain>>();
		private static readonly Stack<List<Brain>> spareLists = new Stack<List<Brain>>();

		public static void Postfix(NavGrid nav_grid)
		{
			lastGrid = nav_grid;
			nav_grid.OnNavGridUpdateComplete += cells => OnGridUpdated(nav_grid, cells);
		}

		/// <summary>
		/// A door finished changing state: arm its cells. The door updates its world state twice: at the
		/// start, when its control setting changes (updateSim false), and when its state machine enters
		/// open, closed or locked after the animation (updateSim true). Only the second counts, so a
		/// critter is not dropped while the door is still visibly moving.
		/// </summary>
		[HarmonyPatch(typeof(Door), "SetWorldState")]
		public static class Door_SetWorldState_Patch
		{
			public static void Postfix(Door __instance, bool updateSim)
			{
				if (!updateSim || KMonoBehaviour.isLoadingScene)
					return;
				Building building = __instance.GetComponent<Building>();
				if (building == null)
					return;
				float now = Time.realtimeSinceStartup;
				Purge(now);
				// The armed set changes: gather the critters again on the next lookup, even within this frame.
				cacheFrame = -1;
				bool around = OpenDoorsAreAir;
				foreach (int cell in building.PlacementCells)
				{
					armed[cell] = now;
					if (!around)
						continue;
					Arm(Grid.CellBelow(cell), now);
					Arm(Grid.CellAbove(cell), now);
					Arm(Grid.CellLeft(cell), now);
					Arm(Grid.CellRight(cell), now);
				}
			}
		}

		private static void Arm(int cell, float now)
		{
			if (Grid.IsValidCell(cell))
				armed[cell] = now;
		}

		/// <summary>Drops armed cells no batch ever touched.</summary>
		private static void Purge(float now)
		{
			if (armed.Count == 0)
				return;
			scratch.Clear();
			foreach (KeyValuePair<int, float> pair in armed)
				if (now - pair.Value > ArmSeconds)
					scratch.Add(pair.Key);
			foreach (int cell in scratch)
				armed.Remove(cell);
		}

		private static void OnGridUpdated(NavGrid grid, List<int> cells)
		{
			if (armed.Count == 0 || KMonoBehaviour.isLoadingScene)
				return;
			long start = Stopwatch.GetTimestamp();
			int pokes = 0, hits = 0;
			scratch.Clear();
			foreach (int cell in cells)
			{
				if (!armed.ContainsKey(cell))
					continue;
				hits++;
				if (CrittersAt(cell, out List<Brain> critters))
				{
					foreach (Brain brain in critters)
					{
						Navigator navigator = brain.GetComponent<Navigator>();
						if (navigator == null || navigator.NavGrid != grid || !brain.IsRunning())
							continue;
						if (grid.NavTable.IsValid(cell, navigator.CurrentNavType))
							continue;
						brain.UpdateBrain();
						pokes++;
						if (LogPokes)
							UnityEngine.Debug.Log("[JustFallAlready] " + brain.name + " at cell " + cell + " lost its " + navigator.CurrentNavType + " footing to a door; told it to fall");
					}
				}
				// Every grid gets its own batch for the same dirty cells, in registration order; disarm after the last one.
				if (grid == lastGrid)
					scratch.Add(cell);
			}
			foreach (int cell in scratch)
				armed.Remove(cell);
			double ms = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
			if (ms > SlowMilliseconds)
				UnityEngine.Debug.Log("[JustFallAlready] " + grid.id + " check took " + ms.ToString("F1") + " ms: " + cells.Count + " cells in the batch, " + hits + " armed, " + crittersByCell.Count + " critter cells, " + pokes + " pokes");
		}

		/// <summary>Whether Sgt_Imalas's Critters Fall Through Open Doors is loaded; decided once, after every mod has loaded.</summary>
		private static bool OpenDoorsAreAir
		{
			get
			{
				if (openDoorsAreAir == null)
				{
					bool found = false;
					foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
					{
						if (assembly.GetName().Name.IndexOf("CrittersFallThroughOpenDoors", System.StringComparison.OrdinalIgnoreCase) >= 0)
						{
							found = true;
							break;
						}
					}
					openDoorsAreAir = found;
					UnityEngine.Debug.Log("[JustFallAlready] Critters Fall Through Open Doors " + (found ? "found: cells around doors are watched too" : "not found: only door cells are watched"));
				}
				return openDoorsAreAir.Value;
			}
		}

		private static bool StartedByTestRunner()
		{
			foreach (string arg in System.Environment.GetCommandLineArgs())
				if (arg.StartsWith("--onitest-", System.StringComparison.Ordinal))
					return true;
			return false;
		}

		/// <summary>
		/// Critters (running brains with a creature fall monitor) in the given armed cell. The first
		/// lookup in a frame walks the brains once and keeps only those standing in an armed cell, so
		/// the component checks run for a handful of brains, not every brain in the colony.
		/// </summary>
		private static bool CrittersAt(int cell, out List<Brain> critters)
		{
			if (cacheFrame != Time.frameCount)
			{
				cacheFrame = Time.frameCount;
				foreach (List<Brain> list in crittersByCell.Values)
				{
					list.Clear();
					spareLists.Push(list);
				}
				crittersByCell.Clear();
				foreach (Brain brain in Components.Brains.Items)
				{
					int at = Grid.PosToCell(brain);
					if (!armed.ContainsKey(at) || !brain.IsRunning())
						continue;
					StateMachineController controller = brain.GetComponent<StateMachineController>();
					if (controller == null || controller.GetSMI<CreatureFallMonitor.Instance>() == null)
						continue;
					if (!crittersByCell.TryGetValue(at, out List<Brain> list))
						crittersByCell[at] = list = spareLists.Count != 0 ? spareLists.Pop() : new List<Brain>();
					list.Add(brain);
				}
			}
			return crittersByCell.TryGetValue(cell, out critters);
		}
	}
}
