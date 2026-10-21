using System.Collections;

namespace EB
{
	public static class Dot
	{
		public static object Find(string name, object obj)
		{
			if (string.IsNullOrEmpty(name) || obj == null)
			{
				return obj;
			}
			string text = name;
			string name2 = string.Empty;
			int num = name.IndexOf('.');
			if (num != -1)
			{
				text = name.Substring(0, num);
				name2 = name.Substring(num + 1);
			}
			int result = -1;
			int num2 = text.IndexOf('[');
			if (num2 != -1)
			{
				int num3 = text.IndexOf(']', num2 + 1);
				if (num3 != -1)
				{
					string text2 = text.Substring(num2 + 1, num3 - num2 - 1);
					if (!int.TryParse(text2, out result))
					{
						result = -1;
						Debug.LogError("invalid index: " + text2);
						return null;
					}
					text = text.Substring(0, num2);
				}
				if (!string.IsNullOrEmpty(text) && obj is Hashtable)
				{
					Hashtable hashtable = (Hashtable)obj;
					obj = hashtable[text];
				}
				if (result >= 0 && obj is ArrayList)
				{
					ArrayList arrayList = (ArrayList)obj;
					obj = ((result >= arrayList.Count) ? null : arrayList[result]);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (obj is Hashtable)
				{
					Hashtable hashtable2 = (Hashtable)obj;
					obj = hashtable2[text];
				}
				else if (obj is ArrayList && int.TryParse(text, out result))
				{
					ArrayList arrayList2 = (ArrayList)obj;
					if (result < 0 || result >= arrayList2.Count)
					{
						Debug.LogWarning("out of array bounds [index " + result + " vs. arr size " + arrayList2.Count + "], stopping");
						return null;
					}
					obj = arrayList2[result];
				}
			}
			return Find(name2, obj);
		}

		public static object DeepCopy(object source, object dest)
		{
			if (source is Hashtable)
			{
				Hashtable hashtable = (Hashtable)source;
				Hashtable hashtable2 = (Hashtable)dest;
				foreach (DictionaryEntry item in hashtable)
				{
					object value = item.Value;
					if (hashtable2.ContainsKey(item.Key))
					{
						hashtable2[item.Key] = DeepCopy(value, hashtable2[item.Key]);
					}
					else
					{
						hashtable2.Add(item.Key, item.Value);
					}
				}
			}
			else
			{
				dest = source;
			}
			return dest;
		}

		public static ArrayList Array(string name, object obj, ArrayList defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null && obj2 is ArrayList)
			{
				return (ArrayList)obj2;
			}
			return defaultValue;
		}

		public static Hashtable Object(string name, object obj, Hashtable defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null && obj2 is Hashtable)
			{
				return (Hashtable)obj2;
			}
			return defaultValue;
		}

		public static string String(string name, object obj, string defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null)
			{
				return obj2.ToString();
			}
			return defaultValue;
		}

		public static int Integer(string name, object obj, int defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null)
			{
				int result = 0;
				if (int.TryParse(obj2.ToString(), out result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public static uint UInteger(string name, object obj, uint defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null)
			{
				uint result = 0u;
				if (uint.TryParse(obj2.ToString(), out result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public static long Long(string name, object obj, long defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null)
			{
				long result = 0L;
				if (long.TryParse(obj2.ToString(), out result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public static float Single(string name, object obj, float defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null)
			{
				float result = 0f;
				if (float.TryParse(obj2.ToString(), out result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public static bool Bool(string name, object obj, bool defaultValue)
		{
			object obj2 = Find(name, obj);
			if (obj2 != null)
			{
				bool result = false;
				if (bool.TryParse(obj2.ToString(), out result))
				{
					return result;
				}
			}
			return defaultValue;
		}
	}
}
