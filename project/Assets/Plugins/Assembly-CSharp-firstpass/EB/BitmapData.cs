using UnityEngine;

namespace EB
{
	internal class BitmapData
	{
		public int width;

		public int height;

		private Texture2D _text;

		private float _scale;

		private float _invWidth;

		private float _invHeight;

		public BitmapData(Texture2D texture, float scale)
		{
			_text = texture;
			height = Mathf.RoundToInt((float)texture.height * scale);
			width = Mathf.RoundToInt((float)texture.width * scale);
			_invWidth = 1f / (float)width;
			_invHeight = 1f / (float)height;
		}

		public Color getPixelColor(int x, int y)
		{
			float u = Mathf.Clamp01((float)x * _invWidth);
			float v = Mathf.Clamp01((float)y * _invHeight);
			return _text.GetPixelBilinear(u, v);
		}
	}
}
