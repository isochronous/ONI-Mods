using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

namespace OvagrosOverhead
{
	/// <summary>
	/// The vines of the ceiling Ovagro: the vanilla Ovagro vine (<c>VineBranchConfig</c>) under its own
	/// prefab id, built with the same calls and values. The ceiling form of the plant grows these, and
	/// each one grows the next of its own kind, so the whole vine follows the surface exactly as the
	/// vanilla one does (along the ceiling, round corners, down walls and across floors).
	///
	/// Their own id is what makes removing the mod clean. Without the mod the game drops every object
	/// whose prefab it does not know, so the ceiling plant and all of its vines go together. Vanilla
	/// vines would stay behind with no plant, and one of them throws on load and keeps the radiation
	/// monitor throwing every tick.
	/// </summary>
	public sealed class VineBranchCeilingConfig : IEntityConfig, IHasDlcRestrictions
	{
		/// <summary>Must differ from the vanilla "VineBranchOriginal": the plant template creates a trait under this id.</summary>
		private const string BaseTraitId = "VineBranchCeilingOriginal";

		public string[] GetRequiredDlcIds()
		{
			return DlcManager.DLC4;
		}

		public string[] GetForbiddenDlcIds()
		{
			return null;
		}

		// Written out for net48, as in VineMotherCeilingConfig; GetDlcIds stays null.
		public string[] GetAnyRequiredDlcIds()
		{
			return null;
		}

		public string[] GetDlcIds()
		{
			return null;
		}

		public GameObject CreatePrefab()
		{
			string name = STRINGS.CREATURES.SPECIES.VINEBRANCH.NAME;
			List<Tag> additionalTags = new List<Tag>
			{
				GameTags.HideFromSpawnTool,
				GameTags.HideFromCodex,
				GameTags.PlantBranch,
				GameTags.ExcludeFromTemplate,
			};
			GameObject branch = EntityTemplates.CreatePlacedEntity(CeilingForm.BranchId, name,
				STRINGS.CREATURES.SPECIES.VINEBRANCH.DESC, 1f, Assets.GetAnim("vine_kanim"), "line_idle",
				Grid.SceneLayer.BuildingFront, 1, 1, DECOR.BONUS.TIER0, default(EffectorValues), SimHashes.Creature,
				additionalTags, 308.15f);
			EntityTemplates.ExtendEntityToBasicPlant(branch, 273.15f, 298.15f, 318.15f, 378.15f, null,
				pressure_sensitive: false, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, crop_id: null,
				can_drown: true, can_tinker: true, require_solid_tile: false, require_Backwall_Foundation: false,
				should_grow_old: false, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f,
				baseTraitId: BaseTraitId, baseTraitName: name);
			branch.AddOrGet<HarvestDesignatable>();
			branch.AddOrGet<CodexEntryRedirector>().CodexID = VineMotherConfig.ID;
			branch.AddOrGet<PlantFiberProducer>().amount = VineBranchConfig.PLANT_FIBER_PRODUCED_PER_CYCLE;
			branch.AddOrGet<UprootedMonitor>();
			Crop.CropVal crop = CROPS.CROP_TYPES.Find(c => c.cropId == VineFruitConfig.ID);
			branch.AddOrGet<Crop>().Configure(crop);
			Modifiers modifiers = branch.GetComponent<Modifiers>();
			if (branch.GetComponent<Traits>() == null)
			{
				branch.AddOrGet<Traits>();
				modifiers.initialTraits.Add(BaseTraitId);
			}
			modifiers.initialAmounts.Add(Db.Get().Amounts.Maturity.Id);
			modifiers.initialAmounts.Add(Db.Get().Amounts.Maturity2.Id);
			modifiers.initialAttributes.Add(Db.Get().PlantAttributes.YieldAmount.Id);
			Trait trait = Db.Get().traits.Get(modifiers.initialTraits[0]);
			trait.Add(new AttributeModifier(Db.Get().Amounts.Maturity.maxAttribute.Id, VineBranchConfig.GROWING_DURATION_CYCLES, name));
			trait.Add(new AttributeModifier(Db.Get().Amounts.Maturity2.maxAttribute.Id, VineBranchConfig.GROWING_DURATION_CYCLES, name));
			trait.Add(new AttributeModifier(Db.Get().PlantAttributes.YieldAmount.Id, crop.numProduced, name));
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, CeilingForm.BranchId);
			branch.AddOrGetDef<VineBranch.Def>().BRANCH_PREFAB_NAME = CeilingForm.BranchId;
			branch.AddOrGet<Harvestable>();
			WiltCondition wilt = branch.AddOrGet<WiltCondition>();
			wilt.WiltDelay = 0f;
			wilt.RecoveryDelay = 0f;
			SeedProducer seeds = branch.AddOrGet<SeedProducer>();
			seeds.Configure(VineMotherConfig.SEED_ID, SeedProducer.ProductionType.HarvestOnly);
			seeds.seedDropChanceMultiplier = 1f / 6f;
			return branch;
		}

		public void OnPrefabInit(GameObject inst)
		{
			inst.AddOrGet<UprootedMonitor>().monitorCells = new CellOffset[0];
			inst.AddOrGet<HarvestDesignatable>().iconOffset = new Vector2(0f, Grid.CellSizeInMeters * 0.75f);
		}

		public void OnSpawn(GameObject inst)
		{
		}
	}
}
