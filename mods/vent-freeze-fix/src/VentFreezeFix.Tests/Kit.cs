using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Isochronous.TestKit;
using UnityEngine;

namespace VentFreezeFix.Tests
{
	/// <summary>Set-up and the watcher's private state, shared by the vent tests.</summary>
	internal static class Vents
	{
		private static readonly System.Type SnapshotType = typeof(VentFreezeWatcher).GetNestedType("Snapshot", BindingFlags.NonPublic);

		public static GameObject Build(TestContext t, int cell)
		{
			return t.World.Build("LiquidVent", cell);
		}

		public static VentFreezeWatcher Watcher(GameObject vent) => vent.GetComponent<VentFreezeWatcher>();

		public static int Ticks(VentFreezeWatcher w) => Traverse.Create(w).Field("ticks").GetValue<int>();

		/// <summary>The watcher's window after a load: five 200 ms ticks. A vent just built is past it after this.</summary>
		public static Wait WindowExpires(TestContext t) => t.WaitGameSeconds(2);

		/// <summary>
		/// Starts the watcher's first second over again, with a snapshot of what its cell held the tick
		/// before, as a load restores it from the save: the state a watcher is in the moment a game loads.
		/// </summary>
		public static void Rearm(VentFreezeWatcher watcher, params (int cell, SimHashes element, float mass)[] before)
		{
			var traverse = Traverse.Create(watcher);
			traverse.Field("ticks").SetValue(0);
			var previous = (System.Collections.IDictionary)traverse.Field("previous").GetValue();
			previous.Clear();
			foreach (var (cell, element, mass) in before)
			{
				object snapshot = System.Activator.CreateInstance(SnapshotType);
				SnapshotType.GetField("element").SetValue(snapshot, ElementLoader.FindElementByHash(element));
				SnapshotType.GetField("mass").SetValue(snapshot, mass);
				previous[cell] = snapshot;
			}
			SimAndRenderScheduler.instance.Remove(watcher);
			SimAndRenderScheduler.instance.Add(watcher);
		}

		/// <summary>The loose pieces of the element lying in or within one cell of the cell, in kg.</summary>
		public static float Debris(int cell, SimHashes element)
		{
			float total = 0f;
			Vector3 at = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore);
			foreach (Pickupable pickupable in Object.FindObjectsOfType<Pickupable>())
			{
				PrimaryElement primary = pickupable.GetComponent<PrimaryElement>();
				if (primary == null || primary.ElementID != element || pickupable.storage != null)
					continue;
				if ((pickupable.transform.position - at).sqrMagnitude < 2.25f)
					total += primary.Mass;
			}
			return total;
		}

		public static void Put(int cell, SimHashes element, float kg, float kelvin)
		{
			SimMessages.ReplaceElement(cell, element, CellEventLogger.Instance.SandBoxTool, kg, kelvin);
		}
	}
}
