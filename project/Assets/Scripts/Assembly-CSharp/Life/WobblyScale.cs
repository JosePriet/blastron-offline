using System;
using UnityEngine;

namespace Life
{
	public class WobblyScale : LifeBase
	{
		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float num = 1f + base.Amplitude * (Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI * 2f) * Misc.H0(base.Elapsed / base.Duration));
			base.Transform.localScale = base.RestScale * num;
			return !Finished();
		}
	}
}
