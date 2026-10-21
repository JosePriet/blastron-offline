using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public struct Transform
	{
		public FVector2 p;

		public Rot q;

		public Transform(ref FVector2 position, ref Rot rotation)
		{
			p = position;
			q = rotation;
		}

		public void SetIdentity()
		{
			p = FVector2.Zero;
			q.SetIdentity();
		}

		public void Set(FVector2 position, float angle)
		{
			p = position;
			q.Set(angle);
		}
	}
}
