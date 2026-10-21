using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Delaunay;
using Poly2Tri.Triangulation.Delaunay.Sweep;
using Poly2Tri.Triangulation.Polygon;
using UnityEngine;

namespace FarseerPhysics.Common.Decomposition
{
	public static class CDTDecomposer
	{
		public static List<Vertices> ConvexPartition(Vertices vertices)
		{
			Polygon polygon = new Polygon();
			foreach (FVector2 vertex in vertices)
			{
				polygon.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));
			}
			DTSweepContext dTSweepContext = new DTSweepContext();
			dTSweepContext.PrepareTriangulation(polygon);
			DTSweep.Triangulate(dTSweepContext);
			List<Vertices> list = new List<Vertices>();
			foreach (DelaunayTriangle triangle in polygon.Triangles)
			{
				Vertices vertices2 = new Vertices();
				foreach (TriangulationPoint point in triangle.Points)
				{
					vertices2.Add(new FVector2((float)point.X, (float)point.Y));
				}
				list.Add(vertices2);
			}
			return list;
		}

		public static List<Vertices> ConvexPartition(DetectedVertices vertices)
		{
			Debug.Log("DEF DECOMPOSING DETECTEDVERTICES");
			Polygon polygon = new Polygon();
			foreach (FVector2 vertex in vertices)
			{
				polygon.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));
			}
			if (vertices.Holes != null)
			{
				foreach (Vertices hole in vertices.Holes)
				{
					Polygon polygon2 = new Polygon();
					foreach (FVector2 item in hole)
					{
						polygon2.Points.Add(new TriangulationPoint(item.X, item.Y));
					}
					polygon.AddHole(polygon2);
				}
			}
			DTSweepContext dTSweepContext = new DTSweepContext();
			dTSweepContext.PrepareTriangulation(polygon);
			DTSweep.Triangulate(dTSweepContext);
			List<Vertices> list = new List<Vertices>();
			foreach (DelaunayTriangle triangle in polygon.Triangles)
			{
				Vertices vertices2 = new Vertices();
				foreach (TriangulationPoint point in triangle.Points)
				{
					vertices2.Add(new FVector2((float)point.X, (float)point.Y));
				}
				list.Add(vertices2);
			}
			return list;
		}

		public static List<Vertices> ConvexPartition(List<DetectedVertices> vertices)
		{
			List<Vertices> list = new List<Vertices>();
			foreach (DetectedVertices vertex in vertices)
			{
				list.AddRange(ConvexPartition(vertex));
			}
			return list;
		}
	}
}
