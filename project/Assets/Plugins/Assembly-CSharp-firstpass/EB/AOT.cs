using System;
using System.Collections;
using System.Reflection;

namespace EB
{
	public static class AOT
	{
		public static IEnumerator GetEnumerator(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			Type[] interfaces = obj.GetType().GetInterfaces();
			Type type = Array.Find(interfaces, (Type x) => !x.IsGenericType && x == typeof(IEnumerable));
			if (type == null)
			{
				Debug.LogError("Object does not implement IEnumerable interface: " + obj.GetType());
				return null;
			}
			MethodInfo method = type.GetMethod("GetEnumerator");
			if (method == null)
			{
				Debug.LogError("Failed to get method on: " + obj.GetType());
				return null;
			}
			IEnumerator result = null;
			try
			{
				result = (IEnumerator)method.Invoke(obj, null);
			}
			catch
			{
			}
			return result;
		}

		public static IDictionaryEnumerator GetDictionaryEnumerator(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			Type[] interfaces = obj.GetType().GetInterfaces();
			Type type = Array.Find(interfaces, (Type x) => !x.IsGenericType && x == typeof(IDictionary));
			if (type == null)
			{
				Debug.LogError("Object does not implement IEnumerable interface: " + obj.GetType());
				return null;
			}
			MethodInfo method = type.GetMethod("GetEnumerator");
			if (method == null)
			{
				Debug.LogError("Failed to get method on: " + obj.GetType());
				return null;
			}
			IDictionaryEnumerator result = null;
			try
			{
				result = (IDictionaryEnumerator)method.Invoke(obj, null);
			}
			catch
			{
			}
			return result;
		}
	}
}
