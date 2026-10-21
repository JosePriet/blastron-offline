using Microsoft.Xna.Framework;
using UnityEngine;

namespace FarseerPhysics.DebugViews
{
	public struct FDVertex
	{
		public Color Color;

		public Vector3[] Position;

		public static FDVertex FromLine(FVector3 begin, FVector3 end, Color color)
		{
			FDVertex result = new FDVertex
			{
				Position = new Vector3[2]
			};
			result.Position[0] = FSHelper.FVector3ToVector3(begin);
			result.Position[1] = FSHelper.FVector3ToVector3(end);
			result.Color = color;
			return result;
		}

		public static FDVertex FromTriangleList(FVector3 p0, FVector3 p1, FVector3 p2, Color color)
		{
			FDVertex result = new FDVertex
			{
				Position = new Vector3[3]
			};
			result.Position[0] = FSHelper.FVector3ToVector3(p0);
			result.Position[1] = FSHelper.FVector3ToVector3(p1);
			result.Position[2] = FSHelper.FVector3ToVector3(p2);
			result.Color = color;
			return result;
		}

		public static FDVertex FromLine(FVector2 begin, FVector2 end, Color color)
		{
			FDVertex result = new FDVertex
			{
				Position = new Vector3[2]
			};
			result.Position[0] = FSHelper.FVector2ToVector3(begin);
			result.Position[1] = FSHelper.FVector2ToVector3(end);
			result.Color = color;
			return result;
		}

		public static FDVertex FromTriangleList(FVector2 p0, FVector2 p1, FVector2 p2, Color color)
		{
			FDVertex result = new FDVertex
			{
				Position = new Vector3[3]
			};
			result.Position[0] = FSHelper.FVector2ToVector3(p0);
			result.Position[1] = FSHelper.FVector2ToVector3(p1);
			result.Position[2] = FSHelper.FVector2ToVector3(p2);
			result.Color = color;
			return result;
		}
	}
}
