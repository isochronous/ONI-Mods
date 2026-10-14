using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The tool's settings window, with two tabs under a row of saved presets.
	///
	/// Change material: a Materials column (what to replace, any material or one, and the one to
	/// build in, each in the game's own material picker) and a Buildings column (every type that can
	/// be rebuilt as itself, by family, as toggle buttons; a type the materials rule out is greyed,
	/// its tooltip saying why). Families fold away under their header, which also selects them all.
	///
	/// Replace type: "Replace" (every replaceable building type by family, then any material or
	/// specific ones) and "With" (the types the game lets you build over the chosen one, then its
	/// materials).
	///
	/// Columns scroll on their own. Types are PLib buttons; materials are picked in the game's own
	/// material panel, the one the build menu shows. The window is rebuilt when a choice changes the
	/// layout or a button's state, so every highlight reflects the plan. It is an ordinary
	/// (non-modal) screen: the world stays interactive while it is open.
	/// </summary>
	internal sealed class UpgradeDialog
	{
		private const int ButtonsPerRow = 7;
		/// <summary>Both columns ask for this much, so they split the window evenly whatever their rows hold.</summary>
		private const float ColumnWidth = 460f;
		/// <summary>The rows the Replace type tab's With picker shows before scrolling.</summary>
		private const int TargetPickerRows = 6;
		/// <summary>The window's height, as a share of the screen's (in the game's UI units, so UI scale counts).</summary>
		private const float ScreenShare = 0.85f;
		/// <summary>The header rows above the columns: as tall as the Clear button, on both sides.</summary>
		private const float HeaderHeight = 24f;
		private const string SourceScroll = "SourceScroll";
		private const string TargetScroll = "TargetScroll";
		private const string BuildingsScroll = "BuildingsScroll";
		/// <summary>The Change material pickers' panes: each card scrolls its own picker.</summary>
		private const string MaterialSourceScroll = "MaterialSourceScroll";
		private const string MaterialTargetScroll = "MaterialTargetScroll";
		/// <summary>The Replace type tab's source picker pane, in the material card under the types.</summary>
		private const string SourcePickerScroll = "SourcePickerScroll";
		private static readonly string[] ScrollNames = { SourceScroll, TargetScroll, BuildingsScroll, MaterialSourceScroll, MaterialTargetScroll, SourcePickerScroll };
		/// <summary>The game's tooltip colour for a reason something cannot be done.</summary>
		private const string ReasonColor = "#F44A47";
		/// <summary>Families folded away on the Change material tab, for the session.</summary>
		private static readonly HashSet<string> collapsed = new HashSet<string>();
		private static readonly Vector2 IconSize = new Vector2(44f, 44f);
		private static readonly RectOffset ButtonMargin = new RectOffset(5, 5, 5, 5);
		private static readonly RectOffset SectionPadding = new RectOffset(10, 10, 6, 10);

		// The game's management screens' palette: a grey body, grey cards with a slate header
		// strip framing white content (rows, pickers), and one dark action bar across the bottom.
		private static readonly Color BodyColor = new Color32(149, 151, 160, 255);
		private static readonly Color CardColor = new Color32(201, 201, 201, 255);
		private static readonly Color HeaderStripColor = new Color32(95, 107, 119, 255);
		private static readonly Color RowAreaColor = new Color32(255, 255, 255, 255);
		private static readonly ColorStyleSetting ActionBarStyle = MakeActionBarStyle();
		/// <summary>A button that is part of a header strip (a family's chevron and name): the strip's colour, a shade lighter under the pointer.</summary>
		private static readonly ColorStyleSetting HeaderStripStyle = MakeHeaderStripStyle();

		private static ColorStyleSetting MakeHeaderStripStyle()
		{
			var style = ScriptableObject.CreateInstance<ColorStyleSetting>();
			style.inactiveColor = style.disabledColor = style.disabledActiveColor = HeaderStripColor;
			style.activeColor = style.hoverColor = style.disabledhoverColor = new Color32(118, 131, 145, 255);
			return style;
		}
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
		private GameObject materialSourceHost;
		private GameObject materialTargetHost;
		private GameObject presetNameField;
		/// <summary>The preset picked in the list, per tab: each tab lists its own.</summary>
		private readonly Dictionary<UpgradePlan.PlanMode, PlanPreset> selectedPresets = new Dictionary<UpgradePlan.PlanMode, PlanPreset>();

		private PlanPreset selectedPreset
		{
			get => selectedPresets.TryGetValue(plan.Mode, out PlanPreset preset) ? preset : null;
			set => selectedPresets[plan.Mode] = value;
		}
		/// <summary>What the last preset action did, shown in the footer until the next change.</summary>
		private string notice;
		/// <summary>The Change material tab's families, listed once per build of the window.</summary>
		private List<ReplacementRules.Category> materialCategories;
		private bool suppressCloseCallback;

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
			bool wasOpen = IsOpen;
			var scrolls = new Dictionary<string, float>();
			foreach (string name in ScrollNames)
				scrolls[name] = ScrollPosition(name);
			Close();
			sourceHost = targetHost = status = materialSourceHost = materialTargetHost = presetNameField = null;
			if (plan.Mode == UpgradePlan.PlanMode.ChangeMaterial)
			{
				if (!wasOpen)
					MaterialSwap.Refresh();
				materialCategories = MaterialSwap.Categories(plan.MaterialAllBuildings);
			}
			if (selectedPreset != null && !Presets.For(plan.Mode).Contains(selectedPreset))
				selectedPreset = null;
			float height = WindowHeight();
			var dialog = new PDialog("UpgradePlannerDialog")
			{
				Title = ModStrings.DialogTitle,
				// One height whatever the window holds (PLib sizes a dialog to its content between
				// Size and MaxSize): folding the families or switching tabs must not resize it.
				Size = new Vector2(1040f, height),
				MaxSize = new Vector2(1500f, height),
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
			dialog.Body.AddChild(BuildTabsAndPresets());
			dialog.Body.AddChild(BuildColumns());
			dialog.Body.AddChild(new PLabel("Status")
			{
				Text = StatusText(),
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
			foreach (string name in ScrollNames)
				SetScrollPosition(name, scrolls[name]);
		}

		/// <summary>
		/// The window's fixed height: <see cref="ScreenShare"/> of the screen the dialogs open on,
		/// measured in that canvas's units so the game's UI scale is taken into account.
		/// </summary>
		internal static float WindowHeight()
		{
			GameObject parent = PDialog.GetParentObject();
			RectTransform rect = parent != null ? parent.transform as RectTransform : null;
			float screen = rect != null && rect.rect.height > 0f ? rect.rect.height : Screen.height;
			return Mathf.Floor(screen * ScreenShare);
		}

		/// <summary>The plan, with what the last preset action did after it.</summary>
		private string StatusText()
		{
			string summary = plan.Summary();
			return string.IsNullOrEmpty(notice) ? summary : summary + "    " + notice;
		}

		/// <summary>
		/// The wheel reaches a scroll area through whatever the pointer is over inside it, and the
		/// gaps between buttons are over nothing; a clear image across each viewport makes the whole
		/// column answer.
		/// </summary>
		private void MakeScrollAreasCatchTheWheel()
		{
			foreach (string name in ScrollNames)
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

		/// <summary>Lays the window out again after a change; the notice of a preset action goes with the next one.</summary>
		private void Rebuild(string newNotice = null)
		{
			notice = newNotice;
			Show();
			onChanged?.Invoke();
		}

		/// <summary>A material pick changes the plan and the footer, not the layout.</summary>
		private void RefreshStatus()
		{
			notice = null;
			LocText text = status != null ? status.GetComponent<LocText>() : null;
			if (text != null)
				text.text = StatusText();
			onChanged?.Invoke();
		}

		// ---- the game's material panels, added once the window exists ----

		private void AttachMaterialPanels()
		{
			if (materialSourceHost != null || materialTargetHost != null)
			{
				// One recipe offering every material any listed type takes, in one row: the picker's
				// one selection is the plan's one material. A pick can grey out or free types, so the
				// window is laid out again.
				Recipe recipe = MaterialSwap.PickerRecipe();
				if (recipe != null && materialSourceHost != null)
					MaterialPanel.Attach(materialSourceHost, recipe, new List<Tag> { plan.MaterialSource }, forBuilding: false, choices =>
					{
						plan.SetMaterialSource(choices.Count > 0 ? choices[0] : Tag.Invalid);
						Rebuild();
					}, strip: true, maxRows: MaterialPanel.AllRows);
				if (recipe != null && materialTargetHost != null)
					MaterialPanel.Attach(materialTargetHost, recipe, new List<Tag> { plan.MaterialTarget }, forBuilding: false, choices =>
					{
						plan.SetMaterialTarget(choices.Count > 0 ? choices[0] : Tag.Invalid);
						Rebuild();
					}, strip: true, maxRows: MaterialPanel.AllRows);
				return;
			}
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
				}, maxRows: MaterialPanel.AllRows, hideHeader: true);
			if (targetHost != null && plan.Target != null)
			{
				MaterialSelectionPanel panel = MaterialPanel.Attach(targetHost, plan.Target.Representative.CraftRecipe, plan.TargetMaterials, forBuilding: true, choices =>
				{
					plan.SetTargetMaterials(choices);
					RefreshStatus();
				}, maxRows: TargetPickerRows, hideHeader: true);
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
			if (plan.Mode == UpgradePlan.PlanMode.ChangeMaterial)
			{
				// The Materials column does not scroll: Replace and With have half of it each and
				// scroll their pickers inside.
				columns.AddChild(Side("Materials", ModStrings.MaterialsHeading, null, BuildMaterialsColumn(), new List<IUIComponent>
				{
					ClearButton(ModStrings.ClearMaterialTooltip),
				}, carded: true));
				columns.AddChild(Side("Buildings", ModStrings.BuildingsHeading, BuildingsScroll, BuildBuildingsColumn(), BuildingsHeaderTools(),
					new List<IUIComponent> { SelectBox("SelectAll", null, ModStrings.SelectAllTooltip, Usable(AllListed())) }, carded: true));
				return columns;
			}
			// The Replace side does not scroll as a whole: the types scroll in one card in its top two
			// thirds and the material card has the bottom third.
			columns.AddChild(Side("Source", ModStrings.ReplaceHeading, SourceScroll, BuildSourceColumn(), new List<IUIComponent>
			{
				ClearButton(ModStrings.ClearTooltip),
			}, carded: true, after: SourceMaterialCard()));
			columns.AddChild(Side("Target", ModStrings.WithHeading, TargetScroll, BuildTargetColumn(), null, carded: true));
			return columns;
		}

		private PButton ClearButton(string tooltip)
		{
			return HeaderButton("Clear", ModStrings.Clear, tooltip, () => { plan.Clear(); Rebuild(); });
		}

		/// <summary>A small blue button for a header strip.</summary>
		private static PButton HeaderButton(string name, string text, string tooltip, System.Action onClick)
		{
			return new PButton(name)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(13),
				Color = PUITuning.Colors.ButtonBlueStyle,
				Margin = new RectOffset(10, 10, 3, 3),
				OnClick = _ => onClick(),
			};
		}

		// ---- tabs and presets ----

		/// <summary>The two tabs on the left; the presets on the right: pick one, load or delete it, or save the current plan under a name.</summary>
		private IUIComponent BuildTabsAndPresets()
		{
			var row = new PPanel("TabsAndPresets")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(6, 6, 4, 0),
			};
			row.AddChild(TabButton(ModStrings.TabMaterial, ModStrings.TabMaterialTooltip, UpgradePlan.PlanMode.ChangeMaterial));
			row.AddChild(TabButton(ModStrings.TabType, ModStrings.TabTypeTooltip, UpgradePlan.PlanMode.ReplaceType));
			row.AddChild(new PSpacer { FlexSize = new Vector2(1f, 0f), PreferredSize = new Vector2(16f, 0f) });
			row.AddChild(new PLabel("PresetLabel")
			{
				Text = ModStrings.PresetLabel,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
			});
			// Only this tab's presets: a preset is saved from and loaded into its own tab.
			List<PlanPreset> presets = Presets.For(plan.Mode);
			if (presets.Count == 0)
				row.AddChild(new PLabel("NoPresets")
				{
					Text = ModStrings.NoPresets,
					TextStyle = PUITuning.Fonts.TextDarkStyle,
					TextAlignment = TextAnchor.MiddleLeft,
				});
			else
			{
				if (selectedPreset == null)
					selectedPreset = presets[0];
				row.AddChild(new PComboBox<PlanPreset>("PresetChoice")
				{
					Content = presets,
					InitialItem = selectedPreset,
					MinWidth = 180,
					TextStyle = PUITuning.Fonts.TextLightStyle,
					OnOptionSelected = (_, choice) => selectedPreset = choice,
				}.SetKleiBlueStyle());
				row.AddChild(HeaderButton("PresetLoad", ModStrings.PresetLoad, ModStrings.PresetLoadTooltip, LoadPreset));
				row.AddChild(HeaderButton("PresetDelete", ModStrings.PresetDelete, ModStrings.PresetDeleteTooltip, DeletePreset));
			}
			row.AddChild(new PSpacer { PreferredSize = new Vector2(12f, 0f) });
			row.AddChild(new PTextField("PresetName")
			{
				PlaceholderText = ModStrings.PresetNamePlaceholder,
				MinWidth = 150,
				MaxLength = 60,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
			}.AddOnRealize(go => presetNameField = go));
			row.AddChild(HeaderButton("PresetSave", ModStrings.PresetSave, ModStrings.PresetSaveTooltip, SavePreset));
			return row;
		}

		/// <summary>A tab: pink while it is the current one, as a picked button is.</summary>
		private PButton TabButton(string text, string tooltip, UpgradePlan.PlanMode mode)
		{
			return new PButton("Tab" + mode)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
				Color = plan.Mode == mode ? PUITuning.Colors.ButtonPinkStyle : PUITuning.Colors.ButtonBlueStyle,
				Margin = new RectOffset(14, 14, 6, 6),
				OnClick = _ =>
				{
					if (plan.Mode == mode)
						return;
					plan.SetMode(mode);
					Rebuild();
				},
			};
		}

		private void SavePreset()
		{
			string name = presetNameField != null ? PTextField.GetText(presetNameField)?.Trim() : null;
			if (string.IsNullOrEmpty(name))
			{
				notice = ModStrings.PresetNeedsName;
				LocText text = status != null ? status.GetComponent<LocText>() : null;
				if (text != null)
					text.text = StatusText();
				return;
			}
			PlanPreset preset = plan.ToPreset(name);
			bool replaced = Presets.Put(preset);
			selectedPreset = preset;
			Rebuild((replaced ? ModStrings.PresetUpdated : ModStrings.PresetSaved).Replace("{0}", name));
		}

		private void LoadPreset()
		{
			if (selectedPreset == null)
				return;
			plan.Apply(selectedPreset);
			Rebuild(ModStrings.PresetLoaded.Replace("{0}", selectedPreset.Name));
		}

		private void DeletePreset()
		{
			if (selectedPreset == null)
				return;
			string name = selectedPreset.Name;
			Presets.Remove(selectedPreset);
			selectedPreset = null;
			Rebuild(ModStrings.PresetDeleted.Replace("{0}", name));
		}

		/// <summary>
		/// One side of the window: its heading on a fixed row above the scroll area, so it stays
		/// put while the column scrolls, with <paramref name="tools"/> (the Clear button, say) at its end.
		/// With no <paramref name="scrollName"/> the column fills the side and does not scroll.
		/// </summary>
		private PPanel Side(string name, string heading, string scrollName, PPanel column, List<IUIComponent> tools, List<IUIComponent> leading = null,
			bool carded = false, IUIComponent after = null)
		{
			var side = new PPanel(name + "Side")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = Vector2.one,
			}.AddOnRealize(go =>
			{
				// Both sides ask for no width and an equal share, so they split the window in half
				// whatever their content would ask for.
				var share = go.AddOrGet<UnityEngine.UI.LayoutElement>();
				share.minWidth = ColumnWidth;
				share.preferredWidth = ColumnWidth;
				share.flexibleWidth = 1f;
				share.layoutPriority = 10;
			});
			var header = new PPanel(name + "Header")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				// A strip like the cards' own headers; the room on the right keeps the Clear
				// button clear of the scrollbar below it (inside a card, the card's frame does).
				BackColor = HeaderStripColor,
				Margin = new RectOffset(10, carded ? 10 : 24, 4, 4),
			};
			// Both header rows the height of the one with the button, so the two columns start level.
			header.AddChild(new PSpacer { PreferredSize = new Vector2(0f, HeaderHeight), FlexSize = Vector2.zero });
			if (leading != null)
				foreach (IUIComponent part in leading)
					header.AddChild(part);
			header.AddChild(new PLabel(name + "Heading")
			{
				Text = heading,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(18),
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
			});
			if (tools != null)
				foreach (IUIComponent tool in tools)
					header.AddChild(tool);
			if (!carded)
			{
				side.AddChild(header);
				if (scrollName != null)
					side.AddChild(Scrollable(scrollName, column));
				else
				{
					column.FlexSize = Vector2.one;
					side.AddChild(column);
				}
				return side;
			}
			// One card under the strip, as the mockup's columns: the strip on top, then the content
			// in the card's grey; with something after it, the card has two shares to its one.
			var group = new PPanel(name + "Group")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = Vector2.one,
			};
			if (after != null)
				group.AddOnRealize(go => TakeShares(go, 2f));
			group.AddChild(header);
			var card = new PPanel(name + "ListCard")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				BackColor = CardColor,
			}.AddOnRealize(TakeAShare);
			if (scrollName != null)
				card.AddChild(Scrollable(scrollName, column).AddOnRealize(TakeAShare));
			else
			{
				column.FlexSize = Vector2.one;
				card.AddChild(column);
			}
			group.AddChild(card);
			side.AddChild(group);
			if (after != null)
			{
				// The gap above it is the holder's margin, not the side's spacing: the shares are of
				// the side's whole height, and spacing between them would push the card past its foot.
				side.Spacing = 0;
				side.AddChild(new PPanel(name + "After")
				{
					Direction = PanelDirection.Vertical,
					Alignment = TextAnchor.UpperLeft,
					FlexSize = Vector2.one,
					Margin = new RectOffset(0, 0, 6, 0),
				}.AddChild(after).AddOnRealize(TakeAShare));
			}
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
			// Width only: a spacer is flexible both ways by default, and in a column that fills its side
			// (the Materials column) it would take a share of the height like a card.
			column.AddChild(new PSpacer { PreferredSize = new Vector2(ColumnWidth, 0f), FlexSize = new Vector2(1f, 0f) });
			return column;
		}

		/// <summary>The slate strip the schedule editor heads each of its cards with.</summary>
		private static void AddHeader(PPanel card, string text)
		{
			// The label in a strip of its own: a label's margin does not inset its text as far.
			card.AddChild(new PPanel(text + "Strip")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				BackColor = HeaderStripColor,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(10, 10, 6, 6),
			}.AddChild(new PLabel(text)
			{
				Text = text,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
			}));
		}

		/// <summary>
		/// A card of the column, as the schedule editor lays out each schedule: a grey card with
		/// a slate header strip and a white area holding the rows. One per building category, and
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
			card.AddChild(Framed(rows, new Vector2(1f, 0f)));
			return card;
		}

		/// <summary>
		/// <paramref name="rows"/> in the card's grey frame, on its sides and bottom only: the strip
		/// above runs flush to the card's edges, as in the game's schedule editor.
		/// </summary>
		private static PPanel Framed(PPanel rows, Vector2 flex)
		{
			return new PPanel(rows.Name + "Frame")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = flex,
				Margin = new RectOffset(4, 4, 0, 4),
			}.AddChild(rows);
		}

		/// <summary>The steps of a plan, one line each, the one at <paramref name="current"/> in bold with a marker.</summary>
		private static PPanel HowToCard(int current)
		{
			PPanel card = NewGroup("HowTo", ModStrings.HowToHeading, out PPanel rows);
			string[] steps = { ModStrings.StepPickSource, ModStrings.StepPickSourceMaterial, ModStrings.StepPickTarget, ModStrings.StepApply };
			for (int i = 0; i < steps.Length; i++)
			{
				bool now = i == current;
				rows.AddChild(new PLabel("Step" + i)
				{
					Text = (now ? "\u25B6 " : "   ") + (i + 1) + ".  " + steps[i],
					TextStyle = now ? PUITuning.Fonts.TextDarkStyle.DeriveStyle(15, style: TMPro.FontStyles.Bold) : PUITuning.Fonts.TextDarkStyle.DeriveStyle(15),
					TextAlignment = TextAnchor.MiddleLeft,
					FlexSize = new Vector2(1f, 0f),
					Margin = new RectOffset(4, 4, 3, 3),
				});
			}
			return card;
		}

		/// <summary>What the With side needs next, in larger dark text across the top of the column.</summary>
		private static void AddPrompt(PPanel column, string text)
		{
			column.AddChild(new PLabel("Prompt")
			{
				Text = text,
				TextStyle = PUITuning.Fonts.TextDarkStyle.DeriveStyle(17),
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(4, 4, 8, 10),
			});
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

		// ---- Change material ----

		/// <summary>Replace (any material, or one in the picker) over With (the one to build in), half the column each.</summary>
		private PPanel BuildMaterialsColumn()
		{
			PPanel column = NewColumn("Materials");
			// The column never scrolls (its cards share its fixed height), so no room is kept for a
			// scrollbar: the cards sit as far from the card's frame on every side.
			column.Margin = new RectOffset(0, 0, 0, 0);
			bool specific = plan.MaterialSourceMode == UpgradePlan.MaterialMode.Specific;
			// Any or Specific in Replace's strip; the card holds the picker, or a line saying what Any does.
			var choice = new List<IUIComponent>
			{
				HeaderToggle(ModStrings.AnyMaterial, ModStrings.MaterialAnyTooltip, !specific,
					() => { plan.SetMaterialSourceAny(); Rebuild(); }),
				HeaderToggle(ModStrings.SpecificMaterial, ModStrings.MaterialSpecificTooltip, specific,
					() => { plan.SetMaterialSourceSpecific(); Rebuild(); }),
			};
			IUIComponent replaceContent = specific
				? (IUIComponent)Scrollable(MaterialSourceScroll, MaterialHost("MaterialSourcePicker", go => materialSourceHost = go)).AddOnRealize(TakeAShare)
				: Paragraph("MaterialAnyContent", ModStrings.MaterialAnyContent);
			column.AddChild(MaterialCard("MaterialSourceCard", ModStrings.ReplaceHeading, choice, replaceContent));
			column.AddChild(MaterialCard("MaterialTargetCard", ModStrings.WithHeading, null,
				Scrollable(MaterialTargetScroll, MaterialHost("MaterialTargetPicker", go => materialTargetHost = go)).AddOnRealize(TakeAShare)));
			return column;
		}

		/// <summary>The window's dark body text, wrapping at the label's width.</summary>
		private static TextStyleSetting WrappingStyle()
		{
			TextStyleSetting style = PUITuning.Fonts.TextDarkStyle.DeriveStyle(15);
			style.enableWordWrapping = true;
			return style;
		}

		/// <summary>A small toggle for a header strip: pink while chosen, as a picked button is.</summary>
		private static PButton HeaderToggle(string text, string tooltip, bool selected, System.Action onClick)
		{
			return new PButton(text)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(13),
				Color = selected ? PUITuning.Colors.ButtonPinkStyle : PUITuning.Colors.ButtonBlueStyle,
				Margin = new RectOffset(10, 10, 3, 3),
				OnClick = _ => onClick(),
			};
		}

		/// <summary>
		/// A card of the Materials column: the slate strip with its heading and
		/// <paramref name="headerTools"/>, then the white area holding <paramref name="content"/> (a
		/// picker in a pane of its own, which scrolls when the picker is taller than the card). Every
		/// card asks for no height and an equal share of what is left, so two of them split the
		/// column in half whatever they hold.
		/// </summary>
		private static PPanel MaterialCard(string name, string heading, List<IUIComponent> headerTools, IUIComponent content)
		{
			var card = new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = Vector2.one,
				BackColor = CardColor,
			}.AddOnRealize(TakeAShare);
			var header = new PPanel(name + "Header")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				BackColor = HeaderStripColor,
				Margin = new RectOffset(10, 10, 4, 4),
			};
			// As tall as a strip with buttons, so both cards' strips match.
			header.AddChild(new PSpacer { PreferredSize = new Vector2(0f, HeaderHeight), FlexSize = Vector2.zero });
			header.AddChild(new PLabel(name + "Heading")
			{
				Text = heading,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
			});
			if (headerTools != null)
				foreach (IUIComponent tool in headerTools)
					header.AddChild(tool);
			card.AddChild(header);
			var rows = new PPanel(name + "Rows")
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = Vector2.one,
				Margin = SectionPadding,
				BackColor = RowAreaColor,
			};
			rows.AddChild(content);
			card.AddChild(Framed(rows, Vector2.one));
			return card;
		}

		/// <summary>Asks the layout for no height of its own and an equal share of the space left (PLib honours the higher priority).</summary>
		private static void TakeAShare(GameObject go) => TakeShares(go, 1f);

		/// <summary>As <see cref="TakeAShare"/>, with <paramref name="shares"/> shares of the space left.</summary>
		private static void TakeShares(GameObject go, float shares)
		{
			var share = go.AddOrGet<UnityEngine.UI.LayoutElement>();
			share.minHeight = 0f;
			share.preferredHeight = 0f;
			share.flexibleHeight = shares;
			share.layoutPriority = 10;
		}

		/// <summary>
		/// A paragraph for a card: dark text, aligned left (the game's text style centres it), wrapped
		/// at whatever width the card has. It asks for no width: as one line it would widen its column.
		/// </summary>
		private static PPanel Paragraph(string name, string text)
		{
			// Not a PLabel: a PLabel fixes its text's box at the text's size when it is built (one
			// line as long as the sentence) and keeps it. This is a plain panel whose text is laid
			// out like any other child, at the panel's width, and as tall as its wrapped lines.
			return new PPanel(name)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(4, 4, 4, 4),
			}.AddOnRealize(go =>
			{
				var holder = new GameObject("Text", typeof(RectTransform));
				holder.SetActive(false);
				holder.transform.SetParent(go.transform, false);
				// As PLib makes its own text: key and style set while the object is inactive.
				LocText label = holder.AddComponent<LocText>();
				label.key = string.Empty;
				label.textStyleSetting = WrappingStyle();
				holder.SetActive(true);
				label.text = text;
				label.alignment = TMPro.TextAlignmentOptions.TopLeft;
				label.textWrappingMode = TMPro.TextWrappingModes.Normal;
				label.raycastTarget = false;
				// The width is the panel's, whatever the sentence would ask for on one line.
				var width = holder.AddComponent<UnityEngine.UI.LayoutElement>();
				width.minWidth = 0f;
				width.preferredWidth = 0f;
				width.flexibleWidth = 1f;
				width.layoutPriority = 10;
			});
		}

		/// <summary>The families the materials allow, of every listed one.</summary>
		private List<ReplacementRules.Family> Usable(IEnumerable<ReplacementRules.Family> families)
		{
			var usable = new List<ReplacementRules.Family>();
			foreach (ReplacementRules.Family family in families)
				if (MaterialSwap.Blocked(family, plan) == null)
					usable.Add(family);
			return usable;
		}

		private List<ReplacementRules.Family> AllListed()
		{
			var all = new List<ReplacementRules.Family>();
			foreach (ReplacementRules.Category category in materialCategories)
				all.AddRange(category.Families);
			return all;
		}

		/// <summary>The state a select-all box shows for these families: on, off, or some.</summary>
		private int CheckState(List<ReplacementRules.Family> usable)
		{
			int on = 0;
			foreach (ReplacementRules.Family family in usable)
				if (plan.IsMaterialTypeSelected(family.Representative.PrefabID))
					on++;
			if (usable.Count > 0 && on == usable.Count)
				return PCheckBox.STATE_CHECKED;
			return on > 0 ? PCheckBox.STATE_PARTIAL : PCheckBox.STATE_UNCHECKED;
		}

		/// <summary>
		/// All buildings (offered with Blueprints Expanded, which keeps the settings of what is rebuilt),
		/// Expand all and Collapse all, in the Buildings header after its name; the select-all box goes
		/// before the name.
		/// </summary>
		private List<IUIComponent> BuildingsHeaderTools()
		{
			var tools = new List<IUIComponent>();
			if (BlueprintsCompat.Loaded)
				tools.Add(new PCheckBox("AllBuildings")
				{
					Text = ModStrings.AllBuildings,
					ToolTip = ModStrings.AllBuildingsTooltip,
					TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(14),
					InitialState = plan.MaterialAllBuildings ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED,
					CheckColor = PUITuning.Colors.ComponentLightStyle,
					Margin = new RectOffset(0, 8, 0, 0),
					OnChecked = (_, current) =>
					{
						plan.SetMaterialAllBuildings(current != PCheckBox.STATE_CHECKED);
						Rebuild();
					},
				});
			tools.AddRange(new List<IUIComponent>
			{
				HeaderButton("ExpandAll", ModStrings.ExpandAll, null, () => { collapsed.Clear(); Rebuild(); }),
				HeaderButton("CollapseAll", ModStrings.CollapseAll, null, () =>
				{
					foreach (ReplacementRules.Category category in materialCategories)
						collapsed.Add(category.Key);
					Rebuild();
				}),
			});
			return tools;
		}

		/// <summary>
		/// A select-all box over <paramref name="usable"/>: checked when all of them are picked,
		/// partly when some are; a click on a checked box drops them all, otherwise picks them all.
		/// </summary>
		private PCheckBox SelectBox(string name, string text, string tooltip, List<ReplacementRules.Family> usable)
		{
			int state = CheckState(usable);
			return new PCheckBox(name)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(14),
				InitialState = state,
				CheckColor = PUITuning.Colors.ComponentLightStyle,
				Margin = new RectOffset(0, 4, 0, 0),
				OnChecked = (_, current) =>
				{
					if (usable.Count == 0)
						return;
					plan.SetMaterialTypesSelected(usable, current != PCheckBox.STATE_CHECKED);
					Rebuild();
				},
			};
		}

		private PPanel BuildBuildingsColumn()
		{
			PPanel column = NewColumn("Buildings");
			foreach (ReplacementRules.Category category in materialCategories)
				column.AddChild(FamilyCard(category));
			return column;
		}

		/// <summary>
		/// One family's card, as the Replace type tab's: the slate strip holds its select-all box, the
		/// chevron that folds it and its name (a click on the name folds it too), and how many of it
		/// are picked; the white area holds the types as the build menu shows them.
		/// </summary>
		/// <summary>A button drawn as part of the strip it sits on: its colour without the border every button has.</summary>
		private static void Flatten(GameObject button)
		{
			KImage background = button.GetComponent<KImage>();
			if (background != null)
				background.sprite = null;
		}

		private PPanel FamilyCard(ReplacementRules.Category category)
		{
			bool open = !collapsed.Contains(category.Key);
			System.Action fold = () =>
			{
				if (!collapsed.Remove(category.Key))
					collapsed.Add(category.Key);
				Rebuild();
			};
			List<ReplacementRules.Family> usable = Usable(category.Families);
			int picked = 0;
			foreach (ReplacementRules.Family family in usable)
				if (plan.IsMaterialTypeSelected(family.Representative.PrefabID))
					picked++;
			var card = new PPanel("Family" + category.Key)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 0,
				FlexSize = new Vector2(1f, 0f),
				BackColor = CardColor,
			};
			var header = new PPanel("FamilyHeader" + category.Key)
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				BackColor = HeaderStripColor,
				Margin = new RectOffset(10, 10, 4, 4),
			};
			header.AddChild(SelectBox("FamilySelect" + category.Key, null, ModStrings.SelectAllTooltip, usable));
			header.AddChild(new PButton("FamilyFold" + category.Key)
			{
				Sprite = open ? PUITuning.Images.Contract : PUITuning.Images.Expand,
				SpriteSize = new Vector2(14f, 14f),
				Color = HeaderStripStyle,
				Margin = new RectOffset(3, 3, 3, 3),
				OnClick = _ => fold(),
			}.AddOnRealize(Flatten));
			header.AddChild(new PButton("FamilyName" + category.Key)
			{
				Text = category.Name,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
				TextAlignment = TextAnchor.MiddleLeft,
				Color = HeaderStripStyle,
				Margin = new RectOffset(0, 0, 2, 2),
				FlexSize = new Vector2(1f, 0f),
				OnClick = _ => fold(),
			}.AddOnRealize(Flatten));
			header.AddChild(new PLabel("FamilyCount" + category.Key)
			{
				Text = ModStrings.CategoryCount.Replace("{0}", picked.ToString()).Replace("{1}", category.Families.Count.ToString()),
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(13),
				TextAlignment = TextAnchor.MiddleRight,
			});
			card.AddChild(header);
			if (!open)
				return card;
			var rows = new PPanel("FamilyRows" + category.Key)
			{
				Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				Margin = SectionPadding,
				BackColor = RowAreaColor,
			};
			var buttons = new List<IUIComponent>();
			foreach (ReplacementRules.Family family in category.Families)
			{
				ReplacementRules.Family type = family;
				string reason = MaterialSwap.Blocked(type, plan);
				bool selected = reason == null && plan.IsMaterialTypeSelected(type.Representative.PrefabID);
				if (reason != null)
					buttons.Add(BuildingButton(type, false, null, reason));
				else
					buttons.Add(BuildingButton(type, selected, () =>
					{
						plan.SetMaterialTypesSelected(new[] { type }, !selected);
						Rebuild();
					}, selected ? ModStrings.LeaveOutTooltip : ModStrings.IncludeTooltip, isReason: false));
			}
			AddRows(rows, buttons);
			card.AddChild(Framed(rows, new Vector2(1f, 0f)));
			return card;
		}

		// ---- Replace type ----

		private PPanel BuildSourceColumn()
		{
			PPanel column = NewColumn("Source");
			foreach (ReplacementRules.Category category in ReplacementRules.SourceCategories())
				AddCategory(column, category, plan.Source, family =>
				{
					plan.SetSource(family);
					Rebuild();
				});
			return column;
		}

		/// <summary>
		/// The Replace side's material card: Any or Specific in its strip once a type is picked, and
		/// the picker for Specific (in a pane, which scrolls), or a line saying what Any does.
		/// </summary>
		private PPanel SourceMaterialCard()
		{
			List<IUIComponent> tools = null;
			IUIComponent content;
			if (plan.Source == null || plan.SourceMaterials.Length == 0)
				content = Paragraph("SourceMaterialPrompt", ModStrings.SourceMaterialPrompt);
			else
			{
				tools = new List<IUIComponent>
				{
					HeaderToggle(ModStrings.AnyMaterial, ModStrings.AnyMaterialTooltip, plan.SourceMode == UpgradePlan.MaterialMode.Any,
						() => { plan.SetSourceAnyMaterial(); Rebuild(); }),
					HeaderToggle(ModStrings.SpecificMaterial, ModStrings.SpecificMaterialTooltip, plan.SourceMode == UpgradePlan.MaterialMode.Specific,
						() => { plan.SetSourceSpecific(); Rebuild(); }),
				};
				if (plan.SourceMode == UpgradePlan.MaterialMode.Specific)
					content = Scrollable(SourcePickerScroll, MaterialHost("SourceMaterials", go => sourceHost = go)).AddOnRealize(TakeAShare);
				else if (plan.SourceMode == UpgradePlan.MaterialMode.Any)
					content = Paragraph("SourceAnyContent", ModStrings.SourceAnyContent);
				else
					content = Paragraph("SourceModePrompt", ModStrings.StepPickSourceMaterial);
			}
			return MaterialCard("SourceMaterialCard", ModStrings.MaterialHeading, tools, content);
		}

		private PPanel BuildTargetColumn()
		{
			PPanel column = NewColumn("Target");
			// Its cards fit the side (the picker scrolls inside its own), so no room is kept for a
			// scrollbar: they sit as far from the card's frame on every side.
			column.Margin = new RectOffset(0, 0, 0, 0);
			if (!plan.SourceComplete)
			{
				// Nothing to choose from until the Replace side is complete: the steps, with the
				// next one marked, in a card of the column's own kind.
				column.AddChild(HowToCard(plan.Source == null ? 0 : 1));
				return column;
			}
			List<ReplacementRules.Category> categories = ReplacementRules.TargetCategories(plan.Source);
			if (categories.Count == 0)
				AddNote(column, ModStrings.NoTargets);
			else if (plan.Target == null)
				AddPrompt(column, ModStrings.ChooseTarget);
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

		/// <summary>
		/// A type as the build menu shows it. With no <paramref name="onClick"/> it is greyed and does
		/// nothing. <paramref name="note"/> goes on a second line of the tooltip: the reason it is
		/// greyed (in the game's colour for one), or what a click does.
		/// </summary>
		private static PButton BuildingButton(ReplacementRules.Family family, bool selected, System.Action onClick, string note = null, bool isReason = true)
		{
			BuildingDef def = family.Representative;
			string tooltip = family.Name;
			if (family.Members.Count > 1)
				tooltip += ModStrings.VariantsTooltip.Replace("{0}", family.Members.Count.ToString());
			if (!string.IsNullOrEmpty(note))
				tooltip += "\n" + (isReason ? "<color=" + ReasonColor + ">" + note + "</color>" : note);
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
