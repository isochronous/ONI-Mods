using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The Replace type tab. On the left, Replace: every type something can be built over, by
	/// family, in a card taking two thirds of the side, over a Material card (any material, or
	/// specific ones in the game's picker). On the right, With: once the Replace side is complete,
	/// the types the game lets you build over the chosen one and the chosen type's materials; until
	/// then, the steps of a plan with the next one marked.
	/// </summary>
	internal sealed class TypeTabView : ITabView
	{
		internal const string SourceScroll = "SourceScroll";
		internal const string TargetScroll = "TargetScroll";
		internal const string SourcePickerScroll = "SourcePickerScroll";
		/// <summary>The rows the With picker shows before its own scrollbar.</summary>
		private const int TargetPickerRows = 6;

		private readonly ReplaceTypePlan plan;
		private readonly UpgradeDialog dialog;
		private GameObject sourceHost;
		private GameObject targetHost;

		public TypeTabView(ReplaceTypePlan plan, UpgradeDialog dialog)
		{
			this.plan = plan;
			this.dialog = dialog;
		}

		// ---- Replace ----

		public IUIComponent LeftSide()
		{
			PPanel column = Widgets.Column("Source", scrolls: true);
			foreach (Category category in ReplacementRules.SourceCategories())
				column.AddChild(CategoryCard(category, plan.Source, family => { plan.SetSource(family); dialog.Rebuild(); }));
			return Widgets.Side("Source", ModStrings.UPGRADEPLANNER.ReplaceHeading, column, SourceScroll,
				trailing: new[] { Widgets.StripButton("Clear", ModStrings.UPGRADEPLANNER.Clear, ModStrings.UPGRADEPLANNER.ClearTooltip, dialog.ClearPlan) },
				below: SourceMaterialCard());
		}

		/// <summary>
		/// The Replace side's Material card: once a type is picked, Any or Specific in its strip, and
		/// for Specific the picker in a pane of its own; otherwise a line saying what to do or what Any does.
		/// </summary>
		private PPanel SourceMaterialCard()
		{
			PPanel strip = Widgets.Strip("SourceMaterialCardHeader")
				.AddChild(Widgets.Heading("SourceMaterialCardHeading", ModStrings.UPGRADEPLANNER.MaterialHeading, 15));
			IUIComponent content;
			if (plan.Source == null || plan.SourceMaterials.Length == 0)
				content = Widgets.Paragraph("SourceMaterialPrompt", ModStrings.UPGRADEPLANNER.SourceMaterialPrompt);
			else
			{
				strip.AddChild(Widgets.StripToggle("Any", ModStrings.UPGRADEPLANNER.AnyMaterial, ModStrings.UPGRADEPLANNER.AnyMaterialTooltip,
					plan.SourceMode == MaterialMode.Any, () => { plan.SetSourceAny(); dialog.Rebuild(); }));
				strip.AddChild(Widgets.StripToggle("Specific", ModStrings.UPGRADEPLANNER.SpecificMaterial, ModStrings.UPGRADEPLANNER.SpecificMaterialTooltip,
					plan.SourceMode == MaterialMode.Specific, () => { plan.SetSourceSpecific(); dialog.Rebuild(); }));
				switch (plan.SourceMode)
				{
				case MaterialMode.Specific:
					content = Widgets.Scrollable(SourcePickerScroll, Widgets.PickerHost("SourceMaterials", go => sourceHost = go)).AddOnRealize(Widgets.TakeAShare);
					break;
				case MaterialMode.Any:
					content = Widgets.Paragraph("SourceAnyContent", ModStrings.UPGRADEPLANNER.SourceAnyContent);
					break;
				default:
					content = Widgets.Paragraph("SourceModePrompt", ModStrings.UPGRADEPLANNER.StepPickSourceMaterial);
					break;
				}
			}
			return Widgets.Card("SourceMaterialCard", strip, Widgets.Rows("SourceMaterialCardRows", fill: true).AddChild(content), fill: true);
		}

		// ---- With ----

		public IUIComponent RightSide()
		{
			// Its cards fit the side (the picker scrolls inside its own), so no room is kept for a
			// scrollbar: they sit as far from the card's frame on every side.
			PPanel column = Widgets.Column("Target", scrolls: false);
			if (!plan.SourceComplete)
				// Nothing to choose from until the Replace side is complete: the steps, the next one marked.
				column.AddChild(HowToCard(plan.Source == null ? 0 : 1));
			else
				AddTargets(column);
			return Widgets.Side("Target", ModStrings.UPGRADEPLANNER.WithHeading, column, TargetScroll);
		}

		private void AddTargets(PPanel column)
		{
			List<Category> categories = ReplacementRules.TargetCategories(plan.Source);
			if (categories.Count == 0)
				column.AddChild(Widgets.Line("Note", ModStrings.UPGRADEPLANNER.NoTargets, 0, new RectOffset(0, 0, 6, 6)));
			else if (plan.Target == null)
				column.AddChild(Widgets.Line("Prompt", ModStrings.UPGRADEPLANNER.ChooseTarget, 17, new RectOffset(4, 4, 8, 10)));
			foreach (Category category in categories)
				column.AddChild(CategoryCard(category, plan.Target, family => { plan.SetTarget(family); dialog.Rebuild(); }));
			if (plan.Target == null)
				return;
			column.AddChild(Widgets.Card("TargetMaterialSection", ModStrings.UPGRADEPLANNER.MaterialHeading, out PPanel rows));
			if (plan.TargetMaterials.Length == 0)
				rows.AddChild(Widgets.Line("Note", ModStrings.UPGRADEPLANNER.NoMaterials, 0, new RectOffset(0, 0, 6, 6)));
			else
				rows.AddChild(Widgets.PickerHost("TargetMaterials", go => targetHost = go));
		}

		/// <summary>The steps of a plan, one line each, the one at <paramref name="current"/> in bold with a marker.</summary>
		private static PPanel HowToCard(int current)
		{
			PPanel card = Widgets.Card("HowTo", ModStrings.UPGRADEPLANNER.HowToHeading, out PPanel rows);
			string[] steps =
			{
				ModStrings.UPGRADEPLANNER.StepPickSource, ModStrings.UPGRADEPLANNER.StepPickSourceMaterial,
				ModStrings.UPGRADEPLANNER.StepPickTarget, ModStrings.UPGRADEPLANNER.StepApply,
			};
			for (int i = 0; i < steps.Length; i++)
			{
				bool now = i == current;
				rows.AddChild(new PLabel("Step" + i)
				{
					Text = (now ? "▶ " : "   ") + (i + 1) + ".  " + steps[i],
					TextStyle = PUITuning.Fonts.TextDarkStyle.DeriveStyle(15, style: now ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal),
					TextAlignment = TextAnchor.MiddleLeft,
					FlexSize = new Vector2(1f, 0f),
					Margin = new RectOffset(4, 4, 3, 3),
				});
			}
			return card;
		}

		/// <summary>A family's card of types as the build menu shows them, <paramref name="picked"/> in pink.</summary>
		private static PPanel CategoryCard(Category category, Family picked, System.Action<Family> onPick)
		{
			PPanel card = Widgets.Card("Category" + category.Key, category.Name, out PPanel rows);
			var buttons = new List<IUIComponent>();
			foreach (Family family in category.Families)
				buttons.Add(Widgets.BuildingButton(family, family == picked, () => onPick(family)));
			Widgets.AddButtonRows(rows, buttons);
			return card;
		}

		// ---- pickers ----

		public void AttachPickers()
		{
			if (sourceHost != null && plan.Source != null)
				MaterialPicker.Attach(sourceHost, plan.Source.Representative.CraftRecipe, plan.SourceMaterials,
					new MaterialPicker.Options { HideHeader = true, MaxRows = MaterialPicker.AllRows }, choices =>
					{
						// The With side has choices only once the Replace side is complete, so a pick
						// that completes it (or undoes that) lays the window out again.
						bool wasComplete = plan.SourceComplete;
						plan.SetSourceMaterials(choices);
						if (plan.SourceComplete != wasComplete)
							dialog.Rebuild();
						else
							dialog.RefreshStatus();
					});
			if (targetHost != null && plan.Target != null)
			{
				MaterialSelectionPanel panel = MaterialPicker.Attach(targetHost, plan.Target.Representative.CraftRecipe, plan.TargetMaterials,
					new MaterialPicker.Options { ForBuilding = true, HideHeader = true, MaxRows = TargetPickerRows }, choices =>
					{
						plan.SetTargetMaterials(choices);
						dialog.RefreshStatus();
					});
				// Nothing chosen yet: start from what the build menu would pick, the material in stock.
				if (panel != null && !plan.TargetComplete)
					panel.AutoSelectAvailableMaterial();
			}
		}
	}
}
