using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Signal Challenge Complete")]
public class SequenceAction_SignalChallengeComplete : EB.Sequence.Runtime.Action
{
	[Entry]
	public void Signal()
	{
		Debug.Log(">>>>> SequenceAction_SignalChallengeComplete: " + base.Parent);
		ChallengeManager.Instance.Complete(base.Parent.name, true);
		base.Parent.KillSequence();
	}
}
