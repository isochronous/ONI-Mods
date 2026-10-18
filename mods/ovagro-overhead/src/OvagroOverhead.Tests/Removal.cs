using System.Collections;
using System.Collections.Generic;
using System.IO;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// The first half of the check of what removing the mod does to a save, run on request
	/// (-Filter REMOVAL): ceiling Ovagro in each state a player can leave one in, saved to the run
	/// folder with a list of their cells. The second half loads that save with the mod off: run a
	/// [OniTest(KeepSave = true)] test from another mod's suite with -Fixture pointing at the save, so
	/// the harness leaves the scene as saved. On 2026-10-18 that showed the ceiling plant and its vines
	/// dropped on load with one "Could not find prefab" warning each, no seed and no errors, and an
	/// open Ovagro order on a downward Farm Tile kept.
	/// </summary>
	public static class Removal
	{
		public static string SavePath => Path.Combine(Session.Dir, "ovagro-removal.sav");

		public static string CellsPath => Path.Combine(Session.Dir, "ovagro-removal.txt");

		/// <summary>The set-up in the rectangle; its cells go into the list under the prefix.</summary>
		private static IEnumerator Build(TestContext t, CellRect rect, string prefix, List<string> lines)
		{
			int row = Preview.GlassRow;
			const int farmX = 15, wildX = 4, tileX = 25;
			for (int x = 0; x < rect.W; x++)
				if (x != farmX && x != tileX)
					t.World.Build("GlassTile", rect.Cell(x, row), element: SimHashes.Glass);

			int farmCell = rect.Cell(farmX, row);
			PlantablePlot farm = Kit.Plot(t, "HydroponicFarm", farmCell, Orientation.FlipV);
			t.World.Build("LiquidConduit", farmCell);
			PlantablePlot tile = Kit.Plot(t, "FarmTile", rect.Cell(tileX, row), Orientation.FlipV);
			GameObject seed = Kit.Seed(t, rect.Cell(farmX + 2, 0));
			yield return t.WaitGameSeconds(2);
			farm.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			tile.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			farm.ForceDeposit(seed);
			yield return t.WaitGameSeconds(1);
			Kit.Water(farm, 50f);
			GameObject farmPlant = farm.Occupant;
			t.Assert.True(Kit.Is(farmPlant, CeilingForm.PlantTag), prefix + ": the farm holds a hanging Ovagro");

			int wildCell = rect.Cell(wildX, row - 1);
			GameObject wild = Kit.Spawn(t, CeilingForm.PlantId, wildCell, Grid.SceneLayer.BuildingFront);
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(wild, Kit.PlantAt(wildCell), prefix + ": the wild ceiling Ovagro hangs under the glass");

			yield return Kit.GrowMother(t, farmPlant);
			VineMother.Instance smi = farmPlant.GetSMI<VineMother.Instance>();
			yield return Kit.GrowChains(t, smi, 3, 3, 90);
			t.Assert.True(tile.Occupant == null, prefix + ": the Farm Tile has an open order and no plant");

			lines.Add(prefix + " rect " + rect.X + " " + rect.Y + " " + rect.W + " " + rect.H);
			lines.Add(prefix + " farm " + farmCell);
			lines.Add(prefix + " farmplant " + Grid.PosToCell(farmPlant));
			lines.Add(prefix + " wild " + wildCell);
			lines.Add(prefix + " tile " + rect.Cell(tileX, row));
			foreach (VineBranch.Instance b in Kit.Chain(smi.LeftBranch))
				lines.Add(prefix + " branch " + Grid.PosToCell(b.gameObject));
			foreach (VineBranch.Instance b in Kit.Chain(smi.RightBranch))
				lines.Add(prefix + " branch " + Grid.PosToCell(b.gameObject));
		}

		[OniTest("REMOVAL: ceiling Ovagro in a Hydroponic Farm with vines, a wild one, and an open order are saved for loading without the mod", Timeout = 300, Explicit = true, FreshSave = true), Layer(0)]
		public static IEnumerator SaveForRemoval(TestContext t)
		{
			t.Speed(3);
			var lines = new List<string>();
			var inside = new CellRect[1];
			yield return Preview.Stage(t, Preview.FindFrozenSpot(), inside);
			yield return Build(t, inside[0], "a", lines);
			File.WriteAllLines(CellsPath, lines.ToArray());
			t.Log(string.Join("; ", lines.ToArray()));

			if (File.Exists(SavePath))
				File.Delete(SavePath);
			SaveLoader.Instance.Save(SavePath, false, false);
			t.Assert.True(File.Exists(SavePath), "the save was written to " + SavePath);
		}
	}
}
