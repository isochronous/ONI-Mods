namespace NaturalBackwalls
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations
	/// (ModStrings.NATURALBACKWALLS.X is the key STRINGS.NATURALBACKWALLS.X) and replaced from
	/// translations/&lt;locale&gt;.po. Code reads the fields when it needs the text, so a translation
	/// loaded later is what the player sees. Option attributes name these fields by key, which PLib
	/// resolves through the string table.
	/// </summary>
	public static class ModStrings
	{
		public static class NATURALBACKWALLS
		{
			public static class OPTIONS
			{
				public static class CATEGORY
				{
					public static LocString GENERAL = "General";
					public static LocString STARTING_BIOME = "Starting biome";
					public static LocString BIOMES = "Biomes";
					public static LocString PATTERNS = "Patterns";
					public static LocString DEVELOPER = "Developer options";
				}

				public static class LOG_CENSUS
				{
					public static LocString NAME = "Log backwall census";
					public static LocString TOOLTIP = "Logs backwall counts per zone type to Player.log at each load.";
				}

				public static class FEATURE_BACKWALLS
				{
					public static LocString NAME = "Backwalls behind carved caves and lakes";
					public static LocString TOOLTIP = "Worldgen carves feature rooms (caves, lakes, geodes) into a biome after claiming their cells, and the game's own backwall pass skips claimed cells, so those rooms would stay bare. On, they get backwalls from the same noise as the rest of the biome.";
				}

				public static class OTHER_PLANETOIDS
				{
					public static LocString NAME = "Backwalls on other planetoids";
					public static LocString TOOLTIP = "Spaced Out: also give the asteroids you did not start on backwalls. Off matches the Aquatic Planet Pack, whose other planetoids have none.";
				}

				public static class OTHER_MOD_BIOMES
				{
					public static LocString NAME = "Backwalls in biomes from other mods";
					public static LocString TOOLTIP = "Biomes this mod does not know (added by worldgen mods) get a backwall of their own most common solid at the coverage below, with the pattern of their zone. Off leaves them bare.";
				}

				public static class OTHER_MOD_COVERAGE
				{
					public static LocString NAME = "Coverage in biomes from other mods";
					public static LocString TOOLTIP = "Band size used for biomes this mod does not know.";
				}

				public static class EXPORT_OVERLAY
				{
					public static LocString NAME = "Export worldgen overlay";
					public static LocString TOOLTIP = "Writes the current settings as a mod-style folder of worldgen YAML (NaturalBackwalls-overlay next to your saves) for external renderers such as onimaxxing.com/worldgen or the BiomePreview tool. Feature caves and the starting-biome coverage are not representable there.";
				}

				public static class START_COVERAGE
				{
					public static LocString NAME = "Starting biome coverage";
					public static LocString TOOLTIP = "Coverage in whatever biome the colony starts in, replacing that biome's own value. The Aquatic Planet Pack fully walls its beach, hence 1.";
				}

				// Help above each dynamic section. PLib's text block does not wrap on its own, so the
				// line breaks are part of the string.
				public static LocString BIOMES_HELP = "Per biome: the backwall material, from the solids found there (None leaves it bare),\nand the coverage, a 0 to 1 band size used when it is not the starting biome.";
				public static LocString PATTERNS_HELP = "Per subworld zone: the noise pattern the backwall patches follow. The pack's trees are shipped with the mod.\nThe kelp forest tree is about twice as dense as the others for the same coverage. Hover a zone for its biomes.";

				/// <summary>The material dropdown's "no backwall" choice.</summary>
				public static class NO_MATERIAL
				{
					public static LocString NAME = "None";
					public static LocString TOOLTIP = "No natural backwall in this biome.";
				}

				/// <summary>Dropdown choice that is the row's default. {0} the choice's name.</summary>
				public static LocString DEFAULT_CHOICE = "{0} (default)";
				/// <summary>Tooltip of a biome row's coverage box. {0} the row's default coverage.</summary>
				public static LocString COVERAGE_TOOLTIP = "Coverage when this is not the starting biome: fraction of its cells that get a backwall, 0 to 1. Default {0}.";
				/// <summary>Tooltip of a pattern row. {0} the comma-separated biome families of the zone.</summary>
				public static LocString ZONE_BIOMES_TOOLTIP = "Biomes: {0}";
			}

			/// <summary>Biome group titles, one per BiomeGroup id in upper case.</summary>
			public static class BIOMES
			{
				public static LocString SANDSTONE = "Sandstone";
				public static LocString FOREST = "Forest";
				public static LocString FROZEN = "Tundra (frozen)";
				public static LocString CAUSTIC = "Caustic (jungle)";
				public static LocString MARSH = "Marsh";
				public static LocString OCEAN = "Ocean (tide pool)";
				public static LocString OIL = "Oily";
				public static LocString RUST = "Rust";
				public static LocString MAGMA = "Magma and Niobium";
				public static LocString BARREN = "Barren (regolith)";
				public static LocString SWAMP = "Swamp";
				public static LocString WASTELAND = "Wasteland";
				public static LocString METALLIC = "Metallic";
				public static LocString MOO = "Moo";
				public static LocString RADIOACTIVE = "Radioactive";
				public static LocString AQUATIC = "Aquatic (water asteroid)";
				public static LocString CARROTQUARRY = "Cool Pool (carrot quarry)";
				public static LocString ICECAVES = "Ice Caves";
				public static LocString SUGARWOODS = "Nectar (sugar woods)";
				public static LocString GARDEN = "Garden";
				public static LocString RAPTOR = "Feather (raptor)";
				public static LocString WETLANDS = "Wetlands";
				public static LocString BEACH = "Beach (Aquatic pack)";
				public static LocString REEF = "Reef (Aquatic pack)";
				public static LocString KELPFOREST = "Kelp Forest (Aquatic pack)";
				public static LocString ABYSS = "Abyss (Aquatic pack)";
			}

			/// <summary>Noise pattern names and descriptions, one class per Pattern id in upper case.</summary>
			public static class PATTERNS
			{
				public static class REEF
				{
					public static LocString NAME = "Reef";
					public static LocString DESC = "The pack's reef and beach pattern. Patches at about 45% of cells for a 0.4 band.";
				}

				public static class KELP
				{
					public static LocString NAME = "Kelp forest";
					public static LocString DESC = "The pack's kelp forest pattern: broad and dense. About 78% of cells for a 0.4 band, 52% for 0.2.";
				}

				public static class ABYSS
				{
					public static LocString NAME = "Abyss";
					public static LocString DESC = "The pack's abyss (murky brine) pattern. About 45% of cells for a 0.4 band, in a different shape from the reef.";
				}

				public static class BEACH
				{
					public static LocString NAME = "Beach";
					public static LocString DESC = "The pack's beach terrain pattern. About 45% of cells for a 0.4 band.";
				}

				public static class STRANGE
				{
					public static LocString NAME = "Strange (base game)";
					public static LocString DESC = "The base game's SandstoneStrange tree, which the pack uses for two subworlds. Dense: about 80% of cells for a 0.4 band.";
				}

				public static class VANILLA
				{
					public static LocString NAME = "As the game made it";
					public static LocString DESC = "Keeps the subworld's own backwall noise tree.";
				}
			}

			/// <summary>
			/// Subworld zone names and the biome families they are built from (the pattern row's
			/// tooltip), one class per ProcGen.SubWorld.ZoneType name in upper case.
			/// </summary>
			public static class ZONES
			{
				public static class SANDSTONE
				{
					public static LocString NAME = "Sandstone";
					public static LocString BIOMES = "Sandstone, Barren (regolith)";
				}

				public static class FOREST
				{
					public static LocString NAME = "Forest";
					public static LocString BIOMES = "Forest";
				}

				public static class FROZENWASTES
				{
					public static LocString NAME = "Tundra (frozen)";
					public static LocString BIOMES = "Tundra (frozen), Moo";
				}

				public static class TOXICJUNGLE
				{
					public static LocString NAME = "Caustic (jungle)";
					public static LocString BIOMES = "Caustic (jungle)";
				}

				public static class BOGGYMARSH
				{
					public static LocString NAME = "Marsh";
					public static LocString BIOMES = "Marsh";
				}

				public static class OCEAN
				{
					public static LocString NAME = "Ocean (tide pool)";
					public static LocString BIOMES = "Ocean (tide pool)";
				}

				public static class OILFIELD
				{
					public static LocString NAME = "Oily";
					public static LocString BIOMES = "Oily, Magma and Niobium";
				}

				public static class RUST
				{
					public static LocString NAME = "Rust";
					public static LocString BIOMES = "Rust, Metallic";
				}

				public static class MAGMACORE
				{
					public static LocString NAME = "Magma";
					public static LocString BIOMES = "Magma and Niobium";
				}

				public static class BARREN
				{
					public static LocString NAME = "Barren (regolith)";
					public static LocString BIOMES = "Barren (regolith), Aquatic (water asteroid)";
				}

				public static class SWAMP
				{
					public static LocString NAME = "Swamp";
					public static LocString BIOMES = "Swamp, Aquatic (water asteroid)";
				}

				public static class WASTELAND
				{
					public static LocString NAME = "Wasteland";
					public static LocString BIOMES = "Wasteland";
				}

				public static class METALLIC
				{
					public static LocString NAME = "Metallic";
					public static LocString BIOMES = "Metallic";
				}

				public static class MOO
				{
					public static LocString NAME = "Moo";
					public static LocString BIOMES = "Moo";
				}

				public static class RADIOACTIVE
				{
					public static LocString NAME = "Radioactive";
					public static LocString BIOMES = "Radioactive";
				}

				public static class CARROTQUARRY
				{
					public static LocString NAME = "Cool Pool (carrot quarry)";
					public static LocString BIOMES = "Cool Pool (carrot quarry)";
				}

				public static class ICECAVES
				{
					public static LocString NAME = "Ice Caves";
					public static LocString BIOMES = "Ice Caves";
				}

				public static class SUGARWOODS
				{
					public static LocString NAME = "Nectar (sugar woods)";
					public static LocString BIOMES = "Nectar (sugar woods)";
				}

				public static class PREHISTORICGARDEN
				{
					public static LocString NAME = "Garden";
					public static LocString BIOMES = "Garden";
				}

				public static class PREHISTORICRAPTOR
				{
					public static LocString NAME = "Feather (raptor)";
					public static LocString BIOMES = "Feather (raptor)";
				}

				public static class PREHISTORICWETLANDS
				{
					public static LocString NAME = "Wetlands";
					public static LocString BIOMES = "Wetlands";
				}

				public static class BEACH
				{
					public static LocString NAME = "Beach (Aquatic pack)";
					public static LocString BIOMES = "Beach (Aquatic pack)";
				}

				public static class REEF
				{
					public static LocString NAME = "Reef (Aquatic pack)";
					public static LocString BIOMES = "Reef (Aquatic pack)";
				}

				public static class KELPFOREST
				{
					public static LocString NAME = "Kelp Forest (Aquatic pack)";
					public static LocString BIOMES = "Kelp Forest (Aquatic pack)";
				}

				public static class ABYSS
				{
					public static LocString NAME = "Abyss (Aquatic pack)";
					public static LocString BIOMES = "Abyss (Aquatic pack)";
				}
			}
		}

		/// <summary>
		/// The current text for a key such as "STRINGS.NATURALBACKWALLS.BIOMES.SANDSTONE", for the
		/// biome, pattern and zone tables whose text is picked by id at runtime. Falls back to the key.
		/// </summary>
		public static string Lookup(string key)
		{
			return Strings.TryGet(key, out StringEntry entry) ? entry.String : key;
		}
	}
}
