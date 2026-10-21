public class InventorySnapPanel : UIDraggablePanelSnap
{
	public InventoryItem.Category category;

	public override void Press(bool pressed)
	{
		base.Press(pressed);
	}

	protected override void OnGridUpdated()
	{
	}
}
