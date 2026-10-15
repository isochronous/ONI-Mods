using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace DirectAutomationBridges.Tests
{
	/// <summary>
	/// A bridge from a wire to a logic gate's input port. Reported in game: a bridge built from a
	/// red wire onto a gate's input left the port white until a wire was built under the end.
	/// </summary>
	public static class Gates
	{
		[OniTest("DAB: bridge from a red wire to a gate's input puts the input on the wire's network", Timeout = 40), Layer(1)]
		public static IEnumerator RedWireToGateInput(TestContext t)
		{
			CellRect r = t.Rect(8, 1);
			int sensorCell = r.Cell(1, 0);
			int endCell = r.Cell(3, 0);
			int middle = r.Cell(4, 0);
			int gateInput = r.Cell(5, 0);
			GameObject sensor = t.World.Build("LogicSwitch", sensorCell);
			LogicSwitch logicSwitch = sensor.GetComponent<LogicSwitch>();
			if (logicSwitch.IsSwitchedOn)
				logicSwitch.HandleToggle();
			UtilityDragSession drag = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return drag.Path(sensorCell, r.Cell(2, 0), endCell);
			drag.Dispose();
			GameObject gate = t.World.Build("LogicGateNOT", gateInput);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.NotNull(t.World.LogicNetwork(endCell), "the wire is on a network before the bridge");
			t.Assert.Null(t.World.LogicNetwork(gateInput), "the gate's input is on no network before the bridge");

			t.World.Build("LogicWireBridge", middle);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(gateInput, out _), 3, "the node on the gate's input");
			yield return t.WaitGameSeconds(0.5);

			LogicCircuitNetwork wire = t.World.LogicNetwork(endCell);
			LogicCircuitNetwork input = t.World.LogicNetwork(gateInput);
			Debug.Log("[DAB test] wire network " + (wire != null ? wire.id.ToString() : "none") + ", gate input network " + (input != null ? input.id.ToString() : "none")
				+ ", node connections " + t.World.LogicConnections(gateInput) + ", end connections " + t.World.LogicConnections(endCell));
			t.Assert.NotNull(input, "the gate's input is on a network");
			t.Assert.True(ReferenceEquals(wire, input), "the gate's input is on the wire's network");
			t.Assert.True(gate.GetComponent<LogicGate>().GetPortConnected(LogicGateBase.PortId.InputOne), "the gate reports its input connected");
		}

		/// <summary>
		/// The same, built while the game is paused, as the report was: the input must join the
		/// wire's network without the game running.
		/// </summary>
		[OniTest("DAB: bridge built while paused connects a gate's input at once", Timeout = 40), Layer(1)]
		public static IEnumerator BuiltWhilePaused(TestContext t)
		{
			CellRect r = t.Rect(8, 1);
			int sensorCell = r.Cell(1, 0);
			int endCell = r.Cell(3, 0);
			int gateInput = r.Cell(5, 0);
			GameObject sensor = t.World.Build("LogicSwitch", sensorCell);
			LogicSwitch logicSwitch = sensor.GetComponent<LogicSwitch>();
			if (logicSwitch.IsSwitchedOn)
				logicSwitch.HandleToggle();
			UtilityDragSession drag = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return drag.Path(sensorCell, r.Cell(2, 0), endCell);
			drag.Dispose();
			t.World.Build("LogicGateNOT", gateInput);
			yield return t.WaitGameSeconds(0.5);

			SpeedControlScreen speed = SpeedControlScreen.Instance;
			bool wasPaused = speed.IsPaused;
			if (!wasPaused)
				speed.Pause(playSound: false);
			t.Cleanup(() =>
			{
				if (!wasPaused && speed.IsPaused)
					speed.Unpause(playSound: false);
			});
			t.World.Build("LogicWireBridge", r.Cell(4, 0));
			yield return t.WaitRealSeconds(1);
			t.Assert.True(speed.IsPaused, "the game is still paused");
			t.Assert.True(BridgeEndNode.TryGet(gateInput, out _), "the node on the gate's input, while paused");
			LogicCircuitNetwork input = t.World.LogicNetwork(gateInput);
			t.Assert.NotNull(input, "the gate's input is on a network while paused");
			t.Assert.True(ReferenceEquals(t.World.LogicNetwork(endCell), input), "the gate's input is on the wire's network while paused");
		}

		/// <summary>
		/// Screenshots, for looking at: two NOT gates, the upper fed through a bridge end on its
		/// input, the lower through a wire under its input, both from the same red wire; normal
		/// view, then the automation overlay.
		/// </summary>
		[OniTest("looks: gate input fed by a bridge end next to one fed by a wire", Timeout = 60, Explicit = true), Layer(1)]
		public static IEnumerator GateLooks(TestContext t)
		{
			CellRect r = t.Rect(8, 3);
			int sensorCell = r.Cell(1, 0);
			GameObject sensor = t.World.Build("LogicSwitch", sensorCell);
			LogicSwitch logicSwitch = sensor.GetComponent<LogicSwitch>();
			if (logicSwitch.IsSwitchedOn)
				logicSwitch.HandleToggle();
			// Wire: switch, right two cells, up two; the bridge's near end sits on the top wire cell.
			UtilityDragSession drag = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return drag.Path(sensorCell, r.Cell(2, 0), r.Cell(3, 0));
			drag.Dispose();
			yield return t.Frames(2);
			drag = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return drag.Path(r.Cell(3, 0), r.Cell(3, 1), r.Cell(3, 2));
			drag.Dispose();
			yield return t.Frames(2);
			GameObject bridged = t.World.Build("LogicGateNOT", r.Cell(5, 2));
			GameObject wired = t.World.Build("LogicGateNOT", r.Cell(5, 0));
			// The lower gate: a wire from the wire's corner straight into its input.
			drag = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return drag.Path(r.Cell(3, 0), r.Cell(4, 0), r.Cell(5, 0));
			drag.Dispose();
			yield return t.WaitGameSeconds(0.5);
			t.World.Build("LogicWireBridge", r.Cell(4, 2));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(r.Cell(5, 2), out _), 3, "the node on the upper gate's input");
			yield return t.WaitGameSeconds(1);
			CameraController.Instance.SnapTo(Grid.CellToPosCCC(r.Cell(4, 1), Grid.SceneLayer.Building), 5f);
			yield return t.WaitRealSeconds(1);
			yield return t.Screenshot("normal");
			OverlayScreen.Instance.ToggleOverlay(OverlayModes.Logic.ID);
			t.Cleanup(() => OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID));
			yield return t.WaitRealSeconds(1);
			yield return t.Screenshot("overlay");
			t.Log("upper gate input network " + (t.World.LogicNetwork(r.Cell(5, 2))?.id.ToString() ?? "none") + ", lower " + (t.World.LogicNetwork(r.Cell(5, 0))?.id.ToString() ?? "none"));
			t.Assert.NotNull(bridged, "the upper gate");
			t.Assert.NotNull(wired, "the lower gate");
		}
	}
}
