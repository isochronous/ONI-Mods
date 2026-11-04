using System;
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
	/// component to the vanilla Refrigerator and, by type name, to Freezer and the fridges of the
	/// old standalone Dupes Refrigeration mod. Two cases it misses:
	///
	/// Ronivan's Legacy - Industrial Revolution (Workshop 3557584850) carries the Dupes
	/// Refrigeration fridges in its own assembly and namespace, so they get no component. They use
	/// the game's Refrigerator, so the component is all they need: it is added in a prefix on each
	/// config's DoPostConfigureComplete, ahead of the Refrigerator, as the thresholds mod does for
	/// the vanilla fridge, so it is registered before the Refrigerator's first logic update.
	///
	/// Freezer (Psyko, Workshop 2618339179) has the component, but runs its own copy of the
	/// Refrigerator class, whose UpdateLogicCircuit sends IsFull() and IsOperational: the sliders
	/// show and save, and the port ignores them. That method gets the thresholds mod's
	/// Refrigerator prefix, rebuilt against Freezer's fields, and Freezer's OnCopySettings gets
	/// the thresholds mod's copy of the three settings. The thresholds mod adds the component in a
	/// postfix, after Freezer, so it is not registered yet when Freezer.OnSpawn sends the first
	/// signal; a prefix on FreezerConfig.DoPostConfigureComplete adds it ahead of Freezer instead,
	/// and the thresholds mod's AddOrGet then finds it.
	///
	/// None of the three mods is referenced at build time. Every type is found by name once all
	/// mods are loaded, the thresholds members are bound here once, and a part whose members are
	/// missing is skipped with a warning.
	/// </summary>
	internal static class ThresholdsCompat
	{
		private const string ThresholdsBaseType = "StorageRefrigeratorThresholds.ThresholdsBase";
		private const string RefrigeratorThresholdsType = "StorageRefrigeratorThresholds.RefrigeratorThresholds";
		private const string FreezerType = "Psyko.Freezer.Freezer";
		private const string FreezerConfigType = "Psyko.Freezer.FreezerConfig";
		private const string IRFridgeNamespace = "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesRefrigeration.";

		// Matched by class: the two Hightech configs have each other's building ID.
		private static readonly string[] IRFridgeConfigs =
		{
			"FridgeSmallConfig",
			"FridgeLargeConfig",
			"FridgePodConfig",
			"HightechBigFridgeConfig",
			"HightechSmallFridgeConfig",
		};

		private static Type refrigeratorThresholds;

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
			PatchIRFridges(harmony);
			Type freezer = AccessTools.TypeByName(FreezerType);
			if (freezer == null)
				return;
			PatchFreezerLogic(harmony, freezer, thresholdsBase);
			PatchFreezerCopySettings(harmony, freezer, thresholdsBase);
		}

		private static void PatchIRFridges(Harmony harmony)
		{
			try
			{
				Type[] configs = IRFridgeConfigs.Select(name => AccessTools.TypeByName(IRFridgeNamespace + name)).ToArray();
				if (configs.All(config => config == null))
					return;
				var prefix = new HarmonyMethod(typeof(ThresholdsCompat), nameof(AddThresholdsPrefix));
				int patched = 0;
				for (int i = 0; i < configs.Length; i++)
				{
					if (configs[i] == null)
					{
						Debug.LogWarning("[FridgeThresholdsExtended] Industrial Revolution " + IRFridgeConfigs[i] + " skipped: class not found");
						continue;
					}
					MethodInfo original = AccessTools.DeclaredMethod(configs[i], "DoPostConfigureComplete");
					if (original == null)
					{
						Debug.LogWarning("[FridgeThresholdsExtended] Industrial Revolution " + IRFridgeConfigs[i] + " skipped: DoPostConfigureComplete not found");
						continue;
					}
					harmony.Patch(original, prefix: prefix);
					patched++;
				}
				Debug.Log("[FridgeThresholdsExtended] Industrial Revolution found; thresholds added to " + patched + " fridges");
			}
			catch (Exception e)
			{
				Debug.LogWarning("[FridgeThresholdsExtended] Industrial Revolution fridges skipped: " + e.GetBaseException().Message);
			}
		}

		private static void AddThresholdsPrefix(GameObject go)
		{
			if (go.GetComponent(refrigeratorThresholds) == null)
				go.AddComponent(refrigeratorThresholds);
		}

		private static void PatchFreezerLogic(Harmony harmony, Type freezer, Type thresholdsBase)
		{
			try
			{
				MethodInfo original = AccessTools.DeclaredMethod(freezer, "UpdateLogicCircuit");
				Type freezerConfig = AccessTools.TypeByName(FreezerConfigType);
				MethodInfo configure = freezerConfig != null ? AccessTools.DeclaredMethod(freezerConfig, "DoPostConfigureComplete") : null;
				MethodInfo get = AccessTools.Method(thresholdsBase, "Get", new[] { typeof(GameObject) });
				updateLogicState = AccessTools.Method(thresholdsBase, "UpdateLogicState", new[] { typeof(float) });
				isActuallyOperational = AccessTools.Method(thresholdsBase, "IsActuallyOperational", new[] { typeof(Operational) });
				lastSetFlag = AccessTools.Property(thresholdsBase, "LastSetFlag");
				MethodInfo amountStored = AccessTools.Method(typeof(FilteredStorage), "GetAmountStored");
				MethodInfo maxCapacity = AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin");
				string missing =
					original == null ? "Freezer.UpdateLogicCircuit"
					: configure == null ? "FreezerConfig.DoPostConfigureComplete"
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
				harmony.Patch(configure, prefix: new HarmonyMethod(typeof(ThresholdsCompat), nameof(AddThresholdsPrefix)));
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
				return true; // no thresholds on this Freezer: its own full signal
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
}
