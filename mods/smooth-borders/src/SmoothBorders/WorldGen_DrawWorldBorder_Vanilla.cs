using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using ProcGenGame;
using UnityEngine;

namespace SmoothBorders
{
	/// <summary>
	/// The game's own DrawWorldBorder, untouched by any transpiler (this mod's or another's). The
	/// prefix runs it on a copy of the cells to learn the POI rectangles the game would have set.
	/// </summary>
	[HarmonyPatch(typeof(WorldGen), "DrawWorldBorder")]
	public static class WorldGen_DrawWorldBorder_Vanilla
	{
		[HarmonyReversePatch(HarmonyReversePatchType.Original)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void DrawWorldBorder(WorldGen instance, ref WorldgenSimData simData, Chunk world, SeededRandom rnd,
			ref HashSet<int> borderCells, ref List<RectInt> poiBounds, WorldGen.OfflineCallbackFunction updateProgressFn)
		{
			throw new NotImplementedException("Harmony replaces this with the game's DrawWorldBorder");
		}
	}
}
