using System;
using System.Reflection;
using EB;
using EB.Sparx;
using UnityEngine;

public class Tuning : MonoBehaviour
{
	[System.Serializable]
	[EB.Serializable(Tag = "pointreward")]
	public class PointReward
	{
		[EB.Serializable(Tag = "m")]
		public GameAttributes.GameModeType GameMode;

		[EB.Serializable(Tag = "r")]
		public float rivetMultiplier;

		[EB.Serializable(Tag = "x")]
		public float xpMultiplier;
	}

	[System.Serializable]
	[EB.Serializable(Tag = "placereward")]
	public class PlaceReward
	{
		[EB.Serializable(Tag = "m")]
		public GameAttributes.GameModeType GameMode;

		[EB.Serializable(Tag = "r")]
		public int[] rivetBonus = new int[4];

		[EB.Serializable(Tag = "x")]
		public int[] xpBonus = new int[4];
	}

	[System.Serializable]
	[EB.Serializable(Tag = "levelinfo")]
	public class LevelInfo
	{
		[EB.Serializable(Tag = "x")]
		public int XP;

		[EB.Serializable(Tag = "b")]
		public int Bucket;

		[EB.Serializable(Tag = "r")]
		public LevelReward[] Rewards;

		public int Level { get; set; }

		public string Name
		{
			get
			{
				return Localizer.GetString("ID_LEVEL_" + Level);
			}
		}
	}

	public enum LevelRewardType
	{
		SoftCurrency = 0,
		HardCurrency = 1,
		Inventory = 2,
		SkillPoints = 3,
		None = 4
	}

	[System.Serializable]
	[EB.Serializable(Tag = "levelreward")]
	public class LevelReward
	{
		[EB.Serializable(Tag = "t")]
		public LevelRewardType type = LevelRewardType.None;

		[EB.Serializable(Tag = "q")]
		public int quantity;

		[EB.Serializable(Tag = "n")]
		public string name;
	}

	public const int MaxPlayers = 4;

	public float Player_JumpHeight = 7.5f;

	public float Player_RunSpeed = 7f;

	public float Player_Height = 32f;

	public float Player_FlySpeed = 7f;

	public float Player_CollisionHeightFrac = 0.66f;

	public float Player_CollisionWidthFrac = 0.66f;

	public float Player_MaxStepHeight = 10f;

	public float Player_KillFloor = -50f;

	public float Player_SpecialIdleWaitMin = 4f;

	public float Player_SpecialIdleWaitMax = 7f;

	public float Player_PlayStunnedAtPercentHealth = 25f;

	public float Player_RespawnDelay = 4f;

	public float Player_MinFallHeight = 10f;

	public float Player_MaxFallHeight = 60f;

	public float Player_FallDamageMultiplier = 10f;

	public float Player_DrownPeriod = 3f;

	public float Control_MinPowerActivationDeflection = 0.2f;

	public float Control_MaxPowerDeactiavationDeflection = 0.1f;

	public float Control_PowerMeterDisplayScale = 10f;

	public float Control_PowerMeterRotationCatchup = 0.02f;

	public float Control_StopMovingMinAngle = 60f;

	public float Control_StopMovingMinDeflection = 0.5f;

	public float Control_StartMovingMaxAngle = 60f;

	public float Control_StartMovingMinDeflection = 0.25f;

	public float Control_StopJumpingMaxAngle = 30f;

	public float Control_StopJumpingMaxDeflection = 0.25f;

	public float Control_StartJumpingMinAngle = 45f;

	public float Control_StartJumpingMinDeflection = 0.5f;

	public float Control_LateralDeadZone = 0.25f;

	public float Camera_OrthoFracMin = 0.3f;

	public float Camera_OrthoFracDefault = 0.65f;

	public float Camera_OrthoFracMax = 0.975f;

	public float Camera_OrthoFracMinPhone = 0.1f;

	public float Camera_OrthoFracDefaultPhone = 0.25f;

	public float Camera_OrthoFracMaxPhone = 0.3f;

	public float Camera_OrthoZoomGap = 0.3f;

	public float Camera_OrthoFracCatchup = 3f;

	public float Camera_ZoomSensitivity = 0.005f;

	public float Camera_TruckSensitivity = 0.5f;

	public bool Camera_ZoomOutOnFire;

	public bool Camera_ZoomOnGap = true;

	public bool Camera_ReturnToLocalOnGap = true;

