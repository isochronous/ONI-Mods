using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FridgeThresholdsExtended
{
	/// <summary>
	/// Storage Refrigerator Thresholds (llunak, Workshop 2959340381) gives a fridge High and Low
	/// threshold sliders through its RefrigeratorThresholds component, and makes the fridge's full
	/// port follow them by prefixing the game's Refrigerator.UpdateLogicCircuit. It adds the
	/// component to the vanilla Refrigerator, Freezer and the old standalone Advanced Refrigeration
	/// fridges, by config class name. This adds it to more fridges, picked by building ID once every
	/// building is registered (a postfix on BuildingConfigManager.ConfigurePost):
	///
	/// - the Mini Fridge (The Aquatic Planet Pack), the Mini Freezer (Workshop 3756115269), Storage Pod Redux's Cool Pod and the fridges of Ronivan's Legacy -
	///   Industrial Revolution (Workshop 3557584850). They use the game's Refrigerator, so the
	///   component is all they need.
	/// - Freezer (Psyko, Workshop 2618339179), which the thresholds mod already covers; the
	///   component is added only if it is missing.
	///
	/// The thresholds mod registers a fridge's component in its OnSpawn, and the fridge sends its
	/// first signal from its own OnSpawn. Components spawn in the order they were added, and a
	/// component added to the finished prefab comes after the fridge's own, so that first signal
	/// would ignore the thresholds and a loaded fridge between Low and High would show the wrong
	/// state until its fill or power changed. A ThresholdsRefresh, added after the thresholds
	/// component, runs the fridge's UpdateLogicCircuit again once both have spawned.
	///
	/// Freezer runs its own copy of the Refrigerator class, whose UpdateLogicCircuit sends IsFull()
	/// and IsOperational: the sliders show and save, and the port ignores them. That method gets
	/// the thresholds mod's Refrigerator prefix, rebuilt against Freezer's fields, and Freezer's
	/// OnCopySettings gets the thresholds mod's copy of the three settings.
	///
	/// None of these mods is referenced at build time. Buildings that are not registered are
	/// skipped, Freezer's class is found by name, and a part whose members are missing is skipped
	/// with a warning.
	/// </summary>
	internal static class ThresholdsCompat
	{
		private const string ThresholdsBaseType = "StorageRefrigeratorThresholds.ThresholdsBase";
		private const string RefrigeratorThresholdsType = "StorageRefrigeratorThresholds.RefrigeratorThresholds";
		private const string FreezerType = "Psyko.Freezer.Freezer";

		// Building IDs, with what each one is for the log.
		private static readonly (string id, string name)[] Buildings =
		{
			("MiniFridge", "Mini Fridge"), // The Aquatic Planet Pack
			("MiniFreezer", "Mini Freezer"), // Workshop 3756115269
			("CoolPodConfig", "Storage Pod Redux Cool Pod"),
			("SimpleFridge", "Industrial Revolution Simple Fridge"),
			("AIO_FridgeLarge", "Industrial Revolution Large Fridge"),
			("FridgePod", "Industrial Revolution Fridge Pod"),
			("HightechSmallFridge", "Industrial Revolution High-tech Small Fridge"),
			("HightechBigFridge", "Industrial Revolution High-tech Big Fridge"),
			("Freezer", "Freezer"),
		};

		private static Type refrigeratorThresholds;
		private static Type freezerType;

		private static Action<Refrigerator> refrigeratorUpdate;
		private static MethodInfo freezerUpdate;

		// ThresholdsBase members, bound once in Apply.
		private static Func<GameObject, object> getThresholds;
		private static MethodInfo updateLogicState;
		private static MethodInfo isActuallyOperational;
		private static PropertyInfo lastSetFlag;
		private static PropertyInfo[] copiedSettings;

		private static Func<FilteredStorage, float> getAmountStored;
		private static Func<FilteredStorage, float> getMaxCapacity;

		public static void Apply(Harmony harmony)
		{
			Type thresholdsBase = AccessTools.TypeByName(ThresholdsBaseType);
			refrigeratorThresholds = AccessTools.TypeByName(RefrigeratorThresholdsType);
			if (thresholdsBase == null || refrigeratorThresholds == null)
			{
				Debug.Log("[FridgeThresholdsExtended] Storage Refrigerator Thresholds not found; nothing to do");
				return;
			}
			MethodInfo update = AccessTools.Method(typeof(Refrigerator), "UpdateLogicCircuit");
			MethodInfo configurePost = AccessTools.Method(typeof(BuildingConfigManager), nameof(BuildingConfigManager.ConfigurePost));
			if (update == null || configurePost == null)
			{
				Debug.LogWarning("[FridgeThresholdsExtended] skipped: " + (update == null ? "Refrigerator.UpdateLogicCircuit" : "BuildingConfigManager.ConfigurePost") + " not found");
				return;
			}
			refrigeratorUpdate = AccessTools.MethodDelegate<Action<Refrigerator>>(update);
			freezerType = AccessTools.TypeByName(FreezerType);
			if (freezerType != null)
			{
				PatchFreezerLogic(harmony, freezerType, thresholdsBase);
				PatchFreezerCopySettings(harmony, freezerType, thresholdsBase);
			}
			harmony.Patch(configurePost, postfix: new HarmonyMethod(typeof(ThresholdsCompat), nameof(ConfigurePostPostfix)));
		}

		/// <summary>Every building is registered: adds the thresholds and the refresh to the fridges that are there.</summary>
		private static void ConfigurePostPostfix()
		{
			var added = new List<string>();
			foreach (var (id, name) in Buildings)
			{
				try
				{
					GameObject prefab = Assets.GetBuildingDef(id)?.BuildingComplete;
					if (prefab == null)
						continue;
					if (prefab.GetComponent<Refrigerator>() == null && (freezerType == null || prefab.GetComponent(freezerType) == null))
					{
						Debug.LogWarning("[FridgeThresholdsExtended] " + name + " (" + id + ") skipped: it has no Refrigerator");
						continue;
					}
					if (prefab.GetComponent(refrigeratorThresholds) == null)
						prefab.AddComponent(refrigeratorThresholds);
					prefab.AddOrGet<ThresholdsRefresh>();
					added.Add(name);
				}
				catch (Exception e)
				{
					Debug.LogWarning("[FridgeThresholdsExtended] " + name + " (" + id + ") skipped: " + e.GetBaseException().Message);
				}
			}
			Debug.Log("[FridgeThresholdsExtended] thresholds on: " + (added.Count > 0 ? string.Join(", ", added) : "none of the fridges it knows"));
		}

		/// <summary>
		/// Runs the fridge's UpdateLogicCircuit, from ThresholdsRefresh.OnSpawn: the thresholds are
		/// registered by then, so the signal follows them.
		/// </summary>
		internal static void RefreshPort(GameObject go)
		{
			Refrigerator fridge = go.GetComponent<Refrigerator>();
			if (fridge != null)
			{
				refrigeratorUpdate(fridge);
				return;
			}
			if (freezerUpdate == null)
				return;
			Component freezer = go.GetComponent(freezerType);
			if (freezer != null)
				freezerUpdate.Invoke(freezer, null);
		}

		private static void PatchFreezerLogic(Harmony harmony, Type freezer, Type thresholdsBase)
		{
			try
			{
				MethodInfo original = AccessTools.DeclaredMethod(freezer, "UpdateLogicCircuit");
				MethodInfo get = AccessTools.Method(thresholdsBase, "Get", new[] { typeof(GameObject) });
				updateLogicState = AccessTools.Method(thresholdsBase, "UpdateLogicState", new[] { typeof(float) });
				isActuallyOperational = AccessTools.Method(thresholdsBase, "IsActuallyOperational", new[] { typeof(Operational) });
				lastSetFlag = AccessTools.Property(thresholdsBase, "LastSetFlag");
				MethodInfo amountStored = AccessTools.Method(typeof(FilteredStorage), "GetAmountStored");
				MethodInfo maxCapacity = AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin");
				string missing =
					original == null || original.GetParameters().Length != 0 ? "Freezer.UpdateLogicCircuit"
					: !HasField(freezer, "filteredStorage", typeof(FilteredStorage)) ? "Freezer.filteredStorage"
					: !HasField(freezer, "operational", typeof(Operational)) ? "Freezer.operational"
					: !HasField(freezer, "ports", typeof(LogicPorts)) ? "Freezer.ports"
					: get == null ? "ThresholdsBase.Get"
					: updateLogicState == null || updateLogicState.ReturnType != typeof(bool) ? "ThresholdsBase.UpdateLogicState"
					: isActuallyOperational == null || isActuallyOperational.ReturnType != typeof(bool) ? "ThresholdsBase.IsActuallyOperational"
					: lastSetFlag == null || lastSetFlag.PropertyType != typeof(bool?) ? "ThresholdsBase.LastSetFlag"
					: amountStored == null ? "FilteredStorage.GetAmountStored"
					: maxCapacity == null ? "FilteredStorage.GetMaxCapacityMinusStorageMargin"
					: null;
				if (missing != null)
				{
					Debug.LogWarning("[FridgeThresholdsExtended] Freezer logic port skipped: " + missing + " not found");
					return;
				}
				freezerUpdate = original;
				// A later Storage Refrigerator Thresholds that handles Freezer itself wins.
				Patches existing = Harmony.GetPatchInfo(original);
				if (existing != null && existing.Prefixes.Any(patch => patch.owner.IndexOf("llunak", StringComparison.OrdinalIgnoreCase) >= 0))
				{
					Debug.Log("[FridgeThresholdsExtended] Freezer logic port already handled by Storage Refrigerator Thresholds");
					return;
				}
				getThresholds = (Func<GameObject, object>)Delegate.CreateDelegate(typeof(Func<GameObject, object>), get);
				getAmountStored = AccessTools.MethodDelegate<Func<FilteredStorage, float>>(amountStored);
				getMaxCapacity = AccessTools.MethodDelegate<Func<FilteredStorage, float>>(maxCapacity);
				harmony.Patch(original, prefix: new HarmonyMethod(typeof(ThresholdsCompat), nameof(UpdateLogicCircuitPrefix)));
				Debug.Log("[FridgeThresholdsExtended] Freezer found; its logic port follows the thresholds");
			}
			catch (Exception e)
			{
				Debug.LogWarning("[FridgeThresholdsExtended] Freezer logic port skipped: " + e.GetBaseException().Message);
			}
		}

		/// <summary>
		/// The thresholds mod's Refrigerator.UpdateLogicCircuit prefix, against Freezer's fields:
		/// the port is green from the High threshold until the fill drops to the Low one (or the
		/// reverse when the player picks green on low), and only while the building is operational.
		/// </summary>
		private static bool UpdateLogicCircuitPrefix(Component __instance, FilteredStorage ___filteredStorage,
			Operational ___operational, LogicPorts ___ports)
		{
			object thresholds = getThresholds(__instance.gameObject);
			if (thresholds == null)
				return true; // no thresholds on this Freezer (or not spawned yet): its own full signal
			float stored = getAmountStored(___filteredStorage);
			float capacity = getMaxCapacity(___filteredStorage);
			bool operational = (bool)isActuallyOperational.Invoke(thresholds, new object[] { ___operational });
			bool flag = (bool)updateLogicState.Invoke(thresholds, new object[] { stored / capacity }) && operational;
			if (!(lastSetFlag.GetValue(thresholds) is bool last) || last != flag)
			{
				___ports.SendSignal(FilteredStorage.FULL_PORT_ID, flag ? 1 : 0);
				lastSetFlag.SetValue(thresholds, flag);
			}
			___filteredStorage.SetLogicMeter(flag);
			return false;
		}

		private static void PatchFreezerCopySettings(Harmony harmony, Type freezer, Type thresholdsBase)
		{
			try
			{
				MethodInfo original = AccessTools.DeclaredMethod(freezer, "OnCopySettings", new[] { typeof(object) });
				string[] names = { "SendGreenOnLow", "ActivateValue", "DeactivateValue" };
				PropertyInfo[] settings = names.Select(name => AccessTools.Property(thresholdsBase, name)).ToArray();
				int gap = Array.FindIndex(settings, setting => setting == null || !setting.CanRead || !setting.CanWrite);
				if (original == null || gap >= 0)
				{
					Debug.LogWarning("[FridgeThresholdsExtended] Freezer copy settings skipped: "
						+ (original == null ? "Freezer.OnCopySettings" : "ThresholdsBase." + names[gap]) + " not found");
					return;
				}
				copiedSettings = settings;
				harmony.Patch(original, postfix: new HarmonyMethod(typeof(ThresholdsCompat), nameof(OnCopySettingsPostfix)));
			}
			catch (Exception e)
			{
				Debug.LogWarning("[FridgeThresholdsExtended] Freezer copy settings skipped: " + e.GetBaseException().Message);
			}
		}

		/// <summary>Copy settings onto a Freezer carries the thresholds too, as the thresholds mod does for the Refrigerator.</summary>
		private static void OnCopySettingsPostfix(Component __instance, object data)
		{
			GameObject source = data as GameObject;
			if (source == null)
				return;
			Component from = source.GetComponent(refrigeratorThresholds);
			Component to = __instance.GetComponent(refrigeratorThresholds);
			if (from == null || to == null)
				return;
			foreach (PropertyInfo setting in copiedSettings)
				setting.SetValue(to, setting.GetValue(from));
		}

		private static bool HasField(Type type, string name, Type fieldType)
		{
			FieldInfo field = AccessTools.DeclaredField(type, name);
			return field != null && field.FieldType == fieldType;
		}
	}

	/// <summary>
	/// Added after the thresholds component, so it spawns after it: sends the fridge's port again
	/// once the thresholds are registered (see ThresholdsCompat). It has no state, so it is left
	/// out of saves.
	/// </summary>
	[SkipSaveFileSerialization]
	public sealed class ThresholdsRefresh : KMonoBehaviour
	{
		protected override void OnSpawn()
		{
			base.OnSpawn();
			ThresholdsCompat.RefreshPort(gameObject);
		}
	}
}
