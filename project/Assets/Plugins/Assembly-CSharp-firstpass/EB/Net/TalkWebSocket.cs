using System;
using System.Collections;
using System.IO;

namespace EB.Net
{
	public class TalkWebSocket : WebSocket
	{
		private const int TYPE_CALL = 0;

		private const int TYPE_RESULT = 1;

		private int _nextId;

		private Hashtable _callbacks = new Hashtable();

		public event Action<string, ArrayList, Action<string, object>> OnRPC;

		public TalkWebSocket()
		{
			_nextId = 1;
			base.OnMessage += MessageHandler;
		}

		public override void Dispose()
		{
			base.Dispose();
			_callbacks.Clear();
		}

		public void RPC(string name, ArrayList args, Action<string, object> callback)
		{
			int num = 0;
			lock (_callbacks)
			{
				num = _nextId++;
				_callbacks[num] = callback;
			}
			SendMsg(new object[4] { 0, num, name, args });
		}

		protected override void Error(string error)
		{
			Hashtable callbacks;
			lock (_callbacks)
			{
				callbacks = _callbacks;
				_callbacks = new Hashtable();
			}
			foreach (DictionaryEntry item in callbacks)
			{
				Debug.Log("Doing Error Callbacks");
				Action<string, object> action = (Action<string, object>)item.Value;
				action(error, null);
			}
			base.Error(error);
		}

		private void MessageHandler(string message)
		{
			try
			{
				object obj = JSON.Parse(message);
				if (!(obj is ArrayList))
				{
					return;
				}
				ArrayList arrayList = obj as ArrayList;
				if (arrayList.Count != 4)
				{
					throw new IOException("Invalid message: " + message);
				}
				Debug.Log("<-N " + message);
				int num = (int)(double)arrayList[0];
				int id = (int)(double)arrayList[1];
				switch (num)
				{
				case 0:
				{
					string obj6 = arrayList[2] as string;
					ArrayList obj7 = arrayList[3] as ArrayList;
					Action<string, object> action = (string error, object result) =>
					{
						SendMsg(new object[4] { 1, id, error, result });
					};
					if (OnRPC != null)
					{
						OnRPC(obj6, obj7, action);
					}
					else
					{
						action("no handler for rpc", null);
					}
					break;
				}
				case 1:
				{
					object obj2 = arrayList[2];
					string obj3 = ((obj2 == null) ? null : obj2.ToString());
					object obj4 = arrayList[3];
					object obj5 = null;
					lock (_callbacks)
					{
						obj5 = _callbacks[id];
						_callbacks.Remove(id);
					}
					if (obj5 != null)
					{
						((Action<string, object>)obj5)(obj3, obj4);
					}
					else
					{
						Debug.LogError("Callback missing for id " + id);
					}
					break;
				}
				default:
					throw new IOException("Unknown message type " + num);
				}
			}
			catch (Exception ex)
			{
				Error("wstalk exception: " + ex);
			}
		}

		private void SendMsg(object data)
		{
			string text = JSON.Stringify(data);
			Debug.Log("->N " + text);
			SendUTF8(text);
		}

		protected override void DidClose(Impl impl)
		{
			base.DidClose(impl);
			ArrayList arrayList = new ArrayList();
			lock (_callbacks)
			{
				foreach (object value in _callbacks.Values)
				{
					arrayList.Add(value);
				}
				_callbacks.Clear();
			}
			foreach (object item in arrayList)
			{
				Action<string, object> action = (Action<string, object>)item;
				if (action != null)
				{
					action("Lost connection", null);
				}
			}
		}
	}
}
