using System;
using System.Collections.Generic;
using HarmonyLib;

namespace UpgradePlanner
{
	/// <summary>
	/// A building type as the build menu shows it. Usually one def. Some mods make one def per
	/// material (Decor Pack's stained glass tiles: one per dye), list only one of them in the build
	/// menu and swap in the right one from the chosen material when the build tool starts; those
	/// defs come from the same config class, so they are grouped under the listed one.
	/// </summary>
	internal sealed class Family
	{
		public readonly BuildingDef Representative;
		public readonly List<BuildingDef> Members = new List<BuildingDef>();

		public Family(BuildingDef representative)
		{
			Representative = representative;
		}

		public string PrefabID => Representative.PrefabID;

		/// <summary>The build menu's name, without the link markup the game's names carry.</summary>
		public string Name => STRINGS.UI.StripLinkFormatting(Representative.Name);

		public bool Contains(BuildingDef def) => def != null && Members.Contains(def);
	}

	/// <summary>A heading of families in the window: a replacement layer (liquid pipes, tiles...) or a build menu category.</summary>
	internal sealed class Category
	{
		/// <summary>Names the category in the window's objects and the folded set: its layer, or its build menu category.</summary>
		public readonly string Key;
		public readonly string Name;
		/// <summary>The replacement layer its families share; NumLayers for a build menu category.</summary>
		public readonly ObjectLayer Layer;
		public readonly List<Family> Families = new List<Family>();

		public Category(string key, string name, ObjectLayer layer)
		{
			Key = key;
			Name = name;
			Layer = layer;
		}
	}

	/// <summary>Every building type as a <see cref="Family"/>, and the two ways the window groups them.</summary>
	internal static class Families
	{
		private static List<Family> all;
		private static Dictionary<BuildingDef, Family> byDef;
		private static int builtForDefs = -1;

		/// <summary>The family a def belongs to; a family of its own when it is not a variant of anything.</summary>
		public static Family Of(BuildingDef def)
		{
			Ensure();
			if (def != null && byDef.TryGetValue(def, out Family family))
				return family;
			var single = new Family(def);
			single.Members.Add(def);
			return single;
		}

		/// <summary>Every type <paramref name="include"/> takes, by replacement layer, in registration order (vanilla first, then mods).</summary>
		public static List<Category> ByLayer(Predicate<Family> include)
		{
			Ensure();
			var byLayer = new Dictionary<ObjectLayer, Category>();
			var result = new List<Category>();
			foreach (Family family in all)
			{
				if (!include(family))
					continue;
				ObjectLayer layer = family.Representative.ReplacementLayer;
				if (!byLayer.TryGetValue(layer, out Category category))
				{
					category = new Category(layer.ToString(), LayerName(layer), layer);
					byLayer.Add(layer, category);
					result.Add(category);
				}
				category.Families.Add(family);
			}
			return result;
		}

		/// <summary>
		/// Every type <paramref name="include"/> takes, by build menu category in the build menu's
		/// order (Base, Oxygen, Power...), each type once, under the entry the menu lists.
		/// </summary>
		public static List<Category> ByBuildMenu(Predicate<Family> include)
		{
			Ensure();
			var result = new List<Category>();
			var seen = new HashSet<Family>();
			// The menu names buildings a DLC or mod may not have; only loaded ones are looked up.
			var loaded = new Dictionary<string, BuildingDef>();
			foreach (BuildingDef def in Assets.BuildingDefs)
				if (def != null)
					loaded[def.PrefabID] = def;
			foreach (PlanScreen.PlanInfo plan in TUNING.BUILDINGS.PLANORDER)
			{
				string id = HashCache.Get().Get(plan.category);
				var category = new Category("Menu" + id,
					STRINGS.UI.StripLinkFormatting(Strings.Get("STRINGS.UI.BUILDCATEGORIES." + id.ToUpper() + ".NAME")), ObjectLayer.NumLayers);
				foreach (KeyValuePair<string, string> entry in plan.buildingAndSubcategoryData)
				{
					if (!loaded.TryGetValue(entry.Key, out BuildingDef def) || !byDef.TryGetValue(def, out Family family) || family.Representative != def)
						continue;
					if (seen.Add(family) && include(family))
						category.Families.Add(family);
				}
				if (category.Families.Count > 0)
					result.Add(category);
			}
			return result;
		}

