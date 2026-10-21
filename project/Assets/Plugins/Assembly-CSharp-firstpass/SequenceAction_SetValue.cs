using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Set Value")]
public class SequenceAction_SetValue : Action
{
	[Property]
	public int value;

	[Variable(ExpectedType = typeof(float))]
	public Variable Target = Variable.Null;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Set()
	{
		Target.Value = (float)value;
		Finished.Invoke();
	}
}
