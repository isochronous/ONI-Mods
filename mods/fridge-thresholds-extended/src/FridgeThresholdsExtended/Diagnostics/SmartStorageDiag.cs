using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FridgeThresholdsExtended.Diagnostics
{
	/// <summary>
	/// OM-017, temporary: logging only, no behaviour change. Logs, with the tag [FTE-DIAG], what a
	/// Smart Storage Locker and Storage Refrigerator Thresholds' StorageThresholds component do around
	/// spawn: StorageLockerSmart.OnSpawn (entry and exit), every UpdateLogicAndActiveState call
	/// (state before, whether the original body runs, state after), the thresholds component's
	/// OnSpawn, and a sweep of every locker 2 and 20 game seconds after Game.OnSpawn. Per locker only
	/// the first few calls are logged, plus any call where the port output changes.
	/// </summary>
	internal static class SmartStorageDiag
	{
		private const string Tag = "[FTE-DIAG] ";
		private const int CallsPerLocker = 3;
		private const int MaxLines = 4000;

		private static Func<GameObject, object> getThresholds;
		private static PropertyInfo lastSetFlag;
		private static FieldInfo thActivated, thActivate, thDeactivate, thGreenOnLow;
		private static FieldInfo filteredStorageField, vanillaActivated, operationalField, portsField;
		private static MethodInfo amountStored, maxCapacity;

		private static readonly Dictionary<int, int> calls = new Dictionary<int, int>();
		private static readonly HashSet<StorageLockerSmart> lockers = new HashSet<StorageLockerSmart>();
		private static int lines;
		private static int depth;

		private sealed class CallState
		{
			public string before;
			public int portBefore;
			public bool? lastBefore;
			public bool? runOriginal;
		}

		public static void Apply(Harmony harmony)
		{
			try
			{
				Type thresholdsBase = AccessTools.TypeByName("StorageRefrigeratorThresholds.ThresholdsBase");
				if (thresholdsBase == null)
				{
					Log("thresholds mod not loaded; diagnostic off");
					return;
				}
				MethodInfo get = AccessTools.Method(thresholdsBase, "Get", new[] { typeof(GameObject) });
				getThresholds = (Func<GameObject, object>)Delegate.CreateDelegate(typeof(Func<GameObject, object>), get);
				lastSetFlag = AccessTools.Property(thresholdsBase, "LastSetFlag");
				thActivated = AccessTools.Field(thresholdsBase, "activated");
				thActivate = AccessTools.Field(thresholdsBase, "activateValue");
				thDeactivate = AccessTools.Field(thresholdsBase, "deactivateValue");
				thGreenOnLow = AccessTools.Field(thresholdsBase, "sendGreenOnLow");
				filteredStorageField = AccessTools.Field(typeof(StorageLocker), "filteredStorage");
				vanillaActivated = AccessTools.Field(typeof(StorageLockerSmart), "activated");
				operationalField = AccessTools.Field(typeof(StorageLockerSmart), "operational");
				portsField = AccessTools.Field(typeof(StorageLockerSmart), "ports");
				amountStored = AccessTools.Method(typeof(FilteredStorage), "GetAmountStored");
				maxCapacity = AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin");

				MethodInfo update = AccessTools.Method(typeof(StorageLockerSmart), "UpdateLogicAndActiveState");
				MethodInfo spawn = AccessTools.Method(typeof(StorageLockerSmart), "OnSpawn");
				MethodInfo thSpawn = AccessTools.DeclaredMethod(thresholdsBase, "OnSpawn");
				MethodInfo gameSpawn = AccessTools.Method(typeof(Game), "OnSpawn");

				LogPatches("UpdateLogicAndActiveState (before diag)", update);
				LogPatches("StorageLockerSmart.OnSpawn (before diag)", spawn);

				harmony.Patch(update,
					prefix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(UpdateFirstPrefix)) { priority = Priority.First },
					postfix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(UpdatePostfix)) { priority = Priority.Last });
				// A second prefix after every other one: its __runOriginal says whether the thresholds
				// mod's prefix let the vanilla body run.
				harmony.Patch(update,
					prefix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(UpdateLastPrefix)) { priority = Priority.Last });
				harmony.Patch(spawn,
					prefix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(SpawnPrefix)) { priority = Priority.First },
					postfix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(SpawnPostfix)) { priority = Priority.Last });
				if (thSpawn != null)
					harmony.Patch(thSpawn, postfix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(ThresholdsSpawnPostfix)));
				else
					Log("ThresholdsBase.OnSpawn not found");
				harmony.Patch(gameSpawn, postfix: new HarmonyMethod(typeof(SmartStorageDiag), nameof(GameSpawnPostfix)));

				LogPatches("UpdateLogicAndActiveState (after diag)", update);
				Log("installed");
			}
			catch (Exception e)
			{
				Log("install failed: " + e);
			}
		}

		private static void LogPatches(string what, MethodBase method)
		{
			Patches info = Harmony.GetPatchInfo(method);
			if (info == null)
			{
				Log(what + ": no patches");
				return;
			}
			string Describe(IEnumerable<Patch> patches) => string.Join(", ", patches.Select(p =>
				p.owner + ":" + p.PatchMethod.DeclaringType?.FullName + "." + p.PatchMethod.Name + "@" + p.priority));
			Log(what + ": prefixes [" + Describe(info.Prefixes) + "] postfixes [" + Describe(info.Postfixes)
				+ "] transpilers [" + Describe(info.Transpilers) + "]");
		}

		private static void SpawnPrefix(StorageLockerSmart __instance)
		{
			lockers.Add(__instance);
			Log("Locker.OnSpawn enter " + Describe(__instance));
		}

		private static void SpawnPostfix(StorageLockerSmart __instance)
		{
			Log("Locker.OnSpawn exit  " + Describe(__instance));
		}

		private static void ThresholdsSpawnPostfix(Component __instance)
		{
			StorageLockerSmart locker = __instance.GetComponent<StorageLockerSmart>();
			if (locker == null)
				return;
			Log("Thresholds.OnSpawn exit " + Describe(locker));
		}

		private static void UpdateFirstPrefix(StorageLockerSmart __instance, out CallState __state)
		{
			depth++;
			__state = new CallState
			{
				before = Describe(__instance),
				portBefore = PortValue(__instance),
				lastBefore = LastSetFlag(__instance),
			};
			__state.runOriginal = null;
			pending = __state;
		}

		// Shared between the two prefixes of one call (no recursion into this method is expected;
		// depth is logged in case there is).
		private static CallState pending;

		private static void UpdateLastPrefix(bool __runOriginal)
		{
			if (pending != null)
				pending.runOriginal = __runOriginal;
		}

		private static void UpdatePostfix(StorageLockerSmart __instance, CallState __state)
		{
			int d = depth;
			depth--;
			pending = null;
			if (__state == null)
				return;
			int id = __instance.GetInstanceID();
			calls.TryGetValue(id, out int n);
			calls[id] = ++n;
			int portAfter = PortValue(__instance);
			bool? lastAfter = LastSetFlag(__instance);
			bool changed = portAfter != __state.portBefore || !Equals(lastAfter, __state.lastBefore);
			if (n > CallsPerLocker && !changed)
				return;
			string stack = n <= CallsPerLocker ? " caller=" + Caller() : "";
			Log("Update #" + n + " depth=" + d + " originalRan=" + (__state.runOriginal?.ToString() ?? "?")
				+ " port " + __state.portBefore + "->" + portAfter
				+ " last " + Show(__state.lastBefore) + "->" + Show(lastAfter)
				+ stack + "\n    before: " + __state.before + "\n    after:  " + Describe(__instance));
		}

		private static void GameSpawnPostfix()
		{
			Log("Game.OnSpawn; " + lockers.Count + " locker(s) spawned so far");
			GameScheduler.Instance.Schedule("FTE-DIAG sweep 2", 2f, _ => Sweep("2 s"));
			GameScheduler.Instance.Schedule("FTE-DIAG sweep 20", 20f, _ => Sweep("20 s"));
		}

		private static void Sweep(string when)
		{
			lockers.RemoveWhere(l => l == null);
			int wrong = 0;
			foreach (StorageLockerSmart locker in lockers)
			{
				object th = getThresholds(locker.gameObject);
				bool? last = LastSetFlag(locker);
				bool active = th != null && (bool)thActivated.GetValue(th);
				Operational op = (Operational)operationalField.GetValue(locker);
				int expected = active && op != null && op.IsOperational ? 1 : 0;
				int port = PortValue(locker);
				bool bad = th == null || last == null || port != expected;
				if (bad)
					wrong++;
				Log("Sweep " + when + (bad ? " MISMATCH " : " ok ") + "expected=" + expected + " " + Describe(locker));
			}
			Log("Sweep " + when + ": " + lockers.Count + " locker(s), " + wrong + " mismatched");
		}

		private static string Describe(StorageLockerSmart locker)
		{
			try
			{
				GameObject go = locker.gameObject;
				object th = getThresholds(go);
				Component thComp = th as Component;
				if (thComp == null)
					thComp = go.GetComponent(AccessTools.TypeByName("StorageRefrigeratorThresholds.StorageThresholds"));
				FilteredStorage fs = (FilteredStorage)filteredStorageField.GetValue(locker);
				string fill = "?";
				if (fs != null)
				{
					float stored = (float)amountStored.Invoke(fs, null);
					float cap = (float)maxCapacity.Invoke(fs, null);
					fill = stored.ToString("0.###") + "/" + cap.ToString("0.###") + "=" + (cap > 0 ? (stored / cap).ToString("0.####") : "n/a");
				}
				Operational op = (Operational)operationalField.GetValue(locker);
				string thState = thComp == null ? "comp=absent"
					: "comp=present spawned=" + ((KMonoBehaviour)thComp).isSpawned + " registered=" + (th != null)
						+ (th == null ? "" : " activated=" + thActivated.GetValue(th) + " high=" + thActivate.GetValue(th)
							+ " low=" + thDeactivate.GetValue(th) + " greenOnLow=" + thGreenOnLow.GetValue(th)
							+ " last=" + Show((bool?)lastSetFlag.GetValue(th)));
				return "t=" + (GameClock.Instance != null ? GameClock.Instance.GetTime().ToString("0.00") : "-")
					+ " frame=" + Time.frameCount
					+ " id=" + locker.GetInstanceID() + " cell=" + Grid.PosToCell(go)
					+ " name=" + go.GetProperName()
					+ " lockerSpawned=" + locker.isSpawned
					+ " fill=" + fill + " pct=" + locker.PercentFull.ToString("0.##")
					+ " vanillaActivated=" + vanillaActivated.GetValue(locker)
					+ " op=" + (op == null ? "null" : op.IsOperational + "/active=" + op.IsActive)
					+ " port=" + PortValue(locker) + " net=" + NetworkValue(locker)
					+ " | " + thState;
			}
			catch (Exception e)
			{
				return "describe failed: " + e.GetBaseException().Message;
			}
		}

		private static bool? LastSetFlag(StorageLockerSmart locker)
		{
			object th = getThresholds(locker.gameObject);
			return th == null ? null : (bool?)lastSetFlag.GetValue(th);
		}

		private static int PortValue(StorageLockerSmart locker)
		{
			try
			{
				LogicPorts ports = locker.GetComponent<LogicPorts>();
				return ports == null ? -1 : ports.GetOutputValue(FilteredStorage.FULL_PORT_ID);
			}
			catch
			{
				return -2;
			}
		}

		private static string NetworkValue(StorageLockerSmart locker)
		{
			try
			{
				LogicPorts ports = locker.GetComponent<LogicPorts>();
				if (ports == null || Game.Instance == null)
					return "-";
				LogicCircuitNetwork net = Game.Instance.logicCircuitManager.GetNetworkForCell(ports.GetPortCell(FilteredStorage.FULL_PORT_ID));
				return net == null ? "none" : net.OutputValue.ToString();
			}
			catch (Exception e)
			{
				return "err:" + e.GetBaseException().GetType().Name;
			}
		}

		private static string Caller()
		{
			var trace = new System.Diagnostics.StackTrace(2, false);
			var names = new List<string>();
			for (int i = 0; i < trace.FrameCount && names.Count < 4; i++)
			{
				MethodBase m = trace.GetFrame(i)?.GetMethod();
				if (m == null)
					continue;
				string name = (m.DeclaringType?.Name ?? "?") + "." + m.Name;
				if (name.Contains("SmartStorageDiag") || name.Contains("UpdateLogicAndActiveState"))
					continue;
				names.Add(name);
			}
			return string.Join("<", names);
		}

		private static string Show(bool? value) => value.HasValue ? value.Value.ToString() : "null";

		private static void Log(string message)
		{
			if (lines >= MaxLines)
				return;
			lines++;
			Debug.Log(Tag + message + (lines == MaxLines ? " (line cap reached; no more diag lines)" : ""));
		}
	}
}
