using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.UI;
using PANEL = Bitshifter.ModStrings.BITSHIFTER.DIAGNOSTICPANEL;

namespace Bitshifter
{
	/// <summary>
	/// The Ribbon Diagnostic Panel's switches: a PLib grid of 31 buttons laid out as the lamps on
	/// the panel's face (four rows of eight, a gap between the nibbles, the 32nd place empty) and
	/// coloured as they are: purple for a bit switched on, green for a bit the input carries, the
	/// face's dark socket otherwise. While the output is unwired the switches do nothing, so the
	/// buttons are disabled and show the input only, and the line above them says why.
	/// </summary>
	public sealed class RibbonDiagnosticPanelSideScreen : SideScreenContent, IRender200ms
	{
		/// <summary>Horizontal inset; the root itself carries no margin so it never exceeds the vanilla 280px width.</summary>
		private const int Inset = 8;
		private const int Columns = 8;
		private const int Spacing = 2;
		/// <summary>The spacer between the nibbles; with the spacing on both sides of it the gap is 16px.</summary>
		private const float NibbleSpacer = 12f;
		private static readonly Vector2 CellSize = new Vector2(28f, 24f);

		/// <summary>The panel face's lamp socket.</summary>
		private static readonly Color Socket = new Color32(46, 49, 56, 255);
		/// <summary>The socket faded into the side screen, for a switch that cannot be used.</summary>
		private static readonly Color SocketFaded = new Color32(46, 49, 56, 150);

		private static ColorStyleSetting offStyle;
		private static ColorStyleSetting inputStyle;
		private static ColorStyleSetting switchStyle;

		private RibbonDiagnosticPanel target;
		private bool built;
		private LocText hint;
		private readonly GameObject[] cells = new GameObject[BitshifterMod.Bits];
		private GameObject allOff;

		private bool shown;
		private int shownInput;
		private int shownSwitches;
		private bool shownWired;

		public override bool IsValidForTarget(GameObject go)
		{
			return go != null && go.GetComponent<RibbonDiagnosticPanel>() != null;
		}

		public override string GetTitle()
		{
			return PANEL.SIDE_SCREEN_TITLE;
		}

		public override void SetTarget(GameObject go)
		{
			base.SetTarget(go);
			target = go != null ? go.GetComponent<RibbonDiagnosticPanel>() : null;
			if (target == null)
				return;
			EnsureBuilt();
			Refresh(true);
		}

		public override void ClearTarget()
		{
			base.ClearTarget();
			target = null;
		}

		public void Render200ms(float dt)
		{
			if (target != null && built && gameObject.activeInHierarchy)
				Refresh(false);
		}

		private void Toggle(int bit)
		{
			if (target == null || !target.OutputWired)
				return;
			target.ToggleSwitch(bit);
			Refresh(false);
		}

		private void SwitchAllOff()
		{
			if (target == null || !target.OutputWired)
				return;
			target.SetSwitches(0);
			Refresh(false);
		}

		private void Refresh(bool force)
		{
			if (target == null || !built)
				return;
			int input = target.InputValue;
			int switches = target.Switches;
			bool wired = target.OutputWired;
			if (!force && shown && input == shownInput && switches == shownSwitches && wired == shownWired)
				return;
			shown = true;
			shownInput = input;
			shownSwitches = switches;
			shownWired = wired;

			if (hint != null)
				hint.text = wired ? PANEL.SIDE_SCREEN_WIRED : PANEL.SIDE_SCREEN_UNWIRED;
			for (int bit = 0; bit < cells.Length; bit++)
			{
				GameObject cell = cells[bit];
				if (cell == null)
					continue;
				bool inputOn = (input & (1 << bit)) != 0;
				bool switchOn = (switches & (1 << bit)) != 0;
				ColorStyleSetting style = wired && switchOn ? switchStyle : inputOn ? inputStyle : offStyle;
				KImage image = cell.GetComponent<KImage>();
				if (image != null)
				{
					image.colorStyleSetting = style;
					image.ApplyColorStyleSetting();
				}
				PButton.SetButtonEnabled(cell, wired);
				string tip = string.Format(wired ? switchOn ? PANEL.CELL_ON : PANEL.CELL_OFF : PANEL.CELL_UNWIRED, bit + 1);
				if (inputOn)
					tip += "\n" + string.Format(PANEL.CELL_INPUT, bit + 1);
				PUIElements.SetToolTip(cell, tip);
			}
			if (allOff != null)
				PButton.SetButtonEnabled(allOff, wired && switches != 0);
		}

