using HarmonyLib;
using Isochronous;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace JumperBridges
{
	public sealed class JumperBridgesMod : UserMod2
	{
		public const string LogPrefix = "[JumperBridges] ";

		public override void OnLoad(Harmony harmony)
		{
			// Before base.OnLoad, which applies the patches: a kind that is off has its patches skipped (Prepare).
			Options options = Options.Load();
			BridgeKind.Logic.Enabled = options.AutomationBridges;
			BridgeKind.Power.Enabled = options.PowerBridges;
			base.OnLoad(harmony);
			PUtil.InitLibrary(false);
			Translations.Register(harmony, typeof(ModStrings));
			new POptions().RegisterOptions(this, typeof(Options));
			Debug.Log(LogPrefix + "Loaded version " + typeof(JumperBridgesMod).Assembly.GetName().Version
				+ "; automation bridges " + (options.AutomationBridges ? "on" : "off") + ", power bridges " + (options.PowerBridges ? "on" : "off"));
		}
	}
}
