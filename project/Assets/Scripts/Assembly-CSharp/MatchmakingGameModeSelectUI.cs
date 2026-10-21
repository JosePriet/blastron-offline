using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class MatchmakingGameModeSelectUI : MonoBehaviour
{
	private int iconCount;

	private bool haveWeRunIntro;

	private bool isReady;

	private GameAttributes.GameModeType selectedMode = GameAttributes.GameModeType.Any;

	private bool _enableInput = true;

	public void Awake()
	{
		EnableInput(false);
	}

	public void OnOpenPopup()
	{
		haveWeRunIntro = false;
		isReady = false;
		iconCount = 0;
		TournamentManager manager = Hub.Instance.GetManager<TournamentManager>();
		GameModeItemScript component = Util.GetObjectExactMatch(base.gameObject, "GameModeItem_A").GetComponent<GameModeItemScript>();
		component.SetIcon(GameAttributes.GameModeIcon[0], OnIconReady);
		component.SetText(GameAttributes.GameModeTitle[0]);
		UIEventListener uIEventListener = UIEventListener.Get(component.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnGameModeDeathMatch));
		if (manager != null && manager.Active != null)
		{
			Util.GetObjectExactMatch(component.gameObject, "TournamentBanner").SetActive(manager.Active.tournamentType == GameAttributes.GameModeType.DeathMatch);
		}
		component = Util.GetObjectExactMatch(base.gameObject, "GameModeItem_B").GetComponent<GameModeItemScript>();
		component.SetIcon(GameAttributes.GameModeIcon[1], OnIconReady);
		component.SetText(GameAttributes.GameModeTitle[1]);
		if (Tuning.Instance.Matchmaking_EnableTeamDeathMatch)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(component.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnGameModeTeamDeathMatch));
			GameObject objectExactMatch = Util.GetObjectExactMatch(component.gameObject, "ComingSoon");
			NGUITools.SetActive(objectExactMatch, false);
			if (manager != null && manager.Active != null)
			{
				Util.GetObjectExactMatch(component.gameObject, "TournamentBanner").SetActive(manager.Active.tournamentType == GameAttributes.GameModeType.TeamDeathMatch);
			}
		}
		component = Util.GetObjectExactMatch(base.gameObject, "GameModeItem_C").GetComponent<GameModeItemScript>();
		component.SetIcon(GameAttributes.GameModeIcon[2], OnIconReady);
		component.SetText(GameAttributes.GameModeTitle[2]);
		if (Tuning.Instance.Matchmaking_EnableKillCount)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(component.gameObject);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnGameModeKillCount));
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(component.gameObject, "ComingSoon");
			NGUITools.SetActive(objectExactMatch2, false);
			if (manager != null && manager.Active != null)
			{
				Util.GetObjectExactMatch(component.gameObject, "TournamentBanner").SetActive(manager.Active.tournamentType == GameAttributes.GameModeType.KillCount);
			}
		}
		component = Util.GetObjectExactMatch(base.gameObject, "GameModeItem_D").GetComponent<GameModeItemScript>();
		component.SetIcon(GameAttributes.GameModeIcon[3], OnIconReady);
		component.SetText(GameAttributes.GameModeTitle[3]);
		if (Tuning.Instance.Matchmaking_EnableSuddenDeath)
		{
			UIEventListener uIEventListener4 = UIEventListener.Get(component.gameObject);
			uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnGameModeSuddenDeath));
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(component.gameObject, "ComingSoon");
			NGUITools.SetActive(objectExactMatch3, false);
			if (manager != null && manager.Active != null)
			{
				Util.GetObjectExactMatch(component.gameObject, "TournamentBanner").SetActive(manager.Active.tournamentType == GameAttributes.GameModeType.SuddenDeath);
			}
		}
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		ButtonScript component2 = objectExactMatch4.GetComponent<ButtonScript>();
		component2.onButtonClick += OnBack;
		GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "PrivateMatchButton");
		ButtonScript component3 = objectExactMatch5.GetComponent<ButtonScript>();
		component3.onButtonClick += OnPrivateGame;
		GameObject objectExactMatch6 = Util.GetObjectExactMatch(base.gameObject, "RandomButton");
		if (Tuning.Instance.Matchmaking_EnableRandom)
		{
			ButtonScript component4 = objectExactMatch6.GetComponent<ButtonScript>();
			component4.onButtonClick += OnRandomize;
		}
		else
		{
			NGUITools.SetActive(objectExactMatch6, false);
		}
		isReady = true;
	}

	public void OnIconReady()
	{
		iconCount++;
	}

	public void OnReady()
	{
		ScreenManager.Instance.EnableTransitionBlocker(false);
		EB.Sequence.Component.Activate(base.gameObject, "MatchmakingGameModeSelectUI", typeof(SequenceEvent_OnScreenOpened));
		EnableInput(true);
	}

	public void Update()
	{
		if (!haveWeRunIntro && isReady && iconCount >= 3)
		{
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnReady));
			haveWeRunIntro = true;
		}
	}

	public void OnPrivateGame(GameObject button)
	{
		if (_enableInput)
		{
			CheckForNewContent(SafeAction.Wrap(this, () =>
			{
				GameObject gameObject = ScreenManager.Instance.OpenPopup("PrivateGameUI", true, true);
				PrivateGameUI component = gameObject.GetComponent<PrivateGameUI>();
				component.Attributes = CreateParamsPrivate().ResetAttributes;
			}));
		}
	}

	private GameManager.FindOrCreateGameParams CreateParamsPrivate()
	{
		GameManager.FindOrCreateGameParams result = new GameManager.FindOrCreateGameParams
		{
			MaxPlayers = 4,
			PlayerAttributes = Misc.CreatePlayerAttributes(),
			SearchAttributes = Misc.CreateDefaultSearchAttributes(),
			ResetAttributes = Misc.CreateDefaultAttributes()
		};
		GameHUD._settings._eVisFlags = GameHUD.Settings.Visibility.kChat | GameHUD.Settings.Visibility.kBoosts;
		ArrayList arrayList = new ArrayList();
		foreach (int value3 in Enum.GetValues(typeof(GameAttributes.GameModeType)))
		{
			if (value3 != -1 && value3 != 4 && (value3 != 1 || Tuning.Instance.Matchmaking_EnableTeamDeathMatch) && (value3 != 2 || Tuning.Instance.Matchmaking_EnableKillCount) && (value3 != 3 || Tuning.Instance.Matchmaking_EnableSuddenDeath))
			{
				arrayList.Add((GameAttributes.GameModeType)value3);
			}
		}
		object value = arrayList[UnityEngine.Random.Range(0, arrayList.Count)];
		GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, value, result.ResetAttributes);
		string value2 = Setup.Instance.Levels[UnityEngine.Random.Range(0, Setup.Instance.Levels.Length)];
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value2, result.ResetAttributes);
		return result;
	}

	private GameManager.FindOrCreateGameParams CreateParams()
	{
		GameManager.FindOrCreateGameParams result = new GameManager.FindOrCreateGameParams
		{
			MaxPlayers = 4,
			PlayerAttributes = Misc.CreatePlayerAttributes(),
			SearchAttributes = Misc.CreateDefaultSearchAttributes(),
			ResetAttributes = Misc.CreateDefaultAttributes()
		};
		GameHUD._settings._eVisFlags = GameHUD.Settings.Visibility.kChat | GameHUD.Settings.Visibility.kBoosts;
		if (selectedMode == GameAttributes.GameModeType.Any)
		{
			ArrayList arrayList = new ArrayList();
			foreach (int value3 in Enum.GetValues(typeof(GameAttributes.GameModeType)))
			{
				if (value3 != -1 && value3 != 4)
				{
					arrayList.Add((GameAttributes.GameModeType)value3);
				}
			}
			object value = arrayList[UnityEngine.Random.Range(0, arrayList.Count)];
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, value, result.ResetAttributes);
		}
		else
		{
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, selectedMode, result.ResetAttributes);
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, selectedMode, result.SearchAttributes);
		}
		List<string> list = new List<string>();
		string[] levels = Setup.Instance.Levels;
		foreach (string text in levels)
		{
			if (!Setup.Instance.IsExcluded(text))
			{
				list.Add(text);
			}
		}
		string value2 = list[UnityEngine.Random.Range(0, list.Count)];
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value2, result.ResetAttributes);
		return result;
	}

	public void OnGotoPrevious()
	{
		ScreenManager.Instance.ClosePopup();
	}

	public void OnGameModeKillCount(GameObject go)
	{
		if (_enableInput)
		{
			selectedMode = GameAttributes.GameModeType.KillCount;
			if (Tuning.Instance.Matchmaking_EnableLevelSelect)
			{
				EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoLevelSelect);
			}
			else
			{
				OnPlay(go);
			}
		}
	}

	public void OnGameModeDeathMatch(GameObject go)
	{
		if (_enableInput)
		{
			selectedMode = GameAttributes.GameModeType.DeathMatch;
			if (Tuning.Instance.Matchmaking_EnableLevelSelect)
			{
				EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoLevelSelect);
			}
			else
			{
				OnPlay(go);
			}
		}
	}

	public void OnGameModeTeamDeathMatch(GameObject go)
	{
		if (_enableInput)
		{
			selectedMode = GameAttributes.GameModeType.TeamDeathMatch;
			if (Tuning.Instance.Matchmaking_EnableLevelSelect)
			{
				EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoLevelSelect);
			}
			else
			{
				OnPlay(go);
			}
		}
	}

	public void OnGameModeSuddenDeath(GameObject go)
	{
		if (_enableInput)
		{
			selectedMode = GameAttributes.GameModeType.SuddenDeath;
			if (Tuning.Instance.Matchmaking_EnableLevelSelect)
			{
				EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoLevelSelect);
			}
			else
			{
				OnPlay(go);
			}
		}
	}

	public void OnGotoLevelSelect()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("MatchmakingLevelSelectUI");
		MatchmakingLevelSelectUI matchmakingLevelSelectUI = Util.FindComponent<MatchmakingLevelSelectUI>(obj);
		matchmakingLevelSelectUI.SetGameMode(selectedMode);
	}

	public void OnBack(GameObject caller)
	{
		if (_enableInput)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoPrevious);
		}
	}

	public void OnRandomize(GameObject caller)
	{
		if (_enableInput)
		{
			selectedMode = GameAttributes.GameModeType.Any;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoLevelSelect);
		}
	}

	public void OnPlay(GameObject caller)
	{
		GameManager.FindOrCreateGameParams pars = CreateParams();
		GameManager gm = Hub.Instance.GameManager;
		CheckForNewContent(SafeAction.Wrap(this, () =>
		{
			ScreenManager.Instance.BlockingUI("ID_UI_ONLINE_SEARCHING_TITLE", "ID_UI_ONLINE_SEARCHING_DESC");
			Telemetry.Event("game_search");
			gm.FindOrCreateGame(pars);
		}));
	}

	private void CheckForNewContent(EB.Action cb)
	{
		ScreenManager.Instance.BlockingUI(string.Empty, "ID_UI_LOGIN_BLOCKING_DESC");
		ContentManager.CheckForNewContent((bool newContent) =>
		{
			if (newContent)
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
				ScreenManager.Instance.ClosePopup("BlockingUI");
				cb();
			}
		});
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void OnMultiplayerTutorial()
	{
		selectedMode = GameAttributes.GameModeType.DeathMatch;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "GameModeItem_A");
		OnPlay(objectExactMatch);
	}
}
