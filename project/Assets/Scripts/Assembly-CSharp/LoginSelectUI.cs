using EB;
using EB.Sparx;
using UnityEngine;

public class LoginSelectUI : PopupScript
{
	private string email;

	private string password;

	private string nextScreen = string.Empty;

	public void SetEmail(string email)
	{
		this.email = email;
	}

	public void SetPassword(string password)
	{
		this.password = password;
	}

	public void SetCanSkip(bool canSkip)
	{
		EB.Debug.Log("> LoginSelectUI > canSkip > " + canSkip);
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (nextScreen == "guest")
		{
			Hub.Instance.LoginManager.LoginGuest();
		}
		else if (nextScreen == "facebook")
		{
			Hub.Instance.LoginManager.LoginFacebook();
		}
		else if (!string.IsNullOrEmpty(nextScreen))
		{
			ScreenManager.Instance.OpenPopup(nextScreen, true, true);
		}
		else if (Hub.Instance.State == HubState.Idle)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonCreate");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnCreate;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonLogin");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnLogin;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonSkip");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnSkip;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonFacebook");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnFacebook;
	}

	private void OnCreate(GameObject caller)
	{
		nextScreen = "LoginCreateUI";
		ClosePopup();
	}

	private void OnLogin(GameObject caller)
	{
		nextScreen = "LoginExistingUI";
		ClosePopup();
	}

	private void OnSkip(GameObject caller)
	{
		nextScreen = "guest";
		ClosePopup();
	}

	public void OnFacebook(GameObject go)
	{
		nextScreen = "facebook";
		ClosePopup();
	}

	protected override void OnClose()
	{
		GameObject popup = ScreenManager.Instance.GetPopup("OptionsUI");
		if (popup != null)
		{
			popup.SendMessage("OnChildScreenOSBackButtonPressed");
		}
		base.OnClose();
	}
}
