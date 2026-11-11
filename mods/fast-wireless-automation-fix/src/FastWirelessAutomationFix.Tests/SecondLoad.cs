using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Isochronous.TestKit;
using Isochronous.TestRunner;
using UnityEngine;

namespace FastWirelessAutomationFix.Tests
{
	/// <summary>
	/// Fast Wireless Automation+ (Workshop 1722812098) loaded: a switch wired to a wireless emitter,
	/// a wireless receiver on the same channel, a dev generator on a copper wire. Run with
	/// -Steam 1722812098. The mod's types are reached by name only.
	///
	/// The bug: the mod keeps its channels in a static table that its own Game.OnSpawn postfix clears,
	/// which is after a save's buildings are created, so a second load in one session leaves
	/// receivers of the first game registered while the new emitter's EnergyConsumer.OnPrefabInit
	/// signals the channel; the stale receiver throws and the emitter is half initialised.
	/// </summary>
	public static class SecondLoad
	{
		private const string EmitterId = "FastWirelessEmitter";
		private const string ReceiverId = "FastWirelessReceiver";
		private const string ChannelManagerType = "FastWirelessAutomation.Networks.ChannelManager";
		private const string EmitterType = "FastWirelessAutomation.SignalEmitter";
		private const string ReceiverType = "FastWirelessAutomation.SignalReceiver";
		private const int DefaultChannel = 128;
		private const float EmitterWatts = 5f;

		private static string SavePath => Path.Combine(Session.Dir, "fwa-secondload.sav");
		private static string CellsPath => Path.Combine(Session.Dir, "fwa-secondload-cells.txt");

		private static Type Fwa(TestContext t, string name)
		{
			Type type = AccessTools.TypeByName(name);
			t.Assert.NotNull(type, "Fast Wireless Automation+ is loaded (" + name + ")");
			return type;
		}

		private static int ReceiverOutput(GameObject receiver)
		{
			return receiver.GetComponent<LogicPorts>().GetOutputValue(LogicSwitch.PORT_ID);
		}

		/// <summary>
		/// Puts a delegate bound to this receiver into the channel's subscribers a second time. The
		/// harness clears the fixture area right after the test and the receiver's OnCleanUp takes one
		/// registration out, so one is left bound to a destroyed receiver: what the first game's
		/// receivers are in the table when the next save loads, without the fix.
		/// </summary>
		private static System.Action LeaveReceiverRegistered(GameObject receiver, TestContext t)
		{
			// Everything the injection needs is looked up and asserted here, in the test body, so that a
			// change in the mod fails this test; the cleanup it returns only invokes what was found.
			Type managerType = AccessTools.TypeByName(ChannelManagerType);
			FieldInfo channelsField = AccessTools.Field(managerType, "channels");
			t.Assert.NotNull(channelsField, "ChannelManager has a 'channels' field");
			object channels = channelsField?.GetValue(null);
			t.Assert.NotNull(channels, "ChannelManager.channels holds a table");
			object channel = (channels as IDictionary)?[DefaultChannel];
			t.Assert.NotNull(channel, "the table has a channel " + DefaultChannel);
			Component part = receiver.GetComponent(AccessTools.TypeByName(ReceiverType));
			FieldInfo callbackField = part == null ? null : AccessTools.Field(part.GetType(), "on_updateState_cb");
			t.Assert.NotNull(callbackField, "the receiver has an 'on_updateState_cb' field");
			var callback = callbackField?.GetValue(part) as Action<bool>;
			t.Assert.NotNull(callback, "the receiver's 'on_updateState_cb' holds a callback");
			MethodInfo add = channel == null ? null : AccessTools.Method(channel.GetType(), "add_OnConnectChanged");
			t.Assert.NotNull(add, "the channel has an 'OnConnectChanged' event to add to");
			return () =>
			{
				add.Invoke(channel, new object[] { callback });
				// LoadAgain asserts this: it is the proof that the stale registration was left.
				File.AppendAllText(CellsPath, ",injected");
				t.Log("left a second registration of the receiver on channel " + DefaultChannel);
			};
		}

