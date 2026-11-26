using System.Collections.Generic;
using HarmonyLib;
using KMod;

namespace FridgeThresholdsExtended
{
	public sealed class FridgeThresholdsExtendedMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[FridgeThresholdsExtended] Loaded version " + typeof(FridgeThresholdsExtendedMod).Assembly.GetName().Version);
		}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
		{
			base.OnAllModsLoaded(harmony, mods);
			ThresholdsCompat.Apply(harmony);
		}
	}
}
