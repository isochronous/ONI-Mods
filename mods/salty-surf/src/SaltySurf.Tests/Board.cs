using System.Collections;
using System.Reflection;
using Isochronous.TestKit;
using UnityEngine;

namespace SaltySurf.Tests
{
	/// <summary>A Mechanical Surfboard on a floor of tiles, with its liquid intake pipe, and the calls into the vanilla parts the mod patched.</summary>
	internal sealed class Board
	{
		public GameObject Go;
		public CellRect Rect;
		public int IntakeCell;
		public int Origin;
		public int X;
		/// <summary>The row of the test area the board stands in.</summary>
		public int Row = 1;
		public Storage Tank;
		public ConduitConsumer Consumer;

		private static readonly MethodInfo IsReady = typeof(MechanicalSurfboard.States).GetMethod("IsReady", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo WorkTick = typeof(MechanicalSurfboardWorkable).GetMethod("OnWorkTick", BindingFlags.Instance | BindingFlags.NonPublic);

		/// <summary>Floor tiles along the bottom row of the rect from x to x+3, the board on them, a pipe in its intake cell.</summary>
		public static Board Build(TestContext t, CellRect r, int x)
		{
			for (int i = 0; i < 4; i++)
				t.World.Build("Tile", r.Cell(x + i, 0));
			var board = new Board();
			board.Rect = r;
			board.X = x;
			board.Origin = r.Cell(x, 1);
			board.Go = t.World.Build("MechanicalSurfboard", board.Origin);
			board.IntakeCell = board.Go.GetComponent<Building>().GetUtilityInputCell();
			t.World.Build("LiquidConduit", board.IntakeCell);
			board.Tank = board.Go.GetComponent<Storage>();
			board.Consumer = board.Go.GetComponent<ConduitConsumer>();
			// No power in the fixture: the intake would wait for the board to be operational.
			board.Consumer.alwaysConsume = true;
			return board;
		}

		public static Board Wrap(GameObject go)
		{
			var board = new Board();
			board.Go = go;
			board.Origin = Grid.PosToCell(go);
			board.IntakeCell = go.GetComponent<Building>().GetUtilityInputCell();
			board.Tank = go.GetComponent<Storage>();
			board.Consumer = go.GetComponent<ConduitConsumer>();
			return board;
		}

		public float Mass(SimHashes element)
		{
			float total = 0f;
			foreach (GameObject item in Tank.items)
			{
				PrimaryElement primary = item != null ? item.GetComponent<PrimaryElement>() : null;
				if (primary != null && primary.ElementID == element)
					total += primary.Mass;
			}
			return total;
		}

		/// <summary>
		/// Mass of the element lying on the floor in front of this board (its width and a cell either side); a spill
		/// spreads over the floor and some of it dries away, so tests ask whether any landed, not how much.
		/// </summary>
		public float OnFloor(SimHashes element)
		{
			float total = 0f;
			for (int x = System.Math.Max(0, X - 1); x < System.Math.Min(Rect.W, X + 5); x++)
			{
				int cell = Rect.Cell(x, Row);
				if (Grid.Element[cell].id == element)
					total += Grid.Mass[cell];
			}
			return total;
		}

		public bool Ready()
		{
			MechanicalSurfboard surfboard = Go.GetComponent<MechanicalSurfboard>();
			return (bool)IsReady.Invoke(surfboard.smi.sm, new object[] { surfboard.smi });
		}

		/// <summary>One work tick of dt seconds, as the Duplicant on the board would run it (the patched code never reads the worker).</summary>
		public void Surf(float dt)
		{
			WorkTick.Invoke(Go.GetComponent<MechanicalSurfboardWorkable>(), new object[] { null, dt });
		}

		/// <summary>Puts a pipe packet at the intake; the intake takes it within a conduit update.</summary>
		public void Feed(SimHashes element, float kg)
		{
			ConduitFlow flow = Conduit.GetFlowManager(ConduitType.Liquid);
			flow.SetContents(IntakeCell, ConduitFlow.ConduitContents.Empty);
			flow.AddElement(IntakeCell, element, kg, 293.15f, byte.MaxValue, 0);
		}
	}
}
