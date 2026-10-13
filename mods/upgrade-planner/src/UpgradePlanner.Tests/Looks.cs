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
			yield return t.Frames(5);
			t.Assert.NotNull(GameObject.Find("UpgradePlannerDialog"), "the planner window is open");
			yield return t.Screenshot("fresh");
			// A whole plan in it.
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			BuildingDef pipe = Assets.GetBuildingDef("LiquidConduit");
			plan.SetSource(ReplacementRules.FamilyOf(pipe));
			plan.SetSourceAnyMaterial();
			plan.SetTarget(ReplacementRules.FamilyOf(Assets.GetBuildingDef("InsulatedLiquidConduit")));
			plan.SetTargetMaterials(new List<Tag> { SimHashes.Ceramic.CreateTag() });
			PlayerController.Instance.ActivateTool(tool);
			yield return t.Frames(5);
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
			yield return t.Frames(5);
			yield return t.Screenshot("sandbox");
		}
	}
}
