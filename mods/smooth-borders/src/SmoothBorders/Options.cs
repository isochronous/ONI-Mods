using Newtonsoft.Json;
using System.Collections.Generic;
using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace SmoothBorders
{
	/// <summary>
	/// Mod options: the wall thickness in tiles for each kind of asteroid. Read again for every
	/// world generated, so changes need no restart. The dialog lists the properties in declaration
	/// order, and PLib itself reads, writes and resets them: the intro block is a dynamic entry
	/// declared first, the four settings use RangedIntOptionsEntry.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options : IOptions
	{
		/// <summary>The intro block above the settings. It holds no value and is never saved.</summary>
		[DynamicOption(typeof(TopLeftTextOptionsEntry))]
		public LocText Intro => null;

		[DynamicOption(typeof(RangedIntOptionsEntry))]
		[Limit(1, 16)]
		[JsonProperty]
		public int Primary { get; set; } = Groups.Primary.Default;

		[DynamicOption(typeof(RangedIntOptionsEntry))]
		[Limit(1, 7)]
		[JsonProperty]
		public int Small { get; set; } = Groups.Small.Default;

		[DynamicOption(typeof(RangedIntOptionsEntry))]
		[Limit(1, 6)]
		[JsonProperty]
		public int Moonlets { get; set; } = Groups.Moonlets.Default;

		[DynamicOption(typeof(RangedIntOptionsEntry))]
		[Limit(1, 3)]
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

		/// <summary>No extra entries: every entry is a declared property.</summary>
		public IEnumerable<IOptionsEntry> CreateOptions()
		{
			return null;
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
