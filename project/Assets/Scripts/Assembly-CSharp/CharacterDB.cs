using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Collections;
using EB.Sparx;
using SmoothMoves;
using UnityEngine;

public class CharacterDB
{
	public enum Category
	{
		kHead = 0,
		kTorso = 1,
		kLegs = 2,
		kNum = 3,
		kUnknown = 4
	}

	[EB.Serializable(Tag = "char")]
	public class MetaData
	{
		[EB.Serializable(Tag = "c")]
		public int category;

		[EB.Serializable(Tag = "n")]
		public string name;

		[EB.Serializable(Tag = "app")]
		public string assetPrefabPath;

		[EB.Serializable(Tag = "an")]
		public string attributeName = string.Empty;

		[EB.Serializable(Tag = "ctp")]
		public string cacTexturePath;

		[EB.Serializable(Tag = "cpox")]
		public float cacPivotOffsetX;

		[EB.Serializable(Tag = "cpoy")]
		public float cacPivotOffsetY;

		[EB.Serializable(Tag = "pc")]
		public int coins;

		[EB.Serializable(Tag = "pd")]
		public int bucks;

		[EB.Serializable(Tag = "soo")]
		public int sortOrderOverride = -1;

		[EB.Serializable(Tag = "ul")]
		public int unlockLevel;

		[EB.Serializable(Tag = "is")]
		public bool isDefault;

		[EB.Serializable(Tag = "ih")]
		public bool isHidden;

		[EB.Serializable(Tag = "icb")]
		public bool isChampionshipBoss;

		[EB.Serializable(Tag = "ie")]
		public bool isEpic;

		[EB.Serializable(Tag = "os")]
		public bool onSale;

		[EB.Serializable(Tag = "xp")]
		public int xp;

		[EB.Serializable(Tag = "us")]
		public int numUpgradeStars = 5;

		private int _hash;

		[Unchecked]
		public int Tier = -1;

		private MetaData _previousTierItem;

		private bool _previousTierItemKnown;

		private MetaData _nextTierItem;

		private bool _nextTierItemKnown;

