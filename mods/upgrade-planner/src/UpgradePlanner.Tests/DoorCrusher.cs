using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// A reported case, run on the player's own colony (-Fixture "...\Mod Test\ActuallyTestMapNow.sav";
	/// skipped elsewhere): the right-most door crusher at the bottom of the map, two mechanical
	/// airlocks laid flat and stacked, changed to steel from any material with every type picked,
	/// in sandbox, dragged over the insulated tiles around it. With the sandbox's instant build on,
	/// the airlocks and the logic gates beside them were left as orders: the planner finished each
	/// order from a scheduler entry due the same moment as the game's placement of it, which that
	/// scheduler may run first.
	/// </summary>
	public static class DoorCrusher
	{
		private const SimHashes Target = SimHashes.Steel;

		[OniTest("Upgrade Planner: a door crusher's airlocks are rebuilt in steel with sandbox instant build", Timeout = 60, FreshSave = true), Layer(1)]
		public static IEnumerator AirlocksInstant(TestContext t) => Run(t, instant: true);

		[OniTest("Upgrade Planner: in sandbox without instant build, a door crusher's airlocks are marked, not torn down", Timeout = 60, FreshSave = true), Layer(1)]
		public static IEnumerator AirlocksSandboxOnly(TestContext t) => Run(t, instant: false);

		private static IEnumerator Run(TestContext t, bool instant)
		{
			if (!BlueprintsCompat.Loaded)
				t.Skip("Blueprints Expanded is not loaded (run with -Steam 3468585385)");
			List<BuildingComplete> doors = Crusher();
			if (doors.Count == 0)
				t.Skip("no mechanical airlocks in this save");
			CellRect r = Around(doors);
			t.Log("doors: " + string.Join("; ", doors.ConvertAll(Describe)) + "; drag " + r.X + "," + r.Y + " " + r.W + "x" + r.H);
			var doorCells = new List<int>();
			foreach (BuildingComplete door in doors)
				doorCells.Add(Grid.PosToCell(door));

			bool sandbox = Game.Instance.SandboxModeActive;
			t.Cleanup(() => Game.Instance.SandboxModeActive = sandbox);
			Game.Instance.SandboxModeActive = true;
			t.Assert.NotNull(SandboxToolParameterMenu.instance, "the sandbox tool menu");
			bool wasInstant = SandboxToolParameterMenu.instance.settings.InstantBuild;
			t.Cleanup(() => SandboxToolParameterMenu.instance.settings.InstantBuild = wasInstant);
			SandboxToolParameterMenu.instance.settings.InstantBuild = instant;
			t.InstantBuild(false);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			MaterialSwap.Refresh();
			foreach (Category category in MaterialSwap.Categories(true))
				plan.ChangeMaterial.Pick(category.Families, true);
			plan.ChangeMaterial.SetSourceAny();
			plan.ChangeMaterial.SetTarget(Target.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());

			CameraController.Instance.SnapTo(Grid.CellToPosCCC(doorCells[0], Grid.SceneLayer.Building));
			yield return t.Frames(3);
			yield return t.Screenshot("before");
			yield return Drag(t, r);
			foreach (int cell in doorCells)
				t.Log("just after the drag, at " + Xy(cell) + ": " + Describe(t.World.At(cell, ObjectLayer.Building)));
			yield return t.WaitRealSeconds(2);
			string suffix = instant ? "instant" : "sandbox";
			yield return t.Screenshot("after-" + suffix);

			var wrong = new List<string>();
			foreach (int cell in doorCells)
			{
				GameObject go = t.World.At(cell, ObjectLayer.Building);
				t.Log("2 s later, at " + Xy(cell) + ": " + Describe(go));
				bool built = go != null && go.GetComponent<BuildingComplete>() != null;
				bool ok = instant
					? built && go.GetComponent<PrimaryElement>().ElementID == Target
					: built && go.GetComponent<Reconstructable>().ReconstructRequested;
				if (!ok)
					wrong.Add(Xy(cell) + " " + Describe(go));
			}
			t.Assert.Eq(0, wrong.Count, (instant ? "airlocks not rebuilt in steel: " : "airlocks not left standing, marked for rebuild: ") + string.Join("; ", wrong));
			if (instant)
			{
				// The automation beside the doors is rebuilt the same way: nothing is left an order.
				var orders = new List<string>();
				foreach (int cell in r.Cells)
					for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
					{
						GameObject go = Grid.Objects[cell, layer];
						if (go != null && go.GetComponent<Constructable>() != null && !orders.Contains(Describe(go)))
							orders.Add(Describe(go));
					}
				t.Assert.Eq(0, orders.Count, "orders left in the dragged area: " + string.Join("; ", orders));
			}
		}

		/// <summary>The right-most group of mechanical airlocks among those at the bottom of the map.</summary>
		private static List<BuildingComplete> Crusher()
		{
			var all = new List<BuildingComplete>();
			foreach (BuildingComplete b in Components.BuildingCompletes.Items)
				if (b != null && b.Def.PrefabID == "PressureDoor" && b.GetMyWorldId() == ClusterManager.Instance.activeWorldId)
					all.Add(b);
			var found = new List<BuildingComplete>();
			if (all.Count == 0)
				return found;
			int minY = int.MaxValue;
			foreach (BuildingComplete b in all)
				minY = Mathf.Min(minY, Y(b));
			BuildingComplete right = null;
			foreach (BuildingComplete b in all)
				if (Y(b) <= minY + 6 && (right == null || X(b) > X(right)))
					right = b;
			foreach (BuildingComplete b in all)
				if (Mathf.Abs(X(b) - X(right)) <= 3 && Mathf.Abs(Y(b) - Y(right)) <= 3)
					found.Add(b);
			return found;
		}

		/// <summary>The doors and the insulated tiles touching them (within a few cells), as one rectangle.</summary>
		private static CellRect Around(List<BuildingComplete> doors)
		{
			int x0 = int.MaxValue, y0 = int.MaxValue, x1 = int.MinValue, y1 = int.MinValue;
			foreach (BuildingComplete door in doors)
				foreach (int cell in door.PlacementCells)
				{
					Grid.CellToXY(cell, out int x, out int y);
					x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y); x1 = Mathf.Max(x1, x); y1 = Mathf.Max(y1, y);
				}
			int dx0 = x0, dy0 = y0, dx1 = x1, dy1 = y1;
			for (int y = dy0 - 4; y <= dy1 + 4; y++)
				for (int x = dx0 - 4; x <= dx1 + 4; x++)
				{
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidCell(cell))
						continue;
					GameObject tile = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
					if (tile != null && tile.PrefabID() == "InsulationTile")
					{
						x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y); x1 = Mathf.Max(x1, x); y1 = Mathf.Max(y1, y);
					}
				}
			return new CellRect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
		}

		private static IEnumerator Drag(TestContext t, CellRect r)
		{
			UpgradeTool tool = null;
			foreach (InterfaceTool candidate in PlayerController.Instance.tools)
				if (candidate is UpgradeTool found)
					tool = found;
			t.Assert.NotNull(tool, "the Upgrade Planner tool is among the player's tools");
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			PlayerController.Instance.ActivateTool(tool);
			yield return t.Frames(2);
			Vector3 from = Grid.CellToPosCCC(r.Cell(0, 0), Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(r.Cell(r.W - 1, r.H - 1), Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return null;
			tool.OnLeftClickDown(from);
			yield return null;
			tool.OnMouseMove(to);
			yield return null;
			tool.OnLeftClickUp(to);
			yield return t.Frames(3);
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
		}

		private static int X(BuildingComplete b) { Grid.CellToXY(Grid.PosToCell(b), out int x, out _); return x; }
		private static int Y(BuildingComplete b) { Grid.CellToXY(Grid.PosToCell(b), out _, out int y); return y; }
		private static string Xy(int cell) { Grid.CellToXY(cell, out int x, out int y); return x + "," + y; }

		private static string Describe(BuildingComplete b) => Describe(b != null ? b.gameObject : null);

		private static string Describe(GameObject go)
		{
			if (go == null)
				return "nothing";
			string kind = go.GetComponent<BuildingComplete>() != null ? "built" : go.GetComponent<Constructable>() != null ? "ORDER" : "?";
			Building building = go.GetComponent<Building>();
			return go.PrefabID().Name + " " + kind + " " + go.GetComponent<PrimaryElement>()?.ElementID
				+ (building != null ? " " + building.Orientation : "") + " at " + Xy(Grid.PosToCell(go));
		}
	}
}
