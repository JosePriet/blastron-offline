using System.Collections;
using System.Collections.Generic;

namespace EB.Sparx
{
	public class GachaManager : SubSystem, Updatable
	{
		private GachaConfig _config = new GachaConfig();

		private GachaAPI _api;

		private Dictionary<string, GachaSet> _sets = new Dictionary<string, GachaSet>();

		private bool _initialTokensComplete;

		private Dictionary<string, int> _tokens = new Dictionary<string, int>();

		public IEnumerable<string> SetNames
		{
			get
			{
				return _sets.Keys;
			}
		}

		public bool UpdateOffline
		{
			get
			{
				return false;
			}
		}

		public int GetTokenCount(string box)
		{
			int value = 0;
			if (!_tokens.TryGetValue(box, out value))
			{
				value = 0;
			}
			return value;
		}

		public GachaSet GetGachaSet(string name)
		{
			GachaSet value = null;
			if (!_sets.TryGetValue(name, out value))
			{
				value = null;
			}
			return value;
		}

		public void PickFromBox(string group, string setName, string box, string payment, Action<string, GachaPickResult> onComplete)
		{
			GachaSet set = null;
			if (_sets.TryGetValue(group, out set))
			{
				if (set.Name != setName)
				{
					set = null;
				}
			}
			else
			{
				set = null;
			}
			Debug.Log("SparxGachaManager > PickFromBox > Initialize: " + Hub.Instance.GachaManager.ToString());
			if (set != null)
			{
				_api.PickFromBox(group, set.Version, set.Name, box, payment, (string error, Hashtable data) =>
				{
					if (data != null)
					{
						GachaPickResult obj = new GachaPickResult(data);
						Debug.Log("SparxGachaManager > PickFromBox > Server Responsed for box " + box + " : " + JSON.Stringify(data));
						if (payment == "token")
						{
							int num = Dot.Integer("balance", data, -1);
							if (num != -1)
							{
								GachaBox gachaBox = set.Boxes.Find((GachaBox b) => b.Name == box);
								_tokens[gachaBox.Token] = num;
							}
						}
						onComplete(null, obj);
					}
					else
					{
						Debug.LogError("Server responded with error to Gacha Pick: '{0}'", error);
						onComplete(error, null);
					}
					Debug.Log("SparxGachaManager > PickFromBox > OnCompleted: " + Hub.Instance.GachaManager.ToString());
				});
			}
			else
			{
				string text = string.Format("Requested a pick on an unknown set: {0}", setName);
				Debug.LogError(text);
				onComplete(text, null);
			}
		}

		public void AddTokenForBox(string group, string boxName, Action<string, int> onComplete)
		{
			GachaSet value = null;
			if (!_sets.TryGetValue(group, out value))
			{
				value = null;
			}
			if (value != null)
			{
				GachaBox box = value.Boxes.Find((GachaBox b) => b.Name == boxName);
				if (box == null)
				{
					return;
				}
				_api.AddToken(box.Token, (string error, int count) =>
				{
					if (!string.IsNullOrEmpty(error))
					{
						Debug.LogError("Server responded with error to Gacha AddTokenForBox: '{0}'", error);
					}
					_tokens[box.Token] = count;
					Debug.Log(string.Format("SparxGachaManager > AddTokenForBox > Changing Balance for box '{0}' to {1}", box.Token, count));
					onComplete(error, count);
				});
			}
			else
			{
				string text = string.Format("Could not find set for group: {0}", group);
				Debug.LogError(text);
				onComplete(text, -1);
			}
		}

		public void GetServerTimestamp(Action<string, string> onCompleted)
		{
			_api.GetTimestamp((string error, Hashtable data) =>
			{
				if (data != null)
				{
					string obj = Dot.String("version", data, string.Empty);
					onCompleted(null, obj);
				}
				else
				{
					Debug.LogError("Server responded with error to Gacha Timestamp: '{0}' '{1}'", error, data.ToString());
					onCompleted(error, null);
				}
			});
		}

