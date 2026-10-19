namespace Bitshifter
{
	/// <summary>
	/// The ribbon toolkit: buildings for 31-bit ribbons that Bitshifter adds next to its wider
	/// Reader and Writer. Their patches carry <see cref="PatchCategory"/> and are applied in
	/// OnLoad on their own, so the buildings stay available when the mod leaves the Reader and
	/// Writer to another mod. Each building has one row in <see cref="Buildings"/>.
	/// </summary>
	internal static class Toolkit
	{
		/// <summary>The Harmony category of every toolkit patch.</summary>
		public const string PatchCategory = "BitshifterToolkit";

		/// <summary>The build menu category the toolkit's buildings go in.</summary>
		public const string PlanCategory = "Automation";

		/// <summary>A toolkit building: its id, build menu subcategory, the building it follows there, and the tech that unlocks it.</summary>
		public sealed class Entry
		{
			public readonly string Id;
			public readonly string Subcategory;
			public readonly string After;
			public readonly string Tech;

			public Entry(string id, string subcategory, string after, string tech)
			{
				Id = id;
				Subcategory = subcategory;
				After = after;
				Tech = tech;
			}
		}

		public static readonly Entry[] Buildings =
		{
			// Automation > wires, right after the vanilla Ribbon Writer, unlocked with it.
			new Entry(RibbonDiagnosticPanelConfig.ID, "wires", LogicRibbonWriterConfig.ID, "ParallelAutomation"),
		};
	}
}
