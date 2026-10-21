using System;
using System.Collections.Generic;

namespace EB
{
	public static class ProfanityFilter
	{
		private static List<string> _naughtyList = new List<string>();

		public static void Init(string words)
		{
			string[] array = words.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in array)
			{
				_naughtyList.Add(item);
			}
		}

		public static bool IsOk(string text)
		{
			foreach (string naughty in _naughtyList)
			{
				string value = naughty.Replace(" ", string.Empty);
				if (text.IndexOf(naughty, StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return false;
				}
			}
			return true;
		}

		public static string Filter(string text)
		{
			string text2 = text;
			foreach (string naughty in _naughtyList)
			{
				string text3 = new string('*', naughty.Length);
				int num = 0;
				while ((num = text2.IndexOf(naughty, StringComparison.OrdinalIgnoreCase)) >= 0)
				{
					text2 = text2.Substring(0, num) + text3 + text2.Substring(num + text3.Length);
				}
			}
			return text2;
		}
	}
}
