using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class InventoryDB : MonoBehaviour
{
	[Serializable(Tag = "inv")]
	public class MetaData
	{
		[Serializable(Tag = "c")]
		public int category;

		[Serializable(Tag = "n")]
		public string name;

		[Serializable(Tag = "p")]
		public string assetPrefabPath;

		[Serializable(Tag = "uip")]
		public string uiTexturePath;

		[Serializable(Tag = "r")]
		public int softCost;

		[Serializable(Tag = "g")]
		public int hardCost;

		[Serializable(Tag = "ur")]
		public int unlockSoftCost;

		[Serializable(Tag = "ug")]
		public int unlockHardCost;

		[Serializable(Tag = "is")]
		public bool isDefault;

		[Serializable(Tag = "ih")]
		public bool isHidden;

		[Serializable(Tag = "q")]
		public int quantity = 1;

		[Serializable(Tag = "rsq")]
		public int revengeSaleQuantity = 1;

		[Serializable(Tag = "l")]
		public int levelRequired = 1;

		[Serializable(Tag = "sc")]
		public int startingCount;

		[Serializable(Tag = "soo")]
		public int sortOrderOverride = -1;

		[Serializable(Tag = "cpw")]
		public int campaignPlusWeight;

		[Serializable(Tag = "cpc")]
		public int campaignPlusCount;

		[Serializable(Tag = "kid")]
		public int kabamItemId;

		[Serializable(Tag = "icr")]
		public bool isChampionshipTierReward;

		[Serializable(Tag = "ild")]
		public string itemLevelPrefabName;

		[Serializable(Tag = "os")]
		public bool onSale;

		private int _hash;

		public int hash
		{
			get
			{
				if (_hash == 0)
				{
					_hash = Hash.StringHash(name);
				}
				return _hash;
			}
		}

		public string friendlyName
		{
			get
			{
				return LocPrefix + "_NAME";
			}
		}

		public string description
		{
			get
			{
				return LocPrefix + "_DESC";
			}
		}

		public string LocPrefix
		{
			get
			{
				return "ID_INVENTORY_" + StringUtil.SafeKey(name).ToUpper();
			}
		}

		public int revengeSaleSoftCost
		{
			get
			{
				return Mathf.CeilToInt((float)revengeSaleQuantity * (float)softCost / (float)quantity);
			}
		}

		public int revengeSaleHardCost
		{
			get
			{
				return Mathf.CeilToInt((float)revengeSaleQuantity * (float)hardCost / (float)quantity);
			}
		}

		public bool RevengeSaleable
		{
			get
			{
				return revengeSaleQuantity > 0;
			}
		}

		public bool IsFree
		{
			get
			{
				return softCost == 0 && hardCost == 0;
			}
		}

		public InventoryItem.Category ItemCategory
		{
			get
			{
				return (InventoryItem.Category)category;
			}
		}

		public bool IsLocked
		{
			get
			{
				if (isChampionshipTierReward)
				{
					int num = ChampionshipData.TotalStars();
					ChampionshipRewardTiers.Tier tierWithWeapon = ChampionshipData.RewardTiers.GetTierWithWeapon(name);
					if (tierWithWeapon != null)
					{
						return tierWithWeapon.NumStars > num;
					}
					return true;
				}
				bool flag = Profile.Instance.Level < levelRequired;
				if (flag)
				{
					flag = !UnlocksManager.Instance.IsUnlocked(name);
				}
				return flag;
			}
		}

		public bool IsHidden
		{
			get
			{
				return isHidden;
			}
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public override bool Equals(object obj)
		{
			return GetHashCode() == obj.GetHashCode();
		}

		public static int Compare(MetaData m1, MetaData m2)
		{
			if (m1.sortOrderOverride != m2.sortOrderOverride)
			{
				return (m1.sortOrderOverride > m2.sortOrderOverride) ? 1 : (-1);
			}
			if (m1.levelRequired != m2.levelRequired)
			{
				return (m1.levelRequired > m2.levelRequired) ? 1 : (-1);
			}
			if (m1.hardCost != m2.hardCost)
			{
				return (m1.hardCost > m2.hardCost) ? 1 : (-1);
			}
			return string.Compare(m1.friendlyName, m2.friendlyName);
		}
	}

	private static List<MetaData>[] _metaData;

	private static Dictionary<int, MetaData> _metaDataLookup;

	private static Dictionary<int, MetaData> _metaDataServerIdLookup;

	static InventoryDB()
	{
		_metaDataLookup = new Dictionary<int, MetaData>();
		_metaDataServerIdLookup = new Dictionary<int, MetaData>();
		_metaData = new List<MetaData>[2];
		for (int i = 0; i < _metaData.Length; i++)
		{
			_metaData[i] = new List<MetaData>();
		}
	}

	public static void LoadMetaData()
	{
		List<MetaData> result = null;
		TextAsset textAsset = Assets.Load<TextAsset>("inventorydb");
		object value = JSON.Parse(textAsset.text);
		if (Serialization.Deserialize(value, ref result))
		{
			foreach (MetaData item in result)
			{
				_metaDataLookup[item.hash] = item;
				_metaData[item.category].Add(item);
				_metaDataServerIdLookup[item.kabamItemId] = item;
				Memory.Lock(item);
			}
		}
		Assets.Unload(textAsset);
		EB.Debug.Log("Loaded " + _metaDataLookup.Count + " items");
	}

	public static void LoadTuningData()
	{
		TuningManager tuningManager = Hub.Instance.TuningManager;
		foreach (MetaData value in _metaDataLookup.Values)
		{
			string text = "inventory." + StringUtil.SafeKey(value.name);
			Memory.Unlock(value);
			tuningManager.Apply(value, text);
			_metaDataServerIdLookup[value.kabamItemId] = value;
			Memory.Lock(value);
		}
	}

	public static Hashtable GetStartingInventory()
	{
		Hashtable hashtable = new Hashtable();
		foreach (MetaData value in _metaDataLookup.Values)
		{
			if (value.startingCount > 0)
			{
				hashtable[value.name] = value.startingCount;
			}
		}
		return hashtable;
	}

	public static MetaData GetMetaData(int hash)
	{
		MetaData value = null;
		if (_metaDataLookup.TryGetValue(hash, out value))
		{
			return value;
		}
		return null;
	}

	public static MetaData GetMetaDataFromServerId(int id)
	{
		MetaData value = null;
		if (_metaDataServerIdLookup.TryGetValue(id, out value))
		{
			return value;
		}
		return null;
	}

	public static List<MetaData> GetCategoryList(InventoryItem.Category category)
	{
		List<MetaData> list = new List<MetaData>();
		foreach (MetaData item in _metaData[(int)category])
		{
			list.Add(item);
		}
		list.Sort(MetaData.Compare);
		return list;
	}

	public static bool HasWeapons()
	{
		List<MetaData> categoryList = GetCategoryList(InventoryItem.Category.Weapon);
		foreach (MetaData item in categoryList)
		{
			if ("teleportgrenade".Equals(StringUtil.SafeKey(item.name)) || 0 >= Hub.Instance.InventoryManager.GetCount(item.name))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public static List<MetaData> GetUnlockedItems(int level)
	{
		List<MetaData> list = new List<MetaData>();
		foreach (MetaData item in _metaData[0])
		{
			if (item.levelRequired == level)
			{
				list.Add(item);
			}
		}
		foreach (MetaData item2 in _metaData[1])
		{
			if (item2.levelRequired == level)
			{
				list.Add(item2);
			}
		}
		list.Sort(MetaData.Compare);
		return list;
	}

	public static List<MetaData> GetUnlockableItemsOfType(InventoryItem.Category cat, int level, bool checkChamp)
	{
		List<MetaData> list = new List<MetaData>();
		bool flag = true;
		UnityEngine.Debug.LogWarning("Metadata count " + _metaData[(int)cat].Count);
		foreach (MetaData item in _metaData[(int)cat])
		{
			if (checkChamp)
			{
				flag = !item.isChampionshipTierReward;
			}
			if (item.levelRequired == level && item.IsLocked && !item.IsHidden && flag)
			{
				list.Add(item);
			}
		}
		list.Sort(MetaData.Compare);
		return list;
	}

	public static bool HasUnlockableItemOfType(InventoryItem.Category cat, int level)
	{
		foreach (MetaData item in _metaData[(int)cat])
		{
			if (item.levelRequired == level && item.IsLocked && !item.IsHidden)
			{
				return true;
			}
		}
		return false;
	}

	public static List<MetaData> GetUnlockPrerequisite(MetaData item)
	{
		List<MetaData> list = new List<MetaData>();
		int levelRequired = item.levelRequired;
		int maxItemUnlockLevel = Profile.Instance.GetMaxItemUnlockLevel(item.ItemCategory);
		for (int num = levelRequired - 1; num >= maxItemUnlockLevel; num--)
		{
			foreach (MetaData item2 in _metaData[(int)item.ItemCategory])
			{
				if (item2.levelRequired == num && !item2.IsHidden)
				{
					list.Add(item2);
				}
			}
			if (list.Count > 0)
			{
				break;
			}
		}
		return list;
	}
}
