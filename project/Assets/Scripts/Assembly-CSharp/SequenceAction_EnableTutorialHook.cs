using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/EnableTutorialHook")]
public class SequenceAction_EnableTutorialHook : EB.Sequence.Runtime.Action
{
	[Property]
	public bool Enable;

	[Property]
	public string TutorialId;

	[Property]
	public bool HideItem = true;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		EB.Debug.Log("TUTORIAL HOOK ID:" + TutorialId + "State:" + Enable + " SEQUENCE:" + base.Parent.name);
		TutorialHook.Parameters parameters = new TutorialHook.Parameters();
		parameters.tutorialId = TutorialId;
		parameters.hideItem = HideItem;
		GameObject gameObject = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
		if (Enable)
		{
			gameObject.BroadcastMessage("EnableButton", parameters, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			gameObject.BroadcastMessage("DisableButton", parameters, SendMessageOptions.DontRequireReceiver);
		}
		Finished.Invoke();
	}
}
