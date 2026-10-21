using System;
using System.Collections;

namespace EB
{
	public class Deferred : IDisposable
	{
		private struct Info
		{
			public Delegate Callback;

			public object[] Args;
		}

		private ArrayList _list;

		private ArrayList _copy;

		public Deferred(int capacity)
		{
			_list = new ArrayList(capacity);
			_copy = new ArrayList(capacity);
		}

		public void Defer(Delegate callback, params object[] args)
		{
			lock (_list)
			{
				_list.Add(new Info
				{
					Callback = callback,
					Args = args
				});
			}
		}

		public void Dispose()
		{
			lock (_list)
			{
				_list.Clear();
			}
		}

		public void Dispatch()
		{
			lock (_list)
			{
				if (_list.Count == 0)
				{
					return;
				}
				_copy.Clear();
				foreach (object item in _list)
				{
					_copy.Add(item);
				}
				_list.Clear();
			}
			foreach (Info item2 in _copy)
			{
				item2.Callback.DynamicInvoke(item2.Args);
			}
			_copy.Clear();
		}
	}
}
