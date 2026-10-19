namespace Bitshifter
{
	/// <summary>
	/// The mod's text, registered under the game's STRINGS tree by Isochronous.Translations
	/// (ModStrings.BUILDINGS.X is the key STRINGS.BUILDINGS.X, which is how the game finds a
	/// building's name) and replaced from translations/&lt;locale&gt;.po. Code reads the fields
	/// when it needs the text, so a translation loaded later is what the player sees.
	/// </summary>
	public static class ModStrings
	{
		private static readonly string Green = STRINGS.UI.FormatAsAutomationState("Green Signal", STRINGS.UI.AutomationState.Active);
		private static readonly string Red = STRINGS.UI.FormatAsAutomationState("Red Signal", STRINGS.UI.AutomationState.Standby);
		private static readonly string Ribbon = STRINGS.UI.FormatAsLink("Automation Ribbon", "LOGICRIBBON");

		public static class BUILDINGS
		{
			public static class PREFABS
			{
				public static class RIBBONDIAGNOSTICPANEL
				{
					public static LocString NAME = STRINGS.UI.FormatAsLink("Ribbon Diagnostic Panel", "RIBBONDIAGNOSTICPANEL");
					public static LocString DESC = "Seeing every bit of a ribbon at once makes a circuit quick to trace.";
					public static LocString EFFECT =
						"Shows all 31 bits of an " + Ribbon + " on a grid of lamps: a lamp lights green for each bit carrying a " + Green + ".\n" +
						"\n" +
						"With its output wired, it is also a 31-channel switch. Bits switched on light purple, and the output carries every bit that is on at the input or switched on.";

					public static LocString LOGIC_PORT = "Ribbon Input";
					public static LocString LOGIC_PORT_ACTIVE = "Lights a green lamp for each bit carrying a " + Green;
					public static LocString LOGIC_PORT_INACTIVE = "Otherwise, the bit's lamp stays dark";
					public static LocString LOGIC_PORT_OUTPUT = "Ribbon Output";
					public static LocString OUTPUT_PORT_ACTIVE = "Sends a " + Green + " on each bit that is on at the input or switched on";
					public static LocString OUTPUT_PORT_INACTIVE = "Otherwise, sends a " + Red + " on that bit";
				}
			}
		}

		public static class BITSHIFTER
		{
			public static class DIAGNOSTICPANEL
			{
				// Status items. {0} is a list of bits, {1} the ribbon's value as a number.
				public static LocString STATUS_INPUT = "Input: {0}";
				public static LocString STATUS_INPUT_TOOLTIP = "Bits on the input ribbon carrying a " + Green + "\nRibbon value: {1}";
				public static LocString STATUS_SWITCHES = "Switches: {0}";
				public static LocString STATUS_SWITCHES_TOOLTIP = "Bits switched on in the side screen. The output carries these and every input bit.";

				// A list of bits: "1, 3, 9-12", or "none".
				public static LocString BITS_NONE = "none";
				public static LocString BITS_SEPARATOR = ", ";
				public static LocString BITS_RANGE = "{0}-{1}";

				// The side screen. {0} is a bit number, 1-31.
				public static LocString SIDE_SCREEN_TITLE = "Switches";
				public static LocString SIDE_SCREEN_WIRED = "Switched bits join the input on the output.";
				public static LocString SIDE_SCREEN_UNWIRED = "Wire the output port to use the switches.";
				public static LocString CELL_ON = "Bit {0}: switched on. Click to switch it off.";
				public static LocString CELL_OFF = "Bit {0}: switched off. Click to switch it on.";
				public static LocString CELL_INPUT = "The input has bit {0} on.";
				public static LocString CELL_UNWIRED = "Bit {0}. Wire the output port to use the switches.";
				public static LocString ALL_OFF = "All off";
				public static LocString ALL_OFF_TOOLTIP = "Switch every bit off";
			}
		}
	}
}
