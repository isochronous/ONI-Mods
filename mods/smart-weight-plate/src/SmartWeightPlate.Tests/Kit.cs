using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartWeightPlate.Tests
{
	/// <summary>Set-up shared by the plate tests.</summary>
	internal static class Kit
	{
		/// <summary>A plate at the cell with the thresholds the way the two sliders set them.</summary>
		public static SmartWeightPlate Plate(TestContext t, int cell, int low, int high)
		{
			GameObject go = t.World.Build(SmartWeightPlateConfig.ID, cell);
			var plate = go.GetComponent<SmartWeightPlate>();
			plate.ActivateValue = high;
			plate.DeactivateValue = low;
			return plate;
		}

		/// <summary>The value the plate has put on its output port.</summary>
		public static int Port(SmartWeightPlate plate) => plate.GetComponent<LogicPorts>().GetOutputValue(SmartWeightPlate.PortId);

		/// <summary>The animation the plate is playing.</summary>
		public static string Anim(SmartWeightPlate plate) => plate.GetComponent<KBatchedAnimController>().CurrentAnim?.name ?? "none";

		/// <summary>The plate senses every 200 ms; a second always covers a scan and the signal that follows.</summary>
		public static Wait Settle(TestContext t) => t.WaitGameSeconds(1);

		public static void Click(GameObject go)
		{
			ExecuteEvents.Execute(go, new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
		}

		/// <summary>Selects the building and returns the side screens showing for it.</summary>
		public static IEnumerator Show(TestContext t, GameObject building, System.Action<List<SideScreenContent>> done)
		{
			SelectTool.Instance.Select(building.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
			var showing = new List<SideScreenContent>();
			foreach (DetailsScreen.SideScreenRef sr in Traverse.Create(DetailsScreen.Instance).Field("sideScreens").GetValue<List<DetailsScreen.SideScreenRef>>())
				if (sr.screenInstance != null && sr.screenInstance.gameObject.activeInHierarchy)
					showing.Add(sr.screenInstance);
			done(showing);
		}
	}
}
