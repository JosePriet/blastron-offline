using System.Collections;

namespace EB.Sparx
{
	public class DataAPI
	{
		private EndPoint _api;

		public DataAPI(EndPoint api)
		{
			_api = api;
		}

		public void Load(Id userId, string key, Action<string, Hashtable> callback)
		{
			Request request = _api.Get(string.Format("/ds/{0}/{1}", userId, key));
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

		public void Save(Id userId, Hashtable data, Action<string> callback)
		{
			Request request = _api.Post(string.Format("/ds/{0}", userId));
			request.AddData("data", data);
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
