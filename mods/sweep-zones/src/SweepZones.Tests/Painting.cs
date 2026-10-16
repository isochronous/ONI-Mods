using System.Collections;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace SweepZones.Tests
{
	/// <summary>
	/// The tool as a player uses it: opened, a mode picked in the side menu, a priority picked, a box
	/// dragged. Every test starts from an empty zone store and leaves one.
	/// </summary>
	public static class Painting
	{
		[OniTest("Sweep Zones: the tool, its toolbar button, its icon, its hotkey and the zone store are all in the game", Timeout = 30), Layer(1)]
		public static IEnumerator Registered(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			t.Assert.NotNull(Zones.Tool, "the tool is among the player's tools");
			var collection = ToolMenu.Instance.basicTools.FirstOrDefault(c => c.tools.Any(tool => tool.toolName == "SweepZoneTool"));
			t.Assert.NotNull(collection, "the toolbar has a Sweep Zones button");
			t.Assert.True(Assets.Sprites.ContainsKey(collection.icon), "its icon sprite is registered");
			t.Assert.NotEq(PeterHan.PLib.Actions.PAction.MaxAction, SweepZonesMod.ToolAction, "the hotkey action is registered");
			t.Assert.True(store.gameObject == SaveGame.Instance.gameObject, "the store lives on the save game, so it is saved with it");

			t.Assert.NotNull(Zones.Field(Zones.Tool, "areaVisualizer"), "the tool has a box-drag area visualizer (a box drag does nothing without one)");
			t.Assert.NotNull(Zones.Field(Zones.Tool, "visualizer") , "and a cursor visualizer");

			yield return t.Press(SweepZonesMod.ToolAction);
			yield return t.WaitRealSeconds(0.3);
			t.Assert.True(PlayerController.Instance.ActiveTool == Zones.Tool, "the hotkey opens the tool");
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			yield return t.WaitRealSeconds(0.2);
		}

		[OniTest("Sweep Zones: a box drag paints sweep zone cells at the chosen priority, only on open cells above solid ground", Timeout = 40), Layer(1)]
		public static IEnumerator PaintSweep(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(10, 5);
			Zones.Floor(t, r, 0, 9);
			yield return t.WaitGameSeconds(1);
			yield return Zones.Open(t);
			yield return Zones.Priority(t, Zones.Basic(7));
			// A box four rows high: only the row on the floor qualifies.
			yield return Zones.Drag(t, r.Cell(2, 1), r.Cell(6, 4));

			t.Assert.Eq(5, store.SweepCells.Count, "five cells: the row above the floor");
			foreach (int x in Enumerable.Range(2, 5))
			{
				t.Assert.True(store.SweepCells.ContainsKey(r.Cell(x, 1)), "painted at x=" + x);
				Zones.Same(t, Zones.Basic(7), store.SweepCells[r.Cell(x, 1)], "priority 7 at x=" + x);
			}
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(3, 2)), "a cell with open air below is not painted");
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(3, 0)), "nor the floor tile itself");
			t.Assert.Eq(0, store.MopCells.Count, "nothing in the mop zones");
		}

		[OniTest("Sweep Zones: the number keys set the priority, yellow alert included, and repainting a cell changes its priority", Timeout = 40), Layer(1)]
		public static IEnumerator PriorityKeys(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(10, 3);
			Zones.Floor(t, r, 0, 9);
			yield return t.WaitGameSeconds(1);
			yield return Zones.Open(t);
			foreach (var key in new[] { (Action.Plan3, Zones.Basic(3)), (Action.Plan10, Zones.YellowAlert) })
			{
				Zones.Tool.OnKeyDown(new KButtonEvent(null, InputEventType.KeyDown, key.Item1));
				yield return t.WaitRealSeconds(0.2);
				Zones.Same(t, key.Item2, ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority(), "the " + key.Item1 + " key selects it");
			}
			Zones.Tool.OnKeyDown(new KButtonEvent(null, InputEventType.KeyDown, Action.Plan3));
			yield return t.WaitRealSeconds(0.2);
			yield return Zones.Drag(t, r.Cell(1, 1), r.Cell(6, 1));
			Zones.Tool.OnKeyDown(new KButtonEvent(null, InputEventType.KeyDown, Action.Plan10));
			yield return t.WaitRealSeconds(0.2);
			yield return Zones.Drag(t, r.Cell(4, 1), r.Cell(8, 1));

			Zones.Same(t, Zones.Basic(3), store.SweepCells[r.Cell(2, 1)], "painted at 3 stays 3");
			Zones.Same(t, Zones.YellowAlert, store.SweepCells[r.Cell(5, 1)], "painted over at yellow alert becomes yellow alert");
			Zones.Same(t, Zones.YellowAlert, store.SweepCells[r.Cell(8, 1)], "and new cells get it too");
			t.Assert.Eq(8, store.SweepCells.Count, "cells 1 to 8");
		}

		[OniTest("Sweep Zones: mop mode paints its own zone, erase modes remove only their kind, and erasing is not limited to valid cells", Timeout = 60), Layer(1)]
		public static IEnumerator ModesAndErasing(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(12, 3);
			Zones.Floor(t, r, 0, 11);
			yield return t.WaitGameSeconds(1);
			yield return Zones.Open(t);
			yield return Zones.Priority(t, Zones.Basic(4));
			yield return Zones.Drag(t, r.Cell(1, 1), r.Cell(10, 1));
			t.Assert.Eq(10, store.SweepCells.Count, "sweep cells painted");

			yield return Zones.Mode(t, ModStrings.FilterMop);
			yield return Zones.Priority(t, Zones.Basic(6));
			yield return Zones.Drag(t, r.Cell(5, 1), r.Cell(8, 1));
			t.Assert.Eq(4, store.MopCells.Count, "mop cells painted apart from the sweep cells");
			Zones.Same(t, Zones.Basic(6), store.MopCells[r.Cell(6, 1)], "at their own priority");
			t.Assert.Eq(10, store.SweepCells.Count, "the sweep zone is as it was");

			yield return Zones.Mode(t, ModStrings.FilterClearMop);
			yield return Zones.Drag(t, r.Cell(6, 1), r.Cell(7, 1));
			t.Assert.Eq(2, store.MopCells.Count, "erase mop removed two mop cells");
			t.Assert.Eq(10, store.SweepCells.Count, "and no sweep cell");

			// A cell the painting rules would refuse (open air) can still be erased.
			store.SetSweep(r.Cell(3, 2), Zones.Basic(4));
			yield return Zones.Mode(t, ModStrings.FilterClearSweep);
			yield return Zones.Drag(t, r.Cell(2, 1), r.Cell(4, 2));
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(3, 2)), "the cell above open air is erased");
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(3, 1)), "and the ones in the box");
			t.Assert.Eq(7, store.SweepCells.Count, "three sweep cells gone, seven left");
			t.Assert.Eq(2, store.MopCells.Count, "mop cells untouched by erase sweep");
		}

		[OniTest("Sweep Zones: a pneumatic door in the floor is not ground, a tile is", Timeout = 40), Layer(1)]
		public static IEnumerator DoorIsNotGround(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(8, 4);
			var tiles = Zones.Floor(t, r, 0, 7);
			t.World.Destroy(tiles[4]);
			yield return t.WaitGameSeconds(1);
			GameObject door = t.World.Build("Door", r.Cell(4, 0));
			yield return t.WaitGameSeconds(1);
			door.GetComponent<Door>().QueueStateChange(Door.ControlState.Opened);
			yield return t.WaitUntil(() => door.GetComponent<Door>().IsOpen(), 10, "the door to open");
			yield return t.WaitGameSeconds(1);
			t.Assert.False(Grid.Solid[r.Cell(4, 0)], "an open door is not solid to the sim");
			t.Assert.True(Grid.Solid[r.Cell(3, 0)], "the tile beside it is");

			yield return Zones.Open(t);
			// The door is two cells high, so the cell above it has the door's top cell under it.
			yield return Zones.Drag(t, r.Cell(2, 1), r.Cell(6, 2));
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(4, 2)), "the cell above the open door is not painted");
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(4, 1)), "nor the door's own cell");
			t.Assert.Eq(4, store.SweepCells.Count, "the four cells above tiles are");
		}

		[OniTest("Sweep Zones: reopening the tool or switching mode drops zone cells whose floor is gone or that were built over", Timeout = 60), Layer(1)]
		public static IEnumerator Pruning(TestContext t)
		{
			ZoneStore store = Zones.Fresh(t);
			CellRect r = t.Rect(10, 3);
			var floor = Zones.Floor(t, r, 0, 9);
			yield return t.WaitGameSeconds(1);
			yield return Zones.Open(t);
			yield return Zones.Drag(t, r.Cell(1, 1), r.Cell(8, 1));
			yield return Zones.Mode(t, ModStrings.FilterMop);
			yield return Zones.Drag(t, r.Cell(1, 1), r.Cell(8, 1));
			t.Assert.Eq(8, store.SweepCells.Count, "sweep painted");
			t.Assert.Eq(8, store.MopCells.Count, "mop painted");

			// Floor dug out under x=2 and a tile built into the zone at x=5, while the tool is open.
			t.World.Destroy(floor[2]);
			yield return t.WaitGameSeconds(1);
			Zones.Pillar(t, r.Cell(5, 1));
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(8, store.SweepCells.Count, "nothing is dropped while the tool stays as it is");
			yield return Zones.Mode(t, ModStrings.FilterClearMop);
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(2, 1)), "switching mode pruned the cell over the dug floor (sweep)");
			t.Assert.False(store.MopCells.ContainsKey(r.Cell(2, 1)), "and in the mop zones");
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(5, 1)), "the built-over cell (sweep)");
			t.Assert.False(store.MopCells.ContainsKey(r.Cell(5, 1)), "and (mop)");
			t.Assert.Eq(6, store.SweepCells.Count, "six sweep cells left");
			t.Assert.Eq(6, store.MopCells.Count, "six mop cells left");

			// Reopening prunes too.
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			yield return t.WaitRealSeconds(0.3);
			t.World.Destroy(floor[7]);
			yield return t.WaitGameSeconds(1);
			yield return Zones.Open(t);
			t.Assert.False(store.SweepCells.ContainsKey(r.Cell(7, 1)), "opening the tool pruned the cell over the dug floor");
			t.Assert.Eq(5, store.SweepCells.Count, "five left");
		}
	}
}
