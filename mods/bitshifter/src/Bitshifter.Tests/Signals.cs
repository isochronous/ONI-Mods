using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>
	/// Signals through a real Writer, ribbon and Reader. The code path differs in three ways: the
	/// four bits the game animates, the first bit above them, and the highest (1 &lt;&lt; 30, the last
	/// bit before the sign bit). One representative of each.
	/// </summary>
	public static class Signals
	{
		/// <summary>Index of the first bit the buildings have no animation for, and of the last bit the mod offers.</summary>
		private const int FirstModdedBit = 4;
		private const int LastBit = 30;

		[OniTest("Bitshifter: both buildings offer 31 bits", Timeout = 20), Layer(0)]
		public static IEnumerator Depth(TestContext t)
		{
			CellRect r = t.Rect(4, 1);
			ILogicRibbonBitSelector writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0)).GetComponent<LogicRibbonWriter>();
			ILogicRibbonBitSelector reader = t.World.Build("LogicRibbonReader", r.Cell(2, 0)).GetComponent<LogicRibbonReader>();
			t.Assert.Eq(BitshifterMod.Bits, writer.GetBitDepth(), "the writer's bit depth");
			t.Assert.Eq(31, reader.GetBitDepth(), "the reader's bit depth");
			yield return t.WaitGameSeconds(0.5);
		}

		[OniTest("Bitshifter: a bit above the vanilla four carries a signal from the writer to the reader", Timeout = 60), Layer(1)]
		public static IEnumerator FirstModdedBitCarries(TestContext t)
		{
			yield return Carry(t, FirstModdedBit);
		}

		[OniTest("Bitshifter: the highest bit carries a signal from the writer to the reader", Timeout = 60), Layer(1)]
		public static IEnumerator HighestBitCarries(TestContext t)
		{
			yield return Carry(t, LastBit);
		}

		[OniTest("Bitshifter: a vanilla bit still carries a signal", Timeout = 60), Layer(1)]
		public static IEnumerator VanillaBitCarries(TestContext t)
		{
			yield return Carry(t, 1);
		}

		private static IEnumerator Carry(TestContext t, int bit)
		{
			Rig rig = null;
			yield return Rig.Build(t, t.Rect(6, 1), bit, bit, r => rig = r);
			t.Assert.Eq(0, rig.AlarmInput, "off with the switch off");
			rig.Set(true);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1 << bit, rig.WriterPart.GetOutputValue(), "the writer puts its input on bit " + bit);
			t.Assert.True(rig.AlarmInput > 0, "the alarm is on with the switch on, bit " + bit);
			rig.Set(false);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, rig.AlarmInput, "off again with the switch off, bit " + bit);
		}

		[OniTest("Bitshifter: a reader on another bit than the writer's does not see the signal", Timeout = 60), Layer(1)]
		public static IEnumerator ReaderOnAnotherBit(TestContext t)
		{
			Rig rig = null;
			yield return Rig.Build(t, t.Rect(6, 1), LastBit, FirstModdedBit + 3, r => rig = r);
			rig.Set(true);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1 << LastBit, rig.WriterPart.GetOutputValue(), "the ribbon carries bit " + LastBit);
			t.Assert.Eq(0, rig.AlarmInput, "a reader on bit " + (FirstModdedBit + 3) + " reads a clear bit");
			rig.ReaderPart.SetBitSelection(LastBit);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(rig.AlarmInput > 0, "the reader picks the signal up once it is on the writer's bit");
		}
	}
}
