using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// Wires upgraded to another metal: a row of three copper wires, the plan "this wire, any
	/// material, with the same wire in gold", a box dragged over them with the real tool. The
	/// game lets the utility build tool replace a wire with no tag asked of it, so the planner
	/// must too; the new wire or order must draw the old one's connections.
	/// </summary>
	public static class WireReplace
	{
		[OniTest("Upgrade Planner: automation wires queue gold replacements with their connections", Timeout = 40), Layer(1)]
		public static IEnumerator LogicWireOrders(TestContext t)
		{
			yield return Run(t, "LogicWire", ObjectLayer.LogicWire, ObjectLayer.ReplacementLogicWire, instant: false);
		}

		[OniTest("Upgrade Planner: power wires queue gold replacements with their connections", Timeout = 40), Layer(1)]
		public static IEnumerator PowerWireOrders(TestContext t)
		{
			yield return Run(t, "Wire", ObjectLayer.Wire, ObjectLayer.ReplacementWire, instant: false);
		}

		[OniTest("Upgrade Planner: with instant build, automation wires are gold at once, still connected", Timeout = 40), Layer(1)]
		public static IEnumerator LogicWireInstant(TestContext t)
		{
			yield return Run(t, "LogicWire", ObjectLayer.LogicWire, ObjectLayer.ReplacementLogicWire, instant: true);
		}

		[OniTest("Upgrade Planner: with instant build, power wires are gold at once, still connected", Timeout = 40), Layer(1)]
		public static IEnumerator PowerWireInstant(TestContext t)
		{
			yield return Run(t, "Wire", ObjectLayer.Wire, ObjectLayer.ReplacementWire, instant: true);
		}

		private static IEnumerator Run(TestContext t, string wireId, ObjectLayer layer, ObjectLayer replacementLayer, bool instant)
		{
			CellRect r = t.Rect(5, 1);
			int[] cells = { r.Cell(1, 0), r.Cell(2, 0), r.Cell(3, 0) };
			// Three copper wires in a row, dragged with the real wire tool so their connections are real.
			UtilityDragSession wire = t.Tools.UtilityTool(wireId, SimHashes.Copper);
			yield return wire.Path(cells);
			wire.Dispose();
			yield return t.Frames(2);
			foreach (int cell in cells)
				t.Assert.NotNull(t.World.At(cell, layer)?.GetComponent<BuildingComplete>(), "a copper " + wireId + " at " + Cells.Describe(cell));

			// The plan: this wire, whatever it is made of, replaced with the same wire in gold. Made
			// while instant build is still on, as the window lists only researched targets otherwise
			// and the fixture has researched nothing.
			BuildingDef def = Assets.GetBuildingDef(wireId);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ReplaceType;
			plan.ReplaceType.SetSource(Families.Of(def));
			plan.ReplaceType.SetSourceAny();
			t.Assert.True(ReplacementRules.TargetCategories(plan.ReplaceType.Source).Exists(c => c.Families.Exists(f => f.Contains(def))), "the wire is offered as its own replacement");
			plan.ReplaceType.SetTarget(Families.Of(def));
			plan.ReplaceType.SetTargetMaterials(new List<Tag> { SimHashes.Gold.CreateTag() });
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(instant);

			// The planner tool, driven as the player drives it: a box from the first wire to the last.
			UpgradeTool tool = null;
			foreach (InterfaceTool candidate in PlayerController.Instance.tools)
				if (candidate is UpgradeTool found)
					tool = found;
			t.Assert.NotNull(tool, "the Upgrade Planner tool is among the player's tools");
			PlayerController.Instance.ActivateTool(tool);
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			yield return t.Frames(2);
			Vector3 from = Grid.CellToPosCCC(cells[0], Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(cells[2], Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return null;
			tool.OnLeftClickDown(from);
			yield return null;
			tool.OnMouseMove(to);
			yield return null;
			tool.OnLeftClickUp(to);
			yield return t.Frames(3);

			UtilityConnections[] expected = { UtilityConnections.Right, UtilityConnections.Left | UtilityConnections.Right, UtilityConnections.Left };
			for (int i = 0; i < cells.Length; i++)
			{
				int cell = cells[i];
				string at = Cells.Describe(cell);
				if (instant)
				{
					GameObject built = t.World.At(cell, layer);
					t.Assert.NotNull(built?.GetComponent<BuildingComplete>(), "a finished wire at " + at);
					t.Assert.Null(built.GetComponent<Constructable>(), "no order left at " + at);
					t.Assert.Eq(SimHashes.Gold, built.GetComponent<PrimaryElement>().ElementID, "the wire at " + at + " is gold");
					t.Assert.Eq(expected[i], built.GetComponent<KAnimGraphTileVisualizer>().Connections, "the gold wire at " + at + " keeps its connections");
					t.Assert.Null(t.World.At(cell, replacementLayer), "nothing on the replacement layer at " + at);
				}
				else
				{
					GameObject order = t.World.At(cell, replacementLayer);
					t.Assert.NotNull(order, "a replacement order at " + at);
					t.Assert.NotNull(order.GetComponent<Constructable>(), "the replacement at " + at + " is an order");
					t.Assert.Eq(SimHashes.Gold, order.GetComponent<PrimaryElement>().ElementID, "the order at " + at + " is for gold");
					t.Assert.Eq(expected[i], order.GetComponent<KAnimGraphTileVisualizer>().Connections, "the order at " + at + " draws the old wire's connections");
					t.Assert.Eq(SimHashes.Copper, t.World.At(cell, layer).GetComponent<PrimaryElement>().ElementID, "the copper wire at " + at + " is still there until the order completes");
				}
			}
		}
	}
}
