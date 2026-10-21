using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PolygonManipulation
{
	public static class YuPengClipper
	{
		private sealed class Edge
		{
			public FVector2 EdgeStart { get; private set; }

			public FVector2 EdgeEnd { get; private set; }

			public Edge(FVector2 edgeStart, FVector2 edgeEnd)
			{
				EdgeStart = edgeStart;
				EdgeEnd = edgeEnd;
			}

			public FVector2 GetCenter()
			{
				return (EdgeStart + EdgeEnd) / 2f;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					return false;
				}
				return Equals(obj as Edge);
			}

			public bool Equals(Edge e)
			{
				if (e == null)
				{
					return false;
				}
				return VectorEqual(EdgeStart, e.EdgeStart) && VectorEqual(EdgeEnd, e.EdgeEnd);
			}

			public override int GetHashCode()
			{
				return EdgeStart.GetHashCode() ^ EdgeEnd.GetHashCode();
			}

			public static Edge operator -(Edge e)
			{
				return new Edge(e.EdgeEnd, e.EdgeStart);
			}
		}

		private const float ClipperEpsilonSquared = 1.1920929E-07f;

		public static List<Vertices> Union(Vertices polygon1, Vertices polygon2, out PolyClipError error)
		{
			return Execute(polygon1, polygon2, PolyClipType.Union, out error);
		}

		public static List<Vertices> Difference(Vertices polygon1, Vertices polygon2, out PolyClipError error)
		{
			return Execute(polygon1, polygon2, PolyClipType.Difference, out error);
		}

		public static List<Vertices> Intersect(Vertices polygon1, Vertices polygon2, out PolyClipError error)
		{
			return Execute(polygon1, polygon2, PolyClipType.Intersect, out error);
		}

		private static List<Vertices> Execute(Vertices subject, Vertices clip, PolyClipType clipType, out PolyClipError error)
		{
			Vertices slicedPoly;
			Vertices slicedPoly2;
			CalculateIntersections(subject, clip, out slicedPoly, out slicedPoly2);
			FVector2 value = subject.GetCollisionBox().LowerBound;
			FVector2 value2 = clip.GetCollisionBox().LowerBound;
			FVector2 result;
			FVector2.Min(ref value, ref value2, out result);
			result = FVector2.One - result;
			if (result != FVector2.Zero)
			{
				slicedPoly.Translate(ref result);
				slicedPoly2.Translate(ref result);
			}
			slicedPoly.ForceCounterClockWise();
			slicedPoly2.ForceCounterClockWise();
			List<float> coeff;
			List<Edge> simplicies;
			CalculateSimplicalChain(slicedPoly, out coeff, out simplicies);
			List<float> coeff2;
			List<Edge> simplicies2;
			CalculateSimplicalChain(slicedPoly2, out coeff2, out simplicies2);
			List<Edge> resultSimplices;
			CalculateResultChain(coeff, simplicies, coeff2, simplicies2, clipType, out resultSimplices);
			List<Vertices> result2;
			error = BuildPolygonsFromChain(resultSimplices, out result2);
			result *= -1f;
			for (int i = 0; i < result2.Count; i++)
			{
				result2[i].Translate(ref result);
				SimplifyTools.CollinearSimplify(result2[i]);
			}
			return result2;
		}

		private static void CalculateIntersections(Vertices polygon1, Vertices polygon2, out Vertices slicedPoly1, out Vertices slicedPoly2)
		{
			slicedPoly1 = new Vertices(polygon1);
			slicedPoly2 = new Vertices(polygon2);
			for (int i = 0; i < polygon1.Count; i++)
			{
				FVector2 fVector = polygon1[i];
				FVector2 fVector2 = polygon1[polygon1.NextIndex(i)];
				for (int j = 0; j < polygon2.Count; j++)
				{
					FVector2 fVector3 = polygon2[j];
					FVector2 fVector4 = polygon2[polygon2.NextIndex(j)];
					FVector2 intersectionPoint;
					if (!LineTools.LineIntersect(fVector, fVector2, fVector3, fVector4, out intersectionPoint))
					{
						continue;
					}
					float alpha = GetAlpha(fVector, fVector2, intersectionPoint);
					if (alpha > 0f && alpha < 1f)
					{
						int k;
						for (k = slicedPoly1.IndexOf(fVector) + 1; k < slicedPoly1.Count && GetAlpha(fVector, fVector2, slicedPoly1[k]) <= alpha; k++)
						{
						}
						slicedPoly1.Insert(k, intersectionPoint);
					}
					alpha = GetAlpha(fVector3, fVector4, intersectionPoint);
					if (alpha > 0f && alpha < 1f)
					{
						int l;
						for (l = slicedPoly2.IndexOf(fVector3) + 1; l < slicedPoly2.Count && GetAlpha(fVector3, fVector4, slicedPoly2[l]) <= alpha; l++)
						{
						}
						slicedPoly2.Insert(l, intersectionPoint);
					}
				}
			}
			for (int m = 0; m < slicedPoly1.Count; m++)
			{
				int index = slicedPoly1.NextIndex(m);
				if ((slicedPoly1[index] - slicedPoly1[m]).LengthSquared() <= 1.1920929E-07f)
				{
					slicedPoly1.RemoveAt(m);
					m--;
				}
			}
			for (int n = 0; n < slicedPoly2.Count; n++)
			{
				int index2 = slicedPoly2.NextIndex(n);
				if ((slicedPoly2[index2] - slicedPoly2[n]).LengthSquared() <= 1.1920929E-07f)
				{
					slicedPoly2.RemoveAt(n);
					n--;
				}
			}
		}

		private static void CalculateSimplicalChain(Vertices poly, out List<float> coeff, out List<Edge> simplicies)
		{
			simplicies = new List<Edge>();
			coeff = new List<float>();
			for (int i = 0; i < poly.Count; i++)
			{
				simplicies.Add(new Edge(poly[i], poly[poly.NextIndex(i)]));
				coeff.Add(CalculateSimplexCoefficient(FVector2.Zero, poly[i], poly[poly.NextIndex(i)]));
			}
		}

		private static void CalculateResultChain(List<float> poly1Coeff, List<Edge> poly1Simplicies, List<float> poly2Coeff, List<Edge> poly2Simplicies, PolyClipType clipType, out List<Edge> resultSimplices)
		{
			resultSimplices = new List<Edge>();
			for (int i = 0; i < poly1Simplicies.Count; i++)
			{
				float num = 0f;
				if (poly2Simplicies.Contains(poly1Simplicies[i]) || (poly2Simplicies.Contains(-poly1Simplicies[i]) && clipType == PolyClipType.Union))
				{
					num = 1f;
				}
				else
				{
					for (int j = 0; j < poly2Simplicies.Count; j++)
					{
						if (!poly2Simplicies.Contains(-poly1Simplicies[i]))
						{
							num += CalculateBeta(poly1Simplicies[i].GetCenter(), poly2Simplicies[j], poly2Coeff[j]);
						}
					}
				}
				if (clipType == PolyClipType.Intersect)
				{
					if (num == 1f)
					{
						resultSimplices.Add(poly1Simplicies[i]);
					}
				}
				else if (num == 0f)
				{
					resultSimplices.Add(poly1Simplicies[i]);
				}
			}
			for (int k = 0; k < poly2Simplicies.Count; k++)
			{
				if (resultSimplices.Contains(poly2Simplicies[k]) || resultSimplices.Contains(-poly2Simplicies[k]))
				{
					continue;
				}
				float num2 = 0f;
				if (poly1Simplicies.Contains(poly2Simplicies[k]) || (poly1Simplicies.Contains(-poly2Simplicies[k]) && clipType == PolyClipType.Union))
				{
					num2 = 1f;
				}
				else
				{
					for (int l = 0; l < poly1Simplicies.Count; l++)
					{
						if (!poly1Simplicies.Contains(-poly2Simplicies[k]))
						{
							num2 += CalculateBeta(poly2Simplicies[k].GetCenter(), poly1Simplicies[l], poly1Coeff[l]);
						}
					}
				}
				if (clipType == PolyClipType.Intersect || clipType == PolyClipType.Difference)
				{
					if (num2 == 1f)
					{
						resultSimplices.Add(-poly2Simplicies[k]);
					}
				}
				else if (num2 == 0f)
				{
					resultSimplices.Add(poly2Simplicies[k]);
				}
			}
		}

		private static PolyClipError BuildPolygonsFromChain(List<Edge> simplicies, out List<Vertices> result)
		{
			result = new List<Vertices>();
			PolyClipError result2 = PolyClipError.None;
			while (simplicies.Count > 0)
			{
				Vertices vertices = new Vertices();
				vertices.Add(simplicies[0].EdgeStart);
				vertices.Add(simplicies[0].EdgeEnd);
				simplicies.RemoveAt(0);
				bool flag = false;
				int num = 0;
				int count = simplicies.Count;
				while (!flag && simplicies.Count > 0)
				{
					if (VectorEqual(vertices[vertices.Count - 1], simplicies[num].EdgeStart))
					{
						if (VectorEqual(simplicies[num].EdgeEnd, vertices[0]))
						{
							flag = true;
						}
						else
						{
							vertices.Add(simplicies[num].EdgeEnd);
						}
						simplicies.RemoveAt(num);
						num--;
					}
					else if (VectorEqual(vertices[vertices.Count - 1], simplicies[num].EdgeEnd))
					{
						if (VectorEqual(simplicies[num].EdgeStart, vertices[0]))
						{
							flag = true;
						}
						else
						{
							vertices.Add(simplicies[num].EdgeStart);
						}
						simplicies.RemoveAt(num);
						num--;
					}
					if (!flag && ++num == simplicies.Count)
					{
						if (count == simplicies.Count)
						{
							result = new List<Vertices>();
							return PolyClipError.BrokenResult;
						}
						num = 0;
						count = simplicies.Count;
					}
				}
				if (vertices.Count < 3)
				{
					result2 = PolyClipError.DegeneratedOutput;
				}
				result.Add(vertices);
			}
			return result2;
		}

		private static float CalculateBeta(FVector2 point, Edge e, float coefficient)
		{
			float result = 0f;
			if (PointInSimplex(point, e))
			{
				result = coefficient;
			}
			if (PointOnLineSegment(FVector2.Zero, e.EdgeStart, point) || PointOnLineSegment(FVector2.Zero, e.EdgeEnd, point))
			{
				result = 0.5f * coefficient;
			}
			return result;
		}

		private static float GetAlpha(FVector2 start, FVector2 end, FVector2 point)
		{
			return (point - start).LengthSquared() / (end - start).LengthSquared();
		}

		private static float CalculateSimplexCoefficient(FVector2 a, FVector2 b, FVector2 c)
		{
			float num = MathUtils.Area(ref a, ref b, ref c);
			if (num < 0f)
			{
				return -1f;
			}
			if (num > 0f)
			{
				return 1f;
			}
			return 0f;
		}

		private static bool PointInSimplex(FVector2 point, Edge edge)
		{
			Vertices vertices = new Vertices();
			vertices.Add(FVector2.Zero);
			vertices.Add(edge.EdgeStart);
			vertices.Add(edge.EdgeEnd);
			return vertices.PointInPolygon(ref point) == 1;
		}

		private static bool PointOnLineSegment(FVector2 start, FVector2 end, FVector2 point)
		{
			FVector2 value = end - start;
			return MathUtils.Area(ref start, ref end, ref point) == 0f && FVector2.Dot(point - start, value) >= 0f && FVector2.Dot(point - end, value) <= 0f;
		}

		private static bool VectorEqual(FVector2 vec1, FVector2 vec2)
		{
			return (vec2 - vec1).LengthSquared() <= 1.1920929E-07f;
		}
	}
}
