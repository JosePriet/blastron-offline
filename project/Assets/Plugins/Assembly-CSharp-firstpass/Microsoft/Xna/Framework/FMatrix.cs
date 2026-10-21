using System;

namespace Microsoft.Xna.Framework
{
	public struct FMatrix : IEquatable<FMatrix>
	{
		public float M11;

		public float M12;

		public float M13;

		public float M14;

		public float M21;

		public float M22;

		public float M23;

		public float M24;

		public float M31;

		public float M32;

		public float M33;

		public float M34;

		public float M41;

		public float M42;

		public float M43;

		public float M44;

		private static FMatrix identity = new FMatrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);

		public static FMatrix Identity
		{
			get
			{
				return identity;
			}
		}

		public FVector3 Backward
		{
			get
			{
				return new FVector3(M31, M32, M33);
			}
			set
			{
				M31 = value.X;
				M32 = value.Y;
				M33 = value.Z;
			}
		}

		public FVector3 Down
		{
			get
			{
				return new FVector3(0f - M21, 0f - M22, 0f - M23);
			}
			set
			{
				M21 = 0f - value.X;
				M22 = 0f - value.Y;
				M23 = 0f - value.Z;
			}
		}

		public FVector3 Forward
		{
			get
			{
				return new FVector3(0f - M31, 0f - M32, 0f - M33);
			}
			set
			{
				M31 = 0f - value.X;
				M32 = 0f - value.Y;
				M33 = 0f - value.Z;
			}
		}

		public FVector3 Left
		{
			get
			{
				return new FVector3(0f - M11, 0f - M12, 0f - M13);
			}
			set
			{
				M11 = 0f - value.X;
				M12 = 0f - value.Y;
				M13 = 0f - value.Z;
			}
		}

		public FVector3 Right
		{
			get
			{
				return new FVector3(M11, M12, M13);
			}
			set
			{
				M11 = value.X;
				M12 = value.Y;
				M13 = value.Z;
			}
		}

		public FVector3 Translation
		{
			get
			{
				return new FVector3(M41, M42, M43);
			}
			set
			{
				M41 = value.X;
				M42 = value.Y;
				M43 = value.Z;
			}
		}

		public FVector3 Up
		{
			get
			{
				return new FVector3(M21, M22, M23);
			}
			set
			{
				M21 = value.X;
				M22 = value.Y;
				M23 = value.Z;
			}
		}

		public FMatrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
		{
			M11 = m11;
			M12 = m12;
			M13 = m13;
			M14 = m14;
			M21 = m21;
			M22 = m22;
			M23 = m23;
			M24 = m24;
			M31 = m31;
			M32 = m32;
			M33 = m33;
			M34 = m34;
			M41 = m41;
			M42 = m42;
			M43 = m43;
			M44 = m44;
		}

		public static FMatrix CreateWorld(FVector3 position, FVector3 forward, FVector3 up)
		{
			FMatrix result;
			CreateWorld(ref position, ref forward, ref up, out result);
			return result;
		}

		public static void CreateWorld(ref FVector3 position, ref FVector3 forward, ref FVector3 up, out FMatrix result)
		{
			FVector3 result2;
			FVector3.Normalize(ref forward, out result2);
			FVector3 result3;
			FVector3.Cross(ref forward, ref up, out result3);
			FVector3 result4;
			FVector3.Cross(ref result3, ref forward, out result4);
			result3.Normalize();
			result4.Normalize();
			result = default(FMatrix);
			result.Right = result3;
			result.Up = result4;
			result.Forward = result2;
			result.Translation = position;
			result.M44 = 1f;
		}

		public static FMatrix Add(FMatrix matrix1, FMatrix matrix2)
		{
			matrix1.M11 += matrix2.M11;
			matrix1.M12 += matrix2.M12;
			matrix1.M13 += matrix2.M13;
			matrix1.M14 += matrix2.M14;
			matrix1.M21 += matrix2.M21;
			matrix1.M22 += matrix2.M22;
			matrix1.M23 += matrix2.M23;
			matrix1.M24 += matrix2.M24;
			matrix1.M31 += matrix2.M31;
			matrix1.M32 += matrix2.M32;
			matrix1.M33 += matrix2.M33;
			matrix1.M34 += matrix2.M34;
			matrix1.M41 += matrix2.M41;
			matrix1.M42 += matrix2.M42;
			matrix1.M43 += matrix2.M43;
			matrix1.M44 += matrix2.M44;
			return matrix1;
		}

