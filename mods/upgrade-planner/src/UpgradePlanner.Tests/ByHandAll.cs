using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;
using static UpgradePlanner.Tests.ByHand;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// The by-hand tests over everything the planner offers, for when a game or mod update may have
	/// changed what Duplicants do with its orders. Both are Explicit (most of an hour together):
	///
	/// EveryType: every type All buildings offers, in batches along the box's floor, its settings
	/// changed, then changed to its second material and built again by the crew, settings kept.
	///
	/// EveryReplacement: every pairing the Replace type tab offers (a type and each type that can be
	/// built over it, its own included), three of the type laid down and built over by the crew.
	///
	/// A crew and a pile of materials at each end of the box, so a door that closes cuts nothing
	/// off. Problems are collected and reported together at the end rather than stopping at the first.
	/// </summary>
	public static class ByHandAll
	{
		/// <summary>Problems, each logged as it is found, so a run stopped halfway still has them.</summary>
		private sealed class Problems : List<string>
		{
			private readonly TestContext t;

			public Problems(TestContext t)
			{
				this.t = t;
			}

			public new void Add(string problem)
			{
				base.Add(problem);
				t.Log("FAILED " + problem);
			}

			public new void AddRange(IEnumerable<string> problems)
			{
				foreach (string problem in problems)
					Add(problem);
			}
		}

		/// <summary>Real seconds the crews get for one batch, at speed 3.</summary>
		private const double BatchSeconds = 300;

		/// <summary>Settings left as they are: a door locked against the crew would shut them out.</summary>
		private static readonly string[] Leave = { "AccessControl" };

		[OniTest("Upgrade Planner: Duplicants build every type All buildings offers again in another material, with its settings", Timeout = 5400, Boxed = true, Explicit = true), Layer(1)]
		public static IEnumerator EveryType(TestContext t)
		{
			BuildingSettings.RequireBlueprints(t);
			t.ResearchAll();
			List<BuildingDef> defs = BuildingSettings.Eligible(t, out List<string> excluded);
			// Dev-only types can be built only in debug mode with sandbox or instant build on, and then
			// at once: never by hand, so the planner rightly leaves them be with instant build off.
			foreach (BuildingDef def in defs.Where(def => def.DebugOnly))
				excluded.Add(def.PrefabID + ": dev-only, never built by hand");
			defs.RemoveAll(def => def.DebugOnly);
			t.Log(defs.Count + " types; left out: " + string.Join("; ", excluded));
			return Types(t, defs);
		}

		private static IEnumerator Types(TestContext t, List<BuildingDef> defs)
		{
			BuildingSettings.RequireBlueprints(t);
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			t.InstantBuild(false);
			t.Speed(3);
			var failures = new Problems(t);
			int batches = 0, rebuilt = 0, settings = 0;
			foreach (BuildingSettings.Batch batch in Layout(t, defs, inside, failures))
			{
				batches++;
				var built = new List<GameObject>();
				var unplaced = new List<string>();
				yield return BuildingSettings.BuildBatch(t, batch, built, unplaced);
				failures.AddRange(unplaced);
				yield return t.WaitGameSeconds(2);
				var expected = built.Where(go => go != null).Select(go => new Expected(go, ChangeSettings(t, go, Leave))).ToList();
				yield return Settle(built);

				SpawnCrews(t, inside);
				foreach (Expected e in expected)
				{
					Supply(t, e.Def, Remake(e.Def), 1, inside.Cell(0, 0));
					Supply(t, e.Def, Remake(e.Def), 1, inside.Cell(inside.W - 1, 0));
				}
				yield return t.WaitGameSeconds(2);

				foreach (var group in expected.Select(e => e.Def).Distinct().GroupBy(def => def.CraftRecipe.Ingredients[0].tag))
				{
					UpgradePlan plan = UpgradePlan.Current;
					plan.Mode = PlanMode.ChangeMaterial;
					plan.Clear();
					plan.ChangeMaterial.SetAllBuildings(true);
					MaterialSwap.Refresh();
					plan.ChangeMaterial.Pick(group.Select(Families.Of).Distinct().ToList(), true);
					plan.ChangeMaterial.SetSourceAny();
					plan.ChangeMaterial.SetTarget(BuildingSettings.Target(group.First()).Value);
					if (!plan.IsComplete)
					{
						failures.Add(string.Join(", ", group.Select(def => def.PrefabID)) + ": the plan is not complete: " + plan.Summary());
						continue;
					}
					yield return Drag(t, inside.Cell(2, 0), inside.Cell(inside.W - 3, inside.H - 1));
					yield return t.WaitGameSeconds(1);
				}

				float start = GameClock.Instance.GetTime();
				yield return Until(() => expected.All(e => e.Done), () => expected.Count(e => e.Done), BatchSeconds);
				yield return t.WaitGameSeconds(2);
				foreach (Expected e in expected)
				{
					if (!e.Done)
					{
						failures.Add(e.Def.PrefabID + ": not built again in " + BuildingSettings.Target(e.Def).Value.Name + " (" + Now(e.Cell, e.Def) + ")");
						continue;
					}
					rebuilt++;
					settings += e.Values.Count;
				}
				failures.AddRange(Lost(expected.Where(e => e.Done)));
				t.Log("batch " + batches + ": " + expected.Count(e => e.Done) + " of " + expected.Count + " built again in "
					+ (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds: " + string.Join(", ", expected.Select(e => e.Def.PrefabID)));
				if (batches == 1)
					yield return t.Screenshot("first-batch");
				t.Dupes.RemoveAll();
				yield return t.Dupes.ClearBox();
			}
			Report(t, failures, rebuilt + " of " + defs.Count + " types built again by hand in " + batches + " batches, " + settings + " settings checked");
		}

		/// <summary>
		/// The same few types many times over, rebuilt at speed 3: a building rebuilt by hand once lost
		/// every setting now and then, one in a hundred or so, in EveryType.
		/// </summary>
		[OniTest("Upgrade Planner: many buildings rebuilt by hand at speed 3 all keep their settings", Timeout = 1200, Boxed = true, Explicit = true), Layer(1)]
		public static IEnumerator SettingsUnderLoad(TestContext t)
		{
			var defs = new List<BuildingDef>();
			for (int i = 0; i < 4; i++)
				defs.AddRange(new[] { "FlushToilet", "MarbleSculpture", "SolidFilter", "HighEnergyParticleRedirector", "Outhouse", "LiquidBottler" }.Select(Assets.GetBuildingDef));
			return Types(t, defs);
		}

		/// <summary>Types EveryType once found a problem with, rebuilt on their own for a closer look.</summary>
		private static readonly string[] SuspectTypes = { "UnderwaterCritterCondo", "DevPumpLiquid", "Outhouse", "FlushToilet", "Shower", "GunkEmptier", "LiquidPumpingStation", "BottleEmptier", "BottleEmptierConduitLiquid", "LiquidBottler" };

		[OniTest("Upgrade Planner: Duplicants rebuild the types EveryType had a problem with, with their settings", Timeout = 600, Boxed = true, Explicit = true), Layer(1)]
		public static IEnumerator SuspectTypesRebuilt(TestContext t)
		{
			return Types(t, SuspectTypes.Select(Assets.GetBuildingDef).ToList());
		}

		[OniTest("Upgrade Planner: Duplicants build every type the Replace type tab offers over every type it offers it for", Timeout = 5400, Boxed = true, Explicit = true), Layer(1)]
		public static IEnumerator EveryReplacement(TestContext t)
		{
			return Replacements(t, null);
		}

		/// <summary>Pairings EveryReplacement once found a problem with, run on their own for a closer look.</summary>
		private static readonly string[] Suspects =
		{
			"Tile to StorageTile", "Wire to WireRefinedHighWattage", "BunkerTile to WireRefinedBridgeHighWattage", "SolidConduit to SolidConduit",
		};

		[OniTest("Upgrade Planner: Duplicants build the pairings EveryReplacement had a problem with", Timeout = 900, Boxed = true, Explicit = true), Layer(1)]
		public static IEnumerator SuspectReplacements(TestContext t)
		{
			return Replacements(t, pair => Suspects.Any(s => pair.ToString().StartsWith(s + " (")));
		}

		private static IEnumerator Replacements(TestContext t, System.Func<Pair, bool> only)
		{
			t.ResearchAll();
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			t.Speed(3);
			var pairs = new List<Pair>();
			var failures = new Problems(t);
			var leftOut = new List<string>();
			foreach (Category sources in ReplacementRules.SourceCategories())
				foreach (Family source in sources.Families)
					foreach (Category targets in ReplacementRules.TargetCategories(source))
						foreach (Family target in targets.Families)
						{
							Pair pair = Pair.Make(source, target, out string why);
							if (only != null && (pair == null || !only(pair)))
								continue;
							if (pair != null)
								pairs.Add(pair);
							else
								leftOut.Add(source.PrefabID + " to " + target.PrefabID + ": " + why);
						}
			t.Log(pairs.Count + " pairings: " + string.Join(", ", pairs.Select(p => p.ToString())));
			t.Log(leftOut.Count + " left out: " + string.Join("; ", leftOut));

			int batches = 0, done = 0;
			// Three of each source in a row, a cell between pairings, as many pairings as fit between the crews.
			int first = 3, last = inside.W - 4;
			for (int i = 0; i < pairs.Count;)
			{
				var batch = new List<Pair>();
				int x = first;
				while (i < pairs.Count && x + pairs[i].Width <= last)
				{
					pairs[i].Place(inside, x);
					batch.Add(pairs[i]);
					x += pairs[i].Width + 1;
					i++;
				}
				if (batch.Count == 0)
				{
					failures.Add(pairs[i] + ": too wide for the box");
					i++;
					continue;
				}
				batches++;

				t.InstantBuild(true);
				foreach (Pair pair in batch)
					yield return pair.Lay(t, failures);
				yield return t.WaitGameSeconds(2);
				foreach (Pair pair in batch)
					pair.Remember();
				t.InstantBuild(false);
				SpawnCrews(t, inside);
				foreach (Pair pair in batch)
				{
					Supply(t, pair.Target, pair.Materials, pair.Cells.Count, inside.Cell(0, 0));
					Supply(t, pair.Target, pair.Materials, pair.Cells.Count, inside.Cell(inside.W - 1, 0));
				}
				yield return t.WaitGameSeconds(2);

				foreach (Pair pair in batch)
				{
					if (!pair.Laid)
						continue;
					UpgradePlan plan = UpgradePlan.Current;
					plan.Mode = PlanMode.ReplaceType;
					plan.ReplaceType.SetSource(pair.SourceFamily);
					plan.ReplaceType.SetSourceAny();
					plan.ReplaceType.SetTarget(pair.TargetFamily);
					plan.ReplaceType.SetTargetMaterials(pair.Materials);
					if (!plan.IsComplete)
					{
						failures.Add(pair + ": the plan is not complete: " + plan.Summary());
						pair.Laid = false;
						continue;
					}
					yield return Drag(t, pair.Cells.First(), pair.Cells.Last());
					int orders = pair.Cells.Count(cell => Grid.Objects[cell, (int)pair.Target.ReplacementLayer]?.GetComponent<Constructable>() != null);
					if (orders < pair.Cells.Count)
						failures.Add(pair + ": " + orders + " of " + pair.Cells.Count + " orders placed");
				}

				float start = GameClock.Instance.GetTime();
				yield return Until(() => batch.All(pair => !pair.Laid || pair.Done), () => batch.Sum(pair => pair.Laid ? pair.Built : 0), BatchSeconds);
				yield return t.WaitGameSeconds(1);
				foreach (Pair pair in batch.Where(pair => pair.Laid))
				{
					string problem = pair.Check();
					if (problem == null)
						done++;
					else
						failures.Add(pair + ": " + problem);
				}
				t.Log("batch " + batches + " in " + (GameClock.Instance.GetTime() - start).ToString("0") + " game seconds: " + string.Join(", ", batch.Select(p => p.ToString())));
				if (batches == 1)
					yield return t.Screenshot("first-batch");
				t.Dupes.RemoveAll();
				yield return t.Dupes.ClearBox();
			}
			Report(t, failures, done + " of " + pairs.Count + " pairings built over by hand in " + batches + " batches");
		}

		/// <summary>
		/// One pairing of the Replace type tab: three of the source type in a row, the target built
		/// over them in its first materials (its second for the first ingredient when it is the source
		/// type itself, so something changes).
		/// </summary>
		private sealed class Pair
		{
			public Family SourceFamily, TargetFamily;
			public BuildingDef Source, Target;
			public List<Tag> SourceMaterials, Materials;
			public readonly List<int> Cells = new List<int>();
			public readonly Dictionary<int, UtilityConnections> Connections = new Dictionary<int, UtilityConnections>();
			public bool Laid;

			/// <summary>Cells between one piece and the next: room for the wider of the two types.</summary>
			public int Stride => Mathf.Max(Source.WidthInCells, Target.WidthInCells);

			/// <summary>
			/// A joint plate's wire ends go either side of it, which the game will not have in a tile or
			/// on another plate's end: its pieces are laid as a wall two high, above head height so the
			/// crews pass under and within their reach, with a clear cell on each side for the ends.
			/// </summary>
			public bool Wall => Target.BuildLocationRule == BuildLocationRule.HighWattBridgeTile;

			public int Width => Wall ? 3 : Stride * 3;

			public static Pair Make(Family source, Family target, out string why)
			{
				why = null;
				var pair = new Pair { SourceFamily = source, TargetFamily = target, Source = source.Representative, Target = target.Representative };
				pair.SourceMaterials = BuildingSettings.Materials(pair.Source);
				if (pair.SourceMaterials == null)
					why = "nothing to build " + pair.Source.PrefabID + " of";
				else if (pair.Target == pair.Source)
				{
					Tag? second = BuildingSettings.Target(pair.Target);
					if (second == null)
						why = "it comes in one material, so building it over itself changes nothing";
					else
					{
						pair.Materials = new List<Tag>(pair.SourceMaterials);
						pair.Materials[0] = second.Value;
					}
				}
				else
				{
					pair.Materials = BuildingSettings.Materials(pair.Target);
					if (pair.Materials == null)
						why = "nothing to build " + pair.Target.PrefabID + " of";
				}
				if (why == null)
				{
					Tag melts = pair.SourceMaterials.Concat(pair.Materials).FirstOrDefault(m => ElementLoader.GetElement(m)?.highTemp < RoomTemperature + 10f);
					if (melts.IsValid)
						why = melts.Name + " melts in the box's room-temperature air";
				}
				return why == null ? pair : null;
			}

			/// <summary>The box's air and the dev life support's target temperature, about.</summary>
			private const float RoomTemperature = 294.15f;

			/// <summary>Tiles in the floor row, everything else at head height.</summary>
			public void Place(CellRect inside, int x)
			{
				Cells.Clear();
				int y = Source.IsTilePiece ? 0 : 1;
				// A source taller than a cell (a backwall window) is laid once: a second would not be in reach.
				int pieces = Wall ? (Source.HeightInCells > 1 ? 1 : 2) : 3;
				for (int i = 0; i < pieces; i++)
					Cells.Add(Wall ? inside.Cell(x + 1, 2 + i) : inside.Cell(x + i * Stride + Stride / 2, y));
				Laid = false;
			}

			/// <summary>The three sources: wires, pipes and rails dragged with the real tool so their connections are real.</summary>
			public IEnumerator Lay(TestContext t, Problems failures)
			{
				Element element = ElementLoader.GetElement(SourceMaterials[0]);
				if (Source.isUtility && element != null && SourceMaterials.Count == 1)
				{
					UtilityDragSession lay = t.Tools.UtilityTool(Source.PrefabID, element.id);
					yield return lay.Path(Cells.ToArray());
					lay.Dispose();
				}
				else
				{
					foreach (int cell in Cells)
					{
						if (!Source.IsValidPlaceLocation(null, cell, Orientation.Neutral, out string reason))
						{
							failures.Add(this + ": cannot place " + Source.PrefabID + " at " + Isochronous.TestKit.Cells.Describe(cell) + ": " + reason);
							yield break;
						}
						float temperature = Mathf.Min(Source.Temperature, ElementLoader.GetMinMeltingPointAmongElements(SourceMaterials) - 10f);
						Source.Build(cell, Orientation.Neutral, null, SourceMaterials, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
					}
				}
				Laid = true;
			}

			/// <summary>What was laid, and the connections each piece drew.</summary>
			public void Remember()
			{
				if (!Laid)
					return;
				foreach (int cell in Cells)
				{
					GameObject go = Grid.Objects[cell, (int)Source.ObjectLayer];
					if (go == null || go.GetComponent<BuildingComplete>() == null)
					{
						Laid = false;
						return;
					}
					KAnimGraphTileVisualizer visualizer = go.GetComponent<KAnimGraphTileVisualizer>();
					if (visualizer != null)
						Connections[cell] = visualizer.Connections;
				}
			}

			public bool Done => Built == Cells.Count;

			/// <summary>Pieces built over so far.</summary>
			public int Built => Cells.Count(cell => IsTarget(Grid.Objects[cell, (int)Target.ObjectLayer]) && Grid.Objects[cell, (int)Target.ReplacementLayer] == null);

			private bool IsTarget(GameObject go)
			{
				return go != null && go.GetComponent<BuildingComplete>() != null && TargetFamily.Contains(go.GetComponent<Building>().Def)
					&& go.GetComponent<PrimaryElement>().Element.tag == Materials[0];
			}

			/// <summary>Null when every piece is the target in its material with the source's connections.</summary>
			public string Check()
			{
				foreach (int cell in Cells)
				{
					GameObject go = Grid.Objects[cell, (int)Target.ObjectLayer];
					if (!IsTarget(go))
						return "not built over at " + Isochronous.TestKit.Cells.Describe(cell) + " (" + Now(cell, Target) + ")";
					KAnimGraphTileVisualizer visualizer = go.GetComponent<KAnimGraphTileVisualizer>();
					if (visualizer != null && Connections.TryGetValue(cell, out UtilityConnections was) && visualizer.Connections != was)
						return "connections at " + Isochronous.TestKit.Cells.Describe(cell) + " were " + was + ", are " + visualizer.Connections;
				}
				return null;
			}

			public override string ToString() => Source.PrefabID + " to " + Target.PrefabID + " (" + string.Join(", ", Materials.Select(m => m.Name)) + ")";
		}

		/// <summary>
		/// The batch's types along the box's floor, left to right between the crews: tiles in the
		/// floor row, wall pieces on a column of tiles above head height (so the crews can pass
		/// under), backwall pieces on drywall, everything else standing on the box's floor. Types
		/// too tall for the box are a failure.
		/// </summary>
		private static IEnumerable<BuildingSettings.Batch> Layout(TestContext t, List<BuildingDef> defs, CellRect inside, Problems failures)
		{
			int first = 3, last = inside.W - 4;
			BuildingSettings.Batch batch = null;
			int x = first;
			foreach (BuildingDef def in defs)
			{
				bool wall = def.BuildLocationRule == BuildLocationRule.OnWall;
				int baseY = wall ? 2 : 0;
				if (baseY + def.HeightInCells > inside.H)
				{
					failures.Add(def.PrefabID + ": " + def.WidthInCells + "x" + def.HeightInCells + " does not fit the box");
					continue;
				}
				int need = def.WidthInCells + (wall ? 1 : 0);
				if (need > last - first)
				{
					failures.Add(def.PrefabID + ": " + def.WidthInCells + "x" + def.HeightInCells + " does not fit the box");
					continue;
				}
				if (batch == null || x + need > last)
				{
					if (batch != null)
						yield return batch;
					batch = new BuildingSettings.Batch { Kind = "by hand" };
					x = first;
				}
				if (wall)
				{
					for (int wy = 0; wy < def.HeightInCells; wy++)
						batch.Tiles.Add(inside.Cell(x, baseY + wy));
					x++;
				}
				int minX = 0, minY = 0;
				foreach (CellOffset offset in def.PlacementOffsets)
				{
					minX = Mathf.Min(minX, offset.x);
					minY = Mathf.Min(minY, offset.y);
				}
				int cell = inside.Cell(x - minX, baseY - minY);
				if (def.BuildLocationRule == BuildLocationRule.OnBackWall)
					foreach (CellOffset offset in def.PlacementOffsets)
						batch.Backwalls.Add(Grid.OffsetCell(cell, offset));
				batch.Slots.Add(new BuildingSettings.Slot { Def = def, Cell = cell });
				x += def.WidthInCells + 1;
			}
			if (batch != null)
				yield return batch;
		}

		/// <summary>A crew of three at each end of the box, with every skill: some types ask one of whoever builds them.</summary>
		private static void SpawnCrews(TestContext t, CellRect inside)
		{
			t.Dupes.Spawn(Crew, inside.Cell(1, 0));
			t.Dupes.Spawn(Crew, inside.Cell(inside.W - 2, 0));
			t.Dupes.GrantAll();
		}

		/// <summary>Game seconds without a building finished before a batch is given up as stuck.</summary>
		private const float StallSeconds = 90f;

		/// <summary>
		/// Until the condition holds, nothing more is finished (<paramref name="progress"/> unchanged)
		/// for <see cref="StallSeconds"/>, or the real seconds run out. None of these is a failure here;
		/// the caller reports what is left.
		/// </summary>
		private static IEnumerator Until(System.Func<bool> condition, System.Func<int> progress, double seconds)
		{
			float until = Time.realtimeSinceStartup + (float)seconds;
			int last = progress();
			float since = GameClock.Instance.GetTime();
			while (!condition() && Time.realtimeSinceStartup < until && GameClock.Instance.GetTime() - since < StallSeconds)
			{
				yield return null;
				int now = progress();
				if (now != last)
				{
					last = now;
					since = GameClock.Instance.GetTime();
				}
			}
		}

		/// <summary>What stands at the cell on the type's layer, for a failure.</summary>
		private static string Now(int cell, BuildingDef def)
		{
			GameObject go = Grid.Objects[cell, (int)def.ObjectLayer];
			GameObject order = def.ReplacementLayer != ObjectLayer.NumLayers ? Grid.Objects[cell, (int)def.ReplacementLayer] : null;
			string what = go == null ? "nothing" : go.name + " of " + go.GetComponent<PrimaryElement>()?.ElementID
				+ (go.GetComponent<Constructable>() != null ? ", an order" : "");
			return what + (order != null ? "; replacement " + order.name + Status(order) : "");
		}

		/// <summary>An order's status items (what the game says it waits for) and what it has been brought.</summary>
		private static string Status(GameObject order)
		{
			var parts = new List<string>();
			KSelectable selectable = order.GetComponent<KSelectable>();
			StatusItemGroup group = selectable != null ? selectable.GetStatusItemGroup() : null;
			if (group != null)
				foreach (StatusItemGroup.Entry entry in group)
					if (entry.item != null)
						parts.Add(entry.item.Id);
			Storage storage = order.GetComponent<Storage>();
			if (storage != null)
				foreach (GameObject item in storage.items)
					if (item != null)
						parts.Add("holds " + item.GetComponent<PrimaryElement>()?.Mass.ToString("0.#") + " kg " + item.PrefabID().Name);
			return parts.Count == 0 ? "" : " [" + string.Join(", ", parts) + "]";
		}

		private static void Report(TestContext t, List<string> failures, string summary)
		{
			t.Log(summary);
			t.Assert.Eq(0, failures.Count, failures.Count + " problems, first: " + failures.FirstOrDefault());
		}
	}
}
