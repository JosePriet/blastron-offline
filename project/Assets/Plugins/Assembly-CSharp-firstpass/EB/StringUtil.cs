using System;

namespace EB
{
	public static class StringUtil
	{
		private static char[] valid = "abcdefghijklmnopqrstuvwxyz0123456789/_".ToCharArray();

		public static string Possesive(string value)
		{
			if (value.EndsWith("s") || value.EndsWith("S"))
			{
				return value + "'";
			}
			return value + "'s";
		}

		public static string SafeKey(string src)
		{
			src = src.ToLower();
			int num = 0;
			while (num < src.Length)
			{
				if (Array.IndexOf(valid, src[num]) < 0)
				{
					src = src.Remove(num, 1);
				}
				else
				{
					num++;
				}
			}
			return src;
		}
	}
}
