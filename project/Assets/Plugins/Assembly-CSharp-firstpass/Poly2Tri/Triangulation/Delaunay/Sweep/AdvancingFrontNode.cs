namespace Poly2Tri.Triangulation.Delaunay.Sweep
{
	public class AdvancingFrontNode
	{
		public AdvancingFrontNode Next;

		public TriangulationPoint Point;

		public AdvancingFrontNode Prev;

		public DelaunayTriangle Triangle;

		public double Value;

		public bool HasNext
		{
			get
			{
				return Next != null;
			}
		}

		public bool HasPrev
		{
			get
			{
				return Prev != null;
			}
		}

		public AdvancingFrontNode(TriangulationPoint point)
		{
			Point = point;
			Value = point.X;
		}
	}
}
