using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Tutorial Done")]
public class SequenceAction_MarkTutorialDone : Action
{
	[Property]
	public string tutorialId;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Profile.Instance.MarkTutorialDone(tutorialId);
		Finished.Invoke();
	}
}
