using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace Terrain
{
	public class Tools
	{
		public delegate void RayCastCallback(FVector2 hit, FVector2 normal);

		public static void DownProbe(FVector2 p0, FVector2 p1, Vertices poly, RayCastCallback fnCB)
		{
			FVector2 b = poly[poly.Count - 1];
			for (int i = 0; i < poly.Count; i++)
			{
				FVector2 fVector = poly[i];
				float ta;
				float tb;
				if (fVector.X < b.X && Misc.EBGLineIntersect(p0, p1, b, fVector, out ta, out tb))
				{
					fnCB(FVector2.Lerp(p0, p1, ta), FVector2.UnitY);
				}
				b = fVector;
			}
		}

		public static void Raycast(FVector2 p0, FVector2 p1, Vertices poly, RayCastCallback fnCB)
		{
			FVector2 b = poly[poly.Count - 1];
			for (int i = 0; i < poly.Count; i++)
			{
				FVector2 fVector = poly[i];
				float ta;
				float tb;
				if (Misc.EBGLineIntersect(p0, p1, b, fVector, out ta, out tb))
				{
					fnCB(FVector2.Lerp(p0, p1, ta), FVector2.UnitY);
				}
				b = fVector;
			}
		}
	}
}
