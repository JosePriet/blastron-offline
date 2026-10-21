using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class LoginListener : EB.Sparx.LoginListener
{
	private LoginScreenFlags _flags;

	private string[] loginScreenNames = new string[5] { "ChangeNameUI", "LoginCreateUI", "LoginExistingUI", "LoginMergeUI", "LoginSelectUI" };

	public void ShowLoginScreen(LoginScreenFlags flags, Hashtable data)
	{
		ScreenManager.Instance.ForceHideLoadingScreen();
		_flags = flags;
		switch (flags)
		{
		case LoginScreenFlags.NameSelect:
			if (ScreenManager.Instance.GetPopup("ChangeNameUI") == null)
			{
				ScreenManager.Instance.OpenPopup("ChangeNameUI", true, true);
			}
			return;
		case LoginScreenFlags.AccountMerge:
			if (ScreenManager.Instance.GetPopup("LoginMergeUI") == null)
			{
				GameObject gameObject = ScreenManager.Instance.OpenPopup("LoginMergeUI", true, true);
				gameObject.GetComponent<LoginMergeUI>().SetData(data);
			}
			return;
		case LoginScreenFlags.CreateAccount:
		{
			GameObject gameObject2 = ScreenManager.Instance.GetPopup("LoginCreateUI");
			if (gameObject2 == null)
			{
				gameObject2 = ScreenManager.Instance.OpenPopup("LoginCreateUI", true, true);
			}
			LoginCreateUI component = gameObject2.GetComponent<LoginCreateUI>();
			component.SetData(data);
			return;
		}
		}
		if (ScreenManager.Instance.GetPopup("LoginSelectUI") == null)
		{
			ScreenManager.Instance.OpenPopup("LoginSelectUI", true, true);
		}
		List<GameObject> loginScreens = GetLoginScreens();
		bool flag = (flags & LoginScreenFlags.AllowSkip) == LoginScreenFlags.AllowSkip;
		foreach (GameObject item in loginScreens)
		{
			item.SendMessage("SetEmail", PlayerPrefs.GetString("sparx-email", string.Empty));
			item.SendMessage("SetPassword", PlayerPrefs.GetString("sparx-password", string.Empty));
			item.SendMessage("SetCanSkip", flag);
		}
	}

	public void OnOfflineDataLoaded()
	{
		ScreenManager.Instance.BroadcastMessage("OnOfflineDataLoaded", SendMessageOptions.DontRequireReceiver);
	}

	public void CloseLoginScreen()
	{
		GameObject topLoginScreen = GetTopLoginScreen();
		if (topLoginScreen != null)
		{
			ScreenManager.Instance.ClosePopup(topLoginScreen.name);
		}
	}

	public void EnableInput(bool enabled)
	{
		if (!enabled)
		{
			List<GameObject> loginScreens = GetLoginScreens();
			GameObject popup = ScreenManager.Instance.GetPopup("OptionsUI");
			if (loginScreens.Count > 0 || popup != null)
			{
				GameObject overlayScreen = ScreenManager.Instance.GetOverlayScreen("LoginActivity");
				overlayScreen.SendMessage("EnableRings", true, SendMessageOptions.DontRequireReceiver);
				overlayScreen.transform.localPosition = new Vector3(0f, 0f, -500f);
			}
		}
		else
		{
			GameObject overlayScreen2 = ScreenManager.Instance.GetOverlayScreen("LoginActivity");
			overlayScreen2.SendMessage("EnableRings", false, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SaveCredentials()
	{
		GameObject popup = ScreenManager.Instance.GetPopup("ChangeNameUI");
		ChangeNameUI changeNameUI = Util.FindComponent<ChangeNameUI>(popup);
		if (changeNameUI != null && _flags != LoginScreenFlags.NameSelect)
		{
			PlayerPrefs.SetString("sparx-email", changeNameUI.Email);
			PlayerPrefs.SetString("sparx-password", changeNameUI.Password);
			PlayerPrefs.Save();
		}
	}

	public void OnLoggedIn()
	{
		GameObject topLoginScreen = GetTopLoginScreen();
		if (topLoginScreen != null)
		{
			PopupScript popupScript = Util.FindComponent<PopupScript>(topLoginScreen);
			popupScript.ClosePopup();
			popupScript.onClosed += OnLoginScreenClosed;
		}
		else
		{
			Setup.Instance.OnLoggedIn();
		}
	}

	private void OnLoginScreenClosed(PopupScript caller)
	{
		Setup.Instance.OnLoggedIn();
	}

	public void OnLoginFailed(string error, bool silent, bool fatal)
	{
		EnableInput(true);
		GameObject topLoginScreen = GetTopLoginScreen();
		if (topLoginScreen != null && !fatal)
		{
			topLoginScreen.SendMessage("ReportLoginError", error);
		}
		else
		{
			Setup.Instance.OnLoginFailed(error, silent);
		}
	}

	public void OnDisconnected(string error)
	{
		Setup.Instance.OnDisconnected(error);
	}

	public void OnNewVersionRequired(string storeUrl)
	{
		Setup.Instance.OnNewVersionRequired(storeUrl);
	}

	private List<GameObject> GetLoginScreens()
	{
		List<GameObject> list = new List<GameObject>();
		string[] array = loginScreenNames;
		foreach (string name in array)
		{
			GameObject popup = ScreenManager.Instance.GetPopup(name);
			if (popup != null)
			{
				list.Add(popup);
			}
		}
		return list;
	}

	private GameObject GetTopLoginScreen()
	{
		string[] array = loginScreenNames;
		foreach (string name in array)
		{
			GameObject popup = ScreenManager.Instance.GetPopup(name);
			if (popup != null)
			{
				return popup;
			}
		}
		return null;
	}
}
