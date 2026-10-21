using System;
using Microsoft.Xna.Framework;
using UnityEngine;

public static class FSHelper
{
	public static float ang;

	public static float len;

	public static Vector3 outputV3;

	public static FVector2 Vector3ToFVector2(Vector3 input)
	{
		return new FVector2(input.x, input.y);
	}

	public static Vector3 FVector3ToVector3(FVector3 input)
	{
		return new Vector3(input.X, input.Y, input.Z);
	}

	public static Vector3 FVector2ToVector3(FVector2 input)
	{
		return new Vector3(input.X, input.Y, 0f);
	}

	public static FVector2 Vector2ToFVector2(Vector2 input)
	{
		return new FVector2(input.x, input.y);
	}

	public static Vector3 LocalTranslatedVec3(Vector3 input, Transform localObj)
	{
		outputV3 = default(Vector3);
		ang = Mathf.Atan2(input.y, input.x);
		ang += localObj.localRotation.eulerAngles.z * ((float)Math.PI / 180f);
		len = input.magnitude;
		outputV3.x = Mathf.Cos(ang) * len;
		outputV3.y = Mathf.Sin(ang) * len;
		return outputV3 + localObj.localPosition;
	}
}
