using EB;
using UnityEngine;

public class ResizeForText : MonoBehaviour
{
	public UILabel text;

	public GameObject artLeft;

	public GameObject artStretch;

	public GameObject artRight;

	private int framesToDelay;

	public void Resize()
	{
		int num = Mathf.CeilToInt(UIUtils.GetLabelTextSize(text).x);
		int num2 = 0;
		switch (text.pivot)
		{
		case UIWidget.Pivot.TopRight:
		case UIWidget.Pivot.Right:
		case UIWidget.Pivot.BottomRight:
			num2 = -1;
			break;
		case UIWidget.Pivot.TopLeft:
		case UIWidget.Pivot.Left:
		case UIWidget.Pivot.BottomLeft:
			num2 = 1;
			break;
		}
		if (num2 == -1)
		{
			float num3 = text.transform.localPosition.x - artRight.transform.localPosition.x;
			Vector3 localScale = artStretch.transform.localScale;
			localScale.x = (float)num - num3;
			artStretch.transform.localScale = localScale;
			Vector3 localPosition = artLeft.transform.localPosition;
			localPosition.x = text.transform.localPosition.x - (float)num;
			artLeft.transform.localPosition = localPosition;
		}
	}

	private void Start()
	{
		framesToDelay = 1;
	}

	private void Update()
	{
		if (framesToDelay > 0)
		{
			framesToDelay--;
			if (framesToDelay == 0)
			{
				Resize();
			}
		}
	}
}
