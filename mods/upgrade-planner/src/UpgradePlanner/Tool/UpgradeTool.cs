using System.Collections.Generic;
using System.Linq;
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
	/// Apply hands drags a copy of it, which they use until the next Apply or a Clear, whatever is
	/// changed in the window meanwhile. Until something is applied the cursor shows a "no" sign and
	/// a drag shows an error instead. While the tool is active, every building the plan would touch
	/// is highlighted (the plan being edited while the window is open, the applied one once it is
	/// closed), and the overlay is the one the plan's types are built in.
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
		/// <summary>
		/// One already marked (a replacement is pending on it or a rebuild asked for, so a drag skips
		/// it), or one the box being dragged holds, which the drag will mark.
		/// </summary>
		private static readonly Color MarkedColor = new Color(0.45f, 1f, 0.4f, 0.5f);
		/// <summary>Research and sandbox mode change what a plan allows without changing the plan, so it is worked out again this often.</summary>
		private const float RecompileSeconds = 1f;

		private UpgradePlan plan;
		private UpgradeDialog dialog;
		private readonly WindowToggle toggle = new WindowToggle();
		private PlannerHoverCard hoverCard;
		private readonly Overlay overlay = new Overlay();
		/// <summary>The plan as it stood at the last Apply, which drags use; null before one, or after a Clear.</summary>
		private UpgradePlan applied;

		private bool Ready => applied != null && applied.IsComplete;

		/// <summary>The plan the highlight and overlay show: the one being edited while the window is open, else the applied one.</summary>
		private UpgradePlan Shown => dialog.IsOpen ? plan : applied;

		/// <summary>The plan as the highlight sees it, worked out again when it changes or after <see cref="RecompileSeconds"/>.</summary>
		private Matcher matcher;
		private UpgradePlan matcherPlan;
		private int matcherVersion = -1;
		private float matcherTime;
		private readonly HashSet<ToolMenu.CellColorData> cellColors = new HashSet<ToolMenu.CellColorData>();

		/// <summary>The plan as the drag in progress uses it, and the buildings it has taken so far.</summary>
		private Matcher dragMatcher;
		private readonly HashSet<BuildingComplete> dragged = new HashSet<BuildingComplete>();

		/// <summary>What the box being dragged holds that the drag would mark, with what each becomes; the cells it covers; and a count of its changes, for the highlight.</summary>
		private readonly Dictionary<BuildingComplete, Upgrade> boxed = new Dictionary<BuildingComplete, Upgrade>();
		private RectInt boxCells;
		private int boxVersion, colorsBoxVersion = -1;

		protected override void OnPrefabInit()
		{
			plan = UpgradePlan.Current;
			DragTool donor = ClearTool.Instance;
			if (donor != null)
			{
				// Sweep's cell marker, with the planner's picture in place of its dolly.
				visualizer = ToolMarker.Make(donor.visualizer) ?? donor.visualizer;
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
				Applied = OnApplied,
				Cleared = () => applied = null,
				Closed = () =>
				{
					toggle.SetPressed(false);
					OnPlanChanged();
				},
			};
		}

		private void AddHoverCard(DragTool donor)
		{
			HoverTextConfiguration donorCard = donor.GetComponent<HoverTextConfiguration>();
			if (donorCard == null)
				return;
			hoverCard = gameObject.AddComponent<PlannerHoverCard>();
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
			ClearBox();
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

		/// <summary>A copy of the plan for drags, through the same save and load presets use. An unfinished plan applies nothing and leaves the last one.</summary>
		private void OnApplied()
		{
			if (!plan.IsComplete)
				return;
			applied = new UpgradePlan();
			applied.Load(plan.ToPreset(null));
		}

		private void OnPlanChanged()
		{
			overlay.Follow(Shown?.TouchedDefs() ?? new List<BuildingDef>());
			if (hoverCard != null)
				hoverCard.ActionName = Ready ? ModStrings.UPGRADEPLANNER.HoverActionReady : ModStrings.UPGRADEPLANNER.HoverActionIdle;
		}

		// ---- dragging ----

		public override void OnLeftClickDown(Vector3 cursor_pos)
		{
			dragMatcher = Ready ? applied.Compile() : null;
			if (dragMatcher == null)
			{
				// Nothing applied: finish the choices, or apply the finished ones.
				string error = plan.IsComplete ? ModStrings.UPGRADEPLANNER.ErrorNotApplied : ModStrings.UPGRADEPLANNER.ErrorIncomplete;
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, error, null, cursor_pos);
				OpenWindow();
				return;
			}
			dragged.Clear();
			ClearBox();
			base.OnLeftClickDown(cursor_pos);
		}

		/// <summary>As the box grows or shrinks by a cell, what it holds is looked at again: the highlight marks it and the hover card counts it.</summary>
		public override void OnMouseMove(Vector3 cursorPos)
		{
			base.OnMouseMove(cursorPos);
			if (!Dragging || dragMatcher == null)
				return;
			Grid.PosToXY(downPos, out int x0, out int y0);
			Grid.PosToXY(cursorPos, out int x1, out int y1);
			var cells = new RectInt(Mathf.Min(x0, x1), Mathf.Min(y0, y1), Mathf.Abs(x1 - x0) + 1, Mathf.Abs(y1 - y0) + 1);
			if (cells.Equals(boxCells))
				return;
			boxCells = cells;
			boxed.Clear();
			int world = ClusterManager.Instance.activeWorldId;
			for (int y = cells.yMin; y < cells.yMax; y++)
				for (int x = cells.xMin; x < cells.xMax; x++)
				{
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] != world)
						continue;
					foreach (ObjectLayer layer in dragMatcher.Layers)
					{
						BuildingComplete building = Grid.Objects[cell, (int)layer]?.GetComponent<BuildingComplete>();
						if (building != null && !boxed.ContainsKey(building) && dragMatcher.Matches(building) && dragMatcher.TryPlan(building, out Upgrade upgrade))
							boxed[building] = upgrade;
					}
				}
			boxVersion++;
			Count();
		}

		/// <summary>The hover card's counts: one line per type in the box, the most first, and one per material all of them take, the most first.</summary>
		private void Count()
		{
			if (hoverCard == null)
				return;
			var types = new Dictionary<BuildingDef, int>();
			var materials = new Dictionary<Tag, float>();
			foreach (KeyValuePair<BuildingComplete, Upgrade> entry in boxed)
			{
				BuildingDef def = entry.Key.Def;
				types[def] = types.TryGetValue(def, out int count) ? count + 1 : 1;
				Upgrade upgrade = entry.Value;
				if (upgrade.Target?.CraftRecipe == null)
					continue;
				foreach (Recipe.Ingredient ingredient in upgrade.Target.CraftRecipe.GetAllIngredients(upgrade.Materials))
					materials[ingredient.tag] = (materials.TryGetValue(ingredient.tag, out float mass) ? mass : 0f) + ingredient.amount;
			}
			hoverCard.Types = types.OrderByDescending(type => type.Value).ThenBy(type => type.Key.Name)
				.Select(type => type.Key.Name + ": " + type.Value).ToList();
			hoverCard.Materials = materials.OrderByDescending(material => material.Value)
				.Select(material => GameUtil.GetFormattedMass(material.Value) + " " + material.Key.ProperName()).ToList();
		}

		/// <summary>No box: nothing marked by it in the highlight, nothing counted in the hover card.</summary>
		private void ClearBox()
		{
			if (boxed.Count == 0 && boxCells.width == 0)
				return;
			boxed.Clear();
			boxCells = default;
			boxVersion++;
			Count();
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
			// What each building becomes is settled now, from the plan as it was dragged; the placing
			// is spread over frames (PlacementQueue), each building checked again as it is placed.
			var upgrades = new List<KeyValuePair<BuildingComplete, Upgrade>>();
			int refused = 0;
			foreach (BuildingComplete building in dragged)
			{
				if (building == null)
					continue;
				if (dragMatcher.TryPlan(building, out Upgrade upgrade))
					upgrades.Add(new KeyValuePair<BuildingComplete, Upgrade>(building, upgrade));
				else
					refused++;
			}
			dragged.Clear();
			dragMatcher = null;
			ClearBox();
			matcher = null;
			PlacementQueue.Add(upgrades, refused, ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority(), cursorUp);
		}

		// ---- the highlight of every building the plan would touch ----

		public override void GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors)
		{
			colors = cellColors;
			// A drag let go some other way than its end (a right click, say) leaves a box behind.
			if (!Dragging && boxed.Count > 0)
				ClearBox();
			UpgradePlan shown = Shown;
			if (colorsBoxVersion == boxVersion && matcher != null && matcherPlan == shown && matcherVersion == (shown?.Version ?? -1) && Time.unscaledTime - matcherTime < RecompileSeconds)
				return;
			matcher = shown?.Compile();
			matcherPlan = shown;
			matcherVersion = shown?.Version ?? -1;
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
				Color color = boxed.ContainsKey(building) || IsMarked(building, cell) ? MarkedColor : HighlightColor;
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
