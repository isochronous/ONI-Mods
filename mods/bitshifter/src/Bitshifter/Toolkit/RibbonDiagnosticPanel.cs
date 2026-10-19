using System.Text;
using KSerialization;
using UnityEngine;
using PANEL = Bitshifter.ModStrings.BITSHIFTER.DIAGNOSTICPANEL;

namespace Bitshifter
{
	/// <summary>
	/// The Ribbon Diagnostic Panel. Its face has one lamp per bit (symbols bit_bloom_1 to
	/// bit_bloom_31, laid out 8 x 4 like the side screen): green while that bit of the input
	/// ribbon is on, purple while the player has switched it on and the output is wired, hidden
	/// otherwise (the body draws a dark socket under every lamp). The output always carries
	/// input OR switches, wired or not, so a wire added later takes the right value at once; only
	/// the lamps and the side screen ignore the switches while nothing reads the output.
	///
	/// The game raises no event when a wire is added to or removed from a port, so Render200ms
	/// polls the wiring, as the vanilla Ribbon Reader and Writer do in UpdateVisuals. Lamp tints are
	/// not saved by the game; the first redraw after loading puts them back.
	/// </summary>
	[SerializationConfig(MemberSerialization.OptIn)]
	public sealed class RibbonDiagnosticPanel : KMonoBehaviour, IRender200ms
	{
		public static readonly HashedString InputPort = new HashedString("RibbonDiagnosticPanelInput");
		public static readonly HashedString OutputPort = new HashedString("RibbonDiagnosticPanelOutput");

		/// <summary>The bits the switches can set: 1-31 (0-based 0-30).</summary>
		public const int Mask = int.MaxValue;

		/// <summary>The vanilla Ribbon Reader's and Writer's green (their colorOn).</summary>
		public static readonly Color Green = new Color(29f / 85f, 37f / 51f, 0.36862746f);
		/// <summary>The side screen colour of a bit switched on.</summary>
		public static readonly Color Purple = new Color(0.608f, 0.349f, 0.816f);
		/// <summary>The lamp tint of a bit switched on: bluer than <see cref="Purple"/>, as the bloom pass pulls it toward pink.</summary>
		public static readonly Color LampPurple = new Color(0.416f, 0.122f, 0.839f);

		public enum LampState
		{
			Off,
			Input,
			Switch,
		}

		private static readonly KAnimHashedString[] Lamps = MakeLamps();

		private static readonly EventSystem.IntraObjectHandler<RibbonDiagnosticPanel> OnLogicValueChangedDelegate =
			new EventSystem.IntraObjectHandler<RibbonDiagnosticPanel>((component, data) => component.OnLogicValueChanged(data));
		private static readonly EventSystem.IntraObjectHandler<RibbonDiagnosticPanel> OnCopySettingsDelegate =
			new EventSystem.IntraObjectHandler<RibbonDiagnosticPanel>((component, data) => component.OnCopySettings(data));

		private static StatusItem inputStatus;
		private static StatusItem switchesStatus;

#pragma warning disable CS0169, CS0649 // set by the game; CopyBuildingSettings is there for the copy settings tool
		[MyCmpAdd]
		private CopyBuildingSettings copyBuildingSettings;
		[MyCmpGet]
		private LogicPorts ports;
		[MyCmpGet]
		private KBatchedAnimController kbac;
		[MyCmpGet]
		private KSelectable selectable;
#pragma warning restore CS0169, CS0649

		[Serialize]
		private int switches;

		private bool drawn;
		private int drawnInput;
		private int drawnSwitches;
		private bool drawnWired;

		/// <summary>The switched-on bits, bit 0 for bit 1.</summary>
		public int Switches => switches;

		/// <summary>The input ribbon's value, or 0 while no ribbon is wired to the input.</summary>
		public int InputValue => ports != null && ports.IsPortConnected(InputPort) ? ports.GetInputValue(InputPort) : 0;

		public bool OutputWired => ports != null && ports.IsPortConnected(OutputPort);

		public int OutputValue => ports != null ? ports.GetOutputValue(OutputPort) : 0;

		public bool IsSwitchOn(int bit) => (switches & (1 << bit)) != 0;

		public void SetSwitch(int bit, bool on)
		{
			SetSwitches(on ? switches | (1 << bit) : switches & ~(1 << bit));
		}

		public void ToggleSwitch(int bit)
		{
			SetSwitch(bit, !IsSwitchOn(bit));
		}

		public void SetSwitches(int mask)
		{
			switches = mask & Mask;
			SendOutput();
			Redraw(false);
		}

		/// <summary>What the lamp of a bit (0-based) shows: purple wins over green, and switches count only while the output is wired.</summary>
		public LampState Lamp(int bit)
		{
			return LampOf(bit, InputValue, OutputWired);
		}

