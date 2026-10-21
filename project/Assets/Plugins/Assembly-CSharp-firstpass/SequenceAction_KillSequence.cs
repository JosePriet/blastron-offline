using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Kill Sequence")]
public class SequenceAction_KillSequence : EB.Sequence.Runtime.Action
{
	[Entry]
	public void Destroy()
	{
		Debug.Log("KillSequence:" + base.Parent.name);
		base.Parent.KillSequence();
	}
}
