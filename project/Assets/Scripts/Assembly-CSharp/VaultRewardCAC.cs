using EB;
using UnityEngine;

public class VaultRewardCAC : VaultReward
{
	private UITexture image;

	private bool textureLoaded;

	private UIWidget[] cachedWidgets;

	private void Awake()
	{
		image = Util.FindComponent<UITexture>(base.gameObject);
		cachedWidgets = Util.FindAllComponents<UIWidget>(base.gameObject);
		textureLoaded = false;
	}

	public void SetData(int index, CharacterDB.MetaData cacMetaData, int quantity)
	{
		string labelText = "INVALID_NAME";
		if (cacMetaData != null)
		{
			labelText = cacMetaData.friendlyName;
			TexturePoolManager.Instance.LoadTexture("cac_ui_parts/" + cacMetaData.cacTexturePath, OnTextureLoaded);
		}
		base.index = index;
		UIUtils.SetLabelContents(base.gameObject, "ItemName", labelText);
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
