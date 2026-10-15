using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// Picks a few of many cases that between them show every trait the many show, for a by-hand run
	/// that covers what the planner and the game do differently by kind of building without doing
	/// every building. The traits are what the planner's orders and the game's construction branch
	/// on, and the settings a building holds; a type the game adds later brings its traits along.
	/// </summary>
	internal static class Coverage
	{
		/// <summary>
		/// The fewest cases, taken greedily, whose traits together are every trait of
		/// <paramref name="cases"/>: each step takes the case showing the most traits not yet shown,
		/// the earliest of equals.
		/// </summary>
		internal static List<T> Cover<T>(IList<T> cases, Func<T, HashSet<string>> traitsOf)
		{
			List<HashSet<string>> traits = cases.Select(traitsOf).ToList();
			var unshown = new HashSet<string>(traits.SelectMany(set => set));
			var chosen = new List<T>();
			while (unshown.Count > 0)
			{
				int best = -1, bestCount = 0;
				for (int i = 0; i < cases.Count; i++)
				{
					int count = traits[i].Count(unshown.Contains);
					if (count > bestCount)
					{
						best = i;
						bestCount = count;
					}
				}
				chosen.Add(cases[best]);
				unshown.ExceptWith(traits[best]);
			}
			return chosen;
		}

		/// <summary>
		/// What sets a type apart for a by-hand change: whether it is rebuilt or built over, where and
		/// how it stands, what it is made of, who may build it, its ports and parts, and with
		/// <paramref name="settings"/> each setting kind it holds.
		/// </summary>
		internal static HashSet<string> Of(BuildingDef def, bool settings)
		{
			var traits = new HashSet<string>
			{
				MaterialSwap.Rebuilds(def) ? "rebuilt" : "built over",
				"layer " + def.ObjectLayer,
				"placement " + def.BuildLocationRule,
				"size " + (def.WidthInCells > 1 ? "wide" : "narrow") + " " + (def.HeightInCells > 1 ? "tall" : "short"),
				def.CraftRecipe.Ingredients.Count + " ingredient(s)",
			};
			if (def.IsTilePiece)
				traits.Add("tile");
			if (def.isUtility)
				traits.Add("utility");
			if (def.PermittedRotations != PermittedRotations.Unrotatable)
				traits.Add("rotations " + def.PermittedRotations);
			List<Tag> materials = BuildingSettings.Materials(def);
			if (materials != null && materials.Any(tag => ElementLoader.GetElement(tag) == null))
				traits.Add("item ingredient");
			if (!string.IsNullOrEmpty(def.RequiredSkillPerkID))
				traits.Add("needs a skill");
			if (def.InputConduitType != ConduitType.None)
				traits.Add("input " + def.InputConduitType);
			if (def.OutputConduitType != ConduitType.None)
				traits.Add("output " + def.OutputConduitType);
			if (def.RequiresPowerInput)
				traits.Add("power in");
			if (def.GeneratorWattageRating > 0f)
				traits.Add("power out");
			if (def.LogicInputPorts?.Count > 0 || def.LogicOutputPorts?.Count > 0)
				traits.Add("logic ports");
			GameObject prefab = def.BuildingComplete;
			if (prefab.GetComponent<Door>() != null)
				traits.Add("door");
			if (prefab.GetComponent<Storage>() != null)
				traits.Add("storage");
			if (settings)
			{
				// A setting kind is named for the component or interface that holds it; the kinds of
				// other mods (dotted names) sit on nearly every building and are shown by any of them.
				HashSet<string> names = Names(prefab);
				foreach (string kind in SettingKinds.All.Keys)
					if (kind.IndexOf('.') < 0 && names.Contains(kind))
						traits.Add("setting " + kind);
			}
			return traits;
		}

		/// <summary>The names of the prefab's components, their base types and their interfaces.</summary>
		private static HashSet<string> Names(GameObject prefab)
		{
			var names = new HashSet<string>();
			foreach (Component component in prefab.GetComponents<Component>())
			{
				if (component == null)
					continue;
				for (Type type = component.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
					names.Add(type.Name);
				foreach (Type type in component.GetType().GetInterfaces())
					names.Add(type.Name);
			}
			return names;
		}
	}
}
