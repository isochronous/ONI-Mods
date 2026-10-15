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
		private const int Crew = 3;

		/// <summary>Real seconds the crew gets for a test's work, at speed 3.</summary>
		private const double WorkSeconds = 240;

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
		/// The types built along the floor in their first materials, their settings changed, then each
		/// changed to its second material with All buildings and dragged over: taken down and built
		/// again through the game's reconstruct (<paramref name="rebuild"/>), or built over by a
		/// replacement order. The crew finishes every one, and the settings are as they were.
		/// </summary>
		private static IEnumerator Remade(TestContext t, string[] types, bool rebuild)
		{
			BuildingSettings.RequireBlueprints(t);
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			// Along the floor, a few cells apart: the farm tile in the floor row, the rest standing on it
			// or on the box's floor, the sensor and gate at head height.
			var defs = types.Select(Assets.GetBuildingDef).ToList();
			var built = new List<GameObject>();
			int x = 3;
			foreach (BuildingDef def in defs)
			{
				int cell = def.IsTilePiece ? inside.Cell(x + def.WidthInCells / 2, 0) : inside.Cell(x + def.WidthInCells / 2, def.BuildLocationRule == BuildLocationRule.OnFloor ? 0 : 1);
				List<Tag> materials = BuildingSettings.Materials(def);
				if (!def.IsValidPlaceLocation(null, cell, Orientation.Neutral, out string reason))
					t.Assert.Fail("cannot place " + def.PrefabID + " at " + Cells.Describe(cell) + ": " + reason);
				float temperature = Mathf.Min(def.Temperature, ElementLoader.GetMinMeltingPointAmongElements(materials) - 10f);
				built.Add(def.Build(cell, Orientation.Neutral, null, materials, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime()));
				t.Log("built " + def.PrefabID + " of " + string.Join(", ", materials.Select(m => m.Name)) + " at " + Cells.Describe(cell));
				x += def.WidthInCells + 2;
			}
			yield return t.WaitGameSeconds(2);

			// Each building's settings changed from the defaults where the player can change them.
			var expected = new List<(int cell, BuildingDef def, Dictionary<string, string> values)>();
			foreach (GameObject go in built)
			{
				BuildingDef def = go.GetComponent<Building>().Def;
				var values = new Dictionary<string, string>();
				HashSet<string> offered = SettingKinds.Offered(go);
				Dictionary<string, string> keys = BlueprintsCompat.SettingsOf(go);
				foreach (string key in keys.Keys)
				{
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
				t.Log(def.PrefabID + " settings: " + string.Join(", ", values.Select(kv => kv.Key + "=" + kv.Value)));
				expected.Add((Grid.PosToCell(go), def, values));
			}
			t.Assert.True(expected.Sum(e => e.values.Count) > 0, "some settings were changed to check");

			t.InstantBuild(false);
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 6, 0));
			t.Speed(3);
			int pile = inside.W - 3;
			foreach (BuildingDef def in defs)
			{
				var materials = BuildingSettings.Materials(def);
				materials[0] = BuildingSettings.Target(def).Value;
				Supply(t, def, materials, 1, inside.Cell(pile, 0));
			}
			yield return t.WaitGameSeconds(2);

			// One drag per first ingredient, each with its own target, as a player would plan them.
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
				yield return Drag(t, inside.Cell(1, 0), inside.Cell(pile - 2, 2));
				if (!rebuild)
					foreach (BuildingDef def in group)
						foreach (var e in expected.Where(e => e.def == def))
							t.Assert.NotNull(t.World.At(e.cell, def.ReplacementLayer)?.GetComponent<Constructable>(), "a replacement order over the " + def.PrefabID);
				yield return t.WaitGameSeconds(1);
			}
			float start = GameClock.Instance.GetTime();
			yield return t.WaitUntil(() => expected.All(e => Built(e.cell, e.def.ObjectLayer, e.def, BuildingSettings.Target(e.def).Value)),
				WorkSeconds + 60, "the crew to build every building again in its new material");
			t.Log("the crew " + (rebuild ? "rebuilt " : "built over ") + expected.Count + " buildings in " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds");
			// Blueprints Expanded applies the settings a little after the building is finished.
			yield return t.WaitGameSeconds(2);
			var wrong = new List<string>();
			foreach (var e in expected)
			{
				GameObject now = t.World.At(e.cell, e.def.ObjectLayer);
				foreach (KeyValuePair<string, string> setting in e.values)
				{
					string after = SettingKinds.All[setting.Key].Read(now);
					if (after != setting.Value)
						wrong.Add(e.def.PrefabID + " " + setting.Key + " was " + setting.Value + ", is " + after);
				}
			}
			yield return t.Screenshot("rebuilt");
			t.Assert.Eq(0, wrong.Count, "settings lost: " + string.Join("; ", wrong));
		}

		// ---- shared ----

		/// <summary>A finished <paramref name="def"/> of <paramref name="material"/> at the cell.</summary>
		private static bool Built(int cell, ObjectLayer layer, BuildingDef def, Tag material)
		{
			GameObject go = Grid.Objects[cell, (int)layer];
			return go != null && go.GetComponent<BuildingComplete>() != null && go.GetComponent<Building>().Def == def
				&& go.GetComponent<PrimaryElement>().Element.tag == material;
		}

		/// <summary>Enough of each ingredient for <paramref name="count"/> of the type, and some over, in one pile.</summary>
		private static void Supply(TestContext t, BuildingDef def, List<Tag> materials, int count, int cell)
		{
			Recipe.Ingredient[] ingredients = def.CraftRecipe.Ingredients.ToArray();
			for (int i = 0; i < ingredients.Length; i++)
				t.Dupes.Drop(materials[i], ingredients[i].amount * count * 1.5f, cell);
		}

		/// <summary>A box drag with the planner's tool from one cell to another, at a hand's pace.</summary>
		private static IEnumerator Drag(TestContext t, int fromCell, int toCell)
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
