namespace UpgradePlanner
{
	public static class ModStrings
	{
		public const string ToolNameKey = "STRINGS.UI.TOOLS.UPGRADEPLANNER.NAME";
		public const string ToolTooltipKey = "STRINGS.UI.TOOLS.UPGRADEPLANNER.TOOLTIP";
		/// <summary>The window toggle's label is a game label that looks its text up by key.</summary>
		public const string ConfigButtonKey = "STRINGS.UI.TOOLS.UPGRADEPLANNER.TOGGLE";

		public const string ToolName = "Upgrade Planner";
		public const string ToolTooltip = "Pick buildings and the material to rebuild them in, or a type and what to replace it with, then box-drag: every matching building in the area is marked for replacement";

		public const string DialogTitle = "Upgrade Planner";
		public const string ReplaceHeading = "Replace";
		public const string WithHeading = "With";
		public const string MaterialHeading = "Material";
		public const string AnyMaterial = "Any";
		public const string AnyMaterialTooltip = "Replace these buildings whatever they are made of";
		public const string SpecificMaterial = "Specific";
		public const string SpecificMaterialTooltip = "Replace only the ones built from the materials picked below";
		/// <summary>Appended to a type's tooltip when one build menu entry stands for several defs; {0} is their number.</summary>
		public const string VariantsTooltip = " ({0} variants behind one build menu entry; the material picks which)";
		// The With side's card until the Replace side is complete: the steps, the next one marked.
		public const string HowToHeading = "How it works";
		public const string StepPickSource = "Under Replace, pick the type of building to replace.";
		public const string StepPickSourceMaterial = "Choose Any material, or Specific and pick which.";
		public const string StepPickTarget = "Pick what to replace it with here, and its material.";
		public const string StepApply = "Apply, then box-drag over the buildings to mark them.";
		public const string ChooseTarget = "Choose what to replace it with.";
		public const string Clear = "Clear";
		public const string ClearTooltip = "Forget every choice and start over";
		public const string NoTargets = "Nothing can be built over this.";
		public const string NoMaterials = "No usable material for this ingredient.";

		public const string Apply = "Apply";
		public const string ApplyTooltip = "Close the window and keep the tool: box-drag to mark buildings. The button by the priority bar opens it again";
		public const string ConfigButton = "Upgrade Planner";
		public const string ConfigButtonTooltip = "Open the Upgrade Planner window to change what is replaced with what";

		// Tabs and presets
		public const string TabMaterial = "Change material";
		public const string TabMaterialTooltip = "Rebuild buildings of the picked types in another material";
		public const string TabType = "Replace type";
		public const string TabTypeTooltip = "Replace one type of building with another, in the materials you pick";
		public const string PresetLabel = "Preset";
		public const string NoPresets = "No saved presets";
		public const string PresetLoad = "Load";
		public const string PresetLoadTooltip = "Replace the current plan with the picked preset";
		public const string PresetDelete = "Delete";
		public const string PresetDeleteTooltip = "Forget the picked preset";
		public const string PresetNamePlaceholder = "Preset name";
		public const string PresetSave = "Save current plan";
		public const string PresetSaveTooltip = "Save this tab's choices under the name typed beside it; a preset with that name is replaced";
		/// <summary>After a Replace type preset's name in the list.</summary>
		public const string PresetTypeSuffix = " (replace type)";
		public const string PresetNeedsName = "Type a name for the preset first.";
		public const string PresetSaved = "Saved {0}.";
		public const string PresetUpdated = "Updated {0}.";
		public const string PresetLoaded = "Loaded {0}.";
		public const string PresetDeleted = "Deleted {0}.";

		// Change material
		public const string MaterialsHeading = "Materials";
		public const string BuildingsHeading = "Buildings";
		public const string MaterialAnyTooltip = "Replace the picked buildings whatever they are made of";
		public const string MaterialSpecificTooltip = "Replace only the picked buildings made of the material picked below";
		public const string ClearMaterialTooltip = "Forget this tab's choices and start over";
		public const string SelectAll = "Select all";
		public const string SelectAllTooltip = "Pick every building that can use these materials";
		public const string ExpandAll = "Expand all";
		public const string CollapseAll = "Collapse all";
		public const string CategoryCount = "{0} of {1}";
		public const string IncludeTooltip = "Click to include";
		public const string LeaveOutTooltip = "Click to leave out";
		public const string BlockedResearch = "Not researched yet";
		public const string BlockedSource = "Never built from {0}";
		public const string BlockedTarget = "Can't be built in {0}";
		public const string AnyMaterialLower = "any material";
		public const string MaterialPromptSource = "Pick the material to replace, or choose Any.";
		public const string MaterialPromptTarget = "Pick the material to build in.";
		public const string MaterialSameMaterial = "Replace and With are the same material. Pick a different one.";
		public const string MaterialPromptTypes = "Pick at least one building that can be built in {0}.";
		/// <summary>{0} the material replaced, {1} the new one, {2} the types.</summary>
		public const string MaterialSummary = "Replace {0} with {1} in {2}.";
		/// <summary>The Replace card's content when Any is chosen in its header.</summary>
		/// <summary>The Replace type tab's material card before a type is picked, and with Any.</summary>
		public const string SourceMaterialPrompt = "Pick the type of building to replace above.";
		public const string SourceAnyContent = "Replaces every building of the chosen type, made from any material";
		public const string MaterialAnyContent = "Replaces the selected buildings, made from any material, with the material chosen below";

		public const string ErrorIncomplete = "Finish the Upgrade Planner choices first";
		public const string HoverActionIdle = "CHOOSE AN UPGRADE";
		public const string HoverActionReady = "MARK FOR UPGRADE";

		public static void Register()
		{
			Strings.Add(ToolNameKey, ToolName);
			Strings.Add(ToolTooltipKey, ToolTooltip);
			Strings.Add(ConfigButtonKey, ConfigButton);
		}
	}
}
