using System;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using OPTIONS = SmoothBorders.ModStrings.SMOOTHBORDERS.OPTIONS;

namespace SmoothBorders
{
	/// <summary>
	/// The number field with slider for one wall setting. PLib builds it from the property's
	/// [DynamicOption] and [Limit], then reads, writes and resets the value itself. The label
	/// gets its range appended here so translators never handle it and the label cannot
	/// disagree with the slider's limits.
	/// </summary>
	public sealed class RangedIntOptionsEntry : IntOptionsEntry
	{
		public RangedIntOptionsEntry(string field, LimitAttribute limit)
			: base(field, Spec(field, limit), limit)
		{
		}

		private static OptionAttribute Spec(string field, LimitAttribute limit)
		{
			string name, tooltip;
			switch (field)
			{
				case nameof(Options.Primary):
					name = OPTIONS.PRIMARY.NAME; tooltip = OPTIONS.PRIMARY.TOOLTIP; break;
				case nameof(Options.Small):
					name = OPTIONS.SMALL.NAME; tooltip = OPTIONS.SMALL.TOOLTIP; break;
				case nameof(Options.Moonlets):
					name = OPTIONS.MOONLETS.NAME; tooltip = OPTIONS.MOONLETS.TOOLTIP; break;
				case nameof(Options.TinyMoonlets):
					name = OPTIONS.TINY_MOONLETS.NAME; tooltip = OPTIONS.TINY_MOONLETS.TOOLTIP; break;
				default:
					throw new ArgumentException("No wall setting named " + field);
			}
			return new OptionAttribute(name + " (" + (int)limit.Minimum + "-" + (int)limit.Maximum + ")", tooltip);
		}
	}
}
