using EB;
using UnityEngine;

public class LevelUpUnlockUiItem : MonoBehaviour
{
	private string uiTexturePath;

	private UITexture image;

	private void Awake()
	{
		image = GetComponentInChildren<UITexture>();
		UIPanel component = GetComponent<UIPanel>();
		if (component != null)
		{
			Object.Destroy(component);
		}
	}

	public void SetTexturePath(string texturePath)
	{
		uiTexturePath = texturePath;
		Refresh();
	}

	public void Refresh()
	{
		TexturePoolManager.Instance.LoadTexture(uiTexturePath, OnTextureLoaded);
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null)
		{
			image.mainTexture = t;
			image.MakePixelPerfect();
			TextureFade component = GetComponent<TextureFade>();
			component.SetTexture(image);
		}
	}

	public void SetItemName(string s)
	{
		UIUtils.SetLabelContents(base.gameObject, "LabelUnlocked", s);
	}
}
