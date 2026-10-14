using System.Collections;
using System.Collections.Generic;
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
	}
}
