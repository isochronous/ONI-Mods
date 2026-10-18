namespace OvagroOverhead
{
	/// <summary>
	/// Routes the one vanilla Ovagro seed to the ceiling form of the plant. The game decides where a
	/// seed goes and what it spawns from three plain fields on its <c>PlantableSeed</c>:
	/// <c>direction</c> (Top for the Ovagro seed, so it needs a floor), <c>PlantID</c> and
	/// <c>PreviewID</c>. None of them is saved. For the length of one synchronous game call a patch
	/// enters the swap, so the call sees a Bottom seed that plants and previews
	/// <see cref="VineMotherCeilingConfig"/>'s prefabs, and a finalizer exits it, so an exception
	/// cannot leave a seed swapped.
	///
	/// Which form a place gets is exact: the floor form needs solid ground below and no solid above,
	/// the ceiling form needs solid above and no solid below, so no cell can host both. A wild cell
	/// wants the ceiling form when the cell above it is solid; a farm plot wants it when it is a
	/// rotatable plot turned upside down (a flipped Farm Tile or Hydroponic Farm).
	/// </summary>
	public static class CeilingForm
	{
		/// <summary>Saved in save files; never rename.</summary>
		public const string PlantId = "VineMotherCeiling";
		public const string PreviewId = PlantId + "_preview";

		public static readonly Tag PlantTag = TagManager.Create(PlantId);
		public static readonly Tag PreviewTag = TagManager.Create(PreviewId);
		public static readonly Tag VanillaPlantTag = TagManager.Create(VineMotherConfig.ID);
		public static readonly Tag SeedTag = TagManager.Create(VineMotherConfig.SEED_ID);

		/// <summary>A seed's routing fields as they were before <see cref="Enter"/>.</summary>
		public struct Swap
		{
			public bool Active;
			public SingleEntityReceptacle.ReceptacleDirection Direction;
			public Tag PlantID;
			public Tag PreviewID;
		}

		/// <summary>An Ovagro seed in its vanilla (floor) form.</summary>
		public static bool IsOvagro(PlantableSeed seed)
		{
			return seed != null && seed.PlantID == VanillaPlantTag
				&& seed.direction == SingleEntityReceptacle.ReceptacleDirection.Top;
		}

		/// <summary>Points the seed at the ceiling form. Pass the result to <see cref="Exit"/>.</summary>
		public static Swap Enter(PlantableSeed seed)
		{
			var swap = new Swap
			{
				Active = true,
				Direction = seed.direction,
				PlantID = seed.PlantID,
				PreviewID = seed.PreviewID,
			};
			seed.direction = SingleEntityReceptacle.ReceptacleDirection.Bottom;
			seed.PlantID = PlantTag;
			seed.PreviewID = PreviewTag;
			return swap;
		}

		/// <summary>Restores what <see cref="Enter"/> changed; does nothing for a swap that was never entered.</summary>
		public static void Exit(PlantableSeed seed, Swap swap)
		{
			if (!swap.Active || seed == null)
				return;
			seed.direction = swap.Direction;
			seed.PlantID = swap.PlantID;
			seed.PreviewID = swap.PreviewID;
		}

		/// <summary>A wild planting cell under a solid cell, where only the ceiling form fits.</summary>
		public static bool WantsCeiling(int cell)
		{
			if (!Grid.IsValidCell(cell))
				return false;
			int above = Grid.CellAbove(cell);
			return Grid.IsValidCell(above) && Grid.Solid[above];
		}

		/// <summary>A rotatable farm plot turned upside down: a flipped Farm Tile or Hydroponic Farm.</summary>
		public static bool FacesDown(PlantablePlot plot)
		{
			return plot != null && plot.rotatable != null
				&& plot.Direction == SingleEntityReceptacle.ReceptacleDirection.Bottom;
		}
	}
}
