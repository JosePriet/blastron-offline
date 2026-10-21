using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class Profile : MonoBehaviour, ISaveLoadInterface
{
	private enum BeginnerSaleState
	{
		NOT_STARTED = 0,
		STARTED = 1,
		EXPIRED = 2
	}

	private const int _DAILY_OFFERS_CURRENT_VERSION = 1;

	public const int MaxDailyOffers = 21;

	private SafeInt _soft = 0;

	private SafeInt _level = 1;

	private SafeInt _xp = 0;

	private SafeInt _lastXP = -1;

	private SafeInt _lastLevel = -1;

	private SafeInt _energy = 3;

	private SafeInt _energyMax = 3;

	private SafeBool _energyRefilledBefore = false;

	private SafeInt _nextEnergyAt = -1;

	private float _levelProgress;

	private bool _isLoaded;

	private Dictionary<int, float> _partUpgrades;

	private Hashtable _tutorialsDone;

	private SafeInt _sessionXP = 0;

	private SafeInt _sessionSoft = 0;

	private SafeBool _sessionHighScore = false;

	private SafeInt _sessionRevengeSales = 0;

	private SafeInt _spentSoft = 0;

	private SafeInt _spentHard = 0;

	private SafeInt _maxWeaponUnlockLevel = 0;

	private SafeInt _maxBoostUnlockLevel = 0;

	private SafeInt _deprecatedFreeLootinators = 0;

	private bool ShouldSaveConvertedDeprecatedLootinators;

	private SafeInt _hofPoints = 0;

	private SafeInt _hofLevel = 0;

	private SafeInt _bgSaleStartTime = 0;

	private SafeInt _bgSaleState = 1;

	private SafeInt _dailyOffersNextTime = 0;

	private SafeInt _dailyOffersCollected = 0;

	private SafeInt _dailyOffersVersion = 1;

	private SafeInt _revengeSaleGameCounter = 0;

	private bool _bgSaleHasSeenOnce;

	private bool _dailyOffersSeen;

	private bool _lootinatorOpenSeen;

	public bool recentRename;

	public int ConsecutiveChampionshipLosses;

	public CharacterDB.Recipe recipe;

	public List<int> sessionGachaAwards;

	public List<int> seriesGachaAwards;

	public bool RevengeSaleAlwaysShows;

	public bool GachaSpinCollectionSeen;

	public int DailyOffersPeriod = 86400;

	public static SafeBool TutorialSkip = false;

	private ArrayList _mutedUsers = new ArrayList();

	public static Profile Instance { get; private set; }

	public int SoftCurrency
	{
		get
		{
			return _soft;
		}
	}

	public int HardCurrency
	{
		get
		{
			return Hub.Instance.WalletManager.Balance;
		}
	}

	public int Level
	{
		get
		{
			return _level;
		}
	}

	public float LevelProgress
	{
		get
		{
			return _levelProgress;
		}
	}

	public Tuning.LevelInfo LevelInfo
	{
		get
		{
			return Tuning.Instance.GetLevelInfo(Level);
		}
	}

	public Tuning.LevelInfo NextLevelInfo
	{
		get
		{
			return Tuning.Instance.GetLevelInfo(Level + 1);
		}
	}

	public bool DoesNotGainXP { get; set; }

	public int XP
	{
		get
		{
			int num = _xp;
			if (num != (int)_lastXP && _isLoaded)
			{
				UpdateLevel(num);
			}
			return num;
		}
	}

	public int SessionXP
	{
		get
		{
			return _sessionXP;
		}
	}

	public int SessionSoftCurrency
	{
		get
		{
			return _sessionSoft;
		}
	}

	public bool SessionHighScoreAchieved
	{
		get
		{
			return _sessionHighScore;
		}
		set
		{
			_sessionHighScore = value;
		}
	}

	public bool BeginnerSalePopupSeen
	{
		get
		{
			return _bgSaleHasSeenOnce;
		}
		set
		{
			_bgSaleHasSeenOnce = value;
		}
	}

	public bool DailyOffersSeen
	{
		get
		{
			return _dailyOffersSeen;
		}
		set
		{
			_dailyOffersSeen = value;
		}
	}

	public int DailyOffersNextTime
	{
		get
		{
			return _dailyOffersNextTime;
		}
	}

	public int DailyOffersCollected
	{
		get
		{
			return _dailyOffersCollected;
		}
	}

	public int DailyOffersVersion
	{
		get
		{
			return _dailyOffersVersion;
		}
	}

	public bool RevengeSaleAvailable
	{
		get
		{
			if (RevengeSaleAlwaysShows)
			{
				return true;
			}
			if ((int)_revengeSaleGameCounter == Tuning.Instance.RevengeSaleGapNumberOfGames)
			{
				return (int)_sessionRevengeSales > 0 && (int)_sessionRevengeSales < Tuning.Instance.RevengeSaleMaxFrequencyPerGame;
			}
			return (int)_revengeSaleGameCounter == 0;
		}
	}

	public int HoFPoints
	{
		get
		{
			return _hofPoints;
		}
		set
		{
			_hofPoints = value;
			_hofLevel = Mathf.Max(_hofLevel, HoFData.Instance.Level(_hofPoints));
			EB.Debug.LogError(string.Format("HoF now {0} points at level {1}", _hofPoints, _hofLevel));
			Hub.Instance.SaveLoadManager.Save(this);
		}
	}

	public bool CanSave { get; private set; }

	public string Key
	{
		get
		{
			return "profile";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return ShouldSaveConvertedDeprecatedLootinators;
		}
	}

	public int NextEnergyAt
	{
		get
		{
			return _nextEnergyAt;
		}
	}

	public int Energy
	{
		get
		{
			return _energy;
		}
	}

	public int EnergyMax
	{
		get
		{
			return _energyMax;
		}
		set
		{
			_energyMax = value;
			Hashtable hashtable = new Hashtable();
			ChampionshipData.Instance.AddTelemetryEventData(hashtable);
			hashtable["current"] = _energy;
			hashtable["hardCurrency"] = Instance.HardCurrency;
			Telemetry.Event("energy_upgrade", 1, hashtable);
			Save();
		}
	}

	public bool HasRefilledEnergyBefore
	{
		get
		{
			return _energyRefilledBefore;
		}
		set
		{
			_energyRefilledBefore = value;
		}
	}

	public int GetMaxItemUnlockLevel(InventoryItem.Category cat)
	{
		switch (cat)
		{
		case InventoryItem.Category.Weapon:
			return _maxWeaponUnlockLevel;
		case InventoryItem.Category.Boost:
			return _maxBoostUnlockLevel;
		default:
			return 0;
		}
	}

	private void Awake()
	{
		Instance = this;
		_partUpgrades = new Dictionary<int, float>();
		sessionGachaAwards = new List<int>();
		seriesGachaAwards = new List<int>();
		_tutorialsDone = new Hashtable();
	}

	private void Start()
	{
		Hub.Instance.SaveLoadManager.Register(this);
		recipe = CharacterDB.GetDefaultRecipe();
	}

	public Coroutine Save()
	{
		return Hub.Instance.SaveLoadManager.Save(this);
	}

	public void FinalizeSessionData()
	{
		AddSoftCurrency(_sessionSoft);
		AddXP(_sessionXP);
		for (int i = 0; i < sessionGachaAwards.Count; i++)
		{
			GachaManager.Instance.AddFreeSpin(sessionGachaAwards[i]);
		}
	}

	public void RewardSessionGachaSpins()
	{
		for (int i = 0; i < sessionGachaAwards.Count; i++)
		{
			seriesGachaAwards.Add(sessionGachaAwards[i]);
		}
	}

	public void ClearSessionData()
	{
		_sessionSoft = 0;
		_sessionXP = 0;
		_sessionHighScore = false;
		sessionGachaAwards.Clear();
		_sessionRevengeSales = 0;
	}

	public void ClearSeriesGachaAwards()
	{
		seriesGachaAwards.Clear();
	}

	public void AddSessionGachaToken(int id)
	{
		sessionGachaAwards.Add(id);
	}

	public bool HasBeginnerSaleStarted()
	{
		return (int)_bgSaleState != 0;
	}

	public bool HasBeginnerSaleExpired()
	{
		return (int)_bgSaleState == 2 || UpdateBeginnerSaleTimeRemaining() < 0;
	}

	public int UpdateBeginnerSaleTimeRemaining()
	{
		if ((int)_bgSaleState == 2)
		{
			return 0;
		}
		int num = 259200 - (EB.Time.Now - (int)_bgSaleStartTime);
		if (num < 0)
		{
			_bgSaleState = 2;
			num = 0;
		}
		return num;
	}

	public void ExpireBeginnerSale()
	{
		if ((int)_bgSaleState != 0)
		{
			_bgSaleState = 2;
			Save();
		}
	}

	public void IncrementDailyOffersCollection()
	{
		_dailyOffersCollected += (SafeInt)1;
		if ((int)_dailyOffersCollected >= 21)
		{
			CancelExistingDailyOffersNotifications();
		}
		else
		{
			_dailyOffersNextTime = GetNextDailyOfferAvailableTimeFrom(EB.Time.Now);
			ScheduleDailyOffersNotification(EB.Time.Now);
		}
		Save();
	}

	private int GetNextDailyOfferAvailableTimeFrom(int referenceTime)
	{
		int num = 86400;
		int dailyRewardsResetTime24HUTC = Tuning.Instance.DailyRewardsResetTime24HUTC;
		int num2 = referenceTime / num * num + dailyRewardsResetTime24HUTC * 60 * 60;
		if (num2 < referenceTime)
		{
			num2 += num;
		}
		return num2;
	}

	public void ScheduleDailyOffersNotification(int refTime)
	{
		CancelExistingDailyOffersNotifications();
	}

	private void CancelExistingDailyOffersNotifications()
	{
	}

	public void OnRevengeSaleShown()
	{
		_revengeSaleGameCounter = Tuning.Instance.RevengeSaleGapNumberOfGames;
		_sessionRevengeSales += (SafeInt)1;
		Save();
	}

	public void OnGamePlayed()
	{
		_revengeSaleGameCounter = Mathf.Max(0, _revengeSaleGameCounter - 1);
	}

	public bool IsTutorialDone(string id)
	{
		if ((bool)TutorialSkip)
		{
			return true;
		}
		return Dot.Integer(id, _tutorialsDone, 0) == 1 && !DebugSettings.ForceTutorial;
	}

	public void MarkTutorialDone(string id)
	{
		object obj = _tutorialsDone[id];
		if ("mp".Equals(id) && (obj == null || !obj.Equals(1)))
		{
			_bgSaleState = 1;
			UpdateBeginnerSaleTimeRemaining();
		}
		_tutorialsDone[id] = 1;
		Save();
	}

	public void AddSessionSoftCurrency(int value)
	{
		_sessionSoft += (SafeInt)value;
	}

	public void AddSoftCurrency(int value)
	{
		_soft += (SafeInt)value;
	}

	public bool RemoveSoftCurrency(int value)
	{
		if (SoftCurrency >= value)
		{
			_soft -= (SafeInt)value;
			_spentSoft += (SafeInt)value;
			UpdateAchievementSpentSoft();
			return true;
		}
		return false;
	}

	private void UpdateAchievementSpentSoft()
	{
		double progress = (int)_spentSoft / 10;
		Hub.Instance.GameCenterManager.ReportProgress("RIVETS_1", progress);
		double progress2 = (int)_spentSoft / 50;
		Hub.Instance.GameCenterManager.ReportProgress("RIVETS_2", progress2);
		double progress3 = (int)_spentSoft / 250;
		Hub.Instance.GameCenterManager.ReportProgress("RIVETS_3", progress3);
		double progress4 = (int)_spentSoft / 500;
		Hub.Instance.GameCenterManager.ReportProgress("RIVETS_4", progress4);
		double progress5 = (int)_spentSoft / 2500;
		Hub.Instance.GameCenterManager.ReportProgress("RIVETS_5", progress5);
	}

	public void SpentHardCurrency(int value)
	{
		_spentHard += (SafeInt)value;
		UpdateAchievementSpentHard();
	}

	private void UpdateAchievementSpentHard()
	{
		double progress = (int)_spentHard * 10;
		Hub.Instance.GameCenterManager.ReportProgress("GEARS_1", progress);
		double progress2 = (int)_spentHard * 4;
		Hub.Instance.GameCenterManager.ReportProgress("GEARS_2", progress2);
		double progress3 = (int)_spentHard;
		Hub.Instance.GameCenterManager.ReportProgress("GEARS_3", progress3);
		double progress4 = (double)(int)_spentHard / 2.5;
		Hub.Instance.GameCenterManager.ReportProgress("GEARS_4", progress4);
		double progress5 = (int)_spentHard / 5;
		Hub.Instance.GameCenterManager.ReportProgress("GEARS_5", progress5);
	}

	public float GetPartUpgradeProgress(int partHash)
	{
		float value = 0f;
		if (_partUpgrades != null)
		{
			_partUpgrades.TryGetValue(partHash, out value);
		}
		return value;
	}

	public void SetPartUpgradeProgress(int partHash, float progress)
	{
		if (progress < 0f || progress > 100f)
		{
			EB.Debug.LogError("Error: invalid progress value of " + progress + "!!! Aborting!");
			return;
		}
		_partUpgrades[partHash] = progress;
		CharacterDB.MetaData metaData = CharacterDB.GetMetaData(partHash);
		recipe.SetPart(metaData, progress);
	}

	public void ResetPartUpgradeProgress()
	{
		_partUpgrades.Clear();
		Save();
	}

	public float GetRobotAttributeValue(CharacterDB.Category category, RobotAttribute.RobotAttributeType attributeType)
	{
		float result = 0f;
		if ((int)category < recipe.parts.Length)
		{
			int part = recipe.GetPart(category);
			float partUpgradeProgress = GetPartUpgradeProgress(part);
			result = PartManager.Instance.GetPartAttributeValue(part, attributeType, partUpgradeProgress);
		}
		return result;
	}

	public float GetRobotAttributeValue(RobotAttribute.RobotAttributeType attributeType)
	{
		float num = 0f;
		int[] parts = recipe.parts;
		foreach (int partHash in parts)
		{
			float partUpgradeProgress = GetPartUpgradeProgress(partHash);
			num += PartManager.Instance.GetPartAttributeValue(partHash, attributeType, partUpgradeProgress);
		}
		return num;
	}

	public void AddSessionXP(int value)
	{
		if (value != 0)
		{
			_sessionXP += (SafeInt)value;
		}
	}

	public void AddXP(int value)
	{
		if (value != 0 && !DoesNotGainXP)
		{
			_xp += (SafeInt)value;
		}
	}

	public void ClearXP()
	{
		_xp = 0;
	}

	public void UpdateTuningData()
	{
		_lastLevel = -1;
		UpdateLevel(_xp);
	}

	public void SetNextMaxUnlockLevel(InventoryItem.Category cat, int levelToStart)
	{
		int num = levelToStart;
		while (!InventoryDB.HasUnlockableItemOfType(cat, num))
		{
			num++;
			if (num >= Tuning.Instance.Levels.Length)
			{
				break;
			}
		}
		switch (cat)
		{
		case InventoryItem.Category.Weapon:
			_maxWeaponUnlockLevel = num;
			break;
		case InventoryItem.Category.Boost:
			_maxBoostUnlockLevel = num;
			break;
		}
		Save();
	}

	private void UpdateLevel(int newXP)
	{
		Tuning.LevelInfo[] levels = Tuning.Instance.Levels;
		int num = _lastLevel;
		_level = GetLevelFromXP(newXP);
		if ((int)_level >= (int)_maxWeaponUnlockLevel)
		{
			SetNextMaxUnlockLevel(InventoryItem.Category.Weapon, _level + 1);
			UnityEngine.Debug.LogWarning("Level now higher than the max visible weapon unlock! Raising the max visible unlock to " + _maxWeaponUnlockLevel);
		}
		if ((int)_level >= (int)_maxBoostUnlockLevel)
		{
			SetNextMaxUnlockLevel(InventoryItem.Category.Boost, _level + 1);
			UnityEngine.Debug.LogWarning("Level now higher than the max visible boost unlock! Raising the max visible unlock to " + _maxBoostUnlockLevel);
		}
		if (!InventoryDB.HasUnlockableItemOfType(InventoryItem.Category.Weapon, _maxWeaponUnlockLevel))
		{
			SetNextMaxUnlockLevel(InventoryItem.Category.Weapon, _maxWeaponUnlockLevel + 1);
		}
		if (!InventoryDB.HasUnlockableItemOfType(InventoryItem.Category.Boost, _maxBoostUnlockLevel))
		{
			SetNextMaxUnlockLevel(InventoryItem.Category.Boost, _maxBoostUnlockLevel + 1);
		}
		if ((int)_level >= levels.Length)
		{
			_levelProgress = 1f;
			_lastXP = newXP;
			return;
		}
		float num2 = newXP - levels[Level - 1].XP;
		float num3 = levels[Level].XP - levels[Level - 1].XP;
		_levelProgress = Mathf.Clamp01(num2 / num3);
		_lastXP = newXP;
		EB.Debug.Log("XP: " + newXP + " UpdateLevel: " + Level + " Progress: " + LevelProgress);
		Telemetry.UserLevel = Level;
		if (Level > num && num >= 0)
		{
			Hub.Instance.LeaderboardManager.ReportScore(1, string.Empty, LeaderboardManager.UpdateType.Set, Level);
			UpdateAchievement();
			Telemetry.Event("levelup");
			Telemetry.Flush();
		}
		_lastLevel = Level;
	}

	private void UpdateAchievement()
	{
		double progress = (Level - 1) / 4 * 100;
		Hub.Instance.GameCenterManager.ReportProgress("LEVEL_1", progress);
		double progress2 = (Level - 1) / 14 * 100;
		Hub.Instance.GameCenterManager.ReportProgress("LEVEL_2", progress2);
		double progress3 = (Level - 1) / 29 * 100;
		Hub.Instance.GameCenterManager.ReportProgress("LEVEL_3", progress3);
		double progress4 = (Level - 1) / 44 * 100;
		Hub.Instance.GameCenterManager.ReportProgress("LEVEL_4", progress4);
	}

	public static int GetLevelFromXP(int xp)
	{
		int a = 0;
		Tuning.LevelInfo[] levels = Tuning.Instance.Levels;
		for (int i = 0; i < levels.Length && xp >= levels[i].XP; i++)
		{
			a = i;
		}
		return Mathf.Min(a, levels.Length - 1) + 1;
	}

	public int HoFLevel()
	{
		if (HoFData.Instance != null)
		{
			_hofLevel = HoFData.Instance.Level(_hofPoints);
		}
		return _hofLevel;
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		Hashtable serializedRecipe = recipe.ToHashtable();
		data["recipe"] = serializedRecipe;
		data["coins"] = (int)_soft;
		data["coinsSpent"] = (int)_spentSoft;
		data["hardSpent"] = (int)_spentHard;
		data["xp"] = (int)_xp;
		data["freeltor"] = 0;
		data["maxWeaponUnlock"] = (int)_maxWeaponUnlockLevel;
		data["maxBoostUnlock"] = (int)_maxBoostUnlockLevel;
		data["bgSaleState"] = (int)_bgSaleState;
		data["bgSaleStartTime"] = (int)_bgSaleStartTime;
		data["dailyOffersNextTime"] = (int)_dailyOffersNextTime;
		data["dailyOffersCollected"] = (int)_dailyOffersCollected;
		data["dailyOffersVersion"] = (int)_dailyOffersVersion;
		data["revengeSaleCounter"] = (int)_revengeSaleGameCounter;
		data["hofPoints"] = (int)_hofPoints;
		data["hofLevel"] = (int)_hofLevel;
		data["energy"] = (int)_energy;
		data["energyMax"] = (int)_energyMax;
		data["energyRefilledBefore"] = (bool)_energyRefilledBefore;
		data["nextEnergyAt"] = (int)_nextEnergyAt;
		Hashtable partUpgrades = new Hashtable(_partUpgrades);
		data["upgrade"] = partUpgrades;
		data["tutorial"] = _tutorialsDone;
		data["mutedusers"] = _mutedUsers;
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		_lastLevel = -1;
		recipe = new CharacterDB.Recipe(data["recipe"]);
		_soft = Dot.Integer("coins", data, Tuning.Instance.DefaultRivets);
		_spentSoft = Dot.Integer("coinsSpent", data, 0);
		_spentHard = Dot.Integer("hardSpent", data, 0);
		_xp = Dot.Integer("xp", data, 0);
		_maxWeaponUnlockLevel = Dot.Integer("maxWeaponUnlock", data, 0);
		_maxBoostUnlockLevel = Dot.Integer("maxBoostUnlock", data, 0);
		_partUpgrades = new Dictionary<int, float>();
		if (data["upgrade"] != null)
		{
			Hashtable partUpgrades = Dot.Object("upgrade", data, new Hashtable());
			if (partUpgrades != null)
			{
				foreach (DictionaryEntry partUpgrade in partUpgrades)
				{
					int partHash = 0;
					Serialization.Deserialize(partUpgrade.Key, ref partHash);
					float progress = 0f;
					Serialization.Deserialize(partUpgrade.Value, ref progress);
					EB.Debug.Log("adding key = " + partHash + ", value = " + progress);
					_partUpgrades[partHash] = progress;
				}
			}
		}
		_deprecatedFreeLootinators = Dot.Integer("freeltor", data, 0);
		ShouldSaveConvertedDeprecatedLootinators = (int)_deprecatedFreeLootinators > 0;
		_bgSaleState = Dot.Integer("bgSaleState", data, 0);
		_bgSaleStartTime = Dot.Integer("bgSaleStartTime", data, EB.Time.Now);
		int defaultDailyOffersNextTime = GetNextDailyOfferAvailableTimeFrom(EB.Time.Now);
		_dailyOffersNextTime = Dot.Integer("dailyOffersNextTime", data, defaultDailyOffersNextTime);
		int oldStartTime = Dot.Integer("dailyOffersStartTime", data, EB.Time.Now);
		if ((int)_dailyOffersNextTime == defaultDailyOffersNextTime)
		{
			int nextTime = GetNextDailyOfferAvailableTimeFrom(oldStartTime);
			EB.Debug.Log("loading old save format for daily offers timing, massaging: old start time: " + EB.Time.FromPosixTime(oldStartTime).ToString("u") + ", new nextTime: " + EB.Time.FromPosixTime(nextTime).ToString("u"));
			_dailyOffersNextTime = nextTime;
			ScheduleDailyOffersNotification(nextTime);
		}
		_dailyOffersCollected = Dot.Integer("dailyOffersCollected", data, 0);
		_dailyOffersVersion = Dot.Integer("dailyOffersVersion", data, -1);
		if ((int)_dailyOffersVersion == -1)
		{
			EB.Debug.Log("old daily offers save version, resetting to beginning");
			_dailyOffersCollected = 0;
			_dailyOffersVersion = 1;
		}
		_revengeSaleGameCounter = Dot.Integer("revengeSaleCounter", data, 0);
		_sessionRevengeSales = 0;
		_hofPoints = Dot.Integer("hofPoints", data, 0);
		_hofLevel = Dot.Integer("hofLevel", data, 0);
		Hashtable tutorials = Dot.Object("tutorial", data, null);
		if (tutorials != null)
		{
			foreach (DictionaryEntry tutorialId in tutorials)
			{
				_tutorialsDone[tutorialId.Key] = tutorialId.Value;
			}
		}
		else
		{
			_tutorialsDone = new Hashtable();
		}
		CharacterManager.Instance.CreateLocalCharacter(recipe);
		_mutedUsers = Dot.Array("mutedusers", data, new ArrayList());
		_isLoaded = true;
		UpdateLevel(_xp);
		if (!recentRename)
		{
			_bgSaleHasSeenOnce = false;
			_dailyOffersSeen = false;
			_lootinatorOpenSeen = false;
		}
		else
		{
			recentRename = false;
		}
		_energyMax = Dot.Integer("energyMax", data, 3);
		_energy = Dot.Integer("energy", data, _energyMax);
		_energyRefilledBefore = Dot.Bool("energyRefilledBefore", data, false);
		_nextEnergyAt = Dot.Integer("nextEnergyAt", data, -1);
		ConsecutiveChampionshipLosses = 0;
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		CanSave = true;
		if (ShouldSaveConvertedDeprecatedLootinators)
		{
			while ((int)_deprecatedFreeLootinators > 0)
			{
				--_deprecatedFreeLootinators;
				GachaManager.Instance.AddFreeSpin(GachaManager.GachaConfig.DefaultBoxID);
			}
		}
		yield break;
	}

	public void RestoreEnergy(int count, int resetTimeTo)
	{
		_energy = Mathf.Clamp(_energy + count, 0, _energyMax);
		_nextEnergyAt = resetTimeTo;
		Save();
	}

	public void FullyRestoreEnergy()
	{
		Hashtable hashtable = new Hashtable();
		ChampionshipData.Instance.AddTelemetryEventData(hashtable);
		hashtable["current"] = _energy;
		hashtable["hardCurrency"] = Instance.HardCurrency;
		Telemetry.Event("energy_refill", 1, hashtable);
		_energy = _energyMax;
		Save();
	}

	public void ConsumeEnergy(int count)
	{
		_energy = Mathf.Clamp(_energy - count, 0, _energyMax);
		if ((int)_energy == 0)
		{
			Hashtable hashtable = new Hashtable();
			ChampionshipData.Instance.AddTelemetryEventData(hashtable);
			hashtable["current"] = 0;
			hashtable["hardCurrency"] = Instance.HardCurrency;
			Telemetry.Event("energy_out", 1, hashtable);
		}
		if ((int)_nextEnergyAt <= 0)
		{
			_nextEnergyAt = EB.Time.Now + Tuning.Instance.GrantEnergyInSecs;
		}
		Save();
	}

	public void MuteUser(Id userID, bool bMute)
	{
		if (bMute && !UserMuted(userID.Value))
		{
			_mutedUsers.Add((double)userID.Value);
		}
		else
		{
			_mutedUsers.Remove((double)userID.Value);
		}
		Save();
	}

	public bool UserMuted(double userID)
	{
		return _mutedUsers.Contains(userID);
	}

	public void ClearMutedUsers()
	{
		_mutedUsers.Clear();
		Save();
	}
}
