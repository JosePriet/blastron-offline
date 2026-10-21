using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;

[MenuItem(Path = "Actions/Inventory Everything Free")]
public class SequenceAction_InventoryEverythingFree : Action
{
	[Property]
	public bool enable = true;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Hub.Instance.InventoryManager.EverythingFree = enable;
		Finished.Invoke();
	}
}
