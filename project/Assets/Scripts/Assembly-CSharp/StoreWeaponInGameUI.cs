using System;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class StoreWeaponInGameUI : MonoBehaviour
{
	private class DisplayedItem
	{
		public InventoryDB.MetaData md;

		public GameObject go;

		public WeaponUiItemInGame weaponUiItemInGame;

		public DisplayedItem(InventoryDB.MetaData md, GameObject go)
		{
			this.md = md;
			this.go = go;
			weaponUiItemInGame = go.GetComponent<WeaponUiItemInGame>();
		}
	}

	private bool initializedUiElements;

	public InventoryUiItemFactory.LevelRange[] levelRanges;

	public GameObject itemPrefab;

	public GameObject emptyItemPrefab;

	public GameObject inBetweenItemPrefab;

	public AudioEvent _itemSelectAudio;

	private UIGrid grid;

	private UIScrollBar scrollBar;

	private bool dragging;

	private GameObject scrollbar;

	private GameObject buttonLeft;

	private GameObject buttonRight;

	private bool isSP;

	private bool receivingInput;

	private float pageScrollSize;

	private bool isOpen;

	private UIPanel gridPanel;

	private UIDraggablePanel dragPanel;

	private StaticPanelController staticPanelController;

	private Dictionary<int, DisplayedItem> displayedItems = new Dictionary<int, DisplayedItem>();

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
		dragPanel = Util.GetObjectExactMatch(base.gameObject, "Grid").GetComponent<UIDraggablePanel>();
		scrollBar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar").GetComponent<UIScrollBar>();
		staticPanelController = Util.FindComponent<StaticPanelController>(gridPanel.gameObject);
		UIEventListener uIEventListener = UIEventListener.Get(scrollBar.background.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnScrollDrag));
		buttonLeft = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		buttonRight = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		scrollbar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar");
		GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		isSP = mode == GameAttributes.ModeType.SP && championshipMode == GameAttributes.ChampionshipType.kOff;
	}

	private void Exit()
	{
		GameHUD.Instance.ToggleSubscreen(HUDSubscreenManager.Subscreen.None);
	}

	public void EnableNavigation(bool enable)
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
			dragging = false;
		}
		if (!initializedUiElements)
		{
			UpdateItems();
			initializedUiElements = true;
		}
	}

	public void Open()
	{
		receivingInput = true;
		isOpen = true;
		UpdateItems();
		EB.Sequence.Component.Activate(base.gameObject, "StoreWeaponInGameUI", typeof(SequenceEvent_OnScreenOpened));
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		UIPanel[] array = componentsInChildren;
		foreach (UIPanel uIPanel in array)
		{
			uIPanel.enabled = true;
		}
		HUD component = Setup.Instance.GetComponent<HUD>();
		if (component != null)
		{
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
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		Collider[] array2 = componentsInChildren2;
		foreach (Collider collider in array2)
		{
			collider.enabled = true;
		}
		Player player = LevelData.Instance.GetPlayer();
		player.SelectingWeapon = true;
	}

	public void Close()
	{
		isOpen = false;
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		UIPanel[] array = componentsInChildren;
		foreach (UIPanel uIPanel in array)
		{
			uIPanel.enabled = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		Collider[] array2 = componentsInChildren2;
		foreach (Collider collider in array2)
		{
			collider.enabled = false;
		}
		Player player = LevelData.Instance.GetPlayer();
		if (player != null)
		{
			player.SelectingWeapon = false;
		}
		receivingInput = false;
		EB.Sequence.Component.Activate(base.gameObject, "StoreWeaponInGameUI", typeof(SequenceEvent_OnScreenClosed));
	}

	private void OnItemSelected(GameObject g)
	{
		if (!receivingInput)
		{
			return;
		}
		if (EBUI_TransitionManager.Instance.IsAnimating)
		{
			EB.Debug.LogWarning("Attempting to select a weapon while animating, early out!");
			return;
		}
		if (!initializedUiElements)
		{
			EB.Debug.LogWarning("Attempting to select a weapon before UI finished initializing");
			return;
		}
		Player player = LevelData.Instance.GetPlayer();
		WeaponUiItemInGame component = g.GetComponent<WeaponUiItemInGame>();
		if (component == null)
		{
			return;
		}
		string text = component.GetMetaData().name;
		int num = Arsenal.NumItems(InventoryItem.Category.Weapon);
		for (int i = 0; i < num; i++)
		{
			if (Arsenal.ItemName(InventoryItem.Category.Weapon, i) == text)
			{
				if (player.CanUseWeapon(i))
				{
					player.SelectWeapon(i);
					Exit();
				}
				else
				{
					BuyWeapon(component.GetMetaData());
				}
				break;
			}
		}
		AudioEmitter component2 = GetComponent<AudioEmitter>();
		if (component2 != null)
		{
			component2.Play(_itemSelectAudio);
		}
		receivingInput = false;
	}

	private void SelectWeapon(InventoryDB.MetaData md)
	{
		Player player = LevelData.Instance.GetPlayer();
		EB.Debug.Log("StoreWeaponInGameUI > SelectWeapon > " + md.name);
		if (!(player != null))
		{
			return;
		}
		int num = Arsenal.NumItems(InventoryItem.Category.Weapon);
		for (int i = 0; i < num; i++)
		{
			if (Arsenal.ItemName(InventoryItem.Category.Weapon, i) == md.name && player.CanUseWeapon(i))
			{
				player.SelectWeapon(i);
				break;
			}
		}
	}

	private void BuyWeapon(InventoryDB.MetaData metaData)
	{
		GameObject gameObject = ScreenManager.Instance.OpenPopup("StorePurchaseWeaponUI", true, true);
		StorePurchaseWeaponUI component = gameObject.GetComponent<StorePurchaseWeaponUI>();
		component.SetParameters(metaData.name, InventoryItem.Category.Weapon, true);
		component.SetParentScreen(base.gameObject);
	}

	private void OnPurchaseComplete(InventoryDB.MetaData metaData)
	{
		EB.Debug.Log("StoreWeaponInGameUI > OnPurchaseComplete > " + metaData.friendlyName);
		ScreenManager.Instance.ClosePopup("StorePurchaseWeaponUI");
		UpdateItems();
		SelectWeapon(metaData);
		Exit();
	}

	private void OnClosePurchaseWeaponScreen()
	{
		EB.Debug.Log("StoreWeaponInGameUI > OnClosePurchaseWeaponScreen");
		receivingInput = true;
	}

	private void OnInventoryUpdated()
	{
		EB.Debug.Log("INVENTORY UPDATE");
	}

	private bool ShouldShowItem(InventoryDB.MetaData md)
	{
		InventoryManager inventoryManager = Hub.Instance.InventoryManager;
		bool flag = false;
		bool flag2 = inventoryManager.GetCount(md.name) > 0;
		flag |= flag2;
		flag |= DebugSettings.UnlockAllWeapons;
		if (!isSP)
		{
			flag |= !md.IsLocked;
		}
		return flag & (!md.IsHidden || (flag2 && md.name != "Empty"));
	}

	public void MakeFaster()
	{
		OptimizeGridPanel(true);
	}

	public void OptimizeGridPanel(bool enable)
	{
		EB.Debug.Log("MAKE FASTER " + enable);
		if ((bool)gridPanel)
		{
			gridPanel.widgetsAreStatic = enable;
			EB.Debug.Log("MAKE FASTER2 name " + gridPanel.name + enable);
		}
	}

	private void UpdateItems()
	{
		OptimizeGridPanel(false);
		List<DisplayedItem> list = new List<DisplayedItem>();
		bool flag = false;
		foreach (KeyValuePair<int, DisplayedItem> displayedItem2 in displayedItems)
		{
			if (!ShouldShowItem(displayedItem2.Value.md))
			{
				list.Add(displayedItem2.Value);
				flag = true;
			}
		}
		foreach (DisplayedItem item in list)
		{
			item.go.transform.parent = null;
			UnityEngine.Object.Destroy(item.go);
			displayedItems.Remove(item.md.hash);
		}
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		bool flag2 = GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kTutorialOverride);
		int num = 0;
		int num2 = 0;
		foreach (InventoryDB.MetaData item2 in categoryList)
		{
			if (ShouldShowItem(item2))
			{
				num2++;
				if (!displayedItems.ContainsKey(item2.hash))
				{
					GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
					DisplayedItem displayedItem = new DisplayedItem(item2, gameObject);
					displayedItem.weaponUiItemInGame.staticPanelController = staticPanelController;
					displayedItems.Add(item2.hash, displayedItem);
					gameObject.name = itemPrefab.name + string.Format(" ([{0}.0] {1})", num, categoryList[num].friendlyName);
					displayedItem.weaponUiItemInGame.SetMetaData(categoryList[num]);
					gameObject.AddComponent<UIDragPanelContents>();
					UIEventListener uIEventListener = UIEventListener.Get(gameObject);
					uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnItemSelected));
					if (!isOpen)
					{
						gameObject.GetComponent<Collider>().enabled = false;
					}
					if (gameMode == GameAttributes.GameModeType.Tutorial || flag2)
					{
						UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
						uITutorialArrow.tutorialId = categoryList[num].name;
						uITutorialArrow.offset = new Vector3(grid.cellWidth / 2f, 120f, -50f);
						uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
						uITutorialArrow.tutorialId = "GenericTutorialWeapon";
						uITutorialArrow.offset = new Vector3(grid.cellWidth / 2f, 120f, -50f);
						TutorialHook tutorialHook = gameObject.AddComponent<TutorialHook>();
						tutorialHook._tutorialId = categoryList[num].name;
					}
					flag = true;
				}
				else
				{
					int count = Hub.Instance.InventoryManager.GetCount(item2.name);
					if (displayedItems[item2.hash].weaponUiItemInGame.GetItemCount() != count)
					{
						displayedItems[item2.hash].weaponUiItemInGame.SetItemCount(count);
					}
					displayedItems[item2.hash].weaponUiItemInGame.SetItemLevel(item2);
				}
			}
			num++;
		}
		if (flag)
		{
			grid.Reposition();
			scrollBar.scrollValue = 0f;
			scrollBar.ForceUpdate();
			EnableNavigation(num2 >= 5);
		}
	}

	public void OnPurchaseComplete()
	{
		UpdateItems();
		Profile.Instance.MarkTutorialDone("sequence_tutorial_weapons");
	}

	public void OnUnlockComplete()
	{
		UpdateItems();
	}

	public void OnScrollRight()
	{
		if (receivingInput)
		{
			Fuckwank(pageScrollSize);
		}
	}

	public void OnScrollLeft()
	{
		if (receivingInput)
		{
			Fuckwank(0f - pageScrollSize);
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
