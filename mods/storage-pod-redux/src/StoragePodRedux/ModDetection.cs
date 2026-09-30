using System.Collections.Generic;
using System.IO;
using KMod;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace StoragePodRedux
{
	internal static class ModDetection
	{
		public static bool IsEnabled(IReadOnlyList<Mod> mods, string staticID)
		{
			foreach (Mod mod in mods)
			{
				if (mod.staticID == staticID && mod.IsEnabledForActiveDlc())
					return true;
			}
			return false;
		}

		/// <summary>The folder a mod's PLib-style config lives in: mods/config/&lt;staticID&gt;.</summary>
		public static string ConfigFolder(string staticID)
		{
			return Path.Combine(Manager.GetDirectory(), "config", staticID);
		}

		/// <summary>Parses a JSON file, or returns null when it is missing or unreadable.</summary>
		public static JObject ReadJson(string path)
		{
			try
			{
				return File.Exists(path) ? JObject.Parse(File.ReadAllText(path)) : null;
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("[StoragePodRedux] Could not read " + path + ": " + e.Message);
				return null;
			}
		}
	}

	/// <summary>
	/// Ronivan's Legacy - Industrial Revolution ships its own 1x1 fridge, the Fridge Pod.
	/// That building is active when the mod is enabled, its Dupes' Refrigeration module is on
	/// (config.json, DupesRefrigeration_Enabled, default true) and the building itself is on
	/// (RonivanAIO_BuildingConfig.json, BuildingConfigurations.FridgePod.BuildingEnabled,
	/// default true) or force-enabled (BuildingEnabledForce). Both files are optional: a
	/// missing file or key means the default.
	/// </summary>
	internal static class RonivansLegacy
	{
		public const string StaticID = "RonivansLegacy_ChemicalProcessing";

		public static bool FridgePodActive(IReadOnlyList<Mod> mods)
		{
			if (!ModDetection.IsEnabled(mods, StaticID))
				return false;
			string folder = ModDetection.ConfigFolder(StaticID);
			JObject config = ModDetection.ReadJson(Path.Combine(folder, "config.json"));
			JObject buildings = ModDetection.ReadJson(Path.Combine(folder, "RonivanAIO_BuildingConfig.json"));
			bool moduleOn = config?["DupesRefrigeration_Enabled"]?.Value<bool>() ?? true;
			JToken fridgePod = buildings?["BuildingConfigurations"]?["FridgePod"];
			bool buildingOn = fridgePod?["BuildingEnabled"]?.Value<bool>() ?? true;
			bool forced = fridgePod?["BuildingEnabledForce"]?.Value<bool>() ?? false;
			return forced || (moduleOn && buildingOn);
		}
	}
}
