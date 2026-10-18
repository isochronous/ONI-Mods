using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace OvagroOverhead.Tests
{
	/// <summary>
	/// The vines of a ceiling Ovagro: they grow out of the mother along the underside of the ceiling,
	/// each branch drawn hanging from it, and turn the corner where the ceiling ends, as a floor
	/// Ovagro's vines lie along the floor. A wild vine grows a branch in about 40 minutes, so the
	/// tests set each branch's maturity to full and wait for the next (the game checks every four
	/// seconds).
	/// </summary>
	public static class Vines
	{
		/// <summary>Sets maturity to full on every branch of each side until that side has its count.</summary>
		private static IEnumerator GrowChains(TestContext t, VineMother.Instance mother, int left, int right, float realSeconds)
		{
			float start = Time.realtimeSinceStartup;
			while (true)
			{
				List<VineBranch.Instance> l = Kit.Chain(mother.LeftBranch);
				List<VineBranch.Instance> r = Kit.Chain(mother.RightBranch);
				if (l.Count >= left && r.Count >= right)
					yield break;
				if (Time.realtimeSinceStartup - start > realSeconds)
					t.Assert.Fail("the vine did not grow " + left + " branches left and " + right + " right; left: " + Kit.Describe(l) + "; right: " + Kit.Describe(r));
				if (l.Count < left)
					foreach (VineBranch.Instance b in l)
						Kit.ForceMature(b.gameObject);
				if (r.Count < right)
					foreach (VineBranch.Instance b in r)
						Kit.ForceMature(b.gameObject);
				yield return t.WaitGameSeconds(1);
			}
		}

		/// <summary>
		/// The mother has no maturity amount: its state machine plays its grow animation once, marks
		/// itself grown, and then tries for a branch on each side every four seconds.
		/// </summary>
		private static IEnumerator GrowMother(TestContext t, GameObject mother)
		{
			VineMother.Instance smi = mother.GetSMI<VineMother.Instance>();
			float start = Time.realtimeSinceStartup;
			yield return t.WaitUntil(() => smi.LeftBranch != null && smi.RightBranch != null, 90, "the mother to grow and put out a branch on each side");
			t.Log("the mother put out its branches after " + (Time.realtimeSinceStartup - start).ToString("0") + " s");
		}

		private static void AssertAt(TestContext t, VineBranch.Instance b, int cell, VineBranch.Shape shape, string which)
		{
			t.Assert.Eq(Cells.Describe(cell), Cells.Describe(Grid.PosToCell(b.gameObject)), which + " is at the expected cell");
			t.Assert.Eq(shape, b.MyShape, which + " has shape " + shape);
		}

		private static void AssertRooted(TestContext t, List<VineBranch.Instance> chain, string side)
		{
			for (int i = 0; i < chain.Count; i++)
			{
				GameObject go = chain[i].gameObject;
				t.Assert.True(go != null && !go.HasTag(GameTags.Uprooted), side + " branch " + (i + 1) + " is rooted");
			}
		}

		/// <summary>
		/// The strip runs from three cells left of the mother to the far right of the box, so the left
		/// side has three branches under the ceiling and a fourth that turns its end; the right side
		/// is checked for three branches under the ceiling.
		/// </summary>
		[OniTest("VINES: a ceiling Ovagro's vines grow along the underside of the ceiling and turn its end", Timeout = 200, Boxed = true), Layer(0)]
		public static IEnumerator AlongTheCeiling(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			int y = inside.H - 2;
			const int mx = 20;
			yield return Kit.NaturalCeiling(t, inside, mx - 3, inside.W - 3, y);
			int motherCell = inside.Cell(mx, y - 1);
			GameObject mother = Kit.Spawn(t, CeilingForm.PlantId, motherCell, Grid.SceneLayer.BuildingFront);
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(mother, Kit.PlantAt(motherCell), "the ceiling mother hangs at its cell");

			yield return GrowMother(t, mother);
			VineMother.Instance smi = mother.GetSMI<VineMother.Instance>();
			yield return GrowChains(t, smi, 4, 3, 90);
			yield return t.WaitGameSeconds(5);

			List<VineBranch.Instance> left = Kit.Chain(smi.LeftBranch);
			List<VineBranch.Instance> right = Kit.Chain(smi.RightBranch);
			t.Log("left: " + Kit.Describe(left));
			t.Log("right: " + Kit.Describe(right));
			t.Assert.Eq(VineBranch.Shape.Top, left[0].RootShape, "the first left branch takes the ceiling as its root's shape");
			t.Assert.Eq(VineBranch.Shape.Top, right[0].RootShape, "the first right branch takes the ceiling as its root's shape");
			for (int i = 0; i < 3; i++)
			{
				AssertAt(t, left[i], inside.Cell(mx - 1 - i, y - 1), VineBranch.Shape.Top, "left branch " + (i + 1));
				AssertAt(t, right[i], inside.Cell(mx + 1 + i, y - 1), VineBranch.Shape.Top, "right branch " + (i + 1));
			}
			AssertAt(t, left[3], inside.Cell(mx - 4, y - 1), VineBranch.Shape.OutCornerTopRight, "left branch 4, past the ceiling's end,");
			t.Assert.True(mother != null && !mother.HasTag(GameTags.Uprooted), "the mother is rooted");
			AssertRooted(t, left, "left");
			AssertRooted(t, right, "right");

			// A branch draws nothing until it grows; grow the corner so the picture shows the turn.
			Kit.ForceMature(left[3].gameObject);
			yield return t.WaitGameSeconds(6);
			t.Log("left after the corner grew: " + Kit.Describe(Kit.Chain(smi.LeftBranch)));
			t.Assert.Eq(VineBranch.Shape.OutCornerTopRight, left[3].MyShape, "the grown corner keeps its shape");
			AssertRooted(t, Kit.Chain(smi.LeftBranch), "left");

			yield return t.LookAt(inside.Cell(mx - 1, y - 1), 5f);
			yield return t.Screenshot("ceiling-vine");
			yield return t.LookAt(inside.Cell(mx, y - 1), 9f);
			yield return t.Screenshot("ceiling-vine-wide");
		}

		[OniTest("VINES: a floor Ovagro's vines still lie along the floor", Timeout = 120, Boxed = true), Layer(0)]
		public static IEnumerator AlongTheFloor(TestContext t)
		{
			t.Speed(3);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(2);
			const int mx = 20;
			yield return Kit.NaturalFloor(t, inside, mx - 6, mx + 6, 0);
			int motherCell = inside.Cell(mx, 1);
			GameObject mother = Kit.Spawn(t, VineMotherConfig.ID, motherCell, Grid.SceneLayer.BuildingFront);
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(mother, Kit.PlantAt(motherCell), "the floor mother stands at its cell");

			yield return GrowMother(t, mother);
			VineMother.Instance smi = mother.GetSMI<VineMother.Instance>();
			yield return t.WaitGameSeconds(3);
			List<VineBranch.Instance> left = Kit.Chain(smi.LeftBranch);
			List<VineBranch.Instance> right = Kit.Chain(smi.RightBranch);
			t.Log("left: " + Kit.Describe(left) + "; right: " + Kit.Describe(right));
			t.Assert.Eq(VineBranch.Shape.Bottom, left[0].RootShape, "the first left branch takes the floor as its root's shape");
			t.Assert.Eq(VineBranch.Shape.Bottom, right[0].RootShape, "the first right branch takes the floor as its root's shape");
			AssertAt(t, left[0], inside.Cell(mx - 1, 1), VineBranch.Shape.Bottom, "left branch 1");
			AssertAt(t, right[0], inside.Cell(mx + 1, 1), VineBranch.Shape.Bottom, "right branch 1");
		}
	}
}
