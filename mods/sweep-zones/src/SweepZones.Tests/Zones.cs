using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace SweepZones.Tests
{
	/// <summary>The tool, its mode menu, the priority screen and the zone store, as a test needs them.</summary>
	internal static class Zones
	{
		public static SweepZoneTool Tool
		{
			get { return PlayerController.Instance.tools.OfType<SweepZoneTool>().First(); }
		}

		public static PrioritySetting Basic(int value)
		{
			return new PrioritySetting(PriorityScreen.PriorityClass.basic, value);
		}

		public static PrioritySetting YellowAlert
		{
			get { return new PrioritySetting(PriorityScreen.PriorityClass.topPriority, 1); }
		}

		/// <summary>An empty store for the test, and empty again after it, with the select tool back.</summary>
		public static ZoneStore Fresh(TestContext t)
		{
			ZoneStore store = ZoneStore.Instance;
			t.Assert.NotNull(store, "the zone store is on the save game");
			Empty(store);
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == Tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
				Empty(store);
			});
			return store;
		}

		private static void Empty(ZoneStore store)
		{
			foreach (int cell in store.SweepCells.Keys.ToList())
				store.RemoveSweep(cell);
			foreach (int cell in store.MopCells.Keys.ToList())
				store.RemoveMop(cell);
		}

		/// <summary>Tiles along the bottom row of the rectangle from x0 to x1; the cells above them are zone ground.</summary>
		public static List<GameObject> Floor(TestContext t, CellRect r, int x0, int x1)
		{
			var tiles = new List<GameObject>();
			for (int x = x0; x <= x1; x++)
				tiles.Add(t.World.Build("Tile", r.Cell(x, 0), element: SimHashes.IgneousRock));
			return tiles;
		}

		/// <summary>A tile built into the cell, the way a player builds over a zone.</summary>
		public static GameObject Pillar(TestContext t, int cell)
		{
			return t.World.Build("Tile", cell, element: SimHashes.IgneousRock);
		}

		/// <summary>The tool made active, as the toolbar button or the hotkey would.</summary>
		public static IEnumerator Open(TestContext t)
		{
			PlayerController.Instance.ActivateTool(Tool);
			yield return t.WaitRealSeconds(0.3);
			// The tool keeps its mode between uses; every test starts in the default, sweep zone.
			yield return Mode(t, ModStrings.FilterSweep);
		}

		/// <summary>The mode button clicked in the tool's parameter menu, by its toggle id.</summary>
		public static IEnumerator Mode(TestContext t, string toggleId)
		{
			// The menu keeps its buttons by toggle id; the button is the MultiToggle the player clicks.
			var widgets = (System.Collections.IDictionary)Field(ToolMenu.Instance.toolParameterMenu, "widgets");
			t.Assert.True(widgets.Contains(toggleId), "the mode menu has a button " + toggleId);
			GameObject widget = (GameObject)Field(widgets[toggleId], "gameObject");
			MultiToggle button = widget.GetComponentInChildren<MultiToggle>();
			t.Assert.NotNull(button, "a button for " + toggleId);
			button.onClick();
			yield return t.WaitRealSeconds(0.3);
		}

		public static IEnumerator Priority(TestContext t, PrioritySetting priority)
		{
			ToolMenu.Instance.PriorityScreen.SetScreenPriority(priority, play_sound: false);
			yield return t.WaitRealSeconds(0.2);
		}

		/// <summary>A box drag with the tool at a hand's pace.</summary>
		public static IEnumerator Drag(TestContext t, int fromCell, int toCell)
		{
			SweepZoneTool tool = Tool;
			t.Assert.True(PlayerController.Instance.ActiveTool == tool, "the Sweep Zones tool is the active tool");
			Vector3 from = Grid.CellToPosCCC(fromCell, Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(toCell, Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(0.3);
			tool.OnLeftClickUp(to);
			yield return t.WaitRealSeconds(0.3);
		}

		public static void Same(TestContext t, PrioritySetting expected, PrioritySetting actual, string what)
		{
			t.Assert.True(expected == actual, what + " (expected " + Describe(expected) + ", got " + Describe(actual) + ")");
		}

		public static string Describe(PrioritySetting p)
		{
			return p.priority_class + " " + p.priority_value;
		}

		public static object Field(object target, string name)
		{
			for (System.Type type = target.GetType(); type != null; type = type.BaseType)
			{
				var field = type.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly);
				if (field != null)
					return field.GetValue(target);
			}
			throw new OniAssertException("no field " + name + " on " + target.GetType().Name);
		}
	}
}
