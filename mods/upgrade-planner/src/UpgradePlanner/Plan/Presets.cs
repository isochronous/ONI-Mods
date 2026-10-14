using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UpgradePlanner
{
	/// <summary>A saved plan: one tab's choices, by prefab id and element tag so it reads back in any colony.</summary>
	internal sealed class PlanPreset : IListableOption
	{
		public string Name;
		/// <summary>The tab: ChangeMaterial or ReplaceType.</summary>
		public string Mode;
		/// <summary>Any, Specific (or Unset, on the Replace type tab).</summary>
		public string SourceMode;

		// Change material
		public List<string> Types;
		public bool AllBuildings;
		public string Source;
		public string Target;

		// Replace type
		public string SourceType;
		public List<string> SourceMaterials;
		public string TargetType;
		public List<string> TargetMaterials;

		public string GetProperName() => Name;

		/// <summary>The preset's tab; one saved before presets had a tab is a Change material one.</summary>
		[JsonIgnore]
		public PlanMode Tab => Enum.TryParse(Mode, out PlanMode tab) ? tab : PlanMode.ChangeMaterial;

		[JsonIgnore]
		public MaterialMode SourceMaterialMode => Enum.TryParse(SourceMode, out MaterialMode mode) ? mode : MaterialMode.Unset;

		public static Tag TagOf(string name) => string.IsNullOrEmpty(name) ? Tag.Invalid : new Tag(name);

		public static List<Tag> Tags(List<string> names)
		{
			var tags = new List<Tag>();
			if (names != null)
				foreach (string name in names)
					tags.Add(TagOf(name));
			return tags;
		}

		public static List<string> Names(IEnumerable<Tag> tags)
		{
			var names = new List<string>();
			foreach (Tag tag in tags)
				names.Add(tag.IsValid ? tag.Name : null);
			return names;
		}

		/// <summary>The family of a saved type, or null when the game no longer has it.</summary>
		public static Family FamilyOf(string prefabId)
		{
			BuildingDef def = string.IsNullOrEmpty(prefabId) ? null : Assets.GetBuildingDef(prefabId);
			return def != null ? Families.Of(def) : null;
		}
	}

	/// <summary>
	/// The player's saved plans, kept in one JSON file in the game's mod config folder, beside what
	/// other mods keep there, so they last across colonies and mod updates. Each preset belongs to
	/// one tab: each tab lists only its own, and a name is unique within a tab.
	/// </summary>
	internal static class Presets
	{
		private static List<PlanPreset> all;

		/// <summary>Where the file goes; tests point it elsewhere so the player's own presets are not touched.</summary>
		internal static string FilePath { get; private set; } = Path.Combine(KMod.Manager.GetDirectory(), "config", "UpgradePlanner", "presets.json");

		/// <summary>Uses another file from now on, read afresh; for tests.</summary>
		internal static void UseFile(string path)
		{
			FilePath = path;
			all = null;
		}

		public static List<PlanPreset> All
		{
			get
			{
				if (all == null)
					all = Read();
				return all;
			}
		}

		/// <summary>The presets of one tab, in the order they were saved.</summary>
		public static List<PlanPreset> For(PlanMode tab) => All.FindAll(p => p.Tab == tab);

		public static PlanPreset Find(string name, PlanMode tab) => All.Find(p => p.Name == name && p.Tab == tab);

		/// <summary>Adds the preset, or replaces the one of its tab with its name. Returns true when one was replaced.</summary>
		public static bool Put(PlanPreset preset)
		{
			int index = All.FindIndex(p => p.Name == preset.Name && p.Tab == preset.Tab);
			if (index >= 0)
				all[index] = preset;
			else
				all.Add(preset);
			Write();
			return index >= 0;
		}

		public static void Remove(PlanPreset preset)
		{
			if (preset != null && All.Remove(preset))
				Write();
		}

		/// <summary>The file's presets; a missing or unreadable file is none.</summary>
		private static List<PlanPreset> Read()
		{
			try
			{
				if (File.Exists(FilePath))
				{
					List<PlanPreset> read = JsonConvert.DeserializeObject<List<PlanPreset>>(File.ReadAllText(FilePath)) ?? new List<PlanPreset>();
					read.RemoveAll(p => p == null || string.IsNullOrEmpty(p.Name));
					return read;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not read the presets in " + FilePath + ": " + e.Message);
			}
			return new List<PlanPreset>();
		}

		private static void Write()
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
				File.WriteAllText(FilePath, JsonConvert.SerializeObject(all, Formatting.Indented,
					new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not save the presets to " + FilePath + ": " + e.Message);
			}
		}
	}
}
