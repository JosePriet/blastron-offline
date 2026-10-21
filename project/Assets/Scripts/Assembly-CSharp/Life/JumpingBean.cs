using System;
using UnityEngine;

namespace Life
{
	public class JumpingBean : LifeBase
	{
		private Vector3 _vel;

		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float y = base.Amplitude * Mathf.Abs(Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI / base.Duration)) * Misc.H0(base.Elapsed / base.Duration);
			base.Transform.position = base.RestPos + new Vector3(0f, y, 0f);
			return !Finished();
		}
	}
}
