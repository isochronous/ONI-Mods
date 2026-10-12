using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// Registers the toolbar icon: a procedurally drawn glyph (a pipe run with an upward
	/// chevron over it) in the game's white-on-transparent tool-icon style. Replace with
	/// embedded art later; the drawn glyph keeps the toolbar button visible meanwhile.
	/// </summary>
	internal static class ToolIcon
	{
		public const string SpriteName = "isochronous_upgradeplanner_tool";

		private const int Size = 64;

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

			// A horizontal pipe along the bottom third: two rails with a gap.
			FillRect(pixels, 4, 10, 56, 5);
			FillRect(pixels, 4, 21, 56, 5);
			// Pipe joints.
			FillRect(pixels, 16, 8, 5, 20);
			FillRect(pixels, 43, 8, 5, 20);

			// Upward chevron above the pipe.
			Stroke(pixels, 12, 36, 32, 56, 6);
			Stroke(pixels, 32, 56, 52, 36, 6);

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

		private static void FillRect(Color32[] pixels, int x, int y, int width, int height)
		{
			Color32 white = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			for (int row = y; row < y + height; row++)
				for (int col = x; col < x + width; col++)
					if (col >= 0 && col < Size && row >= 0 && row < Size)
						pixels[row * Size + col] = white;
		}

		/// <summary>A thick line from (x0,y0) to (x1,y1), drawn as a stack of short vertical bars.</summary>
		private static void Stroke(Color32[] pixels, int x0, int y0, int x1, int y1, int thickness)
		{
			if (x1 == x0)
				return;
			for (int x = x0; x <= x1; x++)
			{
				int y = y0 + (y1 - y0) * (x - x0) / (x1 - x0);
				FillRect(pixels, x, y - thickness / 2, 1, thickness);
			}
		}
	}
}
