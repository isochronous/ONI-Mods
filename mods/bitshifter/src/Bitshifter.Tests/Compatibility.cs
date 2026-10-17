using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>
	/// The mod stands down when Digital CPU is enabled. Needs that Workshop mod, so it runs only
	/// when asked: Invoke-OniTests.ps1 -Mods Bitshifter -Steam 3244925649 -Filter "*Digital*"
	/// </summary>
	public static class Compatibility
	{
		[OniTest("Bitshifter: stands down, patching nothing, when Digital CPU is enabled", Timeout = 30, Explicit = true), Layer(0)]
		public static IEnumerator StandsDownForDigitalCpu(TestContext t)
		{
			bool cpu = Global.Instance.modManager.mods.Any(m => m.label.id == "3244925649" && m.IsEnabledForActiveDlc());
			if (!cpu)
				t.Skip("Digital CPU is not enabled; run with -Steam 3244925649");
			foreach (string method in new[] { "GetBitDepth", "UpdateVisuals" })
				foreach (System.Type type in new[] { typeof(LogicRibbonReader), typeof(LogicRibbonWriter) })
				{
					Patches patches = Harmony.GetPatchInfo(AccessTools.Method(type, method));
					bool ours = patches != null && patches.Owners.Any(o => o.ToLowerInvariant().Contains("bitshifter"));
					t.Assert.False(ours, type.Name + "." + method + " is left alone");
				}
			CellRect r = t.Rect(4, 1);
			ILogicRibbonBitSelector writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0)).GetComponent<LogicRibbonWriter>();
			t.Log("the writer offers " + writer.GetBitDepth() + " bits with Digital CPU");
			yield return t.WaitGameSeconds(0.5);
		}

		/// <summary>
		/// Better Automation Overlay's label on a Ribbon Reader, read in the frame the reader is
		/// built, before the label has spawned: what Blueprints Expanded does, which crashed the game
		/// with a NullReferenceException in the overlay's UpdateUI. The label reads blank until it
		/// spawns, then shows the bit. Needs that Workshop mod:
		/// Invoke-OniTests.ps1 -Mods Bitshifter -Steam 1878896484 -Filter "*Overlay*"
		/// </summary>
		[OniTest("Bitshifter: Better Automation Overlay's ribbon label reads blank, not throwing, before it spawns", Timeout = 30, Explicit = true), Layer(0)]
		public static IEnumerator OverlayLabelBeforeSpawn(TestContext t)
		{
			Type labelType = AccessTools.TypeByName("BetterLogicOverlay.LogicSettingDisplay.LogicRibbonBitSelectorSetting");
			if (labelType == null)
				t.Skip("Better Automation Overlay is not enabled; run with -Steam 1878896484");
			OverlayScreen.Instance.ToggleOverlay(OverlayModes.Logic.ID);
			t.Cleanup(() => OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID));
			yield return t.Frames(2);

			CellRect r = t.Rect(3, 1);
			GameObject reader = t.World.Build("LogicRibbonReader", r.Cell(1, 0));
			Component label = reader.GetComponent(labelType);
			t.Assert.NotNull(label, "the reader carries the overlay's label");
			t.Assert.False(((KMonoBehaviour)label).isSpawned, "the label has not spawned in the frame the reader was built");
			Func<string> read = () => (string)AccessTools.Method(labelType, "GetSetting").Invoke(label, null);
			t.Assert.Eq("", read(), "the unspawned label reads blank");

			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(((KMonoBehaviour)label).isSpawned, "the label has spawned");
			t.Assert.Eq("1", read(), "the spawned label shows the reader's bit");
		}
	}
}
