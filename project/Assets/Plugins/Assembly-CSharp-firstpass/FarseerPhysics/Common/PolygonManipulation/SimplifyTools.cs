using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PolygonManipulation
{
	public static class SimplifyTools
	{
		private static bool[] _usePt;

		private static double _distanceTolerance;

		public static Vertices CollinearSimplify(Vertices vertices, float collinearityTolerance)
		{
			if (vertices.Count < 3)
			{
				return vertices;
			}
			Vertices vertices2 = new Vertices();
			for (int i = 0; i < vertices.Count; i++)
			{
				int index = vertices.PreviousIndex(i);
				int index2 = vertices.NextIndex(i);
				FVector2 a = vertices[index];
				FVector2 b = vertices[i];
				FVector2 c = vertices[index2];
				if (!MathUtils.Collinear(ref a, ref b, ref c, collinearityTolerance))
				{
					vertices2.Add(b);
				}
			}
			return vertices2;
		}

		public static Vertices CollinearSimplify(Vertices vertices)
		{
			return CollinearSimplify(vertices, 0f);
		}

		public static Vertices DouglasPeuckerSimplify(Vertices vertices, float distanceTolerance)
		{
			_distanceTolerance = distanceTolerance;
			_usePt = new bool[vertices.Count];
			for (int i = 0; i < vertices.Count; i++)
			{
				_usePt[i] = true;
			}
			SimplifySection(vertices, 0, vertices.Count - 1);
			Vertices vertices2 = new Vertices();
			for (int j = 0; j < vertices.Count; j++)
			{
				if (_usePt[j])
				{
					vertices2.Add(vertices[j]);
				}
			}
			return vertices2;
		}

		private static void SimplifySection(Vertices vertices, int i, int j)
		{
			if (i + 1 == j)
			{
				return;
			}
			FVector2 a = vertices[i];
			FVector2 b = vertices[j];
			double num = -1.0;
			int num2 = i;
			for (int k = i + 1; k < j; k++)
			{
				double num3 = DistancePointLine(vertices[k], a, b);
				if (num3 > num)
				{
					num = num3;
					num2 = k;
				}
			}
			if (num <= _distanceTolerance)
			{
				for (int l = i + 1; l < j; l++)
				{
					_usePt[l] = false;
				}
			}
			else
			{
				SimplifySection(vertices, i, num2);
				SimplifySection(vertices, num2, j);
			}
		}

		private static double DistancePointPoint(FVector2 p, FVector2 p2)
		{
			double num = p.X - p2.X;
			double num2 = p.Y - p2.X;
			return Math.Sqrt(num * num + num2 * num2);
		}

		private static double DistancePointLine(FVector2 p, FVector2 A, FVector2 B)
		{
			if (A.X == B.X && A.Y == B.Y)
			{
				return DistancePointPoint(p, A);
			}
			double num = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y)) / ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
			if (num <= 0.0)
			{
				return DistancePointPoint(p, A);
			}
			if (num >= 1.0)
			{
				return DistancePointPoint(p, B);
			}
			double value = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y)) / ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
			return Math.Abs(value) * Math.Sqrt((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
		}

		public static Vertices ReduceByArea(Vertices vertices, float areaTolerance)
		{
			if (vertices.Count <= 3)
			{
				return vertices;
			}
			if (areaTolerance < 0f)
			{
				throw new ArgumentOutOfRangeException("areaTolerance", "must be equal to or greater then zero.");
			}
			Vertices vertices2 = new Vertices();
			FVector2 a = vertices[vertices.Count - 2];
			FVector2 b = vertices[vertices.Count - 1];
			areaTolerance *= 2f;
			int num = 0;
			while (num < vertices.Count)
			{
				FVector2 b2;
				if (num == vertices.Count - 1)
				{
					if (vertices2.Count == 0)
					{
						throw new ArgumentOutOfRangeException("areaTolerance", "The tolerance is too high!");
					}
					b2 = vertices2[0];
				}
				else
				{
					b2 = vertices[num];
				}
				float c;
				MathUtils.Cross(ref a, ref b, out c);
				float c2;
				MathUtils.Cross(ref b, ref b2, out c2);
				float c3;
				MathUtils.Cross(ref a, ref b2, out c3);
				if (Math.Abs(c3 - (c + c2)) > areaTolerance)
				{
					vertices2.Add(b);
					a = b;
				}
				num++;
				b = b2;
			}
			return vertices2;
		}

		public static void MergeParallelEdges(Vertices vertices, float tolerance)
		{
			if (vertices.Count <= 3)
			{
				return;
			}
			bool[] array = new bool[vertices.Count];
			int num = vertices.Count;
			for (int i = 0; i < vertices.Count; i++)
			{
				int index = ((i != 0) ? (i - 1) : (vertices.Count - 1));
				int index2 = i;
				int index3 = ((i != vertices.Count - 1) ? (i + 1) : 0);
				float num2 = vertices[index2].X - vertices[index].X;
				float num3 = vertices[index2].Y - vertices[index].Y;
				float num4 = vertices[index3].Y - vertices[index2].X;
				float num5 = vertices[index3].Y - vertices[index2].Y;
				float num6 = (float)Math.Sqrt(num2 * num2 + num3 * num3);
				float num7 = (float)Math.Sqrt(num4 * num4 + num5 * num5);
				if ((!(num6 > 0f) || !(num7 > 0f)) && num > 3)
				{
					array[i] = true;
					num--;
				}
				num2 /= num6;
				num3 /= num6;
				num4 /= num7;
				num5 /= num7;
				float value = num2 * num5 - num4 * num3;
				float num8 = num2 * num4 + num3 * num5;
				if (Math.Abs(value) < tolerance && num8 > 0f && num > 3)
				{
					array[i] = true;
					num--;
				}
				else
				{
					array[i] = false;
				}
			}
			if (num == vertices.Count || num == 0)
			{
				return;
			}
			int num9 = 0;
			Vertices vertices2 = new Vertices(vertices);
			vertices.Clear();
			for (int j = 0; j < vertices2.Count; j++)
			{
				if (!array[j] && num != 0 && num9 != num)
				{
					vertices.Add(vertices2[j]);
					num9++;
				}
			}
		}

		public static Vertices MergeIdenticalPoints(Vertices vertices)
		{
			HashSet<FVector2> hashSet = new HashSet<FVector2>();
			for (int i = 0; i < vertices.Count; i++)
			{
				hashSet.Add(vertices[i]);
			}
			Vertices vertices2 = new Vertices();
			foreach (FVector2 item in hashSet)
			{
				vertices2.Add(item);
			}
			return vertices2;
		}

		public static Vertices ReduceByDistance(Vertices vertices, float distance)
		{
			if (vertices.Count < 3)
			{
				return vertices;
			}
			Vertices vertices2 = new Vertices();
			for (int i = 0; i < vertices.Count; i++)
			{
				FVector2 fVector = vertices[i];
				FVector2 fVector2 = vertices.NextVertex(i);
				if (!((fVector2 - fVector).LengthSquared() <= distance))
				{
					vertices2.Add(fVector);
				}
			}
			return vertices2;
		}

		public static Vertices ReduceByNth(Vertices vertices, int nth)
		{
			if (vertices.Count < 3)
			{
				return vertices;
			}
			if (nth == 0)
			{
				return vertices;
			}
			Vertices vertices2 = new Vertices(vertices.Count);
			for (int i = 0; i < vertices.Count; i++)
			{
				if (i % nth != 0)
				{
					vertices2.Add(vertices[i]);
				}
			}
			return vertices2;
		}
	}
}
