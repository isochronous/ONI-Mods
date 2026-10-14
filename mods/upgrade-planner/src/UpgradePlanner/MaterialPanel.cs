using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradePlanner
{
	/// <summary>
	/// The game's own material picker (the panel under a building in the build menu), placed
	/// inside the planner's window: one row of toggles per recipe ingredient, with the player's
	/// stock shown on each. The priority strip the panel carries is hidden, since the tool menu
	/// already shows one, and the rows are widened to fit the window instead of the side bar.
	/// </summary>
	internal static class MaterialPanel
	{
		internal const int Columns = 8;
		internal const int RowHeight = 74;
		/// <summary>The rows a picker shows before its own scrollbar, when the window does not scroll it.</summary>
		internal const int DefaultRows = 4;
		/// <summary>Every row, for a pane of the window's to scroll; see <see cref="Attach"/>.</summary>
		internal const int AllRows = 0;

		/// <summary>
		/// Puts a picker for <paramref name="recipe"/> under <paramref name="parent"/>, selecting
		/// <paramref name="preselect"/> where those materials are offered, and calls
		/// <paramref name="onChanged"/> with the current choices (Tag.Invalid where none yet) on every
		/// pick. With <paramref name="forBuilding"/> the game's "research required" check applies, as
		/// in the build menu. <paramref name="strip"/> is for a picker that stands for no one building
		/// (the Change material tab's): its header (which would list every material class) and the
		/// amount each toggle says a building needs are hidden. <paramref name="maxRows"/> is how many
		/// rows show before the picker's own scrollbar; with <see cref="AllRows"/> every row shows, for
		/// a scroll pane of the window's to scroll (the picker's own scroll view is on only when its
		/// rows overflow; see Widen). Null if the game's prefab is not around (no colony loaded).
		/// </summary>
		public static MaterialSelectionPanel Attach(GameObject parent, Recipe recipe, IList<Tag> preselect, bool forBuilding, System.Action<IList<Tag>> onChanged, bool strip = false, int maxRows = DefaultRows)
		{
			ProductInfoScreen productInfo = PlanScreen.Instance?.ProductInfoScreen;
			if (parent == null || recipe == null || productInfo == null || productInfo.materialSelectionPanelPrefab == null)
				return null;
			// Before the picker exists, so other mods' layout code sees whose it is from its first setup.
			parent.AddOrGet<PlannerPickerHost>();
			MaterialSelectionPanel panel = Util.KInstantiateUI<MaterialSelectionPanel>(productInfo.materialSelectionPanelPrefab.gameObject, parent, force_active: true);
			panel.ClearSelectActions();
			if (forBuilding)
				panel.ConfigureScreen(recipe, PlanScreen.Instance.IsDefBuildable, PlanScreen.Instance.GetTooltipForBuildable);
			else
				panel.ConfigureScreen(recipe, _ => true, _ => "");
			panel.ToggleShowDescriptorPanels(show: false);
			if (panel.PriorityScreen != null && panel.PriorityScreen.transform.parent != null)
				panel.PriorityScreen.transform.parent.gameObject.SetActive(false);

			MaterialSelector[] selectors = panel.GetComponentsInChildren<MaterialSelector>(includeInactive: true);
			foreach (MaterialSelector selector in selectors)
			{
				if (!selector.gameObject.activeSelf)
					continue;
				SizeKeeper keeper = selector.gameObject.AddOrGet<SizeKeeper>();
				keeper.Selector = selector;
				keeper.MaxRows = maxRows;
				keeper.Apply();
				if (strip)
					Strip(selector);
				int index = selector.selectorIndex;
				if (preselect != null && index < preselect.Count && preselect[index].IsValid && selector.ElementToggles.ContainsKey(preselect[index]))
					selector.OnSelectMaterial(preselect[index], recipe);
			}
			panel.AddSelectAction(() => onChanged(Choices(selectors)));
			return panel;
		}

		/// <summary>The current pick of every ingredient's selector, Tag.Invalid where none is made.</summary>
		public static IList<Tag> Choices(MaterialSelector[] selectors)
		{
			var choices = new List<Tag>();
			foreach (MaterialSelector selector in selectors)
			{
				if (!selector.gameObject.activeSelf)
					continue;
				while (choices.Count <= selector.selectorIndex)
					choices.Add(Tag.Invalid);
				choices[selector.selectorIndex] = selector.CurrentSelectedElement.IsValid ? selector.CurrentSelectedElement : Tag.Invalid;
			}
			return choices;
		}

		/// <summary>
		/// Hides the header and the per-toggle amount. Both are still set by the game every 200 ms
		/// (MaterialSelector.RefreshToggleContents, which finds the labels by position among the
		/// active ones), so the objects stay active: the header is shrunk out of the layout and the
		/// amount's text component is switched off.
		/// </summary>
		private static void Strip(MaterialSelector selector)
		{
			if (selector.Headerbar != null)
			{
				selector.Headerbar.AddOrGet<LayoutElement>().ignoreLayout = true;
				selector.Headerbar.transform.localScale = Vector3.zero;
			}
			foreach (KToggle toggle in selector.ElementToggles.Values)
			{
				LocText[] labels = toggle.GetComponentsInChildren<LocText>();
				if (labels.Length > 1)
					labels[0].enabled = false;
			}
		}

		/// <summary>
		/// The planner's size for one picker: eight toggles a row (the side bar shows five), and up to
		/// <see cref="MaxRows"/> rows with the picker's own scrollbar past that, or every row and no
		/// scrollbar of its own when the window scrolls it (MaxRows 0). Applied when the picker is
		/// made and again each time the game sets its own size (see the UpdateScrollBar patch).
		/// </summary>
		internal sealed class SizeKeeper : MonoBehaviour
		{
			public MaterialSelector Selector;
			public int MaxRows = DefaultRows;

			public void Apply()
			{
				if (Selector != null)
					Widen(Selector, MaxRows);
			}
		}

		internal static void Widen(MaterialSelector selector, int maxRows)
		{
			GridLayoutGroup grid = selector.LayoutContainer != null ? selector.LayoutContainer.GetComponent<GridLayoutGroup>() : null;
			if (grid != null)
			{
				grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
				grid.constraintCount = Columns;
			}
			LayoutElement scroll = selector.ScrollRect != null ? selector.ScrollRect.GetComponent<LayoutElement>() : null;
			if (scroll == null)
				return;
			int needed = RowsNeeded(selector);
			bool all = maxRows <= 0;
			float height = FitHeight(selector, all ? Mathf.Max(needed, 1) : Mathf.Clamp(needed, 1, maxRows));
			scroll.minHeight = height;
			scroll.preferredHeight = height;
			// The game shows the scrollbar past five toggles, the side bar's one row; here it shows
			// only when there are more rows than the picker is tall.
			bool scrolls = !all && needed > maxRows;
			// The picker's scroll view is on only when its rows overflow. Its scrollbar is set to show
			// permanently, which Unity enforces by switching it back on every frame while the view is
			// on; with the game switching it off on each refresh, it flickered. Off, the view leaves the
			// scrollbar alone and takes no wheel events, so they reach the window's own pane.
			KScrollRect rect = selector.ScrollRect;
			if (rect != null && rect.enabled != scrolls)
			{
				if (!scrolls)
					rect.verticalNormalizedPosition = 1f;
				rect.enabled = scrolls;
			}
			if (selector.Scrollbar != null && selector.Scrollbar.activeSelf != scrolls)
				selector.Scrollbar.SetActive(scrolls);
		}

		/// <summary>Rows the shown toggles take; an undiscovered material's toggle is there but hidden.</summary>
		internal static int RowsNeeded(MaterialSelector selector)
		{
			int shown = 0;
			foreach (KToggle toggle in selector.ElementToggles.Values)
				if (toggle != null && toggle.gameObject.activeSelf)
					shown++;
			return (shown + Columns - 1) / Columns;
		}

		/// <summary>
		/// The height <paramref name="rows"/> of toggles take in the grid: its padding, its cells and
		/// the spacing between them. The game's own figure (74 a row) leaves out the padding, which
		/// left the toggles a few pixels taller than their view and a scrollbar always showing.
		/// </summary>
		internal static float FitHeight(MaterialSelector selector, int rows)
		{
			GridLayoutGroup grid = selector.LayoutContainer != null ? selector.LayoutContainer.GetComponent<GridLayoutGroup>() : null;
			if (grid == null)
				return RowHeight * rows;
			return grid.padding.top + grid.padding.bottom + rows * grid.cellSize.y + (rows - 1) * grid.spacing.y;
		}
	}
}
