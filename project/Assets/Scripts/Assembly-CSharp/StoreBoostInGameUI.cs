using System;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class StoreBoostInGameUI : MonoBehaviour
{
	private bool initializedUiElements;

	private Dictionary<string, int> levelRangeNameToIndex = new Dictionary<string, int>();

	public GameObject itemPrefab;

	private TableHelperUI tableHelper;

	private UIGrid grid;

	private UIScrollBar scrollBar;

	private bool dragging;

	private List<InventoryDB.MetaData> currentMetaDataList = new List<InventoryDB.MetaData>();

	private int currentLevel;

	private bool isSP;

	private bool isChampionship;

	private bool receivingInput;

	private float pageScrollSize;

	private GameObject scrollbar;

	private GameObject buttonLeft;

	private GameObject buttonRight;

	private HUDSubscreenManager manager;

	private UIPanel gridPanel;

	public void Open()
	{
		receivingInput = true;
		UpdateItems();
		manager = GameObject.Find("GameHUDTablet(Clone)").GetComponent<HUDSubscreenManager>();
		EB.Sequence.Component.Activate(base.gameObject, "StoreWeaponInGameUI", typeof(SequenceEvent_OnScreenOpened));
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		UIPanel[] array = componentsInChildren;
		foreach (UIPanel uIPanel in array)
		{
			uIPanel.enabled = true;
		}
		HUD component = Setup.Instance.GetComponent<HUD>();
		if (!(component != null))
		{
			return;
		}
		GameObject hudObject = component.HudObject;
		if (hudObject != null)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(hudObject, "ButtonAim");
			if (objectExactMatch != null)
			{
				objectExactMatch.SendMessage("ResetAim");
			}
		}
	}

	public void Close()
	{
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		UIPanel[] array = componentsInChildren;
		foreach (UIPanel uIPanel in array)
		{
			uIPanel.enabled = false;
		}
		receivingInput = false;
	}

	public void SetPanelExtentsX(float x)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BlockerHolderLeft");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "BlockerHolderRight");
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		GameObject gameObject = grid.gameObject;
		UIWidget uIWidget = Util.FindComponent<UIWidget>(objectExactMatch3);
		float x2 = uIWidget.transform.localScale.x;
		float num = Mathf.Round(x2 / 2f);
		SetXPos(gameObject.transform, x2 - x);
		SetXPos(objectExactMatch.transform, 0f - x + x2);
		SetXPos(objectExactMatch2.transform, x - x2);
		SetXPos(objectExactMatch3.transform, 0f - x + num);
		SetXPos(objectExactMatch4.transform, x - num);
		UIPanel uIPanel = Util.FindComponent<UIPanel>(gameObject);
		Vector4 clipRange = uIPanel.clipRange;
		clipRange.z = (x - x2) * 2f;
		clipRange.x = clipRange.z / 2f;
		uIPanel.clipRange = clipRange;
		pageScrollSize = clipRange.z;
	}

	private void SetXPos(Transform t, float x)
	{
		Vector3 localPosition = t.localPosition;
		localPosition.x = x;
		t.localPosition = localPosition;
	}

	private void Awake()
	{
		Close();
	}

	private void Start()
	{
		grid = Util.GetObjectExactMatch(base.gameObject, "Grid").GetComponent<UIGrid>();
		gridPanel = Util.GetObjectExactMatch(base.gameObject, "Grid").GetComponent<UIPanel>();
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		tableHelper.SetMaxVisibleItems(5);
		tableHelper.SetDelta(2);
		scrollBar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar").GetComponent<UIScrollBar>();
		UIEventListener uIEventListener = UIEventListener.Get(scrollBar.background.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnScrollDrag));
		buttonLeft = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		UIEventListener uIEventListener2 = UIEventListener.Get(buttonLeft);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnScrollLeft));
		buttonRight = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		UIEventListener uIEventListener3 = UIEventListener.Get(buttonRight);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnScrollRight));
		scrollbar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar");
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		isSP = mode == GameAttributes.ModeType.SP;
		isChampionship = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn;
	}

	public void MakeFaster()
	{
		OptimizeGridPanel(true);
	}

	public void OptimizeGridPanel(bool enable)
	{
		if ((bool)gridPanel)
		{
			gridPanel.widgetsAreStatic = enable;
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
			UpdateItems();
			tableHelper.Reset();
		}
	}

	private void OnItemSelected(GameObject g)
	{
		if (!receivingInput || manager._animating)
		{
			return;
		}
		List<GameObject> itemList = tableHelper.GetItemList();
		int num = -1;
		for (int i = 0; i < itemList.Count; i++)
		{
			if (itemList[i] == g)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			int count = Hub.Instance.InventoryManager.GetCount(currentMetaDataList[num].name);
			if (count <= 0)
			{
				BuyBoost(currentMetaDataList[num]);
			}
			else
			{
				ActivateBoost(currentMetaDataList[num]);
			}
		}
	}

	private void ActivateBoost(InventoryDB.MetaData metaData)
	{
		if (!receivingInput)
		{
			return;
		}
		Player player = LevelData.Instance.GetPlayer();
		string errorReason = string.Empty;
		if (player.CanUseBoost(metaData.name, ref errorReason))
		{
			int freeBoostSlot = player.GetFreeBoostSlot();
			if (freeBoostSlot >= 0)
			{
				player.SetBoost(freeBoostSlot, metaData.name);
				EB.Debug.Log("> Attempting to use item: '" + metaData.name + "'");
				InventoryListener.Instance.UseInGame(metaData.name, 1);
				ScreenManager.Instance.BroadcastMessage("UpdateBoosts");
				UpdateItems();
			}
		}
		else
		{
			receivingInput = false;
			GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
			ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
			confirmationDialogUI.SetParameters(true, errorReason, Localizer.GetString("ID_UI_BOOSTS_FAIL_TITLE"), "ID_UI_COMMON_OK", string.Empty);
			confirmationDialogUI.SetParentScreen(base.gameObject);
		}
	}

	public void OnConfirmationDialogUIClosed(ConfirmationDialogUI.ButtonIds b)
	{
		receivingInput = true;
		EB.Debug.Log("ON CONFIRMATION DIALOG!");
		ScreenManager.Instance.EnableBgFade(false);
	}

	private void BuyBoost(InventoryDB.MetaData metadata)
	{
		GameObject gameObject = ScreenManager.Instance.OpenPopup("StorePurchaseWeaponUI", true, true);
		StorePurchaseWeaponUI component = gameObject.GetComponent<StorePurchaseWeaponUI>();
		component.SetParameters(metadata.name, InventoryItem.Category.Boost, false);
		component.SetParentScreen(base.gameObject);
	}

	private void OnPurchaseComplete(InventoryDB.MetaData metaData)
	{
		EB.Debug.Log("PURCHASE COMPLETE");
		ScreenManager.Instance.ClosePopup("StorePurchaseWeaponUI");
		string empty = string.Empty;
		ActivateBoost(metaData);
		Profile.Instance.MarkTutorialDone("sequence_tutorial_boosts");
		receivingInput = true;
	}

	public void UpdateItems()
	{
		OptimizeGridPanel(false);
		EB.Debug.Log("UPDATING ITEMS");
		tableHelper.Clear();
		currentMetaDataList.Clear();
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Boost);
		for (int i = 0; i < categoryList.Count; i++)
		{
			if (!ShouldShowItem(categoryList[i]))
			{
				continue;
			}
			GameObject obj = Assets.Load(categoryList[i].assetPrefabPath) as GameObject;
			BoostItem boostItem = Util.FindComponent<BoostItem>(obj);
			if (boostItem == null)
			{
				continue;
			}
			GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
			gameObject.GetComponent<BoostUiItem>().SetMetaData(categoryList[i]);
			gameObject.AddComponent<UIDragPanelContents>();
			UIEventListener uIEventListener = UIEventListener.Get(gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnItemSelected));
			tableHelper.AddItem(gameObject);
			currentMetaDataList.Add(categoryList[i]);
			bool active = false;
			BoostItem.Type[] array = LevelData.Instance.GetPlayer().inactiveBoostTypes();
			foreach (BoostItem.Type type in array)
			{
				if (type == boostItem._type)
				{
					active = true;
					break;
				}
			}
			Util.GetObjectExactMatch(gameObject, "NoAmmo").SetActive(active);
		}
		tableHelper.Reset();
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
	}

	private bool ShouldShowItem(InventoryDB.MetaData md)
	{
		InventoryManager inventoryManager = Hub.Instance.InventoryManager;
		bool flag = false;
		flag |= DebugSettings.UnlockAllWeapons;
		if (!isSP || isChampionship)
		{
			flag |= md.levelRequired <= Profile.Instance.Level;
			flag |= UnlocksManager.Instance.IsUnlocked(md.name);
		}
		flag &= !md.IsHidden;
		return flag & (inventoryManager.GetCount(md.name) > 0);
	}

	private void OnScrollLeft(GameObject caller)
	{
		if (receivingInput)
		{
			Fuckwank(0f - pageScrollSize);
		}
	}

	private void OnScrollRight(GameObject caller)
	{
		if (receivingInput)
		{
			Fuckwank(pageScrollSize);
		}
	}

	public void OnPurchaseComplete()
	{
		receivingInput = true;
	}

	public void OnUnlockComplete()
	{
		receivingInput = true;
		OnInventoryUpdated();
	}

	public void OnConfirmationDialogUIClosed()
	{
		receivingInput = true;
	}

	private void OnInventoryUpdated()
	{
		List<GameObject> itemList = tableHelper.GetItemList();
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].GetComponent<BoostUiItem>().Refresh();
		}
	}

	public void Fuckwank(float deltaX)
	{
		UIDraggablePanel component = grid.GetComponent<UIDraggablePanel>();
		Vector4 clipRange = component.panel.clipRange;
		Transform cachedTransform = component.panel.cachedTransform;
		Vector3 localPosition = cachedTransform.localPosition;
		localPosition.x += clipRange.x;
		localPosition.y += clipRange.y;
		localPosition = cachedTransform.parent.TransformPoint(localPosition);
		component.currentMomentum = Vector3.zero;
		Vector3 vector = Vector3.right * deltaX;
		if (component.scale.x == 0f)
		{
			vector.x = 0f;
		}
		if (component.scale.y == 0f)
		{
			vector.y = 0f;
		}
		if (component.scale.z == 0f)
		{
			vector.z = 0f;
		}
		Bounds bounds = component.bounds;
		float num = clipRange.z * 0.5f;
		Vector3 pos = cachedTransform.localPosition - vector;
		float num2 = 0f - pos.x;
		if (num2 - num < bounds.min.x)
		{
			num2 = bounds.min.x + num;
			pos.x = 0f - num2;
		}
		else if (num2 + num > bounds.max.x)
		{
			num2 = bounds.max.x - num;
			pos.x = 0f - num2;
		}
		SpringPanel.Begin(component.gameObject, pos, 8f);
	}
}
