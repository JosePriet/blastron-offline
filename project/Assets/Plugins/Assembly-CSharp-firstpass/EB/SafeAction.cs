using UnityEngine;

namespace EB
{
	public static class SafeAction
	{
		public static Action Wrap(MonoBehaviour b, Action cb)
		{
			return () =>
			{
				if (b != null)
				{
					cb();
				}
			};
		}

		public static Action<T> Wrap<T>(MonoBehaviour b, Action<T> cb)
		{
			return (T obj) =>
			{
				if (b != null)
				{
					cb(obj);
				}
			};
		}

		public static Action<T, U> Wrap<T, U>(MonoBehaviour b, Action<T, U> cb)
		{
			return (T obj1, U obj2) =>
			{
				if (b != null)
				{
					cb(obj1, obj2);
				}
			};
		}
	}
}
