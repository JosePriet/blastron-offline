using UnityEngine;

namespace EB
{
	public static class TextureUtils
	{
		public static Texture2D GetFader(Color color)
		{
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.SetPixel(0, 0, color);
			texture2D.Apply();
			Object.DontDestroyOnLoad(texture2D);
			return texture2D;
		}

		public static Texture2D FromRenderTexture(RenderTexture rt, bool mipMaps = false, bool readable = true)
		{
			Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, mipMaps);
			return FromRenderTexture(rt, texture, mipMaps, readable);
		}

		public static Texture2D FromRenderTexture(RenderTexture rt, Texture2D texture, bool mipMaps = false, bool readable = true)
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = rt;
			texture.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
			RenderTexture.active = active;
			texture.Apply(mipMaps, !readable);
			return texture;
		}

		public static byte[] ToPNG(RenderTexture rt)
		{
			Texture2D texture2D = FromRenderTexture(rt);
			byte[] result = ToPNG(texture2D);
			Object.Destroy(texture2D);
			return result;
		}

		public static byte[] ToPNG(Texture2D texture)
		{
			return texture.EncodeToPNG();
		}

		public static byte[] ToJPG(RenderTexture rt, float quality)
		{
			Texture2D texture2D = FromRenderTexture(rt);
			byte[] result = ToJPG(texture2D, quality);
			Object.Destroy(texture2D);
			return result;
		}

		public static byte[] ToJPG(Texture2D texture, float quality)
		{
			JPGEncoder jPGEncoder = new JPGEncoder(texture, quality);
			return jPGEncoder.GetBytes();
		}
	}
}
