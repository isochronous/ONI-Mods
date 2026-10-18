using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace OvagrosOverhead.Tests
{
	/// <summary>
	/// The ceiling Ovagro through a save and a load: a wild one under a natural ceiling with its vines,
	/// and one in an upside-down Hydroponic Farm. The first test saves the colony to the run folder and
	/// writes down where everything was; the second (FreshSave) starts from that save and finds each
	/// thing where it was, in the same form.
	/// </summary>
	public static class Persistence
	{
		private static string RoundTripPath => Path.Combine(Session.Dir, "ovagro-roundtrip.sav");

		/// <summary>What the first test saved, one "key value..." per line, for the second to check.</summary>
		private static string ExpectedPath => Path.Combine(Session.Dir, "ovagro-roundtrip.txt");

		private const int MotherX = 20;
		private const int FarmX = 6;

		private static void AddChain(List<string> lines, string side, List<VineBranch.Instance> chain)
		{
			foreach (VineBranch.Instance b in chain)
				lines.Add(side + " " + Grid.PosToCell(b.gameObject) + " " + b.MyShape);
		}

		[OniTest("PERSIST: (round trip 1/2) a ceiling Ovagro with its vines and one in an upside-down Hydroponic Farm are saved to the run folder", Timeout = 200, Boxed = true), Layer(0)]
		public static IEnumerator WriteRoundTrip(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int y = inside.H - 2;
			yield return Kit.NaturalCeiling(t, inside, MotherX - 6, MotherX + 6, y);
			int motherCell = inside.Cell(MotherX, y - 1);
			GameObject mother = Kit.Spawn(t, CeilingForm.PlantId, motherCell, Grid.SceneLayer.BuildingFront);

			int farmCell = inside.Cell(FarmX, 3);
			PlantablePlot farm = Kit.Plot(t, "HydroponicFarm", farmCell, Orientation.FlipV);
			t.World.Build("LiquidConduit", farmCell);
			GameObject seed = Kit.Seed(t, inside.Cell(FarmX + 2, 0));
			yield return t.WaitGameSeconds(2);
			farm.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			farm.ForceDeposit(seed);
			yield return t.WaitGameSeconds(1);
			Kit.Water(farm, 20f);
			GameObject farmPlant = farm.Occupant;
			t.Assert.True(Kit.Is(farmPlant, CeilingForm.PlantTag), "the farm holds a hanging Ovagro");

			yield return Kit.GrowMother(t, mother);
			VineMother.Instance smi = mother.GetSMI<VineMother.Instance>();
			yield return Kit.GrowChains(t, smi, 3, 3, 90);
			VineMother.Instance farmSmi = farmPlant.GetSMI<VineMother.Instance>();
			yield return t.WaitUntil(() => Kit.IsGrown(farmSmi), 60, "the farm's Ovagro to grow");
			yield return t.WaitGameSeconds(5);

			List<VineBranch.Instance> left = Kit.Chain(smi.LeftBranch);
			List<VineBranch.Instance> right = Kit.Chain(smi.RightBranch);
			t.Log("left: " + Kit.Describe(left));
			t.Log("right: " + Kit.Describe(right));
			t.Assert.True(left.Count >= 3 && right.Count >= 3, "three branches or more on each side before saving");
			foreach (VineBranch.Instance b in left.Concat(right))
				t.Assert.Eq(VineBranch.Shape.Top, b.MyShape, "every branch hugs the ceiling before saving (" + Cells.Describe(Grid.PosToCell(b.gameObject)) + ")");
			t.Assert.Eq(farmPlant, farm.Occupant, "the farm still holds its plant before saving");

			var lines = new List<string>
			{
				"mother " + motherCell,
				"farm " + farmCell,
				"farmplant " + Grid.PosToCell(farmPlant),
			};
			AddChain(lines, "left", left);
			AddChain(lines, "right", right);
			File.WriteAllLines(ExpectedPath, lines.ToArray());

			string path = RoundTripPath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (FreshSave) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
		}

		private static List<KeyValuePair<int, VineBranch.Shape>> Expected(string[] lines, string side)
		{
			var list = new List<KeyValuePair<int, VineBranch.Shape>>();
			foreach (string line in lines)
			{
				string[] parts = line.Split(' ');
				if (parts[0] == side)
					list.Add(new KeyValuePair<int, VineBranch.Shape>(int.Parse(parts[1]), (VineBranch.Shape)System.Enum.Parse(typeof(VineBranch.Shape), parts[2])));
			}
			return list;
		}

		private static int Cell(string[] lines, string key)
		{
			return int.Parse(lines.First(l => l.StartsWith(key + " ")).Split(' ')[1]);
		}

		private static void AssertChain(TestContext t, List<VineBranch.Instance> chain, List<KeyValuePair<int, VineBranch.Shape>> expected, string side)
		{
			t.Assert.Eq(expected.Count, chain.Count, side + " side has as many branches as were saved (" + Kit.Describe(chain) + ")");
			for (int i = 0; i < expected.Count; i++)
			{
				t.Assert.Eq(Cells.Describe(expected[i].Key), Cells.Describe(Grid.PosToCell(chain[i].gameObject)), side + " branch " + (i + 1) + " is where it was");
				t.Assert.Eq(expected[i].Value, chain[i].MyShape, side + " branch " + (i + 1) + " keeps its shape");
				t.Assert.False(chain[i].gameObject.HasTag(GameTags.Uprooted), side + " branch " + (i + 1) + " is rooted");
			}
			t.Assert.Eq(VineBranch.Shape.Top, chain[0].RootShape, "the first " + side + " branch still takes the ceiling as its root's shape");
		}

		private static void AssertHanging(TestContext t, GameObject plant, string which)
		{
			KBatchedAnimController anim = plant.GetComponent<KBatchedAnimController>();
			t.Assert.Eq(180f, anim.Rotation, which + "'s art is still turned over");
			t.Assert.True(anim.FlipX, which + "'s art is still mirrored back");
			t.Assert.False(plant.HasTag(GameTags.Uprooted), which + " is rooted");
			t.Assert.True(Kit.IsGrown(plant.GetSMI<VineMother.Instance>()), which + " is still grown");
		}

		[OniTest("PERSIST: (round trip 2/2) after loading, both ceiling Ovagro hang where they were, the vines keep their shapes and the farm keeps its plant", Timeout = 90, FreshSave = true), Layer(0)]
		public static IEnumerator RoundTripLoad(TestContext t)
		{
			string loaded = Session.SavePath;
			t.Cleanup(() =>
			{
				string original = Directory.GetFiles(Session.Dir, "fixture.sav").FirstOrDefault();
				if (original != null)
					Session.SavePath = original;
				if (File.Exists(RoundTripPath))
					File.Delete(RoundTripPath);
				if (File.Exists(ExpectedPath))
					File.Delete(ExpectedPath);
			});
			t.Assert.True(loaded.EndsWith("ovagro-roundtrip.sav"), "this test started from the round trip save (" + loaded + ")");
			t.Assert.True(File.Exists(ExpectedPath), "the first test wrote down what it saved");
			string[] lines = File.ReadAllLines(ExpectedPath);
			t.Log("fixture area after the load: " + t.Area);
			yield return t.WaitGameSeconds(2);

			int motherCell = Cell(lines, "mother");
			GameObject mother = Kit.PlantAt(motherCell);
			t.Assert.True(Kit.Is(mother, CeilingForm.PlantTag), "the wild ceiling Ovagro is at " + Cells.Describe(motherCell) + " (found " + (mother != null ? mother.PrefabID().Name : "nothing") + ")");
			t.Assert.Eq(mother, Kit.PlantAt(Grid.CellBelow(motherCell)), "it still hangs into the cell below");
			AssertHanging(t, mother, "the wild ceiling Ovagro");
			VineMother.Instance smi = mother.GetSMI<VineMother.Instance>();
			AssertChain(t, Kit.Chain(smi.LeftBranch), Expected(lines, "left"), "left");
			AssertChain(t, Kit.Chain(smi.RightBranch), Expected(lines, "right"), "right");

			int farmCell = Cell(lines, "farm");
			PlantablePlot farm = Object.FindObjectsByType<PlantablePlot>(FindObjectsSortMode.None).FirstOrDefault(p => Grid.PosToCell(p) == farmCell);
			t.Assert.NotNull(farm, "the Hydroponic Farm is at " + Cells.Describe(farmCell));
			t.Assert.True(CeilingForm.FacesDown(farm), "the farm still faces down");
			GameObject farmPlant = farm.Occupant;
			int farmPlantCell = Cell(lines, "farmplant");
			t.Assert.True(Kit.Is(farmPlant, CeilingForm.PlantTag), "the farm still holds the ceiling Ovagro (found " + (farmPlant != null ? farmPlant.PrefabID().Name : "nothing") + ")");
			t.Assert.Eq(Cells.Describe(farmPlantCell), Cells.Describe(Grid.PosToCell(farmPlant)), "the farm's plant hangs where it did");
			t.Assert.Eq(farmPlant, Kit.PlantAt(Grid.CellBelow(farmPlantCell)), "the farm's plant still hangs into the next cell down");
			t.Assert.True(farm.plant != null && farm.plant.gameObject == farmPlant, "the farm's saved link to its plant is restored");
			t.Assert.True(farmPlant.GetComponent<ReceptacleMonitor>().Replanted, "the plant knows it is in a plot");
			t.Assert.True(farm.GetComponent<RequireInputs>().RequiresInputConduit, "the farm still asks for its water pipe");
			AssertHanging(t, farmPlant, "the farm's Ovagro");
			Kit.AssertSeedPrefabIsVanilla(t, "after the load");

			// A few seconds of the loaded game: nothing is uprooted or lost once the sim has run.
			t.Speed(3);
			yield return t.WaitGameSeconds(6);
			t.Assert.True(mother != null && !mother.HasTag(GameTags.Uprooted), "the wild ceiling Ovagro is still rooted after the game ran");
			AssertChain(t, Kit.Chain(smi.LeftBranch), Expected(lines, "left"), "left");
			AssertChain(t, Kit.Chain(smi.RightBranch), Expected(lines, "right"), "right");
			t.Assert.True(farmPlant != null && farm.Occupant == farmPlant && !farmPlant.HasTag(GameTags.Uprooted), "the farm's Ovagro is still in the farm, rooted");

			Grid.CellToXY(motherCell, out int mx, out int my);
			Grid.CellToXY(farmCell, out int fx, out int _);
			yield return t.LookAt(Grid.XYToCell((mx + fx) / 2 + 2, my - 1), 8f);
			yield return t.Screenshot("loaded");
		}
	}
}
