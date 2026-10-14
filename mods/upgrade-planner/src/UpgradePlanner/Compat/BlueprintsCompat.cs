using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Blueprints Expanded, which the planner depends on for a building's settings (a sensor's
	/// threshold, a door's access, a storage's filters...). It stores them for every type it knows,
	/// and puts a carrier on every building under construction whose data it applies to the finished
	/// building of the same type in the same cell, saved with the colony. A replacement order the
	/// planner places gets the old building's settings in its carrier; a building rebuilt through the
	/// game's Reconstructable gets them from Blueprints Expanded's own patch on it.
	/// Set up from OnAllModsLoaded only when the mod with Blueprints Expanded's static ID is enabled,
	/// as its author asks of integrations, and reached by delegates made from its API methods.
	/// </summary>
	internal static class BlueprintsCompat
	{
		/// <summary>Blueprints Expanded's static ID (its mod.yaml).</summary>
		public const string StaticId = "BlueprintsV2";
		private const string Api = "BlueprintsV2.ModAPI.API_Methods";
		private const string Carrier = "BlueprintsV2.BlueprintData.UnderConstructionDataTransfer";

		private delegate Dictionary<string, JObject> GetAllDataDelegate(GameObject gameObject);
		private delegate void SetDataToApplyDelegate(string id, JObject value);

		private static GetAllDataDelegate getAllData;
		private static Type carrierType;
		private static MethodInfo setDataToApply;

		/// <summary>True once Blueprints Expanded is found with everything the planner calls.</summary>
		public static bool Loaded { get; private set; }

		/// <summary>Looks for Blueprints Expanded among the loaded mods and, if it is enabled, binds to its API.</summary>
		public static void Init(IReadOnlyList<KMod.Mod> mods)
		{
			bool enabled = false;
			foreach (KMod.Mod mod in mods)
				if (mod.IsEnabledForActiveDlc() && mod.staticID == StaticId)
					enabled = true;
			if (!enabled)
			{
				Debug.LogWarning("[UpgradePlanner] Blueprints Expanded is not enabled: upgraded buildings lose their settings, and only buildings the game lets you build over are offered.");
				return;
			}
			bool bound = TryCreateDelegate(Api, "GetAllAdditionalBuildingData", out getAllData);
			carrierType = AccessTools.TypeByName(Carrier);
			setDataToApply = carrierType != null ? AccessTools.Method(carrierType, "SetDataToApply", Parameters<SetDataToApplyDelegate>()) : null;
			if (setDataToApply == null)
				Debug.LogWarning("[UpgradePlanner] " + Carrier + ".SetDataToApply not found.");
			Loaded = bound && setDataToApply != null;
			if (Loaded)
				OtherModSettings.Register();
			Debug.Log("[UpgradePlanner] Blueprints Expanded integration: " + (Loaded
				? "settings are kept when buildings are upgraded."
				: "its API has changed; upgraded buildings lose their settings."));
		}

		/// <summary>
		/// Copies the settings of <paramref name="building"/> into the carrier of the order that
		/// replaces it, for Blueprints Expanded to apply when the order completes. Nothing is applied
		/// to the order itself (its API's apply would also set them on the order: the order's
		/// priority is the tool's). Returns how many settings were copied.
		/// </summary>
		public static int CarrySettings(GameObject building, GameObject order)
		{
			if (!Loaded || building == null || order == null)
				return 0;
			return GiveSettings(TakeSettings(building), order, building.name);
		}

		/// <summary>The settings Blueprints Expanded stores for a building, to give an order later (null when there are none or it is not loaded).</summary>
		public static Dictionary<string, JObject> TakeSettings(GameObject building)
		{
			if (!Loaded || building == null)
				return null;
			try
			{
				return getAllData(building);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not read the settings of " + building.name + ": " + e.GetBaseException().Message);
				return null;
			}
		}

		/// <summary>Puts settings taken from a building in an order's carrier, for Blueprints Expanded to apply when it is built. Returns how many.</summary>
		public static int GiveSettings(Dictionary<string, JObject> data, GameObject order, string from)
		{
			if (!Loaded || data == null || order == null)
				return 0;
			Component carrier = order.GetComponent(carrierType);
			if (carrier == null)
				return 0;
			int copied = 0;
			try
			{
				var setData = (SetDataToApplyDelegate)Delegate.CreateDelegate(typeof(SetDataToApplyDelegate), carrier, setDataToApply);
				foreach (KeyValuePair<string, JObject> entry in data)
					if (!string.IsNullOrEmpty(entry.Key) && entry.Value != null)
					{
						setData(entry.Key, entry.Value);
						copied++;
					}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not copy the settings of " + from + ": " + e.GetBaseException().Message);
			}
			return copied;
		}

		/// <summary>The settings Blueprints Expanded stores for a building, by key, as JSON text (for tests).</summary>
		internal static Dictionary<string, string> SettingsOf(GameObject building)
		{
			var settings = new Dictionary<string, string>();
			if (!Loaded || building == null)
				return settings;
			Dictionary<string, JObject> data = getAllData(building);
			if (data != null)
				foreach (KeyValuePair<string, JObject> entry in data)
					if (!string.IsNullOrEmpty(entry.Key) && entry.Value != null)
						settings[entry.Key] = entry.Value.ToString(Newtonsoft.Json.Formatting.None);
			return settings;
		}

		/// <summary>The parameter types of a delegate type's signature.</summary>
		private static Type[] Parameters<T>() where T : Delegate
		{
			ParameterInfo[] parameters = typeof(T).GetMethod("Invoke").GetParameters();
			var types = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
				types[i] = parameters[i].ParameterType;
			return types;
		}

		/// <summary>
		/// A delegate of type <typeparamref name="T"/> to the static method of that name on the type
		/// of that name, found by the delegate's own parameter types and checked for its return type;
		/// false, with what is missing logged, when there is none.
		/// </summary>
		private static bool TryCreateDelegate<T>(string typeName, string methodName, out T result) where T : Delegate
		{
			result = null;
			Type type = AccessTools.TypeByName(typeName);
			MethodInfo method = type != null ? AccessTools.Method(type, methodName, Parameters<T>()) : null;
			Type returns = typeof(T).GetMethod("Invoke").ReturnType;
			if (method == null || method.ReturnType != returns)
			{
				Debug.LogWarning("[UpgradePlanner] " + typeName + "." + methodName + (method == null ? " not found." : " returns " + method.ReturnType + ", not " + returns + "."));
				return false;
			}
			try
			{
				result = (T)Delegate.CreateDelegate(typeof(T), method);
				return true;
			}
			catch (ArgumentException e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not bind to " + typeName + "." + methodName + ": " + e.Message);
				return false;
			}
		}
	}
}
