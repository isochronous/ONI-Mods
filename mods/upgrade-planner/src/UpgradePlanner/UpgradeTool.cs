using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Box-drag tool that queues a replacement build order on every finished building in the
	/// box matching the plan (type, and material if one was chosen), using the same calls the
	/// build tool makes when a pipe is dragged over an existing pipe. The plan is edited in
	/// <see cref="UpgradeDialog"/>, which opens with the tool; until the plan is complete a
	/// drag does nothing but show an error.
	/// </summary>
	public sealed class UpgradeTool : DragTool
	{
		// Cursor textures and the drag-area visualizer are cloned from a vanilla box tool;
		// these DragTool/InterfaceTool fields are Unity-serialized on its prefab and not
		// otherwise reachable (names verified against U59-744825).
		private static readonly FieldInfo CursorField = AccessTools.Field(typeof(InterfaceTool), "cursor");
		private static readonly FieldInfo BoxCursorField = AccessTools.Field(typeof(DragTool), "boxCursor");
		private static readonly FieldInfo AreaVisualizerField = AccessTools.Field(typeof(DragTool), "areaVisualizer");
		private static readonly FieldInfo AreaVisualizerTextPrefabField = AccessTools.Field(typeof(DragTool), "areaVisualizerTextPrefab");
		private static readonly FieldInfo AreaColourField = AccessTools.Field(typeof(DragTool), "areaColour");

		private static readonly Color32 AreaColor = new Color32(96, 200, 255, 96);
		/// <summary>A building the plan would touch.</summary>
		private static readonly Color HighlightColor = new Color(0.35f, 0.8f, 1f, 0.45f);
		/// <summary>One already marked: a replacement is pending on its cell, so a drag skips it.</summary>
		private static readonly Color MarkedColor = new Color(0.45f, 1f, 0.4f, 0.5f);
		private const float HighlightRefreshSeconds = 1f;

		private UpgradePlan plan;
		private UpgradeDialog dialog;
		private GameObject configButton;
		private MultiToggle configToggle;
		private HoverTextConfiguration hoverCard;
		private readonly HashSet<BuildingComplete> pending = new HashSet<BuildingComplete>();
		private readonly HashSet<ToolMenu.CellColorData> cellColors = new HashSet<ToolMenu.CellColorData>();
		private int highlightVersion = -1;
		private float highlightTime = -1f;
		private HashedString previousOverlay;
		private bool overlayChanged;

		protected override void OnPrefabInit()
		{
			plan = UpgradePlan.Current;
			DragTool donor = ClearTool.Instance;
			if (donor != null)
			{
				visualizer = donor.visualizer;
				visualizerLayer = donor.visualizerLayer;
				CopyField(CursorField, donor);
				CopyField(BoxCursorField, donor);
				CopyField(AreaVisualizerField, donor);
				CopyField(AreaVisualizerTextPrefabField, donor);
				if (AreaColourField != null)
					AreaColourField.SetValue(this, AreaColor);
				AddHoverCard(donor);
			}
			base.OnPrefabInit();
			interceptNumberKeysForPriority = true;
			dialog = new UpgradeDialog(plan, OnPlanChanged, OnDialogClosedByUser, RefreshConfigButton);
		}

		// ---- the "Upgrade Planner Config" button in the tool parameter menu, by the priority picker ----

		/// <summary>
		/// One of the game's own filter widgets (the toggles the Dig and Deconstruct tools put in
		/// that menu), relabelled: a click opens the window; while the window is open it shows as
		/// on and does nothing.
		/// </summary>
		private void AddConfigButton()
		{
			ToolParameterMenu menu = ToolMenu.Instance != null ? ToolMenu.Instance.toolParameterMenu : null;
			if (menu == null || menu.widgetPrefab == null || menu.widgetContainer == null)
				return;
			RemoveConfigButton();
			configButton = Util.KInstantiateUI(menu.widgetPrefab, menu.widgetContainer, force_active: true);
			LocText label = configButton.GetComponentInChildren<LocText>();
			if (label != null)
				label.text = ModStrings.ConfigButton;
			ToolTip tooltip = configButton.GetComponentInChildren<ToolTip>();
			if (tooltip != null)
				tooltip.SetSimpleTooltip(ModStrings.ConfigButtonTooltip);
			configToggle = configButton.GetComponentInChildren<MultiToggle>();
			if (configToggle != null)
				configToggle.onClick += OnConfigButton;
			menu.content.SetActive(true);
			RefreshConfigButton();
		}

		private void RemoveConfigButton()
		{
			if (configButton != null)
				Util.KDestroyGameObject(configButton);
			configButton = null;
			configToggle = null;
			ToolParameterMenu menu = ToolMenu.Instance != null ? ToolMenu.Instance.toolParameterMenu : null;
			if (menu != null)
				menu.ClearMenu();
		}

		private void OnConfigButton()
		{
			if (dialog.IsOpen)
				return;
			dialog.Show();
			RefreshConfigButton();
		}

		/// <summary>The widget's states as the filter menu uses them: 0 off, 1 on, 2 disabled.</summary>
		private void RefreshConfigButton()
		{
			if (configToggle != null)
				configToggle.ChangeState(dialog.IsOpen ? 1 : 0);
		}

		private void CopyField(FieldInfo field, DragTool donor)
		{
			// Tolerate a future game update renaming one of these fields: the tool degrades
			// cosmetically instead of failing to initialize.
			if (field != null)
				field.SetValue(this, field.GetValue(donor));
			else
				Debug.LogWarning("[UpgradePlanner] A donor tool field is missing; cursor/visualizer may look wrong.");
		}

		private void AddHoverCard(DragTool donor)
		{
			HoverTextConfiguration donorCard = donor.GetComponent<HoverTextConfiguration>();
			if (donorCard == null)
				return;
			hoverCard = gameObject.AddComponent<HoverTextConfiguration>();
			hoverCard.ToolName = ModStrings.ToolName.ToUpper();
			hoverCard.ActionName = ModStrings.HoverActionIdle;
			hoverCard.HoverTextStyleSettings = donorCard.HoverTextStyleSettings;
			hoverCard.ToolTitleTextStyle = donorCard.ToolTitleTextStyle;
			hoverCard.Styles_Title = donorCard.Styles_Title;
			hoverCard.Styles_BodyText = donorCard.Styles_BodyText;
			hoverCard.Styles_Instruction = donorCard.Styles_Instruction;
			hoverCard.Styles_Warning = donorCard.Styles_Warning;
			hoverCard.Styles_Values = donorCard.Styles_Values;
		}

		protected override Mode GetMode()
		{
			return Mode.Box;
		}

		protected override void OnActivateTool()
		{
			base.OnActivateTool();
			AddConfigButton();
			ToolMenu.Instance.PriorityScreen.Show(show: true);
			ApplyOverlay();
			RefreshHoverCard();
			highlightVersion = -1;
			dialog.Show();
			RefreshConfigButton();
		}

		protected override void OnDeactivateTool(InterfaceTool new_tool)
		{
			dialog.Close();
			RemoveConfigButton();
			RestoreOverlay();
			ToolMenu.Instance.PriorityScreen.Show(show: false);
			pending.Clear();
			cellColors.Clear();
			base.OnDeactivateTool(new_tool);
		}

		private void OnPlanChanged()
		{
			ApplyOverlay();
			RefreshHoverCard();
			highlightVersion = -1;
		}

		/// <summary>The X or Escape on the window ends the tool, like closing a sandbox tool's panel.</summary>
		private void OnDialogClosedByUser()
		{
			PlayerController controller = PlayerController.Instance;
			if (controller == null || controller.ActiveTool != this)
				return;
			ToolMenu.Instance.ClearSelection();
			if (controller.ActiveTool == this)
				controller.ActivateTool(SelectTool.Instance);
		}

		private void RefreshHoverCard()
		{
			if (hoverCard != null)
				hoverCard.ActionName = plan.IsComplete ? ModStrings.HoverActionReady : ModStrings.HoverActionIdle;
		}

		// ---- overlay follows the source family, as the build tool does for pipes and wires ----

		private void ApplyOverlay()
		{
			OverlayScreen overlay = OverlayScreen.Instance;
			if (overlay == null || plan.SourceDef == null)
				return;
			HashedString wanted = plan.SourceDef.ViewMode;
			if (wanted == OverlayModes.None.ID || overlay.mode == wanted)
				return;
			if (!overlayChanged)
			{
				previousOverlay = overlay.mode;
				overlayChanged = true;
			}
			overlay.ToggleOverlay(wanted, allowSound: false);
		}

		private void RestoreOverlay()
		{
			if (!overlayChanged)
				return;
			overlayChanged = false;
			OverlayScreen overlay = OverlayScreen.Instance;
			if (overlay != null && overlay.mode != previousOverlay)
				overlay.ToggleOverlay(previousOverlay, allowSound: false);
		}

		// ---- dragging ----

		public override void OnLeftClickDown(Vector3 cursor_pos)
		{
			if (!plan.IsComplete)
			{
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, ModStrings.ErrorIncomplete, null, cursor_pos);
				if (!dialog.IsOpen)
				{
					dialog.Show();
					RefreshConfigButton();
				}
				return;
			}
			base.OnLeftClickDown(cursor_pos);
		}

		protected override void OnDragTool(int cell, int distFromOrigin)
		{
			if (!plan.IsComplete)
				return;
			Consider(Grid.Objects[cell, (int)plan.SourceDef.ObjectLayer]);
			if (plan.SourceDef.TileLayer != ObjectLayer.NumLayers)
				Consider(Grid.Objects[cell, (int)plan.SourceDef.TileLayer]);
		}

		private void Consider(GameObject go)
		{
			if (go == null)
				return;
			BuildingComplete building = go.GetComponent<BuildingComplete>();
			if (building != null && plan.Matches(building))
				pending.Add(building);
		}

		protected override void OnDragComplete(Vector3 cursorDown, Vector3 cursorUp)
		{
			int queued = 0;
			int skipped = 0;
			foreach (BuildingComplete building in pending)
			{
				if (building == null)
					continue;
				if (TryQueue(building))
					queued++;
				else
					skipped++;
			}
			pending.Clear();
			highlightVersion = -1;
			if (queued + skipped == 0)
				return;
			string text = queued + (InstantBuild ? " replaced" : " marked for upgrade") + (skipped > 0 ? ", " + skipped + " skipped" : "");
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Building, text, null, cursorUp);
		}

		/// <summary>
		/// The build tool's replacement sequence for one building: the def the chosen materials
		/// stand for (a family of per-material variants resolves to one of them), candidate found
		/// on the target's layer, nothing already pending there, not the same type in the same
		/// materials, then TryReplaceTile with the building's own orientation and the materials.
		/// </summary>
		private bool TryQueue(BuildingComplete building)
		{
			var materials = new List<Tag>(plan.TargetMaterials);
			BuildingDef target = ReplacementRules.ResolveVariant(plan.Target, materials);
			if (target == null)
				return false;
			GameObject go = building.gameObject;
			int cell = Grid.PosToCell(go);
			if (!Grid.IsValidCell(cell) || !building.Def.Replaceable || !target.CanReplace(go))
				return false;
			if (target.GetReplacementCandidate(cell) != go)
				return false;
			Orientation orientation = building.Orientation;
			bool occupied = false;
			target.RunOnArea(cell, orientation, delegate(int offset_cell)
			{
				if (target.IsReplacementLayerOccupied(offset_cell))
					occupied = true;
			});
			if (occupied)
				return false;
			if (building.Def == target && ReplacementRules.SameMaterials(ReplacementRules.MaterialsOf(building), materials))
				return false;
			Vector3 pos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
			if (InstantBuild)
			{
				// The build tool's instant path (BuildTool.TryBuild): the old building goes and the
				// new one stands at once, with no order, no materials and no Duplicant.
				if (!target.IsValidBuildLocation(null, pos, orientation, replace_tile: true)
					|| !target.IsValidPlaceLocation(null, pos, orientation, replace_tile: true, out _))
					return false;
				InstantReplace(target, cell, pos, orientation, materials, go);
				return true;
			}
			GameObject built = target.TryReplaceTile(null, pos, orientation, materials);
			if (built == null)
				return false;
			Grid.Objects[cell, (int)target.ReplacementLayer] = built;
			Prioritizable prioritizable = built.GetComponent<Prioritizable>();
			if (prioritizable != null)
				prioritizable.SetMasterPriority(ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority());
			return true;
		}

		/// <summary>The build tool's own test: the debug toggle, or sandbox mode with its instant build setting on.</summary>
		private static bool InstantBuild =>
			DebugHandler.InstantBuildMode
			|| (Game.Instance != null && Game.Instance.SandboxModeActive
				&& SandboxToolParameterMenu.instance != null && SandboxToolParameterMenu.instance.settings.InstantBuild);

		/// <summary>
		/// BuildTool.InstantBuildReplace, step for step: a wider building first clears the
		/// candidates under its other cells; a tile that occupies its sim cell is released
		/// before the new building is built in its place, anything else is destroyed outright.
		/// </summary>
		private static void InstantReplace(BuildingDef target, int cell, Vector3 pos, Orientation orientation, IList<Tag> materials, GameObject old)
		{
			if (target.PlacementOffsets.Length > 1)
			{
				target.RunOnArea(cell, orientation, delegate(int offsetCell)
				{
					if (offsetCell == cell)
						return;
					GameObject neighbour = target.GetReplacementCandidate(offsetCell);
					if (neighbour == null)
						return;
					SimCellOccupier occupier = neighbour.GetComponent<SimCellOccupier>();
					if (occupier != null)
						occupier.DestroySelf(delegate { Object.Destroy(neighbour); });
					else
						Object.Destroy(neighbour);
				});
			}
			float temperature = Mathf.Min(target.Temperature, ElementLoader.GetMinMeltingPointAmongElements(materials) - 10f);
			SimCellOccupier oldOccupier = old.GetComponent<SimCellOccupier>();
			if (oldOccupier == null)
			{
				Object.Destroy(old);
				target.Build(cell, orientation, null, materials, temperature, null, playsound: false, GameClock.Instance.GetTime());
				return;
			}
			oldOccupier.DestroySelf(delegate
			{
				Object.Destroy(old);
				target.Build(cell, orientation, null, materials, temperature, null, playsound: false, GameClock.Instance.GetTime());
			});
		}

		// ---- highlight of every building the plan would touch, while the tool is active ----

		public override void GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors)
		{
			colors = cellColors;
			if (!plan.SourceComplete)
			{
				cellColors.Clear();
				return;
			}
			if (highlightVersion == plan.Version && Time.unscaledTime - highlightTime < HighlightRefreshSeconds)
				return;
			highlightVersion = plan.Version;
			highlightTime = Time.unscaledTime;
			cellColors.Clear();
			byte worldId = (byte)ClusterManager.Instance.activeWorldId;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
			{
				if (building == null || !plan.Matches(building))
					continue;
				int cell = Grid.PosToCell(building);
				if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] != worldId)
					continue;
				// Marked already: something is pending on the replacement layer, as TryQueue checks.
				Color color = building.Def.IsReplacementLayerOccupied(cell) ? MarkedColor : HighlightColor;
				building.RunOnArea(delegate(int offset_cell)
				{
					cellColors.Add(new ToolMenu.CellColorData(offset_cell, color));
				});
			}
		}
	}
}
