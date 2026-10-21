using System;
using System.Collections;
using EB;
using EB.Sparx;

public static class GameAttributes
{
	public enum AttributeType
	{
		Level = 0,
		Mode = 1,
		GameMode = 2,
		Version = 3,
		CampaignData = 4,
		Order = 5,
		AuxFlags = 6,
		Championship = 7
	}

	public enum ModeType
	{
		MP = 0,
		SP = 1,
		CHAMPIONSHIP = 2
	}

	public enum CampaignGameModeType
	{
		Challenge = 0,
		BotMatch = 1
	}

	public enum GameModeType
	{
		DeathMatch = 0,
		TeamDeathMatch = 1,
		KillCount = 2,
		SuddenDeath = 3,
		Tutorial = 4,
		Any = -1
	}

	[Flags]
	public enum AuxFlagsType
	{
		kNone = 0,
		kTutorialOverride = 1,
		kResetable = 2,
		kDetonationVis = 4,
		kInfiniteAmmo = 8
	}

	public enum ChampionshipType
	{
		kOff = 0,
		kOn = 1
	}

	public static string[] GameModeTitle = new string[5] { "ID_GAME_ATTRIBUTE_GAMEMODE_DEATHMATCH", "ID_GAME_ATTRIBUTE_GAMEMODE_TEAMDEATHMATCH", "ID_GAME_ATTRIBUTE_GAMEMODE_KILLCOUNT", "ID_GAME_ATTRIBUTE_GAMEMODE_SUDDENDEATH", "ID_GAME_ATTRIBUTE_GAMEMODE_ANY" };

	public static string[] GameModeIcon = new string[5] { "icon_deathmatch", "icon_teamdeathmatch", "icon_killcount", "icon_suddendeath", "icon_randomdeath" };

	public static int GetTeamCount(GameModeType type)
	{
		if (type == GameModeType.TeamDeathMatch)
		{
			return 2;
		}
		return 4;
	}

	public static bool IsAvailableIn(AttributeType type, ModeType mode)
	{
		if (mode == ModeType.MP)
		{
			if (type == AttributeType.CampaignData)
			{
				return false;
			}
		}
		return true;
	}

	public static int GetTeamCount()
	{
		GameModeType gameMode = GetGameMode(GetValue(AttributeType.GameMode));
		return GetTeamCount(gameMode);
	}

	public static ModeType GetMode(string value)
	{
		return (ModeType)(int)Enum.Parse(typeof(ModeType), value);
	}

	public static bool IsTutorialGameMode()
	{
		return IsGameModeType(GameModeType.Tutorial);
	}

	public static bool IsGameModeType(GameModeType eGameModeType)
	{
		return GetGameMode() == eGameModeType;
	}

	public static bool IsAuxFlagSet(AuxFlagsType eFlag)
	{
		return (eFlag & GetAuxFlags()) != 0;
	}

	public static AuxFlagsType GetAuxFlags()
	{
		return GetAuxFlags(GetValue(AttributeType.AuxFlags));
	}

	public static AuxFlagsType GetAuxFlags(string value)
	{
		return (AuxFlagsType)(int)Enum.Parse(typeof(AuxFlagsType), value);
	}

	public static GameModeType GetGameMode()
	{
		return GetGameMode(GetValue(AttributeType.GameMode));
	}

	public static GameModeType GetGameMode(string value)
	{
		return (GameModeType)(int)Enum.Parse(typeof(GameModeType), value);
	}

	public static ChampionshipType GetChampionshipMode(string value)
	{
		return (ChampionshipType)(int)Enum.Parse(typeof(ChampionshipType), value);
	}

	public static AttributeType[] GetAttributes()
	{
		return (AttributeType[])Enum.GetValues(typeof(AttributeType));
	}

	public static AttributeType GetAttribute(string key)
	{
		return (AttributeType)(int)Enum.Parse(typeof(AttributeType), key, true);
	}

	public static void SetAttribute(AttributeType type, object value, Hashtable attributes)
	{
		attributes[type.ToString()] = value.ToString();
	}

	public static string GetValue(AttributeType type)
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			return GetValue(type, game.Attributes);
		}
		return GetDefault(type);
	}

	public static uint GetSortOrder()
	{
		string value = GetValue(AttributeType.Order);
		uint result = 0u;
		uint.TryParse(value, out result);
		return result;
	}

	public static string GetValue(AttributeType type, Hashtable attributes)
	{
		string key = type.ToString();
		object obj = attributes[key];
		if (obj != null)
		{
			return obj.ToString();
		}
		return GetDefault(type);
	}

	public static string GetLocalizedValue(AttributeType type, Hashtable attributes)
	{
		string value = GetValue(type, attributes);
		return GetLocalizedValue(type, value);
	}

	public static string GetLocalizedValue(AttributeType type, string value)
	{
		string id = string.Format("ID_GAME_ATTRIBUTE_{0}_{1}", type.ToString(), value).ToUpper();
		string value2 = string.Empty;
		if (Localizer.GetString(id, out value2))
		{
			return value2;
		}
		return value;
	}

	public static string[] GetValues(AttributeType type)
	{
		switch (type)
		{
		case AttributeType.Level:
			return Setup.Instance.Levels;
		case AttributeType.GameMode:
			return Enum.GetNames(typeof(GameModeType));
		case AttributeType.Mode:
			return Enum.GetNames(typeof(ModeType));
		case AttributeType.Version:
			return new string[1] { Setup.Version };
		case AttributeType.CampaignData:
			return CampaignData.Instance.CampaignDataStrings();
		case AttributeType.Order:
			return new string[1] { "0" };
		case AttributeType.AuxFlags:
			return Enum.GetNames(typeof(AuxFlagsType));
		case AttributeType.Championship:
			return Enum.GetNames(typeof(ChampionshipType));
		default:
			throw new Exception("Unknown attribute type: " + type);
		}
	}

	public static string GetDefault(AttributeType type)
	{
		string[] values = GetValues(type);
		return (values.Length <= 0) ? string.Empty : values[0];
	}

	public static Hashtable CreateDefault(ModeType mode)
	{
		Hashtable hashtable = new Hashtable();
		AttributeType[] attributes = GetAttributes();
		foreach (AttributeType attributeType in attributes)
		{
			if (IsAvailableIn(attributeType, mode))
			{
				hashtable[attributeType.ToString()] = GetDefault(attributeType);
			}
		}
		hashtable[AttributeType.Mode.ToString()] = mode.ToString();
		return hashtable;
	}
}
