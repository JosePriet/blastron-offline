using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	public class CircleShape : Shape
	{
		internal FVector2 _position;

		public override int ChildCount
		{
			get
			{
				return 1;
			}
		}

		public FVector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				ComputeProperties();
			}
		}

		public CircleShape(float radius, float density)
			: base(density)
		{
			base.ShapeType = ShapeType.Circle;
			_radius = radius;
			_position = FVector2.Zero;
			ComputeProperties();
		}

		internal CircleShape()
			: base(0f)
		{
			base.ShapeType = ShapeType.Circle;
			_radius = 0f;
			_position = FVector2.Zero;
		}

		public override Shape Clone()
		{
			CircleShape circleShape = new CircleShape();
			circleShape._radius = base.Radius;
			circleShape._density = _density;
			circleShape._position = _position;
			circleShape.ShapeType = base.ShapeType;
			circleShape.MassData = MassData;
			return circleShape;
		}

		public override bool TestPoint(ref Transform transform, ref FVector2 point)
		{
			FVector2 fVector = transform.p + MathUtils.Mul(transform.q, Position);
			FVector2 fVector2 = point - fVector;
			return FVector2.Dot(fVector2, fVector2) <= base.Radius * base.Radius;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			output = default(RayCastOutput);
			FVector2 fVector = transform.p + MathUtils.Mul(transform.q, Position);
			FVector2 fVector2 = input.Point1 - fVector;
			float num = FVector2.Dot(fVector2, fVector2) - base.Radius * base.Radius;
			FVector2 fVector3 = input.Point2 - input.Point1;
			float num2 = FVector2.Dot(fVector2, fVector3);
			float num3 = FVector2.Dot(fVector3, fVector3);
			float num4 = num2 * num2 - num3 * num;
			if (num4 < 0f || num3 < 1.1920929E-07f)
			{
				return false;
			}
			float num5 = 0f - (num2 + (float)Math.Sqrt(num4));
			if (0f <= num5 && num5 <= input.MaxFraction * num3)
			{
				output.Normal = fVector2 + (output.Fraction = num5 / num3) * fVector3;
				output.Normal.Normalize();
				return true;
			}
			return false;
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			FVector2 fVector = transform.p + MathUtils.Mul(transform.q, Position);
			aabb.LowerBound = new FVector2(fVector.X - base.Radius, fVector.Y - base.Radius);
			aabb.UpperBound = new FVector2(fVector.X + base.Radius, fVector.Y + base.Radius);
		}

		protected sealed override void ComputeProperties()
		{
			float num = (float)Math.PI * base.Radius * base.Radius;
			MassData.Area = num;
			MassData.Mass = base.Density * num;
			MassData.Centroid = Position;
			MassData.Inertia = MassData.Mass * (0.5f * base.Radius * base.Radius + FVector2.Dot(Position, Position));
		}

		public bool CompareTo(CircleShape shape)
		{
			return base.Radius == shape.Radius && Position == shape.Position;
		}

		public override float ComputeSubmergedArea(FVector2 normal, float offset, Transform xf, out FVector2 sc)
		{
			sc = FVector2.Zero;
			FVector2 fVector = MathUtils.Mul(ref xf, Position);
			float num = 0f - (FVector2.Dot(normal, fVector) - offset);
			if (num < 0f - base.Radius + 1.1920929E-07f)
			{
				return 0f;
			}
			if (num > base.Radius)
			{
				sc = fVector;
				return (float)Math.PI * base.Radius * base.Radius;
			}
			float num2 = base.Radius * base.Radius;
			float num3 = num * num;
			float num4 = num2 * (float)(Math.Asin(num / base.Radius) + 1.5707963705062866 + (double)num * Math.Sqrt(num2 - num3));
			float num5 = -2f / 3f * (float)Math.Pow(num2 - num3, 1.5) / num4;
			sc.X = fVector.X + normal.X * num5;
			sc.Y = fVector.Y + normal.Y * num5;
			return num4;
		}
	}
}
