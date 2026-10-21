using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isochronous.TestKit;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// Identity's three tests on a representative set of the clusters the default run leaves out: a
	/// worldgen DLC's cluster in the base game (Ceres) and in Spaced Out (Ceres and classic Ceres),
	/// the Skewed Asteroid in both modes, and the upside-down mini cluster (MiniFlippedStart); the
	/// upside-down and skewed asteroids at every kind's maximum as well. Every cluster is checked
	/// flat at the defaults and the maxima by the nightly WorldgenDriver sweep. Explicit: run a mode's
	/// set with -Filter '*Clusters.*' (base-game clusters need the launcher's -Mode base). A cluster's
	/// maximum test compares against its vanilla baseline, so run it with that cluster's other tests.
	/// </summary>
	public static class Clusters
	{
		// Base game: one primary asteroid each.
		private const string CeresBase = "CER-A-100-0-0-0";
		// The Skewed Asteroid's cluster has a fixed seed, 7, which the destination screen always sets.
		private const string SkewedBase = "KF23-A-7-0-0-0";

		// Spaced Out.
		private const string Ceres = "CER-C-100-0-0-0";
		private const string CeresClassic = "V-CER-C-100-0-0-0";
		private const string Skewed = "KF23-C-7-0-0-0";
		private const string MiniFlipped = "M-FLIP-C-100-0-0-0";

		private static readonly Group[] MiniKinds = { Groups.Small, Groups.Moonlets, Groups.TinyMoonlets };

		private static Options MaximaConfig => Support.Config(Groups.Primary.Max, Groups.Small.Max, Groups.Moonlets.Max, Groups.TinyMoonlets.Max);

		public static System.Action Vanilla()
		{
			return Support.VanillaDraw();
		}

		public static System.Action Defaults()
		{
			return Support.Configure(new Options());
		}

		public static System.Action Maxima()
		{
			return Support.Configure(MaximaConfig);
		}

		/// <summary>The cluster holds an asteroid generated from this world file (its name's last part).</summary>
		private static void ExpectAsteroid(TestContext t, string world)
		{
			t.Assert.True(Support.Asteroids().Any(container => Support.Short(container.worldName) == world),
				"the cluster has the asteroid " + world + " (it has " + string.Join(", ", Support.Asteroids().Select(container => Support.Short(container.worldName))) + ")");
		}

		private static IEnumerator FlatWith(TestContext t, Options options, string asteroid, params Group[] kinds)
		{
			if (asteroid != null)
				ExpectAsteroid(t, asteroid);
			yield return Support.ExpectFlat(t, options, kinds);
		}

		/// <summary>
		/// At the maxima: flat at each kind's maximum. Where the grown POI rectangles left every
		/// building and geyser where the vanilla world has it, no neutronium the vanilla world has
		/// inside the strips is gone. Where they moved templates, the templates' own neutronium moves
		/// with them, so the cells are compared and logged only.
		/// </summary>
		private static IEnumerator AtMaxima(TestContext t, string coordinate, string asteroid, params Group[] kinds)
		{
			yield return FlatWith(t, MaximaConfig, asteroid, kinds);
			foreach (string line in Support.GrewLines())
				t.Log(line);
			string file = Support.PlacementFile(coordinate, "vanilla");
			t.Assert.True(File.Exists(file), "the baseline placements of " + coordinate + " were written earlier in this run (" + file + "); run the baseline test with this one");
			var baseline = new HashSet<string>(File.ReadAllLines(file));
			List<string> here = Support.Placements();
			int moved = here.Count(line => !baseline.Contains(line)) + baseline.Count(line => !here.Contains(line));
			t.Log(moved == 0 ? "the buildings and geysers sit where the vanilla world has them"
				: moved + " building and geyser placement(s) differ from the vanilla world's (templates moved with the grown POI rectangles)");
			yield return Support.ExpectInnerNeutroniumKept(t, coordinate, "vanilla", MaximaConfig, false, moved == 0);
		}

		// ======== base game ========

		// ---- Ceres (Frosty Planet Pack) ----

		[OniTest("control: base-game Ceres with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = CeresBase, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresBaseVanilla(TestContext t) => Support.RecordBaseline(t, CeresBase, "vanilla");

		[OniTest("SB: at the defaults base-game Ceres has a flat wall at the primary default thickness", Worldgen = CeresBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresBaseFlat(TestContext t) => FlatWith(t, new Options(), "CeresBaseGameAsteroid", Groups.Primary);

		[OniTest("SB: at the defaults base-game Ceres matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = CeresBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator CeresBaseSame(TestContext t) => Identity.SameAndNoGrowth(t, CeresBase);

		// ---- the Skewed Asteroid ----

		[OniTest("control: the base-game Skewed Asteroid with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = SkewedBase, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedBaseVanilla(TestContext t) => Support.RecordBaseline(t, SkewedBase, "vanilla");

		[OniTest("SB: at the defaults the base-game Skewed Asteroid has a flat wall at the primary default thickness", Worldgen = SkewedBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedBaseFlat(TestContext t) => FlatWith(t, new Options(), "StrangeAsteroidKleiFest2023", Groups.Primary);

		[OniTest("SB: at the defaults the base-game Skewed Asteroid matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = SkewedBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator SkewedBaseSame(TestContext t) => Identity.SameAndNoGrowth(t, SkewedBase);

		[OniTest("SB: at the maxima the base-game Skewed Asteroid has a flat wall 16 deep and keeps its inner neutronium", Worldgen = SkewedBase, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedBaseAtMaxima(TestContext t) => AtMaxima(t, SkewedBase, "StrangeAsteroidKleiFest2023", Groups.Primary);

		// ======== Spaced Out ========

		// ---- Ceres ----

		[OniTest("control: Spaced Out Ceres with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = Ceres, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresVanilla(TestContext t) => Support.RecordBaseline(t, Ceres, "vanilla");

		[OniTest("SB: at the defaults every asteroid of Spaced Out Ceres has a flat wall at its kind's default thickness", Worldgen = Ceres, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresFlat(TestContext t) => FlatWith(t, new Options(), "CeresSpacedOutAsteroid", Groups.All);

		[OniTest("SB: at the defaults Spaced Out Ceres matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = Ceres, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator CeresSame(TestContext t) => Identity.SameAndNoGrowth(t, Ceres);

		// ---- classic Ceres ----

		[OniTest("control: classic Ceres with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = CeresClassic, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresClassicVanilla(TestContext t) => Support.RecordBaseline(t, CeresClassic, "vanilla");

		[OniTest("SB: at the defaults every asteroid of classic Ceres has a flat wall at its kind's default thickness", Worldgen = CeresClassic, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresClassicFlat(TestContext t) => FlatWith(t, new Options(), "CeresClassicAsteroid", Groups.All);

		[OniTest("SB: at the defaults classic Ceres matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = CeresClassic, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator CeresClassicSame(TestContext t) => Identity.SameAndNoGrowth(t, CeresClassic);

		// ---- the Skewed Asteroid ----

		[OniTest("control: the Spaced Out Skewed Asteroid cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = Skewed, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedVanilla(TestContext t) => Support.RecordBaseline(t, Skewed, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the Spaced Out Skewed Asteroid cluster has a flat wall at its kind's default thickness", Worldgen = Skewed, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedFlat(TestContext t) => FlatWith(t, new Options(), "StrangeAsteroidKleiFest2023Cluster", Groups.All);

		[OniTest("SB: at the defaults the Spaced Out Skewed Asteroid cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = Skewed, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator SkewedSame(TestContext t) => Identity.SameAndNoGrowth(t, Skewed);

		[OniTest("SB: at the maxima every asteroid of the Spaced Out Skewed Asteroid cluster has a flat wall at its kind's maximum and keeps its inner neutronium", Worldgen = Skewed, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedAtMaxima(TestContext t) => AtMaxima(t, Skewed, "StrangeAsteroidKleiFest2023Cluster", Groups.All);

		// ---- mini cluster, upside-down start (MiniFlippedStart) ----

		[OniTest("control: the upside-down mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = MiniFlipped, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFlippedVanilla(TestContext t) => Support.RecordBaseline(t, MiniFlipped, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the upside-down mini cluster, MiniFlippedStart among them, has a flat wall at its kind's default thickness", Worldgen = MiniFlipped, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFlippedFlat(TestContext t) => FlatWith(t, new Options(), "MiniFlippedStart", MiniKinds);

		[OniTest("SB: at the defaults the upside-down mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = MiniFlipped, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniFlippedSame(TestContext t) => Identity.SameAndNoGrowth(t, MiniFlipped);

		[OniTest("SB: at the maxima every asteroid of the upside-down mini cluster, MiniFlippedStart among them, has a flat wall at its kind's maximum and keeps its inner neutronium", Worldgen = MiniFlipped, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFlippedAtMaxima(TestContext t) => AtMaxima(t, MiniFlipped, "MiniFlippedStart", MiniKinds);
	}
}
