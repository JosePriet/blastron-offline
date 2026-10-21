using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
	public class GachaConfig : EB.Sparx.GachaConfig
	{
		public static int DefaultBoxID;

		public static string DefaultGroupName = "base";

		public static string DefaultTutorialGroupName = "tutorial";

		public static string DefaultTutorialBoxName = "token_tutorial";

		public override string[] Groups
		{
			get
			{
				return new string[2] { DefaultGroupName, DefaultTutorialGroupName };
			}
		}
	}

	public class GachaRewardInfo
	{
		public string texturePath { get; private set; }

		public string friendlyName { get; private set; }

		public GachaResult.RewardCategory category { get; private set; }

		public GachaRewardInfo(string name, string path, GachaResult.RewardCategory category)
		{
			texturePath = path;
			friendlyName = name;
			this.category = category;
		}
	}

	public class GachaResult
	{
		public enum RewardCategory
		{
			Unknown = 0,
			Weapon = 1,
			Boost = 2,
			Cac = 3,
			SoftCurrency = 4,
			SkillPoints = 5,
			GachaSpin = 6
		}

		public RewardCategory rewardCategory { get; private set; }

		public string rewardType { get; private set; }

		public int rewardCount { get; private set; }

		public int paymentBalance { get; private set; }

		public string rewardName { get; private set; }

		public string rewardNameWithQuantity { get; private set; }

		public string rewardTexturePath { get; private set; }

		public GachaResult(GachaPickResult result)
			: this(result.RewardData, result.RewardType, result.RewardCount, result.PaymentBalance)
		{
		}

		public GachaResult(string name, string type = "", int quatity = 0, int balance = 0)
		{
			rewardType = type;
			rewardName = name;
			rewardCount = quatity;
			paymentBalance = balance;
			rewardTexturePath = null;
			if (rewardType.Length < 1)
			{
				EB.Debug.LogError("GachaResult > parameter contained no rewardType or rewardName data! ['{0}', '{1}', '{2}']", name, type, quatity);
				return;
			}
			string name2 = ((!string.IsNullOrEmpty(rewardName)) ? rewardName : rewardType);
			GachaRewardInfo rewardInfo = Instance.GetRewardInfo(name2);
			if (rewardInfo != null)
			{
				rewardTexturePath = rewardInfo.texturePath;
				rewardName = rewardInfo.friendlyName;
				rewardCategory = rewardInfo.category;
				EB.Debug.Log("GachaResult > {0}", ToString());
			}
			if (rewardCount > 1)
			{
				rewardNameWithQuantity = Localizer.Format("ID_UI_GACHA_REWARD_FORMAT_MULTI", rewardName, rewardCount);
			}
			else
			{
				rewardNameWithQuantity = Localizer.Format("ID_UI_GACHA_REWARD_FORMAT_SINGLE", rewardName);
			}
		}

		public override string ToString()
		{
			return string.Format("rewardCategory:{0} rewardType:{1} rewardCount:{2} rewardName:{3}", rewardCategory.ToString(), rewardType, rewardCount, rewardName);
		}
	}

	private class PossiblePrizes
	{
		private Dictionary<string, List<string>> BoxPossiblePrizes = new Dictionary<string, List<string>>();

		public List<string> SetPossiblePrizes { get; private set; }

		public PossiblePrizes(GachaSet set, List<string> prizes)
		{
			SetPossiblePrizes = new List<string>();
			if (set == null)
			{
				return;
			}
			SetPossiblePrizes = ConvertPossiblePrizes(set.PossiblePrizes, prizes);
			foreach (GachaBox box in set.Boxes)
			{
				List<string> value = ConvertPossiblePrizes(box.PossiblePrizes, null);
				BoxPossiblePrizes[box.Name] = value;
			}
		}

		private List<string> ConvertPossiblePrizes(List<GachaPossiblePrize> prizes, List<string> cars)
		{
			List<string> list = new List<string>();
			foreach (GachaPossiblePrize prize in prizes)
			{
				if (!string.IsNullOrEmpty(prize.Data))
				{
					list.Add(prize.Data);
				}
			}
			return list;
		}

		public List<string> GetBoxPossiblePrizes(string box)
		{
			List<string> value = null;
			if (!BoxPossiblePrizes.TryGetValue(box, out value))
			{
				value = new List<string>();
			}
			return value;
		}
	}

	private class GroupInfo
	{
		private GachaSet _set;

		public string Set { get; private set; }

		public PossiblePrizes PossiblePrizes { get; private set; }

		public List<string> RewardCars { get; private set; }

		public List<string> AttractImages { get; private set; }

		public GroupInfo(GachaSet set)
		{
			RewardCars = new List<string>();
			PossiblePrizes = new PossiblePrizes(set, RewardCars);
			_set = set;
			if (set != null)
			{
				AttractImages = set.AttractImages;
				Set = set.Name;
			}
			else
			{
				AttractImages = new List<string>();
				Set = "Unknown";
			}
		}

		public int GetNumBoxes()
		{
			int result = 0;
			if (_set != null)
			{
				result = _set.Boxes.Count;
			}
			return result;
		}

		public GachaBox GetBoxByIndex(int index)
		{
			GachaBox result = null;
			if (_set != null && index < _set.Boxes.Count)
			{
				result = _set.Boxes[index];
			}
			return result;
		}

		public int GetIndexFromBox(string boxName)
		{
			int result = -1;
			if (_set != null)
			{
				foreach (GachaBox box in _set.Boxes)
				{
					if (box.Name == boxName)
					{
						result = box.Index;
						break;
					}
				}
			}
			return result;
		}
	}

	private const string InvGachaSpinsPrefix = "gacha_spins_";

	private const int GetTablesRetryTime = 5000;

	private const string RewardType_SoftCurrency = "sc";

	private const string RewardType_SkillPoints = "skillpoint";

	private Dictionary<string, GachaRewardInfo> rewardInfos;

	private string gachaTimestamp = string.Empty;

	private Dictionary<string, GroupInfo> _groups = new Dictionary<string, GroupInfo>();

	private Hub hub;

	private static GachaManager _this;

	public static GachaManager Instance
	{
		get
		{
			return _this;
		}
	}

	public string LanguageCode { get; private set; }

	public void AddFreeSpin(int boxId)
	{
		GachaBox box = GetBox(GachaConfig.DefaultGroupName, boxId);
		if (box == null)
		{
			EB.Debug.LogError("Can not find box " + boxId + " for " + GachaConfig.DefaultGroupName);
			return;
		}
		Hub.Instance.GachaManager.AddTokenForBox(GachaConfig.DefaultGroupName, box.Name, (string error, int count) =>
		{
			if (!string.IsNullOrEmpty(error))
			{
				EB.Debug.LogError("Can not add free token for boxId: " + boxId + " -> " + error);
				ScreenManager.Instance.ShowError("ID_SPARX_ERROR_UNKNOWN", null);
			}
		});
	}

	public int GetFreeSpins(string groupName, int boxId)
	{
		GachaBox box = GetBox(groupName, boxId);
		if (box == null)
		{
			EB.Debug.LogError("Can not find group: " + groupName + ", box: " + boxId);
			return 0;
		}
		int num = Hub.Instance.GachaManager.GetTokenCount(box.Token);
		if (boxId == GachaConfig.DefaultBoxID && groupName == GachaConfig.DefaultGroupName && VaultTimer.Instance.FreeBundleAvailable)
		{
			num++;
		}
		return num;
	}

	public int GetTotalFreeSpins()
	{
		string text = (Profile.Instance.IsTutorialDone("vault") ? GachaConfig.DefaultGroupName : GachaConfig.DefaultTutorialGroupName);
		int numBoxes = GetNumBoxes(text);
		int num = 0;
		for (int i = 0; i < numBoxes; i++)
		{
			num += GetFreeSpins(text, i);
		}
		return num;
	}

	public void UseFreeSpin(string groupName, int boxId, Action<string, int> onComplete)
	{
		GachaBox box = Instance.GetBox(groupName, boxId);
		EB.Debug.Log("GachaManager > UseFreeSpin > Initializing: " + Hub.Instance.GachaManager.ToString());
		if (boxId == GachaConfig.DefaultBoxID && groupName == GachaConfig.DefaultGroupName && VaultTimer.Instance.FreeBundleAvailable)
		{
			Hub.Instance.GachaManager.AddTokenForBox(groupName, box.Name, (string error, int updatedCount) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					EB.Debug.LogError("Can not add free token for vault: " + groupName + " [" + box.Name + "]");
					onComplete(error, -1);
				}
				else
				{
					VaultTimer.Instance.ResetVaultTimer();
					EB.Debug.Log("GachaManager > UseFreeSpin > Pre-OnCompleted (after free token): " + Hub.Instance.GachaManager.ToString());
					onComplete(null, updatedCount);
				}
			});
		}
		else
		{
			EB.Debug.Log("GachaManager > UseFreeSpin > Pre-OnCompleted (no free token): " + GetFreeSpins(groupName, boxId) + " = " + Hub.Instance.GachaManager.ToString());
			onComplete(null, GetFreeSpins(groupName, boxId) - box.Tokens.Cost);
		}
	}

	public void Initialize()
	{
		hub = Hub.Instance;
		LanguageCode = Localizer.GetOldLanguageCode(Setup.Locale);
		BuildRewardsData();
		GetTables();
	}

	public void Restock(Action<bool> cb)
	{
		Hub.Instance.GachaManager.RebuildSets((string error) =>
		{
			if (cb != null)
			{
				cb(string.IsNullOrEmpty(error));
			}
		});
	}

	public bool IsReady()
	{
		bool flag = true;
		if (_groups != null)
		{
			foreach (GroupInfo value in _groups.Values)
			{
				flag &= value.AttractImages != null;
			}
			return flag && !string.IsNullOrEmpty(gachaTimestamp);
		}
		return false;
	}

	public List<string> GetAttractItems(string group)
	{
		List<string> list = null;
		GroupInfo value = null;
		if (_groups.TryGetValue(group, out value))
		{
			return value.AttractImages;
		}
		return new List<string>();
	}

	public GachaBox GetBox(string group, int index)
	{
		GachaBox result = null;
		GroupInfo value = null;
		if (_groups.TryGetValue(group, out value))
		{
			result = value.GetBoxByIndex(index);
		}
		return result;
	}

	public int GetNumBoxes(string group)
	{
		int result = 0;
		GroupInfo value = null;
		if (_groups.TryGetValue(group, out value))
		{
			result = value.GetNumBoxes();
		}
		return result;
	}

	private void GetTables()
	{
		EB.Debug.Log("GachaManager > GetTables > Do request ...");
		GachaConfig gachaConfig = new GachaConfig();
		string[] groups = gachaConfig.Groups;
		foreach (string key in groups)
		{
			GachaSet gachaSet = hub.GachaManager.GetGachaSet(key);
			if (gachaSet != null)
			{
				GroupInfo value = new GroupInfo(gachaSet);
				_groups[key] = value;
				gachaTimestamp = gachaSet.Version;
			}
		}
	}

	public void CheckGachaTimestamp(Action<bool> resultCb)
	{
		hub.GachaManager.GetServerTimestamp(SafeAction.Wrap(this, (string error, string timestamp) =>
		{
			if (string.IsNullOrEmpty(error))
			{
				resultCb(gachaTimestamp == timestamp);
			}
			else
			{
				resultCb(false);
			}
		}));
	}

	public GachaRewardInfo GetRewardInfo(string name)
	{
		if (rewardInfos.ContainsKey(name))
		{
			return rewardInfos[name];
		}
		EB.Debug.LogError("GachaManager > GetRewardInfo > GachaRewardInfo could not be provided for a reward called '{0}'", name);
		return null;
	}

	public void DoGachaPick(string group, string boxName, int hardPaid, int softPaid, string payment, Action<GachaResult, int, List<GachaPossiblePrize>> resultCallback)
	{
		List<GachaPossiblePrize> randomPicks = null;
		Action<GachaResult, int> randomPickCallback = SafeAction.Wrap(this, (GachaResult gachaResult, int xpAwarded) =>
		{
			GachaSet gachaSet = hub.GachaManager.GetGachaSet(GachaConfig.DefaultGroupName);
			if (gachaSet != null)
			{
				List<GachaPossiblePrize> possiblePrizes = gachaSet.PossiblePrizes;
				if (Profile.Instance.IsTutorialDone("vault"))
				{
					GachaBox box = gachaSet.GetBox(boxName);
					if (box != null)
					{
						possiblePrizes = box.PossiblePrizes;
					}
				}
				if (possiblePrizes != null)
				{
					possiblePrizes.Shuffle();
					randomPicks = possiblePrizes.GetRange(0, Mathf.Min(Tuning.Instance.Gacha_Spin_PrespinCount, possiblePrizes.Count));
					while (randomPicks.Count < Tuning.Instance.Gacha_Spin_PrespinCount)
					{
						randomPicks.AddRange(possiblePrizes.GetRange(0, Mathf.Min(Tuning.Instance.Gacha_Spin_PrespinCount - randomPicks.Count, possiblePrizes.Count)));
					}
				}
			}
			resultCallback(gachaResult, xpAwarded, randomPicks);
		});
		GroupInfo value = null;
		if (_groups.TryGetValue(group, out value))
		{
			hub.GachaManager.PickFromBox(group, value.Set, boxName, payment, (string error, GachaPickResult pickResult) =>
			{
				GachaResult gachaResult = null;
				int obj = 0;
				if (string.IsNullOrEmpty(error) && pickResult != null)
				{
					gachaResult = new GachaResult(pickResult);
					EB.Debug.Log(string.Concat("GachaManager > DoGachaPick > Awarded item was: ", gachaResult.rewardType, " x", gachaResult.rewardCount, " [", gachaResult.rewardCategory, "]"));
					string text = gachaResult.rewardType.Split('.')[0];
					if (text == "sc")
					{
						Profile.Instance.AddSoftCurrency(gachaResult.rewardCount);
					}
					else if (text == "skillpoint")
					{
						AttributesManager.instance.AddSkillPoints(gachaResult.rewardCount);
					}
					Hub.Instance.InventoryManager.Sync();
					Profile.Instance.SpentHardCurrency(hardPaid);
					int num = softPaid - pickResult.SoftCurrentToPay;
					if (num != 0)
					{
						Profile.Instance.AddSoftCurrency(num);
					}
					obj = pickResult.XpToGive;
				}
				randomPickCallback(gachaResult, obj);
			});
		}
		else
		{
			EB.Debug.LogError("Can not get set: " + group);
			resultCallback(null, 0, null);
		}
	}

	private void BuildRewardsData()
	{
		if (rewardInfos != null)
		{
			return;
		}
		rewardInfos = new Dictionary<string, GachaRewardInfo>();
		rewardInfos["sc"] = new GachaRewardInfo("ID_UI_CURRENCY_STORE_SOFT_DESCRIPTION", "currency_ui/gacha_rivets.jpg", GachaResult.RewardCategory.SoftCurrency);
		rewardInfos["skillpoint"] = new GachaRewardInfo("ID_UI_SKILLPOINTS", "currency_ui/gacha_skillpoints_token.png", GachaResult.RewardCategory.SkillPoints);
		string text = (Profile.Instance.IsTutorialDone("vault") ? GachaConfig.DefaultGroupName : GachaConfig.DefaultTutorialGroupName);
		GachaSet gachaSet = Hub.Instance.GachaManager.GetGachaSet(text);
		if (gachaSet != null)
		{
			foreach (GachaBox box in gachaSet.Boxes)
			{
				string path = "currency_ui/gacha_" + box.Token + ".png";
				rewardInfos[box.Token] = new GachaRewardInfo(box.LocalizedDisplayName(Setup.Locale), path, GachaResult.RewardCategory.GachaSpin);
			}
		}
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		foreach (InventoryDB.MetaData item in categoryList)
		{
			string key = StringUtil.SafeKey(item.name);
			string friendlyName = item.friendlyName;
			string uiTexturePath = item.uiTexturePath;
			rewardInfos[key] = new GachaRewardInfo(friendlyName, uiTexturePath, GachaResult.RewardCategory.Weapon);
		}
		categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Boost);
		foreach (InventoryDB.MetaData item2 in categoryList)
		{
			string key2 = StringUtil.SafeKey(item2.name);
			string friendlyName2 = item2.friendlyName;
			string uiTexturePath2 = item2.uiTexturePath;
			rewardInfos[key2] = new GachaRewardInfo(friendlyName2, uiTexturePath2, GachaResult.RewardCategory.Boost);
		}
		List<CharacterDB.MetaData> category = CharacterDB.GetCategory(CharacterDB.Category.kHead);
		foreach (CharacterDB.MetaData item3 in category)
		{
			string key3 = StringUtil.SafeKey(item3.name);
			string friendlyName3 = item3.friendlyName;
			string path2 = "cac_ui_parts/" + item3.cacTexturePath;
			rewardInfos[key3] = new GachaRewardInfo(friendlyName3, path2, GachaResult.RewardCategory.Cac);
		}
		List<CharacterDB.MetaData> category2 = CharacterDB.GetCategory(CharacterDB.Category.kTorso);
		foreach (CharacterDB.MetaData item4 in category2)
		{
			string key4 = StringUtil.SafeKey(item4.name);
			string friendlyName4 = item4.friendlyName;
			string path3 = "cac_ui_parts/" + item4.cacTexturePath;
			rewardInfos[key4] = new GachaRewardInfo(friendlyName4, path3, GachaResult.RewardCategory.Cac);
		}
		List<CharacterDB.MetaData> category3 = CharacterDB.GetCategory(CharacterDB.Category.kLegs);
		foreach (CharacterDB.MetaData item5 in category3)
		{
			string key5 = StringUtil.SafeKey(item5.name);
			string friendlyName5 = item5.friendlyName;
			string path4 = "cac_ui_parts/" + item5.cacTexturePath;
			rewardInfos[key5] = new GachaRewardInfo(friendlyName5, path4, GachaResult.RewardCategory.Cac);
		}
	}

	private void Awake()
	{
		_this = this;
	}
}