		private static string LayerName(ObjectLayer replacementLayer)
		{
			switch (replacementLayer)
			{
			case ObjectLayer.ReplacementLiquidConduit: return ModStrings.UPGRADEPLANNER.FamilyLiquidPipes;
			case ObjectLayer.ReplacementGasConduit: return ModStrings.UPGRADEPLANNER.FamilyVentilationPipes;
			case ObjectLayer.ReplacementSolidConduit: return ModStrings.UPGRADEPLANNER.FamilyConveyorRails;
			case ObjectLayer.ReplacementWire: return ModStrings.UPGRADEPLANNER.FamilyWires;
			case ObjectLayer.ReplacementLogicWire: return ModStrings.UPGRADEPLANNER.FamilyAutomationWires;
			case ObjectLayer.ReplacementTile: return ModStrings.UPGRADEPLANNER.FamilyTiles;
			case ObjectLayer.ReplacementLadder: return ModStrings.UPGRADEPLANNER.FamilyLadders;
			case ObjectLayer.ReplacementTravelTube: return ModStrings.UPGRADEPLANNER.FamilyTransitTubes;
			case ObjectLayer.ReplacementBackwall: return ModStrings.UPGRADEPLANNER.FamilyBackwalls;
			default: return replacementLayer.ToString().Replace("Replacement", "");
			}
		}

		/// <summary>
		/// Groups the defs, once per set of loaded defs. Defs made by one config class, of which
		/// exactly one is in the build menu, are that one's variants; every other def stands alone.
		/// </summary>
		private static void Ensure()
		{
			if (all != null && builtForDefs == Assets.BuildingDefs.Count)
				return;
			builtForDefs = Assets.BuildingDefs.Count;
			all = new List<Family>();
			byDef = new Dictionary<BuildingDef, Family>();

			var listed = new HashSet<string>();
			foreach (PlanScreen.PlanInfo plan in TUNING.BUILDINGS.PLANORDER)
				foreach (KeyValuePair<string, string> entry in plan.buildingAndSubcategoryData)
					listed.Add(entry.Key);

			// Which config class made each def. The table is private; without it every def stands alone.
			var byConfig = new Dictionary<Type, List<BuildingDef>>();
			if (AccessTools.Field(typeof(BuildingConfigManager), "configTable")?.GetValue(BuildingConfigManager.Instance)
				is Dictionary<IBuildingConfig, BuildingDef> configTable)
				foreach (KeyValuePair<IBuildingConfig, BuildingDef> entry in configTable)
				{
					if (entry.Value == null)
						continue;
					Type type = entry.Key.GetType();
					if (!byConfig.TryGetValue(type, out List<BuildingDef> defs))
						byConfig[type] = defs = new List<BuildingDef>();
					defs.Add(entry.Value);
				}
			foreach (List<BuildingDef> defs in byConfig.Values)
			{
				if (defs.Count < 2)
					continue;
				// One listed def and the rest hidden: the hidden ones are its variants. Anything else
				// (several listed defs from one class, as a mod might do for a set of doors) stays apart.
				List<BuildingDef> visible = defs.FindAll(d => listed.Contains(d.PrefabID));
				if (visible.Count != 1)
					continue;
				var family = new Family(visible[0]);
				family.Members.AddRange(defs);
				foreach (BuildingDef member in defs)
					byDef[member] = family;
			}

			// In the order the game registered the defs (vanilla first, then mods), each family once,
			// where its listed def is.
			foreach (BuildingDef def in Assets.BuildingDefs)
			{
				if (def == null)
					continue;
				if (!byDef.TryGetValue(def, out Family family))
				{
					family = new Family(def);
					family.Members.Add(def);
					byDef[def] = family;
				}
				if (family.Representative == def)
					all.Add(family);
			}
		}
	}
}
