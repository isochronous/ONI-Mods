using System.Collections.Generic;
using System.Text;

namespace UpgradePlanner
{
	/// <summary>
	/// The current choices, for both tabs of the window. Replace type: a building type as the build
	/// menu lists it (in any material, or one material per recipe ingredient) and what to replace it
	/// with (a buildable type and one material per ingredient). Change material: which types, the
	/// material to replace (any, or one) and the one material to rebuild them in. Kept for the whole
	/// session, so re-selecting the tool resumes the last plan; presets keep plans across sessions.
	/// </summary>
	internal sealed class UpgradePlan
	{
		public enum MaterialMode { Unset, Any, Specific }

		/// <summary>Which tab the plan is on, and so which half of it a drag uses.</summary>
		public enum PlanMode { ChangeMaterial, ReplaceType }

		/// <summary>How many type names the footer lists before it counts the rest.</summary>
		private const int NamesShown = 4;

		public static readonly UpgradePlan Current = new UpgradePlan();

		public PlanMode Mode { get; private set; } = PlanMode.ChangeMaterial;

		// ---- Replace type ----

		public ReplacementRules.Family Source { get; private set; }
		public MaterialMode SourceMode { get; private set; } = MaterialMode.Unset;
		/// <summary>One entry per ingredient of the source's recipe; Tag.Invalid until chosen.</summary>
		public Tag[] SourceMaterials { get; private set; } = new Tag[0];
		public ReplacementRules.Family Target { get; private set; }
		/// <summary>One entry per ingredient of the target's recipe; Tag.Invalid until chosen.</summary>
		public Tag[] TargetMaterials { get; private set; } = new Tag[0];

		// ---- Change material ----

		/// <summary>The picked types, by the prefab id of the type the build menu lists; null until first set, meaning all.</summary>
		private HashSet<string> materialTypes;
		public MaterialMode MaterialSourceMode { get; private set; } = MaterialMode.Any;
		public Tag MaterialSource { get; private set; } = Tag.Invalid;
		public Tag MaterialTarget { get; private set; } = Tag.Invalid;

		/// <summary>Bumped on every change, so caches (the highlight, the dialog) know to refresh.</summary>
		public int Version { get; private set; }

		public BuildingDef SourceDef => Source?.Representative;

		public bool SourceComplete => Source != null
			&& (SourceMode == MaterialMode.Any || (SourceMode == MaterialMode.Specific && AllValid(SourceMaterials)));

		public bool TargetComplete => Target != null && TargetMaterials.Length > 0 && AllValid(TargetMaterials);

		/// <summary>The material to replace is settled: any, or one that differs from the new one.</summary>
		public bool MaterialSourceComplete => MaterialSourceMode == MaterialMode.Any
			|| (MaterialSourceMode == MaterialMode.Specific && MaterialSource.IsValid && MaterialSource != MaterialTarget);

		/// <summary>Everything a drag needs on the current tab.</summary>
		public bool IsComplete => Mode == PlanMode.ReplaceType
			? SourceComplete && TargetComplete
			: MaterialSourceComplete && MaterialTarget.IsValid && UsableTypes().Count > 0;

		/// <summary>Enough is chosen to show which buildings would be touched.</summary>
		public bool CanHighlight => Mode == PlanMode.ReplaceType ? SourceComplete : IsComplete;

		private static bool AllValid(Tag[] tags)
		{
			foreach (Tag tag in tags)
				if (!tag.IsValid)
					return false;
			return true;
		}

		public void SetMode(PlanMode mode)
		{
			if (Mode == mode)
				return;
			Mode = mode;
			Version++;
		}

		// ---- Replace type ----

		public void SetSource(ReplacementRules.Family family)
		{
			if (Source == family)
				return;
			Source = family;
			SourceMode = MaterialMode.Unset;
			SourceMaterials = new Tag[family?.Representative.CraftRecipe?.Ingredients?.Count ?? 0];
			for (int i = 0; i < SourceMaterials.Length; i++)
				SourceMaterials[i] = Tag.Invalid;
			SetTarget(null);
			Version++;
		}

