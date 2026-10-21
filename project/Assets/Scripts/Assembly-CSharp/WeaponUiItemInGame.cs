using EB;
using EB.Sparx;
using UnityEngine;

public class WeaponUiItemInGame : MonoBehaviour
{
	private const int numberBeyondMeasure = 99999;

	public StaticPanelController staticPanelController;

	private InventoryDB.MetaData metaData;

	private UILabel title;

	private UILabel inventoryCount;

	private GameObject inventoryCountGroup;

	private GameObject lockGroup;

	private GameObject noAmmoGroup;

	private UILabel lockLevel;

	private UISlicedSprite inventoryCountIcon;

	private GameObject weaponLevelContainer;

	private GameObject weaponLevelProgress;

	private GameObject weaponLevelMax;

	private UILabel weaponLevel;

	private UISprite weaponLevelBar;

	private UILabel itemNameLevel;

	public UITexture image;

	private TextureFade fader;

	private bool isInGame;

	private bool isSP;

	private bool isTutorial;

	private int itemCount;

	private string weaponTexturePath;

	private float maxLevelBarWidth;

	private bool isMaxLevel;

	private bool inInventory;

	public Color LevelColor = Color.yellow;

	public Color MaxLevelColor = Color.red;

	public float MaxLevelPulseDuration = 0.5f;

	private UILabel activeTitle;

	private Color defaultInventoryCountIconColor;

	private void Awake()
	{
		UIPanel component = GetComponent<UIPanel>();
		Object.Destroy(component);
		title = base.transform.Find("ItemName").GetComponent<UILabel>();
		image = base.transform.Find("ItemTexture").GetComponent<UITexture>();
		inventoryCountGroup = base.transform.Find("ItemCount").gameObject;
		inventoryCount = inventoryCountGroup.transform.Find("ItemCountLabel").GetComponent<UILabel>();
		inventoryCountIcon = inventoryCountGroup.GetComponentInChildren<UISlicedSprite>();
		defaultInventoryCountIconColor = inventoryCountIcon.color;
		lockGroup = base.transform.Find("Lock").gameObject;
		lockLevel = lockGroup.transform.Find("LockLabel").GetComponent<UILabel>();
		noAmmoGroup = base.transform.Find("NoAmmo").gameObject;
		Transform transform = base.transform.Find("ItemLevel");
		if (transform != null)
		{
			weaponLevelContainer = transform.gameObject;
			if (weaponLevelContainer != null)
			{
				weaponLevelProgress = weaponLevelContainer.transform.Find("Progress").gameObject;
				weaponLevelMax = weaponLevelContainer.transform.Find("Max").gameObject;
				weaponLevelMax.SetActive(false);
				weaponLevel = weaponLevelProgress.transform.Find("LVL").GetComponent<UILabel>();
				weaponLevelBar = weaponLevelProgress.transform.Find("Bar").GetComponent<UISprite>();
				itemNameLevel = transform.Find("ItemName").GetComponent<UILabel>();
				maxLevelBarWidth = weaponLevelBar.transform.localScale.x;
			}
		}
		fader = GetComponent<TextureFade>();
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		isSP = mode == GameAttributes.ModeType.SP;
		isTutorial = GameAttributes.IsTutorialGameMode();
	}

	private void OnDestroy()
	{
		ReleaseWeaponTexture();
	}

	public void SetMetaData(InventoryDB.MetaData m, bool inGame = true, bool isInventory = false)
	{
		metaData = m;
		isInGame = inGame;
		inInventory = isInventory;
		Refresh();
	}

	public InventoryDB.MetaData GetMetaData()
	{
		return metaData;
	}

	public void SetItemCount(int count)
	{
		itemCount = count;
		if ((metaData.IsFree && !isSP) || itemCount > 99999)
		{
			inventoryCount.text = "∞".ToString();
			SetNormalAmmoCount();
			noAmmoGroup.SetActive(false);
		}
		else if (isSP && isTutorial)
		{
			inventoryCount.text = "∞".ToString();
			SetNormalAmmoCount();
			noAmmoGroup.SetActive(false);
		}
		else
		{
			inventoryCount.text = itemCount.ToString();
			if (itemCount == 0)
			{
				NGUITools.SetActive(inventoryCountGroup, !metaData.IsLocked);
				image.alpha = 0.3f;
				SetLowAmmoCount();
				noAmmoGroup.SetActive(true);
			}
			else if (itemCount == 1)
			{
				SetLowAmmoCount();
				noAmmoGroup.SetActive(false);
			}
			else
			{
				NGUITools.SetActive(inventoryCountGroup, true);
				image.alpha = 1f;
				SetNormalAmmoCount();
				noAmmoGroup.SetActive(false);
			}
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "SaleBundle");
		if (!isInGame)
		{
			NGUITools.SetActive(lockGroup, metaData.IsLocked);
			if (metaData.IsLocked)
			{
				noAmmoGroup.SetActive(false);
			}
			NGUITools.SetActive(Util.GetObject(lockGroup, "LVL"), metaData.IsLocked && !metaData.isChampionshipTierReward);
			NGUITools.SetActive(Util.GetObject(lockGroup, "LockLabel"), metaData.IsLocked && !metaData.isChampionshipTierReward);
		}
		else
		{
			NGUITools.SetActive(lockGroup, metaData.IsLocked && itemCount == 0);
			objectExactMatch.SetActive(false);
		}
		if (!isInGame || inInventory)
		{
			objectExactMatch.SetActive(metaData.onSale);
		}
	}

