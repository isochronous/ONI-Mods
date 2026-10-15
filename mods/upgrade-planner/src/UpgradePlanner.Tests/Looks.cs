using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>Pictures of the planner's window and toolbar button, for looking at; nothing is asserted beyond the window being there.</summary>
	public static class Looks
	{
		[OniTest("Upgrade Planner: screenshots of the window with a plan in it", Timeout = 40), Layer(2)]
		public static IEnumerator Window(TestContext t)
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
			// Fresh: nothing chosen yet.
			UpgradePlan plan = UpgradePlan.Current;
			plan.Clear();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			t.Assert.NotNull(GameObject.Find("UpgradePlannerDialog"), "the planner window is open");
			yield return t.Screenshot("fresh");
			// A whole plan in it.
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			BuildingDef pipe = Assets.GetBuildingDef("LiquidConduit");
			plan.Mode = PlanMode.ReplaceType;
			plan.ReplaceType.SetSource(Families.Of(pipe));
			plan.ReplaceType.SetSourceAny();
			plan.ReplaceType.SetTarget(Families.Of(Assets.GetBuildingDef("InsulatedLiquidConduit")));
			plan.ReplaceType.SetTargetMaterials(new List<Tag> { SimHashes.Ceramic.CreateTag() });
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			GameObject window = GameObject.Find("UpgradePlannerDialog");
			t.Assert.NotNull(window, "the planner window is open");
			yield return t.Screenshot("window");
			// The toggle above the toolbar, with the sandbox row shown as well. Changing the mode
			// drops the active tool, so the tool is activated again after it.
			bool sandbox = Game.Instance.SandboxModeActive;
			t.Cleanup(() => Game.Instance.SandboxModeActive = sandbox);
			Game.Instance.SandboxModeActive = true;
			yield return t.Frames(2);
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			yield return t.Screenshot("sandbox");
		}

		[OniTest("Upgrade Planner: screenshots of the Change material tab", Timeout = 40), Layer(2)]
		public static IEnumerator MaterialWindow(TestContext t)
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
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			yield return t.Screenshot("material-fresh");
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			var all = new List<Family>();
			foreach (Category category in MaterialSwap.Categories(false))
				all.AddRange(category.Families);
			plan.ChangeMaterial.Pick(all, true);
			plan.ChangeMaterial.SetSource(SimHashes.SandStone.CreateTag());
			plan.ChangeMaterial.SetTarget(SimHashes.Granite.CreateTag());
			// Two saved presets, in a file of the test's own, so the preset row shows its list.
			string file = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "upgrade-planner-looks-presets.json");
			string original = Presets.FilePath;
			t.Cleanup(() =>
			{
				Presets.UseFile(original);
				if (System.IO.File.Exists(file))
					System.IO.File.Delete(file);
			});
			if (System.IO.File.Exists(file))
				System.IO.File.Delete(file);
			Presets.UseFile(file);
			Presets.Put(plan.ToPreset("Sandstone to granite"));
			var typed = new UpgradePlan();
			typed.Mode = PlanMode.ReplaceType;
			Presets.Put(typed.ToPreset("Insulate liquid pipes"));
			PlayerController.Instance.ActivateTool(tool);
			yield return t.WaitRealSeconds(1);
			t.Assert.NotNull(GameObject.Find("UpgradePlannerDialog"), "the planner window is open");
			yield return t.Screenshot("material-plan");
		}

		[OniTest("Upgrade Planner: the game's tool cursors and the planner's, saved as pictures", Timeout = 40, Explicit = true), Layer(2)]
		public static IEnumerator Cursors(TestContext t)
		{
			string dir = System.IO.Path.Combine(Isochronous.TestRunner.Session.Dir, "artifacts");
			System.IO.Directory.CreateDirectory(dir);
			var cursor = HarmonyLib.AccessTools.Field(typeof(InterfaceTool), "cursor");
			var boxCursor = HarmonyLib.AccessTools.Field(typeof(DragTool), "boxCursor");
			foreach (InterfaceTool tool in PlayerController.Instance.tools)
			{
				Save(t, dir, tool.name + "-cursor", cursor.GetValue(tool) as Texture2D);
				if (tool is DragTool)
					Save(t, dir, tool.name + "-box", boxCursor.GetValue(tool) as Texture2D);
			}
			// The marker that follows the cursor over the cells: the planner's, and Sweep's it was cloned from.
			foreach (InterfaceTool tool in new InterfaceTool[] { ClearTool.Instance, PlayerController.Instance.tools.First(x => x is UpgradeTool) })
				Describe(t, dir, tool.name + "-visualizer", tool.visualizer != null ? tool.visualizer.transform : null, 0);
			Save(t, dir, "planner-icon", Assets.GetSprite(ToolIcon.SpriteName)?.texture);
			yield return null;
		}

		[OniTest("Upgrade Planner: a screenshot of the cell marker over tiles", Timeout = 40, Explicit = true), Layer(2)]
		public static IEnumerator Marker(TestContext t)
		{
			CellRect r = t.Rect(6, 3);
			for (int x = 0; x < 6; x++)
				t.World.Build("Tile", r.Cell(x, 0), element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(1);
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.tools.First(x => x is UpgradeTool);
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			PlayerController.Instance.ActivateTool(tool);
			PlannerWindow.PressApply(t);
			int cell = r.Cell(2, 0);
			CameraController.Instance.SnapTo(Grid.CellToPosCCC(cell, Grid.SceneLayer.Building), 6f);
			yield return t.WaitRealSeconds(0.5);
			t.PointAt(cell);
			tool.OnMouseMove(Grid.CellToPosCCC(cell, Grid.SceneLayer.Building));
			yield return t.WaitRealSeconds(0.5);
			yield return t.Screenshot("marker");
		}

		[OniTest("Upgrade Planner: a screenshot of the hover card over a box of several types", Timeout = 60, Explicit = true), Layer(2)]
		public static IEnumerator SelectionCard(TestContext t)
		{
			t.ResearchAll();
			CellRect r = t.Rect(12, 4);
			var defs = new[] { "FirePole", "WireRefined", "WireRubber" }.Select(Assets.GetBuildingDef).ToList();
			for (int x = 0; x < 9; x++)
			{
				BuildingDef def = defs[x % 3];
				ByHand.Build(t, def, BuildingSettings.Materials(def), r.Cell(x + 1, 1));
			}
			yield return t.WaitGameSeconds(1);
			t.InstantBuild(false);
			UpgradePlan plan = UpgradePlan.Current;
			plan.Mode = PlanMode.ChangeMaterial;
			plan.Clear();
			plan.ChangeMaterial.Pick(defs.Select(Families.Of).Distinct().ToList(), true);
			plan.ChangeMaterial.SetSourceAny();
			plan.ChangeMaterial.SetTarget(SimHashes.Steel.CreateTag());
			t.Assert.True(plan.IsComplete, "the plan is complete: " + plan.Summary());
			UpgradeTool tool = (UpgradeTool)PlayerController.Instance.tools.First(x => x is UpgradeTool);
			t.Cleanup(() =>
			{
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			PlayerController.Instance.ActivateTool(tool);
			PlannerWindow.PressApply(t);
			CameraController.Instance.SnapTo(Grid.CellToPosCCC(r.Cell(5, 1), Grid.SceneLayer.Building), 8f);
			yield return t.WaitRealSeconds(0.5);
			Vector3 from = Grid.CellToPosCCC(r.Cell(0, 0), Grid.SceneLayer.Building), to = Grid.CellToPosCCC(r.Cell(10, 3), Grid.SceneLayer.Building);
			tool.OnMouseMove(from);
			yield return t.WaitRealSeconds(0.2);
			tool.OnLeftClickDown(from);
			yield return t.WaitRealSeconds(0.2);
			t.PointAt(r.Cell(10, 3));
			tool.OnMouseMove(to);
			yield return t.WaitRealSeconds(1);
			var card = (PlannerHoverCard)tool.GetComponent<HoverTextConfiguration>();
			t.Log("card: " + string.Join(" | ", card.Types) + " || " + string.Join(" | ", card.Materials));
			yield return t.Screenshot("card");
			tool.OnRightClickDown(to, null);
			yield return t.WaitRealSeconds(0.3);
		}

		/// <summary>Logs a marker's objects and their parts, saving any picture they show.</summary>
		private static void Describe(TestContext t, string dir, string name, Transform node, int depth)
		{
			if (node == null)
			{
				t.Log(name + ": none");
				return;
			}
			foreach (Component part in node.GetComponents<Component>())
			{
				string what = part.GetType().Name;
				if (part is SpriteRenderer sr && sr.sprite != null)
				{
					what += " sprite " + sr.sprite.name;
					Save(t, dir, name + "-" + node.name, sr.sprite.texture);
				}
				else if (part is UnityEngine.UI.Image image && image.sprite != null)
				{
					what += " sprite " + image.sprite.name;
					Save(t, dir, name + "-" + node.name, image.sprite.texture);
				}
				else if (part is Renderer renderer && renderer.sharedMaterial != null)
				{
					what += " material " + renderer.sharedMaterial.name + " texture " + renderer.sharedMaterial.mainTexture?.name;
					Save(t, dir, name + "-" + node.name, renderer.sharedMaterial.mainTexture as Texture2D);
				}
				else if (part is KBatchedAnimController anim)
					what += " anim " + string.Join(",", System.Linq.Enumerable.Select(anim.AnimFiles ?? new KAnimFile[0], f => f != null ? f.name : "null")) + " initial " + anim.initialAnim;
				t.Log(name + ": " + new string(' ', depth * 2) + node.name + " (" + (node.gameObject.activeSelf ? "on" : "off") + "): " + what);
			}
			for (int i = 0; i < node.childCount; i++)
				Describe(t, dir, name, node.GetChild(i), depth + 1);
		}

		/// <summary>A texture written as a PNG through a render texture, readable or not.</summary>
		private static void Save(TestContext t, string dir, string name, Texture2D texture)
		{
			if (texture == null)
				return;
			RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			Graphics.Blit(texture, rt);
			RenderTexture was = RenderTexture.active;
			RenderTexture.active = rt;
			var copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
			copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			copy.Apply();
			RenderTexture.active = was;
			RenderTexture.ReleaseTemporary(rt);
			string file = System.IO.Path.Combine(dir, name + ".png");
			System.IO.File.WriteAllBytes(file, copy.EncodeToPNG());
			t.Log(name + ": " + texture.name + " " + texture.width + "x" + texture.height);
		}
	}
}
