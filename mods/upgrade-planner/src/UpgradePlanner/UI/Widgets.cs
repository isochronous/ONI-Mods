using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradePlanner
{
	/// <summary>
	/// The window's look, in the colours of the game's management screens (the schedule editor's):
	/// a grey body, grey cards headed by a slate strip and framing white content, and one dark action
	/// bar across the foot.
	/// </summary>
	internal static class Palette
	{
		public static readonly Color Body = new Color32(149, 151, 160, 255);
		public static readonly Color Card = new Color32(201, 201, 201, 255);
		public static readonly Color Strip = new Color32(95, 107, 119, 255);
		public static readonly Color RowArea = new Color32(255, 255, 255, 255);
		/// <summary>The game's tooltip colour for a reason something cannot be done.</summary>
		public const string Reason = "#F44A47";

		/// <summary>The action bar's button.</summary>
		public static readonly ColorStyleSetting ActionBar = Style(new Color32(62, 67, 87, 255), new Color32(88, 95, 122, 255), new Color32(128, 138, 178, 255),
			disabled: new Color32(106, 105, 102, 255), disabledHover: new Color32(128, 128, 128, 255));

		/// <summary>A button drawn as part of a strip (a family's chevron and name): the strip's colour, a shade lighter under the pointer.</summary>
		public static readonly ColorStyleSetting StripButton = Style(Strip, new Color32(118, 131, 145, 255), new Color32(118, 131, 145, 255),
			disabled: Strip, disabledHover: new Color32(118, 131, 145, 255));

		/// <summary>A button that cannot be picked: the blue style's disabled grey in every state.</summary>
		public static readonly ColorStyleSetting Disabled = Style(PUITuning.Colors.ButtonBlueStyle.disabledColor, PUITuning.Colors.ButtonBlueStyle.disabledColor,
			PUITuning.Colors.ButtonBlueStyle.disabledColor, PUITuning.Colors.ButtonBlueStyle.disabledColor, PUITuning.Colors.ButtonBlueStyle.disabledColor);

		/// <summary>Blue, or pink while picked, as the game's buttons are.</summary>
		public static ColorStyleSetting Picked(bool picked) => picked ? PUITuning.Colors.ButtonPinkStyle : PUITuning.Colors.ButtonBlueStyle;

		private static ColorStyleSetting Style(Color inactive, Color hover, Color active, Color disabled, Color disabledHover)
		{
			var style = ScriptableObject.CreateInstance<ColorStyleSetting>();
			style.inactiveColor = inactive;
			style.hoverColor = hover;
			style.activeColor = active;
			style.disabledColor = style.disabledActiveColor = disabled;
			style.disabledhoverColor = disabledHover;
			return style;
		}
	}

	/// <summary>The window's building blocks: strips, cards, sides, columns of buttons, paragraphs and panes.</summary>
	internal static class Widgets
	{
		/// <summary>Both sides of a tab ask for this much, so they split the window evenly whatever their rows hold.</summary>
		public const float ColumnWidth = 460f;
		/// <summary>Every strip is at least as tall as the buttons some strips hold, so strips side by side are level.</summary>
		private const float StripHeight = 24f;
		private const int ButtonsPerRow = 7;
		private static readonly Vector2 IconSize = new Vector2(44f, 44f);
		private static readonly RectOffset ButtonMargin = new RectOffset(5, 5, 5, 5);
		private static readonly RectOffset RowPadding = new RectOffset(10, 10, 6, 10);

		// ---- strips and cards ----

		/// <summary>A slate strip for a card's or a side's heading and tools, laid out left to right.</summary>
		public static PPanel Strip(string name)
		{
			return new PPanel(name)
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				BackColor = Palette.Strip,
				Margin = new RectOffset(10, 10, 4, 4),
			}.AddChild(new PSpacer { PreferredSize = new Vector2(0f, StripHeight), FlexSize = Vector2.zero });
		}

		/// <summary>A strip's heading, taking the room its tools leave.</summary>
		public static PLabel Heading(string name, string text, int size)
		{
			return new PLabel(name)
			{
				Text = text,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(size),
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
			};
		}

		/// <summary>The white area of a card, where its rows go; with <paramref name="fill"/> it takes the card's whole height.</summary>
		public static PPanel Rows(string name, bool fill = false)
		{
			return new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, fill ? 1f : 0f),
				Margin = RowPadding,
				BackColor = Palette.RowArea,
			};
		}

		/// <summary>
		/// A grey card: <paramref name="strip"/> flush with its edges, then <paramref name="rows"/>
		/// framed in its grey on the sides and bottom (none: the strip alone). With <paramref name="fill"/> the card asks for
		/// no height of its own and an equal share of what is left, so cards side by side split it.
		/// </summary>
		public static PPanel Card(string name, PPanel strip, PPanel rows, bool fill = false)
		{
			var flex = new Vector2(1f, fill ? 1f : 0f);
			var card = new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = flex,
				BackColor = Palette.Card,
			};
			if (fill)
				card.AddOnRealize(TakeAShare);
			card.AddChild(strip);
			if (rows == null)
				return card;
			card.AddChild(new PPanel(rows.Name + "Frame")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = flex,
				Margin = new RectOffset(4, 4, 0, 4),
			}.AddChild(rows));
			return card;
		}

		/// <summary>A card with a plain heading; the rows go into <paramref name="rows"/>.</summary>
		public static PPanel Card(string name, string heading, out PPanel rows)
		{
			rows = Rows(name + "Rows");
			return Card(name, Strip(name + "Header").AddChild(Heading(name + "Heading", heading, 15)), rows);
		}

		/// <summary>
		/// One side of a tab: its heading in a strip, with <paramref name="leading"/> before it and
		/// <paramref name="trailing"/> after (a select-all box, a Clear button), over one grey card
		/// holding <paramref name="column"/>, which scrolls in a pane named <paramref name="scrollName"/>
		/// or, with none, fills the card. <paramref name="below"/> goes under the card, which then has
		/// two shares of the side's height to its one. Both sides of a tab split the window in half.
		/// </summary>
		public static PPanel Side(string name, string heading, PPanel column, string scrollName,
			IEnumerable<IUIComponent> leading = null, IEnumerable<IUIComponent> trailing = null, IUIComponent below = null)
		{
			var side = new PPanel(name + "Side")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = Vector2.one,
			}.AddOnRealize(go =>
			{
				var share = go.AddOrGet<LayoutElement>();
				share.minWidth = ColumnWidth;
				share.preferredWidth = ColumnWidth;
				share.flexibleWidth = 1f;
				share.layoutPriority = 10;
			});
			PPanel header = Strip(name + "Header");
			if (leading != null)
				foreach (IUIComponent part in leading)
					header.AddChild(part);
			header.AddChild(Heading(name + "Heading", heading, 18));
			if (trailing != null)
				foreach (IUIComponent part in trailing)
					header.AddChild(part);
			var card = new PPanel(name + "ListCard")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				BackColor = Palette.Card,
			}.AddOnRealize(TakeAShare);
			if (scrollName != null)
				card.AddChild(Scrollable(scrollName, column).AddOnRealize(TakeAShare));
			else
			{
				column.FlexSize = Vector2.one;
				card.AddChild(column);
			}
			var group = new PPanel(name + "Group")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = Vector2.one,
			}.AddChild(header).AddChild(card);
			side.AddChild(group);
			if (below == null)
				return side;
			group.AddOnRealize(go => TakeShares(go, 2f));
			// The gap above it is the holder's margin, not the side's spacing: the shares are of the
			// side's whole height, and spacing between them would push the card past its foot.
			side.AddChild(new PPanel(name + "After")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = Vector2.one,
				Margin = new RectOffset(0, 0, 6, 0),
			}.AddChild(below).AddOnRealize(TakeAShare));
			return side;
		}

		/// <summary>
		/// A side's column of cards, at least <see cref="ColumnWidth"/> wide. One that scrolls keeps
		/// room on its right for the scrollbar; one that never does sits as far from the card's frame
		/// on every side.
		/// </summary>
		public static PPanel Column(string name, bool scrolls)
		{
			return new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 4,
				FlexSize = new Vector2(1f, 0f),
				Margin = scrolls ? new RectOffset(0, 12, 0, 8) : new RectOffset(0, 0, 0, 0),
			// Width only: a spacer is flexible both ways by default, and in a column that fills its side
			// it would take a share of the height like a card.
			}.AddChild(new PSpacer { PreferredSize = new Vector2(ColumnWidth, 0f), FlexSize = new Vector2(1f, 0f) });
		}

		/// <summary>A pane of its own, scrolling vertically.</summary>
		public static PScrollPane Scrollable(string name, IUIComponent child)
		{
			return new PScrollPane(name)
			{
				ScrollVertical = true,
				ScrollHorizontal = false,
				AlwaysShowVertical = false,
				FlexSize = Vector2.one,
				Child = child,
			};
		}

		/// <summary>Asks the layout for no height of its own and an equal share of the space left (PLib honours the higher priority).</summary>
		public static void TakeAShare(GameObject go) => TakeShares(go, 1f);

		/// <summary>As <see cref="TakeAShare"/>, with <paramref name="shares"/> shares of the space left.</summary>
		public static void TakeShares(GameObject go, float shares)
		{
			var share = go.AddOrGet<LayoutElement>();
			share.minHeight = 0f;
			share.preferredHeight = 0f;
			share.flexibleHeight = shares;
			share.layoutPriority = 10;
		}

		// ---- text ----

		/// <summary>
		/// A paragraph for a card: dark text, aligned left (the game's text style centres it), wrapped
		/// at whatever width the card has. It asks for no width: as one line it would widen its column.
		/// </summary>
		public static PPanel Paragraph(string name, string text)
		{
			// Not a PLabel: a PLabel fixes its text's box at the text's size when it is built (one line
			// as long as the sentence) and keeps it. This is a plain panel whose text is laid out like
			// any other child, at the panel's width, and as tall as its wrapped lines.
			return new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(4, 4, 4, 4),
			}.AddOnRealize(go =>
			{
				var holder = new GameObject("Text", typeof(RectTransform));
				holder.SetActive(false);
				holder.transform.SetParent(go.transform, false);
				// As PLib makes its own text: key and style set while the object is inactive.
				LocText label = holder.AddComponent<LocText>();
				label.key = string.Empty;
				TextStyleSetting style = PUITuning.Fonts.TextDarkStyle.DeriveStyle(15);
				style.enableWordWrapping = true;
				label.textStyleSetting = style;
				holder.SetActive(true);
				label.text = text;
				label.alignment = TMPro.TextAlignmentOptions.TopLeft;
				label.textWrappingMode = TMPro.TextWrappingModes.Normal;
				label.raycastTarget = false;
				var width = holder.AddComponent<LayoutElement>();
				width.minWidth = 0f;
				width.preferredWidth = 0f;
				width.flexibleWidth = 1f;
				width.layoutPriority = 10;
			});
		}

		/// <summary>A line of dark text across a column, at <paramref name="size"/>.</summary>
		public static PLabel Line(string name, string text, int size, RectOffset margin)
		{
			return new PLabel(name)
			{
				Text = text,
				TextStyle = size > 0 ? PUITuning.Fonts.TextDarkStyle.DeriveStyle(size) : PUITuning.Fonts.TextDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = margin,
			};
		}

		// ---- buttons ----

		/// <summary>A small blue button for a strip.</summary>
		public static PButton StripButton(string name, string text, string tooltip, System.Action onClick)
		{
			return StripToggle(name, text, tooltip, false, onClick);
		}

		/// <summary>A small button for a strip: pink while <paramref name="picked"/>, as a picked button is.</summary>
		public static PButton StripToggle(string name, string text, string tooltip, bool picked, System.Action onClick)
		{
			return new PButton(name)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(13),
				Color = Palette.Picked(picked),
				Margin = new RectOffset(10, 10, 3, 3),
				OnClick = _ => onClick(),
			};
		}

		/// <summary>A button drawn as part of the strip it sits on: its colour without the border every button has.</summary>
		public static void Flatten(GameObject button)
		{
			KImage background = button.GetComponent<KImage>();
			if (background != null)
				background.sprite = null;
		}

		/// <summary>
		/// A type as the build menu shows it, named by its prefab id. With no <paramref name="onClick"/>
		/// it is greyed and does nothing. <paramref name="note"/> goes on a second line of the tooltip:
		/// what a click does, or with <paramref name="isReason"/>, why it is greyed, in the game's
		/// colour for one.
		/// </summary>
		public static PButton BuildingButton(Family family, bool picked, System.Action onClick, string note = null, bool isReason = false)
		{
			BuildingDef def = family.Representative;
			string tooltip = family.Name;
			if (family.Members.Count > 1)
				tooltip += ModStrings.UPGRADEPLANNER.VariantsTooltip.Replace("{0}", family.Members.Count.ToString());
			if (!string.IsNullOrEmpty(note))
				tooltip += "\n" + (isReason ? "<color=" + Palette.Reason + ">" + note + "</color>" : note);
			var button = new PButton(def.PrefabID)
			{
				Sprite = def.GetUISprite(),
				SpriteSize = IconSize,
				MaintainSpriteAspect = true,
				ToolTip = tooltip,
				Margin = ButtonMargin,
				Color = Palette.Picked(picked),
			};
			if (onClick != null)
				return button.SetClick(onClick);
			// Greyed: the disabled grey for the plate, the icon faded, and no clicks or hover.
			button.Color = Palette.Disabled;
			button.AddOnRealize(go =>
			{
				KButton k = go.GetComponent<KButton>();
				if (k != null)
					k.isInteractable = false;
				foreach (Image image in go.GetComponentsInChildren<Image>())
					if (!(image is KImage))
						image.color = new Color(1f, 1f, 1f, 0.45f);
			});
			return button;
		}

		private static PButton SetClick(this PButton button, System.Action onClick)
		{
			button.OnClick = _ => onClick();
			return button;
		}

		/// <summary>Buttons in rows of <see cref="ButtonsPerRow"/>, since a horizontal panel never wraps.</summary>
		public static void AddButtonRows(PPanel rows, List<IUIComponent> buttons)
		{
			PPanel row = null;
			for (int i = 0; i < buttons.Count; i++)
			{
				if (i % ButtonsPerRow == 0)
				{
					row = new PPanel("Row" + i / ButtonsPerRow)
					{
						Direction = PanelDirection.Horizontal,
						Alignment = TextAnchor.MiddleLeft,
						Spacing = 2,
					};
					rows.AddChild(row);
				}
				row.AddChild(buttons[i]);
			}
		}

		/// <summary>Where a game material picker goes once the window is built; see <see cref="MaterialPicker"/>.</summary>
		public static PPanel PickerHost(string name, System.Action<GameObject> onRealize)
		{
			return new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(0, 0, 4, 4),
			}.AddOnRealize(go => onRealize(go));
		}
	}
}
