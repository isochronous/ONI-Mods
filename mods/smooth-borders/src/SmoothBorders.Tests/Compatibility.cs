using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isochronous.TestKit;
using Path = System.IO.Path;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// Smooth Borders next to five Workshop worldgen mods, one run per mod with the launcher's
	/// -Steam id (Explicit: the default suite needs no Workshop mod). For each cluster: a world with
	/// the mod's border patches off (the worldgen mod alone) is written down; the same coordinate
	/// with Smooth Borders at its defaults is flat where the game's DrawWorldBorder walled it, an
	/// asteroid the worldgen mod builds itself is cell for cell as the mod alone made it, buildings,
	/// geysers and inner neutronium are the mod-alone world's, and the colony plays without an
	/// error; at every kind's maximum it is flat again and the mod's own asteroids still untouched.
	/// Filters: '*Compatibility.MiniBase*' with -Steam 3382061617, '*Compatibility.Empty*' with
	/// 1825146346, '*Compatibility.Voidria*' with 3430682737, '*Compatibility.Baator*' with
	/// 2582905897, '*Compatibility.Omnificia*' with 3743010929; base-game sets with -Mode base.
	/// </summary>
	public static class Compatibility
	{
		// MiniBaseFE (3382061617). At its defaults the cluster is the one MiniBase asteroid; the
		// optional moonlets are off in its options.
		private const string MiniBase = "MINI-B-100-0-0-0";
		private const string MiniBaseBase = "V-MINI-100-0-0-0";
		// Empty Worlds (1825146346): one prefix per cluster in both modes.
		private const string Emptera = "EMP-PHO-100-0-0-0";
		private const string Islands = "ISD-PHO-100-0-0-0";
		// Voidria worldgen (3430682737).
		private const string Voidria = "VOA-TG-C-100-0-0-0";
		private const string VoidriaMini = "VOA-TG-M-100-0-0-0";
		private const string VoidriaBase = "VOA-TG-B-100-0-0-0";
		// Baator (2582905897), Spaced Out only.
		private const string BaatorStart = "BTOR-L-100-0-0-0";
		private const string BaatorMoonlet = "BTOR-S-100-0-0-0";
		// Omnificia Asteroid II (3743010929), Spaced Out with every worldgen DLC.
		private const string OmnificiaSandstone = "XL-OM2-SND-C-100-0-0-0";
		private const string OmnificiaBeach = "XL-OM2-BCH-C-100-0-0-0";

		/// <summary>MiniBase's own asteroids: its RenderOffline prefix skips the game's worldgen for them and draws its own walls.</summary>
		private static readonly string[] MiniBaseOwn = { "MiniBase", "BabyOilyMoonlet", "BabyMarshyMoonlet", "BabyNiobiumMoonlet" };
		private static readonly string[] NoneOwn = new string[0];

		private const string Baseline = "alone";

		private static Options MaximaConfig => Support.Config(Groups.Primary.Max, Groups.Small.Max, Groups.Moonlets.Max, Groups.TinyMoonlets.Max);

		public static System.Action Alone()
		{
			return WithRects(Support.VanillaDraw());
		}

		/// <summary>The same as Alone, under another name so that it is a world of its own.</summary>
		public static System.Action AloneAgain()
		{
			return WithRects(Support.VanillaDraw());
		}

		public static System.Action Defaults()
		{
			return WithRects(Support.Configure(new Options()));
		}

		public static System.Action Maxima()
		{
			return WithRects(Support.Configure(MaximaConfig));
		}

		private static System.Action WithRects(System.Action undo)
		{
			System.Action detach = RectProbe.Attach();
			return () =>
			{
				detach();
				undo();
			};
		}

		/// <summary>
		/// Writes down, for every asteroid, the four POI rectangles DrawWorldBorder leaves for the
		/// templates to keep out of (the last postfix on it, after Smooth Borders' merge), to tell a
		/// placement difference that comes from the rectangles from one that does not.
		/// </summary>
		internal static class RectProbe
		{
			private const string HarmonyId = "SmoothBorders.Tests.RectProbe";
			private static readonly object Sync = new object();
			private static readonly Dictionary<string, string> Rects = new Dictionary<string, string>();

			private static System.Reflection.MethodBase Target => HarmonyLib.AccessTools.Method(typeof(ProcGenGame.WorldGen), "DrawWorldBorder");

			public static System.Action Attach()
			{
				lock (Sync)
					Rects.Clear();
				var harmony = new HarmonyLib.Harmony(HarmonyId);
				harmony.Patch(Target, postfix: new HarmonyLib.HarmonyMethod(typeof(RectProbe), nameof(Postfix)) { priority = HarmonyLib.Priority.Last });
				return () => harmony.Unpatch(Target, HarmonyLib.HarmonyPatchType.All, HarmonyId);
			}

			public static void Postfix(ProcGenGame.WorldGen __instance)
			{
				List<UnityEngine.RectInt> bounds = __instance.POIBounds;
				if (bounds == null || bounds.Count < 4)
					return;
				string text = string.Join("; ", bounds.Skip(bounds.Count - 4).Select(r => r.x + "," + r.y + " " + r.width + "x" + r.height));
				lock (Sync)
					Rects[Support.Short(__instance.Settings.world.filePath)] = text;
			}

			/// <summary>"world: left; right; bottom; top" per asteroid, sorted.</summary>
			public static List<string> Lines()
			{
				lock (Sync)
					return Rects.Select(pair => pair.Key + ": " + pair.Value).OrderBy(line => line, System.StringComparer.Ordinal).ToList();
			}
		}

		/// <summary>How deep the edge strip compared cell by cell reaches: the deepest wall the game or the mod at its maxima draws.</summary>
		private static int EdgeStrip(Support.BorderPair pair)
		{
			return Math.Max(pair.T + pair.R, Groups.Thickness(pair.T, pair.R, MaximaConfig));
		}

		/// <summary>Every neutronium cell of each asteroid within EdgeStrip of an edge, as "world x y" from the asteroid's corner.</summary>
		private static List<string> EdgeNeutronium()
		{
			var cells = new List<string>();
			foreach (WorldContainer world in Support.Asteroids())
			{
				int s = EdgeStrip(Support.PairOf(world));
				int x0 = world.WorldOffset.x, y0 = world.WorldOffset.y, w = world.WorldSize.x, h = world.WorldSize.y;
				for (int y = 0; y < h; y++)
					for (int x = 0; x < w; x++)
					{
						if (x >= s && x < w - s && y >= s && y < h - s)
							continue;
						int cell = Grid.XYToCell(x0 + x, y0 + y);
						Element element = Grid.IsValidCell(cell) ? Grid.Element[cell] : null;
						if (element != null && element.id == SimHashes.Unobtanium)
							cells.Add(Support.Short(world.worldName) + " " + x + " " + y);
					}
			}
			cells.Sort(string.CompareOrdinal);
			return cells;
		}

		private static string EdgeFile(string coordinate)
		{
			return Path.Combine(Isochronous.TestRunner.Session.Dir, "edge-" + Baseline + "-" + coordinate + ".txt");
		}

		/// <summary>
		/// The neutronium near each asteroid's edges here against the mod-alone world's, asteroid by
		/// asteroid (logged): how much wall the game drew, and how much the mod added and took away.
		/// </summary>
		private static void CompareEdges(TestContext t, string coordinate)
		{
			string file = EdgeFile(coordinate);
			if (!File.Exists(file))
				return;
			List<string> alone = File.ReadAllLines(file).ToList();
			List<string> here = EdgeNeutronium();
			var aloneSet = new HashSet<string>(alone);
			var hereSet = new HashSet<string>(here);
			foreach (WorldContainer world in Support.Asteroids())
			{
				string name = Support.Short(world.worldName) + " ";
				int a = alone.Count(line => line.StartsWith(name, StringComparison.Ordinal));
				int h = here.Count(line => line.StartsWith(name, StringComparison.Ordinal));
				int added = here.Count(line => line.StartsWith(name, StringComparison.Ordinal) && !aloneSet.Contains(line));
				int gone = alone.Count(line => line.StartsWith(name, StringComparison.Ordinal) && !hereSet.Contains(line));
				int perimeter = 2 * (world.WorldSize.x + world.WorldSize.y);
				t.Log("edge neutronium of " + Support.Short(world.worldName) + " (strip " + EdgeStrip(Support.PairOf(world)) + ", perimeter " + perimeter + " cells): "
					+ a + " with the mod alone, " + h + " here; " + added + " added, " + gone + " taken away");
			}
		}

		private static string RectsFile(string coordinate)
		{
			return Path.Combine(Isochronous.TestRunner.Session.Dir, "rects-" + Baseline + "-" + coordinate + ".txt");
		}

		/// <summary>The POI rectangles here against the mod-alone world's, asteroid by asteroid (logged, not asserted).</summary>
		private static void CompareRects(TestContext t, string coordinate)
		{
			string file = RectsFile(coordinate);
			if (!File.Exists(file))
				return;
			var alone = new HashSet<string>(File.ReadAllLines(file));
			List<string> here = RectProbe.Lines();
			List<string> differ = here.Where(line => !alone.Contains(line)).ToList();
			t.Log(differ.Count == 0 ? "every asteroid's POI rectangles (left; right; bottom; top) are the mod-alone world's"
				: differ.Count + " asteroid(s) with other POI rectangles than the mod-alone world's: "
					+ string.Join(" | ", differ.Select(line => "here " + line + ", alone " + (alone.FirstOrDefault(a => a.StartsWith(line.Substring(0, line.IndexOf(':') + 1), System.StringComparison.Ordinal)) ?? "?"))));
		}

		/// <summary>Every prefix on DrawWorldBorder, in the order Harmony runs them, with owner and priority.</summary>
		private static string BorderPrefixes()
		{
			HarmonyLib.Patches info = HarmonyLib.Harmony.GetPatchInfo(RectProbeTarget);
			if (info == null)
				return "none";
			return string.Join(", ", info.Prefixes.OrderByDescending(p => p.priority).ThenBy(p => p.index)
				.Select(p => p.PatchMethod.DeclaringType.FullName + " (" + p.owner + ", priority " + p.priority + ", index " + p.index + ")"));
		}

		private static System.Reflection.MethodBase RectProbeTarget => HarmonyLib.AccessTools.Method(typeof(ProcGenGame.WorldGen), "DrawWorldBorder");

		// ======== shared steps ========

		private static void ExpectStart(TestContext t, string startPrefix)
		{
			List<string> names = Support.Asteroids().Select(world => Support.Short(world.worldName)).ToList();
			t.Assert.True(names.Any(name => name.StartsWith(startPrefix, System.StringComparison.Ordinal)),
				"the cluster has the worldgen mod's asteroid " + startPrefix + " (it has " + string.Join(", ", names) + ")");
		}

		/// <summary>Every asteroid: size, border pair, whether the game draws it, the Smooth Borders kind and F at the defaults and maxima.</summary>
		private static void DescribeWorlds(TestContext t, string[] own)
		{
			foreach (WorldContainer world in Support.Asteroids())
			{
				Support.BorderPair pair = Support.PairOf(world);
				Group group = Groups.ForWorld(pair.T, pair.R);
				t.Log("asteroid " + Support.Short(world.worldName) + " " + world.WorldSize.x + "x" + world.WorldSize.y + ": (" + pair.T + ", " + pair.R + ")"
					+ (pair.Drawn ? "" : ", DrawWorldBorder false")
					+ (own.Contains(Support.Short(world.worldName)) ? ", built by its own mod" : "")
					+ ", kind " + (group != null ? group.Id : "own pair")
					+ ", F " + Groups.Thickness(pair.T, pair.R, new Options()) + " at the defaults, " + Groups.Thickness(pair.T, pair.R, MaximaConfig) + " at the maxima");
			}
		}

		private static List<WorldContainer> OwnAsteroids(string[] own)
		{
			return Support.Asteroids().Where(world => own.Contains(Support.Short(world.worldName))).ToList();
		}

		private static bool AnyDrawnByTheGame(string[] own)
		{
			return Support.Asteroids().Any(world =>
			{
				Support.BorderPair pair = Support.PairOf(world);
				return !own.Contains(Support.Short(world.worldName)) && pair.Drawn && pair.T > 0;
			});
		}

		private static string SolidsFile(string coordinate)
		{
			return Path.Combine(Isochronous.TestRunner.Session.Dir, "solids-" + Baseline + "-" + coordinate + ".txt");
		}

		/// <summary>Every stable solid cell of the mod's own asteroids (walls, rock, its neutronium and abyssalite), as "world x y element".</summary>
		private static List<string> OwnSolids(string[] own)
		{
			var cells = new List<string>();
			foreach (WorldContainer world in OwnAsteroids(own))
			{
				int x0 = world.WorldOffset.x, y0 = world.WorldOffset.y;
				for (int y = 0; y < world.WorldSize.y; y++)
					for (int x = 0; x < world.WorldSize.x; x++)
					{
						int cell = Grid.XYToCell(x0 + x, y0 + y);
						if (!Grid.IsValidCell(cell))
							continue;
						Element element = Grid.Element[cell];
						if (element != null && element.IsSolid && !element.IsUnstable)
							cells.Add(Support.Short(world.worldName) + " " + x + " " + y + " " + element.id);
					}
			}
			cells.Sort(string.CompareOrdinal);
			return cells;
		}

		/// <summary>
		/// The worldgen mod alone (Smooth Borders' border patches off): generated without errors, no
		/// line from Smooth Borders, the game's own wall uneven where the game draws one; the
		/// buildings, geysers, inner neutronium and the mod's own asteroids' solid cells written down.
		/// </summary>
		private static IEnumerator RecordAlone(TestContext t, string coordinate, string startPrefix, string[] own)
		{
			t.Assert.NotNull(t.Generated, "the test runs in a generated world");
			t.Assert.Eq(0, t.Generated.Errors.Count, "the worldgen reported no errors: " + string.Join(" | ", t.Generated.Errors));
			ExpectStart(t, startPrefix);
			DescribeWorlds(t, own);
			t.Assert.Eq(0, Support.Logged().Count, "Smooth Borders logged nothing while its patches were off (" + string.Join(" | ", Support.Logged()) + ")");
			var defaults = new Options();
			int uneven = 0;
			foreach (WorldContainer world in Support.Asteroids())
			{
				Support.BorderPair pair = Support.PairOf(world);
				if (own.Contains(Support.Short(world.worldName)) || !pair.Drawn || pair.T <= 0)
					continue;
				Support.Edge[] edges = Support.Measure(world, Groups.Thickness(pair.T, pair.R, defaults));
				t.Log("game's wall, " + pair + ": " + string.Join("; ", edges.Select(e => e.ToString())));
				uneven += edges.Count(e => e.TooDeep);
			}
			List<string> placements = Support.Placements();
			File.WriteAllLines(Support.PlacementFile(coordinate, Baseline), placements);
			List<string> inner = Support.InnerNeutronium(defaults);
			File.WriteAllLines(Support.NeutroniumFile(coordinate, Baseline), inner);
			List<string> solids = OwnSolids(own);
			File.WriteAllLines(SolidsFile(coordinate), solids);
			t.Log(placements.Count + " buildings and geysers, " + inner.Count + " inner neutronium cells, " + solids.Count + " solid cells of the mod's own asteroids written down");
			File.WriteAllLines(RectsFile(coordinate), RectProbe.Lines());
			File.WriteAllLines(EdgeFile(coordinate), EdgeNeutronium());
			CompareEdges(t, coordinate);
			// Not asserted: a world whose edges are vacuum in terrain the game leaves unwalled
			// (RemoveWorldBorderOverVacuum, Voidria) has next to no wall to be uneven.
			t.Log("the game's own wall is deeper than Smooth Borders' default on " + uneven + " edge(s)");
			t.Assert.True(placements.Count > 0, "the world has buildings and geysers to compare");
			yield break;
		}

		/// <summary>
		/// Smooth Borders said nothing about the mod's own asteroids (it never ran for them) and drew
		/// every asteroid the game walls; the own asteroids' solid cells are the mod-alone world's.
		/// </summary>
		private static IEnumerator ExpectOwnLeftAlone(TestContext t, string coordinate, string[] own)
		{
			List<string> logged = Support.Logged();
			foreach (WorldContainer world in OwnAsteroids(own))
			{
				List<string> about = logged.Where(line => line.StartsWith(world.worldName + ":", System.StringComparison.Ordinal)).ToList();
				t.Assert.Eq(0, about.Count, "Smooth Borders did nothing on " + world.worldName + ", which its mod builds itself (" + string.Join(" | ", about) + ")");
			}
			string file = SolidsFile(coordinate);
			t.Assert.True(File.Exists(file), "the mod-alone solids of " + coordinate + " were written earlier in this run (" + file + ")");
			var expected = new HashSet<string>(File.ReadAllLines(file));
			List<string> actual = OwnSolids(own);
			var actualSet = new HashSet<string>(actual);
			List<string> gone = expected.Where(line => !actualSet.Contains(line)).ToList();
			List<string> added = actual.Where(line => !expected.Contains(line)).ToList();
			t.Log(OwnAsteroids(own).Count + " asteroid(s) built by their own mod (" + string.Join(", ", OwnAsteroids(own).Select(w => Support.Short(w.worldName))) + "): "
				+ actual.Count + " solid cells here, " + expected.Count + " with the mod alone; " + gone.Count + " differ, " + added.Count + " new");
			t.Assert.True(gone.Count == 0 && added.Count == 0, "the mod's own asteroids are cell for cell as the mod alone made them; differ: "
				+ string.Join(", ", gone.Take(5)) + "; new: " + string.Join(", ", added.Take(5)));
			yield break;
		}

		private static IEnumerator Flat(TestContext t, string coordinate, string startPrefix, string[] own)
		{
			ExpectStart(t, startPrefix);
			DescribeWorlds(t, own);
			foreach (string line in Support.Logged())
				t.Log("SB: " + line);
			t.Log("prefixes on DrawWorldBorder, in run order: " + BorderPrefixes());
			CompareEdges(t, coordinate);
			yield return Support.ExpectFlat(t, new Options(), own);
			yield return ExpectOwnLeftAlone(t, coordinate, own);
		}

		/// <summary>At the defaults: no POI rectangle grew, buildings, geysers and inner neutronium as with the mod alone.</summary>
		private static IEnumerator Same(TestContext t, string coordinate, string[] own, bool kindsVary = false)
		{
			if (AnyDrawnByTheGame(own))
				t.Assert.True(Support.FlatWalls().Count > 0, "Smooth Borders drew this world's walls (" + string.Join(" | ", Support.Logged()) + ")");
			t.Assert.Eq(0, Support.GrewLines().Count, "no POI rectangle grew at the defaults: " + string.Join(" | ", Support.GrewLines()));
			CompareRects(t, coordinate);
			if (kindsVary)
				yield return ExpectSameSpots(t, coordinate, Baseline);
			else
				yield return Support.ExpectSamePlacements(t, coordinate, Baseline);
			yield return Support.ExpectInnerNeutroniumKept(t, coordinate, Baseline, new Options(), true);
		}

		private static bool IsGeyser(string placement)
		{
			string[] parts = placement.Split(' ');
			return parts.Length == 4 && parts[1].StartsWith("GeyserGeneric", System.StringComparison.Ordinal);
		}

		/// <summary>
		/// For a worldgen mod that picks a geyser with an unseeded random (MiniBase's random useful
		/// feature: Util.GetRandom, which is UnityEngine.Random; one of its eleven picks is an oil
		/// reservoir, not a geyser): every building is the mod-alone world's, and the geysers that
		/// differ are logged, not compared. MiniBaseAloneAgain shows the same difference with the
		/// patches off both times.
		/// </summary>
		private static IEnumerator ExpectSameSpots(TestContext t, string coordinate, string baseline)
		{
			string file = Support.PlacementFile(coordinate, baseline);
			t.Assert.True(File.Exists(file), "the baseline placements of " + coordinate + " were written earlier in this run (" + file + ")");
			List<string> expected = File.ReadAllLines(file).ToList();
			List<string> actual = Support.Placements();
			List<string> geysers = expected.Except(actual).Where(IsGeyser).Select(line => "was " + line)
				.Concat(actual.Except(expected).Where(IsGeyser).Select(line => "now " + line)).ToList();
			List<string> missing = expected.Where(line => !IsGeyser(line)).Except(actual).ToList();
			List<string> extra = actual.Where(line => !IsGeyser(line)).Except(expected).ToList();
			t.Log(actual.Count + " buildings and geysers here, " + expected.Count + " in the baseline; " + missing.Count + " building(s) missing, " + extra.Count + " extra"
				+ (geysers.Count > 0 ? "; geysers that differ: " + string.Join(", ", geysers) : "; every geyser the same"));
			t.Assert.True(missing.Count == 0 && extra.Count == 0,
				"the buildings sit where the baseline has them; missing: " + string.Join(", ", missing.Take(5)) + "; extra: " + string.Join(", ", extra.Take(5)));
			yield break;
		}

		/// <summary>
		/// The colony runs at normal speed with the camera on each asteroid in turn, switched as the
		/// game's asteroid list switches it, each asteroid revealed (test-only) and photographed, two
		/// game seconds each; an error or exception the game logs meanwhile fails the test (the
		/// harness's log check).
		/// </summary>
		private static IEnumerator Plays(TestContext t)
		{
			if (SpeedControlScreen.Instance != null && SpeedControlScreen.Instance.IsPaused)
				SpeedControlScreen.Instance.Unpause(false);
			int start = ClusterManager.Instance.activeWorldId;
			foreach (WorldContainer world in Support.Asteroids())
			{
				t.World.RevealAsteroid(world);
				yield return Support.ShowAsteroid(t, world.id);
				string spot = Support.CameraSpot();
				t.Log(Support.Short(world.worldName) + ": " + spot);
				t.Assert.True(spot.Contains("inside it"), "the camera points inside " + world.worldName + ": " + spot);
				yield return t.Screenshot("asteroid-" + Support.Short(world.worldName));
				yield return t.WaitGameSeconds(2);
			}
			yield return Support.ShowAsteroid(t, start);
			yield return t.WaitGameSeconds(4);
			t.Log("played " + (GameClock.Instance != null ? GameClock.Instance.GetTime().ToString("F0") : "?") + " game seconds in, " + Support.Asteroids().Count + " asteroid(s) shown");
		}

		/// <summary>Generation succeeds with Smooth Borders on: no worldgen error, flat at these options, the mod's own asteroids untouched by Smooth Borders.</summary>
		private static IEnumerator Generates(TestContext t, Options options, string startPrefix, string[] own)
		{
			ExpectStart(t, startPrefix);
			foreach (string line in Support.Logged())
				t.Log("SB: " + line);
			foreach (WorldContainer world in OwnAsteroids(own))
				t.Assert.Eq(0, Support.Logged().Count(line => line.StartsWith(world.worldName + ":", StringComparison.Ordinal)),
					"Smooth Borders did nothing on " + world.worldName + ", which its mod builds itself");
			yield return Support.ExpectFlat(t, options, own);
		}

		private static IEnumerator AtMaxima(TestContext t, string coordinate, string startPrefix, string[] own)
		{
			ExpectStart(t, startPrefix);
			foreach (string line in Support.Logged())
				t.Log("SB: " + line);
			CompareEdges(t, coordinate);
			yield return Support.ExpectFlat(t, MaximaConfig, own);
			string file = Support.PlacementFile(coordinate, Baseline);
			t.Assert.True(File.Exists(file), "the mod-alone placements of " + coordinate + " were written earlier in this run (" + file + "); run the baseline test with this one");
			var baseline = new HashSet<string>(File.ReadAllLines(file));
			List<string> here = Support.Placements();
			int moved = here.Count(line => !baseline.Contains(line)) + baseline.Count(line => !here.Contains(line));
			t.Log(moved == 0 ? "the buildings and geysers sit where the mod-alone world has them"
				: moved + " building and geyser placement(s) differ from the mod-alone world's (grown POI rectangles move templates; a mod's unseeded random geysers differ too)");
			yield return Support.ExpectInnerNeutroniumKept(t, coordinate, Baseline, MaximaConfig, false, moved == 0);
			yield return ExpectOwnLeftAlone(t, coordinate, own);
		}

		// ======== MiniBaseFE, -Steam 3382061617 ========

		[OniTest("control: MiniBase with Smooth Borders' patches off; its buildings, geysers, inner neutronium and own asteroid's cells are written down", Worldgen = MiniBase, Prepare = nameof(Alone), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseAlone(TestContext t) => RecordAlone(t, MiniBase, "MiniBase", MiniBaseOwn);

		[OniTest("SB+MiniBase: Smooth Borders leaves MiniBase's own asteroid cell for cell as MiniBase made it", Worldgen = MiniBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseFlat(TestContext t) => Flat(t, MiniBase, "MiniBase", MiniBaseOwn);

		[OniTest("SB+MiniBase: at the defaults buildings, geysers and inner neutronium are MiniBase's alone", Worldgen = MiniBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniBaseSame(TestContext t) => Same(t, MiniBase, MiniBaseOwn, true);

		[OniTest("SB+MiniBase: the colony plays without an error", Worldgen = MiniBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator MiniBasePlays(TestContext t) => Plays(t);

		[OniTest("SB+MiniBase: at the maxima MiniBase's own asteroid is still as MiniBase made it", Worldgen = MiniBase, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseAtMaxima(TestContext t) => AtMaxima(t, MiniBase, "MiniBase", MiniBaseOwn);

		[OniTest("control: MiniBase generated a second time with Smooth Borders' patches off puts its buildings where the first world has them; the geysers that differ are logged", Worldgen = MiniBase, Prepare = nameof(AloneAgain), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseAloneAgain(TestContext t) => ExpectSameSpots(t, MiniBase, Baseline);

		[OniTest("control: base-game MiniBase with Smooth Borders' patches off; written down", Worldgen = MiniBaseBase, Prepare = nameof(Alone), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseBaseAlone(TestContext t) => RecordAlone(t, MiniBaseBase, "MiniBase", MiniBaseOwn);

		[OniTest("SB+MiniBase: Smooth Borders leaves base-game MiniBase cell for cell as MiniBase made it", Worldgen = MiniBaseBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseBaseFlat(TestContext t) => Flat(t, MiniBaseBase, "MiniBase", MiniBaseOwn);

		[OniTest("SB+MiniBase: base game, at the defaults buildings, geysers and inner neutronium are MiniBase's alone", Worldgen = MiniBaseBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniBaseBaseSame(TestContext t) => Same(t, MiniBaseBase, MiniBaseOwn, true);

		[OniTest("SB+MiniBase: base game, the colony plays without an error", Worldgen = MiniBaseBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator MiniBaseBasePlays(TestContext t) => Plays(t);

		[OniTest("SB+MiniBase: base game, at the maxima MiniBase is still as MiniBase made it", Worldgen = MiniBaseBase, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBaseBaseAtMaxima(TestContext t) => AtMaxima(t, MiniBaseBase, "MiniBase", MiniBaseOwn);

		// ======== Empty Worlds, -Steam 1825146346 (both modes) ========

		[OniTest("control: Emptera with Smooth Borders' patches off; written down", Worldgen = Emptera, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyEmpteraAlone(TestContext t) => RecordAlone(t, Emptera, "Emptera", NoneOwn);

		[OniTest("SB+Empty Worlds: at the defaults every asteroid of Emptera has a flat wall", Worldgen = Emptera, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyEmpteraFlat(TestContext t) => Flat(t, Emptera, "Emptera", NoneOwn);

		[OniTest("SB+Empty Worlds: at the defaults Emptera's buildings, geysers and inner neutronium are Empty Worlds' alone", Worldgen = Emptera, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator EmptyEmpteraSame(TestContext t) => Same(t, Emptera, NoneOwn);

		[OniTest("SB+Empty Worlds: Emptera plays without an error", Worldgen = Emptera, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyEmpteraPlays(TestContext t) => Plays(t);

		[OniTest("SB+Empty Worlds: at the maxima every asteroid of Emptera has a flat wall", Worldgen = Emptera, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyEmpteraAtMaxima(TestContext t) => AtMaxima(t, Emptera, "Emptera", NoneOwn);

		[OniTest("control: Islands with Smooth Borders' patches off; written down", Worldgen = Islands, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyIslandsAlone(TestContext t) => RecordAlone(t, Islands, "Islands", NoneOwn);

		[OniTest("SB+Empty Worlds: at the defaults every asteroid of Islands has a flat wall", Worldgen = Islands, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyIslandsFlat(TestContext t) => Flat(t, Islands, "Islands", NoneOwn);

		[OniTest("SB+Empty Worlds: at the defaults Islands' buildings, geysers and inner neutronium are Empty Worlds' alone", Worldgen = Islands, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator EmptyIslandsSame(TestContext t) => Same(t, Islands, NoneOwn);

		[OniTest("SB+Empty Worlds: Islands plays without an error", Worldgen = Islands, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyIslandsPlays(TestContext t) => Plays(t);

		[OniTest("SB+Empty Worlds: at the maxima every asteroid of Islands has a flat wall", Worldgen = Islands, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator EmptyIslandsAtMaxima(TestContext t) => AtMaxima(t, Islands, "Islands", NoneOwn);

		// ======== Voidria, -Steam 3430682737 ========

		[OniTest("control: Voidria with Smooth Borders' patches off; written down", Worldgen = Voidria, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaAlone(TestContext t) => RecordAlone(t, Voidria, "Voidria", NoneOwn);

		[OniTest("SB+Voidria: at the defaults every asteroid of Voidria has a flat wall", Worldgen = Voidria, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaFlat(TestContext t) => Flat(t, Voidria, "Voidria", NoneOwn);

		[OniTest("SB+Voidria: at the defaults Voidria's buildings, geysers and inner neutronium are Voidria's alone", Worldgen = Voidria, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator VoidriaSame(TestContext t) => Same(t, Voidria, NoneOwn);

		[OniTest("SB+Voidria: Voidria plays without an error", Worldgen = Voidria, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaPlays(TestContext t) => Plays(t);

		[OniTest("SB+Voidria: at the maxima every asteroid of Voidria has a flat wall", Worldgen = Voidria, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaAtMaxima(TestContext t) => AtMaxima(t, Voidria, "Voidria", NoneOwn);

		[OniTest("control: Voidria mini with Smooth Borders' patches off; written down", Worldgen = VoidriaMini, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaMiniAlone(TestContext t) => RecordAlone(t, VoidriaMini, "VoidriaMini", NoneOwn);

		[OniTest("SB+Voidria: at the defaults every asteroid of Voidria mini has a flat wall", Worldgen = VoidriaMini, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaMiniFlat(TestContext t) => Flat(t, VoidriaMini, "VoidriaMini", NoneOwn);

		[OniTest("SB+Voidria: at the defaults Voidria mini's buildings, geysers and inner neutronium are Voidria's alone", Worldgen = VoidriaMini, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator VoidriaMiniSame(TestContext t) => Same(t, VoidriaMini, NoneOwn);

		[OniTest("SB+Voidria: Voidria mini plays without an error", Worldgen = VoidriaMini, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaMiniPlays(TestContext t) => Plays(t);

		[OniTest("SB+Voidria: at the maxima every asteroid of Voidria mini has a flat wall", Worldgen = VoidriaMini, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaMiniAtMaxima(TestContext t) => AtMaxima(t, VoidriaMini, "VoidriaMini", NoneOwn);

		[OniTest("control: base-game Voidria with Smooth Borders' patches off; written down", Worldgen = VoidriaBase, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaBaseAlone(TestContext t) => RecordAlone(t, VoidriaBase, "Voidria", NoneOwn);

		[OniTest("SB+Voidria: at the defaults base-game Voidria has a flat wall", Worldgen = VoidriaBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaBaseFlat(TestContext t) => Flat(t, VoidriaBase, "Voidria", NoneOwn);

		[OniTest("SB+Voidria: at the defaults base-game Voidria's buildings, geysers and inner neutronium are Voidria's alone", Worldgen = VoidriaBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator VoidriaBaseSame(TestContext t) => Same(t, VoidriaBase, NoneOwn);

		[OniTest("SB+Voidria: base-game Voidria plays without an error", Worldgen = VoidriaBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaBasePlays(TestContext t) => Plays(t);

		[OniTest("SB+Voidria: at the maxima base-game Voidria has a flat wall", Worldgen = VoidriaBase, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator VoidriaBaseAtMaxima(TestContext t) => AtMaxima(t, VoidriaBase, "Voidria", NoneOwn);

		// ======== Baator, -Steam 2582905897 ========

		[OniTest("control: Baator with Smooth Borders' patches off; written down", Worldgen = BaatorStart, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorAlone(TestContext t) => RecordAlone(t, BaatorStart, "Baator", NoneOwn);

		[OniTest("SB+Baator: at the defaults every asteroid of Baator has a flat wall", Worldgen = BaatorStart, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorFlat(TestContext t) => Flat(t, BaatorStart, "Baator", NoneOwn);

		[OniTest("SB+Baator: at the defaults Baator's buildings, geysers and inner neutronium are Baator's alone", Worldgen = BaatorStart, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator BaatorSame(TestContext t) => Same(t, BaatorStart, NoneOwn);

		[OniTest("SB+Baator: Baator plays without an error", Worldgen = BaatorStart, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorPlays(TestContext t) => Plays(t);

		[OniTest("SB+Baator: at the maxima every asteroid of Baator has a flat wall", Worldgen = BaatorStart, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorAtMaxima(TestContext t) => AtMaxima(t, BaatorStart, "Baator", NoneOwn);

		[OniTest("control: the Baator moonlet cluster with Smooth Borders' patches off; written down", Worldgen = BaatorMoonlet, Prepare = nameof(Alone), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorMoonletAlone(TestContext t) => RecordAlone(t, BaatorMoonlet, "Baator_moonlet", NoneOwn);

		[OniTest("SB+Baator: at the defaults every asteroid of the Baator moonlet cluster has a flat wall", Worldgen = BaatorMoonlet, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorMoonletFlat(TestContext t) => Flat(t, BaatorMoonlet, "Baator_moonlet", NoneOwn);

		[OniTest("SB+Baator: at the defaults the Baator moonlet cluster's buildings, geysers and inner neutronium are Baator's alone", Worldgen = BaatorMoonlet, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator BaatorMoonletSame(TestContext t) => Same(t, BaatorMoonlet, NoneOwn);

		[OniTest("SB+Baator: the Baator moonlet cluster plays without an error", Worldgen = BaatorMoonlet, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorMoonletPlays(TestContext t) => Plays(t);

		[OniTest("SB+Baator: at the maxima every asteroid of the Baator moonlet cluster has a flat wall", Worldgen = BaatorMoonlet, Prepare = nameof(Maxima), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator BaatorMoonletAtMaxima(TestContext t) => AtMaxima(t, BaatorMoonlet, "Baator_moonlet", NoneOwn);

		// ======== Omnificia Asteroid II, -Steam 3743010929 ========

		[OniTest("control: Omnificia sandstone with Smooth Borders' patches off; written down", Worldgen = OmnificiaSandstone, Prepare = nameof(Alone), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator OmnificiaAlone(TestContext t) => RecordAlone(t, OmnificiaSandstone, "Omnificia2Sandstone", NoneOwn);

		[OniTest("SB+Omnificia: at the defaults Omnificia sandstone has a flat wall", Worldgen = OmnificiaSandstone, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator OmnificiaFlat(TestContext t) => Flat(t, OmnificiaSandstone, "Omnificia2Sandstone", NoneOwn);

		[OniTest("SB+Omnificia: at the defaults Omnificia sandstone's buildings, geysers and inner neutronium are Omnificia's alone", Worldgen = OmnificiaSandstone, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator OmnificiaSame(TestContext t) => Same(t, OmnificiaSandstone, NoneOwn);

		[OniTest("SB+Omnificia: Omnificia sandstone plays without an error", Worldgen = OmnificiaSandstone, Prepare = nameof(Defaults), Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator OmnificiaPlays(TestContext t) => Plays(t);

		[OniTest("SB+Omnificia: at the maxima Omnificia sandstone has a flat wall", Worldgen = OmnificiaSandstone, Prepare = nameof(Maxima), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator OmnificiaAtMaxima(TestContext t) => AtMaxima(t, OmnificiaSandstone, "Omnificia2Sandstone", NoneOwn);

		// ---- one more Omnificia cluster: generation succeeds and every wall is flat ----

		[OniTest("SB+Omnificia: Omnificia Beach generates and has a flat wall at the defaults", Worldgen = OmnificiaBeach, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator OmnificiaBeachFlat(TestContext t) => Generates(t, new Options(), "Omnificia2Beach", NoneOwn);

		[OniTest("SB+Omnificia: Omnificia Beach generates and has a flat wall at the maxima", Worldgen = OmnificiaBeach, Prepare = nameof(Maxima), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator OmnificiaBeachAtMaxima(TestContext t) => Generates(t, MaximaConfig, "Omnificia2Beach", NoneOwn);

		// ======== all five together, -Steam 3382061617,1825146346,3430682737,2582905897,3743010929 ========
		// One cluster of each mod, at the defaults, with every one of the five mods enabled.

		[OniTest("SB with all five worldgen mods: MiniBase generates, flat where the game walls it, the mod's own asteroids left alone", Worldgen = MiniBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherMiniBase(TestContext t) => Generates(t, new Options(), "MiniBase", MiniBaseOwn);

		[OniTest("SB with all five worldgen mods: MiniBase plays without an error", Worldgen = MiniBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherMiniBasePlays(TestContext t) => Plays(t);

		[OniTest("SB with all five worldgen mods: Emptera generates, flat where the game walls it, the mod's own asteroids left alone", Worldgen = Emptera, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherEmptera(TestContext t) => Generates(t, new Options(), "Emptera", NoneOwn);

		[OniTest("SB with all five worldgen mods: Emptera plays without an error", Worldgen = Emptera, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherEmpteraPlays(TestContext t) => Plays(t);

		[OniTest("SB with all five worldgen mods: Voidria generates, flat where the game walls it, the mod's own asteroids left alone", Worldgen = Voidria, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherVoidria(TestContext t) => Generates(t, new Options(), "Voidria", NoneOwn);

		[OniTest("SB with all five worldgen mods: Voidria plays without an error", Worldgen = Voidria, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherVoidriaPlays(TestContext t) => Plays(t);

		[OniTest("SB with all five worldgen mods: Baator generates, flat where the game walls it, the mod's own asteroids left alone", Worldgen = BaatorStart, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherBaator(TestContext t) => Generates(t, new Options(), "Baator", NoneOwn);

		[OniTest("SB with all five worldgen mods: Baator plays without an error", Worldgen = BaatorStart, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherBaatorPlays(TestContext t) => Plays(t);

		[OniTest("SB with all five worldgen mods: Omnificia generates, flat where the game walls it, the mod's own asteroids left alone", Worldgen = OmnificiaSandstone, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherOmnificia(TestContext t) => Generates(t, new Options(), "Omnificia2Sandstone", NoneOwn);

		[OniTest("SB with all five worldgen mods: Omnificia plays without an error", Worldgen = OmnificiaSandstone, Prepare = nameof(Defaults), Explicit = true, Timeout = 150), Layer(0)]
		public static IEnumerator TogetherOmnificiaPlays(TestContext t) => Plays(t);
	}
}
