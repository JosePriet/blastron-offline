namespace Poly2Tri.Triangulation.Polygon
{
	public class PolygonPoint : TriangulationPoint
	{
		public PolygonPoint Next { get; set; }

		public PolygonPoint Previous { get; set; }

		public PolygonPoint(double x, double y)
			: base(x, y)
		{
		}
	}
}