		[OniTest("FWA fix: (second load 1/2) a switch, a powered emitter and a receiver on one channel work, and the receiver stays registered as the game is left", Timeout = 90), Layer(0)]
		public static IEnumerator WriteSave(TestContext t)
		{
			t.Speed(2);
			Fwa(t, EmitterType);
			Fwa(t, ReceiverType);
			Fwa(t, ChannelManagerType);
			CellRect inside = t.Dupes.Box();
			yield return t.WaitGameSeconds(1);
			int switchCell = inside.Cell(2, 0), emitterCell = inside.Cell(3, 0), generatorCell = inside.Cell(4, 0), receiverCell = inside.Cell(6, 0);

			UtilityDragSession power = t.Tools.UtilityTool("Wire", SimHashes.Copper);
			yield return power.Path(emitterCell, inside.Cell(4, 0), inside.Cell(5, 0), receiverCell);
			power.Dispose();
			yield return t.WaitGameSeconds(2);

			GameObject toggle = t.World.Build("LogicSwitch", switchCell);
			LogicSwitch switchPart = toggle.GetComponent<LogicSwitch>();
			if (switchPart.IsSwitchedOn)
				switchPart.HandleToggle();
			GameObject emitter = t.World.Build(EmitterId, emitterCell);
			GameObject receiver = t.World.Build(ReceiverId, receiverCell);
			yield return t.WaitGameSeconds(1);
			t.Assert.NotNull(emitter.GetComponent(Fwa(t, EmitterType)), "the emitter has the wireless emitter component");
			t.Assert.NotNull(receiver.GetComponent(Fwa(t, ReceiverType)), "the receiver has the wireless receiver component");

			UtilityDragSession logic = t.Tools.UtilityTool("LogicWire", SimHashes.Copper);
			yield return logic.Path(switchCell, emitterCell);
			logic.Dispose();
			yield return t.WaitGameSeconds(1);

			t.World.Build("DevGenerator", generatorCell);
			yield return t.WaitUntil(() => emitter.GetComponent<EnergyConsumer>().IsPowered && receiver.GetComponent<EnergyConsumer>().IsPowered, 20, "the emitter and the receiver to be powered through the wire");
			yield return t.WaitGameSeconds(1);

			t.Assert.Eq(0, ReceiverOutput(receiver), "the receiver sends nothing while the switch is off");
			switchPart.HandleToggle();
			yield return t.WaitUntil(() => ReceiverOutput(receiver) == 1, 10, "the receiver to send the emitter's signal after the switch is turned on");
			yield return t.WaitGameSeconds(2);
			t.Assert.Eq(EmitterWatts, emitter.GetComponent<EnergyConsumer>().BaseWattageRating, "the emitter's EnergyConsumer finished its init in the first game");

			File.WriteAllText(CellsPath, string.Join(",", switchCell, emitterCell, receiverCell));
			string path = SavePath;
			if (File.Exists(path))
				File.Delete(path);
			SaveLoader.Instance.Save(path, false, false);
			t.Assert.True(File.Exists(path), "the save was written to " + path);
			// The next test (FreshSave) loads this file instead of the fixture; it puts the fixture back.
			Session.SavePath = path;
			t.Cleanup(LeaveReceiverRegistered(receiver, t));
		}

		[OniTest("FWA fix: (second load 2/2) the second load in a session raises nothing from the receivers, the emitter finishes its init and the receiver follows the switch", Timeout = 90, FreshSave = true), Layer(0)]
		public static IEnumerator LoadAgain(TestContext t)
		{
			string loaded = Session.SavePath;
			t.Cleanup(() =>
			{
				string original = Directory.GetFiles(Session.Dir, "fixture.sav").FirstOrDefault();
				if (original != null)
					Session.SavePath = original;
				if (File.Exists(SavePath))
					File.Delete(SavePath);
				if (File.Exists(CellsPath))
					File.Delete(CellsPath);
			});
			t.Assert.True(loaded.EndsWith("fwa-secondload.sav"), "this test started from the save of the first test (" + loaded + ")");
			t.Assert.True(Session.Reloads > 0, "the colony was loaded again in this session (" + Session.Reloads + " reload(s))");

			t.Log("errors logged while the save loaded: " + t.LoadErrors.Count + (t.LoadErrors.Count > 0 ? "; first: " + t.LoadErrors[0] : ""));
			// The frames of the known trace (ChannelData / SignalNetworkBase / the mod's OnUpdateState) all name the mod.
			List<string> fromMod =t.LoadErrors.Where(e => e.Contains("FastWireless") || e.Contains("OnUpdateState") || e.Contains("ChannelData") || e.Contains("SignalNetworkBase")).ToList();
			t.Assert.Eq(0, fromMod.Count, "no error from Fast Wireless Automation+ while the save loaded" + (fromMod.Count > 0 ? ": " + fromMod[0] : ""));

			string[] saved = File.ReadAllText(CellsPath).Split(',');
			t.Assert.True(saved.Contains("injected"), "the first test left the stale registration (without it this test would pass with or without the fix)");
			int[] cells = saved.Where(s => s != "injected").Select(int.Parse).ToArray();
			GameObject toggle = t.World.At(cells[0], ObjectLayer.Building);
			GameObject emitter = t.World.At(cells[1], ObjectLayer.Building);
			GameObject receiver = t.World.At(cells[2], ObjectLayer.Building);
			t.Assert.NotNull(toggle, "a building is at " + Cells.Describe(cells[0]));
			t.Assert.NotNull(emitter, "a building is at " + Cells.Describe(cells[1]));
			t.Assert.NotNull(receiver, "a building is at " + Cells.Describe(cells[2]));
			t.Assert.NotNull(emitter.GetComponent(Fwa(t, EmitterType)), "the building at " + Cells.Describe(cells[1]) + " is the wireless emitter");
			t.Assert.NotNull(receiver.GetComponent(Fwa(t, ReceiverType)), "the building at " + Cells.Describe(cells[2]) + " is the wireless receiver");
			// EnergyConsumer.OnPrefabInit sets IsPowered (the call that raised the signal) and then the
			// wattage; an exception out of the first leaves the wattage unset.
			t.Assert.Eq(EmitterWatts, emitter.GetComponent<EnergyConsumer>().BaseWattageRating, "the emitter's EnergyConsumer finished its init");

			LogicSwitch switchPart = toggle.GetComponent<LogicSwitch>();
			t.Assert.True(switchPart.IsSwitchedOn, "the switch was saved on");
			yield return t.WaitUntil(() => emitter.GetComponent<EnergyConsumer>().IsPowered && ReceiverOutput(receiver) == 1, 20, "the loaded emitter to be powered and the receiver to send its signal");
			switchPart.HandleToggle();
			yield return t.WaitUntil(() => ReceiverOutput(receiver) == 0, 10, "the receiver to follow the switch off");
			switchPart.HandleToggle();
			yield return t.WaitUntil(() => ReceiverOutput(receiver) == 1, 10, "the receiver to follow the switch on");
			yield return t.WaitGameSeconds(1);
		}
	}
}
