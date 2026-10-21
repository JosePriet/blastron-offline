using System;

namespace EB.Sequence.Serialization
{
	[System.Serializable]
	public enum PropertyType
	{
		None = 0,
		Int = 1,
		Float = 2,
		String = 3,
		GameObject = 4,
		Boolean = 5,
		Color = 6,
		Vector2 = 7,
		Vector3 = 8,
		Vector4 = 9
	}
}
