using System.Collections.Generic;

namespace NaturalBackwalls
{
	/// <summary>A noise tree the backwall band is sampled with: the shape and density of the patches.</summary>
	public sealed class Pattern
	{
		/// <summary>Keeps the subworld's own tree; only meaningful for biomes the game already backwalls.</summary>
		public const string VanillaId = "Vanilla";

		public string Id { get; }
		/// <summary>Scoped worldgen tree name, or null for the subworld's own tree.</summary>
		public string Tree { get; }
		/// <summary>ModStrings.NATURALBACKWALLS.PATTERNS.&lt;ID&gt;.NAME, read in the current language.</summary>
		public string Title => ModStrings.Lookup(Key + "NAME");
		/// <summary>ModStrings.NATURALBACKWALLS.PATTERNS.&lt;ID&gt;.DESC, read in the current language.</summary>
		public string Description => ModStrings.Lookup(Key + "DESC");
		private string Key => "STRINGS.NATURALBACKWALLS.PATTERNS." + Id.ToUpperInvariant() + ".";

		public Pattern(string id, string tree)
		{
			Id = id; Tree = tree;
		}
	}

	/// <summary>
	/// A subworld zone type, the unit a noise pattern applies to: a backwall noise tree belongs to a
	/// subworld, not a biome, and every subworld declares one of these. The game's own zones (the
	/// Aquatic pack's) default to keeping their trees.
	/// </summary>
	public sealed class PatternGroup
	{
		public ProcGen.SubWorld.ZoneType Zone { get; }
		public string Id => Zone.ToString();
		public string Dlc { get; }
		public string DefaultPattern { get; }
		public bool Vanilla { get; }
		/// <summary>ModStrings.NATURALBACKWALLS.ZONES.&lt;ZONE&gt;.NAME, read in the current language.</summary>
		public string Title => ModStrings.Lookup(Key + "NAME");
		/// <summary>The biome families this zone's subworlds are built from, for the tooltip: ModStrings.NATURALBACKWALLS.ZONES.&lt;ZONE&gt;.BIOMES.</summary>
		public string Biomes => ModStrings.Lookup(Key + "BIOMES");
		private string Key => "STRINGS.NATURALBACKWALLS.ZONES." + Id.ToUpperInvariant() + ".";

		public PatternGroup(ProcGen.SubWorld.ZoneType zone, string dlc, string defaultPattern, bool vanilla = false)
		{
			Zone = zone; Dlc = dlc; DefaultPattern = defaultPattern; Vanilla = vanilla;
		}

		public bool IsAvailable => Dlc == null || DlcManager.IsContentSubscribed(Dlc);
	}

	public static class Patterns
	{
		/// <summary>
		/// The Aquatic Planet Pack's trees, shipped with the mod under their own names so no DLC is
		/// needed, plus one base-game tree. Densities are what a 0.4 band covers in the preview tool.
		/// </summary>
		public static readonly Pattern[] All =
		{
			new Pattern("Reef", "noise/NaturalBackwallsReef"),
			new Pattern("Kelp", "noise/NaturalBackwallsKelp"),
			new Pattern("Abyss", "noise/NaturalBackwallsAbyss"),
			new Pattern("Beach", "noise/NaturalBackwallsBeach"),
			new Pattern("Strange", "noise/SandstoneStrange"),
		};

		public static readonly Pattern Vanilla = new Pattern(Pattern.VanillaId, null);

		/// <summary>Defaults: the dense kelp forest tree for lush and marshy zones, the abyss tree for hot and rocky ones, the reef elsewhere.</summary>
		public static readonly PatternGroup[] Groups =
		{
			new PatternGroup(ProcGen.SubWorld.ZoneType.Sandstone, null, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Forest, null, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.FrozenWastes, null, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.ToxicJungle, null, "Kelp"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.BoggyMarsh, null, "Kelp"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Ocean, null, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.OilField, null, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Rust, null, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.MagmaCore, null, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Barren, null, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Swamp, DlcManager.EXPANSION1_ID, "Kelp"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Wasteland, DlcManager.EXPANSION1_ID, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Metallic, DlcManager.EXPANSION1_ID, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Moo, DlcManager.EXPANSION1_ID, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Radioactive, DlcManager.EXPANSION1_ID, "Abyss"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.CarrotQuarry, DlcManager.DLC2_ID, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.IceCaves, DlcManager.DLC2_ID, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.SugarWoods, DlcManager.DLC2_ID, "Kelp"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.PrehistoricGarden, DlcManager.DLC4_ID, "Kelp"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.PrehistoricRaptor, DlcManager.DLC4_ID, "Reef"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.PrehistoricWetlands, DlcManager.DLC4_ID, "Kelp"),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Beach, DlcManager.DLC5_ID, Pattern.VanillaId, vanilla: true),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Reef, DlcManager.DLC5_ID, Pattern.VanillaId, vanilla: true),
			new PatternGroup(ProcGen.SubWorld.ZoneType.KelpForest, DlcManager.DLC5_ID, Pattern.VanillaId, vanilla: true),
			new PatternGroup(ProcGen.SubWorld.ZoneType.Abyss, DlcManager.DLC5_ID, Pattern.VanillaId, vanilla: true),
		};

		private static Dictionary<ProcGen.SubWorld.ZoneType, PatternGroup> byZone;

		public static PatternGroup GroupFor(ProcGen.SubWorld.ZoneType zone)
		{
			if (byZone == null)
			{
				byZone = new Dictionary<ProcGen.SubWorld.ZoneType, PatternGroup>();
				foreach (PatternGroup g in Groups) byZone[g.Zone] = g;
			}
			return byZone.TryGetValue(zone, out PatternGroup group) ? group : null;
		}

		private static Dictionary<string, Pattern> byId;

		public static Pattern Find(string id)
		{
			if (byId == null)
			{
				byId = new Dictionary<string, Pattern>();
				foreach (Pattern p in All) byId[p.Id] = p;
				byId[Vanilla.Id] = Vanilla;
			}
			return id != null && byId.TryGetValue(id, out Pattern pattern) ? pattern : null;
		}
	}
}
