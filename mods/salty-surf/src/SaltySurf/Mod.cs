using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Isochronous;
using Klei;
using KMod;
using STRINGS;
using UnityEngine;

namespace SaltySurf
{
	public sealed class SaltySurfMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Translations.Register(harmony, typeof(ModStrings));
			Debug.Log("[SaltySurf] Loaded version " + typeof(SaltySurfMod).Assembly.GetName().Version);
		}
	}

	/// <summary>
	/// The liquids the surfboard accepts. The vanilla building keys its intake, its readiness
	/// check, and its spill to the Water element alone; anything else arriving in the pipe damages
	/// the building and is dumped on the floor. The mod tags the acceptable elements instead and
	/// points those three places at the tag.
	/// </summary>
	public static class SurfWater
	{
		public static readonly Tag Tag = TagManager.Create("SaltySurfWater");
		public static readonly SimHashes[] Elements = { SimHashes.Water, SimHashes.SaltWater };

		/// <summary>"Water or Salt Water", with the names the game is using, joined by the mod's translated connective.</summary>
		public static string Names()
		{
			string first = ElementLoader.FindElementByHash(Elements[0]).name;
			string second = ElementLoader.FindElementByHash(Elements[1]).name;
			return string.Format(ModStrings.SALTYSURF.ELEMENT_LIST, first, second);
		}
	}

	/// <summary>
	/// Element tags are copied onto every stored chunk when it is created, so adding the tag to
	/// the element definitions once, right after they load, covers the pipe check, the storage
	/// mass check, and the spill.
	/// </summary>
	[HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
	public static class ElementLoader_Load_Patch
	{
		public static void Postfix()
		{
			// The tag has no display name, so keep it out of the Properties line. A hidden tag writes
			// nothing itself; the visible tag before it writes the separator unless it is last. A
			// hidden tag in last place would leave a trailing separator, so the tag goes first.
			GameTags.HiddenElementTags.Add(SurfWater.Tag);
			foreach (SimHashes hash in SurfWater.Elements)
			{
				Element element = ElementLoader.FindElementByHash(hash);
				if (element != null && !element.HasTag(SurfWater.Tag))
				{
					Tag[] tags = new Tag[element.oreTags.Length + 1];
					tags[0] = SurfWater.Tag;
					element.oreTags.CopyTo(tags, 1);
					element.oreTags = tags;
				}
			}
		}
	}

	[HarmonyPatch(typeof(MechanicalSurfboardConfig), nameof(MechanicalSurfboardConfig.ConfigureBuildingTemplate))]
	public static class MechanicalSurfboardConfig_Patch
	{
		public static void Postfix(GameObject go)
		{
			ConduitConsumer consumer = go.GetComponent<ConduitConsumer>();
			if (consumer != null)
				consumer.capacityTag = SurfWater.Tag;
		}
	}

	/// <summary>Ready when enough of any accepted liquid is in the tank, not only Water.</summary>
	[HarmonyPatch(typeof(MechanicalSurfboard.States), "IsReady")]
	public static class MechanicalSurfboard_IsReady_Patch
	{
		public static bool Prefix(MechanicalSurfboard.StatesInstance smi, ref bool __result)
		{
			__result = smi.GetComponent<Storage>().GetMassAvailable(SurfWater.Tag) >= smi.master.minOperationalWaterKG;
			return false;
		}
	}

	/// <summary>
	/// The vanilla tick draws Water from the tank and drops Water on the floor. This draws from
	/// whatever accepted liquid is in the tank and drops that same element.
	/// </summary>
	[HarmonyPatch(typeof(MechanicalSurfboardWorkable), "OnWorkTick")]
	public static class MechanicalSurfboardWorkable_OnWorkTick_Patch
	{
		public static bool Prefix(MechanicalSurfboardWorkable __instance, float dt, ref bool __result)
		{
			Building building = __instance.GetComponent<Building>();
			MechanicalSurfboard surfboard = __instance.GetComponent<MechanicalSurfboard>();
			int width = building.Def.WidthInCells;
			int x = Random.Range(-(width - 1) / 2, width / 2);
			float amount = surfboard.waterSpillRateKG * dt;
			__instance.GetComponent<Storage>().ConsumeAndGetDisease(SurfWater.Tag, amount, out float consumed,
				out SimUtil.DiseaseInfo disease, out float temperature, out SimHashes element);
			if (consumed > 0f && element != SimHashes.Vacuum)
			{
				int cell = Grid.OffsetCell(Grid.PosToCell(__instance.gameObject), new CellOffset(x, 0));
				FallingWater.instance.AddParticle(cell, ElementLoader.GetElementIndex(element), consumed, temperature,
					disease.idx, disease.count, skip_sound: true);
			}
			__result = false;
			return false;
		}
	}

	/// <summary>The building's info panel lists the accepted liquids instead of Water alone.</summary>
	[HarmonyPatch]
	public static class MechanicalSurfboard_GetDescriptors_Patch
	{
		public static MethodBase TargetMethod()
		{
			return typeof(MechanicalSurfboard).GetMethod("IGameObjectEffectDescriptor.GetDescriptors",
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static void Postfix(MechanicalSurfboard __instance, List<Descriptor> __result)
		{
			string water = ElementLoader.FindElementByHash(SimHashes.Water).name;
			string amount = GameUtil.GetFormattedMass(__instance.minOperationalWaterKG);
			string vanilla = BUILDINGS.PREFABS.MECHANICALSURFBOARD.WATER_REQUIREMENT.Replace("{element}", water).Replace("{amount}", amount);
			for (int i = 0; i < __result.Count; i++)
			{
				if (__result[i].text != vanilla)
					continue;
				Descriptor descriptor = __result[i];
				descriptor.text = BUILDINGS.PREFABS.MECHANICALSURFBOARD.WATER_REQUIREMENT.Replace("{element}", SurfWater.Names()).Replace("{amount}", amount);
				descriptor.tooltipText = BUILDINGS.PREFABS.MECHANICALSURFBOARD.WATER_REQUIREMENT_TOOLTIP.Replace("{element}", SurfWater.Names()).Replace("{amount}", amount);
				__result[i] = descriptor;
			}
		}
	}
}
