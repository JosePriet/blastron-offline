using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	public class EdgeShape : Shape
	{
		public bool HasVertex0;

		public bool HasVertex3;

		public FVector2 Vertex0;

		public FVector2 Vertex3;

		internal FVector2 _vertex1;

		internal FVector2 _vertex2;

		public override int ChildCount
		{
			get
			{
				return 1;
			}
		}

		public FVector2 Vertex1
		{
			get
			{
				return _vertex1;
			}
			set
			{
				_vertex1 = value;
				ComputeProperties();
			}
		}

		public FVector2 Vertex2
		{
			get
			{
				return _vertex2;
			}
			set
			{
				_vertex2 = value;
				ComputeProperties();
			}
		}

		internal EdgeShape()
			: base(0f)
		{
			base.ShapeType = ShapeType.Edge;
			_radius = 0.01f;
		}

		public EdgeShape(FVector2 start, FVector2 end)
			: base(0f)
		{
			base.ShapeType = ShapeType.Edge;
			_radius = 0.01f;
			Set(start, end);
		}

		public void Set(FVector2 start, FVector2 end)
		{
			_vertex1 = start;
			_vertex2 = end;
			HasVertex0 = false;
			HasVertex3 = false;
			ComputeProperties();
		}

		public override Shape Clone()
		{
			EdgeShape edgeShape = new EdgeShape();
			edgeShape._radius = _radius;
			edgeShape._density = _density;
			edgeShape.HasVertex0 = HasVertex0;
			edgeShape.HasVertex3 = HasVertex3;
			edgeShape.Vertex0 = Vertex0;
			edgeShape._vertex1 = _vertex1;
			edgeShape._vertex2 = _vertex2;
			edgeShape.Vertex3 = Vertex3;
			edgeShape.MassData = MassData;
			return edgeShape;
		}

		public override bool TestPoint(ref Transform transform, ref FVector2 point)
		{
			return false;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			output = default(RayCastOutput);
			FVector2 fVector = MathUtils.MulT(transform.q, input.Point1 - transform.p);
			FVector2 fVector2 = MathUtils.MulT(transform.q, input.Point2 - transform.p);
			FVector2 fVector3 = fVector2 - fVector;
			FVector2 vertex = _vertex1;
			FVector2 vertex2 = _vertex2;
			FVector2 fVector4 = vertex2 - vertex;
			FVector2 fVector5 = new FVector2(fVector4.Y, 0f - fVector4.X);
			fVector5.Normalize();
			float num = FVector2.Dot(fVector5, vertex - fVector);
			float num2 = FVector2.Dot(fVector5, fVector3);
			if (num2 == 0f)
			{
				return false;
			}
			float num3 = num / num2;
			if (num3 < 0f || input.MaxFraction < num3)
			{
				return false;
			}
			FVector2 fVector6 = fVector + num3 * fVector3;
			FVector2 fVector7 = vertex2 - vertex;
			float num4 = FVector2.Dot(fVector7, fVector7);
			if (num4 == 0f)
			{
				return false;
			}
			float num5 = FVector2.Dot(fVector6 - vertex, fVector7) / num4;
			if (num5 < 0f || 1f < num5)
			{
				return false;
			}
			output.Fraction = num3;
			if (num > 0f)
			{
				output.Normal = -fVector5;
			}
			else
			{
				output.Normal = fVector5;
			}
			return true;
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			FVector2 value = MathUtils.Mul(ref transform, _vertex1);
			FVector2 value2 = MathUtils.Mul(ref transform, _vertex2);
			FVector2 fVector = FVector2.Min(value, value2);
			FVector2 fVector2 = FVector2.Max(value, value2);
			FVector2 fVector3 = new FVector2(base.Radius, base.Radius);
			aabb.LowerBound = fVector - fVector3;
			aabb.UpperBound = fVector2 + fVector3;
		}

		protected override void ComputeProperties()
		{
			MassData.Centroid = 0.5f * (_vertex1 + _vertex2);
		}

		public override float ComputeSubmergedArea(FVector2 normal, float offset, Transform xf, out FVector2 sc)
		{
			sc = FVector2.Zero;
			return 0f;
		}

		public bool CompareTo(EdgeShape shape)
		{
			return HasVertex0 == shape.HasVertex0 && HasVertex3 == shape.HasVertex3 && Vertex0 == shape.Vertex0 && Vertex1 == shape.Vertex1 && Vertex2 == shape.Vertex2 && Vertex3 == shape.Vertex3;
		}
	}
}
