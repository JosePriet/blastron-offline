using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/AIFireAtPlayer")]
public class SequenceAction_AIFireAtPlayer : Action
{
	[Property]
	public int aiIndex;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Fire()
	{
		LevelData.Instance.AIFireAtPlayer(aiIndex);
		Finished.Invoke();
	}
}
