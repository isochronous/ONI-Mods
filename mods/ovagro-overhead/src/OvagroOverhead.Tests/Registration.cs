using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// The mod loaded and registered its prefabs as the design says (the ceiling plant, its preview
	/// and its vines), and left the vanilla Ovagro Node, its vines and its seed as they were. Also the natural-ceiling helper the later tests build on.
	/// </summary>
	public static class Registration
	{
		private static GameObject Prefab(TestContext t, string id)
		{
			GameObject prefab = Assets.TryGetPrefab(id);
			t.Assert.NotNull(prefab, "prefab " + id + " is registered");
			return prefab;
		}

		[OniTest("REGISTRATION: the ceiling Ovagro, its preview and its vines are registered and the vanilla plant, vines and seed are untouched", Timeout = 30, NoColony = true), Layer(0)]
		public static IEnumerator Prefabs(TestContext t)
		{
			t.Assert.True(DlcManager.IsContentSubscribed(DlcManager.DLC4_ID), "the Prehistoric Planet Pack is active");

			GameObject ceiling = Prefab(t, CeilingForm.PlantId);
			GameObject vanilla = Prefab(t, VineMotherConfig.ID);
			t.Assert.True(ceiling.HasTag(GameTags.Hanging), "the ceiling plant is Hanging");
			t.Assert.True(ceiling.HasTag(GameTags.HideFromCodex), "the ceiling plant is hidden from the codex");
			t.Assert.False(vanilla.HasTag(GameTags.Hanging), "the vanilla plant is not Hanging");

			CellOffset[] occupied = ceiling.GetComponent<OccupyArea>()._UnrotatedOccupiedCellsOffsets;
			t.Assert.Eq(2, occupied.Length, "the ceiling plant occupies two cells");
			t.Assert.True(System.Array.IndexOf(occupied, new CellOffset(0, 0)) >= 0, "it occupies (0,0)");
			t.Assert.True(System.Array.IndexOf(occupied, new CellOffset(0, -1)) >= 0, "it occupies (0,-1)");

			CellOffset[] uproot = ceiling.GetComponent<UprootedMonitor>().monitorCells;
			t.Assert.Eq(1, uproot.Length, "one uproot cell");
			t.Assert.Eq(new CellOffset(0, 1), uproot[0], "the uproot cell is the one above");

			t.Assert.Eq(VineMotherConfig.SEED_ID, ceiling.GetComponent<SeedProducer>().seedInfo.seedId, "the ceiling plant drops the vanilla seed");

			TemperatureVulnerable temp = ceiling.GetComponent<TemperatureVulnerable>();
			TemperatureVulnerable vanillaTemp = vanilla.GetComponent<TemperatureVulnerable>();
			t.Assert.Eq(vanillaTemp.TemperatureLethalLow, temp.TemperatureLethalLow, "lethal low temperature");
			t.Assert.Eq(vanillaTemp.TemperatureLethalHigh, temp.TemperatureLethalHigh, "lethal high temperature");
			t.Assert.Eq(vanillaTemp.TemperatureWarningLow, temp.TemperatureWarningLow, "warning low temperature");
			t.Assert.Eq(vanillaTemp.TemperatureWarningHigh, temp.TemperatureWarningHigh, "warning high temperature");

			IrrigationMonitor.Def irrigation = ceiling.GetDef<IrrigationMonitor.Def>();
			IrrigationMonitor.Def vanillaIrrigation = vanilla.GetDef<IrrigationMonitor.Def>();
			t.Assert.NotNull(irrigation, "the ceiling plant is irrigated");
			t.Assert.Eq(vanillaIrrigation.consumedElements.Length, irrigation.consumedElements.Length, "irrigation entries");
			t.Assert.Eq(vanillaIrrigation.consumedElements[0].tag, irrigation.consumedElements[0].tag, "irrigation tag");
			t.Assert.Eq(vanillaIrrigation.consumedElements[0].massConsumptionRate, irrigation.consumedElements[0].massConsumptionRate, "irrigation rate");

			t.Assert.Eq(vanilla.GetComponent<PrimaryElement>().Mass, ceiling.GetComponent<PrimaryElement>().Mass, "the ceiling plant's mass is the vanilla plant's");
			t.Assert.Eq(vanilla.GetDef<VineMother.Def>().MAX_BRANCH_COUNT, ceiling.GetDef<VineMother.Def>().MAX_BRANCH_COUNT, "branch count");
			t.Assert.Eq(CeilingForm.BranchId, ceiling.GetDef<VineMother.Def>().BRANCH_PREFAB_NAME, "the ceiling plant grows the ceiling vines");
			t.Assert.Eq(VineBranchConfig.ID, vanilla.GetDef<VineMother.Def>().BRANCH_PREFAB_NAME, "the vanilla plant still grows the vanilla vines");

			// The ceiling vines are the vanilla vines under their own id, so removing the mod drops them with the plant.
			GameObject branch = Prefab(t, CeilingForm.BranchId);
			GameObject vanillaBranch = Prefab(t, VineBranchConfig.ID);
			t.Assert.Eq(CeilingForm.BranchId, branch.GetDef<VineBranch.Def>().BRANCH_PREFAB_NAME, "a ceiling vine grows ceiling vines");
			t.Assert.Eq(VineBranchConfig.ID, vanillaBranch.GetDef<VineBranch.Def>().BRANCH_PREFAB_NAME, "a vanilla vine still grows vanilla vines");
			t.Assert.True(branch.HasTag(GameTags.PlantBranch) && branch.HasTag(GameTags.HideFromCodex), "the ceiling vine is a hidden plant branch");
			t.Assert.Eq(vanillaBranch.GetComponent<PrimaryElement>().Mass, branch.GetComponent<PrimaryElement>().Mass, "the ceiling vine's mass is the vanilla vine's");
			t.Assert.Eq(vanillaBranch.GetComponent<Crop>().cropId, branch.GetComponent<Crop>().cropId, "the ceiling vine bears Ovagro Figs");
			t.Assert.Eq(vanillaBranch.GetComponent<PlantFiberProducer>().amount, branch.GetComponent<PlantFiberProducer>().amount, "plant fiber");
			SeedProducer seeds = branch.GetComponent<SeedProducer>();
			SeedProducer vanillaSeeds = vanillaBranch.GetComponent<SeedProducer>();
			t.Assert.Eq(vanillaSeeds.seedInfo.seedId, seeds.seedInfo.seedId, "the ceiling vine drops the vanilla seed");
			t.Assert.Eq(vanillaSeeds.seedDropChanceMultiplier, seeds.seedDropChanceMultiplier, "seed drop chance");
			TemperatureVulnerable branchTemp = branch.GetComponent<TemperatureVulnerable>();
			TemperatureVulnerable vanillaBranchTemp = vanillaBranch.GetComponent<TemperatureVulnerable>();
			t.Assert.Eq(vanillaBranchTemp.TemperatureWarningLow, branchTemp.TemperatureWarningLow, "the ceiling vine's warning low temperature");
			t.Assert.Eq(vanillaBranchTemp.TemperatureWarningHigh, branchTemp.TemperatureWarningHigh, "the ceiling vine's warning high temperature");

			GameObject preview = Prefab(t, CeilingForm.PreviewId);
			CellOffset[] previewCells = preview.GetComponent<OccupyArea>()._UnrotatedOccupiedCellsOffsets;
			t.Assert.Eq(2, previewCells.Length, "the preview occupies two cells");
			t.Assert.True(System.Array.IndexOf(previewCells, new CellOffset(0, -1)) >= 0, "the preview hangs a cell below its origin");

			PlantableSeed seed = Prefab(t, VineMotherConfig.SEED_ID).GetComponent<PlantableSeed>();
			t.Assert.Eq(SingleEntityReceptacle.ReceptacleDirection.Top, seed.direction, "the seed is still direction Top");
			t.Assert.Eq(VineMotherConfig.ID, seed.PlantID.Name, "the seed still plants the vanilla plant");
			t.Assert.Eq(VineMotherConfig.ID + "_preview", seed.PreviewID.Name, "the seed still previews the vanilla plant");
			yield break;
		}

		[OniTest("REGISTRATION: the natural-ceiling helper lays sandstone that is solid and not Foundation", Timeout = 60, Boxed = true), Layer(0)]
		public static IEnumerator NaturalCeilingHelper(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int y = inside.H - 2;
			yield return Kit.NaturalCeiling(t, inside, 2, inside.W - 3, y);
			for (int x = 2; x <= inside.W - 3; x++)
			{
				int cell = inside.Cell(x, y);
				t.Assert.True(Grid.Solid[cell], "solid at " + Cells.Describe(cell));
				t.Assert.False(Grid.Foundation[cell], "not Foundation at " + Cells.Describe(cell));
				t.Assert.Eq(SimHashes.SandStone, Grid.Element[cell].id, "sandstone at " + Cells.Describe(cell));
			}
			t.Assert.False(Grid.Solid[inside.Cell(2, y - 1)], "the cell below the ceiling is open");
			yield return t.LookAt(inside.Cell(inside.W / 2, y), 14f);
			yield return t.Screenshot("natural-ceiling");
		}
	}
}
