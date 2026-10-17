using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace JumperBridges
{
	// Titles and tooltips are string keys (see ModStrings), resolved by PLib when the options screen is built.
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options
	{
		[Option("STRINGS.JUMPERBRIDGES.OPTIONS.AUTOMATION.NAME", "STRINGS.JUMPERBRIDGES.OPTIONS.AUTOMATION.TOOLTIP")]
		[RestartRequired]
		[JsonProperty]
		public bool AutomationBridges { get; set; } = true;

		[Option("STRINGS.JUMPERBRIDGES.OPTIONS.POWER.NAME", "STRINGS.JUMPERBRIDGES.OPTIONS.POWER.TOOLTIP")]
		[RestartRequired]
		[JsonProperty]
		public bool PowerBridges { get; set; } = true;

		public static Options Load()
		{
			return POptions.ReadSettings<Options>() ?? new Options();
		}
	}
}
