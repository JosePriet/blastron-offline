using EB;
using EB.Sparx;
using UnityEngine;

public class OptionsUI : PopupScript
{
	private GameObject audioIconState;

	private UILabel label;

	private bool _loginChangeInProgress;

	protected bool soundEnabled
	{
		get
		{
			return Options.sound != 0;
		}
		set
		{
			Options.sound = (value ? 1 : 0);
		}
	}

	protected override void OnStart()
	{
		EB.Debug.Log("ON START!");
		base.OnStart();
		Util.GetObjectExactMatch(base.gameObject, "AudioButton");
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "LogoutButton");
		objectExactMatch.GetComponent<ButtonScript>().onButtonClick += OnLogout;
		label = Util.FindComponent<UILabel>(Util.GetObjectExactMatch(Util.GetObjectExactMatch(base.gameObject, "logout"), "KabamIdTitle"));
		label.text = Localizer.GetString("ID_UI_LOGIN_SELECT_TITLE");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "UpgradeButton");
		objectExactMatch2.GetComponent<ButtonScript>().onButtonClick += OnUpgrade;
		label = Util.FindComponent<UILabel>(Util.GetObjectExactMatch(Util.GetObjectExactMatch(base.gameObject, "upgrade"), "KabamIdTitle"));
		label.text = Localizer.GetString("ID_UI_LOGIN_SELECT_TITLE");
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "WatchButton");
		objectExactMatch3.GetComponent<ButtonScript>().onButtonClick += OnWatch;
		label = objectExactMatch3.GetComponentInChildren<UILabel>();
		label.text = Localizer.GetString("ID_UI_OPTIONS_WATCH");
		label = Util.FindComponent<UILabel>(Util.GetObjectExactMatch(Util.GetObjectExactMatch(base.gameObject, "Intro"), "IntroVideoTitle"));
		label.text = Localizer.GetString("ID_UI_OPTIONS_INTRO_VIDEO");
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "HelpButton");
		objectExactMatch4.GetComponent<ButtonScript>().onButtonClick += OnHelp;
		label = Util.FindComponent<UILabel>(Util.GetObjectExactMatch(Util.GetObjectExactMatch(base.gameObject, "Help"), "HelpTitle"));
		label.text = Localizer.GetString("ID_UI_OPTIONS_HELP");
		ButtonScript component = Util.GetObjectExactMatch(base.gameObject, "AudioButton").GetComponent<ButtonScript>();
		audioIconState = Util.GetObjectExactMatch(base.gameObject, "IconOff");
		component.onButtonClick += OnToggleAudio;
		label = Util.FindComponent<UILabel>(Util.GetObjectExactMatch(Util.GetObjectExactMatch(base.gameObject, "Sound"), "SoundTitle"));
		label.text = Localizer.GetString("ID_UI_OPTIONS_SOUND");
		Refresh();
	}

	protected override void OnClose()
	{
		if (!_loginChangeInProgress)
		{
			base.OnClose();
		}
	}

	public void OnFocusPopup()
	{
		ScreenManager.Instance.FadeOut(null);
	}

	private void Refresh()
	{
		EB.Debug.Log("ON START333!");
		bool flag = Hub.Instance.LoginManager.LoginState == LoginState.LoggedIn;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "logout");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "upgrade");
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Help");
		if (flag)
		{
			string labelText = "ID_UI_OPTIONS_GUEST_ID";
			User localUser = Hub.Instance.LoginManager.LocalUser;
			if (localUser.HasEmail)
			{
				labelText = localUser.Email;
				NGUITools.SetActive(objectExactMatch2, false);
				NGUITools.SetActive(objectExactMatch, true);
			}
			else
			{
				NGUITools.SetActive(objectExactMatch2, true);
				NGUITools.SetActive(objectExactMatch, false);
			}
			objectExactMatch3.SetActive(true);
			UIUtils.SetLabelContents(base.gameObject, "UpgradeButton", "ID_UI_OPTIONS_KABAM_UPGRADE");
			UIUtils.SetLabelContents(base.gameObject, "KabamId", labelText);
		}
		else
		{
			UIUtils.SetLabelContents(base.gameObject, "UpgradeButton", "ID_UI_OPTIONS_KABAM_LOGIN");
			objectExactMatch3.SetActive(false);
			NGUITools.SetActive(objectExactMatch, false);
			NGUITools.SetActive(objectExactMatch2, true);
			if (_loginChangeInProgress)
			{
				objectExactMatch2.GetComponentInChildren<ButtonScript>().SetEnabled(false);
			}
		}
		RefreshAudioState();
	}

	public void RefreshAudioState()
	{
		NGUITools.SetActive(audioIconState, !soundEnabled);
	}

	private void OnToggleAudio(GameObject caller)
	{
		if (!_loginChangeInProgress)
		{
			soundEnabled = !soundEnabled;
			RefreshAudioState();
		}
	}

	private void OnLogout(GameObject caller)
	{
		if (!_loginChangeInProgress)
		{
			ScreenManager.Instance.Confirmation(false, "ID_UI_OPTIONS_LOGOUT_CONFIRM_TITLE", "ID_UI_OPTIONS_LOGOUT_CONFIRM_HINT", "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", OnConfirmLogout);
		}
	}

	private void OnHelp(GameObject caller)
	{
		if (!_loginChangeInProgress)
		{
			HomeUI.OpenWebPage(HomeUI.PageType.Help);
		}
	}

	private void OnConfirmLogout(ConfirmationDialogUI.ButtonIds buttonId)
	{
		if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
		{
			Hub.Instance.LoginManager.Logout();
			_loginChangeInProgress = true;
			Refresh();
			Coroutines.SetTimeout(Setup.Instance.ReturnToFrontendScreen, 1000);
		}
	}

	private void OnChildScreenOSBackButtonPressed()
	{
		_loginChangeInProgress = false;
	}

	private void OnUpgrade(GameObject caller)
	{
		if (!_loginChangeInProgress)
		{
			if (Hub.Instance.LoginManager.LoginState == LoginState.LoggedIn)
			{
				Hub.Instance.LoginManager.Upgrade();
			}
			else
			{
				Hub.Instance.LoginManager.Login(false);
			}
			_loginChangeInProgress = true;
		}
	}

	private void OnWatch(GameObject caller)
	{
		if (!_loginChangeInProgress)
		{
			ScreenManager.Instance.OpenPopup("IntroSequenceUI", true, true);
		}
	}
}
