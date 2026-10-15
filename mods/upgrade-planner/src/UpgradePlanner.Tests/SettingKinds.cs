using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// For each kind of setting Blueprints Expanded stores (by its key), how to change it from what a
	/// new building has and how to read it back from the game's own state. Change returns null when
	/// it changed something, else why it could not.
	/// </summary>
	internal static class SettingKinds
	{
		internal sealed class Kind
		{
			public Func<GameObject, string> Change;
			public Func<GameObject, string> Read;
		}

		private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

		/// <summary>
		/// What the game shows the player for a building: the icons of its menu buttons (gathered
		/// the way the menu screen does) and the side screens that take it.
		/// </summary>
		internal static HashSet<string> Offered(GameObject go)
		{
			var offered = new HashSet<string>();
			var buttons = Traverse.Create(Game.Instance.userMenu).Field("buttons").GetValue<List<KeyValuePair<KIconButtonMenu.ButtonInfo, float>>>();
			buttons.Clear();
			go.Trigger((int)GameHashes.RefreshUserMenu);
			foreach (KeyValuePair<KIconButtonMenu.ButtonInfo, float> button in buttons)
				offered.Add("menu:" + button.Key.iconName);
			buttons.Clear();
			var screens = Traverse.Create(DetailsScreen.Instance).Field("sideScreens").GetValue<List<DetailsScreen.SideScreenRef>>();
			if (screens != null)
				foreach (DetailsScreen.SideScreenRef screen in screens)
				{
					bool valid;
					try
					{
						valid = screen.screenPrefab != null && screen.screenPrefab.IsValidForTarget(go);
					}
					catch (Exception)
					{
						valid = false;
					}
					if (valid)
						offered.Add("screen:" + screen.screenPrefab.GetType().Name);
				}
			return offered;
		}

		/// <summary>
		/// For a setting the player changes through one control, that control; the setting is only
		/// changed when the building offers it. Settings of their own side screen are offered by
		/// having the component at all.
		/// </summary>
		internal static readonly Dictionary<string, string> Control = new Dictionary<string, string>
		{
			["BuildingEnabledButton"] = "menu:action_building_disabled",
			// The meteor blaster has a tree filter of its own but shows the player a flat target list.
			["TreeFilterable"] = "screen:TreeFilterableSideScreen",
			["FlatTagFilterable"] = "screen:FlatTagFilterSideScreen",
		};

		private static string F(float v) => v.ToString("0.###", Inv);

		/// <summary>A value in [min, max] a little over a third of the way, or two thirds if that is the current one.</summary>
		private static float Pick(float min, float max, float current)
		{
			float a = min + (max - min) * 0.37f;
			return Mathf.Abs(a - current) > 0.01f * (max - min) ? a : min + (max - min) * 0.63f;
		}

		/// <summary>A switch the player flips, not a sensor's output.</summary>
		private static bool Manual(GameObject go) => C<Switch>(go).manuallyControlled && go.GetComponent<IThresholdSwitch>() == null;

		private static T C<T>(GameObject go) where T : class => go.GetComponent(typeof(T)) as T;

		/// <summary>The start of what Change returns when the setting does not apply to this building (not a failure).</summary>
		internal const string NotHere = "not here: ";

		private static string Slider(ISliderControl s, int index = 0)
		{
			if (s == null)
				return NotHere + "no slider";
			if (s.GetSliderMax(index) <= s.GetSliderMin(index))
				return NotHere + "the slider has no range";
			s.SetSliderValue(Pick(s.GetSliderMin(index), s.GetSliderMax(index), s.GetSliderValue(index)), index);
			return null;
		}

		private static string ReadSlider(ISliderControl s, int index = 0) => s == null ? "-" : F(s.GetSliderValue(index));

		/// <summary>Another mod's component on the building, by type name.</summary>
		private static Component Comp(GameObject go, string type)
		{
			Type t = AccessTools.TypeByName(type);
			return t != null ? go.GetComponent(t) : null;
		}

		/// <summary>The player's "suppress" on whatever the building shows now; nothing shown, nothing to suppress.</summary>
		private static string SuppressAll(GameObject go, string type, string method, string list)
		{
			Component comp = Comp(go, type);
			if (comp == null)
				return NotHere + "no " + type;
			Traverse.Create(comp).Method(method).GetValue();
			var items = Traverse.Create(comp).Field(list).GetValue<List<string>>();
			return items != null && items.Count > 0 ? null : NotHere + "nothing shown to suppress";
		}

		private static string ReadList(GameObject go, string type, string list)
		{
			Component comp = Comp(go, type);
			var items = comp != null ? Traverse.Create(comp).Field(list).GetValue<List<string>>() : null;
			return items == null ? "" : string.Join(",", items.OrderBy(n => n, StringComparer.Ordinal));
		}

		public static readonly Dictionary<string, Kind> All = new Dictionary<string, Kind>
		{
			["AccessControl"] = new Kind
			{
				Change = go => { C<AccessControl>(go).SetDefaultPermission(GameTags.Minions.Models.Standard, AccessControl.Permission.Neither); return null; },
				Read = go => C<AccessControl>(go).GetDefaultPermission(GameTags.Minions.Models.Standard).ToString(),
			},
			["Automatable"] = new Kind
			{
				Change = go => { Automatable a = C<Automatable>(go); a.SetAutomationOnly(!a.GetAutomationOnly()); return null; },
				Read = go => C<Automatable>(go).GetAutomationOnly().ToString(),
			},
			["BuildingEnabledButton"] = new Kind
			{
				Change = go => { C<BuildingEnabledButton>(go).IsEnabled = false; return null; },
				Read = go => C<BuildingEnabledButton>(go).IsEnabled.ToString(),
			},
			["Clinic"] = new Kind
			{
				Change = go => Slider(C<Clinic>(go)),
				Read = go => ReadSlider(C<Clinic>(go)),
			},
			["DirectionControl"] = new Kind
			{
				Change = go => { Traverse.Create(C<DirectionControl>(go)).Method("SetAllowedDirection", WorkableReactable.AllowedDirection.Left).GetValue(); return null; },
				Read = go => C<DirectionControl>(go).allowedDirection.ToString(),
			},
			["Door"] = new Kind
			{
				Change = go => { C<Door>(go).QueueStateChange(Door.ControlState.Opened); return null; },
				Read = go => C<Door>(go).RequestedState.ToString(),
			},
			["EnergyGenerator"] = new Kind
			{
				Change = go => Slider(C<EnergyGenerator>(go)),
				Read = go => ReadSlider(C<EnergyGenerator>(go)),
			},
			["Filterable"] = new Kind
			{
				Change = go =>
				{
					Filterable f = C<Filterable>(go);
					Tag other = f.GetTagOptions().SelectMany(kv => kv.Value).Where(tag => tag != f.SelectedTag).OrderBy(tag => tag.Name, StringComparer.Ordinal).FirstOrDefault();
					if (!other.IsValid)
						return "no other filter option";
					f.SelectedTag = other;
					return null;
				},
				Read = go => C<Filterable>(go).SelectedTag.Name,
			},
			["FlatTagFilterable"] = new Kind
			{
				Change = go =>
				{
					FlatTagFilterable f = C<FlatTagFilterable>(go);
					if (f.tagOptions.Count == 0)
						return "no tag options";
					f.ToggleTag(f.tagOptions[0]);
					return null;
				},
				Read = go => string.Join(",", C<FlatTagFilterable>(go).selectedTags.Select(tag => tag.Name).OrderBy(n => n, StringComparer.Ordinal)),
			},
			["HEPBattery"] = new Kind
			{
				Change = go => Slider(go.GetSMI<HEPBattery.Instance>()),
				Read = go => ReadSlider(go.GetSMI<HEPBattery.Instance>()),
			},
			["HighEnergyParticleRedirector"] = new Kind
			{
				Change = go =>
				{
					HighEnergyParticleRedirector r = C<HighEnergyParticleRedirector>(go);
					r.Direction = r.Direction == EightDirection.Left ? EightDirection.Up : EightDirection.Left;
					return null;
				},
				Read = go => C<HighEnergyParticleRedirector>(go).Direction.ToString(),
			},
			["HighEnergyParticleSpawner"] = new Kind
			{
				Change = go =>
				{
					HighEnergyParticleSpawner s = C<HighEnergyParticleSpawner>(go);
					s.Direction = s.Direction == EightDirection.Left ? EightDirection.Up : EightDirection.Left;
					return Slider(s);
				},
				Read = go => { HighEnergyParticleSpawner s = C<HighEnergyParticleSpawner>(go); return s.Direction + " " + ReadSlider(s); },
			},
			["IActivationRangeTarget"] = new Kind
			{
				Change = go =>
				{
					IActivationRangeTarget r = C<IActivationRangeTarget>(go);
					float lo = Pick(r.MinValue, r.MaxValue, r.ActivateValue);
					float hi = Mathf.Max(lo + 1f, Pick(r.MinValue, r.MaxValue, r.DeactivateValue) + 20f);
					r.ActivateValue = lo;
					r.DeactivateValue = Mathf.Min(hi, r.MaxValue);
					return null;
				},
				Read = go => { IActivationRangeTarget r = C<IActivationRangeTarget>(go); return F(r.ActivateValue) + "-" + F(r.DeactivateValue); },
			},
			["IThresholdSwitch"] = new Kind
			{
				Change = go =>
				{
					IThresholdSwitch s = C<IThresholdSwitch>(go);
					s.Threshold = Pick(s.RangeMin, s.RangeMax, s.Threshold);
					s.ActivateAboveThreshold = !s.ActivateAboveThreshold;
					return null;
				},
				Read = go => { IThresholdSwitch s = C<IThresholdSwitch>(go); return F(s.Threshold) + (s.ActivateAboveThreshold ? " above" : " below"); },
			},
			["IUserControlledCapacity"] = new Kind
			{
				Change = go =>
				{
					IUserControlledCapacity c = C<IUserControlledCapacity>(go);
					c.UserMaxCapacity = Pick(c.MinCapacity, c.MaxCapacity, c.UserMaxCapacity);
					return null;
				},
				Read = go => F(C<IUserControlledCapacity>(go).UserMaxCapacity),
			},
			["LimitValve"] = new Kind
			{
				Change = go => { LimitValve v = C<LimitValve>(go); v.Limit = Pick(0f, v.maxLimitKg, v.Limit); return null; },
				Read = go => F(C<LimitValve>(go).Limit),
			},
			["LogicAlarm"] = new Kind
			{
				Change = go =>
				{
					LogicAlarm a = C<LogicAlarm>(go);
					a.notificationName = "Planner alarm";
					a.notificationTooltip = "Planner tooltip";
					a.pauseOnNotify = !a.pauseOnNotify;
					a.zoomOnNotify = !a.zoomOnNotify;
					a.notificationType = a.notificationType == NotificationType.Bad ? NotificationType.Neutral : NotificationType.Bad;
					return null;
				},
				Read = go => { LogicAlarm a = C<LogicAlarm>(go); return a.notificationName + "|" + a.notificationTooltip + "|" + a.pauseOnNotify + "|" + a.zoomOnNotify + "|" + a.notificationType; },
			},
			["LogicClusterLocationSensor"] = new Kind
			{
				Change = go => { LogicClusterLocationSensor s = C<LogicClusterLocationSensor>(go); s.SetSpaceEnabled(!s.ActiveInSpace); return null; },
				Read = go => C<LogicClusterLocationSensor>(go).ActiveInSpace.ToString(),
			},
			["LogicCounter"] = new Kind
			{
				Change = go =>
				{
					LogicCounter c = C<LogicCounter>(go);
					c.maxCount = c.maxCount == 7 ? 5 : 7;
					c.advancedMode = !c.advancedMode;
					c.resetCountAtMax = !c.resetCountAtMax;
					return null;
				},
				Read = go => { LogicCounter c = C<LogicCounter>(go); return c.maxCount + "|" + c.advancedMode + "|" + c.resetCountAtMax; },
			},
			["LogicCritterCountSensor"] = new Kind
			{
				Change = go => { LogicCritterCountSensor s = C<LogicCritterCountSensor>(go); s.countEggs = !s.countEggs; return null; },
				Read = go => { LogicCritterCountSensor s = C<LogicCritterCountSensor>(go); return s.countEggs + "|" + s.countCritters; },
			},
			["LogicGateBuffer"] = new Kind
			{
				Change = go => { LogicGateBuffer g = C<LogicGateBuffer>(go); g.DelayAmount = g.DelayAmount == 3f ? 4f : 3f; return null; },
				Read = go => F(C<LogicGateBuffer>(go).DelayAmount),
			},
			["LogicGateFilter"] = new Kind
			{
				Change = go => { LogicGateFilter g = C<LogicGateFilter>(go); g.DelayAmount = g.DelayAmount == 3f ? 4f : 3f; return null; },
				Read = go => F(C<LogicGateFilter>(go).DelayAmount),
			},
			["LogicRibbonReader"] = new Kind
			{
				Change = go => { LogicRibbonReader r = C<LogicRibbonReader>(go); r.SetBitSelection(r.GetBitSelection() == 2 ? 1 : 2); return null; },
				Read = go => C<LogicRibbonReader>(go).GetBitSelection().ToString(),
			},
			["LogicRibbonWriter"] = new Kind
			{
				Change = go => { LogicRibbonWriter w = C<LogicRibbonWriter>(go); w.SetBitSelection(w.GetBitSelection() == 2 ? 1 : 2); return null; },
				Read = go => C<LogicRibbonWriter>(go).GetBitSelection().ToString(),
			},
			["LogicTimeOfDaySensor"] = new Kind
			{
				Change = go => { LogicTimeOfDaySensor s = C<LogicTimeOfDaySensor>(go); s.startTime = 0.3f; s.duration = 0.25f; return null; },
				Read = go => { LogicTimeOfDaySensor s = C<LogicTimeOfDaySensor>(go); return F(s.startTime) + "|" + F(s.duration); },
			},
			["LogicTimerSensor"] = new Kind
			{
				Change = go => { LogicTimerSensor s = C<LogicTimerSensor>(go); s.onDuration = 7f; s.offDuration = 13f; s.displayCyclesMode = !s.displayCyclesMode; return null; },
				Read = go => { LogicTimerSensor s = C<LogicTimerSensor>(go); return F(s.onDuration) + "|" + F(s.offDuration) + "|" + s.displayCyclesMode; },
			},
			["PixelPack"] = new Kind
			{
				Change = go =>
				{
					PixelPack p = C<PixelPack>(go);
					if (p.colorSettings == null || p.colorSettings.Count == 0)
						return "no colours";
					PixelPack.ColorPair pair = p.colorSettings[0];
					pair.activeColor = new Color(0.2f, 0.4f, 0.6f, 1f);
					pair.standbyColor = new Color(0.7f, 0.1f, 0.3f, 1f);
					p.colorSettings[0] = pair;
					p.UpdateColors();
					return null;
				},
				Read = go => string.Join(";", C<PixelPack>(go).colorSettings.Select(c => ColorUtility.ToHtmlStringRGB(c.activeColor) + "/" + ColorUtility.ToHtmlStringRGB(c.standbyColor))),
			},
			["SingleEntityReceptacle"] = new Kind
			{
				Change = go =>
				{
					// The first of what it takes that the order sticks to (a farm tile turns some seeds down).
					SingleEntityReceptacle r = C<SingleEntityReceptacle>(go);
					Tag before = r.requestedEntityTag;
					var tried = new List<string>();
					foreach (Tag tag in r.possibleDepositObjectTags)
						foreach (GameObject prefab in Assets.GetPrefabsWithTag(tag).OrderBy(p => p.PrefabID().Name, StringComparer.Ordinal))
							if (prefab != null && prefab.PrefabID() != before && r.IsValidEntity(prefab))
							{
								r.CreateOrder(prefab.PrefabID(), Tag.Invalid);
								if (r.requestedEntityTag != before)
									return null;
								tried.Add(prefab.PrefabID().Name);
								if (tried.Count >= 20)
									return "no request stuck; tried " + string.Join(", ", tried);
							}
					return tried.Count > 0 ? "no request stuck; tried " + string.Join(", ", tried) : "nothing to request";
				},
				Read = go => C<SingleEntityReceptacle>(go).requestedEntityTag.Name ?? "",
			},
			["SpaceHeater"] = new Kind
			{
				Change = go => Slider(C<SpaceHeater>(go)),
				Read = go => ReadSlider(C<SpaceHeater>(go)),
			},
			["StorageTile"] = new Kind
			{
				Change = go => { go.GetSMI<StorageTile.Instance>().SetTargetItem(SimHashes.Sand.CreateTag()); return null; },
				Read = go => go.GetSMI<StorageTile.Instance>().TargetTag.Name,
			},
			["Switch"] = new Kind
			{
				// Only a switch the player flips; a sensor's switch is its output, set by what it senses.
				Change = go =>
				{
					Switch s = C<Switch>(go);
					if (!Manual(go))
						return null;
					Traverse.Create(s).Method("SetState", !s.IsSwitchedOn).GetValue();
					return null;
				},
				Read = go => Manual(go) ? C<Switch>(go).IsSwitchedOn.ToString() : "sensor",
			},
			["TreeFilterable"] = new Kind
			{
				Change = go =>
				{
					// One filter flipped: a category it offers, or failing that an item of one, the first the filter keeps.
					TreeFilterable f = C<TreeFilterable>(go);
					Storage storage = f.GetFilterStorage();
					List<Tag> categories = storage != null ? storage.storageFilters : null;
					if (categories == null || categories.Count == 0)
						return "no storage filters";
					string before = string.Join(",", f.AcceptedTags.Select(tag => tag.Name).OrderBy(n => n, StringComparer.Ordinal));
					// One the player has on, switched off: always one of the side screen's own choices.
					Tag on = f.AcceptedTags.OrderBy(tag => tag.Name, StringComparer.Ordinal).FirstOrDefault();
					if (on.IsValid)
					{
						var fewer = new HashSet<Tag>(f.AcceptedTags);
						fewer.Remove(on);
						f.UpdateFilters(fewer);
						if (string.Join(",", f.AcceptedTags.Select(tag => tag.Name).OrderBy(n => n, StringComparer.Ordinal)) != before)
							return null;
					}
					var candidates = categories.OrderBy(tag => tag.Name, StringComparer.Ordinal).ToList();
					foreach (Tag category in categories.OrderBy(tag => tag.Name, StringComparer.Ordinal))
						candidates.AddRange(Assets.GetPrefabsWithTag(category).Select(p => p.PrefabID()).OrderBy(tag => tag.Name, StringComparer.Ordinal).Take(5));
					foreach (Tag pick in candidates)
					{
						var tags = new HashSet<Tag>(f.AcceptedTags);
						if (!tags.Remove(pick))
							tags.Add(pick);
						f.UpdateFilters(tags);
						if (string.Join(",", f.AcceptedTags.Select(tag => tag.Name).OrderBy(n => n, StringComparer.Ordinal)) != before)
							return null;
					}
					return "no filter change stuck";
				},
				Read = go => string.Join(",", C<TreeFilterable>(go).AcceptedTags.Select(tag => tag.Name).OrderBy(n => n, StringComparer.Ordinal)),
			},
			["UserNameable"] = new Kind
			{
				Change = go => { C<UserNameable>(go).SetName("Planner name"); return null; },
				Read = go => C<UserNameable>(go).savedName,
			},
			// Other mods' settings, kept through the planner's own registrations (OtherModSettings).
			["UpgradePlanner.SuppressNotifications.Notifications"] = new Kind
			{
				Change = go => SuppressAll(go, "SuppressNotifications.NotificationsSuppressedComp", "SuppressNotifications", "suppressedNotifications"),
				Read = go => ReadList(go, "SuppressNotifications.NotificationsSuppressedComp", "suppressedNotifications"),
			},
			["UpgradePlanner.SuppressNotifications.StatusItems"] = new Kind
			{
				Change = go => SuppressAll(go, "SuppressNotifications.StatusItemsSuppressedComp", "SuppressStatusItems", "suppressedStatusItemTitles"),
				Read = go => ReadList(go, "SuppressNotifications.StatusItemsSuppressedComp", "suppressedStatusItemTitles"),
			},
			["UpgradePlanner.SuppressNotifications.DamageBar"] = new Kind
			{
				Change = go => { Traverse.Create(Comp(go, "SuppressNotifications.BuildingHealthSuppressedComp")).Method("SetDamageBar", true).GetValue(); return null; },
				Read = go => Traverse.Create(Comp(go, "SuppressNotifications.BuildingHealthSuppressedComp")).Field("_hideDMGBar").GetValue<bool>().ToString(),
			},
			["UpgradePlanner.NoManualDelivery.Hold"] = new Kind
			{
				Change = go =>
				{
					Component a = Comp(go, "NoManualDelivery.Automatable2");
					if (a == null || !Traverse.Create(a).Field("allowHold").GetValue<bool>())
						return NotHere + "no hold";
					bool hold = Traverse.Create(a).Method("GetAutomationHold").GetValue<bool>();
					Traverse.Create(a).Method("SetAutomation", false, !hold).GetValue();
					return null;
				},
				Read = go => { Component a = Comp(go, "NoManualDelivery.Automatable2"); return a == null ? "-" : ((Automatable)a).GetAutomationOnly() + "|" + Traverse.Create(a).Method("GetAutomationHold").GetValue<bool>(); },
			},
			["Valve"] = new Kind
			{
				Change = go => { Valve v = C<Valve>(go); v.ChangeFlow(Pick(0f, v.MaxFlow, v.DesiredFlow)); return null; },
				Read = go => F(C<Valve>(go).DesiredFlow),
			},
		};
	}
}
