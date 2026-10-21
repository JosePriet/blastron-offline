using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Display No Player Weapon")]
public class SequenceAction_DisplayNoWeapon : Action
{
	[Property]
	public bool enabled;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameHUD.Instance.SendMessage("DisplayNoWeapon", enabled);
		Finished.Invoke();
	}
}
