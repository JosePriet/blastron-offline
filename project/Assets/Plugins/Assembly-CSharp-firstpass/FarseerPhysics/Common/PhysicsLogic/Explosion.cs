using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PhysicsLogic
{
	public sealed class Explosion : PhysicsLogic
	{
		private const float MaxEdgeOffset = (float)Math.PI / 90f;

		public float EdgeRatio = 0.025f;

		public bool IgnoreWhenInsideShape;

		public float MaxAngle = (float)Math.PI / 15f;

		public int MaxShapes = 100;

		public int MinRays = 5;

		private List<ShapeData> _data = new List<ShapeData>();

		private Dictionary<Fixture, List<FVector2>> _exploded;

		private RayDataComparer _rdc;

		public Explosion(World world)
			: base(world, PhysicsLogicType.Explosion)
		{
			_exploded = new Dictionary<Fixture, List<FVector2>>();
			_rdc = new RayDataComparer();
			_data = new List<ShapeData>();
		}

		public Dictionary<Fixture, List<FVector2>> Activate(FVector2 pos, float radius, float maxForce)
		{
			_exploded.Clear();
			AABB aabb = default(AABB);
			aabb.LowerBound = pos + new FVector2(0f - radius, 0f - radius);
			aabb.UpperBound = pos + new FVector2(radius, radius);
			Fixture[] shapes = new Fixture[MaxShapes];
			Fixture[] containedShapes = new Fixture[5];
			bool exit = false;
			int shapeCount = 0;
			int containedShapeCount = 0;
			World.QueryAABB((Fixture fixture3) =>
			{
				if (fixture3.TestPoint(ref pos))
				{
					if (IgnoreWhenInsideShape)
					{
						exit = true;
					}
					else
					{
						containedShapes[containedShapeCount++] = fixture3;
					}
				}
				else
				{
					shapes[shapeCount++] = fixture3;
				}
				return true;
			}, ref aabb);
			if (exit)
			{
				return _exploded;
			}
			float[] array = new float[shapeCount * 2];
			int num = 0;
			for (int num2 = 0; num2 < shapeCount; num2++)
			{
				CircleShape circleShape = shapes[num2].Shape as CircleShape;
				PolygonShape polygonShape;
				if (circleShape != null)
				{
					Vertices vertices = new Vertices();
					FVector2 item = FVector2.Zero + new FVector2(circleShape.Radius, 0f);
					vertices.Add(item);
					item = FVector2.Zero + new FVector2(0f, circleShape.Radius);
					vertices.Add(item);
					item = FVector2.Zero + new FVector2(0f - circleShape.Radius, circleShape.Radius);
					vertices.Add(item);
					item = FVector2.Zero + new FVector2(0f, 0f - circleShape.Radius);
					vertices.Add(item);
					polygonShape = new PolygonShape(vertices, 0f);
				}
				else
				{
					polygonShape = shapes[num2].Shape as PolygonShape;
				}
				if (shapes[num2].Body.BodyType != BodyType.Dynamic || polygonShape == null)
				{
					continue;
				}
				FVector2 fVector = shapes[num2].Body.GetWorldPoint(polygonShape.MassData.Centroid) - pos;
				float num3 = (float)Math.Atan2(fVector.Y, fVector.X);
				float num4 = float.MaxValue;
				float num5 = float.MinValue;
				float num6 = 0f;
				float num7 = 0f;
				for (int num8 = 0; num8 < polygonShape.Vertices.Count(); num8++)
				{
					FVector2 fVector2 = shapes[num2].Body.GetWorldPoint(polygonShape.Vertices[num8]) - pos;
					float num9 = (float)Math.Atan2(fVector2.Y, fVector2.X);
					float num10 = num9 - num3;
					num10 = (num10 - (float)Math.PI) % ((float)Math.PI * 2f);
					if (num10 < 0f)
					{
						num10 += (float)Math.PI * 2f;
					}
					num10 -= (float)Math.PI;
					if (Math.Abs(num10) > (float)Math.PI)
					{
						throw new ArgumentException("OMG!");
					}
					if (num10 > num5)
					{
						num5 = num10;
						num7 = num9;
					}
					if (num10 < num4)
					{
						num4 = num10;
						num6 = num9;
					}
				}
				array[num] = num6;
				num++;
				array[num] = num7;
				num++;
			}
			Array.Sort(array, 0, num, _rdc);
			_data.Clear();
			bool flag = true;
			ShapeData item2 = default(ShapeData);
			for (int num11 = 0; num11 < num; num11++)
			{
				Fixture shape = null;
				int num12 = ((num11 != num - 1) ? (num11 + 1) : 0);
				if (array[num11] == array[num12])
				{
					continue;
				}
				float num13 = ((num11 != num - 1) ? (array[num11 + 1] + array[num11]) : (array[0] + (float)Math.PI * 2f + array[num11]));
				num13 /= 2f;
				FVector2 point = pos;
				FVector2 point2 = radius * new FVector2((float)Math.Cos(num13), (float)Math.Sin(num13)) + pos;
				bool hitClosest = false;
				World.RayCast((Fixture f, FVector2 p, FVector2 n, float fr) =>
				{
					Body body = f.Body;
					if (!IsActiveOn(body))
					{
						return 0f;
					}
					if (body.UserData != null && (int)body.UserData == 0)
					{
						return -1f;
					}
					hitClosest = true;
					shape = f;
					return fr;
				}, point, point2);
				if (hitClosest && shape.Body.BodyType == BodyType.Dynamic)
				{
					if (_data.Count() > 0 && _data.Last().Body == shape.Body && !flag)
					{
						int index = _data.Count - 1;
						ShapeData value = _data[index];
						value.Max = array[num12];
						_data[index] = value;
					}
					else
					{
						item2.Body = shape.Body;
						item2.Min = array[num11];
						item2.Max = array[num12];
						_data.Add(item2);
					}
					if (_data.Count() > 1 && num11 == num - 1 && _data.Last().Body == _data.First().Body && _data.Last().Max == _data.First().Min)
					{
						ShapeData value2 = _data[0];
						value2.Min = _data.Last().Min;
						_data.RemoveAt(_data.Count() - 1);
						_data[0] = value2;
						while (_data.First().Min >= _data.First().Max)
						{
							value2.Min -= (float)Math.PI * 2f;
							_data[0] = value2;
						}
					}
					int index2 = _data.Count - 1;
					ShapeData value3 = _data[index2];
					while (_data.Count() > 0 && _data.Last().Min >= _data.Last().Max)
					{
						value3.Min = _data.Last().Min - (float)Math.PI * 2f;
						_data[index2] = value3;
					}
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			RayCastInput input = default(RayCastInput);
			for (int num14 = 0; num14 < _data.Count(); num14++)
			{
				if (!IsActiveOn(_data[num14].Body))
				{
					continue;
				}
				float num15 = _data[num14].Max - _data[num14].Min;
				float num16 = MathHelper.Min((float)Math.PI / 90f, EdgeRatio * num15);
				int num17 = (int)Math.Ceiling((num15 - 2f * num16 - (float)(MinRays - 1) * MaxAngle) / MaxAngle);
				if (num17 < 0)
				{
					num17 = 0;
				}
				float num18 = (num15 - num16 * 2f) / ((float)MinRays + (float)num17 - 1f);
				for (float num19 = _data[num14].Min + num16; num19 < _data[num14].Max || MathUtils.FloatEquals(num19, _data[num14].Max, 0.0001f); num19 += num18)
				{
					FVector2 fVector3 = pos;
					FVector2 fVector4 = pos + radius * new FVector2((float)Math.Cos(num19), (float)Math.Sin(num19));
					FVector2 point3 = FVector2.Zero;
					float num20 = float.MaxValue;
					List<Fixture> fixtureList = _data[num14].Body.FixtureList;
					for (int num21 = 0; num21 < fixtureList.Count; num21++)
					{
						Fixture fixture = fixtureList[num21];
						input.Point1 = fVector3;
						input.Point2 = fVector4;
						input.MaxFraction = 50f;
						RayCastOutput output;
						if (fixture.RayCast(out output, ref input, 0) && num20 > output.Fraction)
						{
							num20 = output.Fraction;
							point3 = output.Fraction * fVector4 + (1f - output.Fraction) * fVector3;
						}
						float num22 = num15 / (float)(MinRays + num17) * maxForce * 180f / (float)Math.PI * (1f - Math.Min(1f, num20));
						FVector2 impulse = FVector2.Dot(num22 * new FVector2((float)Math.Cos(num19), (float)Math.Sin(num19)), -output.Normal) * new FVector2((float)Math.Cos(num19), (float)Math.Sin(num19));
						_data[num14].Body.ApplyLinearImpulse(ref impulse, ref point3);
						FVector2 zero = FVector2.Zero;
						List<FVector2> value4;
						if (_exploded.TryGetValue(fixture, out value4))
						{
							zero.X += Math.Abs(impulse.X);
							zero.Y += Math.Abs(impulse.Y);
							value4.Add(zero);
						}
						else
						{
							value4 = new List<FVector2>();
							zero.X = Math.Abs(impulse.X);
							zero.Y = Math.Abs(impulse.Y);
							value4.Add(zero);
							_exploded.Add(fixture, value4);
						}
						if (num20 > 1f)
						{
							point3 = fVector4;
						}
					}
				}
			}
			for (int num23 = 0; num23 < containedShapeCount; num23++)
			{
				Fixture fixture2 = containedShapes[num23];
				if (IsActiveOn(fixture2.Body))
				{
					float num24 = (float)MinRays * maxForce * 180f / (float)Math.PI;
					CircleShape circleShape2 = fixture2.Shape as CircleShape;
					FVector2 point4;
					if (circleShape2 != null)
					{
						point4 = fixture2.Body.GetWorldPoint(circleShape2.Position);
					}
					else
					{
						PolygonShape polygonShape2 = fixture2.Shape as PolygonShape;
						point4 = fixture2.Body.GetWorldPoint(polygonShape2.MassData.Centroid);
					}
					FVector2 impulse2 = num24 * (point4 - pos);
					List<FVector2> list = new List<FVector2>();
					list.Add(impulse2);
					fixture2.Body.ApplyLinearImpulse(ref impulse2, ref point4);
					if (!_exploded.ContainsKey(fixture2))
					{
						_exploded.Add(fixture2, list);
					}
				}
			}
			return _exploded;
		}
	}
}
