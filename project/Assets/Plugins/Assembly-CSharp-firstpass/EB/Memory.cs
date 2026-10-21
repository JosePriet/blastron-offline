using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EB
{
	public static class Memory
	{
		private class TypeInfo
		{
			public List<FieldInfo> fields = new List<FieldInfo>();

			public TypeInfo(Type type)
			{
				FieldInfo[] array = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
				FieldInfo[] array2 = array;
				foreach (FieldInfo fieldInfo in array2)
				{
					if (fieldInfo.GetCustomAttributes(typeof(UncheckedAttribute), true).Length == 0)
					{
						fields.Add(fieldInfo);
					}
				}
			}

			private void Hash(Digest digest, object value, bool deep)
			{
				if (value == null)
				{
					return;
				}
				Type type = value.GetType();
				if (type.IsArray || value is IList)
				{
					IEnumerator enumerator = AOT.GetEnumerator(value);
					if (enumerator != null)
					{
						while (enumerator.MoveNext())
						{
							Hash(digest, enumerator.Current, deep);
						}
					}
				}
				else if (value is IDictionary)
				{
					IDictionaryEnumerator dictionaryEnumerator = AOT.GetDictionaryEnumerator(value);
					if (dictionaryEnumerator != null)
					{
						while (dictionaryEnumerator.MoveNext())
						{
							Hash(digest, dictionaryEnumerator.Key, deep);
							Hash(digest, dictionaryEnumerator.Value, deep);
						}
					}
				}
				else if (type == typeof(int))
				{
					digest.Update(BitConverter.GetBytes((int)value));
				}
				else if (type == typeof(uint))
				{
					digest.Update(BitConverter.GetBytes((uint)value));
				}
				else if (type == typeof(bool))
				{
					digest.Update(BitConverter.GetBytes((bool)value));
				}
				else if (type == typeof(ushort))
				{
					digest.Update(BitConverter.GetBytes((ushort)value));
				}
				else if (type == typeof(short))
				{
					digest.Update(BitConverter.GetBytes((short)value));
				}
				else if (type == typeof(float))
				{
					digest.Update(BitConverter.GetBytes((float)value));
				}
				else if (type == typeof(double))
				{
					digest.Update(BitConverter.GetBytes((double)value));
				}
				else if (type == typeof(ulong))
				{
					digest.Update(BitConverter.GetBytes((ulong)value));
				}
				else if (type == typeof(long))
				{
					digest.Update(BitConverter.GetBytes((uint)value));
				}
				else if (type == typeof(string))
				{
					digest.Update(Encoding.GetBytes((string)value));
				}
				else if (type.IsEnum)
				{
					digest.Update(BitConverter.GetBytes(Convert.ToInt32(value)));
				}
				else if (type.IsClass && deep)
				{
					TypeInfo typeInfo = GetTypeInfo(type);
					typeInfo._GetHash(digest, value, deep);
				}
			}

			private void _GetHash(Digest digest, object instance, bool deep)
			{
				foreach (FieldInfo field in fields)
				{
					object value = field.GetValue(instance);
					Hash(digest, value, deep);
				}
			}

			public byte[] GetHash(object instance, bool deep)
			{
				Digest digest = Digest.FNV64();
				_GetHash(digest, instance, deep);
				return digest.Final();
			}
		}

		private class Locked
		{
			public object instance;

			public TypeInfo info;

			public byte[] hash;

			public bool deep;
		}

		private static Dictionary<Type, TypeInfo> _types = new Dictionary<Type, TypeInfo>();

		private static List<Locked> _locked = new List<Locked>();

		private static int _index = 0;

		public static SafeBool Breach = false;

		public static event Action OnBreach;

		private static TypeInfo GetTypeInfo(Type type)
		{
			TypeInfo value;
			lock (_types)
			{
				if (!_types.TryGetValue(type, out value))
				{
					value = new TypeInfo(type);
					_types[type] = value;
				}
			}
			return value;
		}

		public static void Lock(object instance)
		{
			Lock(instance, false);
		}

		public static void Lock(object instance, bool deep)
		{
			if (instance == null)
			{
				return;
			}
			Locked locked = new Locked();
			locked.instance = instance;
			locked.info = GetTypeInfo(instance.GetType());
			locked.deep = deep;
			locked.hash = locked.info.GetHash(instance, locked.deep);
			lock (_locked)
			{
				_locked.Add(locked);
			}
		}

		public static void Unlock(object instance)
		{
			lock (_locked)
			{
				_locked.RemoveAll((Locked obj) => obj.instance == instance);
			}
		}

		public static void Update(int maxItems)
		{
			if ((bool)Breach)
			{
				return;
			}
			lock (_locked)
			{
				maxItems = Mathf.Min(maxItems, _locked.Count);
				for (int i = 0; i < maxItems; i++)
				{
					_index = (_index + 1) % _locked.Count;
					Locked locked = _locked[_index];
					if (locked.instance == null)
					{
						_locked[_index] = _locked[_locked.Count - 1];
						_locked.RemoveAt(_locked.Count - 1);
						continue;
					}
					byte[] hash = locked.info.GetHash(locked.instance, locked.deep);
					for (int j = 0; j < hash.Length; j++)
					{
						if (hash[j] != locked.hash[j])
						{
							Breach = true;
							Debug.LogError(string.Concat("Breach detected! for object ", locked.instance, " ", Encoding.ToHexString(hash), " != ", Encoding.ToHexString(locked.hash)));
							if (OnBreach != null)
							{
								OnBreach();
							}
							return;
						}
					}
				}
			}
		}
	}
}
