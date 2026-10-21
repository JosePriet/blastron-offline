using System;
using UnityEngine;

namespace EB.Sequence.Serialization
{
	[System.Serializable]
	public class Property : ICloneable
	{
		public PropertyType type;

		public string name;

		public int intValue;

		public float floatValue;

		public string stringValue = string.Empty;

		public GameObject gameObjectValue;

		public Color colorValue = default(Color);

		public Vector4 vector4Value = Vector4.zero;

		public Vector2 vector2Value
		{
			get
			{
				return new Vector2(vector4Value.x, vector4Value.y);
			}
			set
			{
				vector4Value = new Vector4(value.x, value.y, 0f, 0f);
			}
		}

		public Vector3 vector3Value
		{
			get
			{
				return new Vector3(vector4Value.x, vector4Value.y, vector4Value.z);
			}
			set
			{
				vector4Value = new Vector4(value.x, value.y, value.z, 0f);
			}
		}

		public object Value
		{
			get
			{
				switch (type)
				{
				case PropertyType.Int:
					return intValue;
				case PropertyType.Float:
					return floatValue;
				case PropertyType.String:
					return stringValue;
				case PropertyType.GameObject:
					return gameObjectValue;
				case PropertyType.Boolean:
					return intValue != 0;
				case PropertyType.Color:
					return colorValue;
				case PropertyType.Vector2:
					return vector2Value;
				case PropertyType.Vector3:
					return vector3Value;
				case PropertyType.Vector4:
					return vector4Value;
				default:
					return null;
				}
			}
			set
			{
				switch (type)
				{
				case PropertyType.Int:
					intValue = (int)value;
					break;
				case PropertyType.Float:
					floatValue = (float)value;
					break;
				case PropertyType.String:
					stringValue = (string)value;
					break;
				case PropertyType.GameObject:
					gameObjectValue = (GameObject)value;
					break;
				case PropertyType.Boolean:
				{
					bool flag = (bool)value;
					intValue = (flag ? 1 : 0);
					break;
				}
				case PropertyType.Color:
				{
					Color color = (Color)value;
					vector4Value = new Vector4(color.r, color.g, color.b, color.a);
					colorValue = color;
					break;
				}
				case PropertyType.Vector2:
					vector2Value = (Vector2)value;
					break;
				case PropertyType.Vector3:
					vector3Value = (Vector3)value;
					break;
				case PropertyType.Vector4:
					vector4Value = (Vector4)value;
					break;
				}
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
