using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	public static class SeparationFunction
	{
		private static FVector2 _axis;

		private static FVector2 _localPoint;

		private static DistanceProxy _proxyA = new DistanceProxy();

		private static DistanceProxy _proxyB = new DistanceProxy();

		private static Sweep _sweepA;

		private static Sweep _sweepB;

		private static SeparationFunctionType _type;

		public static void Set(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, float t1)
		{
			_localPoint = FVector2.Zero;
			_proxyA = proxyA;
			_proxyB = proxyB;
			int count = cache.Count;
			_sweepA = sweepA;
			_sweepB = sweepB;
			Transform xfb;
			_sweepA.GetTransform(out xfb, t1);
			Transform xfb2;
			_sweepB.GetTransform(out xfb2, t1);
			if (count == 1)
			{
				_type = SeparationFunctionType.Points;
				FVector2 v = _proxyA.Vertices[cache.IndexA[0]];
				FVector2 v2 = _proxyB.Vertices[cache.IndexB[0]];
				FVector2 fVector = MathUtils.Mul(ref xfb, v);
				FVector2 fVector2 = MathUtils.Mul(ref xfb2, v2);
				_axis = fVector2 - fVector;
				_axis.Normalize();
			}
			else if (cache.IndexA[0] == cache.IndexA[1])
			{
				_type = SeparationFunctionType.FaceB;
				FVector2 fVector3 = proxyB.Vertices[cache.IndexB[0]];
				FVector2 fVector4 = proxyB.Vertices[cache.IndexB[1]];
				FVector2 fVector5 = fVector4 - fVector3;
				_axis = new FVector2(fVector5.Y, 0f - fVector5.X);
				_axis.Normalize();
				FVector2 value = MathUtils.Mul(ref xfb2.q, _axis);
				_localPoint = 0.5f * (fVector3 + fVector4);
				FVector2 fVector6 = MathUtils.Mul(ref xfb2, _localPoint);
				FVector2 v3 = proxyA.Vertices[cache.IndexA[0]];
				FVector2 fVector7 = MathUtils.Mul(ref xfb, v3);
				float num = FVector2.Dot(fVector7 - fVector6, value);
				if (num < 0f)
				{
					_axis = -_axis;
					num = 0f - num;
				}
			}
			else
			{
				_type = SeparationFunctionType.FaceA;
				FVector2 fVector8 = _proxyA.Vertices[cache.IndexA[0]];
				FVector2 fVector9 = _proxyA.Vertices[cache.IndexA[1]];
				FVector2 fVector10 = fVector9 - fVector8;
				_axis = new FVector2(fVector10.Y, 0f - fVector10.X);
				_axis.Normalize();
				FVector2 value2 = MathUtils.Mul(ref xfb.q, _axis);
				_localPoint = 0.5f * (fVector8 + fVector9);
				FVector2 fVector11 = MathUtils.Mul(ref xfb, _localPoint);
				FVector2 v4 = _proxyB.Vertices[cache.IndexB[0]];
				FVector2 fVector12 = MathUtils.Mul(ref xfb2, v4);
				float num2 = FVector2.Dot(fVector12 - fVector11, value2);
				if (num2 < 0f)
				{
					_axis = -_axis;
					num2 = 0f - num2;
				}
			}
		}

		public static float FindMinSeparation(out int indexA, out int indexB, float t)
		{
			Transform xfb;
			_sweepA.GetTransform(out xfb, t);
			Transform xfb2;
			_sweepB.GetTransform(out xfb2, t);
			switch (_type)
			{
			case SeparationFunctionType.Points:
			{
				FVector2 direction3 = MathUtils.MulT(ref xfb.q, _axis);
				FVector2 direction4 = MathUtils.MulT(ref xfb2.q, -_axis);
				indexA = _proxyA.GetSupport(direction3);
				indexB = _proxyB.GetSupport(direction4);
				FVector2 v3 = _proxyA.Vertices[indexA];
				FVector2 v4 = _proxyB.Vertices[indexB];
				FVector2 fVector7 = MathUtils.Mul(ref xfb, v3);
				FVector2 fVector8 = MathUtils.Mul(ref xfb2, v4);
				return FVector2.Dot(fVector8 - fVector7, _axis);
			}
			case SeparationFunctionType.FaceA:
			{
				FVector2 fVector4 = MathUtils.Mul(ref xfb.q, _axis);
				FVector2 fVector5 = MathUtils.Mul(ref xfb, _localPoint);
				FVector2 direction2 = MathUtils.MulT(ref xfb2.q, -fVector4);
				indexA = -1;
				indexB = _proxyB.GetSupport(direction2);
				FVector2 v2 = _proxyB.Vertices[indexB];
				FVector2 fVector6 = MathUtils.Mul(ref xfb2, v2);
				return FVector2.Dot(fVector6 - fVector5, fVector4);
			}
			case SeparationFunctionType.FaceB:
			{
				FVector2 fVector = MathUtils.Mul(ref xfb2.q, _axis);
				FVector2 fVector2 = MathUtils.Mul(ref xfb2, _localPoint);
				FVector2 direction = MathUtils.MulT(ref xfb.q, -fVector);
				indexB = -1;
				indexA = _proxyA.GetSupport(direction);
				FVector2 v = _proxyA.Vertices[indexA];
				FVector2 fVector3 = MathUtils.Mul(ref xfb, v);
				return FVector2.Dot(fVector3 - fVector2, fVector);
			}
			default:
				indexA = -1;
				indexB = -1;
				return 0f;
			}
		}

		public static float Evaluate(int indexA, int indexB, float t)
		{
			Transform xfb;
			_sweepA.GetTransform(out xfb, t);
			Transform xfb2;
			_sweepB.GetTransform(out xfb2, t);
			switch (_type)
			{
			case SeparationFunctionType.Points:
			{
				FVector2 v3 = _proxyA.Vertices[indexA];
				FVector2 v4 = _proxyB.Vertices[indexB];
				FVector2 fVector5 = MathUtils.Mul(ref xfb, v3);
				FVector2 fVector6 = MathUtils.Mul(ref xfb2, v4);
				return FVector2.Dot(fVector6 - fVector5, _axis);
			}
			case SeparationFunctionType.FaceA:
			{
				FVector2 value2 = MathUtils.Mul(ref xfb.q, _axis);
				FVector2 fVector3 = MathUtils.Mul(ref xfb, _localPoint);
				FVector2 v2 = _proxyB.Vertices[indexB];
				FVector2 fVector4 = MathUtils.Mul(ref xfb2, v2);
				return FVector2.Dot(fVector4 - fVector3, value2);
			}
			case SeparationFunctionType.FaceB:
			{
				FVector2 value = MathUtils.Mul(ref xfb2.q, _axis);
				FVector2 fVector = MathUtils.Mul(ref xfb2, _localPoint);
				FVector2 v = _proxyA.Vertices[indexA];
				FVector2 fVector2 = MathUtils.Mul(ref xfb, v);
				return FVector2.Dot(fVector2 - fVector, value);
			}
			default:
				return 0f;
			}
		}
	}
}
