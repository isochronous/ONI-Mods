using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Isochronous.TestKit;
using KMod;

namespace StoragePodRedux.Tests
{
	/// <summary>
	/// Which buildings the game gets: the options, the Cool Pod's Auto mode (which reads Ronivan's
	/// Legacy config files) and the original mod standing the pods aside. Options need a restart to
	/// take effect, so these drive the mod's own decision code with the option values changed for the
	/// test and put back after, and ask the registration patch what it would do.
	/// </summary>
	public static class Decisions
	{
		private static readonly Type BuildingsType = Helpers.ModType("Buildings");
		private const string RonivanId = "RonivansLegacy_ChemicalProcessing";

		/// <summary>Puts the options and the mod's decisions back after the test.</summary>
		private static void Restore(TestContext t)
		{
			bool storage = Options.Instance.StoragePodEnabled;
			CoolPodMode mode = Options.Instance.CoolPod;
			bool podOn = Get("StoragePodEnabled"), coolOn = Get("CoolPodEnabled");
			string reason = (string)BuildingsType.GetProperty("CoolPodReason").GetValue(null);
			bool original = StoragePodReduxMod.OriginalActive;
			t.Cleanup(() =>
			{
				Options.Instance.StoragePodEnabled = storage;
				Options.Instance.CoolPod = mode;
				Set("StoragePodEnabled", podOn);
				Set("CoolPodEnabled", coolOn);
				BuildingsType.GetProperty("CoolPodReason").GetSetMethod(true).Invoke(null, new object[] { reason });
				typeof(StoragePodReduxMod).GetProperty("OriginalActive").GetSetMethod(true).Invoke(null, new object[] { original });
			});
		}

		private static bool Get(string property)
		{
			return (bool)BuildingsType.GetProperty(property).GetValue(null);
		}

		private static void Set(string property, bool value)
		{
			BuildingsType.GetProperty(property).GetSetMethod(true).Invoke(null, new object[] { value });
		}

		private static void Decide(IReadOnlyList<Mod> mods)
		{
			BuildingsType.GetMethod("Decide").Invoke(null, new object[] { mods });
		}

		private static bool IsEnabled(string id)
		{
			return (bool)BuildingsType.GetMethod("IsEnabled", new[] { typeof(string) }).Invoke(null, new object[] { id });
		}

		private static string Reason()
		{
			return (string)BuildingsType.GetProperty("CoolPodReason").GetValue(null);
		}

		private static Mod FakeMod(string staticId, bool enabled)
		{
			var mod = new Mod();
			typeof(Mod).GetProperty("staticID").GetSetMethod(true).Invoke(mod, new object[] { staticId });
			mod.enabledForDlc = enabled ? new List<string> { DlcManager.GetHighestActiveDlcId() } : new List<string>();
			return mod;
		}

		private static bool Prefix(IBuildingConfig config)
		{
			Type patch = typeof(Patches).GetNestedType("BuildingConfigManager_RegisterBuilding_Patch");
			return (bool)patch.GetMethod("Prefix").Invoke(null, new object[] { config });
		}

		[OniTest("Storage Pod Redux: the options turn each building on or off, and the Cool Pod's three modes decide", Timeout = 20), Layer(0)]
		public static IEnumerator OptionsDecide(TestContext t)
		{
			Restore(t);
			var none = new List<Mod>();
			Options.Instance.StoragePodEnabled = false;
			Options.Instance.CoolPod = CoolPodMode.Auto;
			Decide(none);
			t.Assert.False(IsEnabled(StoragePodConfig.ID), "Storage Pod off");
			t.Assert.True(IsEnabled(CoolPodConfig.ID), "Cool Pod Auto with no other mod: on");
			Options.Instance.StoragePodEnabled = true;
			Options.Instance.CoolPod = CoolPodMode.Disabled;
			Decide(none);
			t.Assert.True(IsEnabled(StoragePodConfig.ID), "Storage Pod on");
			t.Assert.False(IsEnabled(CoolPodConfig.ID), "Cool Pod Disabled: off");
			t.Assert.Eq("option: Disabled", Reason(), "and says why");
			Options.Instance.CoolPod = CoolPodMode.Enabled;
			Decide(new List<Mod> { FakeMod(RonivanId, true) });
			t.Assert.True(IsEnabled(CoolPodConfig.ID), "Cool Pod Enabled: on even beside Ronivan's Legacy");
			t.Assert.True(IsEnabled("SomeOtherBuilding"), "other buildings are never affected");
			yield break;
		}

