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

		// The game's Debug.Log writes to the console, which Unity sends to Player.log; it never reaches
		// Application.logMessageReceived. So the capture is a stretch of Player.log: from where the file
		// ended when the capture began to where it ended when the capture stopped (or its end now).
		private static string logPath;
		private static long captureStart = -1;
		private static long captureEnd = -1;

		private static string LogPath
		{
			get
			{
				string path = Application.consoleLogPath;
				return string.IsNullOrEmpty(path) ? Path.Combine(Application.persistentDataPath, "Player.log") : path;
			}
		}

		private static long LogLength(string path)
		{
			var info = new FileInfo(path);
			return info.Exists ? info.Length : 0;
		}

		/// <summary>
		/// From now on every Player.log line holding "[SmoothBorders] " belongs to this capture (worldgen
		/// logs from its own threads into the same file). The lines of an earlier world are dropped. The
		/// returned action stops the capture; its lines stay readable until the next capture starts.
		/// </summary>
		public static System.Action CaptureLog()
		{
			logPath = LogPath;
			captureStart = LogLength(logPath);
			captureEnd = -1;
			return () => captureEnd = LogLength(logPath);
		}

		public static List<string> Logged()
		{
			var found = new List<string>();
			if (logPath == null || captureStart < 0)
				return found;
			using (var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			{
				long end = captureEnd >= 0 ? captureEnd : stream.Length;
				if (end <= captureStart)
					return found;
				stream.Seek(captureStart, SeekOrigin.Begin);
				var buffer = new byte[end - captureStart];
				int read = 0;
				while (read < buffer.Length)
				{
					int n = stream.Read(buffer, read, buffer.Length - read);
					if (n <= 0)
						break;
					read += n;
				}
				foreach (string line in System.Text.Encoding.UTF8.GetString(buffer, 0, read).Split('\n'))
				{
					int at = line.IndexOf(LogTag, StringComparison.Ordinal);
					if (at >= 0)
						found.Add(line.Substring(at + LogTag.Length).Trim());
				}
			}
			return found;
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
			/// <summary>Cells at depth F that are neutronium outside a magma zone: the wall is deeper than F there.</summary>
			public readonly List<int> Deep = new List<int>();
			/// <summary>
			/// Cells at depth F that are neutronium in a magma zone. The magma bed at the bottom of an
			/// asteroid has neutronium of its own, which meets the wall in places; counted apart.
			/// </summary>
			public readonly List<int> DeepInMagma = new List<int>();

			/// <summary>Neutronium at depth F a flat wall still tolerates outside magma (templates, other biome bands).</summary>
			public int DeepAllowed => Math.Max(2, Lines / 50);

			/// <summary>
			/// Neutronium at depth F tolerated in a magma zone: a quarter of the lines. The game's own wall
			/// there reaches a third and more; the magma bed's neutronium alone reached a fifth on the
			/// bottom of a 64-wide moonlet (MarshyMoonlet, KF23-C-7, run sb-so-mini-20261019-133502).
			/// </summary>
			public int MagmaAllowed => Math.Max(2, Lines / 4);

			public bool TooDeep => Deep.Count > DeepAllowed || DeepInMagma.Count > MagmaAllowed;

			public override string ToString()
			{
				return Name + " " + Gaps.Count + " gap(s), " + Deep.Count + "/" + Lines + " deeper (" + DeepAllowed + " allowed), "
					+ DeepInMagma.Count + " in magma (" + MagmaAllowed + " allowed)";
			}
		}

		private static bool InMagma(int cell)
		{
			SubWorld.ZoneType[] zones = global::World.Instance.zoneRenderData.worldZoneTypes;
			return cell < zones.Length && zones[cell] == SubWorld.ZoneType.MagmaCore;
		}

		/// <summary>The zone types of the first few cells, for a failure message.</summary>
		public static string Zones(IEnumerable<int> cells)
		{
			SubWorld.ZoneType[] zones = global::World.Instance.zoneRenderData.worldZoneTypes;
			return string.Join(", ", cells.Take(5).Select(cell => cell < zones.Length ? zones[cell].ToString() : "?"));
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
				(InMagma(beyond) ? edge.DeepInMagma : edge.Deep).Add(beyond);
		}

		/// <summary>
		/// The cells of the templates the asteroid's world file places at a fixed spot
		/// (overridePlacement): the game stamps them there without looking at the border. Every name
		/// a rule lists is counted, so a rule that picks one of several excuses all of them.
		/// </summary>
		public static HashSet<int> FixedTemplateCells(WorldContainer world)
		{
			var cells = new HashSet<int>();
			ProcGen.World data = SettingsCache.worlds.GetWorldData(world.worldName);
			if (data?.worldTemplateRules == null)
				return cells;
			foreach (ProcGen.World.TemplateSpawnRules rule in data.worldTemplateRules)
			{
				if (rule.overridePlacement == Vector2I.minusone || rule.names == null)
					continue;
				foreach (string name in rule.names)
				{
					TemplateContainer template = TemplateCache.GetTemplate(name);
					if (template?.cells == null)
						continue;
					foreach (Cell c in template.cells)
					{
						int cell = Grid.XYToCell(world.WorldOffset.x + rule.overridePlacement.x + c.location_x, world.WorldOffset.y + rule.overridePlacement.y + c.location_y);
						if (Grid.IsValidCell(cell) && Grid.WorldIdx[cell] == world.id)
							cells.Add(cell);
					}
				}
			}
			return cells;
		}

		private static bool IsVacuum(int cell)
		{
			Element element = Grid.Element[cell];
			return element != null && (element.id == SimHashes.Vacuum || element.id == SimHashes.Void);
		}

		/// <summary>
		/// Whether the game skips vacuum on this cell's line: the line's point on the edge lies in a
		/// terrain cell tagged RemoveWorldBorderOverVacuum (the saved overworld cells keep the tags).
		/// </summary>
		public static bool EdgePointSkipsVacuum(WorldContainer world, string edge, int cell)
		{
			Grid.CellToXY(cell, out int x, out int y);
			int x0 = world.WorldOffset.x, y0 = world.WorldOffset.y;
			Vector2 point;
			switch (edge)
			{
				case "left": point = new Vector2(x0, y); break;
				case "right": point = new Vector2(x0 + world.WorldSize.x - 1, y); break;
				case "bottom": point = new Vector2(x, y0); break;
				default: point = new Vector2(x, y0 + world.WorldSize.y - 1); break;
			}
			List<Klei.WorldDetailSave.OverworldCell> cells = SaveLoader.Instance?.clusterDetailSave?.overworldCells;
			if (cells == null)
				return false;
			foreach (Klei.WorldDetailSave.OverworldCell overworld in cells)
				if (overworld.tags != null && overworld.tags.Contains(WorldGenTags.RemoveWorldBorderOverVacuum) && overworld.poly.Contains(point))
					return true;
			return false;
		}

		/// <summary>How far a cell is from one edge of its asteroid (0 is the outermost line).</summary>
		public static int DepthOf(WorldContainer world, string edge, int cell)
		{
			Grid.CellToXY(cell, out int x, out int y);
			x -= world.WorldOffset.x;
			y -= world.WorldOffset.y;
			switch (edge)
			{
				case "left": return x;
				case "right": return world.WorldSize.x - 1 - x;
				case "bottom": return y;
				default: return world.WorldSize.y - 1 - y;
			}
		}

		/// <summary>"(x, y) Element" with x and y from the asteroid's corner.</summary>
		public static string Describe(WorldContainer world, int cell)
		{
			Grid.CellToXY(cell, out int x, out int y);
			Element element = Grid.Element[cell];
			return "(" + (x - world.WorldOffset.x) + ", " + (y - world.WorldOffset.y) + ") " + (element != null ? element.id.ToString() : "?");
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
				HashSet<int> fixedTemplates = FixedTemplateCells(world);
				foreach (Edge edge in edges)
				{
					// A template the world file places at a fixed spot (overridePlacement) is stamped
					// wherever it lands, border or not, as in the game without the mod; its cells are
					// the game's, not gaps in the wall. Said in a note, with the shallowest depth hit.
					List<int> stamped = edge.Gaps.Where(fixedTemplates.Contains).ToList();
					if (stamped.Count > 0)
					{
						edge.Gaps.RemoveAll(fixedTemplates.Contains);
						t.Log(world.worldName + " " + edge.Name + ": " + stamped.Count + " cell(s) nearer the edge than " + f + " belong to a template placed at a fixed spot (overridePlacement), "
							+ "the nearest " + stamped.Min(cell => DepthOf(world, edge.Name, cell)) + " from the edge: " + string.Join(", ", stamped.Take(6).Select(cell => Describe(world, cell))));
					}
					// The game leaves vacuum unwalled on a line whose edge point lies in terrain tagged
					// RemoveWorldBorderOverVacuum (DrawWorldBorder), whatever the zone; so does the mod.
					List<int> skipped = edge.Gaps.Where(cell => IsVacuum(cell) && EdgePointSkipsVacuum(world, edge.Name, cell)).ToList();
					if (skipped.Count > 0)
					{
						edge.Gaps.RemoveAll(skipped.Contains);
						t.Log(world.worldName + " " + edge.Name + ": " + skipped.Count + " vacuum cell(s) nearer the edge than " + f + " on lines the game leaves unwalled over vacuum (RemoveWorldBorderOverVacuum): "
							+ string.Join(", ", skipped.Take(6).Select(cell => Describe(world, cell))));
					}
					if (edge.Gaps.Count > 0)
					{
						problems.Add(world.worldName + " " + edge.Name + ": " + edge.Gaps.Count + " cell(s) nearer the edge than " + f + " are not neutronium (" + string.Join(", ", edge.Gaps.Take(4).Select(cell => Describe(world, cell))) + ")");
						spots.Add(new KeyValuePair<string, int>(Short(world.worldName) + "-" + edge.Name + "-gap", edge.Gaps[0]));
					}
					if (edge.TooDeep)
					{
						List<int> deep = edge.Deep.Concat(edge.DeepInMagma).ToList();
						problems.Add(world.worldName + " " + edge.Name + ": neutronium at depth " + f + " on " + edge.Deep.Count + " of " + edge.Lines
							+ " lines outside magma and " + edge.DeepInMagma.Count + " in magma (zones: " + Zones(deep) + ")");
						spots.Add(new KeyValuePair<string, int>(Short(world.worldName) + "-" + edge.Name + "-deep", deep[0]));
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
				uneven += edges.Count(e => e.TooDeep);
			}
			List<string> placements = Placements();
			File.WriteAllLines(PlacementFile(coordinate, what), placements);
			t.Log(placements.Count + " buildings and geysers written to " + PlacementFile(coordinate, what));
			List<string> inner = InnerNeutronium(defaults);
			File.WriteAllLines(NeutroniumFile(coordinate, what), inner);
			t.Log(inner.Count + " neutronium cells inside the border strips written to " + NeutroniumFile(coordinate, what));
			t.Assert.True(uneven > 0, "the flatness measure finds the game's own wall deeper than the default on some edge");
			t.Assert.True(placements.Count > 0, "the baseline world has buildings and geysers to compare");
			yield break;
		}

		// ---- neutronium away from the border ----

		/// <summary>
		/// How deep an asteroid's border strip reaches: the game's wall is T - R to T + R deep on every
		/// edge, the mod's is F; the strip is the deeper of the two. Cells deeper than that are the
		/// asteroid's inside, where neither draws a wall.
		/// </summary>
		public static int StripDepth(BorderPair pair, Options options)
		{
			if (!pair.Drawn || pair.T <= 0)
				return 0;
			return Math.Max(Groups.Thickness(pair.T, pair.R, options), pair.T + pair.R);
		}

		/// <summary>
		/// Every neutronium cell of the loaded cluster inside the border strips (the strip as deep as
		/// StripDepth gives for these options), as "world x y" lines with x and y counted from the
		/// asteroid's corner, sorted. The magma beds' own neutronium and any a template brings.
		/// </summary>
		public static List<string> InnerNeutronium(Options options)
		{
			var cells = new List<string>();
			foreach (WorldContainer world in Asteroids())
			{
				int s = StripDepth(PairOf(world), options);
				int x0 = world.WorldOffset.x, y0 = world.WorldOffset.y, w = world.WorldSize.x, h = world.WorldSize.y;
				for (int y = s; y < h - s; y++)
					for (int x = s; x < w - s; x++)
					{
						int cell = Grid.XYToCell(x0 + x, y0 + y);
						if (Grid.IsValidCell(cell) && IsNeutronium(cell))
							cells.Add(Short(world.worldName) + " " + x + " " + y);
					}
			}
			cells.Sort(string.CompareOrdinal);
			return cells;
		}

		public static string NeutroniumFile(string coordinate, string what)
		{
			return Path.Combine(Isochronous.TestRunner.Session.Dir, "neutronium-" + what + "-" + coordinate + ".txt");
		}

		/// <summary>
		/// No neutronium the baseline world has inside the border strips is gone from this world: the
		/// mod changed the wall and nothing else. The strip is as deep as these options make it, so
		/// the baseline's cells are compared only where both worlds are inside. With exact, no
		/// neutronium was added there either (the defaults, where the placements are the baseline's).
		/// </summary>
		public static IEnumerator ExpectInnerNeutroniumKept(TestContext t, string coordinate, string baseline, Options options, bool exact, bool assertKept = true)
		{
			string file = NeutroniumFile(coordinate, baseline);
			t.Assert.True(File.Exists(file), "the baseline neutronium of " + coordinate + " was written earlier in this run (" + file + "); run the baseline test with this one");
			var strips = new Dictionary<string, KeyValuePair<int, Vector2I>>();
			foreach (WorldContainer world in Asteroids())
				strips[Short(world.worldName)] = new KeyValuePair<int, Vector2I>(StripDepth(PairOf(world), options), world.WorldSize);
			var expected = new List<string>();
			foreach (string line in File.ReadAllLines(file))
			{
				string[] parts = line.Split(' ');
				if (parts.Length != 3 || !strips.TryGetValue(parts[0], out var strip))
				{
					expected.Add(line);
					continue;
				}
				int x = int.Parse(parts[1]), y = int.Parse(parts[2]), s = strip.Key;
				if (x >= s && x < strip.Value.x - s && y >= s && y < strip.Value.y - s)
					expected.Add(line);
			}
			List<string> actual = InnerNeutronium(options);
			File.WriteAllLines(Path.ChangeExtension(file, null) + "-compared-" + options.Primary + "-" + options.Small + "-" + options.Moonlets + "-" + options.TinyMoonlets + ".txt", actual);
			var actualSet = new HashSet<string>(actual);
			var expectedSet = new HashSet<string>(expected);
			List<string> removed = expected.Where(line => !actualSet.Contains(line)).ToList();
			List<string> added = actual.Where(line => !expectedSet.Contains(line)).ToList();
			t.Log(actual.Count + " neutronium cells inside the strips here, " + expected.Count + " in the baseline; " + removed.Count + " gone, " + added.Count + " new"
				+ (removed.Count > 0 ? " (gone: " + string.Join(", ", removed.Take(5)) + ")" : "")
				+ (added.Count > 0 ? " (new: " + string.Join(", ", added.Take(5)) + ")" : ""));
			if (!assertKept)
				yield break;
			t.Assert.True(removed.Count == 0, "no neutronium inside the border strips is gone: " + string.Join(", ", removed.Take(8)));
			if (exact)
				t.Assert.True(added.Count == 0, "no neutronium inside the border strips is new: " + string.Join(", ", added.Take(8)));
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
