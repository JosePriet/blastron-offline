using System;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class StoreWeaponUI : MonoBehaviour
{
	private bool initializedUiElements;

	private Dictionary<string, int> levelRangeNameToIndex = new Dictionary<string, int>();

	public InventoryUiItemFactory.LevelRange[] levelRanges;

	public GameObject itemPrefab;

	public AudioEvent _itemSelectAudio;

	public UILabel TitleLabel;

	private TableHelperUI tableHelper;

	private UIGrid grid;

	private UIScrollBar scrollBar;

	private bool dragging;

	private List<InventoryDB.MetaData> currentMetaDataList = new List<InventoryDB.MetaData>();

	private int currentLevel;

	private GameObject scrollbar;

	private GameObject buttonLeft;

	private GameObject buttonRight;

	public bool startInStore;

	private List<UISprite> backgroundsToTint;

	private bool _enableInput = true;

	private bool _showingOwned;

	private void Start()
	{
		grid = Util.GetObjectExactMatch(base.gameObject, "Grid").GetComponent<UIGrid>();
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		tableHelper.SetMaxVisibleItems(12);
		tableHelper.SetDelta(3);
		scrollBar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar").GetComponent<UIScrollBar>();
		UIEventListener uIEventListener = UIEventListener.Get(scrollBar.background.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnScrollDrag));
		Util.GetObjectExactMatch(base.gameObject, "BackButton").GetComponent<ButtonScript>().onButtonClick += OnBack;
		Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab0").GetComponentInChildren<TabButtonScript>().onTabSelected += OnTabSelect_0;
		Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab1").GetComponentInChildren<TabButtonScript>().onTabSelected += OnTabSelect_1;
		Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab2").GetComponentInChildren<TabButtonScript>().onTabSelected += OnTabSelect_2;
		Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab3").GetComponentInChildren<TabButtonScript>().onTabSelected += OnTabSelect_3;
		Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab4").GetComponentInChildren<TabButtonScript>().onTabSelected += OnTabSelect_4;
		buttonLeft = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		buttonRight = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		scrollbar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar");
		TitleLabel.text = Localizer.GetString("ID_UI_STORE_WEAPONS");
		backgroundsToTint = new List<UISprite>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackgroundTinted");
		InitBackgroundTint(objectExactMatch);
		GameObject[] objects = Util.GetObjects(base.gameObject, "CheckboxCategoryTab");
		GameObject[] array = objects;
		foreach (GameObject obj in array)
		{
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(obj, "Unselected");
			objectExactMatch = Util.GetObjectExactMatch(objectExactMatch2, "Background");
			InitBackgroundTint(objectExactMatch);
		}
		EB.Sequence.Component.Activate(base.gameObject, "StoreWeaponUI", typeof(SequenceEvent_OnScreenOpened));
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

	public void EnableNavigiation(bool enable)
	{
		NGUITools.SetActive(buttonLeft, enable);
		NGUITools.SetActive(buttonRight, enable);
		NGUITools.SetActive(scrollbar, enable);
	}

	public void OnScrollDrag(GameObject caller, bool pressed)
	{
		if (pressed)
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
			Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab0").GetComponentInChildren<UICheckbox>().isChecked = !startInStore;
			Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab1").GetComponentInChildren<UICheckbox>().isChecked = startInStore;
			tableHelper.Reset();
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, null);
		}
	}

	private void DoBuyWeaponsTutorial()
	{
		if (!Profile.Instance.IsTutorialDone("sequence_tutorial_weapons"))
		{
			UnityEngine.Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_weapons");
			UnityEngine.Object.Instantiate(original);
		}
	}

	private void DoGoToAllTabTutorial()
	{
		if (!startInStore && !Profile.Instance.IsTutorialDone("sequence_tutorial_weapons") && !Profile.Instance.IsTutorialDone("sequence_tutorial_weapons_mystuffintro"))
		{
			UnityEngine.Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_weapons_mystuffintro");
			UnityEngine.Object.Instantiate(original);
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

	private void OnItemSelected(GameObject g)
	{
		if (!_enableInput)
		{
			return;
		}
		List<GameObject> itemList = tableHelper.GetItemList();
		for (int i = 0; i < itemList.Count; i++)
		{
			if (itemList[i] == g)
			{
				GameObject gameObject = ScreenManager.Instance.OpenPopup("StorePurchaseWeaponUI", true, true);
				gameObject.GetComponent<StorePurchaseWeaponUI>().SetParameters(currentMetaDataList[i], itemList[i].GetComponent<WeaponUiItemInGame>().image.mainTexture as Texture2D, !_showingOwned);
				gameObject.GetComponent<StorePurchaseWeaponUI>().SetParentScreen(base.gameObject);
				break;
			}
		}
		AudioEmitter component = GetComponent<AudioEmitter>();
		if (component != null)
		{
			UnityEngine.Debug.Log("Playing select item audio ");
			component.Play(_itemSelectAudio);
		}
	}

	public void OnTabSelect_0(GameObject g)
	{
		if (_enableInput)
		{
			DoGoToAllTabTutorial();
			UpdateItemsOwned();
		}
	}

	public void OnTabSelect_1(GameObject g)
	{
		if (_enableInput)
		{
			DoBuyWeaponsTutorial();
			UpdateItemsFromLevel(1);
		}
	}

	public void OnTabSelect_2(GameObject g)
	{
		if (_enableInput)
		{
			UpdateItemsFromLevel(2);
		}
	}

	public void OnTabSelect_3(GameObject g)
	{
		if (_enableInput)
		{
			UpdateItemsFromLevel(3);
		}
	}

	public void OnTabSelect_4(GameObject g)
	{
		if (_enableInput)
		{
			UpdateItemsFromLevel(0);
		}
	}

	private void UpdateItemsFromLevel(int level)
	{
		_showingOwned = false;
		currentLevel = level;
		tableHelper.Clear();
		currentMetaDataList.Clear();
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		int num = 0;
		GameObject itemToCenter = null;
		for (int i = 0; i < categoryList.Count; i++)
		{
			if (categoryList[i].levelRequired >= levelRanges[level].min && categoryList[i].levelRequired <= levelRanges[level].max && (!categoryList[i].IsFree || categoryList[i].isChampionshipTierReward) && !categoryList[i].IsHidden)
			{
				GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
				gameObject.transform.localScale = Vector3.one;
				gameObject.name = string.Format("{0}[{1}] {2}", itemPrefab, num, categoryList[i].friendlyName);
				gameObject.GetComponent<WeaponUiItemInGame>().SetMetaData(categoryList[i], false);
				gameObject.AddComponent<UIDragPanelContents>();
				UIEventListener uIEventListener = UIEventListener.Get(gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnItemSelected));
				tableHelper.AddItem(gameObject);
				currentMetaDataList.Add(categoryList[i]);
				num++;
				UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
				uITutorialArrow.tutorialId = categoryList[i].name;
				uITutorialArrow.offset = Vector3.up * 100f + Vector3.back * 50f;
				if (categoryList[i].IsLocked && categoryList[i].levelRequired == Profile.Instance.GetMaxItemUnlockLevel(InventoryItem.Category.Weapon))
				{
					itemToCenter = gameObject;
				}
			}
		}
		tableHelper.Reset();
		if (itemToCenter != null && Profile.Instance.IsTutorialDone("sequence_tutorial_weapons"))
		{
			Coroutines.NextFrame(SafeAction.Wrap(this, () =>
			{
				tableHelper.CenterOnItem(itemToCenter);
			}));
		}
		scrollBar.scrollValue = 0f;
		scrollBar.ForceUpdate();
		if (currentMetaDataList.Count < 7)
		{
			EnableNavigiation(false);
		}
		else
		{
			EnableNavigiation(true);
		}
		UpdateBackgroundTint(false);
	}

	private void UpdateBackgroundTint(bool showOwned)
	{
		foreach (UISprite item in backgroundsToTint)
		{
			item.color = ((!showOwned) ? Tuning.Instance.ColorTint_Store : Tuning.Instance.ColorTint_Owned);
		}
	}

	private void UpdateItemsOwned()
	{
		_showingOwned = true;
		tableHelper.Clear();
		currentMetaDataList.Clear();
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		int num = -1;
		int num2 = int.MaxValue;
		int num3 = 0;
		for (int i = 0; i < categoryList.Count; i++)
		{
			if (Hub.Instance.InventoryManager.GetCount(categoryList[i].name) <= 0)
			{
				continue;
			}
			GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
			gameObject.name = string.Format("{0}[{1}] {2}", itemPrefab, num3, categoryList[i].friendlyName);
			gameObject.GetComponent<WeaponUiItemInGame>().SetMetaData(categoryList[i], true, true);
			gameObject.AddComponent<UIDragPanelContents>();
			UIEventListener uIEventListener = UIEventListener.Get(gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnItemSelected));
			tableHelper.AddItem(gameObject);
			currentMetaDataList.Add(categoryList[i]);
			num3++;
			UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
			uITutorialArrow.tutorialId = categoryList[i].name;
			uITutorialArrow.offset = Vector3.up * 120f + Vector3.back * 50f;
			if (categoryList[i].IsLocked)
			{
				int levelRequired = categoryList[i].levelRequired;
				int num4 = Mathf.Abs(levelRequired - Profile.Instance.Level);
				if (num4 < num2)
				{
					num2 = num4;
					num = num3 - 1;
				}
			}
		}
		tableHelper.Reset();
		if (num != -1)
		{
			List<GameObject> itemList = tableHelper.GetItemList();
			if (itemList != null && itemList.Count > num)
			{
				tableHelper.CenterOnItem(itemList[num]);
			}
		}
		scrollBar.scrollValue = 0f;
		scrollBar.ForceUpdate();
		if (currentMetaDataList.Count < 7)
		{
			EnableNavigiation(false);
		}
		else
		{
			EnableNavigiation(true);
		}
		UpdateBackgroundTint(true);
	}

	public void OnPurchaseComplete()
	{
	}

	private void OnInventoryUpdated()
	{
		List<GameObject> itemList = tableHelper.GetItemList();
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].GetComponent<WeaponUiItemInGame>().Refresh();
		}
	}

	public void OnUnlockComplete()
	{
		OnInventoryUpdated();
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
		EnableNavigiation(enable);
		Util.FindComponent<UIDraggablePanel>(base.gameObject).enabled = enable;
	}

	private void OnFETutorialTabSelect_1()
	{
		DoBuyWeaponsTutorial();
		UpdateItemsFromLevel(0);
	}

	private void OnFETutorialCentreOnWeapon()
	{
		List<GameObject> itemList = tableHelper.GetItemList();
		for (int i = 0; i < itemList.Count; i++)
		{
			if (itemList[i].GetComponent<WeaponUiItemInGame>().GetMetaData().name == "Mo' Betta Rocket")
			{
				tableHelper.CenterOnItem(itemList[i]);
				break;
			}
		}
	}

	private void OnFETutorialSelectWeapon()
	{
		List<GameObject> itemList = tableHelper.GetItemList();
		for (int i = 0; i < itemList.Count; i++)
		{
			WeaponUiItemInGame component = itemList[i].GetComponent<WeaponUiItemInGame>();
			if (component.GetMetaData().name == "Mo' Betta Rocket")
			{
				GameObject gameObject = ScreenManager.Instance.OpenPopup("StorePurchaseWeaponUI", true, true);
				gameObject.GetComponent<StorePurchaseWeaponUI>().SetParameters(currentMetaDataList[i], component.image.mainTexture as Texture2D, true);
				gameObject.GetComponent<StorePurchaseWeaponUI>().SetParentScreen(base.gameObject);
				break;
			}
		}
	}
}
