using System;

namespace EB
{
	public static class Crypto
	{
		public static byte[] CreateKey(string str)
		{
			return Encoding.GetBytes(str);
		}

		public static byte[] RandomBytes(int length)
		{
			Random random = new Random(Time.Now);
			byte[] array = new byte[length];
			random.NextBytes(array);
			return array;
		}
	}
}
