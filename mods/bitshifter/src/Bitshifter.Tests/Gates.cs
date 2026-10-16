using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>
	/// A NOT gate on a ribbon: the base game inverts the low four bits only; the mod inverts every
	/// bit the reader and writer offer.
	/// </summary>
	public static class Gates
	{
		/// <summary>Writers put the switch on bits 1, 3, 5 and 7 (indexes 0, 2, 4, 6).</summary>
		private static readonly int[] WriterBits = { 0, 2, 4, 6 };
		private const int Written = 0b1010101;

		[OniTest("Bitshifter: a NOT gate on a ribbon inverts every bit", Timeout = 90), Layer(1)]
		public static IEnumerator NotInvertsEveryBit(TestContext t)
		{
			CellRect r = t.Rect(7, 4);
			LogicSwitch bits = t.World.Build("LogicSwitch", r.Cell(0, 0)).GetComponent<LogicSwitch>();
			var writers = new LogicRibbonWriter[WriterBits.Length];
			for (int i = 0; i < WriterBits.Length; i++)
				writers[i] = t.World.Build("LogicRibbonWriter", r.Cell(1, i)).GetComponent<LogicRibbonWriter>();
			t.World.Build("LogicGateNOT", r.Cell(4, 0));
			if (bits.IsSwitchedOn)
				bits.HandleToggle();
			yield return t.WaitGameSeconds(1);
			for (int i = 0; i < WriterBits.Length; i++)
				writers[i].SetBitSelection(WriterBits[i]);
			yield return t.WaitGameSeconds(0.5);

			UtilityDragSession wire = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return wire.Path(r.Cell(0, 0), r.Cell(1, 0), r.Cell(1, 1), r.Cell(1, 2), r.Cell(1, 3));
			wire.Dispose();
			yield return t.WaitGameSeconds(1);
			UtilityDragSession ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
			yield return ribbon.Path(r.Cell(2, 3), r.Cell(2, 2), r.Cell(2, 1), r.Cell(2, 0), r.Cell(3, 0), r.Cell(4, 0));
			ribbon.Dispose();
			yield return t.WaitGameSeconds(1);
			ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
			yield return ribbon.Path(r.Cell(5, 0), r.Cell(6, 0));
			ribbon.Dispose();
			yield return t.WaitGameSeconds(1);

			int all = (int)((1L << BitshifterMod.Bits) - 1);
			t.Log("switch off: in " + Binary(Value(r.Cell(4, 0))) + ", out " + Binary(Value(r.Cell(6, 0))));
			t.Assert.Eq(0, Value(r.Cell(4, 0)), "nothing on the ribbon with the switch off");
			t.Assert.Eq(all, Value(r.Cell(6, 0)), "every bit on out of the gate with the switch off");
			bits.HandleToggle();
			yield return t.WaitGameSeconds(1);
			t.Log("switch on: in " + Binary(Value(r.Cell(4, 0))) + ", out " + Binary(Value(r.Cell(6, 0))));
			t.Assert.Eq(Written, Value(r.Cell(4, 0)), "the writers set bits 1, 3, 5 and 7");
			t.Assert.Eq(all & ~Written, Value(r.Cell(6, 0)), "every bit but 1, 3, 5 and 7 out of the gate");
		}

		private static int Value(int cell)
		{
			return Game.Instance.logicCircuitSystem.GetNetworkForCell(cell) is LogicCircuitNetwork n ? n.OutputValue : -1;
		}

		private static string Binary(int value)
		{
			return System.Convert.ToString(value, 2);
		}
	}
}
