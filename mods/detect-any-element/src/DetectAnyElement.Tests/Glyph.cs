using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace DetectAnyElement.Tests
{
	/// <summary>The asterisk a sensor on Any flashes in place of the drop, gust or lump.</summary>
	public static class Glyph
	{
		private static readonly HashedString Elements = "elements";
		private const string AsteriskKanim = "detect_any_asterisk_kanim";

		private static readonly string[] SensorPrefabs =
		{
			"LiquidConduitElementSensor", "GasConduitElementSensor", "SolidConduitElementSensor",
			"LogicElementSensorGas", "LogicElementSensorLiquid",
		};

		private static KAnim.Build.Symbol Override(GameObject sensor)
		{
			foreach (SymbolOverrideController.SymbolEntry entry in sensor.GetComponent<SymbolOverrideController>().GetSymbolOverrides)
				if (entry.targetSymbol == Elements && entry.priority == 0)
					return entry.sourceSymbol;
			return null;
		}

		[OniTest("DAE: the asterisk art has a symbol for every sensor's own animation", Timeout = 20), Layer(0)]
		public static IEnumerator AsteriskHasEverySensorSymbol(TestContext t)
		{
			KAnimFile asterisk = Assets.GetAnim(AsteriskKanim);
			t.Assert.NotNull(asterisk, AsteriskKanim + " loaded");
			foreach (string id in SensorPrefabs)
			{
				GameObject prefab = Assets.GetPrefab(id);
				t.Assert.NotNull(prefab, id + " prefab exists");
				t.Assert.NotNull(prefab.GetComponent<AnyGlyph>(), id + " carries the glyph component");
				t.Assert.NotNull(prefab.GetComponent<SymbolOverrideController>(), id + " carries a symbol override controller");
				string kanim = prefab.GetComponent<KBatchedAnimController>().AnimFiles[0].name;
				string symbol = "asterisk_" + (kanim.EndsWith("_kanim") ? kanim.Substring(0, kanim.Length - 6) : kanim);
				t.Assert.NotNull(asterisk.GetData().build.GetSymbol(symbol), id + " draws with " + kanim + " and the art has " + symbol);
			}
			yield break;
		}

		[OniTest("DAE: a sensor on Any swaps its icon symbol for the asterisk, and an element puts it back", Timeout = 60), Layer(0)]
		public static IEnumerator SymbolSwaps(TestContext t)
		{
			CellRect r = t.Rect(10, 1);
			var sensors = new List<GameObject>();
			for (int i = 0; i < SensorPrefabs.Length; i++)
			{
				int cell = r.Cell(i * 2, 0);
				if (SensorPrefabs[i].Contains("Conduit"))
					t.World.Build(SensorPrefabs[i].StartsWith("Liquid") ? "LiquidConduit" : SensorPrefabs[i].StartsWith("Gas") ? "GasConduit" : "SolidConduit", cell);
				sensors.Add(t.World.Build(SensorPrefabs[i], cell));
				yield return t.WaitGameSeconds(0.3);
			}
			yield return t.WaitGameSeconds(1);
			KAnimFile asterisk = Assets.GetAnim(AsteriskKanim);
			for (int i = 0; i < sensors.Count; i++)
			{
				GameObject sensor = sensors[i];
				string id = SensorPrefabs[i];
				t.Assert.Null(Override(sensor), id + ": no override while on None");
				Sensors.PickAny(sensor);
				yield return t.WaitGameSeconds(0.5);
				KAnim.Build.Symbol swapped = Override(sensor);
				t.Assert.NotNull(swapped, id + ": the icon symbol is overridden on Any");
				string kanim = sensor.GetComponent<KBatchedAnimController>().AnimFiles[0].name;
				KAnim.Build.Symbol expected = asterisk.GetData().build.GetSymbol("asterisk_" + kanim.Replace("_kanim", ""));
				t.Assert.Eq(expected.hash.HashValue, swapped.hash.HashValue, id + ": it is this sensor's own asterisk");
				SimHashes element = id.Contains("Solid") ? SimHashes.Cuprite : id.Contains("Gas") ? SimHashes.Oxygen : SimHashes.Water;
				Sensors.Pick(sensor, element);
				yield return t.WaitGameSeconds(0.5);
				t.Assert.Null(Override(sensor), id + ": an element puts the vanilla icon back");
			}
		}

		[OniTest("DAE: screenshots of each sensor on Any and on an element", Timeout = 60, Explicit = true), Layer(0)]
		public static IEnumerator Screenshots(TestContext t)
		{
			t.Speed(1);
			CellRect r = t.Rect(10, 3);
			int row = 1;
			GameObject gas = t.World.Build("LogicElementSensorGas", r.Cell(1, row));
			GameObject liquid = t.World.Build("LogicElementSensorLiquid", r.Cell(3, row));
			Sensors.Fill(r.Cell(1, row), SimHashes.Oxygen, 1f);
			Sensors.Fill(r.Cell(3, row), SimHashes.Water, 100f);
			GameObject liquidPipe = t.World.Build("LiquidConduit", r.Cell(5, row));
			GameObject pipeSensor = t.World.Build("LiquidConduitElementSensor", r.Cell(5, row));
			Sensors.PickAny(gas);
			Sensors.PickAny(liquid);
			Sensors.PickAny(pipeSensor);
			Conduit.GetFlowManager(ConduitType.Liquid).AddElement(r.Cell(5, row), SimHashes.Water, 1f, 293.15f, byte.MaxValue, 0);
			yield return t.WaitGameSeconds(3);
			yield return t.Screenshot("any-on");
			Sensors.Pick(gas, SimHashes.Oxygen);
			Sensors.Pick(liquid, SimHashes.Water);
			Sensors.Pick(pipeSensor, SimHashes.Water);
			yield return t.WaitGameSeconds(3);
			yield return t.Screenshot("element-on");
		}
	}
}
