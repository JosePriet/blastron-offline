using System.Collections;

namespace EB.Sparx
{
	public class PushAPI
	{
		private EndPoint _api;

		public PushAPI(EndPoint api)
		{
			_api = api;
		}

		public void GetPushToken(Action<string, Hashtable> callback)
		{
			Request request = _api.Get("/push/token");
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

		public void SetApplePushToken(string token, Action<string> callback)
		{
			Request request = _api.Post("/push/apple");
			request.AddData("token", token);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null);
				}
				else
				{
					callback(result.localizedError);
				}
			});
		}
	}
}
