using System;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class InventoryUiItemFactory : GridItemFactory
{
	[System.Serializable]
	public class LevelRange
	{
		public int min;

		public int max = 999;

		public bool Contains(int i)
		{
			return min <= i && i <= max;
		}
	}

	public InventoryItem.Category itemCategory;

	public bool resetTextureScale = true;

	private List<InventoryDB.MetaData> _cachedMetadataList;

	public string interactiveChildName = string.Empty;

	private LevelRange _displayRange = new LevelRange();

	public LevelRange displayRange
	{
		get
		{
			return _displayRange;
		}
		set
		{
			_displayRange = value;
			_cachedMetadataList = GetMetadata(_displayRange);
		}
	}

	private List<InventoryDB.MetaData> cachedMetadataList
	{
		get
		{
			if (_cachedMetadataList == null)
			{
				_cachedMetadataList = GetMetadata(_displayRange);
			}
			return _cachedMetadataList;
		}
	}

	public override int GetNumItems()
	{
		return cachedMetadataList.Count;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	private List<InventoryDB.MetaData> GetMetadata(LevelRange range)
	{
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(itemCategory);
		List<InventoryDB.MetaData> list = new List<InventoryDB.MetaData>();
		InventoryManager inventoryManager = Hub.Instance.InventoryManager;
		foreach (InventoryDB.MetaData item in categoryList)
		{
			if (range.Contains(item.levelRequired) && (!inventoryManager.IsOverriden || inventoryManager.GetCount(item.name) != 0))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, itemPrefab);
		GridItem gridItem = Util.FindComponent<GridItem>(gameObject);
		Cleanup(gameObject);
		if (index < cachedMetadataList.Count)
		{
			InventoryDB.MetaData metaData = cachedMetadataList[index];
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, index, metaData.name);
			GameObject obj = Assets.Load(metaData.assetPrefabPath) as GameObject;
			InventoryItem inventoryItem = Util.FindComponent<InventoryItem>(obj);
			gridItem.texturePath = inventoryItem.uiTexturePath;
			UIUtils.SetLabelContents(gameObject, "ItemName", (!string.IsNullOrEmpty(metaData.friendlyName)) ? metaData.friendlyName : metaData.name);
			UIUtils.SetLabelContents(gameObject, "ItemDescription", metaData.description);
			InventoryUiItem component = gameObject.GetComponent<InventoryUiItem>();
			component.metaData = metaData;
			component.UpdateCount();
			if (GameAttributes.IsTutorialGameMode())
			{
				UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
				uITutorialArrow.tutorialId = metaData.name;
			}
		}
		else
		{
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, index, "empty");
			UIUtils.SetLabelContents(gameObject, "ItemName", string.Empty);
			UIUtils.SetLabelContents(gameObject, "Count", string.Empty);
			UIUtils.SetLabelContents(gameObject, "ItemDescription", string.Empty);
			gridItem.texturePath = "weapon_ui/UI_weapon_coming_soon";
		}
		UITexture componentInChildren = gameObject.GetComponentInChildren<UITexture>();
		Vector3 localScale = componentInChildren.transform.localScale;
		if (interactiveChildName != string.Empty)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(gameObject, interactiveChildName);
			if (objectExactMatch != null)
			{
				localScale = objectExactMatch.transform.localScale;
				localScale.z = 0f;
			}
		}
		BoxCollider boxCollider = gameObject.collider as BoxCollider;
		boxCollider.size = localScale;
		return gameObject;
	}

	public override void OnInit()
	{
		base.gameObject.SendMessageUpwards("OnItemCreationComplete", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}
}
