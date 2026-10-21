using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;

[MenuItem(Path = "Actions/Inventory Add Override")]
public class SequenceAction_InventoryAddOverrideItem : Action
{
	[Property]
	public string itemName;

	[Property]
	public int count;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Hub.Instance.InventoryManager.AddOverride(itemName, count);
		Finished.Invoke();
	}
}
