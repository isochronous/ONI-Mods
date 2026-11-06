namespace SmoothBorders
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations
	/// (ModStrings.SMOOTHBORDERS.X is the key STRINGS.SMOOTHBORDERS.X) and replaced from
	/// translations/&lt;locale&gt;.po. The options dialog reads the fields when it is built, so
	/// the text shown is the current language's. The range after each label is added in code.
	/// </summary>
	public static class ModStrings
	{
		public static class SMOOTHBORDERS
		{
			public static class OPTIONS
			{
				public static LocString INTRO = "Vanilla gives each asteroid type its own wall thickness, so each has its own setting. Defaults match vanilla. Changes apply to new worlds.";

				public static class PRIMARY
				{
					public static LocString NAME = "Primary asteroids";
					public static LocString TOOLTIP = "Game default: 4.";
				}

				public static class SMALL
				{
					public static LocString NAME = "Small asteroids";
					public static LocString TOOLTIP = "Game default: 3.";
				}

				public static class MOONLETS
				{
					public static LocString NAME = "Moonlets";
					public static LocString TOOLTIP = "Game default: 2.";
				}

				public static class TINY_MOONLETS
				{
					public static LocString NAME = "Tiny moonlets";
					public static LocString TOOLTIP = "Game default: 1.";
				}
			}
		}
	}
}
