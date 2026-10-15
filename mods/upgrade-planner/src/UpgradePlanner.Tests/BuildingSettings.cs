using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// Every type All buildings offers that stands on an ordinary floor, a wall or a backwall (or
	/// anywhere), built in the fixture area in batches (one-cell pieces as a mosaic, the rest along
	/// two floors), its Blueprints Expanded settings changed from their defaults where the player
	/// can change them, then changed to another material with the real tool and instant build: the
	/// new building has the same settings.
	/// </summary>
	public static class BuildingSettings
	{
		private static readonly BuildLocationRule[] Rules =
		{
			BuildLocationRule.OnFloor, BuildLocationRule.Anywhere, BuildLocationRule.OnWall,
			BuildLocationRule.Tile, BuildLocationRule.NotInTiles, BuildLocationRule.OnBackWall,
		};

		/// <summary>What the test builds its floors, walls and backwalls of; not types under test.</summary>
		private static readonly string[] Scaffolding = { "Tile", "ExteriorWall" };

		[OniTest("Upgrade Planner: survey of the settings Blueprints Expanded stores for each floor and wall type", Timeout = 600, Explicit = true), Layer(0)]
		public static IEnumerator Survey(TestContext t)
		{
			RequireBlueprints(t);
			List<BuildingDef> defs = Eligible(t, out List<string> excluded);
			t.Log("eligible: " + defs.Count + "; excluded: " + excluded.Count);
			foreach (string line in excluded)
				t.Log("EXCLUDED " + line);
			var byKey = new SortedDictionary<string, List<string>>();
			foreach (Batch batch in Layout(defs, Bay(t)))
			{
				var built = new List<GameObject>();
				yield return BuildBatch(t, batch, built, null);
				yield return t.WaitGameSeconds(2);
				foreach (GameObject go in built)
				{
					Dictionary<string, string> settings = BlueprintsCompat.SettingsOf(go);
					string id = go.PrefabID().Name;
					t.Log("SURVEY " + id + " " + string.Join(" ", settings.Select(kv => kv.Key + "=" + kv.Value)) + " OFFERS " + string.Join(" ", SettingKinds.Offered(go).OrderBy(n => n, System.StringComparer.Ordinal)));
					foreach (string key in settings.Keys)
					{
						if (!byKey.TryGetValue(key, out List<string> ids))
							byKey[key] = ids = new List<string>();
						ids.Add(id);
					}
				}
				yield return Clear(t);
			}
			foreach (KeyValuePair<string, List<string>> entry in byKey)
				t.Log("KEY " + entry.Key + " (" + entry.Value.Count + "): " + string.Join(", ", entry.Value));
		}

		/// <summary>
		/// The smoke test: every type that has had a problem (lost settings, a ghost order, an error
		/// from the sim, a placement the test got wrong), a sensor and gate of the mosaic, and the
		/// buildings that carry other mods' settings, in one bay or two.
		/// </summary>
		private static readonly string[] ProblemTypes =
		{
			// Doors (access, open state, animation).
			"Door", "WoodenDoor", "ManualPressureDoor", "PressureDoor",
			// Tiles built over or rebuilt late (carpet's item, the wide tile, farm requests).
			"CarpetTile", "StorageTile", "WideFarmTile", "HydroponicFarm", "FarmTile",
			// Rebuilt beside other buildings (the first bay that broke the sim).
			"ThermalBlock", "ParkSign",
			// Settings lost at one time or another.
			"MissileLauncher", "LiquidLimitValve", "WoodSculpture", "LeadSuitMarker", "MilkFeeder", "FoodDehydrator", "EggIncubator", "PlanterBox",
			// Settings the test once changed wrongly.
			"MedicalCot", "LiquidHeater", "ManualGenerator", "FishFeeder",
			// Wall pieces and the mosaic.
			"FlowerVaseWall", "RadiationLight", "LogicTimerSensor", "LogicDiseaseSensor", "LogicGateBUFFER",
			// Suppress Notifications and No Manual Delivery settings, when those mods are loaded.
			"StorageLocker", "Generator",
		};

		[OniTest("Upgrade Planner: the problem types keep their settings when their material is changed", Timeout = 300, FreshSave = true), Layer(1)]
		public static IEnumerator ProblemTypesKept(TestContext t)
		{
			return Kept(t, ProblemTypes.Select(Assets.GetBuildingDef).Where(def => def != null).ToList());
		}

		[OniTest("Upgrade Planner: every floor and wall type keeps its changed settings when its material is changed", Timeout = 1800, FreshSave = true, Explicit = true), Layer(1)]
		public static IEnumerator SettingsKept(TestContext t)
		{
			return Kept(t, null);
		}

		private static IEnumerator Kept(TestContext t, List<BuildingDef> only)
		{
			RequireBlueprints(t);
			List<BuildingDef> defs = only ?? Eligible(t, out _);
			var failures = new List<string>();
			int checkedBuildings = 0, checkedSettings = 0, batches = 0;
			var checkedByKey = new SortedDictionary<string, int>(System.StringComparer.Ordinal);
			foreach (Batch batch in Layout(defs, Bay(t)))
			{
				var built = new List<GameObject>();
				yield return BuildBatch(t, batch, built, failures);
				yield return t.WaitGameSeconds(2);
				// Each building's settings changed from the defaults, and what they read now.
				var expected = new List<(int cell, BuildingDef def, Dictionary<string, string> values, Dictionary<string, string> stored)>();
				foreach (GameObject go in built)
				{
					BuildingDef def = go.GetComponent<Building>().Def;
					Dictionary<string, string> keys = BlueprintsCompat.SettingsOf(go);
					if (keys.Count == 0)
						continue;
					var values = new Dictionary<string, string>();
					HashSet<string> offered = SettingKinds.Offered(go);
					foreach (string key in keys.Keys)
					{
						// Only what the player can change on this building.
						if (SettingKinds.Control.TryGetValue(key, out string control) && !offered.Contains(control))
							continue;
						// No Manual Delivery's automatable holds "automation only" and its "hold" together:
						// one setting, changed and read through its own entry.
						if (key == "Automatable" && keys.ContainsKey("UpgradePlanner.NoManualDelivery.Hold"))
							continue;
						if (!SettingKinds.All.TryGetValue(key, out SettingKinds.Kind kind))
						{
							failures.Add(def.PrefabID + ": the test has no way to change " + key);
							continue;
						}
						string before = kind.Read(go);
						string why = kind.Change(go);
						if (why != null && why.StartsWith(SettingKinds.NotHere))
							continue;
						if (why != null)
						{
							failures.Add(def.PrefabID + ": cannot change " + key + ": " + why);
							continue;
						}
						values[key] = kind.Read(go);
						if (values[key] == before && before != "sensor")
							failures.Add(def.PrefabID + ": changing " + key + " left it at " + before);
					}
					expected.Add((Grid.PosToCell(go), def, values, BlueprintsCompat.SettingsOf(go)));
				}
				yield return t.WaitGameSeconds(1);
				// A door asked to open is not open until its animation has played out, which can take
				// a game minute; the player would not upgrade it halfway.
				List<Door> doors = built.Where(go => go != null).Select(go => go.GetComponent<Door>()).Where(door => door != null).ToList();
				float settle = GameClock.Instance.GetTime() + 60f;
				while (GameClock.Instance.GetTime() < settle && doors.Any(door => door != null && !Settled(door)))
					yield return null;
				foreach (Door door in doors)
					if (door != null && !Settled(door))
						t.Log("door " + door.name + " still " + door.CurrentState + (door.IsOpen() ? " (open)" : " (not open)") + ", asked " + door.RequestedState);
				// The real tool over the bay, once per material group: the first ingredient changed to the second material.
				var groups = batch.Slots.Select(slot => slot.Def).Distinct().GroupBy(def => def.CraftRecipe.Ingredients[0].tag).ToList();
				foreach (var group in groups)
				{
					List<BuildingDef> types = group.ToList();
					yield return Drag(t, types, Materials(types[0])[0], Target(types[0]).Value);
					yield return t.WaitGameSeconds(1);
				}
				float until = Time.realtimeSinceStartup + 30f;
				while (Time.realtimeSinceStartup < until && !expected.All(e => Rebuilt(e.cell, e.def, Target(e.def).Value)))
					yield return null;
				// Blueprints Expanded applies the settings a little after the building is finished.
				yield return t.WaitGameSeconds(2);
				foreach (var e in expected)
				{
					Tag to = Target(e.def).Value;
					GameObject now = Grid.Objects[e.cell, (int)e.def.ObjectLayer];
					if (!Rebuilt(e.cell, e.def, to))
					{
						failures.Add(e.def.PrefabID + ": not rebuilt in " + to.Name + " (" + (now != null ? now.name : "nothing") + ")");
						continue;
					}
					checkedBuildings++;
					foreach (KeyValuePair<string, string> setting in e.values)
					{
						checkedSettings++;
						checkedByKey[setting.Key] = checkedByKey.TryGetValue(setting.Key, out int n) ? n + 1 : 1;
						string after = SettingKinds.All[setting.Key].Read(now);
						if (after != setting.Value)
							failures.Add(e.def.PrefabID + ": " + setting.Key + " was " + setting.Value + ", is " + after
								+ " (Blueprints Expanded had " + (e.stored.TryGetValue(setting.Key, out string json) ? json : "nothing") + ")");
					}
				}
				batches++;
				t.Log("batch " + batches + " (" + batch.Kind + ", " + groups.Count + " material groups): " + string.Join(", ", batch.Slots.Select(slot => slot.Def.PrefabID)));
				yield return Clear(t);
			}
			foreach (string failure in failures)
				t.Log("FAILED " + failure);
			t.Log("checked " + checkedSettings + " settings on " + checkedBuildings + " buildings of " + defs.Count + " types in " + batches + " batches");
			t.Log("by setting: " + string.Join(", ", checkedByKey.Select(kv => kv.Key + " " + kv.Value)));
			// Other mods' settings the planner registers: checked whenever their mod is loaded.
			foreach (KeyValuePair<string, string> mod in OtherMods)
				if (HarmonyLib.AccessTools.TypeByName(mod.Value) != null && !checkedByKey.ContainsKey(mod.Key))
					failures.Add(mod.Key + ": its mod is loaded but no building had the setting checked");
			t.Assert.Eq(0, failures.Count, failures.Count + " problems, first: " + failures.FirstOrDefault());
		}

		/// <summary>The other mods' settings the planner registers, by key, with a type of that mod.</summary>
		private static readonly Dictionary<string, string> OtherMods = new Dictionary<string, string>
		{
			["UpgradePlanner.SuppressNotifications.StatusItems"] = "SuppressNotifications.StatusItemsSuppressedComp",
			["UpgradePlanner.SuppressNotifications.DamageBar"] = "SuppressNotifications.BuildingHealthSuppressedComp",
			["UpgradePlanner.NoManualDelivery.Hold"] = "NoManualDelivery.Automatable2",
		};

		/// <summary>The door is in the state it was asked for, and an opened one has finished opening.</summary>
		private static bool Settled(Door door) => door.CurrentState == door.RequestedState && (door.RequestedState != Door.ControlState.Opened || door.IsOpen());

		private static bool Rebuilt(int cell, BuildingDef def, Tag material)
		{
			GameObject go = Grid.Objects[cell, (int)def.ObjectLayer];
			return go != null && go.GetComponent<BuildingComplete>() != null && go.GetComponent<PrimaryElement>().Element.tag == material;
		}

		/// <summary>The types, from one material to another, with All buildings, dragged over the bay with the real tool and instant build.</summary>
		private static IEnumerator Drag(TestContext t, List<BuildingDef> types, Tag from, Tag to)
		{
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.SetAllBuildings(true);
			MaterialSwap.Refresh();
			var families = types.Select(Families.Of).Distinct().ToList();
			plan.ChangeMaterial.Pick(families, true);
			plan.ChangeMaterial.SetSource(from);
			plan.ChangeMaterial.SetTarget(to);
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			t.InstantBuild(true);
			UpgradeTool tool = null;
			foreach (InterfaceTool candidate in PlayerController.Instance.tools)
				if (candidate is UpgradeTool found)
					tool = found;
			t.Assert.NotNull(tool, "the Upgrade Planner tool is among the player's tools");
			PlayerController.Instance.ActivateTool(tool);
			PlannerWindow.PressApply(t);
			// At a hand's pace: a moment between each step of the drag.
			yield return t.WaitRealSeconds(0.3);
			CellRect bay = Bay(t);
			Vector3 start = Grid.CellToPosCCC(bay.Cell(0, 0), Grid.SceneLayer.Building);
			Vector3 end = Grid.CellToPosCCC(bay.Cell(bay.W - 1, bay.H - 1), Grid.SceneLayer.Building);
			tool.OnMouseMove(start);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(start);
			yield return t.WaitRealSeconds(0.2);
			tool.OnMouseMove(end);
			yield return t.WaitRealSeconds(0.3);
			tool.OnLeftClickUp(end);
			yield return t.WaitRealSeconds(0.3);
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
		}

		// ---- shared ----

		internal static void RequireBlueprints(TestContext t)
		{
			if (!BlueprintsCompat.Loaded)
				t.Skip("Blueprints Expanded is not loaded (run with -Steam 3468585385)");
		}

		/// <summary>The rectangle buildings go in.</summary>
		internal static CellRect Bay(TestContext t) => t.Rect(t.Area.W - 2, t.Area.H - 2);

		/// <summary>Types All buildings offers, of the placement rules above, that fit the bay and have a second material.</summary>
		internal static List<BuildingDef> Eligible(TestContext t, out List<string> excluded)
		{
			excluded = new List<string>();
			var defs = new List<BuildingDef>();
			CellRect bay = Bay(t);
			MaterialSwap.Refresh();
			foreach (Category category in MaterialSwap.Categories(true))
				foreach (Family family in category.Families)
					foreach (BuildingDef def in family.Members)
					{
						string why = null;
						if (System.Array.IndexOf(Rules, def.BuildLocationRule) < 0)
							why = "placement " + def.BuildLocationRule;
						else if (System.Array.IndexOf(Scaffolding, def.PrefabID) >= 0)
							why = "the test's own floor and wall";
						else if (def.BuildingComplete.HasTag(GameTags.RocketInteriorBuilding))
							why = "rocket interior only";
						else if (def.WidthInCells > bay.W - 2 || def.HeightInCells > bay.H - 1)
							why = "size " + def.WidthInCells + "x" + def.HeightInCells;
						else if (Materials(def) == null)
							why = "no material to build it of";
						else if (Target(def) == null)
							why = "no second material for its first ingredient";
						if (why != null)
							excluded.Add(def.PrefabID + ": " + why);
						else if (!defs.Contains(def))
							defs.Add(def);
					}
			return defs;
		}

		/// <summary>What a type is built of: the first material, by name, of each ingredient.</summary>
		internal static List<Tag> Materials(BuildingDef def)
		{
			var materials = new List<Tag>();
			foreach (Recipe.Ingredient ingredient in def.CraftRecipe.Ingredients)
			{
				Tag first = Options(ingredient.tag).FirstOrDefault();
				if (!first.IsValid)
					return null;
				materials.Add(first);
			}
			return materials;
		}

		/// <summary>The material the first ingredient is changed to: the second by name.</summary>
		internal static Tag? Target(BuildingDef def)
		{
			List<Tag> options = Options(def.CraftRecipe.Ingredients[0].tag);
			return options.Count > 1 ? options[1] : (Tag?)null;
		}

		private static List<Tag> Options(Tag ingredient)
		{
			var options = new List<Tag>();
			foreach (Tag tag in UpgradePlanner.Materials.Valid(ingredient))
			{
				Element element = ElementLoader.GetElement(tag);
				if (element != null ? element.IsSolid : Assets.TryGetPrefab(tag) != null)
					options.Add(tag);
			}
			options.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
			return options;
		}

		/// <summary>One building of a batch: its type and the cell it is built at (its origin).</summary>
		internal sealed class Slot
		{
			public BuildingDef Def;
			public int Cell;
		}

		/// <summary>A bay's worth of buildings and the floors, walls and backwalls they stand on or against.</summary>
		internal sealed class Batch
		{
			public string Kind;
			public readonly List<Slot> Slots = new List<Slot>();
			public readonly List<int> Tiles = new List<int>();
			public readonly List<int> Backwalls = new List<int>();
		}

		/// <summary>Types of one cell that need no floor: sensors, wall pieces and the like.</summary>
		private static bool Mosaic(BuildingDef def) => def.WidthInCells == 1 && def.HeightInCells == 1
			&& def.BuildLocationRule != BuildLocationRule.OnFloor && def.BuildLocationRule != BuildLocationRule.Tile;

		/// <summary>Floor types this tall or less go two floors to a bay.</summary>
		private const int ShortRow = 3;

		/// <summary>
		/// The types laid out in bays: one-cell pieces as a mosaic a cell apart each way (a wall
		/// piece with a tile left of it, a backwall piece on drywall); floor types along floors of
		/// tiles a cell apart, two floors to a bay for those up to <see cref="ShortRow"/> tall.
		/// </summary>
		internal static IEnumerable<Batch> Layout(List<BuildingDef> defs, CellRect bay)
		{
			Batch batch = null;
			int x = 1, y = 1;
			foreach (BuildingDef def in defs.Where(Mosaic))
			{
				if (batch == null)
				{
					batch = new Batch { Kind = "mosaic" };
					x = 1;
					y = 1;
				}
				int cell = bay.Cell(x, y);
				if (def.BuildLocationRule == BuildLocationRule.OnWall)
					batch.Tiles.Add(bay.Cell(x - 1, y));
				if (def.BuildLocationRule == BuildLocationRule.OnBackWall)
					batch.Backwalls.Add(cell);
				batch.Slots.Add(new Slot { Def = def, Cell = cell });
				x += 2;
				if (x >= bay.W)
				{
					x = 1;
					y += 2;
					if (y >= bay.H)
					{
						yield return batch;
						batch = null;
					}
				}
			}
			if (batch != null)
				yield return batch;
			foreach (bool tall in new[] { false, true })
			{
				int[] floors = tall ? new[] { 0 } : new[] { 0, ShortRow + 1 };
				batch = null;
				int row = 0, left = 1;
				foreach (BuildingDef def in defs.Where(d => !Mosaic(d) && (d.HeightInCells > ShortRow) == tall))
				{
					int need = def.WidthInCells + 1 + (def.BuildLocationRule == BuildLocationRule.OnWall ? 1 : 0);
					if (batch != null && left + need > bay.W)
					{
						row++;
						left = 1;
						if (row >= floors.Length)
						{
							yield return batch;
							batch = null;
						}
					}
					if (batch == null)
					{
						batch = new Batch { Kind = tall ? "tall floor" : "floor" };
						foreach (int floor in floors)
							for (int fx = 0; fx < bay.W; fx++)
								batch.Tiles.Add(bay.Cell(fx, floor));
						row = 0;
						left = 1;
					}
					int baseY = floors[row] + 1;
					if (def.BuildLocationRule == BuildLocationRule.OnWall)
					{
						for (int wy = 0; wy < def.HeightInCells; wy++)
							batch.Tiles.Add(bay.Cell(left, baseY + wy));
						left++;
					}
					int minX = 0, minY = 0;
					foreach (CellOffset offset in def.PlacementOffsets)
					{
						minX = Mathf.Min(minX, offset.x);
						minY = Mathf.Min(minY, offset.y);
					}
					int cell = bay.Cell(left - minX, baseY - minY);
					if (def.BuildLocationRule == BuildLocationRule.OnBackWall)
						foreach (CellOffset offset in def.PlacementOffsets)
							batch.Backwalls.Add(Grid.OffsetCell(cell, offset));
					batch.Slots.Add(new Slot { Def = def, Cell = cell });
					left += need - (def.BuildLocationRule == BuildLocationRule.OnWall ? 1 : 0);
				}
				if (batch != null)
					yield return batch;
			}
		}

		/// <summary>
		/// The batch's floors, walls and backwalls, a pause for the sim to take them in, then each
		/// building in its materials. What could not be placed is logged and left out of
		/// <paramref name="built"/>; <paramref name="failures"/>, when given, gets a line for it.
		/// </summary>
		internal static IEnumerator BuildBatch(TestContext t, Batch batch, List<GameObject> built, List<string> failures)
		{
			foreach (int cell in batch.Tiles.Distinct())
				t.World.Build("Tile", cell, element: SimHashes.Granite);
			foreach (int cell in batch.Backwalls.Distinct())
				t.World.Build("ExteriorWall", cell, element: SimHashes.Granite);
			yield return t.WaitGameSeconds(2);
			foreach (Slot slot in batch.Slots)
			{
				BuildingDef def = slot.Def;
				List<Tag> materials = Materials(def);
				if (!def.IsValidPlaceLocation(null, slot.Cell, Orientation.Neutral, out string reason))
				{
					t.Log("CANNOT PLACE " + def.PrefabID + ": " + reason);
					failures?.Add(def.PrefabID + " could not be placed: " + reason);
					continue;
				}
				float temperature = Mathf.Min(def.Temperature, ElementLoader.GetMinMeltingPointAmongElements(materials) - 10f);
				GameObject go = def.Build(slot.Cell, Orientation.Neutral, null, materials, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
				if (go != null)
					built.Add(go);
			}
		}

		/// <summary>Everything in the bay removed, tiles included.</summary>
		internal static IEnumerator Clear(TestContext t)
		{
			CellRect bay = Bay(t);
			var seen = new HashSet<GameObject>();
			foreach (int cell in bay.Cells)
				for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
				{
					GameObject go = Grid.Objects[cell, layer];
					if (go != null && seen.Add(go))
						t.World.Destroy(go);
				}
			// What the old buildings dropped when they were taken down; stored items go with their building.
			foreach (Pickupable item in Components.Pickupables.Items.ToList())
				if (item != null && !item.KPrefabID.HasTag(GameTags.Stored) && bay.Contains(Grid.PosToCell(item)))
					t.World.Destroy(item.gameObject);
			yield return t.WaitGameSeconds(2);
		}
	}
}
