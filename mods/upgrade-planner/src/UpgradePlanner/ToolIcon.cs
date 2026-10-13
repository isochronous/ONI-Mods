using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Registers the tool's icon, drawn at start-up: a green plate with a pale dashed border
	/// and a light plus in the middle, the design Factorio's own upgrade planner uses, which
	/// this tool is modelled on.
	/// </summary>
	internal static class ToolIcon
	{
		public const string SpriteName = "isochronous_upgradeplanner_tool";

		private const int Size = 64;
		private static readonly Color32 Plate = new Color32(16, 140, 16, 255);
		private static readonly Color32 PlateLight = new Color32(22, 158, 22, 255);
		private static readonly Color32 Dash = new Color32(168, 228, 168, 255);
		private static readonly Color32 Plus = new Color32(222, 222, 222, 255);

		public static void Register()
		{
			if (Assets.Sprites == null || Assets.Sprites.ContainsKey(SpriteName))
				return;
			Sprite sprite = Draw();
			sprite.name = SpriteName;
			Assets.Sprites.Add(SpriteName, sprite);
		}

		private static Sprite Draw()
		{
			Color32[] pixels = new Color32[Size * Size];

			// The plate, a shade lighter down the middle band, as the original shades it.
			FillRect(pixels, 0, 0, Size, Size, Plate);
			FillRect(pixels, 20, 0, 24, Size, PlateLight);

			// A dashed border just inside the edge: 6 px dashes, 4 px gaps, 4 px thick.
			const int inset = 2, thick = 4, dash = 6, gap = 4;
			for (int start = inset; start < Size - inset; start += dash + gap)
			{
				int length = Mathf.Min(dash, Size - inset - start);
				FillRect(pixels, start, inset, length, thick, Dash);
				FillRect(pixels, start, Size - inset - thick, length, thick, Dash);
				FillRect(pixels, inset, start, thick, length, Dash);
				FillRect(pixels, Size - inset - thick, start, thick, length, Dash);
			}

			// The plus: arms 10 px thick, 36 px long, centred.
			FillRect(pixels, 14, 27, 36, 10, Plus);
			FillRect(pixels, 27, 14, 10, 36, Plus);

			Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: false)
			{
				name = SpriteName,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
			};
			texture.SetPixels32(pixels);
			texture.Apply();
			return Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f));
		}

		private static void FillRect(Color32[] pixels, int x, int y, int width, int height, Color32 color)
		{
			for (int row = y; row < y + height; row++)
				for (int col = x; col < x + width; col++)
					if (col >= 0 && col < Size && row >= 0 && row < Size)
						pixels[row * Size + col] = color;
		}
	}
}
