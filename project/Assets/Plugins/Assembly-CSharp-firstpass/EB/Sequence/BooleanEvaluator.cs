using System;
using System.Collections.Generic;

namespace EB.Sequence
{
	public class BooleanEvaluator
	{
		public static bool Evaluate(string expression)
		{
			string text = expression.ToUpper().Replace("FALSE", "0").Replace("TRUE", "1");
			text = text.Replace("AND", "&&").Replace("OR", "||").Replace(" ", string.Empty);
			text = text.Replace("!0", "1");
			text = text.Replace("!1", "0");
			Dictionary<string, string> rules = GetRules();
			while (text.Length > 1)
			{
				bool flag = false;
				foreach (KeyValuePair<string, string> item in rules)
				{
					if (text.Contains(item.Key))
					{
						text = text.Replace(item.Key, item.Value);
						flag = true;
					}
					if (flag)
					{
						text = text.Replace("!(0)", "(1)");
						text = text.Replace("!(1)", "(0)");
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (text == "0")
			{
				return false;
			}
			if (text == "1")
			{
				return true;
			}
			throw new Exception("Couldn't evaluate expression: " + expression);
		}

		private static Dictionary<string, string> GetRules()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("(0)", "0");
			dictionary.Add("(1)", "1");
			dictionary.Add("0&&0", "0");
			dictionary.Add("0&&1", "0");
			dictionary.Add("1&&0", "0");
			dictionary.Add("1&&1", "1");
			dictionary.Add("0||0", "0");
			dictionary.Add("0||1", "1");
			dictionary.Add("1||0", "1");
			dictionary.Add("1||1", "1");
			return dictionary;
		}
	}
}
