using System.Collections;
using Isochronous.TestKit;
using Klei.AI;
using UnityEngine;

namespace SaltySurf.Tests
{
	/// <summary>A Duplicant on the board, doing the whole chore: the board is ready on Salt Water, the Duplicant surfs, the board spills.</summary>
	public static class Surfing
	{
		[OniTest("SaltySurf: a Duplicant surfs on a board filled with Salt Water; with real power, it spills Salt Water at the usual rate and the Duplicant gets the effect", Timeout = 100), Layer(0)]
		public static IEnumerator DupeSurfsOnSaltWater(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1.5);
			GameObject go = t.World.Build("MechanicalSurfboard", inside.Cell(2, 0));
			Board board = Board.Wrap(go);
			board.Rect = inside;
			board.Row = 0;
			board.X = 1;
			board.Tank.AddLiquid(SimHashes.SaltWater, 10f, 293.15f, byte.MaxValue, 0);
			t.World.Build("DevGenerator", inside.Cell(6, 0));
			yield return t.WaitGameSeconds(0.5);
			UtilityDragSession wire = t.Tools.UtilityTool("Wire", SimHashes.Copper);
			yield return wire.Path(inside.Cell(6, 0), inside.Cell(5, 0), inside.Cell(4, 0), inside.Cell(3, 0), inside.Cell(2, 0));
			wire.Dispose();
			yield return t.WaitGameSeconds(1);
			var dupes = t.Dupes.Spawn(1, inside.Cell(10, 0));
			MinionIdentity dupe = dupes[0];
			Schedule schedule = ScheduleManager.Instance.GetSchedule(dupe.GetComponent<Schedulable>());
			for (int hour = 0; hour < schedule.GetBlocks().Count; hour++)
				schedule.SetBlockGroup(hour, Db.Get().ScheduleGroups.Recreation);
			yield return t.WaitGameSeconds(1);
			float start = board.Mass(SimHashes.SaltWater);
			Effects effects = dupe.GetComponent<Effects>();
			bool surfed = false;
			for (int second = 0; second < 120 && !surfed; second++)
			{
				surfed = effects.HasEffect("MechanicalSurfboard");
				yield return t.WaitGameSeconds(1);
			}
			float spilled = start - board.Mass(SimHashes.SaltWater);
			t.Log("tank " + start + " kg -> " + board.Mass(SimHashes.SaltWater) + " kg");
			t.Assert.True(surfed, "the Duplicant got the Mechanical Surfboard effect");
			t.Assert.True(spilled > 1.2f && spilled < 1.8f, "a 30 second surf spilled about 1.5 kg at 0.05 kg/s (" + spilled + " kg)");
			t.Assert.True(board.OnFloor(SimHashes.SaltWater) > 0.1f, "Salt Water lies on the floor in front of the board (" + board.OnFloor(SimHashes.SaltWater) + " kg)");
			t.Assert.Eq(0f, board.OnFloor(SimHashes.Water), "and no Water");
		}
	}
}
