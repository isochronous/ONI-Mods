using System;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace OvagroOverhead
{
	public sealed class OvagroOverheadMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			// All or nothing: a game update that removes one target would otherwise leave planting half
			// routed, such as a hanging preview over a plot that still delivers the floor plant.
			try
			{
				base.OnLoad(harmony);
			}
			catch (Exception e)
			{
				harmony.UnpatchAll(harmony.Id);
				Debug.LogWarning("[OvagroOverhead] Could not patch the game, Ovagro stays floor-only: " + e.GetBaseException().Message);
			}
			Debug.Log("[OvagroOverhead] Loaded version " + typeof(OvagroOverheadMod).Assembly.GetName().Version);
		}
	}
}
