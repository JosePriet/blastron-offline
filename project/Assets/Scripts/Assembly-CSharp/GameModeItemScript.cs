using EB;
using UnityEngine;

public class GameModeItemScript : MonoBehaviour
{
	public delegate void cb();

	private UILabel label;

	private UITexture icon;

	private float offset;

	private float offsetSpeed = 0.1f;

	private cb callback;

	public Color selectedColor = Color.red;

	private Color unselectedColor = Color.white;

	private string iconTexturePath;

	private UISprite bgSprite;

	private UISprite circuitSprite;

	private void Awake()
	{
		label = Util.GetObjectExactMatch(base.gameObject, "LabelTitle").GetComponent<UILabel>();
		icon = Util.GetObjectExactMatch(base.gameObject, "Icon").GetComponent<UITexture>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Background");
		if (objectExactMatch != null)
		{
			UISprite component = objectExactMatch.GetComponent<UISprite>();
			if (component != null)
			{
				bgSprite = component;
				unselectedColor = component.color;
			}
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Circuit");
		if (objectExactMatch2 != null)
		{
			circuitSprite = objectExactMatch2.GetComponent<UISprite>();
		}
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		ReleaseIconTexture();
	}

	public void SetIcon(string gameModeIcon, cb c)
	{
		ReleaseIconTexture();
		callback = c;
		iconTexturePath = "gamemodes_ui/" + gameModeIcon;
		TexturePoolManager.Instance.LoadTexture(iconTexturePath, this, OnTextureLoaded);
	}

	public void SetText(string text)
	{
		if (label != null)
		{
			label.text = text;
		}
	}

	private void OnTextureLoaded(Texture2D text)
	{
		icon.mainTexture = text;
		icon.MakePixelPerfect();
		if (callback != null)
		{
			callback();
		}
	}

	private void ReleaseIconTexture()
	{
		if (!string.IsNullOrEmpty(iconTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(iconTexturePath);
			iconTexturePath = null;
		}
	}

	private void OnPress(bool isPressed)
	{
		if (bgSprite != null)
		{
			float alpha = bgSprite.alpha;
			bgSprite.color = ((!isPressed) ? unselectedColor : selectedColor);
			bgSprite.alpha = alpha;
		}
		if (circuitSprite != null)
		{
			float alpha2 = circuitSprite.alpha;
			circuitSprite.color = ((!isPressed) ? unselectedColor : selectedColor);
			circuitSprite.alpha = alpha2;
		}
	}
}
