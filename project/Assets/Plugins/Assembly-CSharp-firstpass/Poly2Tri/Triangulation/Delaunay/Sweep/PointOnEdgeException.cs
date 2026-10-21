using System;

namespace Poly2Tri.Triangulation.Delaunay.Sweep
{
	public class PointOnEdgeException : NotImplementedException
	{
		public PointOnEdgeException(string message)
			: base(message)
		{
		}
	}
}
