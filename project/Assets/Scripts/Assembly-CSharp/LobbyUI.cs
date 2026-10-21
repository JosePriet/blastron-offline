using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sparx;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
	public UIWidget[] gameModeTintableElements;

	public UIWidget[] gameModeTintableElementsLowAlpha;

	private UILabel countdownLabel;

	private UIFilledSprite countdownTimerDisk;

	private GameObject countdownContainer;

	private GameObject circuitBg;

	private GameObject countdownArt;

	private GameObject countdownDescriptionContainer;

	private GameObject waitingForPlayersContainer;

	private UILabel countdownDescriptionLabel;

	private float countdownTimerStart;

	private float timeToStart;

	private string lastTimerText = string.Empty;

	private uint tryingToKickId;

	private bool timerIsUpdating;

	private bool needsCentreCountdown;

	private UITexture levelImage;

	private UILabel lobbyTitle;

	private bool hasGameStarted;

	private GameObject[] kickButtons = new GameObject[4];

	private string lastLevelId = string.Empty;

	public static bool fiddleMode;

	private List<string> levels;

	private int _entered;

	private Color[] gameModeTints;

	private float darkenedAlpha = 20f / 255f;

	private UITexture modeIcon;

	private string gameModeTexturePath;

	private string levelThumbTexturePath;

	public Vector3 gameModeIconScale = new Vector3(90f, 90f, 1f);

	private UISprite backSprite;

	private int minPlayersForGame
	{
		get
		{
			return (GameAttributes.GetTeamCount() != 2) ? 2 : 4;
		}
	}

	private bool isGameValid
	{
		get
		{
			return Hub.Instance.GameManager.Game != null;
		}
	}

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

	private bool InputEnabled { get; set; }

	private void InitGameModeTints()
	{
		int length = Enum.GetValues(typeof(GameAttributes.GameModeType)).Length;
		gameModeTints = new Color[length];
		for (int i = 0; i < length; i++)
		{
			gameModeTints[i] = new Color(0f, 0f, 0f, 1f);
		}
		gameModeTints[0] = new Color(11f / 255f, 161f / 255f, 207f / 255f);
		gameModeTints[1] = new Color(122f / 255f, 227f / 255f, 242f / 255f);
		gameModeTints[2] = new Color(1f, 158f / 255f, 72f / 255f);
		gameModeTints[3] = new Color(244f / 255f, 74f / 255f, 70f / 255f);
	}

	private void Awake()
	{
		InitGameModeTints();
		for (int i = 0; i < 4; i++)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Player" + i);
			LobbyPlayerUiItem lobbyPlayerUiItem = Util.FindComponent<LobbyPlayerUiItem>(objectExactMatch);
			lobbyPlayerUiItem.isPresent = false;
			lobbyPlayerUiItem.teamId = -1;
			kickButtons[i] = Util.GetObjectExactMatch(objectExactMatch, "KickButton");
			kickButtons[i].GetComponent<ButtonScript>().onButtonClick += PlayerKicked;
			Util.SetLayerRecursive(objectExactMatch, base.gameObject.layer);
			CacDisplay componentInChildren = objectExactMatch.GetComponentInChildren<CacDisplay>();
			if (componentInChildren != null)
			{
				componentInChildren.AutoSpawn = false;
				componentInChildren.FixQueue = true;
			}
		}
		Telemetry.Event("lobby_enter");
		_entered = EB.Time.Now;
		countdownContainer = Util.GetObjectExactMatch(base.gameObject, "CountdownTimerPanel");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(countdownContainer, "LabelCountdown");
		circuitBg = Util.GetObjectExactMatch(countdownContainer, "CircuitBg");
		countdownArt = Util.GetObjectExactMatch(countdownContainer, "Countdown");
		countdownDescriptionContainer = Util.GetObjectExactMatch(countdownContainer, "LabelCountdownDescription");
		waitingForPlayersContainer = Util.GetObjectExactMatch(countdownContainer, "LabelCountdownWaiting");
		countdownLabel = Util.FindComponent<UILabel>(objectExactMatch2);
		countdownTimerDisk = Util.FindComponent<UIFilledSprite>(countdownContainer);
		countdownDescriptionLabel = Util.FindComponent<UILabel>(countdownDescriptionContainer);
		countdownContainer.SetActive(!isPrivateGame);
		if (Hub.Instance.GameManager.Game == null || !isPrivateGame)
		{
			DisableTimer();
		}
		RefreshGameDetails();
		levelImage = Util.GetObjectExactMatch(base.gameObject, "LevelImage").GetComponent<UITexture>();
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "LevelButtonRight");
		UIEventListener uIEventListener = UIEventListener.Get(objectExactMatch3);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnLevelRight));
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "LevelButtonLeft");
		UIEventListener uIEventListener2 = UIEventListener.Get(objectExactMatch4);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnLevelLeft));
		GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "ModeButtonRight");
		UIEventListener uIEventListener3 = UIEventListener.Get(objectExactMatch5);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnModeRight));
		GameObject objectExactMatch6 = Util.GetObjectExactMatch(base.gameObject, "ModeButtonLeft");
		UIEventListener uIEventListener4 = UIEventListener.Get(objectExactMatch6);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnModeLeft));
		levels = new List<string>();
		string[] array = Setup.Instance.Levels;
		foreach (string text in array)
		{
			if (!Setup.Instance.IsExcluded(text))
			{
				levels.Add(text);
			}
		}
		ScreenManager.Instance.RmvPersistentScreen("StatusBar");
		InputEnabled = true;
		timeToStart = Tuning.Instance.GameStartTimer;
		EB.Replication.Manager.RegisterRPCs(this);
	}

	private void OnDestroy()
	{
		ReleaseGameModeTexture();
		ReleaseLevelThumbTexture();
	}

	private void CentreCountdown()
	{
		float x = Util.GetObjectExactMatch(countdownContainer, "CountdownTimerFill").transform.localScale.x;
		GameObject objectExactMatch = Util.GetObjectExactMatch(countdownContainer, "LabelCountdownDescription");
		UILabel label = Util.FindComponent<UILabel>(objectExactMatch);
		float x2 = UIUtils.GetLabelTextSize(label).x;
		float num = 20f;
		float num2 = x2 + num + x;
		float num3 = (0f - num2) / 2f;
		Vector3 localPosition = objectExactMatch.transform.localPosition;
		localPosition.x = num3;
		objectExactMatch.transform.localPosition = localPosition;
		Transform transform = Util.GetObjectExactMatch(countdownContainer, "Countdown").transform;
		localPosition = transform.localPosition;
		localPosition.x = num3 + num + x2;
		transform.localPosition = localPosition;
	}

	private void RefreshGameDetails()
	{
		Game game = Hub.Instance.GameManager.Game;
		lobbyTitle = Util.GetObjectExactMatch(base.gameObject, "LobbyTitle").GetComponent<UILabel>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "PrivateGameTitle");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "PrivateGameMisc");
		if (game != null && isPrivateGame)
		{
			lobbyTitle.text = "ID_UI_LOBBY_PRIVATEGAME";
			UIUtils.SetLabelContents(objectExactMatch, "PrivateName", game.PrivateKey);
			NGUITools.SetActive(objectExactMatch2, game.IsHost);
			countdownContainer.SetActive(false);
			if (!Tuning.Instance.Matchmaking_EnableTeamDeathMatch && !Tuning.Instance.Matchmaking_EnableKillCount && !Tuning.Instance.Matchmaking_EnableSuddenDeath)
			{
				GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch2, "ModeButtonLeft");
				NGUITools.SetActive(objectExactMatch3, false);
				GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch2, "ModeButtonRight");
				NGUITools.SetActive(objectExactMatch4, false);
			}
		}
		else
		{
			NGUITools.SetActive(objectExactMatch2, false);
			NGUITools.SetActive(objectExactMatch, false);
			lobbyTitle.text = "ID_UI_LOBBY_LOBBY";
		}
	}

	private void PlayerKicked(GameObject go)
	{
		Game game = Hub.Instance.GameManager.Game;
		List<EB.Sparx.Player> players = game.Players;
		for (int i = 0; i < 4; i++)
		{
			if (go == kickButtons[i])
			{
				tryingToKickId = players[i].PlayerId;
				string text = Dot.String("name", players[i].Attributes, string.Empty);
				EB.Debug.Log("Host wants to kick player: " + text);
				string desc = Localizer.Format("ID_UI_LOBBY_KICK_CONFIRM_BODY", text);
				ScreenManager.Instance.Confirmation(true, "ID_UI_LOBBY_KICK_CONFIRM_TITLE", desc, "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", OnKickDialogResponse);
				break;
			}
		}
	}

	private void OnKickDialogResponse(ConfirmationDialogUI.ButtonIds buttonId)
	{
		if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
		{
			Hub.Instance.GameManager.Game.KickPlayer(tryingToKickId);
		}
	}

	private void OnOpenPopup()
	{
		LoadPlayers();
		LoadGameInfo();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		objectExactMatch.GetComponent<ButtonScript>().onButtonClick += OnBack;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "StartButton");
		Game game = Hub.Instance.GameManager.Game;
		if (fiddleMode)
		{
			NGUITools.SetActive(objectExactMatch2, EB.Replication.Manager.IsHost);
			objectExactMatch2.GetComponent<ButtonScript>().onButtonClick += OnStart;
		}
		else if (game != null && !game.IsPrivate)
		{
			NGUITools.SetActive(objectExactMatch2, false);
		}
		else
		{
			objectExactMatch2.GetComponent<ButtonScript>().onButtonClick += OnStart;
		}
		UpdateMode();
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, OnTransitionIntroComplete);
	}

	public void OnTransitionIntroComplete()
	{
		EBUI_TransitionManager.Instance.HackClear();
		ScreenManager.Instance.EnableTransitionBlocker(false);
	}

	private void ReleaseGameModeTexture()
	{
		if (!string.IsNullOrEmpty(gameModeTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(gameModeTexturePath);
			gameModeTexturePath = null;
		}
	}

	private void ReleaseLevelThumbTexture()
	{
		if (!string.IsNullOrEmpty(levelThumbTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(levelThumbTexturePath);
			levelThumbTexturePath = null;
		}
	}

	private void UpdateMode()
	{
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		modeIcon = Util.GetObjectExactMatch(base.gameObject, "ModeIcon").GetComponent<UITexture>();
		ReleaseGameModeTexture();
		gameModeTexturePath = "gamemodes_ui/" + GameAttributes.GameModeIcon[(int)gameMode];
		TexturePoolManager.Instance.LoadTexture(gameModeTexturePath, this, OnGameModeTextureLoaded);
		UIUtils.SetLabelContents(base.gameObject, "LabelGameMode", "ID_GAME_ATTRIBUTE_GAMEMODE_" + gameMode.ToString().ToUpper());
		int num = (int)gameMode;
		UIWidget[] array = gameModeTintableElements;
		foreach (UIWidget uIWidget in array)
		{
			if (uIWidget != null)
			{
				uIWidget.color = gameModeTints[num];
			}
		}
		UIWidget[] array2 = gameModeTintableElementsLowAlpha;
		foreach (UIWidget uIWidget2 in array2)
		{
			if (uIWidget2 != null)
			{
				uIWidget2.color = gameModeTints[num];
				uIWidget2.alpha = darkenedAlpha;
			}
		}
	}

	private void OnGameModeTextureLoaded(Texture2D text)
	{
		if (text.name == gameModeTexturePath)
		{
			modeIcon.mainTexture = text;
			modeIcon.MakePixelPerfect();
			Vector3 localScale = modeIcon.transform.localScale;
			localScale *= 0.8f;
			modeIcon.transform.localScale = localScale;
		}
	}

	private void OnBack(GameObject caller)
	{
		if (InputEnabled)
		{
			float num = UnityEngine.Time.realtimeSinceStartup - countdownTimerStart;
			float num2 = Mathf.Max(timeToStart - num, 0f);
			if (!timerIsUpdating || !(num2 < Tuning.Instance.LockoutTime))
			{
				InputEnabled = false;
				EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToPrevious);
			}
		}
	}

	private void OnLevelRight(GameObject go)
	{
		AlterLevelIndex(1);
	}

	private void OnLevelLeft(GameObject go)
	{
		AlterLevelIndex(-1);
	}

	private static int PositiveModulo(int val, int mod)
	{
		return (Math.Abs(val * mod) + val) % mod;
	}

	private void AlterLevelIndex(int delta)
	{
		Game game = Hub.Instance.GameManager.Game;
		string value = GameAttributes.GetValue(GameAttributes.AttributeType.Level);
		int num = levels.IndexOf(value);
		num = PositiveModulo(num + delta, levels.Count);
		string value2 = levels[num];
		Hashtable attributes = new Hashtable(game.Attributes);
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value2, attributes);
		game.SetAttributes(attributes);
	}

	private void OnModeLeft(GameObject go)
	{
		Game game = Hub.Instance.GameManager.Game;
		GameAttributes.GameModeType gameModeType = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		bool flag = false;
		for (int i = 0; i <= 3; i++)
		{
			if (flag)
			{
				break;
			}
			gameModeType = ((gameModeType != GameAttributes.GameModeType.DeathMatch) ? (gameModeType - 1) : GameAttributes.GameModeType.SuddenDeath);
			if (ModeAllowed(gameModeType))
			{
				flag = true;
			}
		}
		Hashtable attributes = new Hashtable(game.Attributes);
		GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, gameModeType, attributes);
		game.SetAttributes(attributes);
		UpdateMode();
	}

	private void OnModeRight(GameObject go)
	{
		Game game = Hub.Instance.GameManager.Game;
		GameAttributes.GameModeType gameModeType = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		bool flag = false;
		for (int i = 0; i <= 3; i++)
		{
			if (flag)
			{
				break;
			}
			gameModeType++;
			if (gameModeType > GameAttributes.GameModeType.SuddenDeath)
			{
				gameModeType = GameAttributes.GameModeType.DeathMatch;
			}
			if (ModeAllowed(gameModeType))
			{
				flag = true;
			}
		}
		Hashtable attributes = new Hashtable(game.Attributes);
		GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, gameModeType, attributes);
		game.SetAttributes(attributes);
		UpdateMode();
	}

	private bool ModeAllowed(GameAttributes.GameModeType type)
	{
		bool flag = false;
		switch (type)
		{
		case GameAttributes.GameModeType.DeathMatch:
			return true;
		case GameAttributes.GameModeType.TeamDeathMatch:
			return Tuning.Instance.Matchmaking_EnableTeamDeathMatch;
		case GameAttributes.GameModeType.KillCount:
			return Tuning.Instance.Matchmaking_EnableKillCount;
		case GameAttributes.GameModeType.SuddenDeath:
			return Tuning.Instance.Matchmaking_EnableSuddenDeath;
		default:
			return false;
		}
	}

	private void OnExitToPrevious()
	{
		if (EB.Replication.Manager.IsHost)
		{
			EB.Replication.Manager.RPC("DisableTimer", RPCMode.AllBuffered);
		}
		Game game = Hub.Instance.GameManager.Game;
		if (game != null && !game.IsPrivate)
		{
			int value = EB.Time.Since(_entered);
			Telemetry.Event("lobby_leave_" + game.Players.Count, value);
			Telemetry.Flush();
		}
		Hub.Instance.GameManager.LeaveGame();
	}

	private void OnExitToStart()
	{
		FacebookPortraitManager.FlushAll();
		ScreenManager.Instance.SetOSBackButtonEnabled();
		Hub.Instance.GameManager.Game.Start();
	}

	private void OnStart(GameObject caller)
	{
		if (InputEnabled)
		{
			StartGame();
		}
	}

	private void StartGame()
	{
		if (!hasGameStarted)
		{
			hasGameStarted = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToStart);
		}
	}

	private void LoadPlayers()
	{
		Game game = Hub.Instance.GameManager.Game;
		List<EB.Sparx.Player> players = game.Players;
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		int teamCount = GameAttributes.GetTeamCount(gameMode);
		for (int i = 0; i < 4; i++)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Player" + i);
			LobbyPlayerUiItem lobbyPlayerUiItem = Util.FindComponent<LobbyPlayerUiItem>(objectExactMatch);
			int teamId = i % teamCount;
			if (i < players.Count)
			{
				lobbyPlayerUiItem.playerName = Dot.String("name", players[i].Attributes, string.Empty);
				lobbyPlayerUiItem.isPresent = true;
				lobbyPlayerUiItem.level = Dot.Integer("level", players[i].Attributes, 1);
				if (teamCount == 2)
				{
					lobbyPlayerUiItem.teamId = teamId;
				}
				else
				{
					lobbyPlayerUiItem.teamId = 1;
				}
				GameObject[] objects = Util.GetObjects(objectExactMatch, "Level");
				GameObject[] array = objects;
				foreach (GameObject gameObject in array)
				{
					gameObject.SetActive(false);
				}
				CacDisplay componentInChildren = objectExactMatch.GetComponentInChildren<CacDisplay>();
				if (componentInChildren != null)
				{
					CharacterDB.Recipe recipe = CharacterManager.Instance.DeserializeRecipe(players[i].Attributes);
					componentInChildren.Recipe = recipe;
					componentInChildren.FixQueue = true;
					componentInChildren.SpawnPlayer();
				}
			}
			else
			{
				lobbyPlayerUiItem.isPresent = false;
				lobbyPlayerUiItem.teamId = 1;
			}
		}
		for (int k = 0; k < 4; k++)
		{
			kickButtons[k].SetActive(false);
			if (k < players.Count && game.IsPrivate && EB.Replication.Manager.IsHost && players[k].PlayerId != EB.Replication.Manager.LocalPlayerId)
			{
				kickButtons[k].SetActive(true);
			}
		}
		GameObject go = Util.GetObject(base.gameObject, "StartButton");
		if (fiddleMode)
		{
			NGUITools.SetActive(go, EB.Replication.Manager.IsHost);
		}
		else if (game.IsPrivate)
		{
			int count = Hub.Instance.GameManager.Game.Players.Count;
			if (count > 1)
			{
				NGUITools.SetActive(go, EB.Replication.Manager.IsHost);
			}
			else
			{
				NGUITools.SetActive(go, false);
			}
		}
		if (game.Players.Count == 4 || (!Profile.Instance.IsTutorialDone("mp") && game.Players.Count == 3))
		{
			timeToStart = Tuning.Instance.FullPlayersTimer;
			countdownTimerStart = UnityEngine.Time.realtimeSinceStartup;
			EB.Debug.Log("Reached max players, shortening timer to " + timeToStart);
		}
		RefreshGameDetails();
	}

	private void LoadGameInfo()
	{
		Game game = Hub.Instance.GameManager.Game;
		string localizedValue = GameAttributes.GetLocalizedValue(GameAttributes.AttributeType.GameMode, game.Attributes);
		string localizedValue2 = GameAttributes.GetLocalizedValue(GameAttributes.AttributeType.Level, game.Attributes);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "GameInfoPanel");
		UIUtils.SetLabelContents(objectExactMatch, "LabelTitle", localizedValue);
		string labelText = Localizer.Format("ID_UI_LOBBY_LEVEL", localizedValue2);
		UIUtils.SetLabelContents(objectExactMatch, "LabelLevelName", labelText);
		string value = GameAttributes.GetValue(GameAttributes.AttributeType.Level, game.Attributes);
		if (value != lastLevelId)
		{
			ReleaseLevelThumbTexture();
			levelThumbTexturePath = "levelthumb_ui/ui_" + value;
			TexturePoolManager.Instance.LoadTexture(levelThumbTexturePath, this, OnGameInfoTextureLoaded);
			lastLevelId = value;
		}
	}

	private void OnGameInfoTextureLoaded(Texture2D t)
	{
		if (t.name == levelThumbTexturePath)
		{
			levelImage.mainTexture = t;
			levelImage.GetComponent<TextureFade>().SetTexture(levelImage);
		}
	}

	private void OnAttributesUpdated()
	{
		LoadGameInfo();
		UpdateMode();
	}

	private void OnPlayerJoined(EB.Sparx.Player player)
	{
		LoadPlayers();
	}

	private void OnPlayerLeft(EB.Sparx.Player player)
	{
		LoadPlayers();
	}

	private void CheckTimerStatus()
	{
		if (!EB.Replication.Manager.IsHost)
		{
			return;
		}
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			EB.Replication.Manager.RPC("DisableTimer", RPCMode.AllBuffered);
		}
		else if (!game.IsPrivate)
		{
			int count = Hub.Instance.GameManager.Game.Players.Count;
			bool flag = count >= minPlayersForGame;
			if (flag && !timerIsUpdating)
			{
				EB.Replication.Manager.RPC("EnableTimer", RPCMode.AllBuffered);
			}
			else if (!flag && timerIsUpdating)
			{
				EB.Replication.Manager.RPC("DisableTimer", RPCMode.AllBuffered);
			}
		}
	}

	[RPC]
	private void EnableTimer()
	{
		if (!isPrivateGame)
		{
			countdownTimerStart = UnityEngine.Time.realtimeSinceStartup;
			timerIsUpdating = true;
			circuitBg.SetActive(true);
			countdownArt.SetActive(true);
			countdownDescriptionContainer.SetActive(true);
			waitingForPlayersContainer.SetActive(false);
		}
	}

	[RPC]
	private void DisableTimer()
	{
		if (!isPrivateGame)
		{
			timerIsUpdating = false;
			circuitBg.SetActive(true);
			countdownArt.SetActive(false);
			countdownDescriptionContainer.SetActive(false);
			waitingForPlayersContainer.SetActive(true);
		}
	}

	[RPC]
	private void UpdateTimer(float remainingTime)
	{
		if (!isPrivateGame && !timerIsUpdating)
		{
			countdownTimerStart = UnityEngine.Time.realtimeSinceStartup;
			timeToStart = remainingTime;
			timerIsUpdating = true;
			circuitBg.SetActive(true);
			countdownArt.SetActive(true);
			countdownDescriptionContainer.SetActive(true);
			waitingForPlayersContainer.SetActive(false);
		}
	}

	private void Update()
	{
		if (needsCentreCountdown && !countdownDescriptionLabel.text.StartsWith("ID_UI_"))
		{
			needsCentreCountdown = false;
			CentreCountdown();
		}
		CheckTimerStatus();
		if (timerIsUpdating)
		{
			float num = UnityEngine.Time.realtimeSinceStartup - countdownTimerStart;
			float num2 = Mathf.Max(timeToStart - num, 0f);
			string text = ((int)Mathf.Ceil(num2)).ToString();
			if (text.Length == 1)
			{
				text = "0" + text;
			}
			float fillAmount = num2 / Tuning.Instance.GameStartTimer;
			countdownTimerDisk.fillAmount = fillAmount;
			if (lastTimerText != text)
			{
				countdownLabel.text = text;
				lastTimerText = text;
				if (EB.Replication.Manager.IsHost)
				{
					EB.Replication.Manager.RPC("UpdateTimer", RPCMode.AllBuffered, num2);
				}
			}
			if (num2 <= 0f && EB.Replication.Manager.IsHost)
			{
				countdownLabel.text = string.Empty;
				FacebookPortraitManager.FlushAll();
				Hub.Instance.GameManager.Game.Start();
				return;
			}
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
			if (!(objectExactMatch != null))
			{
				return;
			}
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Background");
			if (!(objectExactMatch2 != null))
			{
				return;
			}
			backSprite = objectExactMatch2.GetComponent<UISprite>();
			if (backSprite != null)
			{
				if (num2 < Tuning.Instance.LockoutTime)
				{
					backSprite.alpha = 0.3f;
				}
				else
				{
					backSprite.alpha = 1f;
				}
			}
		}
		else if (backSprite != null && backSprite.alpha != 1f)
		{
			backSprite.alpha = 1f;
		}
	}

	private void EnableInput(bool enable)
	{
		InputEnabled = enable;
	}

	public void SetCountdownTimerInterval(float time)
	{
		timeToStart = time;
	}
}
