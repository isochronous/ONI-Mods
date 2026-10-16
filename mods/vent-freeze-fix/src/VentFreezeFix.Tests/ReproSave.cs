using System.Collections;
using System.IO;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace VentFreezeFix.Tests
{
	/// <summary>
	/// The developer option "Add button to reproduce bug conditions": once armed, the next time a
	/// liquid vent emits a packet the game is saved next to the current save. The button itself is
	/// made on a game's start from the option on disk; this arms it the way a click does.
	/// </summary>
	public static class ReproSave
	{
		private static readonly System.Type DebugSave = typeof(VentFreezeWatcher).Assembly.GetType("VentFreezeFix.DebugSave");

		[OniTest("VFF: an armed repro button saves the game on the first vent emit, once", Timeout = 60), Layer(0)]
		public static IEnumerator SavesOnEmit(TestContext t)
		{
			t.Speed(3);
			string active = SaveLoader.GetActiveSaveFilePath();
			t.Assert.False(string.IsNullOrEmpty(active), "the harness loaded the fixture from a path");
			string expected = Path.Combine(Path.GetDirectoryName(active), "VentFreezeRepro.sav");
			if (File.Exists(expected))
				File.Delete(expected);
			var debug = Traverse.Create(DebugSave);
			bool buttonWas = debug.Field("buttonEnabled").GetValue<bool>();
			t.Cleanup(() =>
			{
				debug.Field("buttonEnabled").SetValue(buttonWas);
				debug.Field("armed").SetValue(false);
				if (File.Exists(expected))
					File.Delete(expected);
			});

			CellRect r = t.Rect(5, 6);
			t.World.Build("Tile", r.Cell(2, 0));
			int cell = r.Cell(2, 4);
			t.World.Build("LiquidConduit", cell);
			GameObject vent = Vents.Build(t, cell);
			yield return t.WaitGameSeconds(1);

			// Unarmed, a vent emitting does not save.
			Conduit.GetFlowManager(ConduitType.Liquid).AddElement(cell, SimHashes.Water, 2f, 293.15f, byte.MaxValue, 0);
			yield return t.WaitGameSeconds(2);
			t.Assert.False(File.Exists(expected), "nothing is saved while the button is not armed");

			debug.Field("buttonEnabled").SetValue(true);
			debug.Field("armed").SetValue(true);
			Conduit.GetFlowManager(ConduitType.Liquid).SetContents(cell, ConduitFlow.ConduitContents.Empty);
			Conduit.GetFlowManager(ConduitType.Liquid).AddElement(cell, SimHashes.Water, 3f, 293.15f, byte.MaxValue, 0);
			yield return t.WaitUntil(() => File.Exists(expected), 15, "VentFreezeRepro.sav next to the current save");
			yield return t.WaitGameSeconds(1);
			t.Assert.False(debug.Field("armed").GetValue<bool>(), "the button disarmed after saving");
			long length = new FileInfo(expected).Length;
			t.Assert.True(length > 10000, "a whole save was written (" + length + " bytes)");
			File.Delete(expected);
			Conduit.GetFlowManager(ConduitType.Liquid).AddElement(cell, SimHashes.Water, 3f, 293.15f, byte.MaxValue, 0);
			yield return t.WaitGameSeconds(2);
			t.Assert.False(File.Exists(expected), "and it does not save again until re-armed");
		}
	}
}
