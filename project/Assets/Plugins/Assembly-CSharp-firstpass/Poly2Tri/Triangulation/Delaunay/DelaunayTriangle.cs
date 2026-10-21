using System;
using System.Collections.Generic;
using Poly2Tri.Triangulation.Delaunay.Sweep;
using Poly2Tri.Triangulation.Util;

namespace Poly2Tri.Triangulation.Delaunay
{
	public class DelaunayTriangle
	{
		public FixedBitArray3 EdgeIsConstrained;

		public FixedBitArray3 EdgeIsDelaunay;

		public FixedArray3<DelaunayTriangle> Neighbors;

		public FixedArray3<TriangulationPoint> Points;

		public bool IsInterior { get; set; }

		public DelaunayTriangle(TriangulationPoint p1, TriangulationPoint p2, TriangulationPoint p3)
		{
			Points[0] = p1;
			Points[1] = p2;
			Points[2] = p3;
		}

		public int IndexOf(TriangulationPoint p)
		{
			int num = Points.IndexOf(p);
			if (num == -1)
			{
				throw new Exception("Calling index with a point that doesn't exist in triangle");
			}
			return num;
		}

		public int IndexCW(TriangulationPoint p)
		{
			switch (IndexOf(p))
			{
			case 0:
				return 2;
			case 1:
				return 0;
			default:
				return 1;
			}
		}

		public int IndexCCW(TriangulationPoint p)
		{
			switch (IndexOf(p))
			{
			case 0:
				return 1;
			case 1:
				return 2;
			default:
				return 0;
			}
		}

		public bool Contains(TriangulationPoint p)
		{
			return p == Points[0] || p == Points[1] || p == Points[2];
		}

		public bool Contains(DTSweepConstraint e)
		{
			return Contains(e.P) && Contains(e.Q);
		}

		public bool Contains(TriangulationPoint p, TriangulationPoint q)
		{
			return Contains(p) && Contains(q);
		}

		private void MarkNeighbor(TriangulationPoint p1, TriangulationPoint p2, DelaunayTriangle t)
		{
			if ((p1 == Points[2] && p2 == Points[1]) || (p1 == Points[1] && p2 == Points[2]))
			{
				Neighbors[0] = t;
			}
			else if ((p1 == Points[0] && p2 == Points[2]) || (p1 == Points[2] && p2 == Points[0]))
			{
				Neighbors[1] = t;
			}
			else if ((p1 == Points[0] && p2 == Points[1]) || (p1 == Points[1] && p2 == Points[0]))
			{
				Neighbors[2] = t;
			}
		}

		public void MarkNeighbor(DelaunayTriangle t)
		{
			if (t.Contains(Points[1], Points[2]))
			{
				Neighbors[0] = t;
				t.MarkNeighbor(Points[1], Points[2], this);
			}
			else if (t.Contains(Points[0], Points[2]))
			{
				Neighbors[1] = t;
				t.MarkNeighbor(Points[0], Points[2], this);
			}
			else if (t.Contains(Points[0], Points[1]))
			{
				Neighbors[2] = t;
				t.MarkNeighbor(Points[0], Points[1], this);
			}
		}

		public void ClearNeighbors()
		{
			DelaunayTriangle delaunayTriangle = null;
			Neighbors[2] = delaunayTriangle;
			delaunayTriangle = delaunayTriangle;
			Neighbors[1] = delaunayTriangle;
			Neighbors[0] = delaunayTriangle;
		}

		public void ClearNeighbor(DelaunayTriangle triangle)
		{
			if (Neighbors[0] == triangle)
			{
				Neighbors[0] = null;
			}
			else if (Neighbors[1] == triangle)
			{
				Neighbors[1] = null;
			}
			else
			{
				Neighbors[2] = null;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < 3; i++)
			{
				DelaunayTriangle delaunayTriangle = Neighbors[i];
				if (delaunayTriangle != null)
				{
					delaunayTriangle.ClearNeighbor(this);
				}
			}
			ClearNeighbors();
			TriangulationPoint triangulationPoint = null;
			Points[2] = triangulationPoint;
			triangulationPoint = triangulationPoint;
			Points[1] = triangulationPoint;
			Points[0] = triangulationPoint;
		}

		public TriangulationPoint OppositePoint(DelaunayTriangle t, TriangulationPoint p)
		{
			return PointCW(t.PointCW(p));
		}

		public DelaunayTriangle NeighborCW(TriangulationPoint point)
		{
			return Neighbors[(Points.IndexOf(point) + 1) % 3];
		}

		public DelaunayTriangle NeighborCCW(TriangulationPoint point)
		{
			return Neighbors[(Points.IndexOf(point) + 2) % 3];
		}

		public DelaunayTriangle NeighborAcross(TriangulationPoint point)
		{
			return Neighbors[Points.IndexOf(point)];
		}

		public TriangulationPoint PointCCW(TriangulationPoint point)
		{
			return Points[(IndexOf(point) + 1) % 3];
		}

		public TriangulationPoint PointCW(TriangulationPoint point)
		{
			return Points[(IndexOf(point) + 2) % 3];
		}

