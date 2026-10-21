using System.Collections.Generic;
using FarseerPhysics.Common.PolygonManipulation;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
	public static class BayazitDecomposer
	{
		private static FVector2 At(int i, Vertices vertices)
		{
			int count = vertices.Count;
			return vertices[(i >= 0) ? (i % count) : (count - -i % count)];
		}

		private static Vertices Copy(int i, int j, Vertices vertices)
		{
			Vertices vertices2 = new Vertices();
			while (j < i)
			{
				j += vertices.Count;
			}
			while (i <= j)
			{
				vertices2.Add(At(i, vertices));
				i++;
			}
			return vertices2;
		}

		public static List<Vertices> ConvexPartition(Vertices vertices)
		{
			vertices.ForceCounterClockWise();
			List<Vertices> list = new List<Vertices>();
			FVector2 fVector = default(FVector2);
			FVector2 fVector2 = default(FVector2);
			int num = 0;
			int i = 0;
			for (int j = 0; j < vertices.Count; j++)
			{
				if (!Reflex(j, vertices))
				{
					continue;
				}
				float num3;
				float num2 = (num3 = float.MaxValue);
				for (int k = 0; k < vertices.Count; k++)
				{
					FVector2 fVector3;
					if (Left(At(j - 1, vertices), At(j, vertices), At(k, vertices)) && RightOn(At(j - 1, vertices), At(j, vertices), At(k - 1, vertices)))
					{
						fVector3 = LineTools.LineIntersect(At(j - 1, vertices), At(j, vertices), At(k, vertices), At(k - 1, vertices));
						if (Right(At(j + 1, vertices), At(j, vertices), fVector3))
						{
							float num4 = SquareDist(At(j, vertices), fVector3);
							if (num4 < num2)
							{
								num2 = num4;
								fVector = fVector3;
								num = k;
							}
						}
					}
					if (!Left(At(j + 1, vertices), At(j, vertices), At(k + 1, vertices)) || !RightOn(At(j + 1, vertices), At(j, vertices), At(k, vertices)))
					{
						continue;
					}
					fVector3 = LineTools.LineIntersect(At(j + 1, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices));
					if (Left(At(j - 1, vertices), At(j, vertices), fVector3))
					{
						float num4 = SquareDist(At(j, vertices), fVector3);
						if (num4 < num3)
						{
							num3 = num4;
							i = k;
							fVector2 = fVector3;
						}
					}
				}
				Vertices vertices2;
				Vertices vertices3;
				if (num == (i + 1) % vertices.Count)
				{
					FVector2 item = (fVector + fVector2) / 2f;
					vertices2 = Copy(j, i, vertices);
					vertices2.Add(item);
					vertices3 = Copy(num, j, vertices);
					vertices3.Add(item);
				}
				else
				{
					double num5 = 0.0;
					double num6 = num;
					for (; i < num; i += vertices.Count)
					{
					}
					for (int l = num; l <= i; l++)
					{
						if (CanSee(j, l, vertices))
						{
							double num7 = 1f / (SquareDist(At(j, vertices), At(l, vertices)) + 1f);
							if (Reflex(l, vertices))
							{
								num7 = ((!RightOn(At(l - 1, vertices), At(l, vertices), At(j, vertices)) || !LeftOn(At(l + 1, vertices), At(l, vertices), At(j, vertices))) ? (num7 + 2.0) : (num7 + 3.0));
							}
							else
							{
								num7++;
							}
							if (num7 > num5)
							{
								num6 = l;
								num5 = num7;
							}
						}
					}
					vertices2 = Copy(j, (int)num6, vertices);
					vertices3 = Copy((int)num6, j, vertices);
				}
				list.AddRange(ConvexPartition(vertices2));
				list.AddRange(ConvexPartition(vertices3));
				return list;
			}
			if (vertices.Count > Settings.MaxPolygonVertices)
			{
				Vertices vertices2 = Copy(0, vertices.Count / 2, vertices);
				Vertices vertices3 = Copy(vertices.Count / 2, 0, vertices);
				list.AddRange(ConvexPartition(vertices2));
				list.AddRange(ConvexPartition(vertices3));
			}
			else
			{
				list.Add(vertices);
			}
			for (int m = 0; m < list.Count; m++)
			{
				list[m] = SimplifyTools.CollinearSimplify(list[m], 0f);
			}
			for (int num8 = list.Count - 1; num8 >= 0; num8--)
			{
				if (list[num8].Count == 0)
				{
					list.RemoveAt(num8);
				}
			}
			return list;
		}

		private static bool CanSee(int i, int j, Vertices vertices)
		{
			if (Reflex(i, vertices))
			{
				if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) && RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)))
				{
					return false;
				}
			}
			else if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) || LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)))
			{
				return false;
			}
			if (Reflex(j, vertices))
			{
				if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) && RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)))
				{
					return false;
				}
			}
			else if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) || LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)))
			{
				return false;
			}
			for (int k = 0; k < vertices.Count; k++)
			{
				FVector2 intersectionPoint;
				if ((k + 1) % vertices.Count != i && k != i && (k + 1) % vertices.Count != j && k != j && LineTools.LineIntersect(At(i, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices), out intersectionPoint))
				{
					return false;
				}
			}
			return true;
		}

		private static bool Reflex(int i, Vertices vertices)
		{
			return Right(i, vertices);
		}

		private static bool Right(int i, Vertices vertices)
		{
			return Right(At(i - 1, vertices), At(i, vertices), At(i + 1, vertices));
		}

		private static bool Left(FVector2 a, FVector2 b, FVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) > 0f;
		}

		private static bool LeftOn(FVector2 a, FVector2 b, FVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) >= 0f;
		}

		private static bool Right(FVector2 a, FVector2 b, FVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) < 0f;
		}

		private static bool RightOn(FVector2 a, FVector2 b, FVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) <= 0f;
		}

		private static float SquareDist(FVector2 a, FVector2 b)
		{
			float num = b.X - a.X;
			float num2 = b.Y - a.Y;
			return num * num + num2 * num2;
		}
	}
}
