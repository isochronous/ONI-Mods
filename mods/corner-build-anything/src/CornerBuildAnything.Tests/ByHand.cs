using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace CornerBuildAnything.Tests
{
	/// <summary>
	/// Duplicants doing the work through the corner, as in a colony: instant build off, the orders
	/// placed, materials on the floor, a crew in a sealed box. Each building stands on the floor in
	/// an enclosure that is walled on every side; the only way to it is the corner cell at its top
	/// right, which a stair of one-tile steps leads up to. Duplicants climb one tile and drop at most
	/// two, so the route there is walking, stepping up and short drops only.
	/// </summary>
	public static class ByHand
	{
		[OniTest("Corner Build Anything: Duplicants build a switch, a 2x2 generator and a bridge end through corners and take them down again", Timeout = 240, Boxed = true), Layer(1)]
		public static IEnumerator BuildAndTakeDown(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			// Left to right, so the crew coming from the pile on the right climbs over the lower
			// enclosures and never has to drop more than two tiles: the 2x2 generator, the switch,
			// and the bridge, all three of its cells sealed in so only its right end has the corner.
			int generatorCorner = Enclose(t, inside, 2, 2, 2);
			int switchCorner = Enclose(t, inside, 9, 1, 1);
			int bridgeCorner = Enclose(t, inside, 16, 3, 1);
			yield return t.WaitGameSeconds(2);

			BuildingDef generator = Assets.GetBuildingDef("ManualGenerator");
			BuildingDef logicSwitch = Assets.GetBuildingDef("LogicSwitch");
			BuildingDef bridge = Assets.GetBuildingDef("LogicWireBridge");
			int generatorCell = inside.Cell(2, 0);
			int switchCell = inside.Cell(9, 0);
			int bridgeCell = inside.Cell(17, 0);
			int pile = inside.Cell(30, 0);

			t.InstantBuild(false);
			List<MinionIdentity> crew = t.Dupes.Spawn(2, pile);
			t.Dupes.GrantAll();
			yield return t.WaitGameSeconds(1);
			// The layout, checked before any order: a Duplicant can walk to each corner cell, and no
			// cell beside a building is open to stand in, so the corner is the only way to it.
			Navigator navigator = crew[0].GetComponent<Navigator>();
			foreach (var (name, corner) in new[] { ("generator", generatorCorner), ("switch", switchCorner), ("bridge", bridgeCorner) })
				t.Assert.True(navigator.GetNavigationCost(corner) != -1, "a Duplicant can walk to the " + name + "'s corner cell " + Cells.Describe(corner));
			foreach (var (name, def, cell) in new[] { ("generator", generator, generatorCell), ("switch", logicSwitch, switchCell), ("bridge", bridge, bridgeCell) })
				foreach (int beside in Beside(def, cell))
					t.Assert.True(Grid.Solid[beside], "the " + name + " is walled at " + Cells.Describe(beside));

			t.Speed(3);
			t.Dupes.Drop(SimHashes.Copper, generator.CraftRecipe.Ingredients[0].amount * 2, pile);
			t.Dupes.Drop(SimHashes.Copper, logicSwitch.CraftRecipe.Ingredients[0].amount * 2, pile);
			t.Dupes.Drop(SimHashes.Copper, bridge.CraftRecipe.Ingredients[0].amount * 2, pile);
			yield return t.WaitGameSeconds(2);

			Pockets.Order(t, "ManualGenerator", generatorCell);
			GameObject switchOrder = Pockets.Order(t, "LogicSwitch", switchCell);
			Pockets.Order(t, "LogicWireBridge", bridgeCell);
			yield return t.Frames(3);
			t.Log("switch order reach: " + Pockets.Describe(switchOrder.GetComponent<Constructable>().GetOffsets(switchCell))
				+ ", corner at " + Pockets.Describe(new[] { Pockets.Offset(switchCell, switchCorner) }));
			var jobs = new[] { ("generator", generator, generatorCell), ("switch", logicSwitch, switchCell), ("bridge", bridge, bridgeCell) };
			float start = GameClock.Instance.GetTime();
			foreach (var (name, def, cell) in jobs)
			{
				yield return t.WaitUntil(() => Pockets.Finished(cell, def) != null, 60, "the crew to build the " + name + " through its corner");
				t.Log(name + " built after " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds");
			}
			yield return t.Screenshot("built");

			yield return t.WaitGameSeconds(1);
			foreach (var (name, def, cell) in jobs)
				Pockets.Finished(cell, def).GetComponent<Deconstructable>().QueueDeconstruction(true);
			start = GameClock.Instance.GetTime();
			foreach (var (name, def, cell) in jobs)
			{
				yield return t.WaitUntil(() => Pockets.Status(cell, def) == "nothing there", 40, "the crew to take the " + name + " down through its corner");
				t.Log(name + " down after " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds");
			}
		}

		/// <summary>
		/// Seals a <paramref name="w"/> x <paramref name="h"/> space standing on the floor, bottom-left
		/// cell (x0, 0): a wall on the left, a roof over it, a column on the right, and a stair of
		/// one-tile steps down from that column to the floor. Returns the corner cell, on top of the
		/// column and diagonally above the space's top-right cell; the roof and the column are that
		/// cell's two orthogonal neighbours, so only diagonal reach gets a Duplicant from there in.
		/// </summary>
		private static int Enclose(TestContext t, CellRect inside, int x0, int w, int h)
		{
			for (int y = 0; y < h; y++)
			{
				Pockets.Tile(t, inside.Cell(x0 - 1, y));
				Pockets.Tile(t, inside.Cell(x0 + w, y));
			}
			for (int x = x0 - 1; x < x0 + w; x++)
				Pockets.Tile(t, inside.Cell(x, h));
			for (int k = 1; k < h; k++)
				for (int y = 0; y < h - k; y++)
					Pockets.Tile(t, inside.Cell(x0 + w + k, y));
			return inside.Cell(x0 + w, h);
		}

		/// <summary>The cells orthogonally next to the def's footprint at the cell, outside the footprint.</summary>
		private static IEnumerable<int> Beside(BuildingDef def, int origin)
		{
			var footprint = new HashSet<int>(Pockets.Footprint(def, origin));
			return footprint.SelectMany(c => new[] { Grid.CellLeft(c), Grid.CellRight(c), Grid.CellAbove(c), Grid.CellBelow(c) })
				.Where(c => !footprint.Contains(c)).Distinct();
		}
	}
}
