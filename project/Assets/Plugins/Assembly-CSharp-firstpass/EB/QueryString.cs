using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EB
{
	public static class QueryString
	{
		public static Hashtable Parse(string query)
		{
			Hashtable hashtable = new Hashtable();
			int num = query.IndexOf('?');
			if (num >= 0)
			{
				query = query.Substring(num + 1);
			}
			string[] array = query.Split(new char[1] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				string[] array2 = text.Split('=');
				string key = WWW.UnEscapeURL(array2[0]);
				if (array2.Length == 1)
				{
					hashtable[key] = string.Empty;
				}
				else if (array2.Length == 2)
				{
					hashtable[key] = WWW.UnEscapeURL(array2[1]);
				}
			}
			return hashtable;
		}

		public static string Stringify(Hashtable data)
		{
			StringBuilder stringBuilder = new StringBuilder(2048);
			bool flag = true;
			foreach (DictionaryEntry datum in data)
			{
				string value = WWW.EscapeURL(datum.Key.ToString()) + '=' + WWW.EscapeURL(datum.Value.ToString());
				if (!flag)
				{
					stringBuilder.Append('&');
				}
				flag = false;
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		public static string StringifySorted(Hashtable data)
		{
			StringBuilder stringBuilder = new StringBuilder(2048);
			bool flag = true;
			List<string> list = new List<string>(data.Keys.Count);
			foreach (object key in data.Keys)
			{
				list.Add(key.ToString());
			}
			list.Sort();
			foreach (string item in list)
			{
				string s = data[item].ToString();
				string value = WWW.EscapeURL(item) + '=' + WWW.EscapeURL(s);
				if (!flag)
				{
					stringBuilder.Append('&');
				}
				flag = false;
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}
	}
}
