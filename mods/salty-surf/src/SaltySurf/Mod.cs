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

	/// <summary>
	/// The building's info panel lists the accepted liquids instead of Water alone. The panel text is
	/// edited in place after the game has built it, in whichever way the running build builds it:
	/// builds up to U59 list the descriptors through the explicit
	/// <c>IGameObjectEffectDescriptor.GetDescriptors</c>, later builds through <c>CollectDescriptors</c>.
	/// </summary>
	public static class SurfDescriptors
	{
		private const string Amount = "{amount}";

		/// <summary>Swaps the Water requirement line for one naming every accepted liquid. The line is found by its template, so the amount's formatting is whatever the game used.</summary>
		public static void Rewrite(List<Descriptor> descriptors)
		{
			if (descriptors == null)
				return;
			string template = BUILDINGS.PREFABS.MECHANICALSURFBOARD.WATER_REQUIREMENT;
			int at = template.IndexOf(Amount);
			if (at < 0)
				return;
			string water = ElementLoader.FindElementByHash(SimHashes.Water).name;
			string head = template.Substring(0, at).Replace("{element}", water);
			string tail = template.Substring(at + Amount.Length).Replace("{element}", water);
			for (int i = 0; i < descriptors.Count; i++)
			{
				string text = descriptors[i].text;
				if (text == null || text.Length < head.Length + tail.Length || !text.StartsWith(head) || !text.EndsWith(tail))
					continue;
				string amount = text.Substring(head.Length, text.Length - head.Length - tail.Length);
				Descriptor descriptor = descriptors[i];
				descriptor.text = BUILDINGS.PREFABS.MECHANICALSURFBOARD.WATER_REQUIREMENT.Replace("{element}", SurfWater.Names()).Replace(Amount, amount);
				descriptor.tooltipText = BUILDINGS.PREFABS.MECHANICALSURFBOARD.WATER_REQUIREMENT_TOOLTIP.Replace("{element}", SurfWater.Names()).Replace(Amount, amount);
				descriptors[i] = descriptor;
			}
		}
	}

	/// <summary>Builds up to U59: the building implements <c>IGameObjectEffectDescriptor.GetDescriptors</c> explicitly.</summary>
	[HarmonyPatch]
	public static class MechanicalSurfboard_GetDescriptors_Patch
	{
		private static MethodBase Find()
		{
			return typeof(MechanicalSurfboard).GetMethod("IGameObjectEffectDescriptor.GetDescriptors",
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static bool Prepare()
		{
			return Find() != null;
		}

		public static MethodBase TargetMethod()
		{
			return Find();
		}

		public static void Postfix(List<Descriptor> __result)
		{
			SurfDescriptors.Rewrite(__result);
		}
	}

	/// <summary>Builds from U60: the building adds its descriptors to the caller's list in <c>CollectDescriptors</c>.</summary>
	[HarmonyPatch]
	public static class MechanicalSurfboard_CollectDescriptors_Patch
	{
		private static MethodBase Find()
		{
			return typeof(MechanicalSurfboard).GetMethod("CollectDescriptors",
				BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(GameObject), typeof(List<Descriptor>) }, null);
		}

		public static bool Prepare()
		{
			return Find() != null;
		}

		public static MethodBase TargetMethod()
		{
			return Find();
		}

		public static void Postfix(List<Descriptor> descriptors)
		{
			SurfDescriptors.Rewrite(descriptors);
		}
	}
}
