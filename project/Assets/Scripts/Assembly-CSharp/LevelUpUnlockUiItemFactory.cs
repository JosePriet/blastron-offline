using System.Collections.Generic;
using EB;
using UnityEngine;

public class LevelUpUnlockUiItemFactory : GridItemFactory
{
	private List<InventoryDB.MetaData> inventoryMdList = new List<InventoryDB.MetaData>();

	public override int GetNumItems()
	{
		return inventoryMdList.Count;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, itemPrefab);
		LevelUpUnlockUiItem levelUpUnlockUiItem = Util.FindComponent<LevelUpUnlockUiItem>(gameObject);
		GridItem gridItem = Util.FindComponent<GridItem>(gameObject);
		Cleanup(gameObject);
		if (index < GetNumItems())
		{
			InventoryDB.MetaData metaData = inventoryMdList[index];
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, index, metaData.name);
			string itemName = Localizer.Format("ID_UI_LEVELUP_UNLOCKED", inventoryMdList[index].friendlyName);
			levelUpUnlockUiItem.SetItemName(itemName);
			gridItem.texturePath = metaData.uiTexturePath;
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
		base.gameObject.SendMessageUpwards("OnLevelUpUnlockItemCreationComplete", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}

	protected override void Init()
	{
		inventoryMdList = InventoryDB.GetUnlockedItems(Profile.Instance.Level);
		base.Init();
	}
}
