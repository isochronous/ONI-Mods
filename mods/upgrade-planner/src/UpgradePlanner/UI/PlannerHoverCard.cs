using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>
	/// The tool's hover card: the game's title and instructions, and while a box is being dragged, a
	/// second bar laid out as the game's overlay cards lay theirs (the decor card's, say): a title,
	/// then a dashed line per type the drag would change with its count, then the materials all of
	/// it takes under their own heading.
	/// </summary>
	internal sealed class PlannerHoverCard : HoverTextConfiguration
	{
		/// <summary>"Type: count" per type in the box, and "mass material" per material; empty when no box holds anything.</summary>
		public List<string> Types = new List<string>(), Materials = new List<string>();

		public override void UpdateHoverElements(List<KSelectable> hover_objects)
		{
			HoverTextScreen screen = HoverTextScreen.Instance;
			HoverTextDrawer drawer = screen.BeginDrawing();
			int cell = Grid.PosToCell(UnityEngine.Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));
			if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] != ClusterManager.Instance.activeWorldId)
			{
				drawer.EndDrawing();
				return;
			}
			drawer.BeginShadowBar();
			DrawTitle(screen, drawer);
			DrawInstructions(screen, drawer);
			drawer.EndShadowBar();
			if (Types.Count > 0)
			{
				UnityEngine.Sprite dash = screen.GetSprite("dash");
				drawer.BeginShadowBar();
				drawer.DrawText(ModStrings.UPGRADEPLANNER.HoverSelected, Styles_Title.Standard);
				foreach (string type in Types)
				{
					drawer.NewLine(18);
					drawer.DrawIcon(dash);
					drawer.DrawText(type, Styles_BodyText.Standard);
				}
				if (Materials.Count > 0)
				{
					drawer.NewLine();
					drawer.DrawText(ModStrings.UPGRADEPLANNER.HoverMaterials, Styles_BodyText.Standard);
					foreach (string material in Materials)
					{
						drawer.NewLine(18);
						drawer.DrawIcon(dash);
						drawer.DrawText(material, Styles_BodyText.Standard);
					}
				}
				drawer.EndShadowBar();
			}
			drawer.EndDrawing();
		}
	}
}
