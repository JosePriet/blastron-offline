using System.Collections;
using EB;
using UnityEngine;

public class UIDraggablePanelSnap : UIDraggablePanel
{
	public UIGridSnap gridObject;

	private GameObject panelObject;

	private Vector3 SpringPanelTarget;

	public float relativeDeadzone = 0.05f;

	private SpringPanel sp;

	public int gridItemsHorizontal = 1;

	public int gridItemsVertical = 1;

	protected override void Awake()
	{
		if (gridObject == null)
		{
			EB.Debug.LogError("gridObject for " + base.gameObject.name + " is null!");
		}
		panelObject = GetComponent<UIPanel>().gameObject;
		SpringPanel.Begin(panelObject, base.transform.localPosition, 1000f);
		sp = panelObject.GetComponent<SpringPanel>();
		base.Awake();
		SpringPanelTarget = base.transform.localPosition;
	}

	public override void Press(bool pressed)
	{
		if (!pressed)
		{
			SpringPanelTarget = sp.target;
			float num = relativeDeadzone * (float)Screen.width;
			if (UICamera.currentTouch.totalDelta.x > num)
			{
				SwipeRight();
			}
			else if (UICamera.currentTouch.totalDelta.x < -1f * num)
			{
				SwipeLeft();
			}
			else
			{
				SpringPanel.Begin(panelObject, SpringPanelTarget, 13f);
			}
		}
		else
		{
			base.Press(pressed);
		}
	}

	public void SwipeLeft()
	{
		SpringPanelTarget.x -= gridObject.cellWidth * (float)gridItemsHorizontal;
		SpringPanel.Begin(panelObject, SpringPanelTarget, 13f);
		StartCoroutine("DoUpdateGrid", false);
	}

	public void SwipeRight()
	{
		SpringPanelTarget.x += gridObject.cellWidth * (float)gridItemsHorizontal;
		SpringPanel.Begin(panelObject, SpringPanelTarget, 13f);
		StartCoroutine("DoUpdateGrid", true);
	}

	private IEnumerator DoUpdateGrid(bool right)
	{
		if (sp.enabled)
		{
			yield return new WaitForSeconds(0.1f);
		}
		gridObject.RepositionUpdate(right);
		OnGridUpdated();
	}

	protected virtual void OnGridUpdated()
	{
	}
}
