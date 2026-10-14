using System.Reflection;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The button that opens the planner's window while the tool is active: one of the toggles the
	/// top-right menu (Vitals, Consumables...) is made of, hung off the left edge of the priority
	/// bar, outside the bar's own layout so the bar stays as it is, and going wherever the bar goes.
	/// It shows pressed while the window is open, as that menu shows an open screen.
	/// </summary>
	internal sealed class WindowToggle
	{
		private static readonly FieldInfo MenuTogglePrefab = typeof(KIconToggleMenu).GetField("prefab", BindingFlags.NonPublic | BindingFlags.Instance);

		private GameObject button;
		private KToggle toggle;

		/// <summary>Puts the button by the priority bar; <paramref name="onClick"/> runs on a click.</summary>
		public void Show(System.Action onClick)
		{
			Hide();
			ManagementMenu menu = ManagementMenu.Instance;
			PriorityScreen bar = ToolMenu.Instance != null ? ToolMenu.Instance.PriorityScreen : null;
			KToggle prefab = menu != null && MenuTogglePrefab != null ? MenuTogglePrefab.GetValue(menu) as KToggle : null;
			if (prefab == null || bar == null)
				return;
			button = Util.KInstantiateUI(prefab.gameObject, bar.gameObject, force_active: true);
			button.name = "UpgradePlannerToggle";
			button.AddOrGet<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
			RectTransform rect = button.rectTransform();
			rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
			rect.pivot = new Vector2(1f, 0.5f);
			rect.anchoredPosition = new Vector2(-12f, 0f);
			float width = 160f;
			toggle = button.GetComponent<KToggle>();
			if (toggle != null)
			{
				toggle.group = null;
				LocText label = toggle.GetComponentInChildren<LocText>();
				if (label != null)
				{
					// The prefab's label looks its text up by key when it spawns: our key, our text.
					label.key = ModStrings.ToggleKey;
					label.SetText(ModStrings.UI.TOOLS.UPGRADEPLANNER.TOGGLE);
					// The menu's layout sizes its toggles to their content; alone, this one does the
					// sum itself: padding, the icon, the spacing, the text and its container's padding.
					width = label.preferredWidth + 56f;
				}
				Sprite sprite = Assets.GetSprite(ToolIcon.SpriteName);
				if (toggle.fgImage != null && sprite != null)
					toggle.fgImage.sprite = sprite;
				toggle.onClick += () => onClick();
			}
			// The prefab's alert badge (the menu shows it for a screen that wants attention) is not ours.
			button.transform.Find("AlertIcon")?.gameObject.SetActive(false);
			rect.sizeDelta = new Vector2(width, 32f);
			button.AddOrGet<ToolTip>().SetSimpleTooltip(ModStrings.UPGRADEPLANNER.ConfigButtonTooltip);
		}

		public void Hide()
		{
			if (button != null)
				Util.KDestroyGameObject(button);
			button = null;
			toggle = null;
		}

		/// <summary>Pressed while the window is open, the way ManagementMenu.ToggleScreen shows an open screen.</summary>
		public void SetPressed(bool pressed)
		{
			if (toggle == null)
				return;
			toggle.isOn = pressed;
			toggle.ActivateFlourish(pressed);
			ImageToggleState state = toggle.GetComponentInChildren<ImageToggleState>();
			if (state == null)
				return;
			if (pressed)
				state.SetActive();
			else
				state.SetInactive();
		}
	}
}
