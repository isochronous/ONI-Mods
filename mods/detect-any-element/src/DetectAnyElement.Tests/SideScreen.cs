using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace DetectAnyElement.Tests
{
	/// <summary>
	/// The Any row in the filter side screen, with the real screen open on a selected sensor. The
	/// screen is shared by the in-world and pipe sensors (isLogicFilter) and by the filters, which
	/// keep only None.
	/// </summary>
	public static class SideScreen
	{
		private static FilterSideScreen Screen(bool logic)
		{
			return Resources.FindObjectsOfTypeAll<FilterSideScreen>().FirstOrDefault(s => s.isLogicFilter == logic && s.gameObject.activeInHierarchy);
		}

		private static SingleItemSelectionRow Row(FilterSideScreen screen, Tag tag)
		{
			return screen.GetComponentsInChildren<SingleItemSelectionRow>(true).FirstOrDefault(r => r.tag == tag);
		}

		private static IEnumerator Select(TestContext t, GameObject go)
		{
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			SelectTool.Instance.Select(go.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
		}

		[OniTest("DAE: the sensors' side screen lists Any right under None, and choosing rows moves the choice", Timeout = 40), Layer(2)]
		public static IEnumerator AnyRow(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			GameObject sensor = t.World.Build("LogicElementSensorGas", r.Cell(1, 0));
			yield return t.WaitGameSeconds(1);
			yield return Select(t, sensor);
			FilterSideScreen screen = Screen(true);
			t.Assert.NotNull(screen, "the logic filter side screen is open on the sensor");

			SingleItemSelectionRow any = Row(screen, AnyElement.Tag);
			SingleItemSelectionRow none = Row(screen, GameTags.Void);
			t.Assert.NotNull(any, "an Any row exists");
			t.Assert.NotNull(none, "the None row exists");
			t.Assert.True(any.gameObject.activeSelf, "the Any row is shown");
			t.Assert.Eq(0, none.transform.GetSiblingIndex(), "None is first");
			t.Assert.Eq(1, any.transform.GetSiblingIndex(), "Any is directly under it");
			t.Assert.Eq((string)ModStrings.MISC.TAGS.DETECTANYELEMENT_ANY, any.GetComponentInChildren<LocText>(true).text, "the row reads Any");

			Filterable filterable = sensor.GetComponent<Filterable>();
			screen.ItemRowClicked(any);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(AnyElement.Is(filterable.SelectedTag), "clicking Any chooses Any");
			t.Assert.True(any.IsSelected, "the Any row is highlighted");

			SingleItemSelectionRow oxygen = Row(screen, SimHashes.Oxygen.CreateTag());
			t.Assert.NotNull(oxygen, "a gas row exists");
			screen.ItemRowClicked(oxygen);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(SimHashes.Oxygen.CreateTag(), filterable.SelectedTag, "clicking Oxygen chooses Oxygen");
			t.Assert.False(any.IsSelected, "Any is no longer highlighted");

			screen.ItemRowClicked(none);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(GameTags.Void, filterable.SelectedTag, "clicking None clears the choice");
		}

		[OniTest("DAE: reopening the screen on a sensor that is on Any highlights the Any row", Timeout = 40), Layer(2)]
		public static IEnumerator AnyHighlightedOnOpen(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			GameObject sensor = t.World.Build("LogicElementSensorLiquid", r.Cell(1, 0));
			GameObject other = t.World.Build("LogicElementSensorLiquid", r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			Sensors.PickAny(sensor);
			Sensors.Pick(other, SimHashes.Water);
			yield return Select(t, other);
			FilterSideScreen screen = Screen(true);
			t.Assert.NotNull(screen, "the screen is open");
			t.Assert.False(Row(screen, AnyElement.Tag).IsSelected, "Any is not highlighted on a sensor set to water");
			yield return Select(t, sensor);
			t.Assert.True(Row(screen, AnyElement.Tag).IsSelected, "Any is highlighted on the sensor set to Any");
		}

		[OniTest("DAE: the pipe sensors' side screen lists Any too; the pipe filter's does not", Timeout = 40), Layer(2)]
		public static IEnumerator PipeScreens(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			t.World.Build("LiquidConduit", r.Cell(1, 0));
			GameObject sensor = t.World.Build("LiquidConduitElementSensor", r.Cell(1, 0));
			GameObject filter = t.World.Build("GasFilter", r.Cell(4, 0));
			yield return t.WaitGameSeconds(1);

			yield return Select(t, sensor);
			FilterSideScreen logic = Screen(true);
			t.Assert.NotNull(logic, "the sensor screen is open");
			SingleItemSelectionRow any = Row(logic, AnyElement.Tag);
			t.Assert.NotNull(any, "a pipe sensor has the Any row");
			t.Assert.Eq(1, any.transform.GetSiblingIndex(), "directly under None");
			logic.ItemRowClicked(any);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(AnyElement.Is(sensor.GetComponent<Filterable>().SelectedTag), "clicking Any on a pipe sensor chooses it");

			yield return Select(t, filter);
			FilterSideScreen plain = Screen(false);
			t.Assert.NotNull(plain, "the gas filter's screen is open");
			t.Assert.Null(Row(plain, AnyElement.Tag), "the filter's screen has no Any row");
			t.Assert.NotNull(Row(plain, GameTags.Void), "it keeps None");
		}
	}
}
