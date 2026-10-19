using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bitshifter.Tests
{
	/// <summary>
	/// Screenshots of the Ribbon Diagnostic Panel for the look review: the switch grid with the
	/// output wired and unwired, the lit face, the hover card in and out of the automation overlay,
	/// and the build menu icon. Pictures, not oracles; run with -Filter "*DiagnosticPanelLook*".
	///
	/// The scene, one row: a source panel whose switches put bits 1, 3 and 9-12 on a ribbon, the
	/// panel under review fed by that ribbon (input top left), and a Ribbon Reader on its output.
	/// </summary>
	public static class DiagnosticPanelLook
	{
		private const int SourceBits = (1 << 0) | (1 << 2) | (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11);
		private const int PanelSwitches = (1 << 2) | (1 << 16);
		/// <summary>Bit 31 is switched on through the grid, as a player would.</summary>
		private const int ClickedBit = 30;

		[OniTest("Bitshifter: diagnostic panel screenshots for the look review", Timeout = 120, Explicit = true), Layer(2)]
		public static IEnumerator Screenshots(TestContext t)
		{
			// Cells: 0-1 the source panel, 2 the ribbon's climb, 3-4 the panel, 5 ribbon, 6-7 the reader.
			CellRect r = t.Rect(8, 2);
			GameObject source = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(0, 0));
			yield return t.WaitGameSeconds(1);
			GameObject panelGo = t.World.Build(RibbonDiagnosticPanelConfig.ID, r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			GameObject reader = t.World.Build("LogicRibbonReader", r.Cell(6, 0));
			yield return t.WaitGameSeconds(1);
			RibbonDiagnosticPanel panel = panelGo.GetComponent<RibbonDiagnosticPanel>();

			source.GetComponent<RibbonDiagnosticPanel>().SetSwitches(SourceBits);
			yield return t.WaitGameSeconds(1);
			panel.SetSwitches(PanelSwitches);
			yield return t.WaitGameSeconds(1);

			// The source's output (its bottom left) up to the panel's input (its top left).
			UtilityDragSession ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
			yield return ribbon.Path(r.Cell(0, 0), r.Cell(1, 0), r.Cell(2, 0), r.Cell(2, 1), r.Cell(3, 1));
			ribbon.Dispose();
			yield return t.WaitGameSeconds(1);
			// The panel's output (its bottom left) to the reader's input.
			ribbon = t.Tools.UtilityTool("LogicRibbon", SimHashes.Copper);
			yield return ribbon.Path(r.Cell(3, 0), r.Cell(4, 0), r.Cell(5, 0), r.Cell(6, 0));
			ribbon.Dispose();
			yield return t.WaitGameSeconds(1);

			t.Assert.Eq(SourceBits, panel.InputValue, "the panel's input carries the source's bits");
			t.Assert.True(panel.OutputWired, "the panel's output is wired");

			yield return t.LookAt(r.Cell(4, 1), 6f);
			yield return Select(t, panelGo);
			RibbonDiagnosticPanelSideScreen screen = ShownSideScreen();
			t.Assert.NotNull(screen, "the panel's side screen is showing");
			GameObject cell = FindChild(screen.gameObject, "Bit" + (ClickedBit + 1));
			t.Assert.NotNull(cell, "the grid has a cell for bit " + (ClickedBit + 1));
			Click(cell);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(panel.IsSwitchOn(ClickedBit), "clicking cell " + (ClickedBit + 1) + " switches bit " + (ClickedBit + 1) + " on");
			t.Assert.Eq(SourceBits | PanelSwitches | (1 << ClickedBit), panel.OutputValue, "the output is input OR switches");
			LogState(t, panel);
			yield return t.Screenshot("1-grid-wired");

			// The face close up, neither selected nor under the pointer: the game brightens a
			// selected or hovered building, which washes the lamps out to white.
			SelectTool.Instance.Select(null);
			yield return t.WaitGameSeconds(1);
			yield return t.LookAt(r.Cell(4, 1), 3f);
			yield return t.WaitGameSeconds(1);
			yield return t.Screenshot("3-face-lit");

			// The hover card, with nothing selected, over the panel.
			yield return t.LookAt(r.Cell(4, 1), 6f);
			t.PointAt(r.Cell(4, 1));
			yield return t.WaitGameSeconds(1);
			yield return t.Screenshot("4a-hover");
			OverlayScreen.Instance.ToggleOverlay(OverlayModes.Logic.ID);
			t.Cleanup(() => OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID));
			yield return t.WaitGameSeconds(1);
			t.PointAt(r.Cell(4, 1));
			yield return t.WaitGameSeconds(1);
			yield return t.Screenshot("4b-hover-automation-overlay");
			OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID);
			yield return t.WaitGameSeconds(1);

			// Unwire the output, one ribbon piece at a time.
			for (int x = 6; x >= 3; x--)
			{
				t.World.Destroy(t.World.At(r.Cell(x, 0), ObjectLayer.LogicWire));
				yield return t.WaitGameSeconds(1);
			}
			t.Assert.False(panel.OutputWired, "the panel's output is unwired");
			yield return Select(t, panelGo);
			LogState(t, panel);
			yield return t.Screenshot("2-grid-unwired");
			SelectTool.Instance.Select(null);
			yield return t.WaitGameSeconds(1);

			// The build menu: Automation, where the panel follows the Ribbon Writer.
			PlanScreen.Instance.OpenCategoryByName("Automation");
			t.Cleanup(() => PlanScreen.Instance.CloseCategoryPanel(false));
			yield return t.WaitGameSeconds(1);
			yield return t.Screenshot("5-build-menu");
		}

		private static IEnumerator Select(TestContext t, GameObject building)
		{
			SelectTool.Instance.Select(building.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
		}

		private static RibbonDiagnosticPanelSideScreen ShownSideScreen()
		{
			foreach (DetailsScreen.SideScreenRef sr in Traverse.Create(DetailsScreen.Instance).Field("sideScreens").GetValue<List<DetailsScreen.SideScreenRef>>())
				if (sr.screenInstance is RibbonDiagnosticPanelSideScreen s && s.gameObject.activeInHierarchy)
					return s;
			return null;
		}

		private static GameObject FindChild(GameObject root, string name)
		{
			foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
				if (child.name == name)
					return child.gameObject;
			return null;
		}

		private static void Click(GameObject go)
		{
			var data = new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = PointerEventData.InputButton.Left };
			ExecuteEvents.Execute(go, data, ExecuteEvents.pointerDownHandler);
			ExecuteEvents.Execute(go, data, ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(go, data, ExecuteEvents.pointerClickHandler);
		}

		private static void LogState(TestContext t, RibbonDiagnosticPanel panel)
		{
			t.Log("input " + RibbonDiagnosticPanel.DescribeBits(panel.InputValue) + ", switches " + RibbonDiagnosticPanel.DescribeBits(panel.Switches) +
				", output wired " + panel.OutputWired + ", output value " + panel.OutputValue);
			var lamps = new List<string>();
			for (int bit = 0; bit < BitshifterMod.Bits; bit++)
				if (panel.Lamp(bit) != RibbonDiagnosticPanel.LampState.Off)
					lamps.Add((bit + 1) + "=" + panel.Lamp(bit));
			t.Log("lamps lit: " + string.Join(" ", lamps.ToArray()));
			var items = new List<string>();
			foreach (StatusItemGroup.Entry entry in panel.GetComponent<KSelectable>().GetStatusItemGroup())
				items.Add(entry.GetName());
			t.Log("status items: " + string.Join(" | ", items.ToArray()));
		}
	}
}
