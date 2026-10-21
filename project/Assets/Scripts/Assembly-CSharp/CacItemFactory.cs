using System.Collections.Generic;
using EB;
using UnityEngine;

public class CacItemFactory : DummyGridItemFactory
{
	public CharacterDB.Category cacCategory;

	private List<CharacterDB.MetaData> categoryMetaData;

	protected override void Start()
	{
		categoryMetaData = new List<CharacterDB.MetaData>();
		RefreshMetadataCache(true);
		base.Start();
	}

	public override int GetNumItems()
	{
		return categoryMetaData.Count;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	public int GetPartIndex(CharacterDB.MetaData data)
	{
		for (int i = 0; i < categoryMetaData.Count; i++)
		{
			if (categoryMetaData[i] == data)
			{
				return i;
			}
		}
		return -1;
	}

	protected override void LoadObject(GameObject parent, int index)
	{
		GameObject gameObject = NGUITools.AddChild(parent, itemPrefab);
		CacGridItem cacGridItem = Util.FindComponent<CacGridItem>(gameObject);
		if (index < categoryMetaData.Count)
		{
			CharacterDB.MetaData metaData = categoryMetaData[index];
			gameObject.name = string.Format("{0}([{1}]{2})", itemPrefab.name, index, metaData.name);
			cacGridItem.metaData = metaData;
			cacGridItem.texturePath = "cac_ui_parts/" + metaData.cacTexturePath;
			UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
			uITutorialArrow.tutorialId = metaData.name;
			uITutorialArrow.offset = Vector3.up * 120f + Vector3.back * 50f;
		}
		else
		{
			gameObject.name = string.Format("{0}([{1}]{2})", itemPrefab.name, index, "empty");
		}
		cacGridItem.UpdateArt();
		cacGridItem.Load();
	}

	protected override void UnloadObject(GameObject parent, int index)
	{
		CacGridItem cacGridItem = Util.FindComponent<CacGridItem>(parent);
		if ((bool)cacGridItem)
		{
			cacGridItem.Unload();
			Object.Destroy(cacGridItem.gameObject);
		}
	}

	public override void OnInit()
	{
		base.gameObject.SendMessageUpwards("OnCacItemsReady", cacCategory);
	}

	public void RefreshMetadataCache(bool showOwned)
	{
		categoryMetaData.Clear();
		List<CharacterDB.MetaData> category = CharacterDB.GetCategory(cacCategory);
		foreach (CharacterDB.MetaData item in category)
		{
			bool flag = item.previousTierItem != null;
			if (showOwned)
			{
				if (item.IsOwned)
				{
					categoryMetaData.Add(item);
					if ((item.isChampionshipBoss || item.isEpic) && item.nextTierItem != null && item.nextTierItem.IsOwned)
					{
						categoryMetaData.Remove(item);
					}
				}
			}
			else if (!item.IsOwned)
			{
				bool flag2 = true;
				bool flag3 = flag && Profile.Instance.GetPartUpgradeProgress(item.previousTierItem.hash) == 100f;
				bool flag4;
				if (item.isEpic)
				{
					flag4 = flag3;
				}
				else
				{
					flag4 = ((!item.isChampionshipBoss) ? (!item.IsHidden || flag3) : (!item.IsHidden && (!flag || flag3)));
				}
				if (flag4)
				{
					categoryMetaData.Add(item);
				}
			}
		}
		categoryMetaData.Sort(CompareCacItems);
		targetGrid.SetActive(categoryMetaData.Count > 0);
	}

	private static int CompareCacItems(CharacterDB.MetaData a, CharacterDB.MetaData b)
	{
		if (a.IsOwned && !b.IsOwned)
		{
			return -1;
		}
		if (!a.IsOwned && b.IsOwned)
		{
			return 1;
		}
		return CharacterDB.MetaData.Compare(a, b);
	}
}
