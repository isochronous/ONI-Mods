using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>
	/// The deconstruct tool reaches a bridge by its end cells only, as in the base game. Reported in
	/// game: dragging over a bridge's middle cell marked it too.
	/// </summary>
	public static class Deconstruct
	{
		[OniTest("Jumper Bridges: the deconstruct tool marks a bridge from its ends, not its middle", Timeout = 40), Layer(1)]
		public static IEnumerator EndsOnly(TestContext t)
		{
			CellRect r = t.Rect(3, 1);
			GameObject bridge = t.World.Build("LogicWireBridge", r.Cell(1, 0));
			yield return t.WaitGameSeconds(0.5);
			yield return Check(t, bridge, r.Cell(1, 0), r.Cell(0, 0));
		}

		/// <summary>
		/// Both ends on gates' ports, so the bridge is on no layer and only the mod's own registry
		/// finds it: still from the ends alone.
		/// </summary>
		[OniTest("Jumper Bridges: a bridge between two gates is marked from its ends, not its middle", Timeout = 40), Layer(1)]
		public static IEnumerator BetweenGatesEndsOnly(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			t.World.Build("LogicGateNOT", r.Cell(1, 0));
			t.World.Build("LogicGateNOT", r.Cell(4, 0));
			GameObject bridge = t.World.Build("LogicWireBridge", r.Cell(3, 0));
			yield return t.WaitGameSeconds(0.5);
			yield return Check(t, bridge, r.Cell(3, 0), r.Cell(2, 0));
		}

		private static IEnumerator Check(TestContext t, GameObject bridge, int middle, int end)
		{
			t.InstantBuild(false);
			PlayerController.Instance.ActivateTool(DeconstructTool.Instance);
			t.Cleanup(() => SelectTool.Instance.Activate());
			yield return t.Frames(2);
			Deconstructable deconstructable = bridge.GetComponent<Deconstructable>();
			DeconstructTool.Instance.DeconstructCell(middle);
			yield return t.Frames(2);
			t.Assert.False(deconstructable.IsMarkedForDeconstruction(), "the middle cell does not mark the bridge");
			DeconstructTool.Instance.DeconstructCell(end);
			yield return t.Frames(2);
			t.Assert.True(deconstructable.IsMarkedForDeconstruction(), "an end cell marks the bridge");
		}
	}
}
