using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
	public struct Settings
	{
		[Flags]
		public enum Visibility
		{
			kChat = 1,
			kBoosts = 4
		}

		public const Visibility SPVisFlags = (Visibility)0;

		public const Visibility ChampionshipVisFlags = Visibility.kBoosts;

		public const Visibility MPVisFlags = Visibility.kChat | Visibility.kBoosts;

		public Visibility _eVisFlags;
	}

	private const int GAME_TIME_SLIDER_TOTAL_STEPS = 100;

	private const string TURN_TIMER_SPRITE_NORMAL = "hud_turn_timer_disk";

	private const string TURN_TIMER_SPRITE_RED = "hud_turn_timer_slice";

	private const string TURN_CIRCLE_TEX_NAME = "hud_character_turn_circle";

	private const string CROWN_TEX_NAME = "hud_character_crown_icon";

	private const string PORTRAIT_BG_SPRITE_NORMAL = "hud_portrait_bg";

	private const string PORTRAIT_BG_SPRITE_HIGHLIGHTED = "hud_portrait_outline";

	private const string TELEPORT_SPRITE_ACTIVE = "hud_teleport_button_ON";

	private const string TELEPORT_SPRITE_INACTIVE = "hud_teleport_button_OFF";

	private const string kTeleportGrenadeName = "Teleport Grenade";

	private const float _waitForServerPeriod = 1f;

	public static Settings _settings;

	private static GameHUD _this;

	public GameObject _chatUiPrefab;

	public GameObject _weaponUiPrefab;

	public GameObject _boostUiPrefab;

	public GameObject _impulseBuyPrefab;

	[NonSerialized]
	public float lastInventoryWeaponView;

	public float champSuddenDeathTimerCountdownSecsPerGameMinute = 1f;

	private string[] _impulseBuySkipWeapons = new string[5] { "Teleport Grenade", "Rocket", "Grenade", "Ricochet Grenade", "Spear" };

	private PlayerControlByGUI _playerControl;

	private float _turnTimerRedThreshold;

	private UILabel _message;

	private UILabel _message2;

	private UILabel _turnTimerLbl;

	private UISprite _turnTimerTex;

	private UIFilledSprite _turnTimerDisk;

	private UISprite _signalProblemIndicator;

	private UILabel _gameTimeLeftLbl;

	private UISprite _teleportButtonSprite;

	private UITexture _weaponUiTexture;

	private UISprite _weaponCountCircle;

	private UILabel _weaponCount;

	private Color _weaponCountCircleDefaultColor;

	private UILabel _teleportCount;

	private UILabel _pointsAccumulatorLabel;

	private HudChat _hudChat;

	private ChatUI _chatUI;

	private EmotesPanel _emotesPanel;

	private HUDSubscreenManager _subscreenManager;

	private ImpulseWeaponPurchaseUI _impulseUI;

	private Dictionary<uint, GameObject> _playerPortraits = new Dictionary<uint, GameObject>();

	private int _weaponEquippedBeforeTeleport;

	private string _selectedWeaponTexturePath;

	private UISprite[] _boosts = new UISprite[3];

	private UISprite[] _boostsStateBar = new UISprite[3];

	private float _boostStateBarMaxSize;

	private GameObject _noWeaponIcon;

	private GameObject _noWeaponButton;

	private GameObject _weaponButton;

	private bool isSP;

	private bool isChampionship;

	private bool isTutorial;

	private bool bShowImpulsePurchasePopup;

	private ButtonAim _buttonAim;

	private int _teleportWeaponId = -1;

	private float _timerGameDuration;

	private bool _suddenDeathTimerCountdownRunning;

	private int _timerGameRemaining;

	private float _localTurnExpiredAt;

	private bool _localTurnExpired;

	private bool _visible = true;

	private Misc.Timer _waitingForServerSignal;

	public static GameHUD Instance
	{
		get
		{
			return _this;
		}
	}

	protected int teleportWeaponId
	{
		get
		{
			if (_teleportWeaponId == -1)
			{
				int num = Arsenal.NumItems(InventoryItem.Category.Weapon);
				for (int i = 0; i < num; i++)
				{
					if (Arsenal.ItemName(InventoryItem.Category.Weapon, i) == "Teleport Grenade")
					{
						_teleportWeaponId = i;
					}
				}
			}
			return _teleportWeaponId;
		}
	}

	public bool IsSuddenDeathTimerCountdownRunning
	{
		get
		{
			return _suddenDeathTimerCountdownRunning;
		}
	}

	public int GameTimeSecsRemaining
	{
		get
		{
			return _timerGameRemaining;
		}
	}

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			_visible = value;
			ToggleUIVisibility(Util.GetObjectExactMatch(base.gameObject, "Aim"), _visible);
			ToggleUIVisibility(Util.GetObjectExactMatch(base.gameObject, "Chat"), _visible);
			ToggleUIVisibility(Util.GetObjectExactMatch(base.gameObject, "Move"), _visible);
			ToggleUIVisibility(Util.GetObjectExactMatch(base.gameObject, "Players"), _visible);
			ToggleUIVisibility(Util.GetObjectExactMatch(base.gameObject, "TopButtons"), _visible);
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BottomScreenAnchor");
			ToggleUIVisibility(Util.GetObjectExactMatch(objectExactMatch, "BoostSelectButton"), _visible);
			ToggleUIVisibility(Util.GetObjectExactMatch(objectExactMatch, "TurnTimer"), _visible);
			ToggleUIVisibility(Util.GetObjectExactMatch(objectExactMatch, "WeaponSelectButton"), _visible);
			foreach (Player player in LevelData.Instance.GetPlayers())
			{
				player.OnGameHUDVisible(_visible);
			}
		}
	}

	private Misc.Timer WaitingForServerSignal
	{
		get
		{
			if (_waitingForServerSignal == null)
			{
				_waitingForServerSignal = new Misc.Timer(OnServerWaitPeriodExpired);
			}
			return _waitingForServerSignal;
		}
	}

	public bool PointsAccumulatorEnable
	{
		set
		{
			_pointsAccumulatorLabel.gameObject.SetActive(value);
		}
	}

	private void Awake()
	{
		_this = this;
		_turnTimerRedThreshold = Tuning.Instance.Timer_Fire_Truncate;
		_message = Util.GetObject(base.gameObject, "Message", true).GetComponent<UILabel>();
		_message2 = Util.GetObject(base.gameObject, "Message2", true).GetComponent<UILabel>();
		GameObject obj = Util.GetObject(base.gameObject, "TurnTimer");
		_turnTimerLbl = Util.GetObject(obj, "Turn_timer_label", true).GetComponent<UILabel>();
		_turnTimerTex = Util.GetObject(obj, "Turn_timer_background", true).GetComponent<UISprite>();
		_turnTimerDisk = Util.GetObject(obj, "Turn_timer_disk", true).GetComponent<UIFilledSprite>();
		GameObject gameObject = Util.GetObject(obj, "Signal_Problem");
		if (gameObject != null)
		{
			_signalProblemIndicator = gameObject.GetComponent<UISprite>();
			_signalProblemIndicator.gameObject.SetActive(false);
		}
		_gameTimeLeftLbl = Util.GetObject(base.gameObject, "Time_left_label", true).GetComponent<UILabel>();
		_gameTimeLeftLbl.gameObject.SetActive(false);
		_emotesPanel = Util.FindComponent<EmotesPanel>(base.gameObject);
		_subscreenManager = Util.FindComponent<HUDSubscreenManager>(base.gameObject);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "TeleportButton");
		_teleportButtonSprite = Util.FindComponent<UISprite>(objectExactMatch);
		_weaponButton = Util.GetObjectExactMatch(base.gameObject, "WeaponSelectButton");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(_weaponButton, "TextureSelectedWeapon");
		_weaponUiTexture = Util.FindComponent<UITexture>(objectExactMatch2);
		_noWeaponButton = Util.GetObjectExactMatch(_weaponButton, "TextureNoWeapon");
		_noWeaponButton.SetActive(false);
		_noWeaponIcon = Util.GetObjectExactMatch(_weaponButton, "Icon_cross");
		_noWeaponIcon.SetActive(false);
		_hudChat = Util.GetObject(base.gameObject, "Chat", true).GetComponent<HudChat>();
		GameObject gameObject2 = UnityEngine.Object.Instantiate(_chatUiPrefab) as GameObject;
		gameObject2.transform.parent = base.gameObject.transform;
		gameObject2.transform.localScale = Vector3.one;
		Vector3 localPosition = gameObject2.transform.localPosition;
		localPosition.z = -2f;
		gameObject2.transform.localPosition = localPosition;
		_chatUI = Util.FindComponent<ChatUI>(gameObject2);
		_chatUI._chatMessageHandler = base.gameObject;
		GameObject gameObject3 = UnityEngine.Object.Instantiate(_weaponUiPrefab) as GameObject;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "WeaponUiContainer");
		gameObject3.transform.parent = objectExactMatch3.transform;
		gameObject3.transform.localPosition = Vector3.zero;
		gameObject3.transform.localScale = Vector3.one;
		GameObject topButtons = Util.GetObjectExactMatch(base.gameObject, "TopButtons");
		StoreWeaponInGameUI storeWeaponInGameUi = Util.FindComponent<StoreWeaponInGameUI>(gameObject3);
		_subscreenManager.storeWeaponInGameUi = storeWeaponInGameUi;
		_subscreenManager.weaponUiContainer = objectExactMatch3;
		GameObject gameObject4 = UnityEngine.Object.Instantiate(_boostUiPrefab) as GameObject;
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "BoostUiContainer");
		gameObject4.transform.parent = objectExactMatch4.transform;
		gameObject4.transform.localPosition = Vector3.zero;
		gameObject4.transform.localScale = Vector3.one;
		StoreBoostInGameUI storeBoostInGameUi = Util.FindComponent<StoreBoostInGameUI>(gameObject4);
		_subscreenManager.storeBoostInGameUi = storeBoostInGameUi;
		_subscreenManager.boostUiContainer = objectExactMatch4;
		Coroutines.NextFrame(SafeAction.Wrap(this, () =>
		{
			storeWeaponInGameUi.SetPanelExtentsX(topButtons.transform.localPosition.x);
			storeBoostInGameUi.SetPanelExtentsX(topButtons.transform.localPosition.x);
		}));
		_weaponCount = Util.GetObject(base.gameObject, "Weapon_Count", true).GetComponent<UILabel>();
		_weaponCountCircle = Util.GetObjectExactMatch(base.gameObject, "Weapon_Circle").GetComponent<UISprite>();
		_weaponCountCircleDefaultColor = _weaponCountCircle.color;
		_teleportCount = Util.GetObject(base.gameObject, "Teleport_Count", true).GetComponent<UILabel>();
		_pointsAccumulatorLabel = Util.GetObject(base.gameObject, "PointsAccumulator", true).GetComponent<UILabel>();
		for (int num = 0; num < _boosts.Length; num++)
		{
			_boosts[num] = Util.GetObject(base.gameObject, "boost_" + num, true).GetComponent<UISprite>();
			_boostsStateBar[num] = Util.GetObject(base.gameObject, "FillBar_" + num, true).GetComponent<UISprite>();
		}
		_boostStateBarMaxSize = _boostsStateBar[0].transform.localScale.y;
		_message.text = string.Empty;
		_message2.text = string.Empty;
		_turnTimerLbl.text = string.Empty;
		_selectedWeaponTexturePath = string.Empty;
		EB.Replication.Manager.RegisterRPCs(this);
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		isSP = mode == GameAttributes.ModeType.SP;
		GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
		isChampionship = championshipMode == GameAttributes.ChampionshipType.kOn;
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		isTutorial = gameMode == GameAttributes.GameModeType.Tutorial;
		if (!Profile.Instance.IsTutorialDone("firstboot_shown"))
		{
			Coroutines.NextFrame(SafeAction.Wrap(this, () =>
			{
				NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "InFrontOfMobileContainer"), false);
			}));
		}
		if (!isSP || isChampionship)
		{
			GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "ImpulsePurchaseContainer");
			GameObject gameObject5 = UnityEngine.Object.Instantiate(_impulseBuyPrefab) as GameObject;
			gameObject5.transform.parent = objectExactMatch5.transform;
			gameObject5.transform.localScale = Vector3.one;
			gameObject5.transform.localPosition = new Vector3(0f, 500f, 0f);
			EB.Debug.Log("GameHUD > Awake > impulseUiInstancePos: " + gameObject5.transform.localPosition.ToString());
			_impulseUI = Util.FindComponent<ImpulseWeaponPurchaseUI>(gameObject5);
		}
		_chatUI.gameObject.SetActive((_settings._eVisFlags & Settings.Visibility.kChat) != 0);
		GameObject objectExactMatch6 = Util.GetObjectExactMatch(base.gameObject, "BoostSelectButton");
		NGUITools.SetActive(objectExactMatch6, (_settings._eVisFlags & Settings.Visibility.kBoosts) != 0);
		GameObject objectExactMatch7 = Util.GetObjectExactMatch(base.gameObject, "ButtonAim");
		_buttonAim = objectExactMatch7.GetComponent<ButtonAim>();
		_buttonAim.EquipWeaponLabel = Util.GetObjectExactMatch(base.gameObject, "EquipWeapon");
		UILabel component = Util.GetObjectExactMatch(base.gameObject, "GameModeText").GetComponent<UILabel>();
		if (isChampionship)
		{
			component.text = "ID_GAME_ATTRIBUTE_GAMEMODE_" + gameMode.ToString().ToUpper();
		}
		else
		{
			component.gameObject.SetActive(false);
			float y = component.gameObject.transform.localPosition.y;
			for (int num2 = 0; num2 < 4; num2++)
			{
				GameObject objectExactMatch8 = Util.GetObjectExactMatch(base.gameObject, "PortraitAnchor" + num2);
				objectExactMatch8.transform.localPosition = new Vector3(objectExactMatch8.transform.localPosition.x, y - 20f, objectExactMatch8.transform.localPosition.z);
			}
		}
		base.gameObject.AddComponent<HUDLabels>();
	}

	private void OnDisable()
	{
		_this = null;
		_signalProblemIndicator.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		HUDLabels.Instance.Clear();
	}

	public void HideElementsForTutorialPopup(bool shouldHide)
	{
		if (!isSP && shouldHide)
		{
			_chatUI.TransitionScreen(false);
		}
	}

	public void HideElementsForCeleb()
	{
		NGUITools.SetActive(base.gameObject, false);
		base.gameObject.active = true;
		_this = this;
		NGUITools.SetActive(_message2.gameObject, true);
	}

	public void Initialize()
	{
	}

	public void ToggleSubscreen(HUDSubscreenManager.Subscreen subscreen)
	{
		if (Visible)
		{
			if (subscreen == HUDSubscreenManager.Subscreen.BoostInventory || subscreen == HUDSubscreenManager.Subscreen.WeaponInventory)
			{
				_chatUI.TransitionScreen(false);
			}
			_subscreenManager.ToggleSubScreen(subscreen);
		}
	}

	public GameObject AddNewPortrait(int objectIndex)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("GameHUDPortrait")) as GameObject;
		GameObject obj = Util.GetObject(base.gameObject, "Players", true);
		GameObject objectExactMatch = Util.GetObjectExactMatch(obj, "PortraitAnchor" + objectIndex);
		Util.Parent(objectExactMatch, gameObject);
		gameObject.transform.localPosition = Vector3.zero;
		HudPlayerPortrait component = gameObject.GetComponent<HudPlayerPortrait>();
		component.Initialize("hud_character_turn_circle", "hud_character_crown_icon");
		return gameObject;
	}

	[RPC]
	public void NewMessageCallback(uint playerId, string message)
	{
		if (message == null)
		{
			return;
		}
		message = message.Trim();
		if (message.Length < 1)
		{
			return;
		}
		message = ProfanityFilter.Filter(message);
		foreach (EB.Sparx.Player player in Hub.Instance.GameManager.Game.Players)
		{
			int teamId = Scoring.Instance.GetTeamId(player.PlayerId);
			if (player.PlayerId == playerId)
			{
				Color textColor = Tuning.Instance.TeamColors[teamId];
				bool isLocalPlayer = player.PlayerId == EB.Replication.Manager.LocalPlayerId;
				string playerName = Dot.String("name", player.Attributes, string.Empty);
				if (!Profile.Instance.UserMuted(player.UserId.Value))
				{
					_hudChat.AddMessage(playerName, message, textColor);
				}
				HudPlayerPortrait component = _playerPortraits[player.PlayerId].GetComponent<HudPlayerPortrait>();
				_chatUI.AddMessage(component.PortraitTexturePath, playerName, message, isLocalPlayer, textColor, player.UserId);
				break;
			}
		}
	}

	public void SendChatMessage(string message)
	{
		message = message.Trim();
		if (message.Length >= 1)
		{
			EB.Replication.Manager.RPC("NewMessageCallback", RPCMode.AllBuffered, EB.Replication.Manager.LocalPlayerId, message);
		}
	}

	public void OnKeyboardHidden(string message)
	{
	}

	public void OnLeaveGame()
	{
		_chatUI.TransitionScreen(false);
		bool showTransition = true;
		string title;
		string desc;
		if (isSP)
		{
			title = "ID_UI_EXITGAME_SP_TITLE";
			desc = ((!GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kResetable) || !GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kDetonationVis)) ? "ID_UI_EXITGAME_SP_BODY" : "ID_UI_EXITGAME_SP_BODY_NO_PROGRESS_CHECK");
			showTransition = !LevelData.Instance.PauseGame;
		}
		else
		{
			title = "ID_UI_EXITGAME_MP_TITLE";
			desc = "ID_UI_EXITGAME_MP_BODY";
		}
		if (ScreenManager.Instance.GetPopup("ConfirmationDialogUI") == null)
		{
			ScreenManager.Instance.Confirmation(true, title, desc, "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", LeaveGameConfirmation, showTransition);
		}
	}

	private void LeaveGameConfirmation(ConfirmationDialogUI.ButtonIds buttonId)
	{
		EB.Debug.Log("THIS IS LEAVING THE GAME CONFIRMATION!!!");
		ScreenManager.Instance.EnableBgFade(false);
		WeaponMonitor.Instance.CleanUp();
		if (buttonId != ConfirmationDialogUI.ButtonIds.Left)
		{
			return;
		}
		ScreenManager.Instance.FadeIn(() =>
		{
			GameManager gameManager = Hub.Instance.GameManager;
			if (isSP)
			{
				if (LevelData.Instance.PauseGame)
				{
					LevelData.Instance.PauseGame = false;
				}
				if (isChampionship)
				{
					Hashtable hashtable = new Hashtable();
					ChampionshipData.Instance.AddTelemetryEventData(hashtable);
					Scoring instance = Scoring.Instance;
					Game game = Hub.Instance.GameManager.Game;
					int scoreByPlayer = instance.GetScoreByPlayer(game.LocalPlayer.PlayerId);
					hashtable["result"] = "quit";
					hashtable["partsLevel"] = Profile.Instance.recipe.GetAverageTier();
					bool flag = false;
					Division.Stage selectedStage = ChampionshipData.Instance.GetSelectedStage();
					if (selectedStage != null)
					{
						int selectedDivision = ChampionshipData.Instance.GetSelectedDivision();
						flag = ChampionshipData.Instance.IsFirstTime(selectedDivision, selectedStage);
					}
					hashtable["firstTime"] = flag;
					hashtable["finalscore"] = scoreByPlayer;
					UnityEngine.Debug.LogWarning("Final Score " + scoreByPlayer);
					Telemetry.Event("champ_result", 1, hashtable);
				}
				else
				{
					Telemetry.Event("sp_quit/" + GameAttributes.GetValue(GameAttributes.AttributeType.CampaignData));
				}
			}
			else
			{
				Telemetry.Event("mp_quit/" + GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
			}
			GameObject gameObject = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
			gameObject.BroadcastMessage("CloseAllOverlays", SendMessageOptions.DontRequireReceiver);
			if (isChampionship)
			{
				List<string> nextScreenStack = new List<string> { "HomeUI", "SinglePlayerUI" };
				Setup.Instance.nextScreenStack = nextScreenStack;
			}
			gameManager.LeaveGame();
		});
	}

	public void UpdatePlayerPortrait(uint playerId, int score, int rank)
	{
		GameObject gameObject = _playerPortraits[playerId];
		HudPlayerPortrait component = gameObject.GetComponent<HudPlayerPortrait>();
		component.Score = score;
		component.Rank = rank;
	}

	public void UpdateGameTime(int secondsLeft, int total)
	{
		_timerGameRemaining = secondsLeft;
		string text = (secondsLeft / 60).ToString();
		if (text.Length == 1)
		{
			text = "0" + text;
		}
		string text2 = (secondsLeft % 60).ToString();
		if (text2.Length == 1)
		{
			text2 = "0" + text2;
		}
		_gameTimeLeftLbl.text = string.Format("{0}:{1}", text, text2);
	}

	public void UpdateCurrentPlayerName(string playerName, uint playerId)
	{
		foreach (uint key in _playerPortraits.Keys)
		{
			GameObject gameObject = _playerPortraits[key];
			HudPlayerPortrait component = gameObject.GetComponent<HudPlayerPortrait>();
			component.ToggleCurrentPlayer(key == playerId);
		}
	}

	public void ClearTimer()
	{
		StopCoroutine("_ShowTurnTimer");
		_turnTimerLbl.text = string.Empty;
		if (bShowImpulsePurchasePopup)
		{
			bShowImpulsePurchasePopup = false;
			_impulseUI.Show();
		}
	}

	public void ClearMessage()
	{
		_message.text = string.Empty;
	}

	public void ClearMessage2()
	{
		_message2.text = string.Empty;
	}

	public void ShowTurnTimer(float duration)
	{
		ReceivedServerSignal();
		StopCoroutine("_ShowTurnTimer");
		StartCoroutine("_ShowTurnTimer", duration);
	}

	public void SetPlayerOrder(List<uint> order)
	{
		Game game = Hub.Instance.GameManager.Game;
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		for (int i = 0; i < game.Players.Count; i++)
		{
			GameObject gameObject = AddNewPortrait(i);
			HudPlayerPortrait component = gameObject.GetComponent<HudPlayerPortrait>();
			if (i >= game.Players.Count)
			{
				component.SetAsPlaceholder();
				continue;
			}
			EB.Sparx.Player player = game.GetPlayer(order[i]);
			component.PlayerId = player.PlayerId;
			component.TeamId = Scoring.Instance.GetTeamId(player.PlayerId);
			component.Name = Dot.String("name", player.Attributes, string.Empty);
			component.Score = 0;
			component.Rank = i + 1;
			CharacterDB.Recipe recipe = new CharacterDB.Recipe(player.Attributes["recipe"]);
			int part = recipe.GetPart(CharacterDB.Category.kHead);
			CharacterDB.MetaData metaDataOrDefault = CharacterDB.GetMetaDataOrDefault(part, CharacterDB.Category.kHead);
			if (metaDataOrDefault != null)
			{
				component.PortraitTexturePath = "cac_ui_parts/" + metaDataOrDefault.cacTexturePath;
			}
			if (gameMode == GameAttributes.GameModeType.SuddenDeath)
			{
				component.HideRank();
			}
			_playerPortraits.Add(player.PlayerId, gameObject);
			ToggleUIVisibility(gameObject, Visible);
		}
	}

	[RPC]
	public void ShowGameTimer(float duration)
	{
		ReceivedServerSignal();
		StopCoroutine("_ShowGamerTimer");
		StartCoroutine("_ShowGamerTimer", duration);
	}

	private IEnumerator _ShowTurnTimer(float duration)
	{
		_signalProblemIndicator.gameObject.SetActive(false);
		_turnTimerTex.spriteName = "hud_turn_timer_disk";
		_turnTimerTex.color = new Color(1f, 1f, 1f);
		float total = duration;
		while (duration > 0f)
		{
			string durationString = ((int)Mathf.Ceil(duration)).ToString();
			if (durationString.Length == 1)
			{
				durationString = "0" + durationString;
			}
			_turnTimerLbl.text = durationString;
			_turnTimerDisk.fillAmount = duration / total;
			if (duration < _turnTimerRedThreshold && !_turnTimerTex.spriteName.Equals("hud_turn_timer_slice"))
			{
				_turnTimerTex.spriteName = "hud_turn_timer_slice";
				_turnTimerTex.color = new Color(1f, 0f, 0f);
			}
			yield return new WaitForFixedUpdate();
			duration -= UnityEngine.Time.fixedDeltaTime;
		}
		GameObject p = LevelData.Instance.GetPlayerObject();
		if (p != null)
		{
			p.SendMessage("EnableInput", false, SendMessageOptions.DontRequireReceiver);
		}
		OnTurnTimerExpired();
		_localTurnExpired = true;
		_localTurnExpiredAt = UnityEngine.Time.realtimeSinceStartup;
		_turnTimerLbl.text = string.Empty;
		if (bShowImpulsePurchasePopup)
		{
			bShowImpulsePurchasePopup = false;
			_impulseUI.Show();
		}
	}

	private IEnumerator _ShowGamerTimer(float duration)
	{
		GUI.color = Color.magenta;
		_timerGameDuration = duration;
		_gameTimeLeftLbl.gameObject.SetActive(true);
		int total = Mathf.RoundToInt(_timerGameDuration);
		while (true)
		{
			float percent = (float)VariableManager.Instance.GetNumber("game_timer", 0.0);
			int remaining = Mathf.CeilToInt(Mathf.Clamp01(1f - percent) * _timerGameDuration);
			UpdateGameTime(remaining, total);
			yield return new WaitForFixedUpdate();
		}
	}

	public void ShowMessage(string text)
	{
		_message.text = text;
		CancelInvoke("ClearMessage");
	}

	public void ShowMessage(string text, float duration)
	{
		CancelInvoke("ClearMessage");
		_message.text = text;
		Invoke("ClearMessage", duration);
	}

	public void ShowMessage2(string text)
	{
		StopCoroutine("ResetMessage2Delayed");
		_message2.text = text;
	}

	public void ShowMessage2(string text, float duration)
	{
		_message2.text = text;
		StopCoroutine("ResetMessage2Delayed");
		StartCoroutine("ResetMessage2Delayed", duration);
	}

	private IEnumerator ResetMessage2Delayed(float delay)
	{
		yield return new WaitForSeconds(delay);
		_message2.text = string.Empty;
	}

	public void OnHudElementOpened(GameObject caller)
	{
		if (_emotesPanel != null && _emotesPanel.gameObject != caller)
		{
			_emotesPanel.ShowPanel(false);
		}
		GameObject obj = base.gameObject.transform.parent.gameObject;
		WeaponInventoryGameplayUI weaponInventoryGameplayUI = Util.FindComponent<WeaponInventoryGameplayUI>(obj);
		if (weaponInventoryGameplayUI != null && weaponInventoryGameplayUI.gameObject != caller)
		{
			weaponInventoryGameplayUI.OnCloseInventory(caller);
		}
	}

	public void OnTeleportButtonPressed(GameObject caller)
	{
		if (!Visible)
		{
			return;
		}
		InventoryManager inventoryManager = Hub.Instance.InventoryManager;
		Player player = LevelData.Instance.GetPlayer();
		int num = player.WeaponIndex();
		if (num == teleportWeaponId)
		{
			if (player.CanUseWeapon(_weaponEquippedBeforeTeleport))
			{
				player.SelectWeapon(_weaponEquippedBeforeTeleport);
			}
			else if (inventoryManager.IsOverriden)
			{
				player.SelectFirstOverrideWeapon();
			}
			else
			{
				player.SelectFirstAvailableWeapon();
			}
		}
		else if (player.CanUseWeapon(teleportWeaponId))
		{
			player.SelectWeapon(teleportWeaponId);
			_weaponEquippedBeforeTeleport = num;
		}
		else if (!inventoryManager.IsOverriden && !_playerControl.IsAiming)
		{
			GameObject obj = ScreenManager.Instance.OpenPopup("StorePurchaseWeaponUI", true, true);
			StorePurchaseWeaponUI storePurchaseWeaponUI = Util.FindComponent<StorePurchaseWeaponUI>(obj);
			storePurchaseWeaponUI.SetParameters("Teleport Grenade", InventoryItem.Category.Weapon, true);
			storePurchaseWeaponUI.SetParentScreen(base.gameObject);
		}
	}

	private void OnPurchaseComplete(InventoryDB.MetaData metadata)
	{
		Player player = LevelData.Instance.GetPlayer();
		if (metadata.name == "Teleport Grenade" && player.CanUseWeapon(teleportWeaponId))
		{
			_weaponEquippedBeforeTeleport = player.WeaponIndex();
			player.SelectWeapon(teleportWeaponId);
			ScreenManager.Instance.ClosePopup("StorePurchaseWeaponUI");
		}
	}

	public void OnWeaponFired(Player player)
	{
		if (player.PlayerId != EB.Replication.Manager.LocalPlayerId)
		{
			return;
		}
		if (player.WeaponIndex() == teleportWeaponId && _weaponEquippedBeforeTeleport != -1 && player.CanUseWeapon(_weaponEquippedBeforeTeleport))
		{
			player.SelectWeapon(_weaponEquippedBeforeTeleport);
		}
		if (!(null != _impulseUI) || ImpulseWeaponPurchaseUI.IsActive || !((float)_timerGameRemaining >= Tuning.Instance.RevengeSaleMinimumGameTimeSecs) || isTutorial || (isSP && (!isSP || !isChampionship)))
		{
			return;
		}
		int index = player.WeaponIndex();
		if (!player.CanUseWeapon(index))
		{
			Weapon weapon = Arsenal.WeaponAt(index);
			InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(weapon.name));
			if (Array.Exists(_impulseBuySkipWeapons, (string obj) => obj.Equals(weapon.name)))
			{
				EB.Debug.Log("OnWeaponFired: suppressing impulse weapon buy because weapon is in exclusion list: " + weapon.name);
				return;
			}
			if (metaData.IsHidden)
			{
				EB.Debug.Log("OnWeaponFired: suppressing impulse weapon buy because weapon is marked as hidden: " + weapon.name);
				return;
			}
			if (metaData.isChampionshipTierReward && metaData.IsLocked)
			{
				EB.Debug.Log("OnWeaponFired: suppressing impulse weapon buy because weapon is campaign-locked: " + weapon.name);
				return;
			}
			EB.Debug.Log("OnWeaponFired: showing impulseUI for weapon " + weapon.name);
			_impulseUI.SetData(ImpulseWeaponPurchaseUI.Mode.kImpulse, weapon.name, null, InventoryItem.Category.Weapon);
			bShowImpulsePurchasePopup = true;
		}
	}

	public void OnWeaponSelected(Player player)
	{
		if (player.PlayerId != EB.Replication.Manager.LocalPlayerId)
		{
			return;
		}
		int num = player.WeaponIndex();
		Weapon weapon = Arsenal.WeaponAt(num);
		if (weapon != null)
		{
			string uiTexturePath = weapon.uiTexturePath;
			if (!string.IsNullOrEmpty(_selectedWeaponTexturePath))
			{
				_weaponUiTexture.mainTexture = null;
				_weaponUiTexture.enabled = false;
				TexturePoolManager.Instance.ReleaseTexture(_selectedWeaponTexturePath);
			}
			if (weapon.name.ToLower() == "empty")
			{
				DisplayNoWeapon(true);
				_selectedWeaponTexturePath = string.Empty;
			}
			else
			{
				DisplayNoWeapon(false);
				_selectedWeaponTexturePath = uiTexturePath;
				TexturePoolManager.Instance.LoadTexture(uiTexturePath, this, OnSelectedWeaponTextureLoaded);
			}
		}
		if (num == teleportWeaponId)
		{
			_teleportButtonSprite.spriteName = "hud_teleport_button_ON";
		}
		else
		{
			_teleportButtonSprite.spriteName = "hud_teleport_button_OFF";
		}
		UpdateCounts();
		int count = Hub.Instance.InventoryManager.GetCount(weapon.name);
		if (count > 0)
		{
			DisplayNoWeapon(false);
		}
		bool flag = num == Arsenal.EmptyWeaponIndex;
		Util.GetObjectExactMatch(_weaponButton, "Weapon_Circle").SetActive(!flag);
		Util.GetObjectExactMatch(_weaponButton, "Weapon_Count").SetActive(!flag);
		_buttonAim.ShouldShowEquipWeaponLabel = flag;
	}

	protected virtual void OnSelectedWeaponTextureLoaded(Texture2D texture)
	{
		if (texture != null)
		{
			_weaponUiTexture.mainTexture = texture;
			_weaponUiTexture.enabled = _visible;
		}
	}

	private void DisplayNoWeapon(bool display)
	{
		_noWeaponButton.SetActive(display);
		_noWeaponIcon.SetActive(display);
		_weaponUiTexture.gameObject.SetActive(!display);
		_weaponCount.gameObject.SetActive(!display);
		_weaponCountCircle.gameObject.SetActive(!display);
	}

	private IEnumerator Start()
	{
		if (!isTutorial)
		{
			Visible = false;
		}
		while (_playerControl == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		GameObject cameraObj = Util.GetObjectExactMatch(ScreenManager.Instance.gameObject, "Camera");
		if (cameraObj != null)
		{
			Camera camera = cameraObj.GetComponent<Camera>();
			if (camera != null)
			{
				foreach (Transform t in base.gameObject.transform)
				{
					UIAnchor anchor = t.gameObject.GetComponent<UIAnchor>();
					if (anchor != null)
					{
						anchor.uiCamera = camera;
					}
					UIStretch stretch = t.gameObject.GetComponent<UIStretch>();
					if (stretch != null)
					{
						stretch.uiCamera = camera;
					}
				}
			}
			else
			{
				EB.Debug.LogError("No UI Camera!");
			}
		}
		else
		{
			EB.Debug.LogError("No UI Camera!");
		}
		GameObject buttonObject = Misc.GetObjectExactMatch(base.gameObject, "ButtonJump");
		if (buttonObject != null)
		{
			buttonObject.SetActive(false);
		}
		if (DebugSettings.SlideJumpLeft)
		{
			GameObject moveButton = Misc.GetObjectExactMatch(base.gameObject, "ButtonMove");
			if (moveButton != null)
			{
				moveButton.SetActive(false);
			}
			moveButton = Misc.GetObjectExactMatch(base.gameObject, "ButtonJumpMove");
			if (moveButton != null)
			{
				if (DebugSettings.JoystickLeft)
				{
					moveButton.SetActive(false);
				}
				else
				{
					moveButton.SetActive(true);
				}
			}
			moveButton = Misc.GetObjectExactMatch(base.gameObject, "ButtonJoystick");
			if (moveButton != null)
			{
				if (DebugSettings.JoystickLeft)
				{
					moveButton.SetActive(true);
				}
				else
				{
					moveButton.SetActive(false);
				}
			}
		}
		if (Hub.Instance.InventoryManager.IsOverriden && Hub.Instance.InventoryManager.OverrideCount == 0)
		{
			DisplayNoWeapon(true);
			yield break;
		}
		EB.Debug.Log("IsOverriden = " + Hub.Instance.InventoryManager.IsOverriden + ", OverrideCount = " + Hub.Instance.InventoryManager.OverrideCount);
	}

	private void OnPlayerReady()
	{
		GameObject playerObject = LevelData.Instance.GetPlayerObject();
		if (playerObject != null)
		{
			_playerControl = playerObject.GetComponent<PlayerControlByGUI>();
		}
		UpdateCounts();
		UpdateBoosts();
		CameraController component = Camera.mainCamera.GetComponent<CameraController>();
		if (!(component == null))
		{
		}
	}

	private void OnZoomBarChanged(UIScrollBar bar)
	{
		CameraController component = Camera.mainCamera.GetComponent<CameraController>();
		if (!(component == null))
		{
			component.Zoom01 = bar.scrollValue;
		}
	}

	private void OnInventoryUpdated()
	{
		UpdateCounts();
	}

	private void UpdateCounts()
	{
		Player player = LevelData.Instance.GetPlayer();
		if (player != null)
		{
			Weapon currentWeapon = player.CurrentWeapon;
			if (currentWeapon != null)
			{
				int count = Hub.Instance.InventoryManager.GetCount(currentWeapon.name);
				if (((currentWeapon.IsFree || Hub.Instance.InventoryManager.EverythingFree) && !isSP) || count > 99999)
				{
					_weaponCount.text = "∞";
					_weaponCount.color = Color.black;
					_weaponCountCircle.color = _weaponCountCircleDefaultColor;
				}
				else if (isTutorial)
				{
					_weaponCount.text = "∞";
					_weaponCount.color = Color.black;
					_weaponCountCircle.color = _weaponCountCircleDefaultColor;
				}
				else
				{
					count = Mathf.Clamp(count, 0, 99);
					_weaponCount.text = count.ToString();
					_weaponCountCircle.color = ((count > 1) ? _weaponCountCircleDefaultColor : Color.red);
					_weaponCount.color = ((count > 1) ? Color.black : Color.white);
				}
			}
		}
		int count2 = Hub.Instance.InventoryManager.GetCount("Teleport Grenade");
		count2 = Mathf.Clamp(count2, 0, 99);
		_teleportCount.text = count2.ToString();
		if (isTutorial)
		{
			_teleportCount.text = "∞";
		}
	}

	private void UpdateBoosts()
	{
		Player player = LevelData.Instance.GetPlayer();
		if (player == null)
		{
			return;
		}
		for (int i = 0; i < _boosts.Length; i++)
		{
			Player.ActiveBoost activeBoost = player.GetActiveBoost(i);
			if (activeBoost.Valid && activeBoost.GetState() == Player.ActiveBoost.State.kActive)
			{
				_boostsStateBar[i].gameObject.SetActive(true);
				UISprite uISprite = _boostsStateBar[i];
				uISprite.pivot = UIWidget.Pivot.Bottom;
				uISprite.transform.localPosition = new Vector3(uISprite.transform.localPosition.x, (0f - _boostStateBarMaxSize) / 2f, uISprite.transform.localPosition.z);
				uISprite.color = Color.green;
				string text = BoostItem.GetHudIcon(activeBoost.Boost._type);
				if (text == null)
				{
					EB.Debug.LogWarning("Invalid boost type activated, displaying the iron cross!");
					text = "hud_cross";
				}
				_boosts[i].spriteName = text;
				_boosts[i].MakePixelPerfect();
			}
			else
			{
				_boosts[i].spriteName = "hud_cross";
				_boosts[i].MakePixelPerfect();
				_boostsStateBar[i].gameObject.SetActive(false);
			}
		}
	}

	private void Update()
	{
		Player player = LevelData.Instance.GetPlayer();
		if (!(player != null))
		{
			return;
		}
		for (int i = 0; i < _boosts.Length; i++)
		{
			Player.ActiveBoost activeBoost = player.GetActiveBoost(i);
			if (activeBoost.Valid && activeBoost.GetState() != Player.ActiveBoost.State.kCooldown)
			{
				Vector3 localScale = _boostsStateBar[i].transform.localScale;
				float num = activeBoost.RemainingDurationPercentage();
				localScale = new Vector3(localScale.x, _boostStateBarMaxSize * num, localScale.z);
				_boostsStateBar[i].transform.localScale = localScale;
				if (num == 0f)
				{
					_boostsStateBar[i].gameObject.SetActive(false);
				}
				else if (activeBoost.GetState() == Player.ActiveBoost.State.kActive)
				{
					_boostsStateBar[i].color = ((!(num < 0.5f)) ? Color.green : Color.yellow);
				}
			}
		}
	}

	private void EndTurn(uint playerId)
	{
		ReceivedServerSignal();
	}

	private void OnPlayerLeft(EB.Sparx.Player player)
	{
		ShowMessage2(Localizer.Format("ID_GAME_MESSAGE_PLAYER_LEFT", player.Attributes["name"] as string), Tuning.Instance.Timer_ShowYourTurn);
		GameObject value;
		if (_playerPortraits.TryGetValue(player.PlayerId, out value))
		{
			value.SendMessage("MarkLeft");
		}
	}

	public Vector3 PortraitPosition(uint playerID)
	{
		GameObject value;
		if (_playerPortraits.TryGetValue(playerID, out value))
		{
			Transform transform = value.transform.Find("Portrait");
			if (transform != null)
			{
				return transform.position;
			}
		}
		return Vector3.zero;
	}

	public void PointsSucked(uint playerID)
	{
		AudioEmitter component = GetComponent<AudioEmitter>();
		if (component != null)
		{
			component.Play();
		}
		GameObject value;
		if (_playerPortraits.TryGetValue(playerID, out value))
		{
			value.SendMessage("StartThrob");
		}
	}

	public void EnableGameHUDElement(string elementName, bool enable)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, elementName);
		if (objectExactMatch != null)
		{
			NGUITools.SetActive(objectExactMatch, enable);
		}
	}

	public void DoSuddenDeathTimerCountdown()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		if (mode != GameAttributes.ModeType.SP || gameMode != GameAttributes.GameModeType.SuddenDeath || GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) != GameAttributes.ChampionshipType.kOn)
		{
			return;
		}
		Scoring.Instance.SortEndGamePlayers();
		List<EB.Sparx.Player> endGamePlayers = Scoring.Instance.GetEndGamePlayers();
		EB.Sparx.Player player = endGamePlayers[0];
		if (player != null && player.PlayerId == game.LocalPlayer.PlayerId)
		{
			Division.Stage selectedStage = ChampionshipData.Instance.GetSelectedStage();
			float num = (float)VariableManager.Instance.GetNumber("game_timer", 0.0);
			if (selectedStage != null)
			{
				float num2 = ((selectedStage.gameTime <= 0) ? Tuning.Instance.Timer_SuddenDeath : ((float)selectedStage.gameTime));
				float num3 = num2 * (1f - num);
				int bonusPoints = Mathf.FloorToInt(num3 * Tuning.Instance.Scoring_Championship_SuddenDeath_TimerBonusMultiplier);
				_suddenDeathTimerCountdownRunning = true;
				StartCoroutine(_SuddenDeathTimerCountdown(bonusPoints));
			}
		}
	}

	private IEnumerator _SuddenDeathTimerCountdown(int bonusPoints)
	{
		StopCoroutine("_ShowGamerTimer");
		float percent = (float)VariableManager.Instance.GetNumber("game_timer", 0.0);
		int remaining = Mathf.CeilToInt(Mathf.Clamp01(1f - percent) * _timerGameDuration);
		int totalRemaining = remaining;
		float timeStart = UnityEngine.Time.realtimeSinceStartup;
		float totalCountdownDuration = (float)remaining / 60f * champSuddenDeathTimerCountdownSecsPerGameMinute;
		while (remaining > 0)
		{
			float timeElapsed = UnityEngine.Time.realtimeSinceStartup - timeStart;
			remaining -= (int)(timeElapsed / totalCountdownDuration * (float)totalRemaining);
			remaining = Mathf.Clamp(remaining, 0, remaining);
			string minutes = (remaining / 60).ToString();
			if (minutes.Length == 1)
			{
				minutes = "0" + minutes;
			}
			string seconds = (remaining % 60).ToString();
			if (seconds.Length == 1)
			{
				seconds = "0" + seconds;
			}
			_gameTimeLeftLbl.text = string.Format("{0}:{1}", minutes, seconds);
			yield return new WaitForFixedUpdate();
		}
		EB.Debug.Log("awarding " + bonusPoints + " bonus points for sudden death time remaining");
		Scoring.Instance.AddScore(EB.Replication.Manager.LocalPlayerId, bonusPoints);
		_suddenDeathTimerCountdownRunning = false;
	}

	public void ShowRevengeSalePopup(string weaponName, string targetPlayerName)
	{
		if (_impulseUI != null && !ImpulseWeaponPurchaseUI.IsActive && !bShowImpulsePurchasePopup)
		{
			EB.Debug.Log("Showing revenge sale popup for weapon: " + weaponName + ", target: " + targetPlayerName);
			_impulseUI.SetData(ImpulseWeaponPurchaseUI.Mode.kRevenge, weaponName, targetPlayerName, InventoryItem.Category.Weapon);
			_impulseUI.Show();
		}
	}

	private void ToggleUIVisibility(GameObject go, bool bVisible)
	{
		if (!(null == go))
		{
			UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>(true);
			UIWidget[] array = componentsInChildren;
			foreach (UIWidget uIWidget in array)
			{
				uIWidget.enabled = bVisible;
			}
		}
	}

	public void ToggleHudChatVisibility(bool bVisible)
	{
		_hudChat.ToggleVisibility(bVisible);
	}

	private void OnTurnTimerExpired()
	{
		WaitingForServerSignal.Start(1f);
	}

	private void OnServerWaitPeriodExpired()
	{
		if (_signalProblemIndicator != null)
		{
			_signalProblemIndicator.gameObject.SetActive(true);
		}
	}

	private void ReceivedServerSignal()
	{
		if (_signalProblemIndicator != null)
		{
			_signalProblemIndicator.gameObject.SetActive(false);
		}
		WaitingForServerSignal.Stop();
	}

	private void LateUpdate()
	{
		WaitingForServerSignal.Update();
	}

	public void SetPointsAccumulator(string number)
	{
		_pointsAccumulatorLabel.text = number;
	}
}
