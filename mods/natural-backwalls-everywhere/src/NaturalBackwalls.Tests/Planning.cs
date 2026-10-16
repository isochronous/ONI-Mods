using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using ProcGen;

namespace NaturalBackwalls.Tests
{
	/// <summary>
	/// The decision the mod makes at world generation, which is plain data: which subworlds get a
	/// backwall noise tree and which band tables their biomes get. CloneInToNewWorld_Patch.Plan is
	/// the function the patch and the YAML export share, so it can be run here on the game's own
	/// worldgen data and on made-up subworlds. What these tests cannot show is the generated
	/// world itself (see the notes in WorldgenOnly).
	/// </summary>
	public static class Planning
	{
		private const SubWorld.ZoneType Sandstone = SubWorld.ZoneType.Sandstone;

		[OniTest("NB: on the game's own worldgen data every subworld with a mapped biome gets its zone's tree and its group's material and coverage", Timeout = 40), Layer(0)]
		public static IEnumerator DefaultsOnRealData(TestContext t)
		{
			if (SettingsCache.subworlds.Count == 0)
				t.Skip("the game has no worldgen subworlds loaded");
			var options = new Options();
			var bands = SettingsCache.biomes.BiomeBackgroundElementBandConfigurations;
			var plans = CloneInToNewWorld_Patch.Plan(SettingsCache.subworlds, bands, new HashSet<string>(), options, out int surface);
			int enabled = 0, checkedBands = 0;
			foreach (var plan in plans)
			{
				if (!plan.Enabled)
					continue;
				enabled++;
				PatternGroup zone = Patterns.GroupFor(plan.Subworld.zoneType);
				Pattern pattern = zone != null ? options.PatternFor(zone) : Patterns.All[0];
				t.Assert.Eq(CloneInToNewWorld_Patch.TreeName(pattern ?? Patterns.All[0]), plan.Tree, plan.Name + ": the tree of its zone's default pattern");
				t.Assert.False(CloneInToNewWorld_Patch.IsSurface(plan.Name, plan.Subworld), plan.Name + " is not a surface or space subworld");
				foreach (var pair in plan.Bands)
				{
					BiomeGroup group = BiomeGroups.ForBiome(pair.Key.Substring(0, pair.Key.Length - CloneInToNewWorld_Patch.Suffix.Length));
					if (group == null)
						continue;
					var read = Data.Read(pair.Value);
					if (read[0].element == "Vacuum")
						continue;
					checkedBands++;
					t.Assert.Eq(options.MaterialFor(group), read[0].element, pair.Key + ": the group's default material");
					t.Assert.True(System.Math.Abs(read[0].size - options.CoverageFor(group)) < 1e-4f, pair.Key + ": coverage " + read[0].size + " is the group's " + options.CoverageFor(group));
					t.Assert.True(System.Math.Abs(read.Sum(b => b.size) - 1f) < 1e-4f, pair.Key + ": the bands add up to 1");
				}
			}
			t.Log(plans.Count + " subworlds planned, " + enabled + " enabled, " + surface + " surface or space skipped, " + checkedBands + " band tables checked");
			t.Assert.True(enabled > 10, "most rock subworlds are enabled (" + enabled + ")");
			t.Assert.True(surface > 0, "surface and space subworlds are skipped (" + surface + ")");
			t.Assert.True(checkedBands > 10, "band tables were checked (" + checkedBands + ")");
			yield break;
		}

		[OniTest("NB: a surface or space subworld is never enabled, whatever its biomes", Timeout = 20), Layer(0)]
		public static IEnumerator SurfaceStaysOpen(TestContext t)
		{
			var options = new Options();
			foreach (var sub in new[]
			{
				Data.SubWorld(SubWorld.ZoneType.Space, null, "biomes/Sedimentary/Core"),
				Data.SubWorld(Sandstone, null, "biomes/Surface/Core"),
				Data.SubWorld(Sandstone, null, "biomes/Space"),
			})
				t.Assert.Null(Data.Plan(sub, options), "no plan for a subworld open to space");
			yield break;
		}

		[OniTest("NB: the material, coverage and pattern options change the band table and the tree", Timeout = 20), Layer(0)]
		public static IEnumerator OptionsChangeThePlan(TestContext t)
		{
			var sub = Data.SubWorld(Sandstone, null, "biomes/Sedimentary/Core");
			var normal = Data.Plan(sub, new Options());
			t.Assert.True(normal.Enabled, "enabled by default");
			t.Assert.Eq("noise/NaturalBackwallsReef", normal.Tree, "the Sandstone zone follows the reef pattern");
			var table = Data.Read(normal.Bands["biomes/Sedimentary/Core_backwall"]);
			t.Assert.Eq("SandStone", table[0].element, "filler rock of the biome");
			t.Assert.Eq(0.4f, table[0].size, "at 0.4");
			t.Assert.Eq("Vacuum", table[1].element, "the rest is no backwall");

			var custom = new Options();
			custom.Materials["Sandstone"] = "Granite";
			custom.Coverages["Sandstone"] = 0.25f;
			custom.PatternIds["Sandstone"] = "Abyss";
			var changed = Data.Plan(sub, custom);
			table = Data.Read(changed.Bands["biomes/Sedimentary/Core_backwall"]);
			t.Assert.Eq("Granite", table[0].element, "the chosen material");
			t.Assert.Eq(0.25f, table[0].size, "the chosen coverage");
			t.Assert.Eq("noise/NaturalBackwallsAbyss", changed.Tree, "the chosen pattern");

			var start = Data.Plan(sub, new Options(), null, "biomes/Sedimentary/Core");
			table = Data.Read(start.Bands["biomes/Sedimentary/Core_backwall"]);
			t.Assert.Eq(1, table.Count, "the starting biome at coverage 1 is a single band");
			t.Assert.Eq(1f, table[0].size, "of all of it");
			var halfStart = new Options { StartCoverage = 0.5f };
			table = Data.Read(Data.Plan(sub, halfStart, null, "biomes/Sedimentary/Core").Bands["biomes/Sedimentary/Core_backwall"]);
			t.Assert.Eq(0.5f, table[0].size, "the starting coverage option applies to the starting biome");

			var over = new Options { StartCoverage = 7f };
			table = Data.Read(Data.Plan(sub, over, null, "biomes/Sedimentary/Core").Bands["biomes/Sedimentary/Core_backwall"]);
			t.Assert.Eq(1f, table[0].size, "a coverage above 1 is clamped");
			yield break;
		}

