using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace DetectAnyElement.Tests
{
	/// <summary>Shared set-up for the sensor tests.</summary>
	internal static class Sensors
	{
		/// <summary>A sensor is switched on (the output the logic wire carries).</summary>
		public static bool IsOn(GameObject sensor) => sensor.GetComponent<Switch>().IsSwitchedOn;

		/// <summary>The Any choice on a sensor, the way the side screen sets it.</summary>
		public static void PickAny(GameObject sensor) => sensor.GetComponent<Filterable>().SelectedTag = AnyElement.Tag;

		public static void Pick(GameObject sensor, SimHashes element) => sensor.GetComponent<Filterable>().SelectedTag = element.CreateTag();

		/// <summary>
		/// A sensor in the middle cell of a ring of tiles, so the gas or liquid put in the cell stays
		/// there for the test. Returns the sensor; <paramref name="cell"/> is its cell.
		/// </summary>
		public static GameObject InChamber(TestContext t, CellRect r, string prefabId, out int cell)
		{
			cell = r.Cell(2, 2);
			for (int y = 1; y <= 3; y++)
				for (int x = 1; x <= 3; x++)
					if (x != 2 || y != 2)
						t.World.Build("Tile", r.Cell(x, y));
			GameObject sensor = t.World.Build(prefabId, cell);
			Empty(cell);
			return sensor;
		}

		/// <summary>Takes whatever gas or liquid is in the cell out; replacing a gas with vacuum leaves a gas of no mass.</summary>
		public static void Empty(int cell)
		{
			if (Grid.Mass[cell] > 0f && !Grid.Element[cell].IsVacuum)
				SimMessages.ConsumeMass(cell, Grid.Element[cell].id, Grid.Mass[cell] + 1f, 255);
		}

		public static string Describe(int cell)
		{
			return Grid.Element[cell].id + " " + Grid.Mass[cell].ToString("F2") + " kg";
		}

		public static void Fill(int cell, SimHashes element, float kg, float kelvin = 293.15f)
		{
			SimMessages.ReplaceElement(cell, element, CellEventLogger.Instance.SandBoxTool, kg, kelvin);
		}

		/// <summary>
		/// The in-world sensors sample every 200 ms and decide after eight samples; three game
		/// seconds always cover a full window.
		/// </summary>
		public static Wait Settle(TestContext t) => t.WaitGameSeconds(4.5);

		public static Wait Ready(TestContext t) => t.WaitGameSeconds(1);

		/// <summary>
		/// Ready, for a sensor in a chamber: the chamber's tiles turn solid in the sim a moment after
		/// they are built, so liquid or gas left in the area can still flow into the sensor's cell
		/// before then. Once the chamber is sealed the cell is emptied again.
		/// </summary>
		public static System.Collections.IEnumerator Ready(TestContext t, int cell)
		{
			yield return t.WaitGameSeconds(1);
			Empty(cell);
			yield return t.WaitGameSeconds(0.5);
		}

		/// <summary>The conduit flow updates its sensors about once a second.</summary>
		public static Wait Flow(TestContext t) => t.WaitGameSeconds(2.5);

		public static string PortSignal(GameObject sensor)
		{
			return sensor.GetComponent<LogicPorts>().GetOutputValue(LogicSwitch.PORT_ID).ToString();
		}
	}
}
