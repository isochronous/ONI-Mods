using System;
using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>
	/// The Change material tab: which types to rebuild, the material to replace (any, or one) and
	/// the one material to rebuild them in, and whether all buildings are offered or only the ones
	/// the game lets you build over.
	/// </summary>
	internal sealed class ChangeMaterialPlan : IPlanTab
	{
		/// <summary>How many type names the footer lists before it counts the rest.</summary>
		private const int NamesShown = 4;

		private readonly System.Action changed;
		/// <summary>The picked types by the prefab id of the type the build menu lists; null for a fresh plan, which picks every type offered.</summary>
		private HashSet<string> picked;

		public ChangeMaterialPlan(System.Action changed)
		{
			this.changed = changed;
		}

		/// <summary>Any or Specific; never Unset.</summary>
		public MaterialMode SourceMode { get; private set; } = MaterialMode.Any;
		/// <summary>The material to replace, with Specific; Tag.Invalid until picked.</summary>
		public Tag Source { get; private set; } = Tag.Invalid;
		/// <summary>The material to rebuild in; Tag.Invalid until picked.</summary>
		public Tag Target { get; private set; } = Tag.Invalid;
		/// <summary>Offer every building the game can reconstruct, not only the ones it lets you build over.</summary>
		public bool AllBuildings { get; private set; }

		/// <summary>The material to replace as the rules take it: Tag.Invalid for any.</summary>
		private Tag From => SourceMode == MaterialMode.Specific ? Source : Tag.Invalid;

		/// <summary>The material to replace is settled: any, or one that differs from the new one.</summary>
		public bool SourceComplete => SourceMode == MaterialMode.Any || (Source.IsValid && Source != Target);

		public bool IsComplete => SourceComplete && Target.IsValid && UsableTypes().Count > 0;

		// ---- choices ----

		public void SetSourceAny()
		{
			SourceMode = MaterialMode.Any;
			changed();
		}

		/// <summary>Replace only buildings of one material; the material itself comes from the picker.</summary>
		public void SetSourceSpecific()
		{
			SourceMode = MaterialMode.Specific;
			changed();
		}

		public void SetSource(Tag material)
		{
			SourceMode = MaterialMode.Specific;
			Source = material;
			changed();
		}

		public void SetTarget(Tag material)
		{
			Target = material;
			changed();
		}

		public bool IsPicked(string prefabId) => picked == null || picked.Contains(prefabId);

		public bool IsPicked(Family family) => IsPicked(family.PrefabID);

		/// <summary>Picks or drops the types. The first change turns a fresh plan's "every type" into a list of the ones offered.</summary>
		public void Pick(IEnumerable<Family> families, bool pick)
		{
			if (picked == null)
				picked = new HashSet<string>(OfferedIds());
			foreach (Family family in families)
			{
				if (pick)
					picked.Add(family.PrefabID);
				else
					picked.Remove(family.PrefabID);
			}
			changed();
		}

		/// <summary>
		/// Offers all buildings, or only the ones that can be built over. The types newly offered
		/// start unpicked: a plan that picked everything picks only what it offered before, so turning
		/// this on never sends every machine in a drag to be rebuilt.
		/// </summary>
		public void SetAllBuildings(bool all)
		{
			if (AllBuildings == all)
				return;
			if (picked == null)
				picked = new HashSet<string>(OfferedIds());
			AllBuildings = all;
			changed();
		}

		/// <summary>Nothing picked: no types, any material, no material to build in, only buildings that can be built over.</summary>
		public void Clear()
		{
			picked = new HashSet<string>();
			AllBuildings = false;
			SourceMode = MaterialMode.Any;
			Source = Tag.Invalid;
			Target = Tag.Invalid;
			changed();
		}

		// ---- what the choices allow ----

		/// <summary>Every type the tab offers, as it lists them.</summary>
		public List<Category> Offered() => MaterialSwap.Categories(AllBuildings);

		private IEnumerable<string> OfferedIds()
		{
			foreach (Category category in Offered())
				foreach (Family family in category.Families)
					yield return family.PrefabID;
		}

		/// <summary>Why the chosen materials rule the type out, or null when they allow it.</summary>
		public string Blocked(Family family) => MaterialSwap.Blocked(family, From, Target);

		/// <summary>The picked types the chosen materials allow, in the order the tab lists them.</summary>
		public List<Family> UsableTypes()
		{
			var usable = new List<Family>();
			foreach (Category category in Offered())
				foreach (Family family in category.Families)
					if (IsPicked(family) && Blocked(family) == null)
						usable.Add(family);
			return usable;
		}

		public List<BuildingDef> TouchedDefs()
		{
			var defs = new List<BuildingDef>();
			foreach (Family family in UsableTypes())
				defs.AddRange(family.Members);
			return defs;
		}

		public Matcher Compile() => IsComplete ? new MaterialMatcher(this, UsableTypes()) : null;

		// ---- presets ----

		public void Save(PlanPreset preset)
		{
			preset.Types = new List<string>(OfferedIds());
			preset.Types.RemoveAll(id => !IsPicked(id));
			preset.AllBuildings = AllBuildings;
			preset.SourceMode = SourceMode.ToString();
			preset.Source = Source.IsValid ? Source.Name : null;
			preset.Target = Target.IsValid ? Target.Name : null;
		}

		public void Load(PlanPreset preset)
		{
			picked = new HashSet<string>(preset.Types ?? new List<string>());
			AllBuildings = preset.AllBuildings;
			SourceMode = preset.SourceMaterialMode == MaterialMode.Specific ? MaterialMode.Specific : MaterialMode.Any;
			Source = PlanPreset.TagOf(preset.Source);
			Target = PlanPreset.TagOf(preset.Target);
			changed();
		}

		// ---- text ----

		public string Summary()
		{
			if (SourceMode == MaterialMode.Specific && !Source.IsValid)
				return ModStrings.UPGRADEPLANNER.MaterialPromptSource;
			if (!Target.IsValid)
				return ModStrings.UPGRADEPLANNER.MaterialPromptTarget;
			if (SourceMode == MaterialMode.Specific && Source == Target)
				return ModStrings.UPGRADEPLANNER.MaterialSameMaterial;
			List<Family> usable = UsableTypes();
			string target = Materials.Name(Target);
			if (usable.Count == 0)
				return ModStrings.UPGRADEPLANNER.MaterialPromptTypes.Replace("{0}", target);
			string from = SourceMode == MaterialMode.Specific ? Materials.Name(Source) : (string)ModStrings.UPGRADEPLANNER.AnyMaterialLower;
			return ModStrings.UPGRADEPLANNER.MaterialSummary.Replace("{0}", from).Replace("{1}", target).Replace("{2}", NameList(usable));
		}

		/// <summary>"A, B and C", or "A, B, C, D and 7 more" past <see cref="NamesShown"/> names.</summary>
		private static string NameList(List<Family> families)
		{
			var names = new List<string>();
			for (int i = 0; i < families.Count && i < NamesShown; i++)
				names.Add(families[i].Name);
			int more = families.Count - names.Count;
			string separator = ModStrings.UPGRADEPLANNER.ListSeparator;
			if (more > 0)
				return ModStrings.UPGRADEPLANNER.ListAndMore.Replace("{0}", string.Join(separator, names)).Replace("{1}", more.ToString());
			if (names.Count == 1)
				return names[0];
			return ModStrings.UPGRADEPLANNER.ListAnd.Replace("{0}", string.Join(separator, names.GetRange(0, names.Count - 1))).Replace("{1}", names[names.Count - 1]);
		}

		/// <summary>
		/// The plan for each building: its type is one of the usable ones, it is made of the material
		/// to replace in the first ingredient that can be made of both, and not already of the new
		/// one; it is rebuilt as the same type (the variant its new materials stand for) with that
		/// ingredient swapped, the others as they were.
		/// </summary>
		private sealed class MaterialMatcher : Matcher
		{
			private readonly Dictionary<BuildingDef, Family> families = new Dictionary<BuildingDef, Family>();
			private readonly Dictionary<BuildingDef, int> slots = new Dictionary<BuildingDef, int>();
			private readonly Tag from;
			private readonly Tag to;

			public MaterialMatcher(ChangeMaterialPlan plan, List<Family> usable) : base(plan.TouchedDefs())
			{
				from = plan.From;
				to = plan.Target;
				foreach (Family family in usable)
					foreach (BuildingDef def in family.Members)
					{
						families[def] = family;
						slots[def] = MaterialSwap.Slot(def, from, to);
					}
			}

			public override bool Matches(BuildingComplete building) => NewMaterials(building, out _, out _) != null;

			public override bool TryPlan(BuildingComplete building, out Upgrade upgrade)
			{
				upgrade = default;
				List<Tag> materials = NewMaterials(building, out Family family, out bool rebuild);
				if (materials == null)
					return false;
				upgrade = new Upgrade { Target = Resolve(family, materials), Materials = materials, Rebuild = rebuild };
				return upgrade.Target != null;
			}

			private List<Tag> NewMaterials(BuildingComplete building, out Family family, out bool rebuild)
			{
				rebuild = false;
				if (building == null || !families.TryGetValue(building.Def, out family))
				{
					family = null;
					return null;
				}
				rebuild = MaterialSwap.Rebuilds(building.Def);
				// What the game will not reconstruct, asked of the building itself.
				if (rebuild)
				{
					Reconstructable reconstructable = building.GetComponent<Reconstructable>();
					if (reconstructable == null || !reconstructable.AllowReconstruct)
						return null;
				}
				int slot = slots[building.Def];
				Tag[] current = Materials.Of(building);
				if (slot < 0 || slot >= current.Length || current[slot] == to || (from.IsValid && current[slot] != from))
					return null;
				var materials = new List<Tag>(current);
				materials[slot] = to;
				return materials;
			}
		}
	}
}
