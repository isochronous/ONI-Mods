using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Places what a drag asks for, the way the game's own tools do: a replacement order over a
	/// building the game lets you build over, or the game's reconstruct for one it does not. With
	/// instant build on, both are finished on the spot, as the build tool finishes its orders.
	/// </summary>
	internal static class Orders
	{
		/// <summary>Reconstructable's materials for the rebuild; the game's button sets only the first.</summary>
		internal static readonly AccessTools.FieldRef<Reconstructable, Tag[]> ReconstructMaterials =
			AccessTools.FieldRefAccess<Reconstructable, Tag[]>("selectedElementsTags");

		/// <summary>Unpaused frames an instant rebuild waits for the game to place the new building's order.</summary>
		private const int RebuildFrames = 150;

		/// <summary>The build tool's own test: the debug toggle, or sandbox mode with its instant build setting on.</summary>
		public static bool InstantBuild =>
			DebugHandler.InstantBuildMode
			|| (Game.Instance != null && Game.Instance.SandboxModeActive
				&& SandboxToolParameterMenu.instance != null && SandboxToolParameterMenu.instance.settings.InstantBuild);

		/// <summary>Places the upgrade of one building at the given priority. False when the game would not have it (already pending, say).</summary>
		public static bool Place(BuildingComplete building, Upgrade upgrade, PrioritySetting priority)
		{
			return upgrade.Rebuild ? Reconstruct(building, upgrade.Materials) : Replace(building, upgrade.Target, upgrade.Materials, priority);
		}

		/// <summary>
		/// The build tool's replacement sequence for one building: the candidate found on the
		/// target's layer, nothing already pending there, not the same type in the same materials,
		/// then TryReplaceTile with the building's own orientation and the materials.
		/// </summary>
		private static bool Replace(BuildingComplete building, BuildingDef target, List<Tag> materials, PrioritySetting priority)
		{
			GameObject go = building.gameObject;
			int cell = Grid.PosToCell(go);
			if (!Grid.IsValidCell(cell) || !building.Def.Replaceable)
				return false;
			// The utility build tool's BuildPath asks no tag of the building it replaces; the build tool does.
			if (!target.isUtility && !target.CanReplace(go))
				return false;
			if (target.GetReplacementCandidate(cell) != go)
				return false;
			Orientation orientation = building.Orientation;
			bool occupied = false;
			target.RunOnArea(cell, orientation, offsetCell => occupied |= target.IsReplacementLayerOccupied(offsetCell));
			if (occupied)
				return false;
			if (building.Def == target && Materials.Same(Materials.Of(building), materials))
				return false;
			Vector3 pos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
			GameObject order = target.TryReplaceTile(null, pos, orientation, materials);
			if (order == null)
				return false;
			Grid.Objects[cell, (int)target.ReplacementLayer] = order;
			// The old building's settings ride with the order, for Blueprints Expanded to apply to the
			// new building when it is built (of the same type: a type change keeps none).
			BlueprintsCompat.CarrySettings(go, order);
			// A wire or pipe order over a wire or pipe draws the connections of the one it replaces,
			// as the utility build tool's BuildPath gives it; the game carries them to the new
			// building when the order completes.
			KAnimGraphTileVisualizer oldVisualizer = go.GetComponent<KAnimGraphTileVisualizer>();
			KAnimGraphTileVisualizer newVisualizer = order.GetComponent<KAnimGraphTileVisualizer>();
			if (oldVisualizer != null && newVisualizer != null)
			{
				newVisualizer.Connections = oldVisualizer.Connections;
				TileVisualizer.RefreshCell(cell, target.TileLayer, target.ReplacementLayer);
			}
			order.GetComponent<Prioritizable>()?.SetMasterPriority(priority);
			if (InstantBuild)
				Finish(order, target, materials);
			return true;
		}

		/// <summary>
		/// A type the game does not let you build over: the game's own reconstruct, as the material
		/// button in the building's side screen asks for it. A Duplicant deconstructs the building, and
		/// the game places it again in the new materials (every ingredient, not only the first one the
		/// game's button sets). Blueprints Expanded carries its settings. Skipped when a reconstruct is
		/// already asked for, since asking again would cancel it.
		/// </summary>
		private static bool Reconstruct(BuildingComplete building, List<Tag> materials)
		{
			Reconstructable reconstructable = building.GetComponent<Reconstructable>();
			if (reconstructable == null || !reconstructable.AllowReconstruct || reconstructable.ReconstructRequested || materials.Count == 0)
				return false;
			reconstructable.RequestReconstruct(materials[0]);
			if (!reconstructable.ReconstructRequested)
				return false;
			ReconstructMaterials(reconstructable) = materials.ToArray();
			if (InstantBuild)
				FinishReconstruct(building, materials);
			return true;
		}

		/// <summary>
		/// Instant build: the deconstruction is finished on the spot (the game finishes its own
		/// deconstruct tool's only in debug mode), the game places the new building's order, and that
		/// order is finished as soon as it is there, the way instant build finishes a replacement.
		/// </summary>
		private static void FinishReconstruct(BuildingComplete building, List<Tag> materials)
		{
			Deconstructable deconstructable = building.GetComponent<Deconstructable>();
			if (deconstructable == null)
				return;
			BuildingDef def = building.Def;
			int cell = Grid.PosToCell(building);
			string name = building.name;
			// Blueprints Expanded gives the new order the old building's settings a moment after the
			// game places it; finished on the spot, the order could beat it to it. They are read here,
			// while the building stands, and given to the order just before it is finished.
			Dictionary<string, JObject> settings = BlueprintsCompat.TakeSettings(building.gameObject);
			deconstructable.CompleteWork(null);
			// The game places the order from its scheduler, due next frame. That scheduler keeps no
			// order among entries due at the same moment, so anything scheduled beside it can run first
			// and find no order: this looks once a frame instead.
			Game.Instance.StartCoroutine(EachUnpausedFrame(RebuildFrames, () =>
			{
				GameObject order = Grid.Objects[cell, (int)def.ObjectLayer];
				if (order == null || order.GetComponent<Constructable>() == null)
					return false;
				BlueprintsCompat.GiveSettings(settings, order, name);
				Finish(order, def, materials);
				return true;
			}, () => Debug.LogWarning("[UpgradePlanner] Instant rebuild of " + def.PrefabID + " at cell " + cell + ": the game placed no order to finish.")));
		}

		/// <summary>
		/// The game places a reconstructed building's new order the frame after the old one is taken
		/// down, and only if the spot is valid by then. For a wide tile (a 3-wide farm tile) it is not
		/// yet: the old tile's cells are still being cleared, the placement is refused, and the
		/// building is gone with nothing ordered in its place. Called as the reconstruct commences,
		/// this places the same order once the spot is valid, if the game's own placement did not.
		/// </summary>
		public static void PlaceRebuildIfMissing(Reconstructable reconstructable)
		{
			Building building = reconstructable.GetComponent<Building>();
			Deconstructable deconstructable = reconstructable.GetComponent<Deconstructable>();
			if (building == null || deconstructable == null || !deconstructable.allowDeconstruction || !reconstructable.ReconstructRequested)
				return;
			BuildingDef def = building.Def;
			Vector3 position = building.transform.position;
			Orientation orientation = building.Orientation;
			Tag[] materials = (Tag[])ReconstructMaterials(reconstructable).Clone();
			string facade = reconstructable.GetComponent<BuildingFacade>()?.CurrentFacade;
			int cell = Grid.PosToCell(position);
			string name = building.name;
			// Blueprints Expanded gives its settings to the order the game places; one placed here
			// gets them from the old building, read while it still stands.
			Dictionary<string, JObject> settings = BlueprintsCompat.TakeSettings(building.gameObject);
			Game.Instance.StartCoroutine(PlaceLater(building.gameObject, def, cell, position, orientation, materials, facade, settings, name));
		}

		/// <summary>Unpaused frames <see cref="PlaceRebuildIfMissing"/> keeps trying before giving the building up.</summary>
		private const int PlaceFrames = 240;

		/// <summary>Game seconds the game's own placement is given first.</summary>
		private const float PlacementGrace = 0.5f;

		private static IEnumerator PlaceLater(GameObject old, BuildingDef def, int cell, Vector3 position, Orientation orientation, Tag[] materials, string facade,
			Dictionary<string, JObject> settings, string name)
		{
			// Done as soon as the game's own placement shows (the old building can stand a frame longer): once it has, whatever happens to that
			// building after (finished and taken down within the grace, say) is not for this to undo.
			bool Placed()
			{
				GameObject there = Grid.Objects[cell, (int)def.ObjectLayer];
				return there != null && there != old && there.GetComponent<Building>()?.Def == def;
			}
			int frames = 0;
			float due = GameClock.Instance.GetTime() + PlacementGrace;
			while (GameClock.Instance.GetTime() < due)
			{
				yield return null;
				if (Placed())
					yield break;
				if (!Paused)
					frames++;
			}
			yield return EachUnpausedFrame(PlaceFrames - frames, () =>
			{
				if (Placed())
					return true;
				GameObject order = def.TryPlace(null, position, orientation, materials, facade, restrictToActiveWorld: false);
				if (order == null)
					return false;
				int given = BlueprintsCompat.GiveSettings(settings, order, name);
				Debug.Log("[UpgradePlanner] Placed the rebuilt " + def.PrefabID + " at cell " + cell + ", with " + given + " settings.");
				return true;
			}, () => Debug.LogWarning("[UpgradePlanner] Could not place the rebuilt " + def.PrefabID + " at cell " + cell + "."), checkFirst: true);
		}

		/// <summary>
		/// Instant build: the order is given its materials and finished on the spot, which is the
		/// game's own completion of an order (Constructable.OnCompleteWork): a replaced building is
		/// released and the new one built in its place with the old one's connections and the order's
		/// materials, the way a Duplicant ends it. The materials come from nowhere, as with any
		/// instant build.
		/// </summary>
		private static void Finish(GameObject order, BuildingDef def, IList<Tag> materials)
		{
			Constructable constructable = order.GetComponent<Constructable>();
			Storage storage = order.GetComponent<Storage>();
			if (constructable == null || storage == null || def.CraftRecipe == null)
				return;
			Vector3 pos = order.transform.position;
			foreach (Recipe.Ingredient ingredient in def.CraftRecipe.GetAllIngredients(materials))
			{
				Element element = ElementLoader.GetElement(ingredient.tag);
				if (element != null)
				{
					storage.AddOre(element.id, ingredient.amount, element.defaultValues.temperature, byte.MaxValue, 0);
					continue;
				}
				// An ingredient that is an item rather than an element (a carpet's reed fiber).
				GameObject prefab = Assets.TryGetPrefab(ingredient.tag);
				if (prefab == null)
					continue;
				GameObject item = GameUtil.KInstantiate(prefab, pos, Grid.SceneLayer.Ore);
				PrimaryElement primary = item.GetComponent<PrimaryElement>();
				if (primary != null)
					primary.Mass = ingredient.amount;
				item.SetActive(true);
				// With its events: they tell the item it is stored. Without them it counts as loose,
				// and what a tile built over drops in the same cell absorbs it, so the order, which the
				// game finishes from a sim callback a moment later, finds it destroyed.
				storage.Store(item, hide_popups: true);
			}
			constructable.CompleteWork(null);
		}

		private static bool Paused => SpeedControlScreen.Instance != null && SpeedControlScreen.Instance.IsPaused;

		/// <summary>
		/// Calls <paramref name="step"/> once a frame until it returns true, counting only frames the
		/// game is not paused (the game's scheduler waits while it is), up to <paramref name="frames"/>;
		/// <paramref name="giveUp"/> after that. <paramref name="checkFirst"/> calls it before the first frame passes.
		/// </summary>
		private static IEnumerator EachUnpausedFrame(int frames, Func<bool> step, System.Action giveUp, bool checkFirst = false)
		{
			int counted = 0;
			if (checkFirst && step())
				yield break;
			while (counted < frames)
			{
				yield return null;
				if (Paused)
					continue;
				counted++;
				if (step())
					yield break;
			}
			giveUp();
		}
	}
}
