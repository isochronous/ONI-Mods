using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;
using PREFAB = Bitshifter.ModStrings.BUILDINGS.PREFABS.FANOUTGATE;

namespace Bitshifter
{
	/// <summary>
	/// Building definition: a 2x1 gate with its own kanim (anim/assets/fanout_gate, drawn by 3GuB),
	/// a 1-bit input on the left cell and a ribbon output on the right one, as the vanilla Ribbon
	/// Writer has them. The other values are the Ribbon Writer's (LogicRibbonWriterConfig),
	/// rotation included: nothing on the gate's face reads one way up.
	/// </summary>
	public sealed class FanoutGateConfig : IBuildingConfig
	{
		public const string ID = "FanoutGate";

		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 2, 1, "fanout_gate_kanim", 30, 30f,
				TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere,
				noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
			def.Overheatable = false;
			def.Floodable = false;
			def.Entombable = false;
			def.PermittedRotations = PermittedRotations.R360;
			def.ViewMode = OverlayModes.Logic.ID;
			def.AudioCategory = "Metal";
			def.ObjectLayer = ObjectLayer.LogicGate;
			def.SceneLayer = Grid.SceneLayer.LogicGates;
			def.AlwaysOperational = true;
			// The gate needs both ports, so both show the missing-wire icon, as the Ribbon Writer's do.
			def.LogicInputPorts = new List<LogicPorts.Port>
			{
				LogicPorts.Port.InputPort(FanoutGate.InputPort, new CellOffset(0, 0),
					PREFAB.LOGIC_PORT, PREFAB.LOGIC_PORT_ACTIVE, PREFAB.LOGIC_PORT_INACTIVE, show_wire_missing_icon: true),
			};
			def.LogicOutputPorts = new List<LogicPorts.Port>
			{
				LogicPorts.Port.RibbonOutputPort(FanoutGate.OutputPort, new CellOffset(1, 0),
					PREFAB.LOGIC_PORT_OUTPUT, PREFAB.OUTPUT_PORT_ACTIVE, PREFAB.OUTPUT_PORT_INACTIVE, show_wire_missing_icon: true),
			};
			def.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
			GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
			return def;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGet<FanoutGate>();
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits);
		}
	}
}
