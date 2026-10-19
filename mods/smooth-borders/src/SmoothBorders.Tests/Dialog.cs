using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isochronous.TestKit;
using PeterHan.PLib.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OPTIONS = SmoothBorders.ModStrings.SMOOTHBORDERS.OPTIONS;

namespace SmoothBorders.Tests
{
	/// <summary>
	/// The options dialog as a player first sees it (no config file yet): the intro text first and
	/// set top-left, then the four kinds with their ranges in the labels, the game's default as the
	/// tooltip, and sliders from 1 to each kind's maximum standing at the defaults.
	/// </summary>
	public static class Dialog
	{
		private static readonly string[] Labels = { "Primary asteroids (1-16)", "Small asteroids (1-7)", "Moonlets (1-6)", "Tiny moonlets (1-3)" };
		private static readonly string[] Tooltips = { "Game default: 4.", "Game default: 3.", "Game default: 2.", "Game default: 1." };
		private static readonly int[] Defaults = { 4, 3, 2, 1 };
		private static readonly int[] Maxima = { 16, 7, 6, 3 };

		/// <summary>The open dialog's parts, top to bottom.</summary>
		private sealed class Shown
		{
			public KScreen Screen;
			public TMP_Text Intro;
			public List<LocText> Labels;
			public List<TMP_InputField> Fields;
			public List<Slider> Sliders;
		}

		private static float Top(Component c)
		{
			var corners = new Vector3[4];
			((RectTransform)c.transform).GetWorldCorners(corners);
			return corners[1].y;
		}

		/// <summary>
		/// Opens the dialog with no config file (the harness puts the player's back at the end of the
		/// run), waits for it to lay out, and finds its parts; it is closed again at cleanup.
		/// </summary>
		private static IEnumerator Open(TestContext t, Shown shown)
		{
			string config = Support.ConfigPath;
			if (File.Exists(config))
				File.Delete(config);
			POptions.ShowDialog(typeof(Options));
			yield return t.WaitRealSeconds(1.5);
			// PLib names each label's object after the entry: "Label" for the rows. The intro is a PTextArea
			// named "Intro", and its text is the TextMeshPro child called "Text".
			shown.Intro = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).FirstOrDefault(text => text.isActiveAndEnabled
				&& text.gameObject.name == "Text" && text.transform.parent != null && text.transform.parent.parent != null
				&& text.transform.parent.parent.name == "Intro");
			t.Assert.NotNull(shown.Intro, "the dialog's intro text");
			shown.Screen = shown.Intro.GetComponentInParent<KScreen>();
			t.Cleanup(() =>
			{
				if (shown.Screen != null)
					shown.Screen.Deactivate();
			});
			t.Assert.NotNull(shown.Screen, "the dialog's screen");
			GameObject root = shown.Screen.gameObject;
			shown.Labels = root.GetComponentsInChildren<LocText>()
				.Where(text => text.transform.parent != null && text.transform.parent.name == "Label")
				.OrderByDescending(Top).ToList();
			shown.Fields = root.GetComponentsInChildren<TMP_InputField>().Where(field => !field.readOnly).OrderByDescending(Top).ToList();
			shown.Sliders = root.GetComponentsInChildren<Slider>().OrderByDescending(Top).ToList();
		}

		[OniTest("SB: the options dialog lists the four kinds with their ranges, the game's default as each tooltip", NoColony = true, Timeout = 30), Layer(2)]
		public static IEnumerator LabelsAndTooltips(TestContext t)
		{
			var shown = new Shown();
			yield return Open(t, shown);
			t.Log("labels: " + string.Join(" | ", shown.Labels.Select(label => label.text)));
			t.Assert.Eq(4, shown.Labels.Count, "four rows");
			for (int i = 0; i < Labels.Length; i++)
			{
				t.Assert.Eq(Labels[i], shown.Labels[i].text, "row " + (i + 1) + "'s label");
				ToolTip tip = shown.Labels[i].transform.parent.GetComponent<ToolTip>();
				t.Assert.NotNull(tip, "row " + (i + 1) + " has a tooltip");
				t.Assert.Eq(Tooltips[i], tip.multiStringCount == 1 ? tip.GetMultiString(0) : "(" + tip.multiStringCount + " strings)", "row " + (i + 1) + "'s tooltip");
			}
		}

