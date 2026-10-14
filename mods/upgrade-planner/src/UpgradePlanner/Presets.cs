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
		public string Source;
		public string Target;

		// Replace type
		public string SourceType;
		public List<string> SourceMaterials;
		public string TargetType;
		public List<string> TargetMaterials;

		public string GetProperName()
		{
			return Mode == UpgradePlan.PlanMode.ReplaceType.ToString() ? Name + ModStrings.PresetTypeSuffix : Name;
		}
	}

	/// <summary>
	/// The player's saved plans, kept in one JSON file in the game's mod config folder, beside
	/// what other mods keep there, so they last across colonies and mod updates.
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
					Load();
				return all;
			}
		}

		/// <summary>Reads the file again; a missing or unreadable file is no presets.</summary>
		public static void Load()
		{
			all = new List<PlanPreset>();
			try
			{
				if (File.Exists(FilePath))
					all = JsonConvert.DeserializeObject<List<PlanPreset>>(File.ReadAllText(FilePath)) ?? new List<PlanPreset>();
				all.RemoveAll(p => p == null || string.IsNullOrEmpty(p.Name));
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not read the presets in " + FilePath + ": " + e.Message);
			}
		}

		/// <summary>Adds the preset, or replaces the one with its name. Returns true when one was replaced.</summary>
		public static bool Put(PlanPreset preset)
		{
			int index = All.FindIndex(p => p.Name == preset.Name);
			if (index >= 0)
				all[index] = preset;
			else
				all.Add(preset);
			Save();
			return index >= 0;
		}

		public static void Remove(PlanPreset preset)
		{
			if (preset != null && All.Remove(preset))
				Save();
		}

		public static PlanPreset Find(string name) => All.Find(p => p.Name == name);

		private static void Save()
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
