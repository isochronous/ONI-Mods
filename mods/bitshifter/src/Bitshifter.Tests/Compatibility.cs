using System.Collections;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;

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
	}
}
