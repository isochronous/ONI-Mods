namespace JumperBridges
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations
	/// (ModStrings.JUMPERBRIDGES.X is the key STRINGS.JUMPERBRIDGES.X) and
	/// replaced from translations/&lt;locale&gt;.po. The option attributes name these keys, which
	/// PLib resolves through the string table when it builds the options screen.
	/// </summary>
	public static class ModStrings
	{
		public static class JUMPERBRIDGES
		{
			public static class OPTIONS
			{
				public static class AUTOMATION
				{
					public static LocString NAME = "Direct automation bridges";
					public static LocString TOOLTIP = "An Automation Wire Bridge or Automation Ribbon Bridge can be placed with either end directly on an automation port or another bridge's end, and that port is connected. Needs a restart.";
				}

				public static class POWER
				{
					public static LocString NAME = "Direct power bridges";
					public static LocString TOOLTIP = "A Wire Bridge, Conductive Wire Bridge or Insulated Wire Bridge can be placed with either end directly on a power port or another bridge's end, and that port is connected. Heavy-watt joint plates are unchanged. Needs a restart.";
				}
			}
		}
	}
}
