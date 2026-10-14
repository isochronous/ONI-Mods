using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// All buildings, and settings kept on upgrade, with Blueprints Expanded (the planner's
	/// dependency; run with -Steam 3468585385). A storage bin, which the game does not let you
	/// build over, is rebuilt in granite through its reconstruct and keeps its capacity setting.
	/// </summary>
	public static class AllBuildings
	{
		private const float Capacity = 500f;

		[OniTest("Upgrade Planner: All buildings offers what cannot be built over, grouped as the build menu", Timeout = 20), Layer(1)]
		public static IEnumerator Offered(TestContext t)
		{
			RequireBlueprints(t);
			Family locker = FamilyOf("StorageLocker");
			MaterialSwap.Refresh();
			t.Assert.False(Offered(MaterialSwap.Categories(false), locker), "without All buildings, the storage bin is not offered");
			List<Category> all = MaterialSwap.Categories(true);
			t.Assert.True(Offered(all, locker), "with All buildings, the storage bin is offered");
			t.Assert.True(Offered(all, FamilyOf("Tile")), "with All buildings, tiles are still offered");
			t.Log("categories: " + string.Join(", ", all.ConvertAll(c => c.Name)));
			t.Assert.True(all.Count > 5, "grouped as the build menu (" + all.Count + " categories)");
			t.Assert.False(MaterialSwap.Rebuilds(Assets.GetBuildingDef("Tile")), "a tile is built over");
			t.Assert.True(MaterialSwap.Rebuilds(Assets.GetBuildingDef("StorageLocker")), "a storage bin is rebuilt");
			// Turning All buildings on leaves what it adds unpicked.
			var plan = new UpgradePlan();
			plan.ChangeMaterial.SetAllBuildings(true);
			t.Assert.True(plan.ChangeMaterial.IsPicked("Tile"), "tiles, offered before, stay picked");
			t.Assert.False(plan.ChangeMaterial.IsPicked("StorageLocker"), "the storage bin, newly offered, starts unpicked");
			plan.Clear();
			t.Assert.False(plan.ChangeMaterial.AllBuildings, "Clear turns All buildings off with the rest of the tab's choices");
			yield break;
		}

		[OniTest("Upgrade Planner: a replacement order carries the old building's settings", Timeout = 40), Layer(1)]
		public static IEnumerator OrderCarriesSettings(TestContext t)
		{
			RequireBlueprints(t);
			CellRect r = t.Rect(6, 3);
			t.World.Build("Tile", r.Cell(1, 0), element: SimHashes.SandStone);
			GameObject locker = BuildLocker(t, r, 3);
			yield return t.Frames(2);
			// A tile under replacement: its order is a building under construction, with a carrier.
			yield return Drag(t, r, "Tile", instant: false, allBuildings: false);
			GameObject order = t.World.At(r.Cell(1, 0), ObjectLayer.ReplacementTile);
			t.Assert.NotNull(order, "a replacement order on the tile");
			int copied = BlueprintsCompat.CarrySettings(locker, order);
			Dictionary<string, string> carried = Carried(order);
			t.Log("copied " + copied + ": " + string.Join(", ", carried.Keys));
			t.Assert.True(copied > 0, "settings were copied");
			t.Assert.True(carried.ContainsKey("IUserControlledCapacity"), "the capacity is in the order's carrier");
		}

		[OniTest("Upgrade Planner: All buildings marks a storage bin for reconstruction in granite", Timeout = 40), Layer(1)]
		public static IEnumerator RebuildOrder(TestContext t)
		{
			RequireBlueprints(t);
			CellRect r = t.Rect(6, 3);
			GameObject locker = BuildLocker(t, r, 2);
			yield return t.Frames(2);
			yield return Drag(t, r, "StorageLocker", instant: false, allBuildings: true);
			Reconstructable reconstructable = locker.GetComponent<Reconstructable>();
			t.Assert.True(reconstructable.ReconstructRequested, "a reconstruct is asked for");
			t.Assert.True(locker.GetComponent<Deconstructable>().IsMarkedForDeconstruction(), "the bin is marked for deconstruction");
			Tag[] materials = AccessTools.FieldRefAccess<Reconstructable, Tag[]>("selectedElementsTags")(reconstructable);
			t.Assert.Eq(SimHashes.Granite.CreateTag(), materials[0], "it is to be rebuilt in granite");
			// A second drag leaves it marked (asking again would cancel the reconstruct).
			yield return Drag(t, r, "StorageLocker", instant: false, allBuildings: true);
			t.Assert.True(reconstructable.ReconstructRequested, "still asked for after a second drag");
		}

		[OniTest("Upgrade Planner: with instant build, a storage bin is rebuilt in granite and keeps its capacity", Timeout = 40), Layer(1)]
		public static IEnumerator RebuildInstant(TestContext t)
		{
			RequireBlueprints(t);
			CellRect r = t.Rect(6, 3);
			BuildLocker(t, r, 2);
			yield return t.Frames(2);
			yield return Drag(t, r, "StorageLocker", instant: true, allBuildings: true);
			// Deconstruction, then the new building next frame, then its settings a little after.
			yield return t.WaitUntil(() => Granite(t.World.At(r.Cell(2, 1), ObjectLayer.Building)), 5, "the bin rebuilt in granite");
			yield return t.WaitRealSeconds(1);
			GameObject rebuilt = t.World.At(r.Cell(2, 1), ObjectLayer.Building);
			t.Assert.NotNull(rebuilt?.GetComponent<BuildingComplete>(), "a finished storage bin");
			t.Assert.Eq(SimHashes.Granite, rebuilt.GetComponent<PrimaryElement>().ElementID, "it is granite");
			t.Assert.Eq(Capacity, rebuilt.GetComponent<IUserControlledCapacity>().UserMaxCapacity, "it keeps its capacity");
		}

		[OniTest("Upgrade Planner: with instant build, every storage bin of a drag is rebuilt, none left as an order", Timeout = 40), Layer(1)]
		public static IEnumerator RebuildInstantMany(TestContext t)
		{
			RequireBlueprints(t);
			// The game places each bin's new order a frame after its deconstruction, through the
			// same scheduler as the planner's finish, which keeps no order among entries due together.
			const int Count = 12;
			CellRect r = t.Rect(Count + 2, 3);
			for (int x = 1; x <= Count; x++)
				BuildLocker(t, r, x);
			yield return t.Frames(2);
			yield return Drag(t, r, "StorageLocker", instant: true, allBuildings: true);
			yield return t.WaitRealSeconds(2);
			var left = new List<int>();
			for (int x = 1; x <= Count; x++)
				if (!Granite(t.World.At(r.Cell(x, 1), ObjectLayer.Building)))
					left.Add(x);
			t.Assert.Eq(0, left.Count, "bins not rebuilt in granite, by column: " + string.Join(", ", left));
		}

		[OniTest("Upgrade Planner: with instant build, a tempshift plate is rebuilt with no error from the sim", Timeout = 40), Layer(1)]
		public static IEnumerator RebuildSolidInstant(TestContext t)
		{
			RequireBlueprints(t);
			// A building that occupies its cell in the sim, rebuilt on the spot; any error the game
			// logs in the seconds after fails the test.
			CellRect r = t.Rect(4, 2);
			int cell = r.Cell(1, 0);
			ObjectLayer layer = Assets.GetBuildingDef("ThermalBlock").ObjectLayer;
			t.World.Build("ThermalBlock", cell, element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(1);
			yield return Drag(t, r, "ThermalBlock", instant: true, allBuildings: true);
			GameObject now = t.World.At(cell, layer);
			t.Log("after the drag, on " + layer + ": " + (now != null ? now.name + " " + now.GetComponent<PrimaryElement>()?.ElementID : "nothing"));
			yield return t.WaitUntil(() => Granite(t.World.At(cell, layer)), 10, "the tempshift plate rebuilt in granite");
			yield return t.WaitGameSeconds(3);
		}

		[OniTest("Upgrade Planner: with instant build, a carpeted tile is built over with no error from the sim", Timeout = 40), Layer(1)]
		public static IEnumerator CarpetInstant(TestContext t)
		{
			RequireBlueprints(t);
			// A tile the game builds over, of an element and an item (reed fiber): its order finishes
			// from a sim callback, which must still find the materials the planner gave it.
			BuildingDef def = Assets.GetBuildingDef("CarpetTile");
			t.Log("carpet ingredients: " + string.Join(", ", def.CraftRecipe.Ingredients.ConvertAll(i => i.tag.Name + " " + i.amount)));
			CellRect r = t.Rect(4, 2);
			int cell = r.Cell(1, 0);
			var built = new List<Tag>();
			foreach (Recipe.Ingredient ingredient in def.CraftRecipe.Ingredients)
				built.Add(ingredient.tag == GameTags.BuildableRaw || Materials.Valid(ingredient.tag).Contains(SimHashes.SandStone.CreateTag()) ? SimHashes.SandStone.CreateTag() : Materials.Valid(ingredient.tag).OrderBy(tag => tag.Name).First());
			t.Log("built of " + string.Join(", ", built.ConvertAll(tag => tag.Name)));
			def.Build(cell, Orientation.Neutral, null, built, 293f, playsound: false, timeBuilt: GameClock.Instance.GetTime());
			yield return t.WaitGameSeconds(1);
			yield return Drag(t, r, "CarpetTile", instant: true, allBuildings: true);
			yield return t.WaitUntil(() => Granite(t.World.At(cell, def.ObjectLayer)), 10, "the carpeted tile built over in granite");
			yield return t.WaitGameSeconds(3);
		}

		[OniTest("Upgrade Planner: with instant build, a wide hydroponic tile is rebuilt, not lost", Timeout = 40), Layer(1)]
		public static IEnumerator WideTileRebuilt(TestContext t)
		{
			RequireBlueprints(t);
			// The game places a rebuilt building's order a frame after taking the old one down; for
			// a 3-wide tile its cells are not clear by then, and the game placed nothing.
			BuildingDef def = Assets.GetBuildingDef("WideFarmTile");
			List<Tag> materials = BuildingSettings.Materials(def);
			Tag to = BuildingSettings.Target(def).Value;
			CellRect r = t.Rect(7, 3);
			for (int x = 0; x < r.W; x++)
				t.World.Build("Tile", r.Cell(x, 0), element: SimHashes.Granite);
			yield return t.WaitGameSeconds(1);
			int cell = r.Cell(3, 1);
			def.Build(cell, Orientation.Neutral, null, materials, 293f, playsound: false, timeBuilt: GameClock.Instance.GetTime());
			yield return t.WaitGameSeconds(1);
			yield return Drag(t, r, "WideFarmTile", instant: true, allBuildings: true, oldElement: ElementLoader.GetElement(materials[0]).id, newElement: ElementLoader.GetElement(to).id);
			yield return t.WaitUntil(() => Made(t.World.At(cell, def.ObjectLayer), ElementLoader.GetElement(to).id), 10, "the wide tile rebuilt in " + to.Name);
		}

		[OniTest("Upgrade Planner: All buildings leaves out the mini-pod, which the game will not deconstruct", Timeout = 20), Layer(0)]
		public static IEnumerator MiniPodLeftOut(TestContext t)
		{
			RequireBlueprints(t);
			MaterialSwap.Refresh();
			Family pod = FamilyOf("ExobaseHeadquarters");
			t.Assert.False(Offered(MaterialSwap.Categories(true), pod), "the mini-pod is not offered");
			CellRect r = t.Rect(5, 4);
			t.World.Build("Tile", r.Cell(2, 0), element: SimHashes.Granite);
			GameObject built = t.World.Build("ExobaseHeadquarters", r.Cell(2, 1), element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(1);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			plan.ChangeMaterial.Pick(new[] { pod }, true);
			plan.ChangeMaterial.SetSourceAny();
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.False(Takes(plan, built, out _), "a plan does not take a built mini-pod");
		}

		[OniTest("Upgrade Planner: a two-ingredient building keeps the ingredient the plan does not change", Timeout = 40), Layer(1)]
		public static IEnumerator OtherIngredientKept(TestContext t)
		{
			RequireBlueprints(t);
			// A germ sensor is refined metal and plastic. Built of copper and plastium, it is
			// changed from copper to steel: the plastium stays, though plain plastic comes first.
			BuildingDef def = Assets.GetBuildingDef("LogicDiseaseSensor");
			Tag plastium = SimHashes.HardPolypropylene.CreateTag();
			t.Assert.Eq(2, def.CraftRecipe.Ingredients.Count, "the germ sensor has two ingredients");
			t.Assert.True(Materials.Valid(def.CraftRecipe.Ingredients[1].tag).Contains(plastium), "plastium is one of its plastics");
			CellRect r = t.Rect(4, 2);
			int cell = r.Cell(1, 0);
			var built = new List<Tag> { SimHashes.Copper.CreateTag(), plastium };
			float temperature = Mathf.Min(def.Temperature, ElementLoader.GetMinMeltingPointAmongElements(built) - 10f);
			GameObject sensor = def.Build(cell, Orientation.Neutral, null, built, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
			yield return t.Frames(2);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			plan.ChangeMaterial.Pick(new[] { FamilyOf("LogicDiseaseSensor") }, true);
			plan.ChangeMaterial.SetSource(SimHashes.Copper.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Steel.CreateTag());
			t.Assert.True(Takes(plan, sensor, out List<Tag> materials), "the plan takes the germ sensor");
			t.Assert.Eq(SimHashes.Steel.CreateTag(), materials[0], "the refined metal becomes steel");
			t.Assert.Eq(plastium, materials[1], "the plastic stays plastium");
			yield return Drag(t, r, "LogicDiseaseSensor", instant: true, allBuildings: true, oldElement: SimHashes.Copper, newElement: SimHashes.Steel);
			yield return t.WaitUntil(() => Made(t.World.At(cell, ObjectLayer.Building), SimHashes.Steel), 5, "the germ sensor rebuilt in steel");
			Tag[] rebuilt = t.World.At(cell, ObjectLayer.Building).GetComponent<Deconstructable>().constructionElements;
			t.Log("rebuilt of " + string.Join(", ", System.Array.ConvertAll(rebuilt, tag => tag.Name)));
			t.Assert.Eq(plastium, rebuilt[1], "the rebuilt germ sensor's plastic is still plastium");
		}

		[OniTest("Upgrade Planner: a plan can change a building's second ingredient", Timeout = 40), Layer(1)]
		public static IEnumerator SecondIngredientChanged(TestContext t)
		{
			RequireBlueprints(t);
			// A germ sensor of copper and plastic, plastic changed to plastium: the copper stays.
			BuildingDef def = Assets.GetBuildingDef("LogicDiseaseSensor");
			Tag plastic = SimHashes.Polypropylene.CreateTag();
			Tag plastium = SimHashes.HardPolypropylene.CreateTag();
			CellRect r = t.Rect(4, 2);
			int cell = r.Cell(1, 0);
			var built = new List<Tag> { SimHashes.Copper.CreateTag(), plastic };
			float temperature = Mathf.Min(def.Temperature, ElementLoader.GetMinMeltingPointAmongElements(built) - 10f);
			GameObject sensor = def.Build(cell, Orientation.Neutral, null, built, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
			yield return t.Frames(2);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			plan.ChangeMaterial.Pick(new[] { FamilyOf("LogicDiseaseSensor") }, true);
			plan.ChangeMaterial.SetSource(plastic);
			plan.ChangeMaterial.SetTarget(plastium);
			MaterialSwap.Refresh();
			string picker = MaterialSwap.PickerRecipe().Ingredients[0].tag.Name;
			t.Log("picker offers " + picker);
			t.Assert.True(System.Array.IndexOf(picker.Split('&'), def.CraftRecipe.Ingredients[1].tag.Name) >= 0, "the picker offers the plastics");
			t.Assert.Eq(null, plan.ChangeMaterial.Blocked(FamilyOf("LogicDiseaseSensor")), "plastic to plastium leaves the germ sensor offered");
			t.Assert.True(Takes(plan, sensor, out List<Tag> materials), "the plan takes the germ sensor");
			t.Assert.Eq(SimHashes.Copper.CreateTag(), materials[0], "the refined metal stays copper");
			t.Assert.Eq(plastium, materials[1], "the plastic becomes plastium");
			yield return Drag(t, r, "LogicDiseaseSensor", instant: true, allBuildings: true, oldElement: SimHashes.Polypropylene, newElement: SimHashes.HardPolypropylene);
			yield return t.WaitUntil(() => t.World.At(cell, ObjectLayer.Building)?.GetComponent<BuildingComplete>() != null
				&& t.World.At(cell, ObjectLayer.Building).GetComponent<Deconstructable>().constructionElements[1] == plastium, 5, "the germ sensor rebuilt with plastium");
			Tag[] rebuilt = t.World.At(cell, ObjectLayer.Building).GetComponent<Deconstructable>().constructionElements;
			t.Log("rebuilt of " + string.Join(", ", System.Array.ConvertAll(rebuilt, tag => tag.Name)));
			t.Assert.Eq(SimHashes.Copper.CreateTag(), rebuilt[0], "the rebuilt germ sensor is still copper");
		}

		// ---- helpers ----

		private static void RequireBlueprints(TestContext t)
		{
			if (!BlueprintsCompat.Loaded)
				t.Skip("Blueprints Expanded is not loaded (run with -Steam 3468585385)");
		}

		/// <summary>A sandstone storage bin on a tile at column x, its capacity set to <see cref="Capacity"/>.</summary>
		private static GameObject BuildLocker(TestContext t, CellRect r, int x)
		{
			t.World.Build("Tile", r.Cell(x, 0), element: SimHashes.Granite);
			GameObject locker = t.World.Build("StorageLocker", r.Cell(x, 1), element: SimHashes.SandStone);
			locker.GetComponent<IUserControlledCapacity>().UserMaxCapacity = Capacity;
			return locker;
		}

		private static bool Granite(GameObject go) => Made(go, SimHashes.Granite);

		private static bool Made(GameObject go, SimHashes element)
		{
			return go != null && go.GetComponent<BuildingComplete>() != null && go.GetComponent<PrimaryElement>().ElementID == element;
		}

		/// <summary>Sandstone to granite (or <paramref name="oldElement"/> to <paramref name="newElement"/>) in one type, dragged over the whole area with the real tool.</summary>
		private static IEnumerator Drag(TestContext t, CellRect r, string type, bool instant, bool allBuildings,
			SimHashes oldElement = SimHashes.SandStone, SimHashes newElement = SimHashes.Granite)
		{
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(allBuildings);
			plan.ChangeMaterial.Pick(new[] { FamilyOf(type) }, true);
			plan.ChangeMaterial.SetSource(oldElement.CreateTag());
			plan.ChangeMaterial.SetTarget(newElement.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(instant);
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
			PlayerController.Instance.ActivateTool(tool);
			yield return t.Frames(2);
			Vector3 from = Grid.CellToPosCCC(r.Cell(0, 0), Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(r.Cell(r.W - 1, r.H - 1), Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return null;
			tool.OnLeftClickDown(from);
			yield return null;
			tool.OnMouseMove(to);
			yield return null;
			tool.OnLeftClickUp(to);
			yield return t.Frames(3);
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
		}

		/// <summary>What an order's Blueprints Expanded carrier holds, by key.</summary>
		private static Dictionary<string, string> Carried(GameObject order)
		{
			System.Type type = AccessTools.TypeByName("BlueprintsV2.BlueprintData.UnderConstructionDataTransfer");
			Component carrier = type != null ? order.GetComponent(type) : null;
			FieldInfo field = type != null ? AccessTools.Field(type, "ToApplyData") : null;
			return carrier != null && field != null ? new Dictionary<string, string>((Dictionary<string, string>)field.GetValue(carrier)) : new Dictionary<string, string>();
		}

		/// <summary>What the plan, worked out as a drag would, makes of the building: its new materials, or false when it leaves it alone.</summary>
		private static bool Takes(UpgradePlan plan, GameObject building, out List<Tag> materials)
		{
			materials = null;
			Matcher matcher = plan.Compile();
			if (matcher == null || !matcher.TryPlan(building.GetComponent<BuildingComplete>(), out Upgrade upgrade))
				return false;
			materials = upgrade.Materials;
			return true;
		}

		private static Family FamilyOf(string prefabId)
		{
			return Families.Of(Assets.GetBuildingDef(prefabId));
		}

		private static bool Offered(List<Category> categories, Family family)
		{
			foreach (Category category in categories)
				if (category.Families.Contains(family))
					return true;
			return false;
		}
	}
}
