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
		private static IEnumerator Open(TestContext t, Shown shown, bool fresh = true)
		{
			string config = Support.ConfigPath;
			if (fresh && File.Exists(config))
				File.Delete(config);
			POptions.ShowDialog(typeof(Options));
			yield return t.WaitRealSeconds(1.5);
			// PLib names each label's object after the entry: "Label" for the rows. The intro is a PTextArea
			// named "Intro", and its text is the TextMeshPro child called "Text".
			shown.Intro = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).FirstOrDefault(text => text.isActiveAndEnabled
				&& text.gameObject.name == "Text" && text.transform.parent != null && text.transform.parent.parent != null
				&& text.transform.parent.parent.name == "Intro");
			t.Assert.NotNull(shown.Intro, "the dialog's intro text");
			// The intro's input field carries a KScreen of its own; the dialog is PLib's "ModOptions" screen above it.
			KScreen[] screens = shown.Intro.GetComponentsInParent<KScreen>();
			shown.Screen = screens.FirstOrDefault(screen => screen.gameObject.name == "ModOptions") ?? screens.LastOrDefault();
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
			// A TextMeshPro input field ends its text with a zero-width space.
			t.Assert.Eq((string)OPTIONS.INTRO, shown.Intro.text.TrimEnd('\u200B'), "the intro text");
			t.Assert.True(shown.Labels.Count > 0, "the dialog has rows");
			t.Assert.True(Top(shown.Intro) > Top(shown.Labels[0]), "the intro sits above the first row");

			shown.Intro.ForceMeshUpdate();
			TMP_TextInfo info = shown.Intro.textInfo;
			t.Log("intro: " + info.lineCount + " line(s)");
			t.Assert.True(info.lineCount > 1, "the intro wraps inside the dialog");
			shown.Labels[0].ForceMeshUpdate();
			float px = shown.Intro.transform.lossyScale.x;
			float rowLeft = LineLeft(shown.Labels[0], 0);
			float introLeft = LineLeft(shown.Intro, 0);
			// Left-aligned: every line starts at the first line's left edge, give or take a glyph bearing
			// (a centred line would be off by half its spare width).
			var off = new List<string>();
			for (int line = 1; line < info.lineCount; line++)
			{
				float left = LineLeft(shown.Intro, line);
				if (Mathf.Abs(left - introLeft) > 4f * px)
					off.Add("line " + (line + 1) + " starts " + ((left - introLeft) / px).ToString("F0") + " px from line 1");
			}
			float indent = (introLeft - rowLeft) / px;
			t.Log("intro lines start " + indent.ToString("F0") + " px right of the rows' labels");
			t.Assert.Eq(0, off.Count, "every line of the intro starts at one left edge: " + string.Join("; ", off));
			// The intro has a little padding of its own; it still lines up with the rows below, not centred.
			t.Assert.True(indent >= -2f && indent <= 12f, "the intro's left edge is within the padding of the rows' left edge (" + indent.ToString("F0") + " px)");

			// A thin rule between the intro and the first field, as wide as the intro block
			Transform block = shown.Intro.transform.parent.parent.parent;
			t.Assert.Eq("IntroBlock", block.name, "the intro sits in its block");
			Transform rule = block.Find("Spacer");
			t.Assert.NotNull(rule, "the rule exists");
			var ruleBox = new Vector3[4];
			((RectTransform)rule).GetWorldCorners(ruleBox);
			Image ruleImage = rule.GetComponent<Image>();
			t.Assert.NotNull(ruleImage, "the rule is drawn");
			float ruleTop = ruleBox[1].y;
			float ruleHeight = (ruleBox[1].y - ruleBox[0].y) / px;
			t.Log("rule: " + ruleHeight.ToString("F1") + " px high, " + ((ruleBox[2].x - ruleBox[0].x) / px).ToString("F0") + " px wide");
			t.Assert.True(ruleHeight >= 0.9f && ruleHeight <= 2.1f, "the rule is thin (" + ruleHeight.ToString("F1") + " px)");
			t.Assert.True(ruleTop < Top(shown.Intro.transform.parent) , "the rule is below the intro's text area");
			t.Assert.True(ruleBox[0].y > Top(shown.Fields[0]), "the rule is above the first field");
			RectTransform blockBox = (RectTransform)block;
			var bc = new Vector3[4];
			blockBox.GetWorldCorners(bc);
			t.Assert.True(ruleBox[2].x - ruleBox[0].x >= (bc[2].x - bc[0].x) * 0.9f, "the rule spans the intro block's width");

			// The intro's area (the input field's "Text Area" viewport) against the text it holds.
			RectTransform area = (RectTransform)shown.Intro.transform.parent;
			var corners = new Vector3[4];
			area.GetWorldCorners(corners);
			float scale = shown.Intro.transform.lossyScale.y;
			TMP_LineInfo last = info.lineInfo[info.lineCount - 1];
			float textBottom = shown.Intro.transform.TransformPoint(new Vector3(0f, last.descender, 0f)).y;
			float spare = (textBottom - corners[0].y) / scale;
			t.Log("intro area " + ((corners[1].y - corners[0].y) / scale).ToString("F0") + " px high; the text's last line ends "
				+ spare.ToString("F0") + " px above the area's bottom (negative: cut off)");
			t.Assert.True(spare >= -1f, "the whole intro fits its area (last line " + (-spare).ToString("F0") + " px below the bottom)");
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

		private static readonly int[] Chosen = { 7, 5, 4, 2 };

		private static void Click(Shown shown, string name)
		{
			KButton button = shown.Screen.GetComponentsInChildren<KButton>().FirstOrDefault(b => b.gameObject.name == name);
			if (button == null)
				throw new System.InvalidOperationException("no button named " + name);
			button.SignalClick(KKeyCode.Mouse0);
		}

		private static string Fields(Shown shown)
		{
			return string.Join(",", shown.Fields.Select(field => field.text));
		}

		[OniTest("SB: values set in the options dialog are saved on OK, shown on reopen, and Reset to default puts back 4, 3, 2 and 1", NoColony = true, Timeout = 60), Layer(2)]
		public static IEnumerator SavesAndResets(TestContext t)
		{
			var shown = new Shown();
			yield return Open(t, shown);
			for (int i = 0; i < Chosen.Length; i++)
				shown.Sliders[i].value = Chosen[i];
			yield return t.WaitRealSeconds(0.5);
			t.Assert.Eq("7,5,4,2", Fields(shown), "the fields follow the sliders");
			Click(shown, "ok");
			yield return t.WaitRealSeconds(1.0);
			Options saved = Options.Load();
			t.Log("config after OK: " + saved.Primary + "," + saved.Small + "," + saved.Moonlets + "," + saved.TinyMoonlets);
			t.Assert.Eq(7, saved.Primary, "Primary written on OK");
			t.Assert.Eq(5, saved.Small, "Small written on OK");
			t.Assert.Eq(4, saved.Moonlets, "Moonlets written on OK");
			t.Assert.Eq(2, saved.TinyMoonlets, "TinyMoonlets written on OK");

			var again = new Shown();
			yield return Open(t, again, false);
			t.Log("fields on reopen: " + Fields(again));
			t.Assert.Eq("7,5,4,2", Fields(again), "the saved values show on reopen");
			t.Assert.Eq(7f, again.Sliders[0].value, "the first slider stands at the saved value");
			Click(again, "ResetConfig");
			yield return t.WaitRealSeconds(0.5);
			t.Log("fields after Reset to default: " + Fields(again));
			t.Assert.Eq("4,3,2,1", Fields(again), "Reset to default shows 4, 3, 2 and 1");
			for (int i = 0; i < Defaults.Length; i++)
				t.Assert.Eq((float)Defaults[i], again.Sliders[i].value, "row " + (i + 1) + "'s slider is back at the default");
		}
	}
}
