using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;
using OPTIONS = SmoothBorders.ModStrings.SMOOTHBORDERS.OPTIONS;

namespace SmoothBorders
{
	/// <summary>
	/// Mod options: the wall thickness in tiles for each kind of asteroid. Read again for every
	/// world generated, so changes need no restart. All entries come from CreateOptions so the
	/// dialog shows them in the order they are yielded.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options : IOptions
	{
		[JsonProperty]
		public int Primary { get; set; } = Groups.Primary.Default;

		[JsonProperty]
		public int Small { get; set; } = Groups.Small.Default;

		[JsonProperty]
		public int Moonlets { get; set; } = Groups.Moonlets.Default;

		[JsonProperty]
		public int TinyMoonlets { get; set; } = Groups.TinyMoonlets.Default;

		/// <summary>The setting for a group, limited to the group's range.</summary>
		public int ValueFor(Group group)
		{
			int value;
			if (group == Groups.Primary) value = Primary;
			else if (group == Groups.Small) value = Small;
			else if (group == Groups.Moonlets) value = Moonlets;
			else value = TinyMoonlets;
			return group.Clamp(value);
		}

		public IEnumerable<IOptionsEntry> CreateOptions()
		{
			yield return new TopLeftTextOptionsEntry("Intro", new OptionAttribute(OPTIONS.INTRO));
			yield return Row(nameof(Primary), Groups.Primary, OPTIONS.PRIMARY.NAME, OPTIONS.PRIMARY.TOOLTIP);
			yield return Row(nameof(Small), Groups.Small, OPTIONS.SMALL.NAME, OPTIONS.SMALL.TOOLTIP);
			yield return Row(nameof(Moonlets), Groups.Moonlets, OPTIONS.MOONLETS.NAME, OPTIONS.MOONLETS.TOOLTIP);
			yield return Row(nameof(TinyMoonlets), Groups.TinyMoonlets, OPTIONS.TINY_MOONLETS.NAME, OPTIONS.TINY_MOONLETS.TOOLTIP);
		}

		// The range is appended in code so translators never handle it and the label cannot
		// disagree with the slider's limits.
		private static IOptionsEntry Row(string field, Group group, string name, string tooltip)
		{
			return new IntOptionsEntry(field,
				new OptionAttribute(name + " (" + group.RangeText + ")", tooltip),
				new LimitAttribute(group.Min, group.Max));
		}

		public void OnOptionsChanged()
		{
		}

		/// <summary>Reads the current options from disk (falls back to defaults), each value limited to its range.</summary>
		public static Options Load()
		{
			Options options = POptions.ReadSettings<Options>() ?? new Options();
			options.Primary = Groups.Primary.Clamp(options.Primary);
			options.Small = Groups.Small.Clamp(options.Small);
			options.Moonlets = Groups.Moonlets.Clamp(options.Moonlets);
			options.TinyMoonlets = Groups.TinyMoonlets.Clamp(options.TinyMoonlets);
			return options;
		}
	}
}