		private void EnsureBuilt()
		{
			if (built)
				return;
			built = true;
			MakeStyles();
			PPanel root = new PPanel("RibbonDiagnosticPanelRoot")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperCenter,
				Spacing = 8,
				Margin = new RectOffset(0, 0, 8, 8),
				FlexSize = Vector2.right,
				DynamicSize = true,
			};
			root.AddChild(Hint());
			root.AddChild(Grid());
			root.AddChild(new PPanel("AllOffRow")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleRight,
				Margin = new RectOffset(Inset, Inset, 0, 0),
				FlexSize = Vector2.right,
			}.AddChild(new PButton("AllOff")
			{
				Text = PANEL.ALL_OFF,
				ToolTip = PANEL.ALL_OFF_TOOLTIP,
				Margin = new RectOffset(8, 8, 4, 4),
				Color = PUITuning.Colors.ButtonBlueStyle,
				OnClick = _ => SwitchAllOff(),
			}.AddOnRealize(go => allOff = go)));
			root.AddTo(gameObject);
		}

		/// <summary>
		/// The line above the grid, wrapped to the side screen's width. As in UpgradePlanner's
		/// Widgets.Paragraph: a LocText under a PLib panel that asks for no width of its own.
		/// </summary>
		private PPanel Hint()
		{
			return new PPanel("Hint")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = Vector2.right,
				Margin = new RectOffset(Inset, Inset, 0, 0),
			}.AddOnRealize(go =>
			{
				var holder = new GameObject("Text", typeof(RectTransform));
				holder.SetActive(false);
				holder.transform.SetParent(go.transform, false);
				// As PLib makes its own text: key and style set while the object is inactive.
				LocText label = holder.AddComponent<LocText>();
				label.key = string.Empty;
				TextStyleSetting style = PUITuning.Fonts.TextDarkStyle.DeriveStyle(PUITuning.Fonts.TextDarkStyle.fontSize);
				style.enableWordWrapping = true;
				label.textStyleSetting = style;
				holder.SetActive(true);
				label.text = PANEL.SIDE_SCREEN_WIRED;
				label.alignment = TMPro.TextAlignmentOptions.TopLeft;
				label.textWrappingMode = TMPro.TextWrappingModes.Normal;
				label.raycastTarget = false;
				var width = holder.AddComponent<LayoutElement>();
				width.minWidth = 0f;
				width.preferredWidth = 0f;
				width.flexibleWidth = 1f;
				width.layoutPriority = 10;
				hint = label;
			});
		}

		/// <summary>Four rows of eight cells as on the panel's face; the 32nd place is an empty space.</summary>
		private PPanel Grid()
		{
			var grid = new PPanel("Grid")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = Spacing,
			};
			for (int row = 0; row < 4; row++)
			{
				var line = new PPanel("Row" + row)
				{
					Direction = PanelDirection.Horizontal,
					Alignment = TextAnchor.MiddleLeft,
					Spacing = Spacing,
				};
				for (int column = 0; column < Columns; column++)
				{
					if (column == Columns / 2)
						line.AddChild(new PSpacer { PreferredSize = new Vector2(NibbleSpacer, CellSize.y), FlexSize = Vector2.zero });
					int bit = row * Columns + column;
					if (bit >= BitshifterMod.Bits)
						line.AddChild(new PSpacer { PreferredSize = CellSize, FlexSize = Vector2.zero });
					else
						line.AddChild(Cell(bit));
				}
				grid.AddChild(line);
			}
			return grid;
		}

		private PButton Cell(int bit)
		{
			return new PButton("Bit" + (bit + 1))
			{
				Text = (bit + 1).ToString(),
				TextStyle = PUITuning.Fonts.UILightStyle,
				Margin = new RectOffset(0, 0, 0, 0),
				Color = offStyle,
				OnClick = _ => Toggle(bit),
			}.AddOnRealize(go =>
			{
				var size = go.AddComponent<LayoutElement>();
				size.minWidth = size.preferredWidth = CellSize.x;
				size.minHeight = size.preferredHeight = CellSize.y;
				size.flexibleWidth = size.flexibleHeight = 0f;
				size.layoutPriority = 10;
				cells[bit] = go;
			});
		}

		/// <summary>Palette C, the panel's face: each colour 15% lighter under the pointer.</summary>
		private static void MakeStyles()
		{
			if (offStyle != null)
				return;
			offStyle = Style(Socket, SocketFaded);
			inputStyle = Style(RibbonDiagnosticPanel.Green, RibbonDiagnosticPanel.Green);
			switchStyle = Style(RibbonDiagnosticPanel.Purple, RibbonDiagnosticPanel.Purple);
		}

		private static ColorStyleSetting Style(Color colour, Color disabled)
		{
			Color hover = Color.Lerp(colour, Color.white, 0.15f);
			var style = ScriptableObject.CreateInstance<ColorStyleSetting>();
			style.inactiveColor = colour;
			style.hoverColor = hover;
			style.activeColor = hover;
			style.disabledColor = style.disabledActiveColor = style.disabledhoverColor = disabled;
			return style;
		}
	}
}