		[OniTest("SB: the options dialog's intro comes first, says what the settings do, and wraps with every line at the left edge", NoColony = true, Timeout = 30), Layer(2)]
		public static IEnumerator IntroTopLeft(TestContext t)
		{
			var shown = new Shown();
			yield return Open(t, shown);
			yield return t.Screenshot("dialog");
			t.Assert.Eq((string)OPTIONS.INTRO, shown.Intro.text, "the intro text");
			t.Assert.True(shown.Labels.Count > 0, "the dialog has rows");
			t.Assert.True(Top(shown.Intro) > Top(shown.Labels[0]), "the intro sits above the first row");

			shown.Intro.ForceMeshUpdate();
			TMP_TextInfo info = shown.Intro.textInfo;
			t.Log("intro: " + info.lineCount + " line(s)");
			t.Assert.True(info.lineCount > 1, "the intro wraps inside the dialog");
			shown.Labels[0].ForceMeshUpdate();
			float rowLeft = LineLeft(shown.Labels[0], 0);
			// A few pixels for glyph bearings; a centred line is off by half its spare width.
			float slack = 4f * shown.Intro.transform.lossyScale.x;
			var off = new List<string>();
			for (int line = 0; line < info.lineCount; line++)
			{
				float left = LineLeft(shown.Intro, line);
				if (Mathf.Abs(left - rowLeft) > slack)
					off.Add("line " + (line + 1) + " starts " + ((left - rowLeft) / shown.Intro.transform.lossyScale.x).ToString("F0") + " px from the rows' left edge");
			}
			t.Assert.Eq(0, off.Count, "every line of the intro starts where the rows' labels start: " + string.Join("; ", off));
		}

		/// <summary>Where a line of text starts on screen (world x of its first visible character's left edge).</summary>
		private static float LineLeft(TMP_Text text, int line)
		{
			TMP_TextInfo info = text.textInfo;
			TMP_LineInfo lineInfo = info.lineInfo[line];
			TMP_CharacterInfo first = info.characterInfo[lineInfo.firstVisibleCharacterIndex];
			return text.transform.TransformPoint(first.bottomLeft).x;
		}

		[OniTest("SB: the options dialog's sliders run from 1 to each kind's maximum and stand at the defaults 4, 3, 2 and 1", NoColony = true, Timeout = 30), Layer(2)]
		public static IEnumerator SlidersAndDefaults(TestContext t)
		{
			var shown = new Shown();
			yield return Open(t, shown);
			t.Log("fields: " + string.Join(", ", shown.Fields.Select(field => field.text)) + "; sliders: "
				+ string.Join(", ", shown.Sliders.Select(slider => slider.minValue + "-" + slider.maxValue + " at " + slider.value)));
			t.Assert.Eq(4, shown.Fields.Count, "four number fields");
			t.Assert.Eq(4, shown.Sliders.Count, "four sliders");
			for (int i = 0; i < Defaults.Length; i++)
			{
				t.Assert.Eq(Defaults[i].ToString(), shown.Fields[i].text, "row " + (i + 1) + "'s field shows the default");
				t.Assert.Eq(1f, shown.Sliders[i].minValue, "row " + (i + 1) + "'s slider starts at 1");
				t.Assert.Eq((float)Maxima[i], shown.Sliders[i].maxValue, "row " + (i + 1) + "'s slider ends at the kind's maximum");
				t.Assert.Eq((float)Defaults[i], shown.Sliders[i].value, "row " + (i + 1) + "'s slider stands at the default");
			}
		}
	}
}
