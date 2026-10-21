using EB;

public class ErrorUI : PopupScript
{
	public Action _callback;

	public void ShowError(string error, Action callback)
	{
		UIUtils.SetLabelContents(base.gameObject, "Description", error);
		_callback = callback;
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (_callback != null)
		{
			_callback();
		}
	}

	protected override void OnUpdate()
	{
		ScreenManager.Instance.ForceHideLoadingScreen();
	}
}
