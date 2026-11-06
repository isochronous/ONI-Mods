using System;
using ProcGen;

namespace SmoothBorders
{
	/// <summary>
	/// The two border settings WorldGen.DrawWorldBorder reads, answered through
	/// <see cref="Setting"/>. While a world's draw is flat the thickness is the configured one and
	/// the range is 0, so the game's own loops draw every row and column exactly that deep; at all
	/// other times the world's real values come back unchanged.
	/// </summary>
	public static class Border
	{
		public const string ThicknessKey = "WorldBorderThickness";
		public const string RangeKey = "WorldBorderRange";

		/// <summary>
		/// Set by the transpiler when it found and swapped both setting reads. While it is false
		/// the prefix and postfix do nothing and the game draws its own border.
		/// </summary>
		public static bool Patched;

		// Worldgen runs on its own thread (one per world in a cluster), so the state of the world
		// being drawn is kept per thread.
		[ThreadStatic] private static bool flat;
		[ThreadStatic] private static int thickness;

		/// <summary>
		/// Stands in for settings.GetIntSetting(name) inside DrawWorldBorder: the flat thickness
		/// and range 0 while this thread's world is drawn flat, the real value otherwise.
		/// </summary>
		public static int Setting(WorldGenSettings settings, string name)
		{
			int value = settings.GetIntSetting(name);
			if (!flat)
				return value;
			if (name == ThicknessKey)
				return thickness;
			if (name == RangeKey)
				return 0;
			return value;
		}

		/// <summary>Draws this thread's next border flat at the given thickness.</summary>
		internal static void BeginFlat(int tiles)
		{
			thickness = tiles;
			flat = true;
		}

		/// <summary>Back to the game's own values on this thread.</summary>
		internal static void End()
		{
			flat = false;
			thickness = 0;
		}
	}
}
