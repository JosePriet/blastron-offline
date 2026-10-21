namespace FarseerPhysics.Common.Decomposition
{
	public class Triangle
	{
		public float[] X;

		public float[] Y;

		public Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
		{
			X = new float[3];
			Y = new float[3];
			float num = x2 - x1;
			float num2 = x3 - x1;
			float num3 = y2 - y1;
			float num4 = y3 - y1;
			float num5 = num * num4 - num2 * num3;
			if (num5 > 0f)
			{
				X[0] = x1;
				X[1] = x2;
				X[2] = x3;
				Y[0] = y1;
				Y[1] = y2;
				Y[2] = y3;
			}
			else
			{
				X[0] = x1;
				X[1] = x3;
				X[2] = x2;
				Y[0] = y1;
				Y[1] = y3;
				Y[2] = y2;
			}
		}

		public Triangle(Triangle t)
		{
			X = new float[3];
			Y = new float[3];
			X[0] = t.X[0];
			X[1] = t.X[1];
			X[2] = t.X[2];
			Y[0] = t.Y[0];
			Y[1] = t.Y[1];
			Y[2] = t.Y[2];
		}

		public bool IsInside(float x, float y)
		{
			if (x < X[0] && x < X[1] && x < X[2])
			{
				return false;
			}
			if (x > X[0] && x > X[1] && x > X[2])
			{
				return false;
			}
			if (y < Y[0] && y < Y[1] && y < Y[2])
			{
				return false;
			}
			if (y > Y[0] && y > Y[1] && y > Y[2])
			{
				return false;
			}
			float num = x - X[0];
			float num2 = y - Y[0];
			float num3 = X[1] - X[0];
			float num4 = Y[1] - Y[0];
			float num5 = X[2] - X[0];
			float num6 = Y[2] - Y[0];
			float num7 = num5 * num5 + num6 * num6;
			float num8 = num5 * num3 + num6 * num4;
			float num9 = num5 * num + num6 * num2;
			float num10 = num3 * num3 + num4 * num4;
			float num11 = num3 * num + num4 * num2;
			float num12 = 1f / (num7 * num10 - num8 * num8);
			float num13 = (num10 * num9 - num8 * num11) * num12;
			float num14 = (num7 * num11 - num8 * num9) * num12;
			return num13 > 0f && num14 > 0f && num13 + num14 < 1f;
		}
	}
}
