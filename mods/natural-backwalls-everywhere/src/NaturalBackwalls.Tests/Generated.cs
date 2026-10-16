using System.Collections;
using System.Collections.Generic;
using System.Text;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using ProcGen;

namespace NaturalBackwalls.Tests
{
	/// <summary>
	/// The mod's real product, a generated world: a Spaced Out sandstone cluster generated with the
	/// mod active, then looked at cell by cell. The expectations follow the player's options as the
	/// generator read them, not the defaults.
	/// </summary>
	public static class Generated
	{
		private const string Cluster = "SNDST-C-100-0-0-0";

		private sealed class Tally { public int Cells, Open, Backwalls; }

		/// <summary>Open cells and backwalls behind them per zone type, over one world's cells.</summary>
		private static SortedDictionary<SubWorld.ZoneType, Tally> Count(WorldContainer world)
		{
			SubWorld.ZoneType[] zones = World.Instance.zoneRenderData.worldZoneTypes;
			var tallies = new SortedDictionary<SubWorld.ZoneType, Tally>();
			for (int y = world.WorldOffset.y; y < world.WorldOffset.y + world.WorldSize.y; y++)
				for (int x = world.WorldOffset.x; x < world.WorldOffset.x + world.WorldSize.x; x++)
				{
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidCell(cell) || cell >= zones.Length)
						continue;
					if (!tallies.TryGetValue(zones[cell], out Tally t))
						tallies[zones[cell]] = t = new Tally();
					t.Cells++;
					Element element = Grid.Element[cell];
					if (element != null && element.IsSolid)
						continue;
					t.Open++;
					if (BackwallManager.HasBackwall(cell))
						t.Backwalls++;
				}
			return tallies;
		}

		private static string Describe(SortedDictionary<SubWorld.ZoneType, Tally> tallies)
		{
			var sb = new StringBuilder();
			foreach (var pair in tallies)
				sb.Append(pair.Key).Append(' ').Append(pair.Value.Backwalls).Append('/').Append(pair.Value.Open).Append(" open; ");
			return sb.ToString();
		}

		[OniTest("NB: a generated cluster's starting asteroid has backwalls in its biomes, the start biome walled as set, none in space", Worldgen = Cluster), Layer(0)]
		public static IEnumerator StartingAsteroid(TestContext t)
		{
			GeneratedWorld generated = t.Generated;
			t.Assert.NotNull(generated, "the test runs in a generated world");
			t.Assert.Eq(0, generated.Errors.Count, "the worldgen reported no errors: " + string.Join(" | ", generated.Errors));
			t.Log("generated " + generated.Applied + " in " + generated.Seconds.ToString("F0") + " s");
			Options options = Options.Load();
			WorldContainer start = ClusterManager.Instance.GetStartWorld();
			var tallies = Count(start);
			t.Log("start asteroid, backwalls/open cells by zone: " + Describe(tallies));

			// The starting biome's coverage applies to the starting subworld, the overworld cells the
			// generator tags AtStart (other subworlds of the same zone type use their own coverage). The
			// printing pod's template carves its cave there, so the template's footprint has holes.
			var startPolygons = new List<Delaunay.Geo.Polygon>();
			foreach (Klei.WorldDetailSave.OverworldCell overworld in SaveLoader.Instance.clusterDetailSave.overworldCells)
				if (overworld.tags != null && overworld.tags.Contains("AtStart"))
					startPolygons.Add(overworld.poly);
			t.Assert.True(startPolygons.Count > 0, "the starting subworld's cells are known");
			Telepad pod = Components.Telepads.Count > 0 ? Components.Telepads[0] : null;
			t.Assert.NotNull(pod, "the printing pod");
			string templateName = SettingsCache.worlds.GetWorldData(start.worldName).startingBaseTemplate;
			UnityEngine.RectInt template = TemplateCache.GetTemplate(templateName).info.GetBounds(Grid.PosToXY(pod.transform.GetPosition()), 1);
			// Only the starting zone's own cells: a polygon's edge cells can take the neighbouring biome.
			SubWorld.ZoneType[] zones = World.Instance.zoneRenderData.worldZoneTypes;
			SubWorld.ZoneType startZone = zones[Grid.PosToCell(pod)];
			int open = 0, walled = 0;
			var holes = new List<int>();
			for (int y = start.WorldOffset.y; y < start.WorldOffset.y + start.WorldSize.y; y++)
				for (int x = start.WorldOffset.x; x < start.WorldOffset.x + start.WorldSize.x; x++)
				{
					var point = new UnityEngine.Vector2(x + 0.5f, y + 0.5f);
					int cell = Grid.XYToCell(x, y);
					if (zones[cell] != startZone || Grid.Element[cell].IsSolid || !startPolygons.Exists(polygon => polygon.PointInPolygon(point)))
						continue;
					open++;
					if (BackwallManager.HasBackwall(cell))
						walled++;
					else if (!template.Contains(new UnityEngine.Vector2Int(x, y)))
						holes.Add(cell);
				}
			t.Log("starting subworld (" + startZone + "): " + walled + " of " + open + " open cells walled; template " + templateName + " at " + template);
			t.Assert.True(open > 0, "the starting subworld has open cells");
			if (options.StartCoverage >= 0.99f)
			{
				// Each hole on screen before the test fails, so it can be looked at.
				var shown = new List<int>();
				foreach (int hole in holes)
				{
					if (shown.Count >= 4 || shown.Exists(seen => Grid.GetCellDistance(seen, hole) < 8))
						continue;
					shown.Add(hole);
					yield return t.LookAt(hole, 8f);
					yield return t.Screenshot("hole-" + Cells.Describe(hole).Replace(" ", ""));
				}
				t.Assert.Eq(0, holes.Count, "at coverage " + options.StartCoverage + " every open cell of the starting subworld outside the printing pod's template has a backwall"
					+ (holes.Count > 0 ? " (first at " + Cells.Describe(holes[0]) + ")" : ""));
			}
			else if (options.StartCoverage > 0f)
				t.Assert.True(walled > 0, "the starting subworld has backwalls at coverage " + options.StartCoverage);

			int walledZones = 0;
			foreach (var pair in tallies)
				if (pair.Key != SubWorld.ZoneType.Space && pair.Value.Backwalls > 0)
					walledZones++;
			t.Assert.True(walledZones >= 3, "biomes of the asteroid have backwalls too (" + walledZones + " zone types)");
			if (tallies.TryGetValue(SubWorld.ZoneType.Space, out Tally space))
				t.Assert.Eq(0, space.Backwalls, "no backwall in space");
			yield break;
		}

