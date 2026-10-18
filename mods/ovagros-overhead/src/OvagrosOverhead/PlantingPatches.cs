using HarmonyLib;

namespace OvagrosOverhead
{
	/// <summary>
	/// A Pip plants a seed in three game calls: <c>PlantableCellQuery.CheckValidPlotCell</c> tests
	/// each cell it can reach while it looks for a place, <c>SeedPlantingStates.CheckValidPlotCell</c>
	/// tests the chosen cell again when the planting animation ends, and <c>PlantableSeed.TryPlant</c>
	/// spawns the plant at the Pip's own cell. All three read the seed's direction and plant id. Where
	/// the cell has solid ground above it, each patch swaps an Ovagro seed to the ceiling form for the
	/// length of the call, so the game tests the ceiling and spawns the hanging plant. The finalizer
	/// restores the seed whether or not the call throws.
	/// </summary>
	public static class PlantingPatches
	{
		[HarmonyPatch(typeof(PlantableCellQuery), "CheckValidPlotCell", new[] { typeof(PlantableSeed), typeof(int) })]
		public static class PlantableCellQuery_CheckValidPlotCell_Patch
		{
			public static void Prefix(PlantableSeed seed, int plant_cell, out CeilingForm.Swap __state)
			{
				__state = CeilingForm.IsOvagro(seed) && CeilingForm.WantsCeiling(plant_cell) ? CeilingForm.Enter(seed) : default;
			}

			public static void Finalizer(PlantableSeed seed, CeilingForm.Swap __state)
			{
				CeilingForm.Exit(seed, __state);
			}
		}

		[HarmonyPatch(typeof(SeedPlantingStates), "CheckValidPlotCell",
			new[] { typeof(SeedPlantingStates.Instance), typeof(PlantableSeed), typeof(int), typeof(PlantablePlot) },
			new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
		public static class SeedPlantingStates_CheckValidPlotCell_Patch
		{
			public static void Prefix(PlantableSeed seed, int cell, out CeilingForm.Swap __state)
			{
				__state = CeilingForm.IsOvagro(seed) && CeilingForm.WantsCeiling(cell) ? CeilingForm.Enter(seed) : default;
			}

			public static void Finalizer(PlantableSeed seed, CeilingForm.Swap __state)
			{
				CeilingForm.Exit(seed, __state);
			}
		}

		/// <summary>The seed is in the Pip's storage here, so its cell is the Pip's cell, which the game plants at.</summary>
		[HarmonyPatch(typeof(PlantableSeed), nameof(PlantableSeed.TryPlant), new[] { typeof(bool) })]
		public static class PlantableSeed_TryPlant_Patch
		{
			public static void Prefix(PlantableSeed __instance, out CeilingForm.Swap __state)
			{
				__state = CeilingForm.IsOvagro(__instance) && CeilingForm.WantsCeiling(Grid.PosToCell(__instance))
					? CeilingForm.Enter(__instance)
					: default;
			}

			public static void Finalizer(PlantableSeed __instance, CeilingForm.Swap __state)
			{
				CeilingForm.Exit(__instance, __state);
			}
		}
	}
}
