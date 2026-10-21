using System.Collections;

namespace EB.Sparx
{
	public class GachaAPI
	{
		private readonly int GachaAPIVersion = 3;

		private EndPoint _api;

		public GachaAPI(EndPoint api)
		{
			_api = api;
		}

		public void FetchSet(string group, Action<string, string, Hashtable> callback)
		{
			Request request = _api.Get("/gacha/getSet");
			request.numRetries = 3;
			request.AddData("group", group);
			request.AddData("api", GachaAPIVersion);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(group, null, result.hashtable);
				}
				else
				{
					callback(group, result.localizedError, null);
				}
			});
		}

		public void PickFromBox(string group, string version, string set, string box, string payment, Action<string, Hashtable> callback)
		{
			Request request = _api.Post("/gacha/pick");
			request.AddData("group", group);
			request.AddData("api", GachaAPIVersion);
			request.AddData("version", version);
			request.AddData("set", set);
			request.AddData("box", box);
			request.AddData("payment", payment);
			request.AddData("nonce", Nonce.Generate());
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					Debug.Log("PickFromBox Error: {0}", result.localizedError);
					callback(result.localizedError, null);
				}
			});
		}

		public void SyncTokens(Action<string, ArrayList> callback)
		{
			Request request = _api.Get("/gacha/getTokens");
			request.AddData("api", GachaAPIVersion);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.arrayList);
				}
				else
				{
					Debug.Log("SyncTokens Error: {0}", result.localizedError);
					callback(result.localizedError, null);
				}
			});
		}

		public void GetTimestamp(Action<string, Hashtable> callback)
		{
			Request request = _api.Get("/gacha/getTimestamp");
			request.AddData("api", GachaAPIVersion);
			request.numRetries = 3;
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					Debug.Log("Timestamp Error: {0}", result.localizedError);
					callback(result.localizedError, null);
				}
			});
		}

		public void AddToken(string token, Action<string, int> callback)
		{
			Request request = _api.Post("/gacha/addtoken");
			request.AddData("api", GachaAPIVersion);
			request.AddData("token", token);
			request.AddData("nonce", Nonce.Generate());
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, (int)result.number);
				}
				else
				{
					Debug.Log("PickFromBox Error: {0}", result.localizedError);
					callback(result.localizedError, -1);
				}
			});
		}
	}
}
