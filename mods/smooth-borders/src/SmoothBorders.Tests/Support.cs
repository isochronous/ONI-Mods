using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using Isochronous.TestKit;
using PeterHan.PLib.Options;
using ProcGen;
using Path = System.IO.Path;
using ProcGenGame;
using TemplateClasses;
using UnityEngine;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// What the worldgen tests share: writing the mod's config, catching the mod's worldgen log lines,
	/// turning the mod's border patches off for a baseline world, reading a world's border settings,
	/// measuring a world's wall edge by edge, and listing the buildings and geysers the world got.
	/// </summary>
	internal static class Support
	{
		public const string LogTag = "[SmoothBorders] ";

		// ---- the mod's config ----

		public static Options Config(int primary, int small, int moonlets, int tinyMoonlets)
		{
			return new Options { Primary = primary, Small = small, Moonlets = moonlets, TinyMoonlets = tinyMoonlets };
		}

		/// <summary>The file PLib reads the mod's options from (mods\config\SmoothBorders\config.json).</summary>
		public static string ConfigPath => POptions.GetConfigFilePath(typeof(Options));

		/// <summary>
		/// Writes the config as the options dialog would, by hand so that a failed write throws
		/// (PLib's own writer only logs). The harness puts the player's config back at the end of the run.
		/// </summary>
		public static void WriteConfig(string json)
		{
			string path = ConfigPath;
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, json);
		}

		public static void WriteConfig(Options options)
		{
			WriteConfig("{\"Primary\":" + options.Primary + ",\"Small\":" + options.Small + ",\"Moonlets\":" + options.Moonlets
				+ ",\"TinyMoonlets\":" + options.TinyMoonlets + "}");
		}

		public static string Describe(Options options)
		{
			return "Primary " + options.Primary + ", Small " + options.Small + ", Moonlets " + options.Moonlets + ", Tiny moonlets " + options.TinyMoonlets;
		}

		// ---- the mod's log lines, caught while a world generates ----

		private static readonly object gate = new object();
		private static readonly List<string> lines = new List<string>();

		/// <summary>
		/// From now on every log line holding "[SmoothBorders] " is kept (worldgen logs from its own
		/// threads, hence the threaded event). The lines of an earlier world are dropped. The returned
		/// action stops the capture; the lines stay readable until the next capture starts.
		/// </summary>
		public static System.Action CaptureLog()
		{
			lock (gate)
				lines.Clear();
			Application.LogCallback handler = (message, stack, type) =>
			{
				if (message == null)
					return;
				int at = message.IndexOf(LogTag, StringComparison.Ordinal);
				if (at < 0)
					return;
				lock (gate)
					lines.Add(message.Substring(at + LogTag.Length).Trim());
			};
			Application.logMessageReceivedThreaded += handler;
			return () => Application.logMessageReceivedThreaded -= handler;
		}

		public static List<string> Logged()
		{
			lock (gate)
				return new List<string>(lines);
		}

		private static readonly Regex FlatLine = new Regex(@"^(.+): group (.+), flat wall (\d+) tiles$");
		private static readonly Regex GrewLine = new Regex(@"^(.+): (left|right|bottom|top) POI rectangle grew from (\d+) to (\d+) tiles$");

		/// <summary>World file path to the flat thickness the mod logged for it (the last line, should a world be generated twice).</summary>
		public static Dictionary<string, int> FlatWalls()
		{
			var walls = new Dictionary<string, int>();
			foreach (string line in Logged())
			{
				Match match = FlatLine.Match(line);
				if (match.Success)
					walls[match.Groups[1].Value] = int.Parse(match.Groups[3].Value);
			}
			return walls;
		}

		/// <summary>The "POI rectangle grew" lines: edges where the flat wall is deeper than the game's.</summary>
		public static List<string> GrewLines()
		{
			return Logged().Where(line => GrewLine.IsMatch(line)).ToList();
		}

		// ---- Prepare helpers ----

		/// <summary>Writes the config and catches the mod's log lines for the world about to be generated.</summary>
		public static System.Action Configure(Options options)
		{
			WriteConfig(options);
			Debug.Log("[SmoothBorders.Tests] config written: " + Describe(options));
			return CaptureLog();
		}

		private static MethodBase DrawWorldBorder => AccessTools.Method(typeof(WorldGen), "DrawWorldBorder");

		/// <summary>The Harmony ids that hold the mod's DrawWorldBorder prefix, postfix and transpiler.</summary>
		private static List<string> ModPatchOwners()
		{
			Patches info = Harmony.GetPatchInfo(DrawWorldBorder);
			var owners = new List<string>();
			if (info == null)
				return owners;
			foreach (Patch patch in info.Prefixes.Concat(info.Postfixes).Concat(info.Transpilers))
				if (patch.PatchMethod.DeclaringType == typeof(WorldGen_DrawWorldBorder_Patch) && !owners.Contains(patch.owner))
					owners.Add(patch.owner);
			return owners;
		}

		/// <summary>
		/// The next world is drawn by the unmodified game: the mod's DrawWorldBorder patches come off
		/// (the mod stays loaded). The returned action stops the log capture and puts the patches back,
		/// so the transpiler runs again on the original method.
		/// </summary>
		public static System.Action VanillaDraw()
		{
			List<string> owners = ModPatchOwners();
			if (owners.Count != 1)
				throw new InvalidOperationException("expected the mod's DrawWorldBorder patches under one Harmony id, found " + owners.Count);
			var harmony = new Harmony(owners[0]);
			harmony.Unpatch(DrawWorldBorder, HarmonyPatchType.All, owners[0]);
			if (ModPatchOwners().Count != 0)
				throw new InvalidOperationException("the mod's DrawWorldBorder patches are still on after the unpatch");
			Debug.Log("[SmoothBorders.Tests] the mod's DrawWorldBorder patches are off for a baseline world");
			System.Action stop = CaptureLog();
			return () =>
			{
				stop();
				harmony.CreateClassProcessor(typeof(WorldGen_DrawWorldBorder_Patch)).Patch();
				Debug.Log("[SmoothBorders.Tests] the mod's DrawWorldBorder patches are back on (transpiler matched: " + Border.Patched + ")");
			};
		}

		// ---- a world's border settings, as WorldGenSettings reads them ----

		/// <summary>A world's setting: its own defaultsOverrides first, then worldgen/defaults.yaml, as WorldGenSettings.GetSetting looks.</summary>
		private static object RawSetting(ProcGen.World world, string key)
		{
			Dictionary<string, object> own = world?.defaultsOverrides?.data;
			if (own != null && own.TryGetValue(key, out object value))
				return value;
			Dictionary<string, object> defaults = SettingsCache.defaults?.data;
			if (defaults != null && defaults.TryGetValue(key, out value))
				return value;
			return null;
		}

		public static int IntSetting(ProcGen.World world, string key)
		{
			object value = RawSetting(world, key);
			if (value is int number)
				return number;
			return value != null && int.TryParse(value.ToString(), out number) ? number : 0;
		}

		public static bool BoolSetting(ProcGen.World world, string key)
		{
			object value = RawSetting(world, key);
			if (value is bool flag)
				return flag;
			return value != null && bool.TryParse(value.ToString(), out flag) && flag;
		}

		/// <summary>One world's border pair and whether the game draws a border on it at all.</summary>
		public sealed class BorderPair
		{
			public string World;
			public int T, R;
			public bool Drawn;

			public override string ToString()
			{
				return World + " (" + T + ", " + R + ")" + (Drawn ? "" : ", no border drawn");
			}
		}

		public static BorderPair PairOf(ProcGen.World world, string name)
		{
			return new BorderPair
			{
				World = name,
				T = IntSetting(world, Border.ThicknessKey),
				R = IntSetting(world, Border.RangeKey),
				Drawn = BoolSetting(world, "DrawWorldBorder")
			};
		}

		/// <summary>The pair of an asteroid of the loaded cluster, from the world file it was generated from.</summary>
		public static BorderPair PairOf(WorldContainer world)
		{
			ProcGen.World data = SettingsCache.worlds.GetWorldData(world.worldName);
			if (data == null)
				throw new OniAssertException("no world data for " + world.worldName + " in SettingsCache.worlds");
			return PairOf(data, world.worldName);
		}

		/// <summary>The asteroids of the loaded cluster (rocket interiors left out).</summary>
		public static List<WorldContainer> Asteroids()
		{
			return ClusterManager.Instance.WorldContainers.Where(world => world != null && !world.IsModuleInterior).ToList();
		}

		// ---- measuring the wall ----

		/// <summary>What one edge of a world looks like against a flat wall of thickness F.</summary>
		public sealed class Edge
		{
			public string Name;
			/// <summary>Rows (left, right) or columns (bottom, top) whose cell at depth F was looked at.</summary>
			public int Lines;
			/// <summary>Cells at depth below F that are not neutronium, outside Space zones (where the game skips vacuum).</summary>
			public readonly List<int> Gaps = new List<int>();
			/// <summary>Cells at depth F that are neutronium: the wall is deeper than F there.</summary>
			public readonly List<int> Deep = new List<int>();

			/// <summary>Neutronium at depth F a flat wall still tolerates: the game puts a little elsewhere too (biome bands, templates).</summary>
			public int DeepAllowed => Math.Max(2, Lines / 50);

			public override string ToString()
			{
				return Name + " " + Gaps.Count + " gap(s), " + Deep.Count + "/" + Lines + " deeper (" + DeepAllowed + " allowed)";
			}
		}

		private static bool IsNeutronium(int cell)
		{
			Element element = Grid.Element[cell];
			return element != null && element.id == SimHashes.Unobtanium;
		}

		private static bool InSpace(int cell)
		{
			SubWorld.ZoneType[] zones = global::World.Instance.zoneRenderData.worldZoneTypes;
			return cell < zones.Length && zones[cell] == SubWorld.ZoneType.Space;
		}

		/// <summary>
		/// The world's four edges measured against a flat wall F deep: every cell nearer the edge than F
		/// should be neutronium (the game leaves vacuum in Space zones unwalled), and the cell at depth F
		/// should not, away from the corners where the other edges' walls run.
		/// </summary>
		public static Edge[] Measure(WorldContainer world, int f)
		{
			int x0 = world.WorldOffset.x, y0 = world.WorldOffset.y, w = world.WorldSize.x, h = world.WorldSize.y;
			var left = new Edge { Name = "left" };
			var right = new Edge { Name = "right" };
			var bottom = new Edge { Name = "bottom" };
			var top = new Edge { Name = "top" };
			for (int y = 0; y < h; y++)
			{
				bool corner = y < f || y >= h - f;
				Look(left, d => Grid.XYToCell(x0 + d, y0 + y), f, corner);
				Look(right, d => Grid.XYToCell(x0 + w - 1 - d, y0 + y), f, corner);
			}
			for (int x = 0; x < w; x++)
			{
				bool corner = x < f || x >= w - f;
				Look(bottom, d => Grid.XYToCell(x0 + x, y0 + d), f, corner);
				Look(top, d => Grid.XYToCell(x0 + x, y0 + h - 1 - d), f, corner);
			}
			return new[] { left, right, bottom, top };
		}

		private static void Look(Edge edge, Func<int, int> cellAtDepth, int f, bool corner)
		{
			for (int d = 0; d < f; d++)
			{
				int cell = cellAtDepth(d);
				if (Grid.IsValidCell(cell) && !IsNeutronium(cell) && !InSpace(cell))
					edge.Gaps.Add(cell);
			}
			if (corner)
				return;
			int beyond = cellAtDepth(f);
			if (!Grid.IsValidCell(beyond))
				return;
			edge.Lines++;
			if (IsNeutronium(beyond))
				edge.Deep.Add(beyond);
		}

		/// <summary>The fog lifted around the cell and the camera on it (switching to its asteroid), then a screenshot.</summary>
		public static IEnumerator Photograph(TestContext t, int cell, string name)
		{
			int worldId = Grid.WorldIdx[cell];
			if (ClusterManager.Instance.activeWorldId != worldId)
			{
				ClusterManager.Instance.SetActiveWorld(worldId);
				yield return t.Frames(5);
			}
			Grid.CellToXY(cell, out int cx, out int cy);
			for (int y = cy - 12; y <= cy + 12; y++)
				for (int x = cx - 20; x <= cx + 20; x++)
				{
					int around = Grid.XYToCell(x, y);
					if (Grid.IsValidCell(around) && Grid.WorldIdx[around] == worldId)
						Grid.Reveal(around);
				}
			yield return t.WaitRealSeconds(0.5);
			yield return t.LookAt(cell, 10f);
			yield return t.Screenshot(name);
		}

		/// <summary>
		/// The check behind every flatness test: the world was generated without errors; every asteroid
		/// with a border got a log line from the mod with the thickness the mod's own mapping gives for
		/// the config the test wrote, and measures flat at that thickness on all four edges; the groups
		/// the cluster is known to hold all came up. Problem spots are photographed before the test fails.
		/// </summary>
		public static IEnumerator ExpectFlat(TestContext t, Options options, params Group[] groupsExpected)
		{
			t.Assert.NotNull(t.Generated, "the test runs in a generated world");
			t.Assert.Eq(0, t.Generated.Errors.Count, "the worldgen reported no errors: " + string.Join(" | ", t.Generated.Errors));
			t.Log("generated " + t.Generated.Applied + " in " + t.Generated.Seconds.ToString("F0") + " s with " + Describe(options));
			Dictionary<string, int> logged = FlatWalls();
			var problems = new List<string>();
			var spots = new List<KeyValuePair<string, int>>();
			var groupsSeen = new HashSet<string>();
			int measured = 0;
			foreach (WorldContainer world in Asteroids())
			{
				BorderPair pair = PairOf(world);
				if (!pair.Drawn || pair.T <= 0)
				{
					t.Log(pair + ": no wall to measure");
					continue;
				}
				int f = Groups.Thickness(pair.T, pair.R, options);
				Group group = Groups.ForWorld(pair.T, pair.R);
				if (group != null)
					groupsSeen.Add(group.Id);
				if (!logged.TryGetValue(world.worldName, out int drawn))
					problems.Add(world.worldName + ": the mod logged no flat wall for it");
				else if (drawn != f)
					problems.Add(world.worldName + ": the mod drew " + drawn + " tiles, the config gives " + f);
				Edge[] edges = Measure(world, f);
				measured++;
				t.Log(pair + ", " + (group != null ? group.Id : "no group") + ", F = " + f + ": " + string.Join("; ", edges.Select(e => e.ToString())));
				foreach (Edge edge in edges)
				{
					if (edge.Gaps.Count > 0)
					{
						problems.Add(world.worldName + " " + edge.Name + ": " + edge.Gaps.Count + " cell(s) nearer the edge than " + f + " are not neutronium");
						spots.Add(new KeyValuePair<string, int>(Short(world.worldName) + "-" + edge.Name + "-gap", edge.Gaps[0]));
					}
					if (edge.Deep.Count > edge.DeepAllowed)
					{
						problems.Add(world.worldName + " " + edge.Name + ": neutronium at depth " + f + " on " + edge.Deep.Count + " of " + edge.Lines + " lines");
						spots.Add(new KeyValuePair<string, int>(Short(world.worldName) + "-" + edge.Name + "-deep", edge.Deep[0]));
					}
				}
			}
			foreach (Group group in groupsExpected)
				if (!groupsSeen.Contains(group.Id))
					problems.Add("no asteroid of the group " + group.Id + " in the cluster");
			foreach (var spot in spots.Take(4))
				yield return Photograph(t, spot.Value, spot.Key);
			t.Assert.True(measured > 0, "the cluster has asteroids with a wall");
			t.Assert.Eq(0, problems.Count, "every wall is flat at the configured thickness: " + string.Join(" | ", problems));
		}

		public static string Short(string worldName)
		{
			int slash = worldName.LastIndexOf('/');
			return slash >= 0 ? worldName.Substring(slash + 1) : worldName;
		}

		// ---- placements: buildings and geysers the world got ----

		/// <summary>
		/// Every building and geyser worldgen placed in the loaded cluster, as "world id x y" lines,
		/// sorted: the printing pod, POI and story-trait buildings, geysers. Critters and plants are
		/// left out (their placement reads the cells, which the wall changes by design).
		/// </summary>
		public static List<string> Placements()
		{
			var placements = new List<string>();
			foreach (WorldGenSpawner.Spawnable spawnable in SaveGame.Instance.worldGenSpawner.GetSpawnables())
			{
				Prefab prefab = spawnable.spawnInfo;
				if (prefab == null || !IsBuildingOrGeyser(prefab))
					continue;
				int cell = Grid.XYToCell(prefab.location_x, prefab.location_y);
				WorldContainer world = Grid.IsValidCell(cell) ? ClusterManager.Instance.GetWorld(Grid.WorldIdx[cell]) : null;
				placements.Add((world != null ? Short(world.worldName) : "?") + " " + prefab.id + " " + prefab.location_x + " " + prefab.location_y);
			}
			placements.Sort(string.CompareOrdinal);
			return placements;
		}

		private static bool IsBuildingOrGeyser(Prefab prefab)
		{
			if (prefab.type == Prefab.Type.Building)
				return true;
			if (prefab.id != null && prefab.id.StartsWith("GeyserGeneric", StringComparison.Ordinal))
				return true;
			GameObject template = prefab.id != null ? Assets.TryGetPrefab(new Tag(prefab.id)) : null;
			return template != null && template.GetComponent<Geyser>() != null;
		}

		/// <summary>Where a world's placements are kept for a later test of the same run: the run folder.</summary>
		public static string PlacementFile(string coordinate, string what)
		{
			return Path.Combine(Isochronous.TestRunner.Session.Dir, "placements-" + what + "-" + coordinate + ".txt");
		}

		/// <summary>
		/// The test of a baseline world: the mod drew nothing (no log line from it), the game's own wall
		/// is uneven (the flatness measure at the defaults finds it deeper on some edge, so the measure
		/// can tell), and its buildings and geysers are written for the comparison test.
		/// </summary>
		public static IEnumerator RecordBaseline(TestContext t, string coordinate, string what)
		{
			t.Assert.NotNull(t.Generated, "the test runs in a generated world");
			t.Assert.Eq(0, t.Generated.Errors.Count, "the worldgen reported no errors: " + string.Join(" | ", t.Generated.Errors));
			t.Assert.Eq(0, FlatWalls().Count, "the mod drew no flat wall in the baseline world (" + string.Join(" | ", Logged()) + ")");
			var defaults = new Options();
			int uneven = 0;
			foreach (WorldContainer world in Asteroids())
			{
				BorderPair pair = PairOf(world);
				if (!pair.Drawn || pair.T <= 0)
					continue;
				Edge[] edges = Measure(world, Groups.Thickness(pair.T, pair.R, defaults));
				t.Log("game's wall, " + pair + ": " + string.Join("; ", edges.Select(e => e.ToString())));
				uneven += edges.Count(e => e.Deep.Count > e.DeepAllowed);
			}
			List<string> placements = Placements();
			File.WriteAllLines(PlacementFile(coordinate, what), placements);
			t.Log(placements.Count + " buildings and geysers written to " + PlacementFile(coordinate, what));
			t.Assert.True(uneven > 0, "the flatness measure finds the game's own wall deeper than the default on some edge");
			t.Assert.True(placements.Count > 0, "the baseline world has buildings and geysers to compare");
			yield break;
		}

		/// <summary>
		/// The comparison test: this world's buildings and geysers are exactly the baseline's (written
		/// earlier in the run by the baseline test of the same coordinate).
		/// </summary>
		public static IEnumerator ExpectSamePlacements(TestContext t, string coordinate, string baseline)
		{
			string file = PlacementFile(coordinate, baseline);
			t.Assert.True(File.Exists(file), "the baseline placements of " + coordinate + " were written earlier in this run (" + file + "); run the baseline test with this one");
			List<string> expected = File.ReadAllLines(file).ToList();
			List<string> actual = Placements();
			File.WriteAllLines(Path.ChangeExtension(file, null) + "-compared.txt", actual);
			List<string> missing = expected.Except(actual).ToList();
			List<string> extra = actual.Except(expected).ToList();
			t.Log(actual.Count + " buildings and geysers here, " + expected.Count + " in the baseline; " + missing.Count + " missing, " + extra.Count + " extra");
			t.Assert.True(missing.Count == 0 && extra.Count == 0 && actual.Count == expected.Count,
				"the buildings and geysers sit where the baseline has them; missing: " + string.Join(", ", missing.Take(5))
				+ "; extra: " + string.Join(", ", extra.Take(5)));
			yield break;
		}
	}
}
