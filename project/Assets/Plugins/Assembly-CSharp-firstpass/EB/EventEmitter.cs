using System;
using System.Collections;

namespace EB
{
	public class EventEmitter : IDisposable
	{
		public struct Handle
		{
			public Delegate C;

			public EventEmitter P;

			public string N;

			public void Clear()
			{
				if (P != null)
				{
					P.Remove(N, C);
					P = null;
				}
			}
		}

		private Hashtable _events = new Hashtable();

		private ArrayList _tmp = new ArrayList(16);

		public virtual void Dispose()
		{
			lock (_events)
			{
				_events.Clear();
			}
		}

		private Handle Add(string name, Delegate cb)
		{
			lock (_events)
			{
				ArrayList arrayList = (ArrayList)_events[name];
				if (arrayList == null)
				{
					arrayList = new ArrayList();
					_events[name] = arrayList;
				}
				arrayList.Add(cb);
			}
			return new Handle
			{
				C = cb,
				P = this,
				N = name
			};
		}

		private void Call(string name, params object[] pars)
		{
			lock (_tmp)
			{
				_tmp.Clear();
				lock (_events)
				{
					ArrayList arrayList = (ArrayList)_events[name];
					if (arrayList != null)
					{
						foreach (object item in arrayList)
						{
							_tmp.Add(item);
						}
					}
					else
					{
						Debug.Log("EE: no handlers for " + name);
					}
				}
				foreach (Delegate item2 in _tmp)
				{
					item2.DynamicInvoke(pars);
				}
			}
		}

		public void Clear()
		{
			lock (_events)
			{
				_events.Clear();
			}
		}

		public void RemoveAll(string name)
		{
			lock (_events)
			{
				ArrayList arrayList = (ArrayList)_events[name];
				if (arrayList != null)
				{
					arrayList.Clear();
				}
			}
		}

		public void Remove(string name, Delegate cb)
		{
			lock (_events)
			{
				ArrayList arrayList = (ArrayList)_events[name];
				if (arrayList != null)
				{
					if (!arrayList.Contains(cb))
					{
						Debug.LogError("Warning callback not found for name: " + name);
					}
					arrayList.Remove(cb);
				}
			}
		}

		public void Remove(Handle handle)
		{
			Remove(handle.N, handle.C);
		}

		public Handle On(string name, Action callback)
		{
			return Add(name, callback);
		}

		public Handle On<Arg1>(string name, Action<Arg1> callback)
		{
			return Add(name, callback);
		}

		public Handle On<Arg1, Arg2>(string name, Action<Arg1, Arg2> callback)
		{
			return Add(name, callback);
		}

		public Handle On<Arg1, Arg2, Arg3>(string name, Action<Arg1, Arg2, Arg3> callback)
		{
			return Add(name, callback);
		}

		public Handle Once(string name, Action callback)
		{
			Handle h = default(Handle);
			h = Add(name, (Action)(() =>
			{
				h.Clear();
				callback();
			}));
			return h;
		}

		public Handle Once<Arg1>(string name, Action<Arg1> callback)
		{
			Handle h = default(Handle);
			h = Add(name, (Action<Arg1>)((Arg1 arg1) =>
			{
				h.Clear();
				callback(arg1);
			}));
			return h;
		}

		public Handle Once<Arg1, Arg2>(string name, Action<Arg1, Arg2> callback)
		{
			Handle h = default(Handle);
			h = Add(name, (Action<Arg1, Arg2>)((Arg1 arg1, Arg2 arg2) =>
			{
				h.Clear();
				callback(arg1, arg2);
			}));
			return h;
		}

		public Handle Once<Arg1, Arg2, Arg3>(string name, Action<Arg1, Arg2, Arg3> callback)
		{
			Handle h = default(Handle);
			h = Add(name, (Action<Arg1, Arg2, Arg3>)((Arg1 arg1, Arg2 arg2, Arg3 arg3) =>
			{
				h.Clear();
				callback(arg1, arg2, arg3);
			}));
			return h;
		}

		public void Emit(string name)
		{
			Call(name);
		}

		public void Emit<Arg1>(string name, Arg1 arg1)
		{
			Call(name, arg1);
		}

		public void Emit<Arg1, Arg2>(string name, Arg1 arg1, Arg2 arg2)
		{
			Call(name, arg1, arg2);
		}

		public void Emit<Arg1, Arg2, Arg3>(string name, Arg1 arg1, Arg2 arg2, Arg3 arg3)
		{
			Call(name, arg1, arg2, arg3);
		}
	}
}
