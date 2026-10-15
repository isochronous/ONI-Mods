using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The presets of the tab showing, at the right of the window's top row: pick one and load or
	/// delete it, or save the tab's choices under a name. A preset is saved from and loaded into its
	/// own tab, and the one picked is remembered per tab.
	/// </summary>
	internal sealed class PresetBar
	{
		private readonly UpgradePlan plan;
		private readonly UpgradeDialog dialog;
		private readonly Dictionary<PlanMode, PlanPreset> picked = new Dictionary<PlanMode, PlanPreset>();
		private GameObject nameField;

		public PresetBar(UpgradePlan plan, UpgradeDialog dialog)
		{
			this.plan = plan;
			this.dialog = dialog;
		}

		/// <summary>The preset picked on the tab showing; the tab's first when none is, or the one picked is gone.</summary>
		private PlanPreset Picked
		{
			get
			{
				List<PlanPreset> presets = Presets.For(plan.Mode);
				if (!picked.TryGetValue(plan.Mode, out PlanPreset preset) || !presets.Contains(preset))
					picked[plan.Mode] = preset = presets.Count > 0 ? presets[0] : null;
				return preset;
			}
			set => picked[plan.Mode] = value;
		}

		public void AddTo(PPanel row)
		{
			row.AddChild(new PLabel("PresetLabel")
			{
				Text = ModStrings.UPGRADEPLANNER.PresetLabel,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
			});
			PlanPreset current = Picked;
			if (current == null)
				row.AddChild(new PLabel("NoPresets")
				{
					Text = ModStrings.UPGRADEPLANNER.NoPresets,
					TextStyle = PUITuning.Fonts.TextDarkStyle,
					TextAlignment = TextAnchor.MiddleLeft,
				});
			else
			{
				row.AddChild(new PComboBox<PlanPreset>("PresetChoice")
				{
					Content = Presets.For(plan.Mode),
					InitialItem = current,
					MinWidth = 180,
					TextStyle = PUITuning.Fonts.TextLightStyle,
					OnOptionSelected = (_, choice) => Picked = choice,
				}.SetKleiBlueStyle());
				row.AddChild(Widgets.StripButton("PresetLoad", ModStrings.UPGRADEPLANNER.PresetLoad, ModStrings.UPGRADEPLANNER.PresetLoadTooltip, Load));
				row.AddChild(Widgets.StripButton("PresetDelete", ModStrings.UPGRADEPLANNER.PresetDelete, ModStrings.UPGRADEPLANNER.PresetDeleteTooltip, Delete));
			}
			row.AddChild(new PSpacer { PreferredSize = new Vector2(12f, 0f) });
			row.AddChild(new PTextField("PresetName")
			{
				PlaceholderText = ModStrings.UPGRADEPLANNER.PresetNamePlaceholder,
				MinWidth = 150,
				MaxLength = 60,
				TextStyle = PUITuning.Fonts.TextDarkStyle,
			}.AddOnRealize(go => nameField = go));
			row.AddChild(Widgets.StripButton("PresetSave", ModStrings.UPGRADEPLANNER.PresetSave, ModStrings.UPGRADEPLANNER.PresetSaveTooltip, Save));
		}

		private void Save()
		{
			string name = nameField != null ? PTextField.GetText(nameField)?.Trim() : null;
			if (string.IsNullOrEmpty(name))
			{
				dialog.Notify(ModStrings.UPGRADEPLANNER.PresetNeedsName);
				return;
			}
			PlanPreset preset = plan.ToPreset(name);
			bool replaced = Presets.Put(preset);
			Picked = preset;
			dialog.Rebuild((replaced ? ModStrings.UPGRADEPLANNER.PresetUpdated : ModStrings.UPGRADEPLANNER.PresetSaved).Replace("{0}", name));
		}

		private void Load()
		{
			PlanPreset preset = Picked;
			if (preset == null)
				return;
			plan.Load(preset);
			dialog.Rebuild(ModStrings.UPGRADEPLANNER.PresetLoaded.Replace("{0}", preset.Name));
		}

		private void Delete()
		{
			PlanPreset preset = Picked;
			if (preset == null)
				return;
			Presets.Remove(preset);
			Picked = null;
			dialog.Rebuild(ModStrings.UPGRADEPLANNER.PresetDeleted.Replace("{0}", preset.Name));
		}
	}
}
