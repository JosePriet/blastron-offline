using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
	public static class LinkFactory
	{
		public static Path CreateChain(World world, FVector2 start, FVector2 end, float linkWidth, float linkHeight, bool fixStart, bool fixEnd, int numberOfLinks, float linkDensity)
		{
			Path path = new Path();
			path.Add(start);
			path.Add(end);
			PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(linkWidth, linkHeight), linkDensity);
			List<Body> bodies = PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);
			PathManager.AttachBodiesWithRevoluteJoint(world, bodies, new FVector2(0f, 0f - linkHeight), new FVector2(0f, linkHeight), false, false);
			return path;
		}
	}
}