		private LampState LampOf(int bit, int input, bool wired)
		{
			if (wired && IsSwitchOn(bit))
				return LampState.Switch;
			return (input & (1 << bit)) != 0 ? LampState.Input : LampState.Off;
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			Subscribe(-905833192, OnCopySettingsDelegate);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			MakeStatusItems();
			Subscribe(-801688580, OnLogicValueChangedDelegate);
			kbac.Play("on");
			SendOutput();
			Redraw(true);
			selectable.ToggleStatusItem(inputStatus, true, this);
		}

		public void Render200ms(float dt)
		{
			// Catches a wire cut from the input, which changes InputValue without an event.
			if (OutputValue != Output())
				SendOutput();
			Redraw(false);
		}

		private void OnLogicValueChanged(object data)
		{
			// The output port raises the same event when its own value changes.
			if (((LogicValueChanged)data).portID != InputPort)
				return;
			SendOutput();
			Redraw(false);
		}

		private void OnCopySettings(object data)
		{
			RibbonDiagnosticPanel source = data is GameObject go ? go.GetComponent<RibbonDiagnosticPanel>() : null;
			if (source != null)
				SetSwitches(source.Switches);
		}

		/// <summary>Input OR switches, with the input's whole value: a 32nd bit (Digital CPU's ribbons) passes through.</summary>
		private int Output() => InputValue | switches;

		private void SendOutput()
		{
			if (ports != null)
				ports.SendSignal(OutputPort, Output());
		}

		private void Redraw(bool force)
		{
			if (kbac == null)
				return;
			int input = InputValue;
			bool wired = OutputWired;
			if (!force && drawn && input == drawnInput && switches == drawnSwitches && wired == drawnWired)
				return;
			drawn = true;
			drawnInput = input;
			drawnSwitches = switches;
			drawnWired = wired;
			for (int bit = 0; bit < Lamps.Length; bit++)
			{
				LampState lamp = LampOf(bit, input, wired);
				kbac.SetSymbolVisiblity(Lamps[bit], lamp != LampState.Off);
				if (lamp != LampState.Off)
					kbac.SetSymbolTint(Lamps[bit], lamp == LampState.Switch ? LampPurple : Green);
			}
			if (selectable != null && switchesStatus != null)
				selectable.ToggleStatusItem(switchesStatus, wired, this);
		}

		/// <summary>
		/// Built on the first spawn rather than at load, since a status item reads its name and
		/// tooltip when it is made and the language is settled by then.
		/// </summary>
		private static void MakeStatusItems()
		{
			if (inputStatus != null)
				return;
			inputStatus = new StatusItem("RibbonDiagnosticPanelInput", PANEL.STATUS_INPUT, PANEL.STATUS_INPUT_TOOLTIP, "",
				StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID,
				resolve_string_callback: (text, data) => string.Format(text, DescribeBits(((RibbonDiagnosticPanel)data).InputValue)));
			inputStatus.resolveTooltipCallback = (text, data) =>
			{
				int value = ((RibbonDiagnosticPanel)data).InputValue;
				return string.Format(text, DescribeBits(value), value);
			};
			switchesStatus = new StatusItem("RibbonDiagnosticPanelSwitches", PANEL.STATUS_SWITCHES, PANEL.STATUS_SWITCHES_TOOLTIP, "",
				StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID,
				resolve_string_callback: (text, data) => string.Format(text, DescribeBits(((RibbonDiagnosticPanel)data).Switches)));
		}

		/// <summary>The bits 1-31 that are on, 1-based, with runs of three or more as ranges: "1, 3, 9-12"; "none" when none are.</summary>
		public static string DescribeBits(int value)
		{
			var text = new StringBuilder();
			int bit = 0;
			while (bit < BitshifterMod.Bits)
			{
				if ((value & (1 << bit)) == 0)
				{
					bit++;
					continue;
				}
				int end = bit;
				while (end + 1 < BitshifterMod.Bits && (value & (1 << (end + 1))) != 0)
					end++;
				if (text.Length > 0)
					text.Append((string)PANEL.BITS_SEPARATOR);
				if (end - bit >= 2)
					text.Append(string.Format(PANEL.BITS_RANGE, bit + 1, end + 1));
				else
				{
					text.Append(bit + 1);
					if (end > bit)
						text.Append((string)PANEL.BITS_SEPARATOR).Append(end + 1);
				}
				bit = end + 1;
			}
			return text.Length > 0 ? text.ToString() : (string)PANEL.BITS_NONE;
		}

		private static KAnimHashedString[] MakeLamps()
		{
			var lamps = new KAnimHashedString[BitshifterMod.Bits];
			for (int bit = 0; bit < lamps.Length; bit++)
				lamps[bit] = new KAnimHashedString("bit_bloom_" + (bit + 1));
			return lamps;
		}
	}
}