		/// <summary>Back to nothing chosen on the current tab, as a fresh plan.</summary>
		public void Clear()
		{
			if (Mode == PlanMode.ChangeMaterial)
			{
				materialTypes = new HashSet<string>();
				MaterialSourceMode = MaterialMode.Any;
				MaterialSource = Tag.Invalid;
				MaterialTarget = Tag.Invalid;
				Version++;
				return;
			}
			Source = null;
			SourceMode = MaterialMode.Unset;
			SourceMaterials = new Tag[0];
			SetTarget(null);
		}

		public void SetSourceAnyMaterial()
		{
			SourceMode = MaterialMode.Any;
			Version++;
		}

		/// <summary>Switches to one material per ingredient; the materials themselves come from the picker.</summary>
		public void SetSourceSpecific()
		{
			SourceMode = MaterialMode.Specific;
			Version++;
		}

		public void SetSourceMaterials(IList<Tag> materials)
		{
			SourceMode = MaterialMode.Specific;
			for (int i = 0; i < SourceMaterials.Length; i++)
				SourceMaterials[i] = i < materials.Count && materials[i] != null ? materials[i] : Tag.Invalid;
			Version++;
		}

		public void SetTarget(ReplacementRules.Family family)
		{
			Target = family;
			int ingredients = family?.Representative.CraftRecipe?.Ingredients?.Count ?? 0;
			TargetMaterials = new Tag[ingredients];
			for (int i = 0; i < ingredients; i++)
				TargetMaterials[i] = Tag.Invalid;
			Version++;
		}

		public void SetTargetMaterials(IList<Tag> materials)
		{
			for (int i = 0; i < TargetMaterials.Length; i++)
				TargetMaterials[i] = i < materials.Count && materials[i] != null ? materials[i] : Tag.Invalid;
			Version++;
		}

		// ---- Change material ----

		public bool IsMaterialTypeSelected(string prefabId) => materialTypes == null || materialTypes.Contains(prefabId);

		/// <summary>Picks or drops the types; the first change turns "all, by default" into an explicit list of the offered ones.</summary>
		public void SetMaterialTypesSelected(IEnumerable<ReplacementRules.Family> families, bool selected)
		{
			if (materialTypes == null)
			{
				materialTypes = new HashSet<string>();
				foreach (ReplacementRules.Category category in MaterialSwap.Categories())
					foreach (ReplacementRules.Family family in category.Families)
						materialTypes.Add(family.Representative.PrefabID);
			}
			foreach (ReplacementRules.Family family in families)
			{
				if (selected)
					materialTypes.Add(family.Representative.PrefabID);
				else
					materialTypes.Remove(family.Representative.PrefabID);
			}
			Version++;
		}

		public void SetMaterialSourceAny()
		{
			MaterialSourceMode = MaterialMode.Any;
			Version++;
		}

		public void SetMaterialSourceSpecific()
		{
			MaterialSourceMode = MaterialMode.Specific;
			Version++;
		}

		public void SetMaterialSource(Tag material)
		{
			MaterialSourceMode = MaterialMode.Specific;
			MaterialSource = material.IsValid ? material : Tag.Invalid;
			Version++;
		}

		public void SetMaterialTarget(Tag material)
		{
			MaterialTarget = material.IsValid ? material : Tag.Invalid;
			Version++;
		}

		private List<ReplacementRules.Family> usableCache;
		private int usableVersion = -1;
		private float usableTime;

		/// <summary>
		/// The picked types the current materials allow, in the order the tab lists them. Asked on
		/// every cell of a drag, so kept until the plan changes, and for a second at most, since
		/// research and sandbox mode change what is allowed.
		/// </summary>
		public List<ReplacementRules.Family> UsableTypes()
		{
			float now = UnityEngine.Time.unscaledTime;
			if (usableCache != null && usableVersion == Version && now - usableTime < 1f)
				return usableCache;
			usableVersion = Version;
			usableTime = now;
			var usable = usableCache = new List<ReplacementRules.Family>();
			foreach (ReplacementRules.Category category in MaterialSwap.Categories())
				foreach (ReplacementRules.Family family in category.Families)
					if (IsMaterialTypeSelected(family.Representative.PrefabID) && MaterialSwap.Blocked(family, this) == null)
						usable.Add(family);
			return usable;
		}

		// ---- what a drag does ----

