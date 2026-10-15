using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// The planner's orders carried out by Duplicants, as in a player's colony: the fixture area
	/// boxed in with air and life support, a crew spawned inside, the materials dropped on the floor,
	/// instant build off, and the real tool dragged over the buildings. What instant build skips is
	/// what these cover: Duplicants fetching the materials, building a replacement over the old
	/// building, taking a building down and building it again for a reconstruct, and the new
	/// building ending up with the old one's connections and settings. Every tech is researched
	/// first, as the planner offers a player only what they can build.
	/// </summary>
	public static class ByHand
	{
		internal const int Crew = 3;

		/// <summary>Real seconds the crew gets for a test's work, at speed 3.</summary>
		internal const double WorkSeconds = 240;

		[OniTest("Upgrade Planner: Duplicants rebuild sandstone tiles in granite once there is granite to fetch", Timeout = 300, Boxed = true), Layer(1)]
		public static IEnumerator Tiles(TestContext t)
		{
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			BuildingDef tile = Assets.GetBuildingDef("Tile");
			var cells = Enumerable.Range(3, 8).Select(x => inside.Cell(x, 0)).ToList();
			foreach (int cell in cells)
				t.World.Build("Tile", cell, element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(2);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { Families.Of(tile) }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(false);
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			yield return t.WaitGameSeconds(2);

			yield return Drag(t, cells.First(), cells.Last());
			foreach (int cell in cells)
				t.Assert.NotNull(t.World.At(cell, ObjectLayer.ReplacementTile)?.GetComponent<Constructable>(), "a granite order over the tile at " + Cells.Describe(cell));
			// Nothing to build with yet: the orders wait, the sandstone stays.
			yield return t.WaitGameSeconds(10);
			foreach (int cell in cells)
				t.Assert.Eq(SimHashes.SandStone, t.World.At(cell, ObjectLayer.FoundationTile).GetComponent<PrimaryElement>().ElementID, "the tile at " + Cells.Describe(cell) + " waits for granite");

			Supply(t, tile, new List<Tag> { SimHashes.Granite.CreateTag() }, cells.Count, inside.Cell(inside.W - 3, 0));
			float start = GameClock.Instance.GetTime();
			yield return t.WaitUntil(() => cells.All(cell => Built(cell, ObjectLayer.FoundationTile, tile, SimHashes.Granite.CreateTag())
				&& t.World.At(cell, ObjectLayer.ReplacementTile) == null), WorkSeconds, "the crew to rebuild every tile in granite");
			t.Log("the crew rebuilt " + cells.Count + " tiles in " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds");
			yield return t.Screenshot("tiles");
		}

		[OniTest("Upgrade Planner: Duplicants replace gas pipes with insulated ones, still connected", Timeout = 300, Boxed = true), Layer(1)]
		public static IEnumerator Pipes(TestContext t)
		{
			yield return Replace(t, "GasConduit", "InsulatedGasConduit", ObjectLayer.GasConduit, ObjectLayer.ReplacementGasConduit);
		}

		[OniTest("Upgrade Planner: Duplicants replace copper power wires with gold ones, still connected", Timeout = 300, Boxed = true), Layer(1)]
		public static IEnumerator Wires(TestContext t)
		{
			yield return Replace(t, "Wire", "Wire", ObjectLayer.Wire, ObjectLayer.ReplacementWire, SimHashes.Gold.CreateTag());
		}

		/// <summary>
		/// A row of <paramref name="sourceId"/> at head height, laid with the real utility tool so its
		/// connections are real, replaced by <paramref name="targetId"/> in <paramref name="material"/>
		/// (or the target's first material by name) through the Replace type tab.
		/// </summary>
		private static IEnumerator Replace(TestContext t, string sourceId, string targetId, ObjectLayer layer, ObjectLayer replacementLayer, Tag? material = null)
		{
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			int[] cells = Enumerable.Range(3, 6).Select(x => inside.Cell(x, 1)).ToArray();
			UtilityDragSession lay = t.Tools.UtilityTool(sourceId, SimHashes.Copper);
			yield return lay.Path(cells);
			lay.Dispose();
			yield return t.WaitGameSeconds(1);
			var connections = new Dictionary<int, UtilityConnections>();
			foreach (int cell in cells)
			{
				GameObject piece = t.World.At(cell, layer);
				t.Assert.NotNull(piece?.GetComponent<BuildingComplete>(), "a " + sourceId + " at " + Cells.Describe(cell));
				connections[cell] = piece.GetComponent<KAnimGraphTileVisualizer>().Connections;
			}

			BuildingDef source = Assets.GetBuildingDef(sourceId);
			BuildingDef target = Assets.GetBuildingDef(targetId);
			List<Tag> materials = material.HasValue ? new List<Tag> { material.Value } : BuildingSettings.Materials(target);
			t.Assert.NotNull(materials, "a material to build " + targetId + " of");
			t.InstantBuild(false);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ReplaceType;
			plan.ReplaceType.SetSource(Families.Of(source));
			plan.ReplaceType.SetSourceAny();
			plan.ReplaceType.SetTarget(Families.Of(target));
			plan.ReplaceType.SetTargetMaterials(materials);
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			Supply(t, target, materials, cells.Length, inside.Cell(inside.W - 3, 0));
			yield return t.WaitGameSeconds(2);

			yield return Drag(t, cells.First(), cells.Last());
			foreach (int cell in cells)
				t.Assert.NotNull(t.World.At(cell, replacementLayer)?.GetComponent<Constructable>(), "a " + targetId + " order at " + Cells.Describe(cell));
			float start = GameClock.Instance.GetTime();
			yield return t.WaitUntil(() => cells.All(cell => Built(cell, layer, target, materials[0]) && t.World.At(cell, replacementLayer) == null),
				WorkSeconds, "the crew to build every " + targetId + " in " + materials[0].Name);
			t.Log("the crew built " + cells.Length + " " + targetId + " in " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds");
			yield return t.WaitGameSeconds(1);
			foreach (int cell in cells)
				t.Assert.Eq(connections[cell], t.World.At(cell, layer).GetComponent<KAnimGraphTileVisualizer>().Connections, "the " + targetId + " at " + Cells.Describe(cell) + " keeps the old one's connections");
		}

		/// <summary>
		/// Types the game does not let you build over, of each kind that has needed care: a sensor and
		/// a gate (settings only), a floor machine, and the wide farm tile, whose new order the game
		/// cannot place on the first try.
		/// </summary>
		private static readonly string[] RebuiltTypes = { "LogicTimerSensor", "LogicGateBUFFER", "ManualGenerator", "WideFarmTile" };

		[OniTest("Upgrade Planner: Duplicants take buildings down and build them again in another material, with their settings", Timeout = 420, Boxed = true), Layer(1)]
		public static IEnumerator Rebuilt(TestContext t)
		{
			return Remade(t, RebuiltTypes, rebuild: true);
		}

		/// <summary>
		/// Tiles the game lets you build over, with something to carry: the storage tile's filter, and
		/// the carpet's second ingredient, an item rather than an element.
		/// </summary>
		private static readonly string[] ReplacedTypes = { "StorageTile", "CarpetTile" };

		[OniTest("Upgrade Planner: Duplicants build tiles over tiles in another material, with their settings and item ingredients", Timeout = 420, Boxed = true), Layer(1)]
		public static IEnumerator ReplacedWithSettings(TestContext t)
		{
			return Remade(t, ReplacedTypes, rebuild: false);
		}

		/// <summary>
		/// Doors across the corridor the crew walks along, the furthest from them first: a mechanical
		/// airlock, which without power the crew cannot open, so nothing is needed past it, a manual
		/// airlock, a wooden door (logs, an item) and a pneumatic door. Their access is left as it is,
		/// since a door locked against the crew would shut them out of the work behind it.
		/// </summary>
		private static readonly string[] DoorTypes = { "PressureDoor", "ManualPressureDoor", "WoodenDoor", "Door" };

		[OniTest("Upgrade Planner: Duplicants rebuild the doors they walk through in another material, with their settings", Timeout = 420, Boxed = true), Layer(1)]
		public static IEnumerator Doors(TestContext t)
		{
			return Remade(t, DoorTypes, rebuild: true, leave: new[] { "AccessControl" });
		}

		/// <summary>
		/// The types built along the floor in their first materials, their settings changed (all but
		/// <paramref name="leave"/>), then each changed to its second material with All buildings and
		/// dragged over: taken down and built again through the game's reconstruct
		/// (<paramref name="rebuild"/>), or built over by a replacement order. The crew finishes every
		/// one, and the settings are as they were.
		/// </summary>
		private static IEnumerator Remade(TestContext t, string[] types, bool rebuild, string[] leave = null)
		{
			BuildingSettings.RequireBlueprints(t);
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			// Along the floor, a few cells apart: tiles in the floor row, the rest standing on the box's
			// floor, one-cell pieces at head height.
			var defs = types.Select(Assets.GetBuildingDef).ToList();
			var built = new List<GameObject>();
			int x = 3;
			foreach (BuildingDef def in defs)
			{
				int cell = def.IsTilePiece ? inside.Cell(x + def.WidthInCells / 2, 0) : inside.Cell(x + def.WidthInCells / 2, def.BuildLocationRule == BuildLocationRule.OnFloor ? 0 : 1);
				built.Add(Build(t, def, BuildingSettings.Materials(def), cell));
				x += def.WidthInCells + 2;
			}
			yield return t.WaitGameSeconds(2);

			var expected = new List<Expected>();
			foreach (GameObject go in built)
				expected.Add(new Expected(go, ChangeSettings(t, go, leave)));
			t.Assert.True(expected.Sum(e => e.Values.Count) > 0, "some settings were changed to check");
			yield return Settle(built);

			t.InstantBuild(false);
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			int pile = inside.W - 3;
			foreach (BuildingDef def in defs)
				Supply(t, def, Remake(def), 1, inside.Cell(pile, 0));
			yield return t.WaitGameSeconds(2);

			yield return DragEachGroup(t, defs, inside.Cell(1, 0), inside.Cell(pile - 2, inside.H - 1), rebuild ? null : expected);
			float start = GameClock.Instance.GetTime();
			yield return t.WaitUntil(() => expected.All(e => e.Done), WorkSeconds + 60, "the crew to build every building again in its new material");
			t.Log("the crew " + (rebuild ? "rebuilt " : "built over ") + expected.Count + " buildings in " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds");
			// Blueprints Expanded applies the settings a little after the building is finished.
			yield return t.WaitGameSeconds(2);
			List<string> wrong = Lost(expected);
			yield return t.Screenshot("rebuilt");
			t.Assert.Eq(0, wrong.Count, "settings lost: " + string.Join("; ", wrong));
		}

		[OniTest("Upgrade Planner: Duplicants change one ingredient of a two-ingredient building and keep the other", Timeout = 300, Boxed = true), Layer(1)]
		public static IEnumerator TwoIngredients(TestContext t)
		{
			BuildingSettings.RequireBlueprints(t);
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			// Germ sensors of refined metal and plastic: one of copper and plastic has its plastic changed
			// to plastium, one of copper and plastium its copper changed to steel.
			BuildingDef def = Assets.GetBuildingDef("LogicDiseaseSensor");
			Tag copper = SimHashes.Copper.CreateTag(), steel = SimHashes.Steel.CreateTag();
			Tag plastic = SimHashes.Polypropylene.CreateTag(), plastium = SimHashes.HardPolypropylene.CreateTag();
			int second = inside.Cell(4, 1), first = inside.Cell(10, 1);
			Build(t, def, new List<Tag> { copper, plastic }, second);
			Build(t, def, new List<Tag> { copper, plastium }, first);
			yield return t.WaitGameSeconds(2);

			t.InstantBuild(false);
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			Supply(t, def, new List<Tag> { copper, plastium }, 1, inside.Cell(inside.W - 3, 0));
			Supply(t, def, new List<Tag> { steel, plastium }, 1, inside.Cell(inside.W - 3, 0));
			yield return t.WaitGameSeconds(2);

			yield return PlanAndDrag(t, def, plastic, plastium, second);
			yield return PlanAndDrag(t, def, copper, steel, first);
			yield return t.WaitUntil(() => MadeOf(second, def, copper, plastium) && MadeOf(first, def, steel, plastium), WorkSeconds,
				"the crew to rebuild the sensors as copper and plastium, and steel and plastium");
			t.Log("rebuilt of " + Describe(second, def) + ", and of " + Describe(first, def));
		}

		/// <summary>The plan "this type, from one material to another" with All buildings, dragged over one cell.</summary>
		private static IEnumerator PlanAndDrag(TestContext t, BuildingDef def, Tag from, Tag to, int cell)
		{
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			MaterialSwap.Refresh();
			plan.ChangeMaterial.Pick(new[] { Families.Of(def) }, true);
			plan.ChangeMaterial.SetSource(from);
			plan.ChangeMaterial.SetTarget(to);
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			yield return Drag(t, cell, cell);
		}

		private static bool MadeOf(int cell, BuildingDef def, params Tag[] materials)
		{
			GameObject go = Grid.Objects[cell, (int)def.ObjectLayer];
			if (go == null || go.GetComponent<BuildingComplete>() == null)
				return false;
			Tag[] made = go.GetComponent<Deconstructable>().constructionElements;
			return made != null && made.SequenceEqual(materials);
		}

		private static string Describe(int cell, BuildingDef def)
		{
			GameObject go = Grid.Objects[cell, (int)def.ObjectLayer];
			Tag[] made = go?.GetComponent<Deconstructable>()?.constructionElements;
			return made == null ? "nothing" : string.Join(" and ", made.Select(m => m.Name));
		}

		[OniTest("Upgrade Planner: replacement orders cancelled with the Cancel tool are never built, granite or no granite", Timeout = 300, Boxed = true), Layer(1)]
		public static IEnumerator CancelledOrders(TestContext t)
		{
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			BuildingDef tile = Assets.GetBuildingDef("Tile");
			var cells = Enumerable.Range(3, 6).Select(x => inside.Cell(x, 0)).ToList();
			foreach (int cell in cells)
				t.World.Build("Tile", cell, element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(2);
			t.InstantBuild(false);
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			yield return t.WaitGameSeconds(2);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(new[] { Families.Of(tile) }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			yield return Drag(t, cells.First(), cells.Last());
			foreach (int cell in cells)
				t.Assert.NotNull(t.World.At(cell, ObjectLayer.ReplacementTile), "a granite order at " + Cells.Describe(cell));
			yield return Cancel(t, cells.First(), cells.Last());
			foreach (int cell in cells)
				t.Assert.Null(t.World.At(cell, ObjectLayer.ReplacementTile), "the order at " + Cells.Describe(cell) + " is cancelled");

			GameObject granite = t.Dupes.Drop(SimHashes.Granite, 1000f, inside.Cell(inside.W - 3, 0));
			yield return t.WaitGameSeconds(30);
			foreach (int cell in cells)
			{
				t.Assert.Null(t.World.At(cell, ObjectLayer.ReplacementTile), "no order came back at " + Cells.Describe(cell));
				t.Assert.Eq(SimHashes.SandStone, t.World.At(cell, ObjectLayer.FoundationTile).GetComponent<PrimaryElement>().ElementID, "the tile at " + Cells.Describe(cell) + " is still sandstone");
			}
			t.Assert.True(granite != null && granite.GetComponent<PrimaryElement>().Mass >= 999f, "nobody fetched the granite");
		}

		[OniTest("Upgrade Planner: a rebuild cancelled with the Cancel tool before the crew comes leaves the building alone", Timeout = 300, Boxed = true), Layer(1)]
		public static IEnumerator CancelledRebuild(TestContext t)
		{
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			BuildingDef bin = Assets.GetBuildingDef("StorageLocker");
			int cell = inside.Cell(4, 0);
			Build(t, bin, new List<Tag> { SimHashes.SandStone.CreateTag() }, cell);
			yield return t.WaitGameSeconds(2);
			t.InstantBuild(false);

			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			MaterialSwap.Refresh();
			plan.ChangeMaterial.Pick(new[] { Families.Of(bin) }, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			yield return Drag(t, cell, cell);
			Reconstructable reconstructable = t.World.At(cell, bin.ObjectLayer).GetComponent<Reconstructable>();
			t.Assert.True(reconstructable.ReconstructRequested, "the bin is marked to be rebuilt");
			yield return Cancel(t, cell, cell);
			t.Log("after the cancel, rebuild requested " + reconstructable.ReconstructRequested);

			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			t.Dupes.Drop(SimHashes.Granite, 1000f, inside.Cell(inside.W - 3, 0));
			yield return t.WaitGameSeconds(30);
			GameObject now = t.World.At(cell, bin.ObjectLayer);
			t.Assert.True(now != null && now == reconstructable.gameObject, "the same bin still stands");
			t.Assert.Eq(SimHashes.SandStone, now.GetComponent<PrimaryElement>().ElementID, "the bin is still sandstone");
			t.Assert.False(reconstructable.ReconstructRequested, "the bin is no longer marked to be rebuilt");
		}

		/// <summary>A drag with the game's Cancel tool, all layers, from one cell to another, at a hand's pace.</summary>
		private static IEnumerator Cancel(TestContext t, int fromCell, int toCell)
		{
			CancelTool tool = CancelTool.Instance;
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(0.3);
			Vector3 from = Grid.CellToPosCCC(fromCell, Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(toCell, Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(0.3);
			tool.OnLeftClickUp(to);
			yield return t.WaitRealSeconds(0.3);
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			yield return t.WaitGameSeconds(1);
		}

		// ---- shared ----

		/// <summary>A building whose settings were changed, and what they read then.</summary>
		internal sealed class Expected
		{
			public readonly int Cell;
			public readonly BuildingDef Def;
			public readonly Dictionary<string, string> Values;

			public Expected(GameObject go, Dictionary<string, string> values)
			{
				Cell = Grid.PosToCell(go);
				Def = go.GetComponent<Building>().Def;
				Values = values;
			}

			/// <summary>Built again in its second material.</summary>
			public bool Done => Built(Cell, Def.ObjectLayer, Def, BuildingSettings.Target(Def).Value);
		}

		/// <summary>A finished building of the type and materials at the cell, as an instant build makes one.</summary>
		internal static GameObject Build(TestContext t, BuildingDef def, List<Tag> materials, int cell)
		{
			if (!def.IsValidPlaceLocation(null, cell, Orientation.Neutral, out string reason))
				t.Assert.Fail("cannot place " + def.PrefabID + " at " + Cells.Describe(cell) + ": " + reason);
			float temperature = Mathf.Min(def.Temperature, ElementLoader.GetMinMeltingPointAmongElements(materials) - 10f);
			GameObject go = def.Build(cell, Orientation.Neutral, null, materials, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
			t.Log("built " + def.PrefabID + " of " + string.Join(", ", materials.Select(m => m.Name)) + " at " + Cells.Describe(cell));
			return go;
		}

		/// <summary>
		/// The building's settings changed from the defaults where the player can change them, all but
		/// <paramref name="leave"/>, and what they read now.
		/// </summary>
		internal static Dictionary<string, string> ChangeSettings(TestContext t, GameObject go, string[] leave)
		{
			var values = new Dictionary<string, string>();
			HashSet<string> offered = SettingKinds.Offered(go);
			Dictionary<string, string> keys = BlueprintsCompat.SettingsOf(go);
			foreach (string key in keys.Keys)
			{
				if (leave != null && leave.Contains(key))
					continue;
				if (SettingKinds.Control.TryGetValue(key, out string control) && !offered.Contains(control))
					continue;
				// No Manual Delivery's automatable holds "automation only" and its "hold" together:
				// one setting, changed and read through its own entry.
				if (key == "Automatable" && keys.ContainsKey("UpgradePlanner.NoManualDelivery.Hold"))
					continue;
				if (!SettingKinds.All.TryGetValue(key, out SettingKinds.Kind kind))
					continue;
				string why = kind.Change(go);
				if (why == null)
					values[key] = kind.Read(go);
			}
			t.Log(go.GetComponent<Building>().Def.PrefabID + " settings: " + string.Join(", ", values.Select(kv => kv.Key + "=" + kv.Value)));
			return values;
		}

		/// <summary>The settings that read differently on the building now at each one's cell.</summary>
		internal static List<string> Lost(IEnumerable<Expected> expected)
		{
			var wrong = new List<string>();
			foreach (Expected e in expected)
			{
				GameObject now = Grid.Objects[e.Cell, (int)e.Def.ObjectLayer];
				if (now == null)
				{
					wrong.Add(e.Def.PrefabID + " is gone");
					continue;
				}
				foreach (KeyValuePair<string, string> setting in e.Values)
				{
					string after = SettingKinds.All[setting.Key].Read(now);
					if (after != setting.Value)
						wrong.Add(e.Def.PrefabID + " " + setting.Key + " was " + setting.Value + ", is " + after);
				}
			}
			return wrong;
		}

		/// <summary>
		/// Doors asked to open finish opening first (up to a game minute), as a player would not
		/// rebuild one halfway. Not waited for: a mechanical airlock without power, which never does,
		/// and a door whose change is a Duplicant's errand (any door, outside debug instant build),
		/// which the crew does on arriving.
		/// </summary>
		internal static IEnumerator Settle(IEnumerable<GameObject> built)
		{
			List<Door> doors = built.Where(go => go != null).Select(go => go.GetComponent<Door>())
				.Where(door => door != null && door.GetComponent<EnergyConsumer>() == null
					&& !door.GetComponent<KSelectable>().HasStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState)).ToList();
			float until = GameClock.Instance.GetTime() + 60f;
			while (GameClock.Instance.GetTime() < until && doors.Any(door => door != null && door.CurrentState != door.RequestedState))
				yield return null;
		}

		/// <summary>What the type is rebuilt of: its first materials, the first ingredient changed to the second by name.</summary>
		internal static List<Tag> Remake(BuildingDef def)
		{
			List<Tag> materials = BuildingSettings.Materials(def);
			materials[0] = BuildingSettings.Target(def).Value;
			return materials;
		}

		/// <summary>
		/// One drag over the rectangle per first ingredient, each "any material to the second by name"
		/// with All buildings, as a player would plan them. With <paramref name="orders"/>, each of
		/// them has a replacement order after its drag.
		/// </summary>
		internal static IEnumerator DragEachGroup(TestContext t, List<BuildingDef> defs, int fromCell, int toCell, List<Expected> orders)
		{
			foreach (var group in defs.GroupBy(def => def.CraftRecipe.Ingredients[0].tag))
			{
				UpgradePlan plan = UpgradePlan.Current;
				plan.Mode = PlanMode.ChangeMaterial;
				plan.Clear();
				plan.ChangeMaterial.SetAllBuildings(true);
				MaterialSwap.Refresh();
				plan.ChangeMaterial.Pick(group.Select(Families.Of).Distinct().ToList(), true);
				plan.ChangeMaterial.SetSourceAny();
				plan.ChangeMaterial.SetTarget(BuildingSettings.Target(group.First()).Value);
				t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
				yield return Drag(t, fromCell, toCell);
				if (orders != null)
					foreach (Expected e in orders.Where(e => group.Contains(e.Def)))
						t.Assert.NotNull(Grid.Objects[e.Cell, (int)e.Def.ReplacementLayer]?.GetComponent<Constructable>(), "a replacement order over the " + e.Def.PrefabID);
				yield return t.WaitGameSeconds(1);
			}
		}

		/// <summary>A finished <paramref name="def"/> of <paramref name="material"/> at the cell.</summary>
		internal static bool Built(int cell, ObjectLayer layer, BuildingDef def, Tag material)
		{
			GameObject go = Grid.Objects[cell, (int)layer];
			return go != null && go.GetComponent<BuildingComplete>() != null && go.GetComponent<Building>().Def == def
				&& go.GetComponent<PrimaryElement>().Element.tag == material;
		}

		/// <summary>Enough of each ingredient for <paramref name="count"/> of the type, and some over, in one pile.</summary>
		internal static void Supply(TestContext t, BuildingDef def, List<Tag> materials, int count, int cell)
		{
			Recipe.Ingredient[] ingredients = def.CraftRecipe.Ingredients.ToArray();
			for (int i = 0; i < ingredients.Length; i++)
				t.Dupes.Drop(materials[i], ingredients[i].amount * count * 1.5f, cell);
		}

		/// <summary>A box drag with the planner's tool from one cell to another, at a hand's pace.</summary>
		internal static IEnumerator Drag(TestContext t, int fromCell, int toCell)
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
			PlayerController.Instance.ActivateTool(tool);
			PlannerWindow.PressApply(t);
			yield return t.WaitRealSeconds(0.3);
			Vector3 from = Grid.CellToPosCCC(fromCell, Grid.SceneLayer.Building);
			Vector3 to = Grid.CellToPosCCC(toCell, Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(0.3);
			tool.OnLeftClickUp(to);
			yield return t.WaitUntil(() => !PlacementQueue.Busy, 10, "the drag's buildings placed");
			yield return t.WaitRealSeconds(0.3);
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
		}
	}
}
