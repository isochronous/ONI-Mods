using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace VentFreezeFix.Tests
{
	/// <summary>
	/// "Insulate vent storage": a liquid vent's storage keeps its packet hidden, sealed and out of the
	/// temperature simulation, so a packet that is below its freezing point is not turned into a tile
	/// by the sim after a load.
	/// </summary>
	public static class Insulation
	{
		private static List<Storage.StoredItemModifier> Modifiers(Storage storage)
		{
			return Traverse.Create(storage).Field("defaultStoredItemModifers").GetValue<List<Storage.StoredItemModifier>>();
		}

		[OniTest("VFF: a liquid vent's storage hides, seals and insulates what it holds; a gas vent's does not", Timeout = 40), Layer(0)]
		public static IEnumerator VentStorage(TestContext t)
		{
			CellRect r = t.Rect(6, 3);
			GameObject vent = Vents.Build(t, r.Cell(1, 1));
			GameObject gasVent = t.World.Build("GasVent", r.Cell(4, 1));
			yield return t.WaitGameSeconds(1);
			Storage storage = vent.GetComponent<Storage>();
			var modifiers = Modifiers(storage);
			t.Assert.True(modifiers.Contains(Storage.StoredItemModifier.Hide), "hide");
			t.Assert.True(modifiers.Contains(Storage.StoredItemModifier.Seal), "seal");
			t.Assert.True(modifiers.Contains(Storage.StoredItemModifier.Insulate), "insulate");
			t.Assert.False(Modifiers(gasVent.GetComponent<Storage>()).Contains(Storage.StoredItemModifier.Insulate), "a gas vent's storage is the game's own");

			// A packet below freezing, in the vent's storage but not allowed out.
			vent.GetComponent<ConduitConsumer>().consumptionRate = 0f;
			storage.AddLiquid(SimHashes.Water, 1.5f, 253.15f, byte.MaxValue, 0);
			GameObject packet = storage.items.First();
			t.Assert.True(packet.HasTag(GameTags.Sealed), "the packet is sealed");
			t.Assert.False(packet.GetComponent<SimTemperatureTransfer>().enabled, "and not temperature simulated");
			yield return t.WaitGameSeconds(1);
			t.Assert.False(Grid.Solid[Grid.PosToCell(vent)], "the vent's cell is still open");
		}
	}

	/// <summary>
	/// The reported bug end to end: a packet below its freezing point in a vent's storage when a game
	/// is saved must still be a packet in the vent after the load, not a tile entombing it.
	/// </summary>
	public static class SaveLoad
	{
		private static string RoundTripPath => Path.Combine(Session.Dir, "ventfreezefix-roundtrip.sav");

		[OniTest("VFF: (round trip 1/2) a vent holding a packet at -20 C over a puddle is saved to the run folder", Timeout = 60), Layer(0)]
		public static IEnumerator WriteRoundTrip(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(5, 6);
			t.World.Build("Tile", r.Cell(2, 0));
			int ventCell = r.Cell(2, 4);
			GameObject vent = Vents.Build(t, ventCell);
			yield return t.WaitGameSeconds(1);
			vent.GetComponent<ConduitConsumer>().consumptionRate = 0f;
			vent.GetComponent<Storage>().AddLiquid(SimHashes.Water, 1.5f, 253.15f, byte.MaxValue, 0);
			Vents.Put(r.Cell(2, 1), SimHashes.Water, 2f, 293.15f);
			yield return t.WaitGameSeconds(2);
			string path = RoundTripPath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (FreshSave) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
		}

		[OniTest("VFF: (round trip 2/2) after the load the vent cell is open, no tile entombs the vent, and the watcher kept its snapshot", Timeout = 60, FreshSave = true), Layer(0)]
		public static IEnumerator RoundTripLoad(TestContext t)
		{
			string loaded = Session.SavePath;
			t.Cleanup(() =>
			{
				string original = Directory.GetFiles(Session.Dir, "fixture.sav").FirstOrDefault();
				if (original != null)
					Session.SavePath = original;
				if (File.Exists(RoundTripPath))
					File.Delete(RoundTripPath);
			});
			t.Assert.True(loaded.EndsWith("ventfreezefix-roundtrip.sav"), "this test started from the round trip save (" + loaded + ")");
			VentFreezeWatcher watcher = Object.FindObjectsOfType<VentFreezeWatcher>().FirstOrDefault();
			t.Assert.NotNull(watcher, "the vent was loaded with its watcher");
			var traverse = Traverse.Create(watcher);
			int ventCell = Grid.PosToCell(watcher);
			t.Assert.Eq(ventCell, traverse.Field("savedVentCell").GetValue<int>(), "the watcher kept the vent's cell");
			int landing = traverse.Field("savedLandingCell").GetValue<int>();
			t.Assert.True(Grid.IsValidCell(landing) && landing < ventCell, "and a landing cell under it (" + landing + ")");
			t.Assert.Eq(SimHashes.Water, traverse.Field("savedLandingElement").GetValue<SimHashes>(), "holding water");
			t.Assert.True(traverse.Field("savedLandingMass").GetValue<float>() > 0f, "with the mass it had when saved");

			t.Speed(3);
			yield return t.WaitGameSeconds(3);
			t.Assert.False(Grid.Solid[ventCell], "the vent's cell is open, not a tile entombing the vent (" + Grid.Element[ventCell].id + ")");
			// The consumption rate is not saved, so the loaded vent emits its packet on its first tick, as in the base game; the packet is not entombed.
			Storage storage = watcher.GetComponent<Storage>();
			float inVent = 0f;
			foreach (GameObject item in storage.items)
				inVent += item.GetComponent<PrimaryElement>().Mass;
			t.Assert.True(inVent < 0.01f, "the vent emitted its packet (" + inVent + " kg left in it)");
			t.Assert.True(Vents.Ticks(watcher) > 5, "and the watcher ran its window and stopped (" + Vents.Ticks(watcher) + ")");
		}
	}
}