	public bool Camera_KeepPlayerOnScreenDuringAim = true;

	public bool Camera_CenterPlayerOnMove = true;

	public Vector2 Camera_KeepOnScreenZone = new Vector2(0.85f, 0.75f);

	public float Camera_Intro_X_Time = 3f;

	public float Camera_Intro_Y_Time = 2f;

	public float Camera_Intro_Z_Time = 3f;

	public float Gacha_Attract_TimeToShowItem = 3f;

	public float Gacha_Attract_TimeBetweenItems = 0.3f;

	public float Gacha_Attract_FadeDuration = 0.15f;

	public float Gacha_Table_ColorBlendTime = 0.1f;

	public float Gacha_Token_ScaleDownTime = 0.1f;

	public float Gacha_Token_ScaleUpTime = 0.1f;

	public float Gacha_Token_LerpTime = 0.2f;

	public int Gacha_Spin_PrespinCount = 16;

	public int Gacha_Spin_PrespinsAtFullSpeed = 8;

	public float Gacha_Spin_PauseForEachReward = 0.05f;

	public float Gacha_Spin_PauseBetweenEachReward = 0.05f;

	public float Gacha_Spin_RewardFadeTime = 0.03f;

	public float Gacha_Spin_PauseAndFadeTimeScale = 1.5f;

	public float Gacha_Spin_FinalRewardPauseTime = 2.5f;

	public float Gacha_Spin_FinalRewardFadeOutTime = 0.2f;

	public float Gacha_Spin_RewardFlyOffTime = 0.3f;

	public float Gacha_Restock_WaitTime = 3f;

	public float Outro_Camera_PlayerZoom = 0.3f;

	public float Outro_Camera_HoldTime = 10f;

	public float Outro_Camera_HoldTime1_Team = 5f;

	public float Outro_Camera_HoldTime2_Team = 5f;

	public float Outro_CelebMsg_StartDelay = 2f;

	public string Outro_CelebAnimName = "bot_1st_place_celeb01";

	public float Outro_Camera_OrthoFracCatchup = 0.2f;

	public float World_Gravity = -50f;

	public float World_DurationTillStop = 0.2f;

	public float World_Brake = 0.95f;

	public float World_DistanceThreshold = 8f;

	public float Body_MinDamageSpeed = 5f;

	public float Body_MaxDamageSpeed = 10f;

	public float Body_DamageMultiplier = 10f;

	public float Timer_Countdown = 5f;

	public float Timer_Countdown2Player = 5f;

	public float Timer_Gap = 1f;

	public float Timer_Fire_Truncate = 4f;

	public float Timer_Countdown_AI = 10f;

	public float Timer_Countdown_AI_DeathMatch = 10f;

	public float Timer_Countdown_AI_TeamDeathMatch = 10f;

	public float Timer_Countdown_AI_KillCount = 10f;

	public float Timer_Countdown_AI_SuddenDeath = 10f;

	public float Timer_DeathMatch = 60f;

	public float Timer_TutorialDeathMatch = 180f;

	public float Timer_ShowYourTurn = 2f;

	public bool Timer_ShowOthersTurn = true;

	public float Timer_CratePickup = 3f;

	public float Timer_Multiplier = 3f;

	public float Timer_SuddenDeath = 300f;

	public float Timer_KillCount = 300f;

	public bool TrialPay_Enabled = true;

	public string TrialPay_Countries = "all";

	public int TrialPay_Time = 604800;

	public float Chat_MessageDuration = 10f;

	public float Chat_MessageFadeTime = 2f;

	public float Loot_PreSuckPeriod = 2f;

	public float DeathMessage_Duration = 3f;

	public int DefaultRivets = 2000;

	public float HUD_KeySizeRelativeToPlayer = 0.75f;

	public int Scoring_Points_Kill = 250;

	public int Scoring_Points_Suicide = -250;

	public float Scoring_Bonus_Multiplier_Timer = 2f;

	public int Scoring_Bonus_Multiplier_Max = 5;

	public float Scoring_Championship_SuddenDeath_TimerBonusMultiplier = 100f;

	public float Scoring_KillCountTo_Win = 5f;

	public float BackToLobbyTimer = 10f;

	public float LevelUpTimer = 5f;

	public float TutorialDoneTimer = 5f;

	public float NewHighScoreTimer = 3f;

	public float GameStartTimer = 15f;

