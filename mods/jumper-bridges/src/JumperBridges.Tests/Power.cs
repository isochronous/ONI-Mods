using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace JumperBridges.Tests
{
	/// <summary>
	/// Power bridges with their ends directly on power ports: a battery's output and a lamp's input
	/// on one floor, the bridge between them, no wire anywhere.
	/// </summary>
	public static class Power
	{
		private const string Bridge = "WireBridge";

		/// <summary>Tiles along the bottom row, so the floor buildings on row 1 can stand.</summary>
		private static CellRect Floor(TestContext t, int width)
		{
			CellRect r = t.Rect(width, 3);
			for (int x = 0; x < width; x++)
				t.World.Build("Tile", r.Cell(x, 0));
			return r;
		}

		private static ushort Circuit(int cell) => Game.Instance.circuitManager.GetCircuitID(cell);

		/// <summary>
		/// What the build tool asks when the player hovers the def over the cell: the placement
		/// check with a live preview object as the source, as BuildTool's visualizer. World.Build
		/// checks with no source object, which skips the port checks this mod changes.
		/// </summary>
		private static bool ValidWithPreview(string prefabId, int cell, out string reason)
		{
			BuildingDef def = Assets.GetBuildingDef(prefabId);
			GameObject preview = GameUtil.KInstantiate(def.BuildingPreview, Grid.CellToPosCBC(cell, def.SceneLayer), Grid.SceneLayer.Ore, null, LayerMask.NameToLayer("Place"));
			preview.SetActive(true);
			try
			{
				return def.IsValidPlaceLocation(preview, cell, Orientation.Neutral, out reason);
			}
			finally
			{
				Object.Destroy(preview);
			}
		}

		[OniTest("Jumper Bridges power: battery to bridge to lamp is one circuit with no wire", Timeout = 40), Layer(0)]
		public static IEnumerator BatteryBridgeLamp(TestContext t)
		{
			CellRect r = Floor(t, 5);
			int batteryCell = r.Cell(1, 1);
			int lampCell = r.Cell(3, 1);
			GameObject battery = t.World.Build("Battery", batteryCell);
			GameObject lamp = t.World.Build("FloorLamp", lampCell);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(ushort.MaxValue, Circuit(lampCell), "the lamp's port is on no circuit before the bridge");

			bool valid = ValidWithPreview(Bridge, r.Cell(2, 1), out string reason);
			t.Assert.True(valid, "the bridge may be placed with its ends on both ports: " + reason);
			GameObject bridge = t.World.Build(Bridge, r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, batteryCell, out _) && BridgeEndNode.TryGet(BridgeKind.Power, lampCell, out _), 3, "end nodes on both ports");
			yield return t.WaitGameSeconds(0.5);

			ushort circuit = Circuit(batteryCell);
			t.Assert.NotEq(ushort.MaxValue, circuit, "the battery's port is on a circuit");
			t.Assert.Eq(circuit, Circuit(lampCell), "the lamp's port is on the battery's circuit");
			t.Assert.Eq(circuit, lamp.GetComponent<EnergyConsumer>().CircuitID, "the lamp reports the battery's circuit");
			t.Assert.False(battery.HasTag(GameTags.HasInvalidPorts), "the battery has no port overlap");
			t.Assert.False(lamp.HasTag(GameTags.HasInvalidPorts), "the lamp has no port overlap");
			t.Assert.False(bridge.HasTag(GameTags.HasInvalidPorts), "the bridge has no port overlap");
			t.Assert.True(lamp.GetComponent<Operational>().GetFlag(InvalidPortReporter.portsNotOverlapping), "the lamp is not put out of order");
		}

		/// <summary>The other order: bridge first, then a building whose power port lands on the bridge's end.</summary>
		[OniTest("Jumper Bridges power: a lamp placed with its port on a bridge end joins the bridge's circuit", Timeout = 40), Layer(0)]
		public static IEnumerator LampOntoBridgeEnd(TestContext t)
		{
			CellRect r = Floor(t, 5);
			int batteryCell = r.Cell(1, 1);
			int lampCell = r.Cell(3, 1);
			t.World.Build("Battery", batteryCell);
			GameObject bridge = t.World.Build(Bridge, r.Cell(2, 1));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, lampCell, out _), 3, "the bridge's free end node");
			yield return t.WaitGameSeconds(0.5);

			bool valid = ValidWithPreview("FloorLamp", lampCell, out string reason);
			t.Assert.True(valid, "the lamp may be placed with its port on the bridge's end: " + reason);
			GameObject lamp = t.World.Build("FloorLamp", lampCell);
			yield return t.WaitGameSeconds(0.5);
			t.Assert.Eq(Circuit(batteryCell), lamp.GetComponent<EnergyConsumer>().CircuitID, "the lamp is on the battery's circuit");
			t.Assert.NotEq(ushort.MaxValue, Circuit(lampCell), "the lamp's port is on a circuit");
			t.Assert.False(bridge.HasTag(GameTags.HasInvalidPorts), "the bridge has no port overlap");
			t.Assert.False(lamp.HasTag(GameTags.HasInvalidPorts), "the lamp has no port overlap");
		}

		[OniTest("Jumper Bridges power: wire dragged from a bridge end to its neighbour draws both ways", Timeout = 30), Layer(1)]
		public static IEnumerator DragFromEnd(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			int neighbour = r.Cell(1, 0);
			int end = r.Cell(2, 0);
			t.World.Build(Bridge, r.Cell(3, 0));
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(BridgeKind.Power, end, out _), 3, "the bridge's end node at " + Cells.Describe(end));
			IUtilityNetworkMgr system = Game.Instance.electricalConduitSystem;
			t.Assert.Eq((UtilityConnections)0, system.GetConnections(end, is_physical_building: true), "the fresh end has no connections");

			UtilityDragSession wire = t.Tools.UtilityTool("Wire");
			yield return wire.Path(end, neighbour);
			t.Assert.Eq(Color.white, wire.PreviewTint(end), "the end cell previews white");
			wire.Dispose();
			yield return t.Frames(2);

			t.Assert.Null(t.World.At(end, ObjectLayer.Wire), "no wire piece on the end cell");
			t.Assert.NotNull(t.World.At(neighbour, ObjectLayer.Wire), "a wire on the neighbour cell");
			t.Assert.Has(UtilityConnections.Right, system.GetConnections(neighbour, is_physical_building: true), "the wire draws into the end");
			t.Assert.Has(UtilityConnections.Left, system.GetConnections(end, is_physical_building: true), "the end draws back to the wire");
			t.Assert.True(BridgeEndNode.TryGet(BridgeKind.Power, end, out BridgeEndNode node) && node != null, "the end node is still there");
			t.Assert.False(node.GetComponent<KBatchedAnimController>().GetSymbolVisiblity(Wire.OutlineSymbol), "the end piece hides the wire art's outline, as a wire does");
			yield return t.WaitGameSeconds(0.3);
			t.Assert.NotEq(ushort.MaxValue, Circuit(neighbour), "the wire is on a circuit");
			t.Assert.Eq(Circuit(neighbour), Circuit(end), "the end is on the wire's circuit");
		}

		/// <summary>Both ends on ports, so the bridge is on no layer and only the mod's registry finds it: from the ends alone.</summary>
		[OniTest("Jumper Bridges power: a bridge between two ports is marked for deconstruction from its ends, not its middle", Timeout = 40), Layer(1)]
		public static IEnumerator DeconstructBetweenPorts(TestContext t)
		{
			CellRect r = Floor(t, 5);
			t.World.Build("Battery", r.Cell(1, 1));
			t.World.Build("FloorLamp", r.Cell(3, 1));
			GameObject bridge = t.World.Build(Bridge, r.Cell(2, 1));
			yield return t.WaitGameSeconds(0.5);
			t.Assert.NotEq(bridge, t.World.At(r.Cell(3, 1), ObjectLayer.WireConnectors), "the lamp's port keeps the layer slot");

			t.InstantBuild(false);
			PlayerController.Instance.ActivateTool(DeconstructTool.Instance);
			t.Cleanup(() => SelectTool.Instance.Activate());
			yield return t.Frames(2);
			Deconstructable deconstructable = bridge.GetComponent<Deconstructable>();
			DeconstructTool.Instance.DeconstructCell(r.Cell(2, 1));
			yield return t.Frames(2);
			t.Assert.False(deconstructable.IsMarkedForDeconstruction(), "the middle cell does not mark the bridge");
			DeconstructTool.Instance.DeconstructCell(r.Cell(3, 1));
			yield return t.Frames(2);
			t.Assert.True(deconstructable.IsMarkedForDeconstruction(), "an end cell marks the bridge");
		}
	}
}
