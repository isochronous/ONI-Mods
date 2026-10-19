using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using UnityEngine;

namespace SmoothBorders
{
	/// <summary>
	/// PLib's text block centres its text. This one sits top-left, takes the panel's width and
	/// wraps there.
	/// </summary>
	public sealed class TopLeftTextOptionsEntry : TextBlockOptionsEntry
	{
		private static readonly TextStyleSetting WrapStyle = MakeStyle();

		public TopLeftTextOptionsEntry(string field, IOptionSpec spec) : base(field, spec)
		{
		}

		private static TextStyleSetting MakeStyle()
		{
			TextStyleSetting style = PUITuning.Fonts.TextLightStyle.DeriveStyle();
			style.enableWordWrapping = true;
			return style;
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			parent.AddChild(new PLabel(Field)
			{
				Text = LookInStrings(Title),
				ToolTip = LookInStrings(Tooltip),
				TextStyle = WrapStyle,
				TextAlignment = TextAnchor.UpperLeft,
				FlexSize = Vector2.right
			}, new GridComponentSpec(row, 0)
			{
				Margin = CONTROL_MARGIN,
				Alignment = TextAnchor.UpperLeft,
				ColumnSpan = 2
			});
		}
	}
}
