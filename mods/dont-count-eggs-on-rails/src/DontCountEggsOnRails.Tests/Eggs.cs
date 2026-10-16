using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace DontCountEggsOnRails.Tests
{
	/// <summary>
	/// Critter sensors and eggs. Each test seals a small room (a critter sensor needs one: 12 to 64
	/// cells), puts a real critter count sensor in it and watches the number the sensor reports.
	/// An egg is put on a rail with the game's own call for it, SolidConduitFlow.AddPickupable, the
	/// one a rail loader makes: it marks the egg stored with a plain true and no storage behind it,
	/// the one state the mod keys on. Taking it off again is RemovePickupable.
	/// </summary>
	public static class Eggs
	{
		/// <summary>An egg standing on the room's floor at the cell.</summary>
		private static GameObject SpawnEgg(int cell)
		{
			GameObject egg = GameUtil.KInstantiate(Assets.GetPrefab("HatchEgg"), Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore), Grid.SceneLayer.Ore);
			egg.SetActive(true);
			return egg;
		}

		/// <summary>Walls a 10x3 room, puts a critter count sensor in it and waits for the room and the sensor.</summary>
		private static IEnumerator Room(TestContext t, System.Action<CellRect, LogicCritterCountSensor> done)
		{
			CellRect outer = new CellRect(t.Area.X + 1, t.Area.Y + 1, 12, 5);
			CellRect inside = t.Dupes.Box(outer);
			yield return t.WaitGameSeconds(1);
			GameObject go = t.World.Build("LogicCritterCountSensor", inside.Cell(9, 2));
			LogicCritterCountSensor sensor = go.GetComponent<LogicCritterCountSensor>();
			sensor.countCritters = false;
			sensor.countEggs = true;
			yield return t.WaitUntil(() => Game.Instance.roomProber.GetRoomOfGameObject(go) != null, 20, "the sealed area to become a room");
			done(inside, sensor);
		}

		private static CavityInfo Cavity(LogicCritterCountSensor sensor)
		{
			return Game.Instance.roomProber.GetRoomOfGameObject(sensor.gameObject).cavity;
		}

		[OniTest("DCEOR: an egg lying on the floor is counted by the room's critter sensor", Timeout = 60), Layer(0)]
		public static IEnumerator FloorEggCounts(TestContext t)
		{
			t.Speed(3);
			CellRect inside = default;
			LogicCritterCountSensor sensor = null;
			yield return Room(t, (r, s) => { inside = r; sensor = s; });
			t.Assert.Eq(0, sensor.currentCount, "an empty room counts no eggs");

			SpawnEgg(inside.Cell(2, 0));
			yield return t.WaitUntil(() => sensor.currentCount == 1, 15, "the sensor to count the egg on the floor");
			t.Assert.Eq(1, Cavity(sensor).eggs.Count, "the room lists the egg");
		}

		[OniTest("DCEOR: an egg riding a rail is not counted, whether it was counted before or never was, and counts again off the rail", Timeout = 120), Layer(0)]
		public static IEnumerator RailEggIsNotCounted(TestContext t)
		{
			t.Speed(3);
			CellRect inside = default;
			LogicCritterCountSensor sensor = null;
			yield return Room(t, (r, s) => { inside = r; sensor = s; });
			int railA = inside.Cell(2, 0), railB = inside.Cell(4, 0);
			t.World.Build("SolidConduit", railA);
			t.World.Build("SolidConduit", railB);
			yield return t.WaitGameSeconds(2);

			// Egg A is in the room's list first, then goes onto a rail.
			GameObject eggA = SpawnEgg(railA);
			yield return t.WaitUntil(() => sensor.currentCount == 1, 15, "the sensor to count egg A before it rides");
			Game.Instance.solidConduitFlow.AddPickupable(railA, eggA.GetComponent<Pickupable>());
			t.Assert.True(eggA.HasTag(GameTags.Stored) && eggA.GetComponent<Pickupable>().storage == null, "the rail marks egg A stored with no storage");
			yield return t.WaitUntil(() => sensor.currentCount == 0, 15, "the sensor to stop counting egg A on the rail");
			t.Assert.Eq(0, Cavity(sensor).eggs.Count, "the room's list no longer holds egg A");
			yield return t.WaitGameSeconds(4);
			t.Assert.Eq(0, sensor.currentCount, "egg A stays uncounted for as long as it rides");

			// Egg B goes onto a rail before the egg's own update has ever listed it.
			GameObject eggB = SpawnEgg(railB);
			Game.Instance.solidConduitFlow.AddPickupable(railB, eggB.GetComponent<Pickupable>());
			yield return t.WaitGameSeconds(4);
			t.Assert.Eq(0, sensor.currentCount, "egg B is never counted on its rail");

			// A third egg lying on the floor is counted while the other two ride.
			SpawnEgg(inside.Cell(7, 0));
			yield return t.WaitUntil(() => sensor.currentCount == 1, 15, "the sensor to count only the egg on the floor");
			yield return t.WaitGameSeconds(3);
			t.Assert.Eq(1, sensor.currentCount, "one egg counted, two riding");

			// Off the rail the game's own update lists them again.
			Game.Instance.solidConduitFlow.RemovePickupable(railA);
			yield return t.WaitUntil(() => sensor.currentCount == 2, 15, "egg A to count again once off its rail");
			Game.Instance.solidConduitFlow.RemovePickupable(railB);
			yield return t.WaitUntil(() => sensor.currentCount == 3, 15, "egg B to count again once off its rail");
		}

		[OniTest("DCEOR: an egg in an incubator is still counted", Timeout = 60), Layer(0)]
		public static IEnumerator IncubatorEggCounts(TestContext t)
		{
			t.Speed(3);
			CellRect inside = default;
			LogicCritterCountSensor sensor = null;
			yield return Room(t, (r, s) => { inside = r; sensor = s; });
			GameObject incubator = t.World.Build("EggIncubator", inside.Cell(3, 0));
			yield return t.WaitGameSeconds(2);
			GameObject egg = SpawnEgg(inside.Cell(6, 0));
			// The way a Duplicant's delivery ends: the egg goes into the incubator's storage, then the receptacle takes it.
			incubator.GetComponent<Storage>().Store(egg);
			incubator.GetComponent<EggIncubator>().ForceDeposit(egg);
			Pickupable pickupable = egg.GetComponent<Pickupable>();
			t.Assert.NotNull(pickupable.storage, "the incubator is the egg's storage");
			yield return t.WaitUntil(() => sensor.currentCount == 1, 15, "the sensor to count the incubated egg");
			yield return t.WaitGameSeconds(3);
			t.Assert.Eq(1, sensor.currentCount, "the incubated egg stays counted");
		}
	}
}