		public void Sync(Action<Dictionary<string, int>> onComplete)
		{
			_api.SyncTokens((string error, ArrayList data) =>
			{
				if (data != null)
				{
					foreach (Hashtable datum in data)
					{
						string text = Dot.String("token", datum, string.Empty);
						int value = Dot.Integer("count", datum, 0);
						if (text.Length > 0)
						{
							_tokens[text] = value;
						}
					}
					_initialTokensComplete = true;
					CompleteConnection();
				}
				else
				{
					Debug.LogError("Problem getting tokens in Sync. Error:{0}", error);
				}
			});
		}

		public override void Initialize(Config config)
		{
			_config = config.GachaConfig;
			_api = new GachaAPI(base.Hub.ApiEndPoint);
			Debug.Log("[GachaManager::Initialize] initializing Gacha manager");
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public override void Connect()
		{
			Sync((Dictionary<string, int> tokens) =>
			{
				_initialTokensComplete = true;
			});
			string[] groups = _config.Groups;
			foreach (string text in groups)
			{
				_sets[text] = null;
				_api.FetchSet(text, OnSetFetch);
			}
		}

		public void RebuildSets(Action<string> cb)
		{
			List<string> waitingGroups = new List<string>();
			string[] groups = _config.Groups;
			foreach (string text in groups)
			{
				_sets[text] = null;
				waitingGroups.Add(text);
			}
			string[] groups2 = _config.Groups;
			foreach (string text2 in groups2)
			{
				_api.FetchSet(text2, (string setGroup, string error, Hashtable data) =>
				{
					waitingGroups.Remove(setGroup);
					if (data != null)
					{
						GachaSet value = new GachaSet(data);
						_sets[setGroup] = value;
					}
					if (cb != null && waitingGroups.Count == 0)
					{
						cb(null);
					}
				});
			}
		}

		public void Update()
		{
		}

		public override void Disconnect(bool isLogout)
		{
		}

		public override void Async(string message, object payload)
		{
			switch (message.ToLower())
			{
			case "sync":
				Sync(null);
				break;
			}
		}

		private void CompleteConnection()
		{
			if (base.State != SubSystemState.Connecting)
			{
				return;
			}
			bool flag = true;
			foreach (KeyValuePair<string, GachaSet> set in _sets)
			{
				if (set.Value == null)
				{
					flag = false;
				}
			}
			if (flag && _initialTokensComplete)
			{
				base.State = SubSystemState.Connected;
			}
		}

		private void OnSetFetch(string group, string error, Hashtable data)
		{
			if (data != null)
			{
				GachaSet gachaSet = new GachaSet(data);
				if (gachaSet.IsValid)
				{
					_sets[group] = gachaSet;
					CompleteConnection();
				}
				else
				{
					Debug.LogError("GachaManager.OnSetFetch Group {0} was inproperly formatted from the server", group);
					base.State = SubSystemState.Error;
				}
			}
			else
			{
				Debug.LogError("GachaManager.OnSetFetch Group {0} got Error:{1}", group, error);
				base.State = SubSystemState.Error;
			}
		}

		public override string ToString()
		{
			string text = string.Format("GachaManager Debug Dump : [{0}, {1}, {2}]", _config.Groups.Length, _sets.Count, _tokens.Count);
			string[] groups = _config.Groups;
			foreach (string text2 in groups)
			{
				text = text + "\n\tGroup: " + text2;
			}
			foreach (KeyValuePair<string, int> token in _tokens)
			{
				string text3 = text;
				text = text3 + "\n\tToken: " + token.Key + " -> " + token.Value;
			}
			foreach (KeyValuePair<string, GachaSet> set in _sets)
			{
				string text3 = text;
				text = text3 + "\n\tSet: " + set.Key + " -> " + set.Value.ToString();
			}
			return text;
		}
	}
}
