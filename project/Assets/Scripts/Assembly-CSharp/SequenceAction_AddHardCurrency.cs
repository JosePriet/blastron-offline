using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;

[MenuItem(Path = "Actions/Add Hard Currency")]
public class SequenceAction_AddHardCurrency : Action
{
	[Property]
	public int count;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Hub.Instance.WalletManager.Credit(count, "sequence");
		Finished.Invoke();
	}
}
