using HarmonyLib;
using KMod;
using UnityEngine;

namespace RefinedBuilding
{
	public sealed class RefinedBuildingMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[RefinedBuilding] Loaded version " + typeof(RefinedBuildingMod).Assembly.GetName().Version);
		}
	}
}
