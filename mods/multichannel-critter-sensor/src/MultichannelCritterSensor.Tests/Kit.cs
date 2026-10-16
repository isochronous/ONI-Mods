using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MultichannelCritterSensor.Tests
{
	/// <summary>Set-up shared by the sensor tests.</summary>
	internal static class Kit
	{
		public static readonly Tag Hatch = new Tag("Hatch");
		public static readonly Tag Puft = new Tag("Puft");
		public static readonly Tag HatchEgg = new Tag("HatchEgg");
		public static readonly Tag PuftEgg = new Tag("PuftEgg");

		/// <summary>
		/// A ring of tiles, 12 wide and 6 high, which the game turns into a room of 10 by 4. Returns
		/// the sensor built inside it, at the interior's (1, 0) cell; <paramref name="r"/> gives the cells.
		/// </summary>
		public static IEnumerator RoomWithSensor(TestContext t, System.Action<CellRect, MultichannelCritterSensor> done)
		{
			CellRect r = t.Rect(12, 6);
			for (int x = 0; x < 12; x++)
				for (int y = 0; y < 6; y++)
					if (x == 0 || y == 0 || x == 11 || y == 5)
						t.World.Build("Tile", r.Cell(x, y));
			yield return t.WaitGameSeconds(2);
			GameObject go = t.World.Build("MultichannelCritterSensor", r.Cell(2, 1));
			var sensor = go.GetComponent<MultichannelCritterSensor>();
			yield return t.WaitUntil(() => sensor.InRoom, 10, "the sensor in a room");
			done(r, sensor);
		}

		/// <summary>A creature or egg standing on the room's floor.</summary>
		public static GameObject Spawn(string prefabId, int cell)
		{
			GameObject go = GameUtil.KInstantiate(Assets.GetPrefab(prefabId), Grid.CellToPosCBC(cell, Grid.SceneLayer.Creatures), Grid.SceneLayer.Creatures);
			go.SetActive(true);
			return go;
		}

		/// <summary>The value the sensor puts on its ribbon port.</summary>
		public static int Port(MultichannelCritterSensor sensor) => sensor.GetComponent<LogicPorts>().GetOutputValue(MultichannelCritterSensor.PortId);

		/// <summary>The animation a channel's indicator is playing, or its absence.</summary>
		public static string MeterAnim(MultichannelCritterSensor sensor, string field)
		{
			var meter = Traverse.Create(sensor).Field(field).GetValue<MeterController>();
			return meter?.meterController?.CurrentAnim?.name ?? "none";
		}

		/// <summary>
		/// The bits the port shows over the next seconds, frame by frame (a pulse lasts one logic tick).
		/// <paramref name="each"/> runs every frame with the seconds gone so far.
		/// </summary>
		public static IEnumerator Watch(TestContext t, MultichannelCritterSensor sensor, double seconds, System.Action<double> each, System.Action<int, int> done)
		{
			int seen = 0;
			int risingEdges = 0;
			int last = 0;
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - start < seconds)
			{
				each?.Invoke(Time.realtimeSinceStartup - start);
				int value = Port(sensor);
				seen |= value;
				if ((value & 4) != 0 && (last & 4) == 0)
					risingEdges++;
				last = value;
				yield return t.Frames(1);
			}
			done(seen, risingEdges);
		}

		public static void Click(GameObject go)
		{
			ExecuteEvents.Execute(go, new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
		}


		/// <summary>The full press of a button that acts on pointer down and up (the threshold editor's incrementors).</summary>
		public static void Press(GameObject go)
		{
			var data = new PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = PointerEventData.InputButton.Left };
			ExecuteEvents.Execute(go, data, ExecuteEvents.pointerDownHandler);
			ExecuteEvents.Execute(go, data, ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(go, data, ExecuteEvents.pointerClickHandler);
		}

		public static IEnumerator Show(TestContext t, GameObject building, System.Action<MultichannelCritterSensorSideScreen> done)
		{
			SelectTool.Instance.Select(building.GetComponent<KSelectable>());
			yield return t.WaitGameSeconds(1.5);
			MultichannelCritterSensorSideScreen screen = null;
			foreach (DetailsScreen.SideScreenRef sr in Traverse.Create(DetailsScreen.Instance).Field("sideScreens").GetValue<List<DetailsScreen.SideScreenRef>>())
				if (sr.screenInstance is MultichannelCritterSensorSideScreen s && s.gameObject.activeInHierarchy)
					screen = s;
			t.Assert.NotNull(screen, "the sensor's side screen is showing");
			done(screen);
		}

		/// <summary>The first object with the name and component type under the screen (the screen has several of each).</summary>
		public static T Find<T>(Component under, string name, string parentName = null) where T : Component
		{
			foreach (T c in under.GetComponentsInChildren<T>(true))
				if (c.name == name && (parentName == null || c.transform.parent.name == parentName))
					return c;
			return null;
		}
	}
}