		[OniTest("Storage Pod Redux: a turned-off building is never registered, the other still is", Timeout = 30), Layer(0)]
		public static IEnumerator TurnedOffIsNotRegistered(TestContext t)
		{
			Restore(t);
			Options.Instance.StoragePodEnabled = false;
			Options.Instance.CoolPod = CoolPodMode.Enabled;
			Decide(new List<Mod>());
			t.Assert.False(Prefix(new StoragePodConfig()), "the registration patch skips the Storage Pod");
			t.Assert.True(Prefix(new CoolPodConfig()), "and lets the Cool Pod through");
			t.Assert.True(Prefix(new StorageLockerConfig()), "and any other building");
			// The real call: with the pod off, registering it again does nothing.
			int defs = Assets.BuildingDefs.Count;
			BuildingDef before = Assets.GetBuildingDef(StoragePodConfig.ID);
			BuildingConfigManager.Instance.RegisterBuilding(new StoragePodConfig());
			t.Assert.Eq(defs, Assets.BuildingDefs.Count, "no building def was added");
			t.Assert.True(ReferenceEquals(before, Assets.GetBuildingDef(StoragePodConfig.ID)), "and the existing one is untouched");
			yield break;
		}

		[OniTest("Storage Pod Redux: the original Storage Pod mod being enabled takes both pods out of the way", Timeout = 20), Layer(0)]
		public static IEnumerator OriginalModActive(TestContext t)
		{
			Restore(t);
			Type detection = Helpers.ModType("ModDetection");
			MethodInfo isEnabled = detection.GetMethod("IsEnabled");
			var mods = new List<Mod> { FakeMod(StoragePodReduxMod.OriginalStaticID, true) };
			t.Assert.True((bool)isEnabled.Invoke(null, new object[] { mods, StoragePodReduxMod.OriginalStaticID }), "an enabled mod with the original's staticID is found");
			mods[0].enabledForDlc = new List<string>();
			t.Assert.False((bool)isEnabled.Invoke(null, new object[] { mods, StoragePodReduxMod.OriginalStaticID }), "the same mod switched off is not");

			Options.Instance.StoragePodEnabled = true;
			Options.Instance.CoolPod = CoolPodMode.Enabled;
			Decide(new List<Mod>());
			typeof(StoragePodReduxMod).GetProperty("OriginalActive").GetSetMethod(true).Invoke(null, new object[] { true });
			t.Assert.False(IsEnabled(StoragePodConfig.ID), "Storage Pod stands aside");
			t.Assert.False(IsEnabled(CoolPodConfig.ID), "Cool Pod stands aside, whatever its option says");
			t.Assert.False(Prefix(new StoragePodConfig()), "neither is registered: Storage Pod");
			t.Assert.False(Prefix(new CoolPodConfig()), "neither is registered: Cool Pod");
			yield break;
		}

