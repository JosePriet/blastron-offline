using System;
using System.Collections;
using EB;
using UnityEngine;

public class MenuButtonScript : MonoBehaviour
{
	public delegate void buttonEventHandler(GameObject go);

	private GameObject background;

	private GameObject outline;

	private UILabel label;

	private UISlicedSprite glow;

	private GameObject icon;

	private UIPanel panel;

	public Color enabledColor = new Color(117f / 255f, 209f / 255f, 36f / 255f);

	public Color disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

	public Color enabledLabelColor = new Color(1f, 1f, 1f);

	public Color disabledLabelColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

	public Color onPressedColor = new Color(1f, 0f, 0f);

	public string FXPrefab;

	public float FXPrefabStartDelay;

	private ArrayList _onButtonClick = new ArrayList();

	private GameObject glare;

	private float glowAlpha;

	private float glowSpeed = 4f;

	private bool glowActive;

	private bool assigned;

	private bool _enabled = true;

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
		panel = GetComponent<UIPanel>();
		background = Util.GetObjectExactMatch(base.gameObject, "Background");
		if (background != null)
		{
			Material material = new Material(background.GetComponent<UISprite>().material);
			material.shader = Shader.Find("Unlit/Transparent Colored Offset");
			background.GetComponent<UISprite>().material = material;
		}
		outline = Util.GetObjectExactMatch(base.gameObject, "Outline");
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Label");
		if (objectExactMatch != null)
		{
			label = objectExactMatch.GetComponent<UILabel>();
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Glow");
		if (objectExactMatch2 != null)
		{
			glow = objectExactMatch2.GetComponent<UISlicedSprite>();
		}
		if (glow != null)
		{
			glow.alpha = 0f;
		}
		icon = Util.GetObjectExactMatch(base.gameObject, "Icon");
		if (!string.IsNullOrEmpty(FXPrefab) && background != null)
		{
			glare = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("NewUI/" + FXPrefab, typeof(GameObject)));
			glare.transform.parent = base.gameObject.transform;
			glare.transform.localPosition = new Vector3(0f, 0f, background.transform.localPosition.z - 2f);
			glare.transform.localRotation = Quaternion.identity;
			glare.transform.localScale = new Vector3(1f, 1f, 1f);
			ButtonFXScript component = glare.GetComponent<ButtonFXScript>();
			component.StartTimeOffset = FXPrefabStartDelay;
		}
	}

	public void SetGlareActive(bool isActive)
	{
		if (glare != null)
		{
			glare.SetActive(isActive);
		}
	}

	public void SetEnabled(bool isEnabled)
	{
		_enabled = isEnabled;
		Color color = ((!_enabled) ? disabledColor : enabledColor);
		Color color2 = ((!_enabled) ? disabledColor : enabledColor);
		if (background != null)
		{
			UISlicedSprite component = background.GetComponent<UISlicedSprite>();
			if (component != null)
			{
				component.color = color;
			}
			UISprite component2 = background.GetComponent<UISprite>();
			if (component2 != null)
			{
				component2.alpha = 1f;
			}
		}
		if (glow != null)
		{
			glow.alpha = 0f;
		}
		if (label != null)
		{
			label.color = ((!_enabled) ? disabledLabelColor : enabledLabelColor);
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(glare);
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(ButtonPress));
		UIEventListener uIEventListener2 = UIEventListener.Get(base.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(ButtonClick));
	}

	private void ButtonPress(GameObject go, bool isPressed)
	{
		if (!_enabled)
		{
			return;
		}
		if (isPressed)
		{
			glowActive = true;
			if (panel != null)
			{
				panel.widgetsAreStatic = false;
			}
			if (background != null)
			{
				background.GetComponent<UISlicedSprite>().color = onPressedColor;
			}
		}
		else
		{
			glowActive = false;
			if (panel != null)
			{
				panel.widgetsAreStatic = true;
			}
			if (background != null)
			{
				background.GetComponent<UISlicedSprite>().color = ((!_enabled) ? disabledColor : enabledColor);
			}
		}
	}

	private void ButtonClick(GameObject go)
	{
		if (_onButtonClick == null)
		{
			return;
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
