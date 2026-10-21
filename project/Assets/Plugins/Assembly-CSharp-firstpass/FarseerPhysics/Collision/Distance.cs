using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	public static class Distance
	{
		public static int GJKCalls;

		public static int GJKIters;

		public static int GJKMaxIters;

		public static void ComputeDistance(out DistanceOutput output, out SimplexCache cache, DistanceInput input)
		{
			cache = default(SimplexCache);
			GJKCalls++;
			Simplex simplex = default(Simplex);
			simplex.ReadCache(ref cache, input.ProxyA, ref input.TransformA, input.ProxyB, ref input.TransformB);
			FixedArray3<int> fixedArray = default(FixedArray3<int>);
			FixedArray3<int> fixedArray2 = default(FixedArray3<int>);
			float num = simplex.GetClosestPoint().LengthSquared();
			float num2 = num;
			int num3 = 0;
			while (num3 < 20)
			{
				int count = simplex.Count;
				for (int i = 0; i < count; i++)
				{
					fixedArray[i] = simplex.V[i].IndexA;
					fixedArray2[i] = simplex.V[i].IndexB;
				}
				switch (simplex.Count)
				{
				case 2:
					simplex.Solve2();
					break;
				case 3:
					simplex.Solve3();
					break;
				}
				if (simplex.Count == 3)
				{
					break;
				}
				num2 = simplex.GetClosestPoint().LengthSquared();
				if (num2 >= num)
				{
				}
				num = num2;
				FVector2 searchDirection = simplex.GetSearchDirection();
				if (searchDirection.LengthSquared() < 1.4210855E-14f)
				{
					break;
				}
				SimplexVertex value = simplex.V[simplex.Count];
				value.IndexA = input.ProxyA.GetSupport(MathUtils.MulT(input.TransformA.q, -searchDirection));
				value.WA = MathUtils.Mul(ref input.TransformA, input.ProxyA.Vertices[value.IndexA]);
				value.IndexB = input.ProxyB.GetSupport(MathUtils.MulT(input.TransformB.q, searchDirection));
				value.WB = MathUtils.Mul(ref input.TransformB, input.ProxyB.Vertices[value.IndexB]);
				value.W = value.WB - value.WA;
				simplex.V[simplex.Count] = value;
				num3++;
				GJKIters++;
				bool flag = false;
				for (int j = 0; j < count; j++)
				{
					if (value.IndexA == fixedArray[j] && value.IndexB == fixedArray2[j])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				simplex.Count++;
			}
			GJKMaxIters = Math.Max(GJKMaxIters, num3);
			simplex.GetWitnessPoints(out output.PointA, out output.PointB);
			output.Distance = (output.PointA - output.PointB).Length();
			output.Iterations = num3;
			simplex.WriteCache(ref cache);
			if (input.UseRadii)
			{
				float radius = input.ProxyA.Radius;
				float radius2 = input.ProxyB.Radius;
				if (output.Distance > radius + radius2 && output.Distance > 1.1920929E-07f)
				{
					output.Distance -= radius + radius2;
					FVector2 fVector = output.PointB - output.PointA;
					fVector.Normalize();
					output.PointA += radius * fVector;
					output.PointB -= radius2 * fVector;
				}
				else
				{
					output.PointB = (output.PointA = 0.5f * (output.PointA + output.PointB));
					output.Distance = 0f;
				}
			}
		}
	}
}
