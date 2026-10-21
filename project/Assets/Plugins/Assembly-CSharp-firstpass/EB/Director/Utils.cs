using System;
using System.Reflection;
using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director
{
	public static class Utils
	{
		public static bool HasGroupInput(GroupType type)
		{
			switch (type)
			{
			case GroupType.Actor:
			case GroupType.Camera:
			case GroupType.ImageEffect:
				return true;
			default:
				return false;
			}
		}

		public static bool HasBlendMode(TrackType type)
		{
			switch (type)
			{
			case TrackType.Event:
			case TrackType.EnableComponent:
				return false;
			default:
				return true;
			}
		}

		private static void SetValue(FieldInfo field, object instance, object value)
		{
			try
			{
				field.SetValue(instance, value);
			}
			catch
			{
			}
		}

		public static void ApplyVariable(VariableType type, FieldInfo field, object instance, float blend, KeyFrame from, KeyFrame to)
		{
			switch (type)
			{
			case VariableType.Float:
				SetValue(field, instance, Mathf.Lerp(from.FloatValue, to.FloatValue, blend));
				break;
			case VariableType.Vector2:
				SetValue(field, instance, Vector2.Lerp(from.Vector2Value, to.Vector2Value, blend));
				break;
			case VariableType.Vector3:
				SetValue(field, instance, Vector3.Lerp(from.Vector3Value, to.Vector3Value, blend));
				break;
			case VariableType.Vector4:
				SetValue(field, instance, Vector4.Lerp(from.Vector4Value, to.Vector4Value, blend));
				break;
			case VariableType.Color:
				SetValue(field, instance, Color.Lerp(from.ColorValue, to.ColorValue, blend));
				break;
			}
		}

		public static VariableType GetVariableType(FieldInfo field)
		{
			if (field != null)
			{
				if (field.FieldType == typeof(float))
				{
					return VariableType.Float;
				}
				if (field.FieldType == typeof(Vector2))
				{
					return VariableType.Vector2;
				}
				if (field.FieldType == typeof(Vector3))
				{
					return VariableType.Vector3;
				}
				if (field.FieldType == typeof(Vector4))
				{
					return VariableType.Vector4;
				}
				if (field.FieldType == typeof(Color))
				{
					return VariableType.Color;
				}
			}
			return VariableType.None;
		}

		public static Type GetType(string name)
		{
			return Type.GetType(name, false);
		}
	}
}
