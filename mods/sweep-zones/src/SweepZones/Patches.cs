using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using TOOL = SweepZones.ModStrings.UI.TOOLS.SWEEPZONES;

namespace SweepZones
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

		[HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
		public static class SaveGame_OnPrefabInit_Patch
		{
			public static void Postfix(SaveGame __instance)
			{
				__instance.gameObject.AddOrGet<ZoneStore>();
			}
		}

		[HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
		public static class PlayerController_OnPrefabInit_Patch
		{
			public static void Postfix(PlayerController __instance)
			{
				foreach (InterfaceTool existing in __instance.tools)
					if (existing is SweepZoneTool)
						return;
				GameObject go = new GameObject(nameof(SweepZoneTool));
				go.transform.SetParent(__instance.gameObject.transform);
				go.SetActive(value: false);
				SweepZoneTool tool = go.AddComponent<SweepZoneTool>();
				// Force OnPrefabInit now, the same way PlayerController initializes
				// the vanilla tools (activate then park inactive until selected).
				go.SetActive(value: true);
				go.SetActive(value: false);
				InterfaceTool[] tools = new InterfaceTool[__instance.tools.Length + 1];
				Array.Copy(__instance.tools, tools, __instance.tools.Length);
				tools[tools.Length - 1] = tool;
				__instance.tools = tools;
			}
		}

		[HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
		public static class ToolMenu_CreateBasicTools_Patch
		{
			public static void Postfix(ToolMenu __instance)
			{
				__instance.basicTools.Add(ToolMenu.CreateToolCollection(
					TOOL.NAME, ToolIcon.SpriteName, SweepZonesMod.ToolAction,
					nameof(SweepZoneTool), TOOL.TOOLTIP, largeIcon: false));
			}
		}

		// The Cancel tool sends Cancel to the first item in a cell, which cancels the
		// sweep of the whole pile there. Only items that were marked for sweeping and sit
		// in a sweep zone remember it; a Cancel that reaches unmarked debris (a Cancel
		// drag over digs or buildings) or comes from Move To is not a cancelled sweep.
		[HarmonyPatch(typeof(Clearable), "OnCancel")]
		public static class Clearable_OnCancel_Patch
		{
			public static void Prefix(Pickupable ___pickupable, out List<Pickupable> __state)
			{
				__state = null;
				if (Movable_MarkForMove_Patch.Running > 0)
					return;
				for (ObjectLayerListItem item = ___pickupable.objectLayerListItem; item != null; item = item.nextItem)
				{
					if (item.pickupable == null || !ZoneStore.IsMarkedInSweepZone(item.pickupable))
						continue;
					if (__state == null)
						__state = new List<Pickupable>();
					__state.Add(item.pickupable);
				}
			}

			public static void Postfix(List<Pickupable> __state)
			{
				if (__state == null)
					return;
				foreach (Pickupable pickupable in __state)
					if (pickupable != null)
						pickupable.KPrefabID.AddTag(ZoneStore.PlayerCancelled, serialize: true);
			}
		}

		// The item's own "Cancel Sweep" button.
		[HarmonyPatch(typeof(Clearable), "OnClickCancel")]
		public static class Clearable_OnClickCancel_Patch
		{
			public static void Prefix(Pickupable ___pickupable, out bool __state)
			{
				__state = ZoneStore.IsMarkedInSweepZone(___pickupable);
			}

			public static void Postfix(Pickupable ___pickupable, bool __state)
			{
				if (__state)
					___pickupable.KPrefabID.AddTag(ZoneStore.PlayerCancelled, serialize: true);
			}
		}

		// Move To sends Cancel to the item it moves, which cancels its sweep; that Cancel
		// must not count as the player cancelling the sweep.
		[HarmonyPatch(typeof(Movable), "MarkForMove")]
		public static class Movable_MarkForMove_Patch
		{
			internal static int Running;

			public static void Prefix()
			{
				Running++;
			}

			public static void Finalizer()
			{
				Running--;
			}
		}

		// Debris merged into the pile: the whole pile is swept again. Vanilla already
		// marks the pile when the merged debris was marked.
		[HarmonyPatch(typeof(Clearable), "OnAbsorb")]
		public static class Clearable_OnAbsorb_Patch
		{
			public static void Postfix(Pickupable ___pickupable)
			{
				___pickupable.KPrefabID.RemoveTag(ZoneStore.PlayerCancelled);
			}
		}

		// Marked again (Sweep tool, the item's button, or merged with marked debris).
		[HarmonyPatch(typeof(Clearable), nameof(Clearable.MarkForClear))]
		public static class Clearable_MarkForClear_Patch
		{
			public static void Postfix(Pickupable ___pickupable, bool ___isMarkedForClear)
			{
				if (___isMarkedForClear)
					___pickupable.KPrefabID.RemoveTag(ZoneStore.PlayerCancelled);
			}
		}

		// Picked up by a Duplicant: wherever it is dropped, the zones treat it as new.
		[HarmonyPatch(typeof(Clearable), "OnStore")]
		public static class Clearable_OnStore_Patch
		{
			public static void Postfix(Pickupable ___pickupable)
			{
				___pickupable.KPrefabID.RemoveTag(ZoneStore.PlayerCancelled);
			}
		}
	}
}
