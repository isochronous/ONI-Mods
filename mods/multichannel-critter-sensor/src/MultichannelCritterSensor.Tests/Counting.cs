using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace MultichannelCritterSensor.Tests
{
	/// <summary>
	/// What the sensor counts and what it writes on its ribbon: the species filters, the two kinds
	/// of threshold, both modes, and the indicators that follow the bits. Real critters and eggs in
	/// a real room.
	/// </summary>
	public static class Counting
	{
		private const int Primary = 1, Egg = 2, Rising = 4, Falling = 8;

		[OniTest("MCS: critters and eggs are counted by species, kind, threshold and mode", Timeout = 90), Layer(1)]
		public static IEnumerator CountsAndThresholds(TestContext t)
		{
			CellRect r = default;
			MultichannelCritterSensor s = null;
			yield return Kit.RoomWithSensor(t, (rr, ss) => { r = rr; s = ss; });
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, s.TrackedTotal, "an empty room counts nothing");
			t.Assert.Eq(0, Kit.Port(s) & (Primary | Egg), "and its threshold bits are off");

			Kit.Spawn("Hatch", r.Cell(5, 1));
			Kit.Spawn("Puft", r.Cell(7, 2));
			Kit.Spawn("HatchEgg", r.Cell(4, 1));
			Kit.Spawn("PuftEgg", r.Cell(9, 1));
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(2, s.CritterCount, "two critters");
			t.Assert.Eq(2, s.EggCount, "two eggs");
			t.Assert.Eq(4, s.TrackedTotal, "four in all");
			t.Assert.Eq(Primary, Kit.Port(s) & (Primary | Egg), "above the default threshold of 0 the first bit is on");

			// Combined mode: above and below are strict.
			s.combinedThreshold = 4;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, Kit.Port(s) & (Primary | Egg), "4 is not above 4");
			s.combinedThreshold = 3;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(Primary, Kit.Port(s) & (Primary | Egg), "4 is above 3");
			s.combinedAbove = false;
			s.combinedThreshold = 4;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, Kit.Port(s) & (Primary | Egg), "4 is not below 4");
			s.combinedThreshold = 5;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(Primary, Kit.Port(s) & (Primary | Egg), "4 is below 5");
			s.combinedAbove = true;
			s.combinedThreshold = 0;
			s.NotifyConfigChanged();

			// Species filters: leaving the All state unticks just the clicked row.
			var critters = new List<Tag> { Kit.Hatch, Kit.Puft };
			var eggs = new List<Tag> { Kit.HatchEgg, Kit.PuftEgg };
			s.ToggleSpecies(true, Kit.Hatch, critters);
			t.Assert.False(s.allCritters, "no longer counting all critters");
			t.Assert.True(s.IsSpeciesSelected(true, Kit.Puft) && !s.IsSpeciesSelected(true, Kit.Hatch), "Puft stays selected, Hatch is not");
			s.ToggleSpecies(false, Kit.PuftEgg, eggs);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(1, s.CritterCount, "only the Puft");
			t.Assert.Eq(1, s.EggCount, "only the Hatch egg");
			s.ToggleSpecies(true, Kit.Hatch, critters);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(2, s.CritterCount, "ticking Hatch again counts it");
			s.SetAllSpecies(false, true);
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(2, s.EggCount, "All eggs counts both");
			s.SetAllSpecies(true, true);

			// Kinds.
			s.countCritters = false;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, s.CritterCount, "critters switched off count nothing");
			t.Assert.Eq(2, s.TrackedTotal, "the total is the eggs");
			s.countCritters = true;
			s.countEggs = false;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, s.EggCount, "eggs switched off count nothing");
			s.countEggs = true;

			// Separate mode: the critter threshold on bit 1, the egg threshold on bit 2.
			s.separateThresholds = true;
			s.critterThreshold = 1;
			s.eggThreshold = 2;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1.5);
			t.Assert.Eq(Primary, Kit.Port(s) & (Primary | Egg), "2 critters are above 1, 2 eggs are not above 2");
			t.Assert.True(Kit.MeterAnim(s, "critterMeter").Contains("meter_critter_on"), "the critter indicator is on, " + Kit.MeterAnim(s, "critterMeter"));
			t.Assert.True(Kit.MeterAnim(s, "eggMeter").Contains("meter_egg_off"), "the egg indicator is off, " + Kit.MeterAnim(s, "eggMeter"));
			s.eggAbove = false;
			s.eggThreshold = 3;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1.5);
			t.Assert.Eq(Primary | Egg, Kit.Port(s) & (Primary | Egg), "2 eggs are below 3");
			t.Assert.True(Kit.MeterAnim(s, "eggMeter").Contains("meter_egg_on"), "the egg indicator follows bit 2, " + Kit.MeterAnim(s, "eggMeter"));
			s.critterAbove = false;
			s.critterThreshold = 1;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(Egg, Kit.Port(s) & (Primary | Egg), "2 critters are not below 1");
			s.countEggs = false;
			s.NotifyConfigChanged();
			yield return t.WaitGameSeconds(1);
			t.Assert.Eq(0, Kit.Port(s) & Egg, "the egg bit stays off while eggs are not counted");
		}

		[OniTest("MCS: a rise or a fall of the total sends one pulse, and an edit of the settings sends none", Timeout = 90), Layer(1)]
		public static IEnumerator Pulses(TestContext t)
		{
			CellRect r = default;
			MultichannelCritterSensor s = null;
			yield return Kit.RoomWithSensor(t, (rr, ss) => { r = rr; s = ss; });
			yield return t.WaitGameSeconds(1.5);

			// Three arrive in one frame: one pulse.
			int seen = 0, edges = 0;
			var crowd = new List<GameObject>();
			bool spawned = false;
			yield return Kit.Watch(t, s, 2, now =>
			{
				if (spawned)
					return;
				spawned = true;
				for (int i = 0; i < 3; i++)
					crowd.Add(Kit.Spawn("Hatch", r.Cell(4 + i, 1)));
			}, (bits, rising) => { seen = bits; edges = rising; });
			t.Assert.True((seen & Rising) != 0, "a pulse on bit 3 when the count rises");
			t.Assert.Eq(1, edges, "three arrivals in one scan are one pulse");
			t.Assert.Eq(0, seen & Falling, "no pulse on bit 4");
			t.Assert.Eq(0, Kit.Port(s) & (Rising | Falling), "the pulse is over");

			// One leaves: a pulse on bit 4.
			Util.KDestroyGameObject(crowd[0]);
			yield return Kit.Watch(t, s, 2, null, (bits, rising) => { seen = bits; edges = rising; });
			t.Assert.True((seen & Falling) != 0, "a pulse on bit 4 when the count falls");
			t.Assert.Eq(0, seen & Rising, "no pulse on bit 3");
			t.Assert.Eq(0, Kit.Port(s) & (Rising | Falling), "the pulse is over");

			// Two arrivals in consecutive scans: two pulses with a low tick between.
			int arrivals = 0;
			yield return Kit.Watch(t, s, 2, now =>
			{
				if (arrivals == 0 || (arrivals == 1 && now > 0.45))
				{
					arrivals++;
					crowd.Add(Kit.Spawn("Hatch", r.Cell(7 + arrivals, 1)));
				}
			}, (bits, rising) => { seen = bits; edges = rising; });
			t.Assert.Eq(2, edges, "arrivals half a second apart are two pulses");

			// A hatch: an egg goes and a critter comes in the same scan. The total is the same.
			GameObject egg = Kit.Spawn("HatchEgg", r.Cell(3, 1));
			yield return t.WaitGameSeconds(2);
			bool hatched = false;
			yield return Kit.Watch(t, s, 1.5, now =>
			{
				if (hatched)
					return;
				hatched = true;
				Util.KDestroyGameObject(egg);
				crowd.Add(Kit.Spawn("Hatch", r.Cell(2, 2)));
			}, (bits, rising) => { seen = bits; edges = rising; });
			t.Assert.Eq(0, seen & (Rising | Falling), "an egg hatching leaves the total unchanged and sends no pulse");

			// Settings edits move the total and send nothing.
			s.countCritters = false;
			s.NotifyConfigChanged();
			yield return Kit.Watch(t, s, 1.5, null, (bits, rising) => { seen = bits; edges = rising; });
			t.Assert.Eq(0, seen & (Rising | Falling), "switching the critters off is not a fall");
			s.countCritters = true;
			s.NotifyConfigChanged();
			yield return Kit.Watch(t, s, 1.5, null, (bits, rising) => { seen = bits; edges = rising; });
			t.Assert.Eq(0, seen & (Rising | Falling), "switching them on again is not a rise");
		}

		[OniTest("MCS: a sensor that is no longer in a room reads nothing and says so", Timeout = 60), Layer(1)]
		public static IEnumerator NotInARoom(TestContext t)
		{
			// In a room, then the sensor's own cell turned to rock: no cavity, no room.
			CellRect r = default;
			MultichannelCritterSensor s = null;
			yield return Kit.RoomWithSensor(t, (rr, ss) => { r = rr; s = ss; });
			GameObject go = s.gameObject;
			Kit.Spawn("Hatch", r.Cell(5, 1));
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(1, s.TrackedTotal, "counts the Hatch while in the room");
			SimMessages.ReplaceElement(r.Cell(2, 1), SimHashes.Granite, CellEventLogger.Instance.SandBoxTool, 1000f, 293.15f);
			yield return t.WaitUntil(() => !s.InRoom, 10, "the sensor out of any room");
			yield return t.WaitGameSeconds(1);
			t.Assert.False(s.InRoom, "not in a room");
			t.Assert.Eq(0, s.TrackedTotal, "counts nothing");
			t.Assert.Eq(0, Kit.Port(s), "writes nothing");
			t.Assert.True(go.GetComponent<KSelectable>().HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom), "shows the Not In Room status");
		}
	}
}
