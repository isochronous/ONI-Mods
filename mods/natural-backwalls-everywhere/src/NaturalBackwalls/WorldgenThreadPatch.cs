using System;
using System.Reflection;
using HarmonyLib;
using ProcGenGame;

namespace NaturalBackwalls
{
	/// <summary>
	/// Cluster.BeginGeneration is the body of the worldgen thread and has no exception handler of
	/// its own: an exception that escapes the per-stage handlers kills the thread, the cluster
	/// status stays Generating, and the loading screen waits forever with no message. The game's
	/// crash reporter only listens to the main thread, so nothing is shown. This finalizer turns
	/// that into the game's normal "not all seeds germinate" failure and writes the exception to
	/// the log, so a report about a stuck loading screen comes with a stack trace.
	/// </summary>
	[HarmonyPatch(typeof(Cluster), "BeginGeneration")]
	public static class Cluster_BeginGeneration_Patch
	{
		private static readonly MethodInfo SetStatus = AccessTools.PropertySetter(typeof(Cluster), nameof(Cluster.Status));
		private static readonly FieldInfo ErrorCallbackField = AccessTools.Field(typeof(WorldGen), "errorCallback");

		public static Exception Finalizer(Exception __exception, Cluster __instance)
		{
			if (__exception == null)
				return null;
			try
			{
				Debug.LogWarning("[NaturalBackwalls] World generation stopped with an unhandled error; reporting it instead of leaving the loading screen waiting: " + __exception);
				WorldGen world = __instance.currentWorld;
				var callback = world != null ? ErrorCallbackField?.GetValue(world) as Action<OfflineWorldGen.ErrorInfo> : null;
				if (callback != null)
				{
					string seed = CustomGameSettings.Instance != null ? CustomGameSettings.Instance.GetSettingsCoordinate() : "?";
					callback(new OfflineWorldGen.ErrorInfo
					{
						errorDesc = string.Format(STRINGS.UI.FRONTEND.SUPPORTWARNINGS.WORLD_GEN_FAILURE, seed),
						exception = __exception,
					});
				}
				else
					world?.ReportWorldGenError(__exception);
				SetStatus?.Invoke(__instance, new object[] { Cluster.WorldGenStatus.Failed });
			}
			catch (Exception e)
			{
				Debug.LogWarning("[NaturalBackwalls] Could not report the worldgen error: " + e.Message);
			}
			return null;
		}
	}
}
