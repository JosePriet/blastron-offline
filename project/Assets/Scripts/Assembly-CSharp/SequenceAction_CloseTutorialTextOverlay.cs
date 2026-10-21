using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/CloseTutorialTextOverlay")]
public class SequenceAction_CloseTutorialTextOverlay : EB.Sequence.Runtime.Action
{
	[Property]
	public string TutorialId;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameObject gameObject = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
		gameObject.BroadcastMessage("CloseOverlay", TutorialId, SendMessageOptions.DontRequireReceiver);
		Finished.Invoke();
	}
}
