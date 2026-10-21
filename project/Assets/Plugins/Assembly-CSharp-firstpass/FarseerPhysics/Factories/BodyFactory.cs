using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
	public static class BodyFactory
	{
		public static Body CreateBody(World world)
		{
			return CreateBody(world, null);
		}

		public static Body CreateBody(World world, object userData)
		{
			return new Body(world, userData);
		}

		public static Body CreateBody(World world, FVector2 position)
		{
			return CreateBody(world, position, null);
		}

		public static Body CreateBody(World world, FVector2 position, object userData)
		{
			Body body = CreateBody(world, userData);
			body.Position = position;
			return body;
		}

		public static Body CreateEdge(World world, FVector2 start, FVector2 end)
		{
			return CreateEdge(world, start, end, null);
		}

		public static Body CreateEdge(World world, FVector2 start, FVector2 end, object userData)
		{
			Body body = CreateBody(world);
			FixtureFactory.AttachEdge(start, end, body, userData);
			return body;
		}

		public static Body CreateChainShape(World world, Vertices vertices)
		{
			return CreateChainShape(world, vertices, null);
		}

		public static Body CreateChainShape(World world, Vertices vertices, object userData)
		{
			return CreateChainShape(world, vertices, FVector2.Zero, userData);
		}

		public static Body CreateChainShape(World world, Vertices vertices, FVector2 position)
		{
			return CreateChainShape(world, vertices, position, null);
		}

		public static Body CreateChainShape(World world, Vertices vertices, FVector2 position, object userData)
		{
			Body body = CreateBody(world, position);
			FixtureFactory.AttachChainShape(vertices, body, userData);
			return body;
		}

		public static Body CreateRectangle(World world, float width, float height, float density)
		{
			return CreateRectangle(world, width, height, density, null);
		}

		public static Body CreateRectangle(World world, float width, float height, float density, object userData)
		{
			return CreateRectangle(world, width, height, density, FVector2.Zero, userData);
		}

		public static Body CreateRectangle(World world, float width, float height, float density, FVector2 position)
		{
			return CreateRectangle(world, width, height, density, position, null);
		}

		public static Body CreateRectangle(World world, float width, float height, float density, FVector2 position, object userData)
		{
			if (width <= 0f)
			{
				throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");
			}
			if (height <= 0f)
			{
				throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");
			}
			Body body = CreateBody(world, position);
			Vertices vertices = PolygonTools.CreateRectangle(width / 2f, height / 2f);
			PolygonShape shape = new PolygonShape(vertices, density);
			body.CreateFixture(shape, userData);
			return body;
		}

		public static Body CreateCircle(World world, float radius, float density)
		{
			return CreateCircle(world, radius, density, null);
		}

		public static Body CreateCircle(World world, float radius, float density, object userData)
		{
			return CreateCircle(world, radius, density, FVector2.Zero, userData);
		}

		public static Body CreateCircle(World world, float radius, float density, FVector2 position)
		{
			return CreateCircle(world, radius, density, position, null);
		}

		public static Body CreateCircle(World world, float radius, float density, FVector2 position, object userData)
		{
			Body body = CreateBody(world, position);
			FixtureFactory.AttachCircle(radius, density, body, userData);
			return body;
		}

		public static Body CreateEllipse(World world, float xRadius, float yRadius, int edges, float density)
		{
			return CreateEllipse(world, xRadius, yRadius, edges, density, null);
		}

		public static Body CreateEllipse(World world, float xRadius, float yRadius, int edges, float density, object userData)
		{
			return CreateEllipse(world, xRadius, yRadius, edges, density, FVector2.Zero, userData);
		}

		public static Body CreateEllipse(World world, float xRadius, float yRadius, int edges, float density, FVector2 position)
		{
			return CreateEllipse(world, xRadius, yRadius, edges, density, position, null);
		}

		public static Body CreateEllipse(World world, float xRadius, float yRadius, int edges, float density, FVector2 position, object userData)
		{
			Body body = CreateBody(world, position);
			FixtureFactory.AttachEllipse(xRadius, yRadius, edges, density, body, userData);
			return body;
		}

		public static Body CreatePolygon(World world, Vertices vertices, float density)
		{
			return CreatePolygon(world, vertices, density, null);
		}

		public static Body CreatePolygon(World world, Vertices vertices, float density, object userData)
		{
			return CreatePolygon(world, vertices, density, FVector2.Zero, userData);
		}

		public static Body CreatePolygon(World world, Vertices vertices, float density, FVector2 position)
		{
			return CreatePolygon(world, vertices, density, position, null);
		}

		public static Body CreatePolygon(World world, Vertices vertices, float density, FVector2 position, object userData)
		{
			Body body = CreateBody(world, position);
			FixtureFactory.AttachPolygon(vertices, density, body, userData);
			return body;
		}

		public static Body CreateCompoundPolygon(World world, List<Vertices> list, float density)
		{
			return CreateCompoundPolygon(world, list, density, BodyType.Static);
		}

		public static Body CreateCompoundPolygon(World world, List<Vertices> list, float density, object userData)
		{
			return CreateCompoundPolygon(world, list, density, FVector2.Zero, userData);
		}

		public static Body CreateCompoundPolygon(World world, List<Vertices> list, float density, FVector2 position)
		{
			return CreateCompoundPolygon(world, list, density, position, null);
		}

		public static Body CreateCompoundPolygon(World world, List<Vertices> list, float density, FVector2 position, object userData)
		{
			Body body = CreateBody(world, position);
			FixtureFactory.AttachCompoundPolygon(list, density, body, userData);
			return body;
		}

		public static Body CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density)
		{
			return CreateGear(world, radius, numberOfTeeth, tipPercentage, toothHeight, density, null);
		}

		public static Body CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density, object userData)
		{
			Vertices vertices = PolygonTools.CreateGear(radius, numberOfTeeth, tipPercentage, toothHeight);
			if (!vertices.IsConvex())
			{
				List<Vertices> list = EarclipDecomposer.ConvexPartition(vertices);
				return CreateCompoundPolygon(world, list, density, userData);
			}
			return CreatePolygon(world, vertices, density, userData);
		}

		public static Body CreateCapsule(World world, float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges, float density, FVector2 position, object userData)
		{
			Vertices vertices = PolygonTools.CreateCapsule(height, topRadius, topEdges, bottomRadius, bottomEdges);
			Body body;
			if (vertices.Count >= Settings.MaxPolygonVertices)
			{
				List<Vertices> list = EarclipDecomposer.ConvexPartition(vertices);
				body = CreateCompoundPolygon(world, list, density, userData);
				body.Position = position;
				return body;
			}
			body = CreatePolygon(world, vertices, density, userData);
			body.Position = position;
			return body;
		}

		public static Body CreateCapsule(World world, float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges, float density, FVector2 position)
		{
			return CreateCapsule(world, height, topRadius, topEdges, bottomRadius, bottomEdges, density, position, null);
		}

		public static Body CreateCapsule(World world, float height, float endRadius, float density)
		{
			return CreateCapsule(world, height, endRadius, density, null);
		}

		public static Body CreateCapsule(World world, float height, float endRadius, float density, object userData)
		{
			Vertices item = PolygonTools.CreateRectangle(endRadius, height / 2f);
			List<Vertices> list = new List<Vertices>();
			list.Add(item);
			Body body = CreateCompoundPolygon(world, list, density, userData);
			CircleShape circleShape = new CircleShape(endRadius, density);
			circleShape.Position = new FVector2(0f, height / 2f);
			body.CreateFixture(circleShape, userData);
			CircleShape circleShape2 = new CircleShape(endRadius, density);
			circleShape2.Position = new FVector2(0f, 0f - height / 2f);
			body.CreateFixture(circleShape2, userData);
			return body;
		}

		public static Body CreateRoundedRectangle(World world, float width, float height, float xRadius, float yRadius, int segments, float density, FVector2 position, object userData)
		{
			Vertices vertices = PolygonTools.CreateRoundedRectangle(width, height, xRadius, yRadius, segments);
			if (vertices.Count >= Settings.MaxPolygonVertices)
			{
				List<Vertices> list = EarclipDecomposer.ConvexPartition(vertices);
				Body body = CreateCompoundPolygon(world, list, density, userData);
				body.Position = position;
				return body;
			}
			return CreatePolygon(world, vertices, density);
		}

		public static Body CreateRoundedRectangle(World world, float width, float height, float xRadius, float yRadius, int segments, float density, FVector2 position)
		{
			return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, position, null);
		}

		public static Body CreateRoundedRectangle(World world, float width, float height, float xRadius, float yRadius, int segments, float density)
		{
			return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, null);
		}

		public static Body CreateRoundedRectangle(World world, float width, float height, float xRadius, float yRadius, int segments, float density, object userData)
		{
			return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, FVector2.Zero, userData);
		}

		public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density)
		{
			return CreateBreakableBody(world, vertices, density, null);
		}

		public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, object userData)
		{
			return CreateBreakableBody(world, vertices, density, FVector2.Zero, userData);
		}

		public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, FVector2 position, object userData)
		{
			List<Vertices> vertices2 = EarclipDecomposer.ConvexPartition(vertices);
			BreakableBody breakableBody = new BreakableBody(vertices2, world, density, userData);
			breakableBody.MainBody.Position = position;
			world.AddBreakableBody(breakableBody);
			return breakableBody;
		}

		public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density, FVector2 position)
		{
			return CreateBreakableBody(world, vertices, density, position, null);
		}

		public static Body CreateLineArc(World world, float radians, int sides, float radius, FVector2 position, float angle, bool closed)
		{
			Body body = CreateBody(world);
			FixtureFactory.AttachLineArc(radians, sides, radius, position, angle, closed, body);
			return body;
		}

		public static Body CreateSolidArc(World world, float density, float radians, int sides, float radius, FVector2 position, float angle)
		{
			Body body = CreateBody(world);
			FixtureFactory.AttachSolidArc(density, radians, sides, radius, position, angle, body);
			return body;
		}
	}
}
