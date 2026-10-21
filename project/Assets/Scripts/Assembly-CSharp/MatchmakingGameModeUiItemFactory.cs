using System;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class MatchmakingGameModeUiItemFactory : GridItemFactory
{
	private List<int> _gameModes;

	private void Awake()
	{
		_gameModes = new List<int>();
		foreach (object value in Enum.GetValues(typeof(GameAttributes.GameModeType)))
		{
			_gameModes.Add((int)value);
		}
		_gameModes.Sort();
	}

	public override int GetNumItems()
	{
		return _gameModes.Count;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, itemPrefab);
		MatchmakingGameModeUiItem matchmakingGameModeUiItem = Util.FindComponent<MatchmakingGameModeUiItem>(gameObject);
		if (index < _gameModes.Count)
		{
			GameAttributes.GameModeType gameModeType = (GameAttributes.GameModeType)_gameModes[index];
			matchmakingGameModeUiItem.isEmpty = false;
			matchmakingGameModeUiItem.gameModeId = gameModeType;
			matchmakingGameModeUiItem.itemName = GameAttributes.GetLocalizedValue(GameAttributes.AttributeType.GameMode, gameModeType.ToString());
			matchmakingGameModeUiItem.itemSelected = false;
			matchmakingGameModeUiItem.itemIcon = "icon_" + gameModeType.ToString().ToLower();
		}
		else
		{
			matchmakingGameModeUiItem.itemSelected = false;
			matchmakingGameModeUiItem.itemName = string.Empty;
			matchmakingGameModeUiItem.isEmpty = true;
		}
		return gameObject;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	public override void OnInit()
	{
		base.gameObject.SendMessageUpwards("OnGameModeItemCreationComplete", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}
}
