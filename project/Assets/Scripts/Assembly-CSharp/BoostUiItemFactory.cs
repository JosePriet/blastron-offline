using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class BoostUiItemFactory : GridItemFactory
{
	public override int GetNumItems()
	{
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Boost);
		return categoryList.Count;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, itemPrefab);
		GridItem gridItem = Util.FindComponent<GridItem>(gameObject);
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Boost);
		if (index < categoryList.Count)
		{
			InventoryDB.MetaData metaData = categoryList[index];
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, index, metaData.name);
			GameObject obj = Assets.Load(metaData.assetPrefabPath) as GameObject;
			InventoryItem inventoryItem = Util.FindComponent<InventoryItem>(obj);
			string text = inventoryItem.uiTexturePath;
			if (string.IsNullOrEmpty(text))
			{
				text = "weapon_ui/UI_weapon_coming_soon";
			}
			gridItem.texturePath = text;
			UIUtils.SetLabelContents(gameObject, "ItemName", (!string.IsNullOrEmpty(metaData.friendlyName)) ? metaData.friendlyName : metaData.name);
			UIUtils.SetLabelContents(gameObject, "Count", Hub.Instance.InventoryManager.GetCount(metaData.name).ToString());
			UIUtils.SetLabelContents(gameObject, "ItemDescription", metaData.description);
			BoostUiItem component = gameObject.GetComponent<BoostUiItem>();
			component.SetMetaData(metaData);
		}
		else
		{
			gridItem.texturePath = "weapon_ui/UI_weapon_coming_soon";
			UIUtils.SetLabelContents(gameObject, "ItemName", string.Empty);
			UIUtils.SetLabelContents(gameObject, "Count", string.Empty);
			UIUtils.SetLabelContents(gameObject, "ItemDescription", string.Empty);
		}
		return gameObject;
	}

	public override void OnInit()
	{
		base.gameObject.SendMessageUpwards("OnItemCreationComplete", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}
}
