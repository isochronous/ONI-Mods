using System;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using UnityEngine;

namespace SmoothBorders
{
	public sealed class SmoothBordersMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			// All or nothing: a game update that removes one target would otherwise leave the border
			// half patched, so every patch comes off and the game draws its own border.
			try
			{
				base.OnLoad(harmony);
			}
			catch (Exception e)
			{
				harmony.UnpatchAll(harmony.Id);
				Debug.LogWarning("[SmoothBorders] Could not patch the game, borders stay as the game draws them: " + e.GetBaseException().Message);
			}
			PUtil.InitLibrary(false);
			// Registered once the types exist:
			// Translations.Register(harmony, typeof(ModStrings));
			// new POptions().RegisterOptions(this, typeof(Options));
			Debug.Log("[SmoothBorders] Loaded version " + typeof(SmoothBordersMod).Assembly.GetName().Version);
		}
	}
}
