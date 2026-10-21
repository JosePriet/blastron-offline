using System;
using System.Collections;
using EB.Collections;

namespace EB
{
	public static class Debug
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
				if (arg != null)
				{
					empty = arg.ToString();
					if (arg is ICollection)
					{
						empty = JSON.Stringify(arg) ?? empty;
					}
				}
				else
				{
					empty = "null";
				}
				return empty;
			}
		}

		private static CircularBuffer _buffer = new CircularBuffer(256);

		private static FormatProvider _provider = new FormatProvider();

		public static string Format(object message, params object[] args)
		{
			if (args.Length == 0)
			{
				return message.ToString();
			}
			return string.Format(_provider, message.ToString(), args);
		}

		public static void LogIf(bool condition, object message, params object[] args)
		{
			if (condition)
			{
				Log(message, args);
			}
		}

		public static void Log(object message, params object[] args)
		{
			lock (_buffer)
			{
				_buffer.Push(DateTime.UtcNow.ToString() + " I:" + Format(message, args));
			}
		}

		public static void LogWarning(object message, params object[] args)
		{
			lock (_buffer)
			{
				_buffer.Push(DateTime.UtcNow.ToString() + " W:" + Format(message, args));
			}
		}

		public static void LogError(object message, params object[] args)
		{
			lock (_buffer)
			{
				_buffer.Push(DateTime.UtcNow.ToString() + " E:" + Format(message, args));
			}
		}

		public static void Dump(Hashtable table)
		{
			ArrayList arrayList = new ArrayList();
			lock (_buffer)
			{
				foreach (string item in _buffer)
				{
					arrayList.Add(item);
				}
			}
			table["log"] = arrayList;
		}
	}
}
