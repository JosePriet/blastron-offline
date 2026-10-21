using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Enable Screen Input")]
public class SequenceAction_EnableScreenInput : Action
{
	[Property]
	public bool enable = true;

	[Property]
	public string screenName = string.Empty;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameObject gameObject = ScreenManager.Instance.FindPopup(screenName);
		gameObject.SendMessage("EnableInput", enable);
		Finished.Invoke();
	}
}
