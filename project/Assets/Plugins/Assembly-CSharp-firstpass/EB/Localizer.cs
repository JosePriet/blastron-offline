using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EB
{
	public static class Localizer
	{
		private class FormatProvider : IFormatProvider, ICustomFormatter
		{
			public object GetFormat(Type formatType)
			{
				return this;
			}

			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				string empty = string.Empty;
				empty = ((arg == null) ? "null" : arg.ToString());
				if (empty.StartsWith("ID_"))
				{
					empty = GetString(empty);
				}
				return empty;
			}
		}

		private const int SecondsInMin = 60;

		private const int SecondsInHour = 3600;

		private const int SecondsInDay = 86400;

		public static bool ShowStatuses = false;

		public static bool ShowStringIds = false;

		public static bool ShowLongestStrings = false;

		private static Dictionary<string, string> _strings = new Dictionary<string, string>();

		public static Dictionary<string, LocStatus> _status = new Dictionary<string, LocStatus>();

		private static FormatProvider _provider = new FormatProvider();

		private static CultureInfo _culture = null;

		public static Dictionary<string, string> Strings
		{
			get
			{
				return _strings;
			}
		}

		public static Language Current { get; private set; }

		public static void Clear()
		{
			_status.Clear();
			_strings.Clear();
		}

		public static IDictionary LoadAllFromResources(Language locale, bool loadCommon)
		{
			if (ShowLongestStrings)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				Dictionary<string, LocStatus> dictionary2 = new Dictionary<string, LocStatus>();
				foreach (Language value in Symbols.LanguageCode.Values)
				{
					InternalLoadAllFromResources(value, false);
					foreach (string key in _strings.Keys)
					{
						string text = _strings[key];
						if (_status[key] == LocStatus.Translated || _status[key] == LocStatus.Source || _status[key] == LocStatus.Placeholder)
						{
							if (!dictionary.ContainsKey(key))
							{
								dictionary.Add(key, text);
								dictionary2.Add(key, _status[key]);
							}
							else if (dictionary[key].Length < text.Length)
							{
								dictionary[key] = text;
								dictionary2[key] = _status[key];
							}
						}
					}
					Clear();
				}
				_strings = dictionary;
				_status = dictionary2;
				return _strings;
			}
			return InternalLoadAllFromResources(locale, loadCommon);
		}

		private static IDictionary InternalLoadAllFromResources(Language locale, bool loadCommon)
		{
			string oldLanguageCode = GetOldLanguageCode(locale);
			Current = locale;
			if (loadCommon)
			{
				UnityEngine.Object[] array = Assets.LoadAll("Languages/common", typeof(TextAsset));
				for (int i = 0; i < array.Length; i++)
				{
					TextAsset textAsset = (TextAsset)array[i];
					string data = Encoding.GetString(textAsset.bytes);
					LoadStringsInternal(data);
					Assets.Unload(textAsset);
				}
			}
			UnityEngine.Object[] array2 = Assets.LoadAll("Languages/" + oldLanguageCode, typeof(TextAsset));
			for (int j = 0; j < array2.Length; j++)
			{
				TextAsset textAsset2 = (TextAsset)array2[j];
				string data2 = Encoding.GetString(textAsset2.bytes);
				LoadStringsInternal(data2);
				Assets.Unload(textAsset2);
			}
			return _strings;
		}

		public static void Dump()
		{
			foreach (string key in _strings.Keys)
			{
				Debug.Log("String (" + key + ") " + Encoding.ToHexString(Encoding.GetBytes(key)) + " " + _strings[key]);
			}
		}

		public static string GetLanguageCode(Language locale)
		{
			foreach (KeyValuePair<string, Language> item in Symbols.LanguageCode)
			{
				if (item.Value == locale)
				{
					return item.Key;
				}
			}
			return locale.ToString().ToLower();
		}

		public static string GetOldLanguageCode(Language locale)
		{
			foreach (KeyValuePair<string, Language> item in Symbols.OldLanguageCode)
			{
				if (item.Value == locale)
				{
					return item.Key;
				}
			}
			return locale.ToString().ToLower();
		}

		public static Hashtable LoadStrings(Language locale, string database)
		{
			Current = locale;
			string oldLanguageCode = GetOldLanguageCode(locale);
			string path = "Languages/" + oldLanguageCode + "/" + database;
			TextAsset textAsset = (TextAsset)Assets.Load(path, typeof(TextAsset));
			if (textAsset != null)
			{
				string data = Encoding.GetString(textAsset.bytes);
				Hashtable hashtable = LoadStringsInternal(data);
				Debug.Log("Localizer loaded " + hashtable.Count + " strings for database " + database);
				Assets.Unload(textAsset);
				return hashtable;
			}
			Debug.LogWarning(string.Concat("Failed to load local: ", locale, " for database ", database));
			return null;
		}

		public static bool HasString(string id)
		{
			lock (_strings)
			{
				return _strings.ContainsKey(id);
			}
		}

		public static bool GetString(string id, out string value)
		{
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogError("empty or null string id");
				value = "MS_NULL";
				return false;
			}
			if (ShowStringIds)
			{
				value = id;
				return true;
			}
			lock (_strings)
			{
				if (_strings.TryGetValue(id, out value))
				{
					return true;
				}
			}
			Debug.LogWarning("Missing String (" + id + ") ");
			value = "MS_" + id;
			return false;
		}

		public static string GetString(string id)
		{
			string value;
			GetString(id, out value);
			return value;
		}

		public static LocStatus GetStatus(string id)
		{
			lock (_strings)
			{
				LocStatus value;
				if (_status.TryGetValue(id, out value))
				{
					return value;
				}
			}
			return LocStatus.Missing;
		}

		public static Color GetColor(string id)
		{
			switch (GetStatus(id))
			{
			case LocStatus.Translated:
				return Color.blue;
			case LocStatus.Placeholder:
				return Color.yellow;
			case LocStatus.For_Translation:
				return Color.yellow;
			case LocStatus.Missing:
				return Color.red;
			default:
				return Color.white;
			}
		}

		public static string Format(string id, params object[] args)
		{
			if (ShowStringIds)
			{
				return GetString(id);
			}
			return string.Format(_provider, GetString(id), args);
		}

		private static Hashtable LoadStringsInternal(string data)
		{
			LocFile locFile = new LocFile(_strings, _status);
			return locFile.Read(data);
		}

		public static string FormatTime(int totalSeconds, bool showSeconds)
		{
			int num = totalSeconds;
			int num2 = Mathf.FloorToInt(num / 3600);
			num -= num2 * 3600;
			int num3 = Mathf.FloorToInt(num / 60);
			string text = ((num2 <= 0) ? string.Empty : (num2 + ":")) + ((num3 >= 10 || num3 < 0 || num2 <= 0) ? string.Empty : "0") + num3;
			if (showSeconds)
			{
				text = text + ":" + ((totalSeconds % 60 >= 10) ? string.Empty : "0") + totalSeconds % 60;
			}
			return text;
		}

		public static string FormatNumber(float value, bool removeDecimal)
		{
			if (_culture == null)
			{
				_culture = new CultureInfo("en-US");
			}
			value = Mathf.Max(0f, value);
			string text = string.Format(_culture, "{0:N}", value);
			if (removeDecimal)
			{
				int num = text.IndexOf('.');
				if (num >= 0)
				{
					return text.Substring(0, num);
				}
			}
			return text;
		}

		public static string FormatDuration(int seconds, bool showSeconds)
		{
			string text = string.Empty;
			showSeconds = showSeconds || seconds < 60;
			if (seconds >= 86400)
			{
				int num = seconds / 86400;
				text = ((num <= 1) ? (text + "1 day ") : (text + string.Format("{0} days ", num)));
				seconds %= 86400;
			}
			if (seconds >= 3600)
			{
				int num2 = seconds / 3600;
				text = ((num2 <= 1) ? (text + "1 hour ") : (text + string.Format("{0} hours ", num2)));
				seconds %= 3600;
			}
			if (seconds >= 60)
			{
				int num3 = seconds / 60;
				text = ((num3 <= 1) ? (text + "1 minute ") : (text + string.Format("{0} minutes ", num3)));
				seconds %= 60;
			}
			if (seconds > 0 && showSeconds)
			{
				text = ((seconds <= 1) ? (text + "1 second") : (text + string.Format("{0} seconds ", seconds)));
			}
			return text.Trim();
		}
	}
}
