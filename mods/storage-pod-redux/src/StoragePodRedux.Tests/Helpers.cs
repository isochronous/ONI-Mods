using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace StoragePodRedux.Tests
{
	internal static class Helpers
	{
		/// <summary>The finished building of the def whose origin is the cell, looked up by position now.</summary>
		public static GameObject Finished(int cell, string prefabId)
		{
			BuildingDef def = Assets.GetBuildingDef(prefabId);
			foreach (BuildingComplete b in Components.BuildingCompletes.Items)
				if (b != null && b.Def == def && Grid.PosToCell(b.transform.GetPosition()) == cell)
					return b.gameObject;
			return null;
		}

		/// <summary>
		/// A player's placement with the real build tool, as the build menu leaves it: activated with the
		/// def and a material, the pointer moved to the cell, the button pressed and released there. Instant
		/// build is on (the harness default), so the building stands at once.
		/// </summary>
		public static IEnumerator PlaceWithTool(TestContext t, string prefabId, int cell, SimHashes material = SimHashes.Copper)
		{
			BuildingDef def = Assets.GetBuildingDef(prefabId);
			t.Assert.NotNull(def, "the game has a building def " + prefabId);
			yield return UtilityDragSession.PrimeBuildMenu(t);
			BuildTool tool = BuildTool.Instance;
			tool.Activate(def, new List<Tag> { material.CreateTag() });
			Isochronous.TestRunner.Harness.ForceFocus();
			t.Cleanup(() =>
			{
				if (tool.Dragging)
					tool.CancelDragging();
				if (PlayerController.Instance.ActiveTool == tool)
					PlayerController.Instance.ActivateTool(SelectTool.Instance);
			});
			Vector3 pos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Building);
			tool.OnMouseMove(pos);
			yield return t.Frames(3);
			tool.OnLeftClickDown(pos);
			yield return t.Frames(3);
			tool.OnLeftClickUp(pos);
			yield return t.WaitUntil(() => Finished(cell, prefabId) != null, 5, "the " + prefabId + " to stand at " + Cells.Describe(cell));
			PlayerController.Instance.ActivateTool(SelectTool.Instance);
			yield return t.WaitGameSeconds(1);
		}

		/// <summary>The mod's internal type, by name.</summary>
		public static System.Type ModType(string name)
		{
			return typeof(StoragePodReduxMod).Assembly.GetType("StoragePodRedux." + name, true);
		}
	}
}
