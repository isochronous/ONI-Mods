using System;
using HarmonyLib;
using UnityEngine;

namespace UpgradePlanner
{
	public static class Patches
	{
		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Postfix()
			{
				ToolIcon.Register();
			}
		}

		/// <summary>Adds the tool to the player's tool set, the way the game's own tools are set up.</summary>
		[HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
		public static class PlayerController_OnPrefabInit_Patch
		{
			public static void Postfix(PlayerController __instance)
			{
				foreach (InterfaceTool existing in __instance.tools)
					if (existing is UpgradeTool)
						return;
				var go = new GameObject(nameof(UpgradeTool));
				go.transform.SetParent(__instance.gameObject.transform);
				go.SetActive(false);
				UpgradeTool tool = go.AddComponent<UpgradeTool>();
				// Runs OnPrefabInit now, then parks the tool inactive until it is selected.
				go.SetActive(true);
				go.SetActive(false);
				InterfaceTool[] tools = new InterfaceTool[__instance.tools.Length + 1];
				Array.Copy(__instance.tools, tools, __instance.tools.Length);
				tools[tools.Length - 1] = tool;
				__instance.tools = tools;
			}
		}

		/// <summary>Adds the toolbar button.</summary>
		[HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
		public static class ToolMenu_CreateBasicTools_Patch
		{
			public static void Postfix(ToolMenu __instance)
			{
				__instance.basicTools.Add(ToolMenu.CreateToolCollection(
					ModStrings.UI.TOOLS.UPGRADEPLANNER.NAME, ToolIcon.SpriteName, UpgradePlannerMod.ToolAction,
					nameof(UpgradeTool), ModStrings.UI.TOOLS.UPGRADEPLANNER.TOOLTIP, largeIcon: false));
			}
		}

		/// <summary>
		/// The game sets a material picker's height and scrollbar for the build menu's side bar each
		/// time it refreshes the toggles (every 200 ms, from its own LateUpdate). For the planner's
		/// pickers only, the planner's size is put back in the same call, so the game's never reaches
		/// a drawn frame. Build menu pickers carry no keeper and are left as they are.
		/// </summary>
		[HarmonyPatch(typeof(MaterialSelector), "UpdateScrollBar")]
		public static class MaterialSelector_UpdateScrollBar_Patch
		{
			public static void Postfix(MaterialSelector __instance)
			{
				MaterialPicker.SizeKeeper keeper = __instance.GetComponent<MaterialPicker.SizeKeeper>();
				if (keeper != null)
					keeper.Apply();
			}
		}

		/// <summary>A rebuilt building whose new order the game could not place in time is placed once its spot is clear; see <see cref="Orders.PlaceRebuildIfMissing"/>.</summary>
		[HarmonyPatch(typeof(Reconstructable), "TryCommenceReconstruct")]
		public static class Reconstructable_TryCommenceReconstruct_Patch
		{
			public static void Postfix(Reconstructable __instance)
			{
				Orders.PlaceRebuildIfMissing(__instance);
			}
		}
	}
}
