using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// The mod loaded and registered its prefabs as the design says, and left the vanilla Ovagro
	/// Node and its seed as they were. Also the natural-ceiling helper the later tests build on.
	/// </summary>
	public static class Registration
	{
		private static GameObject Prefab(TestContext t, string id)
		{
			GameObject prefab = Assets.TryGetPrefab(id);
			t.Assert.NotNull(prefab, "prefab " + id + " is registered");
			return prefab;
		}

		[OniTest("REGISTRATION: the ceiling Ovagro and its preview are registered and the vanilla plant and seed are untouched", Timeout = 30, NoColony = true), Layer(0)]
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

			var irrigation = ceiling.GetDef<IrrigationMonitor.Def>();
			var vanillaIrrigation = vanilla.GetDef<IrrigationMonitor.Def>();
			t.Assert.NotNull(irrigation, "the ceiling plant is irrigated");
			t.Assert.Eq(vanillaIrrigation.consumedElements.Length, irrigation.consumedElements.Length, "irrigation entries");
			t.Assert.Eq(vanillaIrrigation.consumedElements[0].tag, irrigation.consumedElements[0].tag, "irrigation tag");
			t.Assert.Eq(vanillaIrrigation.consumedElements[0].massConsumptionRate, irrigation.consumedElements[0].massConsumptionRate, "irrigation rate");

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
