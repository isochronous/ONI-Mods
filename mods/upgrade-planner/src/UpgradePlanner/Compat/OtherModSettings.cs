using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Per-building settings of other mods that Blueprints Expanded does not store on its own,
	/// registered with its API so that its blueprints and the planner's rebuilds keep them:
	/// Suppress Notifications' suppressed notifications, status items and damage bar, and No
	/// Manual Delivery's "hold" flag. Each is registered only when its mod's types are loaded, and
	/// read and set the way that mod's own copy settings does.
	/// </summary>
	internal static class OtherModSettings
	{
		private const string Api = "BlueprintsV2.ModAPI.API_Methods";

		/// <summary>Registers what it finds; called once Blueprints Expanded is bound.</summary>
		public static void Register()
		{
			Type api = AccessTools.TypeByName(Api);
			MethodInfo register = api != null ? AccessTools.Method(api, "RegisterAdditionalStorableBuildingData") : null;
			Type getType = api != null ? api.GetNestedType("GetBlueprintDataDelegate", BindingFlags.Public | BindingFlags.NonPublic) : null;
			Type setType = api != null ? api.GetNestedType("SetBlueprintDataDelegate", BindingFlags.Public | BindingFlags.NonPublic) : null;
			if (register == null || getType == null || setType == null)
			{
				Debug.LogWarning("[UpgradePlanner] Blueprints Expanded's API to store other mods' settings was not found.");
				return;
			}
			var registered = new List<string>();
			void Add(string id, Func<GameObject, JObject> get, Action<GameObject, JObject> set)
			{
				Delegate getter = Delegate.CreateDelegate(getType, get.Target, get.Method);
				Delegate setter = Delegate.CreateDelegate(setType, set.Target, set.Method);
				object[] args = register.GetParameters().Length == 4 ? new object[] { id, getter, setter, 0 } : new object[] { id, getter, setter };
				register.Invoke(null, args);
				registered.Add(id);
			}
			if (SuppressNotifications.Find())
			{
				Add("UpgradePlanner.SuppressNotifications.Notifications", SuppressNotifications.GetNotifications, SuppressNotifications.SetNotifications);
				Add("UpgradePlanner.SuppressNotifications.StatusItems", SuppressNotifications.GetStatusItems, SuppressNotifications.SetStatusItems);
				Add("UpgradePlanner.SuppressNotifications.DamageBar", SuppressNotifications.GetDamageBar, SuppressNotifications.SetDamageBar);
			}
			if (NoManualDelivery.Find())
				Add("UpgradePlanner.NoManualDelivery.Hold", NoManualDelivery.GetHold, NoManualDelivery.SetHold);
			if (registered.Count > 0)
				Debug.Log("[UpgradePlanner] Settings of other mods kept with Blueprints Expanded: " + string.Join(", ", registered));
		}

		private static JArray ToArray(List<string> list) => list != null ? new JArray(list) : new JArray();

		private static List<string> FromArray(JObject data) => data["items"] is JArray array ? array.ToObject<List<string>>() : new List<string>();

		/// <summary>Suppress Notifications (Aze): three components, each a list or a flag saved with the building.</summary>
		private static class SuppressNotifications
		{
			private static Type notifications, statusItems, damageBar;
			private static FieldInfo notificationsList, statusItemsList, damageBarHidden;
			private static MethodInfo refreshNotifications, refreshStatusItems, suppressable, suppressed, setDamageBar;

			public static bool Find()
			{
				notifications = AccessTools.TypeByName("SuppressNotifications.NotificationsSuppressedComp");
				statusItems = AccessTools.TypeByName("SuppressNotifications.StatusItemsSuppressedComp");
				damageBar = AccessTools.TypeByName("SuppressNotifications.BuildingHealthSuppressedComp");
				if (notifications == null || statusItems == null || damageBar == null)
					return false;
				notificationsList = AccessTools.Field(notifications, "suppressedNotifications");
				refreshNotifications = AccessTools.Method(notifications, "RefreshNotifications");
				statusItemsList = AccessTools.Field(statusItems, "suppressedStatusItemTitles");
				suppressable = AccessTools.Method(statusItems, "GetSuppressableStatusItems");
				suppressed = AccessTools.Method(statusItems, "GetSuppressedStatusItems");
				refreshStatusItems = AccessTools.Method(statusItems, "RefreshStatusItems");
				damageBarHidden = AccessTools.Field(damageBar, "_hideDMGBar");
				setDamageBar = AccessTools.Method(damageBar, "SetDamageBar");
				bool found = notificationsList != null && refreshNotifications != null && statusItemsList != null && suppressable != null
					&& suppressed != null && refreshStatusItems != null && damageBarHidden != null && setDamageBar != null;
				if (!found)
					Debug.LogWarning("[UpgradePlanner] Suppress Notifications has changed; its settings are not kept on upgrade.");
				return found;
			}

			public static JObject GetNotifications(GameObject go)
			{
				Component comp = go.GetComponent(notifications);
				return comp != null ? new JObject { ["items"] = ToArray((List<string>)notificationsList.GetValue(comp)) } : null;
			}

			public static void SetNotifications(GameObject go, JObject data)
			{
				Component comp = go.GetComponent(notifications);
				if (comp == null || data == null)
					return;
				notificationsList.SetValue(comp, FromArray(data));
				refreshNotifications.Invoke(comp, null);
			}

			public static JObject GetStatusItems(GameObject go)
			{
				Component comp = go.GetComponent(statusItems);
				return comp != null ? new JObject { ["items"] = ToArray((List<string>)statusItemsList.GetValue(comp)) } : null;
			}

			public static void SetStatusItems(GameObject go, JObject data)
			{
				Component comp = go.GetComponent(statusItems);
				if (comp == null || data == null)
					return;
				object wasActive = suppressable.Invoke(comp, null);
				object wasSuppressed = suppressed.Invoke(comp, null);
				statusItemsList.SetValue(comp, FromArray(data));
				refreshStatusItems.Invoke(comp, new[] { wasActive, wasSuppressed });
			}

			public static JObject GetDamageBar(GameObject go)
			{
				Component comp = go.GetComponent(damageBar);
				return comp != null ? new JObject { ["hide"] = (bool)damageBarHidden.GetValue(comp) } : null;
			}

			public static void SetDamageBar(GameObject go, JObject data)
			{
				Component comp = go.GetComponent(damageBar);
				if (comp == null || data == null)
					return;
				bool hide = data.Value<bool>("hide");
				// SetDamageBar reads BuildingHP's state machine, which a building only has once spawned;
				// before that the field is enough, as the bar reads it when it is created.
				if (((KMonoBehaviour)comp).isSpawned)
					setDamageBar.Invoke(comp, new object[] { hide });
				else
					damageBarHidden.SetValue(comp, hide);
			}
		}

		/// <summary>No Manual Delivery (Sanchozz): its automatable adds "hold" to the game's "automation only", which Blueprints Expanded keeps already.</summary>
		private static class NoManualDelivery
		{
			private static Type automatable;
			private static MethodInfo getHold, setAutomation;

			public static bool Find()
			{
				automatable = AccessTools.TypeByName("NoManualDelivery.Automatable2");
				if (automatable == null)
					return false;
				getHold = AccessTools.Method(automatable, "GetAutomationHold");
				setAutomation = AccessTools.Method(automatable, "SetAutomation", new[] { typeof(bool), typeof(bool) });
				if (getHold == null || setAutomation == null)
				{
					Debug.LogWarning("[UpgradePlanner] No Manual Delivery has changed; its hold setting is not kept on upgrade.");
					return false;
				}
				return true;
			}

			public static JObject GetHold(GameObject go)
			{
				Component comp = go.GetComponent(automatable);
				return comp != null ? new JObject { ["only"] = ((Automatable)comp).GetAutomationOnly(), ["hold"] = (bool)getHold.Invoke(comp, null) } : null;
			}

			public static void SetHold(GameObject go, JObject data)
			{
				Component comp = go.GetComponent(automatable);
				if (comp != null && data != null)
					setAutomation.Invoke(comp, new object[] { data.Value<bool>("only"), data.Value<bool>("hold") });
			}
		}
	}
}
