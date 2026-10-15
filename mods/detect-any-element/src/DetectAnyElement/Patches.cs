using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DetectAnyElement
{
	/// <summary>Switch.Toggle is protected; the sensors flip themselves through it.</summary>
	internal static class Switches
	{
		public static readonly System.Action<Switch> Toggle =
			AccessTools.MethodDelegate<System.Action<Switch>>(AccessTools.Method(typeof(Switch), "Toggle"));
	}

	/// <summary>
	/// The sensors' side screen lists "None" as a row of its own above the element categories;
	/// "Any" goes in as a second such row directly under it. The screen is shared by the pipe
	/// element sensors and the in-world ones (isLogicFilter), and by the conveyor and pipe
	/// filters (not), which keep only None.
	/// </summary>
	[HarmonyPatch(typeof(FilterSideScreen), nameof(FilterSideScreen.SetTarget))]
	public static class FilterSideScreen_SetTarget_Patch
	{
		private static readonly AccessTools.FieldRef<FilterSideScreen, Filterable> TargetFilterable =
			AccessTools.FieldRefAccess<FilterSideScreen, Filterable>("targetFilterable");
		private static readonly AccessTools.FieldRef<SingleItemSelectionRow, Image> RowIcon =
			AccessTools.FieldRefAccess<SingleItemSelectionRow, Image>("icon");

		/// <summary>One Any row per screen, kept like the screen keeps its None row.</summary>
		private static readonly ConditionalWeakTable<FilterSideScreen, SingleItemSelectionRow> anyRows =
			new ConditionalWeakTable<FilterSideScreen, SingleItemSelectionRow>();

		public static void Postfix(FilterSideScreen __instance)
		{
			if (!__instance.isLogicFilter)
				return;
			Filterable filterable = TargetFilterable(__instance);
			if (filterable == null)
				return;
			if (!anyRows.TryGetValue(__instance, out SingleItemSelectionRow row) || row == null)
			{
				row = __instance.GetOrCreateItemRow(AnyElement.Tag);
				// Like None, a word without a picture: the game gives an unknown tag a grey "?".
				Image icon = RowIcon(row);
				if (icon != null)
					icon.gameObject.SetActive(false);
				// The game's .NET Framework has no AddOrUpdate on this table.
				anyRows.Remove(__instance);
				anyRows.Add(__instance, row);
			}
			row.gameObject.SetActive(true);
			// The screen has just put None first; Any goes right under it.
			row.transform.SetSiblingIndex(1);
			if (AnyElement.Is(filterable.SelectedTag))
				__instance.SetSelectedItem(row);
		}
	}

	/// <summary>
	/// A pipe or rail sensor on Any is on while the conduit at its cell carries anything at
	/// all, the same test the game makes for one element, minus the element.
	/// </summary>
	[HarmonyPatch(typeof(ConduitElementSensor), "ConduitUpdate")]
	public static class ConduitElementSensor_ConduitUpdate_Patch
	{
		public static bool Prefix(ConduitElementSensor __instance)
		{
			Filterable filterable = __instance.GetComponent<Filterable>();
			if (filterable == null || !AnyElement.Is(filterable.SelectedTag))
				return true;
			bool hasMass = ConduitHasContents(__instance);
			if (__instance.IsSwitchedOn != hasMass)
				Switches.Toggle(__instance);
			return false;
		}

		private static bool ConduitHasContents(ConduitElementSensor sensor)
		{
			int cell = Grid.PosToCell(sensor);
			ConduitType type = sensor.conduitType;
			if (type == ConduitType.Liquid || type == ConduitType.Gas)
				return Conduit.GetFlowManager(type).GetContents(cell).mass > 0f;
			SolidConduitFlow flow = SolidConduit.GetFlowManager();
			Pickupable pickupable = flow.GetPickupable(flow.GetContents(cell).pickupableHandle);
			return pickupable != null && pickupable.PrimaryElement != null && pickupable.PrimaryElement.Mass > 0f;
		}
	}

	/// <summary>
	/// An in-world sensor on Any is on while its cell holds any element of its state: any gas
	/// for the Gas Element Sensor, any liquid for the Liquid one (vacuum is neither). The game's
	/// eight-sample window against flicker is kept.
	/// </summary>
	[HarmonyPatch(typeof(LogicElementSensor), nameof(LogicElementSensor.Sim200ms))]
	public static class LogicElementSensor_Sim200ms_Patch
	{
		private const int WindowSize = 8;
		private static readonly AccessTools.FieldRef<LogicElementSensor, bool[]> Samples =
			AccessTools.FieldRefAccess<LogicElementSensor, bool[]>("samples");
		private static readonly AccessTools.FieldRef<LogicElementSensor, int> SampleIdx =
			AccessTools.FieldRefAccess<LogicElementSensor, int>("sampleIdx");

		public static bool Prefix(LogicElementSensor __instance)
		{
			Filterable filterable = __instance.GetComponent<Filterable>();
			if (filterable == null || !AnyElement.Is(filterable.SelectedTag))
				return true;
			int cell = Grid.PosToCell(__instance);
			bool[] samples = Samples(__instance);
			ref int sampleIdx = ref SampleIdx(__instance);
			if (sampleIdx < WindowSize && sampleIdx < samples.Length)
			{
				samples[sampleIdx] = Grid.Element[cell].IsState(__instance.desiredState);
				sampleIdx++;
				return false;
			}
			sampleIdx = 0;
			bool allPresent = true;
			foreach (bool sample in samples)
				allPresent = sample && allPresent;
			if (__instance.IsSwitchedOn != allPresent)
				Switches.Toggle(__instance);
			return false;
		}
	}

	/// <summary>
	/// The game shows "no element selected" on an in-world sensor whose tag is not an element;
	/// Any is a choice, not a lack of one.
	/// </summary>
	[HarmonyPatch(typeof(LogicElementSensor), "OnElementSelected")]
	public static class LogicElementSensor_OnElementSelected_Patch
	{
		public static void Postfix(LogicElementSensor __instance, Tag element_tag)
		{
			if (AnyElement.Is(element_tag))
				__instance.GetComponent<KSelectable>().ToggleStatusItem(Db.Get().BuildingStatusItems.NoFilterElementSelected, false);
		}
	}
}
