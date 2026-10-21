using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class ItemLevelManager : MonoBehaviour, ISaveLoadInterface
{
	public static ItemLevelManager Instance = null;

	private Dictionary<int, int> _uses;

	private Dictionary<int, int> _levels;

	private Dictionary<int, ItemLevelData> _itemLevelData;

	private static ItemLevelData.Level[] _emptyList = new ItemLevelData.Level[0];

	public string Key
	{
		get
		{
			return "itemlevels";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return false;
		}
	}

	public void LoadTuningData()
	{
		TuningManager tuningManager = Hub.Instance.TuningManager;
		foreach (ItemLevelData value in _itemLevelData.Values)
		{
			string text = "itemlevels." + value.name;
			Memory.Unlock(value);
			tuningManager.Apply(value, text);
			Memory.Lock(value);
		}
	}

	public bool IncrementUse(int hash)
	{
		if (!_itemLevelData.ContainsKey(hash))
		{
			EB.Debug.LogWarning("Invalid item hash " + hash);
			return false;
		}
		if (IsMaxLevel(hash))
		{
			return false;
		}
		int num = GetLevel(hash) + 1;
		if (num >= _itemLevelData[hash]._levels.Length)
		{
			EB.Debug.LogError("There are less levels in the level data for hash " + hash + " than what is expected!");
			return false;
		}
		ItemLevelData.Level level = _itemLevelData[hash]._levels[num];
		bool result = false;
		if (!_uses.ContainsKey(hash))
		{
			_uses[hash] = 1;
			_levels[hash] = 0;
		}
		else
		{
			int num2 = _uses[hash] + 1;
			if (num2 >= level.uses)
			{
				_uses[hash] = 0;
				_levels[hash] = num;
				result = true;
				InventoryDB.MetaData metaData = InventoryDB.GetMetaData(hash);
				string text = ((metaData == null) ? ("itemlevel/" + hash) : ("itemlevel/" + metaData.name));
				Telemetry.Event(text, num);
				Telemetry.Flush();
			}
			else
			{
				_uses[hash] = num2;
			}
		}
		Save();
		return result;
	}

	public bool IsMaxLevel(int hash)
	{
		return GetLevel(hash) == Tuning.Instance.ItemLevelling_MaxLevel - 1;
	}

	public int GetLevel(int hash)
	{
		if (_levels.ContainsKey(hash))
		{
			return _levels[hash];
		}
		return 0;
	}

	public int GetUses(int hash)
	{
		if (_uses.ContainsKey(hash))
		{
			return _uses[hash];
		}
		return 0;
	}

	public float GetPointsMultiplier(int hash, int level)
	{
		float result = 0f;
		if (level >= 0)
		{
			ItemLevelData.Level[] itemLevels = GetItemLevels(hash);
			if (itemLevels.Length > 0)
			{
				int num = Mathf.Min(level, itemLevels.Length - 1);
				result = itemLevels[num].pointsMultiplier;
			}
		}
		return result;
	}

	public ItemLevelData.Level GetLevelData(int hash, int level)
	{
		ItemLevelData.Level result = null;
		ItemLevelData.Level[] itemLevels = GetItemLevels(hash);
		if (itemLevels.Length > 0)
		{
			if (level >= itemLevels.Length)
			{
				result = itemLevels[itemLevels.Length - 1];
			}
			else
			{
				result = ((level >= 0) ? itemLevels[level] : itemLevels[0]);
			}
		}
		return result;
	}

	public bool CanItemLevel(int hash)
	{
		return _itemLevelData.ContainsKey(hash);
	}

	public void DebugIncrementLevel(int hash, int inc)
	{
		ItemLevelData.Level[] itemLevels = GetItemLevels(hash);
		if (itemLevels.Length > 0)
		{
			int num = (_levels.ContainsKey(hash) ? _levels[hash] : 0);
			int num2 = num + inc;
			if (num2 < itemLevels.Length && num2 >= 0)
			{
				_uses[hash] = 0;
				_levels[hash] = num2;
				Save();
			}
		}
	}

	public Coroutine Save()
	{
		return Hub.Instance.SaveLoadManager.Save(this);
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		data["uses"] = new Hashtable(_uses);
		data["levels"] = new Hashtable(_levels);
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		_uses = new Dictionary<int, int>();
		_levels = new Dictionary<int, int>();
		string[] datakeys = new string[2] { "uses", "levels" };
		string[] dataField = new string[2] { "uses", "level" };
		Dictionary<int, int>[] dictionaries = new Dictionary<int, int>[2] { _uses, _levels };
		for (int i = 0; i < datakeys.Length; i++)
		{
			string key = datakeys[i];
			string field = dataField[i];
			Dictionary<int, int> dictionary = dictionaries[i];
			if (!data.ContainsKey(key))
			{
				continue;
			}
			Hashtable loadedData = Dot.Object(key, data, new Hashtable());
			foreach (DictionaryEntry entry in loadedData)
			{
				int itemHash = 0;
				Serialization.Deserialize(entry.Key, ref itemHash);
				int val = 0;
				Serialization.Deserialize(entry.Value, ref val);
				dictionary.Add(itemHash, val);
			}
		}
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		yield break;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		Hub.Instance.SaveLoadManager.Register(this);
		_itemLevelData = new Dictionary<int, ItemLevelData>();
		string text = "Inventory/ItemLevels/";
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		foreach (InventoryDB.MetaData item in categoryList)
		{
			string path = text + item.itemLevelPrefabName;
			ItemLevelData itemLevelData = Assets.Load<ItemLevelData>(path);
			if (itemLevelData != null)
			{
				_itemLevelData.Add(item.hash, itemLevelData);
			}
		}
	}

	private ItemLevelData.Level[] GetItemLevels(int hash)
	{
		if (_itemLevelData.ContainsKey(hash))
		{
			return _itemLevelData[hash]._levels;
		}
		return _emptyList;
	}
}
