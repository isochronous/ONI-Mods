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
			if (!JointPlateEndsClear(target, cell, orientation))
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
		/// The game's own test of where a Heavi-Watt joint plate's two wire ends fall
		/// (BuildingDef.IsValidHighWattBridgeLocation): both ends in the world, neither in a tile, a
		/// door (built or ordered) or another plate's end. The game reads the ends off the placing
		/// tool's preview, standing at the cell, and passes any spot without one; the planner places
		/// with none, so it makes the same test from the cell.
		/// </summary>
		private static bool JointPlateEndsClear(BuildingDef target, int cell, Orientation orientation)
		{
			if (target.BuildLocationRule != BuildLocationRule.HighWattBridgeTile)
				return true;
			UtilityNetworkLink link = target.BuildingPreview.GetComponent<UtilityNetworkLink>() ?? target.BuildingComplete.GetComponent<UtilityNetworkLink>();
			if (link == null)
				return true;
			if (!link.AreCellsValid(cell, orientation))
				return false;
			link.GetCells(cell, orientation, out int first, out int second);
			foreach (int end in new[] { first, second })
			{
				if (Grid.Objects[end, (int)ObjectLayer.WireConnectors] != null || Grid.Objects[end, (int)ObjectLayer.FoundationTile] != null || Grid.HasDoor[end])
					return false;
				BuildingUnderConstruction order = Grid.Objects[end, (int)ObjectLayer.Building]?.GetComponent<BuildingUnderConstruction>();
				if (order != null && order.Def.BuildingComplete.GetComponent<Door>() != null)
					return false;
			}
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
				FinishReconstruct(building);
			return true;
		}

		/// <summary>
		/// Instant build: the deconstruction is finished on the spot (the game finishes its own
		/// deconstruct tool's only in debug mode). That commences the reconstruct, and
		/// <see cref="FollowRebuild"/> finishes the new building's order as soon as it is there, the
		/// way instant build finishes a replacement.
		/// </summary>
		private static void FinishReconstruct(BuildingComplete building)
		{
			Deconstructable deconstructable = building.GetComponent<Deconstructable>();
			if (deconstructable == null)
				return;
			finishing = building.gameObject;
			try
			{
				deconstructable.CompleteWork(null);
			}
			finally
			{
				finishing = null;
			}
		}

		/// <summary>The building <see cref="FinishReconstruct"/> is taking down, while it does.</summary>
		private static GameObject finishing;

		/// <summary>
		/// Called as a reconstruct commences, the old building still standing: sees its new order
		/// through. The game places that order from its scheduler the frame after; this hands it the
		/// old building's Blueprints Expanded settings, places it if the game could not, and finishes
		/// it on the spot for a rebuild <see cref="FinishReconstruct"/> began.
		/// <para>The settings are handed over here, not left to Blueprints Expanded: it gives them to
		/// whatever stands in the cell 0.1 game seconds after, from the same scheduler, which keeps no
		/// order among entries due together, so at speed 3 it could find the old building still there
		/// and the new one was built with none.</para>
		/// <para>The game places the order only if the spot is valid by then. For a wide tile (a
		/// 3-wide farm tile) it is not yet: the old tile's cells are still being cleared, the placement
		/// is refused, and the building would be gone with nothing ordered in its place.</para>
		/// </summary>
		public static void FollowRebuild(Reconstructable reconstructable)
		{
			Building building = reconstructable.GetComponent<Building>();
			Deconstructable deconstructable = reconstructable.GetComponent<Deconstructable>();
			if (building == null || deconstructable == null || !deconstructable.allowDeconstruction || !reconstructable.ReconstructRequested)
				return;
			var rebuild = new Rebuild
			{
				Old = building.gameObject,
				Def = building.Def,
				Cell = Grid.PosToCell(building),
				Position = building.transform.position,
				Orientation = building.Orientation,
				Materials = (Tag[])ReconstructMaterials(reconstructable).Clone(),
				Facade = reconstructable.GetComponent<BuildingFacade>()?.CurrentFacade,
				Settings = BlueprintsCompat.TakeSettings(building.gameObject),
				Name = building.name,
				Finish = building.gameObject == finishing,
			};
			Game.Instance.StartCoroutine(Follow(rebuild));
		}

		/// <summary>Unpaused frames <see cref="FollowRebuild"/> keeps trying to place the order before giving the building up.</summary>
		private const int PlaceFrames = 240;

		/// <summary>Game seconds the game's own placement is given first.</summary>
		private const float PlacementGrace = 0.5f;

		/// <summary>A reconstruct being seen through: what the old building was, read while it stood.</summary>
		private sealed class Rebuild
		{
			public GameObject Old;
			public BuildingDef Def;
			public int Cell;
			public Vector3 Position;
			public Orientation Orientation;
			public Tag[] Materials;
			public string Facade;
			public Dictionary<string, JObject> Settings;
			public string Name;
			public bool Finish;

			/// <summary>The new building's order, once the game has placed it (the old building can stand a frame longer).</summary>
			public GameObject Placed()
			{
				GameObject there = Grid.Objects[Cell, (int)Def.ObjectLayer];
				return there != null && there != Old && there.GetComponent<Building>()?.Def == Def ? there : null;
			}

			/// <summary>The order the game did not place, placed as it would have.</summary>
			public GameObject Place()
			{
				GameObject order = Def.TryPlace(null, Position, Orientation, Materials, Facade, restrictToActiveWorld: false);
				if (order != null)
					Debug.Log("[UpgradePlanner] Placed the rebuilt " + Def.PrefabID + " at cell " + Cell + ".");
				return order;
			}

			/// <summary>The order given the settings and, for an instant rebuild, finished.</summary>
			public void Complete(GameObject order)
			{
				BlueprintsCompat.GiveSettings(Settings, order, Name);
				if (Finish && order.GetComponent<Constructable>() != null)
					Orders.Finish(order, Def, Materials);
			}
		}

		/// <summary>
		/// Waits for the game's placement, then places the order itself; either way completes it.
		/// Once an order is there, whatever happens to it after (finished and taken down within the
		/// grace, say) is not for this to undo.
		/// </summary>
		private static IEnumerator Follow(Rebuild rebuild)
		{
			int frames = 0;
			float due = GameClock.Instance.GetTime() + PlacementGrace;
			while (GameClock.Instance.GetTime() < due)
			{
				yield return null;
				GameObject order = rebuild.Placed();
				if (order != null)
				{
					rebuild.Complete(order);
					yield break;
				}
				if (!Paused)
					frames++;
			}
			yield return EachUnpausedFrame(PlaceFrames - frames, () =>
			{
				GameObject order = rebuild.Placed() ?? rebuild.Place();
				if (order == null)
					return false;
				rebuild.Complete(order);
				return true;
			}, () => Debug.LogWarning("[UpgradePlanner] Could not place the rebuilt " + rebuild.Def.PrefabID + " at cell " + rebuild.Cell + "."), checkFirst: true);
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
