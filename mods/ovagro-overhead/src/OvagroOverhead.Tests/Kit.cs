using System.Collections;
using Isochronous.TestKit;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// Helpers the Ovagro Overhead tests share. Tiles the harness builds are Foundation, and the seed's
	/// ground check rejects them, so a test that plants in the wild lays natural sandstone with the
	/// same sim call the Duplicants' box uses for air.
	/// </summary>
	internal static class Kit
	{
		private const float RoomTemperature = 293.15f;

		/// <summary>
		/// Natural sandstone in the cells x0..x1 (inclusive) of row y of the rectangle, written to the
		/// sim. Yield it: it waits for every cell to be solid and not Foundation, then a game second.
		/// </summary>
		public static IEnumerator NaturalRow(TestContext t, CellRect inside, int x0, int x1, int y)
		{
			Element sandstone = ElementLoader.FindElementByHash(SimHashes.SandStone);
			float mass = sandstone.defaultValues.mass;
			for (int x = x0; x <= x1; x++)
			{
				int cell = inside.Cell(x, y);
				SimMessages.ReplaceElement(cell, SimHashes.SandStone, CellEventLogger.Instance.SandBoxTool, mass, RoomTemperature);
			}
			yield return t.WaitUntil(() => IsNaturalRow(inside, x0, x1, y), 5, "natural sandstone in row " + y);
			yield return t.WaitGameSeconds(1);
		}

		/// <summary>A natural ceiling: sandstone in row y of the rectangle (see <see cref="NaturalRow"/>).</summary>
		public static IEnumerator NaturalCeiling(TestContext t, CellRect inside, int x0, int x1, int y)
		{
			return NaturalRow(t, inside, x0, x1, y);
		}

		/// <summary>A natural floor: sandstone in row y of the rectangle (see <see cref="NaturalRow"/>).</summary>
		public static IEnumerator NaturalFloor(TestContext t, CellRect inside, int x0, int x1, int y)
		{
			return NaturalRow(t, inside, x0, x1, y);
		}

		public static bool IsNaturalRow(CellRect inside, int x0, int x1, int y)
		{
			for (int x = x0; x <= x1; x++)
			{
				int cell = inside.Cell(x, y);
				if (!Grid.Solid[cell] || Grid.Foundation[cell] || Grid.Element[cell].id != SimHashes.SandStone)
					return false;
			}
			return true;
		}

		/// <summary>A prefab standing at the cell, removed at cleanup.</summary>
		public static GameObject Spawn(TestContext t, string prefabId, int cell, Grid.SceneLayer layer = Grid.SceneLayer.Creatures)
		{
			GameObject prefab = Assets.GetPrefab(prefabId);
			if (prefab == null)
				throw new OniAssertException("no prefab named " + prefabId);
			GameObject go = Util.KInstantiate(prefab, Grid.CellToPosCBC(cell, layer));
			go.SetActive(true);
			t.Cleanup(() => { if (go != null) Util.KDestroyGameObject(go); });
			return go;
		}

		/// <summary>An Ovagro seed lying at the cell.</summary>
		public static GameObject Seed(TestContext t, int cell)
		{
			return Spawn(t, "VineMotherSeed", cell, Grid.SceneLayer.Ore);
		}

		/// <summary>The plant's maturity set to its maximum.</summary>
		public static void ForceMature(GameObject plant)
		{
			Klei.AI.AmountInstance maturity = Db.Get().Amounts.Maturity.Lookup(plant);
			if (maturity == null)
				throw new OniAssertException(plant.name + " has no maturity amount");
			maturity.SetValue(maturity.GetMax());
		}
	}
}
