using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>Bridges through the game's own save and load (SaveLoadRoot, as a colony save writes and reads them).</summary>
	public static class Saves
	{
		/// <summary>
		/// Two bridges end to end, their adjacent ends joined by a wire drag and no wire laid: the
		/// link is only between the two end nodes, which are not saved, so the bridges carry it.
		/// </summary>
		[OniTest("Jumper Bridges: two bridge ends joined by a drag stay joined through a save and load", Timeout = 40), Layer(1)]
		public static IEnumerator JoinedEndsSurviveLoad(TestContext t)
		{
			CellRect r = t.Rect(7, 1);
			int sensorCell = r.Cell(0, 0);
			int endA = r.Cell(2, 0);
			int endB = r.Cell(3, 0);
			int machineCell = r.Cell(5, 0);
			GameObject sensor = t.World.Build("LogicSwitch", sensorCell);
			GameObject machine = t.World.Build("LogicAlarm", machineCell);
			GameObject bridgeA = t.World.Build("LogicWireBridge", r.Cell(1, 0));
			GameObject bridgeB = t.World.Build("LogicWireBridge", r.Cell(4, 0));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Logic, endA, out _) && BridgeEndNode.TryGet(BridgeKind.Logic, endB, out _), 3, "end nodes at both inner ends");
			yield return t.WaitGameSeconds(0.3);
			t.Assert.False(ReferenceEquals(t.World.LogicNetwork(endA), t.World.LogicNetwork(endB)), "the bridges start apart");

			UtilityDragSession wire = t.Tools.UtilityTool("LogicWire");
			yield return wire.Path(endA, endB);
			wire.Dispose();
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Null(t.World.At(endA, ObjectLayer.LogicWire), "no wire on the first end");
			t.Assert.Null(t.World.At(endB, ObjectLayer.LogicWire), "no wire on the second end");
			t.Assert.Has(UtilityConnections.Right, t.World.LogicConnections(endA), "the first end is joined to the second");
			t.Assert.Has(UtilityConnections.Left, t.World.LogicConnections(endB), "the second end is joined to the first");
			LogicSwitch logicSwitch = sensor.GetComponent<LogicSwitch>();
			LogicPorts machinePorts = machine.GetComponent<LogicPorts>();
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, machinePorts.GetInputValue(LogicAlarm.INPUT_PORT_ID), "the machine reads the switch before the save");

			Tag bridgeTag = bridgeA.PrefabID();
			byte[] fileA = SaveFile.Capture(bridgeA);
			byte[] fileB = SaveFile.Capture(bridgeB);
			t.World.Destroy(bridgeA);
			t.World.Destroy(bridgeB);
			yield return t.WaitGameSeconds(1);
			t.Assert.False(BridgeEndNode.TryGet(BridgeKind.Logic, endA, out _), "the first end's node went with its bridge");
			SaveFile.Restore(bridgeTag, fileA);
			SaveFile.Restore(bridgeTag, fileB);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Logic, endA, out _) && BridgeEndNode.TryGet(BridgeKind.Logic, endB, out _), 3, "end nodes back at both inner ends");
			yield return t.WaitGameSeconds(0.5);

			t.Assert.Has(UtilityConnections.Right, t.World.LogicConnections(endA), "the loaded first end is joined to the second");
			t.Assert.Has(UtilityConnections.Left, t.World.LogicConnections(endB), "the loaded second end is joined to the first");
			t.Assert.NotNull(t.World.LogicNetwork(endA), "the loaded ends are on a network");
			t.Assert.True(ReferenceEquals(t.World.LogicNetwork(endA), t.World.LogicNetwork(endB)), "the loaded ends share one network");
			int before = machinePorts.GetInputValue(LogicAlarm.INPUT_PORT_ID);
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, before, "the machine reads the switch after the load");
			logicSwitch.HandleToggle();
			yield return t.WaitGameSeconds(0.5);
			int after = machinePorts.GetInputValue(LogicAlarm.INPUT_PORT_ID);
			t.Assert.NotEq(before, after, "the machine follows the switch after the load");
		}
	}
}