		/// <summary>True if this finished building is one the plan replaces.</summary>
		public bool Matches(BuildingComplete building)
		{
			if (Mode == PlanMode.ChangeMaterial)
				return MaterialSwap.TryPlan(this, building, out _, out _, resolve: false);
			if (building == null || Source == null || !Source.Contains(building.Def))
				return false;
			if (SourceMode == MaterialMode.Any)
				return true;
			if (SourceMode != MaterialMode.Specific || !AllValid(SourceMaterials))
				return false;
			Tag[] materials = ReplacementRules.MaterialsOf(building);
			for (int i = 0; i < SourceMaterials.Length; i++)
				if (i >= materials.Length || materials[i] != SourceMaterials[i])
					return false;
			return true;
		}

		/// <summary>
		/// What a matching building is rebuilt as: the def to build and its materials. The Replace
		/// type tab builds its chosen type (the variant the materials stand for) in its chosen
		/// materials; the Change material tab rebuilds the building's own type in the new material.
		/// </summary>
		public bool TryResolve(BuildingComplete building, out BuildingDef target, out List<Tag> materials)
		{
			if (Mode == PlanMode.ChangeMaterial)
				return MaterialSwap.TryPlan(this, building, out target, out materials);
			target = null;
			materials = null;
			if (!IsComplete || !Matches(building))
				return false;
			materials = new List<Tag>(TargetMaterials);
			target = ReplacementRules.ResolveVariant(Target, materials);
			return target != null;
		}

		/// <summary>The types a drag can touch, whose layers it looks at and whose overlay it shows.</summary>
		public List<BuildingDef> TouchedDefs()
		{
			var defs = new List<BuildingDef>();
			if (Mode == PlanMode.ReplaceType)
			{
				if (SourceDef != null)
					defs.Add(SourceDef);
				return defs;
			}
			foreach (ReplacementRules.Family family in UsableTypes())
				defs.Add(family.Representative);
			return defs;
		}

		// ---- presets ----

		public PlanPreset ToPreset(string name)
		{
			var preset = new PlanPreset { Name = name, Mode = Mode.ToString() };
			if (Mode == PlanMode.ChangeMaterial)
			{
				preset.Types = new List<string>();
				foreach (ReplacementRules.Category category in MaterialSwap.Categories())
					foreach (ReplacementRules.Family family in category.Families)
						if (IsMaterialTypeSelected(family.Representative.PrefabID))
							preset.Types.Add(family.Representative.PrefabID);
				preset.SourceMode = MaterialSourceMode.ToString();
				preset.Source = MaterialSource.IsValid ? MaterialSource.Name : null;
				preset.Target = MaterialTarget.IsValid ? MaterialTarget.Name : null;
				return preset;
			}
			preset.SourceType = SourceDef?.PrefabID;
			preset.SourceMode = SourceMode.ToString();
			preset.SourceMaterials = Names(SourceMaterials);
			preset.TargetType = Target?.Representative.PrefabID;
			preset.TargetMaterials = Names(TargetMaterials);
			return preset;
		}

		/// <summary>Takes on a saved plan, on its own tab. A type the game no longer has (a mod removed) is left out.</summary>
		public void Apply(PlanPreset preset)
		{
			if (preset == null)
				return;
			if (!System.Enum.TryParse(preset.Mode, out PlanMode mode))
				mode = PlanMode.ChangeMaterial;
			Mode = mode;
			System.Enum.TryParse(preset.SourceMode, out MaterialMode sourceMode);
			if (mode == PlanMode.ChangeMaterial)
			{
				materialTypes = new HashSet<string>(preset.Types ?? new List<string>());
				MaterialSourceMode = sourceMode == MaterialMode.Specific ? MaterialMode.Specific : MaterialMode.Any;
				MaterialSource = TagOf(preset.Source);
				MaterialTarget = TagOf(preset.Target);
				Version++;
				return;
			}
			BuildingDef source = preset.SourceType != null ? Assets.GetBuildingDef(preset.SourceType) : null;
			SetSource(source != null ? ReplacementRules.FamilyOf(source) : null);
			if (Source == null)
				return;
			if (sourceMode == MaterialMode.Any)
				SetSourceAnyMaterial();
			else if (sourceMode == MaterialMode.Specific)
				SetSourceMaterials(Tags(preset.SourceMaterials));
			BuildingDef target = preset.TargetType != null ? Assets.GetBuildingDef(preset.TargetType) : null;
			if (target == null)
				return;
			SetTarget(ReplacementRules.FamilyOf(target));
			SetTargetMaterials(Tags(preset.TargetMaterials));
		}

