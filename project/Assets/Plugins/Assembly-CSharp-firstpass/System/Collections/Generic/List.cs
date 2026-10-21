using UnityEngine;

namespace System.Collections.Generic
{
	public static class List
	{
		public static void Shuffle<T>(this IList<T> list)
		{
			int num = list.Count;
			while (num > 1)
			{
				num--;
				int index = UnityEngine.Random.Range(0, num);
				T value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
		}
	}
}
