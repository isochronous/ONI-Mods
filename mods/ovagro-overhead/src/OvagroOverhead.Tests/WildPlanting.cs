using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// Wild planting: where a Pip looks for a place for an Ovagro seed, what the seed grows when it is
	/// planted at a cell, and a real Pip doing it from start to finish. Every test lays a strip of
	/// natural sandstone one row below the box's built ceiling, open at both ends (built tiles are
	/// Foundation, which the seed refuses). A Pip starts on top of the strip and reaches its underside
	/// by walking round an end.
	/// </summary>
	public static class WildPlanting
	{
		private const int StripX0 = 4;
		private const int StripX1 = 33;

		private static int StripRow(CellRect inside)
		{
			return inside.H - 2;
		}

		private static IEnumerator Strip(TestContext t, CellRect inside)
		{
			yield return Kit.NaturalCeiling(t, inside, StripX0, StripX1, StripRow(inside));
		}

		private static PlantableSeed PrefabSeed()
		{
			return Assets.GetPrefab(CeilingForm.SeedTag).GetComponent<PlantableSeed>();
		}

		private static GameObject FindPlant(CellRect inside, Tag prefabId)
		{
			foreach (int cell in inside.Cells)
			{
				GameObject go = Kit.PlantAt(cell);
				if (Kit.Is(go, prefabId))
					return go;
			}
			return null;
		}

		[OniTest("WILD: a Pip looking for a place for an Ovagro seed finds the cells under a natural ceiling", Timeout = 60, Boxed = true), Layer(0)]
		public static IEnumerator QueryFindsCeilingCells(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			yield return Strip(t, inside);
			int y = StripRow(inside);
			GameObject pip = Kit.Pip(t, inside.Cell(10, y + 1));
			GameObject seedGo = Kit.Seed(t, inside.Cell(14, y + 1));
			yield return t.WaitGameSeconds(2);
			yield return Kit.AssertReaches(t, pip, inside.Cell(20, y - 1), "a cell under the strip");

			PlantableSeed seed = seedGo.GetComponent<PlantableSeed>();
			PlantableCellQuery query = PathFinderQueries.plantableCellQuery.Reset(seed, 20);
			pip.GetComponent<Navigator>().RunQuery(query);
			var cells = new List<int>(query.result_cells);
			var described = new List<string>();
			foreach (int cell in cells)
				described.Add(Cells.Describe(cell));
			t.Log(cells.Count + " planting cells: " + string.Join(", ", described.ToArray()));

			t.Assert.True(cells.Count > 0, "the Pip finds somewhere to plant the seed");
			foreach (int cell in cells)
			{
				Grid.CellToXY(cell, out int x, out int cy);
				t.Assert.True(Grid.Solid[Grid.CellAbove(cell)], "solid ground above " + Cells.Describe(cell));
				t.Assert.False(Grid.Foundation[Grid.CellAbove(cell)], "the ground above " + Cells.Describe(cell) + " is natural");
				t.Assert.Eq(inside.Y + y - 1, cy, "the cell " + Cells.Describe(cell) + " is in the row under the strip");
				t.Assert.False(Grid.Solid[Grid.CellBelow(cell)], "room for the hanging plant below " + Cells.Describe(cell));
			}
			Kit.AssertSeedIsVanilla(t, seed, "the seed in the world");
			Kit.AssertSeedIsVanilla(t, PrefabSeed(), "the seed prefab");
		}

		[OniTest("WILD: an Ovagro seed planted under a natural ceiling grows the hanging Ovagro", Timeout = 60, Boxed = true), Layer(0)]
		public static IEnumerator PlantUnderCeiling(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			yield return Strip(t, inside);
			int y = StripRow(inside);
			int cell = inside.Cell(20, y - 1);
			GameObject seedGo = Kit.HeldSeed(t, cell);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(cell, Grid.PosToCell(seedGo), "the held seed is still under the ceiling");

			seedGo.GetComponent<PlantableSeed>().TryPlant(true);
			yield return t.WaitGameSeconds(2);

			GameObject plant = Kit.PlantAt(cell);
			if (plant != null)
				t.Cleanup(() => { if (plant != null) Util.KDestroyGameObject(plant); });
			t.Assert.True(Kit.Is(plant, CeilingForm.PlantTag), "a ceiling Ovagro grows at the seed's cell (found " + (plant != null ? plant.PrefabID().Name : "nothing") + ")");
			t.Assert.Eq(plant, Kit.PlantAt(Grid.CellBelow(cell)), "it hangs into the cell below");
			t.Assert.True(seedGo == null, "the seed was used up");
			Kit.AssertSeedIsVanilla(t, PrefabSeed(), "the seed prefab");
			KBatchedAnimController anim = plant.GetComponent<KBatchedAnimController>();
			t.Assert.Eq(180f, anim.Rotation, "the art is turned over");
			t.Assert.True(anim.FlipX, "the art is mirrored back so the plant faces the same way");

			yield return t.WaitGameSeconds(3);
			t.Assert.True(plant != null && !plant.HasTag(GameTags.Uprooted), "the ceiling Ovagro is still rooted a few seconds later");
			yield return t.LookAt(cell, 5f);
			yield return t.Screenshot("ceiling-plant");
		}

		[OniTest("WILD: an Ovagro seed planted on a natural floor still grows the standing Ovagro", Timeout = 60, Boxed = true), Layer(0)]
		public static IEnumerator PlantOnFloor(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			yield return Kit.NaturalFloor(t, inside, StripX0, StripX1, 0);
			int cell = inside.Cell(20, 1);
			GameObject seedGo = Kit.Seed(t, cell);
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(cell, Grid.PosToCell(seedGo), "the seed lies on the floor");

			seedGo.GetComponent<PlantableSeed>().TryPlant(true);
			yield return t.WaitGameSeconds(2);

			GameObject plant = Kit.PlantAt(cell);
			if (plant != null)
				t.Cleanup(() => { if (plant != null) Util.KDestroyGameObject(plant); });
			t.Assert.True(Kit.Is(plant, CeilingForm.VanillaPlantTag), "the vanilla Ovagro grows on the floor (found " + (plant != null ? plant.PrefabID().Name : "nothing") + ")");
			t.Assert.Eq(plant, Kit.PlantAt(Grid.CellAbove(cell)), "it stands into the cell above");
			Kit.AssertSeedIsVanilla(t, PrefabSeed(), "the seed prefab");
		}

		/// <summary>
		/// A real Pip: it finds the seed on top of the strip, picks it up, walks round the strip's end
		/// onto its underside and plants there. The box floor and walls are built tiles, so the
		/// underside of the strip is the only place the seed can go. A Pip looks for seeds every 60 to
		/// 300 seconds; the test sets its next look to now each game second until it has planted.
		/// </summary>
		[OniTest("WILD: a Pip carries an Ovagro seed onto the ceiling and plants the hanging Ovagro", Timeout = 180, Boxed = true), Layer(0)]
		public static IEnumerator PipPlantsOnCeiling(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			yield return Strip(t, inside);
			int y = StripRow(inside);
			GameObject pip = Kit.Pip(t, inside.Cell(10, y + 1));
			int seedCell = inside.Cell(24, y + 1);
			Kit.Seed(t, seedCell);
			yield return t.WaitGameSeconds(2);
			yield return Kit.AssertReaches(t, pip, inside.Cell(20, y - 1), "a cell under the strip");
			yield return Kit.AssertReaches(t, pip, seedCell, "the seed");

			SeedPlantingMonitor.Instance monitor = pip.GetSMI<SeedPlantingMonitor.Instance>();
			t.Assert.NotNull(monitor, "the Pip plants seeds");
			float start = Time.realtimeSinceStartup;
			GameObject plant = null;
			while (plant == null)
			{
				if (Time.realtimeSinceStartup - start > 160f)
					t.Assert.Fail("no ceiling Ovagro after 160 s; the Pip is at " + Cells.Describe(Grid.PosToCell(pip)) + " in " + pip.GetComponent<Navigator>().CurrentNavType);
				monitor.nextSearchTime = 0f;
				yield return t.WaitGameSeconds(1);
				plant = FindPlant(inside, CeilingForm.PlantTag);
			}
			GameObject planted = plant;
			t.Cleanup(() => { if (planted != null) Util.KDestroyGameObject(planted); });
			int cell = Grid.PosToCell(plant);
			t.Log("planted at " + Cells.Describe(cell) + " after " + (Time.realtimeSinceStartup - start).ToString("0") + " s");
			yield return t.WaitGameSeconds(2);

			Grid.CellToXY(cell, out int x, out int cy);
			t.Assert.Eq(inside.Y + y - 1, cy, "the Pip planted in the row under the strip");
			t.Assert.True(x >= inside.X + StripX0 && x <= inside.X + StripX1, "under the strip, not past its ends");
			t.Assert.Eq(plant, Kit.PlantAt(Grid.CellBelow(cell)), "the plant hangs into the cell below");
			t.Assert.Null(FindPlant(inside, CeilingForm.VanillaPlantTag), "no standing Ovagro anywhere in the box");
			Kit.AssertSeedIsVanilla(t, PrefabSeed(), "the seed prefab");
			t.Assert.True(plant != null && !plant.HasTag(GameTags.Uprooted), "the planted Ovagro is rooted");
			yield return t.LookAt(cell, 6f);
			yield return t.Screenshot("pip-planted");
		}
	}
}
