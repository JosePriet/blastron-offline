using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	[Serializable]
	public struct AABB
	{
		private static DistanceInput _input = new DistanceInput();

		public FVector2 LowerBound;

		public FVector2 UpperBound;

		public FVector2 Center
		{
			get
			{
				return 0.5f * (LowerBound + UpperBound);
			}
		}

		public FVector2 Extents
		{
			get
			{
				return 0.5f * (UpperBound - LowerBound);
			}
		}

		public float Perimeter
		{
			get
			{
				float num = UpperBound.X - LowerBound.X;
				float num2 = UpperBound.Y - LowerBound.Y;
				return 2f * (num + num2);
			}
		}

		public Vertices Vertices
		{
			get
			{
				Vertices vertices = new Vertices();
				vertices.Add(LowerBound);
				vertices.Add(new FVector2(LowerBound.X, UpperBound.Y));
				vertices.Add(UpperBound);
				vertices.Add(new FVector2(UpperBound.X, LowerBound.Y));
				return vertices;
			}
		}

		public AABB Q1
		{
			get
			{
				return new AABB(Center, UpperBound);
			}
		}

		public AABB Q2
		{
			get
			{
				return new AABB(new FVector2(LowerBound.X, Center.Y), new FVector2(Center.X, UpperBound.Y));
			}
		}

		public AABB Q3
		{
			get
			{
				return new AABB(LowerBound, Center);
			}
		}

		public AABB Q4
		{
			get
			{
				return new AABB(new FVector2(Center.X, LowerBound.Y), new FVector2(UpperBound.X, Center.Y));
			}
		}

		public AABB(FVector2 min, FVector2 max)
			: this(ref min, ref max)
		{
		}

		public AABB(ref FVector2 min, ref FVector2 max)
		{
			LowerBound = min;
			UpperBound = max;
		}

		public AABB(FVector2 center, float width, float height)
		{
			LowerBound = center - new FVector2(width / 2f, height / 2f);
			UpperBound = center + new FVector2(width / 2f, height / 2f);
		}

		public override string ToString()
		{
			return string.Format("[AABB: Center={0}, Extents={1}]", Center, Extents);
		}

		public FVector2[] GetVertices()
		{
			FVector2 upperBound = UpperBound;
			FVector2 fVector = new FVector2(UpperBound.X, LowerBound.Y);
			FVector2 lowerBound = LowerBound;
			FVector2 fVector2 = new FVector2(LowerBound.X, UpperBound.Y);
			return new FVector2[4] { upperBound, fVector, lowerBound, fVector2 };
		}

		public bool IsValid()
		{
			FVector2 fVector = UpperBound - LowerBound;
			return fVector.X >= 0f && fVector.Y >= 0f && LowerBound.IsValid() && UpperBound.IsValid();
		}

		public void Combine(ref AABB aabb)
		{
			LowerBound = FVector2.Min(LowerBound, aabb.LowerBound);
			UpperBound = FVector2.Max(UpperBound, aabb.UpperBound);
		}

		public void Combine(ref AABB aabb1, ref AABB aabb2)
		{
			LowerBound = FVector2.Min(aabb1.LowerBound, aabb2.LowerBound);
			UpperBound = FVector2.Max(aabb1.UpperBound, aabb2.UpperBound);
		}

		public bool Contains(ref AABB aabb)
		{
			return true && LowerBound.X <= aabb.LowerBound.X && LowerBound.Y <= aabb.LowerBound.Y && aabb.UpperBound.X <= UpperBound.X && aabb.UpperBound.Y <= UpperBound.Y;
		}

		public bool Contains(ref FVector2 point)
		{
			if (point.X > LowerBound.X + 1.1920929E-07f && point.X < UpperBound.X - 1.1920929E-07f && point.Y > LowerBound.Y + 1.1920929E-07f && point.Y < UpperBound.Y - 1.1920929E-07f)
			{
				return true;
			}
			return false;
		}

		public static bool TestOverlap(AABB a, AABB b)
		{
			return TestOverlap(ref a, ref b);
		}

		public static bool TestOverlap(ref AABB a, ref AABB b)
		{
			FVector2 fVector = b.LowerBound - a.UpperBound;
			FVector2 fVector2 = a.LowerBound - b.UpperBound;
			if (fVector.X > 0f || fVector.Y > 0f)
			{
				return false;
			}
			if (fVector2.X > 0f || fVector2.Y > 0f)
			{
				return false;
			}
			return true;
		}

		public static bool TestOverlap(Shape shapeA, int indexA, Shape shapeB, int indexB, ref Transform xfA, ref Transform xfB)
		{
			_input.ProxyA.Set(shapeA, indexA);
			_input.ProxyB.Set(shapeB, indexB);
			_input.TransformA = xfA;
			_input.TransformB = xfB;
			_input.UseRadii = true;
			DistanceOutput output;
			SimplexCache cache;
			Distance.ComputeDistance(out output, out cache, _input);
			return output.Distance < 1.1920929E-06f;
		}

		public bool RayCast(out RayCastOutput output, ref RayCastInput input)
		{
			output = default(RayCastOutput);
			float num = float.MinValue;
			float num2 = float.MaxValue;
			FVector2 point = input.Point1;
			FVector2 v = input.Point2 - input.Point1;
			FVector2 fVector = MathUtils.Abs(v);
			FVector2 zero = FVector2.Zero;
			for (int i = 0; i < 2; i++)
			{
				float num3 = ((i != 0) ? fVector.Y : fVector.X);
				float num4 = ((i != 0) ? LowerBound.Y : LowerBound.X);
				float num5 = ((i != 0) ? UpperBound.Y : UpperBound.X);
				float num6 = ((i != 0) ? point.Y : point.X);
				if (num3 < 1.1920929E-07f)
				{
					if (num6 < num4 || num5 < num6)
					{
						return false;
					}
					continue;
				}
				float num7 = ((i != 0) ? v.Y : v.X);
				float num8 = 1f / num7;
				float a = (num4 - num6) * num8;
				float b = (num5 - num6) * num8;
				float num9 = -1f;
				if (a > b)
				{
					MathUtils.Swap(ref a, ref b);
					num9 = 1f;
				}
				if (a > num)
				{
					if (i == 0)
					{
						zero.X = num9;
					}
					else
					{
						zero.Y = num9;
					}
					num = a;
				}
				num2 = Math.Min(num2, b);
				if (num > num2)
				{
					return false;
				}
			}
			if (num < 0f || input.MaxFraction < num)
			{
				return false;
			}
			output.Fraction = num;
			output.Normal = zero;
			return true;
		}
	}
}
