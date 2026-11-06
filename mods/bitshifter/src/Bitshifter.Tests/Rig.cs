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

	/// <summary>Where a <see cref="PanelRig"/>'s panel gets its input ribbon.</summary>
	internal enum Feed
	{
		/// <summary>Nothing is wired to the input.</summary>
		None,
		/// <summary>A second panel whose switches put any 31-bit value on a ribbon.</summary>
		Panel,
		/// <summary>A switch and a Ribbon Writer: one bit, from the vanilla buildings.</summary>
		Writer,
	}

	/// <summary>
	/// A Ribbon Diagnostic Panel in a row with its feed and its load, wired the way a player would.
	/// Cells: the feed at 0 (a source panel at 0-1, or a switch at 0 and a Writer at 1-2), a ribbon
	/// climbing from its output to the panel's input at the top left of the panel at 3-4, the panel's
	/// output at its bottom left, a Ribbon Reader at 6-7 and an alarm at 8. The row is two cells tall.
	/// The output ribbon (3 to 6) is drawn by <see cref="WireOutput"/> and removed by <see cref="CutOutput"/>.
	/// </summary>
	internal sealed class PanelRig
	{
		public const int Width = 9;
		public const int Height = 2;

		public GameObject Panel, Reader, Alarm, Source, Switch, Writer;
		public RibbonDiagnosticPanel PanelPart, SourcePart;
		public LogicSwitch Toggle;
		public LogicRibbonReader ReaderPart;
		public LogicRibbonWriter WriterPart;
		private CellRect r;

		/// <param name="readerBit">The Ribbon Reader's bit, 0-based.</param>
		/// <param name="writerBit">With <see cref="Feed.Writer"/>: the Writer's bit, 0-based.</param>
		public static IEnumerator Build(TestContext t, CellRect r, Feed feed, bool wireOutput, int readerBit, System.Action<PanelRig> done, int writerBit = 0)
		{
			var rig = new PanelRig { r = r };
			if (feed == Feed.Panel)
				rig.Source = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(0, 0));
			if (feed == Feed.Writer)
			{
				rig.Switch = t.World.Build("LogicSwitch", r.Cell(0, 0));
				rig.Toggle = rig.Switch.GetComponent<LogicSwitch>();
				if (rig.Toggle.IsSwitchedOn)
					rig.Toggle.HandleToggle();
				rig.Writer = t.World.Build("LogicRibbonWriter", r.Cell(1, 0));
			}
			rig.Panel = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(3, 0));
			rig.Reader = t.World.Build("LogicRibbonReader", r.Cell(6, 0));
			rig.Alarm = t.World.Build("LogicAlarm", r.Cell(8, 0));
			rig.PanelPart = rig.Panel.GetComponent<RibbonDiagnosticPanel>();
			rig.ReaderPart = rig.Reader.GetComponent<LogicRibbonReader>();
			if (rig.Source != null)
				rig.SourcePart = rig.Source.GetComponent<RibbonDiagnosticPanel>();
			if (rig.Writer != null)
				rig.WriterPart = rig.Writer.GetComponent<LogicRibbonWriter>();
			yield return t.WaitGameSeconds(1);
			rig.ReaderPart.SetBitSelection(readerBit);
			if (rig.WriterPart != null)
				rig.WriterPart.SetBitSelection(writerBit);
			yield return t.WaitGameSeconds(0.5);

			UtilityDragSession wire;
			if (feed == Feed.Writer)
			{
				wire = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
				yield return wire.Path(r.Cell(0, 0), r.Cell(1, 0));
				wire.Dispose();
				yield return t.WaitGameSeconds(1);
			}
			if (feed != Feed.None)
			{
				// From the feed's output, up to the panel's input.
				UtilityDragSession ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
				if (feed == Feed.Panel)
					yield return ribbon.Path(r.Cell(0, 0), r.Cell(1, 0), r.Cell(2, 0), r.Cell(2, 1), r.Cell(3, 1));
				else
					yield return ribbon.Path(r.Cell(2, 0), r.Cell(2, 1), r.Cell(3, 1));
				ribbon.Dispose();
				yield return t.WaitGameSeconds(1);
			}
			wire = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return wire.Path(r.Cell(7, 0), r.Cell(8, 0));
			wire.Dispose();
			yield return t.WaitGameSeconds(1);
			if (wireOutput)
				yield return rig.WireOutput(t);
			done(rig);
		}

		/// <summary>The ribbon from the panel's output to the Reader.</summary>
		public IEnumerator WireOutput(TestContext t)
		{
			UtilityDragSession ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
			yield return ribbon.Path(r.Cell(3, 0), r.Cell(4, 0), r.Cell(5, 0), r.Cell(6, 0));
			ribbon.Dispose();
			yield return t.WaitGameSeconds(1);
		}

		/// <summary>Takes the output ribbon away one piece at a time, as a player deconstructs it.</summary>
		public IEnumerator CutOutput(TestContext t)
		{
			for (int x = 6; x >= 3; x--)
			{
				t.World.Destroy(t.World.At(r.Cell(x, 0), ObjectLayer.LogicWire));
				yield return t.WaitGameSeconds(1);
			}
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
