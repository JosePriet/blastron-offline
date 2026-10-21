using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Modify Value")]
public class SequenceAction_ModifyValue : Action
{
	[Variable(ExpectedType = typeof(float))]
	public Variable Value = Variable.Null;

	[Property]
	public float IncDecVal = 1f;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Increment()
	{
		Modify(IncDecVal);
		Activate();
	}

	[Entry]
	public void Decrement()
	{
		Modify(IncDecVal);
		Activate();
	}

	[Entry]
	public void Zero()
	{
		Value.Value = 0f;
		Activate();
	}

	public override bool Update()
	{
		Finished.Invoke();
		return false;
	}

	private void Modify(float by)
	{
		float num = (float)Value.Value;
		num += by;
		Value.Value = num;
	}
}
