using System;
using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace NoMagicFridges.Tests
{
	/// <summary>
	/// Powered fridges, with real food in them and a real power circuit (a dev generator on a copper
	/// wire), in a sealed box at 20 C. Every number is read back from the food's own temperature, as
	/// a player would see it. Vanilla would drag any of these items to 1 C within seconds, which is
	/// what the expected values rule out.
	/// </summary>
	public static class Fridges
	{
		private const float Room = 293.15f;
		private const float Frozen = 253.15f;
		private const float Warm = 303.15f;

		/// <summary>The fridge's own setpoint, 1 C.</summary>
		private const float Setpoint = 274.15f;

		private sealed class Rig
		{
			public CellRect Inside;
			public int WireFrom, WireTo, Generator;
			public GameObject GeneratorObject;
		}

		/// <summary>A boxed room with a copper wire dragged with the real tool along its floor from x = 2 to <paramref name="toX"/>, nothing powering it yet.</summary>
		private static IEnumerator Box(TestContext t, Rig rig, int toX = 9)
		{
			rig.Inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			rig.Generator = rig.Inside.Cell(2, 0);
			var cells = new List<int>();
			for (int x = 2; x <= toX; x++)
				cells.Add(rig.Inside.Cell(x, 0));
			UtilityDragSession wire = t.Tools.UtilityTool("Wire", SimHashes.Copper);
			yield return wire.Path(cells.ToArray());
			wire.Dispose();
			yield return t.WaitGameSeconds(2);
		}

		/// <summary>Builds the dev generator at the wire's end and waits until every fridge is powered, by its energy consumer and by its running state.</summary>
		private static IEnumerator Power(TestContext t, Rig rig, params GameObject[] fridges)
		{
			rig.GeneratorObject = t.World.Build("DevGenerator", rig.Generator);
			yield return t.WaitUntil(() => Array.TrueForAll(fridges, f => f.GetComponent<EnergyConsumer>().IsPowered && f.GetComponent<Operational>().IsActive), 20, "the fridge(s) to be powered through the wire and running");
		}

		/// <summary>A food item of the prefab, mass and temperature, stored in the fridge.</summary>
		private static GameObject Food(string prefab, float kg, float kelvin, GameObject fridge)
		{
			Storage storage = fridge.GetComponent<Storage>();
			GameObject go = GameUtil.KInstantiate(Assets.GetPrefab(prefab), Grid.CellToPosCCC(Grid.PosToCell(fridge), Grid.SceneLayer.Ore), Grid.SceneLayer.Ore);
			PrimaryElement element = go.GetComponent<PrimaryElement>();
			element.Mass = kg;
			element.Temperature = kelvin;
			go.SetActive(true);
			storage.Store(go);
			return go;
		}

		private static float Temp(GameObject item) { return item.GetComponent<PrimaryElement>().Temperature; }

		private static float Capacity(GameObject item)
		{
			PrimaryElement element = item.GetComponent<PrimaryElement>();
			return element.Mass * element.Element.specificHeatCapacity;
		}

		private static void Near(TestContext t, float expected, float actual, float tolerance, string what)
		{
			t.Assert.True(Mathf.Abs(expected - actual) <= tolerance, what + ": expected " + expected.ToString("0.00") + " K +-" + tolerance + ", was " + actual.ToString("0.00") + " K");
		}

		[OniTest("NMF: every building on the vanilla fridge controller gets the thermostat, and nothing else does", Timeout = 30), Layer(0)]
		public static IEnumerator ThermostatOnEveryFridge(TestContext t)
		{
			var fridges = new List<string>();
			foreach (BuildingDef def in Assets.BuildingDefs)
			{
				GameObject complete = def.BuildingComplete;
				if (complete == null)
					continue;
				bool controller = complete.GetDef<RefrigeratorController.Def>() != null;
				bool thermostat = complete.GetComponent<FridgeThermostat>() != null;
				t.Assert.Eq(controller, thermostat, def.PrefabID + (controller ? " has a fridge controller" : " has no fridge controller") + " and so " + (controller ? "needs" : "must not have") + " the thermostat");
				if (controller)
					fridges.Add(def.PrefabID);
			}
			t.Log("fridges: " + string.Join(", ", fridges));
			t.Assert.True(fridges.Contains("Refrigerator") && fridges.Contains("MiniFridge"), "the Refrigerator and the Mini Fridge are both fridges");
			yield break;
		}

		[OniTest("NMF: food frozen to -20 C is not warmed when the fridge gets power", Timeout = 90), Layer(0)]
		public static IEnumerator FrozenFoodStaysFrozen(TestContext t)
		{
			t.Speed(3);
			var rig = new Rig();
			yield return Box(t, rig);
			GameObject fridge = t.World.Build("Refrigerator", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			GameObject food = Food("MushBar", 5f, Frozen, fridge);
			yield return t.WaitGameSeconds(3);
			float before = Temp(food);
			t.Assert.False(fridge.GetComponent<EnergyConsumer>().IsPowered, "the fridge has a wire but no source yet");
			Near(t, Frozen, before, 3f, "the food sits near -20 C in the unpowered fridge");

			yield return Power(t, rig, fridge);
			yield return t.WaitGameSeconds(25);
			float after = Temp(food);
			t.Log("frozen food: " + before.ToString("0.00") + " K before power, " + after.ToString("0.00") + " K after 25 s of power");
			t.Assert.True(after < before + 2f, "the frozen food stays frozen (was " + before.ToString("0.0") + " K, now " + after.ToString("0.0") + " K; a 1 C reservoir would have it near " + Setpoint + " K)");
		}

		[OniTest("NMF: a warm item and frozen stock settle at one temperature, with the heat conserved", Timeout = 90), Layer(0)]
		public static IEnumerator ContentsShareHeat(TestContext t)
		{
			t.Speed(3);
			var rig = new Rig();
			yield return Box(t, rig);
			GameObject fridge = t.World.Build("Refrigerator", rig.Inside.Cell(5, 0));
			yield return Power(t, rig, fridge);
			GameObject cold = Food("MushBar", 4f, Frozen, fridge);
			GameObject warm = Food("BasicPlantFood", 1f, Warm, fridge);
			float coldC = Capacity(cold), warmC = Capacity(warm);
			float mean = (coldC * Frozen + warmC * Warm) / (coldC + warmC);
			t.Assert.True(mean < Setpoint, "the mixture is colder than the setpoint, so the compressor stays off (" + mean.ToString("0.0") + " K)");
			yield return t.WaitGameSeconds(3);
			t.Assert.True(Temp(cold) > Frozen + 0.3f, "the frozen stock is warmed a little by the newcomer (" + Temp(cold).ToString("0.00") + " K)");
			t.Assert.True(Temp(warm) < Warm - 8f, "the newcomer is chilled fast (" + Temp(warm).ToString("0.00") + " K)");

			yield return t.WaitGameSeconds(22);
			t.Log("settled: cold " + Temp(cold).ToString("0.00") + " K, warm " + Temp(warm).ToString("0.00") + " K, energy-weighted mean " + mean.ToString("0.00") + " K");
			Near(t, mean, Temp(cold), 1.5f, "the frozen stock ends at the energy-weighted mean");
			Near(t, mean, Temp(warm), 1.5f, "the newcomer ends at the energy-weighted mean");
		}

		[OniTest("NMF: a Mini Fridge cools one warm item to its setpoint and no lower", Timeout = 90), Layer(0)]
		public static IEnumerator CompressorOnlyCools(TestContext t)
		{
			t.Speed(3);
			var rig = new Rig();
			yield return Box(t, rig);
			GameObject fridge = t.World.Build("MiniFridge", rig.Inside.Cell(5, 0));
			yield return Power(t, rig, fridge);
			float setpoint = fridge.GetDef<RefrigeratorController.Def>().simulatedInternalTemperature;
			GameObject food = Food("MushBar", 1f, Warm, fridge);
			float lowest = float.MaxValue;
			for (int second = 0; second < 20; second++)
			{
				yield return t.WaitGameSeconds(1);
				lowest = Mathf.Min(lowest, Temp(food));
			}
			float now = Temp(food);
			t.Log("warm item in the Mini Fridge: " + now.ToString("0.00") + " K after 20 s, lowest " + lowest.ToString("0.00") + " K, setpoint " + setpoint.ToString("0.00") + " K");
			t.Assert.True(now < setpoint + 1f, "the warm item has been cooled to the setpoint (" + now.ToString("0.00") + " K)");
			t.Assert.True(lowest > setpoint - 0.1f, "the compressor never takes it below the setpoint (lowest " + lowest.ToString("0.00") + " K)");
		}

		[OniTest("NMF: a crammed warm fridge cools far more slowly than one warm item, and a warm item stays open to the room", Timeout = 90), Layer(0)]
		public static IEnumerator ThermalBuffer(TestContext t)
		{
			t.Speed(3);
			var rig = new Rig();
			yield return Box(t, rig, 8);
			GameObject single = t.World.Build("Refrigerator", rig.Inside.Cell(5, 0));
			GameObject crammed = t.World.Build("Refrigerator", rig.Inside.Cell(8, 0));
			yield return Power(t, rig, single, crammed);
			GameObject one = Food("MushBar", 1f, Warm, single);
			GameObject lot = Food("BasicPlantFood", 99f, Warm, crammed);
			SimTemperatureTransfer lotTransfer = lot.GetComponent<SimTemperatureTransfer>();
			float area = lotTransfer.SurfaceArea;
			yield return t.WaitGameSeconds(20);
			float oneDrop = Warm - Temp(one);
			float lotDrop = Warm - Temp(lot);
			t.Log("20 s of cooling: one item dropped " + oneDrop.ToString("0.00") + " K, the crammed fridge's food " + lotDrop.ToString("0.00") + " K");
			t.Assert.True(oneDrop > 20f, "a lone warm item is chilled fast (dropped " + oneDrop.ToString("0.0") + " K)");
			t.Assert.True(lotDrop > 0.1f, "the crammed fridge does cool (dropped " + lotDrop.ToString("0.00") + " K)");
			t.Assert.True(lotDrop < 3f, "the crammed fridge cools slowly (dropped " + lotDrop.ToString("0.00") + " K)");
			t.Assert.True(Temp(lot) > Room, "the crammed food is still warmer than the room (" + Temp(lot).ToString("0.0") + " K)");
			Near(t, area, lotTransfer.SurfaceArea, 0.0001f, "food warmer than the room keeps its normal exchange with it");
		}

		[OniTest("NMF: a fridge with no wire or power is vanilla: nothing equalises and nothing is insulated (the powered counterparts are the other tests)", Timeout = 60), Layer(0)]
		public static IEnumerator UnpoweredIsVanilla(TestContext t)
		{
			t.Speed(3);
			var rig = new Rig();
			rig.Inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			GameObject fridge = t.World.Build("Refrigerator", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			t.Assert.False(fridge.GetComponent<EnergyConsumer>().IsPowered, "the fridge is not powered");
			GameObject cold = Food("MushBar", 1f, Frozen, fridge);
			GameObject warm = Food("BasicPlantFood", 1f, Warm, fridge);
			SimTemperatureTransfer coldTransfer = cold.GetComponent<SimTemperatureTransfer>();
			float area = coldTransfer.SurfaceArea, ground = coldTransfer.GroundTransferScale;
			yield return t.WaitGameSeconds(15);
			t.Assert.False(fridge.GetComponent<Operational>().IsActive, "the fridge has no power");
			float gap = Temp(warm) - Temp(cold);
			t.Log("unpowered: cold " + Temp(cold).ToString("0.0") + " K, warm " + Temp(warm).ToString("0.0") + " K");
			t.Assert.True(gap > 25f, "the items do not exchange heat with each other (still " + gap.ToString("0.0") + " K apart; the thermostat would have closed it)");
			Near(t, area, coldTransfer.SurfaceArea, 0.0001f, "the cold item's surface is untouched");
			Near(t, ground, coldTransfer.GroundTransferScale, 0.0001f, "the cold item's ground transfer is untouched");
		}

		[OniTest("NMF: a cold item is insulated from the room, and is released when it leaves, when power fails and re-insulated when power returns", Timeout = 120), Layer(0)]
		public static IEnumerator InsulationLifecycle(TestContext t)
		{
			t.Speed(3);
			var rig = new Rig();
			yield return Box(t, rig);
			GameObject fridge = t.World.Build("Refrigerator", rig.Inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			GameObject leaving = Food("MushBar", 1f, Frozen, fridge);
			GameObject staying = Food("BasicPlantFood", 1f, Frozen, fridge);
			SimTemperatureTransfer leavingT = leaving.GetComponent<SimTemperatureTransfer>();
			SimTemperatureTransfer stayingT = staying.GetComponent<SimTemperatureTransfer>();
			float leavingArea = leavingT.SurfaceArea, leavingGround = leavingT.GroundTransferScale;
			float stayingArea = stayingT.SurfaceArea, stayingGround = stayingT.GroundTransferScale;
			t.Assert.True(leavingArea > 0f && stayingArea > 0f, "the items have a surface to start with");

			yield return Power(t, rig, fridge);
			yield return t.WaitUntil(() => stayingT.SurfaceArea < stayingArea * 0.02f && leavingT.SurfaceArea < leavingArea * 0.02f, 20, "the cold items to be insulated");
			Near(t, stayingArea * 0.01f, stayingT.SurfaceArea, stayingArea * 0.001f, "insulated surface is one hundredth");
			Near(t, stayingGround * 0.01f, stayingT.GroundTransferScale, Mathf.Max(stayingGround, 0.01f) * 0.001f, "insulated ground transfer is one hundredth");

			// Taking one out gives it its normal exchange back at once.
			fridge.GetComponent<Storage>().Drop(leaving);
			yield return t.WaitGameSeconds(1);
			Near(t, leavingArea, leavingT.SurfaceArea, 0.0001f, "the item taken out has its surface back");
			Near(t, leavingGround, leavingT.GroundTransferScale, 0.0001f, "the item taken out has its ground transfer back");

			// Power fails: the item left inside is released.
			t.World.Destroy(rig.GeneratorObject);
			yield return t.WaitUntil(() => !fridge.GetComponent<Operational>().IsActive, 20, "the fridge to lose power");
			yield return t.WaitUntil(() => stayingT.SurfaceArea > stayingArea * 0.99f, 10, "the item to be released when power fails");
			Near(t, stayingGround, stayingT.GroundTransferScale, 0.0001f, "the released item has its ground transfer back");

			// Power returns: insulated again.
			yield return t.WaitGameSeconds(2);
			rig.GeneratorObject = t.World.Build("DevGenerator", rig.Generator);
			yield return t.WaitUntil(() => fridge.GetComponent<Operational>().IsActive, 20, "the fridge to run again");
			yield return t.WaitUntil(() => stayingT.SurfaceArea < stayingArea * 0.02f, 20, "the item to be insulated again");
		}

		[OniTest("NMF: a building that is not a fridge keeps the vanilla reservoir on its contents (the Compost heats its contents)", Timeout = 90), Layer(0)]
		public static IEnumerator OtherUsersOfTheAdjusterAreUntouched(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			GameObject compost = t.World.Build("Compost", inside.Cell(5, 0));
			t.Assert.Null(compost.GetComponent<FridgeThermostat>(), "the Compost has no thermostat");
			yield return t.WaitGameSeconds(1);
			// The Compost's reservoir is on while it runs; run it the way its state machine does.
			Operational operational = compost.GetComponent<Operational>();
			operational.SetActive(true);
			GameObject food = Food("MushBar", 1f, 273.15f, compost);
			float start = Temp(food);
			yield return t.WaitGameSeconds(15);
			float now = Temp(food);
			t.Log("item in the Compost: " + start.ToString("0.0") + " K to " + now.ToString("0.0") + " K");
			t.Assert.True(now > start + 15f, "the Compost's 50 C reservoir still heats its contents (" + start.ToString("0.0") + " K to " + now.ToString("0.0") + " K)");
		}
	}
}
