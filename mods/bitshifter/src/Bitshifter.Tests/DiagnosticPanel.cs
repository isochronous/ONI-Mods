using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;
using static Bitshifter.RibbonDiagnosticPanel;
using PANEL = Bitshifter.ModStrings.BITSHIFTER.DIAGNOSTICPANEL;

namespace Bitshifter.Tests
{
	/// <summary>
	/// The Ribbon Diagnostic Panel: lamps, switches, output, saving, copying, the build menu and its
	/// side screen. The lamps are read back from the building's animation (visibility and tint of
	/// bit_bloom_1 to bit_bloom_31), the status items from the selectable, the side screen from the
	/// widgets the details screen shows. Every step waits game time, as a player's would.
	/// </summary>
	public static class DiagnosticPanel
	{
		/// <summary>Bits 1, 3 and 9-12, 0-based.</summary>
		private const int SourceBits = (1 << 0) | (1 << 2) | (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11);
		/// <summary>Bits 1, 3, 9-12 and 31.</summary>
		private const int FullBits = SourceBits | (1 << 30);

		private static Color TintOf(LampState state)
		{
			switch (state)
			{
				case LampState.Switch:
					return LampPurple;
				case LampState.Input:
					return LampGreen;
				default:
					return LampRed;
			}
		}

		/// <summary>Every lamp against the expected state: visible or hidden on the face, and its tint, and the component's own answer.</summary>
		private static void AssertLamps(TestContext t, GameObject panel, Func<int, LampState> expected, string what)
		{
			var kbac = panel.GetComponent<KBatchedAnimController>();
			var part = panel.GetComponent<RibbonDiagnosticPanel>();
			var wrong = new List<string>();
			for (int bit = 0; bit < BitshifterMod.Bits; bit++)
			{
				LampState want = expected(bit);
				var symbol = new KAnimHashedString("bit_bloom_" + (bit + 1));
				bool visible = Anims.SymbolVisible(kbac, symbol);
				if (part.Lamp(bit) != want)
					wrong.Add((bit + 1) + " is " + part.Lamp(bit) + ", wanted " + want);
				else if (visible != (want != LampState.Off))
					wrong.Add((bit + 1) + " " + (visible ? "shows" : "is hidden") + ", wanted " + want);
				else if (want != LampState.Off && !Near(Anims.SymbolTint(kbac, symbol), TintOf(want)))
					wrong.Add((bit + 1) + " is tinted " + Anims.SymbolTint(kbac, symbol) + ", wanted " + want + " " + TintOf(want));
			}
			if (wrong.Count > 0)
				t.Assert.Fail(what + ": lamps " + string.Join("; ", wrong.ToArray()));
		}

		private static bool Near(Color a, Color b)
		{
			return Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
		}

		private static Func<int, LampState> All(LampState state)
		{
			return bit => state;
		}

		/// <summary>Input bits green, switches purple (they win), everything else the given state.</summary>
		private static Func<int, LampState> Lamps(int input, int switches, bool switchesCount, LampState rest)
		{
			return bit => switchesCount && (switches & (1 << bit)) != 0 ? LampState.Switch : (input & (1 << bit)) != 0 ? LampState.Input : rest;
		}

		private static bool HasStatus(TestContext t, GameObject panel, string text)
		{
			return t.World.StatusItems(panel).Any(name => name == text);
		}

		private static bool HasStatusStartingWith(TestContext t, GameObject panel, string start)
		{
			return t.World.StatusItems(panel).Any(name => name.StartsWith(start));
		}

		private static string Items(TestContext t, GameObject panel)
		{
			return string.Join(" | ", t.World.StatusItems(panel).ToArray());
		}

		[OniTest("Bitshifter: panel lamps follow a Ribbon Writer's bit: green when on, red for every other bit", Timeout = 90), Layer(1)]
		public static IEnumerator LampsFollowInput(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Writer, true, 4, x => rig = x, 4);
			t.Assert.True(rig.PanelPart.InputWired, "the input is wired");
			t.Assert.True(rig.PanelPart.OutputWired, "the output is wired");
			AssertLamps(t, rig.Panel, All(LampState.Inactive), "switch off");

