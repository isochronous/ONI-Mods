using System.Collections.Generic;
using HarmonyLib;
using PeterHan.PLib.UI;
using UnityEngine;

namespace Bitshifter
{
	/// <summary>
	/// The toolkit's patches. Each carries the toolkit's Harmony category, so BitshifterMod.OnLoad
	/// applies them on their own and they stay in place when the mod leaves the Reader and Writer
	/// to another mod. A patch here without the category would stand down with the Reader and
	/// Writer patches instead.
	/// </summary>
	public static class ToolkitPatches
	{
		/// <summary>Puts each toolkit building in the build menu, after the building its row names.</summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
		{
			public static void Prefix()
			{
				foreach (Toolkit.Entry entry in Toolkit.Buildings)
					ModUtil.AddBuildingToPlanScreen(Toolkit.PlanCategory, entry.Id, entry.Subcategory, entry.After, ModUtil.BuildingOrdering.After);
			}
		}

		/// <summary>Adds each toolkit building to its tech; a tech the game does not have leaves the building unlocked from the start.</summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Postfix()
			{
				foreach (Toolkit.Entry entry in Toolkit.Buildings)
				{
					Tech tech = Db.Get().Techs.TryGet(entry.Tech);
					if (tech == null)
					{
						Debug.LogWarning("[Bitshifter] Tech '" + entry.Tech + "' not found; " + entry.Id + " will be unlocked from the start");
						continue;
					}
					if (!tech.unlockedItemIDs.Contains(entry.Id))
						tech.unlockedItemIDs.Add(entry.Id);
				}
			}
		}

		/// <summary>Registers the toolkit's side screens (PLib) with the details screen.</summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class DetailsScreen_OnPrefabInit_Patch
		{
			public static void Postfix()
			{
				PUIUtils.AddSideScreenContent<RibbonDiagnosticPanelSideScreen>();
			}
		}

		/// <summary>
		/// The vanilla bit selector highlights the one row whose bit GetBitSelection returns (state 0)
		/// and shows every other row plain (state 1). It does so on SetTarget and after each click,
		/// both through RefreshToggles. For a Fan-out Gate every picked row is highlighted instead.
		/// The screen keeps the rows it has made, so a 32nd row left from a Reader with 32 bits
		/// (Digital CPU) stays plain; the gate ignores a click on it. Other targets are left alone.
		/// </summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(LogicBitSelectorSideScreen), "RefreshToggles")]
		public static class LogicBitSelectorSideScreen_RefreshToggles_Patch
		{
			public static void Postfix(LogicBitSelectorSideScreen __instance, ILogicRibbonBitSelector ___target)
			{
				if (!(___target is FanoutGate gate))
					return;
				foreach (KeyValuePair<int, MultiToggle> row in __instance.toggles_by_int)
					row.Value.ChangeState(gate.IsPicked(row.Key) ? 0 : 1);
			}
		}

		/// <summary>
		/// Puts the bit selector's rows in a scroll view when a Fan-out Gate is shown, as
		/// LogicBitSelectorSideScreen_SetTarget_Patch does for any target while the mod handles the
		/// Reader and Writer. This one only acts for the gate, so the Reader's and Writer's screens are
		/// left as they are while the mod leaves them to another mod.
		/// </summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(LogicBitSelectorSideScreen), nameof(LogicBitSelectorSideScreen.SetTarget))]
		public static class LogicBitSelectorSideScreen_SetTarget_GatePatch
		{
			public static void Postfix(LogicBitSelectorSideScreen __instance, GameObject new_target)
			{
				if (new_target != null && new_target.GetComponent<FanoutGate>() != null)
					LogicBitSelectorSideScreen_SetTarget_Patch.WrapRows(__instance);
			}
		}
	}
}
