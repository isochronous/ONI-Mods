using HarmonyLib;
using UnityEngine;

namespace OvagrosOverhead
{
	/// <summary>
	/// A Farm Tile or Hydroponic Farm turned upside down has direction Bottom; the Ovagro seed has
	/// direction Top, so the game previews and plants it as a floor plant above the tile. These patches
	/// swap the seed to the ceiling form for one call on such a plot:
	/// <c>SpawnOccupyingObject</c> swaps the delivered seed, so the plot spawns the hanging plant (the
	/// game already moves it to the cell below the tile); <c>SetPreview</c> and <c>OnCopySettings</c>
	/// read the seed's prefab, so they swap the prefab. The finalizer restores the seed whether or not
	/// the call throws.
	/// </summary>
	public static class PlotPatches
	{
		[HarmonyPatch(typeof(PlantablePlot), "SpawnOccupyingObject", new[] { typeof(GameObject) })]
		public static class PlantablePlot_SpawnOccupyingObject_Patch
		{
			public static void Prefix(PlantablePlot __instance, GameObject depositedEntity, out CeilingForm.Swap __state)
			{
				__state = default;
				if (depositedEntity == null || !CeilingForm.FacesDown(__instance))
					return;
				PlantableSeed seed = depositedEntity.GetComponent<PlantableSeed>();
				if (CeilingForm.IsOvagro(seed))
					__state = CeilingForm.Enter(seed);
			}

			public static void Finalizer(GameObject depositedEntity, CeilingForm.Swap __state)
			{
				if (depositedEntity != null)
					CeilingForm.Exit(depositedEntity.GetComponent<PlantableSeed>(), __state);
			}
		}

		/// <summary>
		/// The swapped preview id also keeps the game's "same preview already shown" check consistent,
		/// and the R180 placement it picks for a Bottom seed puts the hanging preview below the tile.
		/// </summary>
		[HarmonyPatch(typeof(PlantablePlot), nameof(PlantablePlot.SetPreview), new[] { typeof(Tag), typeof(bool) })]
		public static class PlantablePlot_SetPreview_Patch
		{
			public static void Prefix(PlantablePlot __instance, Tag entityTag, out CeilingForm.Swap __state)
			{
				__state = entityTag == CeilingForm.SeedTag && CeilingForm.FacesDown(__instance) ? CeilingForm.EnterPrefab() : default;
			}

			public static void Finalizer(CeilingForm.Swap __state)
			{
				CeilingForm.ExitPrefab(__state);
			}
		}

		/// <summary>Copy Settings orders the copied seed only when its direction matches this plot's.</summary>
		[HarmonyPatch(typeof(PlantablePlot), "OnCopySettings", new[] { typeof(object) })]
		public static class PlantablePlot_OnCopySettings_Patch
		{
			public static void Prefix(PlantablePlot __instance, out CeilingForm.Swap __state)
			{
				__state = CeilingForm.FacesDown(__instance) ? CeilingForm.EnterPrefab() : default;
			}

			public static void Finalizer(CeilingForm.Swap __state)
			{
				CeilingForm.ExitPrefab(__state);
			}
		}
	}
}
