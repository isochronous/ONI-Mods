using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// What the game lets one building be built over by another, read from the building
	/// definitions the way BuildTool and BuildingDef apply it. Nothing here extends the rules;
	/// a target is offered only if the vanilla build tool would accept the same replacement.
	/// </summary>
	internal static class ReplacementRules
	{
		/// <summary>
		/// A building type as the build menu shows it. Usually one def. Some mods make one def per
		/// material (Decor Pack's stained glass tiles: one per dye), list only one of them in the
		/// build menu and swap in the right one from the chosen material when the build tool starts;
		/// those defs come from the same config class, so they are grouped under the listed one.
		/// </summary>
		internal sealed class Family
		{
			public BuildingDef Representative;
			public readonly List<BuildingDef> Members = new List<BuildingDef>();

			/// <summary>The build menu's name, without the link markup the game's names carry.</summary>
			public string Name => STRINGS.UI.StripLinkFormatting(Representative.Name);
			public bool Contains(BuildingDef def) => def != null && Members.Contains(def);
		}

		/// <summary>Families that share a replacement layer: liquid pipes, tiles, ...</summary>
		internal sealed class Category
		{
			public ObjectLayer Layer;
			/// <summary>Names the category in the window (folding, object names): its layer, or its build menu category.</summary>
			public string Key;
			public string Name;
			public readonly List<Family> Families = new List<Family>();
		}

		private static List<Family> families;
		private static Dictionary<BuildingDef, Family> familyOf;
		private static int familiesBuiltFor = -1;

		/// <summary>True if buildings of this type can be built over at all.</summary>
		public static bool IsReplaceableSource(BuildingDef def)
		{
			return def != null && def.ReplacementLayer != ObjectLayer.NumLayers && def.Replaceable
				&& !def.Deprecated && def.BuildingComplete != null && def.CraftRecipe != null;
		}

		/// <summary>
		/// True if the build tool would accept dragging <paramref name="target"/> over a finished
		/// <paramref name="source"/>: the target names a replacement layer, the source carries one of
		/// the target's replacement tags, and the target looks for candidates on the layer the source
		/// occupies. The per-building check (GetReplacementCandidate at the cell) happens at queue time.
		/// </summary>
		public static bool CanReplaceDef(BuildingDef target, BuildingDef source)
		{
			return CanReplaceDef(target, source, FinishedExample(source));
		}

		/// <summary>
		/// As above, with <paramref name="example"/> a finished building of the source type (or
		/// null) standing in for the prefab in the tag check; see <see cref="CarriesReplacementTag"/>.
		/// </summary>
		public static bool CanReplaceDef(BuildingDef target, BuildingDef source, GameObject example)
		{
			if (target == null || source == null || target.ReplacementLayer == ObjectLayer.NumLayers)
				return false;
			if (!source.Replaceable || source.BuildingComplete == null)
				return false;
			// Utility buildings (wires, automation wires, pipes, rails) are dragged with the
			// utility build tool, whose BuildPath replaces whatever finished building sits on the
			// tile layer with a different type or material, tags or no tags; wires have none.
			if (target.isUtility && source.isUtility)
				return target.TileLayer != ObjectLayer.NumLayers && target.TileLayer == source.TileLayer;
			if (!CarriesReplacementTag(target, source, example))
				return false;
			if (target.ReplacementCandidateLayers != null)
				return target.ReplacementCandidateLayers.Contains(source.TileLayer)
					|| target.ReplacementCandidateLayers.Contains(source.ObjectLayer);
			return target.TileLayer != ObjectLayer.NumLayers && target.TileLayer == source.TileLayer;
		}

		/// <summary>
		/// Whether a finished <paramref name="source"/> carries one of the target's replacement tags,
		/// which is what the build tool asks of the building under the cursor. Most types tag their
		/// prefab in the config, but a tag a component adds in OnPrefabInit (Ladder adds
		/// GameTags.Ladders, so ladders, fire poles and plastic ladders can replace each other) is on
		/// every finished building and not on the prefab; a finished one answers for those, and
		/// failing that a ladder is taken at its component's word.
		/// </summary>
		private static bool CarriesReplacementTag(BuildingDef target, BuildingDef source, GameObject example)
		{
			if (target.ReplacementTags == null)
				return false;
			if (target.CanReplace(source.BuildingComplete))
				return true;
			if (example != null)
				return target.CanReplace(example);
			return source.BuildingComplete.GetComponent<Ladder>() != null && target.ReplacementTags.Contains(GameTags.Ladders);
		}

		/// <summary>A finished building of this type in the world, if there is one.</summary>
		public static GameObject FinishedExample(BuildingDef def)
		{
			if (def == null)
				return null;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
				if (building != null && building.Def == def)
					return building.gameObject;
			return null;
		}

		/// <summary>A finished building of any variant of this family, if there is one.</summary>
		public static GameObject FinishedExample(Family family)
		{
			if (family == null)
				return null;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
				if (building != null && family.Contains(building.Def))
					return building.gameObject;
			return null;
		}

		/// <summary>True if the player could pick this building in the build menu right now.</summary>
		public static bool IsBuildable(BuildingDef def)
		{
			if (def == null || !def.IsAvailable() || !def.ShouldShowInBuildMenu())
				return false;
			if (DebugHandler.InstantBuildMode || Game.Instance.SandboxModeActive)
				return true;
			return Db.Get().TechItems.IsTechItemComplete(def.PrefabID);
		}

		public static string CategoryName(ObjectLayer replacementLayer)
		{
			switch (replacementLayer)
			{
			case ObjectLayer.ReplacementLiquidConduit: return "Liquid Pipes";
			case ObjectLayer.ReplacementGasConduit: return "Ventilation Pipes";
			case ObjectLayer.ReplacementSolidConduit: return "Conveyor Rails";
			case ObjectLayer.ReplacementWire: return "Wires";
			case ObjectLayer.ReplacementLogicWire: return "Automation Wires";
			case ObjectLayer.ReplacementTile: return "Tiles";
			case ObjectLayer.ReplacementLadder: return "Ladders";
			case ObjectLayer.ReplacementTravelTube: return "Transit Tubes";
			case ObjectLayer.ReplacementBackwall: return "Backwalls";
			default: return replacementLayer.ToString().Replace("Replacement", "");
			}
		}

		// ---- families ----

		/// <summary>The family a def belongs to; a def of its own when it is not a variant of anything.</summary>
		public static Family FamilyOf(BuildingDef def)
		{
			EnsureFamilies();
			if (def != null && familyOf.TryGetValue(def, out Family family))
				return family;
			var single = new Family { Representative = def };
			single.Members.Add(def);
			return single;
		}

		private static void EnsureFamilies()
		{
			if (families != null && familiesBuiltFor == Assets.BuildingDefs.Count)
				return;
			familiesBuiltFor = Assets.BuildingDefs.Count;
			families = new List<Family>();
			familyOf = new Dictionary<BuildingDef, Family>();

			// What the build menu lists, by prefab id.
			var listed = new HashSet<string>();
			foreach (PlanScreen.PlanInfo plan in TUNING.BUILDINGS.PLANORDER)
				foreach (KeyValuePair<string, string> entry in plan.buildingAndSubcategoryData)
					listed.Add(entry.Key);

			// Which config class made each def. The table is private; without it every def stands alone.
			var byConfigType = new Dictionary<Type, List<BuildingDef>>();
			var configTable = AccessTools.Field(typeof(BuildingConfigManager), "configTable")?.GetValue(BuildingConfigManager.Instance)
				as Dictionary<IBuildingConfig, BuildingDef>;
			if (configTable != null)
				foreach (KeyValuePair<IBuildingConfig, BuildingDef> entry in configTable)
				{
					if (entry.Value == null)
						continue;
					Type type = entry.Key.GetType();
					if (!byConfigType.TryGetValue(type, out List<BuildingDef> defs))
						byConfigType[type] = defs = new List<BuildingDef>();
					defs.Add(entry.Value);
				}

			foreach (List<BuildingDef> defs in byConfigType.Values)
			{
				if (defs.Count < 2)
					continue;
				var visible = defs.FindAll(d => listed.Contains(d.PrefabID));
				// One listed def and the rest hidden: the hidden ones are its variants. Anything else
				// (several listed defs from one class, as a mod might do for a set of doors) stays apart.
				if (visible.Count != 1)
					continue;
				var family = new Family { Representative = visible[0] };
				family.Members.AddRange(defs);
				families.Add(family);
				foreach (BuildingDef def in defs)
					familyOf[def] = family;
			}
			foreach (BuildingDef def in Assets.BuildingDefs)
			{
				if (familyOf.ContainsKey(def))
					continue;
				var family = new Family { Representative = def };
				family.Members.Add(def);
				families.Add(family);
				familyOf[def] = family;
			}
		}

		/// <summary>Every replaceable type, grouped by replacement layer, in registration order (vanilla first, then mods).</summary>
		public static List<Category> SourceCategories()
		{
			// Only types something can be built over, so the Replace column has no dead ends.
			return Group(f => IsReplaceableSource(f.Representative) && TargetCategories(f).Count > 0);
		}

		/// <summary>Every buildable type that can be built over <paramref name="source"/>, grouped the same way.</summary>
		public static List<Category> TargetCategories(Family source)
		{
			GameObject example = FinishedExample(source);
			List<Category> categories = Group(f => CanReplaceDef(f.Representative, source.Representative, example) && IsBuildable(f.Representative) && f.Representative.CraftRecipe != null);
			// The source's own kind first (a ladder's ladders before the tiles that can also go
			// there), the rest in the order the build menu has them.
			int own = categories.FindIndex(c => c.Layer == source.Representative.ReplacementLayer);
			if (own > 0)
			{
				Category category = categories[own];
				categories.RemoveAt(own);
				categories.Insert(0, category);
			}
			return categories;
		}

		/// <summary>
		/// Every type <paramref name="include"/> takes, grouped by build menu category in the build
		/// menu's order (Base, Oxygen, Power...), each type once, under the entry the menu lists.
		/// </summary>
		internal static List<Category> GroupByBuildMenu(Predicate<Family> include)
		{
			EnsureFamilies();
			var result = new List<Category>();
			var seen = new HashSet<Family>();
			// The menu names buildings a DLC or mod may not have; only loaded ones are looked up.
			var byId = new Dictionary<string, BuildingDef>();
			foreach (BuildingDef loaded in Assets.BuildingDefs)
				if (loaded != null)
					byId[loaded.PrefabID] = loaded;
			foreach (PlanScreen.PlanInfo plan in TUNING.BUILDINGS.PLANORDER)
			{
				string id = HashCache.Get().Get(plan.category);
				var category = new Category
				{
					Layer = ObjectLayer.NumLayers,
					Key = "Menu" + id,
					Name = STRINGS.UI.StripLinkFormatting(Strings.Get("STRINGS.UI.BUILDCATEGORIES." + id.ToUpper() + ".NAME")),
				};
				foreach (KeyValuePair<string, string> entry in plan.buildingAndSubcategoryData)
				{
					if (!byId.TryGetValue(entry.Key, out BuildingDef def) || !familyOf.TryGetValue(def, out Family family) || family.Representative != def)
						continue;
					if (seen.Add(family) && include(family))
						category.Families.Add(family);
				}
				if (category.Families.Count > 0)
					result.Add(category);
			}
			return result;
		}

		internal static List<Category> Group(Predicate<Family> include)
		{
			EnsureFamilies();
			var byLayer = new Dictionary<ObjectLayer, Category>();
			var result = new List<Category>();
			foreach (BuildingDef def in Assets.BuildingDefs)
			{
				Family family = familyOf[def];
				if (family.Representative != def || !include(family))
					continue;
				ObjectLayer layer = def.ReplacementLayer;
				if (!byLayer.TryGetValue(layer, out Category category))
				{
					category = new Category { Layer = layer, Key = layer.ToString(), Name = CategoryName(layer) };
					byLayer.Add(layer, category);
					result.Add(category);
				}
				category.Families.Add(family);
			}
			return result;
		}

		// ---- materials ----

		/// <summary>
		/// What a finished building was built from, one tag per recipe ingredient: the materials the
		/// deconstruct would give back, or failing that the element it is made of.
		/// </summary>
		public static Tag[] MaterialsOf(BuildingComplete building)
		{
			Deconstructable deconstructable = building.GetComponent<Deconstructable>();
			if (deconstructable != null && deconstructable.constructionElements != null && deconstructable.constructionElements.Length > 0)
				return deconstructable.constructionElements;
			PrimaryElement element = building.GetComponent<PrimaryElement>();
			return element != null && element.Element != null ? new[] { element.Element.tag } : new Tag[0];
		}

		public static bool SameMaterials(IList<Tag> a, IList<Tag> b)
		{
			if (a == null || b == null || a.Count != b.Count)
				return false;
			for (int i = 0; i < a.Count; i++)
				if (a[i] != b[i])
					return false;
			return true;
		}

		/// <summary>
		/// The def to build for a family and a choice of materials. A single def is itself. For a
		/// family of variants the mod that made them decides, in a prefix on BuildTool.Activate that
		/// swaps the def for the chosen material: those prefixes are run here with the same arguments,
		/// so the swap is theirs, not a guess at their naming. Failing that, a finished building of
		/// the family with these exact materials shows which variant it was; failing that, the listed def.
		/// </summary>
		public static BuildingDef ResolveVariant(Family family, IList<Tag> materials)
		{
			if (family == null)
				return null;
			if (family.Members.Count == 1)
				return family.Representative;
			BuildingDef swapped = RunActivatePrefixes(family.Representative, materials);
			if (swapped != null && family.Contains(swapped))
				return swapped;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
				if (building != null && family.Contains(building.Def) && SameMaterials(MaterialsOf(building), materials))
					return building.Def;
			return family.Representative;
		}

		private static BuildingDef RunActivatePrefixes(BuildingDef def, IList<Tag> materials)
		{
			try
			{
				MethodInfo activate = AccessTools.Method(typeof(BuildTool), "Activate", new[] { typeof(BuildingDef), typeof(IList<Tag>) });
				HarmonyLib.Patches patches = activate != null ? Harmony.GetPatchInfo(activate) : null;
				if (patches == null)
					return null;
				BuildingDef current = def;
				foreach (Patch patch in patches.Prefixes)
				{
					MethodInfo method = patch.PatchMethod;
					ParameterInfo[] parameters = method.GetParameters();
					var args = new object[parameters.Length];
					int defIndex = -1;
					for (int i = 0; i < parameters.Length; i++)
					{
						ParameterInfo p = parameters[i];
						switch (p.Name)
						{
						case "__instance": args[i] = BuildTool.Instance; break;
						case "def": args[i] = current; defIndex = i; break;
						case "selected_elements": args[i] = materials; break;
						default: args[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType) : null; break;
						}
					}
					try
					{
						method.Invoke(null, args);
					}
					catch (Exception e)
					{
						Debug.Log("[UpgradePlanner] Build-tool prefix " + method.DeclaringType?.FullName + "." + method.Name + " refused a dry run: " + e.GetBaseException().Message);
						continue;
					}
					if (defIndex >= 0 && args[defIndex] is BuildingDef result && result != null)
						current = result;
				}
				return current;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not ask the build tool's patches for a variant: " + e.Message);
				return null;
			}
		}

		/// <summary>Materials the game would offer for one ingredient of a recipe.</summary>
		public static List<Tag> ValidMaterials(Recipe.Ingredient ingredient)
		{
			return MaterialSelector.GetValidMaterials(ingredient.tag, omitDisabledElements: true);
		}

		public static string MaterialName(Tag material)
		{
			Element element = ElementLoader.GetElement(material);
			return STRINGS.UI.StripLinkFormatting(element != null ? element.name : material.ProperName());
		}
	}
}
