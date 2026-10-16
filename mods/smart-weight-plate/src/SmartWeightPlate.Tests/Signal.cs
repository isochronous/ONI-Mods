using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace SmartWeightPlate.Tests
{
	/// <summary>The plate's signal: the latch between the two thresholds, the equal-threshold case, Invert, and the pressed look.</summary>
	public static class Signal
	{
		[OniTest("SWP: green from the high threshold until the weight drops to the low threshold, inclusive both ways", Timeout = 60), Layer(1)]
		public static IEnumerator Latch(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			int above = r.Cell(1, 1);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 20, 50);
			yield return Kit.Settle(t);
			t.Assert.Eq(0f, plate.CurrentValue, "nothing on the plate");
			t.Assert.False(plate.IsSignalOn, "red when empty");
			t.Assert.Eq(0, Kit.Port(plate), "and the port is low");

			GameObject iron = t.Dupes.Drop(SimHashes.Iron, 60f, above);
			yield return Kit.Settle(t);
			t.Assert.Eq(60f, plate.CurrentValue, "the pile is weighed");
			t.Assert.True(plate.IsSignalOn, "60 kg is past the high threshold of 50: green");
			t.Assert.Eq(1, Kit.Port(plate), "the port is high");

			// 25 kg: between the thresholds. A plate that was heavy stays heavy.
			GameObject copper = t.Dupes.Drop(SimHashes.Copper, 25f, above);
			yield return Kit.Settle(t);
			t.World.Destroy(iron);
			yield return Kit.Settle(t);
			t.Assert.Eq(25f, plate.CurrentValue, "25 kg left");
			t.Assert.True(plate.IsSignalOn, "between the thresholds a heavy plate stays green");

			// Exactly the low threshold: inclusive, red.
			GameObject gold = t.Dupes.Drop(SimHashes.Gold, 20f, above);
			yield return Kit.Settle(t);
			t.World.Destroy(copper);
			yield return Kit.Settle(t);
			t.Assert.Eq(20f, plate.CurrentValue, "20 kg left");
			t.Assert.False(plate.IsSignalOn, "at the low threshold: red");
			t.Assert.Eq(0, Kit.Port(plate), "the port is low");

			// Back between the thresholds: stays red until the high threshold.
			GameObject zinc = t.Dupes.Drop(SimHashes.Lead, 10f, above);
			yield return Kit.Settle(t);
			t.Assert.Eq(30f, plate.CurrentValue, "30 kg");
			t.Assert.False(plate.IsSignalOn, "between the thresholds a light plate stays red");

			// Exactly the high threshold: inclusive, green.
			GameObject nickel = t.Dupes.Drop(SimHashes.Nickel, 20f, above);
			yield return Kit.Settle(t);
			t.Assert.Eq(50f, plate.CurrentValue, "50 kg");
			t.Assert.True(plate.IsSignalOn, "at the high threshold: green");
			_ = gold; _ = zinc; _ = nickel;
		}

		[OniTest("SWP: equal thresholds make a plain at-or-above threshold", Timeout = 40), Layer(1)]
		public static IEnumerator EqualThresholds(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			int above = r.Cell(1, 1);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 30, 30);
			GameObject pile = t.Dupes.Drop(SimHashes.Iron, 30f, above);
			yield return Kit.Settle(t);
			t.Assert.True(plate.IsSignalOn, "exactly 30 kg is at the threshold: green");
			t.World.Destroy(pile);
			yield return Kit.Settle(t);
			pile = t.Dupes.Drop(SimHashes.Iron, 29f, above);
			yield return Kit.Settle(t);
			t.Assert.False(plate.IsSignalOn, "29 kg is below it: red, with no band to hold across");
			t.World.Destroy(pile);
			yield return Kit.Settle(t);
			pile = t.Dupes.Drop(SimHashes.Iron, 31f, above);
			yield return Kit.Settle(t);
			t.Assert.True(plate.IsSignalOn, "31 kg: green");
		}

		[OniTest("SWP: Invert swaps green and red, the port text and the light, and leaves the knobs alone", Timeout = 60), Layer(1)]
		public static IEnumerator InvertAndLook(TestContext t)
		{
			CellRect r = t.Rect(3, 3);
			SmartWeightPlate plate = Kit.Plate(t, r.Cell(1, 0), 20, 50);
			yield return Kit.Settle(t);
			t.Assert.Eq("off_up", Kit.Anim(plate), "empty, red: the light is off and the plate is up");
			LogicPorts ports = plate.GetComponent<LogicPorts>();
			string plain = ports.outputPortInfo[0].activeDescription;

			GameObject pile = t.Dupes.Drop(SimHashes.Iron, 60f, r.Cell(1, 1));
			yield return Kit.Settle(t);
			t.Assert.Eq(1, Kit.Port(plate), "heavy sends green");
			t.Assert.Eq("on_down", Kit.Anim(plate), "heavy and green: lit and pressed");

			plate.Invert = true;
			yield return Kit.Settle(t);
			t.Assert.Eq(0, Kit.Port(plate), "inverted and heavy sends red");
			t.Assert.Eq("off_down", Kit.Anim(plate), "pressed with the light off; the knobs do not move for a signal change alone");
			t.Assert.NotEq(plain, ports.outputPortInfo[0].activeDescription, "the port's text follows the inversion");

			t.World.Destroy(pile);
			yield return Kit.Settle(t);
			t.Assert.Eq(1, Kit.Port(plate), "inverted and light sends green");
			t.Assert.Eq("on_up", Kit.Anim(plate), "up with the light on");

			plate.Invert = false;
			yield return Kit.Settle(t);
			t.Assert.Eq(0, Kit.Port(plate), "back to red when empty");
			t.Assert.Eq(plain, ports.outputPortInfo[0].activeDescription, "and the port text is back");
			yield return t.Screenshot("up");
		}
	}
}
