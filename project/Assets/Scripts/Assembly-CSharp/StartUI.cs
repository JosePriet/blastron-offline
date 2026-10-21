using System;
using EB;
using UnityEngine;

public class StartUI : MonoBehaviour
{
	private bool _clicked;

	private void Start()
	{
		ScreenManager.Instance.AddPersistentScreen("SplashUI");
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnFinish));
	}

	private void OnOpenPopup()
	{
	}

	private void OnFinish(GameObject g)
	{
		if (!_clicked)
		{
			_clicked = true;
			ScreenManager.Instance.SetCameraClearFlags(CameraClearFlags.Color, Color.black);
			ScreenManager.Instance.BlockingUI("ID_UI_LOGIN_BLOCKING_TITLE", "ID_UI_LOGIN_BLOCKING_DESC");
			Coroutines.NextFrame(() =>
			{
				ScreenManager.Instance.EnableNibbles(true);
				ScreenManager.Instance.CloseAllPopups();
				ScreenManager.Instance.ClearPersistentScreens();
				Setup.Instance.CheckForFirstLogin();
				ScreenManager.Instance.SetTitleScreenCleared();
			});
		}
	}

	private void OnStart(GameObject g)
	{
	}
}