			rig.Set(true);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1 << 4, rig.PanelPart.InputValue, "the input carries bit 5");
			AssertLamps(t, rig.Panel, Lamps(1 << 4, 0, true, LampState.Inactive), "switch on");

			rig.Set(false);
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, rig.Panel, All(LampState.Inactive), "switch off again");
		}

		[OniTest("Bitshifter: a panel monitors a full 31-bit value and words it in its status item", Timeout = 90), Layer(1)]
		public static IEnumerator FullValue(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, true, 0, x => rig = x);
			rig.SourcePart.SetSwitches(FullBits);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(FullBits, rig.PanelPart.InputValue, "the monitor's input carries the source's bits");
			AssertLamps(t, rig.Panel, Lamps(FullBits, 0, true, LampState.Inactive), "the monitor");
			t.Assert.True(HasStatus(t, rig.Panel, "Input: 1, 3, 9-12, 31"), "the input status item words the bits (" + Items(t, rig.Panel) + ")");
			t.Assert.True(HasStatus(t, rig.Panel, "Switches: none"), "the switches status item shows with the output wired (" + Items(t, rig.Panel) + ")");
		}

		[OniTest("Bitshifter: switches reach the output: a Reader on bit 17 sees the switch, lamp 17 turns purple", Timeout = 90), Layer(1)]
		public static IEnumerator SwitchesReachOutput(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.None, true, 16, x => rig = x);
			t.Assert.Eq(0, rig.AlarmInput, "the alarm is quiet");
			AssertLamps(t, rig.Panel, All(LampState.Inactive), "an output-only panel with every switch off shows every lamp red");

			rig.PanelPart.SetSwitch(16, true);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1 << 16, rig.PanelPart.OutputValue, "the output carries the switch");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees it");
			AssertLamps(t, rig.Panel, Lamps(0, 1 << 16, true, LampState.Inactive), "switch 17 on");

			rig.PanelPart.SetSwitch(16, false);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, rig.AlarmInput, "the alarm goes quiet again");
			AssertLamps(t, rig.Panel, All(LampState.Inactive), "switch 17 off");
		}

		[OniTest("Bitshifter: a switch and the input on one bit light it purple, not green", Timeout = 90), Layer(1)]
		public static IEnumerator PurpleWins(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, true, 2, x => rig = x);
			rig.SourcePart.SetSwitch(2, true);
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, rig.Panel, Lamps(1 << 2, 0, true, LampState.Inactive), "input bit 3 alone is green");

			rig.PanelPart.SetSwitch(2, true);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(LampState.Switch, rig.PanelPart.Lamp(2), "input and switch: purple wins");
			AssertLamps(t, rig.Panel, Lamps(1 << 2, 1 << 2, true, LampState.Inactive), "input and switch");
			t.Assert.True(rig.AlarmInput > 0, "the output carries the bit");

			rig.PanelPart.SetSwitch(2, false);
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, rig.Panel, Lamps(1 << 2, 0, true, LampState.Inactive), "switch off: back to green");
			t.Assert.True(rig.AlarmInput > 0, "the input still reaches the output");
		}

		[OniTest("Bitshifter: input wired with value 0 shows every lamp red, output unwired", Timeout = 90), Layer(1)]
		public static IEnumerator InputOnlyRed(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, false, 0, x => rig = x);
			t.Assert.Eq(0, rig.PanelPart.InputValue, "the source carries nothing");
			t.Assert.True(rig.PanelPart.InputWired, "the input is wired");
			t.Assert.False(rig.PanelPart.OutputWired, "the output is not");
			AssertLamps(t, rig.Panel, All(LampState.Inactive), "an input-only panel on a quiet ribbon");

			rig.SourcePart.SetSwitches(SourceBits);
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, rig.Panel, Lamps(SourceBits, 0, false, LampState.Inactive), "an input-only panel on a ribbon with bits");
		}

		[OniTest("Bitshifter: a panel with nothing wired shows every lamp hidden, switches or not", Timeout = 30), Layer(0)]
		public static IEnumerator NeitherWired(TestContext t)
		{
			CellRect r = t.Rect(2, 2);
			GameObject panel = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(0, 0));
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, panel, All(LampState.Off), "a new panel");
			panel.GetComponent<RibbonDiagnosticPanel>().SetSwitches(FullBits);
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, panel, All(LampState.Off), "switches on, nothing wired");
		}

		[OniTest("Bitshifter: an unwired output ignores the switches on the face and keeps them; wiring it lights them at once", Timeout = 120), Layer(1)]
		public static IEnumerator OutputUnwired(TestContext t)
		{
			const int switches = (1 << 2) | (1 << 16);
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, false, 16, x => rig = x);
			rig.SourcePart.SetSwitches(SourceBits);
			yield return t.WaitGameSeconds(1);
			rig.PanelPart.SetSwitches(switches);
			yield return t.WaitGameSeconds(1);
			t.Assert.False(rig.PanelPart.OutputWired, "the output is unwired");
			AssertLamps(t, rig.Panel, Lamps(SourceBits, switches, false, LampState.Inactive), "unwired: the input only");
			t.Assert.False(HasStatusStartingWith(t, rig.Panel, "Switches"), "no switches status item (" + Items(t, rig.Panel) + ")");
			t.Assert.True(HasStatus(t, rig.Panel, "Input: 1, 3, 9-12"), "the input status item stays (" + Items(t, rig.Panel) + ")");
			t.Assert.Eq(SourceBits | switches, rig.PanelPart.OutputValue, "the output already carries input OR switches");

			yield return rig.WireOutput(t);
			t.Assert.True(rig.PanelPart.OutputWired, "the output is wired");
			t.Assert.Eq(SourceBits | switches, rig.PanelPart.OutputValue, "the output value did not change when the wire arrived");
			AssertLamps(t, rig.Panel, Lamps(SourceBits, switches, true, LampState.Inactive), "wired: the switches light purple");
			t.Assert.True(HasStatusStartingWith(t, rig.Panel, "Switches: 3, 17"), "the switches status item shows (" + Items(t, rig.Panel) + ")");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 17 sees the switch");

			yield return rig.CutOutput(t);
			t.Assert.False(rig.PanelPart.OutputWired, "the output is unwired again");
			t.Assert.Eq(switches, rig.PanelPart.Switches, "the switches are kept");
			AssertLamps(t, rig.Panel, Lamps(SourceBits, switches, false, LampState.Inactive), "unwired again: the input only");
			t.Assert.False(HasStatusStartingWith(t, rig.Panel, "Switches"), "the switches status item is gone (" + Items(t, rig.Panel) + ")");
		}

		[OniTest("Bitshifter: a saved and loaded panel keeps its switches and output, and draws its lamps again", Timeout = 120), Layer(1)]
		public static IEnumerator SavedAndLoaded(TestContext t)
		{
			const int switches = (1 << 1) | (1 << 29);
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, true, 1, x => rig = x);
			rig.SourcePart.SetSwitches(SourceBits);
			yield return t.WaitGameSeconds(1);
			rig.PanelPart.SetSwitches(switches);
			yield return t.WaitGameSeconds(1);
			AssertLamps(t, rig.Panel, Lamps(SourceBits, switches, true, LampState.Inactive), "before the save");

			Tag tag = rig.Panel.PrefabID();
			byte[] file = SaveFile.Capture(rig.Panel);
			t.World.Destroy(rig.Panel);
			yield return t.WaitGameSeconds(2);
			GameObject back = SaveFile.Restore(tag, file);
			yield return t.WaitGameSeconds(2);
			var part = back.GetComponent<RibbonDiagnosticPanel>();
			t.Assert.Eq(switches, part.Switches, "the switches are kept");
			t.Assert.Eq(SourceBits | switches, part.OutputValue, "the output is kept");
			t.Assert.True(part.OutputWired, "the loaded panel is wired");
			AssertLamps(t, back, Lamps(SourceBits, switches, true, LampState.Inactive), "after the load");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 2 sees the loaded switch");
		}

		[OniTest("Bitshifter: copy settings gives another panel the switches", Timeout = 40), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect r = t.Rect(5, 2);
			GameObject a = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(0, 0));
			GameObject b = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			a.GetComponent<RibbonDiagnosticPanel>().SetSwitches(FullBits);
			yield return t.WaitGameSeconds(1);
			b.Trigger((int)GameHashes.CopySettings, a);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(FullBits, b.GetComponent<RibbonDiagnosticPanel>().Switches, "the copy has the switches");
			t.Assert.Eq(FullBits, a.GetComponent<RibbonDiagnosticPanel>().Switches, "the source keeps them");
		}

		[OniTest("Bitshifter: the panel is in Automation > wires after the Ribbon Writer and unlocked by Parallel Automation", Timeout = 20), Layer(0)]
		public static IEnumerator BuildMenuAndResearch(TestContext t)
		{
			var list = TUNING.BUILDINGS.PLANORDER.First(p => p.category == "Automation").buildingAndSubcategoryData;
			int panel = list.FindIndex(pair => pair.Key == RibbonDiagnosticPanelConfig.ID);
			int writer = list.FindIndex(pair => pair.Key == "LogicRibbonWriter");
			t.Assert.True(panel >= 0, "the panel is in the Automation build menu");
			t.Assert.Eq(writer + 1, panel, "right after the Ribbon Writer");
			t.Assert.Eq("wires", list[panel].Value, "in the wires subcategory");
			Tech tech = Db.Get().Techs.Get("ParallelAutomation");
			t.Assert.True(tech.unlockedItemIDs.Contains(RibbonDiagnosticPanelConfig.ID), "unlocked by Parallel Automation");
			t.Assert.Eq(1, tech.unlockedItemIDs.Count(id => id == RibbonDiagnosticPanelConfig.ID), "listed once");
			yield break;
		}

		[OniTest("Bitshifter: the panel's side screen is a grid of 31 cells in rows of 8, 8, 8 and 7, and clicking a cell switches the bit", Timeout = 90), Layer(2)]
		public static IEnumerator SwitchGrid(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, true, 4, x => rig = x);
			rig.SourcePart.SetSwitches(SourceBits);
			yield return t.WaitGameSeconds(1);
			yield return t.LookAt(PanelCell(t, rig), 6f);
			yield return t.Screens.Select(rig.Panel);
			RibbonDiagnosticPanelSideScreen screen = t.Screens.SideScreen<RibbonDiagnosticPanelSideScreen>();
			t.Assert.NotNull(screen, "the panel's side screen is showing");
			int[] sizes = { 8, 8, 8, 7 };
			for (int row = 0; row < 4; row++)
			{
				GameObject line = t.Screens.Find(screen.gameObject, "Row" + row);
				t.Assert.NotNull(line, "row " + (row + 1) + " exists");
				int cells = 0;
				foreach (Transform child in line.transform)
					if (child.name.StartsWith("Bit"))
						cells++;
				t.Assert.Eq(sizes[row], cells, "cells in row " + (row + 1));
			}
			for (int bit = 1; bit <= BitshifterMod.Bits; bit++)
				t.Assert.NotNull(t.Screens.Find(screen.gameObject, "Bit" + bit), "a cell for bit " + bit);

			GameObject cell = t.Screens.Find(screen.gameObject, "Bit5");
			var image = cell.GetComponent<KImage>();
			t.Assert.False(rig.PanelPart.IsSwitchOn(4), "bit 5 starts off");
			t.Screens.Click(cell);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(rig.PanelPart.IsSwitchOn(4), "clicking cell 5 switches bit 5 on");
			t.Assert.True(Near(image.colorStyleSetting.inactiveColor, Purple), "and the cell turns purple");
			yield return t.WaitGameSeconds(1);
			t.Assert.True(rig.AlarmInput > 0, "the output carries it to the Reader on bit 5");
			yield return t.Screenshot("grid");

			t.Screens.Click(cell);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(rig.PanelPart.IsSwitchOn(4), "clicking again switches it off");
			t.Assert.False(Near(image.colorStyleSetting.inactiveColor, Purple), "and the cell is no longer purple");
			GameObject input = t.Screens.Find(screen.gameObject, "Bit1");
			t.Assert.True(Near(input.GetComponent<KImage>().colorStyleSetting.inactiveColor, RibbonDiagnosticPanel.Green), "a cell for a bit the input carries is green");
		}

		/// <summary>The cell the camera looks at: the middle of the panel.</summary>
		private static int PanelCell(TestContext t, PanelRig rig)
		{
			return global::Grid.PosToCell(rig.Panel);
		}

		[OniTest("Bitshifter: with the output unwired the grid is disabled and says why, and a click changes nothing", Timeout = 90), Layer(2)]
		public static IEnumerator GridDisabled(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.Panel, false, 4, x => rig = x);
			rig.SourcePart.SetSwitches(SourceBits);
			yield return t.WaitGameSeconds(1);
			rig.PanelPart.SetSwitches(1 << 16);
			yield return t.WaitGameSeconds(1);
			yield return t.LookAt(PanelCell(t, rig), 6f);
			yield return t.Screens.Select(rig.Panel);
			RibbonDiagnosticPanelSideScreen screen = t.Screens.SideScreen<RibbonDiagnosticPanelSideScreen>();
			t.Assert.NotNull(screen, "the panel's side screen is showing");

			for (int bit = 1; bit <= BitshifterMod.Bits; bit++)
				t.Assert.False(t.Screens.Find(screen.gameObject, "Bit" + bit).GetComponent<KButton>().isInteractable, "cell " + bit + " is disabled");
			t.Assert.False(t.Screens.Find(screen.gameObject, "AllOff").GetComponent<KButton>().isInteractable, "All off is disabled");
			GameObject hint = t.Screens.Find(t.Screens.Find(screen.gameObject, "Hint"), "Text");
			t.Assert.Eq((string)PANEL.SIDE_SCREEN_UNWIRED, hint.GetComponent<LocText>().text, "the line above the grid says the output is unwired");

			t.Screens.Click(t.Screens.Find(screen.gameObject, "Bit5"));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.False(rig.PanelPart.IsSwitchOn(4), "a click on a disabled cell changes nothing");
			t.Assert.Eq(1 << 16, rig.PanelPart.Switches, "the saved switches are kept");
			yield return t.Screenshot("grid-unwired");

			yield return rig.WireOutput(t);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(t.Screens.Find(screen.gameObject, "Bit5").GetComponent<KButton>().isInteractable, "wiring the output enables the cells");
			t.Assert.Eq((string)PANEL.SIDE_SCREEN_WIRED, hint.GetComponent<LocText>().text, "and the line above the grid changes");
		}

		[OniTest("Bitshifter: All off clears every switch, then disables itself", Timeout = 90), Layer(2)]
		public static IEnumerator AllOff(TestContext t)
		{
			PanelRig rig = null;
			yield return PanelRig.Build(t, t.Rect(PanelRig.Width, PanelRig.Height), Feed.None, true, 3, x => rig = x);
			rig.PanelPart.SetSwitches((1 << 3) | (1 << 20));
			yield return t.WaitGameSeconds(1);
			yield return t.Screens.Select(rig.Panel);
			RibbonDiagnosticPanelSideScreen screen = t.Screens.SideScreen<RibbonDiagnosticPanelSideScreen>();
			t.Assert.NotNull(screen, "the panel's side screen is showing");
			GameObject allOff = t.Screens.Find(screen.gameObject, "AllOff");
			t.Assert.True(allOff.GetComponent<KButton>().isInteractable, "All off is enabled while switches are on");
			t.Assert.True(rig.AlarmInput > 0, "the Reader on bit 4 sees its switch");

			t.Screens.Click(allOff);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(0, rig.PanelPart.Switches, "every switch is off");
			t.Assert.False(allOff.GetComponent<KButton>().isInteractable, "All off disables itself");
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, rig.AlarmInput, "the alarm goes quiet");
		}
	}
}