		[OniTest("Storage Pod Redux: Auto turns the Cool Pod off beside Ronivan's Legacy with its Fridge Pod active, and on otherwise", Timeout = 20), Layer(0)]
		public static IEnumerator AutoBesideRonivan(TestContext t)
		{
			Restore(t);
			string folder = Path.Combine(Manager.GetDirectory(), "config", RonivanId);
			Options.Instance.CoolPod = CoolPodMode.Auto;
			Decide(new List<Mod> { FakeMod(RonivanId, true) });
			// Whatever config this machine has, read the same way with the game's JSON library.
			bool module = Flag(Read(Path.Combine(folder, "config.json"))?["DupesRefrigeration_Enabled"], true);
			var pod = Read(Path.Combine(folder, "RonivanAIO_BuildingConfig.json"))?["BuildingConfigurations"]?["FridgePod"];
			bool active = Flag(pod?["BuildingEnabledForce"], false) || (module && Flag(pod?["BuildingEnabled"], true));
			t.Log("config folder " + (Directory.Exists(folder) ? "present" : "absent") + ": Fridge Pod active = " + active);
			t.Assert.Eq(!active, IsEnabled(CoolPodConfig.ID), "Auto: the Cool Pod is on exactly when the Fridge Pod is not active");
			if (active)
				t.Assert.True(Reason().Contains("Fridge Pod"), "and the reason names the Fridge Pod: " + Reason());
			Decide(new List<Mod> { FakeMod(RonivanId, false) });
			t.Assert.True(IsEnabled(CoolPodConfig.ID), "Ronivan's Legacy installed but not enabled: the Cool Pod is on");
			Decide(new List<Mod>());
			t.Assert.True(IsEnabled(CoolPodConfig.ID), "not installed: the Cool Pod is on");
			yield break;
		}

		private static Newtonsoft.Json.Linq.JObject Read(string path)
		{
			return File.Exists(path) ? Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)) : null;
		}

		private static bool Flag(Newtonsoft.Json.Linq.JToken token, bool fallback)
		{
			return token != null && token.Type == Newtonsoft.Json.Linq.JTokenType.Boolean ? (bool)token : fallback;
		}

		// Explicit: it writes a config folder into the player's real mods/config folder (only when none
		// exists, removed afterwards); a default run never touches the player's Documents.
		[OniTest("Storage Pod Redux: Ronivan's Legacy config files decide whether its Fridge Pod is active", Timeout = 30, Explicit = true), Layer(0)]
		public static IEnumerator RonivanConfigFiles(TestContext t)
		{
			Restore(t);
			string folder = Path.Combine(Manager.GetDirectory(), "config", RonivanId);
			if (Directory.Exists(folder))
				t.Skip("a Ronivan's Legacy config folder exists and is not overwritten: " + folder);
			Directory.CreateDirectory(folder);
			t.Cleanup(() =>
			{
				if (Directory.Exists(folder))
					Directory.Delete(folder, true);
			});
			Options.Instance.CoolPod = CoolPodMode.Auto;
			var mods = new List<Mod> { FakeMod(RonivanId, true) };
			// Each case: config.json, RonivanAIO_BuildingConfig.json (null: no file), expected Cool Pod on?
			var cases = new (string name, string config, string building, bool coolPod)[]
			{
				("module off", "{\"DupesRefrigeration_Enabled\": false}", null, true),
				("Fridge Pod off", null, "{\"BuildingConfigurations\":{\"FridgePod\":{\"BuildingEnabled\":false}}}", true),
				("module off but the building forced", "{\"DupesRefrigeration_Enabled\": false}", "{\"BuildingConfigurations\":{\"FridgePod\":{\"BuildingEnabled\":false,\"BuildingEnabledForce\":true}}}", false),
				("both explicitly on", "{\"DupesRefrigeration_Enabled\": true}", "{\"BuildingConfigurations\":{\"FridgePod\":{\"BuildingEnabled\":true}}}", false),
				("values that are not booleans fall back to on", "{\"DupesRefrigeration_Enabled\": \"no\"}", "{\"BuildingConfigurations\":{\"FridgePod\":{\"BuildingEnabled\":\"no\"}}}", false),
				("unreadable files fall back to on", "this is not json", "{", false),
			};
			string configPath = Path.Combine(folder, "config.json");
			string buildingPath = Path.Combine(folder, "RonivanAIO_BuildingConfig.json");
			t.AllowLogError("Could not read");
			foreach (var c in cases)
			{
				File.Delete(configPath);
				File.Delete(buildingPath);
				if (c.config != null)
					File.WriteAllText(configPath, c.config);
				if (c.building != null)
					File.WriteAllText(buildingPath, c.building);
				Decide(mods);
				t.Assert.Eq(c.coolPod, IsEnabled(CoolPodConfig.ID), c.name + ": Cool Pod on?");
			}
			yield break;
		}
	}
}
