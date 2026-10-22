using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace SaltySurf.Tests
{
	/// <summary>What the surfboard's intake and readiness check accept, and what it still refuses.</summary>
	public static class Intake
	{
		[OniTest("SaltySurf: Water and Salt Water carry the shared tag; other liquids do not; the intake is keyed to it", Timeout = 30), Layer(0)]
		public static IEnumerator Tags(TestContext t)
		{
			foreach (SimHashes hash in new[] { SimHashes.Water, SimHashes.SaltWater })
				t.Assert.True(ElementLoader.FindElementByHash(hash).HasTag(SurfWater.Tag), hash + " carries the tag");
			foreach (SimHashes hash in new[] { SimHashes.Brine, SimHashes.DirtyWater, SimHashes.CrudeOil, SimHashes.Petroleum, SimHashes.Magma })
				t.Assert.False(ElementLoader.FindElementByHash(hash).HasTag(SurfWater.Tag), hash + " does not");
			var consumer = Assets.GetPrefab("MechanicalSurfboard").GetComponent<ConduitConsumer>();
			t.Assert.Eq(SurfWater.Tag, consumer.capacityTag, "the prefab's intake is keyed to the shared tag");
			t.Assert.Eq(ConduitConsumer.WrongElementResult.Dump, consumer.wrongElementResult, "wrong liquids are still dumped");
			yield break;
		}

		[OniTest("SaltySurf: the Properties line of Water and Salt Water has no missing string and no trailing separator", Timeout = 30), Layer(0)]
		public static IEnumerator PropertiesLine(TestContext t)
		{
			foreach (SimHashes hash in SurfWater.Elements)
			{
				Element element = ElementLoader.FindElementByHash(hash);
				t.Assert.True(element.HasTag(SurfWater.Tag), hash + " still carries the tag");
				string text = element.FullDescription(false);
				t.Assert.False(text.Contains("MISSING"), hash + " description has no missing string (got: " + text.Replace("\n", " | ") + ")");
				t.Assert.False(text.ToUpperInvariant().Contains("SALTYSURFWATER"), hash + " description does not name the tag");
				foreach (string line in text.Split('\n'))
					t.Assert.False(line.TrimEnd().EndsWith(","), hash + " no description line ends in a separator (got: " + line + ")");
			}
			yield break;
		}

		[OniTest("SaltySurf: the info panel lists Water and Salt Water, and keeps the other lines", Timeout = 30), Layer(0)]
		public static IEnumerator Descriptors(TestContext t)
		{
			CellRect r = t.Rect(6, 5);
			Board board = Board.Build(t, r, 1);
			yield return t.WaitGameSeconds(1);
			string water = ElementLoader.FindElementByHash(SimHashes.Water).name;
			string salt = ElementLoader.FindElementByHash(SimHashes.SaltWater).name;
			List<Descriptor> list = GameUtil.GetAllDescriptors(board.Go);
			string all = "";
			foreach (Descriptor d in list)
				all += d.text + "\n" + d.tooltipText + "\n";
			t.Assert.True(all.Contains(water) && all.Contains(salt), "the requirement names " + water + " and " + salt + " (got: " + all.Replace("\n", " | ") + ")");
			string expected = string.Format(ModStrings.SALTYSURF.ELEMENT_LIST, water, salt);
			t.Assert.True(all.Contains(expected), "in the mod's wording: " + expected);
			t.Assert.True(all.Contains(GameUtil.GetFormattedMass(0.05f, GameUtil.TimeSlice.PerSecond)), "the leak line is still there");
		}

		[OniTest("SaltySurf: the intake fills the tank from Water or Salt Water, and a wrong liquid is dumped and hurts the board", Timeout = 60), Layer(0)]
		public static IEnumerator IntakeAcceptsBoth(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(6, 5);
			Board board = Board.Build(t, r, 1);
			int damage = 0;
			board.Go.Subscribe(-794517298, _ => damage++);
			yield return t.WaitGameSeconds(1);

			board.Feed(SimHashes.SaltWater, 4f);
			yield return t.WaitGameSeconds(2.5);
			t.Assert.True(board.Mass(SimHashes.SaltWater) > 3.9f, "the tank took the Salt Water (" + board.Mass(SimHashes.SaltWater) + " kg)");
			t.Assert.Eq(0, damage, "Salt Water does not damage the board");

			board.Feed(SimHashes.Water, 3f);
			yield return t.WaitGameSeconds(2.5);
			t.Assert.True(board.Mass(SimHashes.Water) > 2.9f, "the tank took the Water (" + board.Mass(SimHashes.Water) + " kg)");
			t.Assert.Eq(0, damage, "Water does not damage the board");

			board.Feed(SimHashes.CrudeOil, 2f);
			yield return t.WaitGameSeconds(2.5);
			t.Assert.Eq(0f, board.Mass(SimHashes.CrudeOil), "crude oil is not stored");
			t.Assert.True(damage > 0, "crude oil damages the board as in the base game");
			t.Assert.Eq(SimHashes.CrudeOil, Grid.Element[board.Origin].id, "and is dumped in its cell");
		}

		[OniTest("SaltySurf: the board is ready at 2 kg of Salt Water, of Water, or of both together; not at less, not on other liquids", Timeout = 40), Layer(0)]
		public static IEnumerator Ready(TestContext t)
		{
			CellRect r = t.Rect(6, 5);
			Board board = Board.Build(t, r, 1);
			yield return t.WaitGameSeconds(1);
			t.Assert.False(board.Ready(), "an empty tank is not ready");

			board.Tank.AddLiquid(SimHashes.SaltWater, 1.5f, 293.15f, byte.MaxValue, 0);
			t.Assert.False(board.Ready(), "1.5 kg of Salt Water is not enough");
			board.Tank.AddLiquid(SimHashes.SaltWater, 1f, 293.15f, byte.MaxValue, 0);
			t.Assert.True(board.Ready(), "2.5 kg of Salt Water is");
			yield return t.WaitGameSeconds(1);

			board.Tank.ConsumeIgnoringDisease(SurfWater.Tag, 100f);
			board.Tank.AddLiquid(SimHashes.Water, 2.5f, 293.15f, byte.MaxValue, 0);
			t.Assert.True(board.Ready(), "2.5 kg of Water is, as in the base game");
			yield return t.WaitGameSeconds(1);

			board.Tank.ConsumeIgnoringDisease(SurfWater.Tag, 100f);
			board.Tank.AddLiquid(SimHashes.Water, 1.2f, 293.15f, byte.MaxValue, 0);
			board.Tank.AddLiquid(SimHashes.SaltWater, 1.2f, 293.15f, byte.MaxValue, 0);
			t.Assert.True(board.Ready(), "1.2 kg of each is 2.4 kg of the shared tag");
			yield return t.WaitGameSeconds(1);

			board.Tank.ConsumeIgnoringDisease(SurfWater.Tag, 100f);
			board.Tank.AddLiquid(SimHashes.CrudeOil, 5f, 293.15f, byte.MaxValue, 0);
			t.Assert.False(board.Ready(), "5 kg of crude oil is not");
		}
	}
}
