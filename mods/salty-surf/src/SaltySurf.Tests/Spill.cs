using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace SaltySurf.Tests
{
	/// <summary>The spill while a Duplicant surfs: the board drops what it was fed.</summary>
	public static class Spill
	{
		internal static IEnumerator Surf(TestContext t, Board board, int ticks)
		{
			for (int i = 0; i < ticks; i++)
			{
				board.Surf(2f);
				yield return t.WaitGameSeconds(0.5);
			}
		}

		internal static string Describe(int cell) => Grid.Element[cell].id + " " + Grid.Mass[cell].ToString("F2") + " kg";

		[OniTest("SaltySurf: a Salt Water board spills Salt Water, a Water board spills Water, at the usual rate", Timeout = 60), Layer(0)]
		public static IEnumerator SpillsWhatItWasFed(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(12, 5);
			Board salt = Board.Build(t, r, 1);
			yield return t.WaitGameSeconds(0.5);
			Board water = Board.Build(t, r, 7);
			yield return t.WaitGameSeconds(1);
			salt.Tank.AddLiquid(SimHashes.SaltWater, 5f, 293.15f, byte.MaxValue, 0);
			water.Tank.AddLiquid(SimHashes.Water, 5f, 293.15f, byte.MaxValue, 0);
			yield return t.WaitGameSeconds(1);

			// 5 ticks of 2 s at 0.05 kg/s: half a kilogram each.
			yield return Surf(t, salt, 5);
			yield return Surf(t, water, 5);
			yield return t.WaitGameSeconds(2);
			t.Assert.True(Mathf.Abs(salt.Mass(SimHashes.SaltWater) - 4.5f) < 0.02f, "the Salt Water tank lost 0.5 kg (now " + salt.Mass(SimHashes.SaltWater) + ")");
			t.Assert.True(Mathf.Abs(water.Mass(SimHashes.Water) - 4.5f) < 0.02f, "the Water tank lost 0.5 kg (now " + water.Mass(SimHashes.Water) + ")");
			t.Assert.True(salt.OnFloor(SimHashes.SaltWater) > 0.05f, "Salt Water lies in front of the Salt Water board (" + salt.OnFloor(SimHashes.SaltWater) + ")");
			t.Assert.Eq(0f, salt.OnFloor(SimHashes.Water), "and no Water");
			t.Assert.True(water.OnFloor(SimHashes.Water) > 0.05f, "Water lies in front of the Water board (" + water.OnFloor(SimHashes.Water) + ")");
			t.Assert.Eq(0f, water.OnFloor(SimHashes.SaltWater), "and no Salt Water");
		}

		[OniTest("SaltySurf: a tank holding no accepted liquid spills nothing", Timeout = 40), Layer(0)]
		public static IEnumerator NothingToSpill(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(6, 5);
			Board board = Board.Build(t, r, 1);
			yield return t.WaitGameSeconds(1);
			board.Tank.AddLiquid(SimHashes.CrudeOil, 3f, 293.15f, byte.MaxValue, 0);
			int cell = board.Origin;
			SimHashes before = Grid.Element[cell].id;
			yield return Surf(t, board, 3);
			yield return t.WaitGameSeconds(1.5);
			t.Assert.Eq(3f, board.Mass(SimHashes.CrudeOil), "the oil stays in the tank");
			t.Assert.Eq(before, Grid.Element[cell].id, "and nothing lands on the floor");
		}

		[OniTest("SaltySurf: a tank of both gives up its mass and the spill lands on the floor", Timeout = 40), Layer(0)]
		public static IEnumerator MixedTank(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(6, 5);
			Board board = Board.Build(t, r, 1);
			yield return t.WaitGameSeconds(1);
			board.Tank.AddLiquid(SimHashes.Water, 1f, 293.15f, byte.MaxValue, 0);
			board.Tank.AddLiquid(SimHashes.SaltWater, 3f, 293.15f, byte.MaxValue, 0);
			float before = board.Mass(SimHashes.Water) + board.Mass(SimHashes.SaltWater);
			yield return Surf(t, board, 3);
			yield return t.WaitGameSeconds(1.5);
			float after = board.Mass(SimHashes.Water) + board.Mass(SimHashes.SaltWater);
			t.Assert.True(Mathf.Abs(before - after - 0.3f) < 0.02f, "0.3 kg left the tank (" + before + " to " + after + ")");
			float landed = board.OnFloor(SimHashes.Water) + board.OnFloor(SimHashes.SaltWater);
			t.Assert.True(landed > 0.05f, "and it lies on the floor (" + landed + ")");
		}
	}
}
