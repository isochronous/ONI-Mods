using System.Collections;
using System.Linq;
using Isochronous.TestKit;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// Closing the planner's window keeps the tool, whichever way it is closed: a player may open it
	/// only to look at the plan. A second Escape ends the tool, as does its toolbar button. The tool
	/// is picked from the toolbar each time, as a player picks it.
	/// </summary>
	public static class Closing
	{
		private const string WindowName = "UpgradePlannerDialog";

		[OniTest("Upgrade Planner: the window's X closes it and keeps the tool", Timeout = 30), Layer(2)]
		public static IEnumerator CloseButtonKeepsTool(TestContext t)
		{
			yield return Pick(t);
			KButton close = Find(GameObject.Find(WindowName).transform, PDialog.DIALOG_KEY_CLOSE)?.GetComponent<KButton>();
			t.Assert.NotNull(close, "the window's X is there");
			close.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Null(GameObject.Find(WindowName), "the window is closed");
			t.Assert.True(PlayerController.Instance.ActiveTool is UpgradeTool, "the tool is still active");
		}

		[OniTest("Upgrade Planner: Escape closes the window and keeps the tool; a second Escape ends the tool", Timeout = 30), Layer(2)]
		public static IEnumerator EscapeTwice(TestContext t)
		{
			yield return Pick(t);
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Null(GameObject.Find(WindowName), "the first Escape closed the window");
			t.Assert.True(PlayerController.Instance.ActiveTool is UpgradeTool, "the tool is still active after the first Escape");
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.True(PlayerController.Instance.IsUsingDefaultTool(), "the second Escape ended the tool, now " + PlayerController.Instance.ActiveTool?.name);
		}

		[OniTest("Upgrade Planner: Escape after Apply ends the tool", Timeout = 30), Layer(2)]
		public static IEnumerator EscapeAfterApply(TestContext t)
		{
			yield return Pick(t);
			KButton apply = Find(GameObject.Find(WindowName).transform, "Apply")?.GetComponent<KButton>();
			t.Assert.NotNull(apply, "the Apply button is there");
			apply.SignalClick(KKeyCode.Mouse0);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Null(GameObject.Find(WindowName), "Apply closed the window");
			yield return t.Press(Action.Escape);
			yield return t.WaitRealSeconds(0.5);
			t.Assert.True(PlayerController.Instance.IsUsingDefaultTool(), "Escape ended the tool, now " + PlayerController.Instance.ActiveTool?.name);
		}

		[OniTest("Upgrade Planner: the toolbar button pressed again ends the tool and closes the window", Timeout = 30), Layer(2)]
		public static IEnumerator ToolbarButtonAgain(TestContext t)
		{
			yield return Pick(t);
			Button(t).Click();
			yield return t.WaitRealSeconds(0.5);
			t.Assert.True(PlayerController.Instance.IsUsingDefaultTool(), "the tool ended, now " + PlayerController.Instance.ActiveTool?.name);
			t.Assert.Null(GameObject.Find(WindowName), "the window is closed");
		}

		/// <summary>The tool picked with its toolbar button, its window open.</summary>
		private static IEnumerator Pick(TestContext t)
		{
			t.Cleanup(() =>
			{
				ToolMenu.Instance.ClearSelection();
				if (!PlayerController.Instance.IsUsingDefaultTool())
					SelectTool.Instance.Activate();
			});
			Button(t).Click();
			yield return t.WaitRealSeconds(1);
			t.Assert.True(PlayerController.Instance.ActiveTool is UpgradeTool, "the toolbar button picked the tool, now " + PlayerController.Instance.ActiveTool?.name);
			t.Assert.NotNull(GameObject.Find(WindowName), "the window opened with the tool");
		}

		/// <summary>The planner's button in the toolbar.</summary>
		private static KToggle Button(TestContext t)
		{
			ToolMenu.ToolCollection collection = ToolMenu.Instance.basicTools.FirstOrDefault(c => c.tools.Any(tool => tool.toolName == nameof(UpgradeTool)));
			KToggle toggle = collection?.toggle != null ? collection.toggle.GetComponent<KToggle>() : null;
			t.Assert.NotNull(toggle, "the planner's toolbar button is there");
			return toggle;
		}

		private static Transform Find(Transform root, string name)
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
