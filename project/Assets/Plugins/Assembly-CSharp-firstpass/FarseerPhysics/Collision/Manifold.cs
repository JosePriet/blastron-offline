using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	public struct Manifold
	{
		public FVector2 LocalNormal;

		public FVector2 LocalPoint;

		public int PointCount;

		public FixedArray2<ManifoldPoint> Points;

		public ManifoldType Type;
	}
}
