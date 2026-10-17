using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>Signals across a bridge whose ends sit directly on ports, with no wire anywhere: network logic, so the API layer is enough.</summary>
	public static class Signals
	{
		[OniTest("Jumper Bridges: sensor to bridge to machine carries the signal with no wire", Timeout = 30), Layer(0)]
		public static IEnumerator SensorBridgeMachine(TestContext t)
		{
			CellRect r = t.Rect(5, 1);
			int sensorCell = r.Cell(0, 0);
			int middle = r.Cell(1, 0);
			int machineCell = r.Cell(2, 0);
			GameObject sensor = t.World.Build("LogicSwitch", sensorCell);
			GameObject machine = t.World.Build("LogicAlarm", machineCell);
			t.World.Build("LogicWireBridge", middle);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Logic, sensorCell, out _) && BridgeEndNode.TryGet(BridgeKind.Logic, machineCell, out _), 3, "end nodes on both ports");
			yield return t.WaitGameSeconds(0.5);

			LogicSwitch logicSwitch = sensor.GetComponent<LogicSwitch>();
			LogicPorts machinePorts = machine.GetComponent<LogicPorts>();
			t.Assert.True(machinePorts.IsPortConnected(LogicAlarm.INPUT_PORT_ID), "the machine's input port is on a network");
			t.Assert.True(sensor.GetComponent<LogicPorts>().IsPortConnected(LogicSwitch.PORT_ID), "the sensor's output port is on a network");
			int before = machinePorts.GetInputValue(LogicAlarm.INPUT_PORT_ID);
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, before, "the machine reads the switch's state");

			logicSwitch.HandleToggle();
			yield return t.WaitGameSeconds(0.5);
			int after = machinePorts.GetInputValue(LogicAlarm.INPUT_PORT_ID);
			t.Assert.NotEq(before, after, "the machine's input follows the switch");
			t.Assert.Eq(logicSwitch.IsSwitchedOn ? 1 : 0, after, "the machine reads the toggled state");
		}
	}
}
