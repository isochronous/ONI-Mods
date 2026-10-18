using HarmonyLib;
using UnityEngine;

namespace OvagroOverhead
{
	/// <summary>
	/// The planter side screen reads each seed's direction from its prefab. <c>Initialize</c> stores it
	/// with the seed's toggle and greys the toggle out when it differs from a rotated plot's direction,
	/// and <c>SetResultDescriptions</c> adds "rotate this to face the floor" for the same mismatch. For
	/// an upside-down Farm Tile or Hydroponic Farm both patches swap the Ovagro seed's prefab to the
	/// ceiling form for the call, so the toggle is enabled and the description shows the game's own
	/// "Requires ceiling" line. The finalizer restores the prefab whether or not the call throws.
	/// </summary>
	public static class SideScreenPatches
	{
		[HarmonyPatch(typeof(ReceptacleSideScreen), nameof(ReceptacleSideScreen.Initialize), new[] { typeof(SingleEntityReceptacle) })]
		public static class ReceptacleSideScreen_Initialize_Patch
		{
			public static void Prefix(SingleEntityReceptacle target, out CeilingForm.Swap __state)
			{
				__state = CeilingForm.FacesDown(target as PlantablePlot) ? CeilingForm.EnterPrefab() : default;
			}

			public static void Finalizer(CeilingForm.Swap __state)
			{
				CeilingForm.ExitPrefab(__state);
			}
		}

		[HarmonyPatch(typeof(PlanterSideScreen), "SetResultDescriptions", new[] { typeof(GameObject) })]
		public static class PlanterSideScreen_SetResultDescriptions_Patch
		{
			public static void Prefix(SingleEntityReceptacle ___targetReceptacle, out CeilingForm.Swap __state)
			{
				__state = CeilingForm.FacesDown(___targetReceptacle as PlantablePlot) ? CeilingForm.EnterPrefab() : default;
			}

			public static void Finalizer(CeilingForm.Swap __state)
			{
				CeilingForm.ExitPrefab(__state);
			}
		}
	}
}
