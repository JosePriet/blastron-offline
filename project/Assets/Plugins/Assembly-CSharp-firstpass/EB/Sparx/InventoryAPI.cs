using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public class InventoryAPI
	{
		private EndPoint _api;

		private int _next;

		private int _done;

		public InventoryAPI(EndPoint api)
		{
			_api = api;
		}

		public int Sync(Action<int, string, Hashtable> callback)
		{
			int next = _next;
			Coroutines.Run(_Sync(callback));
			return next;
		}

		public int Add(Hashtable items, Action<int, string, Hashtable> callback)
		{
			int next = _next;
			Coroutines.Run(_Call("add", items, -1, callback));
			return next;
		}

		public int Use(Hashtable items, Action<int, string, Hashtable> callback)
		{
			int next = _next;
			Coroutines.Run(_Call("use", items, -1, callback));
			return next;
		}

		public int Purchase(Hashtable items, int cost, Action<int, string, Hashtable> callback)
		{
			int next = _next;
			Coroutines.Run(_Call("purchase", items, cost, callback));
			return next;
		}

		public bool IsDone(int id)
		{
			return id < _done;
		}

		public Coroutine Wait(int id)
		{
			return Coroutines.Run(_Wait(id));
		}

		private IEnumerator _Wait(int id)
		{
			while (!IsDone(id))
			{
				yield return 1;
			}
		}

		private IEnumerator _Sync(Action<int, string, Hashtable> callback)
		{
			int id = _next++;
			while (id != _done)
			{
				yield return 1;
			}
			Request request = _api.Get("/inventory");
			Action<int, string, Hashtable> callback2 = default(Action<int, string, Hashtable>);
			_api.Service(request, (Response result) =>
			{
				_done++;
				if (result.sucessful)
				{
					callback2(result.id, null, result.hashtable);
				}
				else
				{
					callback2(result.id, result.localizedError, null);
				}
			});
		}

		private IEnumerator _Call(string type, Hashtable items, int cost, Action<int, string, Hashtable> callback)
		{
			int id = _next++;
			while (id != _done)
			{
				yield return 1;
			}
			string nonce = Nonce.Generate();
			Request request = _api.Post("/inventory/" + type);
			if (cost >= 0)
			{
				request.AddData("cost", cost);
			}
			if (items != null)
			{
				request.AddData("items", items);
			}
			request.AddData("nonce", nonce);
			Action<int, string, Hashtable> callback2 = default(Action<int, string, Hashtable>);
			_api.Service(request, (Response result) =>
			{
				_done++;
				if (result.sucessful)
				{
					callback2(id, null, result.hashtable);
				}
				else if (result.error != null && result.error.ToString() == "nsf")
				{
					callback2(id, "nsf", null);
				}
				else
				{
					callback2(id, result.localizedError, null);
				}
			});
		}
	}
}
