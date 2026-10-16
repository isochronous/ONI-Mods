using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace JustFallAlready.Tests
{
	/// <summary>
	/// Critters and doors. The mod's one observable act is a line in the log, "[JustFallAlready] ...
	/// told it to fall", written each time it has a critter's brain run its checks because a door
	/// took the critter's footing; each test reads those lines next to what the critter and the door
	/// really do. Doors are the pneumatic door (a 1x2 door whose top cell is a floor for the cell
	/// above), changed with the same call the door's control makes; the critter is a Hatch.
	/// </summary>
	public static class Doors
	{
		private sealed class Pokes
		{
			public readonly List<float> Times = new List<float>();
			public int Count => Times.Count;
		}

		private static Pokes WatchPokes(TestContext t)
		{
			var pokes = new Pokes();
			Application.LogCallback handler = (message, stack, type) =>
			{
				if (message.StartsWith("[JustFallAlready]") && message.Contains("told it to fall"))
					pokes.Times.Add(GameClock.Instance.GetTime());
			};
			Application.logMessageReceived += handler;
			t.Cleanup(() => Application.logMessageReceived -= handler);
			return pokes;
		}

		private static GameObject Critter(TestContext t, int cell)
		{
			GameObject go = Util.KInstantiate(Assets.GetPrefab("Hatch"), Grid.CellToPosCBC(cell, Grid.SceneLayer.Creatures));
			go.SetActive(true);
			t.Cleanup(() => { if (go != null) Util.KDestroyGameObject(go); });
			return go;
		}

		/// <summary>Tiles in the two columns beside the door column and a ceiling over it, so a critter in or on the door cannot wander off.</summary>
		private static void Pocket(TestContext t, CellRect inside, int x)
		{
			for (int y = 0; y < 5; y++)
			{
				t.World.Build("Tile", inside.Cell(x - 1, y), element: SimHashes.SandStone);
				t.World.Build("Tile", inside.Cell(x + 1, y), element: SimHashes.SandStone);
			}
			t.World.Build("Tile", inside.Cell(x, 4), element: SimHashes.SandStone);
		}

		private static bool Valid(GameObject critter)
		{
			Navigator nav = critter.GetComponent<Navigator>();
			return nav.NavGrid.NavTable.IsValid(Grid.PosToCell(critter), nav.CurrentNavType);
		}

		private static IEnumerator Settle(TestContext t, GameObject door, Door.ControlState state)
		{
			yield return t.WaitGameSeconds(1);
			door.GetComponent<Door>().QueueStateChange(state);
			yield return t.WaitGameSeconds(4);
		}

		private static bool OpenDoorsAreAir()
		{
			foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
				if (assembly.GetName().Name.IndexOf("CrittersFallThroughOpenDoors", System.StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			return false;
		}

		[OniTest("JFA: a door closing on a critter pokes it once the door has finished, and opening again pokes nothing", Timeout = 90), Layer(0)]
		public static IEnumerator CloseOnCritter(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			Pocket(t, inside, 5);
			GameObject door = t.World.Build("Door", inside.Cell(5, 0), element: SimHashes.SandStone);
			yield return Settle(t, door, Door.ControlState.Opened);
			GameObject critter = Critter(t, inside.Cell(5, 0));
			yield return t.WaitGameSeconds(3);
			t.Assert.True(Valid(critter), "the critter stands in the open door with valid footing");
			Pokes pokes = WatchPokes(t);

			float closed = GameClock.Instance.GetTime();
			door.GetComponent<Door>().QueueStateChange(Door.ControlState.Locked);
			yield return t.WaitUntil(() => pokes.Count > 0, 20, "the mod to poke the critter");
			float delay = pokes.Times[0] - closed;
			t.Log("poked " + delay.ToString("0.00") + " game seconds after the door was told to close");
			t.Assert.False(Valid(critter), "the closed door took the critter's footing");
			t.Assert.True(delay > 0.6f, "not poked while the door is still moving (" + delay.ToString("0.00") + " s)");
			yield return t.WaitGameSeconds(4);
			t.Assert.Eq(1, pokes.Count, "the critter is poked once, not on every navigation batch after");

			// The door opens again: the footing comes back, so there is nothing to poke.
			door.GetComponent<Door>().QueueStateChange(Door.ControlState.Opened);
			yield return t.WaitGameSeconds(5);
			t.Assert.True(Valid(critter), "the critter's footing is back with the door open");
			t.Assert.Eq(1, pokes.Count, "opening a door onto a critter with valid footing pokes nothing");
		}

		[OniTest("JFA: a critter in the upper door of two stacked doors drops to the lower one when the upper door closes", Timeout = 90), Layer(0)]
		public static IEnumerator PezDropper(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			Pocket(t, inside, 5);
			t.World.Build("Door", inside.Cell(5, 0), element: SimHashes.SandStone);
			GameObject upper = t.World.Build("Door", inside.Cell(5, 2), element: SimHashes.SandStone);
			yield return Settle(t, upper, Door.ControlState.Opened);
			int start = inside.Cell(5, 2);
			GameObject critter = Critter(t, start);
			yield return t.WaitGameSeconds(3);
			t.Assert.Eq(start, Grid.PosToCell(critter), "the critter stands in the upper door");
			upper.GetComponent<Door>().QueueStateChange(Door.ControlState.Locked);
			yield return t.WaitUntil(() => Grid.PosToCell(critter) < start, 10, "the critter to drop out of the closing door");
			var fall = critter.GetComponent<StateMachineController>().GetSMI<CreatureFallMonitor.Instance>();
			yield return t.WaitGameSeconds(3);
			t.Assert.False(fall.ShouldFall(), "the critter has landed");
		}

		[OniTest("JFA: a door opening under a critter standing on top of it pokes nothing (the cell above a door is not watched)", Timeout = 90), Layer(0)]
		public static IEnumerator OpeningUnderCritter(TestContext t)
		{
			t.Speed(3);
			if (OpenDoorsAreAir())
				t.Skip("Critters Fall Through Open Doors is loaded: the cell above the door is watched then");
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			Pocket(t, inside, 5);
			GameObject door = t.World.Build("Door", inside.Cell(5, 0), element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(2);
			int start = inside.Cell(5, 2);
			GameObject critter = Critter(t, start);
			yield return t.WaitGameSeconds(3);
			t.Assert.True(Valid(critter), "the critter stands on the closed door");
			Pokes pokes = WatchPokes(t);
			door.GetComponent<Door>().QueueStateChange(Door.ControlState.Opened);
			yield return t.WaitGameSeconds(6);
			t.Assert.Eq(0, pokes.Count, "no poke for a critter in a cell beside the door when open doors stay floor");
			t.Assert.Eq(start, Grid.PosToCell(critter), "the critter has not moved");
		}

		[OniTest("JFA: a Duplicant in a closing door is not poked", Timeout = 90), Layer(0)]
		public static IEnumerator DuplicantsAreLeftAlone(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			Pocket(t, inside, 5);
			GameObject door = t.World.Build("Door", inside.Cell(5, 0), element: SimHashes.SandStone);
			yield return Settle(t, door, Door.ControlState.Opened);
			t.Dupes.Spawn(1, inside.Cell(5, 0));
			yield return t.WaitGameSeconds(1);
			Pokes pokes = WatchPokes(t);
			door.GetComponent<Door>().QueueStateChange(Door.ControlState.Locked);
			yield return t.WaitGameSeconds(6);
			t.Assert.Eq(0, pokes.Count, "the mod pokes critters only");
		}

		/// <summary>
		/// With Sgt_Imalas's Critters Fall Through Open Doors loaded (run with -Steam 3816086407): the
		/// door opening under a critter standing on it makes the footing vanish, and the mod pokes it.
		/// </summary>
		[OniTest("JFA: with Critters Fall Through Open Doors, a door opening under a critter drops it at once", Explicit = true, Timeout = 90), Layer(0)]
		public static IEnumerator OpeningUnderCritterWithTheOtherMod(TestContext t)
		{
			t.Speed(3);
			if (!OpenDoorsAreAir())
				t.Skip("Critters Fall Through Open Doors is not loaded (run with -Steam 3816086407)");
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			Pocket(t, inside, 5);
			GameObject door = t.World.Build("Door", inside.Cell(5, 0), element: SimHashes.SandStone);
			yield return t.WaitGameSeconds(2);
			int start = inside.Cell(5, 2);
			GameObject critter = Critter(t, start);
			yield return t.WaitGameSeconds(3);
			Pokes pokes = WatchPokes(t);
			door.GetComponent<Door>().QueueStateChange(Door.ControlState.Opened);
			yield return t.WaitUntil(() => pokes.Count > 0, 20, "the mod to poke the critter");
			yield return t.WaitUntil(() => Grid.PosToCell(critter) < start, 10, "the critter to fall through the open door");
		}
	}
}
