using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Color")]
public class UIButtonColor : MonoBehaviour
{
	public GameObject tweenTarget;

	public Color hover = new Color(0.6f, 1f, 0.2f, 1f);

	public Color pressed = Color.grey;

	public float duration = 0.2f;

	protected Color mColor;

	protected bool mInitDone;

	protected bool mStarted;

	protected bool mHighlighted;

	public Color defaultColor
	{
		get
		{
			return mColor;
		}
		set
		{
			mColor = value;
		}
	}

	private void Start()
	{
		mStarted = true;
	}

	protected virtual void OnEnable()
	{
		if (mStarted && mHighlighted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
	}

	private void OnDisable()
	{
		if (tweenTarget != null)
		{
			TweenColor component = tweenTarget.GetComponent<TweenColor>();
			if (component != null)
			{
				component.color = mColor;
				component.enabled = false;
			}
		}
	}

	protected void Init()
	{
		mInitDone = true;
		if (tweenTarget == null)
		{
			tweenTarget = base.gameObject;
		}
		UIWidget component = tweenTarget.GetComponent<UIWidget>();
		if (component != null)
		{
			mColor = component.color;
			return;
		}
		Renderer renderer = tweenTarget.renderer;
		if (renderer != null)
		{
			mColor = renderer.material.color;
			return;
		}
		Light light = tweenTarget.light;
		if (light != null)
		{
			mColor = light.color;
			return;
		}
		Debug.LogWarning(NGUITools.GetHierarchy(base.gameObject) + " has nothing for UIButtonColor to color", this);
		base.enabled = false;
	}

	protected virtual void OnPress(bool isPressed)
	{
		if (!mInitDone)
		{
			Init();
		}
		if (base.enabled)
		{
			GameObject go = tweenTarget;
			float num = duration;
			Color color;
			if (isPressed)
			{
				color = pressed;
			}
			else
			{
				color = ((!UICamera.IsHighlighted(base.gameObject)) ? mColor : hover);
			}
			TweenColor.Begin(go, num, color);
		}
	}

	protected virtual void OnHover(bool isOver)
	{
		if (base.enabled)
		{
			if (!mInitDone)
			{
				Init();
			}
			TweenColor.Begin(tweenTarget, duration, (!isOver) ? mColor : hover);
			mHighlighted = isOver;
		}
	}
}
