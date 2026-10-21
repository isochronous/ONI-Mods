using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace JumperBridges
{
	/// <summary>Both automation bridges get the component that owns their end nodes, and the cell registry.</summary>
	[HarmonyPatch]
	public static class LogicBridgeConfigs_DoPostConfigureComplete_Patch
	{
		public static bool Prepare() => BridgeKind.Logic.Enabled;

		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(LogicWireBridgeConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
			yield return AccessTools.Method(typeof(LogicRibbonBridgeConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
		}

		public static void Postfix(GameObject go)
		{
			go.AddOrGet<BridgeFootprint>();
			go.AddOrGet<JumperBridge>();
		}
	}

	/// <summary>A queued bridge is in the cell registry too, so the cancel tool finds it on a taken cell.</summary>
	[HarmonyPatch]
	public static class LogicBridgeConfigs_DoPostConfigureUnderConstruction_Patch
	{
		public static bool Prepare() => BridgeKind.Logic.Enabled;

		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(LogicWireBridgeConfig), nameof(IBuildingConfig.DoPostConfigureUnderConstruction));
			yield return AccessTools.Method(typeof(LogicRibbonBridgeConfig), nameof(IBuildingConfig.DoPostConfigureUnderConstruction));
		}

		public static void Postfix(GameObject go)
		{
			go.AddOrGet<BridgeFootprint>();
		}
	}

	/// <summary>
	/// The Wire Bridge, and through it the Conductive and Insulated Wire Bridges, which inherit
	/// its configuration methods without overriding them. The joint plates have a config of their
	/// own, patched below.
	/// </summary>
	[HarmonyPatch(typeof(WireBridgeConfig), nameof(WireBridgeConfig.DoPostConfigureComplete))]
	public static class WireBridgeConfig_DoPostConfigureComplete_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static void Postfix(GameObject go)
		{
			go.AddOrGet<BridgeFootprint>();
			go.AddOrGet<JumperBridge>();
		}
	}

	[HarmonyPatch(typeof(WireBridgeConfig), nameof(WireBridgeConfig.DoPostConfigureUnderConstruction))]
	public static class WireBridgeConfig_DoPostConfigureUnderConstruction_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static void Postfix(GameObject go)
		{
			go.AddOrGet<BridgeFootprint>();
		}
	}

	/// <summary>
	/// The Heavi-Watt Joint Plate, and through it the Heavi-Watt Conductive Joint Plate, which
	/// inherits DoPostConfigureComplete and calls this DoPostConfigureUnderConstruction first.
	/// Both methods add the network link before returning, so the postfix finds it.
	/// </summary>
	[HarmonyPatch(typeof(WireBridgeHighWattageConfig), nameof(WireBridgeHighWattageConfig.DoPostConfigureComplete))]
	public static class WireBridgeHighWattageConfig_DoPostConfigureComplete_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static void Postfix(GameObject go)
		{
			go.AddOrGet<BridgeFootprint>();
			go.AddOrGet<JumperBridge>();
		}
	}

	[HarmonyPatch(typeof(WireBridgeHighWattageConfig), nameof(WireBridgeHighWattageConfig.DoPostConfigureUnderConstruction))]
	public static class WireBridgeHighWattageConfig_DoPostConfigureUnderConstruction_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static void Postfix(GameObject go)
		{
			go.AddOrGet<BridgeFootprint>();
		}
	}

	/// <summary>
	/// Two bridge ends on one cell overwrite each other's link in the network manager; the mod
	/// rewrites the links of the whole group after the game's own AddLink/RemoveLink (BridgeLinks).
	/// The circuit managers' Connect/Disconnect, the overload bookkeeping, are left to the game.
	/// A joint plate's link is a WireUtilityNetworkLink too and joins the power groups.
	/// </summary>
	[HarmonyPatch]
	public static class UtilityNetworkLink_OnConnect_Patch
	{
		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(LogicUtilityNetworkLink), "OnConnect");
			yield return AccessTools.Method(typeof(WireUtilityNetworkLink), "OnConnect");
		}

		public static void Postfix(UtilityNetworkLink __instance)
		{
			BridgeKind kind = BridgeKind.OfBridge(__instance.gameObject);
			if (kind != null)
				BridgeLinks.OnConnect(__instance, kind);
		}
	}

	[HarmonyPatch]
	public static class UtilityNetworkLink_OnDisconnect_Patch
	{
		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(LogicUtilityNetworkLink), "OnDisconnect");
			yield return AccessTools.Method(typeof(WireUtilityNetworkLink), "OnDisconnect");
		}

		public static void Postfix(UtilityNetworkLink __instance, int cell1, int cell2)
		{
			BridgeKind kind = BridgeKind.OfBridge(__instance.gameObject);
			if (kind != null)
				BridgeLinks.OnDisconnect(__instance, kind, cell1, cell2);
		}
	}

	/// <summary>
	/// The game marks a bridge on its end layer at its two end cells (LogicGate for automation,
	/// WireConnectors for power) and tags whatever was there (a gate, a machine with a power port,
	/// another bridge) with HasInvalidPorts, which puts that building out of order. A bridge end
	/// may now sit on a port or another bridge's end, so a bridge marks only end cells that are
	/// free and never tags. The tools that sweep layers find such a bridge through BridgeFootprint.
	/// A joint plate is a solid tile at its centre: it keeps the vanilla footprint marks there
	/// (Building and FoundationTile from BuildingComplete, the object or replacement layer and
	/// then the tile layer from Constructable), so the centre stays registered as a tile.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.MarkArea))]
	public static class BuildingDef_MarkArea_Patch
	{
		public static bool Prefix(BuildingDef __instance, int cell, Orientation orientation, ObjectLayer layer, GameObject go)
		{
			BridgeKind kind = BridgeKind.Of(__instance);
			if (kind == null || !kind.Enabled)
				return true;
			// The vanilla method does nothing else for these rules: the wire and automation bridges
			// get no footprint marks, the joint plates get the loop below, and none of them has
			// conduit, power or secondary ports.
			if (__instance.BuildLocationRule == BuildLocationRule.HighWattBridgeTile)
				MarkFootprint(__instance, cell, orientation, layer, go);
			if (kind == BridgeKind.Logic)
			{
				LogicPorts ports = go.GetComponent<LogicPorts>();
				if (ports != null && ports.inputPortInfo != null)
					foreach (LogicPorts.Port port in ports.inputPortInfo)
						MarkIfFree(Grid.OffsetCell(cell, Rotatable.GetRotatedCellOffset(port.cellOffset, orientation)), layer, go);
			}
			else
			{
				UtilityNetworkLink link = go.GetComponent<UtilityNetworkLink>();
				if (link != null)
				{
					link.GetCells(cell, orientation, out int end1, out int end2);
					MarkIfFree(end1, kind.EndLayer, go);
					MarkIfFree(end2, kind.EndLayer, go);
				}
			}
			return false;
		}

		/// <summary>The vanilla footprint loop: an uprootable plant on the layer moves to layer 5 (Plants) first.</summary>
		private static void MarkFootprint(BuildingDef def, int cell, Orientation orientation, ObjectLayer layer, GameObject go)
		{
			foreach (CellOffset offset in def.PlacementOffsets)
			{
				int target = Grid.OffsetCell(cell, Rotatable.GetRotatedCellOffset(offset, orientation));
				GameObject existing = Grid.Objects[target, (int)layer];
				if (Uprootable.CanUproot(existing))
					Grid.Objects[target, (int)ObjectLayer.Plants] = existing;
				Grid.Objects[target, (int)layer] = go;
			}
		}

		private static void MarkIfFree(int cell, ObjectLayer layer, GameObject go)
		{
			if (Grid.IsValidCell(cell) && Grid.Objects[cell, (int)layer] == null)
				Grid.Objects[cell, (int)layer] = go;
		}
	}

	/// <summary>
	/// Power placement. Vanilla refuses a power bridge end or joint plate end on any cell already
	/// holding a power port or another bridge's end; either may now sit on a port, a wire bridge
	/// end on a wire bridge end and a joint plate end on a joint plate end. A wire bridge end and
	/// a joint plate end never share a cell, whichever comes first. Both are looked up in
	/// BridgeFootprint, since an end on a port does not hold the WireConnectors slot.
	/// </summary>
	internal static class PowerPorts
	{
		/// <summary>Whether the object on the WireConnectors layer is a direct power bridge or joint plate with an end on the cell.</summary>
		public static bool IsBridgeEnd(GameObject go, int cell)
		{
			if (go == null)
				return false;
			BridgeFootprint footprint = go.GetComponent<BridgeFootprint>();
			return footprint != null && footprint.Kind == BridgeKind.Power && footprint.IsEnd(cell);
		}

		/// <summary>Whether a power bridge or joint plate other than the one being placed has an end on the cell.</summary>
		public static bool HasEnd(int cell, GameObject self, bool jointPlate)
		{
			if (!Grid.IsValidCell(cell))
				return false;
			GameObject onLayer = Grid.Objects[cell, (int)ObjectLayer.WireConnectors];
			if (onLayer != null && onLayer != self && BridgeKind.IsJointPlate(onLayer) == jointPlate && IsBridgeEnd(onLayer, cell))
				return true;
			return BridgeFootprint.AnyPowerEndAt(cell, self, jointPlate);
		}
	}

	[HarmonyPatch(typeof(BuildingDef), "IsValidWireBridgeLocation")]
	public static class BuildingDef_IsValidWireBridgeLocation_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static bool Prefix(GameObject source_go, ref string fail_reason, ref bool __result)
		{
			UtilityNetworkLink link = source_go != null ? source_go.GetComponent<UtilityNetworkLink>() : null;
			if (link == null)
				return true;
			link.GetCells(out int end1, out int end2);
			if (PowerPorts.HasEnd(end1, source_go, jointPlate: true) || PowerPorts.HasEnd(end2, source_go, jointPlate: true))
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_WIRECONNECTORS_OVERLAP;
				__result = false;
			}
			else
			{
				fail_reason = null;
				__result = true;
			}
			return false;
		}
	}

	/// <summary>
	/// The joint plate's end check, with ends allowed on ports and on other joint plates' ends
	/// and refused on wire bridge ends. The vanilla method's other refusals stay: an end off the
	/// map, an end in a tile (FoundationTile), an end in a door or a door under construction.
	/// The centre is checked as a tile by the caller (IsValidTileLocation) as before.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), "IsValidHighWattBridgeLocation")]
	public static class BuildingDef_IsValidHighWattBridgeLocation_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static bool Prefix(GameObject source_go, int cell, Orientation orientation, ref string fail_reason, ref bool __result)
		{
			fail_reason = null;
			__result = true;
			UtilityNetworkLink link = source_go != null ? source_go.GetComponent<UtilityNetworkLink>() : null;
			if (link == null)
				return false;
			if (!link.AreCellsValid(cell, orientation))
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_INVALID_CELL;
				__result = false;
				return false;
			}
			link.GetCells(out int end1, out int end2);
			if (PowerPorts.HasEnd(end1, source_go, jointPlate: false) || PowerPorts.HasEnd(end2, source_go, jointPlate: false))
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_WIRECONNECTORS_OVERLAP;
				__result = false;
			}
			else if (InTileOrDoor(end1) || InTileOrDoor(end2))
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_HIGHWATT_NOT_IN_TILE;
				__result = false;
			}
			return false;
		}

		private static bool InTileOrDoor(int cell)
		{
			if (Grid.Objects[cell, (int)ObjectLayer.FoundationTile] != null || Grid.HasDoor[cell])
				return true;
			GameObject building = Grid.Objects[cell, (int)ObjectLayer.Building];
			BuildingUnderConstruction queued = building != null ? building.GetComponent<BuildingUnderConstruction>() : null;
			return queued != null && queued.Def.BuildingComplete.GetComponent<Door>() != null;
		}
	}

	/// <summary>
	/// Vanilla keeps a tile, and another joint plate's centre, off a joint plate's end because the
	/// plate holds the WireConnectors slot there. A plate end on a port, or on a shared end whose
	/// slot holder has gone, does not hold it; the plate is found through BridgeFootprint instead,
	/// so the end node is never buried in a solid tile.
	/// The vanilla method's first check reads the Building of whatever is on the WireTile layer,
	/// and a power node has none; on a node's cell the prefix runs the method's other two checks
	/// and counts the node as the wire it stands for, which a tile may cover.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), "IsValidTileLocation")]
	public static class BuildingDef_IsValidTileLocation_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static bool Prefix(GameObject source_go, int cell, bool replacement_tile, ref string fail_reason, ref bool __result)
		{
			GameObject onWireTile = Grid.Objects[cell, (int)ObjectLayer.WireTile];
			if (onWireTile == null || onWireTile.GetComponent<BridgeEndNode>() == null)
				return true;
			__result = true;
			if (RuleOf(Grid.Objects[cell, (int)ObjectLayer.WireConnectors], source_go) == BuildLocationRule.HighWattBridgeTile)
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_WIRE_OBSTRUCTION;
				__result = false;
			}
			else if (!replacement_tile && RuleOf(Grid.Objects[cell, (int)ObjectLayer.Backwall], source_go) == BuildLocationRule.NotInTiles)
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_BACK_WALL;
				__result = false;
			}
			return false;
		}

		/// <summary>The placement rule of a building other than the source; Anywhere for nothing.</summary>
		private static BuildLocationRule RuleOf(GameObject go, GameObject source_go)
		{
			Building building = go != null && go != source_go ? go.GetComponent<Building>() : null;
			return building != null ? building.Def.BuildLocationRule : BuildLocationRule.Anywhere;
		}

		public static void Postfix(GameObject source_go, int cell, ref string fail_reason, ref bool __result)
		{
			if (__result && PowerPorts.HasEnd(cell, source_go, jointPlate: true))
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_WIRE_OBSTRUCTION;
				__result = false;
			}
		}
	}

	/// <summary>
	/// A building placed so that its power port lands on a direct power bridge's end: the vanilla
	/// check sees the bridge on the WireConnectors layer and refuses. The same check with a direct
	/// bridge end counted as free; anything else on the cell refuses as before.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), "ArePowerPortsInValidPositions")]
	public static class BuildingDef_ArePowerPortsInValidPositions_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static bool Prefix(BuildingDef __instance, GameObject source_go, int cell, Orientation orientation, ref string fail_reason, ref bool __result)
		{
			fail_reason = null;
			__result = true;
			if (source_go == null)
				return false;
			if ((__instance.RequiresPowerInput && Taken(source_go, Grid.OffsetCell(cell, Rotatable.GetRotatedCellOffset(__instance.PowerInputOffset, orientation))))
				|| (__instance.RequiresPowerOutput && Taken(source_go, Grid.OffsetCell(cell, Rotatable.GetRotatedCellOffset(__instance.PowerOutputOffset, orientation)))))
			{
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_WIRECONNECTORS_OVERLAP;
				__result = false;
			}
			return false;
		}

		private static bool Taken(GameObject source_go, int portCell)
		{
			GameObject go = Grid.Objects[portCell, (int)ObjectLayer.WireConnectors];
			return go != null && go != source_go && !PowerPorts.IsBridgeEnd(go, portCell);
		}
	}

	/// <summary>
	/// That building then marks its port on the WireConnectors layer over the bridge's end and
	/// tags the bridge with HasInvalidPorts, which shows "Invalid port overlap" on it. A direct power
	/// bridge is never tagged; the port takes the layer slot, and the bridge is found through
	/// BridgeFootprint from then on.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.MarkOverlappingPorts))]
	public static class BuildingDef_MarkOverlappingPorts_Patch
	{
		public static bool Prepare() => BridgeKind.Power.Enabled;

		public static bool Prefix(GameObject existing, GameObject replaced)
		{
			if (existing == null || existing == replaced)
				return true;
			BridgeFootprint footprint = existing.GetComponent<BridgeFootprint>();
			return footprint == null || footprint.Kind != BridgeKind.Power;
		}
	}

	/// <summary>
	/// A bridge that could not take its end layer at a cell is invisible to the tools that sweep
	/// Grid.Objects. Each tool's per-cell action gets the bridges with an end at the cell that the
	/// sweep did not already reach, and does to them what it does to a layer object. Only the
	/// ends, as in the base game, where a bridge is on the layer at its end cells alone.
	/// Deconstruct and cancel are triggers (idempotent on the receiver); prioritize goes through
	/// the tool's own TryPrioritizeGameObject.
	/// </summary>
	internal static class SweptBridges
	{
		public static List<GameObject> Missed(int cell)
		{
			List<GameObject> result = BridgeFootprint.EndsAt(cell);
			result.RemoveAll(go => Grid.Objects[cell, (int)go.GetComponent<BridgeFootprint>().Kind.EndLayer] == go);
			return result;
		}
	}

	[HarmonyPatch(typeof(DeconstructTool), nameof(DeconstructTool.DeconstructCell))]
	public static class DeconstructTool_DeconstructCell_Patch
	{
		public static void Postfix(DeconstructTool __instance, int cell)
		{
			foreach (GameObject go in SweptBridges.Missed(cell))
			{
				if (!__instance.IsActiveLayer(__instance.GetFilterLayerFromGameObject(go)))
					continue;
				go.Trigger((int)GameHashes.MarkForDeconstruct);
				Prioritizable prioritizable = go.GetComponent<Prioritizable>();
				if (prioritizable != null)
					prioritizable.SetMasterPriority(ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority());
			}
		}
	}

	[HarmonyPatch(typeof(CancelTool), "OnDragTool")]
	public static class CancelTool_OnDragTool_Patch
	{
		public static void Postfix(CancelTool __instance, int cell)
		{
			foreach (GameObject go in SweptBridges.Missed(cell))
				if (__instance.IsActiveLayer(__instance.GetFilterLayerFromGameObject(go)))
					go.Trigger((int)GameHashes.Cancel);
		}
	}

	[HarmonyPatch(typeof(PrioritizeTool), "OnDragTool")]
	public static class PrioritizeTool_OnDragTool_Patch
	{
		private static readonly System.Func<PrioritizeTool, GameObject, PrioritySetting, bool> TryPrioritize =
			AccessTools.MethodDelegate<System.Func<PrioritizeTool, GameObject, PrioritySetting, bool>>(
				AccessTools.Method(typeof(PrioritizeTool), "TryPrioritizeGameObject"));

		public static void Postfix(PrioritizeTool __instance, int cell)
		{
			PrioritySetting priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();
			int count = 0;
			foreach (GameObject go in SweptBridges.Missed(cell))
				if (TryPrioritize(__instance, go, priority))
					count++;
			if (count > 0)
				PriorityScreen.PlayPriorityConfirmSound(priority);
		}
	}

	/// <summary>
	/// The automation bridges carry two input ports of their own, one per end. They are kept as
	/// geometry (BuildingDef.MarkArea finds the end cells through LogicPorts.inputPortInfo) but
	/// must never act as ports: the network keeps one endpoint per cell, last writer wins silently,
	/// so a bridge port on a machine's port cell would displace the machine's. These three
	/// prefixes stop a bridge's LogicPorts from registering endpoints (complete bridge), from
	/// removing anything on cleanup (never touches the machine's endpoint), and from creating the
	/// overlay visualizers (preview and under construction) that the placement check compares
	/// against. Power bridges have no ports of this kind.
	/// </summary>
	internal static class BridgePorts
	{
		public static bool IsBridge(LogicPorts ports) => BridgeKind.Logic.Enabled && ports.GetComponent<LogicUtilityNetworkLink>() != null;
	}

	[HarmonyPatch(typeof(LogicPorts), "CreatePhysicalPorts")]
	public static class LogicPorts_CreatePhysicalPorts_Patch
	{
		public static bool Prefix(LogicPorts __instance) => !BridgePorts.IsBridge(__instance);
	}

	[HarmonyPatch(typeof(LogicPorts), "DestroyPhysicalPorts")]
	public static class LogicPorts_DestroyPhysicalPorts_Patch
	{
		public static bool Prefix(LogicPorts __instance) => !BridgePorts.IsBridge(__instance);
	}

	[HarmonyPatch(typeof(LogicPorts), "CreateVisualizers")]
	public static class LogicPorts_CreateVisualizers_Patch
	{
		public static bool Prefix(LogicPorts __instance) => !BridgePorts.IsBridge(__instance);
	}

	/// <summary>
	/// Vanilla refuses an automation bridge end on any cell that already has a port of any kind.
	/// A bridge end may now land on any port cell: a machine's port, a gate's port, another
	/// bridge's end. The two things that made the last two unsafe are handled elsewhere: the
	/// one-link-per-cell table (BridgeLinks) and the LogicGate-layer displacement (BuildingDef.MarkArea
	/// prefix). What vanilla refuses for other reasons (footprint, terrain) is still refused by the
	/// callers of this check. A machine placed so its port lands on an existing bridge end passes
	/// the vanilla check on its own, since the bridge registers no port elements any more.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), "AreLogicPortsInValidPositions")]
	public static class BuildingDef_AreLogicPortsInValidPositions_Patch
	{
		public static bool Prepare() => BridgeKind.Logic.Enabled;

		public static bool Prefix(BuildingDef __instance, GameObject source_go, ref string fail_reason, ref bool __result)
		{
			if (__instance.BuildLocationRule != BuildLocationRule.LogicBridge || source_go == null || source_go.GetComponent<LogicUtilityNetworkLink>() == null)
				return true;
			fail_reason = null;
			__result = true;
			return false;
		}
	}

	/// <summary>
	/// Wires and nodes meet here. A real wire spawning on a node's cell takes over: the node steps
	/// aside first (prefix), so the wire's SetConnections is the last word on that cell. After any
	/// wire's spawn, connection change or removal, the nodes next to it recompute their bits
	/// (postfix), which is what makes a wire dragged up to a bridge end draw into it. When a wire
	/// is removed from under a bridge end, the bridge is asked to put a node back.
	/// </summary>
	internal static class WireKinds
	{
		/// <summary>The enabled kind whose wires the visualizer draws; null for pipes, rails and disabled kinds.</summary>
		public static BridgeKind Of(KAnimGraphTileVisualizer visualizer)
		{
			BridgeKind kind = BridgeKind.Of(visualizer.connectionSource);
			return kind != null && kind.Enabled ? kind : null;
		}
	}

	[HarmonyPatch(typeof(KAnimGraphTileVisualizer), "OnSpawn")]
	public static class KAnimGraphTileVisualizer_OnSpawn_Patch
	{
		public static void Prefix(KAnimGraphTileVisualizer __instance)
		{
			BridgeKind kind = WireKinds.Of(__instance);
			if (kind != null && __instance.isPhysicalBuilding && !(__instance is BridgeEndNode))
				BridgeEndNode.Evict(kind, Grid.PosToCell(__instance.transform.GetPosition()));
		}

		public static void Postfix(KAnimGraphTileVisualizer __instance)
		{
			BridgeKind kind = WireKinds.Of(__instance);
			if (kind != null)
				BridgeEndNode.RefreshNeighbours(kind, Grid.PosToCell(__instance.transform.GetPosition()));
		}
	}

	[HarmonyPatch(typeof(KAnimGraphTileVisualizer), nameof(KAnimGraphTileVisualizer.UpdateConnections))]
	public static class KAnimGraphTileVisualizer_UpdateConnections_Patch
	{
		public static void Postfix(KAnimGraphTileVisualizer __instance)
		{
			BridgeKind kind = WireKinds.Of(__instance);
			if (kind == null)
				return;
			int cell = Grid.PosToCell(__instance.transform.GetPosition());
			BridgeEndNode.RefreshNeighbours(kind, cell);
			// The disconnect tool skips objects without a Building, so this is for other callers.
			if (__instance is BridgeEndNode node)
				node.RefreshFromNeighbours();
		}
	}

	[HarmonyPatch(typeof(KAnimGraphTileVisualizer), "OnCleanUp")]
	public static class KAnimGraphTileVisualizer_OnCleanUp_Patch
	{
		public static void Postfix(KAnimGraphTileVisualizer __instance)
		{
			BridgeKind kind = WireKinds.Of(__instance);
			if (kind == null || __instance is BridgeEndNode || Game.IsQuitting())
				return;
			int cell = Grid.PosToCell(__instance.transform.GetPosition());
			BridgeEndNode.RefreshNeighbours(kind, cell);
			// Every complete bridge with an end here puts (or shares) a node back; a bridge whose
			// middle is here ignores the call. The node inherits the wire's connections: the
			// bridge was conducting through them, and a wire that melted or broke must not take
			// the bridge's links with it.
			List<JumperBridge> bridges = JumperBridge.At(kind, cell);
			// Only an end takes a node; a bridge whose middle is here (a wire bridge's, a joint
			// plate's centre) must not leave the wire's connections for a later node.
			bool endHere = false;
			foreach (JumperBridge bridge in bridges)
				endHere |= bridge.GetComponent<BridgeFootprint>().IsEnd(cell);
			if (endHere && __instance.isPhysicalBuilding)
				JumperBridge.Inherit(kind, cell, __instance.Connections);
			foreach (JumperBridge bridge in bridges)
				bridge.ScheduleCreate();
		}
	}

	/// <summary>
	/// A wire drag writes its path's display bits into every cell on the path, node cells included,
	/// but only calls UpdateConnections on buildings; after the path is built the nodes read the
	/// bits back. Covers a drag ending on a bridge end and one running through it.
	/// </summary>
	[HarmonyPatch(typeof(BaseUtilityBuildTool), "BuildPath")]
	public static class BaseUtilityBuildTool_BuildPath_Patch
	{
		private static readonly FieldInfo ConduitMgr = AccessTools.Field(typeof(BaseUtilityBuildTool), "conduitMgr");

		public static void Postfix(BaseUtilityBuildTool __instance)
		{
			object manager = ConduitMgr.GetValue(__instance);
			foreach (BridgeKind kind in BridgeKind.All)
				if (kind.Enabled && ReferenceEquals(manager, kind.Manager))
					BridgeEndNode.RefreshAll(kind);
		}
	}

	/// <summary>
	/// The disconnect tool's region pass (DisconnectTool.RunOnRegion) skips objects without a
	/// Building, so it never visits a node; its cut of a wire next to a node already reaches the
	/// node through the UpdateConnections postfix, but a node next to a node is untouched. These
	/// two postfixes run the tool's own region rule (GetRegularizedPos and IsInsideRegion, a few
	/// lines each, replicated here) over node cells of the kinds the tool's filter includes: on
	/// completion they cut the node's edges that lie inside the box, and while dragging they add
	/// the tool's pooled cut markers for those edges (CreateVisualizer dedups by cell pair, so
	/// re-adding on every move is harmless).
	/// </summary>
	internal static class DisconnectRegion
	{
		public static readonly FieldInfo SingleMode = AccessTools.Field(typeof(DisconnectTool), "singleDisconnectMode");
		public static readonly FieldInfo DownPos = AccessTools.Field(typeof(DragTool), "downPos");
		public static readonly System.Func<DragTool, Vector3, Vector3> SnapToLine =
			AccessTools.MethodDelegate<System.Func<DragTool, Vector3, Vector3>>(AccessTools.Method(typeof(DragTool), "SnapToLine"));
		public static readonly System.Func<DragTool, Vector3, WorldContainer, Vector3> ClampPositionToWorld =
			AccessTools.MethodDelegate<System.Func<DragTool, Vector3, WorldContainer, Vector3>>(AccessTools.Method(typeof(DragTool), "ClampPositionToWorld"));
		public static readonly System.Action<DisconnectTool, int, int, bool> CreateVisualizer =
			AccessTools.MethodDelegate<System.Action<DisconnectTool, int, int, bool>>(AccessTools.Method(typeof(DisconnectTool), "CreateVisualizer"));

		/// <summary>InterfaceTool.GetRegularizedPos on both corners, then the tool's int truncation.</summary>
		public static void Bounds(Vector3 a, Vector3 b, out Vector2I min, out Vector2I max)
		{
			Vector2 lo = Regularized(Vector2.Min(a, b), minimize: true);
			Vector2 hi = Regularized(Vector2.Max(a, b), minimize: false);
			min = new Vector2I((int)lo.x, (int)lo.y);
			max = new Vector2I((int)hi.x, (int)hi.y);
		}

		private static Vector2 Regularized(Vector2 input, bool minimize)
		{
			Vector3 half = new Vector3(Grid.HalfCellSizeInMeters, Grid.HalfCellSizeInMeters, 0f);
			return Grid.CellToPosCCC(Grid.PosToCell(input), Grid.SceneLayer.Background) + (minimize ? -half : half);
		}

		/// <summary>DisconnectTool.IsInsideRegion: the cell at the offset is inside the half-open box.</summary>
		public static bool Inside(Vector2I min, Vector2I max, int cell, int xoff, int yoff)
		{
			Grid.CellToXY(Grid.OffsetCell(cell, xoff, yoff), out int x, out int y);
			return x >= min.x && x < max.x && y >= min.y && y < max.y;
		}

		/// <summary>The node's edges the tool would cut: those whose far cell is also inside the box.</summary>
		public static UtilityConnections Removable(Vector2I min, Vector2I max, BridgeEndNode node)
		{
			int cell = node.Cell;
			UtilityConnections connections = node.Kind.Manager.GetConnections(cell, is_physical_building: false);
			UtilityConnections result = 0;
			if ((connections & UtilityConnections.Left) != 0 && Inside(min, max, cell, -1, 0))
				result |= UtilityConnections.Left;
			if ((connections & UtilityConnections.Right) != 0 && Inside(min, max, cell, 1, 0))
				result |= UtilityConnections.Right;
			if ((connections & UtilityConnections.Up) != 0 && Inside(min, max, cell, 0, 1))
				result |= UtilityConnections.Up;
			if ((connections & UtilityConnections.Down) != 0 && Inside(min, max, cell, 0, -1))
				result |= UtilityConnections.Down;
			return result;
		}

		public static List<BridgeEndNode> NodesInside(DisconnectTool tool, Vector2I min, Vector2I max)
		{
			List<BridgeEndNode> result = new List<BridgeEndNode>();
			foreach (BridgeKind kind in BridgeKind.All)
			{
				if (BridgeEndNode.Count(kind) == 0 || !tool.IsActiveLayer(kind.FilterLayer))
					continue;
				foreach (BridgeEndNode node in BridgeEndNode.All(kind))
					if (node != null && Inside(min, max, node.Cell, 0, 0) && Grid.IsVisible(node.Cell))
						result.Add(node);
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(DisconnectTool), "OnDragComplete")]
	public static class DisconnectTool_OnDragComplete_Patch
	{
		public static void Postfix(DisconnectTool __instance, Vector3 downPos, Vector3 upPos)
		{
			// The tool snaps its own copy of upPos in line mode; snapping again is idempotent.
			if ((bool)DisconnectRegion.SingleMode.GetValue(__instance))
				upPos = DisconnectRegion.SnapToLine(__instance, upPos);
			DisconnectRegion.Bounds(downPos, upPos, out Vector2I min, out Vector2I max);
			foreach (BridgeEndNode node in DisconnectRegion.NodesInside(__instance, min, max))
				node.Cut(DisconnectRegion.Removable(min, max, node));
		}
	}

	[HarmonyPatch(typeof(DisconnectTool), nameof(DisconnectTool.OnMouseMove))]
	public static class DisconnectTool_OnMouseMove_Patch
	{
		public static void Postfix(DisconnectTool __instance, Vector3 cursorPos)
		{
			if (!__instance.Dragging)
				return;
			cursorPos = DisconnectRegion.ClampPositionToWorld(__instance, cursorPos, ClusterManager.Instance.activeWorld);
			if ((bool)DisconnectRegion.SingleMode.GetValue(__instance))
				cursorPos = DisconnectRegion.SnapToLine(__instance, cursorPos);
			Vector3 downPos = (Vector3)DisconnectRegion.DownPos.GetValue(__instance);
			DisconnectRegion.Bounds(downPos, cursorPos, out Vector2I min, out Vector2I max);
			// The tool draws a marker per Down and Right edge of each visited cell; a node's Up and
			// Left edges are drawn by the cell on the other side (a wire by the tool, a node here).
			foreach (BridgeEndNode node in DisconnectRegion.NodesInside(__instance, min, max))
			{
				UtilityConnections removable = DisconnectRegion.Removable(min, max, node);
				if ((removable & UtilityConnections.Down) != 0)
					DisconnectRegion.CreateVisualizer(__instance, node.Cell, Grid.CellBelow(node.Cell), true);
				if ((removable & UtilityConnections.Right) != 0)
					DisconnectRegion.CreateVisualizer(__instance, node.Cell, Grid.CellRight(node.Cell), false);
			}
		}
	}

	/// <summary>
	/// The automation overlay collects wires by SaveLoadRoot and prefab tag, lifts them onto its
	/// masked camera layer and tints them by their network every frame. Nodes are not in that
	/// collection, so they are coloured here with the overlay's own colours and highlight. The
	/// two private fields are read by FieldInfo (two reads per frame while the overlay is open;
	/// a FieldRef's dynamic method cannot be built on the CoreCLR host that ModCheck runs on).
	/// </summary>
	[HarmonyPatch(typeof(OverlayModes.Logic), nameof(OverlayModes.Logic.Update))]
	public static class OverlayModesLogic_Update_Patch
	{
		private static readonly FieldInfo ConduitTargetLayer = AccessTools.Field(typeof(OverlayModes.Logic), "conduitTargetLayer");
		private static readonly FieldInfo ConnectedNetworks = AccessTools.Field(typeof(OverlayModes.Logic), "connectedNetworks");

		public static void Postfix(OverlayModes.Logic __instance)
		{
			if (BridgeEndNode.Count(BridgeKind.Logic) == 0)
				return;
			int layer = (int)ConduitTargetLayer.GetValue(__instance);
			HashSet<UtilityNetwork> highlighted = (HashSet<UtilityNetwork>)ConnectedNetworks.GetValue(__instance);
			Color32 on = GlobalAssets.Instance.colorSet.logicOn;
			Color32 off = GlobalAssets.Instance.colorSet.logicOff;
			off.a = on.a = 0; // as the overlay sets them for wires
			float scale = highlighted.Count > 0 ? OverlayModes.ModeUtil.GetHighlightScale() : 1f;
			foreach (BridgeEndNode node in BridgeEndNode.All(BridgeKind.Logic))
				if (node != null)
					node.ApplyLogicOverlay(layer, on, off, highlighted, scale);
		}
	}

	[HarmonyPatch(typeof(OverlayModes.Logic), nameof(OverlayModes.Logic.Disable))]
	public static class OverlayModesLogic_Disable_Patch
	{
		public static void Postfix()
		{
			foreach (BridgeEndNode node in BridgeEndNode.All(BridgeKind.Logic))
				if (node != null)
					node.ResetOverlay();
		}
	}

	/// <summary>
	/// The power overlay does the same for wires through IBridgedNetworkItem: each wire is tinted
	/// by the state of the circuit at its cell (unpowered, safe, straining, overloading), and
	/// brightened when the hovered bridge's networks include it. Nodes get the same colour from
	/// the same circuit figures, read from the overlay's own fields.
	/// </summary>
	[HarmonyPatch(typeof(OverlayModes.Power), nameof(OverlayModes.Power.Update))]
	public static class OverlayModesPower_Update_Patch
	{
		private static readonly FieldInfo TargetLayer = AccessTools.Field(typeof(OverlayModes.Power), "targetLayer");
		private static readonly FieldInfo ConnectedNetworks = AccessTools.Field(typeof(OverlayModes.Power), "connectedNetworks");
		private static readonly FieldInfo PreviousTilePower = AccessTools.Field(typeof(OverlayModes.Power), "previousTilePower");

		public static void Postfix(OverlayModes.Power __instance)
		{
			if (BridgeEndNode.Count(BridgeKind.Power) == 0)
				return;
			int layer = (int)TargetLayer.GetValue(__instance);
			HashSet<UtilityNetwork> highlighted = (HashSet<UtilityNetwork>)ConnectedNetworks.GetValue(__instance);
			float[] previous = (float[])PreviousTilePower.GetValue(__instance);
			float scale = highlighted.Count > 0 ? OverlayModes.ModeUtil.GetHighlightScale() : 1f;
			CircuitManager circuits = Game.Instance.circuitManager;
			ColorSet colours = GlobalAssets.Instance.colorSet;
			foreach (BridgeEndNode node in BridgeEndNode.All(BridgeKind.Power))
			{
				if (node == null)
					continue;
				UtilityNetwork network = Game.Instance.electricalConduitSystem.GetNetworkForCell(node.Cell);
				ushort circuit = network != null ? (ushort)network.id : ushort.MaxValue;
				float used = circuits.GetWattsUsedByCircuit(circuit);
				if (used == -1f)
					used = previous[node.Cell];
				float safe = circuits.GetMaxSafeWattageForCircuit(circuit) + TUNING.POWER.FLOAT_FUDGE_FACTOR;
				float needed = circuits.GetWattsNeededWhenActive(circuit);
				Color32 tint = used <= 0f ? colours.powerCircuitUnpowered
					: used > safe ? colours.powerCircuitOverloading
					: needed > safe && safe > 0f && used / safe >= 0.75f ? colours.powerCircuitStraining
					: colours.powerCircuitSafe;
				if (network != null && highlighted.Contains(network))
					tint = BridgeEndNode.Scaled(tint, scale);
				node.ApplyPowerOverlay(layer, tint);
			}
		}
	}

	[HarmonyPatch(typeof(OverlayModes.Power), nameof(OverlayModes.Power.Disable))]
	public static class OverlayModesPower_Disable_Patch
	{
		public static void Postfix()
		{
			foreach (BridgeEndNode node in BridgeEndNode.All(BridgeKind.Power))
				if (node != null)
					node.ResetOverlay();
		}
	}
}
