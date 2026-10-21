using System;
using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

[EB.Serializable(Tag = "chp")]
public class ChampionshipPrefab : MonoBehaviour
{
	[System.Serializable]
	[EB.Serializable(Tag = "rw")]
	public class Reward
	{
		[EB.Serializable(Tag = "n")]
		public string assetName = string.Empty;

		[EB.Serializable(Tag = "q")]
		public int quantity;

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(assetName) && quantity > 0;
		}
	}

	[System.Serializable]
	[EB.Serializable(Tag = "ca")]
	public class CompletionAward
	{
		[EB.Serializable(Tag = "xp")]
		public int XP;

		[EB.Serializable(Tag = "rv")]
		public int Rivets;

		public bool IsValid()
		{
			return XP > 0 || Rivets > 0;
		}
	}

	[NoSerialize]
	public CharacterPrefab[] AiPlayers = new CharacterPrefab[0];

	[EB.Serializable(Tag = "aisp")]
	public string[] AiSpawnPoints = new string[0];

	[EB.Serializable(Tag = "ainv")]
	public SinglePlayerPrefab.Loadout[] AiInventory = new SinglePlayerPrefab.Loadout[0];

	[EB.Serializable(Tag = "aii")]
	public int AdvancedInventoryIndex;

	[EB.Serializable(Tag = "aist")]
	public int AdvancedInventoryScoreThreshold = 1000;

	[EB.Serializable(Tag = "psp")]
	public string PlayerSpawnPointName = string.Empty;

	[EB.Serializable(Tag = "oap")]
	public bool onlyAttackPlayer;

	[NoSerialize]
	public CampaignCinematic[] Cinematic = new CampaignCinematic[0];

	public void Play(string levelName, GameAttributes.GameModeType gameModeType)
	{
		LoadTuningData();
		FacebookPortraitManager.FlushAll();
		GameManager gameManager = Hub.Instance.GameManager;
		GameManager.LocalCreateGameParams pars = new GameManager.LocalCreateGameParams
		{
			Attributes = GameAttributes.CreateDefault(GameAttributes.ModeType.SP)
		};
		GameHUD._settings._eVisFlags = GameHUD.Settings.Visibility.kBoosts;
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Championship, GameAttributes.ChampionshipType.kOn, pars.Attributes);
		GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, gameModeType, pars.Attributes);
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, levelName, pars.Attributes);
		pars.PlayerAttributes = Misc.CreatePlayerAttributes();
		pars.CampaignAttributes = new Hashtable();
		pars.CampaignAttributes["sp_prefab"] = base.name;
		pars.CampaignAttributes["sp_spawnpoint"] = PlayerSpawnPointName;
		AISimple.SetLoadout(AiInventory);
		AISimple.onlyAttackPlayer = onlyAttackPlayer;
		AISimple._AdvancedInventoryIndex = AdvancedInventoryIndex;
		AISimple._AdvancedInventoryThreshold = AdvancedInventoryScoreThreshold;
		for (int i = 0; i < AiPlayers.Length; i++)
		{
			CharacterPrefab characterPrefab = AiPlayers[i];
			if (!(characterPrefab == null))
			{
				string spawnPointName = string.Empty;
				if (i < AiSpawnPoints.Length)
				{
					spawnPointName = AiSpawnPoints[i];
				}
				EB.Debug.Log("Adding AI player");
				pars.AddAIPlayer(characterPrefab.CreatePlayerAttributes(spawnPointName));
			}
		}
		Game game = gameManager.CreateLocalGame(pars);
		Hub.Instance.InventoryManager.ClearOverride();
		game.Start();
	}

	public void LoadTuningData()
	{
		TuningManager tuningManager = Hub.Instance.TuningManager;
		string text = "championship_stage_prefab." + base.gameObject.name.ToString();
		tuningManager.Apply(this, text);
	}
}
