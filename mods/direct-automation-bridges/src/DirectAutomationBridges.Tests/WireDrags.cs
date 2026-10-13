using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace DirectAutomationBridges.Tests
{
	/// <summary>
	/// Wires meeting bridge ends, driven through the real wire tool: the bug this guards lives
	/// in BaseUtilityBuildTool.BuildPath and the mod's patches around it, which no API call
	/// exercises.
	/// </summary>
	public static class WireDrags
	{
		[OniTest("DAB: wire dragged from a bridge end to its neighbour draws both ways", Timeout = 30), Layer(1)]
		public static IEnumerator DragFromEnd(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			int neighbour = r.Cell(1, 0);
			int end = r.Cell(2, 0);
			int middle = r.Cell(3, 0);
			t.World.Build("LogicWireBridge", middle);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(end, out _), 3, "the bridge's end node at " + Cells.Describe(end));
			t.Assert.Eq((UtilityConnections)0, t.World.LogicConnections(end), "the fresh end has no connections");

			UtilityDragSession wire = t.Tools.UtilityTool("LogicWire");
			yield return wire.Path(end, neighbour);
			t.Assert.Eq(Color.white, wire.PreviewTint(end), "the end cell previews white");
			t.Assert.Eq(Color.white, wire.PreviewTint(neighbour), "the neighbour cell previews white");
			wire.Dispose();
			yield return t.Frames(2);

			t.Assert.Null(t.World.At(end, ObjectLayer.LogicWire), "no wire piece on the end cell");
			t.Assert.NotNull(t.World.At(neighbour, ObjectLayer.LogicWire), "a wire on the neighbour cell");
			t.Assert.Has(UtilityConnections.Right, t.World.LogicConnections(neighbour), "the wire draws into the end");
			t.Assert.Has(UtilityConnections.Left, t.World.LogicConnections(end), "the end draws back to the wire");
			t.Assert.True(BridgeEndNode.TryGet(end, out BridgeEndNode node) && node != null, "the end node is still there");

			yield return t.WaitGameSeconds(0.3);
			LogicCircuitNetwork network = t.World.LogicNetwork(neighbour);
			t.Assert.NotNull(network, "the wire is on a logic network");
			t.Assert.True(ReferenceEquals(network, t.World.LogicNetwork(end)), "the end is on the wire's network");
		}

		/// <summary>Baseline with no bridge anywhere: a queued wire drag between two empty cells, so a failure in the queued bridge test can be told from a harness problem.</summary>
		[OniTest("control: a queued wire drag between empty cells leaves two joined orders", Timeout = 30), Layer(1)]
		public static IEnumerator QueuedWireControl(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			int a = r.Cell(1, 0);
			int b = r.Cell(2, 0);
			t.InstantBuild(false);
			UtilityDragSession wire = t.Tools.UtilityTool("LogicWire");
			yield return wire.Path(a, b);
			wire.Dispose();
			yield return t.Frames(2);
			t.Assert.NotNull(t.World.At(a, ObjectLayer.LogicWire)?.GetComponent<Constructable>(), "an order at the first cell");
			t.Assert.NotNull(t.World.At(b, ObjectLayer.LogicWire)?.GetComponent<Constructable>(), "an order at the second cell");
			t.Assert.Has(UtilityConnections.Right, Game.Instance.logicCircuitSystem.GetDisplayConnections(a), "the first order draws toward the second");
			t.Assert.Has(UtilityConnections.Left, Game.Instance.logicCircuitSystem.GetDisplayConnections(b), "the second order draws toward the first");
		}

		/// <summary>
		/// The same drag with instant build off, as a player without sandbox makes it: the wire is
		/// only an order, so nothing physical sits on the neighbour cell yet and the game's own
		/// network rebuild has no bit to echo back onto the end. The end drawing its segment toward
		/// the queued wire is the mod's doing alone (the end mirrors its neighbours' display bits
		/// after the drag), which is what makes this the regression test for the tool-path patches.
		/// </summary>
		[OniTest("DAB: wire dragged from a bridge end to its neighbour, as an order, draws the end's segment at once", Timeout = 30), Layer(1)]
		public static IEnumerator DragFromEndQueued(TestContext t)
		{
			CellRect r = t.Rect(6, 1);
			int neighbour = r.Cell(1, 0);
			int end = r.Cell(2, 0);
			int middle = r.Cell(3, 0);
			t.World.Build("LogicWireBridge", middle);
			yield return t.WaitUntil(() => BridgeEndNode.TryGet(end, out _), 3, "the bridge's end node at " + Cells.Describe(end));
			t.InstantBuild(false);

			UtilityDragSession wire = t.Tools.UtilityTool("LogicWire");
			yield return wire.Path(end, neighbour);
			t.Assert.Eq(Color.white, wire.PreviewTint(end), "the end cell previews white");
			t.Assert.Eq(Color.white, wire.PreviewTint(neighbour), "the neighbour cell previews white");
			wire.Dispose();
			yield return t.Frames(2);

			t.Assert.Null(t.World.At(end, ObjectLayer.LogicWire), "no wire order on the end cell");
			GameObject order = t.World.At(neighbour, ObjectLayer.LogicWire);
			t.Assert.NotNull(order, "a wire order on the neighbour cell");
			t.Assert.NotNull(order.GetComponent<Constructable>(), "the neighbour's wire is an order, not built");
			t.Assert.Has(UtilityConnections.Right, Game.Instance.logicCircuitSystem.GetDisplayConnections(neighbour), "the order draws into the end");
			// Physical bits only ever point at physical items (SetConnections masks them by the node set,
			// for wires as for ends), so toward a queued order the segment lives in the display grid,
			// which is what the wire art is drawn from.
			t.Assert.Has(UtilityConnections.Left, Game.Instance.logicCircuitSystem.GetDisplayConnections(end), "the end draws back toward the queued wire");
			t.Assert.True(BridgeEndNode.TryGet(end, out BridgeEndNode node) && node != null, "the end node is still there");
		}
	}
}
