using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>
	/// A switch, a Ribbon Writer, a Ribbon Reader and an alarm in one row, wired the way a player
	/// would: a wire from the switch to the writer, a ribbon from the writer to the reader, a wire
	/// from the reader to the alarm.
	/// </summary>
	internal sealed class Rig
	{
		public GameObject Switch, Writer, Reader, Alarm;
		public LogicSwitch Toggle;
		public LogicRibbonWriter WriterPart;
		public LogicRibbonReader ReaderPart;

		/// <summary>The row's cells: 0 switch, 1 and 2 the writer, 3 and 4 the reader, 5 the alarm.</summary>
		public static IEnumerator Build(TestContext t, CellRect r, int writerBit, int readerBit, System.Action<Rig> done)
		{
			var rig = new Rig();
			rig.Switch = t.World.Build("LogicSwitch", r.Cell(0, 0));
			rig.Toggle = rig.Switch.GetComponent<LogicSwitch>();
			if (rig.Toggle.IsSwitchedOn)
				rig.Toggle.HandleToggle();
			rig.Writer = t.World.Build("LogicRibbonWriter", r.Cell(1, 0));
			rig.Reader = t.World.Build("LogicRibbonReader", r.Cell(3, 0));
			rig.Alarm = t.World.Build("LogicAlarm", r.Cell(5, 0));
			rig.WriterPart = rig.Writer.GetComponent<LogicRibbonWriter>();
			rig.ReaderPart = rig.Reader.GetComponent<LogicRibbonReader>();
			yield return t.WaitGameSeconds(1);
			rig.WriterPart.SetBitSelection(writerBit);
			rig.ReaderPart.SetBitSelection(readerBit);
			yield return t.WaitGameSeconds(0.5);

			UtilityDragSession wire = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return wire.Path(r.Cell(0, 0), r.Cell(1, 0));
			wire.Dispose();
			yield return t.WaitGameSeconds(1);
			UtilityDragSession ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
			yield return ribbon.Path(r.Cell(2, 0), r.Cell(3, 0));
			ribbon.Dispose();
			yield return t.WaitGameSeconds(1);
			wire = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return wire.Path(r.Cell(4, 0), r.Cell(5, 0));
			wire.Dispose();
			yield return t.WaitGameSeconds(1);
			done(rig);
		}

		/// <summary>The value the alarm's input port reads.</summary>
		public int AlarmInput => Alarm.GetComponent<LogicPorts>().GetInputValue(LogicAlarm.INPUT_PORT_ID);

		public void Set(bool on)
		{
			if (Toggle.IsSwitchedOn != on)
				Toggle.HandleToggle();
		}
	}
}
