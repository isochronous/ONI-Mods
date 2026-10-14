using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>
	/// Which building types the Change material tab offers, and which ingredient of a recipe a
	/// change of material swaps. A type qualifies when the game lets it be built over by itself, the
	/// same-type swap the build tool allows as long as the material changes: the old building works
	/// until the new one is built. With all buildings offered (Blueprints Expanded loaded), any other
	/// type the game can reconstruct qualifies too: it is deconstructed and rebuilt in the new
	/// material by the game's Reconstructable, out of service in between. Either way Blueprints
	/// Expanded keeps its settings.
	/// </summary>
	internal static class MaterialSwap
	{
		/// <summary>
		/// The result the pickers' recipe names. The game remembers the last material picked for a
		/// recipe's result and offers it first next time; this is a real building (the game asks the
		/// result for its def) that the player never builds, so no build menu choice is changed.
		/// </summary>
		private const string PickerResult = "Headquarters";

		/// <summary>Whether each family can be built over by itself, and whether it can be reconstructed; asked for every building a drag or highlight looks at, so kept until <see cref="Refresh"/>.</summary>
		private static readonly Dictionary<Family, bool> replaceable = new Dictionary<Family, bool>();
		private static readonly Dictionary<Family, bool> rebuildable = new Dictionary<Family, bool>();

		/// <summary>
		/// Every type the tab offers: the types that can be built over, by replacement layer as the
		/// Replace type tab groups them; with all buildings, by build menu category.
		/// </summary>
		public static List<Category> Categories(bool allBuildings)
		{
			if (allBuildings && BlueprintsCompat.Loaded)
				return Families.ByBuildMenu(f => Qualifies(f, true));
			return Families.ByLayer(Replaceable);
		}

		/// <summary>Looks at every type afresh (the window does, each time it opens): a ladder's tag shows on a finished one.</summary>
		public static void Refresh()
		{
			replaceable.Clear();
			rebuildable.Clear();
		}

		/// <summary>True if the tab offers the type: built over by itself, or with all buildings offered, reconstructed.</summary>
		public static bool Qualifies(Family family, bool allBuildings)
		{
			return Replaceable(family) || (allBuildings && BlueprintsCompat.Loaded && Rebuildable(family));
		}

		/// <summary>True if a building of this type is deconstructed and rebuilt, rather than built over.</summary>
		public static bool Rebuilds(BuildingDef def) => !Replaceable(Families.Of(def));

		private static bool Replaceable(Family family)
		{
			if (!replaceable.TryGetValue(family, out bool result))
				replaceable[family] = result = CheckReplaceable(family);
			return result;
		}

		private static bool Rebuildable(Family family)
		{
			if (!rebuildable.TryGetValue(family, out bool result))
				rebuildable[family] = result = CheckRebuildable(family);
			return result;
		}

		private static bool CheckReplaceable(Family family)
		{
			BuildingDef def = family.Representative;
			return ReplacementRules.IsReplaceable(def) && def.IsAvailable() && def.ShouldShowInBuildMenu()
				&& HasIngredients(def) && ReplacementRules.CanReplace(def, family);
		}

		/// <summary>A type of the build menu, made from materials, that the game can reconstruct.</summary>
		private static bool CheckRebuildable(Family family)
		{
			BuildingDef def = family.Representative;
			if (def == null || def.Deprecated || !def.IsAvailable() || !def.ShouldShowInBuildMenu() || def.BuildingComplete == null || !HasIngredients(def))
				return false;
			Deconstructable deconstructable = def.BuildingComplete.GetComponent<Deconstructable>();
			// A telepad (the mini-pod) turns its own deconstruction off once built.
			return def.BuildingComplete.GetComponent<Reconstructable>() != null && deconstructable != null && deconstructable.allowDeconstruction
				&& def.BuildingComplete.GetComponent<Telepad>() == null;
		}

		private static bool HasIngredients(BuildingDef def) => def.CraftRecipe?.Ingredients != null && def.CraftRecipe.Ingredients.Count > 0;

		/// <summary>
		/// The first ingredient of the type's recipe that can be made of both materials (an invalid
		/// tag accepts anything), or -1 when none can.
		/// </summary>
		public static int Slot(BuildingDef def, Tag from, Tag to)
		{
			List<Recipe.Ingredient> ingredients = def?.CraftRecipe?.Ingredients;
			if (ingredients == null)
				return -1;
			for (int i = 0; i < ingredients.Count; i++)
			{
				HashSet<Tag> valid = Materials.Valid(ingredients[i].tag);
				if ((!from.IsValid || valid.Contains(from)) && (!to.IsValid || valid.Contains(to)))
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Why the type cannot be changed from <paramref name="from"/> to <paramref name="to"/> (either
		/// invalid for not chosen), or null when it can.
		/// </summary>
		public static string Blocked(Family family, Tag from, Tag to)
		{
			BuildingDef def = family.Representative;
			if (!ReplacementRules.IsBuildable(def))
				return ModStrings.UPGRADEPLANNER.BlockedResearch;
			if (from.IsValid && Slot(def, from, Tag.Invalid) < 0)
				return ModStrings.UPGRADEPLANNER.BlockedSource.Replace("{0}", Materials.Name(from));
			if (to.IsValid && Slot(def, from, to) < 0)
				return ModStrings.UPGRADEPLANNER.BlockedTarget.Replace("{0}", Materials.Name(to));
			return null;
		}

		/// <summary>
		/// A recipe for the game's material picker that offers every material any offered type can
		/// be built from, in one row: the game reads an ingredient tag joined with '&amp;' as "any of
		/// these", and its picker selects one material per row.
		/// </summary>
		public static Recipe PickerRecipe()
		{
			var parts = new List<string>();
			foreach (Category category in Categories(true))
				foreach (Family family in category.Families)
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