		[OniTest("NB: None, a coverage of 0 or a liquid as material leave a biome with no backwall and its subworld off", Timeout = 20), Layer(0)]
		public static IEnumerator NoBackwallChoices(TestContext t)
		{
			var sub = Data.SubWorld(Sandstone, null, "biomes/Sedimentary/Core");
			foreach (var (label, apply) in new (string, System.Action<Options>)[]
			{
				("None", o => o.Materials["Sandstone"] = Options.NoMaterial),
				("coverage 0", o => o.Coverages["Sandstone"] = 0f),
				("Water", o => o.Materials["Sandstone"] = "Water"),
				("an unknown element", o => o.Materials["Sandstone"] = "NoSuchElement"),
			})
			{
				var options = new Options();
				apply(options);
				var plan = Data.Plan(sub, options);
				t.Assert.False(plan.Enabled, label + ": the subworld is not enabled");
				t.Assert.Null(plan.Tree, label + ": it gets no tree");
				var table = Data.Read(plan.Bands["biomes/Sedimentary/Core_backwall"]);
				t.Assert.Eq(1, table.Count, label + ": one band");
				t.Assert.Eq("Vacuum", table[0].element, label + ": of vacuum");
			}
			yield break;
		}

		[OniTest("NB: biomes from other mods get their commonest solid at the other-mod coverage, or nothing when the option is off", Timeout = 20), Layer(0)]
		public static IEnumerator OtherModBiomes(TestContext t)
		{
			var sub = Data.SubWorld(SubWorld.ZoneType.Forest, null, "othermod::biomes/Strange/Core");
			var terrain = new Dictionary<string, ElementBandConfiguration>
			{
				{ "othermod::biomes/Strange/Core", Data.Bands(("Water", 0.9f), ("Unobtanium", 0.8f), ("Granite", 0.3f), ("Dirt", 0.2f), ("Granite", 0.2f)) },
			};
			var on = Data.Plan(sub, new Options { OtherModCoverage = 0.3f }, terrain);
			var table = Data.Read(on.Bands["othermod::biomes/Strange/Core_backwall"]);
			t.Assert.Eq("Granite", table[0].element, "the commonest solid, not the liquid and not the indestructible one");
			t.Assert.Eq(0.3f, table[0].size, "at the other-mod coverage");
			t.Assert.True(on.Enabled, "the subworld is enabled");
			t.Assert.Eq("noise/NaturalBackwallsReef", on.Tree, "with its zone's pattern");

			var off = Data.Plan(sub, new Options { OtherModBiomes = false }, terrain);
			t.Assert.False(off.Enabled, "off: not enabled");
			t.Assert.Eq("Vacuum", Data.Read(off.Bands["othermod::biomes/Strange/Core_backwall"])[0].element, "off: no backwall");
			yield break;
		}

		[OniTest("NB: a subworld or biome that already has backwalls keeps them until the group's setting is changed", Timeout = 20), Layer(0)]
		public static IEnumerator ExistingBackwallsKept(TestContext t)
		{
			string beach = "dlc5::biomes/Beach/Core";
			var vanillaTable = Data.Bands(("Salt", 1f));
			var existing = new Dictionary<string, ElementBandConfiguration> { { beach + "_backwall", vanillaTable } };
			// The Aquatic pack's own subworld: it names its own noise tree.
			var sub = Data.SubWorld(SubWorld.ZoneType.Beach, "noise/BeachBackwall", beach);
			var untouched = Data.Plan(sub, new Options(), existing);
			t.Assert.False(untouched.Bands.ContainsKey(beach + "_backwall"), "the pack's table is kept while the settings are the pack's own");
			t.Assert.Null(untouched.Tree, "and so is its tree");
			t.Assert.False(untouched.Enabled, "the mod does not claim the subworld");

			var changed = new Options();
			changed.Materials["Beach"] = "Sand";
			var replaced = Data.Plan(sub, changed, existing);
			t.Assert.Eq("Sand", Data.Read(replaced.Bands[beach + "_backwall"])[0].element, "changing the material replaces the table");
			t.Assert.Null(replaced.Tree, "the tree stays the pack's unless a pattern is picked");

			var patterned = new Options();
			patterned.PatternIds["Beach"] = "Kelp";
			t.Assert.Eq("noise/NaturalBackwallsKelp", Data.Plan(sub, patterned, existing).Tree, "a chosen pattern replaces the pack's tree");
			yield break;
		}
	}
}
