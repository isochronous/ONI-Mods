using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// A large drag does not hold up the game: the whole test area of sandstone tiles changed to
	/// granite with the real tool is placed a share each frame (PlacementQueue), the drag's own call
	/// stays short and no frame spends much past the budget, and every tile is handled. The times are
	/// logged for comparison.
	/// </summary>
	public static class DragTiming
	{
		/// <summary>The drag's own call, at most: before the queue, 304 tiles took 120 ms as orders and 180 ms with instant build.</summary>
		private const double DragCallMs = 60;

		/// <summary>One frame's placing, at most: the budget plus the building that crosses it.</summary>
		private const double StepMs = PlacementQueue.BudgetMs + 10;

		[OniTest("Upgrade Planner: a drag over a full area of tiles is placed over several frames, as orders", Timeout = 120), Layer(1)]
		public static IEnumerator Orders(TestContext t) => Run(t, instant: false);

		[OniTest("Upgrade Planner: a drag over a full area of tiles is placed over several frames, with instant build", Timeout = 120), Layer(1)]
		public static IEnumerator Instant(TestContext t) => Run(t, instant: true);

		private static IEnumerator Run(TestContext t, bool instant)
		{
			CellRect r = t.Rect(t.Area.W - 2, t.Area.H - 2);
			var cells = new List<int>(r.Cells);
			foreach (int cell in cells)
				t.World.Build("Tile", cell, element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(2);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { Families.Of(Assets.GetBuildingDef("Tile")) }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(instant);

			UpgradeTool tool = null;
			foreach (InterfaceTool candidate in PlayerController.Instance.tools)
				if (candidate is UpgradeTool found)
					tool = found;
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			// As a player would: Apply closes the window and keeps the tool, the camera on the area,
			// so screenshots and screen recordings show the drag.
			KButton apply = FindDeep(GameObject.Find("UpgradePlannerDialog")?.transform, "Apply")?.GetComponent<KButton>();
			t.Assert.NotNull(apply, "the window's Apply button");
			apply.SignalClick(KKeyCode.Mouse0);
			CameraController.Instance.SnapTo(Grid.CellToPosCCC(r.Cell(r.W / 2, r.H / 2), Grid.SceneLayer.Building), 14f);
			yield return t.WaitRealSeconds(1.5);
			// The window open with the tool is not part of the drag; the baseline frame is taken after it.
			yield return Watch(30);
			float before = slowest;

			// A small drag first: the game's first replacement of a session runs a good deal of its
			// code for the first time, once, whatever the planner does. What is timed is the drag after.
			yield return Drag(t, tool, r.Cell(0, 0), r.Cell(1, 0), null);
			yield return t.WaitGameSeconds(1);

			var watch = new Stopwatch();
			yield return Drag(t, tool, r.Cell(0, 0), r.Cell(r.W - 1, r.H - 1), watch);
			int frames = 0;
			while (PlacementQueue.Busy && frames < 600)
			{
				frames++;
				yield return null;
			}
			double longest = PlacementQueue.LongestStepMs;
			yield return Watch(120);
			float after = slowest;
			yield return t.WaitGameSeconds(2);

			int handled = 0;
			foreach (int cell in cells)
			{
				GameObject now = instant ? t.World.At(cell, ObjectLayer.FoundationTile) : t.World.At(cell, ObjectLayer.ReplacementTile);
				if (now != null && now.GetComponent<PrimaryElement>().ElementID == SimHashes.Granite)
					handled++;
			}
			t.Log((instant ? "instant build" : "orders") + ": " + cells.Count + " tiles; the drag's own call took " + watch.Elapsed.TotalMilliseconds.ToString("0.0")
				+ " ms, then " + frames + " more frames of placing, the longest " + longest.ToString("0.0") + " ms; slowest frame in the 120 after "
				+ (after * 1000f).ToString("0.0") + " ms, against " + (before * 1000f).ToString("0.0") + " ms before the drag");
			t.Assert.Eq(cells.Count, handled, "every tile got its granite " + (instant ? "tile" : "order"));
			t.Assert.True(frames > 0, "the placing went on after the drag's own frame");
			t.Assert.True(watch.Elapsed.TotalMilliseconds < DragCallMs, "the drag's own call took " + watch.Elapsed.TotalMilliseconds.ToString("0.0") + " ms, under " + DragCallMs);
			t.Assert.True(longest < StepMs, "no frame spent more than " + StepMs + " ms placing (" + longest.ToString("0.0") + ")");
		}

		[OniTest("Upgrade Planner: timing of reading and writing settings for a full area of tiles", Timeout = 120, Explicit = true), Layer(0)]
		public static IEnumerator SettingsCopy(TestContext t)
		{
			if (!BlueprintsCompat.Loaded)
				t.Skip("Blueprints Expanded is not loaded (run with -Steam 3468585385)");
			yield return Run(t, instant: false);
			CellRect r = t.Rect(t.Area.W - 2, t.Area.H - 2);
			var tiles = new List<GameObject>();
			var orders = new List<GameObject>();
			foreach (int cell in r.Cells)
			{
				tiles.Add(t.World.At(cell, ObjectLayer.FoundationTile));
				orders.Add(t.World.At(cell, ObjectLayer.ReplacementTile));
			}
			var read = Stopwatch.StartNew();
			var taken = new List<Dictionary<string, Newtonsoft.Json.Linq.JObject>>();
			foreach (GameObject tile in tiles)
				taken.Add(BlueprintsCompat.TakeSettings(tile));
			read.Stop();
			var write = Stopwatch.StartNew();
			int keys = 0;
			for (int i = 0; i < orders.Count; i++)
				keys += BlueprintsCompat.GiveSettings(taken[i], orders[i], "tile");
			write.Stop();
			t.Log(tiles.Count + " tiles: reading their settings took " + read.Elapsed.TotalMilliseconds.ToString("0.0") + " ms, writing "
				+ keys + " settings into the orders " + write.Elapsed.TotalMilliseconds.ToString("0.0") + " ms; keys: " + string.Join(", ", taken[0].Keys));
		}

		/// <summary>A box drag with the real tool, at a hand's pace; <paramref name="watch"/>, when given, times the release.</summary>
		private static IEnumerator Drag(TestContext t, UpgradeTool tool, int fromCell, int toCell, Stopwatch watch)
		{
			Vector3 from = Grid.CellToPosCCC(fromCell, Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(toCell, Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(0.3);
			watch?.Start();
			tool.OnLeftClickUp(to);
			watch?.Stop();
			if (watch == null)
				yield return t.WaitUntil(() => !PlacementQueue.Busy, 10, "the small drag placed");
		}

		private static Transform FindDeep(Transform root, string name)
		{
			if (root == null || root.name == name)
				return root;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform found = FindDeep(root.GetChild(i), name);
				if (found != null)
					return found;
			}
			return null;
		}

		private static float slowest;

		/// <summary>Watches the next <paramref name="frames"/> frames and leaves the slowest, in real seconds, in <see cref="slowest"/>.</summary>
		private static IEnumerator Watch(int frames)
		{
			slowest = 0f;
			for (int i = 0; i < frames; i++)
			{
				yield return null;
				slowest = Mathf.Max(slowest, Time.unscaledDeltaTime);
			}
		}
	}
}
