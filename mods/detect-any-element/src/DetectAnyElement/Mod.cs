using HarmonyLib;
using Isochronous;
using KMod;
using UnityEngine;

namespace DetectAnyElement
{
	public sealed class DetectAnyElementMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			// The game keeps its own copy of a tag's proper name, so it is set again after every
			// language pass (TagManager.Create with a proper name renames an existing tag).
			Translations.Register(harmony, typeof(ModStrings), () => TagManager.Create(AnyElement.TagName, ModStrings.MISC.TAGS.DETECTANYELEMENT_ANY));
			Debug.Log("[DetectAnyElement] Loaded version " + typeof(DetectAnyElementMod).Assembly.GetName().Version);
		}
	}

	/// <summary>
	/// The "Any" choice. It is a tag like the elements a sensor's Filterable holds, so it rides
	/// in the same saved field and copies with the building's settings; it only means something
	/// to this mod's patches. Without the mod a sensor saved on Any keeps the tag and never
	/// matches, as if nothing were chosen.
	/// </summary>
	public static class AnyElement
	{
		public const string TagName = "DetectAnyElement_Any";

		/// <summary>
		/// Created with its English name so the tag exists before anything else; the name in the
		/// player's language is set by DetectAnyElementMod.OnLoad.
		/// </summary>
		public static readonly Tag Tag = TagManager.Create(TagName, ModStrings.MISC.TAGS.DETECTANYELEMENT_ANY);

		public static bool Is(Tag tag) => tag == Tag;
	}
}
