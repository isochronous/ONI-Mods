namespace DetectAnyElement
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations
	/// (ModStrings.MISC.TAGS.X is the key STRINGS.MISC.TAGS.X, where the game keeps tag names)
	/// and replaced from translations/&lt;locale&gt;.po. Code reads the fields when it needs the
	/// text, so a translation loaded later is what the player sees.
	/// </summary>
	public static class ModStrings
	{
		public static class MISC
		{
			public static class TAGS
			{
				/// <summary>The "Any" choice in the element sensors' side screen.</summary>
				public static LocString DETECTANYELEMENT_ANY = "Any";
			}
		}
	}
}
