using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts
{
	public sealed class VelocityConstraintPoint
	{
		public FVector2 rA;

		public FVector2 rB;

		public float normalImpulse;

		public float tangentImpulse;

		public float normalMass;

		public float tangentMass;

		public float velocityBias;
	}
}
