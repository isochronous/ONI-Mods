using System;

namespace SmoothBorders
{
	/// <summary>One kind of asteroid: its setting key, default and allowed range in tiles.</summary>
	public sealed class Group
	{
		public readonly string Id;
		public readonly int Default;
		public readonly int Min;
		public readonly int Max;

		public Group(string id, int defaultValue, int min, int max)
		{
			Id = id;
			Default = defaultValue;
			Min = min;
			Max = max;
		}

		/// <summary>The value limited to this group's range.</summary>
		public int Clamp(int value)
		{
			return Math.Max(Min, Math.Min(Max, value));
		}

		/// <summary>The range as shown after the label, such as "1-16".</summary>
		public string RangeText
		{
			get { return Min + "-" + Max; }
		}
	}

	/// <summary>
	/// The one group table and the mapping from a world's border settings to its group. Pure
	/// functions with no game types, so tests without a colony can call them.
	/// </summary>
	public static class Groups
	{
		// Ranges run from 1 to the thickest wall the game draws for the kind (T + R); defaults are
		// the thinnest (T - R).
		public static readonly Group Primary = new Group("Primary", 4, 1, 16);
		public static readonly Group Small = new Group("Small", 3, 1, 7);
		public static readonly Group Moonlets = new Group("Moonlets", 2, 1, 6);
		public static readonly Group TinyMoonlets = new Group("TinyMoonlets", 1, 1, 3);

		public static readonly Group[] All = { Primary, Small, Moonlets, TinyMoonlets };

		/// <summary>
		/// The group of a world with the border thickness t and range r (WorldBorderThickness and
		/// WorldBorderRange), or null when the pair is not one the game uses.
		/// </summary>
		public static Group ForWorld(int t, int r)
		{
			if (t == 10 && r == 6) return Primary;
			if ((t == 5 && r == 2) || (t == 4 && r == 1)) return Small;
			if ((t == 4 && r == 2) || (t == 3 && r == 1)) return Moonlets;
			if ((t == 2 && r == 1) || (t == 1 && r == 0)) return TinyMoonlets;
			return null;
		}

		/// <summary>
		/// The flat wall thickness for a world: its group's setting, or for a pair outside the
		/// table the thinnest wall the game draws there (at least 1). A thickness of 0 or less
		/// means the world has no wall and stays 0.
		/// </summary>
		public static int Thickness(int t, int r, Options options)
		{
			if (t <= 0)
				return 0;
			Group group = ForWorld(t, r);
			if (group == null)
				return Math.Max(1, t - r);
			return options.ValueFor(group);
		}
	}
}