		[Unchecked]
		public bool IsLocked = true;

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
				return "ID_CHAR_" + StringUtil.SafeKey(name).ToUpper();
			}
		}

		public bool IsFree
		{
			get
			{
				return coins == 0 && bucks == 0;
			}
		}

		public Category AssetCategory
		{
			get
			{
				return (Category)category;
			}
		}

		public bool IsOwned
		{
			get
			{
				if (isDefault || IsFree)
				{
					return true;
				}
				return Hub.Instance.InventoryManager.GetCount(name) > 0;
			}
		}

		public MetaData previousTierItem
		{
			get
			{
				if (!_previousTierItemKnown)
				{
					_previousTierItem = GetRelativeTierItem(this, -1);
					_previousTierItemKnown = true;
				}
				return _previousTierItem;
			}
		}

		public MetaData nextTierItem
		{
			get
			{
				if (!_nextTierItemKnown)
				{
					_nextTierItem = GetRelativeTierItem(this, 1);
					_nextTierItemKnown = true;
				}
				return _nextTierItem;
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

		public static void CalculateUnlockStructure()
		{
			InventoryManager inventoryManager = Hub.Instance.InventoryManager;
			for (int i = 0; i < _metaData.Length; i++)
			{
				List<MetaData> allPartsByTier = GetAllPartsByTier(i, 0);
				allPartsByTier.AddRange(GetAllPartsByTier(i, 1));
				foreach (MetaData item in allPartsByTier)
				{
					if (item.isChampionshipBoss)
					{
						item.IsLocked = item.previousTierItem == null || Profile.Instance.GetPartUpgradeProgress(item.previousTierItem.hash) >= 100f;
					}
					else
					{
						item.IsLocked = false;
					}
				}
				bool flag = true;
				int num = 2;
				while (flag)
				{
					allPartsByTier = GetAllPartsByTier(i, num);
					bool flag2 = false;
					foreach (MetaData item2 in allPartsByTier)
					{
						item2.IsLocked = true;
						MetaData metaData = item2.previousTierItem;
						if (metaData != null && metaData.IsOwned)
						{
							float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(metaData.hash);
							if (partUpgradeProgress >= 100f)
							{
								item2.IsLocked = false;
								flag2 = true;
							}
						}
						else if (inventoryManager.GetCount(item2.name) > 0)
						{
							item2.IsLocked = false;
							flag2 = true;
						}
					}
					if (flag2)
					{
						foreach (MetaData item3 in allPartsByTier)
						{
							MetaData metaData2 = item3.previousTierItem;
							if (metaData2 == null)
							{
								item3.IsLocked = false;
							}
						}
					}
					if (allPartsByTier.Count < 1)
					{
						flag = false;
					}
					num++;
				}
			}
		}

		private static MetaData GetRelativeTierItem(MetaData part, int offset)
		{
			int tier = part.Tier;
			List<MetaData> allPartsByTier = GetAllPartsByTier(part.category, tier + offset);
			string namePrefix = part.GetNamePrefix();
			return allPartsByTier.Find((MetaData item) => item.GetNamePrefix() == namePrefix);
		}

		private string GetNamePrefix()
		{
			int length = name.LastIndexOf("_");
			return name.Substring(0, length);
		}

		public static int Compare(MetaData m1, MetaData m2)
		{
			if (m1.sortOrderOverride != m2.sortOrderOverride)
			{
				return (m1.sortOrderOverride > m2.sortOrderOverride) ? 1 : (-1);
			}
			if (m1.unlockLevel != m2.unlockLevel)
			{
				return (m1.unlockLevel > m2.unlockLevel) ? 1 : (-1);
			}
			if (m1.bucks != m2.bucks)
			{
				return (m1.bucks > m2.bucks) ? 1 : (-1);
			}
			return string.Compare(m1.friendlyName, m2.friendlyName);
		}
	}

	public class Recipe
	{
		public int[] parts = new int[3];

		public float[] UpgradeProgress = new float[3];

		[NoSerialize]
		public int useCount;

		public Recipe()
		{
		}

		public Recipe(object data)
		{
			parts = Util.GetIntArray(Dot.Array("p", data, null));
			if (parts.Length < 3)
			{
				int[] array = new int[3];
				Array.Copy(parts, array, Mathf.Min(array.Length, parts.Length));
				parts = array;
			}
			UpgradeProgress = Util.GetFloatArray(Dot.Array("u", data, null));
			if (UpgradeProgress.Length < 3)
			{
				float[] array2 = new float[3];
				Array.Copy(UpgradeProgress, array2, Mathf.Min(array2.Length, UpgradeProgress.Length));
				UpgradeProgress = array2;
			}
		}

		public float GetAverageTier()
		{
			int num = 0;
			for (int i = 0; i < 3; i++)
			{
				int hash = parts[i];
				MetaData metaData = GetMetaData(hash);
				num += metaData.Tier;
			}
			return (float)num / 3f;
		}

		public override bool Equals(object obj)
		{
			if (obj is Recipe)
			{
				Recipe recipe = (Recipe)obj;
				return ToJson() == recipe.ToJson();
			}
			return false;
		}

		public Hashtable ToHashtable()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["p"] = new ArrayList(parts);
			hashtable["u"] = new ArrayList(UpgradeProgress);
			return hashtable;
		}

		public override int GetHashCode()
		{
			return ToJson().GetHashCode();
		}

		public string ToJson()
		{
			return JSON.Stringify(ToHashtable());
		}

		public void SetPart(MetaData info, float upgradeProgress)
		{
			parts[info.category] = info.hash;
			UpgradeProgress[info.category] = upgradeProgress;
		}

		public int GetPart(Category cat)
		{
			return parts[(int)cat];
		}

		public Recipe Clone()
		{
			Recipe recipe = new Recipe();
			recipe.CopyFrom(this);
			return recipe;
		}

		public void CopyFrom(Recipe src)
		{
			Array.Copy(src.parts, parts, parts.Length);
			Array.Copy(src.UpgradeProgress, UpgradeProgress, UpgradeProgress.Length);
		}
	}

	public abstract class ContentInfo
	{
		private SafeInt _bucks;

		private SafeInt _coins;

		private SafeBool _default;

		private CharacterData _characterData;

		private GameObject _asset;

		private int _hash;

		public int RefCount = 1;

		public GameObject Asset
		{
			get
			{
				return _asset;
			}
		}

		public int HashValue
		{
			get
			{
				return _hash;
			}
		}

		public Category Category
		{
			get
			{
				return _characterData.Category;
			}
		}

		public int PartIndex
		{
			get
			{
				return (int)Category;
			}
		}

		public CharacterData CharacterData
		{
			get
			{
				return _characterData;
			}
		}

		public bool IsFree
		{
			get
			{
				return Bucks == 0 && Coins == 0;
			}
		}

		public int Price
		{
			get
			{
				return (Bucks <= 0) ? Coins : Bucks;
			}
		}

		public string FriendlyName
		{
			get
			{
				return CharacterData.FriendlyName;
			}
		}

		public string AssetName
		{
			get
			{
				return CharacterData.name;
			}
		}

		public int Bucks
		{
			get
			{
				return _bucks;
			}
		}

		public int Coins
		{
			get
			{
				return _coins;
			}
		}

		public bool IsDefault
		{
			get
			{
				return _default;
			}
		}

		public ContentInfo(GameObject asset, int hash)
		{
			_asset = asset;
			_hash = hash;
			_characterData = asset.GetComponent<CharacterData>();
			_bucks = _characterData.Bucks;
			_coins = _characterData.Coins;
			_default = _characterData.IsDefault;
		}

		public abstract void Apply(GameObject obj);

		public virtual void Remove(GameObject obj)
		{
		}

		public virtual GameObject Clone(GameObject obj)
		{
			return null;
		}

		public abstract void SwapMaterial(GameObject obj);
	}

	public class TextureAtlasContentInfo : ContentInfo
	{
		public TextureAtlasContentInfo(GameObject asset, int hash)
			: base(asset, hash)
		{
		}

		public override void Apply(GameObject obj)
		{
			PartSwapper component = obj.GetComponent<PartSwapper>();
			if (component != null)
			{
				if (base.CharacterData.IsDefault)
				{
					component.ResetTexture(base.CharacterData.Category);
					return;
				}
				int defaultPartId = GetDefaultPartId((int)base.CharacterData.Category);
				ContentInfo content = GetContent(defaultPartId);
				component.SetTexture(base.CharacterData.Category, content.CharacterData.TextureAtlas, base.CharacterData.TextureAtlas);
				if (base.CharacterData.Category == Category.kHead)
				{
					component.SwapFaceTexture(content.CharacterData.FaceAtlas, base.CharacterData.FaceAtlas);
				}
			}
			else
			{
				EB.Debug.LogError("PartSwapper is missing");
			}
		}

		public override void SwapMaterial(GameObject obj)
		{
			PartSwapper component = obj.GetComponent<PartSwapper>();
			if (component != null)
			{
				component.SwapMaterial(base.CharacterData.uiMaterial, base.CharacterData.inGameMaterial);
				if (base.CharacterData.Category == Category.kHead)
				{
					component.SwapMaterial(base.CharacterData.uiFaceMaterial, base.CharacterData.inGameFaceMaterial);
				}
			}
		}
	}

	private static Dictionary<int, ContentInfo> _lookup;

	private static List<MetaData>[] _metaData;

	private static Dictionary<int, MetaData> _metaDataLookup;

	private static EB.Collections.Stack<GameObject> _pool;

	private static int _next;

	private static int _done;

	static CharacterDB()
	{
		_lookup = new Dictionary<int, ContentInfo>();
		_metaDataLookup = new Dictionary<int, MetaData>();
		_next = 0;
		_done = 0;
		_metaData = new List<MetaData>[3];
		for (int i = 0; i < _metaData.Length; i++)
		{
			_metaData[i] = new List<MetaData>();
		}
	}

	public static Coroutine Initialize(int numCharacters)
	{
		LoadMetaData();
		for (int i = 0; i < _metaData.Length; i++)
		{
			foreach (MetaData item in _metaData[i])
			{
				item.Tier = PartManager.Instance.GetTier(item.hash);
			}
		}
		return Coroutines.Run(LoadPool(numCharacters));
	}

	public static void LoadMetaData()
	{
		UnityEngine.Object[] array = Assets.LoadAll("cac_db", typeof(TextAsset));
		for (int i = 0; i < array.Length; i++)
		{
			TextAsset textAsset = (TextAsset)array[i];
			object value = JSON.Parse(textAsset.text);
			List<MetaData> result = null;
			if (Serialization.Deserialize(value, ref result))
			{
				foreach (MetaData item in result)
				{
					_metaDataLookup[item.hash] = item;
					_metaData[item.category].Add(item);
					if (!string.IsNullOrEmpty(item.attributeName))
					{
						PartManager.Instance.AddPartMapping(item.hash, item.attributeName);
						Memory.Lock(item);
					}
				}
			}
			Assets.Unload(textAsset);
		}
	}

	public static void LoadTuningData()
	{
		TuningManager tuningManager = Hub.Instance.TuningManager;
		foreach (MetaData value in _metaDataLookup.Values)
		{
			string name = "char." + StringUtil.SafeKey(value.name);
			Memory.Unlock(value);
			tuningManager.Apply(value, name);
			Memory.Lock(value);
		}
	}

	public static void ReturnToPool(GameObject instance)
	{
		instance.transform.parent = null;
		instance.transform.localScale = Vector3.zero;
		instance.SetActive(false);
		UnityEngine.Object.DontDestroyOnLoad(instance);
		_pool.Push(instance);
	}

	public static GameObject GetPoolInstance()
	{
		if (_pool.Count > 0)
		{
			return _pool.Pop();
		}
		return null;
	}

	private static IEnumerator LoadPool(int numInstances)
	{
		_pool = new EB.Collections.Stack<GameObject>();
		Recipe recipe = GetDefaultRecipe();
		List<GameObject> tmp = new List<GameObject>(numInstances);
		for (int i = 0; i < numInstances; i++)
		{
			yield return Instantiate(recipe, false, (GameObject gameObject) =>
			{
				if (gameObject == null)
				{
					EB.Debug.LogError("Fatal: failed to load character!");
				}
				gameObject.name = "cac_instance_" + i;
				gameObject.SetActive(false);
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				tmp.Add(gameObject);
			});
		}
		foreach (GameObject instance in tmp)
		{
			ReturnToPool(instance);
		}
	}

	public static string[] GetAssetPaths()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < 3; i++)
		{
			foreach (MetaData item in _metaData[i])
			{
				list.Add(item.assetPrefabPath);
			}
		}
		return list.ToArray();
	}

	public static int Compare(ContentInfo info1, ContentInfo info2)
	{
		return CharacterData.Compare(info1.CharacterData, info2.CharacterData);
	}

	private static ContentInfo StoreAsset(GameObject asset)
	{
		if (asset != null)
		{
			int num = Hash.StringHash(asset.name);
			if (asset.GetComponent<CharacterData>() != null)
			{
				ContentInfo contentInfo = CreateContentInfo(asset, num);
				if (contentInfo != null)
				{
					_lookup[num] = contentInfo;
					return contentInfo;
				}
			}
		}
		return null;
	}

	private static ContentInfo CreateContentInfo(GameObject obj, int hash)
	{
		CharacterData component = obj.GetComponent<CharacterData>();
		if (component == null || component.Category >= Category.kNum)
		{
			return null;
		}
		return new TextureAtlasContentInfo(obj, hash);
	}

	public static ContentInfo GetContent(int hash)
	{
		ContentInfo value = null;
		if (_lookup.TryGetValue(hash, out value))
		{
			return value;
		}
		return null;
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

	public static MetaData GetMetaDataOrDefault(int hash, Category category)
	{
		MetaData value = null;
		if (_metaDataLookup.TryGetValue(hash, out value))
		{
			return value;
		}
		return GetDefaultPart((int)category);
	}

	public static List<MetaData> GetCategory(Category category)
	{
		return GetCategoryList(category);
	}

	public static List<MetaData> GetCategoryList(Category cateogry)
	{
		List<MetaData> list = new List<MetaData>();
		foreach (MetaData item in _metaData[(int)cateogry])
		{
			list.Add(item);
		}
		list.Sort(MetaData.Compare);
		return list;
	}

	public static MetaData GetDefaultPart(int category)
	{
		foreach (MetaData item in _metaData[category])
		{
			if (item.isDefault)
			{
				return item;
			}
		}
		return null;
	}

	private static int GetDefaultPartId(int category)
	{
		foreach (MetaData item in _metaData[category])
		{
			if (item.isDefault)
			{
				return item.hash;
			}
		}
		return 0;
	}

	public static Recipe GetDefaultRecipe()
	{
		Recipe recipe = new Recipe();
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			recipe.parts[i] = GetDefaultPartId(i);
		}
		return recipe;
	}

	public static int GetRandomPart(Category category)
	{
		List<MetaData> categoryList = GetCategoryList(category);
		if (categoryList.Count > 0)
		{
			return categoryList[UnityEngine.Random.Range(0, categoryList.Count)].hash;
		}
		return 0;
	}

	public static Recipe GetRandomRecipe()
	{
		Recipe recipe = new Recipe();
		Randomize(recipe);
		return recipe;
	}

	public static void Randomize(Recipe recipe)
	{
		for (int i = 0; i < 3; i++)
		{
			recipe.parts[i] = GetRandomPart((Category)i);
		}
	}

	public static bool ValidateAndFixRecipe(Recipe recipe)
	{
		bool result = false;
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			MetaData metaData = GetMetaData(recipe.parts[i]);
			if (metaData == null)
			{
				int defaultPartId = GetDefaultPartId(i);
				if (defaultPartId != recipe.parts[i])
				{
					recipe.parts[i] = defaultPartId;
					result = true;
				}
			}
		}
		return result;
	}

	public static void DecRefCount(Recipe recipe)
	{
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			ContentInfo content = GetContent(recipe.parts[i]);
			if (content != null)
			{
				content.RefCount--;
				if (content.RefCount == 0 && !content.IsDefault)
				{
					_lookup.Remove(content.HashValue);
					Assets.Unload(content.Asset);
					Assets.UnloadBundle(content.Asset.name);
				}
			}
		}
	}

	public static void PrintDB()
	{
		foreach (ContentInfo value in _lookup.Values)
		{
			EB.Debug.Log(value.AssetName + " " + value.HashValue + " " + value.RefCount);
		}
	}

	public static Coroutine Instantiate(Recipe recipe, bool doSwap, EB.Action<GameObject> callback)
	{
		return Coroutines.Run(DoInstantiateOrUpdate(null, recipe, doSwap, callback));
	}

	public static Coroutine Update(GameObject cacInstance, Recipe recipe, bool doSwap, EB.Action<GameObject> callback)
	{
		return Coroutines.Run(DoInstantiateOrUpdate(cacInstance, recipe, doSwap, callback));
	}

	private static IEnumerator DoInstantiateOrUpdate(GameObject instance, Recipe recipe, bool doSwap, EB.Action<GameObject> callback)
	{
		int id = _next++;
		while (id != _done)
		{
			yield return 1;
		}
		ValidateAndFixRecipe(recipe);
		List<string> loadList = new List<string>();
		int[] parts = recipe.parts;
		foreach (int hash in parts)
		{
			ContentInfo content = GetContent(hash);
			if (content == null)
			{
				MetaData metaData = GetMetaData(hash);
				if (metaData != null)
				{
					loadList.Add(metaData.assetPrefabPath);
				}
			}
			else
			{
				content.RefCount++;
			}
		}
		if (loadList.Count > 0)
		{
			yield return Assets.LoadMultiAsync(loadList.ToArray(), typeof(GameObject), (UnityEngine.Object[] assets) =>
			{
				for (int j = 0; j < assets.Length; j++)
				{
					if (assets[j] is GameObject)
					{
						StoreAsset((GameObject)assets[j]);
					}
					else
					{
						EB.Debug.LogError("Failed to load  " + loadList[j]);
					}
				}
			});
		}
		if (instance == null)
		{
			instance = GetPoolInstance();
		}
		if (instance == null)
		{
			GameObject instance2;
			yield return Assets.LoadAsync("CAC_player", typeof(GameObject), (UnityEngine.Object asset) =>
			{
				if (asset is GameObject)
				{
					instance2 = (GameObject)UnityEngine.Object.Instantiate(asset, Vector3.zero, Quaternion.identity);
					instance2.SetActive(false);
				}
			});
		}
		if (instance != null)
		{
			BoneAnimation ba = instance.GetComponent<BoneAnimation>();
			ba.ClearAddedAtlases();
			ba.RestoreOriginalMaterials();
			for (int i2 = 0; i2 < recipe.parts.Length; i2++)
			{
				ContentInfo info = GetContent(recipe.parts[i2]);
				if (info != null)
				{
					info.Apply(instance);
					if (doSwap)
					{
						info.SwapMaterial(instance);
					}
				}
				else
				{
					EB.Debug.LogError("Missing content info for " + i2 + " hash: " + recipe.parts[i2]);
				}
			}
			ba.GenerateMesh();
			Material[] array = Util.GatherMaterials(instance, string.Empty);
			foreach (Material mat in array)
			{
				UnityEngine.Object.DontDestroyOnLoad(mat);
			}
			RecipeInstance ri = instance.GetComponent<RecipeInstance>() ?? instance.AddComponent<RecipeInstance>();
			ri.Recipe = recipe.Clone();
			Util.SetLayerRecursive(instance, 0);
			yield return new WaitForEndOfFrame();
		}
		if (callback != null)
		{
			callback(instance);
		}
		_done++;
	}

	public static List<MetaData> GetAllPartsByTier(int category, int tier)
	{
		return _metaData[category].FindAll((MetaData item) => item.Tier == tier);
	}

	public static List<MetaData> GetUnlockedItems()
	{
		List<MetaData> list = new List<MetaData>();
		List<MetaData>[] metaData = _metaData;
		foreach (List<MetaData> list2 in metaData)
		{
			list.AddRange(list2.FindAll((MetaData item) => !item.IsLocked));
		}
		return list;
	}
}