		private void RotateCW()
		{
			TriangulationPoint value = Points[2];
			Points[2] = Points[1];
			Points[1] = Points[0];
			Points[0] = value;
		}

		public void Legalize(TriangulationPoint oPoint, TriangulationPoint nPoint)
		{
			RotateCW();
			Points[IndexCCW(oPoint)] = nPoint;
		}

		public override string ToString()
		{
			return string.Concat(Points[0], ",", Points[1], ",", Points[2]);
		}

		public void MarkNeighborEdges()
		{
			for (int i = 0; i < 3; i++)
			{
				if (EdgeIsConstrained[i] && Neighbors[i] != null)
				{
					Neighbors[i].MarkConstrainedEdge(Points[(i + 1) % 3], Points[(i + 2) % 3]);
				}
			}
		}

		public void MarkEdge(DelaunayTriangle triangle)
		{
			for (int i = 0; i < 3; i++)
			{
				if (EdgeIsConstrained[i])
				{
					triangle.MarkConstrainedEdge(Points[(i + 1) % 3], Points[(i + 2) % 3]);
				}
			}
		}

		public void MarkEdge(List<DelaunayTriangle> tList)
		{
			foreach (DelaunayTriangle t in tList)
			{
				for (int i = 0; i < 3; i++)
				{
					if (t.EdgeIsConstrained[i])
					{
						MarkConstrainedEdge(t.Points[(i + 1) % 3], t.Points[(i + 2) % 3]);
					}
				}
			}
		}

		public void MarkConstrainedEdge(int index)
		{
			EdgeIsConstrained[index] = true;
		}

		public void MarkConstrainedEdge(DTSweepConstraint edge)
		{
			MarkConstrainedEdge(edge.P, edge.Q);
		}

		public void MarkConstrainedEdge(TriangulationPoint p, TriangulationPoint q)
		{
			int num = EdgeIndex(p, q);
			if (num != -1)
			{
				EdgeIsConstrained[num] = true;
			}
		}

		public double Area()
		{
			double num = Points[0].X - Points[1].X;
			double num2 = Points[2].Y - Points[1].Y;
			return Math.Abs(num * num2 * 0.5);
		}

		public TriangulationPoint Centroid()
		{
			double x = (Points[0].X + Points[1].X + Points[2].X) / 3.0;
			double y = (Points[0].Y + Points[1].Y + Points[2].Y) / 3.0;
			return new TriangulationPoint(x, y);
		}

		public int EdgeIndex(TriangulationPoint p1, TriangulationPoint p2)
		{
			int num = Points.IndexOf(p1);
			int num2 = Points.IndexOf(p2);
			bool flag = num == 0 || num2 == 0;
			bool flag2 = num == 1 || num2 == 1;
			bool flag3 = num == 2 || num2 == 2;
			if (flag2 && flag3)
			{
				return 0;
			}
			if (flag && flag3)
			{
				return 1;
			}
			if (flag && flag2)
			{
				return 2;
			}
			return -1;
		}

		public bool GetConstrainedEdgeCCW(TriangulationPoint p)
		{
			return EdgeIsConstrained[(IndexOf(p) + 2) % 3];
		}

		public bool GetConstrainedEdgeCW(TriangulationPoint p)
		{
			return EdgeIsConstrained[(IndexOf(p) + 1) % 3];
		}

		public bool GetConstrainedEdgeAcross(TriangulationPoint p)
		{
			return EdgeIsConstrained[IndexOf(p)];
		}

		public void SetConstrainedEdgeCCW(TriangulationPoint p, bool ce)
		{
			EdgeIsConstrained[(IndexOf(p) + 2) % 3] = ce;
		}

		public void SetConstrainedEdgeCW(TriangulationPoint p, bool ce)
		{
			EdgeIsConstrained[(IndexOf(p) + 1) % 3] = ce;
		}

		public void SetConstrainedEdgeAcross(TriangulationPoint p, bool ce)
		{
			EdgeIsConstrained[IndexOf(p)] = ce;
		}

		public bool GetDelaunayEdgeCCW(TriangulationPoint p)
		{
			return EdgeIsDelaunay[(IndexOf(p) + 2) % 3];
		}

		public bool GetDelaunayEdgeCW(TriangulationPoint p)
		{
			return EdgeIsDelaunay[(IndexOf(p) + 1) % 3];
		}

		public bool GetDelaunayEdgeAcross(TriangulationPoint p)
		{
			return EdgeIsDelaunay[IndexOf(p)];
		}

		public void SetDelaunayEdgeCCW(TriangulationPoint p, bool ce)
		{
			EdgeIsDelaunay[(IndexOf(p) + 2) % 3] = ce;
		}

		public void SetDelaunayEdgeCW(TriangulationPoint p, bool ce)
		{
			EdgeIsDelaunay[(IndexOf(p) + 1) % 3] = ce;
		}

		public void SetDelaunayEdgeAcross(TriangulationPoint p, bool ce)
		{
			EdgeIsDelaunay[IndexOf(p)] = ce;
		}
	}
}
