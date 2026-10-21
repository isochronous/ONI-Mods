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
		/// Both directions, with the first end on a battery's port so that it does not hold the
		/// connector slot and only the mod's registry knows it is there. Each preview is valid
		/// before the other bridge is built.
		/// </summary>
		[OniTest("Jumper Bridges joint plate: a plate end and a wire bridge end may not share a cell", Timeout = 40), Layer(0)]
		public static IEnumerator NotOnWireBridgeEnd(TestContext t)
		{
			CellRect r = Power.Floor(t, 10);
			t.World.Build("Battery", r.Cell(2, 1));
			t.World.Build("Battery", r.Cell(7, 1));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.True(Power.ValidWithPreview(Plate, r.Cell(1, 1), out string reason), "a plate end on the free battery port: " + reason);
			t.Assert.True(Power.ValidWithPreview("WireBridge", r.Cell(6, 1), out reason), "a wire bridge end on the free battery port: " + reason);

			t.World.Build("WireBridge", r.Cell(3, 1));
			t.World.Build(Plate, r.Cell(8, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, r.Cell(2, 1), out _) && BridgeEndNode.TryGet(BridgeKind.Power, r.Cell(7, 1), out _), 3, "end nodes on both battery ports");
			yield return t.WaitGameSeconds(0.5);

			t.Assert.False(Power.ValidWithPreview(Plate, r.Cell(1, 1), out reason), "a plate end on the wire bridge's end is refused");
			t.Assert.Eq((string)UI.TOOLTIPS.HELP_BUILDLOCATION_WIRECONNECTORS_OVERLAP, reason, "refused as a connector overlap");
			t.Assert.False(Power.ValidWithPreview("WireBridge", r.Cell(6, 1), out reason), "a wire bridge end on the plate's end is refused");
			t.Assert.Eq((string)UI.TOOLTIPS.HELP_BUILDLOCATION_WIRECONNECTORS_OVERLAP, reason, "refused as a connector overlap");
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
