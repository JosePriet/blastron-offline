using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EB
{
	public static class Serialization
	{
		private struct SerializationCallbacks
		{
			public SerializeDelagate serialize;

			public DeserializeDelagate deserialize;
		}

		public class CachedClassInfo
		{
			public Type type;

			public SerializableAttribute main;

			public Dictionary<string, FieldInfo> fields;
		}

		public delegate object SerializeDelagate(object obj);

		public delegate object DeserializeDelagate(object value);

		private static Dictionary<Type, SerializationCallbacks> _callbacks = new Dictionary<Type, SerializationCallbacks>();

		private static Dictionary<Type, object> _defaults = new Dictionary<Type, object>();

		private static Dictionary<Type, CachedClassInfo> _classInfo = new Dictionary<Type, CachedClassInfo>();

		private static Type[] kNumericTypes = new Type[6]
		{
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(double),
			typeof(float),
			typeof(byte)
		};

		public static void RegisterCallbacks(Type type, SerializeDelagate serialize, DeserializeDelagate deserialize)
		{
			SerializationCallbacks value = new SerializationCallbacks
			{
				serialize = serialize,
				deserialize = deserialize
			};
			_callbacks[type] = value;
		}

		public static void RegisterDefault(object value)
		{
			RegisterDefault(value.GetType(), value);
		}

		public static void RegisterDefault(Type type, object value)
		{
			_defaults[type] = value;
		}

		public static bool Deserialize<T>(string name, IDictionary hashtable, ref T result)
		{
			try
			{
				object obj = hashtable[name];
				if (obj == null)
				{
					return false;
				}
				result = (T)InternalDeserialize(obj, typeof(T));
			}
			catch (Exception ex)
			{
				Debug.Log("Failed to deserialize: " + ex.ToString());
				return false;
			}
			return result != null;
		}

		public static bool Deserialize<T>(object value, ref T result)
		{
			try
			{
				if (value == null)
				{
					return false;
				}
				result = (T)InternalDeserialize(value, typeof(T));
			}
			catch (Exception ex)
			{
				Debug.Log("Failed to deserialize: " + ex.ToString());
				return false;
			}
			return result != null;
		}

		public static CachedClassInfo GetClassInfo(Type type)
		{
			CachedClassInfo value = null;
			if (!_classInfo.TryGetValue(type, out value))
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(SerializableAttribute), true);
				if (customAttributes.Length > 0)
				{
					value = new CachedClassInfo();
					value.type = type;
					value.main = (SerializableAttribute)customAttributes[0];
					value.fields = new Dictionary<string, FieldInfo>();
					FieldInfo[] fields = type.GetFields();
					FieldInfo[] array = fields;
					foreach (FieldInfo fieldInfo in array)
					{
						if (fieldInfo.GetCustomAttributes(typeof(NoSerializeAttribute), true).Length <= 0 && fieldInfo.IsPublic)
						{
							string fieldName = GetFieldName(fieldInfo);
							if (value.fields.ContainsKey(fieldName))
							{
								Debug.LogError("Error: Type {0} contains conflicting field name {1}", type.Name, fieldName);
							}
							value.fields[fieldName] = fieldInfo;
						}
					}
					_classInfo[type] = value;
				}
			}
			return value;
		}

		private static object CopyArray(List<object> items, Type arrayType, ArrayList counts)
		{
			if (arrayType.IsArray)
			{
				if (arrayType.GetArrayRank() == 2)
				{
					int num = int.Parse(counts[0].ToString());
					int num2 = int.Parse(counts[1].ToString());
					Array array = Array.CreateInstance(arrayType.GetElementType(), num, num2);
					int num3 = num * num2;
					if (num3 != items.Count)
					{
						Debug.Log("Bad!!! size= " + num3 + " count=" + items.Count);
					}
					int num4 = 0;
					int num5 = 0;
					for (int i = 0; i < items.Count; i++)
					{
						array.SetValue(items[i], num4, num5);
						num5++;
						if (num5 == num2)
						{
							num5 = 0;
							num4++;
						}
					}
					return array;
				}
				Array array2 = Array.CreateInstance(arrayType.GetElementType(), items.Count);
				Array.Copy(items.ToArray(), array2, items.Count);
				return array2;
			}
			IList list = (IList)Activator.CreateInstance(arrayType);
			foreach (object item in items)
			{
				list.Add(item);
			}
			return list;
		}

		private static Type GetType(object name)
		{
			if (name == null)
			{
				return null;
			}
			Type type = Type.GetType(name.ToString(), false);
			if (type == null)
			{
				string fullName = Assembly.GetExecutingAssembly().FullName;
				type = Type.GetType(string.Format("{0}, {1}", name.ToString(), fullName));
			}
			return type;
		}

		private static Type GetElementType(Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			if (type.IsGenericType)
			{
				Type[] genericArguments = type.GetGenericArguments();
				if (genericArguments.Length > 0)
				{
					return genericArguments[0];
				}
			}
			return null;
		}

		private static object InternalDeserializeArray(object value, Type type)
		{
			Type elementType = GetElementType(type);
			if (elementType == null)
			{
				Debug.LogError("failed to determine element type of :" + type.ToString());
				return null;
			}
			ArrayList arrayList = null;
			ArrayList arrayList2 = new ArrayList();
			if (value is ArrayList)
			{
				arrayList = (ArrayList)value;
				arrayList2 = new ArrayList();
				arrayList2.Add(arrayList.Count);
			}
			else if (value is Hashtable)
			{
				Hashtable hashtable = (Hashtable)value;
				arrayList = hashtable["items"] as ArrayList;
				arrayList2 = hashtable["ranks"] as ArrayList;
			}
			if (arrayList != null)
			{
				List<object> list = new List<object>();
				foreach (object item in arrayList)
				{
					object obj = InternalDeserialize(item, elementType);
					if (obj != null)
					{
						list.Add(obj);
					}
				}
				return CopyArray(list, type, arrayList2);
			}
			Debug.LogError("Failed to deserialize array of type:" + type.ToString());
			return null;
		}

		private static object InternalDeserialize(object value, Type type)
		{
			if (value == null || value.ToString() == "null")
			{
				return null;
			}
			if (_callbacks.ContainsKey(type))
			{
				return _callbacks[type].deserialize(value);
			}
			if (IsArray(type))
			{
				return InternalDeserializeArray(value, type);
			}
			if (type.IsEnum)
			{
				try
				{
					return Enum.Parse(type, value.ToString(), true);
				}
				catch
				{
					return null;
				}
			}
			if (type == typeof(string))
			{
				return value.ToString();
			}
			if (type == typeof(bool))
			{
				switch (value.ToString().ToLower())
				{
				case "true":
					return true;
				case "false":
					return false;
				case "0":
					return false;
				case "1":
					return true;
				default:
					return false;
				}
			}
			if (type.IsPrimitive)
			{
				return Convert.ChangeType(value.ToString(), type);
			}
			CachedClassInfo classInfo = GetClassInfo(type);
			if (classInfo != null)
			{
				object obj2 = Activator.CreateInstance(type);
				if (value is Hashtable)
				{
					Hashtable hashtable = (Hashtable)value;
					foreach (KeyValuePair<string, FieldInfo> field in classInfo.fields)
					{
						FieldInfo value2 = field.Value;
						string key = field.Key;
						object obj3 = InternalDeserialize(hashtable[key], value2.FieldType);
						if (obj3 != null)
						{
							value2.SetValue(obj2, obj3);
						}
					}
				}
				return obj2;
			}
			Debug.Log("missing serialization attribute " + type.FullName);
			return null;
		}

		public static T CopyFields<T>(object dest, object src)
		{
			Type typeFromHandle = typeof(T);
			FieldInfo[] fields = typeFromHandle.GetFields();
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.GetCustomAttributes(typeof(NoSerializeAttribute), true).Length <= 0 && fieldInfo.IsPublic && !fieldInfo.IsStatic)
				{
					object value = fieldInfo.GetValue(src);
					fieldInfo.SetValue(dest, value);
				}
			}
			return (T)dest;
		}

		public static void Serialize(string name, object obj, IDictionary hashtable)
		{
			hashtable[name] = InternalSerialize(obj);
		}

		public static object Serialize(object obj)
		{
			return InternalSerialize(obj);
		}

		private static object InternalSerializeArray(object obj)
		{
			if (obj == null || !(obj is IList))
			{
				return string.Empty;
			}
			IList list = obj as IList;
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < list.Count; i++)
			{
				arrayList.Add(InternalSerialize(list[i]));
			}
			return arrayList;
		}

		private static bool IsNumeric(object o)
		{
			return Array.IndexOf(kNumericTypes, o.GetType()) >= 0;
		}

		private static bool IsArray(Type type)
		{
			if (type.IsArray)
			{
				return true;
			}
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				if (type2.Equals(typeof(IList)))
				{
					return true;
				}
			}
			return false;
		}

		private static string GetFieldName(FieldInfo field)
		{
			object[] customAttributes = field.GetCustomAttributes(typeof(SerializableAttribute), true);
			if (customAttributes.Length > 0)
			{
				SerializableAttribute serializableAttribute = (SerializableAttribute)customAttributes[0];
				if (!string.IsNullOrEmpty(serializableAttribute.Tag))
				{
					return serializableAttribute.Tag;
				}
			}
			return field.Name.Replace("_", string.Empty);
		}

		private static object InternalSerialize(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			Type type = obj.GetType();
			if (_callbacks.ContainsKey(type))
			{
				return _callbacks[type].serialize(obj);
			}
			if (IsNumeric(obj) || obj is bool)
			{
				return obj;
			}
			if (IsArray(type))
			{
				return InternalSerializeArray(obj);
			}
			if (type.IsEnum)
			{
				return Convert.ChangeType(obj, typeof(int));
			}
			if (type.IsPrimitive || type == typeof(string))
			{
				return obj.ToString();
			}
			CachedClassInfo classInfo = GetClassInfo(type);
			if (classInfo != null)
			{
				Hashtable hashtable = new Hashtable();
				object value = null;
				_defaults.TryGetValue(type, out value);
				{
					foreach (KeyValuePair<string, FieldInfo> field in classInfo.fields)
					{
						FieldInfo value2 = field.Value;
						string key = field.Key;
						object value3 = value2.GetValue(obj);
						if (value != null)
						{
							object value4 = value2.GetValue(value);
							if (value4 != null && value4.Equals(value3))
							{
								continue;
							}
						}
						hashtable[key] = InternalSerialize(value3);
					}
					return hashtable;
				}
			}
			Debug.Log("missing serialization attribute " + type.Name);
			return string.Empty;
		}
	}
}
