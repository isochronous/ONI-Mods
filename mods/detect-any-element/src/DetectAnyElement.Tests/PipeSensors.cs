using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace DetectAnyElement.Tests
{
	/// <summary>
	/// The Liquid Pipe, Gas Pipe and Conveyor Rail Element Sensors on Any. The liquid and gas
	/// sensors share one branch of the patch (the flow manager is chosen by the sensor's conduit
	/// type), the rail sensor reads a Pickupable instead; one of each branch is tested.
	/// </summary>
	public static class PipeSensors
	{
		[OniTest("DAE: a Liquid Pipe Element Sensor on Any is on for any packet, off when empty", Timeout = 40), Layer(0)]
		public static IEnumerator LiquidPipe(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 1);
			int cell = r.Cell(2, 0);
			t.World.Build("LiquidConduit", cell);
			GameObject sensor = t.World.Build("LiquidConduitElementSensor", cell);
			yield return Sensors.Flow(t);
			Sensors.PickAny(sensor);
			yield return Sensors.Flow(t);
			t.Assert.False(Sensors.IsOn(sensor), "an empty pipe");

			ConduitFlow flow = Conduit.GetFlowManager(ConduitType.Liquid);
			float added = flow.AddElement(cell, SimHashes.Water, 1f, 293.15f, byte.MaxValue, 0);
			t.Assert.True(added > 0f, "the pipe took the packet");
			yield return Sensors.Flow(t);
			t.Assert.True(Sensors.IsOn(sensor), "water in the pipe");

			flow.SetContents(cell, ConduitFlow.ConduitContents.Empty);
			yield return Sensors.Flow(t);
			t.Assert.False(Sensors.IsOn(sensor), "the pipe is empty again");

			flow.AddElement(cell, SimHashes.CrudeOil, 1f, 293.15f, byte.MaxValue, 0);
			yield return Sensors.Flow(t);
			t.Assert.True(Sensors.IsOn(sensor), "crude oil, not the first element");

			Sensors.Pick(sensor, SimHashes.Water);
			yield return Sensors.Flow(t);
			t.Assert.False(Sensors.IsOn(sensor), "water chosen over Any: the oil no longer matches");
		}

		[OniTest("DAE: a Gas Pipe Element Sensor on Any is on for any packet, off when empty", Timeout = 40), Layer(0)]
		public static IEnumerator GasPipe(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 1);
			int cell = r.Cell(2, 0);
			t.World.Build("GasConduit", cell);
			GameObject sensor = t.World.Build("GasConduitElementSensor", cell);
			yield return Sensors.Flow(t);
			Sensors.PickAny(sensor);
			yield return Sensors.Flow(t);
			t.Assert.False(Sensors.IsOn(sensor), "an empty pipe");

			ConduitFlow flow = Conduit.GetFlowManager(ConduitType.Gas);
			flow.AddElement(cell, SimHashes.Hydrogen, 0.2f, 293.15f, byte.MaxValue, 0);
			yield return Sensors.Flow(t);
			t.Assert.True(Sensors.IsOn(sensor), "hydrogen in the pipe");
		}

		[OniTest("DAE: a Conveyor Rail Element Sensor on Any is on for any item on the rail, off when empty", Timeout = 40), Layer(0)]
		public static IEnumerator ConveyorRail(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 1);
			int cell = r.Cell(2, 0);
			t.World.Build("SolidConduit", cell);
			GameObject sensor = t.World.Build("SolidConduitElementSensor", cell);
			yield return Sensors.Flow(t);
			Sensors.PickAny(sensor);
			yield return Sensors.Flow(t);
			t.Assert.False(Sensors.IsOn(sensor), "an empty rail");

			SolidConduitFlow flow = SolidConduit.GetFlowManager();
			GameObject ore = ElementLoader.FindElementByHash(SimHashes.Cuprite).substance.SpawnResource(Grid.CellToPosCBC(cell, Grid.SceneLayer.Ore), 20f, 293.15f, byte.MaxValue, 0);
			flow.AddPickupable(cell, ore.GetComponent<Pickupable>());
			yield return Sensors.Flow(t);
			t.Assert.True(Sensors.IsOn(sensor), "a lump of copper ore on the rail");

			Pickupable taken = flow.RemovePickupable(cell);
			t.Assert.NotNull(taken, "the lump came off the rail");
			Util.KDestroyGameObject(taken.gameObject);
			yield return Sensors.Flow(t);
			t.Assert.False(Sensors.IsOn(sensor), "the rail is empty again");
		}
	}
}
