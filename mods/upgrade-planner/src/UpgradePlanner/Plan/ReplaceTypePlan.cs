using System;
using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>
	/// The Replace type tab: a building type as the build menu lists it, in any material or in one
	/// material per recipe ingredient, and what to replace it with: a type the game lets you build
	/// over it, and one material per ingredient of that type's recipe.
	/// </summary>
	internal sealed class ReplaceTypePlan : IPlanTab
	{
		private readonly System.Action changed;

		public ReplaceTypePlan(System.Action changed)
		{
			this.changed = changed;
		}

		public Family Source { get; private set; }
		public MaterialMode SourceMode { get; private set; } = MaterialMode.Unset;
		/// <summary>One entry per ingredient of the source's recipe; Tag.Invalid until chosen.</summary>
		public Tag[] SourceMaterials { get; private set; } = new Tag[0];
		public Family Target { get; private set; }
		/// <summary>One entry per ingredient of the target's recipe; Tag.Invalid until chosen.</summary>
		public Tag[] TargetMaterials { get; private set; } = new Tag[0];

		public bool SourceComplete => Source != null
			&& (SourceMode == MaterialMode.Any || (SourceMode == MaterialMode.Specific && Materials.AllChosen(SourceMaterials)));

		public bool TargetComplete => Target != null && TargetMaterials.Length > 0 && Materials.AllChosen(TargetMaterials);

		public bool IsComplete => SourceComplete && TargetComplete;

		// ---- choices ----

		/// <summary>A new type to replace: its materials and what replaces it are chosen afresh.</summary>
		public void SetSource(Family family)
		{
			if (Source == family)
				return;
			Source = family;
			SourceMode = MaterialMode.Unset;
			SourceMaterials = Unchosen(family);
			Target = null;
			TargetMaterials = new Tag[0];
			changed();
		}

		public void SetSourceAny()
		{
			SourceMode = MaterialMode.Any;
			changed();
		}

		/// <summary>Replace only buildings of the materials picked, one per ingredient; the materials themselves come from the picker.</summary>
		public void SetSourceSpecific()
		{
			SourceMode = MaterialMode.Specific;
			changed();
		}

		public void SetSourceMaterials(IList<Tag> materials)
		{
			SourceMode = MaterialMode.Specific;
			Fill(SourceMaterials, materials);
			changed();
		}

		/// <summary>A new type to replace with: its materials are chosen afresh.</summary>
		public void SetTarget(Family family)
		{
			Target = family;
			TargetMaterials = Unchosen(family);
			changed();
		}

		public void SetTargetMaterials(IList<Tag> materials)
		{
			Fill(TargetMaterials, materials);
			changed();
		}

		public void Clear()
		{
			Source = null;
			SourceMode = MaterialMode.Unset;
			SourceMaterials = new Tag[0];
			Target = null;
			TargetMaterials = new Tag[0];
			changed();
		}

		/// <summary>One unchosen material per ingredient of the family's recipe.</summary>
		private static Tag[] Unchosen(Family family)
		{
			var tags = new Tag[family?.Representative.CraftRecipe?.Ingredients?.Count ?? 0];
			for (int i = 0; i < tags.Length; i++)
				tags[i] = Tag.Invalid;
			return tags;
		}

		private static void Fill(Tag[] slots, IList<Tag> materials)
		{
			for (int i = 0; i < slots.Length; i++)
				slots[i] = materials != null && i < materials.Count ? materials[i] : Tag.Invalid;
		}

		// ---- what the choices allow ----

		public List<BuildingDef> TouchedDefs() => Source != null ? new List<BuildingDef>(Source.Members) : new List<BuildingDef>();

		/// <summary>Made once the source is complete: the highlight shows what would be replaced before the target is chosen.</summary>
		public Matcher Compile() => SourceComplete ? new TypeMatcher(this) : null;

		// ---- presets ----

		public void Save(PlanPreset preset)
		{
			preset.SourceType = Source?.PrefabID;
			preset.SourceMode = SourceMode.ToString();
			preset.SourceMaterials = PlanPreset.Names(SourceMaterials);
			preset.TargetType = Target?.PrefabID;
			preset.TargetMaterials = PlanPreset.Names(TargetMaterials);
		}

		/// <summary>A type the game no longer has (its mod removed) is left out, with what depends on it.</summary>
		public void Load(PlanPreset preset)
		{
			Clear();
			Source = PlanPreset.FamilyOf(preset.SourceType);
			if (Source == null)
				return;
			SourceMode = preset.SourceMaterialMode;
			SourceMaterials = Unchosen(Source);
			if (SourceMode == MaterialMode.Specific)
				Fill(SourceMaterials, PlanPreset.Tags(preset.SourceMaterials));
			Target = PlanPreset.FamilyOf(preset.TargetType);
			TargetMaterials = Unchosen(Target);
			Fill(TargetMaterials, PlanPreset.Tags(preset.TargetMaterials));
			changed();
		}

		// ---- text ----

		public string Summary()
		{
			if (Source == null)
				return ModStrings.UPGRADEPLANNER.SummaryNothing;
			string source = Source.Name;
			if (SourceMode == MaterialMode.Any)
				source = ModStrings.UPGRADEPLANNER.DescribeAnyMaterial.Replace("{0}", source);
			else if (SourceMode == MaterialMode.Specific && Materials.AllChosen(SourceMaterials))
				source = ModStrings.UPGRADEPLANNER.DescribeMaterials.Replace("{0}", source).Replace("{1}", Materials.Names(SourceMaterials));
			if (Target == null)
				return source;
			string target = TargetComplete
				? ModStrings.UPGRADEPLANNER.DescribeMaterials.Replace("{0}", Target.Name).Replace("{1}", Materials.Names(TargetMaterials))
				: Target.Name;
			return ModStrings.UPGRADEPLANNER.DescribeReplace.Replace("{0}", source).Replace("{1}", target);
		}

		/// <summary>Every building of the source type in its materials; replaced with the target, the variant its materials stand for.</summary>
		private sealed class TypeMatcher : Matcher
		{
			private readonly Family source;
			private readonly Tag[] sourceMaterials;
			private readonly Family target;
			private readonly Tag[] targetMaterials;

			public TypeMatcher(ReplaceTypePlan plan) : base(plan.TouchedDefs())
			{
				source = plan.Source;
				sourceMaterials = plan.SourceMode == MaterialMode.Specific ? (Tag[])plan.SourceMaterials.Clone() : null;
				if (plan.TargetComplete)
				{
					target = plan.Target;
					targetMaterials = (Tag[])plan.TargetMaterials.Clone();
				}
			}

			public override bool Matches(BuildingComplete building)
			{
				if (building == null || !source.Contains(building.Def))
					return false;
				if (sourceMaterials == null)
					return true;
				Tag[] materials = Materials.Of(building);
				for (int i = 0; i < sourceMaterials.Length; i++)
					if (i >= materials.Length || materials[i] != sourceMaterials[i])
						return false;
				return true;
			}

			public override bool TryPlan(BuildingComplete building, out Upgrade upgrade)
			{
				upgrade = default;
				if (target == null || !Matches(building))
					return false;
				var materials = new List<Tag>(targetMaterials);
				upgrade = new Upgrade { Target = Resolve(target, materials), Materials = materials };
				return upgrade.Target != null;
			}
		}
	}
}
