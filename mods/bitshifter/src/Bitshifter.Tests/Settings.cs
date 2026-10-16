using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>What the selected bit does besides carry a signal: copy settings, saving, and the building's look.</summary>
	public static class Settings
	{
		[OniTest("Bitshifter: copy settings carries a high bit between two writers and between two readers", Timeout = 40), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(8, 1);
			GameObject w1 = t.World.Build("LogicRibbonWriter", r.Cell(0, 0));
			GameObject w2 = t.World.Build("LogicRibbonWriter", r.Cell(2, 0));
			GameObject r1 = t.World.Build("LogicRibbonReader", r.Cell(4, 0));
			GameObject r2 = t.World.Build("LogicRibbonReader", r.Cell(6, 0));
			yield return t.WaitGameSeconds(1);
			w1.GetComponent<LogicRibbonWriter>().SetBitSelection(26);
			r1.GetComponent<LogicRibbonReader>().SetBitSelection(11);
			yield return t.WaitGameSeconds(0.5);
			w2.Trigger((int)GameHashes.CopySettings, w1);
			r2.Trigger((int)GameHashes.CopySettings, r1);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(26, w2.GetComponent<LogicRibbonWriter>().GetBitSelection(), "the second writer's bit");
			t.Assert.Eq(11, r2.GetComponent<LogicRibbonReader>().GetBitSelection(), "the second reader's bit");
		}

		[OniTest("Bitshifter: the selected bits are saved and a loaded pair still passes a signal", Timeout = 90), Layer(1)]
		public static IEnumerator SavedAndLoaded(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			Rig rig = null;
			yield return Rig.Build(t, r, 20, 20, x => rig = x);
			rig.Set(true);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(rig.AlarmInput > 0, "the signal passes before the save");

			Tag writerTag = rig.Writer.PrefabID();
			Tag readerTag = rig.Reader.PrefabID();
			byte[] writerFile = SaveFile.Capture(rig.Writer);
			byte[] readerFile = SaveFile.Capture(rig.Reader);
			t.World.Destroy(rig.Writer);
			t.World.Destroy(rig.Reader);
			yield return t.WaitGameSeconds(2);
			GameObject writer = SaveFile.Restore(writerTag, writerFile);
			GameObject reader = SaveFile.Restore(readerTag, readerFile);
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(20, writer.GetComponent<LogicRibbonWriter>().GetBitSelection(), "the writer's bit is kept");
			t.Assert.Eq(20, reader.GetComponent<LogicRibbonReader>().GetBitSelection(), "the reader's bit is kept");
			t.Assert.Eq(31, writer.GetComponent<LogicRibbonWriter>().GetBitDepth(), "the loaded writer offers 31 bits");
			t.Assert.True(rig.AlarmInput > 0, "the signal passes through the loaded pair");
			rig.Set(false);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, rig.AlarmInput, "and stops with the switch");
		}

		[OniTest("Bitshifter: above bit 4 both buildings show the plain idle pose, and the vanilla bits keep their own", Timeout = 40), Layer(1)]
		public static IEnumerator Look(TestContext t)
		{
			CellRect r = t.Rect(4, 1);
			GameObject writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0));
			GameObject reader = t.World.Build("LogicRibbonReader", r.Cell(2, 0));
			var writerPart = writer.GetComponent<LogicRibbonWriter>();
			var readerPart = reader.GetComponent<LogicRibbonReader>();
			yield return t.WaitGameSeconds(1);
			var writerAnim = writer.GetComponent<KBatchedAnimController>();
			var readerAnim = reader.GetComponent<KBatchedAnimController>();

			writerPart.SetBitSelection(1);
			readerPart.SetBitSelection(1);
			yield return t.WaitGameSeconds(1);
			t.Assert.NotEq("idle", writerAnim.CurrentAnim.name, "the writer on a vanilla bit highlights it");
			t.Assert.NotEq("idle", readerAnim.CurrentAnim.name, "the reader on a vanilla bit highlights it");
			string vanillaWriter = writerAnim.CurrentAnim.name;

			writerPart.SetBitSelection(4);
			readerPart.SetBitSelection(30);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq("idle", writerAnim.CurrentAnim.name, "the writer on bit 5 is idle");
			t.Assert.Eq("idle", readerAnim.CurrentAnim.name, "the reader on bit 31 is idle");

			writerPart.SetBitSelection(3);
			yield return t.WaitGameSeconds(1);
			t.Assert.NotEq("idle", writerAnim.CurrentAnim.name, "bit 4, the last animated one, highlights again (" + vanillaWriter + " earlier)");
			yield return t.Screenshot("buildings");
		}
	}
}
