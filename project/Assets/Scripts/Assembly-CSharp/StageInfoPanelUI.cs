using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class StageInfoPanelUI : MonoBehaviour
{
	private enum FBMode
	{
		FRIENDS = 0,
		WORLD = 1
	}

	private const string SPRITENAME_STAR_FULL = "upgrade_star_full_big";

	private const string SPRITENAME_STAR_EMPTY = "upgrade_star_empty_big";

	private const string LOCID_FORMAT_POINTS = "ID_UI_STAGEINFO_FORMAT_POINTS";

	private const string LOCID_FORMAT_KILLS = "ID_UI_STAGEINFO_FORMAT_KILLS";

	private const int MAX_LB_ITEMS = 11;

	public UILabel LevelNameLabel;

	public UILabel GameModeLabel;

	public GameObject PlayButton;

	public GameObject StoreButton;

	public GameObject FriendsButton;

	public GameObject WorldButton;

	public GameObject FBButton;

	public UITexture GameModeTexture;

	public UITexture LevelTexture;

	public UISprite[] Stars;

	public GameObject TargetScorePortrait;

	public GameObject TargetScoreStar;

	public UILabel TargetScoreLabel;

	public GameObject SpecialRewards;

	public UITexture SpecialRewardsTexture;

	public GameObject SpecialRewardsCount;

	public UILabel SpecialRewardsLabel;

	public GameObject LBPanel;

	public GameObject NoFBPanel;

	public GameObject StageItemPrefab;

	private UITexture _targetScorePortraitTexture;

	private UIGrid _lbGrid;

	private GameObject _closeCatcher;

	private GameObject _infoCatcher;

	private GameObject _divider;

	private GameObject _stageImageHolder;

	private GameObject _starPanel;

	private GameObject _targetHolder;

	private GameObject _targetNone;

	private GameObject _bgFill;

	private GameObject _bgSprite;

	private GameObject _closeButton;

	private string _rewardTextureName;

	private GameObject _infoPanel;

	private GameObject _socialPanel;

	private Vector3 _socialPanelTargetPos;

	private Vector3 _socialPanelHiddenPos;

	private int _division;

	private Division.Stage _stageData;

	private string _levelTexturePath;

	private string _gameModeTexturePath;

	private bool _bExitToStore;

	private bool _bExitToGame;

	private TextureFade _levelTextureFader;

	private TextureFade _specialRewardTextureFader;

	private TextureFade _targetScorePortraitTextureFade;

	private TextureFade _gameModeTextureFader;

	private List<StageInfoPanelUIItem> _lbPanelItems = new List<StageInfoPanelUIItem>();

	private Dictionary<GameObject, Vector3> _basePositionsUpper = new Dictionary<GameObject, Vector3>();

	private Dictionary<GameObject, Vector3> _basePositionsLower = new Dictionary<GameObject, Vector3>();

	private Dictionary<GameObject, Vector3> _baseScales = new Dictionary<GameObject, Vector3>();

	private Dictionary<GameObject, Vector3> _targetHolderBasePositions = new Dictionary<GameObject, Vector3>();

	private FBMode _fbMode;

	private bool isFBLoading;

	private bool isShowing;

	private Vector4 _lbGridPanelClipping;

	private Vector3 _lbGridPosition;

	private int myIndex;

	private void Awake()
	{
		PlayButton.GetComponent<ButtonScript>().onButtonClick += onPlay;
		StoreButton.GetComponent<ButtonScript>().onButtonClick += onStore;
		FriendsButton.GetComponent<ButtonScript>().onButtonClick += onFBFriends;
		WorldButton.GetComponent<ButtonScript>().onButtonClick += onTopPlayers;
		FBButton.GetComponent<ButtonScript>().onButtonClick += onFBConnect;
		_infoPanel = Util.GetObjectExactMatch(base.gameObject, "InfoPanel");
		_socialPanel = Util.GetObjectExactMatch(base.gameObject, "SocialPanel");
		_socialPanelTargetPos = _socialPanel.transform.localPosition;
		_socialPanelHiddenPos = new Vector3(_infoPanel.transform.localPosition.x, _infoPanel.transform.localPosition.y, _socialPanelTargetPos.z);
		_socialPanel.transform.localPosition = _socialPanelHiddenPos;
		_divider = Util.GetObjectExactMatch(_infoPanel, "Divider");
		_stageImageHolder = Util.GetObjectExactMatch(_infoPanel, "StageImageHolder");
		_starPanel = Util.GetObjectExactMatch(_infoPanel, "StarPanel");
		_targetHolder = Util.GetObjectExactMatch(_infoPanel, "TargetHolder");
		_targetNone = Util.GetObjectExactMatch(_infoPanel, "TargetNone");
		GameObject objectExactMatch = Util.GetObjectExactMatch(_infoPanel, "PanelBackground");
		_bgFill = Util.GetObjectExactMatch(objectExactMatch, "Fill");
		_bgSprite = Util.GetObjectExactMatch(objectExactMatch, "BG");
		_targetScorePortraitTexture = TargetScorePortrait.GetComponentInChildren<UITexture>();
		_targetScorePortraitTextureFade = TargetScorePortrait.AddComponent<TextureFade>();
		_lbGrid = Util.FindComponent<UIGrid>(LBPanel);
		_lbGridPosition = _lbGrid.gameObject.transform.localPosition;
		_lbGridPanelClipping = _lbGrid.gameObject.GetComponent<UIPanel>().clipRange;
		_closeCatcher = Util.GetObjectExactMatch(base.gameObject, "CloseCatcher");
		BoxCollider component = _closeCatcher.GetComponent<BoxCollider>();
		component.size = new Vector3(Screen.width, Screen.height, 1f);
		UIEventListener uIEventListener = _closeCatcher.AddComponent<UIEventListener>();
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(onClose));
		_infoCatcher = Util.GetObjectExactMatch(base.gameObject, "InfoCatcher");
		_levelTextureFader = LevelTexture.gameObject.AddComponent<TextureFade>();
		_levelTextureFader.tex = LevelTexture;
		_specialRewardTextureFader = SpecialRewardsTexture.gameObject.AddComponent<TextureFade>();
		_specialRewardTextureFader.tex = SpecialRewardsTexture;
		_gameModeTextureFader = GameModeTexture.gameObject.AddComponent<TextureFade>();
		_gameModeTextureFader.tex = GameModeTexture;
		_closeButton = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
		ButtonScript component2 = _closeButton.GetComponent<ButtonScript>();
		component2.onButtonClick += onClose;
		_basePositionsUpper.Add(LevelNameLabel.gameObject, LevelNameLabel.transform.localPosition);
		_basePositionsUpper.Add(_divider, _divider.transform.localPosition);
		_basePositionsUpper.Add(GameModeLabel.gameObject, GameModeLabel.transform.localPosition);
		_basePositionsUpper.Add(_stageImageHolder, _stageImageHolder.transform.localPosition);
		_basePositionsUpper.Add(_starPanel, _starPanel.transform.localPosition);
		_basePositionsUpper.Add(_targetHolder, _targetHolder.transform.localPosition);
		_basePositionsUpper.Add(_targetNone, _targetNone.transform.localPosition);
		_basePositionsLower.Add(PlayButton, PlayButton.transform.localPosition);
		_basePositionsLower.Add(StoreButton, StoreButton.transform.localPosition);
		_baseScales.Add(_bgFill, _bgFill.transform.localScale);
		_baseScales.Add(_bgSprite, _bgSprite.transform.localScale);
		_baseScales.Add(_infoCatcher, _bgFill.transform.localScale);
		GameObject[] objects = Util.GetObjects(_targetHolder, "LabelTargetScore");
		GameObject[] array = objects;
		foreach (GameObject gameObject in array)
		{
			_targetHolderBasePositions.Add(gameObject, gameObject.transform.localPosition);
		}
	}

	private void Start()
	{
		_lbGrid.Reposition();
	}

	private void OnDestroy()
	{
		TexturePoolManager.Instance.ReleaseTexture(_levelTexturePath);
	}

	private void ClearLBItems()
	{
		foreach (StageInfoPanelUIItem lbPanelItem in _lbPanelItems)
		{
			lbPanelItem.gameObject.transform.parent = null;
			UnityEngine.Object.Destroy(lbPanelItem.gameObject);
		}
		_lbPanelItems.Clear();
	}

	public void SetParameters(int division, Division.Stage stageData)
	{
		if (_levelTexturePath != null)
		{
			TexturePoolManager.Instance.ReleaseTexture(_levelTexturePath);
			_levelTextureFader.HideTexture();
		}
		_division = division;
		_stageData = stageData;
		Division division2 = ChampionshipData.Instance.GetDivision(_division);
		int num = division2.FindIndexForStage(_stageData);
		LevelNameLabel.text = num + 1 + ". " + Localizer.GetString("ID_GAME_ATTRIBUTE_LEVEL_" + _stageData._level.ToUpper());
		GameModeLabel.text = Localizer.GetString(GameAttributes.GameModeTitle[(int)_stageData._mode]);
		_levelTexturePath = "levelthumb_ui/ui_" + _stageData._level.ToLower() + ".jpg";
		TexturePoolManager.Instance.LoadTexture(_levelTexturePath, (Texture2D tex) =>
		{
			if (tex != null)
			{
				LevelTexture.mainTexture = tex;
				_levelTextureFader.SetTexture(LevelTexture);
			}
		});
		if (_gameModeTexturePath != null)
		{
			TexturePoolManager.Instance.ReleaseTexture(_gameModeTexturePath);
			_gameModeTextureFader.HideTexture();
		}
		_gameModeTexturePath = "gamemodes_ui/" + GameAttributes.GameModeIcon[(int)_stageData._mode];
		TexturePoolManager.Instance.LoadTexture(_gameModeTexturePath, (Texture2D loadedTex) =>
		{
			if (loadedTex != null)
			{
				GameModeTexture.mainTexture = loadedTex;
				_gameModeTextureFader.SetTexture(GameModeTexture);
				GameModeTexture.MakePixelPerfect();
				GameModeTexture.transform.localScale = GameModeTexture.transform.localScale * 0.75f;
			}
		});
		int num2 = ChampionshipData.StarState(_division, _stageData);
		for (int num3 = 0; num3 < Stars.Length; num3++)
		{
			Stars[num3].spriteName = ((num2 < num3) ? "upgrade_star_empty_big" : "upgrade_star_full_big");
		}
		_targetNone.SetActive(num2 < 0);
		_targetHolder.SetActive(num2 >= 0);
		LoadOwnTargetScore();
		if (IsFBLinked())
		{
			TargetScoreStar.SetActive(false);
			TargetScorePortrait.SetActive(false);
			NoFBPanel.SetActive(false);
			LBPanel.SetActive(true);
			ClearLBItems();
			_fbMode = FBMode.FRIENDS;
			LoadFBScores();
		}
		else
		{
			LBPanel.SetActive(false);
			NoFBPanel.SetActive(true);
		}
		bool flag = ChampionshipData.StarState(_division, _stageData) < 0;
		bool flag2 = (flag && _stageData._firstTimeRewards.Length > 0) || (!flag && _stageData._regularRewards.Length > 0);
		if (flag2)
		{
			int num4 = 0;
			string uiTexturePath = null;
			if (flag)
			{
				if (_stageData.HasFirstTimeRewardUiOverride())
				{
					num4 = _stageData._firstTimeRewardUiOverride.quantity;
					uiTexturePath = _stageData._firstTimeRewardUiOverride.assetName;
				}
				else
				{
					InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(_stageData._firstTimeRewards[0].assetName));
					if (metaData != null)
					{
						uiTexturePath = metaData.uiTexturePath;
						num4 = _stageData._firstTimeRewards[0].quantity;
					}
					else
					{
						CharacterDB.MetaData metaData2 = CharacterDB.GetMetaData(Hash.StringHash(_stageData._firstTimeRewards[0].assetName));
						if (metaData2 != null)
						{
							uiTexturePath = "cac_ui_parts/" + metaData2.cacTexturePath;
							num4 = _stageData._firstTimeRewards[0].quantity;
						}
					}
				}
			}
			else if (_stageData.HasRegularRewardUiOverride())
			{
				num4 = _stageData._regularRewardUiOverride.quantity;
				uiTexturePath = _stageData._regularRewardUiOverride.assetName;
			}
			else
			{
				InventoryDB.MetaData metaData3 = InventoryDB.GetMetaData(Hash.StringHash(_stageData._regularRewards[0].assetName));
				if (metaData3 != null)
				{
					uiTexturePath = metaData3.uiTexturePath;
					num4 = _stageData._regularRewards[0].quantity;
				}
			}
			flag2 = !string.IsNullOrEmpty(uiTexturePath) && num4 > 0;
			RepositionAndResizeInfoPanel(flag2);
			SpecialRewardsCount.SetActive(flag2);
			SpecialRewardsLabel.text = num4.ToString();
			if (!string.IsNullOrEmpty(_rewardTextureName))
			{
				TexturePoolManager.Instance.ReleaseTexture(_rewardTextureName);
			}
			TexturePoolManager.Instance.LoadTexture(uiTexturePath, (Texture2D tex) =>
			{
				_rewardTextureName = uiTexturePath;
				SpecialRewardsTexture.mainTexture = tex;
				_specialRewardTextureFader.SetTexture(SpecialRewardsTexture);
			});
		}
		RepositionAndResizeInfoPanel(flag2);
	}

	private void LoadFBScores()
	{
		bool flag = _fbMode == FBMode.FRIENDS;
		UIUtils.ApplyTints(FriendsButton, "Background", (!flag) ? Color.white : Color.green);
		UIUtils.ApplyTints(WorldButton, "Background", (!flag) ? Color.green : Color.white);
		Util.GetObjectExactMatch(_socialPanel, "Loading").SetActive(true);
		ClearLBItems();
		Division division = ChampionshipData.Instance.GetDivision(_division);
		if (!(division != null))
		{
			return;
		}
		int stageIndex = division.FindIndexForStage(_stageData);
		if (stageIndex < 0)
		{
			return;
		}
		if (flag)
		{
			ChampionshipLeaderboards.Instance.GetUserPositionAmongFriends(_division, stageIndex, (int userPos, int size, string error) =>
			{
				myIndex = userPos;
				ChampionshipLeaderboards.Instance.GetFriendScores(_division, stageIndex, 10, 0, OnGetLeaderboardDelegate);
			});
		}
		else
		{
			myIndex = -1;
			ChampionshipLeaderboards.Instance.GetTopScores(_division, stageIndex, 10, OnGetLeaderboardDelegate);
		}
	}

	private void LoadOwnTargetScore()
	{
		TargetScoreStar.SetActive(true);
		TargetScorePortrait.SetActive(false);
		int num = 0;
		int points;
		int kills;
		ChampionshipData.Instance.GetHighScore(_division, _stageData, out points, out kills);
		int num2 = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? points : kills);
		string id = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? "ID_UI_STAGEINFO_FORMAT_POINTS" : "ID_UI_STAGEINFO_FORMAT_KILLS");
		for (int num3 = 2; num3 >= 0; num3--)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Star_" + num3);
			if (num2 > _stageData._goldScoreThreshold)
			{
				NGUITools.SetActive(objectExactMatch, false);
			}
			else if (num2 <= _stageData._silverScoreThreshold)
			{
				NGUITools.SetActive(objectExactMatch, false);
				if (num3 == 2)
				{
					break;
				}
			}
			else if (num2 > _stageData._silverScoreThreshold)
			{
				NGUITools.SetActive(objectExactMatch, true);
			}
		}
		UILabel component = Util.GetObjectExactMatch(_targetHolder, "LabelTargetScoreTitle").GetComponent<UILabel>();
		foreach (GameObject key in _targetHolderBasePositions.Keys)
		{
			key.transform.localPosition = _targetHolderBasePositions[key];
		}
		component.text = "ID_UI_STAGEINFO_TARGET_SCORE";
		num = _stageData._silverScoreThreshold;
		if (num2 >= num)
		{
			num = _stageData._goldScoreThreshold;
		}
		if (num2 >= num)
		{
			num = num2;
			foreach (GameObject key2 in _targetHolderBasePositions.Keys)
			{
				key2.transform.localPosition = new Vector3(0f, key2.transform.localPosition.y, key2.transform.localPosition.z);
			}
			TargetScoreStar.SetActive(false);
			component.text = "ID_UI_RESULTS_SCORE";
		}
		EB.Debug.Log(string.Format("Championship[{0}], level: {1}, relevantScore: {2}, targetScore: {3}, silver: {4}, gold: {5}", _division, _stageData._level, num2, num, _stageData._silverScoreThreshold, _stageData._goldScoreThreshold));
		TargetScoreLabel.text = Localizer.Format(id, num);
	}

	private void RepositionAndResizeInfoPanel(bool showSpecialRewards)
	{
		Vector3 vector = ((!showSpecialRewards) ? new Vector3(0f, -40f, 0f) : Vector3.zero);
		foreach (GameObject key in _basePositionsUpper.Keys)
		{
			key.transform.localPosition = _basePositionsUpper[key] + vector;
		}
		vector = ((!showSpecialRewards) ? new Vector3(0f, 46f, 0f) : Vector3.zero);
		foreach (GameObject key2 in _basePositionsLower.Keys)
		{
			key2.transform.localPosition = _basePositionsLower[key2] + vector;
		}
		vector = ((!showSpecialRewards) ? new Vector3(0f, -86f, 0f) : Vector3.zero);
		foreach (GameObject key3 in _baseScales.Keys)
		{
			BoxCollider component = key3.GetComponent<BoxCollider>();
			if (component == null)
			{
				key3.transform.localScale = _baseScales[key3] + vector;
			}
			else
			{
				component.size = _baseScales[key3] + vector;
			}
		}
		SpecialRewards.SetActive(showSpecialRewards);
	}

	public void Show()
	{
		SinglePlayerUI.EnableUserInput(false);
		SinglePlayerUI.FadeControls(false, true, true, 0.5f);
		SinglePlayerUI.LookAway();
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, onShow));
	}

	public void Hide(bool enableSPUI)
	{
		if (enableSPUI)
		{
			SinglePlayerUI.LookAtWorld();
		}
		AnimatePosition.Do(_socialPanel, EZAnimation.ANIM_MODE.FromTo, _socialPanelTargetPos, _socialPanelHiddenPos, EZAnimation.sinusInOut, 0.2f, 0f, null, null);
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, onHide);
	}

	public void onClose(GameObject go)
	{
		Hide(true);
	}

	public bool IsVisible()
	{
		return isShowing;
	}

	private void onShow()
	{
		isShowing = true;
		AnimatePosition.Do(_socialPanel, EZAnimation.ANIM_MODE.FromTo, _socialPanelHiddenPos, _socialPanelTargetPos, EZAnimation.sinusInOut, 0.2f, 0f, null, null);
	}

	private void onHide()
	{
		isShowing = false;
		if (_bExitToGame)
		{
			StoryManager.Instance.StartStory(_stageData, _division);
			if (!StoryManager.Instance.isStoryPlaying)
			{
				ScreenManager.Instance.FadeIn(() =>
				{
					StartCoroutine(DoHide());
				});
			}
			else
			{
				StartCoroutine(DoHide());
			}
		}
		else
		{
			StartCoroutine(DoHide());
		}
	}

	private IEnumerator DoHide()
	{
		if (_bExitToStore)
		{
			yield return StartCoroutine(SinglePlayerUI.Instance.Cleanup());
			_bExitToStore = false;
			SinglePlayerUI sp = ScreenManager.Instance.GetPopup("SinglePlayerUI").GetComponent<SinglePlayerUI>();
			sp.OnExitToStore();
		}
		if (_bExitToGame)
		{
			yield return StartCoroutine(SinglePlayerUI.Instance.Cleanup());
			_bExitToGame = false;
			if (!StoryManager.Instance.isStoryPlaying)
			{
				SetupGame();
			}
			else
			{
				StartCoroutine(WaitForStoryToFinish());
			}
		}
	}

	private void onPlay(GameObject go)
	{
		if (!InventoryDB.HasWeapons())
		{
			ScreenManager.Instance.Confirmation(true, "ID_UI_ERROR_TITLE", "ID_UI_INVENTORY_NOWEAPONS", "ID_UI_RESULTS_STORE", null, OnStoreConfirmation);
		}
		else if (Profile.Instance.Energy > 0)
		{
			_bExitToGame = true;
			Hide(false);
		}
		else
		{
			ScreenManager.Instance.OpenPopup("GetMoreEnergyUI", true, true);
		}
	}

	private void onStore(GameObject go)
	{
		_bExitToStore = true;
		Hide(false);
	}

	private void OnStoreConfirmation(ConfirmationDialogUI.ButtonIds buttonId)
	{
		if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
		{
			onStore(base.gameObject);
		}
	}

	private void onFBFriends(GameObject go)
	{
		if (_fbMode != FBMode.FRIENDS && !isFBLoading)
		{
			isFBLoading = true;
			_fbMode = FBMode.FRIENDS;
			LoadFBScores();
		}
	}

	private void onTopPlayers(GameObject go)
	{
		if (_fbMode != FBMode.WORLD && !isFBLoading)
		{
			isFBLoading = true;
			_fbMode = FBMode.WORLD;
			LoadFBScores();
		}
	}

	private void onFBConnect(GameObject go)
	{
		Hub.Instance.LoginManager.LoginFacebook();
	}

	private bool IsFBLinked()
	{
		return Hub.Instance.FacebookManager.HasAccessToken;
	}

	private void SetupGame()
	{
		ChampionshipLeaderboards.Instance.CachedRank = -1;
		ChampionshipLeaderboards.Instance.CachedLeaderboardResults.Clear();
		ChampionshipData.Instance.SetupGame(_division, _stageData);
		Profile.Instance.ConsumeEnergy(1);
		if (Hub.Instance.FacebookManager.HasAccessToken)
		{
			Division division = ChampionshipData.Instance.GetDivision(_division);
			int stageIndex = division.FindIndexForStage(_stageData);
			ChampionshipLeaderboards.Instance.GetUserPositionAmongFriends(_division, stageIndex, (int userPos, int size, string error) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					EB.Debug.LogWarning("failed to read user position, you will have no FB leaderboard information in high scores popup...");
					ChampionshipData.Instance.PlayGame();
				}
				else
				{
					ChampionshipLeaderboards.Instance.CachedRank = userPos;
					ChampionshipLeaderboards.Instance.GetFriendScoresAbovePosition(_division, stageIndex, 10, userPos, (ChampionshipLeaderboards.PlayerData[] playerData, string err) =>
					{
						if (!string.IsNullOrEmpty(err))
						{
							EB.Debug.LogWarning("failed to read users above player, you will have no FB leaderboard information in high scores popup...");
						}
						else
						{
							ChampionshipLeaderboards.Instance.CachedLeaderboardResults.Clear();
							foreach (ChampionshipLeaderboards.PlayerData item in playerData)
							{
								ChampionshipLeaderboards.Instance.CachedLeaderboardResults.Add(item);
							}
						}
						ChampionshipData.Instance.PlayGame();
					});
				}
			});
		}
		else
		{
			ChampionshipData.Instance.PlayGame();
		}
	}

	private IEnumerator WaitForStoryToFinish()
	{
		while (StoryManager.Instance.isStoryPlaying)
		{
			yield return new WaitForEndOfFrame();
		}
		ScreenManager.Instance.FadeIn(() =>
		{
			SetupGame();
		});
	}

	private void OnGetLeaderboardDelegate(ChampionshipLeaderboards.PlayerData[] playerData, string error)
	{
		Util.GetObjectExactMatch(_socialPanel, "Loading").SetActive(false);
		if (!string.IsNullOrEmpty(error))
		{
			isFBLoading = false;
			EB.Debug.LogError("error reading leaderboards - " + error);
			LoadOwnTargetScore();
			return;
		}
		StageInfoPanelUIItem.ScoreType scoreType = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? StageInfoPanelUIItem.ScoreType.POINTS : StageInfoPanelUIItem.ScoreType.KILLS);
		int num = -1;
		if (scoreType.Equals(StageInfoPanelUIItem.ScoreType.KILLS))
		{
			for (int i = 0; i < playerData.Length; i++)
			{
				ChampionshipLeaderboards.PlayerData playerData2 = playerData[i];
				if (num == playerData2.score)
				{
					playerData2.isTied = true;
				}
				if (i >= 1 && playerData[i].score == playerData[i - 1].score)
				{
					playerData[i - 1].isTied = true;
				}
				num = playerData2.score;
			}
		}
		Division division = ChampionshipData.Instance.GetDivision(_division);
		int num2 = division.FindIndexForStage(_stageData);
		if (myIndex > 9 && num2 >= 0)
		{
			if (_fbMode == FBMode.FRIENDS)
			{
				ChampionshipLeaderboards.Instance.GetFriendScoresAroundPlayer(_division, num2, 0, 0, (ChampionshipLeaderboards.PlayerData[] aroundData, string err) =>
				{
					ShowOnLeaderboard(playerData, aroundData, myIndex);
				});
			}
		}
		else
		{
			ShowOnLeaderboard(playerData, null, 0);
		}
	}

	private void ShowOnLeaderboard(ChampionshipLeaderboards.PlayerData[] topPlayers, ChampionshipLeaderboards.PlayerData[] playersNearMe, int myRank)
	{
		StageInfoPanelUIItem.ScoreType scoreType = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? StageInfoPanelUIItem.ScoreType.POINTS : StageInfoPanelUIItem.ScoreType.KILLS);
		int num = 10;
		if (playersNearMe != null && playersNearMe.Length > 0)
		{
			num = 11;
		}
		ChampionshipLeaderboards.PlayerData[] array = new ChampionshipLeaderboards.PlayerData[num];
		int num2 = Mathf.Min(topPlayers.Length, num);
		if (playersNearMe != null)
		{
			int num3 = num2 - playersNearMe.Length;
			for (int i = 0; i < num3; i++)
			{
				array[i] = topPlayers[i];
			}
			for (int j = 0; j < playersNearMe.Length; j++)
			{
				array[j + num3] = playersNearMe[j];
			}
		}
		else
		{
			array = topPlayers;
		}
		int num4 = Mathf.Min(num, array.Length);
		for (int k = 0; k < num4; k++)
		{
			GameObject gameObject = NGUITools.AddChild(_lbGrid.gameObject, StageItemPrefab);
			if (k <= 9)
			{
				gameObject.name = "StageInfoPanelUIItem0" + k;
			}
			else
			{
				gameObject.name = "StageInfoPanelUIItem" + k;
			}
			StageInfoPanelUIItem component = gameObject.GetComponent<StageInfoPanelUIItem>();
			int rankIndex = k;
			if (playersNearMe != null && playersNearMe[0].fbId == array[k].fbId)
			{
				rankIndex = myRank;
			}
			component.Init(rankIndex);
			component.SetData(array[k], scoreType);
			_lbPanelItems.Add(component);
		}
		_lbGrid.gameObject.transform.localPosition = _lbGridPosition;
		_lbGrid.gameObject.GetComponent<UIPanel>().clipRange = _lbGridPanelClipping;
		_lbGrid.Reposition();
		int points;
		int kills;
		ChampionshipData.Instance.GetHighScore(_division, _stageData, out points, out kills);
		int num5 = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? points : kills);
		int num6 = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? kills : points);
		ChampionshipLeaderboards.PlayerData playerData = null;
		for (int l = 0; l < array.Length; l++)
		{
			if (!(array[l].fbId == Hub.Instance.FacebookManager.LocalUser.FacebookId))
			{
				if (array[l].score > num5)
				{
					playerData = array[l];
				}
				else if (array[l].score == num5 && array[l].altScore > num6)
				{
					playerData = array[l];
				}
			}
		}
		if (playerData == null)
		{
			LoadOwnTargetScore();
		}
		else if (num5 < _stageData._goldScoreThreshold)
		{
			LoadOwnTargetScore();
		}
		else
		{
			TargetScoreStar.SetActive(false);
			TargetScorePortrait.SetActive(true);
			UILabel component2 = Util.GetObjectExactMatch(_targetHolder, "LabelTargetScoreTitle").GetComponent<UILabel>();
			foreach (GameObject key in _targetHolderBasePositions.Keys)
			{
				key.transform.localPosition = _targetHolderBasePositions[key];
			}
			component2.text = "ID_UI_STAGEINFO_TARGET_SCORE";
			string id = ((_stageData._mode != GameAttributes.GameModeType.KillCount) ? "ID_UI_STAGEINFO_FORMAT_POINTS" : "ID_UI_STAGEINFO_FORMAT_KILLS");
			TargetScoreLabel.text = Localizer.Format(id, playerData.score);
			_targetScorePortraitTextureFade.HideTexture();
			_targetScorePortraitTexture.enabled = false;
			if (!string.IsNullOrEmpty(playerData.fbId))
			{
				string imageUrlForUser = FacebookUser.GetImageUrlForUser(playerData.fbId, FacebookUser.ImageType.square);
				FacebookPortraitManager.Load(imageUrlForUser, (Texture2D loadedTex) =>
				{
					_targetScorePortraitTexture.mainTexture = loadedTex;
					_targetScorePortraitTexture.enabled = true;
					_targetScorePortraitTextureFade.SetTexture(_targetScorePortraitTexture);
					_targetScorePortraitTexture.MarkAsChanged();
				});
			}
			else
			{
				CharacterDB.MetaData metaDataOrDefault = CharacterDB.GetMetaDataOrDefault(playerData.recipe, CharacterDB.Category.kHead);
				if (metaDataOrDefault != null)
				{
					TexturePoolManager.Instance.LoadTexture("cac_ui_parts/" + metaDataOrDefault.cacTexturePath, (Texture2D tex) =>
					{
						_targetScorePortraitTexture.mainTexture = tex;
						_targetScorePortraitTexture.enabled = true;
						_targetScorePortraitTextureFade.SetTexture(_targetScorePortraitTexture);
						_targetScorePortraitTexture.MarkAsChanged();
					});
				}
			}
		}
		isFBLoading = false;
	}
}
