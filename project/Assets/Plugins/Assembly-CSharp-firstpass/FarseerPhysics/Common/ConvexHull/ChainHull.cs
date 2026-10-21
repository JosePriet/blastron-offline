using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.ConvexHull
{
	public static class ChainHull
	{
		public class PointComparer : Comparer<FVector2>
		{
			public override int Compare(FVector2 a, FVector2 b)
			{
				int num = a.X.CompareTo(b.X);
				return (num == 0) ? a.Y.CompareTo(b.Y) : num;
			}
		}

		public static Vertices GetConvexHull(Vertices P)
		{
			P.Sort(new PointComparer());
			FVector2[] array = new FVector2[P.Count];
			Vertices vertices = new Vertices();
			int count = P.Count;
			int num = -1;
			int num2 = 0;
			float x = P[0].X;
			int i;
			for (i = 1; i < count && P[i].X == x; i++)
			{
			}
			int num3 = i - 1;
			if (num3 == count - 1)
			{
				array[++num] = P[num2];
				if (P[num3].Y != P[num2].Y)
				{
					array[++num] = P[num3];
				}
				array[++num] = P[num2];
				for (int j = 0; j < num + 1; j++)
				{
					vertices.Add(array[j]);
				}
				return vertices;
			}
			num = vertices.Count - 1;
			int num4 = count - 1;
			float x2 = P[count - 1].X;
			i = count - 2;
			while (i >= 0 && P[i].X == x2)
			{
				i--;
			}
			int num5 = i + 1;
			array[++num] = P[num2];
			i = num3;
			while (++i <= num5)
			{
				if (!(MathUtils.Area(P[num2], P[num5], P[i]) >= 0f) || i >= num5)
				{
					while (num > 0 && !(MathUtils.Area(array[num - 1], array[num], P[i]) > 0f))
					{
						num--;
					}
					array[++num] = P[i];
				}
			}
			if (num4 != num5)
			{
				array[++num] = P[num4];
			}
			int num6 = num;
			i = num5;
			while (--i >= num3)
			{
				if (!(MathUtils.Area(P[num4], P[num3], P[i]) >= 0f) || i <= num3)
				{
					while (num > num6 && !(MathUtils.Area(array[num - 1], array[num], P[i]) > 0f))
					{
						num--;
					}
					array[++num] = P[i];
				}
			}
			if (num3 != num2)
			{
				array[++num] = P[num2];
			}
			for (int k = 0; k < num + 1; k++)
			{
				vertices.Add(array[k]);
			}
			return vertices;
		}
	}
}
