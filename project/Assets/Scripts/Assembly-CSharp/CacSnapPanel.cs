public class CacSnapPanel : UIDraggablePanelSnap
{
	public CharacterDB.Category category;

	public override void Press(bool pressed)
	{
		base.gameObject.SendMessageUpwards("OnPartSelected", category);
		base.Press(pressed);
	}

	protected override void OnGridUpdated()
	{
		base.gameObject.SendMessageUpwards("OnPartSelected", category);
	}
}
