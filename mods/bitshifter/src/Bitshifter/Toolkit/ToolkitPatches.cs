using HarmonyLib;
using PeterHan.PLib.UI;

namespace Bitshifter
{
	/// <summary>
	/// The toolkit's patches. Each carries the toolkit's Harmony category, so BitshifterMod.OnLoad
	/// applies them on their own and they stay in place when the mod leaves the Reader and Writer
	/// to another mod. A patch here without the category would stand down with the Reader and
	/// Writer patches instead.
	/// </summary>
	public static class ToolkitPatches
	{
		/// <summary>Puts each toolkit building in the build menu, after the building its row names.</summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
		{
			public static void Prefix()
			{
				foreach (Toolkit.Entry entry in Toolkit.Buildings)
					ModUtil.AddBuildingToPlanScreen(Toolkit.PlanCategory, entry.Id, entry.Subcategory, entry.After, ModUtil.BuildingOrdering.After);
			}
		}

		/// <summary>Adds each toolkit building to its tech; a tech the game does not have leaves the building unlocked from the start.</summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Postfix()
			{
				foreach (Toolkit.Entry entry in Toolkit.Buildings)
				{
					Tech tech = Db.Get().Techs.TryGet(entry.Tech);
					if (tech == null)
					{
						Debug.LogWarning("[Bitshifter] Tech '" + entry.Tech + "' not found; " + entry.Id + " will be unlocked from the start");
						continue;
					}
					if (!tech.unlockedItemIDs.Contains(entry.Id))
						tech.unlockedItemIDs.Add(entry.Id);
				}
			}
		}

		/// <summary>Registers the toolkit's side screens (PLib) with the details screen.</summary>
		[HarmonyPatchCategory(Toolkit.PatchCategory)]
		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class DetailsScreen_OnPrefabInit_Patch
		{
			public static void Postfix()
			{
				PUIUtils.AddSideScreenContent<RibbonDiagnosticPanelSideScreen>();
			}
		}
	}
}
