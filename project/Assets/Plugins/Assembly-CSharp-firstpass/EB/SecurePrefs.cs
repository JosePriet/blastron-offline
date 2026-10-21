using System;
using UnityEngine;

namespace EB
{
	public static class SecurePrefs
	{
		public static Hmac Key = Hmac.Sha1(Crypto.CreateKey("3498=32wcqeDS#!wfad93r2/3tf"));

		private static string GetKey(string key)
		{
			return key;
		}

		private static byte[] Get(string key)
		{
			string text = PlayerPrefs.GetString(GetKey(key), string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				return SignedRequest.Parse(text, Key);
			}
			return null;
		}

		private static void Set(string key, byte[] data)
		{
			if (data != null)
			{
				PlayerPrefs.SetString(GetKey(key), SignedRequest.Stringify(data, Key));
			}
			else
			{
				PlayerPrefs.DeleteKey(GetKey(key));
			}
			Save();
		}

		public static void Save()
		{
			PlayerPrefs.Save();
		}

		public static void DeleteKey(params string[] keys)
		{
			foreach (string key in keys)
			{
				PlayerPrefs.DeleteKey(GetKey(key));
			}
			Save();
		}

		public static int GetInt(string key, int defaultValue)
		{
			byte[] array = Get(key);
			if (array != null)
			{
				return BitConverter.ToInt32(array, 0);
			}
			return defaultValue;
		}

		public static void SetInt(string key, int value)
		{
			Set(key, BitConverter.GetBytes(value));
		}

		public static float GetFloat(string key, float defaultValue)
		{
			byte[] array = Get(key);
			if (array != null)
			{
				return BitConverter.ToSingle(array, 0);
			}
			return defaultValue;
		}

		public static void SetFloat(string key, float value)
		{
			Set(key, BitConverter.GetBytes(value));
		}

		public static string GetString(string key, string defaultValue)
		{
			byte[] array = Get(key);
			if (array != null)
			{
				return Encoding.GetString(array);
			}
			return defaultValue;
		}

		public static void SetString(string key, string value)
		{
			Set(key, Encoding.GetBytes(value));
		}

		public static object GetJSON(string key)
		{
			string text = GetString(key, string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				return JSON.Parse(text);
			}
			return null;
		}

		public static string SetJSON(string key, object json)
		{
			string text = JSON.Stringify(json);
			SetString(key, text);
			return text;
		}
	}
}
