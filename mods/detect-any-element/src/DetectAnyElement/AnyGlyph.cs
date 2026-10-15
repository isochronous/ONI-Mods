#pragma warning disable 649 // [MyCmpGet] fields are populated by the game via reflection
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DetectAnyElement
{
	/// <summary>
	/// While a sensor is on Any, the icon its screen flashes while it is on (a drop, a gas gust,
	/// a lump of ore) is an asterisk instead. That icon is the symbol "elements" in all five
	/// sensor kanims; only the looping "on" anim draws it, stepping through its 26 frames, which
	/// carry the fade-in and fade-out themselves. So a symbol override with the 26-frame
	/// asterisk of this mod's detect_any_asterisk kanim flashes the same way, with no anim or
	/// switch code touched, and the off state stays the plain vanilla screen.
	/// </summary>
	public sealed class AnyGlyph : KMonoBehaviour
	{
		private const string AnimName = "detect_any_asterisk_kanim";
		private static readonly HashedString GlyphSymbol = "elements";

		/// <summary>
		/// The vanilla icons' colours are baked into their textures, and each kanim anchors
		/// "elements" at its own spot, not always the middle of the screen. So the asterisk kanim
		/// has one symbol per sensor kanim, asterisk_&lt;kanim&gt;, in that icon's colour and drawn
		/// off-centre in its frames to land on the middle of that screen.
		/// </summary>
		private const string SymbolPrefix = "asterisk_";
		private const string KanimSuffix = "_kanim";
		private static readonly Dictionary<string, KAnim.Build.Symbol> symbols = new Dictionary<string, KAnim.Build.Symbol>();

		[MyCmpGet] private Filterable filterable;
		[MyCmpGet] private SymbolOverrideController symbolOverrides;
		[MyCmpGet] private KBatchedAnimController animController;

		private bool overridden;

		protected override void OnSpawn()
		{
			base.OnSpawn();
			if (filterable == null || symbolOverrides == null)
				return;
			filterable.onFilterChanged += OnFilterChanged;
			OnFilterChanged(filterable.SelectedTag);
		}

		protected override void OnCleanUp()
		{
			if (filterable != null)
				filterable.onFilterChanged -= OnFilterChanged;
			base.OnCleanUp();
		}

		/// <summary>Picking Any (including by copying settings) adds the override; anything else removes it.</summary>
		private void OnFilterChanged(Tag tag)
		{
			bool any = AnyElement.Is(tag);
			if (any == overridden)
				return;
			if (any)
			{
				KAnim.Build.Symbol source = Asterisk(AsteriskName());
				if (source == null)
					return;
				symbolOverrides.AddSymbolOverride(GlyphSymbol, source);
			}
			else
				symbolOverrides.RemoveSymbolOverride(GlyphSymbol);
			overridden = any;
		}

		/// <summary>asterisk_&lt;kanim&gt; for the kanim the sensor is drawn with, or null.</summary>
		private string AsteriskName()
		{
			KAnimFile[] files = animController != null ? animController.AnimFiles : null;
			string kanim = files != null && files.Length > 0 && files[0] != null ? files[0].name : null;
			if (kanim == null)
				return null;
			if (kanim.EndsWith(KanimSuffix))
				kanim = kanim.Substring(0, kanim.Length - KanimSuffix.Length);
			return SymbolPrefix + kanim;
		}

		/// <summary>An asterisk symbol, looked up once; null (and logged) if the kanim did not load.</summary>
		private static KAnim.Build.Symbol Asterisk(string name)
		{
			if (name == null)
				return null;
			if (symbols.TryGetValue(name, out KAnim.Build.Symbol symbol))
				return symbol;
			KAnimFile anim = Assets.GetAnim(AnimName);
			symbol = anim != null ? anim.GetData().build.GetSymbol(name) : null;
			if (symbol == null)
				Debug.LogWarning("[DetectAnyElement] " + AnimName + " has no symbol \"" + name + "\"; sensors on Any keep the vanilla icon.");
			else
				symbols[name] = symbol;
			return symbol;
		}
	}

	/// <summary>
	/// Gives the five element sensors the asterisk. Buildings have no SymbolOverrideController
	/// of their own, and the anim controller has to know about one before it is instantiated,
	/// so it goes on the prefab.
	/// </summary>
	[HarmonyPatch]
	public static class SensorConfigs_DoPostConfigureComplete_Patch
	{
		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(LiquidConduitElementSensorConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
			yield return AccessTools.Method(typeof(GasConduitElementSensorConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
			yield return AccessTools.Method(typeof(SolidConduitElementSensorConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
			yield return AccessTools.Method(typeof(LogicElementSensorGasConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
			yield return AccessTools.Method(typeof(LogicElementSensorLiquidConfig), nameof(IBuildingConfig.DoPostConfigureComplete));
		}

		public static void Postfix(GameObject go)
		{
			if (go.GetComponent<SymbolOverrideController>() == null)
				SymbolOverrideControllerUtil.AddToPrefab(go);
			go.AddOrGet<AnyGlyph>();
		}
	}
}
