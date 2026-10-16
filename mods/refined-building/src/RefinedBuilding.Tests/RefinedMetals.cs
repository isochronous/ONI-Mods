using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Isochronous.TestKit;
using UnityEngine;

namespace RefinedBuilding.Tests
{
	/// <summary>
	/// The mod adds the Metal tag to every solid refined metal the game loaded, so every building that
	/// asks for raw Metal offers them. Asked of the game's own element table and of the call the build
	/// menu's material list makes (MaterialSelector.GetValidMaterials); no Duplicants needed.
	/// </summary>
	public static class RefinedMetals
	{
		private static readonly SimHashes[] Named = { SimHashes.Copper, SimHashes.Iron, SimHashes.Gold, SimHashes.Aluminum, SimHashes.Lead };

		[OniTest("Refined Building: every solid refined metal carries the Metal tag, once", Timeout = 20), Layer(0)]
		public static IEnumerator EveryRefinedMetalIsTagged(TestContext t)
		{
			var refined = ElementLoader.elements.Where(e => e != null && e.IsSolid && e.HasTag(GameTags.RefinedMetal)).ToList();
			t.Log(refined.Count + " solid refined metals: " + string.Join(", ", refined.Select(e => e.id.ToString())));
			t.Assert.True(refined.Count >= 5, "the game has refined metals");
			foreach (Element e in refined)
				t.Assert.Eq(1, e.oreTags.Count(tag => tag == GameTags.Metal), e.id + " carries the Metal tag exactly once");
			foreach (SimHashes id in Named)
				t.Assert.True(ElementLoader.FindElementByHash(id).HasTag(GameTags.Metal), id + " counts as a metal");
			// Steel has both tags in the base game and is left as it is.
			Element steel = ElementLoader.FindElementByHash(SimHashes.Steel);
			t.Assert.Eq(1, steel.oreTags.Count(tag => tag == GameTags.Metal), "steel's Metal tag is not duplicated");
			yield break;
		}

		[OniTest("Refined Building: minerals, fuels, liquids and gases did not gain the Metal tag; raw ores keep it", Timeout = 20), Layer(0)]
		public static IEnumerator OthersUntouched(TestContext t)
		{
			foreach (SimHashes id in new[] { SimHashes.SandStone, SimHashes.Granite, SimHashes.Ice, SimHashes.Dirt, SimHashes.Carbon })
				t.Assert.False(ElementLoader.FindElementByHash(id).HasTag(GameTags.Metal), id + " is not a metal");
			// Raw ores keep theirs.
			t.Assert.True(ElementLoader.FindElementByHash(SimHashes.Cuprite).HasTag(GameTags.Metal), "copper ore is still a metal");
			foreach (SimHashes id in new[] { SimHashes.Water, SimHashes.Oxygen, SimHashes.CarbonDioxide })
				t.Assert.False(ElementLoader.FindElementByHash(id).HasTag(GameTags.Metal), id + " is not a metal");
			yield break;
		}

		[OniTest("Refined Building: a loose refined metal pile counts as Metal for deliveries", Timeout = 20), Layer(0)]
		public static IEnumerator PileIsMetal(TestContext t)
		{
			CellRect r = t.Rect(4, 2);
			GameObject pile = t.Dupes.Drop(SimHashes.Copper, 50, r.Cell(1, 0));
			yield return t.WaitGameSeconds(0.5);
			KPrefabID id = pile.GetComponent<KPrefabID>();
			t.Assert.True(id.HasTag(GameTags.Metal), "a copper pile is delivered wherever any Metal is asked for (oxygen mask station)");
			t.Assert.True(id.HasTag(GameTags.RefinedMetal), "and is still a refined metal for the buildings that ask for that");
		}

		/// <summary>One building per kind of material request; the request itself is asserted so a wrong pick shows.</summary>
		private static List<Tag> Offered(TestContext t, string buildingId, string expectedRequest)
		{
			BuildingDef def = Assets.GetBuildingDef(buildingId);
			Tag request = def.CraftRecipe.Ingredients[0].tag;
			t.Assert.Eq(expectedRequest, request.ToString(), buildingId + " asks for");
			List<Tag> offered = MaterialSelector.GetValidMaterials(request);
			t.Log(buildingId + " (" + request + ") offers " + offered.Count + " materials");
			return offered;
		}

		[OniTest("Refined Building: a building that asks for Metal (wire) offers refined metals beside the ores", Timeout = 20), Layer(0)]
		public static IEnumerator MetalRequest(TestContext t)
		{
			List<Tag> offered = Offered(t, "Wire", "Metal");
			foreach (SimHashes id in Named)
				t.Assert.True(offered.Contains(id.CreateTag()), "the list has " + id);
			t.Assert.True(offered.Contains(SimHashes.Cuprite.CreateTag()), "and still the ore");
			t.Assert.Eq(offered.Count, offered.Distinct().Count(), "no material is listed twice");
			yield break;
		}

		[OniTest("Refined Building: a building that asks for minerals or metal (gas pipe) offers refined metals too", Timeout = 20), Layer(0)]
		public static IEnumerator MineralsOrMetalRequest(TestContext t)
		{
			List<Tag> offered = Offered(t, "GasConduit", "BuildableRaw&Metal");
			t.Assert.True(offered.Contains(SimHashes.Copper.CreateTag()), "copper is offered");
			t.Assert.True(offered.Contains(SimHashes.SandStone.CreateTag()), "and the minerals still are");
			t.Assert.Eq(offered.Count, offered.Distinct().Count(), "no material is listed twice");
			yield break;
		}

		[OniTest("Refined Building: buildings that ask for minerals or for Refined Metal are untouched", Timeout = 20), Layer(0)]
		public static IEnumerator OtherRequestsUnchanged(TestContext t)
		{
			List<Tag> minerals = Offered(t, "Tile", "BuildableRaw");
			t.Assert.False(minerals.Contains(SimHashes.Copper.CreateTag()), "a plain tile is not offered copper");
			t.Assert.True(minerals.Contains(SimHashes.SandStone.CreateTag()), "but sandstone");
			List<Tag> refinedOnly = Offered(t, "MetalTile", "RefinedMetal");
			t.Assert.True(refinedOnly.Contains(SimHashes.Copper.CreateTag()), "a metal tile is offered copper as before");
			t.Assert.False(refinedOnly.Contains(SimHashes.Cuprite.CreateTag()), "and no ore");
			yield break;
		}

		[OniTest("Refined Building: the metal refinery and rock crusher gain no recipe that turns a metal into itself", Timeout = 20), Layer(0)]
		public static IEnumerator RecipesUnchanged(TestContext t)
		{
			int checkedRecipes = 0;
			foreach (ComplexRecipe recipe in ComplexRecipeManager.Get().recipes)
			{
				if (!recipe.fabricators.Contains("MetalRefinery".ToTag()) && !recipe.fabricators.Contains("RockCrusher".ToTag()))
					continue;
				checkedRecipes++;
				foreach (var result in recipe.results)
					t.Assert.False(recipe.ingredients.Any(i => i.material == result.material), "no recipe makes " + result.material + " from itself");
			}
			t.Log(checkedRecipes + " recipes checked");
			t.Assert.True(checkedRecipes > 0, "the refinery and crusher have recipes");
			yield break;
		}
	}
}
