using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;
using OPTIONS = NaturalBackwalls.ModStrings.NATURALBACKWALLS.OPTIONS;

namespace NaturalBackwalls
{
	/// <summary>
	/// Mod options: one row per biome group (material dropdown plus coverage), created dynamically
	/// for the groups the active DLCs make relevant and stored in dictionaries keyed by group id.
	/// Read again for every world generated, so changes need no restart.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	public sealed class Options : IOptions
	{
		public const string NoMaterial = "None";

		// Option titles, tooltips and categories are string table keys (see ModStrings), which PLib
		// resolves when it builds the dialog, so the translation loaded for the game's language shows.
		[Option("STRINGS.NATURALBACKWALLS.OPTIONS.FEATURE_BACKWALLS.NAME", "STRINGS.NATURALBACKWALLS.OPTIONS.FEATURE_BACKWALLS.TOOLTIP", "STRINGS.NATURALBACKWALLS.OPTIONS.CATEGORY.GENERAL")]
		[JsonProperty]
		public bool FeatureBackwalls { get; set; } = true;

		[Option("STRINGS.NATURALBACKWALLS.OPTIONS.OTHER_PLANETOIDS.NAME", "STRINGS.NATURALBACKWALLS.OPTIONS.OTHER_PLANETOIDS.TOOLTIP", "STRINGS.NATURALBACKWALLS.OPTIONS.CATEGORY.GENERAL")]
		[JsonProperty]
		public bool OtherPlanetoids { get; set; } = false;

		[Option("STRINGS.NATURALBACKWALLS.OPTIONS.OTHER_MOD_BIOMES.NAME", "STRINGS.NATURALBACKWALLS.OPTIONS.OTHER_MOD_BIOMES.TOOLTIP", "STRINGS.NATURALBACKWALLS.OPTIONS.CATEGORY.GENERAL")]
		[JsonProperty]
		public bool OtherModBiomes { get; set; } = true;

		[Option("STRINGS.NATURALBACKWALLS.OPTIONS.OTHER_MOD_COVERAGE.NAME", "STRINGS.NATURALBACKWALLS.OPTIONS.OTHER_MOD_COVERAGE.TOOLTIP", "STRINGS.NATURALBACKWALLS.OPTIONS.CATEGORY.GENERAL", Format = "F2")]
		[Limit(0.0, 1.0)]
		[JsonProperty]
		public float OtherModCoverage { get; set; } = 0.4f;

		[Option("STRINGS.NATURALBACKWALLS.OPTIONS.EXPORT_OVERLAY.NAME", "STRINGS.NATURALBACKWALLS.OPTIONS.EXPORT_OVERLAY.TOOLTIP", "STRINGS.NATURALBACKWALLS.OPTIONS.CATEGORY.GENERAL")]
		public System.Action<object> ExportOverlayButton => _ => ExportOverlay.Run();

		[Option("STRINGS.NATURALBACKWALLS.OPTIONS.START_COVERAGE.NAME", "STRINGS.NATURALBACKWALLS.OPTIONS.START_COVERAGE.TOOLTIP", "STRINGS.NATURALBACKWALLS.OPTIONS.CATEGORY.STARTING_BIOME", Format = "F2")]
		[Limit(0.0, 1.0)]
		[JsonProperty]
		public float StartCoverage { get; set; } = 1f;

		/// <summary>Material element id per biome group id; missing means the group's default.</summary>
		[JsonProperty]
		public Dictionary<string, string> Materials { get; set; } = new Dictionary<string, string>();

		/// <summary>Coverage fraction (0 to 1) per biome group id when it is not the starting biome; missing means the group's default.</summary>
		[JsonProperty]
		public Dictionary<string, float> Coverages { get; set; } = new Dictionary<string, float>();

		/// <summary>Backwall noise pattern id per subworld zone type; missing means the zone's default.</summary>
		[JsonProperty]
		public Dictionary<string, string> PatternIds { get; set; } = new Dictionary<string, string>();

		public Pattern PatternFor(PatternGroup group)
		{
			if (PatternIds != null && PatternIds.TryGetValue(group.Id, out string id))
			{
				Pattern chosen = Patterns.Find(id);
				if (chosen != null && (chosen.Tree != null || group.Vanilla))
					return chosen;
			}
			return Patterns.Find(group.DefaultPattern) ?? Patterns.All[0];
		}

		public string MaterialFor(BiomeGroup group)
		{
			if (Materials != null && Materials.TryGetValue(group.Id, out string chosen) && !string.IsNullOrEmpty(chosen))
				return chosen;
			return group.DefaultMaterial;
		}

		public float CoverageFor(BiomeGroup group)
		{
			if (Coverages != null && Coverages.TryGetValue(group.Id, out float coverage))
				return UnityEngine.Mathf.Clamp01(coverage);
			return group.DefaultCoverage;
		}

		// Help above each dynamic section, as dynamic text blocks: static categories are sorted by name,
		// dynamic ones keep the order they are yielded in, after the static ones. The text is read here,
		// each time the dialog is built, so it is the current language's.
		public IEnumerable<IOptionsEntry> CreateOptions()
		{
			yield return new TextBlockOptionsEntry("BiomesHelp", new OptionAttribute(OPTIONS.BIOMES_HELP, "", OPTIONS.CATEGORY.BIOMES));
			foreach (BiomeGroup group in BiomeGroups.All)
				if (BiomeGroups.IsAvailable(group))
					yield return new BiomeOptionsEntry(group, this);
			yield return new TextBlockOptionsEntry("PatternsHelp", new OptionAttribute(OPTIONS.PATTERNS_HELP, "", OPTIONS.CATEGORY.PATTERNS));
			foreach (PatternGroup group in Patterns.Groups)
				if (group.IsAvailable)
					yield return new PatternOptionsEntry(group, this);
		}

		public void OnOptionsChanged()
		{
		}

		/// <summary>Reads the current options from disk (falls back to defaults).</summary>
		public static Options Load()
		{
			return POptions.ReadSettings<Options>() ?? new Options();
		}
	}
}
