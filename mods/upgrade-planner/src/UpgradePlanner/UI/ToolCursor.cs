using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The tool's two cursors, the game's own yellow tool arrow (Dig's, Sweep's) with a badge at its
	/// lower right: ready, the tool icon's green dashed plate; not ready (the plan unfinished or
	/// changed since Apply), a "no" sign in the Cancel arrow's red. Each badge is outlined in a dark
	/// shade of its own colour, as the game's arrows are.
	/// </summary>
	internal static class ToolCursor
	{
		/// <summary>Square, the arrow in its top left corner and the badge in its bottom right.</summary>
		private const int Size = 48;
		private const int Badge = 20;
		/// <summary>Where the badge's bottom left corner sits, in texture pixels from the bottom left.</summary>
		private const int BadgeX = 26, BadgeY = 2;

		// The tool icon's greens (ToolIcon), with a dark outline as the arrows have.
		private static readonly Color GreenDash = new Color32(168, 228, 168, 255);
		private static readonly Color GreenBand = new Color32(22, 158, 22, 255);
		private static readonly Color GreenPlate = new Color32(16, 140, 16, 255);
		private static readonly Color GreenOutline = new Color32(10, 48, 10, 255);
		// The Cancel arrow's red and its outline, read off cursor_arrow_red.
		private static readonly Color RedMid = new Color32(244, 74, 71, 255);
		private static readonly Color RedOutline = new Color32(68, 21, 20, 255);

		public static Texture2D Ready { get; private set; }
		public static Texture2D Prohibited { get; private set; }

		/// <summary>Draws both on <paramref name="arrow"/>, the game's yellow tool arrow; once.</summary>
		public static void Make(Texture2D arrow)
		{
			if (Ready != null || arrow == null)
				return;
			Color[] source = Readable(arrow);
			Ready = Draw("isochronous_upgradeplanner_cursor", source, arrow.width, arrow.height, PlateBadge());
			Prohibited = Draw("isochronous_upgradeplanner_cursor_no", source, arrow.width, arrow.height, NoBadge());
		}

		private static Texture2D Draw(string name, Color[] arrow, int width, int height, Color[] badge)
		{
			var pixels = new Color[Size * Size];
			int top = Size - height;
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width && x < Size; x++)
					pixels[(top + y) * Size + x] = arrow[y * width + x];
			for (int y = 0; y < Badge; y++)
				for (int x = 0; x < Badge; x++)
				{
					int at = (BadgeY + y) * Size + BadgeX + x;
					pixels[at] = Over(badge[y * Badge + x], pixels[at]);
				}
			var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: false)
			{
				name = name,
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp,
			};
			texture.SetPixels(pixels);
			texture.Apply();
			return texture;
		}

		/// <summary>
		/// The tool icon's plate at badge size (<see cref="ToolIcon"/> draws it large): outlined, green
		/// with a lighter band down the middle, and a pale dashed border two pixels wide just inside
		/// the edge.
		/// </summary>
		private static Color[] PlateBadge()
		{
			var badge = new Color[Badge * Badge];
			const int dash = 3, gap = 2, last = Badge - 1;
			for (int y = 0; y < Badge; y++)
				for (int x = 0; x < Badge; x++)
				{
					Color c;
					if (x == 0 || y == 0 || x == last || y == last)
						c = GreenOutline;
					else if (x == 1 || y == 1 || x == last - 1 || y == last - 1)
						c = GreenPlate;
					else if (x <= 3 || y <= 3 || x >= last - 3 || y >= last - 3)
					{
						// Along the side this pixel is on: dashes on the left and right run up, the rest across.
						int along = x <= 3 || x >= last - 3 ? y - 2 : x - 2;
						c = along % (dash + gap) < dash ? GreenDash : GreenPlate;
					}
					else
						c = x >= 7 && x <= 12 ? GreenBand : GreenPlate;
					badge[y * Badge + x] = c;
				}
			return badge;
		}

		/// <summary>A ring with a slash from top left to bottom right, edged in its outline shade, smoothed by sampling each pixel 4x4.</summary>
		private static Color[] NoBadge()
		{
			var badge = new Color[Badge * Badge];
			const int samples = 4;
			float centre = Badge / 2f, outer = Badge / 2f - 1.5f, inner = outer - 3f, half = 1.5f;
			for (int y = 0; y < Badge; y++)
				for (int x = 0; x < Badge; x++)
				{
					int red = 0, dark = 0;
					for (int sy = 0; sy < samples; sy++)
						for (int sx = 0; sx < samples; sx++)
						{
							float px = x + (sx + 0.5f) / samples - centre, py = y + (sy + 0.5f) / samples - centre;
							float r = Mathf.Sqrt(px * px + py * py);
							// The slash runs from top left to bottom right: in texture rows (y up), along x = -y.
							float across = Mathf.Abs(px + py) / Mathf.Sqrt(2f);
							if (r <= outer && (r >= inner || across <= half))
								red++;
							else if (r <= outer + 1f && (r >= inner - 1f || across <= half + 1f))
								dark++;
						}
					if (red + dark == 0)
						continue;
					Color c = (RedMid * red + RedOutline * dark) / (red + dark);
					c.a = (red + dark) / (float)(samples * samples);
					badge[y * Badge + x] = c;
				}
			return badge;
		}

		/// <summary><paramref name="top"/> laid over <paramref name="under"/>.</summary>
		private static Color Over(Color top, Color under)
		{
			float a = top.a + under.a * (1f - top.a);
			if (a <= 0f)
				return Color.clear;
			Color c = (top * top.a + under * under.a * (1f - top.a)) / a;
			c.a = a;
			return c;
		}

		/// <summary>The pixels of a texture the game keeps unreadable, copied through a render texture.</summary>
		internal static Color[] Readable(Texture2D texture)
		{
			RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			Graphics.Blit(texture, rt);
			RenderTexture was = RenderTexture.active;
			RenderTexture.active = rt;
			var copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, mipChain: false);
			copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			copy.Apply();
			RenderTexture.active = was;
			RenderTexture.ReleaseTemporary(rt);
			Color[] pixels = copy.GetPixels();
			Object.Destroy(copy);
			return pixels;
		}
	}
}
