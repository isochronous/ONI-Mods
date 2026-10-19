using System.Collections;
using Isochronous.TestKit;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// The mod away from its defaults, on Terrania (one asteroid of every kind). At every kind's
	/// maximum the wall is flat with no template cell in it (a hole would show as a gap), and the POI
	/// rectangles grew where the flat wall is deeper than the game's; at a mixed set, some kinds
	/// thinner and some thicker than the game, the wall is flat at each kind's own value. The second
	/// world is generated after the first in one session with another config, so it also shows the
	/// options are read again for each world.
	/// </summary>
	public static class Flatness
	{
		private const string Terrania = "SNDST-C-100-0-0-0";

		private static Options MaximaConfig => Support.Config(Groups.Primary.Max, Groups.Small.Max, Groups.Moonlets.Max, Groups.TinyMoonlets.Max);

		private static Options MixedConfig => Support.Config(7, 1, 5, 2);

		public static System.Action Maxima()
		{
			return Support.Configure(MaximaConfig);
		}

		public static System.Action Mixed()
		{
			return Support.Configure(MixedConfig);
		}

		[OniTest("SB: with every kind at its maximum every asteroid of Terrania has a flat wall at that thickness, no template cell in it", Worldgen = Terrania, Prepare = nameof(Maxima), Timeout = 90), Layer(0)]
		public static IEnumerator AtMaxima(TestContext t)
		{
			return Support.ExpectFlat(t, MaximaConfig, Groups.All);
		}

		[OniTest("SB: with every kind at its maximum the POI rectangles grew where the flat wall is deeper than the game's", Worldgen = Terrania, Prepare = nameof(Maxima), Timeout = 30), Layer(0)]
		public static IEnumerator RectanglesGrowAtMaxima(TestContext t)
		{
			foreach (string line in Support.GrewLines())
				t.Log(line);
			// At 16 tiles the primary's wall is deeper than the game's thickest on every edge (15 at most).
			t.Assert.True(Support.GrewLines().Exists(line => line.Contains("TerraMoonlet")),
				"the primary asteroid's POI rectangles grew (" + Support.GrewLines().Count + " grew line(s) in all)");
			yield break;
		}

		[OniTest("SB: with Primary 7, Small 1, Moonlets 5, Tiny moonlets 2 every asteroid of Terrania has a flat wall at its kind's value", Worldgen = Terrania, Prepare = nameof(Mixed), Timeout = 90), Layer(0)]
		public static IEnumerator AtMixedValues(TestContext t)
		{
			return Support.ExpectFlat(t, MixedConfig, Groups.All);
		}
	}
}