		public static void Add(ref FMatrix matrix1, ref FMatrix matrix2, out FMatrix result)
		{
			result.M11 = matrix1.M11 + matrix2.M11;
			result.M12 = matrix1.M12 + matrix2.M12;
			result.M13 = matrix1.M13 + matrix2.M13;
			result.M14 = matrix1.M14 + matrix2.M14;
			result.M21 = matrix1.M21 + matrix2.M21;
			result.M22 = matrix1.M22 + matrix2.M22;
			result.M23 = matrix1.M23 + matrix2.M23;
			result.M24 = matrix1.M24 + matrix2.M24;
			result.M31 = matrix1.M31 + matrix2.M31;
			result.M32 = matrix1.M32 + matrix2.M32;
			result.M33 = matrix1.M33 + matrix2.M33;
			result.M34 = matrix1.M34 + matrix2.M34;
			result.M41 = matrix1.M41 + matrix2.M41;
			result.M42 = matrix1.M42 + matrix2.M42;
			result.M43 = matrix1.M43 + matrix2.M43;
			result.M44 = matrix1.M44 + matrix2.M44;
		}

		public static FMatrix CreateBillboard(FVector3 objectPosition, FVector3 cameraPosition, FVector3 cameraUpVector, FVector3? cameraForwardVector)
		{
			FMatrix result;
			CreateBillboard(ref objectPosition, ref cameraPosition, ref cameraUpVector, cameraForwardVector, out result);
			return result;
		}

		public static void CreateBillboard(ref FVector3 objectPosition, ref FVector3 cameraPosition, ref FVector3 cameraUpVector, FVector3? cameraForwardVector, out FMatrix result)
		{
			FVector3 value = objectPosition - cameraPosition;
			FVector3 result2;
			FVector3.Normalize(ref value, out result2);
			FVector3 result3;
			FVector3.Normalize(ref cameraUpVector, out result3);
			FVector3 result4;
			FVector3.Cross(ref result2, ref result3, out result4);
			FVector3.Cross(ref result2, ref result4, out result3);
			result = Identity;
			result.Backward = result2;
			result.Right = result4;
			result.Up = result3;
			result.Translation = value;
		}

		public static FMatrix CreateConstrainedBillboard(FVector3 objectPosition, FVector3 cameraPosition, FVector3 rotateAxis, FVector3? cameraForwardVector, FVector3? objectForwardVector)
		{
			throw new NotImplementedException();
		}

		public static void CreateConstrainedBillboard(ref FVector3 objectPosition, ref FVector3 cameraPosition, ref FVector3 rotateAxis, FVector3? cameraForwardVector, FVector3? objectForwardVector, out FMatrix result)
		{
			throw new NotImplementedException();
		}

		public static FMatrix CreateFromAxisAngle(FVector3 axis, float angle)
		{
			throw new NotImplementedException();
		}

		public static void CreateFromAxisAngle(ref FVector3 axis, float angle, out FMatrix result)
		{
			throw new NotImplementedException();
		}

