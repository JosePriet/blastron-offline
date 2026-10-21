using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Run Screen Tutorial")]
public class SequenceAction_RunScreenTutorial : Action
{
	[Property]
	public bool run = true;

	[Property]
	public string screenName = string.Empty;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameObject gameObject = ScreenManager.Instance.FindPopup(screenName);
		gameObject.SendMessage("RunTutorial", run);
		Finished.Invoke();
	}
}
