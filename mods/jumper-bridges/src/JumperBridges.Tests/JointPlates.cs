using System.Collections;
using Isochronous.TestKit;
using STRINGS;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>
	/// Heavi-Watt Joint Plates as power bridges: the ends may sit on power ports like a wire
	/// bridge's, the centre stays the vanilla solid tile. Same floor layout as the Power tests:
	/// a battery and a lamp two cells apart, the plate's centre between them.
	/// </summary>
	public static class JointPlates
	{
		private const string Plate = "WireBridgeHighWattage";
		private const string ConductivePlate = "WireRefinedBridgeHighWattage";

		[OniTest("Jumper Bridges joint plate: battery to plate to lamp carries power with no wire", Timeout = 40), Layer(0)]
		public static IEnumerator BatteryPlateLamp(TestContext t)
		{
			CellRect r = Power.Floor(t, 5);
			int batteryCell = r.Cell(1, 1);
			int lampCell = r.Cell(3, 1);
			GameObject battery = t.World.Build("Battery", batteryCell);
			GameObject lamp = t.World.Build("FloorLamp", lampCell);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(ushort.MaxValue, Power.Circuit(lampCell), "the lamp's port is on no circuit before the plate");

			bool valid = Power.ValidWithPreview(Plate, r.Cell(2, 1), out string reason);
			t.Assert.True(valid, "the plate may be placed with its ends on both ports: " + reason);
			GameObject plate = t.World.Build(Plate, r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, batteryCell, out _) && BridgeEndNode.TryGet(BridgeKind.Power, lampCell, out _), 3, "end nodes on both ports");
			yield return t.WaitGameSeconds(0.5);

			ushort circuit = Power.Circuit(batteryCell);
			t.Assert.NotEq(ushort.MaxValue, circuit, "the battery's port is on a circuit");
			t.Assert.Eq(circuit, lamp.GetComponent<EnergyConsumer>().CircuitID, "the lamp reports the battery's circuit");
			t.Assert.Eq(20000f, Game.Instance.circuitManager.GetMaxSafeWattageForCircuit(circuit), "the circuit's limit is the plate's");
			t.Assert.False(battery.HasTag(GameTags.HasInvalidPorts), "the battery has no port overlap");
			t.Assert.False(lamp.HasTag(GameTags.HasInvalidPorts), "the lamp has no port overlap");
			t.Assert.False(plate.HasTag(GameTags.HasInvalidPorts), "the plate has no port overlap");

			t.Assert.True(lamp.GetComponent<Operational>().GetFlag(InvalidPortReporter.portsNotOverlapping), "the lamp is not put out of order");

			battery.GetComponent<Battery>().AddEnergy(10000f);
			EnergyConsumer consumer = lamp.GetComponent<EnergyConsumer>();
			yield return t.WaitUntil(() => consumer.IsPowered, 5, "the lamp powered from the battery");
		}

		/// <summary>Two plates with one end each on the same free cell: allowed, one node there, power passes through.</summary>
		[OniTest("Jumper Bridges joint plate: two plates meeting end to end at one cell connect and carry power", Timeout = 40), Layer(0)]
		public static IEnumerator PlatesEndToEnd(TestContext t)
		{
			CellRect r = Power.Floor(t, 7);
			int batteryCell = r.Cell(1, 1);
			int sharedCell = r.Cell(3, 1);
			int lampCell = r.Cell(5, 1);
			GameObject battery = t.World.Build("Battery", batteryCell);
			GameObject lamp = t.World.Build("FloorLamp", lampCell);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(ushort.MaxValue, Power.Circuit(lampCell), "the lamp's port is on no circuit before the plates");

			bool valid = Power.ValidWithPreview(Plate, r.Cell(2, 1), out string reason);
			t.Assert.True(valid, "the first plate may be placed: " + reason);
			t.World.Build(Plate, r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, sharedCell, out _), 3, "the first plate's free end node at " + Cells.Describe(sharedCell));
			yield return t.WaitGameSeconds(0.5);

			valid = Power.ValidWithPreview(Plate, r.Cell(4, 1), out reason);
			t.Assert.True(valid, "the second plate's end may sit on the first plate's end: " + reason);
			t.World.Build(Plate, r.Cell(4, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, lampCell, out _), 3, "the second plate's end node on the lamp's port");
			yield return t.WaitGameSeconds(0.5);

			ushort circuit = Power.Circuit(batteryCell);
			t.Assert.NotEq(ushort.MaxValue, circuit, "the battery's port is on a circuit");
			t.Assert.Eq(circuit, Power.Circuit(sharedCell), "the shared end is on the battery's circuit");
			t.Assert.Eq(circuit, lamp.GetComponent<EnergyConsumer>().CircuitID, "the lamp reports the battery's circuit");
			t.Assert.False(lamp.HasTag(GameTags.HasInvalidPorts), "the lamp has no port overlap");

			battery.GetComponent<Battery>().AddEnergy(10000f);
			EnergyConsumer consumer = lamp.GetComponent<EnergyConsumer>();
			yield return t.WaitUntil(() => consumer.IsPowered, 5, "the lamp powered through both plates");
		}

		/// <summary>One of the vanilla refusals the mod keeps: a plate end inside a tile.</summary>
		[OniTest("Jumper Bridges joint plate: a plate end inside a tile is still refused", Timeout = 30), Layer(0)]
		public static IEnumerator NotInTile(TestContext t)
		{
			CellRect r = Power.Floor(t, 5);
			t.World.Build("Tile", r.Cell(1, 1));
			yield return t.WaitGameSeconds(0.5);

			bool valid = Power.ValidWithPreview(Plate, r.Cell(2, 1), out string reason);
			t.Assert.False(valid, "a plate end in the tile is refused");
			t.Assert.Eq((string)UI.TOOLTIPS.HELP_BUILDLOCATION_HIGHWATT_NOT_IN_TILE, reason, "refused as not in tiles");
		}

		[OniTest("Jumper Bridges joint plate: the centre is a solid tile and the ends draw conductive heavi-watt wire", Timeout = 30), Layer(0)]
		public static IEnumerator SolidCentre(TestContext t)
		{
			CellRect r = t.Rect(3, 1);
			int centre = r.Cell(1, 0);
			int end = r.Cell(0, 0);
			GameObject plate = t.World.Build(ConductivePlate, centre);
			yield return t.WaitUntil(() => Grid.Solid[centre], 3, "the centre cell turns solid");
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, end, out _), 3, "the plate's end node at " + Cells.Describe(end));
			yield return t.WaitGameSeconds(0.3);

			t.Assert.True(Grid.Foundation[centre], "the centre is a foundation cell");
			t.Assert.Eq(plate, t.World.At(centre, ObjectLayer.Building), "the plate is on the building layer at its centre");
			t.Assert.Eq(plate, t.World.At(centre, ObjectLayer.FoundationTile), "the plate is on the tile layer at its centre");
			t.Assert.Eq(plate, t.World.At(end, ObjectLayer.WireConnectors), "the plate holds its free end's connector slot");
			BridgeEndNode.TryGet(BridgeKind.Power, end, out BridgeEndNode node);
			t.Assert.Eq(Assets.GetAnim("utilities_electric_conduct_hiwatt_kanim"), node.GetComponent<KBatchedAnimController>().AnimFiles[0], "the end draws as conductive heavi-watt wire");
		}

		/// <summary>
		/// The wire bridge is built first, so its end holds the connector slot on the shared cell and
		/// only the mod's registry knows the plate's end is there too.
		/// </summary>
		[OniTest("Jumper Bridges joint plate: a plate end on a wire bridge end joins its circuit and keeps a tile off", Timeout = 40), Layer(0)]
		public static IEnumerator OnWireBridgeEnd(TestContext t)
		{
			CellRect r = Power.Floor(t, 7);
			int batteryCell = r.Cell(1, 1);
			int sharedCell = r.Cell(3, 1);
			int lampCell = r.Cell(5, 1);
			t.World.Build("Battery", batteryCell);
			GameObject lamp = t.World.Build("FloorLamp", lampCell);
			GameObject bridge = t.World.Build("WireBridge", r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, sharedCell, out _), 3, "the wire bridge's free end node at " + Cells.Describe(sharedCell));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(bridge, t.World.At(sharedCell, ObjectLayer.WireConnectors), "the wire bridge holds the shared cell's connector slot");

			bool valid = Power.ValidWithPreview(Plate, r.Cell(4, 1), out string reason);
			t.Assert.True(valid, "the plate's end may sit on the wire bridge's end: " + reason);
			t.World.Build(Plate, r.Cell(4, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, lampCell, out _), 3, "the plate's end node on the lamp's port");
			yield return t.WaitGameSeconds(0.5);

			ushort circuit = Power.Circuit(batteryCell);
			t.Assert.NotEq(ushort.MaxValue, circuit, "the battery's port is on a circuit");
			t.Assert.Eq(circuit, Power.Circuit(sharedCell), "the shared end is on the battery's circuit");
			t.Assert.Eq(circuit, lamp.GetComponent<EnergyConsumer>().CircuitID, "the lamp reports the battery's circuit");
			t.Assert.Eq(1000f, Game.Instance.circuitManager.GetMaxSafeWattageForCircuit(circuit), "the circuit's limit is the wire bridge's");
			BridgeEndNode.TryGet(BridgeKind.Power, sharedCell, out BridgeEndNode node);
			t.Assert.Eq(Assets.GetAnim("utilities_electric_insulated_kanim"), node.GetComponent<KBatchedAnimController>().AnimFiles[0], "the shared end switches to heavi-watt wire");

			t.Assert.False(Power.ValidWithPreview("Tile", sharedCell, out reason), "a tile on the shared end is refused");
			t.Assert.Eq((string)UI.TOOLTIPS.HELP_BUILDLOCATION_WIRE_OBSTRUCTION, reason, "refused as a wire obstruction");
		}

		/// <summary>
		/// The plate is built first, so it holds the connector slot on the shared cell and the tile
		/// check refuses through the plate on that slot. The shared end draws as the plate's wire,
		/// then as the wire bridge's once the plate is gone.
		/// </summary>
		[OniTest("Jumper Bridges joint plate: a wire bridge end on a plate end draws heavi-watt wire until the plate goes", Timeout = 40), Layer(0)]
		public static IEnumerator WireBridgeOnPlateEnd(TestContext t)
		{
			CellRect r = Power.Floor(t, 7);
			int batteryCell = r.Cell(1, 1);
			int sharedCell = r.Cell(3, 1);
			int lampCell = r.Cell(5, 1);
			t.World.Build("Battery", batteryCell);
			yield return t.WaitGameSeconds(0.5);
			GameObject lamp = t.World.Build("FloorLamp", lampCell);
			yield return t.WaitGameSeconds(0.5);
			GameObject plate = t.World.Build(Plate, r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, sharedCell, out _), 3, "the plate's free end node at " + Cells.Describe(sharedCell));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(plate, t.World.At(sharedCell, ObjectLayer.WireConnectors), "the plate holds the shared cell's connector slot");

			bool valid = Power.ValidWithPreview("WireBridge", r.Cell(4, 1), out string reason);
			t.Assert.True(valid, "the wire bridge's end may sit on the plate's end: " + reason);
			t.World.Build("WireBridge", r.Cell(4, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, lampCell, out _), 3, "the wire bridge's end node on the lamp's port");
			yield return t.WaitGameSeconds(0.5);

			ushort circuit = Power.Circuit(batteryCell);
			t.Assert.NotEq(ushort.MaxValue, circuit, "the battery's port is on a circuit");
			t.Assert.Eq(circuit, Power.Circuit(sharedCell), "the shared end is on the battery's circuit");
			t.Assert.Eq(circuit, lamp.GetComponent<EnergyConsumer>().CircuitID, "the lamp reports the battery's circuit");
			t.Assert.False(Power.ValidWithPreview("Tile", sharedCell, out reason), "a tile on the shared end is refused");
			t.Assert.Eq((string)UI.TOOLTIPS.HELP_BUILDLOCATION_WIRE_OBSTRUCTION, reason, "refused as a wire obstruction");
			BridgeEndNode.TryGet(BridgeKind.Power, sharedCell, out BridgeEndNode node);
			KBatchedAnimController art = node.GetComponent<KBatchedAnimController>();
			t.Assert.Eq(Assets.GetAnim("utilities_electric_insulated_kanim"), art.AnimFiles[0], "the shared end draws as heavi-watt wire");

			t.World.Destroy(plate);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(BridgeEndNode.TryGet(BridgeKind.Power, sharedCell, out BridgeEndNode remaining) && remaining == node, "the shared end node stays for the wire bridge");
			ushort bridgeCircuit = Power.Circuit(lampCell);
			t.Assert.NotEq(ushort.MaxValue, bridgeCircuit, "the lamp's port is on a circuit");
			t.Assert.Eq(bridgeCircuit, Power.Circuit(sharedCell), "the shared end is on the wire bridge's circuit");
			t.Assert.Eq(Assets.GetAnim("utilities_electric_kanim"), art.AnimFiles[0], "the end draws as the wire bridge's wire");
		}

		/// <summary>
		/// The plate's end sat on a battery's port, so the port held the connector slot; with the
		/// battery gone the slot is empty, and only the mod's registry keeps a tile off the end.
		/// </summary>
		[OniTest("Jumper Bridges joint plate: a tile may not be built on a plate end that sat on a port", Timeout = 40), Layer(0)]
		public static IEnumerator NoTileOnEnd(TestContext t)
		{
			CellRect r = Power.Floor(t, 5);
			int endCell = r.Cell(1, 1);
			GameObject battery = t.World.Build("Battery", endCell);
			t.World.Build(Plate, r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, endCell, out _), 3, "the end node on the battery's port");
			yield return t.WaitGameSeconds(0.5);

			t.World.Destroy(battery);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Null(t.World.At(endCell, ObjectLayer.WireConnectors), "nothing holds the connector slot at the end");
			t.Assert.True(BridgeEndNode.TryGet(BridgeKind.Power, endCell, out _), "the end node stays");
			t.Assert.False(Power.ValidWithPreview("Tile", endCell, out string reason), "a tile on the plate's end is refused");
			t.Assert.Eq((string)UI.TOOLTIPS.HELP_BUILDLOCATION_WIRE_OBSTRUCTION, reason, "refused as a wire obstruction");
		}

		/// <summary>Unlike a wire bridge, the plate is on the building layer at its centre, as in the base game.</summary>
		[OniTest("Jumper Bridges joint plate: a plate between two ports is marked for deconstruction from its centre", Timeout = 40), Layer(1)]
		public static IEnumerator DeconstructFromCentre(TestContext t)
		{
			CellRect r = Power.Floor(t, 5);
			t.World.Build("Battery", r.Cell(1, 1));
			t.World.Build("FloorLamp", r.Cell(3, 1));
			GameObject plate = t.World.Build(Plate, r.Cell(2, 1));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.NotEq(plate, t.World.At(r.Cell(3, 1), ObjectLayer.WireConnectors), "the lamp's port keeps the connector slot");

			t.InstantBuild(false);
			PlayerController.Instance.ActivateTool(DeconstructTool.Instance);
			t.Cleanup(() => SelectTool.Instance.Activate());
			yield return t.Frames(2);
			Deconstructable deconstructable = plate.GetComponent<Deconstructable>();
			DeconstructTool.Instance.DeconstructCell(r.Cell(2, 1));
			yield return t.Frames(2);
			t.Assert.True(deconstructable.IsMarkedForDeconstruction(), "the centre cell marks the plate");
		}
	}
}
