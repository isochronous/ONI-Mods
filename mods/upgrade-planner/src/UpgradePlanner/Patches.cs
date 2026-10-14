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
				ModStrings.Register();
				ToolIcon.Register();
			}
		}

		/// <summary>Adds the tool to the player's tool set, the way the vanilla tools are set up.</summary>
		[HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
		public static class PlayerController_OnPrefabInit_Patch
		{
			public static void Postfix(PlayerController __instance)
			{
				foreach (InterfaceTool existing in __instance.tools)
					if (existing is UpgradeTool)
						return;
				GameObject go = new GameObject(nameof(UpgradeTool));
				go.transform.SetParent(__instance.gameObject.transform);
				go.SetActive(value: false);
				UpgradeTool tool = go.AddComponent<UpgradeTool>();
				// Force OnPrefabInit now, then park the tool inactive until it is selected.
				go.SetActive(value: true);
				go.SetActive(value: false);
				InterfaceTool[] tools = new InterfaceTool[__instance.tools.Length + 1];
				Array.Copy(__instance.tools, tools, __instance.tools.Length);
				tools[tools.Length - 1] = tool;
				__instance.tools = tools;
			}
		}

		/// <summary>
		/// The game sets a material picker's height and scrollbar for the build menu's side bar every
		/// time it refreshes the toggles (every 200 ms, from its own LateUpdate). For the planner's
		/// pickers only, the planner's size is put back in the same call, so the game's never reaches
		/// a drawn frame. Build menu pickers carry no keeper and are left as they are.
		/// </summary>
		[HarmonyPatch(typeof(MaterialSelector), "UpdateScrollBar")]
		public static class MaterialSelector_UpdateScrollBar_Patch
		{
			public static void Postfix(MaterialSelector __instance)
			{
				MaterialPanel.SizeKeeper keeper = __instance.GetComponent<MaterialPanel.SizeKeeper>();
				if (keeper != null)
					keeper.Apply();
			}
		}

		/// <summary>Adds the toolbar button.</summary>
		[HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
		public static class ToolMenu_CreateBasicTools_Patch
		{
			public static void Postfix(ToolMenu __instance)
			{
				__instance.basicTools.Add(ToolMenu.CreateToolCollection(
					ModStrings.ToolName, ToolIcon.SpriteName, UpgradePlannerMod.ToolAction,
					nameof(UpgradeTool), ModStrings.ToolTooltip, largeIcon: false));
			}
		}
	}
}
