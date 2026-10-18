using System.Collections;
using System.Collections.Generic;
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

		/// <summary>
		/// An Ovagro seed held at the cell, as a Pip holds one in its mouth: it does not fall. A seed
		/// plants at the cell it is in, and a loose seed under a ceiling would drop to the floor, so
		/// the seed is made without its faller (the game's own switch for things that should not fall).
		/// </summary>
		public static GameObject HeldSeed(TestContext t, int cell)
		{
			GameObject go = Util.KInstantiate(Assets.GetPrefab("VineMotherSeed"), Grid.CellToPosCBC(cell, Grid.SceneLayer.Ore));
			go.GetComponent<Pickupable>().handleFallerComponents = false;
			go.SetActive(true);
			t.Cleanup(() => { if (go != null) Util.KDestroyGameObject(go); });
			return go;
		}

		/// <summary>An adult Pip standing at the cell, removed at cleanup.</summary>
		public static GameObject Pip(TestContext t, int cell)
		{
			return Spawn(t, SquirrelConfig.ID, cell);
		}

		/// <summary>
		/// Yield it: waits until the Pip's path grid reaches the cell, and fails at once with a clear
		/// message when it does not, instead of letting a later wait time out.
		/// </summary>
		public static IEnumerator AssertReaches(TestContext t, GameObject pip, int cell, string what)
		{
			Navigator nav = pip.GetComponent<Navigator>();
			yield return t.WaitUntil(() => nav.CanReach(cell), 10, "the Pip at " + Cells.Describe(Grid.PosToCell(pip)) + " to reach " + what + " at " + Cells.Describe(cell));
		}

		/// <summary>The plant (or building) registered on the building layer at the cell, or null.</summary>
		public static GameObject PlantAt(int cell)
		{
			return Grid.IsValidCell(cell) ? Grid.Objects[cell, (int)ObjectLayer.Building] : null;
		}

		public static bool Is(GameObject go, Tag prefabId)
		{
			return go != null && go.PrefabID() == prefabId;
		}

		/// <summary>The branches of one side of a vine, from the one beside the mother outwards.</summary>
		public static List<VineBranch.Instance> Chain(GameObject first)
		{
			var chain = new List<VineBranch.Instance>();
			GameObject go = first;
			while (go != null && chain.Count < 32)
			{
				VineBranch.Instance smi = go.GetSMI<VineBranch.Instance>();
				if (smi == null)
					break;
				chain.Add(smi);
				go = smi.Branch;
			}
			return chain;
		}

		/// <summary>"(x, y) Shape" for each branch, for the log.</summary>
		public static string Describe(List<VineBranch.Instance> chain)
		{
			var parts = new List<string>();
			foreach (VineBranch.Instance b in chain)
				parts.Add(Cells.Describe(Grid.PosToCell(b.gameObject)) + " " + b.MyShape + (b.IsGrown ? " grown" : ""));
			return parts.Count == 0 ? "none" : string.Join(", ", parts.ToArray());
		}
	}
}
