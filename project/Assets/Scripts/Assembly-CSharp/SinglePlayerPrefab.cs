using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class SinglePlayerPrefab : MonoBehaviour
{
	[System.Serializable]
	[EB.Serializable(Tag = "ld")]
	public class Loadout
	{
		[EB.Serializable(Tag = "n")]
		public string name = string.Empty;

		[EB.Serializable(Tag = "c")]
		public int count;
	}

	[System.Serializable]
	public class Reward
	{
		public string assetName = string.Empty;

		public string friendlyName = string.Empty;
	}

	[System.Serializable]
	public class CampaignPlusData
	{
		public int minLevel;

		public CharacterPrefab[] RandomPlayers;
	}

	public CharacterPrefab[] Players = new CharacterPrefab[0];

	public CampaignPlusData[] CampaignPlusTable = new CampaignPlusData[0];

	public string[] AiSpawnPoints = new string[0];

	public string Level = string.Empty;

	public string LevelTexture = string.Empty;

	public GameAttributes.GameModeType GameMode;

	public bool ReloadableLevel;

	public bool EnableBlastVisualization;

	public bool InfiniteAmmo;

	public string playerSpawnPointName = string.Empty;

	public Loadout[] Inventory = new Loadout[0];

	public Reward[] Rewards = new Reward[0];

	public string locPrefix
	{
		get
		{
			return "ID_SP_" + StringUtil.SafeKey(base.name).ToUpper();
		}
	}

	public string missionName
	{
		get
		{
			return locPrefix + "_NAME";
		}
	}

	public string missionSubtitle
	{
		get
		{
			return locPrefix + "_SUBTITLE";
		}
	}

	public string missionDialogDesc
	{
		get
		{
			return locPrefix + "_DESC";
		}
	}

	public void Play()
	{
		FacebookPortraitManager.FlushAll();
		GameManager gameManager = Hub.Instance.GameManager;
		GameManager.LocalCreateGameParams pars = new GameManager.LocalCreateGameParams
		{
			Attributes = GameAttributes.CreateDefault(GameAttributes.ModeType.SP)
		};
		GameHUD._settings._eVisFlags = (GameHUD.Settings.Visibility)0;
		GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, GameMode, pars.Attributes);
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, Level, pars.Attributes);
		GameAttributes.AuxFlagsType auxFlagsType = (GameAttributes.AuxFlagsType)((ReloadableLevel ? 2 : 0) | (EnableBlastVisualization ? 4 : 0) | (InfiniteAmmo ? 8 : 0));
		GameAttributes.SetAttribute(GameAttributes.AttributeType.AuxFlags, auxFlagsType, pars.Attributes);
		pars.PlayerAttributes = Misc.CreatePlayerAttributes();
		pars.CampaignAttributes = CreateCampaignAttributes();
		if (CampaignData.Instance.IsCampaignPlus() && CampaignPlusTable.Length > 0 && GameMode != GameAttributes.GameModeType.Tutorial)
		{
			int num = -1;
			for (int i = 0; i < CampaignPlusTable.Length; i++)
			{
				CampaignPlusData campaignPlusData = CampaignPlusTable[i];
				if (campaignPlusData.minLevel <= Profile.Instance.Level)
				{
					num++;
					continue;
				}
				break;
			}
			if (num >= 0)
			{
				CharacterPrefab[] randomPlayers = CampaignPlusTable[num].RandomPlayers;
				for (int j = 0; j < 3; j++)
				{
					CharacterPrefab characterPrefab = randomPlayers[UnityEngine.Random.Range(0, randomPlayers.Length)];
					pars.AddAIPlayer(characterPrefab.CreatePlayerAttributes(string.Empty));
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Invalid campaign plus data!");
			}
		}
		else
		{
			for (int k = 0; k < Players.Length; k++)
			{
				CharacterPrefab characterPrefab2 = Players[k];
				string spawnPointName = string.Empty;
				if (k < AiSpawnPoints.Length)
				{
					spawnPointName = AiSpawnPoints[k];
				}
				pars.AddAIPlayer(characterPrefab2.CreatePlayerAttributes(spawnPointName));
			}
		}
		Game game = gameManager.CreateLocalGame(pars);
		Hub.Instance.InventoryManager.ClearOverrideKeys();
		Hashtable hashtable = new Hashtable();
		if (CampaignData.Instance.IsCampaignPlus() && GameMode != GameAttributes.GameModeType.Tutorial)
		{
			hashtable["Rocket"] = 10;
			Hub.Instance.InventoryManager.AddOverrideKey("Rocket");
			hashtable["Teleport Grenade"] = 30;
			Hub.Instance.InventoryManager.AddOverrideKey("Teleport Grenade");
			List<InventoryDB.MetaData> list = new List<InventoryDB.MetaData>();
			List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
			foreach (InventoryDB.MetaData item in categoryList)
			{
				if (item.campaignPlusWeight > 0)
				{
					list.Add(item);
				}
			}
			for (int l = 0; l < 4; l++)
			{
				int totalCampaignPlusWeight = GetTotalCampaignPlusWeight(list);
				int num2 = UnityEngine.Random.Range(0, totalCampaignPlusWeight);
				int num3 = 0;
				foreach (InventoryDB.MetaData item2 in list)
				{
					num3 += item2.campaignPlusWeight;
					if (num3 > num2)
					{
						UnityEngine.Debug.LogWarning("adding to campaign+ loadout: " + item2.name + ":" + item2.campaignPlusCount);
						hashtable[item2.name] = item2.campaignPlusCount;
						Hub.Instance.InventoryManager.AddOverrideKey(item2.name);
						list.Remove(item2);
						break;
					}
				}
			}
		}
		else
		{
			Loadout[] inventory = Inventory;
			foreach (Loadout loadout in inventory)
			{
				hashtable[loadout.name] = loadout.count;
				Hub.Instance.InventoryManager.AddOverrideKey(loadout.name);
			}
		}
		Hub.Instance.InventoryManager.Override(hashtable);
		Hub.Instance.InventoryManager.ForceOverride = true;
		Hub.Instance.InventoryManager.SinglePlayerClearUsedInventory();
		game.Start();
	}

	private Hashtable CreateCampaignAttributes()
	{
		Hashtable hashtable = new Hashtable();
		hashtable["sp_prefab"] = base.name;
		hashtable["sp_spawnpoint"] = playerSpawnPointName;
		return hashtable;
	}

	private int GetTotalCampaignPlusWeight(List<InventoryDB.MetaData> mdList)
	{
		int num = 0;
		foreach (InventoryDB.MetaData md in mdList)
		{
			num += md.campaignPlusWeight;
		}
		return num;
	}
}
