using System.Collections;
using System.Linq;
using Isochronous.TestKit;

namespace StoragePodRedux.Tests
{
	/// <summary>Where the two buildings appear in the game's own lists: defs, build menu, research.</summary>
	public static class Registration
	{
		[OniTest("Storage Pod Redux: both buildings are registered under the original mod's prefab IDs", Timeout = 20), Layer(0)]
		public static IEnumerator Registered(TestContext t)
		{
			t.Assert.Eq("StoragePodConfig", StoragePodConfig.ID, "the Storage Pod keeps the original mod's id");
			t.Assert.Eq("CoolPodConfig", CoolPodConfig.ID, "the Cool Pod keeps the original mod's id");
			t.Assert.NotNull(Assets.GetBuildingDef(StoragePodConfig.ID), "the Storage Pod's def");
			t.Assert.NotNull(Assets.GetBuildingDef(CoolPodConfig.ID), "the Cool Pod's def");
			t.Assert.False(StoragePodReduxMod.OriginalActive, "the original mod is not loaded in this run");
			yield break;
		}

		[OniTest("Storage Pod Redux: the pods sit beside the Storage Bin and the Refrigerator in the build menu", Timeout = 20), Layer(0)]
		public static IEnumerator BuildMenuPlaces(TestContext t)
		{
			t.Assert.Eq(1, Position("Base", StoragePodConfig.ID) - Position("Base", StorageLockerConfig.ID), "the Storage Pod follows the Storage Bin in Base");
			t.Assert.Eq(1, Position("Food", CoolPodConfig.ID) - Position("Food", RefrigeratorConfig.ID), "the Cool Pod follows the Refrigerator in Food");
			var storage = TUNING.BUILDINGS.PLANORDER.First(p => p.category == "Base").buildingAndSubcategoryData;
			t.Assert.Eq("storage", storage[Position("Base", StoragePodConfig.ID)].Value, "in the storage subcategory");
			yield break;
		}

		private static int Position(string category, string id)
		{
			var list = TUNING.BUILDINGS.PLANORDER.First(p => p.category == category).buildingAndSubcategoryData;
			int index = list.FindIndex(pair => pair.Key == id);
			if (index < 0)
				throw new OniAssertException(id + " is not in the " + category + " build menu");
			return index;
		}

		[OniTest("Storage Pod Redux: research unlocks the Storage Pod with Refined Renovations and the Cool Pod with Agriculture", Timeout = 20), Layer(0)]
		public static IEnumerator Research(TestContext t)
		{
			Tech refined = Db.Get().Techs.TryGet("RefinedObjects");
			Tech agriculture = Db.Get().Techs.TryGet("Agriculture");
			t.Assert.NotNull(refined, "the tech the Storage Pod hangs on exists");
			t.Assert.NotNull(agriculture, "the tech the Cool Pod hangs on exists");
			t.Assert.True(refined.unlockedItemIDs.Contains(StoragePodConfig.ID), "Refined Renovations unlocks the Storage Pod");
			t.Assert.True(agriculture.unlockedItemIDs.Contains(CoolPodConfig.ID), "Agriculture unlocks the Cool Pod");
			t.Assert.Eq(1, refined.unlockedItemIDs.Count(id => id == StoragePodConfig.ID), "listed once");
			yield break;
		}
	}
}
