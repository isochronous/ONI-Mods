using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OPTIONS = SmoothBorders.ModStrings.SMOOTHBORDERS.OPTIONS;

namespace SmoothBorders
{
	/// <summary>
	/// PLib's text block centres its text. This one is a read-only PTextArea, whose text wraps at the
	/// panel's width with every line left-aligned from the top.
	/// </summary>
	public sealed class TopLeftTextOptionsEntry : TextBlockOptionsEntry
	{
		/// <summary>Lines of height the intro block reserves.</summary>
		private const int Lines = 4;

		/// <summary>Space around the intro block.</summary>
		private static readonly RectOffset Padding = new RectOffset(4, 4, 4, 6);

		// PLib builds a [DynamicOption] handler with the property's name
		public TopLeftTextOptionsEntry(string field) : base(field, new OptionAttribute(OPTIONS.INTRO))
		{
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			// The text above a thin rule that separates it from the settings
			var text = new PTextArea(Field)
			{
				Text = LookInStrings(Title),
				ToolTip = LookInStrings(Tooltip),
				TextStyle = PUITuning.Fonts.TextLightStyle,
				BackColor = PUITuning.Colors.Transparent,
				TextAlignment = TextAlignmentOptions.TopLeft,
				LineCount = Lines,
				FlexSize = Vector2.right
			}.AddOnRealize(realized =>
			{
				realized.GetComponent<TMP_InputField>().readOnly = true;
				// PTextArea draws a white box border on its root; the intro is plain text
				realized.GetComponent<Image>().enabled = false;
			});
			var rule = new PSpacer
			{
				PreferredSize = new Vector2(1f, 1f),
				FlexSize = Vector2.right
			};
			rule.OnRealize += realized => realized.AddComponent<Image>().color = PUITuning.Colors.BackgroundLight;
			parent.AddChild(new PPanel("IntroBlock")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = Vector2.right
			}.AddChild(text).AddChild(rule),
			new GridComponentSpec(row, 0)
			{
				Margin = Padding,
				Alignment = TextAnchor.UpperLeft,
				ColumnSpan = 2
			});
		}
	}
}
