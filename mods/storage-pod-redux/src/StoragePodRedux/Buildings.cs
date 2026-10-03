using System.Collections.Generic;
using KMod;

namespace StoragePodRedux
{
	/// <summary>
	/// Which of the two buildings this game gets. Decided once all mods are loaded (the Cool
	/// Pod's Auto mode looks at another mod) and read by the registration patch.
	/// </summary>
	internal static class Buildings
	{
		public static bool StoragePodEnabled { get; private set; }
		public static bool CoolPodEnabled { get; private set; }
		public static string CoolPodReason { get; private set; } = "not decided yet";

		public static void Decide(IReadOnlyList<Mod> mods)
		{
			Options options = Options.Instance;
			StoragePodEnabled = options.StoragePodEnabled;
			switch (options.CoolPod)
			{
			case CoolPodMode.Enabled:
				CoolPodEnabled = true;
				CoolPodReason = "option: Enabled";
				break;
			case CoolPodMode.Disabled:
				CoolPodEnabled = false;
				CoolPodReason = "option: Disabled";
				break;
			default:
				bool fridgePod = RonivansLegacy.FridgePodActive(mods);
				CoolPodEnabled = !fridgePod;
				CoolPodReason = fridgePod ? "option: Auto, Ronivan's Legacy Fridge Pod is active" : "option: Auto";
				break;
			}
		}

		/// <summary>False only for one of our two buildings when it is turned off.</summary>
		public static bool IsEnabled(IBuildingConfig config)
		{
			if (config is StoragePodConfig)
				return IsEnabled(StoragePodConfig.ID);
			if (config is CoolPodConfig)
				return IsEnabled(CoolPodConfig.ID);
			return true;
		}

		public static bool IsEnabled(string buildingId)
		{
			if (StoragePodReduxMod.OriginalActive)
				return false;
			if (buildingId == StoragePodConfig.ID)
				return StoragePodEnabled;
			if (buildingId == CoolPodConfig.ID)
				return CoolPodEnabled;
			return true;
		}
	}
}
