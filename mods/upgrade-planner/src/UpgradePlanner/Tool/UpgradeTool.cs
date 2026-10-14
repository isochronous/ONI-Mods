using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The box-drag tool. Every finished building in the box that the plan takes gets what the
	/// plan makes of it: a replacement order, built over it as the build tool does when a pipe is
	/// dragged over a pipe, or for a type the game does not let you build over, the game's own
	/// reconstruct. The plan is edited in <see cref="UpgradeDialog"/>, which opens with the tool;
	/// until it is complete a drag shows an error instead. While the tool is active, every building
	/// the plan would touch is highlighted, and the overlay is the one the plan's types are built in.
	/// </summary>
	public sealed class UpgradeTool : DragTool
	{
		// Cursor textures and the drag-area visualizer are cloned from a vanilla box tool; these
		// DragTool/InterfaceTool fields are Unity-serialized on its prefab and not otherwise reachable.
		private static readonly FieldInfo[] DonorFields =
		{
			AccessTools.Field(typeof(InterfaceTool), "cursor"),
			AccessTools.Field(typeof(DragTool), "boxCursor"),
			AccessTools.Field(typeof(DragTool), "areaVisualizer"),
			AccessTools.Field(typeof(DragTool), "areaVisualizerTextPrefab"),
		};
		private static readonly FieldInfo AreaColourField = AccessTools.Field(typeof(DragTool), "areaColour");

		private static readonly Color32 AreaColor = new Color32(96, 200, 255, 96);
		/// <summary>A building the plan would touch.</summary>
		private static readonly Color HighlightColor = new Color(0.35f, 0.8f, 1f, 0.45f);
		/// <summary>One already marked: a replacement is pending on it or a rebuild asked for, so a drag skips it.</summary>
		private static readonly Color MarkedColor = new Color(0.45f, 1f, 0.4f, 0.5f);
		/// <summary>Research and sandbox mode change what a plan allows without changing the plan, so it is worked out again this often.</summary>
		private const float RecompileSeconds = 1f;

		private UpgradePlan plan;
		private UpgradeDialog dialog;
		private readonly WindowToggle toggle = new WindowToggle();
		private HoverTextConfiguration hoverCard;
		private readonly Overlay overlay = new Overlay();

		/// <summary>The plan as the highlight sees it, worked out again when it changes or after <see cref="RecompileSeconds"/>.</summary>
		private Matcher matcher;
		private int matcherVersion = -1;
		private float matcherTime;
		private readonly HashSet<ToolMenu.CellColorData> cellColors = new HashSet<ToolMenu.CellColorData>();

		/// <summary>The plan as the drag in progress uses it, and the buildings it has taken so far.</summary>
		private Matcher dragMatcher;
		private readonly HashSet<BuildingComplete> dragged = new HashSet<BuildingComplete>();

		protected override void OnPrefabInit()
		{
			plan = UpgradePlan.Current;
			DragTool donor = ClearTool.Instance;
			if (donor != null)
			{
				visualizer = donor.visualizer;
				visualizerLayer = donor.visualizerLayer;
				foreach (FieldInfo field in DonorFields)
				{
					// A game update renaming one of these leaves the tool looking a little off, not broken.
					if (field != null)
						field.SetValue(this, field.GetValue(donor));
					else
						Debug.LogWarning("[UpgradePlanner] A donor tool field is missing; the cursor or drag area may look wrong.");
				}
				AreaColourField?.SetValue(this, AreaColor);
				AddHoverCard(donor);
			}
			base.OnPrefabInit();
			interceptNumberKeysForPriority = true;
			dialog = new UpgradeDialog(plan)
			{
				Changed = OnPlanChanged,
				ClosedByPlayer = OnClosedByPlayer,
				Applied = () => toggle.SetPressed(false),
			};
		}

		private void AddHoverCard(DragTool donor)
		{
			HoverTextConfiguration donorCard = donor.GetComponent<HoverTextConfiguration>();
			if (donorCard == null)
				return;
			hoverCard = gameObject.AddComponent<HoverTextConfiguration>();
			hoverCard.ToolName = ModStrings.UI.TOOLS.UPGRADEPLANNER.NAME.ToString().ToUpper();
			hoverCard.ActionName = ModStrings.UPGRADEPLANNER.HoverActionIdle;
			hoverCard.HoverTextStyleSettings = donorCard.HoverTextStyleSettings;
			hoverCard.ToolTitleTextStyle = donorCard.ToolTitleTextStyle;
			hoverCard.Styles_Title = donorCard.Styles_Title;
			hoverCard.Styles_BodyText = donorCard.Styles_BodyText;
			hoverCard.Styles_Instruction = donorCard.Styles_Instruction;
			hoverCard.Styles_Warning = donorCard.Styles_Warning;
			hoverCard.Styles_Values = donorCard.Styles_Values;
		}

		protected override Mode GetMode() => Mode.Box;

		protected override void OnActivateTool()
		{
			base.OnActivateTool();
			toggle.Show(OpenWindow);
			ToolMenu.Instance.PriorityScreen.Show(show: true);
			OnPlanChanged();
			OpenWindow();
		}

		protected override void OnDeactivateTool(InterfaceTool new_tool)
		{
			dialog.Close();
			toggle.Hide();
			overlay.Restore();
			ToolMenu.Instance.PriorityScreen.Show(show: false);
			dragMatcher = null;
			dragged.Clear();
			matcher = null;
			cellColors.Clear();
			base.OnDeactivateTool(new_tool);
		}

		private void OpenWindow()
		{
			if (!dialog.IsOpen)
				dialog.Show();
			toggle.SetPressed(true);
		}

		private void OnPlanChanged()
		{
			overlay.Follow(plan.TouchedDefs());
			if (hoverCard != null)
				hoverCard.ActionName = plan.IsComplete ? ModStrings.UPGRADEPLANNER.HoverActionReady : ModStrings.UPGRADEPLANNER.HoverActionIdle;
		}

		/// <summary>The X or Escape on the window ends the tool, as closing a sandbox tool's panel does.</summary>
		private void OnClosedByPlayer()
		{
			PlayerController controller = PlayerController.Instance;
			if (controller == null || controller.ActiveTool != this)
				return;
			ToolMenu.Instance.ClearSelection();
			if (controller.ActiveTool == this)
				controller.ActivateTool(SelectTool.Instance);
		}

		// ---- dragging ----

		public override void OnLeftClickDown(Vector3 cursor_pos)
		{
			dragMatcher = plan.IsComplete ? plan.Compile() : null;
			if (dragMatcher == null)
			{
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, ModStrings.UPGRADEPLANNER.ErrorIncomplete, null, cursor_pos);
				OpenWindow();
				return;
			}
			dragged.Clear();
			base.OnLeftClickDown(cursor_pos);
		}

		protected override void OnDragTool(int cell, int distFromOrigin)
		{
			if (dragMatcher == null)
				return;
			foreach (ObjectLayer layer in dragMatcher.Layers)
			{
				GameObject go = Grid.Objects[cell, (int)layer];
				BuildingComplete building = go != null ? go.GetComponent<BuildingComplete>() : null;
				if (building != null && dragMatcher.Matches(building))
					dragged.Add(building);
			}
		}

		protected override void OnDragComplete(Vector3 cursorDown, Vector3 cursorUp)
		{
			if (dragMatcher == null)
				return;
			int placed = 0, skipped = 0;
			PrioritySetting priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();
			foreach (BuildingComplete building in dragged)
			{
				if (building == null)
					continue;
				if (dragMatcher.TryPlan(building, out Upgrade upgrade) && Orders.Place(building, upgrade, priority))
					placed++;
				else
					skipped++;
			}
			dragged.Clear();
			dragMatcher = null;
			matcher = null;
			if (placed + skipped == 0)
				return;
			string text = (Orders.InstantBuild ? ModStrings.UPGRADEPLANNER.PopupReplaced : ModStrings.UPGRADEPLANNER.PopupMarked).Replace("{0}", placed.ToString());
			if (skipped > 0)
				text = ModStrings.UPGRADEPLANNER.PopupSkipped.Replace("{0}", text).Replace("{1}", skipped.ToString());
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Building, text, null, cursorUp);
		}

		// ---- the highlight of every building the plan would touch ----

		public override void GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors)
		{
			colors = cellColors;
			if (matcher != null && matcherVersion == plan.Version && Time.unscaledTime - matcherTime < RecompileSeconds)
				return;
			matcher = plan.Compile();
			matcherVersion = plan.Version;
			matcherTime = Time.unscaledTime;
			cellColors.Clear();
			if (matcher == null)
				return;
			int world = ClusterManager.Instance.activeWorldId;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
			{
				if (building == null || !matcher.Matches(building))
					continue;
				int cell = Grid.PosToCell(building);
				if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] != world)
					continue;
				Color color = IsMarked(building, cell) ? MarkedColor : HighlightColor;
				building.RunOnArea(offsetCell => cellColors.Add(new ToolMenu.CellColorData(offsetCell, color)));
			}
		}

		/// <summary>Something is pending on the building's replacement layer, or a rebuild of it is asked for: a drag skips it.</summary>
		private static bool IsMarked(BuildingComplete building, int cell)
		{
			if (building.Def.ReplacementLayer != ObjectLayer.NumLayers && building.Def.IsReplacementLayerOccupied(cell))
				return true;
			Reconstructable reconstructable = building.GetComponent<Reconstructable>();
			return reconstructable != null && reconstructable.ReconstructRequested;
		}

		/// <summary>
		/// The overlay the plan's types are built in (pipes, vents, power, automation), switched on
		/// the way the build tool does it, and the player's own put back when the tool is done.
		/// </summary>
		private sealed class Overlay
		{
			private HashedString previous;
			private bool changed;

			public void Follow(List<BuildingDef> defs)
			{
				OverlayScreen screen = OverlayScreen.Instance;
				if (screen == null)
					return;
				HashedString wanted = SharedViewMode(defs);
				if (wanted == OverlayModes.None.ID)
				{
					// A plan over pipes and tiles together has no one view: back to the player's.
					Restore();
					return;
				}
				if (screen.mode == wanted)
					return;
				if (!changed)
				{
					previous = screen.mode;
					changed = true;
				}
				screen.ToggleOverlay(wanted, allowSound: false);
			}

			public void Restore()
			{
				if (!changed)
					return;
				changed = false;
				OverlayScreen screen = OverlayScreen.Instance;
				if (screen != null && screen.mode != previous)
					screen.ToggleOverlay(previous, allowSound: false);
			}

			/// <summary>The view every one of these types is built in, or None when they differ or there are none.</summary>
			private static HashedString SharedViewMode(List<BuildingDef> defs)
			{
				if (defs.Count == 0)
					return OverlayModes.None.ID;
				HashedString shared = defs[0].ViewMode;
				foreach (BuildingDef def in defs)
					if (def.ViewMode != shared)
						return OverlayModes.None.ID;
				return shared;
			}
		}
	}
}
