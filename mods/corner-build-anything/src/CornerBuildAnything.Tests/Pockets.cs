using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace CornerBuildAnything.Tests
{
	/// <summary>
	/// A corner pocket: one open cell walled in by tiles on the sides a Duplicant could stand beside
	/// it, with a single open cell diagonally next to it that has a floor to stand on. The game's
	/// standard reach table needs one of the pocket's two orthogonal neighbours at that corner to be
	/// open, so only the corner table gets a Duplicant from there to the pocket.
	/// </summary>
	internal static class Pockets
	{
		/// <summary>
		/// Tiles round the pocket at (px, py) of the rectangle: below, above, and (with
		/// <paramref name="far"/>) on the side away from the corner. The corner cell is
		/// (px + side, py + 1), floored by the tile at (px + side, py). With <paramref name="stair"/>
		/// that tile is the top of a stair down to the floor row, so a Duplicant can walk up to the
		/// corner cell.
		/// </summary>
		public static void Build(TestContext t, CellRect r, int px, int py, int side, bool far, bool stair)
		{
			Tile(t, r.Cell(px, py - 1));
			Tile(t, r.Cell(px, py + 1));
			if (far)
				Tile(t, r.Cell(px - side, py));
			if (!stair)
			{
				Tile(t, r.Cell(px + side, py));
				return;
			}
			for (int k = 1; k <= py + 1; k++)
				for (int y = 0; y <= py - k + 1; y++)
					Tile(t, r.Cell(px + k * side, y));
		}

		/// <summary>
		/// A walled hole for a w x h building whose bottom-left cell is (x0, y0): a ring of tiles round
		/// it, the corner cell diagonally beyond its top corner on <paramref name="side"/> standing on the
		/// ring's tile. With <paramref name="stair"/> the ring's column on that side is the top of a
		/// stair down to the floor row. Returns the corner cell.
		/// </summary>
		public static int BuildRing(TestContext t, CellRect r, int x0, int y0, int w, int h, int side, bool stair)
		{
			var tiles = new HashSet<int>();
			for (int x = x0 - 1; x <= x0 + w; x++)
				for (int y = y0 - 1; y <= y0 + h; y++)
					if (x < x0 || x >= x0 + w || y < y0 || y >= y0 + h)
						tiles.Add(r.Cell(x, y));
			int column = side > 0 ? x0 + w : x0 - 1;
			int top = y0 + h;
			if (stair)
				for (int k = 0; k < top; k++)
					for (int y = 0; y < top - k; y++)
						tiles.Add(r.Cell(column + k * side, y));
			int corner = r.Cell(column, top);
			tiles.Remove(corner);
			foreach (int cell in tiles)
				Tile(t, cell);
			return corner;
		}

		/// <summary>The cells a def covers when its origin is at the cell.</summary>
		public static IEnumerable<int> Footprint(BuildingDef def, int origin)
		{
			return def.PlacementOffsets.Select(o => Grid.OffsetCell(origin, o));
		}

		/// <summary>
		/// The finished building of the def whose origin is the cell, looked up now (construction swaps
		/// the order for a new object). Found by position among all finished buildings, not through the
		/// grid at the cell: a bridge is registered on its end cells only.
		/// </summary>
		public static GameObject Finished(int cell, BuildingDef def)
		{
			foreach (BuildingComplete b in Components.BuildingCompletes.Items)
				if (b != null && b.Def == def && Grid.PosToCell(b.transform.GetPosition()) == cell)
					return b.gameObject;
			return null;
		}

		/// <summary>The unbuilt order for the def whose origin is the cell, or null.</summary>
		public static GameObject OrderAt(int cell, BuildingDef def)
		{
			foreach (Constructable c in Object.FindObjectsOfType<Constructable>())
				if (c != null && c.GetComponent<Building>() != null && c.GetComponent<Building>().Def == def && Grid.PosToCell(c.transform.GetPosition()) == cell)
					return c.gameObject;
			return null;
		}

		/// <summary>What exists of the def at the cell: "finished", "an order" or "nothing there".</summary>
		public static string Status(int cell, BuildingDef def)
		{
			if (Finished(cell, def) != null)
				return "finished";
			return OrderAt(cell, def) != null ? "an order" : "nothing there";
		}

		public static int Corner(CellRect r, int px, int py, int side)
		{
			return r.Cell(px + side, py + 1);
		}

		public static void Tile(TestContext t, int cell)
		{
			t.World.Build("Tile", cell, element: SimHashes.IgneousRock);
		}

		/// <summary>The offset of <paramref name="to"/> from <paramref name="from"/>.</summary>
		public static CellOffset Offset(int from, int to)
		{
			Grid.CellToXY(from, out int fx, out int fy);
			Grid.CellToXY(to, out int tx, out int ty);
			return new CellOffset(tx - fx, ty - fy);
		}

		/// <summary>An unbuilt order for the def at the cell, as the build tool leaves one (instant build must be off).</summary>
		public static GameObject Order(TestContext t, string prefabId, int cell, Orientation orientation = Orientation.Neutral, SimHashes element = SimHashes.Copper)
		{
			BuildingDef def = Assets.GetBuildingDef(prefabId);
			var elements = new List<Tag> { element.CreateTag() };
			GameObject go = def.TryPlace(null, Grid.CellToPosCBC(cell, def.SceneLayer), orientation, elements);
			t.Assert.NotNull(go, "the game accepts an order for " + prefabId + " at " + Cells.Describe(cell));
			t.Assert.NotNull(go.GetComponent<Constructable>(), prefabId + " is under construction");
			t.Assert.NotNull(OrderAt(cell, def), "the order for " + prefabId + " is found by position at " + Cells.Describe(cell));
			t.Log("ordered " + prefabId + " at " + Cells.Describe(cell) + (orientation != Orientation.Neutral ? " rotated " + orientation : ""));
			return go;
		}

		/// <summary>
		/// What the game's own standard table would give the same worker: a tracker over the table the
		/// game uses without the mod, for the building's footprint, asked the same question.
		/// </summary>
		public static CellOffset[] StandardOffsets(Workable workable, BuildingDef def, Orientation orientation, int cell, bool useFilter)
		{
			CellOffset[] area = def.PlacementOffsets.Select(o => Rotatable.GetRotatedCellOffset(o, orientation)).ToArray();
			CellOffset[][] table = OffsetGroups.BuildReachabilityTable(area, OffsetGroups.InvertedStandardTable, useFilter ? def.ConstructionOffsetFilter : null);
			var tracker = new OffsetTableTracker(table, workable);
			try
			{
				return tracker.GetOffsets(cell).ToArray();
			}
			finally
			{
				tracker.Clear();
			}
		}

		public static void Has(TestContext t, CellOffset offset, IEnumerable<CellOffset> offsets, string message)
		{
			t.Assert.True(offsets.Contains(offset), message + " (" + Describe(new[] { offset }) + " in " + Describe(offsets) + ")");
		}

		public static void Lacks(TestContext t, CellOffset offset, IEnumerable<CellOffset> offsets, string message)
		{
			t.Assert.False(offsets.Contains(offset), message + " (" + Describe(new[] { offset }) + " in " + Describe(offsets) + ")");
		}

		public static string Describe(IEnumerable<CellOffset> offsets)
		{
			return "{" + string.Join(" ", offsets.OrderBy(o => o.x).ThenBy(o => o.y).Select(o => "(" + o.x + "," + o.y + ")")) + "}";
		}

		/// <summary>Waits for the nav grid to see the tiles just built: the corner cell must be somewhere to stand.</summary>
		public static IEnumerator Settle(TestContext t)
		{
			yield return t.WaitGameSeconds(1.5);
		}
	}
}
