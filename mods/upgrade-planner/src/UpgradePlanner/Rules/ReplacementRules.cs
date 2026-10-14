using System.Collections.Generic;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// What the game lets one building be built over by another, read from the building
	/// definitions the way BuildTool and BuildingDef apply it. Nothing here extends the rules; a
	/// type is offered only if the game's own build tool would accept the same replacement.
	/// </summary>
	internal static class ReplacementRules
	{
		/// <summary>True if buildings of this type can be built over at all.</summary>
		public static bool IsReplaceable(BuildingDef def)
		{
			return def != null && def.ReplacementLayer != ObjectLayer.NumLayers && def.Replaceable
				&& !def.Deprecated && def.BuildingComplete != null && def.CraftRecipe != null;
		}

		/// <summary>
		/// True if the build tool would accept dragging <paramref name="target"/> over a finished
		/// <paramref name="source"/>: the target names a replacement layer, the source carries one of
		/// the target's replacement tags, and the target looks for candidates on the layer the source
		/// occupies. The per-building check (the candidate at the cell) happens when the order is placed.
		/// </summary>
		public static bool CanReplace(BuildingDef target, Family source)
		{
			BuildingDef def = source.Representative;
			if (target == null || def == null || target.ReplacementLayer == ObjectLayer.NumLayers)
				return false;
			if (!def.Replaceable || def.BuildingComplete == null)
				return false;
			// Utility buildings (wires, automation wires, pipes, rails) are dragged with the utility
			// build tool, whose BuildPath replaces whatever finished building sits on the tile layer
			// with a different type or material, tags or no tags; wires have none.
			if (target.isUtility && def.isUtility)
				return target.TileLayer != ObjectLayer.NumLayers && target.TileLayer == def.TileLayer;
			if (!CarriesReplacementTag(target, source))
				return false;
			if (target.ReplacementCandidateLayers != null)
				return target.ReplacementCandidateLayers.Contains(def.TileLayer) || target.ReplacementCandidateLayers.Contains(def.ObjectLayer);
			return target.TileLayer != ObjectLayer.NumLayers && target.TileLayer == def.TileLayer;
		}

		/// <summary>
		/// Whether a finished building of the source type carries one of the target's replacement
		/// tags, which is what the build tool asks of the building under the cursor. Most types tag
		/// their prefab in the config, but a tag a component adds in OnPrefabInit (Ladder adds
		/// GameTags.Ladders, so ladders, fire poles and plastic ladders can replace each other) is on
		/// every finished building and not on the prefab: a finished one answers for those, and
		/// failing that a ladder is taken at its component's word.
		/// </summary>
		private static bool CarriesReplacementTag(BuildingDef target, Family source)
		{
			if (target.ReplacementTags == null)
				return false;
			GameObject prefab = source.Representative.BuildingComplete;
			if (target.CanReplace(prefab))
				return true;
			GameObject example = Examples.Of(source);
			if (example != null)
				return target.CanReplace(example);
			return prefab.GetComponent<Ladder>() != null && target.ReplacementTags.Contains(GameTags.Ladders);
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

		/// <summary>Every type something can be built over, by replacement layer: the Replace type tab's Replace column.</summary>
		public static List<Category> SourceCategories()
		{
			// Only types something can be built over, so the column has no dead ends.
			return Families.ByLayer(f => IsReplaceable(f.Representative) && HasTarget(f));
		}

		/// <summary>Every buildable type that can be built over <paramref name="source"/>, by replacement layer, the source's own first.</summary>
		public static List<Category> TargetCategories(Family source)
		{
			List<Category> categories = Families.ByLayer(f => IsTarget(f, source));
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

		private static bool IsTarget(Family target, Family source)
		{
			BuildingDef def = target.Representative;
			return def.CraftRecipe != null && CanReplace(def, source) && IsBuildable(def);
		}

		private static bool HasTarget(Family source)
		{
			return Families.ByLayer(f => IsTarget(f, source)).Count > 0;
		}
	}

	/// <summary>
	/// A finished building of each type in the world, looked up once a frame at most: a type's
	/// finished building can carry tags its prefab does not (see ReplacementRules), and a family of
	/// variants shows which variant a choice of materials stands for.
	/// </summary>
	internal static class Examples
	{
		private static readonly Dictionary<BuildingDef, BuildingComplete> byDef = new Dictionary<BuildingDef, BuildingComplete>();
		private static int frame = -1;

		public static GameObject Of(BuildingDef def)
		{
			Scan();
			return def != null && byDef.TryGetValue(def, out BuildingComplete building) && building != null ? building.gameObject : null;
		}

		/// <summary>A finished building of any variant of the family.</summary>
		public static GameObject Of(Family family)
		{
			foreach (BuildingDef def in family.Members)
			{
				GameObject example = Of(def);
				if (example != null)
					return example;
			}
			return null;
		}

		private static void Scan()
		{
			if (frame == Time.frameCount)
				return;
			frame = Time.frameCount;
			byDef.Clear();
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
				if (building != null && !byDef.ContainsKey(building.Def))
					byDef[building.Def] = building;
		}
	}
}
