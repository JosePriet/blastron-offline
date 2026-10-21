using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PolygonManipulation
{
	public static class CuttingTools
	{
		public static void SplitShape(Fixture fixture, FVector2 entryPoint, FVector2 exitPoint, float splitSize, out Vertices first, out Vertices second)
		{
			FVector2 localPoint = fixture.Body.GetLocalPoint(ref entryPoint);
			FVector2 localPoint2 = fixture.Body.GetLocalPoint(ref exitPoint);
			PolygonShape polygonShape = fixture.Shape as PolygonShape;
			if (polygonShape == null)
			{
				first = new Vertices();
				second = new Vertices();
				return;
			}
			Vertices vertices = new Vertices(polygonShape.Vertices);
			Vertices[] array = new Vertices[2];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vertices(vertices.Count);
			}
			int[] array2 = new int[2] { -1, -1 };
			int num = -1;
			for (int j = 0; j < vertices.Count; j++)
			{
				int num2 = ((!(FVector2.Dot(MathUtils.Cross(localPoint2 - localPoint, 1f), vertices[j] - localPoint) > 1.1920929E-07f)) ? 1 : 0);
				if (num != num2)
				{
					if (num == 0)
					{
						array2[0] = array[num].Count;
						array[num].Add(localPoint2);
						array[num].Add(localPoint);
					}
					if (num == 1)
					{
						array2[num] = array[num].Count;
						array[num].Add(localPoint);
						array[num].Add(localPoint2);
					}
				}
				array[num2].Add(vertices[j]);
				num = num2;
			}
			if (array2[0] == -1)
			{
				array2[0] = array[0].Count;
				array[0].Add(localPoint2);
				array[0].Add(localPoint);
			}
			if (array2[1] == -1)
			{
				array2[1] = array[1].Count;
				array[1].Add(localPoint);
				array[1].Add(localPoint2);
			}
			for (int k = 0; k < 2; k++)
			{
				FVector2 fVector = ((array2[k] <= 0) ? (array[k][array[k].Count - 1] - array[k][0]) : (array[k][array2[k] - 1] - array[k][array2[k]]));
				fVector.Normalize();
				Vertices vertices3;
				Vertices vertices2 = (vertices3 = array[k]);
				int index2;
				int index = (index2 = array2[k]);
				FVector2 fVector2 = vertices3[index2];
				vertices2[index] = fVector2 + splitSize * fVector;
				fVector = ((array2[k] >= array[k].Count - 2) ? (array[k][0] - array[k][array[k].Count - 1]) : (array[k][array2[k] + 2] - array[k][array2[k] + 1]));
				fVector.Normalize();
				Vertices vertices5;
				Vertices vertices4 = (vertices5 = array[k]);
				int index3 = (index2 = array2[k] + 1);
				fVector2 = vertices5[index2];
				vertices4[index3] = fVector2 + splitSize * fVector;
			}
			first = array[0];
			second = array[1];
		}

		public static void Cut(World world, FVector2 start, FVector2 end, float thickness)
		{
			List<Fixture> fixtures = new List<Fixture>();
			List<FVector2> entryPoints = new List<FVector2>();
			List<FVector2> exitPoints = new List<FVector2>();
			if (world.TestPoint(start) != null || world.TestPoint(end) != null)
			{
				return;
			}
			world.RayCast((Fixture f, FVector2 p, FVector2 n, float fr) =>
			{
				fixtures.Add(f);
				entryPoints.Add(p);
				return 1f;
			}, start, end);
			world.RayCast((Fixture f, FVector2 p, FVector2 n, float fr) =>
			{
				exitPoints.Add(p);
				return 1f;
			}, end, start);
			if (entryPoints.Count + exitPoints.Count < 2)
			{
				return;
			}
			for (int num = 0; num < fixtures.Count; num++)
			{
				if (fixtures[num].Shape.ShapeType == ShapeType.Polygon && fixtures[num].Body.BodyType != BodyType.Static)
				{
					Vertices first;
					Vertices second;
					SplitShape(fixtures[num], entryPoints[num], exitPoints[num], thickness, out first, out second);
					if (SanityCheck(first))
					{
						Body body = BodyFactory.CreatePolygon(world, first, fixtures[num].Shape.Density, fixtures[num].Body.Position);
						body.Rotation = fixtures[num].Body.Rotation;
						body.LinearVelocity = fixtures[num].Body.LinearVelocity;
						body.AngularVelocity = fixtures[num].Body.AngularVelocity;
						body.BodyType = BodyType.Dynamic;
					}
					if (SanityCheck(second))
					{
						Body body2 = BodyFactory.CreatePolygon(world, second, fixtures[num].Shape.Density, fixtures[num].Body.Position);
						body2.Rotation = fixtures[num].Body.Rotation;
						body2.LinearVelocity = fixtures[num].Body.LinearVelocity;
						body2.AngularVelocity = fixtures[num].Body.AngularVelocity;
						body2.BodyType = BodyType.Dynamic;
					}
					world.RemoveBody(fixtures[num].Body);
				}
			}
		}

		private static bool SanityCheck(Vertices vertices)
		{
			if (vertices.Count < 3)
			{
				return false;
			}
			if (vertices.GetArea() < 1E-05f)
			{
				return false;
			}
			for (int i = 0; i < vertices.Count; i++)
			{
				int index = i;
				int index2 = ((i + 1 < vertices.Count) ? (i + 1) : 0);
				if ((vertices[index2] - vertices[index]).LengthSquared() < 1.4210855E-14f)
				{
					return false;
				}
			}
			for (int j = 0; j < vertices.Count; j++)
			{
				int num = j;
				int num2 = ((j + 1 < vertices.Count) ? (j + 1) : 0);
				FVector2 fVector = vertices[num2] - vertices[num];
				for (int k = 0; k < vertices.Count; k++)
				{
					if (k != num && k != num2)
					{
						FVector2 fVector2 = vertices[k] - vertices[num];
						float num3 = fVector.X * fVector2.Y - fVector.Y * fVector2.X;
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
