using System.Collections.Generic;
using Poly2Tri.Triangulation.Delaunay;

namespace Poly2Tri.Triangulation
{
	public interface Triangulatable
	{
		IList<TriangulationPoint> Points { get; }

		IList<DelaunayTriangle> Triangles { get; }

		TriangulationMode TriangulationMode { get; }

		void PrepareTriangulation(TriangulationContext tcx);

		void AddTriangle(DelaunayTriangle t);

		void AddTriangles(IEnumerable<DelaunayTriangle> list);

		void ClearTriangles();
	}
}
