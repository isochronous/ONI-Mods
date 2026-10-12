using System.Collections.Generic;
using System.Text;

namespace UpgradePlanner
{
	/// <summary>
	/// The current choices: what to replace (a building type as the build menu lists it, in any
	/// material or in one material per recipe ingredient) and what to replace it with (a buildable
	/// type and one material per recipe ingredient). Kept for the whole session, so re-selecting
	/// the tool resumes the last plan.
	/// </summary>
	internal sealed class UpgradePlan
	{
		public enum MaterialMode { Unset, Any, Specific }

		public static readonly UpgradePlan Current = new UpgradePlan();

		public ReplacementRules.Family Source { get; private set; }
		public MaterialMode SourceMode { get; private set; } = MaterialMode.Unset;
		/// <summary>One entry per ingredient of the source's recipe; Tag.Invalid until chosen.</summary>
		public Tag[] SourceMaterials { get; private set; } = new Tag[0];
		public ReplacementRules.Family Target { get; private set; }
		/// <summary>One entry per ingredient of the target's recipe; Tag.Invalid until chosen.</summary>
		public Tag[] TargetMaterials { get; private set; } = new Tag[0];

		/// <summary>Bumped on every change, so caches (the highlight, the dialog) know to refresh.</summary>
		public int Version { get; private set; }

		public BuildingDef SourceDef => Source?.Representative;

		public bool SourceComplete => Source != null
			&& (SourceMode == MaterialMode.Any || (SourceMode == MaterialMode.Specific && AllValid(SourceMaterials)));

		public bool TargetComplete => Target != null && TargetMaterials.Length > 0 && AllValid(TargetMaterials);

		public bool IsComplete => SourceComplete && TargetComplete;

		private static bool AllValid(Tag[] tags)
		{
			foreach (Tag tag in tags)
				if (!tag.IsValid)
					return false;
			return true;
		}

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

		/// <summary>True if this finished building is one the plan replaces.</summary>
		public bool Matches(BuildingComplete building)
		{
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

		/// <summary>A one-line description for the hover card and the dialog footer.</summary>
		public string Describe()
		{
			var text = new StringBuilder();
			if (Source == null)
				return "Choose what to replace.";
			text.Append(Source.Name);
			if (SourceMode == MaterialMode.Any)
				text.Append(" (any material)");
			else if (SourceMode == MaterialMode.Specific && AllValid(SourceMaterials))
				text.Append(" (").Append(Names(SourceMaterials)).Append(")");
			else if (SourceMode == MaterialMode.Specific)
				return text.Append(": pick a material for each ingredient.").ToString();
			else
				return text.Append(": any material, or specific ones?").ToString();
			if (Target == null)
				return text.Append(" -> choose what to replace it with.").ToString();
			text.Append(" -> ").Append(Target.Name);
			if (TargetMaterials.Length > 0)
				text.Append(" (").Append(Names(TargetMaterials)).Append(")");
			if (!TargetComplete)
				text.Append(": pick its material.");
			return text.ToString();
		}

		private static string Names(Tag[] tags)
		{
			var names = new List<string>();
			foreach (Tag tag in tags)
				names.Add(tag.IsValid ? ReplacementRules.MaterialName(tag) : "?");
			return string.Join(", ", names);
		}
	}
}
