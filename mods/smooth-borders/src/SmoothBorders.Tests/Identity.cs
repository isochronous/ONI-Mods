using System.Collections;
using Isochronous.TestKit;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// The mod at its defaults, cluster by cluster. First a baseline world generated with the mod's
	/// border patches off (the game's own draw), whose buildings and geysers are written down; then
	/// the same coordinate with the mod on and the default config: every wall flat at the default
	/// thickness, and the buildings and geysers exactly where the baseline has them (the default is
	/// the game's thinnest wall, so the POI rectangles stay the game's). The flat and the placement
	/// tests share one world. Base-game coordinates are skipped in Spaced Out and the reverse; run
	/// them with the launcher's -Mode base. More clusters, explicit, are in Clusters.
	/// </summary>
	public static class Identity
	{
		private const string Terrania = "SNDST-C-100-0-0-0";
		private const string ClassicTerra = "V-SNDST-C-100-0-0-0";
		private const string BaseTerra = "SNDST-A-100-0-0-0";
		private const string Terrania200 = "SNDST-C-200-0-0-0";
		private const string Terrania300 = "SNDST-C-300-0-0-0";

		public static System.Action Vanilla()
		{
			return Support.VanillaDraw();
		}

		/// <summary>The same as Vanilla, under another name so that it is a world of its own.</summary>
		public static System.Action VanillaAgain()
		{
			return Support.VanillaDraw();
		}

		public static System.Action Defaults()
		{
			return Support.Configure(new Options());
		}

		// ---- Terrania (Spaced Out): TerraMoonlet, IdealLandingSite, the 4,2 moonlets, Marshy, Niobium, Regolith ----

		[OniTest("control: Terrania with the mod's border patches off has the game's uneven wall; its buildings and geysers are written down", Worldgen = Terrania, Prepare = nameof(Vanilla), Timeout = 90), Layer(0)]
		public static IEnumerator TerraniaVanilla(TestContext t)
		{
			return Support.RecordBaseline(t, Terrania, "vanilla");
		}

		[OniTest("control: Terrania generated a second time with the patches off puts its buildings and geysers where the first did", Worldgen = Terrania, Prepare = nameof(VanillaAgain), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator TerraniaVanillaAgain(TestContext t)
		{
			return Support.ExpectSamePlacements(t, Terrania, "vanilla");
		}

		[OniTest("SB: at the defaults every asteroid of Terrania has a flat wall at its kind's default thickness", Worldgen = Terrania, Prepare = nameof(Defaults), Timeout = 90), Layer(0)]
		public static IEnumerator TerraniaFlat(TestContext t)
		{
			return Support.ExpectFlat(t, new Options(), Groups.All);
		}

		[OniTest("SB: at the defaults Terrania's buildings and geysers sit where the game puts them without the mod, and no POI rectangle grew", Worldgen = Terrania, Prepare = nameof(Defaults), Timeout = 60), Layer(0)]
		public static IEnumerator TerraniaSame(TestContext t)
		{
			return SameAndNoGrowth(t, Terrania);
		}

		// ---- Terra, classic Spaced Out cluster: VanillaSandstoneDefault, MiniRegolithMoonlet, the moonlets ----

		[OniTest("control: classic Terra with the mod's border patches off has the game's uneven wall; its buildings and geysers are written down", Worldgen = ClassicTerra, Prepare = nameof(Vanilla), Timeout = 90), Layer(0)]
		public static IEnumerator ClassicTerraVanilla(TestContext t)
		{
			return Support.RecordBaseline(t, ClassicTerra, "vanilla");
		}

		[OniTest("SB: at the defaults every asteroid of classic Terra has a flat wall at its kind's default thickness", Worldgen = ClassicTerra, Prepare = nameof(Defaults), Timeout = 90), Layer(0)]
		public static IEnumerator ClassicTerraFlat(TestContext t)
		{
			return Support.ExpectFlat(t, new Options(), Groups.All);
		}

		[OniTest("SB: at the defaults classic Terra's buildings and geysers sit where the game puts them without the mod, and no POI rectangle grew", Worldgen = ClassicTerra, Prepare = nameof(Defaults), Timeout = 60), Layer(0)]
		public static IEnumerator ClassicTerraSame(TestContext t)
		{
			return SameAndNoGrowth(t, ClassicTerra);
		}

		// ---- Terra, base game: one primary asteroid ----

		[OniTest("control: base-game Terra with the mod's border patches off has the game's uneven wall; its buildings and geysers are written down", Worldgen = BaseTerra, Prepare = nameof(Vanilla), Timeout = 90), Layer(0)]
		public static IEnumerator BaseTerraVanilla(TestContext t)
		{
			return Support.RecordBaseline(t, BaseTerra, "vanilla");
		}

		[OniTest("SB: at the defaults base-game Terra has a flat wall at the primary default thickness", Worldgen = BaseTerra, Prepare = nameof(Defaults), Timeout = 90), Layer(0)]
		public static IEnumerator BaseTerraFlat(TestContext t)
		{
			return Support.ExpectFlat(t, new Options(), Groups.Primary);
		}

		[OniTest("SB: at the defaults base-game Terra's buildings and geysers sit where the game puts them without the mod, and no POI rectangle grew", Worldgen = BaseTerra, Prepare = nameof(Defaults), Timeout = 60), Layer(0)]
		public static IEnumerator BaseTerraSame(TestContext t)
		{
			return SameAndNoGrowth(t, BaseTerra);
		}

		// ---- more seeds, run with -Filter '*Identity.Terrania200*;*Identity.Terrania300*' ----

		[OniTest("control: Terrania seed 200 with the mod's border patches off; its buildings and geysers are written down", Worldgen = Terrania200, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator Terrania200Vanilla(TestContext t)
		{
			return Support.RecordBaseline(t, Terrania200, "vanilla");
		}

		[OniTest("SB: at the defaults Terrania seed 200's buildings and geysers sit where the game puts them without the mod", Worldgen = Terrania200, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator Terrania200Same(TestContext t)
		{
			return SameAndNoGrowth(t, Terrania200);
		}

		[OniTest("control: Terrania seed 300 with the mod's border patches off; its buildings and geysers are written down", Worldgen = Terrania300, Prepare = nameof(Vanilla), Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator Terrania300Vanilla(TestContext t)
		{
			return Support.RecordBaseline(t, Terrania300, "vanilla");
		}

		[OniTest("SB: at the defaults Terrania seed 300's buildings and geysers sit where the game puts them without the mod", Worldgen = Terrania300, Prepare = nameof(Defaults), Explicit = true, Timeout = 60), Layer(0)]
		public static IEnumerator Terrania300Same(TestContext t)
		{
			return SameAndNoGrowth(t, Terrania300);
		}

		/// <summary>
		/// The mod drew this world (its log has the flat walls, so this is not a second vanilla world),
		/// no edge's POI rectangle grew, the placements equal the baseline's, and the neutronium inside
		/// the border strips is the baseline's, cell for cell.
		/// </summary>
		internal static IEnumerator SameAndNoGrowth(TestContext t, string coordinate)
		{
			t.Assert.True(Support.FlatWalls().Count > 0, "the mod drew this world's walls (its log lines: " + string.Join(" | ", Support.Logged()) + ")");
			t.Assert.Eq(0, Support.GrewLines().Count, "no POI rectangle grew at the defaults: " + string.Join(" | ", Support.GrewLines()));
			yield return Support.ExpectSamePlacements(t, coordinate, "vanilla");
			yield return Support.ExpectInnerNeutroniumKept(t, coordinate, "vanilla", new Options(), true);
		}
	}
}
