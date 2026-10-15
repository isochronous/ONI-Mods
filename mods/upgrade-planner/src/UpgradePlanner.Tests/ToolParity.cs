using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// The planner places an order wherever the game's own build tool, dragged over the same
	/// buildings, would, and nowhere else. Each case lays its buildings, drags the build tool over
	/// them as orders (instant build off), notes which cells got one, cancels them, then drags the
	/// planner over the same cells with the plan "these, any material, to the target in the same
	/// material" and compares.
	/// </summary>
	public static class ToolParity
	{
		[OniTest("Upgrade Planner: joint plates are ordered over tiles where the build tool orders them, and only there", Timeout = 90), Layer(1)]
		public static IEnumerator JointPlates(TestContext t)
		{
			// A pairing the build tool surely takes, so a difference below is the joint plates' own.
			yield return Compare(t, "Tile", SimHashes.SandStone, "MetalTile", SimHashes.Copper);
			// In a row of tiles a plate's wire ends fall in the tiles beside it, where the game will not
			// have them; in a wall of tiles they fall in the open on either side, where it will.
			foreach (string plate in new[] { "WireBridgeHighWattage", "WireRefinedBridgeHighWattage" })
			{
				yield return Compare(t, "BunkerTile", SimHashes.Steel, plate, SimHashes.Copper);
				yield return Compare(t, "BunkerTile", SimHashes.Steel, plate, SimHashes.Copper, wall: true);
			}
		}

		/// <summary>Three of the source in a row, or with <paramref name="wall"/> stacked as a wall, compared.</summary>
		private static IEnumerator Compare(TestContext t, string sourceId, SimHashes sourceElement, string targetId, SimHashes targetElement, bool wall = false)
		{
			t.ResearchAll();
			BuildingDef source = Assets.GetBuildingDef(sourceId);
			BuildingDef target = Assets.GetBuildingDef(targetId);
			var materials = new List<Tag> { targetElement.CreateTag() };
			CellRect r = t.Rect(5, 3);
			var cells = Enumerable.Range(0, 3).Select(i => wall ? r.Cell(2, i) : r.Cell(1 + i, 0)).ToList();
			foreach (int cell in cells)
				t.World.Build(sourceId, cell, element: sourceElement);
			yield return t.WaitGameSeconds(2);
			t.InstantBuild(false);

			// The game's build tool, as the build menu leaves it, dragged along the row.
			BuildTool build = BuildTool.Instance;
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == build)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			yield return UtilityDragSession.PrimeBuildMenu(t);
			build.Activate(target, materials);
			yield return t.WaitRealSeconds(0.3);
			yield return Drag(t, build, cells.First(), cells.Last());
			List<int> byTool = Ordered(cells, target);
			t.Log(targetId + " over " + sourceId + (wall ? " in a wall" : " in a row") + ": the build tool ordered " + Describe(byTool));
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			yield return CancelAll(t, cells, target);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ReplaceType;
			plan.ReplaceType.SetSource(Families.Of(source));
			plan.ReplaceType.SetSourceAny();
			plan.ReplaceType.SetTarget(Families.Of(target));
			plan.ReplaceType.SetTargetMaterials(materials);
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			yield return ByHand.Drag(t, cells.First(), cells.Last());
			List<int> byPlanner = Ordered(cells, target);
			t.Log(targetId + " over " + sourceId + (wall ? " in a wall" : " in a row") + ": the planner ordered " + Describe(byPlanner));
			yield return CancelAll(t, cells, target);
			t.Assert.Eq(Describe(byTool), Describe(byPlanner), "the planner orders " + targetId + " over " + sourceId + " where the build tool does");
			foreach (int cell in cells)
				t.World.Destroy(t.World.At(cell, source.ObjectLayer));
			yield return t.WaitGameSeconds(2);
		}

		private static List<int> Ordered(List<int> cells, BuildingDef target)
		{
			return cells.Where(cell => Grid.Objects[cell, (int)target.ReplacementLayer]?.GetComponent<Constructable>() != null).ToList();
		}

		private static string Describe(List<int> cells)
		{
			return cells.Count == 0 ? "nothing" : string.Join(", ", cells.Select(Cells.Describe));
		}

		/// <summary>Every order on the target's replacement layer at the cells cancelled, as the Cancel tool does.</summary>
		private static IEnumerator CancelAll(TestContext t, List<int> cells, BuildingDef target)
		{
			foreach (int cell in cells)
				Grid.Objects[cell, (int)target.ReplacementLayer]?.Trigger((int)GameHashes.Cancel);
			yield return t.WaitGameSeconds(1);
		}

		/// <summary>A drag with a drag tool along the cells, at a hand's pace.</summary>
		private static IEnumerator Drag(TestContext t, DragTool tool, int fromCell, int toCell)
		{
			Vector3 from = Grid.CellToPosCCC(fromCell, Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(toCell, Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(0.3);
			tool.OnLeftClickUp(to);
			yield return t.WaitRealSeconds(0.5);
		}
	}
}
