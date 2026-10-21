using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/First Boot Done")]
public class SequenceAction_FirstBootDone : Action
{
	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Profile.Instance.MarkTutorialDone("firstboot_shown");
		Finished.Invoke();
	}
}
