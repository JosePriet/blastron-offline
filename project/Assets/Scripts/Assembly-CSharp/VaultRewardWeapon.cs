using EB;
using UnityEngine;

public class VaultRewardWeapon : VaultReward
{
	private InventoryDB.MetaData metaData;

	private UITexture image;

	private bool textureLoaded;

	private UIWidget[] cachedWidgets;

	private string texturePath;

	private void Awake()
	{
		image = Util.FindComponent<UITexture>(base.gameObject);
		cachedWidgets = Util.FindAllComponents<UIWidget>(base.gameObject);
		textureLoaded = false;
	}

	private void OnDestroy()
	{
		TexturePoolManager.Instance.ReleaseTexture(texturePath);
	}

	public void SetData(int index, InventoryDB.MetaData m, int quantity)
	{
		metaData = m;
		base.index = index;
		GameObject obj = Assets.Load(metaData.assetPrefabPath) as GameObject;
		InventoryItem inventoryItem = Util.FindComponent<InventoryItem>(obj);
		texturePath = inventoryItem.uiTexturePath;
		TexturePoolManager.Instance.LoadTexture(texturePath, OnTextureLoaded);
		UIUtils.SetLabelContents(base.gameObject, "ItemName", m.friendlyName);
		UIUtils.SetLabelContents(base.gameObject, "ItemCountLabel", quantity);
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null)
		{
			image.mainTexture = t;
			textureLoaded = true;
			image.color = Color.white;
		}
	}

	public override void PrepareForAnimation()
	{
		UIWidget[] array = cachedWidgets;
		foreach (UIWidget uIWidget in array)
		{
			uIWidget.MarkAsChanged();
		}
	}

	public override bool IsReady()
	{
		return textureLoaded;
	}
}
