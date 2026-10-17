using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Bitshifter
{
	/// <summary>
	/// Better Automation Overlay (Workshop 1878896484) shows each building's setting as a label in
	/// the automation overlay, read every frame through LogicLabelSetting.GetSetting. Its labels
	/// find their building's component through fields their base class fills in OnSpawn. Blueprints
	/// Expanded builds and configures a building in one frame, so the overlay picks the building up
	/// before the label has spawned, and the Ribbon Reader's or Writer's label throws a
	/// NullReferenceException from the overlay's UpdateUI every frame until the game crashes. Until
	/// a label has spawned it shows blank text; from the next frame it reads as usual. Applied after
	/// all mods have loaded, whether or not the rest of this mod runs, and only when that mod is
	/// enabled.
	/// </summary>
	internal static class OverlayLabelGuard
	{
		private const string LabelType = "BetterLogicOverlay.LogicSettingDisplay.LogicLabelSetting";

		public static void Apply(Harmony harmony)
		{
			Type label = AccessTools.TypeByName(LabelType);
			if (label == null)
				return;
			HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(OverlayLabelGuard), nameof(Prefix)));
			int count = 0;
			foreach (MethodInfo method in GetSettingMethods(label))
			{
				harmony.Patch(method, prefix: prefix);
				count++;
			}
			Debug.Log("[Bitshifter] Better Automation Overlay: " + count + " setting labels show blank until spawned");
		}

		/// <summary>Every GetSetting declared on the label type or a type derived from it, in the label's own assembly.</summary>
		private static IEnumerable<MethodInfo> GetSettingMethods(Type label)
		{
			foreach (Type type in AccessTools.GetTypesFromAssembly(label.Assembly))
			{
				if (type.IsAbstract || !label.IsAssignableFrom(type))
					continue;
				MethodInfo method = type.GetMethod("GetSetting", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
				if (method != null)
					yield return method;
			}
		}

		private static bool Prefix(KMonoBehaviour __instance, ref string __result)
		{
			if (__instance.isSpawned)
				return true;
			__result = "";
			return false;
		}
	}
}
