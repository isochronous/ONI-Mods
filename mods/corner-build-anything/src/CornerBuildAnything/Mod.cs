using HarmonyLib;
using KMod;
using UnityEngine;

namespace CornerBuildAnything
{
	public sealed class CornerBuildAnythingMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[CornerBuildAnything] Loaded version " + typeof(CornerBuildAnythingMod).Assembly.GetName().Version);
		}
	}
}
