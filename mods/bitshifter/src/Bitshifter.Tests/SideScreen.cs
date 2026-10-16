using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bitshifter.Tests
{
	/// <summary>The bit selector side screen: its 31 rows, the scroll view the mod puts them in, and picking a row.</summary>
	public static class SideScreen
	{
		private static IEnumerator Show(TestContext t, GameObject building, System.Action<LogicBitSelectorSideScreen> done)
		{
			SelectTool.Instance.Select(building.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
			LogicBitSelectorSideScreen screen = null;
			foreach (DetailsScreen.SideScreenRef sr in Traverse.Create(DetailsScreen.Instance).Field("sideScreens").GetValue<List<DetailsScreen.SideScreenRef>>())
				if (sr.screenInstance is LogicBitSelectorSideScreen s && s.gameObject.activeInHierarchy)
					screen = s;
			t.Assert.NotNull(screen, "the bit selector side screen is showing");
			done(screen);
		}

		private static Dictionary<int, MultiToggle> Rows(LogicBitSelectorSideScreen screen)
		{
			var rows = new Dictionary<int, MultiToggle>();
			foreach (KeyValuePair<int, MultiToggle> pair in Traverse.Create(screen).Field("toggles_by_int").GetValue<Dictionary<int, MultiToggle>>())
				rows[pair.Key] = pair.Value;
			return rows;
		}

		private static void Click(GameObject go)
		{
			ExecuteEvents.Execute(go, new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
		}

		[OniTest("Bitshifter: the writer's side screen lists 31 rows in one scroll view with a scrollbar", Timeout = 30), Layer(2)]
		public static IEnumerator WriterRowsAndScroll(TestContext t)
		{
			CellRect r = t.Rect(2, 1);
			GameObject writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0));
			yield return t.WaitGameSeconds(1);
			LogicBitSelectorSideScreen screen = null;
			yield return Show(t, writer, s => screen = s);
			t.Assert.Eq(31, Rows(screen).Count, "one row per bit");
			t.Assert.Eq(1, screen.GetComponentsInChildren<KScrollRect>(true).Length, "one scroll view");
			KScrollRect scroll = screen.GetComponentInChildren<KScrollRect>(true);
			t.Assert.NotNull(scroll.verticalScrollbar, "a scrollbar");
			t.Assert.True(scroll.verticalScrollbar.gameObject.activeInHierarchy, "the scrollbar shows");
			t.Assert.True(scroll.content.rect.height > scroll.viewport.rect.height + 1f, "the rows are taller than the view (" + scroll.content.rect.height + " vs " + scroll.viewport.rect.height + ")");
			RectTransform row = screen.rowPrefab.GetComponent<RectTransform>();
			t.Assert.True(scroll.viewport.rect.height > row.rect.height * 4f, "the view is taller than four rows");
			t.Assert.True(scroll.viewport.rect.height < row.rect.height * 8f, "the view is a good deal shorter than the list");

			scroll.verticalNormalizedPosition = 0f;
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(scroll.verticalNormalizedPosition <= 0.01f, "scrolls to the end");
			yield return t.Screenshot("scrolled");
		}

		[OniTest("Bitshifter: the screen is wrapped once however often it is shown, and stays under the details header", Timeout = 40), Layer(2)]
		public static IEnumerator WrappedOnce(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			GameObject writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0));
			GameObject reader = t.World.Build("LogicRibbonReader", r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			LogicBitSelectorSideScreen screen = null;
			for (int i = 0; i < 3; i++)
			{
				yield return Show(t, i % 2 == 0 ? writer : reader, s => screen = s);
				t.Assert.Eq(1, screen.GetComponentsInChildren<KScrollRect>(true).Length, "one scroll view after showing it " + (i + 1) + " time(s)");
				t.Assert.Eq(31, Rows(screen).Count, "31 rows after showing it " + (i + 1) + " time(s)");
				SelectTool.Instance.Select(null);
				yield return t.WaitGameSeconds(1);
			}
			GameObject header = Traverse.Create(DetailsScreen.Instance).Field("sidescreenTabHeader").GetValue<GameObject>();
			float width = header.GetComponent<RectTransform>().rect.width;
			yield return Show(t, reader, s => screen = s);
			foreach (LayoutElement element in screen.GetComponentsInChildren<LayoutElement>(true))
				t.Assert.True(element.minWidth <= width + 0.01f, element.name + " asks for " + element.minWidth + " against a header of " + width);
		}

		[OniTest("Bitshifter: choosing the last row selects bit 31 on the reader and on the writer", Timeout = 40), Layer(2)]
		public static IEnumerator ChooseLastRow(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			GameObject writer = t.World.Build("LogicRibbonWriter", r.Cell(0, 0));
			GameObject reader = t.World.Build("LogicRibbonReader", r.Cell(3, 0));
			yield return t.WaitGameSeconds(1);
			LogicBitSelectorSideScreen screen = null;
			yield return Show(t, writer, s => screen = s);
			Click(Rows(screen)[30].gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(30, writer.GetComponent<LogicRibbonWriter>().GetBitSelection(), "the writer's bit");
			yield return Show(t, reader, s => screen = s);
			Click(Rows(screen)[17].gameObject);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(17, reader.GetComponent<LogicRibbonReader>().GetBitSelection(), "the reader's bit");
			t.Assert.Eq(30, writer.GetComponent<LogicRibbonWriter>().GetBitSelection(), "the writer keeps its own");
		}
	}
}
