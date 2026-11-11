using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace Bitshifter.Tests
{
	/// <summary>
	/// The Fan-out Gate: what its ribbon carries with the input on, off and unwired, saving, copying,
	/// the build menu, its anims and the vanilla bit selector side screen with several rows picked.
	/// The output is read from the gate's port and from a Ribbon Reader and alarm behind it, the look
	/// from the anim the gate plays, the side screen from its rows. Every step waits game time, as a
	/// player's would.
	/// </summary>
	public static class FanoutGate
	{
		/// <summary>Bits 3 and 17, 0-based.</summary>
		private const int TwoBits = (1 << 2) | (1 << 16);
		/// <summary>Bits 1, 3, 9-12 and 31, 0-based.</summary>
		private const int FullBits = (1 << 0) | (1 << 2) | (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) | (1 << 30);

		private static string AnimOf(GameObject gate)
		{
			return Anims.Current(gate.GetComponent<KBatchedAnimController>());
		}

		/// <summary>The rows the bit selector shows highlighted (MultiToggle state 0), by bit, 0-based.</summary>
		private static List<int> Highlighted(LogicBitSelectorSideScreen screen)
		{
			return screen.toggles_by_int.Where(row => row.Value.CurrentState == 0).Select(row => row.Key).OrderBy(bit => bit).ToList();
		}

		private static string Bits(IEnumerable<int> bits)
		{
			return "[" + string.Join(", ", bits.Select(bit => (bit + 1).ToString()).ToArray()) + "]";
		}

		[OniTest("Bitshifter: Fan-out Gate sends its bits while the input is on and 0 while it is off", Timeout = 90), Layer(1)]
		public static IEnumerator InputOnAndOff(TestContext t)
		{
			GateRig rig = null;
			yield return GateRig.Build(t, t.Rect(GateRig.Width, GateRig.Height), true, 16, x => rig = x);
			rig.GatePart.SetBits(TwoBits);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(0, rig.GatePart.OutputValue, "input off: the ribbon carries 0");
			t.Assert.Eq(0, rig.AlarmInput, "the Reader on bit 17 sees nothing");

			rig.Set(true);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(TwoBits, rig.GatePart.OutputValue, "input on: the ribbon carries bits 3 and 17");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees it");

			rig.Set(false);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(0, rig.GatePart.OutputValue, "input off again: 0");
			t.Assert.Eq(0, rig.AlarmInput, "the alarm goes quiet");
		}

		[OniTest("Bitshifter: Fan-out Gate picks up a change of its bits at once while the input is on", Timeout = 90), Layer(1)]
		public static IEnumerator ChangeWhileOn(TestContext t)
		{
			GateRig rig = null;
			yield return GateRig.Build(t, t.Rect(GateRig.Width, GateRig.Height), true, 16, x => rig = x);
			rig.GatePart.SetBits(1 << 2);
			rig.Set(true);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(1 << 2, rig.GatePart.OutputValue, "bit 3 alone");
			t.Assert.Eq(0, rig.AlarmInput, "the Reader on bit 17 sees nothing");

			rig.GatePart.TogglePicked(16);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(TwoBits, rig.GatePart.OutputValue, "bit 17 added");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees it");

			rig.GatePart.TogglePicked(16);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(1 << 2, rig.GatePart.OutputValue, "bit 17 taken away");
			t.Assert.Eq(0, rig.AlarmInput, "the alarm goes quiet");
		}

		[OniTest("Bitshifter: Fan-out Gate with its input unwired or cut sends 0", Timeout = 120), Layer(1)]
		public static IEnumerator UnwiredInput(TestContext t)
		{
			GateRig rig = null;
			yield return GateRig.Build(t, t.Rect(GateRig.Width, GateRig.Height), false, 16, x => rig = x);
			rig.GatePart.SetBits(TwoBits);
			rig.Set(true);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(rig.GatePart.InputWired, "the input is unwired");
			t.Assert.Eq(0, rig.GatePart.OutputValue, "unwired: the ribbon carries 0");
			t.Assert.Eq(0, rig.AlarmInput, "the alarm is quiet");

			yield return rig.WireInput(t);
			t.Assert.Eq(TwoBits, rig.GatePart.OutputValue, "wired to a switch that is on: the bits");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees it");

			yield return rig.CutInput(t);
			t.Assert.False(rig.GatePart.InputWired, "the input is unwired again");
			t.Assert.Eq(0, rig.GatePart.OutputValue, "cut: 0 again");
			t.Assert.Eq(0, rig.AlarmInput, "the alarm goes quiet");
			t.Assert.Eq(TwoBits, rig.GatePart.Bits, "the bits are kept");
		}

		[OniTest("Bitshifter: Fan-out Gate saved and loaded keeps its bits and its output", Timeout = 120), Layer(1)]
		public static IEnumerator SavedAndLoaded(TestContext t)
		{
			GateRig rig = null;
			yield return GateRig.Build(t, t.Rect(GateRig.Width, GateRig.Height), true, 16, x => rig = x);
			rig.GatePart.SetBits(FullBits | (1 << 16));
			rig.Set(true);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(rig.AlarmInput > 0, "before the save the Reader on bit 17 sees it");

			Tag tag = rig.Gate.PrefabID();
			byte[] file = SaveFile.Capture(rig.Gate);
			t.World.Destroy(rig.Gate);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, rig.AlarmInput, "with the gate gone the alarm is quiet");
			GameObject back = SaveFile.Restore(tag, file);
			yield return t.WaitGameSeconds(1);
			var part = back.GetComponent<Bitshifter.FanoutGate>();
			t.Assert.Eq(FullBits | (1 << 16), part.Bits, "the bits are kept");
			t.Assert.True(part.InputWired, "the loaded gate is wired");
			t.Assert.Eq(FullBits | (1 << 16), part.OutputValue, "the output is sent again");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees the loaded gate");
			t.Assert.Eq(Bitshifter.FanoutGate.AnimInputOn, AnimOf(back), "the loaded gate shows its input on");
		}

		[OniTest("Bitshifter: Fan-out Gate copy settings gives another gate the bits", Timeout = 40), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			GameObject a = t.World.Build(FanoutGateConfig.ID, r.Cell(0, 0));
			GameObject b = t.World.Build(FanoutGateConfig.ID, r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			a.GetComponent<Bitshifter.FanoutGate>().SetBits(FullBits);
			yield return t.WaitGameSeconds(0.5);
			b.Trigger((int)GameHashes.CopySettings, a);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(FullBits, b.GetComponent<Bitshifter.FanoutGate>().Bits, "the copy has the bits");
			t.Assert.Eq(FullBits, a.GetComponent<Bitshifter.FanoutGate>().Bits, "the source keeps them");
		}

		[OniTest("Bitshifter: Fan-out Gate is in Automation > wires after the Ribbon Diagnostic Panel and unlocked by Parallel Automation", Timeout = 20), Layer(0)]
		public static IEnumerator BuildMenuAndResearch(TestContext t)
		{
			var list = TUNING.BUILDINGS.PLANORDER.First(p => p.category == "Automation").buildingAndSubcategoryData;
			int gate = list.FindIndex(pair => pair.Key == FanoutGateConfig.ID);
			int panel = list.FindIndex(pair => pair.Key == RibbonDiagnosticPanelConfig.ID);
			t.Assert.True(gate >= 0, "the gate is in the Automation build menu");
			t.Assert.Eq(panel + 1, gate, "right after the Ribbon Diagnostic Panel");
			t.Assert.Eq("wires", list[gate].Value, "in the wires subcategory");
			Tech tech = Db.Get().Techs.Get("ParallelAutomation");
			t.Assert.True(tech.unlockedItemIDs.Contains(FanoutGateConfig.ID), "unlocked by Parallel Automation");
			t.Assert.Eq(1, tech.unlockedItemIDs.Count(id => id == FanoutGateConfig.ID), "listed once");
			yield break;
		}

		[OniTest("Bitshifter: Fan-out Gate anim is off with nothing wired and follows the input once wired", Timeout = 120), Layer(1)]
		public static IEnumerator AnimFollowsInput(TestContext t)
		{
			// The rig in the bottom row, a lone gate two rows above it.
			CellRect r = t.Rect(GateRig.Width, GateRig.Height + 2);
			GameObject lone = t.World.Build(FanoutGateConfig.ID, r.Cell(0, GateRig.Height + 1));
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(Bitshifter.FanoutGate.AnimOff, AnimOf(lone), "nothing wired");

			GateRig rig = null;
			yield return GateRig.Build(t, r, true, 0, x => rig = x);
			rig.GatePart.SetBits(TwoBits);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(Bitshifter.FanoutGate.AnimInputOff, AnimOf(rig.Gate), "wired, input off");

			rig.Set(true);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(Bitshifter.FanoutGate.AnimInputOn, AnimOf(rig.Gate), "input on");
			yield return t.LookAt(global::Grid.PosToCell(rig.Gate), 6f);
			yield return t.Screenshot("input-on");

			rig.Set(false);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(Bitshifter.FanoutGate.AnimInputOff, AnimOf(rig.Gate), "input off again");
		}

		[OniTest("Bitshifter: Fan-out Gate side screen highlights every picked row, and a click toggles a row", Timeout = 120), Layer(2)]
		public static IEnumerator SideScreenMultiSelect(TestContext t)
		{
			GateRig rig = null;
			yield return GateRig.Build(t, t.Rect(GateRig.Width, GateRig.Height), true, 16, x => rig = x);
			rig.Set(true);
			yield return t.WaitGameSeconds(0.5);
			yield return t.LookAt(global::Grid.PosToCell(rig.Gate), 6f);
			yield return t.Screens.Select(rig.Gate);
			LogicBitSelectorSideScreen screen = t.Screens.SideScreen<LogicBitSelectorSideScreen>();
			t.Assert.NotNull(screen, "the bit selector side screen is showing");
			t.Assert.Eq(0, Highlighted(screen).Count, "no row is highlighted with no bits picked (" + Bits(Highlighted(screen)) + ")");
			t.Assert.False(screen.readerDescriptionContainer.activeSelf, "no Reader description");
			t.Assert.False(screen.writerDescriptionContainer.activeSelf, "no Writer description");

			// Rows 2, 3 and 5 are in view without scrolling, for the screenshot; row 17 is further down.
			foreach (int bit in new[] { 1, 2, 4, 16 })
			{
				t.Screens.Click(screen.toggles_by_int[bit].gameObject);
				yield return t.WaitGameSeconds(0.5);
			}
			t.Assert.Eq((1 << 1) | (1 << 2) | (1 << 4) | (1 << 16), rig.GatePart.Bits, "rows 2, 3, 5 and 17 pick bits 2, 3, 5 and 17");
			t.Assert.Eq("[2, 3, 5, 17]", Bits(Highlighted(screen)), "every picked row is highlighted");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees the picked bit");
			yield return t.Screenshot("several-bits");

			t.Screens.Click(screen.toggles_by_int[4].gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq((1 << 1) | TwoBits, rig.GatePart.Bits, "clicking row 5 again takes bit 5 away");
			t.Assert.Eq("[2, 3, 17]", Bits(Highlighted(screen)), "and its row is plain again");

			rig.GatePart.SetBitSelection(31);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq((1 << 1) | TwoBits, rig.GatePart.Bits, "bit 32 cannot be picked");
		}

		[OniTest("Bitshifter: Fan-out Gate leaves a Ribbon Reader's side screen with its one highlighted row", Timeout = 90), Layer(2)]
		public static IEnumerator ReaderStillSingle(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			GameObject gate = t.World.Build(FanoutGateConfig.ID, r.Cell(0, 0));
			GameObject reader = t.World.Build("LogicRibbonReader", r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			gate.GetComponent<Bitshifter.FanoutGate>().SetBits(TwoBits | (1 << 8));
			reader.GetComponent<LogicRibbonReader>().SetBitSelection(4);
			yield return t.WaitGameSeconds(0.5);

			yield return t.Screens.Select(gate);
			LogicBitSelectorSideScreen screen = t.Screens.SideScreen<LogicBitSelectorSideScreen>();
			t.Assert.NotNull(screen, "the gate's side screen is showing");
			t.Assert.Eq("[3, 9, 17]", Bits(Highlighted(screen)), "the gate's three rows");

			yield return t.Screens.Select(reader);
			screen = t.Screens.SideScreen<LogicBitSelectorSideScreen>();
			t.Assert.NotNull(screen, "the Reader's side screen is showing");
			t.Assert.Eq("[5]", Bits(Highlighted(screen)), "the Reader's one row");
			t.Screens.Click(screen.toggles_by_int[10].gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(10, reader.GetComponent<LogicRibbonReader>().GetBitSelection(), "a click picks the Reader's bit");
			t.Assert.Eq("[11]", Bits(Highlighted(screen)), "and moves its one highlight");
			t.Assert.Eq(TwoBits | (1 << 8), gate.GetComponent<Bitshifter.FanoutGate>().Bits, "the gate keeps its bits");
		}
	}
}
