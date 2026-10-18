using HarmonyLib;
using KMod;
using UnityEngine;

namespace OvagroOverhead
{
	public sealed class OvagroOverheadMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[OvagroOverhead] Loaded version " + typeof(OvagroOverheadMod).Assembly.GetName().Version);
		}
	}
}
