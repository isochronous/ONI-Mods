using HarmonyLib;

namespace OvagroOverhead
{
	/// <summary>
	/// An Ovagro mother spawns its first branch beside it and calls
	/// <c>VineBranch.Instance.SetupRootInformation(VineMother.Instance)</c>, which always records the
	/// root's shape as Bottom: a vine lying on a floor. Every later branch copies its parent's shape,
	/// so the whole vine follows that first value. For a ceiling mother the postfix records Top, so the
	/// vine hangs along the ceiling. The branch's state machine starts after this call, so it reads the
	/// corrected value when it first picks its shape.
	/// </summary>
	[HarmonyPatch(typeof(VineBranch.Instance), nameof(VineBranch.Instance.SetupRootInformation), new[] { typeof(VineMother.Instance) })]
	public static class VineBranch_SetupRootInformation_Patch
	{
		public static void Postfix(VineBranch.Instance __instance, VineMother.Instance mother)
		{
			if (mother == null || mother.gameObject.PrefabID() != CeilingForm.PlantTag)
				return;
			__instance.sm.RootShape.Set((int)VineBranch.Shape.Top, __instance);
		}
	}
}
