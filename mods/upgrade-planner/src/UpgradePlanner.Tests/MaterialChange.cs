using System.Collections;
using System.Collections.Generic;
using System.IO;
using Isochronous.TestKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// The Change material tab: a row of tiles and a ladder, the plan "tiles made of sandstone,
	/// rebuilt in granite", a box dragged over all of them with the real tool. Only the sandstone
	/// tiles get orders; the granite tile is already granite and the ladder's type is not picked.
	/// </summary>
	public static class MaterialChange
	{
		[OniTest("Upgrade Planner: Change material queues granite tiles over the sandstone ones only", Timeout = 40), Layer(1)]
		public static IEnumerator TileOrders(TestContext t)
		{
			yield return Run(t, instant: false);
		}

		[OniTest("Upgrade Planner: Change material with instant build turns the sandstone tiles to granite at once", Timeout = 40), Layer(1)]
		public static IEnumerator TileInstant(TestContext t)
		{
			yield return Run(t, instant: true);
		}

		[OniTest("Upgrade Planner: Change material says why a type is left out", Timeout = 20), Layer(1)]
		public static IEnumerator BlockedReasons(TestContext t)
		{
			var plan = new UpgradePlan();
			Family tile = FamilyOf("Tile");
			Family metalTile = FamilyOf("MetalTile");
			t.Assert.True(Offered(tile), "Tile is offered on the tab");
			t.Assert.True(Offered(metalTile), "Metal Tile is offered on the tab");
			plan.ChangeMaterial.SetTarget(SimHashes.Steel.CreateTag());
			t.Assert.Eq("Can't be built in Steel", plan.ChangeMaterial.Blocked(tile), "a tile cannot be steel");
			t.Assert.Null(plan.ChangeMaterial.Blocked(metalTile), "a metal tile can be steel");
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			t.Assert.Eq("Never built from Sandstone", plan.ChangeMaterial.Blocked(metalTile), "no metal tile is made of sandstone");
			plan.ChangeMaterial.SetSourceAny();
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.Null(plan.ChangeMaterial.Blocked(tile), "a tile can be granite");
			t.Assert.Eq("Can't be built in Granite", plan.ChangeMaterial.Blocked(metalTile), "a metal tile cannot be granite");
			yield break;
		}

		[OniTest("Upgrade Planner: a saved preset reads back as the same plan, on either tab", Timeout = 20), Layer(1)]
		public static IEnumerator PresetRoundTrip(TestContext t)
		{
			string file = Path.Combine(Path.GetTempPath(), "upgrade-planner-test-presets.json");
			string original = Presets.FilePath;
			t.Cleanup(() =>
			{
				Presets.UseFile(original);
				if (File.Exists(file))
					File.Delete(file);
			});
			if (File.Exists(file))
				File.Delete(file);
			Presets.UseFile(file);

			var plan = new UpgradePlan();
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { FamilyOf("Tile"), FamilyOf("Ladder") }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			Presets.Put(plan.ToPreset("tiles"));
			var typed = new UpgradePlan();
			typed.Mode = PlanMode.ReplaceType;
			typed.ReplaceType.SetSource(FamilyOf("LiquidConduit"));
			typed.ReplaceType.SetSourceAny();
			typed.ReplaceType.SetTarget(FamilyOf("InsulatedLiquidConduit"));
			typed.ReplaceType.SetTargetMaterials(new List<Tag> { SimHashes.Ceramic.CreateTag() });
			Presets.Put(typed.ToPreset("insulate"));
			t.Assert.True(File.Exists(file), "the presets file is written");

			// Read back from the file, not from memory.
			Presets.UseFile(file);
			t.Assert.Eq(2, Presets.All.Count, "both presets are in the file");
			t.Assert.Eq(1, Presets.For(PlanMode.ChangeMaterial).Count, "the Change material tab lists one");
			t.Assert.Eq("tiles", Presets.For(PlanMode.ChangeMaterial)[0].Name, "the Change material tab lists its own");
			t.Assert.Eq(1, Presets.For(PlanMode.ReplaceType).Count, "the Replace type tab lists one");
			t.Assert.Eq("insulate", Presets.For(PlanMode.ReplaceType)[0].Name, "the Replace type tab lists its own");
			var back = new UpgradePlan();
			back.Load(Presets.Find("tiles", PlanMode.ChangeMaterial));
			t.Assert.Eq(PlanMode.ChangeMaterial, back.Mode, "the tiles preset is on the Change material tab");
			t.Assert.Eq(plan.Summary(), back.Summary(), "the tiles preset is the same plan");
			t.Assert.True(back.ChangeMaterial.IsPicked("Tile") && back.ChangeMaterial.IsPicked("Ladder"), "tiles and ladders are picked");
			t.Assert.False(back.ChangeMaterial.IsPicked("MetalTile"), "metal tiles are not");
			back.Load(Presets.Find("insulate", PlanMode.ReplaceType));
			t.Assert.Eq(PlanMode.ReplaceType, back.Mode, "the insulate preset is on the Replace type tab");
			t.Assert.Eq(typed.Summary(), back.Summary(), "the insulate preset is the same plan");
			t.Assert.True(back.IsComplete, "the insulate plan is complete");
			// A name is the tab's own: the same name on the other tab is another preset.
			t.Assert.False(Presets.Put(plan.ToPreset("insulate")), "a Change material preset named as a Replace type one is a new preset");
			t.Assert.Eq(3, Presets.All.Count, "three presets");
			t.Assert.Eq(PlanMode.ReplaceType.ToString(), Presets.Find("insulate", PlanMode.ReplaceType).Mode, "the Replace type one is untouched");
			// Saving under a name the tab already has replaces that preset.
			t.Assert.True(Presets.Put(plan.ToPreset("tiles")), "a second save under one name on one tab replaces the first");
			t.Assert.Eq(3, Presets.All.Count, "still three presets");
			yield break;
		}

		[OniTest("Upgrade Planner: each tab lists only its own presets", Timeout = 40), Layer(2)]
		public static IEnumerator PresetListsPerTab(TestContext t)
		{
			string file = Path.Combine(Path.GetTempPath(), "upgrade-planner-tab-presets.json");
			string original = Presets.FilePath;
			t.Cleanup(() =>
			{
				Presets.UseFile(original);
				if (File.Exists(file))
					File.Delete(file);
			});
			if (File.Exists(file))
				File.Delete(file);
			Presets.UseFile(file);
			var material = new UpgradePlan();
			material.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			Presets.Put(material.ToPreset("Granite everything"));
			var typed = new UpgradePlan();
			typed.Mode = PlanMode.ReplaceType;
			Presets.Put(typed.ToPreset("Insulate pipes"));

			UpgradeTool tool = Tool(t);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			t.Assert.Eq("Granite everything", PresetShown(), "the Change material tab shows its own preset");
			KButton tab = Find(GameObject.Find("UpgradePlannerDialog").transform, "TabReplaceType")?.GetComponent<KButton>();
			t.Assert.NotNull(tab, "the Replace type tab button is there");
			tab.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(1);
			t.Assert.Eq("Insulate pipes", PresetShown(), "the Replace type tab shows its own preset, with no suffix");
			// Loading it stays on its tab and changes only that tab's choices.
			plan.ChangeMaterial.SetTarget(SimHashes.Steel.CreateTag());
			KButton load = Find(GameObject.Find("UpgradePlannerDialog").transform, "PresetLoad")?.GetComponent<KButton>();
			t.Assert.NotNull(load, "the Load button is there");
			load.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(1);
			t.Assert.Eq(PlanMode.ReplaceType, plan.Mode, "loading keeps the Replace type tab");
			t.Assert.Eq(SimHashes.Steel.CreateTag(), plan.ChangeMaterial.Target, "the Change material tab's choices are untouched");
			yield return t.Screenshot("type-presets");
		}

		/// <summary>The name the preset list shows.</summary>
		private static string PresetShown()
		{
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			GameObject choice = window != null ? Find(window.transform, "PresetChoice") : null;
			LocText text = choice != null ? choice.GetComponentInChildren<LocText>() : null;
			return text != null ? text.text : null;
		}

		[OniTest("Upgrade Planner: a greyed type's button cannot be clicked and its tooltip says why", Timeout = 40), Layer(2)]
		public static IEnumerator GreyedButton(TestContext t)
		{
			UpgradeTool tool = Tool(t);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { FamilyOf("Tile"), FamilyOf("MetalTile") }, true);
			plan.ChangeMaterial.SetSourceAny();
			plan.ChangeMaterial.SetTarget(SimHashes.Steel.CreateTag());
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.NotNull(window, "the planner window is open");
			GameObject tile = Find(window.transform, "Tile");
			GameObject metalTile = Find(window.transform, "MetalTile");
			t.Assert.NotNull(tile, "the Tile button is there");
			t.Assert.NotNull(metalTile, "the Metal Tile button is there");
			t.Assert.False(tile.GetComponent<KButton>().isInteractable, "the Tile button is greyed: a tile cannot be steel");
			t.Assert.True(metalTile.GetComponent<KButton>().isInteractable, "the Metal Tile button can be clicked");
			string tip = tile.GetComponent<ToolTip>()?.GetMultiString(0);
			t.Assert.True(tip != null && tip.Contains("Can't be built in Steel"), "the Tile tooltip gives the reason: " + tip);
			// The pointer over the greyed button: the game's tooltip shows for it.
			var data = new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, tile.transform.position) };
			ExecuteEvents.Execute(tile, data, ExecuteEvents.pointerEnterHandler);
			yield return t.WaitRealSeconds(1);
			yield return t.Screenshot("greyed-tooltip");
			ExecuteEvents.Execute(tile, data, ExecuteEvents.pointerExitHandler);
		}

		[OniTest("Upgrade Planner: Replace and With split the Materials column in half and scroll their pickers inside", Timeout = 40), Layer(2)]
		public static IEnumerator MaterialsLayout(TestContext t)
		{
			// Sandbox mode: every material is shown, far more than half the column holds.
			UpgradeTool tool = Tool(t);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.NotNull(window, "the planner window is open");
			RectTransform replace = Find(window.transform, "MaterialSourceCard")?.rectTransform();
			RectTransform with = Find(window.transform, "MaterialTargetCard")?.rectTransform();
			t.Assert.NotNull(replace, "the Replace card is there");
			t.Assert.NotNull(with, "the With card is there");
			float anyHeight = replace.rect.height;
			AssertEvenColumns(t, window, "with Any");
			// Any to Specific with the window's own button, as the player does.
			KButton specific = Find(window.transform, "Specific")?.GetComponent<KButton>();
			t.Assert.NotNull(specific, "the Specific button is there");
			specific.SignalClick(KKeyCode.Mouse0);
			// The game refreshes a picker's toggles every 200 ms and sets its own size each time.
			yield return t.WaitRealSeconds(1.5);
			window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.Null(Find(window.transform, "MaterialsScroll"), "the Materials column itself does not scroll");
			replace = Find(window.transform, "MaterialSourceCard").rectTransform();
			with = Find(window.transform, "MaterialTargetCard").rectTransform();
			t.Log("Replace " + replace.rect.height + " (was " + anyHeight + " with Any), With " + with.rect.height);
			t.Assert.True(Mathf.Abs(replace.rect.height - with.rect.height) < 1.5f, "Replace and With are the same height (" + replace.rect.height + ", " + with.rect.height + ")");
			t.Assert.True(Mathf.Abs(replace.rect.height - anyHeight) < 1.5f, "Replace is as tall with Specific as with Any");
			AssertEvenColumns(t, window, "with Specific");
			RectTransform column = Find(window.transform, "Materials").rectTransform();
			t.Log("Materials column " + column.rect.height);
			t.Assert.True(replace.rect.height + with.rect.height >= column.rect.height - 24f, "Replace and With fill the column between them (" + (replace.rect.height + with.rect.height) + " of " + column.rect.height + ")");
			RectTransform side = Find(window.transform, "MaterialsSide").rectTransform();
			t.Assert.True(Inside(with, side), "the With card is wholly inside the Materials side");
			t.Assert.True(Inside(replace, side), "the Replace card is wholly inside the Materials side");
			foreach (string pane in new[] { "MaterialSourceScroll", "MaterialTargetScroll" })
			{
				GameObject scroll = Find(window.transform, pane);
				t.Assert.NotNull(scroll, pane + " is there");
				MaterialSelector selector = scroll.GetComponentInChildren<MaterialSelector>();
				t.Assert.NotNull(selector, pane + " holds a picker");
				int rows = Mathf.Max(MaterialPicker.RowsNeeded(selector), 1);
				t.Assert.Eq(MaterialPicker.Columns, selector.LayoutContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount, "the picker in " + pane + " is " + MaterialPicker.Columns + " toggles wide");
				t.Assert.Eq(MaterialPicker.FitHeight(selector, rows), selector.ScrollRect.GetComponent<UnityEngine.UI.LayoutElement>().minHeight, "the picker in " + pane + " shows all " + rows + " rows");
				t.Assert.False(selector.Scrollbar != null && selector.Scrollbar.activeInHierarchy, "the picker in " + pane + " has no scrollbar of its own");
				t.Assert.True(scroll.rectTransform().rect.height < selector.ScrollRect.rectTransform().rect.height, "the pane " + pane + " is shorter than its picker, so it scrolls");
			}
			yield return t.Screenshot("materials-specific");
		}

		[OniTest("Upgrade Planner: with eleven materials discovered, no scrollbar in the window changes in any frame", Timeout = 40), Layer(2)]
		public static IEnumerator PickerSteady(TestContext t)
		{
			// Out of sandbox mode the pickers show only discovered materials. Eleven, as in a young
			// colony: past the game's five, where it turns the side bar's scrollbar on.
			bool sandbox = Game.Instance.SandboxModeActive;
			t.Cleanup(() => Game.Instance.SandboxModeActive = sandbox);
			Game.Instance.SandboxModeActive = false;
			t.InstantBuild(false);
			var extra = new[] { SimHashes.SandStone, SimHashes.Granite, SimHashes.IgneousRock, SimHashes.SedimentaryRock, SimHashes.Obsidian, SimHashes.Cuprite, SimHashes.IronOre, SimHashes.Copper, SimHashes.Iron, SimHashes.Steel, SimHashes.Ceramic };
			foreach (SimHashes element in extra)
				DiscoveredResources.Instance.Discover(element.CreateTag());
			yield return t.Frames(2);
			UpgradeTool tool = Tool(t);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetSourceSpecific();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.NotNull(window, "the planner window is open");
			var selectors = new List<MaterialSelector>(window.GetComponentsInChildren<MaterialSelector>());
			t.Assert.Eq(2, selectors.Count, "two pickers: Replace and With");
			t.Log(MaterialPicker.RowsNeeded(selectors[0]) + " rows of materials shown");
			// What each frame draws: every scrollbar in the window and each picker's height, just
			// before the canvases render, for two seconds, across ten of the game's refreshes.
			var bars = new List<UnityEngine.UI.Scrollbar>(window.GetComponentsInChildren<UnityEngine.UI.Scrollbar>(true));
			var first = new Dictionary<UnityEngine.UI.Scrollbar, string>();
			var heights = new Dictionary<MaterialSelector, float>();
			int frames = 0;
			var changes = new List<string>();
			UnityEngine.Canvas.WillRenderCanvases sample = () =>
			{
				frames++;
				foreach (UnityEngine.UI.Scrollbar bar in bars)
				{
					if (bar == null)
						continue;
					string state = (bar.gameObject.activeInHierarchy ? "shown" : "hidden") + " size " + Mathf.Round(bar.size * 1000f) + " at " + Mathf.Round(bar.value * 1000f);
					if (!first.TryGetValue(bar, out string was))
						first[bar] = state;
					else if (was != state)
						changes.Add("frame " + frames + ": " + bar.transform.parent.name + "/" + bar.name + " " + was + " then " + state);
				}
				foreach (MaterialSelector selector in selectors)
				{
					if (selector == null)
						continue;
					float height = selector.ScrollRect.rectTransform().rect.height;
					if (!heights.TryGetValue(selector, out float h))
						heights[selector] = height;
					else if (h != height)
						changes.Add("frame " + frames + ": a picker was " + h + " tall, then " + height);
				}
			};
			UnityEngine.Canvas.willRenderCanvases += sample;
			t.Cleanup(() => UnityEngine.Canvas.willRenderCanvases -= sample);
			yield return t.WaitRealSeconds(2);
			UnityEngine.Canvas.willRenderCanvases -= sample;
			foreach (KeyValuePair<UnityEngine.UI.Scrollbar, string> bar in first)
				t.Log(bar.Key.transform.parent.name + "/" + bar.Key.name + ": " + bar.Value);
			foreach (string line in changes.GetRange(0, Mathf.Min(8, changes.Count)))
				t.Log(line);
			t.Assert.True(frames > 30, "frames were sampled (" + frames + ")");
			t.Assert.Eq(0, changes.Count, "nothing changed in " + frames + " frames");
			foreach (MaterialSelector selector in selectors)
				t.Assert.False(selector.Scrollbar != null && selector.Scrollbar.activeInHierarchy, "a picker has no scrollbar of its own");
			// The game's refresh itself, looked at in the same call: whatever order Unity runs the
			// frame's scripts in, its side bar size must not be left for any frame to draw.
			foreach (MaterialSelector selector in selectors)
			{
				float height = MaterialPicker.FitHeight(selector, Mathf.Max(MaterialPicker.RowsNeeded(selector), 1));
				selector.RefreshToggleContents();
				t.Assert.Eq(height, selector.ScrollRect.GetComponent<UnityEngine.UI.LayoutElement>().minHeight, "right after the game's refresh the picker keeps its height");
				t.Assert.False(selector.Scrollbar != null && selector.Scrollbar.activeSelf, "right after the game's refresh the picker has no scrollbar");
			}
			yield return t.Screenshot("pickers-eleven");
		}

		[OniTest("Upgrade Planner: the window keeps its height when families fold and tabs change", Timeout = 40), Layer(2)]
		public static IEnumerator FixedHeight(TestContext t)
		{
			UpgradeTool tool = Tool(t);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			float height = WindowHeight();
			float screen = PeterHan.PLib.UI.PDialog.GetParentObject().rectTransform().rect.height;
			t.Log("window " + height + " tall on a screen " + screen + " tall");
			t.Assert.True(Mathf.Abs(height - UpgradeDialog.WindowHeight()) < 2f, "the window is " + UpgradeDialog.WindowHeight() + " tall, 85% of the screen (" + height + ")");
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			GameObject anyLine = Find(window.transform, "MaterialAnyContent");
			t.Assert.NotNull(anyLine, "with Any, the Replace card says what Any does");
			LocText anyText = anyLine.GetComponentInChildren<LocText>();
			t.Log("Any label " + anyLine.rectTransform().rect.width + " wide, its text box " + anyText.rectTransform.rect.width + ", card " + Find(window.transform, "MaterialSourceCard").rectTransform().rect.width + "; text parent " + anyText.transform.parent.name + " anchors " + anyText.rectTransform.anchorMin + "-" + anyText.rectTransform.anchorMax);
			t.Assert.True(Inside(anyText.rectTransform, Find(window.transform, "MaterialSourceCard").rectTransform()), "the line wraps inside the Replace card");
			// What is drawn: the text's rendered bounds, every line of it, inside the card.
			Bounds drawn = anyText.textBounds;
			RectTransform card = Find(window.transform, "MaterialSourceCard").rectTransform();
			bool inside = true;
			foreach (Vector3 corner in new[] { drawn.min, drawn.max, new Vector3(drawn.min.x, drawn.max.y), new Vector3(drawn.max.x, drawn.min.y) })
			{
				Vector3 local = card.InverseTransformPoint(anyText.rectTransform.TransformPoint(corner));
				if (!card.rect.Contains(new Vector2(local.x, local.y)))
					inside = false;
			}
			t.Log("Any line drawn " + drawn.size + " in " + anyText.textInfo.lineCount + " lines; card " + card.rect.size);
			t.Assert.True(anyText.textInfo.lineCount >= 2, "the line wraps (" + anyText.textInfo.lineCount + " lines)");
			t.Assert.True(inside, "every line is drawn inside the Replace card");
			t.Assert.Eq(TMPro.TextAlignmentOptions.TopLeft, anyText.alignment, "the line is aligned left");
			t.Assert.Null(Find(window.transform, "MaterialSourceScroll"), "with Any, there is no picker in the Replace card");
			string[] steps = { "CollapseAll", "ExpandAll", "TabReplaceType", "TabChangeMaterial" };
			foreach (string step in steps)
			{
				KButton button = Find(GameObject.Find("UpgradePlannerDialog").transform, step)?.GetComponent<KButton>();
				t.Assert.NotNull(button, "the " + step + " button is there");
				button.SignalClick(KKeyCode.Mouse0);
				yield return t.WaitRealSeconds(1);
				t.Assert.Eq(height, WindowHeight(), "the window is as tall after " + step);
				if (step == "CollapseAll")
					yield return t.Screenshot("collapsed");
			}
		}

		private static float WindowHeight()
		{
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			return window != null ? window.rectTransform().rect.height : -1f;
		}

		/// <summary>The Materials and Buildings sides split the window evenly.</summary>
		private static void AssertEvenColumns(TestContext t, GameObject window, string when)
		{
			float materials = Find(window.transform, "MaterialsSide").rectTransform().rect.width;
			float buildings = Find(window.transform, "BuildingsSide").rectTransform().rect.width;
			t.Log("Materials side " + materials + " wide, Buildings side " + buildings + ", " + when);
			t.Assert.True(Mathf.Abs(materials - buildings) < 2f, "the two sides are as wide as each other " + when + " (" + materials + ", " + buildings + ")");
		}

		private static bool Inside(RectTransform inner, RectTransform outer)
		{
			var corners = new Vector3[4];
			inner.GetWorldCorners(corners);
			foreach (Vector3 corner in corners)
			{
				Vector3 local = outer.InverseTransformPoint(corner);
				Rect r = outer.rect;
				if (local.x < r.xMin - 1f || local.x > r.xMax + 1f || local.y < r.yMin - 1f || local.y > r.yMax + 1f)
					return false;
			}
			return true;
		}

		// ---- the drag ----

		private static IEnumerator Run(TestContext t, bool instant)
		{
			CellRect r = t.Rect(6, 1);
			int sandstoneA = r.Cell(1, 0), sandstoneB = r.Cell(2, 0), granite = r.Cell(3, 0), ladder = r.Cell(4, 0);
			t.World.Build("Tile", sandstoneA, element: SimHashes.SandStone);
			t.World.Build("Tile", sandstoneB, element: SimHashes.SandStone);
			t.World.Build("Tile", granite, element: SimHashes.Granite);
			t.World.Build("Ladder", ladder, element: SimHashes.SandStone);
			yield return t.Frames(2);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { FamilyOf("Tile") }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(instant);

			UpgradeTool tool = Tool(t);
			PlayerController.Instance.ActivateTool(tool);
			yield return t.Frames(2);
			Vector3 from = Grid.CellToPosCCC(r.Cell(0, 0), Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(r.Cell(5, 0), Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return null;
			tool.OnLeftClickDown(from);
			yield return null;
			tool.OnMouseMove(to);
			yield return null;
			tool.OnLeftClickUp(to);
			yield return t.Frames(3);

			// A finished replacement tile takes the old one's place a few frames after the drag.
			if (instant)
				yield return t.Frames(10);
			foreach (int cell in new[] { sandstoneA, sandstoneB })
			{
				string at = Cells.Describe(cell);
				t.Log("at " + at + ": building layer " + Describe(t.World.At(cell, ObjectLayer.Building)) + ", tile layer " + Describe(t.World.At(cell, ObjectLayer.FoundationTile)) + ", replacement layer " + Describe(t.World.At(cell, ObjectLayer.ReplacementTile)));
				if (instant)
				{
					GameObject built = t.World.At(cell, ObjectLayer.FoundationTile);
					t.Assert.NotNull(built?.GetComponent<BuildingComplete>(), "a finished tile at " + at);
					t.Assert.Eq(SimHashes.Granite, built.GetComponent<PrimaryElement>().ElementID, "the tile at " + at + " is granite");
					t.Assert.Null(t.World.At(cell, ObjectLayer.ReplacementTile), "nothing pending at " + at);
				}
				else
				{
					GameObject order = t.World.At(cell, ObjectLayer.ReplacementTile);
					t.Assert.NotNull(order?.GetComponent<Constructable>(), "a replacement order at " + at);
					t.Assert.Eq(SimHashes.Granite, order.GetComponent<PrimaryElement>().ElementID, "the order at " + at + " is for granite");
					t.Assert.Eq(SimHashes.SandStone, t.World.At(cell, ObjectLayer.FoundationTile).GetComponent<PrimaryElement>().ElementID, "the sandstone tile at " + at + " stays until the order completes");
				}
			}
			t.Assert.Null(t.World.At(granite, ObjectLayer.ReplacementTile), "no order on the tile that is granite already");
			t.Assert.Eq(SimHashes.Granite, t.World.At(granite, ObjectLayer.FoundationTile).GetComponent<PrimaryElement>().ElementID, "the granite tile is as it was");
			t.Assert.Null(t.World.At(ladder, ObjectLayer.ReplacementLadder), "no order on the ladder: ladders are not picked");
			t.Assert.Eq(SimHashes.SandStone, t.World.At(ladder, ObjectLayer.Building).GetComponent<PrimaryElement>().ElementID, "the ladder is still sandstone");
		}

		// ---- helpers ----

		private static string Describe(GameObject go)
		{
			if (go == null)
				return "nothing";
			PrimaryElement element = go.GetComponent<PrimaryElement>();
			return go.name + (element != null ? " in " + element.ElementID : "") + (go.GetComponent<Constructable>() != null ? " (order)" : "");
		}

		private static Family FamilyOf(string prefabId)
		{
			return Families.Of(Assets.GetBuildingDef(prefabId));
		}

		private static bool Offered(Family family)
		{
			foreach (Category category in MaterialSwap.Categories(false))
				if (category.Families.Contains(family))
					return true;
			return false;
		}

		private static UpgradeTool Tool(TestContext t)
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
			return tool;
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
