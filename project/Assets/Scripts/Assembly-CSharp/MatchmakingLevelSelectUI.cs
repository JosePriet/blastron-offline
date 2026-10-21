using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class MatchmakingLevelSelectUI : MonoBehaviour
{
	private GameAttributes.GameModeType selectedGameModeId = GameAttributes.GameModeType.Any;

	private MatchmakingLevelSelectUiItem[] levelSelectUiItems;

	private TableHelperUI tableHelper;

	private MatchmakingLevelSelectUiItem selectedItem;

	private bool popupLoaded;

	private bool itemsReady;

	private bool areWeReady;

	private bool pickRandomLevel;

	public void Awake()
	{
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		tableHelper.SetDelta(2);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		ButtonScript component = objectExactMatch.GetComponent<ButtonScript>();
		component.onButtonClick += OnBack;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "PrivateMatchButton");
		component = objectExactMatch2.GetComponent<ButtonScript>();
		component.onButtonClick += OnPrivateGame;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "RandomButton");
		component = objectExactMatch3.GetComponent<ButtonScript>();
		component.onButtonClick += OnPickRandomLevel;
		tableHelper.onItemSelected += OnItemSelected;
		selectedItem = null;
	}

	public void OnOpenPopup()
	{
		selectedItem = null;
		popupLoaded = true;
		pickRandomLevel = false;
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnReady));
	}

	public void OnReady()
	{
	}

	public void SetGameMode(GameAttributes.GameModeType selectedGameModeId)
	{
		this.selectedGameModeId = selectedGameModeId;
	}

	public void Update()
	{
		if (!areWeReady && popupLoaded && itemsReady)
		{
			areWeReady = true;
			levelSelectUiItems = Util.FindAllComponents<MatchmakingLevelSelectUiItem>(base.gameObject);
			for (int i = 0; i < levelSelectUiItems.Length; i++)
			{
				tableHelper.AddItem(levelSelectUiItems[i].gameObject);
			}
			SetSelectedItem(levelSelectUiItems[0]);
			tableHelper.Reset();
		}
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
		if (selectedGameModeId == GameAttributes.GameModeType.Any)
		{
			ArrayList arrayList = new ArrayList();
			foreach (int value4 in Enum.GetValues(typeof(GameAttributes.GameModeType)))
			{
				if (value4 != -1 && value4 != 4)
				{
					arrayList.Add((GameAttributes.GameModeType)value4);
				}
			}
			object value = arrayList[UnityEngine.Random.Range(0, arrayList.Count)];
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, value, result.ResetAttributes);
		}
		else
		{
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, selectedGameModeId, result.ResetAttributes);
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, selectedGameModeId, result.SearchAttributes);
		}
		int selectedIndex = GetSelectedIndex();
		List<string> list = new List<string>();
		string[] levels = Setup.Instance.Levels;
		foreach (string text in levels)
		{
			if (!Setup.Instance.IsExcluded(text))
			{
				list.Add(text);
			}
		}
		if (pickRandomLevel)
		{
			string value2 = list[UnityEngine.Random.Range(0, list.Count)];
			GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value2, result.ResetAttributes);
		}
		else
		{
			string value3 = list[selectedIndex];
			GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value3, result.ResetAttributes);
			GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value3, result.SearchAttributes);
		}
		return result;
	}

	public void OnPlay(GameObject caller)
	{
		GameManager.FindOrCreateGameParams pars = CreateParams();
		GameManager gm = Hub.Instance.GameManager;
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_ONLINE_SEARCHING_TITLE", "ID_UI_ONLINE_SEARCHING_DESC");
		Coroutines.SetTimeout(() =>
		{
			Telemetry.Event("game_search");
			gm.FindOrCreateGame(pars);
		}, 1000);
	}

	public void OnPrivateGame(GameObject button)
	{
		GameObject gameObject = ScreenManager.Instance.OpenPopup("PrivateGameUI", true, true);
		PrivateGameUI component = gameObject.GetComponent<PrivateGameUI>();
		component.Attributes = CreateParams().ResetAttributes;
	}

	public void OnGotoPrevious()
	{
		ScreenManager.Instance.ClosePopup();
	}

	public void OnBack(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnGotoPrevious);
	}

	public void OnLevelSelectLeft(GameObject caller)
	{
		tableHelper.OnScrollLeft();
	}

	public void OnLevelSelectRight(GameObject caller)
	{
		tableHelper.OnScrollRight();
	}

	public int GetSelectedIndex()
	{
		for (int i = 0; i < levelSelectUiItems.Length; i++)
		{
			if (selectedItem == levelSelectUiItems[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void SetSelectedItem(MatchmakingLevelSelectUiItem item)
	{
		if (selectedItem != null && selectedItem != item)
		{
			selectedItem.EnableBorder(false);
		}
		selectedItem = item;
		selectedItem.EnableBorder(true);
	}

	public void OnItemSelected(GameObject go)
	{
		MatchmakingLevelSelectUiItem[] array = levelSelectUiItems;
		foreach (MatchmakingLevelSelectUiItem matchmakingLevelSelectUiItem in array)
		{
			if (matchmakingLevelSelectUiItem.gameObject == go)
			{
				SetSelectedItem(matchmakingLevelSelectUiItem);
				OnPlay(go);
				break;
			}
		}
	}

	public void OnLevelSelectItemCreationComplete(GameObject caller)
	{
		itemsReady = true;
	}

	public void OnPickRandomLevel(GameObject caller)
	{
		pickRandomLevel = true;
		OnPlay(null);
	}
}