		public static FMatrix CreateLookAt(FVector3 cameraPosition, FVector3 cameraTarget, FVector3 cameraUpVector)
		{
			FMatrix result;
			CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out result);
			return result;
		}

		public static void CreateLookAt(ref FVector3 cameraPosition, ref FVector3 cameraTarget, ref FVector3 cameraUpVector, out FMatrix result)
		{
			FVector3 fVector = FVector3.Normalize(cameraPosition - cameraTarget);
			FVector3 fVector2 = FVector3.Normalize(FVector3.Cross(cameraUpVector, fVector));
			FVector3 vector = FVector3.Cross(fVector, fVector2);
			result = Identity;
			result.M11 = fVector2.X;
			result.M12 = vector.X;
			result.M13 = fVector.X;
			result.M21 = fVector2.Y;
			result.M22 = vector.Y;
			result.M23 = fVector.Y;
			result.M31 = fVector2.Z;
			result.M32 = vector.Z;
			result.M33 = fVector.Z;
			result.M41 = 0f - FVector3.Dot(fVector2, cameraPosition);
			result.M42 = 0f - FVector3.Dot(vector, cameraPosition);
			result.M43 = 0f - FVector3.Dot(fVector, cameraPosition);
		}

		public static FMatrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
		{
			FMatrix result;
			CreateOrthographic(width, height, zNearPlane, zFarPlane, out result);
			return result;
		}

		public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out FMatrix result)
		{
			result.M11 = 2f / width;
			result.M12 = 0f;
			result.M13 = 0f;
			result.M14 = 0f;
			result.M21 = 0f;
			result.M22 = 2f / height;
			result.M23 = 0f;
			result.M24 = 0f;
			result.M31 = 0f;
			result.M32 = 0f;
			result.M33 = 1f / (zNearPlane - zFarPlane);
			result.M34 = 0f;
			result.M41 = 0f;
			result.M42 = 0f;
			result.M43 = zNearPlane / (zNearPlane - zFarPlane);
			result.M44 = 1f;
		}

		public static FMatrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
		{
			FMatrix result;
			CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane, out result);
			return result;
		}

		public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out FMatrix result)
		{
			result.M11 = 2f / (right - left);
			result.M12 = 0f;
			result.M13 = 0f;
			result.M14 = 0f;
			result.M21 = 0f;
			result.M22 = 2f / (top - bottom);
			result.M23 = 0f;
			result.M24 = 0f;
			result.M31 = 0f;
			result.M32 = 0f;
			result.M33 = 1f / (zNearPlane - zFarPlane);
			result.M34 = 0f;
			result.M41 = (left + right) / (left - right);
			result.M42 = (bottom + top) / (bottom - top);
			result.M43 = zNearPlane / (zNearPlane - zFarPlane);
			result.M44 = 1f;
		}

		public static FMatrix CreatePerspective(float width, float height, float zNearPlane, float zFarPlane)
		{
			throw new NotImplementedException();
		}

		public static void CreatePerspective(float width, float height, float zNearPlane, float zFarPlane, out FMatrix result)
		{
			throw new NotImplementedException();
		}

		public static FMatrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
		{
			FMatrix result;
			CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
			return result;
		}

		public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out FMatrix result)
		{
			result = new FMatrix(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
			if (fieldOfView < 0f || fieldOfView > 3.1415925f)
			{
				throw new ArgumentOutOfRangeException("fieldOfView", "fieldOfView takes a value between 0 and Pi (180 degrees) in radians.");
			}
			if (nearPlaneDistance <= 0f)
			{
				throw new ArgumentOutOfRangeException("nearPlaneDistance", "You should specify positive value for nearPlaneDistance.");
			}
			if (farPlaneDistance <= 0f)
			{
				throw new ArgumentOutOfRangeException("farPlaneDistance", "You should specify positive value for farPlaneDistance.");
			}
			if (farPlaneDistance <= nearPlaneDistance)
			{
				throw new ArgumentOutOfRangeException("nearPlaneDistance", "Near plane distance is larger than Far plane distance. Near plane distance must be smaller than Far plane distance.");
			}
			float num = 1f / (float)Math.Tan(fieldOfView / 2f);
			float m = num / aspectRatio;
			result.M11 = m;
			result.M22 = num;
			result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			result.M34 = -1f;
			result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		}

		public static FMatrix CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
		{
			throw new NotImplementedException();
		}

		public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out FMatrix result)
		{
			throw new NotImplementedException();
		}

		public static FMatrix CreateRotationX(float radians)
		{
			FMatrix result = Identity;
			result.M22 = (float)Math.Cos(radians);
			result.M23 = (float)Math.Sin(radians);
			result.M32 = 0f - result.M23;
			result.M33 = result.M22;
			return result;
		}

		public static void CreateRotationX(float radians, out FMatrix result)
		{
			result = Identity;
			result.M22 = (float)Math.Cos(radians);
			result.M23 = (float)Math.Sin(radians);
			result.M32 = 0f - result.M23;
			result.M33 = result.M22;
		}

		public static FMatrix CreateRotationY(float radians)
		{
			FMatrix result = Identity;
			result.M11 = (float)Math.Cos(radians);
			result.M13 = (float)Math.Sin(radians);
			result.M31 = 0f - result.M13;
			result.M33 = result.M11;
			return result;
		}

		public static void CreateRotationY(float radians, out FMatrix result)
		{
			result = Identity;
			result.M11 = (float)Math.Cos(radians);
			result.M13 = (float)Math.Sin(radians);
			result.M31 = 0f - result.M13;
			result.M33 = result.M11;
		}

		public static FMatrix CreateRotationZ(float radians)
		{
			FMatrix result = Identity;
			result.M11 = (float)Math.Cos(radians);
			result.M12 = (float)Math.Sin(radians);
			result.M21 = 0f - result.M12;
			result.M22 = result.M11;
			return result;
		}

		public static void CreateRotationZ(float radians, out FMatrix result)
		{
			result = Identity;
			result.M11 = (float)Math.Cos(radians);
			result.M12 = (float)Math.Sin(radians);
			result.M21 = 0f - result.M12;
			result.M22 = result.M11;
		}

		public static FMatrix CreateScale(float scale)
		{
			FMatrix result = Identity;
			result.M11 = scale;
			result.M22 = scale;
			result.M33 = scale;
			return result;
		}

		public static void CreateScale(float scale, out FMatrix result)
		{
			result = Identity;
			result.M11 = scale;
			result.M22 = scale;
			result.M33 = scale;
		}

		public static FMatrix CreateScale(float xScale, float yScale, float zScale)
		{
			FMatrix result = Identity;
			result.M11 = xScale;
			result.M22 = yScale;
			result.M33 = zScale;
			return result;
		}

		public static void CreateScale(float xScale, float yScale, float zScale, out FMatrix result)
		{
			result = Identity;
			result.M11 = xScale;
			result.M22 = yScale;
			result.M33 = zScale;
		}

		public static FMatrix CreateScale(FVector3 scales)
		{
			FMatrix result = Identity;
			result.M11 = scales.X;
			result.M22 = scales.Y;
			result.M33 = scales.Z;
			return result;
		}

		public static void CreateScale(ref FVector3 scales, out FMatrix result)
		{
			result = Identity;
			result.M11 = scales.X;
			result.M22 = scales.Y;
			result.M33 = scales.Z;
		}

		public static FMatrix CreateTranslation(float xPosition, float yPosition, float zPosition)
		{
			FMatrix result = Identity;
			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
			return result;
		}

		public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out FMatrix result)
		{
			result = Identity;
			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
		}

		public static FMatrix CreateTranslation(FVector3 position)
		{
			FMatrix result = Identity;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
			return result;
		}

		public static void CreateTranslation(ref FVector3 position, out FMatrix result)
		{
			result = Identity;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
		}

		public static FMatrix Divide(FMatrix matrix1, FMatrix matrix2)
		{
			FMatrix result;
			Divide(ref matrix1, ref matrix2, out result);
			return result;
		}

		public static void Divide(ref FMatrix matrix1, ref FMatrix matrix2, out FMatrix result)
		{
			FMatrix matrix3 = Invert(matrix2);
			Multiply(ref matrix1, ref matrix3, out result);
		}

		public static FMatrix Divide(FMatrix matrix1, float divider)
		{
			FMatrix result;
			Divide(ref matrix1, divider, out result);
			return result;
		}

		public static void Divide(ref FMatrix matrix1, float divider, out FMatrix result)
		{
			float factor = 1f / divider;
			Multiply(ref matrix1, factor, out result);
		}

		public static FMatrix Invert(FMatrix matrix)
		{
			Invert(ref matrix, out matrix);
			return matrix;
		}

		public static void Invert(ref FMatrix matrix, out FMatrix result)
		{
			float num = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
			float num2 = matrix.M11 * matrix.M23 - matrix.M13 * matrix.M21;
			float num3 = matrix.M11 * matrix.M24 - matrix.M14 * matrix.M21;
			float num4 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
			float num5 = matrix.M12 * matrix.M24 - matrix.M14 * matrix.M22;
			float num6 = matrix.M13 * matrix.M24 - matrix.M14 * matrix.M23;
			float num7 = matrix.M31 * matrix.M42 - matrix.M32 * matrix.M41;
			float num8 = matrix.M31 * matrix.M43 - matrix.M33 * matrix.M41;
			float num9 = matrix.M31 * matrix.M44 - matrix.M34 * matrix.M41;
			float num10 = matrix.M32 * matrix.M43 - matrix.M33 * matrix.M42;
			float num11 = matrix.M32 * matrix.M44 - matrix.M34 * matrix.M42;
			float num12 = matrix.M33 * matrix.M44 - matrix.M34 * matrix.M43;
			float num13 = num * num12 - num2 * num11 + num3 * num10 + num4 * num9 - num5 * num8 + num6 * num7;
			float num14 = 1f / num13;
			FMatrix fMatrix = default(FMatrix);
			fMatrix.M11 = (matrix.M22 * num12 - matrix.M23 * num11 + matrix.M24 * num10) * num14;
			fMatrix.M12 = ((0f - matrix.M12) * num12 + matrix.M13 * num11 - matrix.M14 * num10) * num14;
			fMatrix.M13 = (matrix.M42 * num6 - matrix.M43 * num5 + matrix.M44 * num4) * num14;
			fMatrix.M14 = ((0f - matrix.M32) * num6 + matrix.M33 * num5 - matrix.M34 * num4) * num14;
			fMatrix.M21 = ((0f - matrix.M21) * num12 + matrix.M23 * num9 - matrix.M24 * num8) * num14;
			fMatrix.M22 = (matrix.M11 * num12 - matrix.M13 * num9 + matrix.M14 * num8) * num14;
			fMatrix.M23 = ((0f - matrix.M41) * num6 + matrix.M43 * num3 - matrix.M44 * num2) * num14;
			fMatrix.M24 = (matrix.M31 * num6 - matrix.M33 * num3 + matrix.M34 * num2) * num14;
			fMatrix.M31 = (matrix.M21 * num11 - matrix.M22 * num9 + matrix.M24 * num7) * num14;
			fMatrix.M32 = ((0f - matrix.M11) * num11 + matrix.M12 * num9 - matrix.M14 * num7) * num14;
			fMatrix.M33 = (matrix.M41 * num5 - matrix.M42 * num3 + matrix.M44 * num) * num14;
			fMatrix.M34 = ((0f - matrix.M31) * num5 + matrix.M32 * num3 - matrix.M34 * num) * num14;
			fMatrix.M41 = ((0f - matrix.M21) * num10 + matrix.M22 * num8 - matrix.M23 * num7) * num14;
			fMatrix.M42 = (matrix.M11 * num10 - matrix.M12 * num8 + matrix.M13 * num7) * num14;
			fMatrix.M43 = ((0f - matrix.M41) * num4 + matrix.M42 * num2 - matrix.M43 * num) * num14;
			fMatrix.M44 = (matrix.M31 * num4 - matrix.M32 * num2 + matrix.M33 * num) * num14;
			result = fMatrix;
		}

		public static FMatrix Lerp(FMatrix matrix1, FMatrix matrix2, float amount)
		{
			throw new NotImplementedException();
		}

		public static void Lerp(ref FMatrix matrix1, ref FMatrix matrix2, float amount, out FMatrix result)
		{
			throw new NotImplementedException();
		}

		public static FMatrix Multiply(FMatrix matrix1, FMatrix matrix2)
		{
			FMatrix result;
			Multiply(ref matrix1, ref matrix2, out result);
			return result;
		}

		public static void Multiply(ref FMatrix matrix1, ref FMatrix matrix2, out FMatrix result)
		{
			result.M11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
			result.M12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
			result.M13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
			result.M14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
			result.M21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
			result.M22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
			result.M23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
			result.M24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
			result.M31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
			result.M32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
			result.M33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
			result.M34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
			result.M41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
			result.M42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
			result.M43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
			result.M44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
		}

		public static FMatrix Multiply(FMatrix matrix1, float factor)
		{
			matrix1.M11 *= factor;
			matrix1.M12 *= factor;
			matrix1.M13 *= factor;
			matrix1.M14 *= factor;
			matrix1.M21 *= factor;
			matrix1.M22 *= factor;
			matrix1.M23 *= factor;
			matrix1.M24 *= factor;
			matrix1.M31 *= factor;
			matrix1.M32 *= factor;
			matrix1.M33 *= factor;
			matrix1.M34 *= factor;
			matrix1.M41 *= factor;
			matrix1.M42 *= factor;
			matrix1.M43 *= factor;
			matrix1.M44 *= factor;
			return matrix1;
		}

		public static void Multiply(ref FMatrix matrix1, float factor, out FMatrix result)
		{
			result.M11 = matrix1.M11 * factor;
			result.M12 = matrix1.M12 * factor;
			result.M13 = matrix1.M13 * factor;
			result.M14 = matrix1.M14 * factor;
			result.M21 = matrix1.M21 * factor;
			result.M22 = matrix1.M22 * factor;
			result.M23 = matrix1.M23 * factor;
			result.M24 = matrix1.M24 * factor;
			result.M31 = matrix1.M31 * factor;
			result.M32 = matrix1.M32 * factor;
			result.M33 = matrix1.M33 * factor;
			result.M34 = matrix1.M34 * factor;
			result.M41 = matrix1.M41 * factor;
			result.M42 = matrix1.M42 * factor;
			result.M43 = matrix1.M43 * factor;
			result.M44 = matrix1.M44 * factor;
		}

		public static FMatrix Negate(FMatrix matrix)
		{
			Multiply(ref matrix, -1f, out matrix);
			return matrix;
		}

		public static void Negate(ref FMatrix matrix, out FMatrix result)
		{
			Multiply(ref matrix, -1f, out result);
		}

		public static FMatrix Subtract(FMatrix matrix1, FMatrix matrix2)
		{
			matrix1.M11 -= matrix2.M11;
			matrix1.M12 -= matrix2.M12;
			matrix1.M13 -= matrix2.M13;
			matrix1.M14 -= matrix2.M14;
			matrix1.M21 -= matrix2.M21;
			matrix1.M22 -= matrix2.M22;
			matrix1.M23 -= matrix2.M23;
			matrix1.M24 -= matrix2.M24;
			matrix1.M31 -= matrix2.M31;
			matrix1.M32 -= matrix2.M32;
			matrix1.M33 -= matrix2.M33;
			matrix1.M34 -= matrix2.M34;
			matrix1.M41 -= matrix2.M41;
			matrix1.M42 -= matrix2.M42;
			matrix1.M43 -= matrix2.M43;
			matrix1.M44 -= matrix2.M44;
			return matrix1;
		}

		public static void Subtract(ref FMatrix matrix1, ref FMatrix matrix2, out FMatrix result)
		{
			result.M11 = matrix1.M11 - matrix2.M11;
			result.M12 = matrix1.M12 - matrix2.M12;
			result.M13 = matrix1.M13 - matrix2.M13;
			result.M14 = matrix1.M14 - matrix2.M14;
			result.M21 = matrix1.M21 - matrix2.M21;
			result.M22 = matrix1.M22 - matrix2.M22;
			result.M23 = matrix1.M23 - matrix2.M23;
			result.M24 = matrix1.M24 - matrix2.M24;
			result.M31 = matrix1.M31 - matrix2.M31;
			result.M32 = matrix1.M32 - matrix2.M32;
			result.M33 = matrix1.M33 - matrix2.M33;
			result.M34 = matrix1.M34 - matrix2.M34;
			result.M41 = matrix1.M41 - matrix2.M41;
			result.M42 = matrix1.M42 - matrix2.M42;
			result.M43 = matrix1.M43 - matrix2.M43;
			result.M44 = matrix1.M44 - matrix2.M44;
		}

		public static FMatrix Transpose(FMatrix matrix)
		{
			FMatrix result;
			Transpose(ref matrix, out result);
			return result;
		}

		public static void Transpose(ref FMatrix matrix, out FMatrix result)
		{
			result.M11 = matrix.M11;
			result.M12 = matrix.M21;
			result.M13 = matrix.M31;
			result.M14 = matrix.M41;
			result.M21 = matrix.M12;
			result.M22 = matrix.M22;
			result.M23 = matrix.M32;
			result.M24 = matrix.M42;
			result.M31 = matrix.M13;
			result.M32 = matrix.M23;
			result.M33 = matrix.M33;
			result.M34 = matrix.M43;
			result.M41 = matrix.M14;
			result.M42 = matrix.M24;
			result.M43 = matrix.M34;
			result.M44 = matrix.M44;
		}

		public float Determinant()
		{
			float num = M31 * M42 - M32 * M41;
			float num2 = M31 * M43 - M33 * M41;
			float num3 = M31 * M44 - M34 * M41;
			float num4 = M32 * M43 - M33 * M42;
			float num5 = M32 * M44 - M34 * M42;
			float num6 = M33 * M44 - M34 * M43;
			return M11 * (M22 * num6 - M23 * num5 + M24 * num4) - M12 * (M21 * num6 - M23 * num3 + M24 * num2) + M13 * (M21 * num5 - M22 * num3 + M24 * num) - M14 * (M21 * num4 - M22 * num2 + M23 * num);
		}

		public bool Equals(FMatrix other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return this == (FMatrix)obj;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return "{ {M11:" + M11 + " M12:" + M12 + " M13:" + M13 + " M14:" + M14 + "} {M21:" + M21 + " M22:" + M22 + " M23:" + M23 + " M24:" + M24 + "} {M31:" + M31 + " M32:" + M32 + " M33:" + M33 + " M34:" + M34 + "} {M41:" + M41 + " M42:" + M42 + " M43:" + M43 + " M44:" + M44 + "} }";
		}

		public static FMatrix operator +(FMatrix matrix1, FMatrix matrix2)
		{
			Add(ref matrix1, ref matrix2, out matrix1);
			return matrix1;
		}

		public static FMatrix operator /(FMatrix matrix1, FMatrix matrix2)
		{
			FMatrix result;
			Divide(ref matrix1, ref matrix2, out result);
			return result;
		}

		public static FMatrix operator /(FMatrix matrix1, float divider)
		{
			FMatrix result;
			Divide(ref matrix1, divider, out result);
			return result;
		}

		public static bool operator ==(FMatrix matrix1, FMatrix matrix2)
		{
			return matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M13 == matrix2.M13 && matrix1.M14 == matrix2.M14 && matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 && matrix1.M23 == matrix2.M23 && matrix1.M24 == matrix2.M24 && matrix1.M31 == matrix2.M31 && matrix1.M32 == matrix2.M32 && matrix1.M33 == matrix2.M33 && matrix1.M34 == matrix2.M34 && matrix1.M41 == matrix2.M41 && matrix1.M42 == matrix2.M42 && matrix1.M43 == matrix2.M43 && matrix1.M44 == matrix2.M44;
		}

		public static bool operator !=(FMatrix matrix1, FMatrix matrix2)
		{
			return !(matrix1 == matrix2);
		}

		public static FMatrix operator *(FMatrix matrix1, FMatrix matrix2)
		{
			FMatrix result = default(FMatrix);
			Multiply(ref matrix1, ref matrix2, out result);
			return result;
		}

		public static FMatrix operator *(FMatrix matrix, float scaleFactor)
		{
			Multiply(ref matrix, scaleFactor, out matrix);
			return matrix;
		}

		public static FMatrix operator *(float scaleFactor, FMatrix matrix)
		{
			FMatrix result = default(FMatrix);
			result.M11 = matrix.M11 * scaleFactor;
			result.M12 = matrix.M12 * scaleFactor;
			result.M13 = matrix.M13 * scaleFactor;
			result.M14 = matrix.M14 * scaleFactor;
			result.M21 = matrix.M21 * scaleFactor;
			result.M22 = matrix.M22 * scaleFactor;
			result.M23 = matrix.M23 * scaleFactor;
			result.M24 = matrix.M24 * scaleFactor;
			result.M31 = matrix.M31 * scaleFactor;
			result.M32 = matrix.M32 * scaleFactor;
			result.M33 = matrix.M33 * scaleFactor;
			result.M34 = matrix.M34 * scaleFactor;
			result.M41 = matrix.M41 * scaleFactor;
			result.M42 = matrix.M42 * scaleFactor;
			result.M43 = matrix.M43 * scaleFactor;
			result.M44 = matrix.M44 * scaleFactor;
			return result;
		}

		public static FMatrix operator -(FMatrix matrix1, FMatrix matrix2)
		{
			FMatrix result = default(FMatrix);
			Subtract(ref matrix1, ref matrix2, out result);
			return result;
		}

		public static FMatrix operator -(FMatrix matrix1)
		{
			Negate(ref matrix1, out matrix1);
			return matrix1;
		}
	}
}
