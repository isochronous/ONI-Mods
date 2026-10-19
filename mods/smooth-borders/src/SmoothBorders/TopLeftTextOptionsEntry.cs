using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmoothBorders
{
	/// <summary>
	/// PLib's text block centres its text. This one is a read-only PTextArea, whose text wraps at the
	/// panel's width with every line left-aligned from the top.
	/// </summary>
	public sealed class TopLeftTextOptionsEntry : TextBlockOptionsEntry
	{
		/// <summary>Lines of height the intro block reserves.</summary>
		private const int Lines = 3;

		/// <summary>Space around the intro block.</summary>
		private static readonly RectOffset Padding = new RectOffset(4, 4, 4, 6);

		public TopLeftTextOptionsEntry(string field, IOptionSpec spec) : base(field, spec)
		{
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			parent.AddChild(new PTextArea(Field)
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
			}),
			new GridComponentSpec(row, 0)
			{
				Margin = Padding,
				Alignment = TextAnchor.UpperLeft,
				ColumnSpan = 2
			});
		}
	}
}
