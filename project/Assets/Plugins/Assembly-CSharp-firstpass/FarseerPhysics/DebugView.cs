using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
	public abstract class DebugView
	{
		protected World World { get; private set; }

		public DebugViewFlags Flags { get; set; }

		protected DebugView(World world)
		{
			World = world;
		}

		public void AppendFlags(DebugViewFlags flags)
		{
			Flags |= flags;
		}

		public void RemoveFlags(DebugViewFlags flags)
		{
			Flags &= ~flags;
		}

		public abstract void DrawPolygon(FVector2[] vertices, int count, float red, float blue, float green);

		public abstract void DrawSolidPolygon(FVector2[] vertices, int count, float red, float blue, float green);

		public abstract void DrawCircle(FVector2 center, float radius, float red, float blue, float green);

		public abstract void DrawSolidCircle(FVector2 center, float radius, FVector2 axis, float red, float blue, float green);

		public abstract void DrawSegment(FVector2 start, FVector2 end, float red, float blue, float green);

		public abstract void DrawTransform(ref Transform transform);
	}
}
