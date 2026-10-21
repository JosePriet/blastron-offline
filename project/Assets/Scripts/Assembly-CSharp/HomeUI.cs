using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class HomeUI : MonoBehaviour
{
	private class DailyRewardInfo
	{
		public DailyOffer.Type rewardType;

		public GameObject prefab;

		public int quantity;

		public DailyRewardInfo(DailyOffer.Type rewardType, GameObject prefab, int quantity)
		{
			this.rewardType = rewardType;
			this.prefab = prefab;
			this.quantity = quantity;
		}
	}

	public enum PageType
	{
		Messages = 0,
		Tournaments = 1,
		HallOfFame = 2,
		Help = 3
	}

	public const string TUTORIAL_FIRSTBOOT = "firstboot_shown";

	public const string TUTORIAL_MP = "mp";

	public const string TUTORIAL_VAULT = "vault";

	public const string ELEPHANT_GUN_NAME = "Elephant Gun";

	public const string extendedScenesPackName = "scenes_ext";

	private GameObject timerActiveObject;

	private GameObject timerWaitingObject;

	private GameObject tournamentCountdown;

	private UILabel _labelHour0;

	private UILabel _labelHour1;

	private UILabel _labelMinute0;

	private UILabel _labelMinute1;

	private UILabel _labelSecond0;

	private UILabel _labelSecond1;

	private GameObject vaultCounts;

	private GameObject vaultCircle;

	private GameObject cacInstance;

	private object _tournamentUpdate;

	private bool _enableInput;

	private bool playedCinematic;

	private bool _isReady;

	private GameObject gcAchievements;

	private GameObject gcLeaderboards;

	private GameObject gcMain;

	private bool gcActivated;

	private UISprite gcMainBackground;

	private Color gcOriginalColor = Color.white;

	private bool _beginnerSaleGotoStore;

	public static int LastMotdShown;

	private bool ShouldShowDailyOffers
	{
		get
		{
			return !Profile.Instance.DailyOffersSeen && Profile.Instance.DailyOffersCollected < 21;
		}
	}

	private bool ShouldShowBeginnersSale
	{
		get
		{
			return !Profile.Instance.BeginnerSalePopupSeen && Profile.Instance.HasBeginnerSaleStarted() && !Profile.Instance.HasBeginnerSaleExpired();
		}
	}

	private bool ShouldShowFreeGachaSpin
	{
		get
		{
			return !Profile.Instance.GachaSpinCollectionSeen && (VaultTimer.Instance.FreeBundleAvailable || GachaManager.Instance.GetTotalFreeSpins() > 0);
		}
	}

	private void Awake()
	{
		Util.GetObjectExactMatch(base.gameObject, "MyRobotButton").GetComponent<MenuButtonScript>().onButtonClick += OnMyRobot;
		Util.GetObjectExactMatch(base.gameObject, "CampaignButton").GetComponent<MenuButtonScript>().onButtonClick += OnCampaign;
		Util.GetObjectExactMatch(base.gameObject, "StoreButton").GetComponent<MenuButtonScript>().onButtonClick += OnStore;
		Util.GetObjectExactMatch(base.gameObject, "PracticeButton").GetComponent<MenuButtonScript>().onButtonClick += OnPractice;
		Util.GetObjectExactMatch(base.gameObject, "VaultButton").GetComponent<MenuButtonScript>().onButtonClick += OnGacha;
		Util.GetObjectExactMatch(base.gameObject, "OnlineButton").GetComponent<MenuButtonScript>().onButtonClick += OnOnline;
		Util.GetObjectExactMatch(base.gameObject, "PlayerButton").GetComponent<ButtonScript>().onButtonClick += OnSelectName;
		List<GameObject> list = new List<GameObject>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "StandingsButton");
		list.Add(objectExactMatch);
		objectExactMatch.GetComponent<ButtonScript>().onButtonClick += OnTournaments;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "TournamentButton");
		list.Add(objectExactMatch2);
		objectExactMatch2.GetComponent<ButtonScript>().onButtonClick += OnTournaments;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "OptionsButton");
		list.Add(objectExactMatch3);
		objectExactMatch3.GetComponent<ButtonScript>().onButtonClick += OnOptions;
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "InfoButton");
		list.Add(objectExactMatch4);
		objectExactMatch4.GetComponent<ButtonScript>().onButtonClick += OnMessages;
		GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "TrialPayButton");
		list.Add(objectExactMatch5);
		objectExactMatch5.GetComponent<ButtonScript>().onButtonClick += OnTrialPay;
		gcMain = Util.GetObjectExactMatch(base.gameObject, "GameCenterButton");
		float num = objectExactMatch.transform.localPosition.x - objectExactMatch3.transform.localPosition.x;
		float num2 = num / 2f;
		if (gcMain != null)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				gcMain.SetActive(false);
				foreach (GameObject item in list)
				{
					Vector3 localPosition = item.transform.localPosition;
					localPosition.x -= num2;
					item.transform.localPosition = localPosition;
				}
			}
			else
			{
				list.Add(gcMain);
				gcMain.GetComponent<ButtonScript>().onButtonClick += OnGameCenter;
				gcMainBackground = Util.GetObjectExactMatch(gcMain, "Background").GetComponent<UISprite>();
				if (gcMainBackground != null)
				{
					gcOriginalColor = gcMainBackground.color;
				}
			}
		}
		gcAchievements = Util.GetObjectExactMatch(base.gameObject, "GCAchievementsButton");
		gcLeaderboards = Util.GetObjectExactMatch(base.gameObject, "GCLeaderboardsButton");
		if (gcAchievements != null)
		{
			gcAchievements.SetActive(false);
			gcAchievements.GetComponent<ButtonScript>().onButtonClick += OnGCAchievements;
		}
		if (gcLeaderboards != null)
		{
			gcLeaderboards.SetActive(false);
			gcLeaderboards.GetComponent<ButtonScript>().onButtonClick += OnGCLeaderboards;
		}
		if (TrialPay.ShouldShowTrialPay())
		{
			foreach (GameObject item2 in list)
			{
				Vector3 localPosition2 = item2.transform.localPosition;
				localPosition2.x -= num2;
				item2.transform.localPosition = localPosition2;
			}
		}
		else
		{
			Util.GetObjectExactMatch(base.gameObject, "TrialPayButton").SetActive(false);
		}
		tournamentCountdown = Util.GetObjectExactMatch(base.gameObject, "Tournament_Countdown");
		GameObject objectExactMatch6 = Util.GetObjectExactMatch(base.gameObject, "VaultTimer");
		timerActiveObject = Util.GetObjectExactMatch(objectExactMatch6, "Active");
		timerWaitingObject = Util.GetObjectExactMatch(objectExactMatch6, "Waiting");
		_labelHour0 = Util.GetObjectExactMatch(timerActiveObject, "LabelHour0").GetComponent<UILabel>();
		_labelHour1 = Util.GetObjectExactMatch(timerActiveObject, "LabelHour1").GetComponent<UILabel>();
		_labelMinute0 = Util.GetObjectExactMatch(timerActiveObject, "LabelMinute0").GetComponent<UILabel>();
		_labelMinute1 = Util.GetObjectExactMatch(timerActiveObject, "LabelMinute1").GetComponent<UILabel>();
		_labelSecond0 = Util.GetObjectExactMatch(timerActiveObject, "LabelSecond0").GetComponent<UILabel>();
		_labelSecond1 = Util.GetObjectExactMatch(timerActiveObject, "LabelSecond1").GetComponent<UILabel>();
		vaultCircle = Util.GetObjectExactMatch(base.gameObject, "Vault_Circle");
		vaultCounts = Util.GetObjectExactMatch(base.gameObject, "Vault_Count");
		cacInstance = Util.GetObjectExactMatch(base.gameObject, "CreateACharacterInstance");
		cacInstance.SetActive(false);
		_enableInput = true;
		_isReady = false;
		Assets.Unload("WelcomeLoadingScreenUI");
		ScreenManager.Instance.RmvPersistentScreen("SplashUI");
	}

	private void Update()
	{
		UpdateTimer();
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void UpdateName()
	{
		string text = Hub.Instance.LoginManager.LocalUser.Name;
		if (string.IsNullOrEmpty(text))
		{
			text = Localizer.GetString("ID_UI_HOME_DEFAULT_NAME");
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "PlayerButton");
		UILabel l = Util.FindComponent<UILabel>(objectExactMatch);
		UIUtils.TruncateWithEllipsis(l, text);
	}

	private void UpdateCounts()
	{
		int count = Hub.Instance.GetManager<MessengerManager>().Count;
		count = Mathf.Min(count, 99);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Message_Circle");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Message_Count");
		if (count > 0)
		{
			objectExactMatch.SetActive(true);
			objectExactMatch2.SetActive(true);
			objectExactMatch2.GetComponent<UILabel>().text = count.ToString();
		}
		else
		{
			objectExactMatch.SetActive(false);
			objectExactMatch2.SetActive(false);
		}
	}

	private void UpdateGachaSpinCounts()
	{
		int totalFreeSpins = GachaManager.Instance.GetTotalFreeSpins();
		bool flag = Hub.Instance.State == HubState.Connected;
		if (totalFreeSpins > 0 && flag)
		{
			vaultCircle.SetActive(true);
			vaultCounts.SetActive(true);
			vaultCounts.GetComponent<UILabel>().text = totalFreeSpins.ToString();
		}
		else
		{
			vaultCircle.SetActive(false);
			vaultCounts.SetActive(false);
		}
	}

	private void UpdateTimer()
	{
		if (Hub.Instance.State == HubState.Connected)
		{
			int grantTimeLeft = VaultTimer.Instance.GrantTimeLeft;
			if (grantTimeLeft > 0)
			{
				NGUITools.SetActive(timerActiveObject, true);
				NGUITools.SetActive(timerWaitingObject, false);
				int num = grantTimeLeft;
				int num2 = Mathf.FloorToInt(num / 3600);
				num -= num2 * 3600;
				int num3 = Mathf.FloorToInt(num / 60);
				num -= num3 * 60;
				int num4 = num2 / 10;
				int num5 = num2 % 10;
				int num6 = num3 / 10;
				int num7 = num3 % 10;
				int num8 = num / 10;
				int num9 = num % 10;
				_labelHour0.text = num4.ToString();
				_labelHour1.text = num5.ToString();
				_labelMinute0.text = num6.ToString();
				_labelMinute1.text = num7.ToString();
				_labelSecond0.text = num8.ToString();
				_labelSecond1.text = num9.ToString();
			}
			else
			{
				NGUITools.SetActive(timerActiveObject, false);
				NGUITools.SetActive(timerWaitingObject, true);
			}
			UpdateGachaSpinCounts();
		}
	}

	private void OnMessageCountChanged()
	{
		UpdateCounts();
	}

	private bool IsLoggedIn()
	{
		return Hub.Instance.State == HubState.Connected;
	}

	private bool ShouldEnableCampaignButton()
	{
		return IsLoggedIn() && (Assets.HasPack("scenes_ext") || Assets.HasDLCPack("scenes_ext"));
	}

	private void OnLoginStateChanged()
	{
		CharacterDB.MetaData.CalculateUnlockStructure();
		bool flag = IsLoggedIn();
		Util.GetObjectExactMatch(base.gameObject, "MyRobotButton").GetComponent<MenuButtonScript>().SetEnabled(flag);
		Util.GetObjectExactMatch(base.gameObject, "OnlineButton").GetComponent<MenuButtonScript>().SetEnabled(flag);
		Util.GetObjectExactMatch(base.gameObject, "StoreButton").GetComponent<MenuButtonScript>().SetEnabled(flag);
		Util.GetObjectExactMatch(base.gameObject, "CampaignButton").GetComponent<MenuButtonScript>().SetEnabled(ShouldEnableCampaignButton());
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "VaultButton");
		objectExactMatch.GetComponent<MenuButtonScript>().SetEnabled(flag);
		if (Hub.Instance.State == HubState.Connected)
		{
			Util.GetObjectExactMatch(base.gameObject, "VaultTimer").SetActive(true);
			Util.GetObjectExactMatch(objectExactMatch, "Label").SetActive(false);
		}
		else
		{
			Util.GetObjectExactMatch(base.gameObject, "VaultTimer").SetActive(false);
			Util.GetObjectExactMatch(objectExactMatch, "Label").SetActive(true);
		}
		UpdateName();
		UpdateCounts();
		UpdateGachaSpinCounts();
		UpdateTournament();
	}

	private void ShowMotd()
	{
		Hub.Instance.GetManager<MessengerManager>().LoadMotd(SafeAction.Wrap(this, (Hashtable motd) =>
		{
			if (motd != null)
			{
				string uri = Dot.String("uri", motd, string.Empty);
				Hashtable hashtable = Dot.Object("query", motd, null);
				string title = Dot.String("title", motd, string.Empty);
				GameObject gameObject = ScreenManager.Instance.OpenPopup("WebViewDialogUI", false, true);
				gameObject.GetComponent<WebViewDialogUI>().OpenUrl(title, uri, null);
			}
			else
			{
				ProcessFullScreenPopups();
			}
		}));
	}

	private bool RequireLogin()
	{
		switch (Hub.Instance.State)
		{
		case HubState.Connected:
			return true;
		case HubState.Idle:
			ScreenManager.Instance.Confirmation(true, "ID_UI_LOGIN_OFFLINE_TITLE", "ID_UI_LOGIN_OFFLINE_BODY", "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", OnRequireLoginResponse);
			break;
		}
		return false;
	}

	private bool RequireInventory()
	{
		if (!InventoryDB.HasWeapons())
		{
			ScreenManager.Instance.Confirmation(true, "ID_UI_ERROR_TITLE", "ID_UI_INVENTORY_NOWEAPONS", "ID_UI_RESULTS_STORE", null, OnRequireStoreResponse);
			return false;
		}
		return true;
	}

	private void OnRequireLoginResponse(ConfirmationDialogUI.ButtonIds buttonId)
	{
		if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
		{
			Setup.Instance.Login(false);
		}
	}

	private void OnRequireStoreResponse(ConfirmationDialogUI.ButtonIds buttonId)
	{
		if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
		{
			OnStore(base.gameObject);
		}
	}

	private void UpdateTournament()
	{
		TournamentManager.TournamentInfo active = Hub.Instance.GetManager<TournamentManager>().Active;
		if (active != null)
		{
			Util.SetActiveRecursive(base.gameObject, "StandingsButton", false);
			Util.SetActiveRecursive(base.gameObject, "TournamentButton", true);
			UIUtils.SetLabelContents(base.gameObject, "TourneyRank", string.Empty);
			UIUtils.SetLabelContents(base.gameObject, "TourneyLive", "ID_UI_HOME_TOURNAMENT_LIVE");
		}
		else
		{
			Util.SetActiveRecursive(base.gameObject, "StandingsButton", true);
			Util.SetActiveRecursive(base.gameObject, "TournamentButton", false);
			UIUtils.SetLabelContents(base.gameObject, "TourneyRank", string.Empty);
			UIUtils.SetLabelContents(base.gameObject, "TourneyLive", string.Empty);
		}
		Coroutines.ClearInterval(_tournamentUpdate);
		_tournamentUpdate = Coroutines.SetInterval(UpdateTournamentCounter, 1000);
		UpdateTournamentCounter();
	}

	private void UpdateTournamentCounter()
	{
		TournamentManager.TournamentInfo active = Hub.Instance.GetManager<TournamentManager>().Active;
		if (active != null)
		{
			int num = Mathf.Max(0, active.ends - EB.Time.Now);
			if (num > 86400)
			{
				int num2 = (num + 86399) / 86400;
				UIUtils.SetLabelContents(tournamentCountdown, Localizer.Format("ID_UI_HOME_TOURNAMENT_COUNTDOWN_DAYS", num2));
			}
			else
			{
				string text = Localizer.FormatTime(num, true);
				UIUtils.SetLabelContents(tournamentCountdown, Localizer.Format("ID_UI_HOME_TOURNAMENT_COUNTDOWN_TIMER", text));
			}
			if (active.rank > 0)
			{
				UIUtils.SetLabelContents(base.gameObject, "TourneyRank", Localizer.Format("ID_UI_HOME_TOURNAMENT_RANK", "#" + active.rank));
			}
			else
			{
				UIUtils.SetLabelContents(base.gameObject, "TourneyRank", string.Empty);
			}
		}
	}

	private void OnDisable()
	{
		Coroutines.ClearInterval(_tournamentUpdate);
		_tournamentUpdate = null;
	}

	private void OnOpenPopup()
	{
		EB.Debug.Log("OPEN POPUP ON HOMEUI");
		ScreenManager.Instance.AddPersistentScreen("StatusBar");
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null && !persistentScreen.activeSelf)
		{
			NGUITools.SetActive(persistentScreen, true);
		}
		UpdateName();
		UpdateCounts();
		GameObject obj = ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
		backgroundUI.ShowCliff(true);
		CacDisplay cacDisplay = Util.FindComponent<CacDisplay>(base.gameObject);
		if (cacDisplay != null)
		{
			cacDisplay.SwapFace = false;
		}
		if (CampaignData.Instance.IsCampaignPlus())
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Practice");
			UILabel uILabel = Util.FindComponent<UILabel>(objectExactMatch);
			uILabel.text = "ID_UI_CAMPAIGN_TITLE_PLUS";
		}
		UpdateTimer();
		OnLoginStateChanged();
		ProcessFullScreenPopups();
	}

	public void OnReady()
	{
		EBUI_TransitionManager.Instance.HackClear();
		ScreenManager.Instance.EnableTransitionBlocker(false);
		ScreenManager.Instance.ResetCameraClearFlags();
		cacInstance.SetActive(true);
		CharacterManager.Instance.UpdateLocalCharacter();
		_isReady = true;
		if (Hub.Instance.State == HubState.Idle)
		{
			return;
		}
		if (!Profile.Instance.IsTutorialDone("firstboot_shown"))
		{
			Object original = Assets.Load("Prefab_Sequences/sequence_firstboot");
			Object.Instantiate(original);
			return;
		}
		string value = Hub.Instance.LoginManager.LocalUser.Name;
		if (Hub.Instance.LoginManager.LoginState != LoginState.LoggedIn)
		{
			return;
		}
		if (string.IsNullOrEmpty(value))
		{
			Hub.Instance.LoginManager.ChangeName();
		}
		else if (!Profile.Instance.IsTutorialDone("vault") && !Profile.Instance.IsTutorialDone("mp"))
		{
			Object original2 = Assets.Load("Prefab_Sequences/sequence_tutorial_vault");
			if (Hub.Instance.GachaManager.GetTokenCount(GachaManager.GachaConfig.DefaultTutorialBoxName) == 0)
			{
				Hub.Instance.GachaManager.AddTokenForBox(GachaManager.GachaConfig.DefaultTutorialGroupName, GachaManager.GachaConfig.DefaultTutorialGroupName, (string error, int updatedCount) =>
				{
					if (!string.IsNullOrEmpty(error))
					{
						UnityEngine.Debug.LogError("Can not add free token for tutorial: " + error);
						ScreenManager.Instance.ShowError("ID_SPARX_ERROR_UNKNOWN", null);
					}
				});
			}
			else
			{
				EB.Debug.Log("HomeUI > OnReady: can not give out more than 1 token for tutorial");
			}
			Object.Instantiate(original2);
		}
		else if (!Profile.Instance.IsTutorialDone("mp"))
		{
			int count = Hub.Instance.InventoryManager.GetCount("Elephant Gun");
			if (count > 4)
			{
				int num = Hub.Instance.InventoryManager.UseItem("Elephant Gun", count - 4);
				UnityEngine.Debug.LogWarning("Removing elephant gun from " + count + " to " + num + " for tutorial_mp");
			}
			else if (count < 4)
			{
				int num2 = Hub.Instance.InventoryManager.AddItem("Elephant Gun", 4 - count);
				UnityEngine.Debug.LogWarning("Adding elephant gun from " + count + " to " + num2 + " for tutorial_mp");
			}
			Object original3 = Assets.Load("Prefab_Sequences/sequence_tutorial_mp");
			Object.Instantiate(original3);
		}
		else if (ScreenManager.Instance.GetPopup("LobbyUI") == null)
		{
			Profile.Instance.ClearSeriesGachaAwards();
		}
	}

	private void ProcessFullScreenPopups()
	{
		HubState state = Hub.Instance.State;
		string value = Hub.Instance.LoginManager.LocalUser.Name;
		if (state == HubState.Idle || !Profile.Instance.IsTutorialDone("firstboot_shown") || string.IsNullOrEmpty(value) || !Profile.Instance.IsTutorialDone("mp") || ScreenManager.Instance.GetPopup("LobbyUI") != null)
		{
			ScreenManager.Instance.FadeOut(null);
			RunTransitions();
			return;
		}
		if (_beginnerSaleGotoStore)
		{
			ScreenManager.Instance.FadeOut(null);
			_beginnerSaleGotoStore = false;
			GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", false, true);
			StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
			storeCurrencyUI.currencyType = CurrencyType.Hard;
			return;
		}
		int num = EB.Time.Now / 86400;
		if (ShouldShowDailyOffers)
		{
			ScreenManager.Instance.FadeOut(null);
			StartCoroutine(TransitionBeginnerFlowPopup("DailyOffersUI"));
		}
		else if (ShouldShowBeginnersSale)
		{
			ScreenManager.Instance.FadeOut(null);
			StartCoroutine(TransitionBeginnerFlowPopup("BeginnerSaleUI"));
		}
		else if (ShouldShowFreeGachaSpin)
		{
			ScreenManager.Instance.FadeOut(null);
			Profile.Instance.GachaSpinCollectionSeen = true;
			ScreenManager.Instance.OpenPopup("GachaUI");
		}
		else if (num != LastMotdShown)
		{
			ScreenManager.Instance.FadeOut(null);
			StartCoroutine(TransitionBeginnerFlowPopup("Motd"));
		}
		else
		{
			RunTransitions();
		}
	}

	private IEnumerator TransitionBeginnerFlowPopup(string popupName)
	{
		yield return new WaitForSeconds(0.4f);
		foreach (ScreenManager.PopupInfo screenInfo in ScreenManager.Instance.GetPopupStack())
		{
			if (screenInfo.name != "HomeUI")
			{
				yield break;
			}
		}
		if ("DailyOffersUI".Equals(popupName))
		{
			GameObject popupObj = ScreenManager.Instance.OpenPopup("DailyOffersUI", false, true);
			PopupScript popup = popupObj.GetComponent<PopupScript>();
			popup.SetParentScreen(base.gameObject);
		}
		else if ("BeginnerSaleUI".Equals(popupName))
		{
			Profile.Instance.BeginnerSalePopupSeen = true;
			GameObject popupObj2 = ScreenManager.Instance.OpenPopup("BeginnerSaleUI", false, true);
			PopupScript popup2 = popupObj2.GetComponent<PopupScript>();
			popup2.SetParentScreen(base.gameObject);
		}
		else if ("Motd".Equals(popupName))
		{
			LastMotdShown = EB.Time.Now / 86400;
			ShowMotd();
		}
	}

	public void OnBeginnerSaleOfferBuyPressed()
	{
		_beginnerSaleGotoStore = true;
	}

	public void ScreenOutroDone()
	{
	}

	public void RunTransitions()
	{
		if (!playedCinematic)
		{
			ScreenManager.Instance.FadeOut(null);
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnReady));
		}
		else
		{
			playedCinematic = false;
		}
	}

	private void OnLoadingScreenClosed()
	{
	}

	private void OnOptions(GameObject g)
	{
		if (_enableInput)
		{
			ScreenManager.Instance.OpenPopup("OptionsUI", true, true);
		}
	}

	private void OnMyRobot(GameObject g)
	{
		if (_enableInput && RequireLogin())
		{
			TransitionOutTo("MyStuffUI");
		}
	}

	private void OnOnline(GameObject g)
	{
		if (_enableInput && RequireLogin() && RequireInventory())
		{
			TransitionOutTo("MatchmakingGameModeSelectUI");
		}
	}

	private void OnCampaign(GameObject g)
	{
		if (!_enableInput || !RequireLogin())
		{
			return;
		}
		if (!Assets.HasPack("scenes_ext") && !Assets.HasDLCPack("scenes_ext"))
		{
			GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
			ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
			confirmationDialogUI.SetParameters(true, "ID_UI_NEW_CONTENT_REQUIRED", "ID_UI_NEW_CONTENT_TITLE", "ID_UI_NEW_CONTENT_BUTTON", string.Empty);
			confirmationDialogUI.callback = (ConfirmationDialogUI.ButtonIds id) =>
			{
				if (id == ConfirmationDialogUI.ButtonIds.Left)
				{
					if (Application.isEditor)
					{
						ScreenManager.Instance.ClosePopup("BlockingUI");
					}
					else
					{
						Application.Quit();
					}
				}
				else
				{
					ScreenManager.Instance.ClosePopup("BlockingUI");
				}
			};
		}
		else
		{
			TransitionOutTo("SinglePlayerUI");
		}
	}

	private void OnPractice(GameObject g)
	{
		if (_enableInput)
		{
			TransitionOutTo("CampaignUI");
		}
	}

	private void OnFirstBootTutorial()
	{
		TransitionOutTo("CampaignUI");
	}

	private void OnMultiplayerTutorial()
	{
		TransitionOutTo("MatchmakingGameModeSelectUI");
	}

	private void OnStore(GameObject g)
	{
		if (_enableInput && RequireLogin())
		{
			TransitionOutTo("StoreUI");
		}
	}

	private void OnSelectName(GameObject go)
	{
		if (_enableInput && RequireLogin())
		{
			Hub.Instance.LoginManager.ChangeName();
		}
	}

	private void OnGacha(GameObject g)
	{
		if (_enableInput && RequireLogin())
		{
			TransitionOutTo("GachaUI");
		}
	}

	private void OnBack(GameObject g)
	{
		if (!_isReady)
		{
			return;
		}
		ScreenManager.Instance.Confirmation(true, "ID_UI_QUIT_CONFIRMATION_QUIT", "ID_UI_QUIT_CONFIRMATION", "ID_UI_QUIT_CONFIRMATION_QUIT", "ID_UI_COMMON_NO", (ConfirmationDialogUI.ButtonIds buttonId) =>
		{
			if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
			{
				Application.Quit();
			}
		});
	}

	private void TransitionOutTo(string targetScreen)
	{
		_isReady = false;
		if (!SinglePlayerUI.ShownAsteroid && targetScreen == "SinglePlayerUI")
		{
			targetScreen = "CampaignFirstCinematicUI";
		}
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, () =>
		{
			if (targetScreen == "CampaignFirstCinematicUI" || targetScreen == "SinglePlayerUI")
			{
				ScreenManager.Instance.FadeIn(() =>
				{
					cacInstance.SetActive(false);
					GameObject cinematicScreen = ScreenManager.Instance.OpenPopup(targetScreen);
					if (targetScreen == "CampaignFirstCinematicUI")
					{
						StartCoroutine(waitForCinematic(cinematicScreen));
					}
				});
			}
			else
			{
				cacInstance.SetActive(false);
				GameObject gameObject = ScreenManager.Instance.OpenPopup(targetScreen);
			}
		});
	}

	private IEnumerator waitForCinematic(GameObject cinematicScreen)
	{
		playedCinematic = true;
		while (cinematicScreen != null)
		{
			yield return new WaitForEndOfFrame();
		}
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.OpenPopup("SinglePlayerUI");
		});
	}

	private void OnMessages(GameObject g)
	{
		LoadWebPage(PageType.Messages);
	}

	private void OnTournaments(GameObject g)
	{
		LoadWebPage(PageType.Tournaments);
	}

	private void OnTrialPay(GameObject g)
	{
		string empty = string.Empty;
		empty = "TrialPay_Android";
		EB.Debug.Log("> HomeUI > OnTrialPay > offername:" + empty);
		Hub.Instance.WalletManager.GetOfferUrl(empty, SafeAction.Wrap(this, (string err, string url) =>
		{
			EB.Debug.Log("> HomeUI > OnTrialPay > offer URL:" + ((url == null) ? "<null>" : url));
			if (!string.IsNullOrEmpty(url))
			{
				Application.OpenURL(url);
			}
		}));
	}

	private static string GenerateWebViewUrl(PageType page, bool debug = false)
	{
		if (!debug)
		{
			string arg = Hub.Instance.ApiEndPoint.Url.Replace("https://", "http://");
			string arg2 = Hub.Instance.ApiEndPoint.GetData("stoken") ?? string.Empty;
			return string.Format("{0}/web?page={1}&stoken={2}", arg, (int)page, arg2);
		}
		switch (page)
		{
		case PageType.Messages:
			return "http://www.example.com";
		case PageType.Tournaments:
			return "http://www.iana.org/domains/reserved";
		case PageType.HallOfFame:
			return "http://www.iana.org/numbers";
		case PageType.Help:
			return "http://www.iana.org/numbers";
		default:
			return "http://www.iana.org/about";
		}
	}

	public static void OpenWebPage(PageType page, bool debug = false)
	{
		GameObject popup = ScreenManager.Instance.GetPopup("ResultsUI");
		if (popup != null)
		{
			ResultsUI component = popup.GetComponent<ResultsUI>();
			if (component != null)
			{
				popup.SendMessage("OnBack", popup, SendMessageOptions.DontRequireReceiver);
			}
		}
		string text = GenerateWebViewUrl(page, debug);
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Url", GenerateWebViewUrl(PageType.Messages, debug));
		dictionary.Add("AltRootUrl", "http://api.rivets-dev.sparx.io/web/messages");
		dictionary.Add("Image", "drawable/message");
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add("Id", 0);
		dictionary2.Add("LeftMargin", 75);
		dictionary2.Add("TopMargin", 5);
		dictionary.Add("Badge", dictionary2);
		list.Add(dictionary);
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		dictionary3.Add("Url", GenerateWebViewUrl(PageType.Tournaments, debug));
		dictionary3.Add("AltRootUrl", "http://api.rivets-dev.sparx.io/web/tournaments");
		dictionary3.Add("Image", "drawable/trophy");
		Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
		dictionary4.Add("Id", 1);
		dictionary4.Add("LeftMargin", 75);
		dictionary4.Add("TopMargin", 5);
		dictionary3.Add("Badge", dictionary4);
		list.Add(dictionary3);
		Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
		dictionary5.Add("Url", GenerateWebViewUrl(PageType.HallOfFame, debug));
		dictionary5.Add("AltRootUrl", "http://api.rivets-dev.sparx.io/web/halloffame");
		dictionary5.Add("Image", "drawable/halloffame");
		Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
		dictionary6.Add("Id", 2);
		dictionary6.Add("LeftMargin", 75);
		dictionary6.Add("TopMargin", 5);
		dictionary5.Add("Badge", dictionary6);
		list.Add(dictionary5);
		Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
		dictionary7.Add("Url", GenerateWebViewUrl(PageType.Help, debug));
		dictionary7.Add("AltRootUrl", "http://api.rivets-dev.sparx.io/web/help");
		dictionary7.Add("Image", "drawable/help");
		Dictionary<string, object> dictionary8 = new Dictionary<string, object>();
		dictionary8.Add("Id", 3);
		dictionary8.Add("LeftMargin", 75);
		dictionary8.Add("TopMargin", 5);
		dictionary7.Add("Badge", dictionary8);
		list.Add(dictionary7);
		Dictionary<string, object> dictionary9 = new Dictionary<string, object>();
		dictionary9.Add("CloseImage", "drawable/home");
		dictionary9.Add("BackgroundColour", 4280189549u);
		dictionary9.Add("TextColour", uint.MaxValue);
		dictionary9.Add("OnTop", false);
		dictionary9.Add("ExpandToFit", true);
		dictionary9.Add("BadgeImage", "drawable/badge");
		dictionary9.Add("HiResImagePostFix", "_high");
		Dictionary<string, object> dictionary10 = new Dictionary<string, object>();
		dictionary10.Add("titleBar", dictionary9);
		dictionary10.Add("tabs", list);
		dictionary10.Add("targetWidth", 1024);
		dictionary10.Add("BackgroundColour", uint.MaxValue);
		AndroidJavaClass androidJavaClass = (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android) ? new AndroidJavaClass("com.unity3d.player.UnityPlayer") : null;
		if (androidJavaClass != null)
		{
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			if (androidJavaObject != null)
			{
				if (androidJavaObject.Call<bool>("WebViewShowFullscreen", new object[2]
				{
					text,
					JSON.Stringify(dictionary10)
				}))
				{
					UnityEngine.Debug.Log("!*!*!*OpenWebView: Success");
				}
				else
				{
					UnityEngine.Debug.LogError("!*!*!*OpenWebView Failed - OpenWebView returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				UnityEngine.Debug.LogError("!*!*!*OpenWebView Init: Failed - Could not get the currentActivity");
			}
		}
		else
		{
			UnityEngine.Debug.LogError("!*!*!*OpenWebView Init: Failed - com.unity3d.player.UnityPlayer lookup failed");
		}
	}

	private void LoadWebPage(PageType page)
	{
		if (_enableInput && RequireLogin())
		{
			OpenWebPage(page);
		}
	}

	private void OnGameCenter(GameObject go)
	{
		if (_enableInput)
		{
			gcActivated = !gcActivated;
			Util.GetObjectExactMatch(base.gameObject, "TourneyLive").SetActive(!gcActivated);
			StartCoroutine(FadeButton(gcAchievements, gcActivated));
			StartCoroutine(FadeButton(gcLeaderboards, gcActivated));
			ButtonScript component = gcMain.GetComponent<ButtonScript>();
			if (gcActivated)
			{
				gcMainBackground.color = component.clickColor;
			}
			else
			{
				gcMainBackground.color = gcOriginalColor;
			}
		}
	}

	private IEnumerator FadeButton(GameObject go, bool fadeIn)
	{
		if (!fadeIn)
		{
			go.GetComponent<ButtonScript>().enabled = false;
		}
		else
		{
			go.SetActive(fadeIn);
		}
		UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>();
		for (int i = 0; i < widgets.Length; i++)
		{
			widgets[i].alpha = ((!fadeIn) ? 1f : 0f);
		}
		float animationTime = 0.25f;
		float curTime = 0f;
		while (curTime < animationTime)
		{
			curTime += UnityEngine.Time.deltaTime;
			float alpha = curTime / animationTime;
			for (int j = 0; j < widgets.Length; j++)
			{
				widgets[j].alpha = ((!fadeIn) ? (1f - alpha) : alpha);
			}
			yield return new WaitForFixedUpdate();
		}
		if (fadeIn)
		{
			go.GetComponent<ButtonScript>().enabled = true;
		}
		else
		{
			go.SetActive(false);
		}
	}

	private void OnGCAchievements(GameObject go)
	{
		if (_enableInput)
		{
			Hub.Instance.GameCenterManager.ShowAchievementsUI();
		}
	}

	private void OnGCLeaderboards(GameObject go)
	{
		if (_enableInput)
		{
			Hub.Instance.GameCenterManager.ShowLeaderboardUI();
		}
	}
}
