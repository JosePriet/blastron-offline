using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class RewardsManager : MonoBehaviour
{
	private static RewardsManager _instance;

	public static RewardsManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public int RewardRivets { get; set; }

	public int RewardXP { get; set; }

	public bool DidLevel { get; set; }

	public bool ChampionshipFirstTimeAttempt { get; set; }

	public ChampionshipPrefab.Reward[] ChampionshipRewardItems { get; private set; }

	public ChampionshipPrefab.Reward ChampionshipRewardUiOverride { get; private set; }

	private void Awake()
	{
		_instance = this;
	}

	public void CalculateEndGameRewards(float percentTimeElapsed)
	{
		RewardRivets = 0;
		RewardXP = 0;
		List<EB.Sparx.Player> endGamePlayers = Scoring.Instance.GetEndGamePlayers();
		Game game = Hub.Instance.GameManager.Game;
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		int num = 0;
		if (gameMode == GameAttributes.GameModeType.SuddenDeath)
		{
			for (int i = 0; i < endGamePlayers.Count; i++)
			{
				EB.Sparx.Player player = endGamePlayers[i];
				if (player.PlayerId == game.LocalPlayer.PlayerId)
				{
					num = i + 1;
					break;
				}
			}
		}
		else
		{
			num = Scoring.Instance.GetRankByTeam(Scoring.Instance.GetTeamId(game.LocalPlayer.PlayerId));
		}
		if (num >= 1 && num <= 4)
		{
			Tuning.PlaceReward[] placeRewardTable = Tuning.Instance.PlaceRewardTable;
			Tuning.PlaceReward[] array = placeRewardTable;
			foreach (Tuning.PlaceReward placeReward in array)
			{
				if (placeReward.GameMode == gameMode)
				{
					RewardRivets = placeReward.rivetBonus[num - 1];
					RewardXP = placeReward.xpBonus[num - 1];
					break;
				}
			}
		}
		else
		{
			EB.Debug.LogWarning("Rank " + num + " out of range, no rank bonus!");
		}
		UnityEngine.Debug.Log("Modifying rewards by % time elapsed: " + percentTimeElapsed);
		RewardRivets = (int)((float)RewardRivets * Mathf.Clamp(percentTimeElapsed, 0f, 1f));
		RewardXP = (int)((float)RewardXP * Mathf.Clamp(percentTimeElapsed, 0f, 1f));
		Tuning.PointReward[] pointRewardTable = Tuning.Instance.PointRewardTable;
		Tuning.PointReward[] array2 = pointRewardTable;
		foreach (Tuning.PointReward pointReward in array2)
		{
			if (pointReward.GameMode == gameMode)
			{
				int scoreByTeam = Scoring.Instance.GetScoreByTeam(Scoring.Instance.GetTeamId(game.LocalPlayer.PlayerId));
				float num2 = (float)scoreByTeam * pointReward.rivetMultiplier;
				float num3 = (float)scoreByTeam * pointReward.xpMultiplier;
				RewardRivets += (int)num2;
				RewardXP += (int)num3;
				break;
			}
		}
		float num4 = HoFData.Instance.RivetMultiplier(Profile.Instance.HoFLevel());
		RewardRivets = (int)((float)RewardRivets * num4);
	}

	public void GrantLevelUpRewards()
	{
		int level = Profile.Instance.Level;
		int num = Mathf.Max(0, level - 2);
		Tuning.LevelInfo levelInfo = Tuning.Instance.Levels[num];
		Tuning.LevelReward[] rewards = levelInfo.Rewards;
		Tuning.LevelReward[] array = rewards;
		foreach (Tuning.LevelReward levelReward in array)
		{
			switch (levelReward.type)
			{
			case Tuning.LevelRewardType.HardCurrency:
				Hub.Instance.WalletManager.Credit(levelReward.quantity, "levelUp");
				break;
			case Tuning.LevelRewardType.SoftCurrency:
				Profile.Instance.AddSoftCurrency(levelReward.quantity);
				break;
			case Tuning.LevelRewardType.Inventory:
				Telemetry.Event("item/levelup", levelReward.quantity, new Hashtable { { "name", levelReward.name } });
				Hub.Instance.InventoryManager.AddItem(levelReward.name, levelReward.quantity);
				break;
			case Tuning.LevelRewardType.SkillPoints:
				AttributesManager.instance.AddSkillPoints(levelReward.quantity);
				break;
			default:
				EB.Debug.LogError("Unknown reward type");
				break;
			case Tuning.LevelRewardType.None:
				break;
			}
		}
	}

	public void GrantChampionshipRewards(ChampionshipPrefab.Reward[] rewards, ChampionshipPrefab.Reward uiOverride)
	{
		ChampionshipRewardItems = ((rewards == null) ? new ChampionshipPrefab.Reward[0] : rewards);
		ChampionshipRewardUiOverride = uiOverride;
		ChampionshipPrefab.Reward[] championshipRewardItems = ChampionshipRewardItems;
		foreach (ChampionshipPrefab.Reward reward in championshipRewardItems)
		{
			if (reward.IsValid())
			{
				EB.Debug.Log("Championship - awarding " + reward.quantity + "x " + reward.assetName);
				Telemetry.Event("item/campaign", reward.quantity, new Hashtable { { "name", reward.assetName } });
				Hub.Instance.InventoryManager.AddItem(reward.assetName, reward.quantity);
			}
		}
	}
}
