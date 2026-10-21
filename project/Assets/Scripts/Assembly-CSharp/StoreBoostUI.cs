using System;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class StoreBoostUI : MonoBehaviour
{
	private const int itemsPerPage = 6;

	private bool initializedUiElements;

	private Dictionary<string, int> levelRangeNameToIndex = new Dictionary<string, int>();

	public GameObject itemPrefab;

	private TableHelperUI tableHelper;

	private UIGrid grid;

	private UIScrollBar scrollBar;

	private bool dragging;

	private List<InventoryDB.MetaData> currentMetaDataList = new List<InventoryDB.MetaData>();

	private GameObject scrollbar;

	private GameObject buttonLeft;

	private GameObject buttonRight;

	private List<UISprite> backgroundsToTint;

	private bool _enableInput = true;

	private void Awake()
	{
		grid = Util.GetObjectExactMatch(base.gameObject, "Grid").GetComponent<UIGrid>();
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		tableHelper.SetMaxVisibleItems(6);
		tableHelper.SetDelta(2);
		scrollBar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar").GetComponent<UIScrollBar>();
		UIEventListener uIEventListener = UIEventListener.Get(scrollBar.background.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(DropTest));
		Util.GetObjectExactMatch(base.gameObject, "BackButton").GetComponent<ButtonScript>().onButtonClick += OnBack;
		Util.GetObjectExactMatch(base.gameObject, "ButtonLootinator").GetComponent<ButtonScript>().onButtonClick += OnLootinator;
		buttonLeft = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		buttonRight = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		scrollbar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar");
		backgroundsToTint = new List<UISprite>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackgroundTinted");
		InitBackgroundTint(objectExactMatch);
	}

	private void Start()
	{
		UpdateItemsFromOwned();
	}

	private void InitBackgroundTint(GameObject bgObj)
	{
		if (bgObj != null)
		{
			UISprite component = bgObj.GetComponent<UISprite>();
			if (component != null)
			{
				backgroundsToTint.Add(component);
			}
		}
	}

	public void EnableNavigation(bool enable)
	{
		NGUITools.SetActive(buttonLeft, enable);
		NGUITools.SetActive(buttonRight, enable);
		NGUITools.SetActive(scrollbar, enable);
	}

	public void DropTest(GameObject v1, Vector2 g1)
	{
		if (_enableInput)
		{
			dragging = true;
		}
	}

	private void Update()
	{
		if (dragging && !Input.GetMouseButton(0))
		{
			tableHelper.CenterOnCurrentItem();
			dragging = false;
		}
		if (!initializedUiElements)
		{
			initializedUiElements = true;
			tableHelper.Reset();
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, null);
		}
	}

	public void OnExitToPrevious()
	{
		ScreenManager.Instance.ClosePopup();
	}

	private void OnBack(GameObject caller)
	{
		if (_enableInput)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToPrevious);
		}
	}

	private void OnLootinator(GameObject caller)
	{
		if (_enableInput)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToLootinator);
		}
	}

	private void OnExitToLootinator()
	{
		ScreenManager.Instance.OpenPopup("GachaUI");
	}

	private void OnOpenPopup()
	{
		UpdateItemsFromOwned();
		ScreenManager.Instance.FadeOut(() =>
		{
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, null);
		});
	}

	private void UpdateItemsFromOwned()
	{
		tableHelper.Clear();
		currentMetaDataList.Clear();
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Boost);
		for (int i = 0; i < categoryList.Count; i++)
		{
			GameObject obj = Assets.Load(categoryList[i].assetPrefabPath) as GameObject;
			BoostItem boostItem = Util.FindComponent<BoostItem>(obj);
			if (!(boostItem == null) && Hub.Instance.InventoryManager.GetCount(categoryList[i].name) > 0)
			{
				GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
				gameObject.GetComponent<BoostUiItem>().SetMetaData(categoryList[i]);
				gameObject.AddComponent<UIDragPanelContents>();
				tableHelper.AddItem(gameObject);
				currentMetaDataList.Add(categoryList[i]);
				UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
				uITutorialArrow.tutorialId = categoryList[i].name;
				uITutorialArrow.offset = Vector3.up * 120f + Vector3.back * 50f;
			}
		}
		tableHelper.Reset();
		scrollBar.scrollValue = 0f;
		scrollBar.ForceUpdate();
		if (currentMetaDataList.Count <= 6)
		{
			EnableNavigation(false);
		}
		else
		{
			EnableNavigation(true);
		}
		UpdateBackgroundTint(true);
	}

	private void UpdateBackgroundTint(bool showOwned)
	{
		foreach (UISprite item in backgroundsToTint)
		{
			item.color = ((!showOwned) ? Tuning.Instance.ColorTint_Store : Tuning.Instance.ColorTint_Owned);
		}
	}

	private void OnInventoryUpdated()
	{
		List<GameObject> itemList = tableHelper.GetItemList();
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].GetComponent<BoostUiItem>().Refresh();
		}
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
		EnableNavigation(enable);
		Util.FindComponent<UIDraggablePanel>(base.gameObject).enabled = enable;
	}
}
