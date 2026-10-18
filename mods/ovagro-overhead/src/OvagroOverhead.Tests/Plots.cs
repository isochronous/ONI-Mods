using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// Ovagro in farm plots turned upside down (a Farm Tile or Hydroponic Farm rotated with FlipV): the
	/// order's preview hangs below the tile, a delivered seed grows the hanging plant below it, Copy
	/// Settings carries an Ovagro order between such plots, and the planter side screen lets the
	/// player pick Ovagro for one. Upright plots still grow the standing Ovagro. Every test works in
	/// the Duplicants' box; a downward plot sits in the box's top row, against its built ceiling,
	/// unless a Duplicant has to reach it from the floor.
	/// </summary>
	public static class Plots
	{
		/// <summary>The row under the box's ceiling, where a downward plot hangs its plant into the box.</summary>
		private static int TopRow(CellRect inside)
		{
			return inside.H - 1;
		}

		/// <summary>The plot has an order for the Ovagro seed and shows the hanging preview, valid, in the cell below it.</summary>
		private static void AssertOrderedHanging(TestContext t, PlantablePlot plot, string which)
		{
			int plotCell = Grid.PosToCell(plot);
			t.Assert.Eq(CeilingForm.SeedTag, plot.requestedEntityTag, which + " asks for the Ovagro seed");
			t.Assert.NotNull(plot.GetActiveRequest, which + " has a delivery errand for it");
			EntityPreview preview = Kit.PreviewOf(plot);
			t.Assert.NotNull(preview, which + " shows a plant preview");
			t.Assert.Eq(CeilingForm.PreviewId, preview.gameObject.PrefabID().Name, which + " previews the hanging Ovagro");
			t.Assert.Eq(Cells.Describe(Grid.CellBelow(plotCell)), Cells.Describe(Grid.PosToCell(preview)), which + "'s preview hangs in the cell below it");
			t.Assert.True(preview.Valid, which + "'s preview is valid (the two cells below are open)");
		}

		/// <summary>The plot holds a hanging Ovagro in the two cells below it.</summary>
		private static GameObject AssertHangingOccupant(TestContext t, PlantablePlot plot, string which)
		{
			GameObject plant = plot.Occupant;
			int below = Grid.CellBelow(Grid.PosToCell(plot));
			t.Assert.True(Kit.Is(plant, CeilingForm.PlantTag), which + " grows the ceiling Ovagro (found " + (plant != null ? plant.PrefabID().Name : "nothing") + ")");
			t.Assert.Eq(Cells.Describe(below), Cells.Describe(Grid.PosToCell(plant)), which + "'s plant has its origin in the cell below");
			t.Assert.Eq(plant, Kit.PlantAt(below), which + "'s plant is registered in the cell below");
			t.Assert.Eq(plant, Kit.PlantAt(Grid.CellBelow(below)), which + "'s plant hangs into the next cell down");
			t.Assert.True(plot.plant != null && plot.plant.gameObject == plant, which + " keeps the plant as its occupant");
			t.Assert.True(plant.GetComponent<ReceptacleMonitor>().Replanted, "the plant knows it is in a plot");
			KBatchedAnimController anim = plant.GetComponent<KBatchedAnimController>();
			t.Assert.Eq(180f, anim.Rotation, "the plant's art is turned over");
			t.Assert.True(anim.FlipX, "and mirrored back");
			t.Assert.False(plant.HasTag(GameTags.Uprooted), "the plant is rooted");
			return plant;
		}

		/// <summary>Yield it: waits until the planted Ovagro has played its growth through.</summary>
		private static IEnumerator WaitGrown(TestContext t, PlantablePlot plot, GameObject plant)
		{
			VineMother.Instance smi = plant.GetSMI<VineMother.Instance>();
			yield return t.WaitUntil(() => Kit.IsGrown(smi), 60, "the planted Ovagro to grow");
			yield return t.WaitGameSeconds(3);
			t.Log("grown; wilting " + smi.IsWilting + "; plant status: " + Kit.Statuses(plant) + "; plot status: " + Kit.Statuses(plot.gameObject));
		}

		/// <summary>
		/// The first Plots test reloads the fixture (FreshSave): Persistence runs before this class and
		/// its second test leaves the round trip save loaded, so this puts the colony back as every
		/// other test of the suite finds it. A filtered run that starts here loads nothing again.
		/// </summary>
		[OniTest("PLOT: a Duplicant plants an Ovagro seed in an upside-down Hydroponic Farm and it hangs below, irrigated", Timeout = 150, Boxed = true, FreshSave = true), Layer(0)]
		public static IEnumerator HydroponicFarmDownward(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			// Low enough that a Duplicant on the floor reaches it from below (the farm's inverted offsets).
			int plotCell = inside.Cell(10, 3);
			PlantablePlot plot = Kit.Plot(t, "HydroponicFarm", plotCell, Orientation.FlipV);
			t.World.Build("LiquidConduit", plotCell);
			GameObject seed = Kit.Seed(t, inside.Cell(16, 0));
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(SingleEntityReceptacle.ReceptacleDirection.Bottom, plot.Direction, "the flipped farm faces down");

			plot.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			AssertOrderedHanging(t, plot, "the farm");
			Kit.AssertSeedPrefabIsVanilla(t, "after the order");

			t.Dupes.Spawn(1, inside.Cell(4, 0));
			yield return t.WaitUntil(() => plot.Occupant != null, 90, "a Duplicant to deliver the seed to the farm");
			yield return t.WaitGameSeconds(2);
			GameObject plant = AssertHangingOccupant(t, plot, "the farm");
			t.Assert.True(seed == null, "the delivered seed was used up");
			t.Assert.Null(Kit.PreviewOf(plot), "the preview is gone");
			t.Assert.True(plot.GetComponent<RequireInputs>().RequiresInputConduit, "the farm asks for its water pipe, as for any irrigated plant");
			Kit.AssertSeedPrefabIsVanilla(t, "after the delivery");

			Kit.Water(plot, 20f);
			yield return WaitGrown(t, plot, plant);
			t.Assert.True(plant != null && plot.Occupant == plant && !plant.HasTag(GameTags.Uprooted), "the grown plant is still in the farm, rooted");
			yield return t.LookAt(plotCell, 4f);
			yield return t.Screenshot("hydroponic-downward");
		}

		/// <summary>
		/// The Farm Tile sits right under the box's built ceiling. Without the mod the game would
		/// preview the standing Ovagro above it, in the ceiling, and refuse the order.
		/// </summary>
		[OniTest("PLOT: an upside-down Farm Tile under a ceiling takes an Ovagro order and grows the hanging Ovagro below", Timeout = 90, Boxed = true), Layer(0)]
		public static IEnumerator FarmTileDownward(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int plotCell = inside.Cell(10, TopRow(inside));
			PlantablePlot plot = Kit.Plot(t, "FarmTile", plotCell, Orientation.FlipV);
			GameObject seed = Kit.Seed(t, inside.Cell(16, 0));
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(SingleEntityReceptacle.ReceptacleDirection.Bottom, plot.Direction, "the flipped tile faces down");
			t.Assert.True(Grid.Solid[Grid.CellAbove(plotCell)], "the cell above the tile is the solid ceiling");

			plot.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			AssertOrderedHanging(t, plot, "the tile");

			plot.ForceDeposit(seed);
			yield return t.WaitGameSeconds(2);
			GameObject plant = AssertHangingOccupant(t, plot, "the tile");
			t.Assert.True(seed == null, "the seed was used up");
			t.Assert.Null(plot.GetActiveRequest, "the delivery errand is done");
			Kit.AssertSeedPrefabIsVanilla(t, "after planting");

			Kit.Water(plot, 20f);
			yield return WaitGrown(t, plot, plant);
			t.Assert.True(plant != null && plot.Occupant == plant && !plant.HasTag(GameTags.Uprooted), "the grown plant is still in the tile, rooted");
			yield return t.LookAt(plotCell, 4f);
			yield return t.Screenshot("farm-tile-downward");
		}

		[OniTest("PLOT: an upright Farm Tile still grows the standing Ovagro above it", Timeout = 60, Boxed = true), Layer(0)]
		public static IEnumerator FarmTileUpright(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int plotCell = inside.Cell(24, 0);
			PlantablePlot plot = Kit.Plot(t, "FarmTile", plotCell, Orientation.Neutral);
			GameObject seed = Kit.Seed(t, inside.Cell(16, 0));
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(SingleEntityReceptacle.ReceptacleDirection.Top, plot.Direction, "the tile faces up");

			plot.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			int above = Grid.CellAbove(plotCell);
			EntityPreview preview = Kit.PreviewOf(plot);
			t.Assert.NotNull(plot.GetActiveRequest, "the tile has a delivery errand");
			t.Assert.Eq(VineMotherConfig.ID + "_preview", Kit.PreviewName(plot), "the tile previews the standing Ovagro");
			t.Assert.Eq(Cells.Describe(above), Cells.Describe(Grid.PosToCell(preview)), "the preview stands in the cell above");
			t.Assert.True(preview.Valid, "the preview is valid");

			plot.ForceDeposit(seed);
			yield return t.WaitGameSeconds(2);
			GameObject plant = plot.Occupant;
			t.Assert.True(Kit.Is(plant, CeilingForm.VanillaPlantTag), "the tile grows the vanilla Ovagro (found " + (plant != null ? plant.PrefabID().Name : "nothing") + ")");
			t.Assert.Eq(Cells.Describe(above), Cells.Describe(Grid.PosToCell(plant)), "the plant stands in the cell above");
			t.Assert.Eq(plant, Kit.PlantAt(Grid.CellAbove(above)), "and into the next cell up");
			t.Assert.Eq(0f, plant.GetComponent<KBatchedAnimController>().Rotation, "its art is upright");
			Kit.AssertSeedPrefabIsVanilla(t, "after planting");
		}

		/// <summary>
		/// The counter path of the preview: a downward tile with a built tile two cells below it has no
		/// room for the hanging plant, so the order is refused, as the game refuses any plant that
		/// does not fit.
		/// </summary>
		[OniTest("PLOT: an upside-down Farm Tile with no room below for the hanging Ovagro gets no order", Timeout = 60, Boxed = true), Layer(0)]
		public static IEnumerator FarmTileNoRoomBelow(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int plotCell = inside.Cell(16, TopRow(inside));
			PlantablePlot plot = Kit.Plot(t, "FarmTile", plotCell, Orientation.FlipV);
			int blocked = Grid.CellBelow(Grid.CellBelow(plotCell));
			t.World.Build("Tile", blocked);
			yield return t.WaitGameSeconds(2);
			t.Assert.True(Grid.Solid[blocked], "the second cell below the tile is solid");

			plot.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			t.Assert.Null(plot.GetActiveRequest, "no delivery errand");
			t.Assert.False(plot.requestedEntityTag.IsValid, "no seed requested (found " + plot.requestedEntityTag + ")");
			t.Assert.Null(Kit.PreviewOf(plot), "no preview left behind");
			Kit.AssertSeedPrefabIsVanilla(t, "after the refused order");
		}

		/// <summary>Copy Settings as the tool applies it to each building dragged over.</summary>
		private static void Copy(TestContext t, PlantablePlot from, PlantablePlot to, string what)
		{
			bool copied = CopyBuildingSettings.ApplyCopy(to.GetComponent<KPrefabID>(), from.gameObject, from.GetComponent<KPrefabID>(), from.GetComponent<CopyBuildingSettings>());
			t.Assert.True(copied, "Copy Settings applies " + what);
		}

		[OniTest("PLOT: Copy Settings carries an Ovagro order between upside-down Farm Tiles, and onto upright ones as the standing Ovagro", Timeout = 90, Boxed = true), Layer(0)]
		public static IEnumerator CopySettings(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int top = TopRow(inside);
			PlantablePlot planted = Kit.Plot(t, "FarmTile", inside.Cell(6, top), Orientation.FlipV);
			PlantablePlot emptyDown = Kit.Plot(t, "FarmTile", inside.Cell(10, top), Orientation.FlipV);
			PlantablePlot orderedDown = Kit.Plot(t, "FarmTile", inside.Cell(14, top), Orientation.FlipV);
			PlantablePlot secondDown = Kit.Plot(t, "FarmTile", inside.Cell(18, top), Orientation.FlipV);
			PlantablePlot thirdDown = Kit.Plot(t, "FarmTile", inside.Cell(30, top), Orientation.FlipV);
			PlantablePlot emptyUp = Kit.Plot(t, "FarmTile", inside.Cell(24, 0), Orientation.Neutral);
			PlantablePlot orderedUp = Kit.Plot(t, "FarmTile", inside.Cell(28, 0), Orientation.Neutral);
			GameObject seed = Kit.Seed(t, inside.Cell(16, 0));
			yield return t.WaitGameSeconds(2);

			planted.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			planted.ForceDeposit(seed);
			orderedDown.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			orderedUp.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(2);
			t.Assert.True(Kit.Is(planted.Occupant, CeilingForm.PlantTag), "the source tile holds a hanging Ovagro");
			AssertOrderedHanging(t, orderedDown, "the ordered downward tile");

			Copy(t, planted, emptyDown, "from the planted downward tile to an empty one");
			yield return t.WaitGameSeconds(1);
			AssertOrderedHanging(t, emptyDown, "the tile copied from the planted one");
			Kit.AssertSeedPrefabIsVanilla(t, "after copying from the planted tile");

			Copy(t, orderedDown, secondDown, "from the ordered downward tile to an empty one");
			yield return t.WaitGameSeconds(1);
			AssertOrderedHanging(t, secondDown, "the tile copied from the ordered one");

			Copy(t, orderedUp, thirdDown, "from an upright tile ordered with Ovagro to a downward one");
			yield return t.WaitGameSeconds(1);
			AssertOrderedHanging(t, thirdDown, "the downward tile copied from the upright one");

			// Counter path: the target faces up, so nothing is swapped and it orders the standing Ovagro.
			Copy(t, planted, emptyUp, "from the planted downward tile to an upright one");
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(CeilingForm.SeedTag, emptyUp.requestedEntityTag, "the upright tile asks for the Ovagro seed");
			t.Assert.NotNull(emptyUp.GetActiveRequest, "the upright tile has a delivery errand");
			t.Assert.Eq(VineMotherConfig.ID + "_preview", Kit.PreviewName(emptyUp), "the upright tile previews the standing Ovagro");
			t.Assert.Eq(Cells.Describe(Grid.CellAbove(Grid.PosToCell(emptyUp))), Cells.Describe(Grid.PosToCell(Kit.PreviewOf(emptyUp))), "above the upright tile");
			Kit.AssertSeedPrefabIsVanilla(t, "after every copy");
		}

		private static PlanterSideScreen OpenPlanterScreen()
		{
			return Resources.FindObjectsOfTypeAll<PlanterSideScreen>().FirstOrDefault(s => s.gameObject.activeInHierarchy);
		}

		/// <summary>The seed toggle the screen built for the seed, by the tag it stores with each toggle.</summary>
		private static ReceptacleToggle ToggleFor(PlanterSideScreen screen, Tag seed)
		{
			var map = Traverse.Create(screen).Field("depositObjectMap").GetValue<IDictionary>();
			foreach (DictionaryEntry entry in map)
				if (Traverse.Create(entry.Value).Field("tag").GetValue<Tag>() == seed)
					return (ReceptacleToggle)entry.Key;
			return null;
		}

		/// <summary>A left click on a widget, as Unity's input module delivers one.</summary>
		private static void Click(GameObject widget)
		{
			var data = new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = PointerEventData.InputButton.Left };
			ExecuteEvents.Execute(widget, data, ExecuteEvents.pointerClickHandler);
		}

		/// <summary>The seed list scrolled until the toggle shows, as a player scrolls to it, for the picture.</summary>
		private static void ScrollTo(Component toggle)
		{
			UnityEngine.UI.ScrollRect scroll = toggle.GetComponentInParent<UnityEngine.UI.ScrollRect>();
			if (scroll == null)
				return;
			RectTransform viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
			var item = (RectTransform)toggle.transform;
			var v = new Vector3[4];
			var i = new Vector3[4];
			for (float p = 1f; p >= 0f; p -= 0.02f)
			{
				scroll.verticalNormalizedPosition = p;
				Canvas.ForceUpdateCanvases();
				viewport.GetWorldCorners(v);
				item.GetWorldCorners(i);
				if (i[0].y >= v[0].y && i[1].y <= v[1].y)
					return;
			}
		}

		private static string Texts(Component panel)
		{
			var texts = new List<string>();
			foreach (LocText text in panel.GetComponentsInChildren<LocText>())
				texts.Add(text.text);
			return string.Join(" | ", texts.ToArray());
		}

		/// <summary>
		/// The real planter side screen on a downward Farm Tile. A seed toggle is greyed out when the
		/// seed's direction does not match the plot's or none is in the colony; Mealwood (a floor seed
		/// that is not Ovagro) stays greyed out on the downward tile, the counter path.
		/// </summary>
		[OniTest("PLOT: the planter side screen of an upside-down Farm Tile lets the player pick Ovagro and order it", Timeout = 90, Boxed = true), Layer(2)]
		public static IEnumerator SideScreen(TestContext t)
		{
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int plotCell = inside.Cell(12, TopRow(inside));
			PlantablePlot plot = Kit.Plot(t, "FarmTile", plotCell, Orientation.FlipV);
			Kit.Seed(t, inside.Cell(16, 0));
			Kit.Spawn(t, "BasicSingleHarvestPlantSeed", inside.Cell(18, 0), Grid.SceneLayer.Ore);
			// The colony counts an item once a Duplicant can reach it; this one cannot reach the tile
			// under the ceiling, so the order the screen places stays an order.
			t.Dupes.Spawn(1, inside.Cell(4, 0));
			WorldInventory inventory = plot.GetMyWorld().worldInventory;
			yield return t.WaitUntil(() => inventory.GetCountWithAdditionalTag(CeilingForm.SeedTag, Tag.Invalid, true) > 0 && inventory.GetCountWithAdditionalTag(TagManager.Create("BasicSingleHarvestPlantSeed"), Tag.Invalid, true) > 0, 30, "the colony to count the two seeds");
			yield return t.WaitGameSeconds(1);

			t.Cleanup(() => { if (SelectTool.Instance != null) SelectTool.Instance.Select(null); });
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			SelectTool.Instance.Select(plot.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
			PlanterSideScreen screen = OpenPlanterScreen();
			t.Assert.NotNull(screen, "the planter side screen is open on the tile");

			ReceptacleToggle ovagro = ToggleFor(screen, CeilingForm.SeedTag);
			ReceptacleToggle mealwood = ToggleFor(screen, TagManager.Create("BasicSingleHarvestPlantSeed"));
			t.Assert.NotNull(ovagro, "the screen lists the Ovagro seed");
			t.Assert.NotNull(mealwood, "the screen lists the Mealwood seed");
			t.Log("toggle states: Ovagro " + ovagro.toggle.CurrentState + ", Mealwood " + mealwood.toggle.CurrentState + " (0 enabled, 1 selected, 2 greyed out, 3 greyed out and selected)");
			t.Assert.Eq(0, ovagro.toggle.CurrentState, "the Ovagro toggle is enabled, not greyed out");
			t.Assert.Eq(2, mealwood.toggle.CurrentState, "the Mealwood toggle stays greyed out on a downward tile");

			Click(ovagro.toggle.gameObject);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1, ovagro.toggle.CurrentState, "the Ovagro toggle is selected and enabled");
			KButton order = Traverse.Create(screen).Field("requestSelectedEntityBtn").GetValue<KButton>();
			t.Assert.True(order.isInteractable, "the order button can be pressed");
			string description = Traverse.Create(screen).Field("descriptionLabel").GetValue<LocText>().text;
			string requirements = Texts(screen.RequirementsDescriptorPanel);
			t.Log("description: " + description);
			t.Log("requirements: " + requirements);
			t.Assert.False(description.Contains((string)STRINGS.UI.UISIDESCREENS.PLANTERSIDESCREEN.ROTATION_NEED_FLOOR), "the description does not ask for an upward plot");
			t.Assert.True(requirements.Contains((string)STRINGS.UI.GAMEOBJECTEFFECTS.SEED_REQUIREMENT_CEILING), "the requirements list the downward plot");
			t.Assert.Eq(CeilingForm.PreviewId, Kit.PreviewName(plot), "the selection previews the hanging Ovagro");
			t.Assert.Eq(Cells.Describe(Grid.CellBelow(plotCell)), Cells.Describe(Grid.PosToCell(Kit.PreviewOf(plot))), "below the tile");
			Kit.AssertSeedPrefabIsVanilla(t, "while the screen is open");
			ScrollTo(ovagro);
			yield return t.WaitGameSeconds(0.5);
			yield return t.LookAt(plotCell, 6f);
			yield return t.Screenshot("side-screen");

			order.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitGameSeconds(1);
			AssertOrderedHanging(t, plot, "the tile ordered from the screen");
			Kit.AssertSeedPrefabIsVanilla(t, "after the order");
		}

		/// <summary>
		/// The vines of an Ovagro in an upside-down Hydroponic Farm grow along the tiles beside the
		/// farm, as ceiling vines, the way a wild ceiling Ovagro's grow along the rock.
		/// </summary>
		[OniTest("PLOT: an Ovagro in an upside-down Hydroponic Farm grows its vines along the tiles beside it", Timeout = 200, Boxed = true), Layer(0)]
		public static IEnumerator VinesFromDownwardFarm(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int top = TopRow(inside);
			const int fx = 20;
			int plotCell = inside.Cell(fx, top);
			for (int x = fx - 4; x <= fx + 4; x++)
				if (x != fx)
					t.World.Build("Tile", inside.Cell(x, top));
			PlantablePlot plot = Kit.Plot(t, "HydroponicFarm", plotCell, Orientation.FlipV);
			t.World.Build("LiquidConduit", plotCell);
			GameObject seed = Kit.Seed(t, inside.Cell(fx + 6, 0));
			yield return t.WaitGameSeconds(2);
			plot.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			plot.ForceDeposit(seed);
			yield return t.WaitGameSeconds(1);
			Kit.Water(plot, 20f);
			GameObject plant = AssertHangingOccupant(t, plot, "the farm");

			yield return Kit.GrowMother(t, plant);
			VineMother.Instance smi = plant.GetSMI<VineMother.Instance>();
			yield return Kit.GrowChains(t, smi, 3, 3, 90);
			yield return t.WaitGameSeconds(3);
			List<VineBranch.Instance> left = Kit.Chain(smi.LeftBranch);
			List<VineBranch.Instance> right = Kit.Chain(smi.RightBranch);
			t.Log("left: " + Kit.Describe(left) + "; right: " + Kit.Describe(right));
			t.Assert.Eq(VineBranch.Shape.Top, left[0].RootShape, "the first left branch takes the tiles as a ceiling");
			t.Assert.Eq(VineBranch.Shape.Top, right[0].RootShape, "the first right branch takes the tiles as a ceiling");
			for (int i = 0; i < 3; i++)
			{
				foreach (KeyValuePair<string, VineBranch.Instance> side in new[] { new KeyValuePair<string, VineBranch.Instance>("left", left[i]), new KeyValuePair<string, VineBranch.Instance>("right", right[i]) })
				{
					int expected = inside.Cell(side.Key == "left" ? fx - 1 - i : fx + 1 + i, top - 1);
					GameObject go = side.Value.gameObject;
					t.Assert.Eq(Cells.Describe(expected), Cells.Describe(Grid.PosToCell(go)), side.Key + " branch " + (i + 1) + " is under the tiles");
					t.Assert.Eq(VineBranch.Shape.Top, side.Value.MyShape, side.Key + " branch " + (i + 1) + " hugs the tiles above it");
					t.Assert.Eq(CeilingForm.BranchTag, go.PrefabID(), side.Key + " branch " + (i + 1) + " is a ceiling vine");
					t.Assert.False(go.HasTag(GameTags.Uprooted), side.Key + " branch " + (i + 1) + " is rooted");
				}
			}
			yield return t.LookAt(plotCell, 5f);
			yield return t.Screenshot("vines-from-downward-farm");
		}
	}
}
