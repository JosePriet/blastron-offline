using System;
using System.Collections;
using EB;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
	public delegate void buttonEventHandler(GameObject go);

	private GameObject background;

	private GameObject outline;

	private UILabel label;

	private UISprite glow;

	private GameObject icon;

	public string FXPrefab;

	private Color enabledColor = new Color(0f, 254f / 255f, 218f / 255f);

	private Color disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

	private Color enabledLabelColor = new Color(1f, 1f, 1f);

	private Color disabledLabelColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

	private ArrayList _onButtonClick = new ArrayList();

	private Vector3 originalLabelPosition;

	private Vector3 originalLabelScale;

	private Vector3 originalOutlinePosition;

	private Vector3 originalIconPosition;

	private GameObject glare;

	private float glowAlpha;

	private float glowSpeed = 4f;

	private bool glowActive;

	private bool assigned;

	public float xFlipOnClick = 180f;

	public float yFlipOnClick = 180f;

	public Color clickColor = new Color(1f, 1f, 1f);

	private Color cachedUnClickColor;

	public bool enableColorSwapOnClick;

	private bool _isEnabled;

	public event buttonEventHandler onButtonClick
	{
		add
		{
			if (_onButtonClick.IndexOf(value) < 0)
			{
				_onButtonClick.Add(value);
			}
		}
		remove
		{
			_onButtonClick.Remove(value);
		}
	}

	private void Awake()
	{
		_isEnabled = true;
		background = Util.GetObjectExactMatch(base.gameObject, "Background");
		if (background != null && (bool)background.GetComponent<UISprite>())
		{
			enabledColor = background.GetComponent<UISprite>().color;
		}
		outline = Util.GetObjectExactMatch(base.gameObject, "Outline");
		if (outline != null)
		{
			originalOutlinePosition = outline.transform.localPosition;
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Label");
		if (objectExactMatch != null)
		{
			label = objectExactMatch.GetComponent<UILabel>();
			originalLabelPosition = label.transform.localPosition;
			originalLabelScale = label.transform.localScale;
			SetText(label.text);
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Glow");
		if (objectExactMatch2 != null)
		{
			glow = objectExactMatch2.GetComponent<UISprite>();
		}
		if (glow != null)
		{
			glow.alpha = 0f;
		}
		icon = Util.GetObjectExactMatch(base.gameObject, "Icon");
		if (icon != null)
		{
			originalIconPosition = icon.transform.localPosition;
		}
		if (!string.IsNullOrEmpty(FXPrefab))
		{
			glare = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("NewUI/" + FXPrefab, typeof(GameObject)));
			glare.transform.parent = base.gameObject.transform;
			glare.transform.localPosition = new Vector3(0f, 0f, background.transform.localPosition.z - 2f);
			glare.transform.localRotation = Quaternion.identity;
			glare.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void SetEnabled(bool isEnabled)
	{
		_isEnabled = isEnabled;
		Color color = ((!isEnabled) ? disabledColor : enabledColor);
		if (background != null)
		{
			UISprite component = background.GetComponent<UISprite>();
			if (component != null)
			{
				component.color = color;
				component.alpha = 1f;
			}
		}
		if (glow != null)
		{
			glow.color = color;
			glow.alpha = 0f;
		}
		if (label != null)
		{
			label.color = ((!isEnabled) ? disabledLabelColor : enabledLabelColor);
		}
	}

	public void SetText(string s)
	{
		if (label != null)
		{
			UIUtils.AssignTextAndResize(label, s, originalLabelScale);
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(glare);
	}

	private void Start()
	{
		if (background != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(background);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(ButtonPress));
			UIEventListener uIEventListener2 = UIEventListener.Get(background);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(ButtonClick));
		}
		else if (icon != null)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(icon);
			uIEventListener3.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onPress, new UIEventListener.BoolDelegate(ButtonPress));
			UIEventListener uIEventListener4 = UIEventListener.Get(icon);
			uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(ButtonClick));
		}
		else
		{
			EB.Debug.LogError("Unable to add UIEventListener to ButtonScript " + base.name, this);
		}
	}

	private void ButtonPress(GameObject go, bool isPressed)
	{
		if (!_isEnabled)
		{
			return;
		}
		float x = 0f;
		float y = 0f;
		float x2 = 0f;
		float y2 = 0f;
		if (isPressed)
		{
			x = xFlipOnClick;
			y = yFlipOnClick;
			x2 = -1f;
			y2 = 1f;
			glowActive = true;
			if (background != null && _isEnabled && enableColorSwapOnClick)
			{
				UISprite component = background.GetComponent<UISprite>();
				if ((bool)component && component.color != clickColor)
				{
					cachedUnClickColor = component.color;
					component.color = clickColor;
				}
			}
		}
		else
		{
			glowActive = false;
			if (background != null && _isEnabled && enableColorSwapOnClick)
			{
				UISprite component2 = background.GetComponent<UISprite>();
				if ((bool)component2)
				{
					component2.color = cachedUnClickColor;
				}
			}
		}
		Quaternion localRotation = Quaternion.Euler(new Vector3(x, y, 0f));
		if (background != null)
		{
			background.transform.localRotation = localRotation;
		}
		if (label != null)
		{
			label.gameObject.transform.localPosition = originalLabelPosition + new Vector3(x2, y2, 0f);
		}
		if (outline != null)
		{
			outline.gameObject.transform.localPosition = originalOutlinePosition + new Vector3(x2, y2, 0f);
		}
		if (icon != null)
		{
			icon.gameObject.transform.localPosition = originalIconPosition + new Vector3(x2, y2, 0f);
		}
	}

	private void ButtonClick(GameObject go)
	{
		if (!_isEnabled)
		{
			return;
		}
		AudioEmitter component = GetComponent<AudioEmitter>();
		if (component != null)
		{
			component.Play();
		}
		ArrayList arrayList = new ArrayList(_onButtonClick);
		foreach (buttonEventHandler item in arrayList)
		{
			item(base.gameObject);
		}
	}

	private void Update()
	{
		if (glowActive)
		{
			glowAlpha += UnityEngine.Time.deltaTime * glowSpeed;
		}
		else
		{
			glowAlpha -= UnityEngine.Time.deltaTime * glowSpeed;
		}
		glowAlpha = Mathf.Clamp(glowAlpha, 0f, 1f);
		if (glow != null)
		{
			glow.alpha = glowAlpha;
		}
	}
}
