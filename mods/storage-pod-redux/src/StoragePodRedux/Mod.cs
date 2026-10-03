using System.Collections.Generic;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace StoragePodRedux
{
	public sealed class StoragePodReduxMod : UserMod2
	{
		/// <summary>staticID of Skyrunner's original Storage Pod, whose prefab IDs we reuse.</summary>
		public const string OriginalStaticID = "Storage Pod";

		/// <summary>True when the original mod is enabled alongside this one; ours then stays out of the way.</summary>
		public static bool OriginalActive { get; private set; }

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary(false);
			new POptions().RegisterOptions(this, typeof(Options));
			ModStrings.Register();
			Debug.Log("[StoragePodRedux] Loaded version " + typeof(StoragePodReduxMod).Assembly.GetName().Version);
		}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
		{
			base.OnAllModsLoaded(harmony, mods);
			OriginalActive = ModDetection.IsEnabled(mods, OriginalStaticID);
			if (OriginalActive)
				Debug.LogWarning("[StoragePodRedux] The original Storage Pod mod is enabled; its buildings share our IDs, so Storage Pod Redux is not adding any. Disable one of the two.");
			Buildings.Decide(mods);
			Debug.Log("[StoragePodRedux] Storage Pod " + (Buildings.StoragePodEnabled ? "enabled" : "disabled")
				+ "; Cool Pod " + (Buildings.CoolPodEnabled ? "enabled" : "disabled") + " (" + Buildings.CoolPodReason + ")");
		}
	}
}