	public void SetItemLevel(InventoryDB.MetaData metadata)
	{
		if (weaponLevelContainer == null)
		{
			return;
		}
		if (!ItemLevelManager.Instance.CanItemLevel(metadata.hash))
		{
			weaponLevelContainer.SetActive(false);
			return;
		}
		if ((!GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kResetable) || !GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kDetonationVis) || !GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kInfiniteAmmo)) && Hub.Instance.InventoryManager.IsOverriden)
		{
			weaponLevelContainer.SetActive(false);
			return;
		}
		weaponLevelContainer.SetActive(true);
		int hash = metadata.hash;
		int level = ItemLevelManager.Instance.GetLevel(hash);
		int uses = ItemLevelManager.Instance.GetUses(hash);
		ItemLevelData.Level levelData = ItemLevelManager.Instance.GetLevelData(hash, level + 1);
		if (ItemLevelManager.Instance.IsMaxLevel(hash))
		{
			isMaxLevel = true;
			weaponLevel.text = Localizer.GetString("ID_UI_ITEM_LEVEL_MAX_LEVEL");
			Vector3 localScale = weaponLevelBar.transform.localScale;
			localScale.x = maxLevelBarWidth;
			weaponLevelBar.enabled = true;
			weaponLevelBar.transform.localScale = localScale;
			TweenColor tweenColor = weaponLevelBar.gameObject.GetComponent<TweenColor>();
			if (tweenColor == null)
			{
				tweenColor = weaponLevelBar.gameObject.AddComponent<TweenColor>();
			}
			tweenColor.from = LevelColor;
			tweenColor.to = MaxLevelColor;
			tweenColor.duration = MaxLevelPulseDuration;
			tweenColor.delay = Random.Range(0f, 1f);
			tweenColor.style = UITweener.Style.PingPong;
			return;
		}
		isMaxLevel = false;
		float num = Mathf.Clamp((float)uses / (float)levelData.uses, 0f, 1f);
		weaponLevel.text = Localizer.Format("ID_UI_ITEM_LEVEL_FORMAT", level + 1);
		Vector3 localScale2 = weaponLevelBar.transform.localScale;
		if (num == 0f)
		{
			weaponLevelBar.enabled = false;
		}
		else
		{
			weaponLevelBar.enabled = true;
			localScale2.x = maxLevelBarWidth * num;
			weaponLevelBar.transform.localScale = localScale2;
		}
		TweenColor component = weaponLevelBar.gameObject.GetComponent<TweenColor>();
		if (component != null)
		{
			weaponLevelBar.color = Color.yellow;
			component.enabled = false;
			Object.Destroy(component);
		}
	}

	public void SetLowAmmoCount()
	{
		if (!isInGame)
		{
			SetNormalAmmoCount();
			return;
		}
		inventoryCountIcon.color = new Color(1f, 0f, 0f, 1f);
		inventoryCount.color = new Color(1f, 1f, 1f, 1f);
	}

	public void SetNormalAmmoCount()
	{
		inventoryCountIcon.color = defaultInventoryCountIconColor;
		inventoryCount.color = new Color(0f, 0f, 0f, 1f);
	}

	public int GetItemCount()
	{
		return itemCount;
	}

	private void OnInventoryUpdated()
	{
		SetItemCount(Hub.Instance.InventoryManager.GetCount(metaData.name));
	}

	public void Refresh()
	{
		if (metaData != null)
		{
			lockLevel.text = metaData.levelRequired.ToString();
			SetItemCount(Hub.Instance.InventoryManager.GetCount(metaData.name));
			SetItemLevel(metaData);
		}
		SetTitle(metaData.friendlyName);
		ReleaseWeaponTexture();
		weaponTexturePath = metaData.uiTexturePath;
		TexturePoolManager.Instance.LoadTexture(weaponTexturePath, OnTextureLoaded);
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null && t.name == weaponTexturePath)
		{
			image.mainTexture = t;
			fader.SetTexture(image);
			if (staticPanelController != null)
			{
				staticPanelController.ToggleStaticWidgets();
			}
		}
	}

	private void ReleaseWeaponTexture()
	{
		if (!string.IsNullOrEmpty(weaponTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(weaponTexturePath);
			weaponTexturePath = null;
		}
	}

	public void SetTitle(string s)
	{
		if (weaponLevelContainer != null && weaponLevelContainer.activeSelf)
		{
			title.enabled = false;
			itemNameLevel.enabled = true;
			UIUtils.AssignTextAndResize(itemNameLevel, s, title.transform.localScale);
		}
		else
		{
			title.enabled = true;
			itemNameLevel.enabled = false;
			UIUtils.AssignTextAndResize(title, s, title.transform.localScale);
		}
	}
}
