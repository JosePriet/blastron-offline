using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public struct Rot
	{
		public float s;

		public float c;

		public Rot(float angle)
		{
			s = (float)Math.Sin(angle);
			c = (float)Math.Cos(angle);
		}

		public void Set(float angle)
		{
			s = (float)Math.Sin(angle);
			c = (float)Math.Cos(angle);
		}

		public void SetIdentity()
		{
			s = 0f;
			c = 1f;
		}

		public float GetAngle()
		{
			return (float)Math.Atan2(s, c);
		}

		public FVector2 GetXAxis()
		{
			return new FVector2(c, s);
		}

		public FVector2 GetYAxis()
		{
			return new FVector2(0f - s, c);
		}
	}
}
