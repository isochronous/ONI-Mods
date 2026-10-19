using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isochronous.TestKit;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// Identity's three tests on the clusters the default run leaves out: every worldgen DLC's
	/// clusters in both modes, the Spaced Out mini clusters (each upside-down asteroid: MiniFlipped,
	/// MiniFlippedStart, MiniFlippedWarp) and the Skewed Asteroid in both modes; the upside-down and
	/// skewed asteroids at every kind's maximum as well. Explicit: run a mode's set with
	/// -Filter '*Clusters.*' (base-game clusters need the launcher's -Mode base). A cluster's maximum
	/// test compares against its vanilla baseline, so run it with that cluster's other tests.
	/// </summary>
	public static class Clusters
	{
		// Base game: one primary asteroid each.
		private const string CeresBase = "CER-A-100-0-0-0";
		private const string CeresShatteredBase = "CERS-A-100-0-0-0";
		private const string PrehistoricBase = "PRE-A-100-0-0-0";
		private const string PrehistoricShatteredBase = "PRES-A-100-0-0-0";
		private const string AquaticBase = "AQU-A-100-0-0-0";
		// The Skewed Asteroid's cluster has a fixed seed, 7, which the destination screen always sets.
		private const string SkewedBase = "KF23-A-7-0-0-0";

		// Spaced Out.
		private const string Ceres = "CER-C-100-0-0-0";
		private const string CeresClassic = "V-CER-C-100-0-0-0";
		private const string CeresMini = "M-CERS-C-100-0-0-0";
		private const string Prehistoric = "PRE-C-100-0-0-0";
		private const string PrehistoricClassic = "V-PRE-C-100-0-0-0";
		private const string Aquatic = "AQU-C-100-0-0-0";
		private const string AquaticClassic = "V-AQU-C-100-0-0-0";
		private const string Skewed = "KF23-C-7-0-0-0";
		private const string MiniBadlands = "M-BAD-C-100-0-0-0";
		private const string MiniFlipped = "M-FLIP-C-100-0-0-0";
		private const string MiniFrozen = "M-FRZ-C-100-0-0-0";
		private const string MiniSwampy = "M-SWMP-C-100-0-0-0";
		private const string MiniRadioactive = "M-RAD-C-100-0-0-0";

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
			var baseline = new HashSet<string>(File.ReadAllLines(Support.PlacementFile(coordinate, "vanilla")));
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

		// ---- Blasted Ceres (shattered) ----

		[OniTest("control: base-game Blasted Ceres with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = CeresShatteredBase, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresShatteredBaseVanilla(TestContext t) => Support.RecordBaseline(t, CeresShatteredBase, "vanilla");

		[OniTest("SB: at the defaults base-game Blasted Ceres has a flat wall at the primary default thickness", Worldgen = CeresShatteredBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresShatteredBaseFlat(TestContext t) => FlatWith(t, new Options(), "CeresBaseGameShatteredAsteroid", Groups.Primary);

		[OniTest("SB: at the defaults base-game Blasted Ceres matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = CeresShatteredBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator CeresShatteredBaseSame(TestContext t) => Identity.SameAndNoGrowth(t, CeresShatteredBase);

		// ---- Relica (Prehistoric Pack) ----

		[OniTest("control: base-game Relica with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = PrehistoricBase, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricBaseVanilla(TestContext t) => Support.RecordBaseline(t, PrehistoricBase, "vanilla");

		[OniTest("SB: at the defaults base-game Relica has a flat wall at the primary default thickness", Worldgen = PrehistoricBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricBaseFlat(TestContext t) => FlatWith(t, new Options(), "PrehistoricBaseGameAsteroid", Groups.Primary);

		[OniTest("SB: at the defaults base-game Relica matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = PrehistoricBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator PrehistoricBaseSame(TestContext t) => Identity.SameAndNoGrowth(t, PrehistoricBase);

		// ---- shattered Relica ----

		[OniTest("control: base-game shattered Relica with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = PrehistoricShatteredBase, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricShatteredBaseVanilla(TestContext t) => Support.RecordBaseline(t, PrehistoricShatteredBase, "vanilla");

		[OniTest("SB: at the defaults base-game shattered Relica has a flat wall at the primary default thickness", Worldgen = PrehistoricShatteredBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricShatteredBaseFlat(TestContext t) => FlatWith(t, new Options(), "PrehistoricShatteredBaseGameAsteroid", Groups.Primary);

		[OniTest("SB: at the defaults base-game shattered Relica matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = PrehistoricShatteredBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator PrehistoricShatteredBaseSame(TestContext t) => Identity.SameAndNoGrowth(t, PrehistoricShatteredBase);

		// ---- Marinea (Aquatic) ----

		[OniTest("control: base-game Marinea with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = AquaticBase, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator AquaticBaseVanilla(TestContext t) => Support.RecordBaseline(t, AquaticBase, "vanilla");

		[OniTest("SB: at the defaults base-game Marinea has a flat wall at the primary default thickness", Worldgen = AquaticBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator AquaticBaseFlat(TestContext t) => FlatWith(t, new Options(), "AquaticBaseGameAsteroid", Groups.Primary);

		[OniTest("SB: at the defaults base-game Marinea matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = AquaticBase, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator AquaticBaseSame(TestContext t) => Identity.SameAndNoGrowth(t, AquaticBase);

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

		// ---- Blasted Ceres mini cluster ----

		[OniTest("control: the Blasted Ceres mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = CeresMini, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresMiniVanilla(TestContext t) => Support.RecordBaseline(t, CeresMini, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the Blasted Ceres mini cluster has a flat wall at its kind's default thickness", Worldgen = CeresMini, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator CeresMiniFlat(TestContext t) => FlatWith(t, new Options(), "MiniShatteredStartAsteroid", MiniKinds);

		[OniTest("SB: at the defaults the Blasted Ceres mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = CeresMini, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator CeresMiniSame(TestContext t) => Identity.SameAndNoGrowth(t, CeresMini);

		// ---- Relica ----

		[OniTest("control: Spaced Out Relica with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = Prehistoric, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricVanilla(TestContext t) => Support.RecordBaseline(t, Prehistoric, "vanilla");

		[OniTest("SB: at the defaults every asteroid of Spaced Out Relica has a flat wall at its kind's default thickness", Worldgen = Prehistoric, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricFlat(TestContext t) => FlatWith(t, new Options(), "PrehistoricSpacedOutAsteroid", Groups.All);

		[OniTest("SB: at the defaults Spaced Out Relica matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = Prehistoric, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator PrehistoricSame(TestContext t) => Identity.SameAndNoGrowth(t, Prehistoric);

		// ---- classic Relica ----

		[OniTest("control: classic Relica with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = PrehistoricClassic, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricClassicVanilla(TestContext t) => Support.RecordBaseline(t, PrehistoricClassic, "vanilla");

		[OniTest("SB: at the defaults every asteroid of classic Relica has a flat wall at its kind's default thickness", Worldgen = PrehistoricClassic, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator PrehistoricClassicFlat(TestContext t) => FlatWith(t, new Options(), "PrehistoricClassicAsteroid", Groups.All);

		[OniTest("SB: at the defaults classic Relica matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = PrehistoricClassic, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator PrehistoricClassicSame(TestContext t) => Identity.SameAndNoGrowth(t, PrehistoricClassic);

		// ---- Marinea ----

		[OniTest("control: Spaced Out Marinea with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = Aquatic, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator AquaticVanilla(TestContext t) => Support.RecordBaseline(t, Aquatic, "vanilla");

		[OniTest("SB: at the defaults every asteroid of Spaced Out Marinea has a flat wall at its kind's default thickness", Worldgen = Aquatic, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator AquaticFlat(TestContext t) => FlatWith(t, new Options(), "AquaticSpacedOutAsteroid", Groups.All);

		[OniTest("SB: at the defaults Spaced Out Marinea matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = Aquatic, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator AquaticSame(TestContext t) => Identity.SameAndNoGrowth(t, Aquatic);

		// ---- classic Marinea ----

		[OniTest("control: classic Marinea with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = AquaticClassic, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator AquaticClassicVanilla(TestContext t) => Support.RecordBaseline(t, AquaticClassic, "vanilla");

		[OniTest("SB: at the defaults every asteroid of classic Marinea has a flat wall at its kind's default thickness", Worldgen = AquaticClassic, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator AquaticClassicFlat(TestContext t) => FlatWith(t, new Options(), "AquaticClassicAsteroid", Groups.All);

		[OniTest("SB: at the defaults classic Marinea matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = AquaticClassic, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator AquaticClassicSame(TestContext t) => Identity.SameAndNoGrowth(t, AquaticClassic);

		// ---- the Skewed Asteroid ----

		[OniTest("control: the Spaced Out Skewed Asteroid cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = Skewed, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedVanilla(TestContext t) => Support.RecordBaseline(t, Skewed, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the Spaced Out Skewed Asteroid cluster has a flat wall at its kind's default thickness", Worldgen = Skewed, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedFlat(TestContext t) => FlatWith(t, new Options(), "StrangeAsteroidKleiFest2023Cluster", Groups.All);

		[OniTest("SB: at the defaults the Spaced Out Skewed Asteroid cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = Skewed, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator SkewedSame(TestContext t) => Identity.SameAndNoGrowth(t, Skewed);

		[OniTest("SB: at the maxima every asteroid of the Spaced Out Skewed Asteroid cluster has a flat wall at its kind's maximum and keeps its inner neutronium", Worldgen = Skewed, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator SkewedAtMaxima(TestContext t) => AtMaxima(t, Skewed, "StrangeAsteroidKleiFest2023Cluster", Groups.All);

		// ---- mini cluster, Badlands start: the plain upside-down asteroid (MiniFlipped) ----

		[OniTest("control: the Badlands mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = MiniBadlands, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBadlandsVanilla(TestContext t) => Support.RecordBaseline(t, MiniBadlands, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the Badlands mini cluster, MiniFlipped among them, has a flat wall at its kind's default thickness", Worldgen = MiniBadlands, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBadlandsFlat(TestContext t) => FlatWith(t, new Options(), "MiniFlipped", MiniKinds);

		[OniTest("SB: at the defaults the Badlands mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = MiniBadlands, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniBadlandsSame(TestContext t) => Identity.SameAndNoGrowth(t, MiniBadlands);

		[OniTest("SB: at the maxima every asteroid of the Badlands mini cluster, MiniFlipped among them, has a flat wall at its kind's maximum and keeps its inner neutronium", Worldgen = MiniBadlands, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniBadlandsAtMaxima(TestContext t) => AtMaxima(t, MiniBadlands, "MiniFlipped", MiniKinds);

		// ---- mini cluster, upside-down start (MiniFlippedStart) ----

		[OniTest("control: the upside-down mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = MiniFlipped, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFlippedVanilla(TestContext t) => Support.RecordBaseline(t, MiniFlipped, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the upside-down mini cluster, MiniFlippedStart among them, has a flat wall at its kind's default thickness", Worldgen = MiniFlipped, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFlippedFlat(TestContext t) => FlatWith(t, new Options(), "MiniFlippedStart", MiniKinds);

		[OniTest("SB: at the defaults the upside-down mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = MiniFlipped, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniFlippedSame(TestContext t) => Identity.SameAndNoGrowth(t, MiniFlipped);

		[OniTest("SB: at the maxima every asteroid of the upside-down mini cluster, MiniFlippedStart among them, has a flat wall at its kind's maximum and keeps its inner neutronium", Worldgen = MiniFlipped, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFlippedAtMaxima(TestContext t) => AtMaxima(t, MiniFlipped, "MiniFlippedStart", MiniKinds);

		// ---- mini cluster, frozen forest start (MiniFlipped) ----

		[OniTest("control: the frozen forest mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = MiniFrozen, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFrozenVanilla(TestContext t) => Support.RecordBaseline(t, MiniFrozen, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the frozen forest mini cluster has a flat wall at its kind's default thickness", Worldgen = MiniFrozen, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniFrozenFlat(TestContext t) => FlatWith(t, new Options(), "MiniForestFrozenStart", MiniKinds);

		[OniTest("SB: at the defaults the frozen forest mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = MiniFrozen, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniFrozenSame(TestContext t) => Identity.SameAndNoGrowth(t, MiniFrozen);

		// ---- mini cluster, metallic swampy start (MiniFlipped) ----

		[OniTest("control: the metallic swampy mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = MiniSwampy, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniSwampyVanilla(TestContext t) => Support.RecordBaseline(t, MiniSwampy, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the metallic swampy mini cluster has a flat wall at its kind's default thickness", Worldgen = MiniSwampy, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniSwampyFlat(TestContext t) => FlatWith(t, new Options(), "MiniMetallicSwampyStart", MiniKinds);

		[OniTest("SB: at the defaults the metallic swampy mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = MiniSwampy, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniSwampySame(TestContext t) => Identity.SameAndNoGrowth(t, MiniSwampy);

		// ---- mini cluster, radioactive ocean start: the upside-down warp asteroid (MiniFlippedWarp) ----

		[OniTest("control: the radioactive ocean mini cluster with the mod's border patches off; its buildings, geysers and inner neutronium are written down", Worldgen = MiniRadioactive, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniRadioactiveVanilla(TestContext t) => Support.RecordBaseline(t, MiniRadioactive, "vanilla");

		[OniTest("SB: at the defaults every asteroid of the radioactive ocean mini cluster, MiniFlippedWarp among them, has a flat wall at its kind's default thickness", Worldgen = MiniRadioactive, Prepare = nameof(Defaults), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniRadioactiveFlat(TestContext t) => FlatWith(t, new Options(), "MiniFlippedWarp", MiniKinds);

		[OniTest("SB: at the defaults the radioactive ocean mini cluster matches the game without the mod: buildings, geysers, inner neutronium", Worldgen = MiniRadioactive, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator MiniRadioactiveSame(TestContext t) => Identity.SameAndNoGrowth(t, MiniRadioactive);

		[OniTest("SB: at the maxima every asteroid of the radioactive ocean mini cluster, MiniFlippedWarp among them, has a flat wall at its kind's maximum and keeps its inner neutronium", Worldgen = MiniRadioactive, Prepare = nameof(Maxima), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator MiniRadioactiveAtMaxima(TestContext t) => AtMaxima(t, MiniRadioactive, "MiniFlippedWarp", MiniKinds);
	}
}
