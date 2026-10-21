using EB;
using EB.Sparx;
using UnityEngine;

public class WeaponUiItem : MonoBehaviour
{
	private InventoryDB.MetaData metaData;

	private UILabel title;

	private UILabel inventoryCount;

	private UILabel lockLevel;

	private UISprite lockFader;

	private GameObject buyButton;

	private GameObject unlockButton;

	private GameObject lockGroup;

	private GameObject inventoryCountGroup;

	private GameObject inventoryItemPrefab;

	public UITexture image;

	private TextureFade fader;

	private string texturePath;

	private bool isSP;

	private void Awake()
	{
		title = Util.GetObjectExactMatch(base.gameObject, "ItemName").GetComponent<UILabel>();
		buyButton = Util.GetObjectExactMatch(base.gameObject, "ButtonBuy");
		unlockButton = Util.GetObjectExactMatch(base.gameObject, "ButtonUnlock");
		image = Util.GetObjectExactMatch(base.gameObject, "ItemTexture").GetComponent<UITexture>();
		inventoryCountGroup = Util.GetObjectExactMatch(base.gameObject, "ItemCount");
		inventoryCount = Util.GetObjectExactMatch(base.gameObject, "ItemCountLabel").GetComponent<UILabel>();
		lockGroup = Util.GetObjectExactMatch(base.gameObject, "Lock");
		lockLevel = Util.GetObjectExactMatch(lockGroup, "LockLabel").GetComponent<UILabel>();
		lockFader = Util.GetObjectExactMatch(lockGroup, "fader").GetComponent<UISprite>();
		fader = GetComponent<TextureFade>();
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		isSP = mode == GameAttributes.ModeType.SP;
		UIPanel component = GetComponent<UIPanel>();
		if (component != null)
		{
			Object.Destroy(component);
		}
	}

	public void SetMetaData(InventoryDB.MetaData m)
	{
		metaData = m;
		inventoryItemPrefab = Assets.Load(metaData.assetPrefabPath) as GameObject;
		Refresh();
	}

	public InventoryDB.MetaData GetMetaData()
	{
		return metaData;
	}

	public void Refresh()
	{
		SetTitle(metaData.friendlyName);
		ReleaseWeaponTexture();
		if (metaData.IsFree)
		{
			NGUITools.SetActive(buyButton, false);
		}
		else
		{
			NGUITools.SetActive(buyButton, !metaData.IsLocked);
		}
		NGUITools.SetActive(unlockButton, metaData.IsLocked);
		InventoryItem inventoryItem = Util.FindComponent<InventoryItem>(inventoryItemPrefab);
		if (!(inventoryItem != null))
		{
			return;
		}
		if (metaData != null)
		{
			lockLevel.text = metaData.levelRequired.ToString();
			int count = Hub.Instance.InventoryManager.GetCount(metaData.name);
			if (metaData.IsFree && !isSP)
			{
				inventoryCount.text = "∞".ToString();
			}
			else
			{
				inventoryCount.text = "x" + count;
			}
			bool isLocked = metaData.IsLocked;
			NGUITools.SetActive(lockGroup, isLocked);
			NGUITools.SetActive(Util.GetObject(lockGroup, "Level"), isLocked && !metaData.isChampionshipTierReward);
			if (isLocked)
			{
				unlockButton.GetComponentInChildren<UILabel>().text = ((!metaData.isChampionshipTierReward) ? "ID_UI_STORE_UNLOCK" : "ID_UI_HOME_CAMPAIGN");
			}
			if (count == 0 || ScreenManager.Instance.GetPopup("ResultsUI") != null)
			{
				NGUITools.SetActive(inventoryCountGroup, !isLocked);
				image.alpha = 0.3f;
			}
			else
			{
				NGUITools.SetActive(inventoryCountGroup, true);
				image.alpha = 1f;
			}
			if (isLocked)
			{
				lockFader.alpha = 0.65f;
			}
		}
		LoadWeaponTexture(inventoryItem.uiTexturePath);
	}

	private void ReleaseWeaponTexture()
	{
		if (!string.IsNullOrEmpty(texturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(texturePath);
			texturePath = null;
		}
	}

	private void LoadWeaponTexture(string weaponTexturePath)
	{
		ReleaseWeaponTexture();
		texturePath = weaponTexturePath;
		TexturePoolManager.Instance.LoadTexture(texturePath, OnTextureLoaded);
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null && texturePath == t.name)
		{
			image.mainTexture = t;
			fader.SetTexture(image);
		}
	}

	public void SetTitle(string s)
	{
		title.text = s;
	}

	private void OnDestroy()
	{
		ReleaseWeaponTexture();
	}
}
