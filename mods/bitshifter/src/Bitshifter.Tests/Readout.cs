using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>
	/// The two-digit display on the buildings: the mod's kanims are in use, the segments shown
	/// match the selected bit, the pose follows the wired ports, and a screenshot of both
	/// buildings in both orientations with the signal on and off.
	/// </summary>
	public static class Readout
	{
		[OniTest("Bitshifter: the buildings use the display kanims", Timeout = 20), Layer(0)]
		public static IEnumerator Kanims(TestContext t)
		{
			t.Assert.Eq("bitshifter_ribbon_reader_kanim", Assets.GetBuildingDef("LogicRibbonReader").AnimFiles[0].name, "the reader's kanim");
			t.Assert.Eq("bitshifter_ribbon_writer_kanim", Assets.GetBuildingDef("LogicRibbonWriter").AnimFiles[0].name, "the writer's kanim");
			yield break;
		}

		[OniTest("Bitshifter: the display shows the selected bit", Timeout = 60), Layer(0)]
		public static IEnumerator ShowsSelectedBit(TestContext t)
		{
			CellRect r = t.Rect(4, 1);
			LogicRibbonWriter writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0)).GetComponent<LogicRibbonWriter>();
			LogicRibbonReader reader = t.World.Build("LogicRibbonReader", r.Cell(2, 0)).GetComponent<LogicRibbonReader>();
			yield return t.WaitGameSeconds(1);
			writer.SetBitSelection(22); // Bit 23
			reader.SetBitSelection(6);  // Bit 7
			yield return t.WaitGameSeconds(1);
			KBatchedAnimController w = writer.GetComponent<KBatchedAnimController>();
			KBatchedAnimController rd = reader.GetComponent<KBatchedAnimController>();
			t.Assert.Eq("net0", w.CurrentAnim?.name, "the writer's pose with nothing wired");
			t.Assert.Eq("net0", rd.CurrentAnim?.name, "the reader's pose with nothing wired");
			AssertDigits(t, w, "abdeg", "abcdg", "the writer showing 23");
			AssertDigits(t, rd, "", "abc", "the reader showing 7, no leading zero");
			reader.SetBitSelection(9); // Bit 10
			yield return t.WaitGameSeconds(1);
			AssertDigits(t, rd, "bc", "abcdef", "the reader showing 10");
		}

		private static void AssertDigits(TestContext t, KBatchedAnimController kbac, string tens, string ones, string what)
		{
			for (int position = 0; position < 2; position++)
			{
				string lit = position == 0 ? tens : ones;
				for (char segment = 'a'; segment <= 'g'; segment++)
				{
					string symbol = "seg_" + segment + "_" + (position + 1);
					t.Assert.Eq(lit.IndexOf(segment) >= 0, kbac.GetSymbolVisiblity(symbol), what + ": " + symbol);
				}
			}
		}

		[OniTest("Bitshifter: wired buildings take the both-ports pose, with screenshots", Timeout = 90), Layer(1)]
		public static IEnumerator Wired(TestContext t)
		{
			CellRect r = t.Rect(8, 3);
			Rig rig = null;
			yield return Rig.Build(t, r, 22, 22, x => rig = x);
			t.World.Build("LogicRibbonReader", r.Cell(6, 1), Orientation.R90).GetComponent<LogicRibbonReader>().SetBitSelection(6);
			t.World.Build("LogicRibbonWriter", r.Cell(7, 1), Orientation.R270).GetComponent<LogicRibbonWriter>().SetBitSelection(30);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq("net5", rig.Reader.GetComponent<KBatchedAnimController>().CurrentAnim?.name, "the reader's pose with both ports wired");
			t.Assert.Eq("net5", rig.Writer.GetComponent<KBatchedAnimController>().CurrentAnim?.name, "the writer's pose with both ports wired");
			yield return t.LookAt(r.Cell(4, 1), 5);
			rig.Set(true);
			yield return t.WaitGameSeconds(1);
			yield return t.Screenshot("on");
			rig.Set(false);
			yield return t.WaitGameSeconds(1);
			yield return t.Screenshot("off");
		}
	}
}
