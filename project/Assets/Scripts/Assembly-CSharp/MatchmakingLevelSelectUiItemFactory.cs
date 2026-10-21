using System.Collections.Generic;
using EB;
using UnityEngine;

public class MatchmakingLevelSelectUiItemFactory : GridItemFactory
{
	private class LevelInfo
	{
		public string localizedName;

		public string internalName;

		public LevelInfo(string localizedName, string internalName)
		{
			this.internalName = internalName;
			this.localizedName = localizedName;
		}
	}

	private List<LevelInfo> levelInfo;

	public override int GetNumItems()
	{
		return levelInfo.Count;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, itemPrefab);
		MatchmakingLevelSelectUiItem matchmakingLevelSelectUiItem = Util.FindComponent<MatchmakingLevelSelectUiItem>(gameObject);
		GridItem gridItem = Util.FindComponent<GridItem>(gameObject);
		Cleanup(gameObject);
		if (index < GetNumItems())
		{
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, index, levelInfo[index].internalName);
			matchmakingLevelSelectUiItem.levelId = index;
			matchmakingLevelSelectUiItem.SetItemName(levelInfo[index].localizedName);
			matchmakingLevelSelectUiItem.EnableBorder(false);
			gridItem.texturePath = "levelthumb_ui/ui_" + levelInfo[index].internalName;
		}
		else
		{
			gridItem.texturePath = string.Empty;
		}
		return gameObject;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	public override void OnInit()
	{
		base.gameObject.SendMessageUpwards("OnLevelSelectItemCreationComplete", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}

	protected override void Init()
	{
		levelInfo = new List<LevelInfo>();
		string[] levels = Setup.Instance.Levels;
		foreach (string text in levels)
		{
			if (!Setup.Instance.IsExcluded(text))
			{
				string localizedValue = GameAttributes.GetLocalizedValue(GameAttributes.AttributeType.Level, text);
				LevelInfo item = new LevelInfo(localizedValue, text);
				levelInfo.Add(item);
			}
		}
		base.Init();
	}
}
