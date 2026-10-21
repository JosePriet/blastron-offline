using EB;
using UnityEngine;

public class CampaignEnemyItem : MonoBehaviour
{
	private UITexture uiTexture;

	private GameObject backgroundOn;

	private GameObject backgroundOff;

	public string texturePath = string.Empty;

	public bool hasTextureAssigned { get; set; }

	public static void EmptyTextureCache()
	{
		TexturePoolManager.Instance.ClearPool();
	}

	private void Awake()
	{
		if (base.gameObject.GetComponent<TextureFade>() == null)
		{
			base.gameObject.AddComponent<TextureFade>();
		}
		hasTextureAssigned = false;
		uiTexture = base.gameObject.GetComponentInChildren<UITexture>();
		uiTexture.alpha = 0f;
		backgroundOn = Util.GetObjectExactMatch(base.gameObject, "background_on");
		backgroundOff = Util.GetObjectExactMatch(base.gameObject, "background_off");
		backgroundOn.SetActive(false);
	}

	public void Load()
	{
		if (!hasTextureAssigned)
		{
			TexturePoolManager.Instance.LoadTexture(texturePath, this, OnTextureLoaded);
			hasTextureAssigned = true;
			backgroundOn.SetActive(true);
			backgroundOff.SetActive(false);
		}
	}

	private void OnTextureLoaded(Texture2D texture)
	{
		if (texture != null)
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
			TexturePoolManager.Instance.ReleaseTexture(texturePath);
			hasTextureAssigned = false;
		}
	}

	private void OnDestroy()
	{
		TexturePoolManager.Instance.ReleaseTexture(texturePath);
	}
}
