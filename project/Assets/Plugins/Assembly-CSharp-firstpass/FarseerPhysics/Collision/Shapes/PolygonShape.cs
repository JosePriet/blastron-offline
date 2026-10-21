using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
	public class PolygonShape : Shape
	{
		public Vertices Normals;

		public Vertices Vertices;

		public override int ChildCount
		{
			get
			{
				return 1;
			}
		}

		public PolygonShape(Vertices vertices, float density)
			: base(density)
		{
			base.ShapeType = ShapeType.Polygon;
			_radius = 0.01f;
			Set(vertices);
		}

		public PolygonShape(float density)
			: base(density)
		{
			base.ShapeType = ShapeType.Polygon;
			_radius = 0.01f;
			Normals = new Vertices(Settings.MaxPolygonVertices);
			Vertices = new Vertices(Settings.MaxPolygonVertices);
		}

		internal PolygonShape()
			: base(0f)
		{
			base.ShapeType = ShapeType.Polygon;
			_radius = 0.01f;
			Normals = new Vertices(Settings.MaxPolygonVertices);
			Vertices = new Vertices(Settings.MaxPolygonVertices);
		}

		public override Shape Clone()
		{
			PolygonShape polygonShape = new PolygonShape();
			polygonShape.ShapeType = base.ShapeType;
			polygonShape._radius = _radius;
			polygonShape._density = _density;
			if (Settings.ConserveMemory)
			{
				polygonShape.Vertices = Vertices;
				polygonShape.Normals = Normals;
			}
			else
			{
				polygonShape.Vertices = new Vertices(Vertices);
				polygonShape.Normals = new Vertices(Normals);
			}
			polygonShape.MassData = MassData;
			return polygonShape;
		}

		public void Set(Vertices input)
		{
			Vertices = new Vertices(input);
			Normals = new Vertices(Vertices.Count);
			for (int i = 0; i < Vertices.Count; i++)
			{
				int index = i;
				int index2 = ((i + 1 < Vertices.Count) ? (i + 1) : 0);
				FVector2 fVector = Vertices[index2] - Vertices[index];
				FVector2 item = new FVector2(fVector.Y, 0f - fVector.X);
				item.Normalize();
				Normals.Add(item);
			}
			ComputeProperties();
		}

		protected override void ComputeProperties()
		{
			if (!(_density <= 0f))
			{
				FVector2 zero = FVector2.Zero;
				float num = 0f;
				float num2 = 0f;
				FVector2 zero2 = FVector2.Zero;
				for (int i = 0; i < Vertices.Count; i++)
				{
					zero2 += Vertices[i];
				}
				zero2 *= 1f / (float)Vertices.Count;
				for (int j = 0; j < Vertices.Count; j++)
				{
					FVector2 fVector = Vertices[j] - zero2;
					FVector2 fVector2 = ((j + 1 >= Vertices.Count) ? (Vertices[0] - zero2) : (Vertices[j + 1] - zero2));
					float num3 = MathUtils.Cross(fVector, fVector2);
					float num4 = 0.5f * num3;
					num += num4;
					zero += num4 * (1f / 3f) * (fVector + fVector2);
					float x = fVector.X;
					float y = fVector.Y;
					float x2 = fVector2.X;
					float y2 = fVector2.Y;
					float num5 = x * x + x2 * x + x2 * x2;
					float num6 = y * y + y2 * y + y2 * y2;
					num2 += 1f / 12f * num3 * (num5 + num6);
				}
				MassData.Area = num;
				MassData.Mass = _density * num;
				zero *= 1f / num;
				MassData.Centroid = zero + zero2;
				MassData.Inertia = _density * num2;
				MassData.Inertia += MassData.Mass * (FVector2.Dot(MassData.Centroid, MassData.Centroid) - FVector2.Dot(zero, zero));
			}
		}

		public void SetAsBox(float halfWidth, float halfHeight)
		{
			Set(PolygonTools.CreateRectangle(halfWidth, halfHeight));
		}

		public void SetAsBox(float halfWidth, float halfHeight, FVector2 center, float angle)
		{
			Set(PolygonTools.CreateRectangle(halfWidth, halfHeight, center, angle));
		}

		public override bool TestPoint(ref Transform transform, ref FVector2 point)
		{
			FVector2 fVector = MathUtils.MulT(transform.q, point - transform.p);
			for (int i = 0; i < Vertices.Count; i++)
			{
				float num = FVector2.Dot(Normals[i], fVector - Vertices[i]);
				if (num > 0f)
				{
					return false;
				}
			}
			return true;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			output = default(RayCastOutput);
			FVector2 fVector = MathUtils.MulT(transform.q, input.Point1 - transform.p);
			FVector2 fVector2 = MathUtils.MulT(transform.q, input.Point2 - transform.p);
			FVector2 value = fVector2 - fVector;
			float num = 0f;
			float num2 = input.MaxFraction;
			int num3 = -1;
			for (int i = 0; i < Vertices.Count; i++)
			{
				float num4 = FVector2.Dot(Normals[i], Vertices[i] - fVector);
				float num5 = FVector2.Dot(Normals[i], value);
				if (num5 == 0f)
				{
					if (num4 < 0f)
					{
						return false;
					}
				}
				else if (num5 < 0f && num4 < num * num5)
				{
					num = num4 / num5;
					num3 = i;
				}
				else if (num5 > 0f && num4 < num2 * num5)
				{
					num2 = num4 / num5;
				}
				if (num2 < num)
				{
					return false;
				}
			}
			if (num3 >= 0)
			{
				output.Fraction = num;
				output.Normal = MathUtils.Mul(transform.q, Normals[num3]);
				return true;
			}
			return false;
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			FVector2 fVector = MathUtils.Mul(ref transform, Vertices[0]);
			FVector2 fVector2 = fVector;
			for (int i = 1; i < Vertices.Count; i++)
			{
				FVector2 value = MathUtils.Mul(ref transform, Vertices[i]);
				fVector = FVector2.Min(fVector, value);
				fVector2 = FVector2.Max(fVector2, value);
			}
			FVector2 fVector3 = new FVector2(base.Radius, base.Radius);
			aabb.LowerBound = fVector - fVector3;
			aabb.UpperBound = fVector2 + fVector3;
		}

		public bool CompareTo(PolygonShape shape)
		{
			if (Vertices.Count != shape.Vertices.Count)
			{
				return false;
			}
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (Vertices[i] != shape.Vertices[i])
				{
					return false;
				}
			}
			return base.Radius == shape.Radius && MassData == shape.MassData;
		}

		public override float ComputeSubmergedArea(FVector2 normal, float offset, Transform xf, out FVector2 sc)
		{
			sc = FVector2.Zero;
			FVector2 value = MathUtils.MulT(xf.q, normal);
			float num = offset - FVector2.Dot(normal, xf.p);
			float[] array = new float[Settings.MaxPolygonVertices];
			int num2 = 0;
			int num3 = -1;
			int num4 = -1;
			bool flag = false;
			int i;
			for (i = 0; i < Vertices.Count; i++)
			{
				array[i] = FVector2.Dot(value, Vertices[i]) - num;
				bool flag2 = array[i] < -1.1920929E-07f;
				if (i > 0)
				{
					if (flag2)
					{
						if (!flag)
						{
							num3 = i - 1;
							num2++;
						}
					}
					else if (flag)
					{
						num4 = i - 1;
						num2++;
					}
				}
				flag = flag2;
			}
			switch (num2)
			{
			case 0:
				if (flag)
				{
					sc = MathUtils.Mul(ref xf, MassData.Centroid);
					return MassData.Mass / base.Density;
				}
				return 0f;
			case 1:
				if (num3 == -1)
				{
					num3 = Vertices.Count - 1;
				}
				else
				{
					num4 = Vertices.Count - 1;
				}
				break;
			}
			int num5 = (num3 + 1) % Vertices.Count;
			int num6 = (num4 + 1) % Vertices.Count;
			float num7 = (0f - array[num3]) / (array[num5] - array[num3]);
			float num8 = (0f - array[num4]) / (array[num6] - array[num4]);
			FVector2 fVector = new FVector2(Vertices[num3].X * (1f - num7) + Vertices[num5].X * num7, Vertices[num3].Y * (1f - num7) + Vertices[num5].Y * num7);
			FVector2 fVector2 = new FVector2(Vertices[num4].X * (1f - num8) + Vertices[num6].X * num8, Vertices[num4].Y * (1f - num8) + Vertices[num6].Y * num8);
			float num9 = 0f;
			FVector2 v = new FVector2(0f, 0f);
			FVector2 fVector3 = Vertices[num5];
			float num10 = 1f / 3f;
			i = num5;
			while (i != num6)
			{
				i = (i + 1) % Vertices.Count;
				FVector2 fVector4 = ((i != num6) ? Vertices[i] : fVector2);
				FVector2 a = fVector3 - fVector;
				FVector2 b = fVector4 - fVector;
				float num11 = MathUtils.Cross(a, b);
				float num12 = 0.5f * num11;
				num9 += num12;
				v += num12 * num10 * (fVector + fVector3 + fVector4);
				fVector3 = fVector4;
			}
			v *= 1f / num9;
			sc = MathUtils.Mul(ref xf, v);
			return num9;
		}

		public bool Validate()
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				int num = i;
				int num2 = ((i < Vertices.Count - 1) ? (num + 1) : 0);
				FVector2 fVector = Vertices[num];
				FVector2 a = Vertices[num2] - fVector;
				for (int j = 0; j < Vertices.Count; j++)
				{
					if (j != num && j != num2)
					{
						FVector2 b = Vertices[j] - fVector;
						float num3 = MathUtils.Cross(a, b);
						if (num3 < 0f)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}
}
