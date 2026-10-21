using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.Decomposition
{
	public static class FlipcodeDecomposer
	{
		private static FVector2 _tmpA;

		private static FVector2 _tmpB;

		private static FVector2 _tmpC;

		private static bool InsideTriangle(ref FVector2 a, ref FVector2 b, ref FVector2 c, ref FVector2 p)
		{
			float num = (c.X - b.X) * (p.Y - b.Y) - (c.Y - b.Y) * (p.X - b.X);
			float num2 = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
			float num3 = (a.X - c.X) * (p.Y - c.Y) - (a.Y - c.Y) * (p.X - c.X);
			return num >= 0f && num3 >= 0f && num2 >= 0f;
		}

		private static bool Snip(Vertices contour, int u, int v, int w, int n, int[] V)
		{
			if (1.1920929E-07f > MathUtils.Area(ref _tmpA, ref _tmpB, ref _tmpC))
			{
				return false;
			}
			for (int i = 0; i < n; i++)
			{
				if (i != u && i != v && i != w)
				{
					FVector2 p = contour[V[i]];
					if (InsideTriangle(ref _tmpA, ref _tmpB, ref _tmpC, ref p))
					{
						return false;
					}
				}
			}
			return true;
		}

		public static List<Vertices> ConvexPartition(Vertices contour)
		{
			int count = contour.Count;
			if (count < 3)
			{
				return new List<Vertices>();
			}
			int[] array = new int[count];
			if (contour.IsCounterClockWise())
			{
				for (int i = 0; i < count; i++)
				{
					array[i] = i;
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					array[j] = count - 1 - j;
				}
			}
			int num = count;
			int num2 = 2 * num;
			List<Vertices> list = new List<Vertices>();
			int num3 = num - 1;
			while (num > 2)
			{
				if (0 >= num2--)
				{
					return new List<Vertices>();
				}
				int num4 = num3;
				if (num <= num4)
				{
					num4 = 0;
				}
				num3 = num4 + 1;
				if (num <= num3)
				{
					num3 = 0;
				}
				int num5 = num3 + 1;
				if (num <= num5)
				{
					num5 = 0;
				}
				_tmpA = contour[array[num4]];
				_tmpB = contour[array[num3]];
				_tmpC = contour[array[num5]];
				if (Snip(contour, num4, num3, num5, num, array))
				{
					Vertices vertices = new Vertices(3);
					vertices.Add(_tmpA);
					vertices.Add(_tmpB);
					vertices.Add(_tmpC);
					list.Add(vertices);
					int num6 = num3;
					for (int k = num3 + 1; k < num; k++)
					{
						array[num6] = array[k];
						num6++;
					}
					num--;
					num2 = 2 * num;
				}
			}
			return list;
		}
	}
}
