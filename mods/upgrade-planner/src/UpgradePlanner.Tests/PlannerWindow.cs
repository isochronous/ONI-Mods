using UnityEngine;
using Isochronous.TestKit;

namespace UpgradePlanner.Tests
{
	/// <summary>The planner's window, as a player works it.</summary>
	internal static class PlannerWindow
	{
		public const string Name = "UpgradePlannerDialog";

		/// <summary>Apply pressed, as a player confirms the plan before dragging; the window opens with the tool.</summary>
		public static void PressApply(TestContext t)
		{
			GameObject window = GameObject.Find(Name);
			KButton apply = window != null ? Find(window.transform, "Apply")?.GetComponent<KButton>() : null;
			t.Assert.NotNull(apply, "the planner window's Apply button");
			apply.SignalClick(KKeyCode.Mouse0);
		}

		public static Transform Find(Transform root, string name)
		{
			if (root == null || root.name == name)
				return root;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform found = Find(root.GetChild(i), name);
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
