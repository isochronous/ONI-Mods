using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using ProcGen;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// The scene for the Workshop preview, run on request (-Filter PREVIEW): one Ovagro in an
	/// upside-down Hydroponic Farm set in a row of Glass Tiles, its vines grown out along the underside
	/// of the glass past both edges of the picture, against a granite backwall in the frozen biome at
	/// the bottom of the test map, in daylight. The screenshots are taken with the game's own
	/// screenshot mode, so no HUD or status icon is in them.
	/// </summary>
	public static class Preview
	{
		private const int Width = 30;
		private const int Height = 15;
		private const int GlassRow = 8;
		/// <summary>The Ovagro's own temperature, inside the 25-45 °C it grows in.</summary>
		private const float WarmAir = 308.15f;

		/// <summary>
		/// Oxygen per cell. Neither the Ovagro nor its vines check pressure (pressure_sensitive false in
		/// VineMotherConfig and VineBranchConfig), only the gas, so a thin fill keeps the gas out of the picture.
		/// </summary>
		private const float ThinAirKg = 0.05f;

		/// <summary>
		/// The lowest rectangle of the active world that is all frozen biome, buildable and empty, big
		/// enough for a Width x Height scene inside a wall of tiles (the test map is all vacuum, so the
		/// scene's air needs walls round it).
		/// </summary>
		private static CellRect FindFrozenSpot()
		{
			WorldContainer world = ClusterManager.Instance.activeWorld;
			int minX = (int)world.minimumBounds.x, minY = (int)world.minimumBounds.y;
			int maxX = (int)world.maximumBounds.x, maxY = (int)world.maximumBounds.y;
			const int w = Width + 2, h = Height + 2;
			for (int y = minY; y + h - 1 <= maxY; y++)
				for (int x = minX; x + w - 1 <= maxX; x++)
					if (Fits(world, x, y, w, h))
						return new CellRect(x, y, w, h);
			throw new OniAssertException("no " + w + "x" + h + " empty frozen biome area in the active world");
		}

		private static bool Fits(WorldContainer world, int x0, int y0, int w, int h)
		{
			for (int y = y0; y < y0 + h; y++)
			{
				for (int x = x0; x < x0 + w; x++)
				{
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidBuildingCell(cell) || Grid.WorldIdx[cell] != world.id || Grid.Solid[cell])
						return false;
					if (global::World.Instance.zoneRenderData.GetSubWorldZoneType(cell) != SubWorld.ZoneType.FrozenWastes)
						return false;
					for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
						if (Grid.Objects[cell, layer] != null)
							return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Granite tiles round the edge of the outer rectangle (the map is all vacuum, so the scene's air
		/// needs walls), then thin warm oxygen in every cell inside and a granite backwall behind it.
		/// Returns the inside through <paramref name="inside"/>. Yield it.
		/// </summary>
		private static IEnumerator Stage(TestContext t, CellRect outer, CellRect[] inside)
		{
			for (int y = 0; y < outer.H; y++)
				for (int x = 0; x < outer.W; x++)
					if (x == 0 || y == 0 || x == outer.W - 1 || y == outer.H - 1)
						t.World.Build("Tile", outer.Cell(x, y), element: SimHashes.Granite);
			CellRect rect = new CellRect(outer.X + 1, outer.Y + 1, outer.W - 2, outer.H - 2);
			inside[0] = rect;
			yield return t.WaitGameSeconds(1);
			Element granite = ElementLoader.FindElementByHash(SimHashes.Granite);
			for (int y = 0; y < rect.H; y++)
			{
				for (int x = 0; x < rect.W; x++)
				{
					int cell = rect.Cell(x, y);
					Grid.Reveal(cell);
					SimMessages.ReplaceElement(cell, SimHashes.Oxygen, CellEventLogger.Instance.SandBoxTool, ThinAirKg, WarmAir);
					SimMessages.SetBackwallData(cell, granite.idx, granite.defaultValues.mass, WarmAir);
				}
			}
			yield return t.WaitUntil(() => IsStaged(rect), 10, "warm oxygen and a granite backwall in " + rect);
			yield return t.WaitGameSeconds(1);
		}

		private static bool IsStaged(CellRect rect)
		{
			for (int y = 0; y < rect.H; y++)
			{
				for (int x = 0; x < rect.W; x++)
				{
					int cell = rect.Cell(x, y);
					if (Grid.Element[cell].id != SimHashes.Oxygen || !BackwallManager.HasBackwall(cell)
						|| BackwallManager.At(cell).Element.id != SimHashes.Granite)
						return false;
				}
			}
			return true;
		}

		/// <summary>The clock moved forward to the middle of the day (a cycle is 600 s; night starts at 87.5 %).</summary>
		private static void Midday()
		{
			GameClock clock = GameClock.Instance;
			float cycleStart = clock.GetCycle() * 600f;
			float target = cycleStart + 250f;
			if (clock.GetTime() > target)
				target += 600f;
			clock.SetTime(target);
		}

		[OniTest("PREVIEW: the Workshop preview scene, a hanging Ovagro in a Hydroponic Farm set in Glass Tiles", Timeout = 300, Explicit = true, FreshSave = true), Layer(0)]
		public static IEnumerator Scene(TestContext t)
		{
			t.Speed(3);
			CellRect outer = FindFrozenSpot();
			var inside = new CellRect[1];
			yield return Stage(t, outer, inside);
			CellRect rect = inside[0];
			t.Log("scene at " + rect);

			int mx = Width / 2;
			int farmCell = rect.Cell(mx, GlassRow);
			for (int x = 0; x < Width; x++)
				if (x != mx)
					t.World.Build("GlassTile", rect.Cell(x, GlassRow), element: SimHashes.Glass);
			PlantablePlot farm = Kit.Plot(t, "HydroponicFarm", farmCell, Orientation.FlipV);
			// A Hydroponic Farm with no pipe reports NeedLiquidIn and its plant wilts, water in storage or not.
			t.World.Build("LiquidConduit", farmCell);
			GameObject seed = Kit.Seed(t, rect.Cell(mx + 2, 0));
			yield return t.WaitGameSeconds(2);
			farm.CreateOrder(CeilingForm.SeedTag, Tag.Invalid);
			yield return t.WaitGameSeconds(1);
			farm.ForceDeposit(seed);
			yield return t.WaitGameSeconds(1);
			Kit.Water(farm, 50f);
			GameObject plant = farm.Occupant;
			t.Assert.True(Kit.Is(plant, CeilingForm.PlantTag), "the farm holds a hanging Ovagro");

			VineMother.Instance smi = plant.GetSMI<VineMother.Instance>();
			float start = Time.realtimeSinceStartup;
			float nextLog = 0f;
			while (smi.LeftBranch == null || smi.RightBranch == null)
			{
				float elapsed = Time.realtimeSinceStartup - start;
				if (elapsed >= nextLog)
				{
					nextLog += 10f;
					int c = Grid.PosToCell(plant);
					int l = Grid.CellLeft(c), r = Grid.CellRight(c);
					t.Log(elapsed.ToString("0") + " s: mother at " + Cells.Describe(c) + " grown " + Kit.IsGrown(smi) + ", state " + smi.GetCurrentState().name
						+ ", wilting " + smi.IsWilting + ", " + plant.GetComponent<PrimaryElement>().Temperature.ToString("0.0") + " K"
						+ ", air " + Grid.Element[c].id + " " + Grid.Mass[c].ToString("0.000") + " kg"
						+ ", left " + Cells.Describe(l) + " available " + VineBranch.IsCellAvailable(plant, l)
						+ ", right " + Cells.Describe(r) + " available " + VineBranch.IsCellAvailable(plant, r)
						+ "; plant " + Kit.Statuses(plant) + "; farm " + Kit.Statuses(farm.gameObject));
				}
				if (elapsed > 90f)
				{
					yield return t.LookAt(Grid.PosToCell(plant), 5f);
					yield return t.Screenshot("stuck");
					t.Assert.Fail("the mother put out no branch on each side in 90 s (see the log and the 'stuck' screenshot)");
				}
				yield return t.WaitGameSeconds(1);
			}
			yield return Kit.GrowChains(t, smi, 11, 11, 200);
			List<VineBranch.Instance> all = new List<VineBranch.Instance>(Kit.Chain(smi.LeftBranch));
			all.AddRange(Kit.Chain(smi.RightBranch));
			foreach (VineBranch.Instance b in all)
				Kit.ForceMature(b.gameObject);
			yield return t.WaitGameSeconds(8);
			t.Log("left: " + Kit.Describe(Kit.Chain(smi.LeftBranch)));
			t.Log("right: " + Kit.Describe(Kit.Chain(smi.RightBranch)));
			t.Log("plant: " + Kit.Statuses(plant) + "; farm: " + Kit.Statuses(farm.gameObject));

			Midday();
			t.Speed(1);
			yield return t.WaitGameSeconds(2);
			t.Log("time of day " + (GameClock.Instance.GetCurrentCycleAsPercentage() * 100f).ToString("0") + " %, daytime " + GameClock.Instance.IsNighttime().Equals(false));

			DebugHandler.ToggleScreenshotMode();
			t.Cleanup(() => { if (DebugHandler.ScreenshotMode) DebugHandler.ToggleScreenshotMode(); });
			// The screen is 16:9, so an orthographic size s shows 2s rows and 32s/9 columns; at 4.5 the screen
			// is 9 cells high, the square the Workshop preview is cropped to; 6 is the wide shot to crop from.
			// The scene runs from the top of the glass row down to the tips of the leaves, about 1.85 rows
			// under the glass: the camera goes on its middle, and on the middle of the farm (LookAt aims
			// at a cell's bottom-left corner), so the title has as much room above the scene as below.
			Vector3 centre = Grid.CellToPos(farmCell) + new Vector3(0.5f, -0.43f, 0f);
			foreach (float size in new[] { 4.5f, 6f })
			{
				CameraController.Instance.SnapTo(centre, size);
				yield return null;
				yield return null;
				yield return t.WaitGameSeconds(1);
				yield return t.Screenshot("preview-" + size.ToString("0.0"));
			}
			DebugHandler.ToggleScreenshotMode();
		}
	}
}
