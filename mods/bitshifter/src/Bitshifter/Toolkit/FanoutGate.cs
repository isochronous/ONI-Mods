using KSerialization;
using UnityEngine;
using GATE = Bitshifter.ModStrings.BITSHIFTER.FANOUTGATE;

namespace Bitshifter
{
	/// <summary>
	/// The Fan-out Gate. While its 1-bit input carries a Green Signal, its ribbon output carries
	/// every bit the player picked in the side screen; otherwise the output is 0. An unwired input
	/// counts as off.
	///
	/// The side screen is the vanilla bit selector (LogicBitSelectorSideScreen), which finds the
	/// gate through ILogicRibbonBitSelector. Picking a row toggles that bit, and
	/// ToolkitPatches.LogicBitSelectorSideScreen_RefreshToggles_Patch highlights every picked row
	/// where the vanilla screen would highlight one.
	///
	/// The art has no tints: each look is an anim of its own. "off" with neither port wired,
	/// "on_1_1111" while the input is on and "on_0_0000" while it is off. The game raises no event
	/// when a wire is added to or removed from a port, so Render200ms polls the wiring, as the
	/// Ribbon Diagnostic Panel does.
	/// </summary>
	[SerializationConfig(MemberSerialization.OptIn)]
	public sealed class FanoutGate : KMonoBehaviour, ILogicRibbonBitSelector, IRender200ms
	{
		public static readonly HashedString InputPort = new HashedString("FanoutGateInput");
		public static readonly HashedString OutputPort = new HashedString("FanoutGateOutput");

		/// <summary>The bits the gate can send: 1-31 (0-based 0-30).</summary>
		public const int Mask = (int)((1L << BitshifterMod.Bits) - 1);

		public const string AnimOff = "off";
		public const string AnimInputOn = "on_1_1111";
		public const string AnimInputOff = "on_0_0000";

		private static readonly System.Action<object, object> OnLogicValueChangedDelegate =
			(context, data) => ((FanoutGate)context).OnLogicValueChanged(data);
		private static readonly System.Action<object, object> OnCopySettingsDelegate =
			(context, data) => ((FanoutGate)context).OnCopySettings(data);

#pragma warning disable CS0169, CS0649 // set by the game; CopyBuildingSettings is there for the copy settings tool
		[MyCmpAdd]
		private CopyBuildingSettings copyBuildingSettings;
		[MyCmpGet]
		private LogicPorts ports;
		[MyCmpGet]
		private KBatchedAnimController kbac;
#pragma warning restore CS0169, CS0649

		[Serialize]
		private int mask;

		private string drawnAnim;

		/// <summary>The picked bits, bit 0 for bit 1.</summary>
		public int Bits => mask;

		public bool InputWired => ports != null && ports.IsPortConnected(InputPort);

		public bool OutputWired => ports != null && ports.IsPortConnected(OutputPort);

		/// <summary>Whether the input carries a Green Signal; an unwired input does not.</summary>
		public bool InputOn => InputWired && LogicCircuitNetwork.IsBitActive(0, ports.GetInputValue(InputPort));

		public int OutputValue => ports != null ? ports.GetOutputValue(OutputPort) : 0;

		public bool IsPicked(int bit) => bit >= 0 && bit < BitshifterMod.Bits && (mask & (1 << bit)) != 0;

		public void SetBits(int bits)
		{
			mask = bits & Mask;
			SendOutput();
			Redraw();
		}

		public void TogglePicked(int bit)
		{
			if (bit < 0 || bit >= BitshifterMod.Bits)
				return;
			SetBits(mask ^ (1 << bit));
		}

		/// <summary>The anim the gate should be playing now.</summary>
		public string Anim()
		{
			if (!InputWired && !OutputWired)
				return AnimOff;
			return InputOn ? AnimInputOn : AnimInputOff;
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate, this);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			Subscribe((int)GameHashes.LogicEvent, OnLogicValueChangedDelegate, this);
			SendOutput();
			Redraw();
		}

		public void Render200ms(float dt)
		{
			// Catches a wire cut from the input, which changes the input without an event.
			if (OutputValue != Output())
				SendOutput();
			Redraw();
		}

		private void OnLogicValueChanged(object data)
		{
			// The output port raises the same event when its own value changes.
			if (((LogicValueChanged)data).portID != InputPort)
				return;
			SendOutput();
			Redraw();
		}

		private void OnCopySettings(object data)
		{
			FanoutGate source = data is GameObject go ? go.GetComponent<FanoutGate>() : null;
			if (source != null)
				SetBits(source.Bits);
		}

		private int Output() => InputOn ? mask : 0;

		private void SendOutput()
		{
			if (ports != null)
				ports.SendSignal(OutputPort, Output());
		}

		private void Redraw()
		{
			if (kbac == null)
				return;
			string anim = Anim();
			if (anim == drawnAnim)
				return;
			drawnAnim = anim;
			kbac.Play(anim);
		}

		// ILogicRibbonBitSelector, for the vanilla bit selector side screen.

		public string SideScreenTitle => "STRINGS.BITSHIFTER.FANOUTGATE.SIDE_SCREEN_TITLE";

		public string SideScreenDescription => GATE.SIDE_SCREEN_TITLE;

		/// <summary>A click on a row: toggles that bit. Bit 32 (a row left from a 32-bit Reader of Digital CPU) is ignored.</summary>
		public void SetBitSelection(int bit)
		{
			TogglePicked(bit);
		}

		/// <summary>The lowest picked bit, or -1; the screen's single highlight is replaced by the patch.</summary>
		public int GetBitSelection()
		{
			for (int bit = 0; bit < BitshifterMod.Bits; bit++)
			{
				if (IsPicked(bit))
					return bit;
			}
			return -1;
		}

		public int GetBitDepth() => BitshifterMod.Bits;

		public bool SideScreenDisplayWriterDescription() => false;

		public bool SideScreenDisplayReaderDescription() => false;

		/// <summary>Whether the output ribbon carries the bit, as the Ribbon Writer answers.</summary>
		public bool IsBitActive(int bit) => bit >= 0 && bit < BitshifterMod.Bits && (OutputValue & (1 << bit)) != 0;

		public int GetOutputValue() => OutputValue;

		public int GetInputValue() => InputWired ? ports.GetInputValue(InputPort) : 0;

		public void UpdateVisuals()
		{
			Redraw();
		}
	}
}
