using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Platform")]
public class SequenceAction_Platform : Action
{
	[Trigger]
	public Trigger Mobile = new Trigger();

	[Trigger]
	public Trigger Web = new Trigger();

	[Entry]
	public void Select()
	{
		Mobile.Invoke();
	}

	[Entry]
	public void _Mobile()
	{
		Mobile.Invoke();
	}

	[Entry]
	public void _Web()
	{
		Web.Invoke();
	}
}
