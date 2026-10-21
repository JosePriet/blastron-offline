using System;
using System.Text;

namespace EB
{
	public static class Encoding
	{
		private static System.Text.Encoding _encoding = System.Text.Encoding.UTF8;

		public static byte[] GetBytes(string str)
		{
			return _encoding.GetBytes(str ?? string.Empty);
		}

		public static string GetString(byte[] bytes)
		{
			return _encoding.GetString(bytes);
		}

		public static string GetString(Buffer buffer)
		{
			ArraySegment<byte> arraySegment = buffer.ToArraySegment(true);
			return GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
		}

		public static string GetString(byte[] bytes, int offset, int count)
		{
			return _encoding.GetString(bytes, offset, count);
		}

		public static string ToBase64String(byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

		public static byte[] FromBase64String(string base64)
		{
			return Convert.FromBase64String(base64);
		}

		public static string ToBase64Url(byte[] data)
		{
			string text = Convert.ToBase64String(data);
			text = text.Replace("=", string.Empty);
			text = text.Replace("+", "-");
			return text.Replace("/", "_");
		}

		public static string ToBase64Url(string data)
		{
			return ToBase64Url(GetBytes(data));
		}

		public static byte[] BytesFromBase64Url(string base64Url)
		{
			string text = base64Url;
			text = text.Replace("-", "+");
			text = text.Replace("_", "/");
			while (text.Length % 4 != 0)
			{
				text += '=';
			}
			return Convert.FromBase64String(text);
		}

		public static string StringFromBase64Url(string base64Url)
		{
			return GetString(BytesFromBase64Url(base64Url));
		}

		public static byte[] FromHexString(string str)
		{
			str = str.Replace(" ", string.Empty);
			if ((str.Length & 1) != 0)
			{
				throw new Exception("str must be even length");
			}
			int num = str.Length / 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
			}
			return array;
		}

		public static string ToHexString(byte[] bytes, int spacing = 0)
		{
			return ToHexString(bytes, 0, bytes.Length, spacing);
		}

		public static string ToHexString(byte[] bytes, int offset, int count, int spacing = 0)
		{
			StringBuilder stringBuilder = new StringBuilder(count * 2);
			string text = string.Empty;
			for (int i = 0; i < spacing; i++)
			{
				text += " ";
			}
			for (int j = 0; j < count; j++)
			{
				stringBuilder.AppendFormat("{0:X2}", bytes[offset + j]);
				stringBuilder.Append(text);
			}
			return stringBuilder.ToString();
		}
	}
}
