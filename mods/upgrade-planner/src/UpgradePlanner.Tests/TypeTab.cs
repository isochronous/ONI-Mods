using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>The Replace type tab's layout: the types over a material card of a third, and pickers at the planner's size.</summary>
	public static class TypeTab
	{
		[OniTest("Upgrade Planner: Replace type keeps the types in two thirds and the material in the last third", Timeout = 40), Layer(2)]
		public static IEnumerator Layout(TestContext t)
		{
			UpgradeTool tool = null;
			foreach (InterfaceTool candidate in PlayerController.Instance.tools)
				if (candidate is UpgradeTool found)
					tool = found;
			t.Assert.NotNull(tool, "the Upgrade Planner tool is among the player's tools");
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			UpgradePlan plan = UpgradePlan.Current;
			plan.SetMode(UpgradePlan.PlanMode.ReplaceType);
			plan.Clear();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.NotNull(Find(window.transform, "SourceMaterialPrompt"), "before a type is picked, the material card says to pick one");
			Check(t, window, "with nothing picked");

			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			plan.SetSource(ReplacementRules.FamilyOf(Assets.GetBuildingDef("LiquidConduit")));
			plan.SetSourceAnyMaterial();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.NotNull(Find(window.transform, "SourceAnyContent"), "with Any, the material card says what Any does");
			Check(t, window, "with Any");

			// Specific with the card's own button, as the player does.
			KButton specific = Find(window.transform, "Specific")?.GetComponent<KButton>();
			t.Assert.NotNull(specific, "the Specific button is in the material card's strip");
			specific.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(1.5);
			window = GameObject.Find("UpgradePlannerDialog");
			Check(t, window, "with Specific");
			GameObject pane = Find(window.transform, "SourcePickerScroll");
			t.Assert.NotNull(pane, "the source picker is in a pane of the material card");
			MaterialSelector source = pane.GetComponentInChildren<MaterialSelector>();
			t.Assert.NotNull(source, "the pane holds the picker");
			int rows = Mathf.Max(MaterialPanel.RowsNeeded(source), 1);
			t.Assert.Eq(MaterialPanel.Columns, source.LayoutContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount, "the source picker is " + MaterialPanel.Columns + " toggles wide");
			t.Assert.Eq(MaterialPanel.FitHeight(source, rows), source.ScrollRect.GetComponent<UnityEngine.UI.LayoutElement>().minHeight, "the source picker shows all " + rows + " rows, for its pane to scroll");
			t.Assert.False(source.Scrollbar != null && source.Scrollbar.activeInHierarchy, "the source picker has no scrollbar of its own");

			// A target: the With picker is taller than the side bar's, up to six rows. The source in
			// any material, so the With side shows its choices.
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			plan.SetSourceAnyMaterial();
			plan.SetTarget(ReplacementRules.FamilyOf(Assets.GetBuildingDef("InsulatedLiquidConduit")));
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1.5);
			window = GameObject.Find("UpgradePlannerDialog");
			Check(t, window, "with a target");
			MaterialSelector target = null;
			foreach (MaterialSelector selector in window.GetComponentsInChildren<MaterialSelector>())
				if (Find(window.transform, "TargetSide") != null && selector.transform.IsChildOf(Find(window.transform, "TargetSide").transform))
					target = selector;
			t.Assert.NotNull(target, "the With side has a picker");
			int needed = MaterialPanel.RowsNeeded(target);
			int shown = Mathf.Clamp(needed, 1, 6);
			t.Log("With picker: " + needed + " rows of materials, " + shown + " shown");
			t.Assert.Eq(MaterialPanel.Columns, target.LayoutContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount, "the With picker is " + MaterialPanel.Columns + " toggles wide");
			t.Assert.Eq(MaterialPanel.FitHeight(target, shown), target.ScrollRect.GetComponent<UnityEngine.UI.LayoutElement>().minHeight, "the With picker shows " + shown + " rows");
			bool bar = target.Scrollbar != null && target.Scrollbar.activeInHierarchy;
			t.Log("With picker scrollbar " + (target.Scrollbar != null ? target.Scrollbar.name + " active " + target.Scrollbar.activeSelf + ", in hierarchy " + bar : "none") + "; scroll rect bar " + (target.ScrollRect.verticalScrollbar != null ? target.ScrollRect.verticalScrollbar.name + " " + target.ScrollRect.verticalScrollbar.gameObject.activeInHierarchy : "none"));
			t.Assert.Eq(needed > 6, bar, "the With picker has a scrollbar only past six rows");
			// Every frame for a second: the scrollbar stays as it is.
			int frames = 0, flips = 0;
			UnityEngine.Canvas.WillRenderCanvases sample = () =>
			{
				frames++;
				if (target != null && target.Scrollbar != null && target.Scrollbar.activeInHierarchy != bar)
					flips++;
			};
			UnityEngine.Canvas.willRenderCanvases += sample;
			t.Cleanup(() => UnityEngine.Canvas.willRenderCanvases -= sample);
			yield return t.WaitRealSeconds(1);
			UnityEngine.Canvas.willRenderCanvases -= sample;
			t.Assert.True(frames > 15, "frames were sampled (" + frames + ")");
			t.Assert.Eq(0, flips, "the With picker's scrollbar did not change in " + frames + " frames");
			yield return t.Screenshot("type-tab");
		}

		/// <summary>
		/// The types, in one card under the Replace strip, take two thirds of the Replace side and the
		/// material card one (each with its strip); the two sides are as wide as each other.
		/// </summary>
		private static void Check(TestContext t, GameObject window, string when)
		{
			RectTransform types = Find(window.transform, "SourceListCard").rectTransform();
			t.Assert.True(Find(window.transform, "SourceScroll").transform.IsChildOf(types), "the types scroll inside one card " + when);
			t.Assert.Eq((Color)new Color32(201, 201, 201, 255), types.GetComponent<UnityEngine.UI.Image>().color, "the types' card is the cards' grey " + when);
			t.Assert.True(types.transform.IsChildOf(Find(window.transform, "SourceGroup").transform), "the types' card is under the Replace strip " + when);
			RectTransform group = Find(window.transform, "SourceGroup").rectTransform();
			RectTransform card = Find(window.transform, "SourceMaterialCard").rectTransform();
			float ratio = group.rect.height / card.rect.height;
			t.Log("types with their strip " + group.rect.height + " tall, material card " + card.rect.height + ", " + when);
			t.Assert.True(ratio > 1.9f && ratio < 2.1f, "the types are twice as tall as the material card " + when + " (" + ratio + ")");
			// The material card ends where the side does, level with the With column's foot.
			var sideCorners = new Vector3[4];
			var cardCorners = new Vector3[4];
			Find(window.transform, "SourceSide").rectTransform().GetWorldCorners(sideCorners);
			card.GetWorldCorners(cardCorners);
			t.Assert.True(Mathf.Abs(cardCorners[0].y - sideCorners[0].y) < 1f, "the material card ends at the foot of the Replace side " + when + " (" + cardCorners[0].y + ", " + sideCorners[0].y + ")");
			float source = Find(window.transform, "SourceSide").rectTransform().rect.width;
			float target = Find(window.transform, "TargetSide").rectTransform().rect.width;
			t.Assert.True(Mathf.Abs(source - target) < 2f, "the Replace and With sides are as wide as each other " + when + " (" + source + ", " + target + ")");
		}

		private static GameObject Find(Transform root, string name)
		{
			if (root.name == name)
				return root.gameObject;
			for (int i = 0; i < root.childCount; i++)
			{
				GameObject found = Find(root.GetChild(i), name);
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
