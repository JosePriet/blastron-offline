using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/CallScreenFn")]
public class SequenceAction_CallScreenFn : Action
{
	[Property]
	public string screenName = string.Empty;

	[Property]
	public string functionName = string.Empty;

	[Property]
	public string parameter = string.Empty;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameObject popup = ScreenManager.Instance.GetPopup(screenName);
		if (null != popup)
		{
			if (parameter.Length > 0)
			{
				popup.SendMessage(functionName, parameter);
			}
			else
			{
				popup.SendMessage(functionName);
			}
		}
		Finished.Invoke();
	}
}
