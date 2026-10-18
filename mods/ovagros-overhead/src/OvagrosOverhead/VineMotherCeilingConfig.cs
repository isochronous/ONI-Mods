using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

namespace OvagrosOverhead
{
	/// <summary>
	/// The ceiling form of the Ovagro Node: a second plant prefab that hangs from the tile above its
	/// origin cell, the way Dasha Saltvine does. The game builds the floor form in
	/// <c>VineMotherConfig.CreatePrefab</c>; this builds the same plant with the same calls and
	/// values, plus the hanging changes from <c>SaltPlantConfig</c>: the <c>Hanging</c> tag, hanging
	/// cell offsets (origin cell and the one below) and an uproot check on the cell above.
	///
	/// There is no second seed. The ceiling form names the vanilla <c>VineMotherSeed</c> in its seed
	/// producer, and <see cref="CeilingForm"/> routes that seed here where the game plants it. So this
	/// config calls neither <c>CreateAndRegisterSeedForPlant</c>, which would register a second
	/// <c>VineMotherSeed</c> prefab, nor <c>CreateAndRegisterPreviewForPlant</c>, which would point the
	/// vanilla seed's <c>PreviewID</c> at this preview.
	///
	/// The art is the vanilla <c>vine_mother_kanim</c> mirrored vertically about the centre of the
	/// origin cell (rotated 180 degrees and flipped horizontally), so it spans from the ceiling surface
	/// down through the cell below. A farm plot rewrites its occupant's anim <c>Offset</c> on every
	/// placement, but nothing touches <c>Rotation</c>, <c>FlipX</c> or <c>Pivot</c>, and all three are
	/// serialised on the prefab's controller, so instances inherit them.
	///
	/// The game's entity loader finds this class by type scan and registers it only when the Prehistoric
	/// Planet Pack is active, as it does the vanilla config.
	/// </summary>
	public sealed class VineMotherCeilingConfig : IEntityConfig, IHasDlcRestrictions
	{
		/// <summary>Must differ from the vanilla "VineMotherOriginal": the plant template creates a trait under this id.</summary>
		private const string BaseTraitId = "VineMotherCeilingOriginal";
		private const string AnimName = "vine_mother_kanim";

		public string[] GetRequiredDlcIds()
		{
			return DlcManager.DLC4;
		}

		public string[] GetForbiddenDlcIds()
		{
			return null;
		}

		// The game's interfaces give these two a default body, which net48 cannot use, so they are
		// written out with the defaults' values. GetDlcIds must stay null: the loader then reads the
		// restrictions above.
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
			List<Tag> additionalTags = new List<Tag>
			{
				GameTags.Hanging,
				GameTags.HideFromSpawnTool,
				GameTags.HideFromCodex,
			};
			GameObject plant = EntityTemplates.CreatePlacedEntity(CeilingForm.PlantId,
				STRINGS.CREATURES.SPECIES.VINEMOTHER.NAME, STRINGS.CREATURES.SPECIES.VINEMOTHER.DESC, 2f,
				Assets.GetAnim(AnimName), "object", Grid.SceneLayer.BuildingFront, 1, 2, DECOR.BONUS.TIER1,
				default(EffectorValues), SimHashes.Creature, additionalTags, 308.15f);
			EntityTemplates.MakeHangingOffsets(plant, 1, 2);
			EntityTemplates.ExtendEntityToBasicPlant(plant,
				VineMotherConfig.TEMPERATURE_LETHAL_LOW, VineMotherConfig.TEMPERATURE_WARNING_LOW,
				VineMotherConfig.TEMPERATURE_WARNING_HIGH, VineMotherConfig.TEMPERATURE_LETHAL_HIGH,
				VineMotherConfig.ALLOWED_ELEMENTS, pressure_sensitive: false, pressure_lethal_low: 0f,
				pressure_warning_low: 0.15f, crop_id: null, can_drown: true, can_tinker: false,
				require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: false,
				max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: BaseTraitId,
				baseTraitName: STRINGS.CREATURES.SPECIES.VINEMOTHER.NAME);
			plant.GetComponent<UprootedMonitor>().monitorCells = new CellOffset[] { new CellOffset(0, 1) };

			WiltCondition wilt = plant.GetComponent<WiltCondition>();
			wilt.WiltDelay = 0f;
			wilt.RecoveryDelay = 0f;
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, CeilingForm.PlantId);
			// The vanilla config adds its base trait here even when the plant template already did; kept for parity.
			plant.AddOrGet<Traits>();
			plant.GetComponent<Modifiers>().initialTraits.Add(BaseTraitId);

			VineMother.Def mother = plant.AddOrGetDef<VineMother.Def>();
			mother.BRANCH_PREFAB_NAME = CeilingForm.BranchId;
			mother.MAX_BRANCH_COUNT = 24;
			plant.AddOrGet<HarvestDesignatable>();
			EntityTemplates.ExtendPlantToIrrigated(plant, new PlantElementAbsorber.ConsumeInfo[]
			{
				new PlantElementAbsorber.ConsumeInfo
				{
					tag = GameTags.Water,
					massConsumptionRate = VineMotherConfig.IRRIGATION_RATE,
				},
			});
			plant.AddOrGet<SeedProducer>().Configure(VineMotherConfig.SEED_ID, SeedProducer.ProductionType.Hidden);
			plant.AddOrGet<CodexEntryRedirector>().CodexID = VineMotherConfig.ID;
			HangFromCeiling(plant);

			GameObject preview = EntityTemplates.CreateAndRegisterPreview(CeilingForm.PreviewId,
				Assets.GetAnim(AnimName), "place", ObjectLayer.Building, 1, 2);
			EntityTemplates.MakeHangingOffsets(preview, 1, 2);
			HangFromCeiling(preview);
			return plant;
		}

		public void OnPrefabInit(GameObject inst)
		{
		}

		public void OnSpawn(GameObject inst)
		{
		}

		/// <summary>
		/// Mirrors the art vertically about the centre of the origin cell (y becomes 1 - y in cells),
		/// so the upright 1x2 anim covers the origin cell and the one below it.
		/// </summary>
		private static void HangFromCeiling(GameObject go)
		{
			KBatchedAnimController anim = go.GetComponent<KBatchedAnimController>();
			anim.Rotation = 180f;
			anim.FlipX = true;
			anim.Pivot = new Vector3(0f, 0.5f, 0f);
		}
	}
}
