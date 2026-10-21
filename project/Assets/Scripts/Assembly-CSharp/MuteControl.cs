using UnityEngine;

public class MuteControl : MonoBehaviour
{
	public delegate void Callback(bool action);

	private Callback _fnCallback;

	public void Init(Callback fnCallback, string message)
	{
		UILabel component = base.transform.FindChild("Label").GetComponent<UILabel>();
		component.text = message;
		_fnCallback = fnCallback;
	}

	private void ActionYes()
	{
		Action(true);
	}

	private void ActionNo()
	{
		Action(false);
	}

	private void Action(bool action)
	{
		if (_fnCallback != null)
		{
			_fnCallback(action);
		}
	}
}
