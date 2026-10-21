using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts
{
	public sealed class ContactPositionConstraint
	{
		public FVector2[] localPoints = new FVector2[2];

		public FVector2 localNormal;

		public FVector2 localPoint;

		public int indexA;

		public int indexB;

		public float invMassA;

		public float invMassB;

		public FVector2 localCenterA;

		public FVector2 localCenterB;

		public float invIA;

		public float invIB;

		public ManifoldType type;

		public float radiusA;

		public float radiusB;

		public int pointCount;
	}
}
