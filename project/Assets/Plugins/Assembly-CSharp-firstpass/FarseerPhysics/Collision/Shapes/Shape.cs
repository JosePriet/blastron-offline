using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	public abstract class Shape
	{
		private static int _shapeIdCounter;

		public MassData MassData;

		public int ShapeId;

		internal float _density;

		internal float _radius;

		public ShapeType ShapeType { get; internal set; }

		public abstract int ChildCount { get; }

		public float Density
		{
			get
			{
				return _density;
			}
			set
			{
				_density = value;
				ComputeProperties();
			}
		}

		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
				ComputeProperties();
			}
		}

		protected Shape(float density)
		{
			_density = density;
			ShapeType = ShapeType.Unknown;
			ShapeId = _shapeIdCounter++;
		}

		public abstract Shape Clone();

		public abstract bool TestPoint(ref Transform transform, ref FVector2 point);

		public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex);

		public abstract void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex);

		protected abstract void ComputeProperties();

		public bool CompareTo(Shape shape)
		{
			if (shape is PolygonShape && this is PolygonShape)
			{
				return ((PolygonShape)this).CompareTo((PolygonShape)shape);
			}
			if (shape is CircleShape && this is CircleShape)
			{
				return ((CircleShape)this).CompareTo((CircleShape)shape);
			}
			if (shape is EdgeShape && this is EdgeShape)
			{
				return ((EdgeShape)this).CompareTo((EdgeShape)shape);
			}
			return false;
		}

		public abstract float ComputeSubmergedArea(FVector2 normal, float offset, Transform xf, out FVector2 sc);
	}
}
