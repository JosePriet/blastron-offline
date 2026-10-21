using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
	public static class FixtureFactory
	{
		public static Fixture AttachEdge(FVector2 start, FVector2 end, Body body)
		{
			return AttachEdge(start, end, body, null);
		}

		public static Fixture AttachEdge(FVector2 start, FVector2 end, Body body, object userData)
		{
			EdgeShape shape = new EdgeShape(start, end);
			return body.CreateFixture(shape, userData);
		}

		public static Fixture AttachChainShape(Vertices vertices, Body body)
		{
			return AttachChainShape(vertices, body, null);
		}

		public static Fixture AttachChainShape(Vertices vertices, Body body, object userData)
		{
			ChainShape shape = new ChainShape(vertices);
			return body.CreateFixture(shape, userData);
		}

		public static Fixture AttachRectangle(float width, float height, float density, FVector2 offset, Body body, object userData)
		{
			Vertices vertices = PolygonTools.CreateRectangle(width / 2f, height / 2f);
			vertices.Translate(ref offset);
			PolygonShape shape = new PolygonShape(vertices, density);
			return body.CreateFixture(shape, userData);
		}

		public static Fixture AttachRectangle(float width, float height, float density, FVector2 offset, Body body)
		{
			return AttachRectangle(width, height, density, offset, body, null);
		}

		public static Fixture AttachCircle(float radius, float density, Body body)
		{
			return AttachCircle(radius, density, body, null);
		}

		public static Fixture AttachCircle(float radius, float density, Body body, object userData)
		{
			if (radius <= 0f)
			{
				throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");
			}
			CircleShape shape = new CircleShape(radius, density);
			return body.CreateFixture(shape, userData);
		}

		public static Fixture AttachCircle(float radius, float density, Body body, FVector2 offset)
		{
			return AttachCircle(radius, density, body, offset, null);
		}

		public static Fixture AttachCircle(float radius, float density, Body body, FVector2 offset, object userData)
		{
			if (radius <= 0f)
			{
				throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");
			}
			CircleShape circleShape = new CircleShape(radius, density);
			circleShape.Position = offset;
			return body.CreateFixture(circleShape, userData);
		}

		public static Fixture AttachPolygon(Vertices vertices, float density, Body body)
		{
			return AttachPolygon(vertices, density, body, null);
		}

		public static Fixture AttachPolygon(Vertices vertices, float density, Body body, object userData)
		{
			if (vertices.Count <= 1)
			{
				throw new ArgumentOutOfRangeException("vertices", "Too few points to be a polygon");
			}
			PolygonShape shape = new PolygonShape(vertices, density);
			return body.CreateFixture(shape, userData);
		}

		public static Fixture AttachEllipse(float xRadius, float yRadius, int edges, float density, Body body)
		{
			return AttachEllipse(xRadius, yRadius, edges, density, body, null);
		}

		public static Fixture AttachEllipse(float xRadius, float yRadius, int edges, float density, Body body, object userData)
		{
			if (xRadius <= 0f)
			{
				throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");
			}
			if (yRadius <= 0f)
			{
				throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");
			}
			Vertices vertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
			PolygonShape shape = new PolygonShape(vertices, density);
			return body.CreateFixture(shape, userData);
		}

		public static List<Fixture> AttachCompoundPolygon(List<Vertices> list, float density, Body body)
		{
			return AttachCompoundPolygon(list, density, body, null);
		}

		public static List<Fixture> AttachCompoundPolygon(List<Vertices> list, float density, Body body, object userData)
		{
			List<Fixture> list2 = new List<Fixture>(list.Count);
			foreach (Vertices item in list)
			{
				if (item.Count == 2)
				{
					EdgeShape shape = new EdgeShape(item[0], item[1]);
					list2.Add(body.CreateFixture(shape, userData));
				}
				else
				{
					PolygonShape shape2 = new PolygonShape(item, density);
					list2.Add(body.CreateFixture(shape2, userData));
				}
			}
			return list2;
		}

		public static List<Fixture> AttachLineArc(float radians, int sides, float radius, FVector2 position, float angle, bool closed, Body body)
		{
			Vertices vertices = PolygonTools.CreateArc(radians, sides, radius);
			vertices.Rotate(((float)Math.PI - radians) / 2f + angle);
			vertices.Translate(ref position);
			List<Fixture> list = new List<Fixture>(vertices.Count);
			if (closed)
			{
				list.Add(AttachChainShape(vertices, body));
			}
			for (int i = 1; i < vertices.Count; i++)
			{
				list.Add(AttachEdge(vertices[i], vertices[i - 1], body));
			}
			return list;
		}

		public static List<Fixture> AttachSolidArc(float density, float radians, int sides, float radius, FVector2 position, float angle, Body body)
		{
			Vertices vertices = PolygonTools.CreateArc(radians, sides, radius);
			vertices.Rotate(((float)Math.PI - radians) / 2f + angle);
			vertices.Translate(ref position);
			vertices.Add(vertices[0]);
			List<Vertices> list = EarclipDecomposer.ConvexPartition(vertices);
			return AttachCompoundPolygon(list, density, body);
		}
	}
}
