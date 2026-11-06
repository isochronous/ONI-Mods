using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// Closing the planner's window keeps the tool, whichever way it is closed: a player may open it
	/// only to look at the plan. A second Escape ends the tool, as does its toolbar button. Apply
	/// hands drags the plan as it stands, and they keep it until the next Apply or a Clear, whatever
	/// is changed meanwhile; with nothing applied a drag marks nothing and says to apply the
	/// settings first. The tool is picked from the toolbar each time, as a player picks it.
	/// </summary>
	public static class Closing
	{
		private const string WindowName = "UpgradePlannerDialog";

		[OniTest("Upgrade Planner: the window's X closes it and keeps the tool", Timeout = 30), Layer(2)]
		public static IEnumerator CloseButtonKeepsTool(TestContext t)
		{
			yield return Pick(t);
			KButton close = Find(GameObject.Find(WindowName).transform, PDialog.DIALOG_KEY_CLOSE)?.GetComponent<KButton>();
			t.Assert.NotNull(close, "the window's X is there");
			close.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Null(GameObject.Find(WindowName), "the window is closed");
			t.Assert.True(PlayerController.Instance.ActiveTool is UpgradeTool, "the tool is still active");
		}

		[OniTest("Upgrade Planner: Escape closes the window and keeps the tool; a second Escape ends the tool", Timeout = 30), Layer(2)]
		public static IEnumerator EscapeTwice(TestContext t)
		{
			yield return Pick(t);
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Null(GameObject.Find(WindowName), "the first Escape closed the window");
			t.Assert.True(PlayerController.Instance.ActiveTool is UpgradeTool, "the tool is still active after the first Escape");
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.True(PlayerController.Instance.IsUsingDefaultTool(), "the second Escape ended the tool, now " + PlayerController.Instance.ActiveTool?.name);
		}

		[OniTest("Upgrade Planner: Escape after Apply ends the tool", Timeout = 30), Layer(2)]
		public static IEnumerator EscapeAfterApply(TestContext t)
		{
			yield return Pick(t);
			KButton apply = Find(GameObject.Find(WindowName).transform, "Apply")?.GetComponent<KButton>();
			t.Assert.NotNull(apply, "the Apply button is there");
			apply.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Null(GameObject.Find(WindowName), "Apply closed the window");
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.True(PlayerController.Instance.IsUsingDefaultTool(), "Escape ended the tool, now " + PlayerController.Instance.ActiveTool?.name);
		}

		[OniTest("Upgrade Planner: the toolbar button pressed again ends the tool and closes the window", Timeout = 30), Layer(2)]
		public static IEnumerator ToolbarButtonAgain(TestContext t)
		{
			yield return Pick(t);
			Button(t).Click();
			yield return t.WaitRealSeconds(0.5);
			t.Assert.True(PlayerController.Instance.IsUsingDefaultTool(), "the tool ended, now " + PlayerController.Instance.ActiveTool?.name);
			t.Assert.Null(GameObject.Find(WindowName), "the window is closed");
		}

		[OniTest("Upgrade Planner: closed without Apply, a drag marks nothing and asks for Apply", Timeout = 40), Layer(2)]
		public static IEnumerator NotAppliedMarksNothing(TestContext t)
		{
			// What an earlier Apply left stays applied until a Clear: cleared first, then a plan made and not applied.
			yield return Pick(t);
			PressClear(t);
			yield return t.WaitRealSeconds(0.5);
			List<int> cells = Tiles(t);
			yield return t.WaitGameSeconds(1);
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.ActiveTool;
			ExpectReady(t, tool, false, "closed without Apply");
			yield return Drag(t, tool, cells);
			t.Assert.Eq(0, Orders(cells), "orders placed by a drag before Apply");
			t.Assert.NotNull(GameObject.Find(WindowName), "the refused drag opened the window again");
			yield return t.Screenshot("refused");
		}

		[OniTest("Upgrade Planner: after Apply, opening the window only to look and closing it keeps the tool ready", Timeout = 40), Layer(2)]
		public static IEnumerator LookAgainKeepsReady(TestContext t)
		{
			List<int> cells = Tiles(t);
			yield return t.WaitGameSeconds(1);
			yield return Pick(t);
			PlannerWindow.PressApply(t);
			yield return t.WaitRealSeconds(0.5);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.ActiveTool;
			ExpectReady(t, tool, true, "after Apply");
			// The button by the priority bar opens the window again; its X closes it.
			KToggle again = GameObject.Find("UpgradePlannerToggle")?.GetComponent<KToggle>();
			t.Assert.NotNull(again, "the button that opens the window again is there");
			again.Click();
			yield return t.WaitRealSeconds(0.5);
			t.Assert.NotNull(GameObject.Find(WindowName), "the window opened again");
			Find(GameObject.Find(WindowName).transform, PDialog.DIALOG_KEY_CLOSE).GetComponent<KButton>().SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(0.5);
			ExpectReady(t, tool, true, "looked at and closed");
			yield return Drag(t, tool, cells);
			t.Assert.Eq(cells.Count, Orders(cells), "orders placed by the drag");
		}

		[OniTest("Upgrade Planner: a change after Apply waits for the next Apply; drags keep the applied plan", Timeout = 40), Layer(2)]
		public static IEnumerator ChangeWaitsForApply(TestContext t)
		{
			List<int> cells = Tiles(t);
			yield return t.WaitGameSeconds(1);
			yield return Pick(t);
			PlannerWindow.PressApply(t);
			yield return t.WaitRealSeconds(0.5);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.ActiveTool;
			GameObject.Find("UpgradePlannerToggle").GetComponent<KToggle>().Click();
			yield return t.WaitRealSeconds(0.5);
			UpgradePlan.Current.ChangeMaterial.SetTarget(SimHashes.IgneousRock.CreateTag());
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			ExpectReady(t, tool, true, "changed and closed without Apply");
			yield return Drag(t, tool, cells);
			t.Assert.Eq(cells.Count, Orders(cells), "orders placed by the drag");
			BuildingDef tile = Assets.GetBuildingDef("Tile");
			foreach (int cell in cells)
			{
				Tag material = Grid.Objects[cell, (int)tile.ReplacementLayer].GetComponent<Constructable>().SelectedElementsTags[0];
				t.Assert.Eq(SimHashes.Granite.CreateTag(), material, "the order at " + Cells.Describe(cell) + " is in the applied material");
			}
		}

		[OniTest("Upgrade Planner: Clear takes back what was applied", Timeout = 40), Layer(2)]
		public static IEnumerator ClearTakesBackApply(TestContext t)
		{
			Tiles(t);
			yield return Pick(t);
			PlannerWindow.PressApply(t);
			yield return t.WaitRealSeconds(0.5);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.ActiveTool;
			ExpectReady(t, tool, true, "after Apply");
			GameObject.Find("UpgradePlannerToggle").GetComponent<KToggle>().Click();
			yield return t.WaitRealSeconds(0.5);
			PressClear(t);
			yield return t.WaitRealSeconds(0.5);
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			ExpectReady(t, tool, false, "cleared");
		}

		[OniTest("Upgrade Planner: a box being dragged marks what it holds as it grows and lets go of what it no longer holds; the hover card counts it", Timeout = 40), Layer(2)]
		public static IEnumerator BoxMarksAsItMoves(TestContext t)
		{
			List<int> cells = Tiles(t);
			yield return t.WaitGameSeconds(1);
			yield return Pick(t);
			PlannerWindow.PressApply(t);
			yield return t.WaitRealSeconds(0.5);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.ActiveTool;
			int focus = cells[1];
			CameraController.Instance.SnapTo(Grid.CellToPosCCC(focus, Grid.SceneLayer.Building), 8f);
			Vector3 Pos(int cell) => Grid.CellToPosCCC(cell, Grid.SceneLayer.Building);
			tool.OnMouseMove(Pos(cells[0]));
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(Pos(cells[0]));
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(Pos(cells[1]));
			yield return t.WaitRealSeconds(0.3);
			ExpectBox(t, tool, cells, 2, "the box over two tiles");
			t.PointAt(cells[2]);
			tool.OnMouseMove(Pos(cells[2]));
			yield return t.WaitRealSeconds(0.5);
			ExpectBox(t, tool, cells, 3, "grown over three");
			yield return t.Screenshot("box");
			tool.OnMouseMove(Pos(cells[0]));
			yield return t.WaitRealSeconds(0.3);
			ExpectBox(t, tool, cells, 1, "shrunk back to one");
			tool.OnLeftClickUp(Pos(cells[0]));
			yield return t.WaitUntil(() => !PlacementQueue.Busy, 10, "the drag's orders placed");
			yield return t.WaitRealSeconds(0.3);
			t.Assert.Eq(1, Orders(cells), "orders placed by the drag let go over one tile");
			t.Assert.Eq(0, ((PlannerHoverCard)tool.GetComponent<HoverTextConfiguration>()).Types.Count, "the hover card counts nothing once the drag is done");
		}

		[OniTest("Upgrade Planner: the highlight is not worked out again on a second call with nothing changed", Timeout = 30), Layer(2)]
		public static IEnumerator HighlightIsCached(TestContext t)
		{
			List<int> cells = Tiles(t);
			yield return t.WaitGameSeconds(1);
			yield return Pick(t);
			PlannerWindow.PressApply(t);
			yield return t.WaitRealSeconds(0.5);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.ActiveTool;
			tool.GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors);
			t.Assert.True(colors.Any(c => c.cell == cells[0]), "the highlight covers the tile at " + Cells.Describe(cells[0]));
			// Working it out again empties the set first, so a cell added to it by hand shows whether that happened.
			var marker = new ToolMenu.CellColorData(cells[0], Color.magenta);
			colors.Add(marker);
			tool.GetOverlayColorData(out colors);
			t.Assert.True(colors.Contains(marker), "a second call with nothing changed leaves the highlight as it was");
		}

		/// <summary>The first <paramref name="held"/> tiles marked in the highlight, the rest not, and the hover card counting them.</summary>
		private static void ExpectBox(TestContext t, UpgradeTool tool, List<int> cells, int held, string when)
		{
			Color marked = (Color)HarmonyLib.AccessTools.Field(typeof(UpgradeTool), "MarkedColor").GetValue(null);
			tool.GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors);
			for (int i = 0; i < cells.Count; i++)
			{
				bool isMarked = colors.Any(c => c.cell == cells[i] && c.color == marked);
				t.Assert.Eq(i < held, isMarked, when + ": the tile at " + Cells.Describe(cells[i]) + (i < held ? " marked" : " not marked"));
			}
			var card = (PlannerHoverCard)tool.GetComponent<HoverTextConfiguration>();
			t.Assert.Eq(Assets.GetBuildingDef("Tile").Name + ": " + held, string.Join(" | ", card.Types), when + ": the hover card's count");
			t.Assert.True(card.Materials.Count == 1 && card.Materials[0].EndsWith(ElementLoader.FindElementByHash(SimHashes.Granite).tag.ProperName()), when + ": the hover card's materials, " + string.Join(" | ", card.Materials));
		}

		/// <summary>Three sandstone tiles in the test area, and the plan "tiles, sandstone to granite"; instant build off.</summary>
		private static List<int> Tiles(TestContext t)
		{
			CellRect r = t.Rect(5, 2);
			var cells = new List<int> { r.Cell(1, 0), r.Cell(2, 0), r.Cell(3, 0) };
			foreach (int cell in cells)
				t.World.Build("Tile", cell, element: SimHashes.SandStone);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { Families.Of(Assets.GetBuildingDef("Tile")) }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(false);
			return cells;
		}

		/// <summary>The open window's Clear, on the tab showing.</summary>
		private static void PressClear(TestContext t)
		{
			KButton clear = Find(GameObject.Find(WindowName).transform, "Clear")?.GetComponent<KButton>();
			t.Assert.NotNull(clear, "the tab's Clear button is there");
			clear.SignalClick(KKeyCode.Mouse0);
		}

		/// <summary>Ready or not as the hover card says it: marking with the applied plan, or asking for one.</summary>
		private static void ExpectReady(TestContext t, UpgradeTool tool, bool ready, string when)
		{
			string action = tool.GetComponent<HoverTextConfiguration>().ActionName;
			string expected = ready ? ModStrings.UPGRADEPLANNER.HoverActionReady : ModStrings.UPGRADEPLANNER.HoverActionIdle;
			t.Assert.Eq(expected, action, when + ": the hover card");
		}

		private static IEnumerator Drag(TestContext t, UpgradeTool tool, List<int> cells)
		{
			Vector3 from = Grid.CellToPosCCC(cells.First(), Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(cells.Last(), Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(0.3);
			tool.OnLeftClickUp(to);
			yield return t.WaitUntil(() => !PlacementQueue.Busy, 10, "the drag's orders placed");
			yield return t.WaitRealSeconds(0.3);
		}

		private static int Orders(List<int> cells)
		{
			BuildingDef tile = Assets.GetBuildingDef("Tile");
			return cells.Count(cell => Grid.Objects[cell, (int)tile.ReplacementLayer]?.GetComponent<Constructable>() != null);
		}

		/// <summary>The tool picked with its toolbar button, its window open.</summary>
		private static IEnumerator Pick(TestContext t)
		{
			t.Cleanup(() =>
			{
				ToolMenu.Instance.ClearSelection();
				if (!PlayerController.Instance.IsUsingDefaultTool())
					SelectTool.Instance.Activate();
			});
			Button(t).Click();
			yield return t.WaitRealSeconds(1);
			t.Assert.True(PlayerController.Instance.ActiveTool is UpgradeTool, "the toolbar button picked the tool, now " + PlayerController.Instance.ActiveTool?.name);
			t.Assert.NotNull(GameObject.Find(WindowName), "the window opened with the tool");
		}

		/// <summary>The planner's button in the toolbar.</summary>
		private static KToggle Button(TestContext t)
		{
			ToolMenu.ToolCollection collection = ToolMenu.Instance.basicTools.FirstOrDefault(c => c.tools.Any(tool => tool.toolName == nameof(UpgradeTool)));
			KToggle toggle = collection?.toggle != null ? collection.toggle.GetComponent<KToggle>() : null;
			t.Assert.NotNull(toggle, "the planner's toolbar button is there");
			return toggle;
		}

		private static Transform Find(Transform root, string name)
		{
			if (root == null || root.name == name)
				return root;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform found = Find(root.GetChild(i), name);
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
