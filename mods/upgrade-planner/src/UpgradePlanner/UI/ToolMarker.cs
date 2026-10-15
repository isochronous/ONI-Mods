using UnityEngine;

namespace UpgradePlanner
{
	/// <summary>
	/// The marker that follows the cursor over the cells while the tool is active: a copy of the
	/// Sweep tool's (corner brackets round a picture of the tool, white, tinted by its material) with
	/// the tool icon's dashed border in place of Sweep's dolly: a dashed square over a fill of the
	/// same colour at <see cref="Fill"/> opacity, so the cell shows through. A copy, so Sweep keeps
	/// its own.
	/// </summary>
	internal static class ToolMarker
	{
		/// <summary>The donor picture's middle, where its tool picture sits inside the corner brackets.</summary>
		private const float Middle = 0.34f;
		/// <summary>The square's half width, its thickness, and its dash and gap, as shares of the picture's size.</summary>
		private const float Plate = 0.27f, Thick = 0.034f, Dash = 0.055f, Gap = 0.035f;
		/// <summary>The square's fill opacity; the material tints it the brackets' colour.</summary>
		private const float Fill = 0.4f;

		private static Texture2D picture;

		/// <summary>A copy of <paramref name="donor"/>, Sweep's marker, showing the dashed square; inactive, as the donor is between uses.</summary>
		public static GameObject Make(GameObject donor)
		{
			if (donor == null)
				return null;
			GameObject marker = Object.Instantiate(donor, donor.transform.parent);
			marker.name = "UpgradePlannerMarker";
			marker.SetActive(false);
			foreach (MeshRenderer renderer in marker.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
			{
				Texture2D original = renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture as Texture2D : null;
				if (original == null)
					continue;
				if (picture == null)
					picture = Draw(original);
				// An instance of the material, so Sweep's is left as it is.
				renderer.material.mainTexture = picture;
			}
			return marker;
		}

		/// <summary>The donor's corner brackets kept, its middle cleared, and the dashed square drawn there.</summary>
		private static Texture2D Draw(Texture2D original)
		{
			int size = original.width;
			Color[] pixels = Readable(original);
			float half = size / 2f;
			for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
				{
					float dx = Mathf.Abs(x + 0.5f - half) / size, dy = Mathf.Abs(y + 0.5f - half) / size;
					if (dx > Middle || dy > Middle)
						continue;
					pixels[y * size + x] = new Color(1f, 1f, 1f, PlateAlpha(dx, dy));
				}
			var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true)
			{
				name = "isochronous_upgradeplanner_marker",
				filterMode = original.filterMode,
				wrapMode = TextureWrapMode.Clamp,
			};
			texture.SetPixels(pixels);
			texture.Apply();
			return texture;
		}

		/// <summary>
		/// The dashed square's opacity, solid on a dash and <see cref="Fill"/> elsewhere inside:
		/// <paramref name="dx"/> and <paramref name="dy"/> are the distance from the middle, as
		/// shares of the picture's size.
		/// </summary>
		private static float PlateAlpha(float dx, float dy)
		{
			float far = Mathf.Max(dx, dy);
			if (far > Plate)
				return 0f;
			if (far <= Plate - Thick)
				return Fill;
			// Along the side this pixel is on, from the corner, dashes then gaps.
			float along = Plate - Mathf.Min(dx, dy);
			return along % (Dash + Gap) < Dash ? 1f : Fill;
		}

		/// <summary>The pixels of a texture the game keeps unreadable, copied through a render texture.</summary>
		private static Color[] Readable(Texture2D texture)
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
