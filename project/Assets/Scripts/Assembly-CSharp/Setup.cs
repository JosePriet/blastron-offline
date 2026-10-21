using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EB;
using EB.Net;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class Setup : MonoBehaviour
{
	private const string introPrefName = "intro_shown";

	public static string ApiEndPointOverride = string.Empty;

	public string ApiEndPoint = string.Empty;

	public static bool NoStats = false;

	public static Language Locale = Language.Unknown;

	public static bool ForceSelectLanguage = false;

	public string[] MPLevelExcludeList;

	[NonSerialized]
	public List<string> nextScreenStack;

	public string nextScreenFn;

	private List<string> defaultNextScreenStack;

	public string appKey = string.Empty;

	private static string _version;

	public bool _gameStarted;

	private bool _defaultStackLoaded;

	private GachaManager gachaManager;

	private bool _inError;

	private bool _inBootFBCancelFlow;

	public static Setup Instance { get; private set; }

	public string[] Levels
	{
		get
		{
			return Assets.Scenes;
		}
	}

	public string[] MP_Levels
	{
		get
		{
			List<string> list = new List<string>(Levels);
			list.RemoveAll(IsExcluded);
			return list.ToArray();
		}
	}

	public static string Version
	{
		get
		{
			if (string.IsNullOrEmpty(_version))
			{
				_version = "code:" + EB.Version.GetVersion() + Assets.ContentVersion;
			}
			UnityEngine.Debug.Log("SETUP.VERSION: " + _version);
			return _version;
		}
	}

	public bool IsLoggedIn
	{
		get
		{
			return Hub.Instance.State == HubState.Connected;
		}
	}

	public bool IsLoggingIn { get; set; }

	public bool IsExcluded(string levelName)
	{
		string[] mPLevelExcludeList = MPLevelExcludeList;
		foreach (string text in mPLevelExcludeList)
		{
			if (text == levelName)
			{
				return true;
			}
		}
		return false;
	}

	public int GetLevelIndex(string s)
	{
		for (int i = 0; i < Levels.Length; i++)
		{
			if (Levels[i] == s)
			{
				return i;
			}
		}
		return -1;
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		DebugSettings.Init();
		defaultNextScreenStack = new List<string>();
		defaultNextScreenStack.Add("HomeUI");
		List<string> list = new List<string>();
		list.Add("StartUI");
		nextScreenStack = list;
		nextScreenFn = string.Empty;
		_gameStarted = false;
	}

	private void SetupSparx(Language locale)
	{
		UnityEngine.Object[] array = Assets.LoadAll("Crypto/Certs", typeof(TextAsset));
		for (int i = 0; i < array.Length; i++)
		{
			TextAsset textAsset = (TextAsset)array[i];
			string[] array2 = textAsset.text.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			List<byte> list = new List<byte>();
			string[] array3 = array2;
			foreach (string text in array3)
			{
				if (!text.StartsWith("-----"))
				{
					list.AddRange(Encoding.FromBase64String(text.Trim()));
				}
			}
			TcpClientBouncy.AddCertificate(list.ToArray());
			Assets.Unload(textAsset);
		}
		if (!string.IsNullOrEmpty(ApiEndPointOverride))
		{
			ApiEndPoint = ApiEndPointOverride;
		}
		Config config = new Config();
		config.ApiEndpoint = ApiEndPoint;
		config.ApiKey = new Key(appKey);
		config.Locale = locale;
		config.LoadLocalizer = false;
		config.LoginConfig.Listener = new LoginListener();
		config.WalletConfig.Listener = new WalletListener();
		config.WalletConfig.IAPPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjl8DmxSP37yt1NzwVC3nowHhtPFx8N7Jo0GNyt5YeOENko2wb7mOuSRfOP6KykgjZN0wk5Jbusle0gQAE96nYkV0tpT9UgLNblZLAIcqrn+covrQwc4l9oPdFAXndj1wZu2DmuUsEly+fHfXVLLuWJZynC8EIHQX5Ph4Wx9WaMTYeiWooM0bSomN8PR2QQSz+AaGNggpEoKrlrv3Gfxw5n81MXCURgTwdZhr3XzLDWEKydNVyJT7Xgn993khz+eXH9hCx3KSl6icheeMtM3Fh5cgkH96GXizTiezJt0VhR7NOp7fn/sfuFHMRXX37+vtr9DjhTCSYyL9XLtmX93s2QIDAQAB";
		config.LoginConfig.LoginManagerType = typeof(LoginManagerKabam);
		config.LoginConfig.RequireName = true;
		config.GameManagerConfig.Listener = base.gameObject.AddComponent<GameListener>();
		config.InventoryConfig.Listener = base.gameObject.AddComponent<InventoryListener>();
		config.GameCenterConfig.LeaderboardPrefix = "BB_LEADERBOARD_";
		config.GameCenterConfig.AchievementPrefix = "ACH_";
		config.GameComponents.Add(typeof(TournamentManager));
		config.GameComponents.Add(typeof(MessengerManager));
		config.FacebookConfig.Scope = new string[1] { "email" };
		config.FacebookConfig.FetchMe = true;
		config.FacebookConfig.FetchFriends = true;
		config.GachaConfig = new GachaManager.GachaConfig();
		config.AppiraterConfig.AppId = "577826108";
		config.AppiraterConfig.UsesUntilPrompt = 10;
		config.AppiraterConfig.DaysUntilPrompt = 3.0;
		config.AppiraterConfig.SignificantEventsUntilPrompt = -1;
		config.AppiraterConfig.TimeBeforeReminding = 3.0;
		config.AppiraterConfig.Listener = new AppiraterListener();
		config.AppiraterConfig.CanPromptOnUse = true;
		config.AppiraterConfig.CanPromptOnLaunched = false;
		config.AppiraterConfig.CanPromptOnEvent = false;
		config.AppiraterConfig.Title = "ID_UI_APPIRATER_TITLE";
		config.AppiraterConfig.Message = "ID_UI_APPIRATER_MESSAGE";
		config.AppiraterConfig.RateButton = "ID_UI_APPIRATER_RATE";
		config.AppiraterConfig.CancelButton = "ID_UI_APPIRATER_CANCEL";
		config.AppiraterConfig.RemindButton = "ID_UI_APPIRATER_REMIND";
		NetworkFactory.Register("udp", typeof(NetworkENet));
		Hub.Create(config);
		if (NoStats)
		{
			Hub.Instance.ApiEndPoint.AddData("nostats", "1");
		}
	}

	private IEnumerator Start()
	{
		Instance = this;
		Input.multiTouchEnabled = false;
		BugReport.OnBugReport += OnCrash;
		Options.SettingsChanged += UpdateSettings;
		UpdateSettings();
		Language locale = ((Locale != Language.Unknown) ? Locale : EB.Version.GetDefaultLanguageFromLanguageCode());
		Localizer.Clear();
		Localizer.LoadAllFromResources(locale, false);
		SetupSparx(locale);
		SplashUI.Locale = locale;
		if (SecurePrefs.GetInt("intro_shown", 0) == 0)
		{
			Telemetry.Event("tut/entry");
			GameObject screen = ScreenManager.Instance.OpenPopup("IntroSequenceUI");
			IntroSequenceUI ui = screen.GetComponent<IntroSequenceUI>();
			ui.DoLoginOnFinish = false;
			while (ScreenManager.Instance.GetPopup("IntroSequenceUI") != null)
			{
				yield return 1;
			}
			SecurePrefs.SetInt("intro_shown", 1);
		}
		UnityEngine.Object.Instantiate(Assets.Load("MAT", typeof(GameObject)));
		GameObject loadingScreen = ScreenManager.Instance.ShowLoadingScreen("WelcomeLoadingScreenUI");
		yield return loadingScreen.GetComponent<WelcomeLoadingScreenUI>().CheckForContent(ApiEndPoint);
		yield return Assets.LoadPacks(true);
		BugReport.AddData("content", Version);
		yield return Assets.LoadAsync("Tuning", typeof(GameObject), (UnityEngine.Object asset) =>
		{
			UnityEngine.Object target = UnityEngine.Object.Instantiate(asset);
			UnityEngine.Object.DontDestroyOnLoad(target);
		});
		yield return Assets.LoadAsync("LootManager", typeof(GameObject), (UnityEngine.Object asset) =>
		{
			UnityEngine.Object target = UnityEngine.Object.Instantiate(asset);
			UnityEngine.Object.DontDestroyOnLoad(target);
		});
		if (ForceSelectLanguage)
		{
			locale = ((Locale == Language.Unknown) ? EB.Version.GetDefaultLanguageFromLanguageCode() : Locale);
			Localizer.Clear();
		}
		Locale = locale;
		Localizer.LoadAllFromResources(locale, false);
		Assembly[] assemblies = new Assembly[2]
		{
			GetType().Assembly,
			typeof(EB.Sequence.Component).Assembly
		};
		Utils.Initialize(assemblies);
		base.gameObject.AddComponent<SequenceVariableManager>();
		InventoryDB.LoadMetaData();
		Hub.Instance.Config.InventoryConfig.StartingInventory = InventoryDB.GetStartingInventory();
		DailyOffersDB.LoadMetaData();
		base.gameObject.AddComponent<UnlocksManager>();
		base.gameObject.AddComponent<Profile>();
		base.gameObject.AddComponent<CharacterManager>();
		base.gameObject.AddComponent<HUD>();
		base.gameObject.AddComponent<EBUI_TransitionManager>();
		base.gameObject.AddComponent<StatManager>();
		base.gameObject.AddComponent<EmoteManager>();
		base.gameObject.AddComponent<PartManager>();
		base.gameObject.AddComponent<ProjectileIndicatorManager>();
		base.gameObject.AddComponent<CampaignData>();
		base.gameObject.AddComponent<TutorialManager>();
		base.gameObject.AddComponent<GarbageCollector>();
		base.gameObject.AddComponent<RewardsManager>();
		base.gameObject.AddComponent<VaultTimer>();
		base.gameObject.AddComponent<ChampionshipData>();
		base.gameObject.AddComponent<ChampionshipLeaderboards>();
		base.gameObject.AddComponent<StoryManager>();
		base.gameObject.AddComponent<ItemLevelManager>();
		base.gameObject.AddComponent<GachaManager>();
		base.gameObject.AddComponent<EBGamepad>();
		base.gameObject.AddComponent<AttributesManager>();
		gachaManager = base.gameObject.GetComponent<GachaManager>();
		yield return CharacterDB.Initialize(4);
		UnlocksManager.Instance.UnlockDefault("sp_00");
		foreach (string item in Hub.Instance.Config.InventoryConfig.StartingInventory.Keys)
		{
			UnlocksManager.Instance.UnlockDefault(item + UnlocksManager.showUnlockSuffix);
		}
		FacebookPortraitManager.Init();
		yield return Hub.Instance.LoadOffineData();
		Login(true);
		HoFData.CreateHoFMasterSeq();
	}

	private void OnCrash()
	{
		if ((bool)Automation.Instance)
		{
			Application.Quit();
		}
		else
		{
			if (!ScreenManager.Instance)
			{
				return;
			}
			Coroutines.NextFrame(() =>
			{
				UnityEngine.Debug.Log("Showing ERROR DIALOG!");
				ScreenManager.Instance.ShowError("ID_UI_ERROR_FATAL", () =>
				{
					Application.Quit();
				});
			});
		}
	}

	private void Update()
	{
		Assets.ProcessDeferedUnloads(false);
		Memory.Update(20);
	}

	public void Login(bool silent)
	{
		if (silent || _defaultStackLoaded)
		{
		}
		if (!silent)
		{
			ScreenManager.Instance.BlockingUI("ID_UI_LOGIN_BLOCKING_TITLE", "ID_UI_LOGIN_BLOCKING_DESC");
		}
		IsLoggingIn = true;
		Hub.Instance.LoginManager.Login(silent);
	}

	public void UpdateSettings()
	{
		AudioListener.pause = Options.sound == 0;
		NGUITools.soundVolume = Options.sound;
		AudioListener.volume = ((Options.sound == 0) ? 0.0001f : 1f);
		bool Argument = Options.sound == 0;
		bool Argument2 = false;
		Messenger<bool, bool>.BroadcastToAllListeners("MuteAudioJukebox", ref Argument, ref Argument2);
	}

	private void OnLevelWasLoaded(int index)
	{
		UpdateSettings();
	}

	public void OnLoggedIn()
	{
		bool flag = Hub.Instance.GameManager.Game != null;
		bool flag2 = false;
		gachaManager.Initialize();
		if (flag)
		{
			GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
			if (mode != GameAttributes.ModeType.SP)
			{
				flag2 = true;
			}
		}
		else
		{
			flag2 = true;
		}
		if (flag2)
		{
			LoadTuningData();
			ReturnToFrontendScreen();
			SecurePrefs.SetInt("login", 1);
			ScreenManager.Instance.BroadcastMessage("OnLoginStateChanged", SendMessageOptions.DontRequireReceiver);
			IsLoggingIn = false;
		}
	}

	private void LoadTuningData()
	{
		Tuning instance = Tuning.Instance;
		instance.LoadTuningData();
		InventoryDB.LoadTuningData();
		CharacterDB.LoadTuningData();
		Profile.Instance.UpdateTuningData();
		ChampionshipData.LoadTuningData();
		ItemLevelManager.Instance.LoadTuningData();
	}

	public void CheckForFirstLogin()
	{
		ReturnToFrontendScreen();
	}

	public void ReturnToFrontendScreen()
	{
		ScreenManager instance = ScreenManager.Instance;
		instance.CloseAllPopups();
		GameObject gameObject = null;
		for (int i = 0; i < nextScreenStack.Count; i++)
		{
			string text = nextScreenStack[i];
			gameObject = ((i != nextScreenStack.Count - 1 || !string.IsNullOrEmpty(nextScreenFn)) ? instance.OpenPopup(text, false, false, false, false) : instance.OpenPopup(text));
		}
		if (!string.IsNullOrEmpty(nextScreenFn))
		{
			gameObject.SendMessage(nextScreenFn);
		}
		GetComponent<HUD>().OnLeaveGame();
		_defaultStackLoaded |= nextScreenStack == defaultNextScreenStack;
		nextScreenStack = defaultNextScreenStack;
		nextScreenFn = string.Empty;
		ScreenManager.Instance.ForceHideLoadingScreen();
	}

	private void OnError()
	{
		_inError = false;
		Sequence.KillAll();
		HoFData.CreateHoFMasterSeq();
		bool inBootFBCancelFlow = _inBootFBCancelFlow;
		_inBootFBCancelFlow = false;
		if (inBootFBCancelFlow)
		{
			Hub.Instance.LoginManager.LoginGuest();
			return;
		}
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			Hub.Instance.GameManager.LeaveGame();
		}
		else
		{
			ReturnToFrontendScreen();
		}
	}

	private void ShowError(string error)
	{
		if (string.IsNullOrEmpty(error))
		{
			error = Localizer.GetString("ID_SPARX_ERROR_UNKNOWN");
		}
		_inError = true;
		_inBootFBCancelFlow = (error == "ID_SPARX_ERROR_FACEBOOK_CANCELED" || error == "ID_SPARX_ERROR_FACEBOOK_APP_BLOCKED") && ScreenManager.Instance.GetPopup("HomeUI") == null;
		ScreenManager.Instance.ShowError(error, OnError);
	}

	public void OnDisconnected(string error)
	{
		if (IsLoggingIn)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
		}
		if (!string.IsNullOrEmpty(error))
		{
			Game game = Hub.Instance.GameManager.Game;
			if (game == null || !game.Network.IsLocal || "kOn" == game.Attributes["Championship"].ToString())
			{
				ShowError(error);
			}
		}
		ScreenManager.Instance.BroadcastMessage("OnLoginStateChanged", SendMessageOptions.DontRequireReceiver);
		IsLoggingIn = false;
	}

	public void OnNewVersionRequired(string storeUrl)
	{
		EB.Debug.LogError("new version required!");
		GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
		ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
		confirmationDialogUI.SetParameters(true, "ID_UI_VERSION_FAILED_DESC", "ID_UI_VERSION_FAILED_TITLE", "ID_UI_VERSION_FAILED_UPDATE", string.Empty);
		confirmationDialogUI.callback = (ConfirmationDialogUI.ButtonIds id) =>
		{
			if (id == ConfirmationDialogUI.ButtonIds.Left)
			{
				Application.OpenURL(storeUrl);
			}
			OnLoginFailed(string.Empty, true);
		};
	}

	public void OnLoginFailed(string error, bool silent)
	{
		EB.Debug.Log("OnLoginFailed " + error + " " + silent);
		if (silent)
		{
			if (Hub.Instance.GameManager.Game != null)
			{
				Hub.Instance.GameManager.LeaveGame();
			}
			else
			{
				ScreenManager instance = ScreenManager.Instance;
				if (null == instance.GetPopup("StartUI"))
				{
					ReturnToFrontendScreen();
				}
			}
		}
		else
		{
			ShowError(error);
		}
		Sequence.KillAll();
		IsLoggingIn = false;
	}

	public void OnStartGame()
	{
		_gameStarted = true;
	}

	public Coroutine OnLeaveGame()
	{
		return StartCoroutine(_OnLeaveGame());
	}

	private IEnumerator _OnLeaveGame()
	{
		UnityEngine.Time.timeScale = 1f;
		while (_inError)
		{
			yield return 1;
		}
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Player));
		for (int i = 0; i < array.Length; i++)
		{
			Player player = (Player)array[i];
			player.ReleasePlayer();
		}
		if (_gameStarted)
		{
			GetComponent<HUD>().OnLeaveGame();
			yield return Application.LoadLevelAsync("3_empty");
			if (Assets.RequiresUnloadAssets())
			{
				yield return Assets.UnloadUnusedAssets();
			}
			_gameStarted = false;
		}
		ScreenManager.Instance.EnableMultiTouch(false);
		ReturnToFrontendScreen();
	}

	private void OnApplicationPause(bool pausing)
	{
		if (pausing)
		{
			AudioListener.pause = true;
		}
		else if (UnityEngine.Time.timeScale == 1f)
		{
			AudioListener.pause = false;
		}
	}
}
