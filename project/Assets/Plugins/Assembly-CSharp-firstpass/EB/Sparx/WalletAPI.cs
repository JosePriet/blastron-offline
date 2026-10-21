using System.Collections;

namespace EB.Sparx
{
	public class WalletAPI
	{
		private EndPoint _api;

		private int _next;

		private int _done;

		public WalletAPI(EndPoint api)
		{
			_api = api;
		}

		public void Fetch(Action<string, Hashtable> callback)
		{
			Coroutines.Run(_Fetch(callback));
		}

		public int Credit(int value, string reason, Action<int, string, Hashtable> callback)
		{
			int next = _next;
			Coroutines.Run(_Call("credit", value, reason, callback));
			return next;
		}

		public int Debit(int value, string reason, Action<int, string, Hashtable> callback)
		{
			int next = _next;
			Coroutines.Run(_Call("debit", value, reason, callback));
			return next;
		}

		public void FetchPayouts(Action<string, Hashtable> callback)
		{
			Request request = _api.Get("/store/payouts");
			request.AddData("platform", Device.MobilePlatform);
			request.AddData("version", Version.GetVersion());
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					callback(result.localizedError, null);
				}
			});
		}

		public void FetchOfferUrl(string offerName, Action<string, string> callback)
		{
			Request request = _api.Get("/store/offerurl");
			request.AddData("offer", offerName);
			request.AddData("platform", Device.MobilePlatform);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.str);
				}
				else
				{
					callback(result.localizedError, null);
				}
			});
		}

		public void FetchOffers(Action<string, Hashtable> callback)
		{
			Request request = _api.Get("/store/offers");
			request.AddData("platform", Device.MobilePlatform);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					callback(result.localizedError, null);
				}
			});
		}

		public void VerifyPayout(Hashtable data, Action<string, Hashtable> callback)
		{
			Request request = _api.Post("/store/verify-payout");
			request.AddData("data", data);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					callback(result.localizedError, null);
				}
			});
		}

		public void Verify(string receipt, Action<string, Hashtable> callback)
		{
			Request request = _api.Post("/store/verify/" + Device.MobilePlatform);
			request.AddData("receipt", receipt);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					callback(result.localizedError, null);
				}
			});
		}

		private IEnumerator _Fetch(Action<string, Hashtable> callback)
		{
			int id = _next++;
			while (id != _done)
			{
				yield return 1;
			}
			Request request = _api.Get("/wallet/balance");
			Action<string, Hashtable> callback2 = default(Action<string, Hashtable>);
			_api.Service(request, (Response result) =>
			{
				_done++;
				if (result.sucessful)
				{
					callback2(null, result.hashtable);
				}
				else if (result.error != null && result.error.ToString() == "nsf")
				{
					callback2("nsf", null);
				}
				else
				{
					callback2(result.localizedError, null);
				}
			});
		}

		private IEnumerator _Call(string type, int value, string reason, Action<int, string, Hashtable> callback)
		{
			int id = _next++;
			while (id != _done)
			{
				yield return 1;
			}
			string nonce = Nonce.Generate();
			Request request = _api.Post("/wallet/" + type);
			if (value > 0)
			{
				request.AddData("value", value);
			}
			if (!string.IsNullOrEmpty(reason))
			{
				request.AddData("reason", reason);
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
