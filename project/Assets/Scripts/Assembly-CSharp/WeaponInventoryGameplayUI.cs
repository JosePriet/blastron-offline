using EB;
using UnityEngine;

public class WeaponInventoryGameplayUI : MonoBehaviour
{
	public GameObject uiPrefab;

	private GameObject uiInstance;

	private bool needsRecenter;

	private UIGrid grid;

	public void OnCloseInventory(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, () =>
		{
			ScreenManager.Instance.ClosePopup();
		});
	}

	private void Awake()
	{
		uiInstance = Object.Instantiate(uiPrefab) as GameObject;
		Transform transform = uiInstance.transform;
		transform.parent = base.transform;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		grid = base.gameObject.GetComponentInChildren<UIGrid>();
	}

	private void OnOpenPopup()
	{
		GameObject gameObject = base.gameObject.transform.parent.gameObject;
		gameObject.BroadcastMessage("OnHudElementOpened", base.gameObject, SendMessageOptions.RequireReceiver);
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnReady));
	}

	public void OnReady()
	{
	}

	public void OnClosePopup()
	{
		StoreLastView();
		CleanUpTextures();
	}

	private void CleanUpTextures()
	{
		GridItem[] array = Util.FindAllComponents<GridItem>(base.gameObject);
		GridItem[] array2 = array;
		foreach (GridItem gridItem in array2)
		{
			if (gridItem.uiTextureContainer.GetComponent<UITexture>().mainTexture != null)
			{
				gridItem.uiTextureContainer.GetComponent<UITexture>().mainTexture = null;
				gridItem.Unload();
			}
		}
	}

	private void OnItemCreationComplete(GameObject caller)
	{
		UICallback[] componentsInChildren = uiInstance.GetComponentsInChildren<UICallback>();
		UICallback[] array = componentsInChildren;
		foreach (UICallback uICallback in array)
		{
			uICallback.Target = this;
		}
		grid.Reposition();
		needsRecenter = true;
	}

	private void Update()
	{
		if (needsRecenter && !grid.repositionNow)
		{
			needsRecenter = false;
			CenterOnView(GameHUD.Instance.lastInventoryWeaponView);
		}
	}

	private void CenterOnView(float lastPosition)
	{
		if (lastPosition != 0f)
		{
			UIDraggablePanel uIDraggablePanel = Util.FindComponent<UIDraggablePanel>(base.gameObject);
			uIDraggablePanel.SetDragAmount(lastPosition, 0f, false);
		}
	}

	private void OnItemSelected(InventoryDB.MetaData metaData)
	{
		Player player = LevelData.Instance.GetPlayer();
		int num = Arsenal.NumItems(InventoryItem.Category.Weapon);
		for (int i = 0; i < num; i++)
		{
			if (Arsenal.ItemName(InventoryItem.Category.Weapon, i) == metaData.name)
			{
				if (player.CanUseWeapon(i))
				{
					player.SelectWeapon(i);
					OnCloseInventory(null);
				}
				else
				{
					BuyWeapon(metaData);
				}
				break;
			}
		}
	}

	private void OnUnlockComplete()
	{
	}

	private void BuyWeapon(InventoryDB.MetaData metaData)
	{
		GameObject gameObject = ScreenManager.Instance.OpenPopup("StorePurchaseWeaponUI", true, true);
		StorePurchaseWeaponUI component = gameObject.GetComponent<StorePurchaseWeaponUI>();
		InventoryUiItem[] array = Util.FindAllComponents<InventoryUiItem>(base.gameObject);
		Texture2D itemTexture = null;
		InventoryUiItem[] array2 = array;
		foreach (InventoryUiItem inventoryUiItem in array2)
		{
			if (inventoryUiItem.metaData != null && inventoryUiItem.metaData.name == metaData.name)
			{
				GridItem gridItem = Util.FindComponent<GridItem>(inventoryUiItem.gameObject);
				UITexture uITexture = Util.FindComponent<UITexture>(gridItem.uiTextureContainer);
				itemTexture = uITexture.mainTexture as Texture2D;
			}
		}
		component.SetParameters(metaData, itemTexture, true);
		component.SetParentScreen(base.gameObject);
	}

	private void OnPurchaseComplete(InventoryDB.MetaData metaData)
	{
		ScreenManager.Instance.ClosePopup("StorePurchaseWeaponUI");
		OnItemSelected(metaData);
	}

	private void StoreLastView()
	{
		UIDraggablePanel uIDraggablePanel = Util.FindComponent<UIDraggablePanel>(base.gameObject);
		if (GameHUD.Instance != null && uIDraggablePanel != null)
		{
			GameHUD.Instance.lastInventoryWeaponView = uIDraggablePanel.horizontalScrollBar.scrollValue;
		}
	}
}
