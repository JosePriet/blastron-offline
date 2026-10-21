using EB;
using UnityEngine;

public class CampaignLoadoutItem : MonoBehaviour
{
	private UITexture uiTexture;

	private GameObject countObj;

	private string texturePath = string.Empty;

	private int _count = 1;

	public int Count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = value;
			if (countObj != null)
			{
				UIUtils.SetLabelContents(countObj, "Label", _count.ToString());
			}
		}
	}

	public bool hasTextureAssigned
	{
		get
		{
			return !string.IsNullOrEmpty(texturePath);
		}
	}

	private void Awake()
	{
		if (base.gameObject.GetComponent<TextureFade>() == null)
		{
			base.gameObject.AddComponent<TextureFade>();
		}
		uiTexture = base.gameObject.GetComponentInChildren<UITexture>();
		uiTexture.alpha = 0f;
		countObj = Util.GetObjectExactMatch(base.gameObject, "Count");
	}

	public void LoadTexture(string texturePath)
	{
		ReleaseTexture();
		this.texturePath = texturePath;
		TexturePoolManager.Instance.LoadTexture(texturePath, this, OnTextureLoaded);
	}

	private void OnTextureLoaded(Texture2D texture)
	{
		if (texture != null && texturePath == texture.name)
		{
			uiTexture.mainTexture = texture;
			uiTexture.enabled = true;
			GetComponent<TextureFade>().SetTexture(uiTexture);
		}
	}

	public virtual void Unload()
	{
		if (hasTextureAssigned)
		{
			uiTexture.alpha = 0f;
			uiTexture.mainTexture = null;
			uiTexture.enabled = false;
			ReleaseTexture();
		}
	}

	public void HideCount()
	{
		countObj.SetActive(false);
	}

	private void OnDestroy()
	{
		ReleaseTexture();
	}

	private void ReleaseTexture()
	{
		if (hasTextureAssigned)
		{
			TexturePoolManager.Instance.ReleaseTexture(texturePath);
			texturePath = string.Empty;
		}
	}
}
