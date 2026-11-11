using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;

namespace FastWirelessAutomationFix
{
	/// <summary>
	/// Fast Wireless Automation+ (Workshop 1722812098) keeps its channels in a static table,
	/// FastWirelessAutomation.Networks.ChannelManager, and empties it only in a postfix on
	/// Game.OnSpawn. When a second save loads in one session the table still holds the receivers of
	/// the first game, destroyed by then. The new game's emitters signal during their own
	/// OnPrefabInit, before Game.OnSpawn runs, call those stale receivers and throw a
	/// NullReferenceException that leaves the emitter half-initialised.
	///
	/// This empties the table with the mod's own ChannelManager.Reset when the old game is torn down
	/// (Game.DestroyInstances) and again when the new Game is set up (Game.OnPrefabInit), which runs
	/// before the save's buildings are created. Reset is safe on an empty table, and every wireless
	/// building registers again in its own OnSpawn. The mod is reached by name only; nothing is
	/// referenced at build time.
	/// </summary>
	public sealed class FastWirelessAutomationFixMod : UserMod2
	{
		private const string ChannelManagerType = "FastWirelessAutomation.Networks.ChannelManager";

		private static System.Action reset;

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[FastWirelessAutomationFix] Loaded version " + typeof(FastWirelessAutomationFixMod).Assembly.GetName().Version);
		}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
		{
			base.OnAllModsLoaded(harmony, mods);
			try
			{
				var type = AccessTools.TypeByName(ChannelManagerType);
				if (type == null)
				{
					Debug.Log("[FastWirelessAutomationFix] Fast Wireless Automation+ not found; nothing to do");
					return;
				}

				var method = AccessTools.Method(type, "Reset", Type.EmptyTypes);
				if (method == null || !method.IsStatic)
				{
					Debug.LogWarning("[FastWirelessAutomationFix] skipped: " + ChannelManagerType + ".Reset not found");
					return;
				}

				var destroy = AccessTools.Method(typeof(Game), "DestroyInstances");
				var prefabInit = AccessTools.Method(typeof(Game), "OnPrefabInit");
				if (destroy == null || prefabInit == null)
				{
					Debug.LogWarning("[FastWirelessAutomationFix] skipped: Game.DestroyInstances or Game.OnPrefabInit not found");
					return;
				}

				reset = (System.Action)Delegate.CreateDelegate(typeof(System.Action), method);
				harmony.Patch(destroy, postfix: new HarmonyMethod(typeof(FastWirelessAutomationFixMod), nameof(ResetChannels)));
				harmony.Patch(prefabInit, prefix: new HarmonyMethod(typeof(FastWirelessAutomationFixMod), nameof(ResetChannels)));
				Debug.Log("[FastWirelessAutomationFix] Patched Game.DestroyInstances and Game.OnPrefabInit to reset the Fast Wireless Automation+ channels");
			}
			catch (Exception e)
			{
				Debug.LogWarning("[FastWirelessAutomationFix] skipped: " + e);
			}
		}

		private static void ResetChannels()
		{
			try
			{
				reset?.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogWarning("[FastWirelessAutomationFix] Reset failed: " + e);
			}
		}
	}
}
