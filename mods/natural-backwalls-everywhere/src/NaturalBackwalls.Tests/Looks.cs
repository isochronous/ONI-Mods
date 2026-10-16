using System.Collections;
using Isochronous.TestKit;
using PeterHan.PLib.Options;
using UnityEngine;
using UnityEngine.UI;

namespace NaturalBackwalls.Tests
{
	/// <summary>Screenshots, for looking at; out of the default run.</summary>
	public static class Looks
	{
		[OniTest("looks: the options dialog, top and bottom (Developer options last)", Timeout = 40, Explicit = true, NoColony = true), Layer(0)]
		public static IEnumerator OptionsDialog(TestContext t)
		{
			POptions.ShowDialog(typeof(Options));
			yield return t.WaitRealSeconds(1.5);
			ScrollRect scroll = null;
			foreach (ScrollRect candidate in Object.FindObjectsOfType<ScrollRect>())
				if (candidate.isActiveAndEnabled && candidate.vertical)
					scroll = candidate;
			t.Assert.NotNull(scroll, "the options dialog's scroll area");
			t.Cleanup(() =>
			{
				KScreen dialog = scroll != null ? scroll.GetComponentInParent<KScreen>() : null;
				if (dialog != null)
					dialog.Deactivate();
			});
			yield return t.Screenshot("top");
			// Which scroll area is the dialog's is not worth the search: every open one goes to the bottom.
			foreach (ScrollRect candidate in Object.FindObjectsOfType<ScrollRect>())
				if (candidate.isActiveAndEnabled && candidate.vertical)
					candidate.verticalNormalizedPosition = 0f;
			yield return t.WaitRealSeconds(0.5);
			yield return t.Screenshot("bottom");
		}
	}
}
