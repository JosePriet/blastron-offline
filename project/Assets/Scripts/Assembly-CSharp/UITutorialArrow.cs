using EB.Sequence;
using UnityEngine;

public class UITutorialArrow : MonoBehaviour
{
	public string tutorialId = string.Empty;

	private BigArrow arrow;

	public Vector3 offset = Vector3.zero;

	public float angle;

	public bool isArrowActive;

	public bool isArrowAllowed = true;

	public BigArrow.ArrowType arrowType = BigArrow.ArrowType.UI;

	public void ShowArrow()
	{
		isArrowActive = true;
		DoShowArrow();
	}

	public void DoShowArrow()
	{
		if (isArrowAllowed)
		{
			if (arrow == null)
			{
				arrow = base.gameObject.AddComponent<BigArrow>();
				arrow.SetArrowType(arrowType);
				arrow.transform.position = base.gameObject.transform.position;
				arrow.SetOffset(offset.x, offset.y, offset.z);
				arrow.SetAngle(angle);
				arrow.SetUIParent(base.gameObject);
				arrow.SetCallback(OnClickedFromArrow);
			}
		}
		else
		{
			DoHideArrow();
		}
	}

	public void OnClickedFromArrow()
	{
	}

	public void Start()
	{
		TutorialManager.Instance.RegisterArrow(tutorialId, this);
	}

	public void HideArrow()
	{
		isArrowActive = false;
		DoHideArrow();
	}

	public void DoHideArrow()
	{
		Object.Destroy(arrow);
	}

	public void ReregisterArrow(string newTutorialId)
	{
		TutorialManager.Instance.UnRegisterArrow(tutorialId, this);
		tutorialId = newTutorialId;
		TutorialManager.Instance.RegisterArrow(tutorialId, this);
	}

	public void OnUILayerChange(bool state)
	{
		TutorialManager.Instance.RegisterArrow(tutorialId, this);
	}

	public void OnDestroy()
	{
		HideArrow();
		if (TutorialManager.Instance != null)
		{
			TutorialManager.Instance.UnRegisterArrow(tutorialId, this);
		}
	}

	public void OnPress(bool down)
	{
		if (down)
		{
			EB.Sequence.Component.Activate(base.gameObject, tutorialId, typeof(SequenceEvent_OnButtonPressed));
		}
	}

	public void OnClick()
	{
		EB.Sequence.Component.Activate(base.gameObject, tutorialId, typeof(SequenceEvent_OnButtonClick));
	}
}
