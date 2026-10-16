using System.Collections;
using System.IO;
using System.Linq;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace SaltySurf.Tests
{
	/// <summary>
	/// The shared tag is added to the elements when the table loads and copied onto each stored
	/// chunk; a tank saved with Salt Water must still count as full after a load.
	/// </summary>
	public static class Persistence
	{
		private static string RoundTripPath => Path.Combine(Session.Dir, "saltysurf-roundtrip.sav");

		[OniTest("SaltySurf: (round trip 1/2) a surfboard with Salt Water in its tank is saved to the run folder", Timeout = 60), Layer(0)]
		public static IEnumerator WriteRoundTrip(TestContext t)
		{
			t.Speed(3);
			CellRect r = t.Rect(6, 5);
			Board board = Board.Build(t, r, 1);
			yield return t.WaitGameSeconds(1);
			board.Tank.AddLiquid(SimHashes.SaltWater, 6f, 293.15f, byte.MaxValue, 0);
			yield return t.WaitGameSeconds(1);
			t.Assert.True(board.Ready(), "ready before saving");
			string path = RoundTripPath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (FreshSave) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
		}

		[OniTest("SaltySurf: (round trip 2/2) after loading, the tank still holds the Salt Water, counts as ready and spills it", Timeout = 60, FreshSave = true), Layer(0)]
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
			t.Speed(3);
			t.Assert.True(loaded.EndsWith("saltysurf-roundtrip.sav"), "this test started from the round trip save (" + loaded + ")");
			yield return t.WaitGameSeconds(1);
			MechanicalSurfboard surfboard = Object.FindObjectsOfType<MechanicalSurfboard>().FirstOrDefault();
			t.Assert.NotNull(surfboard, "the surfboard was loaded");
			Board board = Board.Wrap(surfboard.gameObject);
			t.Assert.True(Mathf.Abs(board.Mass(SimHashes.SaltWater) - 6f) < 0.05f, "the tank holds its Salt Water (" + board.Mass(SimHashes.SaltWater) + " kg)");
			GameObject chunk = board.Tank.items.First(i => i.GetComponent<PrimaryElement>().ElementID == SimHashes.SaltWater);
			t.Assert.True(chunk.HasTag(SurfWater.Tag), "the loaded chunk carries the shared tag");
			t.Assert.True(board.Ready(), "the loaded board is ready");
			board.Consumer.alwaysConsume = true;
			board.Surf(2f);
			yield return t.WaitGameSeconds(1.5);
			t.Assert.True(Mathf.Abs(board.Mass(SimHashes.SaltWater) - 5.9f) < 0.02f, "a work tick spills 0.1 kg of it (" + board.Mass(SimHashes.SaltWater) + " kg left)");
			board.Feed(SimHashes.Water, 2f);
			yield return t.WaitGameSeconds(2.5);
			t.Assert.True(board.Mass(SimHashes.Water) > 1.9f, "and the loaded intake still takes Water (" + board.Mass(SimHashes.Water) + " kg)");
		}
	}
}
