using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// What a drag asks for, placed a little each frame. A box drag hands over every building in it
	/// at once, where the game's own build tool places each cell as the pointer passes over it, so a
	/// large drag done in one go held the game up for most of a second (more with instant build, whose
	/// every order the game finishes on the spot). Each frame places buildings until
	/// <see cref="BudgetMs"/> has been spent, the first frame being the drag's own, so a small drag is
	/// done at once as before. A drag made while one is still being placed joins it, and the popup
	/// counting what was marked comes when the last is placed.
	/// </summary>
	internal sealed class PlacementQueue
	{
		/// <summary>Real milliseconds of placing each frame.</summary>
		internal const double BudgetMs = 4.0;

		private struct Job
		{
			public BuildingComplete Building;
			public Upgrade Upgrade;
			public PrioritySetting Priority;
		}

		/// <summary>The colony's queue; a new one with each game, since the old one's coroutine went with it.</summary>
		private static PlacementQueue current;

		private readonly Game game;
		private readonly Queue<Job> jobs = new Queue<Job>();
		private bool running;
		private int placed;
		private int skipped;
		private Vector3 popupAt;

		private PlacementQueue(Game game)
		{
			this.game = game;
		}

		/// <summary>True while buildings of a drag are still waiting to be placed.</summary>
		public static bool Busy => current != null && current.game == Game.Instance && current.running;

		/// <summary>The longest any one frame spent placing, in real milliseconds, since the last drag started the queue; for tests.</summary>
		internal static double LongestStepMs { get; private set; }

		/// <summary>
		/// Queues the upgrades of one drag at the priority it was made with; <paramref name="refused"/>
		/// buildings the plan took but could not say what to make of are counted as skipped.
		/// </summary>
		public static void Add(List<KeyValuePair<BuildingComplete, Upgrade>> upgrades, int refused, PrioritySetting priority, Vector3 popupAt)
		{
			if (Game.Instance == null)
				return;
			if (current == null || current.game != Game.Instance)
				current = new PlacementQueue(Game.Instance);
			PlacementQueue queue = current;
			foreach (KeyValuePair<BuildingComplete, Upgrade> upgrade in upgrades)
				queue.jobs.Enqueue(new Job { Building = upgrade.Key, Upgrade = upgrade.Value, Priority = priority });
			queue.skipped += refused;
			queue.popupAt = popupAt;
			if (queue.running)
				return;
			if (queue.jobs.Count == 0)
			{
				queue.Popup();
				return;
			}
			queue.running = true;
			LongestStepMs = 0;
			// Runs up to its first yield now: the drag's own frame places the first share.
			queue.game.StartCoroutine(queue.Run());
		}

		private IEnumerator Run()
		{
			var watch = new Stopwatch();
			while (true)
			{
				watch.Restart();
				// At least one a frame, however slow, so the queue always ends.
				do
					Place(jobs.Dequeue());
				while (jobs.Count > 0 && watch.Elapsed.TotalMilliseconds < BudgetMs);
				LongestStepMs = System.Math.Max(LongestStepMs, watch.Elapsed.TotalMilliseconds);
				if (jobs.Count == 0)
					break;
				yield return null;
			}
			running = false;
			Popup();
		}

		/// <summary>One building, checked again as it is placed: it may be gone, or already replaced, since the drag.</summary>
		private void Place(Job job)
		{
			bool done = false;
			try
			{
				done = job.Building != null && Orders.Place(job.Building, job.Upgrade, job.Priority);
			}
			catch (System.Exception e)
			{
				Debug.LogError("[UpgradePlanner] Could not upgrade " + (job.Building != null ? job.Building.name : "a building") + ": " + e);
			}
			if (done)
				placed++;
			else
				skipped++;
		}

		/// <summary>How many were marked (or replaced, with instant build) and skipped, over where the last drag ended.</summary>
		private void Popup()
		{
			int marked = placed, missed = skipped;
			placed = skipped = 0;
			if (marked + missed == 0 || PopFXManager.Instance == null)
				return;
			string text = (Orders.InstantBuild ? ModStrings.UPGRADEPLANNER.PopupReplaced : ModStrings.UPGRADEPLANNER.PopupMarked).Replace("{0}", marked.ToString());
			if (missed > 0)
				text = ModStrings.UPGRADEPLANNER.PopupSkipped.Replace("{0}", text).Replace("{1}", missed.ToString());
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Building, text, null, popupAt);
		}
	}
}