	public float FullPlayersTimer = 5f;

	public float LockoutTime = 3f;

	public float ProjectileIndicatorZDepth = -10f;

	public float bigarrow_uiAmplitude = 30f;

	public float bigarrow_uiSpeed = 5f;

	public float bigarrow_uiSquish = 0.05f;

	public float bigarrow_uiScale = 2f;

	public float bigarrow_worldScale = 1f;

	public float Respawn_MinDistance = 2f;

	public float UpgradeUI_TimeToDoLerp = 0.3f;

	public float UpgradeUI_TimeToDoSineWave = 2f;

	public float UpgradeUI_TimeBetweenWheelAndBar = 0.5f;

	public float UpgradeUI_NumSineWaveOscillations = 4f;

	public float UpgradeUI_ColorSwapDuration = 0.5f;

	public float UpgradeUI_TimeToShowResponsePanel = 2.5f;

	public float UpgradeUI_SpinTimeBeforeDecay = 2f;

	public float UpgradeUI_SpinVelocity = 1f;

	public float UpgradeUI_SpinVelocityDecay = 0.98f;

	public float UpgradeUI_SpinMinVelocity = 0.25f;

	public Color UpgradeUI_UpgradeColor = Color.white;

	public Vector3 UpgradeUI_WheelTop;

	public Vector3 UpgradeUI_WheelBottom;

	public float[] UpgradeBracketStringTiers = new float[0];

	public int Vault_TimerIntervalSeconds = 300;

	public int Vault_TimerExpiryMinutesReminder = 60;

	public float Chest_ChanceToSpawn = 0.25f;

	public float Chest_ChanceToSpawnChampionship = 0.1f;

	public bool Matchmaking_EnableRandom;

	public bool Matchmaking_EnableLevelSelect;

	public bool Matchmaking_EnableTeamDeathMatch;

	public bool Matchmaking_EnableKillCount;

	public bool Matchmaking_EnableSuddenDeath;

	public Color ColorTint_Owned = Color.red;

	public Color ColorTint_Store = Color.cyan;

	public Color ColorTint_Tier1 = Color.gray;

	public Color ColorTint_Tier2 = Color.yellow;

	public Color ColorTint_Tier3 = Color.green;

	public Color ColorTint_Tier4 = Color.magenta;

	public Color ColorTint_Tier5 = Color.red;

	public Color ColorTint_Tier6 = Color.white;

	public Color ColorTint_Championship = Color.red;

	public Color ColorTint_Epic = new Color(45f / 64f, 0.375f, 247f / 256f);

	public string RateInventoryObject = "The NBB";

	public int RateInventoryCount = 3;

	public float StoreUnlock_RemovePadlockFaderDelay;

	public Vector3 DamageTextOffset = new Vector3(0f, 75f, 0f);

	public float KeyDamageRespawnThreshold = 1000000f;

	public int GrantEnergyInSecs = 3600;

	public bool NoEnergyShowMultiplayer = true;

	public int EnergyStartMax = 3;

	public int EnergyUpgradedMax = 5;

	public int EnergyRefillCost = 20;

	public int EnergyUpgradeCost = 100;

	public int DailyRewardsResetTime24HUTC = 9;

	public float ImpulseWeaponSalePurchaseDiscount = 0.15f;

	public float ImpulseWeaponSaleUnlockDiscount = 0.15f;

	public int ImpulseWeaponPopupDurationSecs = 20;

	public float RevengeSaleDiscount = 0.3f;

	public int RevengeSalePopupDurationSecs = 20;

	public int RevengeSaleGapNumberOfGames = 5;

	public float RevengeSaleProbability = 0.2f;

	public float RevengeSaleMinimumGameTimeSecs = 30f;

	public int RevengeSaleMaxFrequencyPerGame = 2;

	public float MaxInaccuracyforBotsBronze = 60f;

	public float MaxInaccuracyforBotsSilver = 30f;

	public float MaxInaccuracyforBotsGold = 10f;

	public int ConsecutiveChampionshipLossesForContextualPopups = 5;

	public int ChampionshipFacebookGlobalLeaderboardCountMultiplier = 10;

	public int ItemLevelling_MaxLevel = 5;

	public int[] ItemLevelling_HudRange = new int[3] { 0, 3, 7 };

	public int NameChangeCost = 50;

	public int HiddenSoftCostThreshold = 999999;

	public float ExtraFloat1;

	public float ExtraFloat2;

