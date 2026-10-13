using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The tool's settings window: two columns, "Replace" (every replaceable building type by
	/// family, then any material or specific ones) and "With" (the types the game lets you build
	/// over the chosen one, then its materials), each scrolling on its own. Types are PLib
	/// buttons; materials are picked in the game's own material panel, the one the build menu
	/// shows, placed in a framed section at the foot of the column. The window is rebuilt when a
	/// type or the any/specific choice changes, so every button's highlight reflects the plan; a
	/// material pick only updates the plan and the footer, unless it decides whether the With
	/// column has anything to show. It is an ordinary (non-modal) screen: the world stays
	/// interactive while it is open.
	/// </summary>
	internal sealed class UpgradeDialog
	{
		private const int ButtonsPerRow = 7;
		/// <summary>Both columns ask for this much, so they split the window evenly whatever their rows hold.</summary>
		private const float ColumnWidth = 460f;
		private const string SourceScroll = "SourceScroll";
		private const string TargetScroll = "TargetScroll";
		private static readonly Vector2 IconSize = new Vector2(44f, 44f);
		private static readonly RectOffset ButtonMargin = new RectOffset(5, 5, 5, 5);
		private static readonly RectOffset SectionPadding = new RectOffset(10, 10, 6, 10);

		// The schedule editor's palette: a grey body, white cards with a slate header strip and
		// a grey row area, and one dark action bar across the bottom.
		private static readonly Color BodyColor = new Color32(149, 151, 160, 255);
		private static readonly Color CardColor = new Color32(255, 255, 255, 255);
		private static readonly Color HeaderStripColor = new Color32(95, 107, 119, 255);
		private static readonly Color RowAreaColor = new Color32(201, 201, 201, 255);
		private static readonly ColorStyleSetting ActionBarStyle = MakeActionBarStyle();
		/// <summary>A button that cannot be picked yet: the blue style's disabled grey in every state.</summary>
		private static readonly ColorStyleSetting DisabledStyle = MakeDisabledStyle();

		private static ColorStyleSetting MakeDisabledStyle()
		{
			var style = ScriptableObject.CreateInstance<ColorStyleSetting>();
			Color grey = PUITuning.Colors.ButtonBlueStyle.disabledColor;
			style.inactiveColor = style.hoverColor = style.activeColor = grey;
			style.disabledColor = style.disabledActiveColor = style.disabledhoverColor = grey;
			return style;
		}

		private static ColorStyleSetting MakeActionBarStyle()
		{
			var style = ScriptableObject.CreateInstance<ColorStyleSetting>();
			style.inactiveColor = new Color32(62, 67, 87, 255);
			style.hoverColor = new Color32(88, 95, 122, 255);
			style.activeColor = new Color32(128, 138, 178, 255);
			style.disabledColor = new Color32(106, 105, 102, 255);
			style.disabledActiveColor = style.disabledColor;
			style.disabledhoverColor = new Color32(128, 128, 128, 255);
			return style;
		}

		private readonly UpgradePlan plan;
		private readonly System.Action onChanged;
		private readonly System.Action onClosedByUser;
		private readonly System.Action onApplied;
		private GameObject dialogObject;
		private GameObject sourceHost;
		private GameObject targetHost;
		private GameObject status;
		private bool suppressCloseCallback;
		/// <summary>Set when a type to replace was just picked: the next build shows its material section.</summary>
		private bool showSourceMaterials;

		public bool IsOpen => dialogObject != null;

		/// <param name="onChanged">Called whenever the plan changes.</param>
		/// <param name="onClosedByUser">Called when the X or Escape closes the window: the tool is done with.</param>
		/// <param name="onApplied">Called when Apply closes the window: the tool stays active.</param>
		public UpgradeDialog(UpgradePlan plan, System.Action onChanged, System.Action onClosedByUser, System.Action onApplied)
		{
			this.plan = plan;
			this.onChanged = onChanged;
			this.onClosedByUser = onClosedByUser;
			this.onApplied = onApplied;
		}

		public void Show()
		{
			// Rebuilding keeps each column's scroll where the player left it, so a pick further
			// down a column does not throw them back to the top.
			float sourceScroll = ScrollPosition(SourceScroll);
			float targetScroll = ScrollPosition(TargetScroll);
			Close();
			sourceHost = targetHost = status = null;
			var dialog = new PDialog("UpgradePlannerDialog")
			{
				Title = ModStrings.DialogTitle,
				Size = new Vector2(1040f, 640f),
				MaxSize = new Vector2(1500f, 950f),
				SortKey = 200f,
				DialogBackColor = BodyColor,
				DialogClosed = OnDialogClosed,
#if PLIB_DRAGGABLE
				// A PLib with the draggable title bar (built with -p:PLibVersion=... from a modified PLib).
				Draggable = true,
#endif
			};
			dialog.Body.Direction = PanelDirection.Vertical;
			dialog.Body.Alignment = TextAnchor.UpperLeft;
			dialog.Body.Spacing = 8;
			dialog.Body.FlexSize = Vector2.one;
			dialog.Body.AddChild(BuildColumns());
			dialog.Body.AddChild(new PLabel("Status")
			{
				Text = plan.Describe(),
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(6, 6, 4, 4),
			}.AddOnRealize(go => status = go));
			// The action bar: one dark button across the foot of the window, as the schedule
			// editor's "Add New Schedule" is.
			dialog.Body.AddChild(new PButton("Apply")
			{
				Text = ModStrings.Apply,
				ToolTip = ModStrings.ApplyTooltip,
				TextStyle = PUITuning.Fonts.UILightStyle,
				Color = ActionBarStyle,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(0, 0, 12, 12),
				OnClick = _ => Apply(),
			});
			// PDialog.Show() builds its own copy, so build once and activate that one: calling both
			// opened a second window behind the first on every change.
			dialogObject = dialog.Build();
			KScreen screen = dialogObject.GetComponent<KScreen>();
			if (screen != null)
			{
				screen.Activate();
				// The wheel over the window scrolls its columns and must not zoom the camera behind it.
				screen.ConsumeMouseScroll = true;
			}
			else
				dialogObject.SetActive(true);
			MakeScrollAreasCatchTheWheel();
			AttachMaterialPanels();
			// Lay the new window out now, so the scroll positions have content sizes to work with.
			UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(dialogObject.transform as RectTransform);
			// The material section is the foot of the Replace column: a fresh pick scrolls down to it.
			SetScrollPosition(SourceScroll, showSourceMaterials ? 0f : sourceScroll);
			SetScrollPosition(TargetScroll, targetScroll);
			showSourceMaterials = false;
		}

		/// <summary>
		/// The wheel reaches a scroll area through whatever the pointer is over inside it, and the
		/// gaps between buttons are over nothing; a clear image across each viewport makes the whole
		/// column answer.
		/// </summary>
		private void MakeScrollAreasCatchTheWheel()
		{
			foreach (string name in new[] { SourceScroll, TargetScroll })
			{
				KScrollRect rect = ScrollRect(name);
				if (rect == null || rect.viewport == null)
					continue;
				UnityEngine.UI.Image catcher = rect.viewport.gameObject.AddOrGet<UnityEngine.UI.Image>();
				catcher.color = Color.clear;
				catcher.raycastTarget = true;
			}
		}

		private KScrollRect ScrollRect(string name)
		{
			Transform found = dialogObject != null ? FindDeep(dialogObject.transform, name) : null;
			return found != null ? found.GetComponent<KScrollRect>() : null;
		}

		private static Transform FindDeep(Transform root, string name)
		{
			if (root.name == name)
				return root;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform found = FindDeep(root.GetChild(i), name);
				if (found != null)
					return found;
			}
			return null;
		}

		/// <summary>Where a column is scrolled to, 1 at the top; 1 when there is no such column.</summary>
		private float ScrollPosition(string name)
		{
			KScrollRect rect = ScrollRect(name);
			return rect != null ? rect.verticalNormalizedPosition : 1f;
		}

		private void SetScrollPosition(string name, float position)
		{
			KScrollRect rect = ScrollRect(name);
			if (rect != null)
				rect.verticalNormalizedPosition = position;
		}

		/// <summary>Closes the window without treating it as the player dismissing the tool.</summary>
		public void Close()
		{
			if (dialogObject == null)
				return;
			GameObject go = dialogObject;
			dialogObject = null;
			suppressCloseCallback = true;
			try
			{
				KScreen screen = go.GetComponent<KScreen>();
				if (screen != null)
					screen.Deactivate();
				// Deactivating runs the close callback; the object itself goes too, whatever the screen did.
				if (go != null)
					Object.Destroy(go);
			}
			finally
			{
				suppressCloseCallback = false;
			}
		}

		/// <summary>The X or Escape: the player is done with the tool.</summary>
		private void OnDialogClosed(string key)
		{
			if (suppressCloseCallback)
				return;
			dialogObject = null;
			onClosedByUser?.Invoke();
		}

		/// <summary>The action bar: the window closes and the tool stays active.</summary>
		private void Apply()
		{
			Close();
			onApplied?.Invoke();
		}

		private void Rebuild()
		{
			Show();
			onChanged?.Invoke();
		}

		/// <summary>A material pick changes the plan and the footer, not the layout.</summary>
		private void RefreshStatus()
		{
			LocText text = status != null ? status.GetComponent<LocText>() : null;
			if (text != null)
				text.text = plan.Describe();
			onChanged?.Invoke();
		}

		// ---- the game's material panels, added once the window exists ----

		private void AttachMaterialPanels()
		{
			if (sourceHost != null && plan.SourceDef != null)
				MaterialPanel.Attach(sourceHost, plan.SourceDef.CraftRecipe, plan.SourceMaterials, forBuilding: false, choices =>
				{
					// The With column exists only once the source is complete, so a pick that
					// completes it (or a change that undoes that) lays the window out again.
					bool wasComplete = plan.SourceComplete;
					plan.SetSourceMaterials(choices);
					if (plan.SourceComplete != wasComplete)
						Rebuild();
					else
						RefreshStatus();
				});
			if (targetHost != null && plan.Target != null)
			{
				MaterialSelectionPanel panel = MaterialPanel.Attach(targetHost, plan.Target.Representative.CraftRecipe, plan.TargetMaterials, forBuilding: true, choices =>
				{
					plan.SetTargetMaterials(choices);
					RefreshStatus();
				});
				// Nothing chosen yet: start from what the build menu would pick, the material in stock.
				if (panel != null && !plan.TargetComplete)
					panel.AutoSelectAvailableMaterial();
			}
		}

		// ---- layout ----

		private IUIComponent BuildColumns()
		{
			var columns = new PPanel("Columns")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 16,
				FlexSize = Vector2.one,
				Margin = new RectOffset(6, 6, 4, 0),
			};
			columns.AddChild(Side("Source", ModStrings.ReplaceHeading, SourceScroll, BuildSourceColumn(), clear: true));
			columns.AddChild(Side("Target", ModStrings.WithHeading, TargetScroll, BuildTargetColumn(), clear: false));
			return columns;
		}

		/// <summary>
		/// One side of the window: its heading on a fixed row above the scroll area, so it stays
		/// put while the column scrolls; the Replace side's row has the Clear button at its end.
		/// </summary>
		private PPanel Side(string name, string heading, string scrollName, PPanel column, bool clear)
		{
			var side = new PPanel(name + "Side")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = Vector2.one,
			};
			var header = new PPanel(name + "Header")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
			};
			header.AddChild(new PLabel(name + "Heading")
			{
				Text = heading,
				TextStyle = PUITuning.Fonts.TextDarkStyle.DeriveStyle(20),
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
			});
			if (clear)
				header.AddChild(new PButton("Clear")
				{
					Text = ModStrings.Clear,
					ToolTip = ModStrings.ClearTooltip,
					TextStyle = PUITuning.Fonts.TextLightStyle,
					Color = PUITuning.Colors.ButtonBlueStyle,
					Margin = new RectOffset(12, 12, 6, 6),
					OnClick = _ => { plan.Clear(); Rebuild(); },
				});
			side.AddChild(header);
			side.AddChild(Scrollable(scrollName, column));
			return side;
		}

		/// <summary>A column in a scroll pane of its own, so each side scrolls independently.</summary>
		private static PScrollPane Scrollable(string name, PPanel column)
		{
			return new PScrollPane(name)
			{
				ScrollVertical = true,
				ScrollHorizontal = false,
				AlwaysShowVertical = false,
				FlexSize = Vector2.one,
				Child = column,
			};
		}

		private static PPanel NewColumn(string name)
		{
			var column = new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 4,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(0, 12, 0, 8),
			};
			column.AddChild(new PSpacer { PreferredSize = new Vector2(ColumnWidth, 0f) });
			return column;
		}

		/// <summary>The slate strip the schedule editor heads each of its cards with.</summary>
		private static void AddHeader(PPanel card, string text)
		{
			card.AddChild(new PLabel(text)
			{
				Text = text,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
				TextAlignment = TextAnchor.MiddleLeft,
				BackColor = HeaderStripColor,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(10, 10, 6, 6),
			});
		}

		/// <summary>
		/// A card of the column, as the schedule editor lays out each schedule: a white card with
		/// a slate header strip and a grey area holding the rows. One per building category, and
		/// one for the materials. Returns the card; the rows go into <paramref name="rows"/>.
		/// </summary>
		private static PPanel NewGroup(string name, string heading, out PPanel rows)
		{
			var card = new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(4, 4, 4, 4),
				BackColor = CardColor,
			};
			AddHeader(card, heading);
			rows = new PPanel(name + "Rows")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				Margin = SectionPadding,
				BackColor = RowAreaColor,
			};
			card.AddChild(rows);
			return card;
		}

		private static void AddNote(PPanel column, string text)
		{
			column.AddChild(new PLabel("Note")
			{
				Text = text,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
				Margin = new RectOffset(0, 0, 6, 6),
			});
		}

		/// <summary>The material card at the foot of a column.</summary>
		private static PPanel NewSection(string name, out PPanel rows)
		{
			return NewGroup(name, ModStrings.MaterialHeading, out rows);
		}

		/// <summary>Lays buttons out in rows of a fixed width, since a horizontal panel never wraps.</summary>
		private static void AddRows(PPanel column, List<IUIComponent> buttons)
		{
			PPanel row = null;
			for (int i = 0; i < buttons.Count; i++)
			{
				if (i % ButtonsPerRow == 0)
				{
					row = new PPanel("Row" + i / ButtonsPerRow)
					{
						Direction = PanelDirection.Horizontal,
						Alignment = TextAnchor.MiddleLeft,
						Spacing = 2,
					};
					column.AddChild(row);
				}
				row.AddChild(buttons[i]);
			}
		}

		/// <summary>Where a game material panel goes once the window is built; see AttachMaterialPanels.</summary>
		private static PPanel MaterialHost(string name, System.Action<GameObject> onRealize)
		{
			return new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(0, 0, 4, 4),
			}.AddOnRealize(go => onRealize(go));
		}

		private PPanel BuildSourceColumn()
		{
			PPanel column = NewColumn("Source");
			foreach (ReplacementRules.Category category in ReplacementRules.SourceCategories())
				AddCategory(column, category, plan.Source, family =>
				{
					plan.SetSource(family);
					showSourceMaterials = true;
					Rebuild();
				});
			if (plan.Source == null || plan.SourceMaterials.Length == 0)
				return column;

			PPanel section = NewSection("SourceMaterialSection", out PPanel rows);
			AddRows(rows, new List<IUIComponent>
			{
				TextButton(ModStrings.AnyMaterial, ModStrings.AnyMaterialTooltip,
					plan.SourceMode == UpgradePlan.MaterialMode.Any,
					() => { plan.SetSourceAnyMaterial(); Rebuild(); }),
				TextButton(ModStrings.SpecificMaterial, ModStrings.SpecificMaterialTooltip,
					plan.SourceMode == UpgradePlan.MaterialMode.Specific,
					() => { plan.SetSourceSpecific(); Rebuild(); }),
			});
			if (plan.SourceMode == UpgradePlan.MaterialMode.Specific)
				rows.AddChild(MaterialHost("SourceMaterials", go => sourceHost = go));
			column.AddChild(section);
			return column;
		}

		private PPanel BuildTargetColumn()
		{
			PPanel column = NewColumn("Target");
			if (!plan.SourceComplete)
			{
				// Nothing to choose from until the Replace side is complete: the whole catalogue,
				// greyed, shows what the column will hold. The footer says what is still needed.
				foreach (ReplacementRules.Category category in ReplacementRules.SourceCategories())
					AddCategory(column, category, null, null);
				return column;
			}
			List<ReplacementRules.Category> categories = ReplacementRules.TargetCategories(plan.Source);
			if (categories.Count == 0)
				AddNote(column, ModStrings.NoTargets);
			foreach (ReplacementRules.Category category in categories)
				AddCategory(column, category, plan.Target, family => { plan.SetTarget(family); Rebuild(); });
			if (plan.Target == null)
				return column;
			PPanel section = NewSection("TargetMaterialSection", out PPanel rows);
			if (plan.TargetMaterials.Length == 0)
				AddNote(rows, ModStrings.NoMaterials);
			else
				rows.AddChild(MaterialHost("TargetMaterials", go => targetHost = go));
			column.AddChild(section);
			return column;
		}

		/// <summary>A category card; with no <paramref name="onPick"/> its buttons are greyed and do nothing.</summary>
		private static void AddCategory(PPanel column, ReplacementRules.Category category, ReplacementRules.Family selected, System.Action<ReplacementRules.Family> onPick)
		{
			PPanel group = NewGroup("Category" + category.Layer, category.Name, out PPanel rows);
			var buttons = new List<IUIComponent>();
			foreach (ReplacementRules.Family family in category.Families)
			{
				ReplacementRules.Family chosen = family;
				buttons.Add(BuildingButton(chosen, chosen == selected, onPick == null ? null : () => onPick(chosen)));
			}
			AddRows(rows, buttons);
			column.AddChild(group);
		}

		// ---- buttons ----

		private static PButton BuildingButton(ReplacementRules.Family family, bool selected, System.Action onClick)
		{
			BuildingDef def = family.Representative;
			string tooltip = def.Name;
			if (family.Members.Count > 1)
				tooltip += ModStrings.VariantsTooltip.Replace("{0}", family.Members.Count.ToString());
			var button = new PButton(def.PrefabID)
			{
				Sprite = def.GetUISprite(),
				SpriteSize = IconSize,
				MaintainSpriteAspect = true,
				ToolTip = tooltip,
				Margin = ButtonMargin,
				Color = selected ? PUITuning.Colors.ButtonPinkStyle : PUITuning.Colors.ButtonBlueStyle,
			};
			if (onClick != null)
				button.OnClick = _ => onClick();
			else
			{
				// Greyed: the disabled grey for the plate, the icon faded, and no clicks or hover.
				button.Color = DisabledStyle;
				button.AddOnRealize(go =>
				{
					KButton k = go.GetComponent<KButton>();
					if (k != null)
						k.isInteractable = false;
					foreach (UnityEngine.UI.Image image in go.GetComponentsInChildren<UnityEngine.UI.Image>())
						if (!(image is KImage))
							image.color = new Color(1f, 1f, 1f, 0.45f);
				});
			}
			return button;
		}

		private static PButton TextButton(string text, string tooltip, bool selected, System.Action onClick)
		{
			return new PButton(text)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Margin = new RectOffset(12, 12, 14, 14),
				Color = selected ? PUITuning.Colors.ButtonPinkStyle : PUITuning.Colors.ButtonBlueStyle,
				OnClick = _ => onClick(),
			};
		}
	}
}
