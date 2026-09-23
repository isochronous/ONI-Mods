using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RefinedMetalsAsRawMetals
{
	/// <summary>
	/// Building recipes that take "raw metal" (MATERIALS.RAW_METALS) accept any solid element
	/// carrying the Metal tag. Refined metals carry RefinedMetal instead, so after the element
	/// table is loaded every solid refined metal that lacks Metal gets it added.
	///
	/// ElementLoader.Load only creates elements whose dlcId is enabled, so iterating the loaded
	/// table covers exactly the metals this game can have: base game, the enabled DLCs, and any
	/// element another mod defined. Steel, Niobium, Thermium and Iridium already ship with both
	/// tags and are left alone. Discovery is not a factor: the material selector lists every
	/// tagged element and applies its own discovered/available checks afterwards.
	/// </summary>
	[HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
	internal static class ElementLoader_Load_Patch
	{
		private static void Postfix()
		{
			var updated = new List<string>();
			foreach (var element in ElementLoader.elements)
			{
				if (element == null || !element.IsSolid)
					continue;
				if (!element.HasTag(GameTags.RefinedMetal) || element.HasTag(GameTags.Metal))
					continue;

				var tags = new Tag[element.oreTags.Length + 1];
				element.oreTags.CopyTo(tags, 0);
				tags[tags.Length - 1] = GameTags.Metal;
				element.oreTags = tags;
				updated.Add(element.id.ToString());
			}
			Debug.Log("[RefinedMetalsAsRawMetals] Tagged " + updated.Count + " refined metal(s) as raw metal: " + string.Join(", ", updated));
		}
	}
}
