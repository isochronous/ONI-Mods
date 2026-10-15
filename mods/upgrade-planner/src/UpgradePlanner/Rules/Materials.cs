using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>What buildings are made of, and what the game offers to make them of.</summary>
	internal static class Materials
	{
		private static readonly Dictionary<Tag, HashSet<Tag>> validFor = new Dictionary<Tag, HashSet<Tag>>();

		/// <summary>The materials the game offers for an ingredient tag (a tag joined with '&amp;' offers each part's).</summary>
		public static HashSet<Tag> Valid(Tag ingredient)
		{
			if (!validFor.TryGetValue(ingredient, out HashSet<Tag> valid))
				validFor[ingredient] = valid = new HashSet<Tag>(MaterialSelector.GetValidMaterials(ingredient, omitDisabledElements: true));
			return valid;
		}

		/// <summary>
		/// What a finished building was built from, one tag per recipe ingredient: the materials its
		/// deconstruction gives back, or failing that the element it is made of.
		/// </summary>
		public static Tag[] Of(BuildingComplete building)
		{
			Deconstructable deconstructable = building.GetComponent<Deconstructable>();
			if (deconstructable != null && deconstructable.constructionElements != null && deconstructable.constructionElements.Length > 0)
				return deconstructable.constructionElements;
			PrimaryElement element = building.GetComponent<PrimaryElement>();
			return element != null && element.Element != null ? new[] { element.Element.tag } : new Tag[0];
		}

		public static bool Same(IList<Tag> a, IList<Tag> b)
		{
			if (a == null || b == null || a.Count != b.Count)
				return false;
			for (int i = 0; i < a.Count; i++)
				if (a[i] != b[i])
					return false;
			return true;
		}

		public static bool AllChosen(IEnumerable<Tag> tags)
		{
			foreach (Tag tag in tags)
				if (!tag.IsValid)
					return false;
			return true;
		}

		/// <summary>A material's name, without link markup.</summary>
		public static string Name(Tag material)
		{
			Element element = ElementLoader.GetElement(material);
			return STRINGS.UI.StripLinkFormatting(element != null ? element.name : material.ProperName());
		}

		/// <summary>Names of the materials, "?" for one not chosen, joined as a list.</summary>
		public static string Names(IEnumerable<Tag> materials)
		{
			var names = new List<string>();
			foreach (Tag tag in materials)
				names.Add(tag.IsValid ? Name(tag) : "?");
			return string.Join(ModStrings.UPGRADEPLANNER.ListSeparator, names);
		}
	}
}
