namespace UpgradePlanner
{
	public static class ModStrings
	{
		public const string ToolNameKey = "STRINGS.UI.TOOLS.UPGRADEPLANNER.NAME";
		public const string ToolTooltipKey = "STRINGS.UI.TOOLS.UPGRADEPLANNER.TOOLTIP";

		public const string ToolName = "Upgrade Planner";
		public const string ToolTooltip = "Pick a building type and what to replace it with, then box-drag: every matching building in the area is marked for replacement";

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
		public const string ChooseSourceFirst = "Choose what to replace first.";
		public const string ChooseMaterialModeFirst = "Any material, or specific ones? Choose under Replace first.";
		public const string PickSourceMaterialFirst = "Pick the material to replace under Replace first.";
		public const string NoTargets = "Nothing can be built over this.";
		public const string NoMaterials = "No usable material for this ingredient.";

		public const string Apply = "Apply";
		public const string ApplyTooltip = "Close the window and keep the tool: box-drag to mark buildings. The button by the priority picker opens it again";
		public const string ConfigButton = "Upgrade Planner Config";
		public const string ConfigButtonTooltip = "Open the Upgrade Planner window to change what is replaced with what";

		public const string ErrorIncomplete = "Finish the Upgrade Planner choices first";
		public const string HoverActionIdle = "CHOOSE AN UPGRADE";
		public const string HoverActionReady = "MARK FOR UPGRADE";

		public static void Register()
		{
			Strings.Add(ToolNameKey, ToolName);
			Strings.Add(ToolTooltipKey, ToolTooltip);
		}
	}
}
