using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Other mods that lay out every material picker in the game, set to leave the planner's alone.
	/// Each is patched only when it is loaded, and found by name, so the planner needs none of them.
	/// </summary>
	internal static class ModCompat
	{
		/// <summary>
		/// Adaptive Building Menu sizes every picker for the build menu's side bar (five toggles a
		/// row, two rows) on every refresh, after the game's own; in the planner's window that fought
		/// the planner's size each 200 ms. Its two layout methods return early for the planner's pickers.
		/// </summary>
		private static readonly string[][] SkipFor =
		{
			new[] { "AdaptiveBuildingMenu.MaterialSelectorLayoutApplier", "Apply" },
			new[] { "AdaptiveBuildingMenu.MaterialSelectionPanelLayoutApplier", "Apply" },
		};

		public static void Apply(Harmony harmony)
		{
			var prefix = new HarmonyMethod(AccessTools.Method(typeof(ModCompat), nameof(LeavePlannerPickers)));
			foreach (string[] target in SkipFor)
			{
				System.Type type = AccessTools.TypeByName(target[0]);
				if (type == null)
					continue;
				MethodInfo method = AccessTools.Method(type, target[1]);
				if (method == null)
				{
					Debug.LogWarning("[UpgradePlanner] " + target[0] + " has no " + target[1] + "; its layout may fight the planner's pickers.");
					continue;
				}
				harmony.Patch(method, prefix: prefix);
				Debug.Log("[UpgradePlanner] " + target[0] + "." + target[1] + " leaves the planner's material pickers alone.");
			}
		}

		/// <summary>Skips the other mod's layout when its first argument (a picker or a picker panel) is the planner's.</summary>
		private static bool LeavePlannerPickers(object[] __args)
		{
			return !(__args.Length > 0 && __args[0] is Component component && PlannerPickerHost.Holds(component));
		}
	}
}
