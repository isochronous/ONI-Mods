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
					public static LocString TOOLTIP = "A Wire Bridge, Conductive Wire Bridge, Insulated Wire Bridge, Heavi-Watt Joint Plate or Heavi-Watt Conductive Joint Plate can be placed with either end directly on a power port, which is then connected, or on the end of any of these. A tile cannot be built over a joint plate's end. Needs a restart.";
				}
			}
		}
	}
}
