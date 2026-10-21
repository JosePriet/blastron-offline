using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	[DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
	public class Vertices : List<FVector2>
	{
		private class PolyNode
		{
			private const int MaxConnected = 32;

			public PolyNode[] Connected = new PolyNode[32];

			public int NConnected;

			public FVector2 Position;

			public PolyNode(FVector2 pos)
			{
				Position = pos;
				NConnected = 0;
			}

			private bool IsRighter(float sinA, float cosA, float sinB, float cosB)
			{
				if (sinA < 0f)
				{
					if (sinB > 0f || cosA <= cosB)
					{
						return true;
					}
					return false;
				}
				if (sinB < 0f || cosA <= cosB)
				{
					return false;
				}
				return true;
			}

			public void AddConnection(PolyNode toMe)
			{
				for (int i = 0; i < NConnected; i++)
				{
					if (Connected[i] == toMe)
					{
						return;
					}
				}
				Connected[NConnected] = toMe;
				NConnected++;
			}

			public void RemoveConnection(PolyNode fromMe)
			{
				bool flag = false;
				int num = -1;
				for (int i = 0; i < NConnected; i++)
				{
					if (fromMe == Connected[i])
					{
						flag = true;
						num = i;
						break;
					}
				}
				NConnected--;
				for (int j = num; j < NConnected; j++)
				{
					Connected[j] = Connected[j + 1];
				}
			}

			public PolyNode GetRightestConnection(PolyNode incoming)
			{
				if (NConnected == 0)
				{
				}
				if (NConnected == 1)
				{
					return incoming;
				}
				FVector2 fVector = Position - incoming.Position;
				float num = fVector.Length();
				fVector.Normalize();
				PolyNode polyNode = null;
				for (int i = 0; i < NConnected; i++)
				{
					if (Connected[i] == incoming)
					{
						continue;
					}
					FVector2 fVector2 = Connected[i].Position - Position;
					float num2 = fVector2.LengthSquared();
					fVector2.Normalize();
					float cosA = FVector2.Dot(fVector, fVector2);
					float sinA = MathUtils.Cross(fVector, fVector2);
					if (polyNode != null)
					{
						FVector2 fVector3 = polyNode.Position - Position;
						fVector3.Normalize();
						float cosB = FVector2.Dot(fVector, fVector3);
						float sinB = MathUtils.Cross(fVector, fVector3);
						if (IsRighter(sinA, cosA, sinB, cosB))
						{
							polyNode = Connected[i];
						}
					}
					else
					{
						polyNode = Connected[i];
					}
				}
				return polyNode;
			}

			public PolyNode GetRightestConnection(FVector2 incomingDir)
			{
				FVector2 pos = Position - incomingDir;
				PolyNode incoming = new PolyNode(pos);
				return GetRightestConnection(incoming);
			}
		}

		public Vertices()
		{
		}

		public Vertices(int capacity)
		{
			base.Capacity = capacity;
		}

		public Vertices(FVector2[] vector2)
		{
			for (int i = 0; i < vector2.Length; i++)
			{
				Add(vector2[i]);
			}
		}

		public Vertices(IList<FVector2> vertices)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				Add(vertices[i]);
			}
		}

		public int NextIndex(int index)
		{
			if (index == Count - 1)
			{
				return 0;
			}
			return index + 1;
		}

		public FVector2 NextVertex(int index)
		{
			return this[NextIndex(index)];
		}

		public int PreviousIndex(int index)
		{
			if (index == 0)
			{
				return Count - 1;
			}
			return index - 1;
		}

		public FVector2 PreviousVertex(int index)
		{
			return this[PreviousIndex(index)];
		}

		public float GetSignedArea()
		{
			float num = 0f;
			for (int i = 0; i < Count; i++)
			{
				int index = (i + 1) % Count;
				num += this[i].X * this[index].Y;
				num -= this[i].Y * this[index].X;
			}
			return num / 2f;
		}

		public float GetArea()
		{
			float num = 0f;
			for (int i = 0; i < Count; i++)
			{
				int index = (i + 1) % Count;
				num += this[i].X * this[index].Y;
				num -= this[i].Y * this[index].X;
			}
			num /= 2f;
			return (!(num < 0f)) ? num : (0f - num);
		}

		public FVector2 GetCentroid()
		{
			FVector2 zero = FVector2.Zero;
			float num = 0f;
			FVector2 zero2 = FVector2.Zero;
			for (int i = 0; i < Count; i++)
			{
				FVector2 fVector = zero2;
				FVector2 fVector2 = this[i];
				FVector2 fVector3 = ((i + 1 >= Count) ? this[0] : this[i + 1]);
				FVector2 a = fVector2 - fVector;
				FVector2 b = fVector3 - fVector;
				float num2 = MathUtils.Cross(a, b);
				float num3 = 0.5f * num2;
				num += num3;
				zero += num3 * (1f / 3f) * (fVector + fVector2 + fVector3);
			}
			return zero * (1f / num);
		}

		public float GetRadius()
		{
			float signedArea = GetSignedArea();
			double num = (double)signedArea / 3.1415927410125732;
			if (num < 0.0)
			{
				num *= -1.0;
			}
			return (float)Math.Sqrt(num);
		}

		public AABB GetCollisionBox()
		{
			FVector2 lowerBound = new FVector2(float.MaxValue, float.MaxValue);
			FVector2 upperBound = new FVector2(float.MinValue, float.MinValue);
			for (int i = 0; i < Count; i++)
			{
				if (this[i].X < lowerBound.X)
				{
					lowerBound.X = this[i].X;
				}
				if (this[i].X > upperBound.X)
				{
					upperBound.X = this[i].X;
				}
				if (this[i].Y < lowerBound.Y)
				{
					lowerBound.Y = this[i].Y;
				}
				if (this[i].Y > upperBound.Y)
				{
					upperBound.Y = this[i].Y;
				}
			}
			AABB result = default(AABB);
			result.LowerBound = lowerBound;
			result.UpperBound = upperBound;
			return result;
		}

		public void Translate(FVector2 vector)
		{
			Translate(ref vector);
		}

		public void Translate(ref FVector2 vector)
		{
			for (int i = 0; i < Count; i++)
			{
				this[i] = FVector2.Add(this[i], vector);
			}
		}

		public void Scale(ref FVector2 value)
		{
			for (int i = 0; i < Count; i++)
			{
				this[i] = FVector2.Multiply(this[i], value);
			}
		}

		public void Rotate(float value)
		{
			FMatrix result;
			FMatrix.CreateRotationZ(value, out result);
			for (int i = 0; i < Count; i++)
			{
				this[i] = FVector2.Transform(this[i], result);
			}
		}

		public bool IsConvex()
		{
			for (int i = 0; i < Count; i++)
			{
				int num = i;
				int num2 = ((i + 1 < Count) ? (i + 1) : 0);
				FVector2 fVector = this[num2] - this[num];
				for (int j = 0; j < Count; j++)
				{
					if (j != num && j != num2)
					{
						FVector2 fVector2 = this[j] - this[num];
						float num3 = fVector.X * fVector2.Y - fVector.Y * fVector2.X;
						if (num3 <= 0f)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public bool IsCounterClockWise()
		{
			if (Count < 3)
			{
				return true;
			}
			return GetSignedArea() > 0f;
		}

		public void ForceCounterClockWise()
		{
			if (!IsCounterClockWise())
			{
				Reverse();
			}
		}

		public bool IsSimple()
		{
			for (int i = 0; i < Count; i++)
			{
				int index = ((i + 1 <= Count - 1) ? (i + 1) : 0);
				FVector2 a = new FVector2(this[i].X, this[i].Y);
				FVector2 a2 = new FVector2(this[index].X, this[index].Y);
				for (int j = i + 1; j < Count; j++)
				{
					int index2 = ((j + 1 <= Count - 1) ? (j + 1) : 0);
					FVector2 b = new FVector2(this[j].X, this[j].Y);
					FVector2 b2 = new FVector2(this[index2].X, this[index2].Y);
					FVector2 intersectionPoint;
					if (LineTools.LineIntersect2(a, a2, b, b2, out intersectionPoint))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool IsSimple2()
		{
			for (int i = 0; i < Count; i++)
			{
				if (i < Count - 1)
				{
					for (int j = i + 1; j < Count; j++)
					{
						if (this[i] == this[j])
						{
							return true;
						}
					}
				}
				int num = (i + 1) % Count;
				FVector2 fVector = this[num] - this[i];
				FVector2 value = new FVector2(fVector.Y, 0f - fVector.X);
				int num2 = (num + 1) % Count;
				int num3 = (i - 1 + Count) % Count;
				num3 += ((num2 >= num3) ? (num2 + 1) : 0);
				int num4 = num2;
				FVector2 value2 = this[num4] - this[i];
				bool flag = FVector2.Dot(value2, value) >= 0f;
				FVector2 fVector2 = this[num4];
				for (num4++; num4 <= num3; num4++)
				{
					int index = num4 % Count;
					value2 = this[index] - this[i];
					if (flag != FVector2.Dot(value2, value) >= 0f)
					{
						FVector2 fVector3 = this[index] - fVector2;
						FVector2 value3 = new FVector2(fVector3.Y, 0f - fVector3.X);
						if (FVector2.Dot(this[i] - fVector2, value3) >= 0f != FVector2.Dot(this[num] - fVector2, value3) >= 0f)
						{
							return true;
						}
					}
					flag = FVector2.Dot(value2, value) > 0f;
					fVector2 = this[index];
				}
			}
			return false;
		}

		public bool CheckPolygon(out int error, out string errorMessage)
		{
			error = -1;
			errorMessage = null;
			if (Count < 3 || Count > Settings.MaxPolygonVertices)
			{
				error = 0;
			}
			if (!IsConvex())
			{
				error = 1;
			}
			if (!IsSimple())
			{
				error = 2;
			}
			if (GetArea() < 1.1920929E-07f)
			{
				error = 3;
			}
			FVector2[] array = new FVector2[Count];
			Vertices vertices = new Vertices(Count);
			for (int i = 0; i < Count; i++)
			{
				vertices.Add(new FVector2(this[i].X, this[i].Y));
				int index = i;
				int index2 = ((i + 1 < Count) ? (i + 1) : 0);
				FVector2 a = new FVector2(this[index2].X - this[index].X, this[index2].Y - this[index].Y);
				array[i] = MathUtils.Cross(a, 1f);
				array[i].Normalize();
			}
			for (int j = 0; j < Count; j++)
			{
				int num = ((j != 0) ? (j - 1) : (Count - 1));
				float a2 = MathUtils.Cross(array[num], array[j]);
				a2 = MathUtils.Clamp(a2, -1f, 1f);
				float num2 = (float)Math.Asin(a2);
				if (num2 <= 0.03490659f)
				{
					error = 4;
					break;
				}
				for (int k = 0; k < Count; k++)
				{
					if (k != j && k != (j + 1) % Count)
					{
						float num3 = FVector2.Dot(array[j], vertices[k] - vertices[j]);
						if (num3 >= -0.005f)
						{
							error = 5;
						}
					}
				}
				FVector2 centroid = vertices.GetCentroid();
				FVector2 value = array[num];
				FVector2 value2 = array[j];
				FVector2 value3 = vertices[j] - centroid;
				FVector2 fVector = new FVector2
				{
					X = FVector2.Dot(value, value3),
					Y = FVector2.Dot(value2, value3)
				};
				if (fVector.X < 0f || fVector.Y < 0f)
				{
					error = 6;
				}
			}
			if (error != -1)
			{
				switch (error)
				{
				case 0:
					errorMessage = string.Format("Polygon error: must have between 3 and {0} vertices.", Settings.MaxPolygonVertices);
					break;
				case 1:
					errorMessage = "Polygon error: must be convex.";
					break;
				case 2:
					errorMessage = "Polygon error: must be simple (cannot intersect itself).";
					break;
				case 3:
					errorMessage = "Polygon error: area is too small.";
					break;
				case 4:
					errorMessage = "Polygon error: sides are too close to parallel.";
					break;
				case 5:
					errorMessage = "Polygon error: polygon is too thin.";
					break;
				case 6:
					errorMessage = "Polygon error: core shape generation would move edge past centroid (too thin).";
					break;
				default:
					errorMessage = "Polygon error: error " + error;
					break;
				}
			}
			return error != -1;
		}

		public bool CheckPolygon()
		{
			int error;
			string errorMessage;
			bool flag = CheckPolygon(out error, out errorMessage);
			if (flag || errorMessage != null)
			{
			}
			return flag;
		}

		public Vertices TraceEdge(Vertices verts)
		{
			PolyNode[] array = new PolyNode[verts.Count * verts.Count];
			int num = 0;
			for (int i = 0; i < verts.Count; i++)
			{
				FVector2 position = new FVector2(verts[i].X, verts[i].Y);
				array[i].Position = position;
				num++;
				int num2 = ((i != verts.Count - 1) ? (i + 1) : 0);
				int num3 = ((i != 0) ? (i - 1) : (verts.Count - 1));
				array[i].AddConnection(array[num2]);
				array[i].AddConnection(array[num3]);
			}
			bool flag = true;
			int num4 = 0;
			while (flag)
			{
				flag = false;
				for (int j = 0; j < num; j++)
				{
					int k;
					int l;
					int num5;
					FVector2 intersectionPoint;
					for (k = 0; k < array[j].NConnected; k++)
					{
						for (l = 0; l < num; l++)
						{
							if (l == j || array[l] == array[j].Connected[k])
							{
								continue;
							}
							num5 = 0;
							while (num5 < array[l].NConnected)
							{
								if (array[l].Connected[num5] == array[j].Connected[k] || array[l].Connected[num5] == array[j] || !LineTools.LineIntersect(array[j].Position, array[j].Connected[k].Position, array[l].Position, array[l].Connected[num5].Position, out intersectionPoint))
								{
									num5++;
									continue;
								}
								goto IL_0175;
							}
						}
					}
					continue;
					IL_0175:
					flag = true;
					PolyNode polyNode = array[j].Connected[k];
					PolyNode polyNode2 = array[l].Connected[num5];
					array[j].Connected[k].RemoveConnection(array[j]);
					array[j].RemoveConnection(polyNode);
					array[l].Connected[num5].RemoveConnection(array[l]);
					array[l].RemoveConnection(polyNode2);
					array[num] = new PolyNode(intersectionPoint);
					array[num].AddConnection(array[j]);
					array[j].AddConnection(array[num]);
					array[num].AddConnection(array[l]);
					array[l].AddConnection(array[num]);
					array[num].AddConnection(polyNode);
					polyNode.AddConnection(array[num]);
					array[num].AddConnection(polyNode2);
					polyNode2.AddConnection(array[num]);
					num++;
					break;
				}
				num4++;
			}
			bool flag2 = true;
			int num6 = num;
			while (flag2)
			{
				flag2 = false;
				for (int m = 0; m < num; m++)
				{
					if (array[m].NConnected == 0)
					{
						continue;
					}
					for (int n = m + 1; n < num; n++)
					{
						if (array[n].NConnected == 0 || !((array[m].Position - array[n].Position).LengthSquared() <= 1.4210855E-14f))
						{
							continue;
						}
						if (num6 <= 3)
						{
							return new Vertices();
						}
						num6--;
						flag2 = true;
						PolyNode polyNode3 = array[m];
						PolyNode polyNode4 = array[n];
						int nConnected = polyNode4.NConnected;
						for (int num7 = 0; num7 < nConnected; num7++)
						{
							PolyNode polyNode5 = polyNode4.Connected[num7];
							if (polyNode5 != polyNode3)
							{
								polyNode3.AddConnection(polyNode5);
								polyNode5.AddConnection(polyNode3);
							}
							polyNode5.RemoveConnection(polyNode4);
						}
						polyNode4.NConnected = 0;
					}
				}
			}
			float num8 = float.MaxValue;
			float num9 = float.MinValue;
			int num10 = -1;
			for (int num11 = 0; num11 < num; num11++)
			{
				if (array[num11].Position.Y < num8 && array[num11].NConnected > 1)
				{
					num8 = array[num11].Position.Y;
					num10 = num11;
					num9 = array[num11].Position.X;
				}
				else if (array[num11].Position.Y == num8 && array[num11].Position.X > num9 && array[num11].NConnected > 1)
				{
					num10 = num11;
					num9 = array[num11].Position.X;
				}
			}
			FVector2 incomingDir = new FVector2(1f, 0f);
			FVector2[] array2 = new FVector2[4 * num];
			int num12 = 0;
			PolyNode polyNode6 = array[num10];
			PolyNode polyNode7 = polyNode6;
			PolyNode rightestConnection = polyNode6.GetRightestConnection(incomingDir);
			if (rightestConnection == null)
			{
				Vertices vertices = new Vertices(num12);
				for (int num13 = 0; num13 < num12; num13++)
				{
					vertices.Add(array2[num13]);
				}
				return vertices;
			}
			array2[0] = polyNode7.Position;
			num12++;
			while (rightestConnection != polyNode7)
			{
				if (num12 > 4 * num)
				{
				}
				array2[num12++] = rightestConnection.Position;
				PolyNode incoming = polyNode6;
				polyNode6 = rightestConnection;
				rightestConnection = polyNode6.GetRightestConnection(incoming);
				if (rightestConnection == null)
				{
					Vertices vertices2 = new Vertices(num12);
					for (int num14 = 0; num14 < num12; num14++)
					{
						vertices2.Add(array2[num14]);
					}
					return vertices2;
				}
			}
			return new Vertices();
		}

		public List<Vertices> SplitAtHoles()
		{
			List<Vertices> list = new List<Vertices>();
			List<FVector2> list2 = new List<FVector2>();
			int num = 0;
			int num2 = 0;
			bool flag = false;
			list.Add(new Vertices());
			for (int i = 0; i < Count; i++)
			{
				for (int j = i + 1; j < Count; j++)
				{
					if (this[i] == this[j])
					{
						list2.Add(this[i]);
					}
				}
				if (flag)
				{
					flag = false;
					continue;
				}
				list[num2].Add(this[i]);
				if (list2.Contains(this[i]))
				{
					if (num2 == 0)
					{
						num++;
						num2 = num;
						list.Add(new Vertices());
					}
					else
					{
						num2 = 0;
					}
					flag = true;
				}
			}
			return list;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Count; i++)
			{
				stringBuilder.Append(this[i].ToString());
				if (i < Count - 1)
				{
					stringBuilder.Append(" ");
				}
			}
			return stringBuilder.ToString();
		}

		public void ProjectToAxis(ref FVector2 axis, out float min, out float max)
		{
			max = (min = FVector2.Dot(axis, this[0]));
			for (int i = 0; i < Count; i++)
			{
				float num = FVector2.Dot(this[i], axis);
				if (num < min)
				{
					min = num;
				}
				else if (num > max)
				{
					max = num;
				}
			}
		}

		public int PointInPolygon(ref FVector2 point)
		{
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				FVector2 a = this[i];
				FVector2 b = this[NextIndex(i)];
				FVector2 value = b - a;
				float num2 = MathUtils.Area(ref a, ref b, ref point);
				if (num2 == 0f && FVector2.Dot(point - a, value) >= 0f && FVector2.Dot(point - b, value) <= 0f)
				{
					return 0;
				}
				if (a.Y <= point.Y)
				{
					if (b.Y > point.Y && num2 > 0f)
					{
						num++;
					}
				}
				else if (b.Y <= point.Y && num2 < 0f)
				{
					num--;
				}
			}
			return (num != 0) ? 1 : (-1);
		}

		public bool PointInPolygonAngle(ref FVector2 point)
		{
			double num = 0.0;
			for (int i = 0; i < Count; i++)
			{
				FVector2 p = this[i] - point;
				FVector2 p2 = this[NextIndex(i)] - point;
				num += MathUtils.VectorAngle(ref p, ref p2);
			}
			if (Math.Abs(num) < Math.PI)
			{
				return false;
			}
			return true;
		}
	}
}
