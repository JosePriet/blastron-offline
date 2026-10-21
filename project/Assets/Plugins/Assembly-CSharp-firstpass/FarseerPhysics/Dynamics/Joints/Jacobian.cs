using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	internal struct Jacobian
	{
		public float AngularA;

		public float AngularB;

		public FVector2 Linear;

		public void SetZero()
		{
			AngularA = 0f;
			Linear = FVector2.Zero;
			AngularB = 0f;
		}
	}
}
