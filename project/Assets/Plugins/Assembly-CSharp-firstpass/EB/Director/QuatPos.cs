using UnityEngine;

namespace EB.Director
{
	public struct QuatPos
	{
		public Quaternion quat;

		public Vector3 pos;

		public static QuatPos FromTransformGlobal(Transform t)
		{
			QuatPos result = default(QuatPos);
			result.quat = t.rotation;
			result.pos = t.position;
			return result;
		}

		public static QuatPos FromTransformLocal(Transform t)
		{
			QuatPos result = default(QuatPos);
			result.quat = t.localRotation;
			result.pos = t.localPosition;
			return result;
		}

		public static QuatPos FromTransform(Transform t, SpaceType space)
		{
			if (space == SpaceType.Local)
			{
				return FromTransformLocal(t);
			}
			return FromTransformGlobal(t);
		}

		public void Apply(Transform t, SpaceType space)
		{
			if (space == SpaceType.Local)
			{
				t.localPosition = pos;
				t.localRotation = quat;
			}
			else
			{
				t.position = pos;
				t.rotation = quat;
			}
		}

		public static QuatPos Lerp(QuatPos from, QuatPos to, float t)
		{
			QuatPos result = default(QuatPos);
			result.quat = Quaternion.Lerp(from.quat, to.quat, t);
			result.pos = Vector3.Lerp(from.pos, to.pos, t);
			return result;
		}

		public override string ToString()
		{
			return "quat: " + quat.ToString() + " pos: " + pos.ToString();
		}
	}
}