	public float ExtraFloat3;

	public float ExtraFloat4;

	public float ExtraFloat5;

	public float ExtraFloat7;

	public float ExtraFloat8;

	public float ExtraFloat9;

	public float ExtraFloat10;

	public AttributesManager.AttributesTunable SkillPointsData;

	public PointReward[] PointRewardTable = new PointReward[4];

	public PlaceReward[] PlaceRewardTable = new PlaceReward[4];

	public LevelInfo[] Levels = new LevelInfo[12]
	{
		new LevelInfo
		{
			Bucket = 1
		},
		new LevelInfo
		{
			XP = 1000,
			Bucket = 1
		},
		new LevelInfo
		{
			XP = 2000,
			Bucket = 1
		},
		new LevelInfo
		{
			XP = 3000,
			Bucket = 1
		},
		new LevelInfo
		{
			XP = 4000,
			Bucket = 2
		},
		new LevelInfo
		{
			XP = 5000,
			Bucket = 2
		},
		new LevelInfo
		{
			XP = 6000,
			Bucket = 2
		},
		new LevelInfo
		{
			XP = 7000,
			Bucket = 2
		},
		new LevelInfo
		{
			XP = 8000,
			Bucket = 3
		},
		new LevelInfo
		{
			XP = 9000,
			Bucket = 3
		},
		new LevelInfo
		{
			XP = 10000,
			Bucket = 3
		},
		new LevelInfo
		{
			XP = 11000,
			Bucket = 3
		}
	};

	[NonSerialized]
	public Color[] TeamColors = new Color[4]
	{
		new Color32(237, 28, 36, byte.MaxValue),
		new Color32(55, 119, 188, byte.MaxValue),
		new Color32(253, 185, 19, byte.MaxValue),
		new Color32(80, 184, 72, byte.MaxValue)
	};

	public int IdleTurnKickCount
	{
		get
		{
			return (int)ExtraFloat1;
		}
	}

	public static Tuning Instance { get; private set; }

	[HideInInspector]
	private void Awake()
	{
		if (Instance != null)
		{
			EB.Debug.LogError("Multiple tuning classes! Did you add the script to setup?");
		}
		Instance = this;
		for (int i = 0; i < Levels.Length; i++)
		{
			Levels[i].Level = i + 1;
		}
		_Lock();
	}

	public LevelInfo GetLevelInfo(int level)
	{
		level = Mathf.Clamp(level - 1, 0, Levels.Length - 1);
		return Levels[level];
	}

	public void LoadTuningData()
	{
		_Unlock();
		TuningManager tuningManager = Hub.Instance.TuningManager;
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType.IsPrimitive)
			{
				string text = "tuning." + StringUtil.SafeKey(fieldInfo.Name);
				tuningManager.Apply(this, fieldInfo, text);
			}
			else if (fieldInfo.Name == "Levels")
			{
				string text2 = "levels";
				tuningManager.Apply(this, fieldInfo, text2);
			}
			else if (fieldInfo.Name == "PointRewardTable")
			{
				string text3 = "pointrewards";
				tuningManager.Apply(this, fieldInfo, text3);
			}
			else if (fieldInfo.Name == "PlaceRewardTable")
			{
				string text4 = "placerewards";
				tuningManager.Apply(this, fieldInfo, text4);
			}
			else if (fieldInfo.Name == "SkillPointsData")
			{
				string text5 = "tuning.skillpointsdata";
				tuningManager.Apply(this, fieldInfo, text5);
			}
		}
		if (DebugSettings.QuickGame)
		{
			Timer_DeathMatch = 30f;
			Timer_Countdown = 25f;
		}
		if (DebugSettings.TurboGame)
		{
			Timer_DeathMatch = 10f;
			Timer_Countdown = 10f;
		}
		_Lock();
	}

	private static void _Lock()
	{
		Memory.Lock(Instance, true);
	}

	private static void _Unlock()
	{
		Memory.Unlock(Instance);
	}

	public Color GetTierColor(int tier)
	{
		switch (tier)
		{
		case 1:
			return ColorTint_Tier1;
		case 2:
			return ColorTint_Tier2;
		case 3:
			return ColorTint_Tier3;
		case 4:
			return ColorTint_Tier4;
		case 5:
			return ColorTint_Tier5;
		case 6:
			return ColorTint_Tier6;
		default:
			return Color.white;
		}
	}
}
