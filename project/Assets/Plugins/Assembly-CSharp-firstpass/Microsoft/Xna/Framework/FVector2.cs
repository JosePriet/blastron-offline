using System;
using System.Text;
using UnityEngine;

namespace Microsoft.Xna.Framework
{
	public struct FVector2
	{
		private static FVector2 zeroVector = new FVector2(0f, 0f);

		private static FVector2 unitVector = new FVector2(1f, 1f);

		private static FVector2 unitXVector = new FVector2(1f, 0f);

		private static FVector2 unitYVector = new FVector2(0f, 1f);

		public float X;

		public float Y;

		public static FVector2 Zero
		{
			get
			{
				return zeroVector;
			}
		}

		public static FVector2 One
		{
			get
			{
				return unitVector;
			}
		}

		public static FVector2 UnitX
		{
			get
			{
				return unitXVector;
			}
		}

		public static FVector2 UnitY
		{
			get
			{
				return unitYVector;
			}
		}

		public FVector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public FVector2(float value)
		{
			X = value;
			Y = value;
		}

		public static void Reflect(ref FVector2 vector, ref FVector2 normal, out FVector2 result)
		{
			float num = Dot(vector, normal);
			result.X = vector.X - 2f * num * normal.X;
			result.Y = vector.Y - 2f * num * normal.Y;
		}

		public static FVector2 Reflect(FVector2 vector, FVector2 normal)
		{
			FVector2 result;
			Reflect(ref vector, ref normal, out result);
			return result;
		}

		public static FVector2 Add(FVector2 value1, FVector2 value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			return value1;
		}

