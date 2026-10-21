using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision
{
	public class TempPolygon
	{
		public int Count;

		public FVector2[] Normals = new FVector2[Settings.MaxPolygonVertices];

		public FVector2[] Vertices = new FVector2[Settings.MaxPolygonVertices];
	}
}
