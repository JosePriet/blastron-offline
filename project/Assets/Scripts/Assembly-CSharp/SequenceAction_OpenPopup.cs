using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/OpenPopup")]
public class SequenceAction_OpenPopup : Action
{
	[Property]
	public string popupName;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		ScreenManager.Instance.OpenPopup(popupName);
		Finished.Invoke();
	}
}
