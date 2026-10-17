using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>
	/// A bridge end that lands on a real wire is the vanilla case: no node, the wire carries the
	/// bridge's connections. When that wire goes (melts, breaks) while the bridge stands, the
	/// node that takes its place must keep what the wire had, or the bridge loses its links.
	/// </summary>
	public static class WireUnderEnd
	{
		[OniTest("Jumper Bridges: a wire under a bridge end that melts leaves a node with the wire's connections", Timeout = 40), Layer(1)]
		public static IEnumerator WireLeavesConnectionsBehind(TestContext t)
		{
			CellRect r = t.Rect(7, 1);
			int sensorCell = r.Cell(1, 0);
			int endCell = r.Cell(3, 0);
			int middle = r.Cell(4, 0);
			int machineCell = r.Cell(5, 0);
			// A switch under the start of a three-cell wire that ends under the bridge's near end.
			GameObject sensor = t.World.Build("LogicSwitch", sensorCell);
			UtilityDragSession drag = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return drag.Path(r.Cell(1, 0), r.Cell(2, 0), endCell);
			drag.Dispose();
			yield return t.Frames(2);
			GameObject machine = t.World.Build("LogicAlarm", machineCell);
			t.World.Build("LogicWireBridge", middle);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Logic, machineCell, out _), 3, "the node on the machine's port");
			t.Assert.False(BridgeEndNode.TryGet(BridgeKind.Logic, endCell, out _), "no node under the end that sits on a wire");
			yield return t.WaitGameSeconds(0.5);

			LogicSwitch logicSwitch = sensor.GetComponent<LogicSwitch>();
			LogicPorts ports = machine.GetComponent<LogicPorts>();
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, ports.GetInputValue(LogicAlarm.INPUT_PORT_ID), "the machine reads the switch through wire and bridge");

			// The wire under the end goes the way a melted or broken one does: destroyed, not deconstructed.
			GameObject wireUnderEnd = t.World.At(endCell, ObjectLayer.LogicWire);
			t.Assert.NotNull(wireUnderEnd, "the wire under the end");
			t.World.Destroy(wireUnderEnd);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Logic, endCell, out BridgeEndNode n) && n != null, 3, "a node where the wire was");
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Has(UtilityConnections.Left, t.World.LogicConnections(endCell), "the node kept the wire's connection to the left");
			t.Assert.Has(UtilityConnections.Right, t.World.LogicConnections(r.Cell(2, 0)), "the wire to the left still draws into the end");
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, ports.GetInputValue(LogicAlarm.INPUT_PORT_ID), "the machine still reads the switch");
			logicSwitch.HandleToggle();
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, ports.GetInputValue(LogicAlarm.INPUT_PORT_ID), "the machine follows the switch after the wire is gone");
		}
	}
}
