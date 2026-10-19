using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isochronous.TestKit;
using ProcGen;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// Which kind of asteroid a world is, from the two border settings the game gives it, checked
	/// against the design's table over every world the game has loaded; the rule for pairs outside
	/// the table; and the config limited to each kind's range when it is read. No colony needed.
	/// </summary>
	public static class AsteroidKinds
	{
		/// <summary>The design's table (WorldBorderThickness, WorldBorderRange) to kind; null: no wall.</summary>
		private static readonly Dictionary<(int t, int r), Group> Table = new Dictionary<(int, int), Group>
		{
			{ (10, 6), Groups.Primary },
			{ (5, 2), Groups.Small },
			{ (4, 1), Groups.Small },
			{ (4, 2), Groups.Moonlets },
			{ (3, 1), Groups.Moonlets },
			{ (2, 1), Groups.TinyMoonlets },
			{ (1, 0), Groups.TinyMoonlets },
			{ (0, 0), null },
		};

		/// <summary>Every world the game has loaded that gets a border drawn, with its pair.</summary>
		private static List<Support.BorderPair> LoadedWorlds()
		{
			ProcGenGame.WorldGen.WaitForPendingLoadSettings();
			return SettingsCache.worlds.worldCache
				.Select(entry => Support.PairOf(entry.Value, entry.Key))
				.Where(pair => pair.Drawn)
				.OrderBy(pair => pair.World)
				.ToList();
		}

		[OniTest("SB: every world the game loads has a border pair from the design's table and falls in that table's kind", NoColony = true), Layer(0)]
		public static IEnumerator EveryWorldInTheTable(TestContext t)
		{
			List<Support.BorderPair> worlds = LoadedWorlds();
			var wrong = new List<string>();
			foreach (Support.BorderPair pair in worlds)
			{
				if (!Table.TryGetValue((pair.T, pair.R), out Group expected))
				{
					wrong.Add(pair + ": not in the table");
					continue;
				}
				Group actual = Groups.ForWorld(pair.T, pair.R);
				if (actual != expected)
					wrong.Add(pair + ": " + (actual != null ? actual.Id : "no kind") + ", the table says " + (expected != null ? expected.Id : "no kind"));
			}
			foreach (var kind in worlds.GroupBy(pair => (pair.T, pair.R)).OrderBy(g => g.Key))
				t.Log("(" + kind.Key.T + ", " + kind.Key.R + "): " + kind.Count() + " world(s), e.g. " + Support.Short(kind.First().World));
			t.Assert.True(worlds.Count > 0, "the game has worlds loaded");
			t.Assert.Eq(0, wrong.Count, "every world maps per the table: " + string.Join(" | ", wrong));
			yield break;
		}

		[OniTest("SB: at the defaults every world's flat wall is the thinnest wall the game draws there", NoColony = true), Layer(0)]
		public static IEnumerator DefaultsAreTheGamesThinnest(TestContext t)
		{
			var defaults = new Options();
			var wrong = new List<string>();
			List<Support.BorderPair> worlds = LoadedWorlds();
			foreach (Support.BorderPair pair in worlds)
			{
				int flat = Groups.Thickness(pair.T, pair.R, defaults);
				int thinnest = System.Math.Max(0, pair.T - pair.R);
				if (flat != thinnest)
					wrong.Add(pair + ": " + flat + " tiles, the game's thinnest is " + thinnest);
			}
			t.Assert.True(worlds.Count > 0, "the game has worlds loaded");
			t.Assert.Eq(0, wrong.Count, "each default equals T - R: " + string.Join(" | ", wrong));
			yield break;
		}

		[OniTest("SB: a border pair outside the table gets its own thinnest wall, at least 1, and a world with no wall keeps none", NoColony = true), Layer(0)]
		public static IEnumerator PairsOutsideTheTable(TestContext t)
		{
			var options = Support.Config(16, 7, 6, 3);
			t.Assert.Null(Groups.ForWorld(12, 3), "(12, 3) is no kind");
			t.Assert.Eq(9, Groups.Thickness(12, 3, options), "(12, 3) is flattened at 12 - 3, whatever the settings");
			t.Assert.Eq(1, Groups.Thickness(2, 5, options), "(2, 5) keeps a wall of 1");
			t.Assert.Eq(0, Groups.Thickness(0, 0, options), "(0, 0) has no wall");
			t.Assert.Eq(0, Groups.Thickness(-1, 0, options), "a negative thickness has no wall");
			t.Assert.Eq(16, Groups.Thickness(10, 6, options), "(10, 6) takes the Primary setting");
			yield break;
		}

		[OniTest("SB: values out of range in the config file are limited to each kind's range when read, values in range kept", NoColony = true), Layer(0)]
		public static IEnumerator ConfigLimitedOnRead(TestContext t)
		{
			Support.WriteConfig("{\"Primary\":99,\"Small\":0,\"Moonlets\":-5,\"TinyMoonlets\":2}");
			t.Log("wrote " + File.ReadAllText(Support.ConfigPath) + " to " + Support.ConfigPath);
			Options read = Options.Load();
			t.Assert.Eq(16, read.Primary, "Primary 99 reads as 16");
			t.Assert.Eq(1, read.Small, "Small 0 reads as 1");
			t.Assert.Eq(1, read.Moonlets, "Moonlets -5 reads as 1");
			t.Assert.Eq(2, read.TinyMoonlets, "Tiny moonlets 2 stays 2");
			yield break;
		}
	}
}
