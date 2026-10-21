using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EB.Sparx
{
	public class InventoryManager : SubSystem
	{
		private InventoryConfig _config;

		private InventoryAPI _api;

		private Hashtable _data;

		private Hashtable _used;

		private Hashtable _overrides;

		public bool ForceOverride;

		public bool EverythingFree;

		private List<string> _overrideKeys;

		private Hashtable _inventory
		{
			get
			{
				return (_overrides.Count <= 0 && !ForceOverride) ? _data : _overrides;
			}
		}

		public bool IsOverriden
		{
			get
			{
				return _overrides.Count > 0 || ForceOverride;
			}
		}

		public int OverrideCount
		{
			get
			{
				return _overrides.Count;
			}
		}

		public string GetOverrideItem(int index)
		{
			if (index < _overrideKeys.Count)
			{
				return _overrideKeys[index];
			}
			Debug.LogWarning("Warning: index out of range, returning empty string");
			return string.Empty;
		}

		public override void Initialize(Config config)
		{
			Debug.LogWarning("Inventory initialize");
			_config = config.InventoryConfig;
			if (_config.Listener == null)
			{
				throw new ArgumentNullException("Must provide a valid inventory listener");
			}
			_api = new InventoryAPI(base.Hub.ApiEndPoint);
			_used = new Hashtable();
			_data = new Hashtable();
			_overrides = new Hashtable();
			_overrideKeys = new List<string>();
			ForceOverride = false;
			EverythingFree = false;
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public override void Connect()
		{
			Debug.LogWarning("Inventory Connect");
			base.State = SubSystemState.Connecting;
			Hashtable hashtable = Dot.Object("inventory", base.Hub.LoginManager.LoginData, null);
			if (hashtable != null)
			{
				OnSync(0, null, hashtable);
			}
			else
			{
				Sync();
			}
		}

		public override void Disconnect(bool isLogout)
		{
		}

		public void Override(Hashtable items)
		{
			_overrides = SafeNames(items);
		}

		public void ClearOverride()
		{
			_overrides.Clear();
			_overrideKeys.Clear();
			ForceOverride = false;
			EverythingFree = false;
		}

		public void AddOverride(string name, int count)
		{
			name = SafeName(name);
			_overrides[name] = count;
			AddOverrideKey(name);
		}

		public void AddOverrideKey(string key)
		{
			_overrideKeys.Add(key);
		}

		public void ClearOverrideKeys()
		{
			_overrideKeys.Clear();
		}

		public void SinglePlayerClearUsedInventory()
		{
			_used.Clear();
		}

		public bool IsEmpty()
		{
			if (EverythingFree)
			{
				return false;
			}
			foreach (string key in _inventory.Keys)
			{
				if (GetCount(key) > 0)
				{
					return false;
				}
			}
			return true;
		}

		public int GetCount(string item)
		{
			item = SafeName(item);
			int num = Dot.Integer(item, _inventory, 0);
			int num2 = Dot.Integer(item, _used, 0);
			return Mathf.Max(0, num - num2);
		}

		public int AddItem(string item, int quantity)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(item, quantity);
			Hashtable items = hashtable;
			return AddItems(items);
		}

		public int UseItem(string item, int quantity)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(item, quantity);
			Hashtable items = hashtable;
			return UseItems(items);
		}

		public int UseItems(Hashtable items)
		{
			items = SafeNames(items);
			return _api.Use(items, OnSync);
		}

		public bool UseInGame(string item, int quantity)
		{
			item = SafeName(item);
			if (GetCount(item) >= quantity)
			{
				int num = Dot.Integer(item, _used, 0);
				num += quantity;
				_used[item] = num;
				if (!IsOverriden && base.Hub.GameManager != null && base.Hub.GameManager.Game != null)
				{
					base.Hub.GameManager.Game.SendGameCommand(0u, "use", new Hashtable { { item, quantity } });
				}
				return true;
			}
			return false;
		}

		public int AddItems(Hashtable items)
		{
			items = SafeNames(items);
			return _api.Add(items, OnAdd);
		}

		public int PurchaseItems(Hashtable items, int cost)
		{
			items = SafeNames(items);
			if (cost > 0)
			{
				return _api.Purchase(items, cost, OnPurchase);
			}
			return _api.Add(items, OnAdd);
		}

		public override void Async(string message, object payload)
		{
			switch (message.ToLower())
			{
			case "sync":
				Debug.Log("recieved sync notification");
				Sync();
				break;
			}
		}

		public int Sync()
		{
			Debug.Log("SparxInventoryManager > Sync");
			return _api.Sync(OnSync);
		}

		public Coroutine Wait(int requestId)
		{
			return _api.Wait(requestId);
		}

		private void OnPurchase(int transId, string err, Hashtable data)
		{
			if (err == "nsf")
			{
				_config.Listener.OnInventoryPurchaseFailed(transId);
				return;
			}
			if (!string.IsNullOrEmpty(err))
			{
				base.State = SubSystemState.Error;
				return;
			}
			Hashtable hashtable = Dot.Object("__wallet", data, null);
			if (hashtable != null)
			{
				base.Hub.WalletManager.StoreBalance(hashtable);
				data.Remove("__wallet");
			}
			else
			{
				base.Hub.WalletManager.Fetch();
			}
			foreach (DictionaryEntry datum in data)
			{
				_inventory[SafeName(datum.Key.ToString())] = datum.Value;
			}
			_config.Listener.OnInventoryItemsAdded(transId, data);
		}

		private void OnAdd(int transId, string err, Hashtable data)
		{
			if (!string.IsNullOrEmpty(err))
			{
				base.State = SubSystemState.Error;
				return;
			}
			foreach (DictionaryEntry datum in data)
			{
				_inventory[SafeName(datum.Key.ToString())] = datum.Value;
			}
			_config.Listener.OnInventoryItemsAdded(transId, data);
		}

		private void OnSync(int transId, string err, Hashtable data)
		{
			if (!string.IsNullOrEmpty(err))
			{
				FatalError(err);
				return;
			}
			Debug.Log("SparxInventoryManager > OnSync: data: " + JSON.Stringify(data));
			base.State = SubSystemState.Connected;
			if (data == null)
			{
				_data = new Hashtable();
				if (_config.StartingInventory.Count > 0)
				{
					Debug.Log("SparxInventoryManager > OnSync > initializing inventory");
					AddItems(_config.StartingInventory);
				}
			}
			else
			{
				string text = "SparxInventoryManager > OnSync > data.Count is " + data.Count;
				foreach (DictionaryEntry datum in data)
				{
					string text2 = text;
					text = text2 + "\nSparxInventoryManager > OnSync > Entry: " + datum.Key.ToString() + " : " + datum.Value.ToString();
				}
				Debug.Log(text);
				_data = SafeNames(data);
			}
			if (base.Hub.GameManager != null)
			{
				string text3 = string.Empty;
				if (base.Hub.GameManager.Game != null)
				{
					text3 = base.Hub.GameManager.Game.Attributes["Championship"].ToString();
				}
				if (base.Hub.GameManager.Game == null || "kOn" == text3)
				{
					_used.Clear();
				}
			}
			_config.Listener.OnInventorySynced(transId);
		}

		public string SafeName(string src)
		{
			return StringUtil.SafeKey(src);
		}

		private Hashtable SafeNames(Hashtable src)
		{
			Hashtable hashtable = new Hashtable();
			foreach (DictionaryEntry item in src)
			{
				hashtable[SafeName(item.Key.ToString())] = item.Value;
			}
			return hashtable;
		}
	}
}
