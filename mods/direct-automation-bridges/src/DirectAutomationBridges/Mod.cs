using HarmonyLib;
using KMod;
using UnityEngine;

namespace DirectAutomationBridges
{
	public sealed class DirectAutomationBridgesMod : UserMod2
	{
		public const string LogPrefix = "[DirectAutomationBridges] ";

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log(LogPrefix + "Loaded version " + typeof(DirectAutomationBridgesMod).Assembly.GetName().Version);
		}
	}
}
