using System.Collections;
using System.Collections.Generic;

namespace EB.Sparx
{
	public class DataManager : Manager
	{
		public const string kOfflineDataKey = "offline-data";

		private DataAPI _api;

		private Dictionary<Id, Data> _data;

		public override void Initialize(Config config)
		{
			_api = new DataAPI(base.Hub.ApiEndPoint);
			_data = new Dictionary<Id, Data>();
		}

		public override IEnumerator LoadOfflineData()
		{
			_data.Clear();
			Data data = GetData(Id.Offline);
			data.Load("offline-data");
			Debug.Log("OfflineData:\n" + JSON.Stringify(data.fragments));
			yield break;
		}

		public void ClearOfflineData()
		{
			SecurePrefs.DeleteKey("offline-data");
			_data.Clear();
		}

		public Data GetData(Id userId)
		{
			Data value;
			if (!_data.TryGetValue(userId, out value))
			{
				value = new Data(userId);
				_data.Add(userId, value);
			}
			return value;
		}

		public void Load(Id userId, string key, Action<string, Data> callback)
		{
			if (userId == Id.Offline)
			{
				callback(null, GetData(userId));
				return;
			}
			Data data = GetData(userId);
			if (userId == base.Hub.LoginManager.LocalUserId)
			{
				Hashtable hashtable = Dot.Object("ds", base.Hub.LoginManager.LoginData, null);
				if (hashtable != null)
				{
					data.StoreResult(hashtable, true);
					callback(null, data);
					return;
				}
			}
			_api.Load(userId, key, (string error, Hashtable result) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					callback(error, data);
				}
				else
				{
					bool full = string.IsNullOrEmpty(key);
					data.StoreResult(result, full);
					callback(null, data);
				}
			});
		}

		public void Save(Id userId, Hashtable obj, Action<string> callback)
		{
			Data data = GetData(userId);
			Hashtable hashtable = data.Delta(obj);
			if (hashtable.Count == 0)
			{
				Debug.Log("Nothing to save");
				callback(null);
				return;
			}
			data.StoreResult(obj, false);
			data.Save("offline-data");
			if (base.Hub.State != HubState.Idle && userId != Id.Offline)
			{
				_api.Save(userId, hashtable, callback);
			}
			else
			{
				callback(string.Empty);
			}
		}
	}
}
