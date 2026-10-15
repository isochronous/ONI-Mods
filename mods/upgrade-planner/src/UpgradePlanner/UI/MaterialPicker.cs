using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradePlanner
{
	/// <summary>
	/// The game's own material picker (the panel under a building in the build menu) placed inside
	/// the planner's window: one row of toggles per recipe ingredient, with the player's stock shown
	/// on each. Its priority strip is hidden, since the tool menu shows one, and its rows are widened
	/// to fit the window instead of the build menu's side bar.
	/// </summary>
	internal static class MaterialPicker
	{
		/// <summary>Toggles a row (the side bar shows five).</summary>
		internal const int Columns = 8;
		/// <summary>Every row shows, for a pane of the window's to scroll.</summary>
		internal const int AllRows = 0;

		/// <summary>How a picker is shown.</summary>
		internal struct Options
		{
			/// <summary>The build menu's "research required" check applies, as for a building to be built.</summary>
			public bool ForBuilding;
			/// <summary>Hides the header bar over the toggles.</summary>
			public bool HideHeader;
			/// <summary>Hides the amount each toggle says a building needs, for a picker that stands for no one building.</summary>
			public bool HideAmounts;
			/// <summary>Rows shown before the picker's own scrollbar; <see cref="AllRows"/> for every row.</summary>
			public int MaxRows;
		}

		/// <summary>
		/// Puts a picker for <paramref name="recipe"/> under <paramref name="parent"/>, selecting
		/// <paramref name="preselect"/> where those materials are offered, and calls
		/// <paramref name="onChanged"/> with the choices (Tag.Invalid where none yet) on every pick.
		/// Null when the game's prefab is not around (no colony loaded).
		/// </summary>
		public static MaterialSelectionPanel Attach(GameObject parent, Recipe recipe, IList<Tag> preselect, Options options, System.Action<IList<Tag>> onChanged)
		{
			ProductInfoScreen productInfo = PlanScreen.Instance?.ProductInfoScreen;
			if (parent == null || recipe == null || productInfo == null || productInfo.materialSelectionPanelPrefab == null)
				return null;
			// Before the picker exists, so other mods' layout code sees whose it is from its first setup.
			parent.AddOrGet<PlannerPickerHost>();
			MaterialSelectionPanel panel = Util.KInstantiateUI<MaterialSelectionPanel>(productInfo.materialSelectionPanelPrefab.gameObject, parent, force_active: true);
			panel.ClearSelectActions();
			if (options.ForBuilding)
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
				keeper.MaxRows = options.MaxRows;
				keeper.Apply();
				if (options.HideHeader)
					HideHeader(selector);
				if (options.HideAmounts)
					HideAmounts(selector);
				int index = selector.selectorIndex;
				if (preselect != null && index < preselect.Count && preselect[index].IsValid && selector.ElementToggles.ContainsKey(preselect[index]))
					selector.OnSelectMaterial(preselect[index], recipe);
			}
			panel.AddSelectAction(() => onChanged(Choices(selectors)));
			return panel;
		}

		/// <summary>The current pick of every ingredient's selector, Tag.Invalid where none is made.</summary>
		private static IList<Tag> Choices(MaterialSelector[] selectors)
		{
			var choices = new List<Tag>();
			foreach (MaterialSelector selector in selectors)
			{
				if (!selector.gameObject.activeSelf)
					continue;
				while (choices.Count <= selector.selectorIndex)
					choices.Add(Tag.Invalid);
				Tag picked = selector.CurrentSelectedElement;
				choices[selector.selectorIndex] = picked.IsValid ? picked : Tag.Invalid;
			}
			return choices;
		}

		/// <summary>
		/// Hides the header. The game still sets it every 200 ms (MaterialSelector.RefreshToggleContents),
		/// so it stays active, shrunk out of the layout.
		/// </summary>
		private static void HideHeader(MaterialSelector selector)
		{
			if (selector.Headerbar == null)
				return;
			selector.Headerbar.AddOrGet<LayoutElement>().ignoreLayout = true;
			selector.Headerbar.transform.localScale = Vector3.zero;
		}

		/// <summary>
		/// Hides the per-toggle amount. The game still sets it every 200 ms (RefreshToggleContents,
		/// which finds the labels by position among the active ones), so only the text component is
		/// switched off.
		/// </summary>
		private static void HideAmounts(MaterialSelector selector)
		{
			foreach (KToggle toggle in selector.ElementToggles.Values)
			{
				LocText[] labels = toggle.GetComponentsInChildren<LocText>();
				if (labels.Length > 1)
					labels[0].enabled = false;
			}
		}

		/// <summary>
		/// The planner's size for one picker, applied when it is made and each time the game sets the
		/// build menu's (see the UpdateScrollBar patch): <see cref="Columns"/> toggles a row, and up to
		/// <see cref="MaxRows"/> rows with the picker's own scrollbar past that, or every row and no
		/// scrollbar of its own when a pane of the window scrolls it.
		/// </summary>
		internal sealed class SizeKeeper : MonoBehaviour
		{
			public MaterialSelector Selector;
			public int MaxRows = AllRows;

			public void Apply()
			{
				if (Selector == null)
					return;
				GridLayoutGroup grid = Grid(Selector);
				if (grid != null)
				{
					grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
					grid.constraintCount = Columns;
				}
				LayoutElement scroll = Selector.ScrollRect != null ? Selector.ScrollRect.GetComponent<LayoutElement>() : null;
				if (scroll == null)
					return;
				int needed = RowsNeeded(Selector);
				bool all = MaxRows == AllRows;
				scroll.minHeight = scroll.preferredHeight = FitHeight(Selector, all ? Mathf.Max(needed, 1) : Mathf.Clamp(needed, 1, MaxRows));
				// The game shows the scrollbar past five toggles, the side bar's one row; here only when
				// there are more rows than the picker is tall. The picker's scroll view is on only then:
				// its scrollbar is set to show permanently, which Unity enforces every frame while the
				// view is on, and with the game hiding it on each refresh, it flickered. Off, the view
				// leaves the scrollbar alone and takes no wheel events, so they reach the window's pane.
				bool scrolls = !all && needed > MaxRows;
				KScrollRect rect = Selector.ScrollRect;
				if (rect != null && rect.enabled != scrolls)
				{
					if (!scrolls)
						rect.verticalNormalizedPosition = 1f;
					rect.enabled = scrolls;
				}
				if (Selector.Scrollbar != null && Selector.Scrollbar.activeSelf != scrolls)
					Selector.Scrollbar.SetActive(scrolls);
			}
		}

		private static GridLayoutGroup Grid(MaterialSelector selector) =>
			selector.LayoutContainer != null ? selector.LayoutContainer.GetComponent<GridLayoutGroup>() : null;

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
		/// The height <paramref name="rows"/> of toggles take in the grid: its padding, its cells and the
		/// spacing between them. The game's own figure (74 a row) leaves out the padding, which left the
		/// toggles a few pixels taller than their view and a scrollbar always showing.
		/// </summary>
		internal static float FitHeight(MaterialSelector selector, int rows)
		{
			GridLayoutGroup grid = Grid(selector);
			if (grid == null)
				return 74f * rows;
			return grid.padding.top + grid.padding.bottom + rows * grid.cellSize.y + (rows - 1) * grid.spacing.y;
		}
	}

	/// <summary>
	/// Marks a container whose material pickers are the planner's own. Placed before the game's
	/// picker is made inside it, so a mod that lays pickers out from the picker's own setup (its
	/// OnPrefabInit) already finds it.
	/// </summary>
	internal sealed class PlannerPickerHost : MonoBehaviour
	{
		public static bool Holds(Component component) =>
			component != null && component.GetComponentInParent<PlannerPickerHost>() != null;
	}
}
