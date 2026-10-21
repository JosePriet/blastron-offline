using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public struct Mat33
	{
		public FVector3 ex;

		public FVector3 ey;

		public FVector3 ez;

		public Mat33(FVector3 c1, FVector3 c2, FVector3 c3)
		{
			ex = c1;
			ey = c2;
			ez = c3;
		}

		public void SetZero()
		{
			ex = FVector3.Zero;
			ey = FVector3.Zero;
			ez = FVector3.Zero;
		}

		public FVector3 Solve33(FVector3 b)
		{
			float num = FVector3.Dot(ex, FVector3.Cross(ey, ez));
			if (num != 0f)
			{
				num = 1f / num;
			}
			return new FVector3(num * FVector3.Dot(b, FVector3.Cross(ey, ez)), num * FVector3.Dot(ex, FVector3.Cross(b, ez)), num * FVector3.Dot(ex, FVector3.Cross(ey, b)));
		}

		public FVector2 Solve22(FVector2 b)
		{
			float x = ex.X;
			float x2 = ey.X;
			float y = ex.Y;
			float y2 = ey.Y;
			float num = x * y2 - x2 * y;
			if (num != 0f)
			{
				num = 1f / num;
			}
			return new FVector2(num * (y2 * b.X - x2 * b.Y), num * (x * b.Y - y * b.X));
		}

		public void GetInverse22(ref Mat33 M)
		{
			float x = ex.X;
			float x2 = ey.X;
			float y = ex.Y;
			float y2 = ey.Y;
			float num = x * y2 - x2 * y;
			if (num != 0f)
			{
				num = 1f / num;
			}
			M.ex.X = num * y2;
			M.ey.X = (0f - num) * x2;
			M.ex.Z = 0f;
			M.ex.Y = (0f - num) * y;
			M.ey.Y = num * x;
			M.ey.Z = 0f;
			M.ez.X = 0f;
			M.ez.Y = 0f;
			M.ez.Z = 0f;
		}

		public void GetSymInverse33(ref Mat33 M)
		{
			float num = MathUtils.Dot(ex, MathUtils.Cross(ey, ez));
			if (num != 0f)
			{
				num = 1f / num;
			}
			float x = ex.X;
			float x2 = ey.X;
			float x3 = ez.X;
			float y = ey.Y;
			float y2 = ez.Y;
			float z = ez.Z;
			M.ex.X = num * (y * z - y2 * y2);
			M.ex.Y = num * (x3 * y2 - x2 * z);
			M.ex.Z = num * (x2 * y2 - x3 * y);
			M.ey.X = M.ex.Y;
			M.ey.Y = num * (x * z - x3 * x3);
			M.ey.Z = num * (x3 * x2 - x * y2);
			M.ez.X = M.ex.Z;
			M.ez.Y = M.ey.Z;
			M.ez.Z = num * (x * y - x2 * x2);
		}
	}
}
