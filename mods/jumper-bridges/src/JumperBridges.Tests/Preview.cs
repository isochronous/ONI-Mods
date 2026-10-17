using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>
	/// Workshop preview shots: compact setups in front of the granite back walls low in the test
	/// map, each shot in its overlay with the game's own screenshot mode (no UI). They show
	/// placement only; nothing is powered or switched. The log line "crop" gives the setup's
	/// rectangle in the screenshot's pixels (top-left origin). Run on their own:
	/// Invoke-OniTests.ps1 -Mods JumperBridges -Filter "*preview*"
	/// </summary>
	public static class Preview
	{
		/// <summary>
		/// Two banks of three Ribbon Readers stood on end (ribbon end up, single-bit end down), a
		/// cell apart. Each bank's ribbon ends are joined by a ribbon cable, and a Ribbon Bridge
		/// spans the gap with its ends on the two inner readers' ribbon ports.
		/// </summary>
		[OniTest("looks: preview, automation bridges", Timeout = 90, Explicit = true), Layer(1)]
		public static IEnumerator Automation(TestContext t)
		{
			const int width = 7, height = 2;
			Setup s = new Setup(t, width, height);
			for (int x = 0; x < width; x++)
				if (x != 3)
					s.Build("LogicRibbonReader", x, 1, orientation: Orientation.R90);
			yield return s.Drag("LogicRibbon", 0, 2, 1);
			yield return s.Drag("LogicRibbon", 4, 6, 1);
			s.Build("LogicRibbonBridge", 3, 1);
			yield return t.WaitGameSeconds(1);
			yield return s.Shoot(OverlayModes.Logic.ID, "automation");
		}

		/// <summary>
		/// Battery, Conductive Wire Bridge, lamp, lamp, Conductive Wire Bridge, battery on a granite
		/// floor, the two bridge ends on the side-by-side lamps joined by a wire drag.
		/// </summary>
		[OniTest("looks: preview, power bridges", Timeout = 90, Explicit = true), Layer(1)]
		public static IEnumerator Power(TestContext t)
		{
			const int width = 6, height = 3;
			Setup s = new Setup(t, width, height);
			for (int x = 0; x < width; x++)
				s.Build("Tile", x, 0, SimHashes.Granite);
			s.Build("Battery", 0, 1);
			s.Build("FloorLamp", 2, 1, SimHashes.Gold);
			s.Build("FloorLamp", 3, 1, SimHashes.Gold);
			s.Build("Battery", 5, 1);
			s.Build("WireRefinedBridge", 1, 1, SimHashes.Gold);
			s.Build("WireRefinedBridge", 4, 1, SimHashes.Gold);
			// The two inner ends joined by a wire drag: no wire is built, the two end pieces draw
			// their segment toward each other.
			yield return t.WaitRealSeconds(0.5);
			yield return s.Drag("WireRefined", 2, 3, 1);
			yield return t.WaitGameSeconds(1);
			yield return s.Shoot(OverlayModes.Power.ID, "power");
		}

		private sealed class Setup
		{
			private readonly TestContext t;
			private readonly int width, height;
			private readonly int origin;
			private readonly List<GameObject> built = new List<GameObject>();

			public Setup(TestContext t, int width, int height)
			{
				this.t = t;
				this.width = width;
				this.height = height;
				origin = Grid.InvalidCell;
				for (int margin = 3; margin >= 1 && origin == Grid.InvalidCell; margin--)
					origin = FindSpot(width, height, margin);
				if (origin == Grid.InvalidCell)
					t.Assert.Fail("no open stretch in front of granite back walls for a " + width + "x" + height + " setup");
				t.Log("building at " + Cells.Describe(origin));
				t.Cleanup(() =>
				{
					for (int dy = -1; dy <= height; dy++)
						for (int dx = -1; dx <= width; dx++)
						{
							int cell = Grid.OffsetCell(origin, dx, dy);
							foreach (ObjectLayer layer in new[] { ObjectLayer.LogicWire, ObjectLayer.Wire })
								t.World.Destroy(Grid.Objects[cell, (int)layer]);
						}
					foreach (GameObject go in built)
						t.World.Destroy(go);
				});
			}

			public GameObject Build(string id, int x, int y, SimHashes element = SimHashes.Copper, Orientation orientation = Orientation.Neutral)
			{
				GameObject go = t.World.Build(id, Grid.OffsetCell(origin, x, y), orientation, element);
				built.Add(go);
				return go;
			}

			/// <summary>A wire drag along a row of the setup, through the real tool.</summary>
			public IEnumerator Drag(string id, int fromX, int toX, int y)
			{
				List<int> cells = new List<int>();
				for (int x = fromX; x <= toX; x++)
					cells.Add(Grid.OffsetCell(origin, x, y));
				UtilityDragSession drag = t.Tools.UtilityTool(id);
				yield return drag.Path(cells.ToArray());
				drag.Dispose();
				yield return t.Frames(2);
			}

			/// <summary>Frames the setup with room to spare, hides the UI, opens the overlay and takes the shot.</summary>
			public IEnumerator Shoot(HashedString overlay, string name)
			{
				Vector3 low = Grid.CellToPos(origin);
				Vector3 high = Grid.CellToPos(Grid.OffsetCell(origin, width, height));
				CameraController.Instance.SnapTo((low + high) / 2f, Mathf.Max((height + 3) / 2f, (width + 3) * 9f / 32f));
				bool wasScreenshotMode = DebugHandler.ScreenshotMode;
				if (!wasScreenshotMode)
					DebugHandler.ToggleScreenshotMode();
				t.Cleanup(() =>
				{
					if (DebugHandler.ScreenshotMode != wasScreenshotMode)
						DebugHandler.ToggleScreenshotMode();
				});
				// The normal view from the same camera, for compositing the scene's colours back in
				// around what the overlay highlights.
				yield return t.WaitRealSeconds(2);
				yield return t.Screenshot(name + "-normal");
				OverlayScreen.Instance.ToggleOverlay(overlay);
				t.Cleanup(() => OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID));
				yield return t.WaitRealSeconds(2);
				Vector3 a = Camera.main.WorldToScreenPoint(low), b = Camera.main.WorldToScreenPoint(high);
				t.Log("crop " + Mathf.RoundToInt(a.x) + "," + Mathf.RoundToInt(Screen.height - b.y) + "," + Mathf.RoundToInt(b.x) + "," + Mathf.RoundToInt(Screen.height - a.y)
					+ " of " + Screen.width + "x" + Screen.height);
				yield return t.Screenshot(name);
				// The overlay's port icons are on the world-space canvas, which screenshot mode hides
				// with the rest of the UI; the same shot again with only that canvas back. Toggling
				// screenshot mode off in the cleanup resets its alpha.
				Component group = GameScreenManager.Instance.worldSpaceCanvas.GetComponent("CanvasGroup");
				group.GetType().GetProperty("alpha").SetValue(group, 1f);
				yield return t.WaitRealSeconds(1);
				yield return t.Screenshot(name + "-ports");
			}

			/// <summary>
			/// The lowest cell from which the setup plus the margin on every side is all granite back
			/// wall, open (no solid, no liquid) and free of buildings; the setup's own origin.
			/// </summary>
			private static int FindSpot(int width, int height, int margin)
			{
				WorldContainer world = ClusterManager.Instance.activeWorld;
				Vector2I min = world.WorldOffset, size = world.WorldSize;
				int w = width + 2 * margin, h = height + 2 * margin;
				for (int y = min.y + 1; y < min.y + size.y - h - 1; y++)
					for (int x = min.x + 1; x < min.x + size.x - w - 1; x++)
						if (Fits(Grid.XYToCell(x, y), w, h))
							return Grid.XYToCell(x + margin, y + margin);
				return Grid.InvalidCell;
			}

			private static bool Fits(int corner, int w, int h)
			{
				for (int dy = 0; dy < h; dy++)
					for (int dx = 0; dx < w; dx++)
					{
						int cell = Grid.OffsetCell(corner, dx, dy);
						if (!Grid.IsValidCell(cell) || Grid.Solid[cell] || Grid.IsLiquid(cell) || !Grid.IsVisible(cell))
							return false;
						if (!BackwallManager.HasBackwall(cell) || BackwallManager.At(cell).Element.id != SimHashes.Granite)
							return false;
						for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
							if (layer != (int)ObjectLayer.Backwall && Grid.Objects[cell, layer] != null)
								return false;
					}
				return true;
			}
		}
	}
}
