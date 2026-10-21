using System;
using UnityEngine;

namespace Life
{
	public class Flash : LifeBase
	{
		private Renderer _renderer;

		private Material _material;

		private Material[] _mats;

		private Color _colourA;

		private Color _colourB;

		public Flash(Renderer r, float duration, float oscillations, Color c, Material[] mats, int matIndex)
		{
			_mats = mats;
			_material = _mats[matIndex];
			_colourA = _material.GetColor("_Color");
			_colourB = c;
			_renderer = r;
			Start(null, duration, 1f, oscillations);
		}

		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float num = 1f;
			if (base.Duration > 0f)
			{
				num = Misc.H0(base.Elapsed / base.Duration);
			}
			float t = 0.5f * (1f + base.Amplitude * Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI * 2f)) * num;
			_material.color = Color.Lerp(_colourA, _colourB, t);
			_renderer.materials = _mats;
			return !Finished();
		}

		public override void Restore()
		{
			base.Restore();
			_material.color = _colourA;
			_renderer.materials = _mats;
		}
	}
}
