using EB;
using UnityEngine;

public class TabButtonScript : MonoBehaviour
{
	public delegate void buttonEventHandler(GameObject go);

	public string text;

	private UILabel labelSelected;

	private UILabel labelUnselected;

	private UISlicedSprite backgroundSelected;

	private UISlicedSprite backgroundUnselected;

	private UISlicedSprite backgroundSelectedGlow;

	private GameObject selected;

	private GameObject unselected;

	private Vector3 defaultTextSize;

	public event buttonEventHandler onTabSelected;

	private void Awake()
	{
		selected = Util.GetObjectExactMatch(base.gameObject, "Selected");
		unselected = Util.GetObjectExactMatch(base.gameObject, "Unselected");
		GameObject objectExactMatch = Util.GetObjectExactMatch(selected, "Background");
		backgroundSelected = Util.FindComponent<UISlicedSprite>(objectExactMatch);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(selected, "Glow");
		if (objectExactMatch2 != null)
		{
			backgroundSelectedGlow = Util.FindComponent<UISlicedSprite>(objectExactMatch2);
		}
		backgroundUnselected = Util.FindComponent<UISlicedSprite>(unselected);
		labelSelected = Util.FindComponent<UILabel>(selected);
		labelUnselected = Util.FindComponent<UILabel>(unselected);
		defaultTextSize = labelSelected.transform.localScale;
		SetText(text);
	}

	private void Start()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Icon");
		if (objectExactMatch != null)
		{
			UIUtils.CenterSpriteAndLabel(base.gameObject, "Icon", "LabelSelected");
			labelUnselected.transform.localPosition = labelSelected.transform.localPosition;
		}
	}

	public void SetSelected(bool isSelected)
	{
		Color color = backgroundSelected.color;
		color.a = ((!isSelected) ? 0f : 1f);
		backgroundSelected.color = color;
		if (backgroundSelectedGlow != null)
		{
			color = backgroundSelectedGlow.color;
			color.a = ((!isSelected) ? 0f : 1f);
			backgroundSelectedGlow.color = color;
		}
		color = backgroundUnselected.color;
		color.a = ((!isSelected) ? 1f : 0f);
		backgroundUnselected.color = color;
		color = labelSelected.color;
		color.a = ((!isSelected) ? 0f : 1f);
		labelSelected.color = color;
		color = labelUnselected.color;
		color.a = ((!isSelected) ? 1f : 0f);
		labelUnselected.color = color;
		if (isSelected && onTabSelected != null)
		{
			onTabSelected(base.gameObject);
		}
	}

	public void SetText(string text)
	{
		UIUtils.AssignTextAndResize(labelSelected, text, defaultTextSize);
		UIUtils.AssignTextAndResize(labelUnselected, text, defaultTextSize);
	}
}
