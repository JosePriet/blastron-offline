using EB;
using UnityEngine;

public class ButtonSliced : MonoBehaviour
{
	private GameObject onState;

	private GameObject offState;

	private bool isOver;

	private bool isPressed;

	private int updateRequiredInFrames;

	private void Start()
	{
		onState = Util.GetObjectExactMatch(base.gameObject, "On");
		offState = Util.GetObjectExactMatch(base.gameObject, "Off");
		updateRequiredInFrames = 2;
	}

	protected virtual void OnPress(bool isPressed)
	{
		this.isPressed = isPressed;
		updateRequiredInFrames = 2;
		onState.SetActive(true);
		offState.SetActive(true);
	}

	protected virtual void OnHover(bool isOver)
	{
		this.isOver = isOver;
		updateRequiredInFrames = 2;
		onState.SetActive(true);
		offState.SetActive(true);
	}

	private void Update()
	{
		if (updateRequiredInFrames > 0)
		{
			updateRequiredInFrames--;
			if (updateRequiredInFrames == 0)
			{
				bool flag = isOver || isPressed;
				onState.SetActive(flag);
				offState.SetActive(!flag);
			}
		}
	}
}
