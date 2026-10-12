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
		private const int Columns = 8;
		private const int RowHeight = 74;
		private const int MaxRows = 4;

		/// <summary>
		/// Puts a picker for <paramref name="recipe"/> under <paramref name="parent"/>, selecting
		/// <paramref name="preselect"/> where those materials are offered, and calls
		/// <paramref name="onChanged"/> with the current choices (Tag.Invalid where none yet) on every
		/// pick. With <paramref name="forBuilding"/> the game's "research required" check applies, as
		/// in the build menu. Null if the game's prefab is not around (no colony loaded).
		/// </summary>
		public static MaterialSelectionPanel Attach(GameObject parent, Recipe recipe, IList<Tag> preselect, bool forBuilding, System.Action<IList<Tag>> onChanged)
		{
			ProductInfoScreen productInfo = PlanScreen.Instance?.ProductInfoScreen;
			if (parent == null || recipe == null || productInfo == null || productInfo.materialSelectionPanelPrefab == null)
				return null;
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
				Widen(selector);
				int index = selector.selectorIndex;
				if (preselect != null && index < preselect.Count && preselect[index].IsValid && selector.ElementToggles.ContainsKey(preselect[index]))
					selector.OnSelectMaterial(preselect[index], recipe);
			}
			panel.AddSelectAction(() =>
			{
				// A pick refreshes the toggles, and the refresh sets the game's own height again.
				foreach (MaterialSelector selector in selectors)
					if (selector.gameObject.activeSelf)
						Widen(selector);
				onChanged(Choices(selectors));
			});
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

		/// <summary>The side bar shows five toggles a row and two rows; the window has room for more.</summary>
		private static void Widen(MaterialSelector selector)
		{
			GridLayoutGroup grid = selector.LayoutContainer != null ? selector.LayoutContainer.GetComponent<GridLayoutGroup>() : null;
			if (grid != null)
			{
				grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
				grid.constraintCount = Columns;
			}
			LayoutElement scroll = selector.ScrollRect != null ? selector.ScrollRect.GetComponent<LayoutElement>() : null;
			if (scroll != null)
			{
				int rows = Mathf.Clamp((selector.ElementToggles.Count + Columns - 1) / Columns, 1, MaxRows);
				// The side bar's two rows are the game's minHeight, set again on every refresh (so
				// this runs after each one, see Attach); the window has room for more.
				scroll.minHeight = RowHeight * rows;
				scroll.preferredHeight = RowHeight * rows;
			}
		}
	}
}