		public static void Add(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
		{
			result.X = value1.X + value2.X;
			result.Y = value1.Y + value2.Y;
		}

		public static FVector2 Barycentric(FVector2 value1, FVector2 value2, FVector2 value3, float amount1, float amount2)
		{
			return new FVector2(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
		}

		public static void Barycentric(ref FVector2 value1, ref FVector2 value2, ref FVector2 value3, float amount1, float amount2, out FVector2 result)
		{
			result = new FVector2(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
		}

		public static FVector2 CatmullRom(FVector2 value1, FVector2 value2, FVector2 value3, FVector2 value4, float amount)
		{
			return new FVector2(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
		}

		public static void CatmullRom(ref FVector2 value1, ref FVector2 value2, ref FVector2 value3, ref FVector2 value4, float amount, out FVector2 result)
		{
			result = new FVector2(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
		}

		public static FVector2 Clamp(FVector2 value1, FVector2 min, FVector2 max)
		{
			return new FVector2(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));
		}

		public static void Clamp(ref FVector2 value1, ref FVector2 min, ref FVector2 max, out FVector2 result)
		{
			result = new FVector2(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));
		}

		public static float Distance(FVector2 value1, FVector2 value2)
		{
			float result;
			DistanceSquared(ref value1, ref value2, out result);
			return (float)Math.Sqrt(result);
		}

		public static void Distance(ref FVector2 value1, ref FVector2 value2, out float result)
		{
			DistanceSquared(ref value1, ref value2, out result);
			result = (float)Math.Sqrt(result);
		}

		public static float DistanceSquared(FVector2 value1, FVector2 value2)
		{
			float result;
			DistanceSquared(ref value1, ref value2, out result);
			return result;
		}

		public static void DistanceSquared(ref FVector2 value1, ref FVector2 value2, out float result)
		{
			result = (value1.X - value2.X) * (value1.X - value2.X) + (value1.Y - value2.Y) * (value1.Y - value2.Y);
		}

		public static FVector2 Divide(FVector2 value1, FVector2 value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			return value1;
		}

		public static void Divide(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
		{
			result.X = value1.X / value2.X;
			result.Y = value1.Y / value2.Y;
		}

		public static FVector2 Divide(FVector2 value1, float divider)
		{
			float num = 1f / divider;
			value1.X *= num;
			value1.Y *= num;
			return value1;
		}

		public static void Divide(ref FVector2 value1, float divider, out FVector2 result)
		{
			float num = 1f / divider;
			result.X = value1.X * num;
			result.Y = value1.Y * num;
		}

		public static float Dot(FVector2 value1, FVector2 value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y;
		}

		public static void Dot(ref FVector2 value1, ref FVector2 value2, out float result)
		{
			result = value1.X * value2.X + value1.Y * value2.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is FVector2 && this == (FVector2)obj;
		}

		public bool Equals(FVector2 other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return (int)(X + Y);
		}

		public static FVector2 Hermite(FVector2 value1, FVector2 tangent1, FVector2 value2, FVector2 tangent2, float amount)
		{
			FVector2 result = default(FVector2);
			Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
			return result;
		}

		public static void Hermite(ref FVector2 value1, ref FVector2 tangent1, ref FVector2 value2, ref FVector2 tangent2, float amount, out FVector2 result)
		{
			result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
			result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
		}

		public float Length()
		{
			float result;
			DistanceSquared(ref this, ref zeroVector, out result);
			return (float)Math.Sqrt(result);
		}

		public float LengthSquared()
		{
			float result;
			DistanceSquared(ref this, ref zeroVector, out result);
			return result;
		}

		public static FVector2 Lerp(FVector2 value1, FVector2 value2, float amount)
		{
			return new FVector2(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount));
		}

		public static void Lerp(ref FVector2 value1, ref FVector2 value2, float amount, out FVector2 result)
		{
			result = new FVector2(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount));
		}

		public static FVector2 Max(FVector2 value1, FVector2 value2)
		{
			return new FVector2(MathHelper.Max(value1.X, value2.X), MathHelper.Max(value1.Y, value2.Y));
		}

		public static void Max(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
		{
			result = new FVector2(MathHelper.Max(value1.X, value2.X), MathHelper.Max(value1.Y, value2.Y));
		}

		public static FVector2 Min(FVector2 value1, FVector2 value2)
		{
			return new FVector2(MathHelper.Min(value1.X, value2.X), MathHelper.Min(value1.Y, value2.Y));
		}

		public static void Min(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
		{
			result = new FVector2(MathHelper.Min(value1.X, value2.X), MathHelper.Min(value1.Y, value2.Y));
		}

		public static FVector2 Multiply(FVector2 value1, FVector2 value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			return value1;
		}

		public static FVector2 Multiply(FVector2 value1, float scaleFactor)
		{
			value1.X *= scaleFactor;
			value1.Y *= scaleFactor;
			return value1;
		}

		public static void Multiply(ref FVector2 value1, float scaleFactor, out FVector2 result)
		{
			result.X = value1.X * scaleFactor;
			result.Y = value1.Y * scaleFactor;
		}

		public static void Multiply(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
		{
			result.X = value1.X * value2.X;
			result.Y = value1.Y * value2.Y;
		}

		public static FVector2 Negate(FVector2 value)
		{
			value.X = 0f - value.X;
			value.Y = 0f - value.Y;
			return value;
		}

		public static void Negate(ref FVector2 value, out FVector2 result)
		{
			result.X = 0f - value.X;
			result.Y = 0f - value.Y;
		}

		public void Normalize()
		{
			Normalize(ref this, out this);
		}

		public static FVector2 Normalize(FVector2 value)
		{
			Normalize(ref value, out value);
			return value;
		}

		public static void Normalize(ref FVector2 value, out FVector2 result)
		{
			float result2;
			DistanceSquared(ref value, ref zeroVector, out result2);
			result2 = 1f / (float)Math.Sqrt(result2);
			result.X = value.X * result2;
			result.Y = value.Y * result2;
		}

		public static FVector2 SmoothStep(FVector2 value1, FVector2 value2, float amount)
		{
			return new FVector2(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));
		}

		public static void SmoothStep(ref FVector2 value1, ref FVector2 value2, float amount, out FVector2 result)
		{
			result = new FVector2(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));
		}

		public static FVector2 Subtract(FVector2 value1, FVector2 value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			return value1;
		}

		public static void Subtract(ref FVector2 value1, ref FVector2 value2, out FVector2 result)
		{
			result.X = value1.X - value2.X;
			result.Y = value1.Y - value2.Y;
		}

		public static FVector2 Transform(FVector2 position, FMatrix matrix)
		{
			Transform(ref position, ref matrix, out position);
			return position;
		}

		public static void Transform(ref FVector2 position, ref FMatrix matrix, out FVector2 result)
		{
			result = new FVector2(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);
		}

		public static void Transform(FVector2[] sourceArray, ref FMatrix matrix, FVector2[] destinationArray)
		{
			for (int i = 0; i < sourceArray.Length; i++)
			{
				destinationArray[i] = new FVector2(sourceArray[i].X * matrix.M11 + sourceArray[i].Y * matrix.M21 + matrix.M41, sourceArray[i].X * matrix.M12 + sourceArray[i].Y * matrix.M22 + matrix.M42);
			}
		}

		public static void Transform(FVector2[] sourceArray, int sourceIndex, ref FMatrix matrix, FVector2[] destinationArray, int destinationIndex, int length)
		{
			throw new NotImplementedException();
		}

		public static FVector2 TransformNormal(FVector2 normal, FMatrix matrix)
		{
			TransformNormal(ref normal, ref matrix, out normal);
			return normal;
		}

		public static void TransformNormal(ref FVector2 normal, ref FMatrix matrix, out FVector2 result)
		{
			result = new FVector2(normal.X * matrix.M11 + normal.Y * matrix.M21, normal.X * matrix.M12 + normal.Y * matrix.M22);
		}

		public static void TransformNormal(FVector2[] sourceArray, ref FMatrix matrix, FVector2[] destinationArray)
		{
			throw new NotImplementedException();
		}

		public static void TransformNormal(FVector2[] sourceArray, int sourceIndex, ref FMatrix matrix, FVector2[] destinationArray, int destinationIndex, int length)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(24);
			stringBuilder.Append("{X:");
			stringBuilder.Append(X);
			stringBuilder.Append(" Y:");
			stringBuilder.Append(Y);
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		public Vector3 ToVector3()
		{
			return new Vector3(X, Y, 0f);
		}

		public static FVector2 operator -(FVector2 value)
		{
			value.X = 0f - value.X;
			value.Y = 0f - value.Y;
			return value;
		}

		public static bool operator ==(FVector2 value1, FVector2 value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y;
		}

		public static bool operator !=(FVector2 value1, FVector2 value2)
		{
			return value1.X != value2.X || value1.Y != value2.Y;
		}

		public static FVector2 operator +(FVector2 value1, FVector2 value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			return value1;
		}

		public static FVector2 operator -(FVector2 value1, FVector2 value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			return value1;
		}

		public static FVector2 operator *(FVector2 value1, FVector2 value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			return value1;
		}

		public static FVector2 operator *(FVector2 value, float scaleFactor)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			return value;
		}

		public static FVector2 operator *(float scaleFactor, FVector2 value)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			return value;
		}

		public static FVector2 operator /(FVector2 value1, FVector2 value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			return value1;
		}

		public static FVector2 operator /(FVector2 value1, float divider)
		{
			float num = 1f / divider;
			value1.X *= num;
			value1.Y *= num;
			return value1;
		}
	}
}
