using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EB
{
	public static class ArrayUtils
	{
		public static string Join(object list, char separator)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			IEnumerator enumerator = AOT.GetEnumerator(list);
			if (enumerator != null)
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					if (current != null)
					{
						if (flag)
						{
							stringBuilder.Append(separator);
						}
						stringBuilder.Append(current.ToString());
						flag = true;
					}
				}
			}
			return stringBuilder.ToString();
		}

		public static List<T> Map<O, T>(List<O> list, Function<T, O> pred) where T : class
		{
			List<T> list2 = new List<T>(list.Count);
			foreach (O item in list)
			{
				T val = pred(item);
				if (val != null)
				{
					list2.Add(val);
				}
			}
			return list2;
		}

		public static List<T> ToList<T>(object array)
		{
			List<T> list = new List<T>();
			IEnumerator enumerator = AOT.GetEnumerator(array);
			if (enumerator != null)
			{
				while (enumerator.MoveNext())
				{
					list.Add((T)enumerator.Current);
				}
			}
			return list;
		}
	}
}