		[OniTest("NB: with Backwalls on other planetoids off, the cluster's other asteroids get none", Worldgen = Cluster), Layer(0)]
		public static IEnumerator OtherPlanetoids(TestContext t)
		{
			Options options = Options.Load();
			if (options.OtherPlanetoids)
				t.Skip("the player's options put backwalls on other planetoids");
			WorldContainer start = ClusterManager.Instance.GetStartWorld();
			int others = 0;
			foreach (WorldContainer world in ClusterManager.Instance.WorldContainers)
			{
				if (world == start || world.IsModuleInterior)
					continue;
				others++;
				int backwalls = 0;
				foreach (var pair in Count(world))
					backwalls += pair.Value.Backwalls;
				t.Log(world.GetProperName() + ": " + backwalls + " backwall(s)");
				t.Assert.Eq(0, backwalls, "no backwalls on " + world.GetProperName());
			}
			t.Assert.True(others > 0, "the cluster has other asteroids to look at");
			yield break;
		}

		/// <summary>The fog of war lifted over the world's cells, so a screenshot shows what is there.</summary>
		private static void Reveal(WorldContainer world)
		{
			for (int y = world.WorldOffset.y; y < world.WorldOffset.y + world.WorldSize.y; y++)
				for (int x = world.WorldOffset.x; x < world.WorldOffset.x + world.WorldSize.x; x++)
					Grid.Reveal(Grid.XYToCell(x, y));
		}

		/// <summary>
		/// For looking at a generated world: the starting area, then one spot in each biome, the
		/// backwall cell nearest the middle of that zone's backwalls. Run with -Filter "*Overview*".
		/// </summary>
		[OniTest("looks: the generated starting asteroid, the start area and one spot per biome", Worldgen = Cluster, Explicit = true, Timeout = 120), Layer(0)]
		public static IEnumerator Overview(TestContext t)
		{
			WorldContainer start = ClusterManager.Instance.GetStartWorld();
			Reveal(start);
			yield return t.WaitRealSeconds(1);
			Telepad pod = Components.Telepads[0];
			yield return t.LookAt(Grid.PosToCell(pod), 14f);
			yield return t.Screenshot("start");
			SubWorld.ZoneType[] zones = World.Instance.zoneRenderData.worldZoneTypes;
			var cells = new Dictionary<SubWorld.ZoneType, List<int>>();
			for (int y = start.WorldOffset.y; y < start.WorldOffset.y + start.WorldSize.y; y++)
				for (int x = start.WorldOffset.x; x < start.WorldOffset.x + start.WorldSize.x; x++)
				{
					int cell = Grid.XYToCell(x, y);
					if (zones[cell] == SubWorld.ZoneType.Space || Grid.Element[cell].IsSolid || !BackwallManager.HasBackwall(cell))
						continue;
					if (!cells.TryGetValue(zones[cell], out List<int> list))
						cells[zones[cell]] = list = new List<int>();
					list.Add(cell);
				}
			foreach (var pair in cells)
			{
				float sx = 0, sy = 0;
				foreach (int cell in pair.Value)
				{
					Grid.CellToXY(cell, out int x, out int y);
					sx += x;
					sy += y;
				}
				int middle = Grid.XYToCell((int)(sx / pair.Value.Count), (int)(sy / pair.Value.Count));
				int nearest = pair.Value[0];
				foreach (int cell in pair.Value)
					if (Grid.GetCellDistance(cell, middle) < Grid.GetCellDistance(nearest, middle))
						nearest = cell;
				yield return t.LookAt(nearest, 12f);
				yield return t.Screenshot(pair.Key.ToString());
			}
		}
	}
}
