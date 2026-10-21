using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
	[Flags]
	private enum ResultsFields
	{
		kKills = 1,
		kRivets = 2,
		kXP = 4,
		kDeaths = 8,
		kSuicides = 0x10,
		kHoF = 0x20,
		kPile = 0x40,
		kAll = kKills | kRivets | kXP | kDeaths | kSuicides | kHoF | kPile
	}

	private const string tutorialString = "tutorial_done";

	private const float cPitchIncrease = 0.1f;

	private const string placeholderChars = "0000000000";

	private ResultsFields _eResFields;

	private bool _gameEnded;

	private UILabel _turnTimerLbl;

	private UIFilledSprite _turnTimerDisk;

	public int[] PromoGamePopupCounts = new int[2] { 5, 10 };

	public float upgradeThreshold = 50f;

	public AudioEvent _spinSound;

	public AudioEvent _digitCompleteSound;

	public AudioEvent _flourishSound;

	public bool _hasChangedCacSize;

	private bool _transitioningOut;

	private string _playerResultString = string.Empty;

	private bool _userHasWeapons = true;

	private float promoPopupCloseTimer;

	private UITexture rewardTexture;

	private AudioEmitter _tallyEmitter;

	private ClippedProgressBar _GreenStarsProgressBar;

	private ClippedProgressBar _GoldStarsProgressBar;

	private UIPanel _greenStarsPanel;

	private Vector4 _greenStarsPanelOrigSize;

	public UILabel _rewardQuantity;

	private bool won;

	private float _bangPitch;

	private float _punchScaleScale = 10f;

	private bool _tallying;

	private GameObject _fxFireworks;

	private float progressRatio;

	private float NumOfStars = 3f;

	private float MaxStarFillRatio = 1f;

	private PunchScale _punchy;

	private string[] positionStrings = new string[4] { "FIRST", "SECOND", "THIRD", "FOURTH" };

	private string[] championshipPositionStrings = new string[4] { "FIRST", "LOST", "LOST", "LOST" };

	public float BackToLobbyTimer { get; set; }

	public bool LevelUpComplete { get; set; }

	private bool isPrivateGame
	{
		get
		{
			if (Hub.Instance.GameManager.Game == null)
			{
				return false;
			}
			return Hub.Instance.GameManager.Game.IsPrivate;
		}
	}

	private bool isChampionship
	{
		get
		{
			return GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn;
		}
	}

	private bool isSP
	{
		get
		{
			return GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) == GameAttributes.ModeType.SP;
		}
	}

	private bool InputEnabled { get; set; }

	private void Awake()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			LevelUpComplete = false;
			InputEnabled = false;
			_tallyEmitter = base.gameObject.AddComponent<AudioEmitter>();
			_tallyEmitter.m_PlayMode = AudioEmitter.PlayMode.Stack;
			if (isChampionship)
			{
				UnityEngine.Debug.Log("Campaign Results UI");
				GetRewards();
				ShowCampaignItems(true);
			}
			else
			{
				ShowCampaignItems(false);
			}
			_turnTimerLbl = Util.GetObject(base.gameObject, "Turn_timer_label", true).GetComponent<UILabel>();
			_turnTimerDisk = Util.GetObject(base.gameObject, "Turn_timer_disk", true).GetComponent<UIFilledSprite>();
			bool flag = GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kTutorialOverride);
			if (!isSP && flag)
			{
				Util.GetObjectExactMatch(base.gameObject, "Timer").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "PanelBackground_timer").SetActive(false);
			}
		}
	}

	private void ShowCampaignItems(bool show)
	{
		Util.GetObjectExactMatch(base.gameObject, "StatsPanel").SetActive(!show);
		Util.GetObjectExactMatch(base.gameObject, "CampaignObjs").SetActive(show);
	}

	private void GetRewards()
	{
		if (RewardsManager.Instance.ChampionshipRewardItems.Length > 0)
		{
			string text = null;
			int quantity = 0;
			if (RewardsManager.Instance.ChampionshipRewardUiOverride != null)
			{
				text = RewardsManager.Instance.ChampionshipRewardUiOverride.assetName;
				quantity = RewardsManager.Instance.ChampionshipRewardUiOverride.quantity;
			}
			else
			{
				string assetName = RewardsManager.Instance.ChampionshipRewardItems[0].assetName;
				quantity = RewardsManager.Instance.ChampionshipRewardItems[0].quantity;
				InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(assetName));
				if (metaData != null)
				{
					text = metaData.uiTexturePath;
				}
				else
				{
					CharacterDB.MetaData metaData2 = CharacterDB.GetMetaData(Hash.StringHash(assetName));
					if (metaData2 != null)
					{
						text = "cac_ui_parts/" + metaData2.cacTexturePath;
					}
				}
			}
			if (!string.IsNullOrEmpty(text) && quantity > 0)
			{
				rewardTexture = Util.FindComponent<UITexture>(Util.GetObjectExactMatch(base.gameObject, "RewardTexture"));
				TexturePoolManager.Instance.LoadTexture(text, (Texture2D tex) =>
				{
					rewardTexture.mainTexture = tex;
					_rewardQuantity.text = quantity.ToString();
				});
			}
			else
			{
				Util.GetObjectExactMatch(base.gameObject, "SpecialRewards").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "PanelBackground_timer").SetActive(false);
			}
		}
		else
		{
			Util.GetObjectExactMatch(base.gameObject, "SpecialRewards").SetActive(false);
			Util.GetObjectExactMatch(base.gameObject, "PanelBackground_timer").SetActive(false);
		}
	}

	private void Start()
	{
		Game game = Hub.Instance.GameManager.Game;
		EB.Debug.Log("START RESULTS UI!!!!!");
		if (game == null)
		{
			Home();
			EB.Debug.Log("NO GAME RESULTS UI!!!!!");
			return;
		}
		if (isChampionship)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "StarContainer");
			_GreenStarsProgressBar = Util.GetObjectExactMatch(objectExactMatch, "GreenStars").GetComponent<ClippedProgressBar>();
			_greenStarsPanel = _GreenStarsProgressBar.gameObject.GetComponent<UIPanel>();
			_greenStarsPanelOrigSize = _greenStarsPanel.clipRange;
			_GreenStarsProgressBar.UpdateClipRange();
			_GreenStarsProgressBar.SetValueImmediate(0.01f);
			_GoldStarsProgressBar = Util.GetObjectExactMatch(objectExactMatch, "GoldStars").GetComponent<ClippedProgressBar>();
			_GoldStarsProgressBar.UpdateClipRange();
			_GoldStarsProgressBar.SetValueImmediate(0.01f);
		}
		EB.Replication.Manager.PauseViewUpdates(true);
		bool Argument = true;
		bool Argument2 = false;
		Messenger<bool, bool>.BroadcastToAllListeners("MuteAudioJukebox", ref Argument, ref Argument2);
		GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
		backgroundUI.ShowCliff(false);
		int rankByPlayer = Scoring.Instance.GetRankByPlayer(game.LocalPlayer.PlayerId);
		RewardsPile rewardsPile = Util.FindComponent<RewardsPile>(base.gameObject);
		rewardsPile.SetPosition(rankByPlayer);
		BackToLobbyTimer = Tuning.Instance.BackToLobbyTimer;
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnReady));
		_fxFireworks = Util.GetObjectExactMatch(base.gameObject, "fx_fireworks_ui");
		ScreenManager.Instance.FadeOut(null);
		EB.Debug.Log("END RESULTS UI!!!!!");
	}

	private void UpdateNextUnlock()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "weapon_item_store");
		List<InventoryDB.MetaData> unlockableItemsOfType = InventoryDB.GetUnlockableItemsOfType(InventoryItem.Category.Weapon, Profile.Instance.GetMaxItemUnlockLevel(InventoryItem.Category.Weapon), true);
		if (unlockableItemsOfType.Count > 0)
		{
			objectExactMatch.GetComponent<WeaponUiItem>().SetMetaData(unlockableItemsOfType[UnityEngine.Random.Range(0, unlockableItemsOfType.Count)]);
			UIEventListener uIEventListener = UIEventListener.Get(objectExactMatch);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnUnlock));
		}
		else
		{
			NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "NextUnlock"), false);
			Util.GetObjectExactMatch(base.gameObject, "PanelBackground_unlock").SetActive(false);
		}
	}

	private void OnReady()
	{
		EB.Debug.Log("ON READY RESULTS UI!!!!!");
		bool flag = Hub.Instance.State == HubState.Connected;
		bool flag2 = !CampaignData.Instance.IsCampaignPlus() && won;
		bool flag3 = CampaignData.Instance.IsCampaignPlus() && Profile.Instance.SessionHighScoreAchieved;
		bool flag4 = isChampionship && won;
		if (!isSP || (flag && (flag2 || flag3 || flag4)))
		{
			RewardsPile rewardsPile = Util.FindComponent<RewardsPile>(base.gameObject);
			rewardsPile.StartPoppingIn();
		}
		else if (Profile.Instance.SessionHighScoreAchieved)
		{
			StartCoroutine(UpdateNewHighScore());
		}
		InputEnabled = true;
		EB.Sequence.Component.Activate(base.gameObject, "ResultsUI", typeof(SequenceEvent_OnScreenOpened));
	}

	public void OnClosePopup()
	{
		EB.Debug.Log("CLOSING REWARDS POPUP");
		GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
		backgroundUI.ShowCliff(true);
		StopAllCoroutines();
	}

	private IEnumerator UpdateNewHighScore()
	{
		EB.Debug.Log("UPDATING NEW HIGH SCORE");
		bool flag = true;
		GameObject PlayersPanel = Util.GetObjectExactMatch(base.gameObject, "PlayersPanel");
		while (true)
		{
			yield return new WaitForSeconds(Tuning.Instance.NewHighScoreTimer);
			if (flag)
			{
				UIUtils.SetLabelContents(PlayersPanel, "LabelTitle", "ID_UI_RESULTS_NEW_HS");
			}
			else
			{
				UIUtils.SetLabelContents(PlayersPanel, "LabelTitle", _playerResultString);
			}
			flag = !flag;
		}
	}

	public void OnHome(GameObject caller)
	{
		if (InputEnabled)
		{
			Home();
		}
	}

	private void Home()
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoHome);
		_transitioningOut = true;
	}

	private void OnBack(GameObject caller)
	{
		OnHome(caller);
	}

	private void GotoHome()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			List<string> list = new List<string> { "HomeUI" };
			if (isChampionship)
			{
				list.Add("SinglePlayerUI");
			}
			ExitGame(list);
		});
	}

	public void OnStore(GameObject caller)
	{
		if (InputEnabled)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoStore);
			_transitioningOut = true;
		}
	}

	private void GotoStore()
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("StoreUI");
		ExitGame(list);
	}

	public void OnUnlock(GameObject caller)
	{
		if (InputEnabled)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoUnlock);
			_transitioningOut = true;
		}
	}

	private void GotoUnlock()
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("StoreUI");
		Setup setup = UnityEngine.Object.FindObjectOfType(typeof(Setup)) as Setup;
		setup.nextScreenFn = "OnExitToWeapons";
		ExitGame(list);
	}

	private void GotoPartsStore()
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("StoreUI");
		Setup setup = UnityEngine.Object.FindObjectOfType(typeof(Setup)) as Setup;
		setup.nextScreenFn = "OnExitToParts";
		ExitGame(list);
	}

	private void GotoUpgradeStore()
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("StoreUI");
		list.Add("MyRobotUI");
		Setup setup = UnityEngine.Object.FindObjectOfType(typeof(Setup)) as Setup;
		setup.nextScreenFn = "OnFETutorialUpgrade";
		ExitGame(list);
	}

	private void GotoWeaponsStore()
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("StoreUI");
		Setup setup = UnityEngine.Object.FindObjectOfType(typeof(Setup)) as Setup;
		setup.nextScreenFn = "OnExitToWeapons";
		ExitGame(list);
	}

	public void OnSinglePlayerPlayNext(GameObject caller)
	{
		if (InputEnabled)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoSinglePlayerPlayNext);
			_transitioningOut = true;
		}
	}

	private void GotoSinglePlayerPlayNext()
	{
		if (isChampionship)
		{
			List<string> list = new List<string>();
			list.Add("HomeUI");
			list.Add("SinglePlayerUI");
			Division division = ChampionshipData.Instance.GetDivision(ChampionshipData.Instance._selectedDivision);
			int num = division.FindIndexForStage(ChampionshipData.Instance.SelectedStage) + 1;
			if (num >= division._stages.Length)
			{
				division = ChampionshipData.Instance.GetDivision(ChampionshipData.Instance._selectedDivision + 1);
				if (null != division)
				{
					ChampionshipData.Instance._selectedDivision++;
					ChampionshipData.Instance.SelectedStage = division._stages[0];
				}
			}
			else
			{
				ChampionshipData.Instance.SelectedStage = division._stages[num];
			}
			ExitGame(list);
		}
		else
		{
			List<SinglePlayerPrefab> missions = CampaignData.Instance.Missions;
			int currentMission = CampaignData.Instance.CurrentMission;
			currentMission++;
			if (currentMission < missions.Count)
			{
				CampaignData.Instance.CurrentMission = currentMission;
				missions[currentMission].Play();
			}
		}
	}

	public void OnSinglePlayerPlayAgain(GameObject caller)
	{
		if (InputEnabled)
		{
			if (Profile.Instance.Energy > 0 || !isChampionship)
			{
				EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoSinglePlayerPlayAgain);
				_transitioningOut = true;
			}
			else
			{
				ScreenManager.Instance.OpenPopup("GetMoreEnergyUI", true, true);
			}
		}
	}

	private void GotoSinglePlayerPlayAgain()
	{
		if (isChampionship)
		{
			Profile.Instance.ConsumeEnergy(1);
			ChampionshipData.Instance.PlayGame();
		}
		else
		{
			CampaignData.Instance.Missions[CampaignData.Instance.CurrentMission].Play();
		}
	}

	public void OnMultiPlayerPlayAgain(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoMultiPlayerPlayAgain);
		_transitioningOut = true;
	}

	public void OnMultiPlayerGameMode(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoMultiPlayerGameMode);
		_transitioningOut = true;
	}

	private void GotoMultiPlayerGameMode()
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("MatchmakingGameModeSelectUI");
		ExitGame(list);
	}

	private void GotoMultiPlayerPlayAgain()
	{
		List<string> list = new List<string>();
		list.Add("LobbyUI");
		ExitGame(list);
	}

	private void ExitGame(List<string> nextScreenStack)
	{
		_gameEnded = true;
		Game game = Hub.Instance.GameManager.Game;
		if (game != null && game.IsHost)
		{
			game.End();
		}
		if (nextScreenStack[0] != "LobbyUI")
		{
			Setup setup = UnityEngine.Object.FindObjectOfType(typeof(Setup)) as Setup;
			setup.nextScreenStack = nextScreenStack;
			Hub.Instance.GameManager.LeaveGame();
		}
	}

	private void OnOpenPopup()
	{
		EB.Debug.Log("ON OPEN POPUP REWARDS");
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		_gameEnded = false;
		ScreenManager.Instance.AddPersistentScreen("StatusBar");
		ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
		InitializeButtons();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Keycards");
		if (objectExactMatch != null)
		{
			objectExactMatch.SetActive(false);
		}
		Scoring instance = Scoring.Instance;
		List<EB.Sparx.Player> endGamePlayers = Scoring.Instance.GetEndGamePlayers();
		int rewardXP = RewardsManager.Instance.RewardXP;
		int rewardRivets = RewardsManager.Instance.RewardRivets;
		UpdatePlayerStandings(endGamePlayers);
		if (!_tallying)
		{
			StartCoroutine(Tally(rewardRivets, rewardXP));
		}
		if (Scoring.Instance.SessionHoFLevel != -1)
		{
			EB.Sequence.Component.Activate(base.gameObject, Scoring.Instance.SessionHoFLevel, typeof(SequenceEvent_HoFBracket));
		}
		SetPlayerResultString(endGamePlayers);
		_eResFields = ResultsFields.kAll;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "StatsPanel");
		_userHasWeapons = InventoryDB.HasWeapons();
		EB.Debug.Log("ON OPEN POPUP REWARDS2");
		if (isSP)
		{
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Timer");
			objectExactMatch3.SetActive(false);
			GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "ButtonStore");
			UIUtils.SetLabelContents(objectExactMatch4, "LabelTitle", "ID_UI_RESULTS_PLAY_AGAIN");
			GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "ButtonPlayNext");
			UIUtils.SetLabelContents(objectExactMatch5, "LabelTitle", "ID_UI_RESULTS_NEXT");
			GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
			won = false;
			if (gameMode == GameAttributes.GameModeType.SuddenDeath)
			{
				EB.Sparx.Player player = endGamePlayers[0];
				if (player != null)
				{
					won = player.PlayerId == game.LocalPlayer.PlayerId;
				}
			}
			else
			{
				won = Scoring.Instance.GetRankByPlayer(Hub.Instance.GameManager.Game.LocalPlayer.PlayerId) == 1;
			}
			if (!_userHasWeapons)
			{
				ScreenManager.Instance.Confirmation(true, "ID_UI_ERROR_TITLE", "ID_UI_INVENTORY_NOWEAPONS", "ID_UI_RESULTS_STORE", null, (ConfirmationDialogUI.ButtonIds buttonId) =>
				{
					if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
					{
						OnStore(base.gameObject);
					}
				});
				objectExactMatch5.SetActive(false);
				objectExactMatch4.SetActive(false);
			}
			else if (won)
			{
				DoUnlocks();
				bool flag = false;
				if (isChampionship)
				{
					instance.ReportChampionship();
					flag = true;
				}
				else
				{
					List<SinglePlayerPrefab> missions = CampaignData.Instance.Missions;
					int num = CampaignData.Instance.CurrentMission + 1;
					flag = num < missions.Count;
				}
				if (!flag)
				{
					objectExactMatch5.SetActive(false);
				}
			}
			else
			{
				objectExactMatch5.SetActive(false);
			}
			if (isChampionship)
			{
				Hashtable hashtable = new Hashtable();
				ChampionshipData.Instance.AddTelemetryEventData(hashtable);
				int scoreByPlayer = instance.GetScoreByPlayer(game.LocalPlayer.PlayerId);
				hashtable["result"] = ((!won) ? "lost" : "won");
				hashtable["partsLevel"] = Profile.Instance.recipe.GetAverageTier();
				hashtable["firstTime"] = RewardsManager.Instance.ChampionshipFirstTimeAttempt;
				hashtable["finalscore"] = scoreByPlayer;
				UnityEngine.Debug.LogWarning("Final Score " + scoreByPlayer);
				Telemetry.Event("champ_result", 1, hashtable);
			}
			else
			{
				string text = ((!won) ? "sp_failed/" : "sp_won/");
				Telemetry.Event(text + GameAttributes.GetValue(GameAttributes.AttributeType.CampaignData));
			}
			bool flag2 = Hub.Instance.State == HubState.Connected;
			bool flag3 = !CampaignData.Instance.IsCampaignPlus() && won;
			bool flag4 = CampaignData.Instance.IsCampaignPlus() && Profile.Instance.SessionHighScoreAchieved;
			if (!flag2 || (isSP && !flag3 && !flag4))
			{
				_eResFields &= ~(ResultsFields.kRivets | ResultsFields.kXP | ResultsFields.kPile);
			}
		}
		else
		{
			Telemetry.Event("mp_end/" + GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
			instance.Report(rewardXP);
			if (!_userHasWeapons)
			{
				GameObject objectExactMatch6 = Util.GetObjectExactMatch(base.gameObject, "Timer");
				objectExactMatch6.SetActive(false);
				ScreenManager.Instance.Confirmation(true, "ID_UI_ERROR_TITLE", "ID_UI_INVENTORY_NOWEAPONS", "ID_UI_RESULTS_STORE", null, (ConfirmationDialogUI.ButtonIds buttonId) =>
				{
					if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
					{
						OnStore(base.gameObject);
					}
				});
			}
		}
		if (isSP || isPrivateGame)
		{
			_eResFields &= ~ResultsFields.kHoF;
		}
		if ((_eResFields & ResultsFields.kRivets) == 0)
		{
			NGUITools.SetActive(Util.GetObjectExactMatch(objectExactMatch2, "Rivets"), false);
		}
		if ((_eResFields & ResultsFields.kXP) == 0)
		{
			NGUITools.SetActive(Util.GetObjectExactMatch(objectExactMatch2, "XP"), false);
		}
		if ((_eResFields & ResultsFields.kPile) == 0)
		{
			NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "fx_resultspile"), false);
		}
		if ((_eResFields & ResultsFields.kHoF) == 0)
		{
			NGUITools.SetActive(Util.GetObjectExactMatch(objectExactMatch2, "HoF"), false);
		}
		Telemetry.Flush();
	}

	private void ReportTournament()
	{
	}

	private void DoUnlocks()
	{
		if (!isChampionship)
		{
			SinglePlayerPrefab.Reward[] rewards = CampaignData.Instance.Missions[CampaignData.Instance.CurrentMission].Rewards;
			if (rewards.Length > 0 && !UnlocksManager.Instance.IsUnlocked(rewards[0].assetName))
			{
				UnlocksManager.Instance.Unlock(rewards[0].assetName);
				GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
				ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
				confirmationDialogUI.SetParameters(false, Localizer.Format("ID_UI_RESULTS_UNLOCK", rewards[0].friendlyName).ToUpper(), "ID_UI_RESULTS_REWARD", "ID_UI_COMMON_OK", null);
			}
			bool flag = CampaignData.Instance.IsCampaignPlus();
			CampaignData.Instance.UnlockNextMission();
			bool flag2 = CampaignData.Instance.IsCampaignPlus();
			if (!flag && flag2)
			{
				ScreenManager.Instance.Confirmation(false, "ID_UI_RESULTS_CAMPAIGNPLUS_UNLOCKED_TITLE", "ID_UI_RESULTS_CAMPAIGNPLUS_UNLOCKED_DESC", "ID_UI_COMMON_OK", string.Empty);
			}
		}
	}

	private void Update()
	{
		if (_transitioningOut)
		{
			if (_gameEnded)
			{
				Game game = Hub.Instance.GameManager.Game;
				if (game != null)
				{
					game.End();
				}
			}
		}
		else
		{
			if (isSP || !_userHasWeapons)
			{
				return;
			}
			BackToLobbyTimer -= UnityEngine.Time.deltaTime;
			if (promoPopupCloseTimer > 0f && BackToLobbyTimer < promoPopupCloseTimer)
			{
				promoPopupCloseTimer = 0f;
				GameObject popup = ScreenManager.Instance.GetPopup("PromoPopupUI");
				if (popup != null)
				{
					popup.SendMessage("OnClose");
				}
			}
			if (BackToLobbyTimer < 0f)
			{
				if (!GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kTutorialOverride))
				{
					GameObject popup2 = ScreenManager.Instance.GetPopup("LevelUpUI");
					if (popup2 != null)
					{
						popup2.SendMessage("OnClose");
					}
					OnMultiPlayerPlayAgain(null);
				}
			}
			else
			{
				int num = Mathf.CeilToInt(BackToLobbyTimer);
				string text = num.ToString();
				if (num < 10)
				{
					text = "0" + text;
				}
				_turnTimerLbl.text = text;
				_turnTimerDisk.fillAmount = BackToLobbyTimer / Tuning.Instance.BackToLobbyTimer;
			}
		}
	}

	private IEnumerator Tally(int rewardRivets, int rewardXP)
	{
		_tallying = true;
		_bangPitch = 0.5f;
		yield return new WaitForSeconds(0.5f);
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			Scoring scoring = Scoring.Instance;
			int kills = scoring.GetKillsByPlayer(game.LocalPlayer.PlayerId);
			int deaths = scoring.GetDeathsByPlayer(game.LocalPlayer.PlayerId);
			int suicides = scoring.GetSuicidesByPlayer(game.LocalPlayer.PlayerId);
			int xp = Profile.Instance.SessionXP + rewardXP;
			int rivets = Profile.Instance.SessionSoftCurrency + rewardRivets;
			int hof = scoring.SessionHoF;
			int score = scoring.GetScoreByPlayer(game.LocalPlayer.PlayerId);
			if (!isChampionship)
			{
				GameObject statsPanel = Util.GetObjectExactMatch(base.gameObject, "StatsPanel");
				GameObject killsObj = Util.GetObjectExactMatch(statsPanel, "Kills");
				GameObject rivetsObj = Util.GetObjectExactMatch(statsPanel, "Rivets");
				GameObject xpObj = Util.GetObjectExactMatch(statsPanel, "XP");
				GameObject deathsObj = Util.GetObjectExactMatch(statsPanel, "Deaths");
				GameObject suicidesObj = Util.GetObjectExactMatch(statsPanel, "Suicides");
				GameObject hofObj = Util.GetObjectExactMatch(statsPanel, "HoF");
				UnityEngine.Debug.Log(string.Format("kills {0} deaths {1} suicides {2} xp {3} rivets {4} hof {5}", kills, deaths, suicides, xp, rivets, hof));
				if ((_eResFields & ResultsFields.kKills) != 0)
				{
					yield return StartCoroutine(Tally(killsObj, "Value", kills));
				}
				if ((_eResFields & ResultsFields.kDeaths) != 0)
				{
					yield return StartCoroutine(Tally(deathsObj, "Value", deaths));
				}
				if ((_eResFields & ResultsFields.kSuicides) != 0)
				{
					yield return StartCoroutine(Tally(suicidesObj, "Value", suicides));
				}
				if ((_eResFields & ResultsFields.kXP) != 0)
				{
					yield return StartCoroutine(Tally(xpObj, "Value", xp));
				}
				if ((_eResFields & ResultsFields.kRivets) != 0)
				{
					yield return StartCoroutine(Tally(rivetsObj, "Value", rivets));
				}
				if ((_eResFields & ResultsFields.kHoF) != 0)
				{
					yield return StartCoroutine(Tally(hofObj, "Value", hof));
				}
			}
			else
			{
				GameObject campaignStatsPanel = Util.GetObjectExactMatch(base.gameObject, "CampaignStatsPanel");
				GameObject scoreObj = Util.GetObjectExactMatch(campaignStatsPanel, "Score");
				GameObject xpObj2 = Util.GetObjectExactMatch(campaignStatsPanel, "XP");
				GameObject rivetsObj2 = Util.GetObjectExactMatch(campaignStatsPanel, "Rivets");
				yield return StartCoroutine(RealCounter(scoreObj, "Value", score));
				if (!_transitioningOut)
				{
					if (Profile.Instance.SessionHighScoreAchieved)
					{
						GameObject screen = ScreenManager.Instance.OpenPopup("CampaignHighScoreUI", true, true);
						CampaignHighScoreUI highScoreUi = Util.FindComponent<CampaignHighScoreUI>(screen);
						highScoreUi._highScore = score;
						highScoreUi._scoreMode = scoring.Mode;
						while (screen != null)
						{
							yield return new WaitForEndOfFrame();
						}
					}
					if (ChampionshipData.Instance._rewardWeapon)
					{
						GameObject screen2 = ScreenManager.Instance.OpenPopup("ChampionshipRewardUI", true, true);
						ChampionshipRewardUI dialog = screen2.GetComponent<ChampionshipRewardUI>();
						InventoryDB.MetaData metadata = InventoryDB.GetMetaData(Hash.StringHash(ChampionshipData.Instance._tierToReward.WeaponName));
						dialog.SetParameters(metadata.uiTexturePath, metadata.friendlyName, true);
						while (ChampionshipData.Instance._rewardWeapon)
						{
							yield return new WaitForEndOfFrame();
						}
					}
					if (RewardsManager.Instance.ChampionshipRewardItems.Length > 0)
					{
						string uiTexturePath = null;
						string inventoryName = string.Empty;
						int quantity = 0;
						if (RewardsManager.Instance.ChampionshipRewardUiOverride != null)
						{
							uiTexturePath = RewardsManager.Instance.ChampionshipRewardUiOverride.assetName;
							quantity = RewardsManager.Instance.ChampionshipRewardUiOverride.quantity;
						}
						else
						{
							string prefabName = RewardsManager.Instance.ChampionshipRewardItems[0].assetName;
							quantity = RewardsManager.Instance.ChampionshipRewardItems[0].quantity;
							InventoryDB.MetaData metadata2 = InventoryDB.GetMetaData(Hash.StringHash(prefabName));
							if (metadata2 != null)
							{
								uiTexturePath = metadata2.uiTexturePath;
								inventoryName = Localizer.GetString(metadata2.friendlyName);
							}
							else
							{
								CharacterDB.MetaData cacMetadata = CharacterDB.GetMetaData(Hash.StringHash(prefabName));
								if (cacMetadata != null)
								{
									uiTexturePath = "cac_ui_parts/" + cacMetadata.cacTexturePath;
								}
							}
						}
						if (!string.IsNullOrEmpty(uiTexturePath) && quantity > 0)
						{
							ChampionshipData.Instance._rewardItem = true;
							GameObject screen3 = ScreenManager.Instance.OpenPopup("ChampionshipRewardUI", true, true);
							ChampionshipRewardUI dialog2 = screen3.GetComponent<ChampionshipRewardUI>();
							dialog2.SetParameters(uiTexturePath, inventoryName + " x" + quantity, false);
							while (ChampionshipData.Instance._rewardItem)
							{
								yield return new WaitForEndOfFrame();
							}
						}
					}
				}
				yield return StartCoroutine(Tally(xpObj2, "Value", xp));
				yield return StartCoroutine(Tally(rivetsObj2, "Value", rivets));
			}
			FurtherScreens();
		}
		else
		{
			StopAllCoroutines();
		}
	}

	private IEnumerator RealCounter(GameObject parent, string labelName, int count)
	{
		GameObject o = Util.GetObjectExactMatch(parent, labelName);
		UILabel label = o.GetComponent<UILabel>();
		label.color = Color.yellow;
		string final = count.ToString();
		float counter = 0f;
		float digits = final.Length;
		Division.Stage stage = ChampionshipData.Instance.SelectedStage;
		float numOfHalves = NumOfStars * 2f;
		int HalvesFilled = 1;
		StartCoroutine(CheckGoldStars());
		bool useGold = false;
		if (won && stage._silverScoreThreshold > 0 && stage._goldScoreThreshold > 0)
		{
			while (counter <= (float)count)
			{
				progressRatio = Mathf.Clamp(counter / (float)stage._goldScoreThreshold, 0f, MaxStarFillRatio);
				_GreenStarsProgressBar.SetValueAnimated(progressRatio, null);
				label.text = counter.ToString();
				counter += (float)(int)digits;
				digits *= 1.01f;
				yield return null;
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("the stages silver or gold score is 0");
		}
		label.text = count.ToString();
		label.color = Color.white;
	}

	private IEnumerator CheckGoldStars()
	{
		float sizePerStar = _greenStarsPanelOrigSize.z / NumOfStars;
		float currentStar = 1f;
		while (true)
		{
			if (_greenStarsPanel.clipRange.z >= _greenStarsPanelOrigSize.z * (currentStar / NumOfStars))
			{
				_GoldStarsProgressBar.SetValueImmediate(currentStar / NumOfStars);
				currentStar++;
				if (currentStar > NumOfStars)
				{
					break;
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator Tally(GameObject parent, string labelName, int count)
	{
		GameObject o = Util.GetObjectExactMatch(parent, labelName);
		UILabel label = o.GetComponent<UILabel>();
		label.color = Color.yellow;
		string final = count.ToString();
		int digits = final.Length;
		char[] cdigits = "0000000000".Substring(0, digits).ToCharArray();
		for (int i = 0; i < digits; i++)
		{
			float rollTime = 1f;
			int digit = 0;
			yield return 0;
			cdigits[i] = final.Substring(i, 1).ToCharArray()[0];
			label.text = new string(cdigits);
			_tallyEmitter.Play(_digitCompleteSound);
		}
		if (_punchy == null)
		{
			_punchy = new PunchScale();
		}
		_tallyEmitter.SetPitchScalar(_bangPitch);
		_tallyEmitter.Play(_flourishSound);
		_bangPitch += 0.1f;
		_punchScaleScale += 5f;
		label.color = Color.white;
		if (!Misc.IsSlow() && _fxFireworks != null)
		{
			GameObject fxFireworks = UnityEngine.Object.Instantiate(_fxFireworks, o.transform.position, Quaternion.identity) as GameObject;
			fxFireworks.particleSystem.loop = false;
			fxFireworks.transform.parent = o.transform;
			fxFireworks.transform.localPosition = Vector3.zero;
			fxFireworks.transform.localScale = Vector3.one * 2f;
		}
		yield return new WaitForSeconds(0.1f);
	}

	private void UpdatePlayerStandings(List<EB.Sparx.Player> duplicatePlayerList)
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		Scoring instance = Scoring.Instance;
		int i;
		for (i = 0; i < duplicatePlayerList.Count; i++)
		{
			EB.Sparx.Player player = duplicatePlayerList[i];
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Player" + i);
			objectExactMatch.SetActive(true);
			ResultsPlayerUiItem uiItem = Util.FindComponent<ResultsPlayerUiItem>(objectExactMatch);
			uiItem.playerName = Dot.String("name", player.Attributes, string.Empty);
			uiItem.showBothKillsAndPoints = false;
			if (gameMode == GameAttributes.GameModeType.KillCount)
			{
				uiItem.playerKills = instance.GetScoreByPlayer(player.PlayerId).ToString();
				uiItem.playerScore = instance.GetPointsByTeam(instance.GetTeamId(player.PlayerId)).ToString();
				uiItem.showBothKillsAndPoints = true;
			}
			else if (gameMode != GameAttributes.GameModeType.SuddenDeath || mode == GameAttributes.ModeType.SP)
			{
				uiItem.playerScore = instance.GetScoreByPlayer(player.PlayerId).ToString();
			}
			else
			{
				uiItem.showScoring = false;
			}
			uiItem.isPresent = true;
			uiItem.playerId = player.PlayerId;
			uiItem.teamId = instance.GetTeamId(player.PlayerId);
			CacDisplay componentInChildren = objectExactMatch.GetComponentInChildren<CacDisplay>();
			if (componentInChildren != null)
			{
				CharacterDB.Recipe recipe = CharacterManager.Instance.DeserializeRecipe(player.Attributes);
				componentInChildren.Recipe = recipe;
				int indexNow = i;
				componentInChildren.SpawnPlayer(() =>
				{
					uiItem.OnPlayerReady(GetAnimations(indexNow));
				});
				componentInChildren.PlayBoredAnimations = false;
				componentInChildren.SwapFace = false;
			}
		}
		for (; i < 4; i++)
		{
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Player" + i);
			ResultsPlayerUiItem resultsPlayerUiItem = Util.FindComponent<ResultsPlayerUiItem>(objectExactMatch2);
			resultsPlayerUiItem.isPresent = false;
		}
		if (gameMode != GameAttributes.GameModeType.TeamDeathMatch)
		{
			return;
		}
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Player3");
		if (duplicatePlayerList.Count > 1)
		{
			EB.Sparx.Player player2 = duplicatePlayerList[0];
			int teamId = Scoring.Instance.GetTeamId(player2.PlayerId);
			EB.Sparx.Player player3 = duplicatePlayerList[1];
			int teamId2 = Scoring.Instance.GetTeamId(player3.PlayerId);
			GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "Player1");
			Vector3 localPosition = objectExactMatch4.transform.localPosition;
			if (teamId == teamId2)
			{
				GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "Player0");
				localPosition.y = objectExactMatch5.transform.localPosition.y;
				objectExactMatch4.transform.localPosition = localPosition;
				UIUtils.SetLabelContents(objectExactMatch4, "Label", "1");
			}
			else
			{
				localPosition.y = objectExactMatch3.transform.localPosition.y;
				objectExactMatch4.transform.localPosition = localPosition;
				UIUtils.SetLabelContents(objectExactMatch4, "Label", "2");
			}
		}
		UIUtils.SetLabelContents(objectExactMatch3, "Label", "2");
		GameObject objectExactMatch6 = Util.GetObjectExactMatch(base.gameObject, "Player2");
		Vector3 localPosition2 = objectExactMatch6.transform.localPosition;
		localPosition2.y = objectExactMatch3.transform.localPosition.y;
		objectExactMatch6.transform.localPosition = localPosition2;
		UIUtils.SetLabelContents(objectExactMatch6, "Label", "2");
	}

	private string[] GetAnimations(int positionIdx)
	{
		List<string> list = new List<string>();
		switch (positionIdx)
		{
		case 0:
			list.Add("bot_1st_place_celeb01");
			break;
		case 1:
			list.Add("bot_2nd_place_idle01");
			break;
		case 2:
			list.Add("bot_3rd_place_crying00");
			break;
		case 3:
			list.Add("bot_4th_place_idle");
			break;
		default:
			EB.Debug.LogError("Unsupported position index " + positionIdx);
			break;
		}
		return list.ToArray();
	}

	private void SetPlayerResultString(List<EB.Sparx.Player> duplicatePlayerList)
	{
		Game game = Hub.Instance.GameManager.Game;
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		Scoring instance = Scoring.Instance;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "PlayersPanel");
		int rankByPlayer = instance.GetRankByPlayer(game.LocalPlayer.PlayerId);
		if (gameMode != GameAttributes.GameModeType.TeamDeathMatch)
		{
			for (int i = 0; i < duplicatePlayerList.Count; i++)
			{
				EB.Sparx.Player player = duplicatePlayerList[i];
				if (player.PlayerId == game.LocalPlayer.PlayerId)
				{
					_playerResultString = GetResultsStringForPosition(i);
					break;
				}
			}
		}
		else if (gameMode == GameAttributes.GameModeType.TeamDeathMatch)
		{
			int num = rankByPlayer;
			if (num == 1)
			{
				_playerResultString = "ID_UI_RESULTS_TEAM_WON";
			}
			else
			{
				_playerResultString = "ID_UI_RESULTS_TEAM_LOSS";
			}
		}
		else
		{
			_playerResultString = GetResultsStringForPosition(rankByPlayer - 1);
		}
		UIUtils.SetLabelContents(objectExactMatch, "LabelTitle", _playerResultString);
	}

	private string GetResultsStringForPosition(int i)
	{
		if (isChampionship)
		{
			if (i >= 0 && i < championshipPositionStrings.Length)
			{
				return "ID_UI_RESULTS_PLAYER_" + championshipPositionStrings[i];
			}
		}
		else if (i >= 0 && i < positionStrings.Length)
		{
			return "ID_UI_RESULTS_PLAYER_" + positionStrings[i];
		}
		EB.Debug.LogError("Unsupported number of players " + i);
		return string.Empty;
	}

	private void InitializeButtons()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonsSP");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonsMP");
		GameObject obj = ((!isSP) ? objectExactMatch2 : objectExactMatch);
		objectExactMatch.SetActive(isSP);
		objectExactMatch2.SetActive(!isSP);
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(obj, "ButtonHome");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch3);
		buttonScript.onButtonClick += OnHome;
		if (isChampionship)
		{
			GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch3, "HomeIcon");
			objectExactMatch4.GetComponent<UISprite>().spriteName = "button_back_off";
		}
		if (isSP && !isChampionship)
		{
			Util.GetObjectExactMatch(base.gameObject, "PanelBackground_unlock").SetActive(false);
			Util.GetObjectExactMatch(base.gameObject, "PanelBackground_timer").SetActive(false);
		}
		List<EB.Sparx.Player> endGamePlayers = Scoring.Instance.GetEndGamePlayers();
		bool flag = endGamePlayers[0].PlayerId == Hub.Instance.GameManager.Game.LocalPlayer.PlayerId;
		if (isSP)
		{
			if (endGamePlayers.Count > 0)
			{
				if (!Profile.Instance.IsTutorialDone("tutorial_done") && CampaignData.Instance.CurrentMission == 0 && flag)
				{
					string achievementId = "TUTORIAL";
					Hub.Instance.GameCenterManager.ReportProgress(achievementId, 100.0);
				}
				objectExactMatch3 = Util.GetObjectExactMatch(obj, "ButtonPlayAgain");
				buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch3);
				buttonScript.onButtonClick += OnSinglePlayerPlayAgain;
				GameObject objectExactMatch5 = Util.GetObjectExactMatch(obj, "ButtonPlayNext");
				if (flag)
				{
					objectExactMatch3 = Util.GetObjectExactMatch(obj, "ButtonPlayNext");
					buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch3);
					if (Profile.Instance.IsTutorialDone("sequence_tutorial_results"))
					{
						buttonScript.onButtonClick += OnSinglePlayerPlayNext;
					}
					else
					{
						Profile.Instance.MarkTutorialDone("sequence_tutorial_results");
					}
					if (!isChampionship)
					{
						string achievementId2 = "CAMPAIGN";
						double progress = (double)(CampaignData.Instance.CurrentMission + 1) / (double)CampaignData.Instance.Missions.Count * 100.0;
						Hub.Instance.GameCenterManager.ReportProgress(achievementId2, progress);
					}
				}
				else
				{
					bool flag2 = false;
					if (!isChampionship)
					{
						flag2 = CampaignData.Instance.IsNextMissionUnlocked();
					}
					if (!flag2)
					{
						NGUITools.SetActive(objectExactMatch5, false);
					}
					else
					{
						buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch5);
						buttonScript.onButtonClick += OnSinglePlayerPlayNext;
					}
				}
			}
			if (isChampionship)
			{
				UpdateNextUnlock();
			}
			else
			{
				Util.GetObjectExactMatch(base.gameObject, "NextUnlock").SetActive(false);
			}
		}
		else
		{
			objectExactMatch3 = Util.GetObjectExactMatch(obj, "ButtonStore");
			buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch3);
			buttonScript.onButtonClick += OnStore;
			if (flag)
			{
				StatManager.Instance.IncrementStat(StatManager.Stat.Wins);
			}
			UpdateNextUnlock();
		}
	}

	private IEnumerator DoTutorialPopup()
	{
		yield return new WaitForSeconds(Tuning.Instance.TutorialDoneTimer);
		ScreenManager.Instance.Confirmation(false, "ID_UI_RESULTS_TUTORIAL_DONE_TITLE", "ID_UI_RESULTS_TUTORIAL_DONE_DESC", "ID_UI_RESULTS_HOME", string.Empty, OnTutorialDone);
	}

	private void OnTutorialDone(ConfirmationDialogUI.ButtonIds buttonId)
	{
		Profile.Instance.MarkTutorialDone("tutorial_done");
		switch (buttonId)
		{
		case ConfirmationDialogUI.ButtonIds.Left:
			OnHome(base.gameObject);
			break;
		case ConfirmationDialogUI.ButtonIds.Right:
			OnSinglePlayerPlayNext(base.gameObject);
			break;
		}
	}

	private void FurtherScreens()
	{
		if (_transitioningOut)
		{
			return;
		}
		if (RewardsManager.Instance.DidLevel)
		{
			Game game = Hub.Instance.GameManager.Game;
			if (game != null)
			{
				game.UpdatePlayerAttributes(Misc.CreatePlayerAttributes());
			}
			LevelUpUI component = ScreenManager.Instance.OpenPopup("LevelUpUI", true, true).GetComponent<LevelUpUI>();
			if (component != null)
			{
				component.BackToLobbyTimer = BackToLobbyTimer;
			}
		}
		else if (!Profile.Instance.IsTutorialDone("promo_popups"))
		{
			GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
			GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
			if (mode == GameAttributes.ModeType.MP || championshipMode == GameAttributes.ChampionshipType.kOn)
			{
				HandleContextualPopup();
			}
		}
		else if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn && Profile.Instance.ConsecutiveChampionshipLosses >= Tuning.Instance.ConsecutiveChampionshipLossesForContextualPopups)
		{
			if (CanUpgradeParts())
			{
				Game game2 = Hub.Instance.GameManager.Game;
				int rankByPlayer = Scoring.Instance.GetRankByPlayer(game2.LocalPlayer.PlayerId);
				GameObject gameObject = ScreenManager.Instance.OpenPopup("PromoPopupUI", true, true);
				gameObject.GetComponent<PromoPopupUI>().Init("ID_UI_RESULTS_PROMO_UPGRADE_TITLE", "ID_UI_RESULTS_PROMO_UPGRADE_DESC" + rankByPlayer, PromoPopupUI.ImageType.Robot, OnUpgradePopupDone);
				SetupPromoCloseTimer(gameObject);
			}
			Profile.Instance.ConsecutiveChampionshipLosses = 0;
		}
	}

	private void SetupPromoCloseTimer(GameObject screen)
	{
		EBUI_TransitionInfo eBUI_TransitionInfo = Util.FindComponent<EBUI_TransitionInfo>(screen);
		if (!(eBUI_TransitionInfo == null))
		{
			EBUI_Transition.TransitionPropertyInfo transition_outro = eBUI_TransitionInfo.transition_outro;
			if (transition_outro != null)
			{
				promoPopupCloseTimer = transition_outro.delay + transition_outro.duration + 0.1f;
			}
		}
	}

	private void HandleContextualPopup()
	{
		int num = (int)StatManager.Instance.GetStat(6);
		int num2 = ((num >= PromoGamePopupCounts.Length) ? PromoGamePopupCounts[PromoGamePopupCounts.Length - 1] : PromoGamePopupCounts[num]);
		if (!(StatManager.Instance.GetStat(5) > (float)num2))
		{
			return;
		}
		Game game = Hub.Instance.GameManager.Game;
		int rankByPlayer = Scoring.Instance.GetRankByPlayer(game.LocalPlayer.PlayerId);
		if (Profile.Instance.IsTutorialDone("sequence_tutorial_parts"))
		{
			float[] upgradeProgress = Profile.Instance.recipe.UpgradeProgress;
			float num3 = 0f;
			float[] array = upgradeProgress;
			foreach (float num4 in array)
			{
				num3 += num4;
			}
			num3 /= (float)upgradeProgress.Length;
			if (num3 > upgradeThreshold)
			{
				if (!Profile.Instance.IsTutorialDone("sequence_tutorial_weapons"))
				{
					GameObject gameObject = ScreenManager.Instance.OpenPopup("PromoPopupUI", true, true);
					gameObject.GetComponent<PromoPopupUI>().Init("ID_UI_RESULTS_PROMO_WEAPON_TITLE", "ID_UI_RESULTS_PROMO_WEAPON_DESC" + rankByPlayer, PromoPopupUI.ImageType.Weapon, OnWeaponsPopupDone);
					SetupPromoCloseTimer(gameObject);
					StatManager.Instance.IncrementStat(StatManager.Stat.PromosDisplayed);
				}
				else
				{
					Profile.Instance.MarkTutorialDone("promo_popups");
				}
			}
			else if (HasGenericParts())
			{
				GameObject gameObject2 = ScreenManager.Instance.OpenPopup("PromoPopupUI", true, true);
				gameObject2.GetComponent<PromoPopupUI>().Init("ID_UI_RESULTS_PROMO_PART_TITLE", "ID_UI_RESULTS_PROMO_PART_DESC" + rankByPlayer, PromoPopupUI.ImageType.Robot, OnPartsPopupDone);
				SetupPromoCloseTimer(gameObject2);
				StatManager.Instance.IncrementStat(StatManager.Stat.PromosDisplayed);
			}
			else if (CanUpgradeParts())
			{
				GameObject gameObject3 = ScreenManager.Instance.OpenPopup("PromoPopupUI", true, true);
				gameObject3.GetComponent<PromoPopupUI>().Init("ID_UI_RESULTS_PROMO_UPGRADE_TITLE", "ID_UI_RESULTS_PROMO_UPGRADE_DESC" + rankByPlayer, PromoPopupUI.ImageType.Robot, OnUpgradePopupDone);
				SetupPromoCloseTimer(gameObject3);
				StatManager.Instance.IncrementStat(StatManager.Stat.PromosDisplayed);
			}
		}
		else
		{
			GameObject gameObject4 = ScreenManager.Instance.OpenPopup("PromoPopupUI", true, true);
			gameObject4.GetComponent<PromoPopupUI>().Init("ID_UI_RESULTS_PROMO_PART_TITLE", "ID_UI_RESULTS_PROMO_PART_DESC" + rankByPlayer, PromoPopupUI.ImageType.Robot, OnPartsPopupDone);
			SetupPromoCloseTimer(gameObject4);
			StatManager.Instance.IncrementStat(StatManager.Stat.PromosDisplayed);
		}
		StatManager.Instance.ResetStat(StatManager.Stat.PromoGamesPlayed);
	}

	private bool HasGenericParts()
	{
		int[] parts = Profile.Instance.recipe.parts;
		foreach (int hash in parts)
		{
			if (PartManager.Instance.GetPartAttributes(hash) == null)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanUpgradeParts()
	{
		float num = 0f;
		int[] parts = Profile.Instance.recipe.parts;
		foreach (int num2 in parts)
		{
			num = ((PartManager.Instance.GetPartAttributes(num2) != null) ? (num + Profile.Instance.GetPartUpgradeProgress(num2)) : (num + 100f));
		}
		return !Mathf.Approximately(300f, num);
	}

	private void OnPartsPopupDone(PromoPopupUI.ButtonIds buttonId)
	{
		Telemetry.Event("context_popup", 1, new Hashtable
		{
			{ "type", "parts" },
			{
				"success",
				buttonId == PromoPopupUI.ButtonIds.Ok
			}
		});
		if (buttonId == PromoPopupUI.ButtonIds.Ok)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoPartsStore);
			_transitioningOut = true;
		}
	}

	private void OnUpgradePopupDone(PromoPopupUI.ButtonIds buttonId)
	{
		Telemetry.Event("context_popup", 1, new Hashtable
		{
			{ "type", "upgrades" },
			{
				"success",
				buttonId == PromoPopupUI.ButtonIds.Ok
			}
		});
		if (buttonId == PromoPopupUI.ButtonIds.Ok)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoUpgradeStore);
			_transitioningOut = true;
		}
	}

	private void OnWeaponsPopupDone(PromoPopupUI.ButtonIds buttonId)
	{
		Telemetry.Event("context_popup", 1, new Hashtable
		{
			{ "type", "weapons" },
			{
				"success",
				buttonId == PromoPopupUI.ButtonIds.Ok
			}
		});
		if (buttonId == PromoPopupUI.ButtonIds.Ok)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoWeaponsStore);
			_transitioningOut = true;
		}
	}

	private void EnableInput(bool enable)
	{
		InputEnabled = enable;
	}

	private void OnDestroy()
	{
		EB.Debug.Log("DESTROYING REWARD UI!");
		ReleaseTexture();
		EZAnimator.instance.StopAll();
		EB.Sequence.Component.Activate(base.gameObject, "ResultsUI", typeof(SequenceEvent_OnScreenClosed));
	}

	private void ReleaseTexture()
	{
		if (rewardTexture != null)
		{
			UnityEngine.Object.Destroy(rewardTexture.mainTexture);
			rewardTexture.mainTexture = null;
		}
	}
}
