using EB;
using EB.Sparx;
using UnityEngine;

public class BoostUiItem : MonoBehaviour
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

	private string texturePath;

	public UITexture image;

	private TextureFade fader;

	private UISlicedSprite inventoryCountSprite;

	private Color inventoryCountNormalColor;

	private void Awake()
	{
		title = Util.GetObjectExactMatch(base.gameObject, "ItemName").GetComponent<UILabel>();
		buyButton = Util.GetObjectExactMatch(base.gameObject, "ButtonBuy");
		unlockButton = Util.GetObjectExactMatch(base.gameObject, "ButtonUnlock");
		image = Util.GetObjectExactMatch(base.gameObject, "ItemTexture").GetComponent<UITexture>();
		inventoryCountGroup = Util.GetObjectExactMatch(base.gameObject, "ItemCount");
		inventoryCount = Util.GetObjectExactMatch(base.gameObject, "ItemCountLabel").GetComponent<UILabel>();
		inventoryCountSprite = inventoryCountGroup.GetComponentInChildren<UISlicedSprite>();
		inventoryCountNormalColor = inventoryCountSprite.color;
		lockGroup = Util.GetObjectExactMatch(base.gameObject, "Lock");
		lockLevel = Util.GetObjectExactMatch(lockGroup, "LockLabel").GetComponent<UILabel>();
		lockFader = Util.GetObjectExactMatch(lockGroup, "fader").GetComponent<UISprite>();
		fader = GetComponent<TextureFade>();
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
		if (unlockButton != null)
		{
			NGUITools.SetActive(unlockButton, metaData.IsLocked);
		}
		Game game = Hub.Instance.GameManager.Game;
		int count = Hub.Instance.InventoryManager.GetCount(metaData.name);
		NGUITools.SetActive(lockGroup, metaData.IsLocked);
		NGUITools.SetActive(inventoryCountGroup, !metaData.IsLocked);
		BoostItem boostItem = Util.FindComponent<BoostItem>(inventoryItemPrefab);
		texturePath = boostItem.uiTexturePath;
		if (boostItem != null && metaData != null)
		{
			lockLevel.text = metaData.levelRequired.ToString();
			inventoryCount.text = count.ToString();
			if (buyButton != null)
			{
				NGUITools.SetActive(buyButton, !metaData.IsLocked && count == 0);
			}
			if (lockGroup != null && game == null)
			{
				NGUITools.SetActive(lockGroup, metaData.IsLocked);
			}
			else if (game != null)
			{
				NGUITools.SetActive(lockGroup, false);
			}
			if (count == 0)
			{
				NGUITools.SetActive(inventoryCountGroup, !metaData.IsLocked);
				if (image != null)
				{
					image.alpha = 0.3f;
				}
			}
			else
			{
				NGUITools.SetActive(inventoryCountGroup, true);
				if (image != null)
				{
					image.alpha = 1f;
				}
				inventoryCountSprite.color = ((count != 1) ? inventoryCountNormalColor : Color.red);
				inventoryCount.color = ((count != 1) ? Color.black : Color.white);
			}
			if (metaData.IsLocked)
			{
				lockFader.alpha = 0.65f;
			}
		}
		TexturePoolManager.Instance.LoadTexture(texturePath, OnTextureLoaded);
	}

	private void OnTextureLoaded(Texture2D text)
	{
		if (text != null && image != null)
		{
			image.mainTexture = text;
		}
	}

	public void SetTitle(string s)
	{
		title.text = s;
	}
}