		private static List<string> Names(Tag[] tags)
		{
			var names = new List<string>();
			foreach (Tag tag in tags)
				names.Add(tag.IsValid ? tag.Name : null);
			return names;
		}

		private static Tag TagOf(string name) => string.IsNullOrEmpty(name) ? Tag.Invalid : new Tag(name);

		private static List<Tag> Tags(List<string> names)
		{
			var tags = new List<Tag>();
			if (names != null)
				foreach (string name in names)
					tags.Add(TagOf(name));
			return tags;
		}

		// ---- text ----

		/// <summary>What is chosen so far, for the dialog footer.</summary>
		public string Summary()
		{
			if (Mode == PlanMode.ChangeMaterial)
				return MaterialSummary();
			if (Source == null)
				return "Nothing chosen yet.";
			var text = new StringBuilder(Source.Name);
			if (SourceMode == MaterialMode.Any)
				text.Append(" (any material)");
			else if (SourceMode == MaterialMode.Specific && AllValid(SourceMaterials))
				text.Append(" (").Append(DisplayNames(SourceMaterials)).Append(")");
			if (Target != null)
			{
				text.Append(" -> ").Append(Target.Name);
				if (TargetComplete)
					text.Append(" (").Append(DisplayNames(TargetMaterials)).Append(")");
			}
			return text.ToString();
		}

		/// <summary>The Change material footer: what a drag does, the types named up to a point and the rest counted.</summary>
		private string MaterialSummary()
		{
			if (MaterialSourceMode == MaterialMode.Specific && !MaterialSource.IsValid)
				return ModStrings.MaterialPromptSource;
			if (!MaterialTarget.IsValid)
				return ModStrings.MaterialPromptTarget;
			if (MaterialSourceMode == MaterialMode.Specific && MaterialSource == MaterialTarget)
				return ModStrings.MaterialSameMaterial;
			List<ReplacementRules.Family> usable = UsableTypes();
			string target = ReplacementRules.MaterialName(MaterialTarget);
			if (usable.Count == 0)
				return ModStrings.MaterialPromptTypes.Replace("{0}", target);
			string from = MaterialSourceMode == MaterialMode.Specific ? ReplacementRules.MaterialName(MaterialSource) : ModStrings.AnyMaterialLower;
			return ModStrings.MaterialSummary.Replace("{0}", from).Replace("{1}", target).Replace("{2}", NameList(usable));
		}

		/// <summary>"A, B and C", or "A, B, C, D and 7 more" past <see cref="NamesShown"/> names.</summary>
		private static string NameList(List<ReplacementRules.Family> families)
		{
			var names = new List<string>();
			for (int i = 0; i < families.Count && i < NamesShown; i++)
				names.Add(families[i].Name);
			int more = families.Count - names.Count;
			if (more > 0)
				return string.Join(", ", names) + " and " + more + " more";
			if (names.Count == 1)
				return names[0];
			return string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
		}

		/// <summary>A one-line description for the hover card.</summary>
		public string Describe()
		{
			if (Mode == PlanMode.ChangeMaterial)
				return MaterialSummary();
			var text = new StringBuilder();
			if (Source == null)
				return "Choose what to replace.";
			text.Append(Source.Name);
			if (SourceMode == MaterialMode.Any)
				text.Append(" (any material)");
			else if (SourceMode == MaterialMode.Specific && AllValid(SourceMaterials))
				text.Append(" (").Append(DisplayNames(SourceMaterials)).Append(")");
			else if (SourceMode == MaterialMode.Specific)
				return text.Append(": pick a material for each ingredient.").ToString();
			else
				return text.Append(": any material, or specific ones?").ToString();
			if (Target == null)
				return text.Append(" -> choose what to replace it with.").ToString();
			text.Append(" -> ").Append(Target.Name);
			if (TargetMaterials.Length > 0)
				text.Append(" (").Append(DisplayNames(TargetMaterials)).Append(")");
			if (!TargetComplete)
				text.Append(": pick its material.");
			return text.ToString();
		}

		private static string DisplayNames(Tag[] tags)
		{
			var names = new List<string>();
			foreach (Tag tag in tags)
				names.Add(tag.IsValid ? ReplacementRules.MaterialName(tag) : "?");
			return string.Join(", ", names);
		}
	}
}
