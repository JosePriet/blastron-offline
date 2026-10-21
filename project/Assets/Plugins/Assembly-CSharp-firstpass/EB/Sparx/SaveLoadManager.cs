using System.Collections;
using System.Collections.Generic;
using EB.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public class SaveLoadManager : SubSystem, Updatable
	{
		private class SaveInfo
		{
			public int id;

			public List<ISaveLoadInterface> interfaces = new List<ISaveLoadInterface>();

			public void Add(ISaveLoadInterface[] other)
			{
				foreach (ISaveLoadInterface item in other)
				{
					if (!interfaces.Contains(item))
					{
						interfaces.Add(item);
					}
				}
			}

			public override string ToString()
			{
				string text = string.Empty;
				for (int i = 0; i < interfaces.Count; i++)
				{
					text = text + interfaces[i].Key + ",";
				}
				return text;
			}
		}

		private const string kVersionKey = "version";

		public static int Version = 1;

		private List<ISaveLoadInterface> _saveMap = new List<ISaveLoadInterface>();

		private bool _doLoad;

		private Id _loadId = Id.Null;

		private Id _loadedId;

		public bool IsLoading;

		public bool IsSaving;

		public bool IsError;

		public static bool IsReady;

		public static bool IsVisiting;

		private EB.Collections.Queue<SaveInfo> _saves = new EB.Collections.Queue<SaveInfo>();

		private int _nextSaveId = 1;

		private int _doneSaveId;

		private WaitForFixedUpdate _waiter = new WaitForFixedUpdate();

		public bool UpdateOffline
		{
			get
			{
				return true;
			}
		}

		public Id LoadedId
		{
			get
			{
				if (_loadedId != null)
				{
					return _loadedId;
				}
				return base.Hub.LoginManager.LocalUserId;
			}
		}

		public void Register(ISaveLoadInterface obj)
		{
			Unregister(obj);
			_saveMap.Add(obj);
		}

		public void Unregister(ISaveLoadInterface obj)
		{
			_saveMap.Remove(obj);
		}

		public override IEnumerator LoadOfflineData()
		{
			yield return StartCoroutine(DoLoad(Id.Offline));
		}

		public void Update()
		{
			if (IsSaving || IsLoading)
			{
				return;
			}
			if (_saves.Count > 0)
			{
				Id localUserId = base.Hub.LoginManager.LocalUserId;
				if (localUserId == _loadedId)
				{
					StartCoroutine(DoSave(_saves.Dequeue()));
					return;
				}
				Debug.LogError("Cant save because we havent loaded yet!");
			}
			if (_doLoad)
			{
				_doLoad = false;
				StartCoroutine(DoLoad(_loadId ?? base.Hub.LoginManager.LocalUserId));
			}
		}

		public Coroutine Save(params ISaveLoadInterface[] saveLoad)
		{
			return _SaveInternal(saveLoad);
		}

		public void Load(Id userId)
		{
			_doLoad = true;
			_loadId = userId;
		}

		public Coroutine LoadSingle(Id userId, ISaveLoadInterface loader)
		{
			return StartCoroutine(DoLoadSingle(userId, loader));
		}

		public Coroutine _SaveInternal(ISaveLoadInterface[] saveLoad)
		{
			if (saveLoad == null || saveLoad.Length == 0)
			{
				Debug.Log("Full Save Requested");
				saveLoad = _saveMap.ToArray();
			}
			SaveInfo saveInfo = null;
			if (_saves.Count > 0)
			{
				saveInfo = _saves.Peek();
			}
			else
			{
				saveInfo = new SaveInfo();
				saveInfo.id = _nextSaveId++;
				_saves.Enqueue(saveInfo);
			}
			saveInfo.Add(saveLoad);
			return Wait(saveInfo.id);
		}

		public Coroutine Wait(int saveId)
		{
			return StartCoroutine(DoWait(saveId));
		}

		private IEnumerator DoWait(int saveId)
		{
			while (_doneSaveId < saveId)
			{
				yield return _waiter;
			}
		}

		private IEnumerator DoLoadSingle(Id userId, ISaveLoadInterface loader)
		{
			bool complete = false;
			base.Hub.DataManager.Load(userId, loader.Key, (string err, Data result) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					base.Hub.FatalError(err ?? "Failed to load data");
					complete = true;
				}
			});
			while (!complete)
			{
				yield return 1;
			}
			Data data = base.Hub.DataManager.GetData(userId);
			object obj = data.data[loader.Key];
			Hashtable value = obj as Hashtable;
			if (value == null)
			{
				value = new Hashtable();
			}
			yield return StartCoroutine(loader.Load(userId, value));
			yield return StartCoroutine(loader.PostLoad(userId));
		}

		private IEnumerator DoSave(SaveInfo info)
		{
			IsSaving = true;
			Debug.Log("Sparx.SaveLoadManager > Do Save: " + info);
			Id userId = base.Hub.LoginManager.LocalUserId;
			Hashtable master = new Hashtable();
			for (int i = 0; i < info.interfaces.Count; i++)
			{
				ISaveLoadInterface saveLoad = info.interfaces[i];
				Hashtable data = new Hashtable();
				yield return StartCoroutine(saveLoad.Save(userId, data));
				master[saveLoad.Key] = data;
			}
			master["version"] = Version.ToString();
			master["platform"] = Application.platform.ToString();
			bool complete = false;
			base.Hub.DataManager.Save(userId, master, (string error) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					base.Hub.FatalError(error);
				}
				complete = true;
			});
			while (!complete)
			{
				yield return 1;
			}
			IsSaving = false;
			_doneSaveId++;
		}

		public Coroutine DebugClear()
		{
			return StartCoroutine(DoDebugClear());
		}

		private IEnumerator DoDebugClear()
		{
			Hashtable tmp = new Hashtable();
			foreach (ISaveLoadInterface saveload in _saveMap)
			{
				tmp[saveload.Key] = new Hashtable();
			}
			tmp["version"] = 0;
			bool complete = false;
			base.Hub.DataManager.Save(base.Hub.LoginManager.LocalUserId, tmp, (string obj) =>
			{
				complete = true;
			});
			while (!complete)
			{
				yield return 1;
			}
		}

		public Coroutine DebugClear(ISaveLoadInterface saveload)
		{
			return StartCoroutine(DoDebugClear(saveload));
		}

		public IEnumerator DoDebugClear(ISaveLoadInterface saveload)
		{
			Hashtable tmp = new Hashtable();
			tmp[saveload.Key] = new Hashtable();
			bool complete = false;
			base.Hub.DataManager.Save(base.Hub.LoginManager.LocalUserId, tmp, (string obj) =>
			{
				complete = true;
			});
			while (!complete)
			{
				yield return 1;
			}
		}

		private int GetVersion(IDictionary data)
		{
			if (data == null)
			{
				return 0;
			}
			object obj = data["version"];
			if (obj != null)
			{
				int result = 0;
				if (int.TryParse(obj.ToString(), out result))
				{
					return result;
				}
			}
			return Version;
		}

		private IEnumerator DoLoad(Id userId)
		{
			IsLoading = true;
			Debug.Log("Sparx.SaveLoadManager > Do Load: " + userId);
			bool complete = false;
			base.Hub.DataManager.Load(userId, string.Empty, (string err, Data res) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					base.Hub.FatalError(err);
				}
				complete = true;
			});
			while (!complete)
			{
				yield return 1;
			}
			_loadedId = userId;
			IDictionary master = base.Hub.DataManager.GetData(userId).data;
			if (GetVersion(master) != Version)
			{
				Debug.Log("Version Mismatch, defaulting data!!");
				master = new Hashtable();
			}
			for (int i = 0; i < _saveMap.Count; i++)
			{
				ISaveLoadInterface saveLoad = _saveMap[i];
				object obj = master[saveLoad.Key];
				Hashtable data = obj as Hashtable;
				if (data == null)
				{
					data = new Hashtable();
				}
				yield return StartCoroutine(saveLoad.Load(userId, data));
			}
			for (int i2 = 0; i2 < _saveMap.Count; i2++)
			{
				ISaveLoadInterface saveLoad2 = _saveMap[i2];
				yield return StartCoroutine(saveLoad2.PostLoad(userId));
			}
			if (userId == base.Hub.LoginManager.LocalUserId)
			{
				Hashtable saveData = new Hashtable();
				for (int i3 = 0; i3 < _saveMap.Count; i3++)
				{
					ISaveLoadInterface saveLoad3 = _saveMap[i3];
					if (saveLoad3.PostLoadSaveRequired)
					{
						Debug.LogWarning("PostLoadSave: " + saveLoad3.Key);
						Hashtable data2 = new Hashtable();
						yield return StartCoroutine(saveLoad3.Save(userId, data2));
						saveData[saveLoad3.Key] = data2;
					}
				}
				if (saveData.Count > 0)
				{
					Debug.Log("Handling post load save");
					complete = false;
					base.Hub.DataManager.Save(userId, saveData, (string error) =>
					{
						complete = true;
					});
					while (!complete)
					{
						yield return 1;
					}
				}
				base.State = SubSystemState.Connected;
			}
			IsLoading = false;
			IsReady = true;
		}

		public override void Connect()
		{
			base.State = SubSystemState.Connecting;
			Load(base.Hub.LoginManager.LocalUserId);
		}

		public override void Initialize(Config config)
		{
		}

		public override void Disconnect(bool isLogout)
		{
			base.State = SubSystemState.Disconnected;
			if (isLogout)
			{
				base.Hub.DataManager.ClearOfflineData();
			}
		}

		public override void Async(string message, object options)
		{
			switch (message.ToLower())
			{
			case "save":
				Save((ISaveLoadInterface[])null);
				return;
			case "load":
				Load(base.Hub.LoginManager.LocalUserId);
				return;
			}
			Debug.LogError("SaveLoadManager > Unknown Async Message: " + message + " -> " + options);
		}
	}
}
