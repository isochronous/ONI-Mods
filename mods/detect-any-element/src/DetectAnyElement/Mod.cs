using HarmonyLib;
using KMod;
using UnityEngine;

namespace DetectAnyElement
{
	public sealed class DetectAnyElementMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
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
		public const string Label = "Any";

		public static readonly Tag Tag = TagManager.Create(TagName, Label);

		public static bool Is(Tag tag) => tag == Tag;
	}
}
