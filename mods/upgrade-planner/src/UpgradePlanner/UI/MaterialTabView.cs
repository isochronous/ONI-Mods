using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The Change material tab. On the left, Materials: Replace (any material, or one in the game's
	/// picker) over With (the one to build in), half the side each, each picker scrolling in a pane
	/// of its own. On the right, Buildings: every type the tab offers, by family, as the build
	/// menu's buttons; a type the materials rule out is greyed, its tooltip saying why. A family's
	/// strip picks it all, folds it away and counts what is picked.
	/// </summary>
	internal sealed class MaterialTabView : ITabView
	{
		internal const string BuildingsScroll = "BuildingsScroll";
		internal const string SourcePickerScroll = "MaterialSourceScroll";
		internal const string TargetPickerScroll = "MaterialTargetScroll";

		/// <summary>Families folded away, for the session.</summary>
		private static readonly HashSet<string> folded = new HashSet<string>();

		private readonly ChangeMaterialPlan plan;
		private readonly UpgradeDialog dialog;
		private readonly List<Category> categories;
		private GameObject sourceHost;
		private GameObject targetHost;

		public MaterialTabView(ChangeMaterialPlan plan, UpgradeDialog dialog)
		{
			this.plan = plan;
			this.dialog = dialog;
			categories = plan.Offered();
		}

		// ---- Materials ----

		public IUIComponent LeftSide()
		{
			bool specific = plan.SourceMode == MaterialMode.Specific;
			PPanel column = Widgets.Column("Materials", scrolls: false);
			column.AddChild(MaterialCard("MaterialSourceCard", ModStrings.UPGRADEPLANNER.ReplaceHeading,
				specific
					? Picker(SourcePickerScroll, "MaterialSourcePicker", go => sourceHost = go)
					: Widgets.Paragraph("MaterialAnyContent", ModStrings.UPGRADEPLANNER.MaterialAnyContent),
				Widgets.StripToggle("Any", ModStrings.UPGRADEPLANNER.AnyMaterial, ModStrings.UPGRADEPLANNER.MaterialAnyTooltip, !specific,
					() => { plan.SetSourceAny(); dialog.Rebuild(); }),
				Widgets.StripToggle("Specific", ModStrings.UPGRADEPLANNER.SpecificMaterial, ModStrings.UPGRADEPLANNER.MaterialSpecificTooltip, specific,
					() => { plan.SetSourceSpecific(); dialog.Rebuild(); })));
			column.AddChild(MaterialCard("MaterialTargetCard", ModStrings.UPGRADEPLANNER.WithHeading,
				Picker(TargetPickerScroll, "MaterialTargetPicker", go => targetHost = go)));
			// The side does not scroll: its two cards share its height and scroll their pickers inside.
			return Widgets.Side("Materials", ModStrings.UPGRADEPLANNER.MaterialsHeading, column, scrollName: null,
				trailing: new[] { Widgets.StripButton("Clear", ModStrings.UPGRADEPLANNER.Clear, ModStrings.UPGRADEPLANNER.ClearMaterialTooltip, dialog.ClearPlan) });
		}

		/// <summary>A card of the Materials side: its strip with the heading and <paramref name="tools"/>, and the white area holding <paramref name="content"/>, half the side.</summary>
		private static PPanel MaterialCard(string name, string heading, IUIComponent content, params IUIComponent[] tools)
		{
			PPanel strip = Widgets.Strip(name + "Header").AddChild(Widgets.Heading(name + "Heading", heading, 15));
			foreach (IUIComponent tool in tools)
				strip.AddChild(tool);
			return Widgets.Card(name, strip, Widgets.Rows(name + "Rows", fill: true).AddChild(content), fill: true);
		}

		/// <summary>A picker's place, in a pane of its own that scrolls it when it is taller than its card.</summary>
		private static IUIComponent Picker(string pane, string host, System.Action<GameObject> onRealize)
		{
			return Widgets.Scrollable(pane, Widgets.PickerHost(host, onRealize)).AddOnRealize(Widgets.TakeAShare);
		}

		/// <summary>
		/// One recipe offering every material any listed type takes, in one row: the picker's one
		/// selection is the plan's one material. A pick can grey out or free types, so the window is
		/// laid out again.
		/// </summary>
		public void AttachPickers()
		{
			Recipe recipe = MaterialSwap.PickerRecipe();
			var options = new MaterialPicker.Options { HideHeader = true, HideAmounts = true, MaxRows = MaterialPicker.AllRows };
			if (sourceHost != null)
				MaterialPicker.Attach(sourceHost, recipe, new[] { plan.Source }, options, choices =>
				{
					plan.SetSource(choices.Count > 0 ? choices[0] : Tag.Invalid);
					dialog.Rebuild();
				});
			if (targetHost != null)
				MaterialPicker.Attach(targetHost, recipe, new[] { plan.Target }, options, choices =>
				{
					plan.SetTarget(choices.Count > 0 ? choices[0] : Tag.Invalid);
					dialog.Rebuild();
				});
		}

		// ---- Buildings ----

		public IUIComponent RightSide()
		{
			PPanel column = Widgets.Column("Buildings", scrolls: true);
			var listed = new List<Family>();
			foreach (Category category in categories)
			{
				column.AddChild(FamilyCard(category));
				listed.AddRange(category.Families);
			}
			var tools = new List<IUIComponent>();
			// All buildings is offered with Blueprints Expanded, which keeps the settings of what is rebuilt.
			if (BlueprintsCompat.Loaded)
				tools.Add(new PCheckBox("AllBuildings")
				{
					Text = ModStrings.UPGRADEPLANNER.AllBuildings,
					ToolTip = ModStrings.UPGRADEPLANNER.AllBuildingsTooltip,
					TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(14),
					InitialState = plan.AllBuildings ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED,
					CheckColor = PUITuning.Colors.ComponentLightStyle,
					Margin = new RectOffset(0, 8, 0, 0),
					OnChecked = (_, state) =>
					{
						plan.SetAllBuildings(state != PCheckBox.STATE_CHECKED);
						dialog.Rebuild();
					},
				});
			tools.Add(Widgets.StripButton("ExpandAll", ModStrings.UPGRADEPLANNER.ExpandAll, null, () => { folded.Clear(); dialog.Rebuild(); }));
			tools.Add(Widgets.StripButton("CollapseAll", ModStrings.UPGRADEPLANNER.CollapseAll, null, () =>
			{
				foreach (Category category in categories)
					folded.Add(category.Key);
				dialog.Rebuild();
			}));
			return Widgets.Side("Buildings", ModStrings.UPGRADEPLANNER.BuildingsHeading, column, BuildingsScroll,
				leading: new[] { SelectBox("SelectAll", Usable(listed)) }, trailing: tools);
		}

		/// <summary>
		/// One family's card: its strip holds a box that picks it all, the chevron that folds it and
		/// its name (a click on the name folds it too), and how many of it are picked; the white area
		/// holds the types as the build menu shows them.
		/// </summary>
		private PPanel FamilyCard(Category category)
		{
			bool open = !folded.Contains(category.Key);
			System.Action fold = () =>
			{
				if (!folded.Remove(category.Key))
					folded.Add(category.Key);
				dialog.Rebuild();
			};
			List<Family> usable = Usable(category.Families);
			PPanel strip = Widgets.Strip("FamilyHeader" + category.Key)
				.AddChild(SelectBox("FamilySelect" + category.Key, usable))
				.AddChild(new PButton("FamilyFold" + category.Key)
				{
					Sprite = open ? PUITuning.Images.Contract : PUITuning.Images.Expand,
					SpriteSize = new Vector2(14f, 14f),
					Color = Palette.StripButton,
					Margin = new RectOffset(3, 3, 3, 3),
					OnClick = _ => fold(),
				}.AddOnRealize(Widgets.Flatten))
				.AddChild(new PButton("FamilyName" + category.Key)
				{
					Text = category.Name,
					TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
					TextAlignment = TextAnchor.MiddleLeft,
					Color = Palette.StripButton,
					Margin = new RectOffset(0, 0, 2, 2),
					FlexSize = new Vector2(1f, 0f),
					OnClick = _ => fold(),
				}.AddOnRealize(Widgets.Flatten))
				.AddChild(new PLabel("FamilyCount" + category.Key)
				{
					Text = ModStrings.UPGRADEPLANNER.CategoryCount.Replace("{0}", usable.FindAll(plan.IsPicked).Count.ToString())
						.Replace("{1}", category.Families.Count.ToString()),
					TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(13),
					TextAlignment = TextAnchor.MiddleRight,
				});
			PPanel rows = open ? Widgets.Rows("FamilyRows" + category.Key) : null;
			PPanel card = Widgets.Card("Family" + category.Key, strip, rows);
			if (!open)
				return card;
			var buttons = new List<IUIComponent>();
			foreach (Family family in category.Families)
			{
				string reason = plan.Blocked(family);
				if (reason != null)
				{
					buttons.Add(Widgets.BuildingButton(family, false, null, reason, isReason: true));
					continue;
				}
				bool picked = plan.IsPicked(family);
				buttons.Add(Widgets.BuildingButton(family, picked, () =>
				{
					plan.Pick(new[] { family }, !picked);
					dialog.Rebuild();
				}, picked ? ModStrings.UPGRADEPLANNER.LeaveOutTooltip : ModStrings.UPGRADEPLANNER.IncludeTooltip));
			}
			Widgets.AddButtonRows(rows, buttons);
			return card;
		}

		/// <summary>
		/// A box that picks every one of <paramref name="usable"/>: checked when all of them are
		/// picked, partly when some are; a click on a checked box drops them all, otherwise picks them all.
		/// </summary>
		private PCheckBox SelectBox(string name, List<Family> usable)
		{
			int picked = usable.FindAll(plan.IsPicked).Count;
			return new PCheckBox(name)
			{
				ToolTip = ModStrings.UPGRADEPLANNER.SelectAllTooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(14),
				InitialState = usable.Count > 0 && picked == usable.Count ? PCheckBox.STATE_CHECKED
					: picked > 0 ? PCheckBox.STATE_PARTIAL : PCheckBox.STATE_UNCHECKED,
				CheckColor = PUITuning.Colors.ComponentLightStyle,
				Margin = new RectOffset(0, 4, 0, 0),
				OnChecked = (_, state) =>
				{
					if (usable.Count == 0)
						return;
					plan.Pick(usable, state != PCheckBox.STATE_CHECKED);
					dialog.Rebuild();
				},
			};
		}

		/// <summary>The families the chosen materials allow.</summary>
		private List<Family> Usable(List<Family> families) => families.FindAll(f => plan.Blocked(f) == null);
	}
}
