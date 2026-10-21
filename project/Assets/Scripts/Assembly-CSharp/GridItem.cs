using EB;
using UnityEngine;

public class GridItem : MonoBehaviour
{
	public GameObject uiTextureContainer;

	public string texturePath = string.Empty;

	protected UITexture uiTexture;

	public bool hasTextureAssigned { get; protected set; }

	public static void EmptyTextureCache()
	{
		TexturePoolManager.Instance.ClearPool();
	}

	public virtual void Load()
	{
		if (!(uiTextureContainer == null) && !hasTextureAssigned && !string.IsNullOrEmpty(texturePath))
		{
			TexturePoolManager.Instance.LoadTexture(texturePath, this, OnTextureLoaded);
			hasTextureAssigned = true;
		}
	}

	private void Awake()
	{
		if (base.gameObject.GetComponent<TextureFade>() == null)
		{
			base.gameObject.AddComponent<TextureFade>();
		}
		hasTextureAssigned = false;
		if (base.gameObject.GetComponent<UIDragPanelContents>() == null)
		{
			base.gameObject.AddComponent<UIDragPanelContents>();
		}
		OnAwake();
	}

	protected virtual void OnAwake()
	{
		if (!(uiTextureContainer == null))
		{
			uiTexture = Util.FindComponent<UITexture>(uiTextureContainer);
			if (uiTexture == null)
			{
				EB.Debug.LogError("Missing UITexture component!", this);
			}
			uiTexture.alpha = 0f;
		}
	}

	protected virtual void OnTextureLoaded(Texture2D texture)
	{
		if (texture != null && uiTexture != null)
		{
			uiTexture.enabled = false;
			uiTexture.alpha = 0f;
			uiTexture.mainTexture = texture;
			GetComponent<TextureFade>().SetTexture(uiTexture);
			uiTexture.enabled = true;
		}
	}

	public virtual void Unload()
	{
		if (!(uiTextureContainer == null) && hasTextureAssigned)
		{
			uiTexture.alpha = 0f;
			uiTexture.mainTexture = null;
			uiTexture.enabled = false;
			TexturePoolManager.Instance.ReleaseTexture(texturePath);
			hasTextureAssigned = false;
		}
	}
}
