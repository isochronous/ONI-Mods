using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace SmartWeightPlate.Tests
{
	/// <summary>What the plate weighs: a solid tile, loose items, a creature standing on it; never a gas or a liquid.</summary>
	public static class Weighing
	{
		[OniTest("SWP: weighs a solid tile, a loose pile and a creature above it, and not a liquid or a gas", Timeout = 60), Layer(1)]
		public static IEnumerator Sources(TestContext t)
		{
			CellRect r = t.Rect(5, 4);
			int cage = r.Cell(2, 1);
			// A cage one cell wide above the plate, so the creature stays on it.
			t.World.Build("Tile", r.Cell(1, 1));
			t.World.Build("Tile", r.Cell(3, 1));
			t.World.Build("Tile", r.Cell(2, 2));
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(2, 0), 10, 100);
			yield return t.WaitGameSeconds(1.5);
			t.Assert.Eq(0f, plate.CurrentValue, "an empty cage weighs nothing");

			GameObject tile = t.World.Build("Tile", cage);
			yield return Kit.Settle(t);
			t.Assert.True(Grid.Mass[cage] > 1f, "the tile has mass");
			t.Assert.Eq(Grid.Mass[cage], plate.CurrentValue, "a solid tile above is weighed by its mass");
			t.World.Destroy(tile);
			yield return t.WaitGameSeconds(1.5);
			t.Assert.Eq(0f, plate.CurrentValue, "nothing left once the tile is gone");

			SimMessages.ReplaceElement(cage, SimHashes.Water, CellEventLogger.Instance.SandBoxTool, 300f, 293.15f);
			yield return Kit.Settle(t);
			t.Assert.Eq(0f, plate.CurrentValue, "water is not weighed");
			SimMessages.ReplaceElement(cage, SimHashes.Oxygen, CellEventLogger.Instance.SandBoxTool, 1f, 293.15f);
			yield return Kit.Settle(t);
			t.Assert.Eq(0f, plate.CurrentValue, "a gas is not weighed");

			GameObject hatch = GameUtil.KInstantiate(Assets.GetPrefab("Hatch"), Grid.CellToPosCBC(cage, Grid.SceneLayer.Creatures), Grid.SceneLayer.Creatures);
			hatch.SetActive(true);
			yield return t.WaitGameSeconds(2);
			float mass = hatch.GetComponent<PrimaryElement>().Mass;
			t.Assert.True(mass > 1f, "the Hatch has mass");
			t.Assert.True(Mathf.Abs(plate.CurrentValue - mass) < 0.01f, "a creature standing on it is weighed once, by its own mass (" + plate.CurrentValue + " against " + mass + ")");

			t.Dupes.Drop(SimHashes.Iron, 40f, cage);
			yield return Kit.Settle(t);
			t.Assert.True(Mathf.Abs(plate.CurrentValue - (mass + 40f)) < 0.01f, "a loose pile adds to it (" + plate.CurrentValue + ")");
		}

		[OniTest("SWP: is a buildable of Advanced Automation under Sensors, costing 75 kg of refined metal, with thresholds from 0 to 2000", Timeout = 20), Layer(0)]
		public static IEnumerator Registration(TestContext t)
		{
			BuildingDef def = Assets.GetBuildingDef(SmartWeightPlateConfig.ID);
			t.Assert.NotNull(def, "the building exists");
			t.Assert.Eq(75f, def.Mass[0], "75 kg");
			t.Assert.Eq("RefinedMetal", def.MaterialCategory[0], "of refined metal");
			t.Assert.True(Db.Get().Techs.Get("LogicCircuits").unlockedItemIDs.Contains(SmartWeightPlateConfig.ID), "unlocked by Advanced Automation");

			bool found = false;
			foreach (PlanScreen.PlanInfo info in TUNING.BUILDINGS.PLANORDER)
				if (info.category == (HashedString)"Automation")
				{
					int plate = -1, vanilla = -1;
					for (int i = 0; i < info.buildingAndSubcategoryData.Count; i++)
					{
						if (info.buildingAndSubcategoryData[i].Key == SmartWeightPlateConfig.ID) plate = i;
						if (info.buildingAndSubcategoryData[i].Key == FloorSwitchConfig.ID) vanilla = i;
					}
					found = plate >= 0 && vanilla >= 0 && plate == vanilla + 1 && info.buildingAndSubcategoryData[plate].Value == "sensors";
				}
			t.Assert.True(found, "right after the Weight Plate under Automation > Sensors");

			var plateComponent = t.World.Build(SmartWeightPlateConfig.ID, t.Rect(1, 1).Cell(0, 0)).GetComponent<SmartWeightPlate>();
			t.Assert.Eq(0f, plateComponent.MinValue, "from 0");
			t.Assert.Eq(2000f, plateComponent.MaxValue, "to 2000 kg");
			t.Assert.True(plateComponent.UseWholeNumbers, "in whole kilograms");
			t.Assert.Eq(100f, plateComponent.ActivateValue, "high threshold default");
			t.Assert.Eq(10f, plateComponent.DeactivateValue, "low threshold default");
			plateComponent.ActivateValue = 5000.4f;
			plateComponent.DeactivateValue = -3f;
			t.Assert.Eq(2000f, plateComponent.ActivateValue, "a value above the range is clamped");
			t.Assert.Eq(0f, plateComponent.DeactivateValue, "and one below");
			yield return t.Frames(1);
		}
	}
}
