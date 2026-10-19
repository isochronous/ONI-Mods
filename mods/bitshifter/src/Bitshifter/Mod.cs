using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Isochronous;
using KMod;
using PeterHan.PLib.Core;
using UnityEngine;

namespace Bitshifter
{
	public sealed class BitshifterMod : UserMod2
	{
		/// <summary>Bits 1-31. Bit 32 is the int's sign bit; the game tests bits with "&gt; 0", so it never reads as on.</summary>
		public const int Bits = 31;

		/// <summary>
		/// Workshop items that already widen the ribbon reader and writer. When one of them is
		/// enabled this mod leaves the reader and writer alone, so the two never fight over the
		/// same methods; the toolkit's buildings stay.
		/// </summary>
		private static readonly string[] YieldToWorkshopIds =
		{
			"3244925649", // Digital CPU (ONICPU): 32-bit ribbons, reader and writer
		};

		private static readonly string[] YieldToStaticIds =
		{
			"OniSmallCpu", // Digital CPU, however it was installed
		};

		// Methods a mod would patch to widen the reader or writer. Digital CPU, for one, sets
		// the bitDepth field from an OnSpawn postfix rather than touching GetBitDepth.
		private static readonly MethodBase[] Contested =
		{
			AccessTools.Method(typeof(LogicRibbonReader), nameof(LogicRibbonReader.GetBitDepth)),
			AccessTools.Method(typeof(LogicRibbonWriter), nameof(LogicRibbonWriter.GetBitDepth)),
			AccessTools.Method(typeof(LogicRibbonReader), "OnSpawn"),
			AccessTools.Method(typeof(LogicRibbonWriter), "OnSpawn"),
		};

		// Two groups of patches. The toolkit's (category Toolkit.PatchCategory) are applied here,
		// whatever other mods do. The Reader and Writer patches (uncategorized) are applied in
		// OnAllModsLoaded, after every other mod had its turn, and only when no other mod widens
		// the Reader or Writer. So deliberately not base.OnLoad, which would PatchAll right away.
		public override void OnLoad(Harmony harmony)
		{
			PUtil.InitLibrary(false);
			Translations.Register(harmony, typeof(ModStrings));
			harmony.PatchCategory(typeof(BitshifterMod).Assembly, Toolkit.PatchCategory);
			Debug.Log("[Bitshifter] Loaded version " + typeof(BitshifterMod).Assembly.GetName().Version);
		}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
		{
			OverlayLabelGuard.Apply(harmony);
			string reason = WhyYield(mods);
			if (reason != null)
			{
				Debug.Log("[Bitshifter] Reader and Writer left alone: " + reason);
				return;
			}
			harmony.PatchAllUncategorized(typeof(BitshifterMod).Assembly);
			Debug.Log("[Bitshifter] Ribbon Reader and Writer can select bits 1-" + Bits);
		}

		private static string WhyYield(IReadOnlyList<Mod> mods)
		{
			foreach (Mod mod in mods)
			{
				if (!mod.IsEnabledForActiveDlc())
					continue;
				foreach (string id in YieldToWorkshopIds)
				{
					if (mod.label.id == id)
						return "'" + mod.title + "' (Workshop " + id + ") is enabled";
				}
				foreach (string id in YieldToStaticIds)
				{
					if (mod.staticID == id)
						return "'" + mod.title + "' (" + id + ") is enabled";
				}
			}
			foreach (MethodBase method in Contested)
			{
				Patches patches = Harmony.GetPatchInfo(method);
				if (patches != null && patches.Owners.Count > 0)
					return method.DeclaringType.Name + "." + method.Name + " is already patched by " + string.Join(", ", patches.Owners);
			}
			return null;
		}
	}
}
