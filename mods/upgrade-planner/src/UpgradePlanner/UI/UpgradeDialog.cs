using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradePlanner
{
	/// <summary>
	/// The tool's window: the tabs and the current tab's presets across the top, the tab's two
	/// sides (<see cref="MaterialTabView"/> or <see cref="TypeTabView"/>), a footer saying what a drag does,
	/// and Apply across the foot. It is an ordinary (non-modal) screen, so the world stays usable
	/// while it is open. A choice that changes what the window shows lays it out again, keeping each
	/// pane's scroll, so every highlight follows the plan; a material pick changes only the footer.
	/// </summary>
	internal sealed class UpgradeDialog
	{
		/// <summary>The window's height, as a share of the screen's (in the game's UI units, so UI scale counts).</summary>
		private const float ScreenShare = 0.85f;
		private const string ObjectName = "UpgradePlannerDialog";

		/// <summary>The panes whose scroll is kept when the window is laid out again.</summary>
		internal static readonly string[] ScrollPanes =
		{
			TypeTabView.SourceScroll, TypeTabView.TargetScroll, TypeTabView.SourcePickerScroll,
			MaterialTabView.BuildingsScroll, MaterialTabView.SourcePickerScroll, MaterialTabView.TargetPickerScroll,
		};

		private readonly UpgradePlan plan;
		private readonly PresetBar presets;
		private GameObject window;
		private LocText status;
		/// <summary>The tab's sides as laid out now, which attach their pickers once the window exists.</summary>
		private ITabView view;
		/// <summary>What the last preset action did, shown in the footer until the next change.</summary>
		private string notice;
		private bool closing;

		/// <summary>Called whenever the plan changes.</summary>
		public System.Action Changed;
		/// <summary>
		/// Called when the player closes the window, with Apply, its X or Escape: the tool stays
		/// active, the plan as it stands. A second Escape ends the tool, as it ends any tool.
		/// </summary>
		public System.Action Closed;

		public UpgradeDialog(UpgradePlan plan)
		{
			this.plan = plan;
			presets = new PresetBar(plan, this);
		}

		public bool IsOpen => window != null;

		/// <summary>Opens the window, or lays it out again as the plan now stands.</summary>
		public void Show()
		{
			bool reopening = IsOpen;
			var scrolls = new Dictionary<string, float>();
			foreach (string pane in ScrollPanes)
				scrolls[pane] = Pane(pane)?.verticalNormalizedPosition ?? 1f;
			Close();
			// Opening looks at every type afresh: research, sandbox mode and the buildings standing
			// change what can be built over what.
			if (!reopening)
				MaterialSwap.Refresh();
			view = plan.Mode == PlanMode.ChangeMaterial ? (ITabView)new MaterialTabView(plan.ChangeMaterial, this) : new TypeTabView(plan.ReplaceType, this);

			float height = WindowHeight();
			var dialog = new PDialog(ObjectName)
			{
				Title = ModStrings.UPGRADEPLANNER.DialogTitle,
				// One height whatever the window holds (PLib sizes a dialog to its content between Size
				// and MaxSize): folding the families or switching tabs must not resize it.
				Size = new Vector2(1040f, height),
				MaxSize = new Vector2(1500f, height),
				SortKey = 200f,
				DialogBackColor = Palette.Body,
				DialogClosed = OnClosed,
#if PLIB_DRAGGABLE
				// A PLib with the draggable title bar (lib/PLib; see the csproj).
				Draggable = true,
#endif
			};
			dialog.Body.Direction = PanelDirection.Vertical;
			dialog.Body.Alignment = TextAnchor.UpperLeft;
			dialog.Body.Spacing = 8;
			dialog.Body.FlexSize = Vector2.one;
			dialog.Body.AddChild(TopRow());
			dialog.Body.AddChild(new PPanel("Columns")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.UpperLeft,
				Spacing = 16,
				FlexSize = Vector2.one,
				Margin = new RectOffset(6, 6, 4, 0),
			}.AddChild(view.LeftSide()).AddChild(view.RightSide()));
			dialog.Body.AddChild(new PLabel("Status")
			{
				Text = StatusText(),
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(6, 6, 4, 4),
			}.AddOnRealize(go => status = go.GetComponent<LocText>()));
			// The action bar: one dark button across the foot, as the schedule editor's "Add New Schedule" is.
			dialog.Body.AddChild(new PButton("Apply")
			{
				Text = ModStrings.UPGRADEPLANNER.Apply,
				ToolTip = ModStrings.UPGRADEPLANNER.ApplyTooltip,
				TextStyle = PUITuning.Fonts.UILightStyle,
				Color = Palette.ActionBar,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(0, 0, 12, 12),
				OnClick = _ => Apply(),
			});

			// PDialog.Show() builds a copy of its own, so build once and activate that one: calling
			// both opened a second window behind the first on every change.
			window = dialog.Build();
			KScreen screen = window.GetComponent<KScreen>();
			if (screen != null)
			{
				screen.Activate();
				// The wheel over the window scrolls its panes and must not zoom the camera behind it.
				screen.ConsumeMouseScroll = true;
			}
			else
				window.SetActive(true);
			CatchTheWheel();
			view.AttachPickers();
			// Lay the new window out now, so the scroll positions have content sizes to work with.
			LayoutRebuilder.ForceRebuildLayoutImmediate(window.transform as RectTransform);
			foreach (KeyValuePair<string, float> scroll in scrolls)
			{
				KScrollRect pane = Pane(scroll.Key);
				if (pane != null)
					pane.verticalNormalizedPosition = scroll.Value;
			}
		}

		/// <summary>Closes the window without taking it as the player dismissing the tool.</summary>
		public void Close()
		{
			if (window == null)
				return;
			GameObject go = window;
			window = null;
			status = null;
			closing = true;
			try
			{
				KScreen screen = go.GetComponent<KScreen>();
				if (screen != null)
					screen.Deactivate();
				// Deactivating runs the close callback; the object goes too, whatever the screen did.
				if (go != null)
					Object.Destroy(go);
			}
			finally
			{
				closing = false;
			}
		}

		/// <summary>
		/// The window's fixed height: <see cref="ScreenShare"/> of the screen the dialogs open on,
		/// measured in that canvas's units so the game's UI scale is taken into account.
		/// </summary>
		internal static float WindowHeight()
		{
			RectTransform parent = PDialog.GetParentObject()?.transform as RectTransform;
			float screen = parent != null && parent.rect.height > 0f ? parent.rect.height : Screen.height;
			return Mathf.Floor(screen * ScreenShare);
		}

		/// <summary>After a choice: the window is laid out again, with <paramref name="newNotice"/> in the footer.</summary>
		internal void Rebuild(string newNotice = null)
		{
			notice = newNotice;
			Show();
			Changed?.Invoke();
		}

		/// <summary>After a choice that changes only what a drag does (a material pick): the footer says so.</summary>
		internal void RefreshStatus()
		{
			notice = null;
			ShowStatus();
			Changed?.Invoke();
		}

		/// <summary>A notice in the footer, the plan unchanged.</summary>
		internal void Notify(string text)
		{
			notice = text;
			ShowStatus();
		}

		private void ShowStatus()
		{
			if (status != null)
				status.text = StatusText();
		}

		/// <summary>The plan, with what the last preset action did after it.</summary>
		private string StatusText()
		{
			string summary = plan.Summary();
			return string.IsNullOrEmpty(notice) ? summary : summary + "    " + notice;
		}

		/// <summary>The X or Escape: PLib has closed the window.</summary>
		private void OnClosed(string key)
		{
			if (closing)
				return;
			window = null;
			Closed?.Invoke();
		}

		/// <summary>The action bar.</summary>
		private void Apply()
		{
			Close();
			Closed?.Invoke();
		}

		// ---- the top row: tabs, then the tab's presets ----

		private IUIComponent TopRow()
		{
			var row = new PPanel("TabsAndPresets")
			{
				Direction = PanelDirection.Horizontal,
				Alignment = TextAnchor.MiddleLeft,
				Spacing = 6,
				FlexSize = new Vector2(1f, 0f),
				Margin = new RectOffset(6, 6, 4, 0),
			};
			row.AddChild(TabButton(PlanMode.ChangeMaterial, ModStrings.UPGRADEPLANNER.TabMaterial, ModStrings.UPGRADEPLANNER.TabMaterialTooltip));
			row.AddChild(TabButton(PlanMode.ReplaceType, ModStrings.UPGRADEPLANNER.TabType, ModStrings.UPGRADEPLANNER.TabTypeTooltip));
			row.AddChild(new PSpacer { FlexSize = new Vector2(1f, 0f), PreferredSize = new Vector2(16f, 0f) });
			presets.AddTo(row);
			return row;
		}

		/// <summary>A tab: pink while it is the one showing, as a picked button is.</summary>
		private PButton TabButton(PlanMode mode, string text, string tooltip)
		{
			return new PButton("Tab" + mode)
			{
				Text = text,
				ToolTip = tooltip,
				TextStyle = PUITuning.Fonts.TextLightStyle.DeriveStyle(15),
				Color = Palette.Picked(plan.Mode == mode),
				Margin = new RectOffset(14, 14, 6, 6),
				OnClick = _ =>
				{
					if (plan.Mode == mode)
						return;
					plan.Mode = mode;
					Rebuild();
				},
			};
		}

		// ---- panes ----

		/// <summary>
		/// The wheel reaches a pane through whatever the pointer is over inside it, and the gaps
		/// between buttons are over nothing; a clear image across each viewport makes the whole pane answer.
		/// </summary>
		private void CatchTheWheel()
		{
			foreach (string name in ScrollPanes)
			{
				KScrollRect pane = Pane(name);
				if (pane == null || pane.viewport == null)
					continue;
				Image catcher = pane.viewport.gameObject.AddOrGet<Image>();
				catcher.color = Color.clear;
				catcher.raycastTarget = true;
			}
		}

		private KScrollRect Pane(string name)
		{
			Transform found = window != null ? FindDeep(window.transform, name) : null;
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
	}

	/// <summary>One tab's two sides of the window.</summary>
	internal interface ITabView
	{
		IUIComponent LeftSide();
		IUIComponent RightSide();
		/// <summary>Puts the game's material pickers in their places, once the window exists.</summary>
		void AttachPickers();
	}
}
