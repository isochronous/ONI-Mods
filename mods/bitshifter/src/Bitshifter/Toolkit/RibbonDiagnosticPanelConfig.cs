using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;
using PREFAB = Bitshifter.ModStrings.BUILDINGS.PREFABS.RIBBONDIAGNOSTICPANEL;

namespace Bitshifter
{
	/// <summary>
	/// Building definition: a 2x2 panel with its own kanim (anim/assets/diagnostic_panel, drawn by
	/// 3GuB), a ribbon input at the top left and a ribbon output at the bottom left. The other
	/// values are the vanilla Ribbon Writer's (LogicRibbonWriterConfig), including its LogicGate
	/// layer, so the panel can sit in front of other buildings. It does not rotate: the digits
	/// on its face would turn with it.
	/// </summary>
	public sealed class RibbonDiagnosticPanelConfig : IBuildingConfig
	{
		public const string ID = "RibbonDiagnosticPanel";

		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 2, 2, "diagnostic_panel_kanim", 30, 30f,
				TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere,
				noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
			def.Overheatable = false;
			def.Floodable = false;
			def.Entombable = false;
			def.PermittedRotations = PermittedRotations.Unrotatable;
			def.ViewMode = OverlayModes.Logic.ID;
			def.AudioCategory = "Metal";
			def.ObjectLayer = ObjectLayer.LogicGate;
			def.SceneLayer = Grid.SceneLayer.LogicGates;
			def.AlwaysOperational = true;
			// Either port alone is a complete use (a monitor, a switch), so neither shows the missing-wire icon.
			def.LogicInputPorts = new List<LogicPorts.Port>
			{
				LogicPorts.Port.RibbonInputPort(RibbonDiagnosticPanel.InputPort, new CellOffset(0, 1),
					PREFAB.LOGIC_PORT, PREFAB.LOGIC_PORT_ACTIVE, PREFAB.LOGIC_PORT_INACTIVE),
			};
			def.LogicOutputPorts = new List<LogicPorts.Port>
			{
				LogicPorts.Port.RibbonOutputPort(RibbonDiagnosticPanel.OutputPort, new CellOffset(0, 0),
					PREFAB.LOGIC_PORT_OUTPUT, PREFAB.OUTPUT_PORT_ACTIVE, PREFAB.OUTPUT_PORT_INACTIVE),
			};
			def.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
			GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
			return def;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGet<RibbonDiagnosticPanel>();
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits);
		}
	}
}
