using HarmonyLib;
using KMod;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using UnityEngine;

namespace UpgradePlanner
{
	public sealed class UpgradePlannerMod : UserMod2
	{
		/// <summary>
		/// Rebindable hotkey (options -> game -> controls -> Mods) that activates the
		/// Upgrade Planner tool. PAction.MaxAction is the runtime "no action" value, used
		/// until OnLoad registers the real action.
		/// </summary>
		public static Action ToolAction { get; private set; } = PAction.MaxAction;

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary(false);
			PAction action = new PActionManager().CreateAction(
				"Isochronous.UpgradePlanner.Tool",
				ModStrings.ToolName + " tool",
				new PKeyBinding(KKeyCode.U, Modifier.Shift));
			ToolAction = action.GetKAction();
			Debug.Log("[UpgradePlanner] Loaded version " + typeof(UpgradePlannerMod).Assembly.GetName().Version);
		}
	}
}
