using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EB;
using EB.Replication;
using EB.Sparx;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
	private enum DebugMenuMode
	{
		Main = 0,
		ResourceUsage = 1,
		Profile = 2,
		Gacha = 3,
		Currency = 4,
		CAC = 5,
		Levels = 6,
		UI = 7,
		Stats = 8,
		Emotes = 9,
		Inventory = 10,
		Tuning = 11,
		Championship = 12,
		Assets = 13,
		ItemLevels = 14,
		Boosts = 15
	}

	private const float hiResButtonSize = 70f;

	private const float loResButtonSize = 35f;

	private const float editorOffset = 0f;

	private const float deviceRetinaOffset = 40f;

	private const float deviceOffset = 20f;

	public bool FarseerVisualization;

	private Vector2 scrollPos = default(Vector2);

	private bool _slow;

	private float _buttonSize;

	private float _yOffset;

	private DebugMenuMode mCurrentMode;

	private bool _AITakeover;

	private int _xp = 10;

	private int _money = 10;

	private int _selectedDivision;

	public static bool Show { get; set; }

	private bool ShowQuickButtons { get; set; }

	private void Start()
	{
		DebugSettings.FSDebugOn = FarseerVisualization;
		if (UnityEngine.Debug.isDebugBuild)
		{
			ShowQuickButtons = true;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void SetDebugMode(DebugMenuMode mode)
	{
		mCurrentMode = mode;
		scrollPos.x = 0f;
		scrollPos.y = 0f;
	}

	public void OnGUI()
	{
		if (Setup.Instance == null)
		{
			return;
		}
		_buttonSize = 35f;
		if (Misc.IsRetina())
		{
			_buttonSize = 70f;
		}
		_yOffset = 20f;
		if (Application.isEditor)
		{
			_yOffset = 0f;
		}
		else if (Misc.IsRetina())
		{
			_yOffset = 40f;
		}
		Event current = Event.current;
		if ((Application.isEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.WindowsPlayer || Application.isWebPlayer) && Event.current.type == EventType.KeyUp)
		{
			KeyCode keyCode = current.keyCode;
			if (keyCode == KeyCode.Backslash)
			{
				if (!current.shift)
				{
					ShowQuickButtons = !ShowQuickButtons;
				}
				else
				{
					ChallengeManager.DebugOn = !ChallengeManager.DebugOn;
				}
				Event.current.Use();
			}
		}
		if (ShowQuickButtons)
		{
			float num = 0f;
			float num2 = 48f;
			float num3 = (float)Screen.width / (float)Screen.height;
			GUI.Label(new Rect(0f, num += num2, 600f, num2), string.Format("Device model {0} name {1} type {2} gpu {7} dpi {3} {4}x{5} aspect {6}", SystemInfo.deviceModel, SystemInfo.deviceName, SystemInfo.deviceType, Screen.dpi, Screen.width, Screen.height, num3, SystemInfo.graphicsDeviceName));
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Show Debug Menu"))
			{
				Show = !Show;
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "End Game"))
			{
				EB.Replication.Manager.RPC("EndGame", RPCMode.All, 1f);
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Add SP"))
			{
				AttributesManager.instance.AddSkillPoints(100);
			}
			if (Profile.Instance != null)
			{
				string text = ((!Profile.Instance.RevengeSaleAlwaysShows) ? "Enable Revenge Sale Always" : "Disable Revenge Sale Always");
				if (GUI.Button(new Rect(0f, num += num2, 200f, num2), text))
				{
					Profile.Instance.RevengeSaleAlwaysShows = !Profile.Instance.RevengeSaleAlwaysShows;
				}
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Kill Everyone Else"))
			{
				List<EB.Sparx.Player> players = EB.Replication.Manager.Players;
				foreach (EB.Sparx.Player item in players)
				{
					if (Dot.Bool("ai", item.Attributes, false))
					{
						Misc.HitInfo hitInfo = new Misc.HitInfo
						{
							_attackerID = EB.Replication.Manager.LocalPlayerId,
							_bullet = true,
							_damage = float.MaxValue
						};
						Player player = LevelData.Instance.GetPlayer(item.PlayerId);
						player.GetComponent<ReplicationView>().RPC("ApplyHit", RPCMode.All, hitInfo);
					}
				}
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Kick Me"))
			{
				Game game = Hub.Instance.GameManager.Game;
				if (game != null && !game.Network.IsLocal)
				{
					game.LeaveGame("ID_GAME_KICKED_IDLE");
					LevelData.Instance.TerminateGame();
				}
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Spawn SP crate"))
			{
				CrateManager.Instance.SpawnSP();
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Gunz Galore!"))
			{
				List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
				Hashtable hashtable = new Hashtable();
				foreach (InventoryDB.MetaData item2 in categoryList)
				{
					if (Hub.Instance.InventoryManager.GetCount(item2.name) <= 0)
					{
						hashtable.Add(item2.name, item2.quantity);
					}
				}
				if (hashtable.Count > 0)
				{
					Hub.Instance.InventoryManager.AddItems(hashtable);
				}
			}
			if (GUI.Button(new Rect(0f, num += num2, 200f, num2), "Up your Grade"))
			{
				ScreenManager.Instance.BroadcastMessage("DoUpgradeDebugFull");
			}
		}
		if (LevelData.Instance != null)
		{
		}
		if (Show)
		{
			GUI.Window(9999, new Rect((float)Screen.width * 0.25f, _yOffset, (float)Screen.width * 0.5f, (float)Screen.height * 0.8f), Window, "Console");
		}
	}

	private void MuteOther()
	{
	}

	private void Update()
	{
		int num = 0;
		Touch[] touches = Input.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
			{
				num++;
			}
		}
		if (num == 4)
		{
			ShowQuickButtons = !ShowQuickButtons;
		}
	}

	private void DrawMainGUI()
	{
		GUILayout.Label("SCREEN WIDTH " + Screen.width);
		GUILayout.Label("SCREEN HEIGHT " + Screen.height);
		GUILayout.Label("TRANSITION BLOCKER STATE " + ScreenManager.Instance.GetTransitionBlockerState());
		if (GUILayout.Button("ENABLE FG"))
		{
			ScreenManager.Instance.FadeIn(null);
		}
		if (GUILayout.Button("DISABLE FG"))
		{
			ScreenManager.Instance.FadeOut(null);
		}
		if (GUILayout.Button("AI Takeover : " + _AITakeover, GUILayout.MinHeight(_buttonSize)))
		{
			_AITakeover = !_AITakeover;
			if (_AITakeover)
			{
				Player player = LevelData.Instance.GetPlayer();
				if (player != null)
				{
					AISimple aISimple = player.gameObject.AddComponent<AISimple>();
					aISimple.IsBot = true;
					aISimple.OrderBotArsenal();
					player.GetComponent<PlayerControlByGUI>().enabled = false;
				}
				else
				{
					UnityEngine.Object.Destroy(player.GetComponent<AISimple>());
					player.GetComponent<PlayerControlByGUI>().enabled = true;
				}
			}
		}
		if (GUILayout.Button("Web View", GUILayout.MinHeight(_buttonSize)))
		{
			UnityEngine.Debug.LogError("SHOW MOTD");
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
			}));
		}
		if (GUILayout.Button("Test Arrow", GUILayout.MinHeight(_buttonSize)))
		{
			TutorialManager.Instance.EnableArrow("Teleport Grenade", true);
		}
		if (GUILayout.Button("FPS Toggle", GUILayout.MinHeight(_buttonSize)))
		{
			FPSHUD.Show = !FPSHUD.Show;
		}
		if (GUILayout.Button("Automate", GUILayout.MinHeight(_buttonSize)))
		{
			if (Automation.Instance == null)
			{
				new GameObject("automation", typeof(Automation));
			}
			Show = false;
			return;
		}
		if ((bool)Scoring.Instance && GUILayout.Button("Score", GUILayout.MinHeight(_buttonSize)))
		{
			Scoring.Instance.AddScore(EB.Replication.Manager.LocalPlayerId, 2000);
			return;
		}
		foreach (int value in Enum.GetValues(typeof(DebugMenuMode)))
		{
			if (GUILayout.Button(((DebugMenuMode)value).ToString(), GUILayout.MinHeight(_buttonSize)))
			{
				SetDebugMode((DebugMenuMode)value);
			}
		}
		if (GUILayout.Button("New UDID", GUILayout.MinHeight(_buttonSize)))
		{
			PlayerPrefs.DeleteAll();
			string text = Device.NewUniqueIdentifier();
			PlayerPrefs.Save();
			EB.Debug.Log("udid: " + text);
		}
		if (GUILayout.Button("Test Exception", GUILayout.MinHeight(_buttonSize)))
		{
			throw new Exception("test exception");
		}
		if (GUILayout.Button("Toggle FS Update", GUILayout.MinHeight(_buttonSize)) && FSWorldComponent.PhysicsWorld != null)
		{
			FSWorldComponent.PhysicsWorld.Enabled = !FSWorldComponent.PhysicsWorld.Enabled;
		}
		if (GUILayout.Button("Toggle Terrain Drawing", GUILayout.MinHeight(_buttonSize)))
		{
			LevelData.DrawingTerrain = !LevelData.DrawingTerrain;
		}
		if (GUILayout.Button("Enable fiddle mode", GUILayout.MinHeight(_buttonSize)))
		{
			LobbyUI.fiddleMode = true;
		}
		if (GUILayout.Button("Unlocks Manager: Unlock All", GUILayout.MinHeight(_buttonSize)))
		{
			UnlocksManager.UnlockAll();
		}
		if (GUILayout.Button("Toggle Level Destruction : " + DebugSettings.LevelDestruction, GUILayout.MinHeight(_buttonSize)))
		{
			DebugSettings.LevelDestruction = !DebugSettings.LevelDestruction;
		}
		if (GUILayout.Button("Toggle Finger Bools : " + DebugSettings.FingerBools, GUILayout.MinHeight(_buttonSize)))
		{
			DebugSettings.FingerBools = !DebugSettings.FingerBools;
		}
		if (GUILayout.Button("Toggle Explosions : " + DebugSettings.ExplosionsOn, GUILayout.MinHeight(_buttonSize)))
		{
			DebugSettings.FingerBools = !DebugSettings.ExplosionsOn;
		}
		if (GUILayout.Button("Toggle LevelData Timed Events : " + DebugSettings.LevelDataTimedEvents, GUILayout.MinHeight(_buttonSize)))
		{
			DebugSettings.LevelDataTimedEvents = !DebugSettings.LevelDataTimedEvents;
		}
		if (GUILayout.Button("Toggle Player Debug : " + Player.DebugOn, GUILayout.MinHeight(_buttonSize)))
		{
			Player.DebugOn = !Player.DebugOn;
		}
		if (GUILayout.Button("Toggle MeleeWeapon Debug : " + MeleeWeapon.DebugOn, GUILayout.MinHeight(_buttonSize)))
		{
			MeleeWeapon.DebugOn = !MeleeWeapon.DebugOn;
		}
		if (GUILayout.Button("Toggle CrateManager Debug : " + CrateManager.DebugOn, GUILayout.MinHeight(_buttonSize)))
		{
			CrateManager.DebugOn = !CrateManager.DebugOn;
		}
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			if (GUILayout.Button("Leave Game", GUILayout.MinHeight(_buttonSize)))
			{
				Hub.Instance.GameManager.LeaveGame();
			}
			if (GUILayout.Button("End Game", GUILayout.MinHeight(_buttonSize)))
			{
				EB.Replication.Manager.RPC("EndGame", RPCMode.All, 1f);
			}
		}
	}

	private void DrawCacGUI()
	{
		if (GUILayout.Button("Reset Upgrade Progress", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.ResetPartUpgradeProgress();
		}
		for (int i = 0; i < 3; i++)
		{
			GUILayout.Label(((CharacterDB.Category)i).ToString());
			List<CharacterDB.MetaData> category = CharacterDB.GetCategory((CharacterDB.Category)i);
			foreach (CharacterDB.MetaData item in category)
			{
				float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(item.hash);
				string friendlyName = item.friendlyName;
				friendlyName = friendlyName + " Prog: " + partUpgradeProgress;
				friendlyName = friendlyName + ", Dmg: " + PartManager.Instance.GetPartAttributeValue(item.hash, RobotAttribute.RobotAttributeType.Damage, partUpgradeProgress);
				friendlyName = friendlyName + ", Arm: " + PartManager.Instance.GetPartAttributeValue(item.hash, RobotAttribute.RobotAttributeType.Armor, partUpgradeProgress);
				friendlyName = friendlyName + ", Hp: " + PartManager.Instance.GetPartAttributeValue(item.hash, RobotAttribute.RobotAttributeType.Health, partUpgradeProgress);
				friendlyName = friendlyName + ", Spd: " + PartManager.Instance.GetPartAttributeValue(item.hash, RobotAttribute.RobotAttributeType.Speed, partUpgradeProgress);
				friendlyName = friendlyName + ", Jmp: " + PartManager.Instance.GetPartAttributeValue(item.hash, RobotAttribute.RobotAttributeType.Jump, partUpgradeProgress);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(friendlyName, GUILayout.MinHeight(_buttonSize)))
				{
					CharacterManager.Instance.LocalPlayer.recipe.SetPart(item, Profile.Instance.GetPartUpgradeProgress(item.hash));
					CharacterManager.Instance.UpdateLocalCharacter();
				}
				if (GUILayout.Button("Upgrade", GUILayout.MinHeight(_buttonSize)))
				{
					float upgradeRatio;
					PartManager.Instance.Upgrade(item.hash, out upgradeRatio);
				}
				if (GUILayout.Button("UI", GUILayout.MinHeight(_buttonSize)))
				{
					GameObject obj = ScreenManager.Instance.OpenPopup("UpgradeUI", true);
					Util.FindComponent<UpgradeUI>(obj).SetParameters(item);
				}
				GUILayout.EndHorizontal();
			}
		}
	}

	private int IntField(int value, params GUILayoutOption[] options)
	{
		string s = GUILayout.TextField(value.ToString(), options);
		int result;
		if (int.TryParse(s, out result))
		{
			return result;
		}
		return value;
	}

	private void DrawGachaGUI()
	{
		GameObject popup = ScreenManager.Instance.GetPopup("GachaUI");
		if (popup == null && GUILayout.Button("Open GachaUI", GUILayout.MinHeight(_buttonSize)))
		{
			HubState state = Hub.Instance.State;
			if (state == HubState.Connected)
			{
				ScreenManager.Instance.OpenPopup("GachaUI");
			}
			else
			{
				ScreenManager.Instance.Confirmation(true, "ID_UI_LOGIN_OFFLINE_TITLE", "ID_UI_LOGIN_OFFLINE_BODY", "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", (ConfirmationDialogUI.ButtonIds buttonId) =>
				{
					if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
					{
						Setup.Instance.Login(false);
					}
				});
			}
		}
		if (GUILayout.Button("Force Gacha Timer Ready", GUILayout.MinHeight(_buttonSize)))
		{
			VaultTimer.Instance.ForceVaultTimerReady();
		}
		if (Hub.Instance.GameManager.Game != null && LevelData.Instance != null && GUILayout.Button("Force Spawn Gacha Token in Game", GUILayout.MinHeight(_buttonSize)))
		{
			CrateManager.Instance.SpawnCoin();
		}
		GUILayout.TextArea("Free Spins: " + GachaManager.Instance.GetTotalFreeSpins(), GUILayout.MinHeight(_buttonSize));
		if (GUILayout.Button("Add Free Spin", GUILayout.MinHeight(_buttonSize)))
		{
			GachaManager.Instance.AddFreeSpin(0);
		}
		if (popup != null && GUILayout.Button("Test GachaUI Tutorial Arrows", GUILayout.MinHeight(_buttonSize)))
		{
			UITutorialArrow[] array = Util.FindAllComponents<UITutorialArrow>(popup);
			UITutorialArrow[] array2 = array;
			foreach (UITutorialArrow uITutorialArrow in array2)
			{
				uITutorialArrow.ShowArrow();
			}
		}
		if (GUILayout.Button("Test Pulling Gacha Tables", GUILayout.MinHeight(_buttonSize)))
		{
			Hub instance = Hub.Instance;
			Request request = instance.ApiEndPoint.Get("/gacha/getTables");
			instance.ApiEndPoint.Service(request, SafeAction.Wrap(this, (Response response) =>
			{
				EB.Debug.Log("Got a server response - successful? " + response.sucessful);
				foreach (Hashtable array3 in response.arrayList)
				{
					int num2 = Dot.Integer("gachaTableID", array3, -1);
					int num3 = Dot.Integer("softCost", array3, -1);
					int num4 = Dot.Integer("hardCost", array3, -1);
					int num5 = Dot.Integer("multiplier", array3, -1);
					EB.Debug.Log("Table " + num2 + " s:" + num3 + " h:" + num4 + " m:" + num5);
				}
			}));
		}
		if (GUILayout.Button("Test Gacha Pick", GUILayout.MinHeight(_buttonSize)))
		{
			Hub instance2 = Hub.Instance;
			Request request2 = instance2.ApiEndPoint.Post("/gacha/pick");
			request2.AddData("gachaTableID", 0);
			request2.AddData("softPaid", 0);
			request2.AddData("hardPaid", 0);
			request2.AddData("xpAwarded", 0);
			request2.AddData("nonce", Nonce.Generate());
			instance2.ApiEndPoint.Service(request2, SafeAction.Wrap(this, (Response response) =>
			{
				EB.Debug.Log("Got a server response - successful? " + response.sucessful);
				string text = Dot.String("type", response.hashtable, string.Empty);
				int num2 = Dot.Integer("quantity", response.hashtable, 0);
				EB.Debug.Log("> rewardType: " + text + " x" + num2);
			}));
		}
		if (GUILayout.Button("Test Random Pick Selection", GUILayout.MinHeight(_buttonSize)))
		{
			Hub instance3 = Hub.Instance;
			Request request3 = instance3.ApiEndPoint.Get("/gacha/getRandomPickSelection");
			request3.AddData("gachaTableID", 0);
			request3.AddData("pickCount", 5);
			instance3.ApiEndPoint.Service(request3, SafeAction.Wrap(this, (Response response) =>
			{
				EB.Debug.Log("Got a server response - successful? " + response.sucessful);
				foreach (Hashtable array4 in response.arrayList)
				{
					string text = Dot.String("type", array4, string.Empty);
					int num2 = Dot.Integer("quantity", array4, 0);
					EB.Debug.Log("> rewardType: " + text + " x" + num2);
				}
			}));
		}
		if (!GUILayout.Button("Test All Gacha Rewards", GUILayout.MinHeight(_buttonSize)))
		{
			return;
		}
		EB.Debug.Log("> Test All Gacha Rewards ...");
		Hub instance4 = Hub.Instance;
		Request request4 = instance4.ApiEndPoint.Get("/gacha/getAllPossibleRewards");
		instance4.ApiEndPoint.Service(request4, SafeAction.Wrap(this, (Response response) =>
		{
			EB.Debug.Log("> Got a server response - successful? " + response.sucessful);
			foreach (string array5 in response.arrayList)
			{
				EB.Debug.Log("> rewardName: " + array5);
				if (GachaManager.Instance.GetRewardInfo(array5) == null)
				{
					EB.Debug.LogError("There is a problem with the reward: '{0}'", array5);
				}
			}
		}));
	}

	private void DrawProfileGUI()
	{
		GUILayout.Label("Level is " + Profile.Instance.Level);
		GUILayout.Label("Max Weapon Unlock Level is: " + Profile.Instance.GetMaxItemUnlockLevel(InventoryItem.Category.Weapon));
		GUILayout.Label("Max Boost Unlock Level is: " + Profile.Instance.GetMaxItemUnlockLevel(InventoryItem.Category.Boost));
		GUILayout.Label("User XP is : " + Profile.Instance.XP + " / " + Profile.Instance.NextLevelInfo.XP);
		GUILayout.Label("XP to add: " + _xp);
		_xp = (int)GUILayout.HorizontalSlider(_xp, 10f, 5000f, GUILayout.MinHeight(_buttonSize));
		if (GUILayout.Button("Add XP", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.AddXP(_xp);
			Profile.Instance.Save();
		}
		if (GUILayout.Button("Jump to next level XP -1", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.AddXP(Profile.Instance.NextLevelInfo.XP - Profile.Instance.XP - 1);
			Profile.Instance.Save();
		}
		if (GUILayout.Button("Clear XP", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.ClearXP();
			Profile.Instance.Save();
		}
		if (GUILayout.Button("Toggle Campaign+", GUILayout.MinHeight(_buttonSize)))
		{
			CampaignData.debugCampaignPlus = !CampaignData.debugCampaignPlus;
		}
		if (GUILayout.Button("Force Enable Tutorials", GUILayout.MinHeight(_buttonSize)))
		{
			DebugSettings.ForceTutorial = true;
		}
		GUILayout.Label("100% Revenge Sale Roll: " + DebugSettings.ForceRevengeSaleRoll);
		if (GUILayout.Button("Toggle 100% Revenge Sale Roll", GUILayout.MinHeight(_buttonSize)))
		{
			DebugSettings.ForceRevengeSaleRoll = !DebugSettings.ForceRevengeSaleRoll;
		}
	}

	private void DrawCurrencyGUI()
	{
		GUILayout.Label("Value");
		_money = IntField(_money, GUILayout.MinHeight(_buttonSize));
		if (GUILayout.Button("Add Hard", GUILayout.MinHeight(_buttonSize)))
		{
			Hub.Instance.WalletManager.Credit(_money, "debug");
		}
		if (GUILayout.Button("Rmv Hard", GUILayout.MinHeight(_buttonSize)))
		{
			Hub.Instance.WalletManager.Debit(_money, "debug");
		}
		if (GUILayout.Button("Zero Hard", GUILayout.MinHeight(_buttonSize)))
		{
			Hub.Instance.WalletManager.Debit(Hub.Instance.WalletManager.Balance, "debug");
		}
		if (GUILayout.Button("Add Soft", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.AddSoftCurrency(_money);
			Profile.Instance.Save();
		}
		if (GUILayout.Button("Rmv Soft", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.RemoveSoftCurrency(_money);
			Profile.Instance.Save();
		}
		if (GUILayout.Button("Zero Soft", GUILayout.MinHeight(_buttonSize)))
		{
			Profile.Instance.RemoveSoftCurrency(Profile.Instance.SoftCurrency);
			Profile.Instance.Save();
		}
	}

	private void DrawLevelsGUI()
	{
		if (!EB.Replication.Manager.IsConnected)
		{
			if (GUILayout.Button("You must create or join a server before selecting a level"))
			{
				Show = !Show;
			}
			return;
		}
		if (!EB.Replication.Manager.IsHost)
		{
			if (GUILayout.Button("You must be the host to select a level"))
			{
				Show = !Show;
			}
			return;
		}
		string[] levels = Setup.Instance.Levels;
		string[] array = levels;
		foreach (string text in array)
		{
			if (GUILayout.Button(text, GUILayout.MinWidth(100f), GUILayout.MinHeight(_buttonSize)))
			{
				EB.Replication.Manager.LoadLevel(text);
				Show = !Show;
			}
		}
	}

	private void DrawUIGUI()
	{
		if (GUILayout.Button("Home Screen", GUILayout.MinHeight(_buttonSize)))
		{
			ScreenManager.Instance.OpenPopup("HomeUI");
			Show = false;
		}
		if (GUILayout.Button("My Robot", GUILayout.MinHeight(_buttonSize)))
		{
			ScreenManager.Instance.OpenPopup("MyRobotUI");
			Show = false;
		}
		if (GUILayout.Button("Weapon Gameplay Inventory", GUILayout.MinHeight(_buttonSize)))
		{
			ScreenManager.Instance.OpenPopup("WeaponInventoryGameplayUI");
			Show = false;
		}
		if (GUILayout.Button("Upgrade UI", GUILayout.MinHeight(_buttonSize)))
		{
			ScreenManager.Instance.OpenPopup("UpgradeUI", true);
			Show = false;
		}
	}

	private void DrawStatsGUI()
	{
		if (GUILayout.Button("Reset All Stats", GUILayout.MinHeight(_buttonSize)))
		{
			StatManager.Instance.ResetAllStats();
		}
		for (int i = 0; i < 10; i++)
		{
			StatManager.Stat stat = (StatManager.Stat)i;
			GUILayout.BeginVertical();
			float stat2 = StatManager.Instance.GetStat(i);
			GUILayout.Label(stat.ToString() + " : " + stat2);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("+", GUILayout.MinHeight(_buttonSize), GUILayout.MinWidth(50f)))
			{
				StatManager.Instance.SetStat(i, stat2 + 1f);
			}
			if (GUILayout.Button("Reset " + stat, GUILayout.MinHeight(_buttonSize), GUILayout.MinWidth(50f)))
			{
				StatManager.Instance.ResetStat(stat);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}

	private void DrawEmotesGUI()
	{
		if (LevelData.Instance != null)
		{
			string[] emotes = EmoteManager.Instance.Emotes;
			string[] array = emotes;
			foreach (string text in array)
			{
				if (GUILayout.Button(text, GUILayout.MinHeight(_buttonSize)))
				{
					EmoteManager.Instance.SpawnEmote(text);
					Show = false;
				}
			}
		}
		else
		{
			GUILayout.Label("You need to be in a game to use emotes");
		}
	}

	private void DrawInventoryGUI()
	{
		GUILayout.Label("InventoryManager Override: " + Hub.Instance.InventoryManager.IsOverriden);
		GUILayout.Label("InventoryManager EverythingFree: " + Hub.Instance.InventoryManager.EverythingFree);
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		foreach (InventoryDB.MetaData item in categoryList)
		{
			GUILayout.BeginHorizontal();
			InventoryManager inventoryManager = Hub.Instance.InventoryManager;
			GUILayout.Label(Localizer.GetString(item.friendlyName) + " (" + item.name + ")");
			if (GUILayout.Button("-", GUILayout.MinHeight(_buttonSize), GUILayout.MinWidth(_buttonSize)))
			{
				inventoryManager.UseItem(item.name, 1);
			}
			GUILayout.Label(inventoryManager.GetCount(item.name).ToString());
			if (GUILayout.Button("+", GUILayout.MinHeight(_buttonSize), GUILayout.MinWidth(_buttonSize)))
			{
				inventoryManager.AddItem(item.name, 1);
			}
			GUILayout.EndHorizontal();
		}
	}

	private void DrawTuningGUI()
	{
		float player_JumpHeight = Tuning.Instance.Player_JumpHeight;
		GUILayout.Label("Player_JumpHeight: " + Tuning.Instance.Player_JumpHeight);
		Tuning.Instance.Player_JumpHeight = GUILayout.HorizontalSlider(Tuning.Instance.Player_JumpHeight, 0f, 150f, GUILayout.MinHeight(_buttonSize));
		if (player_JumpHeight != Tuning.Instance.Player_JumpHeight)
		{
			Player player = LevelData.Instance.GetPlayer();
			if (player != null)
			{
				player.Debug_UpdateTuningVariables();
			}
		}
		GUILayout.Label("Player_RunSpeed: " + Tuning.Instance.Player_RunSpeed);
		Tuning.Instance.Player_RunSpeed = GUILayout.HorizontalSlider(Tuning.Instance.Player_RunSpeed, 0f, 15f, GUILayout.MinHeight(_buttonSize));
	}

	private void DrawAssetsGUI()
	{
	}

	private void DrawChampionshipGUI()
	{
		GUILayout.Label("Value");
		_selectedDivision = IntField(_selectedDivision, GUILayout.MinHeight(_buttonSize));
		if (GUILayout.Button("Reset Division", GUILayout.MinHeight(_buttonSize)))
		{
			ChampionshipData.Instance.ResetProgress(_selectedDivision);
			ChampionshipData.Instance.ResetProgress(_selectedDivision);
			for (int i = 0; i < ChampionshipData.Instance.GetDivision(_selectedDivision)._stages.Length; i++)
			{
				string contextString = ChampionshipLeaderboards.GetContextString(_selectedDivision, i);
				Hub.Instance.LeaderboardManager.ReportScore(5, contextString, LeaderboardManager.UpdateType.Set, 0, "0");
			}
			Hub.Instance.LeaderboardManager.ReportScore(6, _selectedDivision.ToString(), LeaderboardManager.UpdateType.Set, 0);
		}
		if (GUILayout.Button("Unlock Next Level", GUILayout.MinHeight(_buttonSize)))
		{
			Division.Stage stage = ChampionshipData.CurrentForDiv(ChampionshipData.CurrentDivision);
			int num = 0;
			int num2 = 0;
			if (stage._mode == GameAttributes.GameModeType.KillCount)
			{
				num = stage._goldScoreThreshold + 1;
				num2 = 10000;
			}
			else
			{
				num2 = stage._goldScoreThreshold + 100;
				num = 5;
			}
			ChampionshipData.Instance.UpdateStarState(ChampionshipData.CurrentDivision, stage, num2, num);
			ChampionshipData.Instance.SetHighScore(ChampionshipData.CurrentDivision, stage, num2, num);
			ChampionshipData.Instance.SetPlayed(ChampionshipData.CurrentDivision, stage, true);
			ChampionshipPrefab.Reward[] firstTimeRewards = stage._firstTimeRewards;
			foreach (ChampionshipPrefab.Reward reward in firstTimeRewards)
			{
				if (reward.IsValid())
				{
					Hub.Instance.InventoryManager.AddItem(reward.assetName, reward.quantity);
				}
			}
			GameObject popup = ScreenManager.Instance.GetPopup("SinglePlayerUI");
			if (popup != null)
			{
				popup.SendMessage("OnDiv" + (ChampionshipData.CurrentDivision + 1), base.gameObject);
			}
		}
		if (GUILayout.Button("Unlock Entire Championship", GUILayout.MinHeight(_buttonSize)))
		{
			ChampionshipData.Instance.UnlockAll();
		}
	}

	private void DrawItemLevelsGUI()
	{
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		for (int i = 0; i < categoryList.Count; i++)
		{
			InventoryDB.MetaData metaData = categoryList[i];
			if (!metaData.name.ToLower().Equals("empty") && !metaData.name.ToLower().Equals("teleport grenade"))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(metaData.name);
				GUILayout.Label("LVL: " + (ItemLevelManager.Instance.GetLevel(metaData.hash) + 1));
				if (GUILayout.Button("-", GUILayout.MinHeight(_buttonSize)))
				{
					ItemLevelManager.Instance.DebugIncrementLevel(metaData.hash, -1);
				}
				if (GUILayout.Button("+", GUILayout.MinHeight(_buttonSize)))
				{
					ItemLevelManager.Instance.DebugIncrementLevel(metaData.hash, 1);
				}
				GUILayout.EndHorizontal();
			}
		}
	}

	private void DrawBoostsGUI()
	{
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Boost);
		for (int i = 0; i < categoryList.Count; i++)
		{
			InventoryDB.MetaData metaData = categoryList[i];
			GUILayout.BeginHorizontal();
			GUILayout.Label(metaData.name);
			GUILayout.Label("Quantity: " + Hub.Instance.InventoryManager.GetCount(metaData.name));
			if (GUILayout.Button("-", GUILayout.MinHeight(_buttonSize)))
			{
				Hub.Instance.InventoryManager.UseItem(metaData.name, 1);
			}
			if (GUILayout.Button("+", GUILayout.MinHeight(_buttonSize)))
			{
				Hub.Instance.InventoryManager.AddItem(metaData.name, 1);
			}
			GUILayout.EndHorizontal();
		}
	}

	private void DrawResourceUsageGUI()
	{
		if (GUILayout.Button("Print Meshes", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(Mesh));
		}
		if (GUILayout.Button("Print Everything", GUILayout.MinHeight(_buttonSize)))
		{
			UnityEngine.Debug.Log("Dumping Everything");
			PrintType(typeof(UnityEngine.Object));
		}
		if (GUILayout.Button("Print Textures", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(Texture));
		}
		if (GUILayout.Button("Print Meshes", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(Mesh));
		}
		if (GUILayout.Button("Print Transforms", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(Transform));
		}
		if (GUILayout.Button("Print UIbutton", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(UIButton));
		}
		if (GUILayout.Button("Print Asset Bundles", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(AssetBundle));
		}
		if (GUILayout.Button("Print Audio", GUILayout.MinHeight(_buttonSize)))
		{
			PrintType(typeof(AudioClip));
			PrintType(typeof(AudioSource));
		}
		if (GUILayout.Button("Do garbage collection"))
		{
			StartCoroutine(DoGarbageCollection());
		}
	}

	private IEnumerator DoGarbageCollection()
	{
		EB.Debug.Log("Running cleanup task");
		AsyncOperation operation = Resources.UnloadUnusedAssets();
		while (!operation.isDone)
		{
			yield return new WaitForFixedUpdate();
		}
	}

	private string ObjectPath(UnityEngine.Object obj)
	{
		Transform transform = null;
		if (obj is GameObject)
		{
			transform = ((GameObject)obj).transform.parent;
		}
		else if (obj is Component)
		{
			try
			{
				transform = ((Component)obj).transform.parent;
			}
			catch
			{
			}
		}
		if (transform != null)
		{
			return ObjectPath(transform) + "/" + obj.name;
		}
		return obj.name;
	}

	private void PrintType(Type type)
	{
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(type);
		List<string> list = new List<string>(array.Length);
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object obj in array2)
		{
			if (obj is Texture)
			{
				Texture texture = obj as Texture;
				int num = -1;
				int num2 = 1;
				if (obj is Texture2D)
				{
					float num3 = 0f;
					Texture2D texture2D = obj as Texture2D;
					switch (texture2D.format)
					{
					case TextureFormat.Alpha8:
						num3 = 1f;
						break;
					case TextureFormat.RGBA32:
					case TextureFormat.ARGB32:
					case TextureFormat.BGRA32:
						num3 = 4f;
						break;
					case TextureFormat.ARGB4444:
						num3 = 2f;
						break;
					case TextureFormat.PVRTC_4BPP_RGB:
					case TextureFormat.PVRTC_4BPP_RGBA:
						num3 = 0.5f;
						break;
					case TextureFormat.PVRTC_2BPP_RGB:
					case TextureFormat.PVRTC_2BPP_RGBA:
						num3 = 0.25f;
						break;
					}
					num = (int)((float)(texture2D.width * texture2D.height) * num3);
					num2 = texture2D.mipmapCount;
				}
				list.Add(string.Format("{0},{1},{2}x{3}, {4}", obj.GetType().Name, ObjectPath(obj), texture.width, texture.height, Profiler.GetRuntimeMemorySize(obj)));
			}
			else if (obj is Mesh)
			{
				Mesh mesh = obj as Mesh;
				list.Add(string.Format("{0}: {1}, v:{2}, id:{3} mem:{4}", obj.GetType().Name, ObjectPath(obj), mesh.vertexCount, obj.GetInstanceID(), Profiler.GetRuntimeMemorySize(obj)));
			}
			else
			{
				list.Add(string.Format("{1}: {0}, id:{2} mem:{3}", ObjectPath(obj), obj.GetType().Name, obj.GetInstanceID(), Profiler.GetRuntimeMemorySize(obj)));
			}
		}
		list.Sort();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} objects\n", array.Length);
		foreach (string item in list)
		{
			stringBuilder.AppendFormat("\t{0}\n", item);
		}
		UnityEngine.Debug.Log(stringBuilder);
	}

	private void Window(int id)
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		if (GUILayout.Button("Close", GUILayout.MinHeight(_buttonSize)))
		{
			Show = false;
		}
		if (mCurrentMode != DebugMenuMode.Main && GUILayout.Button("Back", GUILayout.MinHeight(_buttonSize)))
		{
			SetDebugMode(DebugMenuMode.Main);
		}
		GUILayout.Space(10f);
		switch (mCurrentMode)
		{
		case DebugMenuMode.ResourceUsage:
			DrawResourceUsageGUI();
			break;
		case DebugMenuMode.Main:
			DrawMainGUI();
			break;
		case DebugMenuMode.Profile:
			DrawProfileGUI();
			break;
		case DebugMenuMode.Gacha:
			DrawGachaGUI();
			break;
		case DebugMenuMode.Currency:
			DrawCurrencyGUI();
			break;
		case DebugMenuMode.CAC:
			DrawCacGUI();
			break;
		case DebugMenuMode.Levels:
			DrawLevelsGUI();
			break;
		case DebugMenuMode.UI:
			DrawUIGUI();
			break;
		case DebugMenuMode.Stats:
			DrawStatsGUI();
			break;
		case DebugMenuMode.Emotes:
			DrawEmotesGUI();
			break;
		case DebugMenuMode.Inventory:
			DrawInventoryGUI();
			break;
		case DebugMenuMode.Tuning:
			DrawTuningGUI();
			break;
		case DebugMenuMode.Championship:
			DrawChampionshipGUI();
			break;
		case DebugMenuMode.Assets:
			DrawAssetsGUI();
			break;
		case DebugMenuMode.ItemLevels:
			DrawItemLevelsGUI();
			break;
		case DebugMenuMode.Boosts:
			DrawBoostsGUI();
			break;
		}
		GUILayout.Space(10f);
		if (GUILayout.Button("Close", GUILayout.MinHeight(_buttonSize)))
		{
			Show = false;
		}
		GUILayout.EndScrollView();
	}
}
