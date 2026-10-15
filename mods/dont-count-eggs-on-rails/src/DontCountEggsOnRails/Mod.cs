using HarmonyLib;
using KMod;
using UnityEngine;

namespace DontCountEggsOnRails
{
	public sealed class DontCountEggsOnRailsMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Debug.Log("[DontCountEggsOnRails] Loaded version " + typeof(DontCountEggsOnRailsMod).Assembly.GetName().Version);
		}
	}

	/// <summary>
	/// A critter sensor counts the eggs the room prober lists for its room, and an egg lists itself
	/// in whatever room its position falls in, once a second, through its overcrowding monitor. An
	/// egg riding a conveyor rail moves through rooms like anything else on the rail, so it gets
	/// counted by every sensor along the way while its packet passes through open cells.
	///
	/// A rail marks the item it carries as stored without giving it a storage (it fires the store
	/// event with a plain true), which nothing else does: an egg in an incubator or carried by a
	/// duplicant has a real storage, and an egg on the floor is not stored at all. While that is the
	/// case the egg is kept out of any room, and dropped from the one it was in; once it leaves the
	/// rail the game's own update lists it again.
	/// </summary>
	[HarmonyPatch(typeof(OvercrowdingMonitor), "UpdateRegion")]
	public static class OvercrowdingMonitor_UpdateRegion_Patch
	{
		public static bool Prefix(OvercrowdingMonitor.Instance smi)
		{
			if (!smi.IsEgg || !OnRail(smi))
				return true;
			if (smi.cavity != null)
			{
				smi.RemoveFromCavity();
				Game.Instance.roomProber.UpdateRoom(smi.cavity);
				smi.cavity = null;
			}
			return false;
		}

		private static bool OnRail(OvercrowdingMonitor.Instance smi)
		{
			if (!smi.kpid.HasTag(GameTags.Stored))
				return false;
			Pickupable pickupable = smi.GetComponent<Pickupable>();
			return pickupable != null && pickupable.storage == null;
		}
	}
}
