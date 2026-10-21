using System;
using UnityEngine;

namespace Life
{
	public class Shiver : LifeBase
	{
		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float y = base.Amplitude * Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI * 2f) * Misc.H0(base.Elapsed / base.Duration);
			base.Transform.rotation = base.RestRot * Quaternion.Euler(0f, y, 0f);
			return !Finished();
		}
	}
}
