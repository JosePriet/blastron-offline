public class UIInputPersistent : UIInput
{
	public void ShowKeyboard()
	{
		base.enabled = true;
		base.gameObject.SendMessage("OnSelect", true);
	}
}
