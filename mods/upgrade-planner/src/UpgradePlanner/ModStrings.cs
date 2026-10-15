namespace UpgradePlanner
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations and
	/// replaced from translations/&lt;locale&gt;.po. The tool's name and tooltip and the window
	/// button's label are under UI.TOOLS.UPGRADEPLANNER, the keys the game looks them up by; the
	/// rest is the mod's own, under UPGRADEPLANNER. Code reads the fields when it needs the text,
	/// so a translation loaded later is what the player sees.
	/// </summary>
	public static class ModStrings
	{
		/// <summary>The window toggle's label is a game label that looks its text up by key.</summary>
		public const string ToggleKey = "STRINGS.UI.TOOLS.UPGRADEPLANNER.TOGGLE";

		public static class UI
		{
			public static class TOOLS
			{
				public static class UPGRADEPLANNER
				{
					public static LocString NAME = "Upgrade Planner";
					public static LocString TOOLTIP = "Pick buildings and the material to rebuild them in, or a type and what to replace it with, then box-drag: every matching building in the area is marked for replacement";
					public static LocString TOGGLE = "Upgrade Planner";
					// Key-binding screen (options -> game -> controls -> Mods).
					public static LocString TOOLNAME = "Upgrade Planner tool";
				}
			}
		}

		public static class UPGRADEPLANNER
		{
			public static LocString DialogTitle = "Upgrade Planner";
			public static LocString ReplaceHeading = "Replace";
			public static LocString WithHeading = "With";
			public static LocString MaterialHeading = "Material";
			public static LocString AnyMaterial = "Any";
			public static LocString AnyMaterialTooltip = "Replace these buildings whatever they are made of";
			public static LocString SpecificMaterial = "Specific";
			public static LocString SpecificMaterialTooltip = "Replace only the ones built from the materials picked below";
			/// <summary>Appended to a type's tooltip when one build menu entry stands for several defs; {0} is their number.</summary>
			public static LocString VariantsTooltip = " ({0} variants behind one build menu entry; the material picks which)";
			// The With side's card until the Replace side is complete: the steps, the next one marked.
			public static LocString HowToHeading = "How it works";
			public static LocString StepPickSource = "Under Replace, pick the type of building to replace.";
			public static LocString StepPickSourceMaterial = "Choose Any material, or Specific and pick which.";
			public static LocString StepPickTarget = "Pick what to replace it with here, and its material.";
			public static LocString StepApply = "Apply, then box-drag over the buildings to mark them.";
			public static LocString ChooseTarget = "Choose what to replace it with.";
			public static LocString Clear = "Clear";
			public static LocString ClearTooltip = "Forget every choice and start over";
			public static LocString NoTargets = "Nothing can be built over this.";
			public static LocString NoMaterials = "No usable material for this ingredient.";

			public static LocString Apply = "Apply";
			public static LocString ApplyTooltip = "Close the window and keep the tool: box-drag to mark buildings. The button by the priority bar opens it again";
			public static LocString ConfigButtonTooltip = "Open the Upgrade Planner window to change what is replaced with what";

			// Tabs and presets
			public static LocString TabMaterial = "Change material";
			public static LocString TabMaterialTooltip = "Rebuild buildings of the picked types in another material";
			public static LocString TabType = "Replace type";
			public static LocString TabTypeTooltip = "Replace one type of building with another, in the materials you pick";
			public static LocString PresetLabel = "Preset";
			public static LocString NoPresets = "No saved presets";
			public static LocString PresetLoad = "Load";
			public static LocString PresetLoadTooltip = "Replace this tab's choices with the picked preset";
			public static LocString PresetDelete = "Delete";
			public static LocString PresetDeleteTooltip = "Forget the picked preset";
			public static LocString PresetNamePlaceholder = "Preset name";
			public static LocString PresetSave = "Save current plan";
			public static LocString PresetSaveTooltip = "Save this tab's choices under the name typed beside it; this tab's preset with that name is replaced";
			public static LocString PresetNeedsName = "Type a name for the preset first.";
			public static LocString PresetSaved = "Saved {0}.";
			public static LocString PresetUpdated = "Updated {0}.";
			public static LocString PresetLoaded = "Loaded {0}.";
			public static LocString PresetDeleted = "Deleted {0}.";

			// Change material
			public static LocString MaterialsHeading = "Materials";
			public static LocString BuildingsHeading = "Buildings";
			public static LocString MaterialAnyTooltip = "Replace the picked buildings whatever they are made of";
			public static LocString MaterialSpecificTooltip = "Replace only the picked buildings made of the material picked below";
			public static LocString ClearMaterialTooltip = "Forget this tab's choices and start over";
			public static LocString SelectAllTooltip = "Pick every building that can use these materials";
			public static LocString AllBuildings = "All buildings";
			public static LocString AllBuildingsTooltip = "Also offer the buildings the game does not let you build over. Those are deconstructed and rebuilt in the new material, out of service until rebuilt; Blueprints Expanded keeps their settings";
			public static LocString ExpandAll = "Expand all";
			public static LocString CollapseAll = "Collapse all";
			public static LocString CategoryCount = "{0} of {1}";
			public static LocString IncludeTooltip = "Click to include";
			public static LocString LeaveOutTooltip = "Click to leave out";
			public static LocString BlockedResearch = "Not researched yet";
			public static LocString BlockedSource = "Never built from {0}";
			public static LocString BlockedTarget = "Can't be built in {0}";
			public static LocString AnyMaterialLower = "any material";
			public static LocString MaterialPromptSource = "Pick the material to replace, or choose Any.";
			public static LocString MaterialPromptTarget = "Pick the material to build in.";
			public static LocString MaterialSameMaterial = "Replace and With are the same material. Pick a different one.";
			public static LocString MaterialPromptTypes = "Pick at least one building that can be built in {0}.";
			/// <summary>{0} the material replaced, {1} the new one, {2} the types.</summary>
			public static LocString MaterialSummary = "Replace {0} with {1} in {2}.";
			// The Replace type tab's material card before a type is picked and with Any, then the
			// Change material tab's Replace card with Any.
			public static LocString SourceMaterialPrompt = "Pick the type of building to replace above.";
			public static LocString SourceAnyContent = "Replaces every building of the chosen type, made from any material";
			public static LocString MaterialAnyContent = "Replaces the selected buildings, made from any material, with the material chosen below";

			public static LocString ErrorIncomplete = "Finish the Upgrade Planner choices first";
			public static LocString HoverActionIdle = "CHOOSE AN UPGRADE";
			public static LocString HoverActionReady = "MARK FOR UPGRADE";
			public static LocString HoverActionApplyFirst = "APPLY SETTINGS FIRST";
			public static LocString ErrorNotApplied = "Apply settings first";
			// The popup over the cursor after a drag: {0} the number marked (or replaced, with instant
			// build); PopupSkipped adds {1} the number skipped to that text.
			public static LocString PopupMarked = "{0} marked for upgrade";
			public static LocString PopupReplaced = "{0} replaced";
			public static LocString PopupSkipped = "{0}, {1} skipped";

			// Families of buildings the game lets you build over one another.
			public static LocString FamilyLiquidPipes = "Liquid Pipes";
			public static LocString FamilyVentilationPipes = "Ventilation Pipes";
			public static LocString FamilyConveyorRails = "Conveyor Rails";
			public static LocString FamilyWires = "Wires";
			public static LocString FamilyAutomationWires = "Automation Wires";
			public static LocString FamilyTiles = "Tiles";
			public static LocString FamilyLadders = "Ladders";
			public static LocString FamilyTransitTubes = "Transit Tubes";
			public static LocString FamilyBackwalls = "Backwalls";

			// Lists of names: "A, B and C", or "A, B, C, D and 7 more".
			public static LocString ListSeparator = ", ";
			/// <summary>{0} the names but the last, {1} the last.</summary>
			public static LocString ListAnd = "{0} and {1}";
			/// <summary>{0} the names shown, {1} how many more.</summary>
			public static LocString ListAndMore = "{0} and {1} more";

			// The Replace type tab's footer: {0} a type, {1} its materials.
			public static LocString SummaryNothing = "Nothing chosen yet.";
			public static LocString DescribeAnyMaterial = "{0} (any material)";
			public static LocString DescribeMaterials = "{0} ({1})";
			/// <summary>{0} the type replaced (with its materials), {1} what replaces it.</summary>
			public static LocString DescribeReplace = "{0} -> {1}";
		}
	}
}
