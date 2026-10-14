using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>
	/// The Change material tab: which building types can be rebuilt in another material, why a
	/// type is left out for a choice of materials, and what a finished building is rebuilt with.
	/// A type qualifies when the game lets it be built over by itself, the same-type swap the
	/// build tool allows as long as the material changes.
	/// </summary>
	internal static class MaterialSwap
	{
		/// <summary>
		/// The result the pickers' recipe names. The game remembers the last material picked for a
		/// recipe's result and offers it first next time; this is a real building (the game asks the
		/// result for its def) that the player never builds, so no build menu choice is changed.
		/// </summary>
		private const string PickerResult = "Headquarters";

		private static readonly Dictionary<Tag, HashSet<Tag>> validFor = new Dictionary<Tag, HashSet<Tag>>();

		/// <summary>The materials the game offers for an ingredient tag (a tag joined with '&amp;' offers each part's).</summary>
		public static HashSet<Tag> Valid(Tag ingredientTag)
		{
			if (!validFor.TryGetValue(ingredientTag, out HashSet<Tag> valid))
			{
				valid = new HashSet<Tag>(MaterialSelector.GetValidMaterials(ingredientTag, omitDisabledElements: true));
				validFor[ingredientTag] = valid;
			}
			return valid;
		}

		/// <summary>Whether each family qualifies; asked once per building on every highlight pass, so kept.</summary>
		private static readonly Dictionary<ReplacementRules.Family, bool> qualifies = new Dictionary<ReplacementRules.Family, bool>();

		/// <summary>Every type the tab offers, grouped as the Replace type tab groups them.</summary>
		public static List<ReplacementRules.Category> Categories()
		{
			return ReplacementRules.Group(Qualifies);
		}

		/// <summary>Looks at every type afresh (the window does, each time it opens): a ladder's tag shows on a finished one.</summary>
		public static void Refresh()
		{
			qualifies.Clear();
		}

		private static bool Qualifies(ReplacementRules.Family family)
		{
			if (!qualifies.TryGetValue(family, out bool result))
				qualifies[family] = result = Check(family);
			return result;
		}

		private static bool Check(ReplacementRules.Family family)
		{
			BuildingDef def = family.Representative;
			return ReplacementRules.IsReplaceableSource(def) && def.IsAvailable() && def.ShouldShowInBuildMenu()
				&& def.CraftRecipe.Ingredients != null && def.CraftRecipe.Ingredients.Count > 0
				&& ReplacementRules.CanReplaceDef(def, def, ReplacementRules.FinishedExample(family));
		}

		/// <summary>
		/// The first ingredient of the type's recipe that can be made of both materials; an invalid
		/// tag accepts anything. -1 when none can.
		/// </summary>
		public static int Slot(BuildingDef def, Tag from, Tag to)
		{
			List<Recipe.Ingredient> ingredients = def?.CraftRecipe?.Ingredients;
			if (ingredients == null)
				return -1;
			for (int i = 0; i < ingredients.Count; i++)
			{
				HashSet<Tag> valid = Valid(ingredients[i].tag);
				if ((!from.IsValid || valid.Contains(from)) && (!to.IsValid || valid.Contains(to)))
					return i;
			}
			return -1;
		}

		/// <summary>Why the type cannot be part of the plan as it stands, or null when it can.</summary>
		public static string Blocked(ReplacementRules.Family family, UpgradePlan plan)
		{
			BuildingDef def = family.Representative;
			if (!ReplacementRules.IsBuildable(def))
				return ModStrings.BlockedResearch;
			Tag from = plan.MaterialSourceMode == UpgradePlan.MaterialMode.Specific ? plan.MaterialSource : Tag.Invalid;
			if (from.IsValid && Slot(def, from, Tag.Invalid) < 0)
				return ModStrings.BlockedSource.Replace("{0}", ReplacementRules.MaterialName(from));
			if (plan.MaterialTarget.IsValid && Slot(def, from, plan.MaterialTarget) < 0)
				return ModStrings.BlockedTarget.Replace("{0}", ReplacementRules.MaterialName(plan.MaterialTarget));
			return null;
		}

		/// <summary>
		/// What a finished building is rebuilt as under a complete plan: the same type (the variant
		/// of its family the new materials stand for), with the ingredient the plan targets swapped
		/// for the new material. False when the plan leaves it alone: its type is not picked or is
		/// left out, it is not made of the material to replace, or it is already made of the new one.
		/// </summary>
		/// <param name="resolve">False to skip working out the variant (which can ask other mods' patches): matching only.</param>
		public static bool TryPlan(UpgradePlan plan, BuildingComplete building, out BuildingDef target, out List<Tag> materials, bool resolve = true)
		{
			target = null;
			materials = null;
			if (building == null || !plan.MaterialTarget.IsValid)
				return false;
			ReplacementRules.Family family = ReplacementRules.FamilyOf(building.Def);
			if (!plan.IsMaterialTypeSelected(family.Representative.PrefabID) || !Qualifies(family) || Blocked(family, plan) != null)
				return false;
			bool specific = plan.MaterialSourceMode == UpgradePlan.MaterialMode.Specific;
			if (specific && !plan.MaterialSource.IsValid)
				return false;
			int slot = Slot(building.Def, specific ? plan.MaterialSource : Tag.Invalid, plan.MaterialTarget);
			Tag[] current = ReplacementRules.MaterialsOf(building);
			if (slot < 0 || slot >= current.Length)
				return false;
			if (specific && current[slot] != plan.MaterialSource)
				return false;
			if (current[slot] == plan.MaterialTarget)
				return false;
			materials = new List<Tag>(current);
			materials[slot] = plan.MaterialTarget;
			if (!resolve)
				return true;
			target = ReplacementRules.ResolveVariant(family, materials);
			return target != null;
		}

		/// <summary>
		/// A recipe for the game's material picker that offers every material any offered type can
		/// be built from, in one row: the game reads an ingredient tag joined with '&amp;' as "any of
		/// these", and its picker selects one material per row.
		/// </summary>
		public static Recipe PickerRecipe()
		{
			var parts = new List<string>();
			foreach (ReplacementRules.Category category in Categories())
				foreach (ReplacementRules.Family family in category.Families)
					foreach (Recipe.Ingredient ingredient in family.Representative.CraftRecipe.Ingredients)
						foreach (string part in ingredient.tag.Name.Split('&'))
							if (part.Length > 0 && !parts.Contains(part))
								parts.Add(part);
			if (parts.Count == 0 || Assets.TryGetPrefab(PickerResult) == null)
				return null;
			var recipe = new Recipe(PickerResult);
			recipe.AddIngredient(new Recipe.Ingredient(string.Join("&", parts), 0f));
			return recipe;
		}
	}
}
