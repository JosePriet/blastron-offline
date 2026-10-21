using System;
using System.Collections;
using System.Collections.Generic;

namespace EB
{
	public class LocFile
	{
		private Dictionary<string, string> _strings;

		private Dictionary<string, LocStatus> _status;

		public LocFile()
		{
			_strings = new Dictionary<string, string>();
			_status = new Dictionary<string, LocStatus>();
		}

		public LocFile(Dictionary<string, string> values)
		{
			_strings = values;
			_status = new Dictionary<string, LocStatus>();
		}

		public LocFile(Dictionary<string, string> values, Dictionary<string, LocStatus> status)
		{
			_strings = values;
			_status = status;
		}

		public string NextId(string prefix)
		{
			int num = 1;
			string text;
			while (true)
			{
				text = string.Format("ID_{0}_{1}", prefix, num).ToUpper();
				if (!_strings.ContainsKey(text))
				{
					break;
				}
				num++;
			}
			return text;
		}

		public void Add(string id, string value)
		{
			_strings[id] = value;
		}

		public LocStatus GetStatus(string id)
		{
			LocStatus value;
			if (_status.TryGetValue(id, out value))
			{
				return value;
			}
			return LocStatus.Missing;
		}

		public bool Get(string id, out string result)
		{
			if (!_strings.TryGetValue(id, out result))
			{
				result = string.Empty;
				return false;
			}
			return true;
		}

		public Hashtable Read(string data)
		{
			Hashtable hashtable = new Hashtable();
			data = data.Replace("\r\n", "\n");
			string[] array = data.Split('\n');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Length > 0 && text[0] == '#')
				{
					continue;
				}
				int num = text.IndexOf(',');
				if (num <= 0)
				{
					continue;
				}
				string text2 = text.Substring(0, num);
				string text3 = text.Substring(num + 1);
				if (text2.StartsWith("ID_"))
				{
					string[] array3 = text2.Split('|');
					text2 = array3[0];
					if (array3.Length > 1)
					{
						_status[text2] = (LocStatus)(int)Enum.Parse(typeof(LocStatus), array3[1], true);
					}
					else
					{
						_status[text2] = LocStatus.Source;
					}
					text3 = text3.Replace("\\n", "\n").Replace("\\t", "\t");
					Add(text2, text3);
					hashtable[text2] = text3;
				}
			}
			return hashtable;
		}
	}
}
