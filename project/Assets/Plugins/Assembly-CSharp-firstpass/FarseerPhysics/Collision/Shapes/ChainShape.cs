using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	public class ChainShape : Shape
	{
		public Vertices Vertices;

		private FVector2 _prevVertex;

		private FVector2 _nextVertex;

		private bool _hasPrevVertex;

		private bool _hasNextVertex;

		public override int ChildCount
		{
			get
			{
				return Vertices.Count - 1;
			}
		}

		private ChainShape()
			: base(0f)
		{
			base.ShapeType = ShapeType.Chain;
			_radius = 0.01f;
		}

		public ChainShape(Vertices vertices)
			: base(0f)
		{
			base.ShapeType = ShapeType.Chain;
			_radius = 0.01f;
			if (Settings.ConserveMemory)
			{
				Vertices = vertices;
			}
			else
			{
				Vertices = new Vertices(vertices);
			}
		}

		public void CreateLoop(Vertices vertices)
		{
			Vertices = new Vertices(vertices);
			Vertices.Add(vertices[0]);
			_prevVertex = Vertices[Vertices.Count - 2];
			_nextVertex = Vertices[1];
			_hasPrevVertex = true;
			_hasNextVertex = true;
		}

		public void CreateChain(Vertices vertices)
		{
			Vertices = new Vertices(vertices);
			_hasPrevVertex = false;
			_hasNextVertex = false;
		}

		public override Shape Clone()
		{
			ChainShape chainShape = new ChainShape();
			chainShape._density = _density;
			chainShape._radius = _radius;
			chainShape.Vertices = Vertices;
			chainShape.MassData = MassData;
			return chainShape;
		}

		public void SetPrevVertex(FVector2 prevVertex)
		{
			_prevVertex = prevVertex;
			_hasPrevVertex = true;
		}

		public void SetNextVertex(FVector2 nextVertex)
		{
			_nextVertex = nextVertex;
			_hasNextVertex = true;
		}

		public void GetChildEdge(ref EdgeShape edge, int index)
		{
			edge.ShapeType = ShapeType.Edge;
			edge._radius = _radius;
			edge.Vertex1 = Vertices[index];
			edge.Vertex2 = Vertices[index + 1];
			if (index > 0)
			{
				edge.Vertex0 = Vertices[index - 1];
				edge.HasVertex0 = true;
			}
			else
			{
				edge.Vertex0 = _prevVertex;
				edge.HasVertex0 = _hasPrevVertex;
			}
			if (index < Vertices.Count - 2)
			{
				edge.Vertex3 = Vertices[index + 2];
				edge.HasVertex3 = true;
			}
			else
			{
				edge.Vertex3 = _nextVertex;
				edge.HasVertex3 = _hasNextVertex;
			}
		}

		public override bool TestPoint(ref Transform transform, ref FVector2 point)
		{
			return false;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			EdgeShape edgeShape = new EdgeShape();
			int num = childIndex + 1;
			if (num == Vertices.Count)
			{
				num = 0;
			}
			edgeShape.Vertex1 = Vertices[childIndex];
			edgeShape.Vertex2 = Vertices[num];
			return edgeShape.RayCast(out output, ref input, ref transform, 0);
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			int num = childIndex + 1;
			if (num == Vertices.Count)
			{
				num = 0;
			}
			FVector2 value = MathUtils.Mul(ref transform, Vertices[childIndex]);
			FVector2 value2 = MathUtils.Mul(ref transform, Vertices[num]);
			aabb.LowerBound = FVector2.Min(value, value2);
			aabb.UpperBound = FVector2.Max(value, value2);
		}

		protected override void ComputeProperties()
		{
		}

		public override float ComputeSubmergedArea(FVector2 normal, float offset, Transform xf, out FVector2 sc)
		{
			sc = FVector2.Zero;
			return 0f;
		}
	}
}
