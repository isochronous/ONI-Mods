namespace StoragePodRedux
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations
	/// (ModStrings.BUILDINGS.PREFABS.X.NAME is the key STRINGS.BUILDINGS.PREFABS.X.NAME, which
	/// is how the game finds a building's name) and replaced from translations/&lt;locale&gt;.po.
	/// The option attributes name their keys, which PLib resolves through the string table.
	/// </summary>
	public static class ModStrings
	{
		public static class BUILDINGS
		{
			public static class PREFABS
			{
				public static class STORAGEPODCONFIG
				{
					public static LocString NAME = STRINGS.UI.FormatAsLink("Storage Pod", StoragePodConfig.ID.ToUpperInvariant());
					public static LocString DESC = "A sealed pod that keeps a lot of material in very little space, and does not care what it is bolted to.";
					public static LocString EFFECT = "Stores the " + STRINGS.UI.FormatAsLink("Solid", "ELEMENTS_SOLID") + " resources of your choosing.\n\nCompact, and can be built anywhere.";
				}

				public static class COOLPODCONFIG
				{
					public static LocString NAME = STRINGS.UI.FormatAsLink("Cool Pod", CoolPodConfig.ID.ToUpperInvariant());
					public static LocString DESC = "A small refrigerated pod that fits where a fridge will not.";
					public static LocString EFFECT = "Stores " + STRINGS.UI.FormatAsLink("Food", "FOOD") + " and slows its spoilage.\n\nCompact, and can be built anywhere. Uses less power once its contents are cold.";
				}
			}
		}

		public static class STORAGEPODREDUX
		{
			public static class OPTIONS
			{
				public static class CATEGORY
				{
					public static LocString STORAGE_POD = "Storage Pod";
					public static LocString COOL_POD = "Cool Pod";
				}

				public static class STORAGE_POD_ENABLED
				{
					public static LocString NAME = "Storage Pod";
					public static LocString TOOLTIP = "Add the Storage Pod building. Takes effect after a restart.";
				}

				public static class STORAGE_POD_CAPACITY
				{
					public static LocString NAME = "Capacity (kg)";
					public static LocString TOOLTIP = "How much the Storage Pod holds.";
				}

				public static class STORAGE_POD_STORES_FOOD
				{
					public static LocString NAME = "Stores food";
					public static LocString TOOLTIP = "Let the Storage Pod hold food as well as other solids. It is not refrigerated.";
				}

				public static class COOL_POD
				{
					public static LocString NAME = "Cool Pod";
					public static LocString TOOLTIP = "Add the Cool Pod building. Takes effect after a restart.";
				}

				public static class COOL_POD_CAPACITY
				{
					public static LocString NAME = "Capacity (kg)";
					public static LocString TOOLTIP = "How much food the Cool Pod holds.";
				}

				public static class COOL_POD_MODE
				{
					public static class AUTO
					{
						public static LocString NAME = "Auto";
						public static LocString TOOLTIP = "Enabled, unless Ronivan's Legacy - Industrial Revolution is enabled with its Fridge Pod turned on.";
					}

					public static class ENABLED
					{
						public static LocString NAME = "Enabled";
						public static LocString TOOLTIP = "Always add the Cool Pod.";
					}

					public static class DISABLED
					{
						public static LocString NAME = "Disabled";
						public static LocString TOOLTIP = "Never add the Cool Pod.";
					}
				}
			}
		}
	}
}
