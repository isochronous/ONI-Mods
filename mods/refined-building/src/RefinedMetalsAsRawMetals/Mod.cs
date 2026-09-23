using HarmonyLib;
using KMod;
using UnityEngine;

namespace RefinedMetalsAsRawMetals
{
	public sealed class RefinedMetalsAsRawMetalsMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[RefinedMetalsAsRawMetals] Loaded version " + typeof(RefinedMetalsAsRawMetalsMod).Assembly.GetName().Version);
		}
	}
}
