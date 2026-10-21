using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EB
{
	public static class JSON
	{
		public class Fragment
		{
			public string JsonString { get; private set; }

			public Fragment(string json)
			{
				JsonString = json;
			}
		}

		private enum Token
		{
			None = 0,
			CurlyOpen = 1,
			CurlyClose = 2,
			SquaredOpen = 3,
			SquaredClose = 4,
			Colon = 5,
			Comma = 6,
			String = 7,
			Number = 8,
			True = 9,
			False = 10,
			Null = 11
		}

		private const int BUILDER_CAPACITY = 2048;

		public static object Parse(string json)
		{
			if (json != null)
			{
				char[] json2 = json.ToCharArray();
				int index = 0;
				bool success = true;
				object obj = ParseValue(json2, ref index, ref success);
				return (!success) ? null : obj;
			}
			return null;
		}

		public static string Stringify(object json)
		{
			StringBuilder stringBuilder = new StringBuilder(2048);
			return (!SerializeValue(json, stringBuilder)) ? null : stringBuilder.ToString();
		}

		private static Hashtable ParseObject(char[] json, ref int index)
		{
			Hashtable hashtable = new Hashtable();
			NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				Token token = LookAhead(json, index);
				if (token == Token.None)
				{
					return null;
				}
				switch (token)
				{
				case Token.None:
					return null;
				case Token.Comma:
					NextToken(json, ref index);
					continue;
				case Token.CurlyClose:
					NextToken(json, ref index);
					return hashtable;
				}
				string text = ParseString(json, ref index);
				if (text == null)
				{
					return null;
				}
				token = NextToken(json, ref index);
				if (token != Token.Colon)
				{
					return null;
				}
				bool success = true;
				object value = ParseValue(json, ref index, ref success);
				if (!success)
				{
					return null;
				}
				hashtable[text] = value;
			}
			return hashtable;
		}

		private static ArrayList ParseArray(char[] json, ref int index)
		{
			ArrayList arrayList = new ArrayList();
			NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				switch (LookAhead(json, index))
				{
				case Token.None:
					return null;
				case Token.Comma:
					NextToken(json, ref index);
					continue;
				case Token.SquaredClose:
					break;
				default:
				{
					bool success = true;
					object value = ParseValue(json, ref index, ref success);
					if (!success)
					{
						return null;
					}
					arrayList.Add(value);
					continue;
				}
				}
				NextToken(json, ref index);
				break;
			}
			return arrayList;
		}

		private static object ParseValue(char[] json, ref int index, ref bool success)
		{
			switch (LookAhead(json, index))
			{
			case Token.String:
				return ParseString(json, ref index);
			case Token.Number:
				return ParseNumber(json, ref index);
			case Token.CurlyOpen:
				return ParseObject(json, ref index);
			case Token.SquaredOpen:
				return ParseArray(json, ref index);
			case Token.True:
				NextToken(json, ref index);
				return true;
			case Token.False:
				NextToken(json, ref index);
				return false;
			case Token.Null:
				NextToken(json, ref index);
				return null;
			default:
				success = false;
				return null;
			}
		}

		private static string ParseString(char[] json, ref int index)
		{
			EatWhitespace(json, ref index);
			char c = json[index];
			index++;
			List<char> list = new List<char>();
			int num = 0;
			bool flag = false;
			while (!flag && index != json.Length)
			{
				c = json[index];
				index++;
				switch (c)
				{
				case '"':
					flag = true;
					break;
				case '\\':
				{
					if (index == json.Length)
					{
						break;
					}
					c = json[index];
					index++;
					switch (c)
					{
					case '"':
						list.Add('"');
						continue;
					case '\\':
						list.Add('\\');
						continue;
					case '/':
						list.Add('/');
						continue;
					case 'b':
						list.Add('\b');
						continue;
					case 'f':
						list.Add('\f');
						continue;
					case 'n':
						list.Add('\n');
						continue;
					case 'r':
						list.Add('\r');
						continue;
					case 't':
						list.Add('\t');
						continue;
					case 'u':
						break;
					default:
						continue;
					}
					int num2 = json.Length - index;
					if (num2 < 4)
					{
						break;
					}
					char[] array = new char[4];
					Array.Copy(json, index, array, 0, 4);
					int num3 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						num3 *= 16;
						char c2 = array[i];
						if (c2 >= 'A' && c2 <= 'F')
						{
							num3 += 10 + (c2 - 65);
						}
						else if (c2 >= 'a' && c2 <= 'f')
						{
							num3 += 10 + (c2 - 97);
						}
						else if (c2 >= '0' && c2 <= '9')
						{
							num3 += c2 - 48;
						}
					}
					if (num3 < 55295 || num3 >= 57344)
					{
						list.Add((char)num3);
						num = 0;
					}
					else if (num == 0 || !char.IsLowSurrogate((char)num3))
					{
						num = (char.IsHighSurrogate((char)num3) ? num3 : 0);
					}
					else
					{
						list.Add((char)num);
						list.Add((char)num3);
						num = 0;
					}
					index += 4;
					continue;
				}
				default:
					list.Add(c);
					continue;
				}
				break;
			}
			if (!flag)
			{
				return null;
			}
			return new string(list.ToArray());
		}

		private static double ParseNumber(char[] json, ref int index)
		{
			EatWhitespace(json, ref index);
			int lastIndexOfNumber = GetLastIndexOfNumber(json, index);
			int num = index;
			index = lastIndexOfNumber + 1;
			string s = new string(json, num, lastIndexOfNumber - num + 1);
			return double.Parse(s);
		}

		private static int GetLastIndexOfNumber(char[] json, int index)
		{
			int i;
			for (i = index; i < json.Length && "0123456789+-.eE".IndexOf(json[i]) != -1; i++)
			{
			}
			return i - 1;
		}

		private static void EatWhitespace(char[] json, ref int index)
		{
			while (index < json.Length && " \t\n\r".IndexOf(json[index]) != -1)
			{
				index++;
			}
		}

		private static Token LookAhead(char[] json, int index)
		{
			int index2 = index;
			return NextToken(json, ref index2);
		}

		private static Token NextToken(char[] json, ref int index)
		{
			EatWhitespace(json, ref index);
			if (index == json.Length)
			{
				return Token.None;
			}
			char c = json[index];
			index++;
			switch (c)
			{
			case '{':
				return Token.CurlyOpen;
			case '}':
				return Token.CurlyClose;
			case '[':
				return Token.SquaredOpen;
			case ']':
				return Token.SquaredClose;
			case ',':
				return Token.Comma;
			case '"':
				return Token.String;
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return Token.Number;
			case ':':
				return Token.Colon;
			default:
			{
				index--;
				int num = json.Length - index;
				if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
				{
					index += 5;
					return Token.False;
				}
				if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
				{
					index += 4;
					return Token.True;
				}
				if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
				{
					index += 4;
					return Token.Null;
				}
				return Token.None;
			}
			}
		}

		private static bool SerializeObject(object obj, StringBuilder builder)
		{
			IDictionaryEnumerator dictionaryEnumerator = AOT.GetDictionaryEnumerator(obj);
			if (dictionaryEnumerator == null)
			{
				Debug.LogError("Object does not implement IDictionaryEnumerator interface: " + obj.GetType());
				return false;
			}
			builder.Append("{");
			bool flag = true;
			bool result = true;
			while (dictionaryEnumerator.MoveNext())
			{
				string aString = dictionaryEnumerator.Key.ToString();
				object value = dictionaryEnumerator.Value;
				if (!flag)
				{
					builder.Append(",");
				}
				SerializeString(aString, builder);
				builder.Append(":");
				if (!SerializeValue(value, builder))
				{
					builder.Append("null");
					result = false;
				}
				flag = false;
			}
			if (dictionaryEnumerator is IDisposable)
			{
				IDisposable disposable = (IDisposable)dictionaryEnumerator;
				disposable.Dispose();
			}
			builder.Append("}");
			return result;
		}

		private static bool SerializeArray(object arr, StringBuilder builder)
		{
			IEnumerator enumerator = AOT.GetEnumerator(arr);
			if (enumerator == null)
			{
				Debug.LogError("Object does not implement IEnumerable interface: " + arr.GetType());
				return false;
			}
			builder.Append("[");
			bool result = true;
			bool flag = true;
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				if (!flag)
				{
					builder.Append(",");
				}
				if (!SerializeValue(current, builder))
				{
					builder.Append("null");
					result = false;
				}
				flag = false;
			}
			if (enumerator is IDisposable)
			{
				IDisposable disposable = (IDisposable)enumerator;
				disposable.Dispose();
			}
			builder.Append("]");
			return result;
		}

		private static bool SerializeValue(object value, StringBuilder builder)
		{
			if (value == null)
			{
				builder.Append("null");
				return true;
			}
			if (value is Fragment)
			{
				Fragment fragment = (Fragment)value;
				builder.Append(fragment.JsonString);
				return true;
			}
			if (value is string)
			{
				SerializeString((string)value, builder);
				return true;
			}
			if (value is IDictionary)
			{
				return SerializeObject(value, builder);
			}
			if (value is ICollection)
			{
				return SerializeArray(value, builder);
			}
			if (value is Enum)
			{
				object obj = Convert.ChangeType(value, typeof(int));
				SerializeNumber(obj.ToString(), builder);
				return true;
			}
			if (value is bool)
			{
				bool flag = (bool)value;
				builder.Append((!flag) ? "false" : "true");
				return true;
			}
			if (IsNumeric(value))
			{
				SerializeNumber(value.ToString(), builder);
				return true;
			}
			Debug.LogError("Failed to serialize type: " + value.GetType());
			return false;
		}

		private static void SerializeString(string aString, StringBuilder builder)
		{
			builder.Append("\"");
			char[] array = aString.ToCharArray();
			foreach (char c in array)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					continue;
				case '\\':
					builder.Append("\\\\");
					continue;
				case '\b':
					builder.Append("\\b");
					continue;
				case '\f':
					builder.Append("\\f");
					continue;
				case '\n':
					builder.Append("\\n");
					continue;
				case '\r':
					builder.Append("\\r");
					continue;
				case '\t':
					builder.Append("\\t");
					continue;
				}
				int num = c;
				if (num >= 32 && num <= 126)
				{
					builder.Append(c);
				}
				else
				{
					builder.AppendFormat("\\u{0:X4}", num);
				}
			}
			builder.Append("\"");
		}

		private static void SerializeNumber(string number, StringBuilder builder)
		{
			builder.Append(number);
		}

		private static bool IsNumeric(object o)
		{
			double result;
			if (o != null)
			{
				return double.TryParse(o.ToString(), out result);
			}
			return false;
		}
	}
}
