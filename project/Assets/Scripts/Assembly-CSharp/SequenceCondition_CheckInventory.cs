using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;

[MenuItem(Path = "Conditions/Check Inventory")]
public class SequenceCondition_CheckInventory : Condition
{
	[Trigger]
	public Trigger Empty = new Trigger();

	public override bool Update()
	{
		if (Hub.Instance.InventoryManager.IsOverriden && !Hub.Instance.InventoryManager.EverythingFree)
		{
			if (Hub.Instance.InventoryManager.IsEmpty())
			{
				Empty.Invoke();
			}
			return false;
		}
		return false;
	}
}
