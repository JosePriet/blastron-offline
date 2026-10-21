using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/AwardVault")]
public class SequenceAction_AwardVault : EB.Sequence.Runtime.Action
{
	[Property]
	public string VaultName;

	[Property]
	public int index;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Award()
	{
		Debug.LogError("Lootinators no longer exist! This class should not be used!");
		Finished.Invoke();
	}
}
