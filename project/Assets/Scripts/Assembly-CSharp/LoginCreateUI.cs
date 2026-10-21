using System;
using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class LoginCreateUI : PopupScript
{
	public Color disabledButtonTint;

	private string nextScreen = string.Empty;

	private Color defaultLoginButtonTint;

	private UISlicedSprite loginButtonBackground;

	private bool loginButtonEnabled = true;

	private string email = string.Empty;

	private string password = string.Empty;

	private string passwordRepeat = string.Empty;

	private Hashtable _loginData;

	public void SetEmail(string email)
	{
		EB.Debug.LogWarning("Why is the server trying to set the user's email on a screen where we're asking the user for their input?");
	}

	public void SetPassword(string password)
	{
		EB.Debug.LogWarning("Why is the server trying to set the user's password on a screen where we're asking the user for their input?");
	}

	public void SetCanSkip(bool canSkip)
	{
		EB.Debug.Log("> LoginCreateUI > canSkip > " + canSkip);
	}

	public void SetData(Hashtable data)
	{
		_loginData = data;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Email");
		UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch);
		string text = Dot.String("email", _loginData, string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			email = text;
			uIInput.text = text;
			uIInput.enabled = false;
		}
		else
		{
			email = string.Empty;
			uIInput.text = email;
			uIInput.enabled = true;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonLogin");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnLogin;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Background");
		loginButtonBackground = Util.FindComponent<UISlicedSprite>(objectExactMatch2);
		defaultLoginButtonTint = loginButtonBackground.color;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Input_Email");
		UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch3);
		UIInput uIInput2 = uIInput;
		uIInput2.onSubmit = (UIInput.OnSubmit)Delegate.Combine(uIInput2.onSubmit, new UIInput.OnSubmit(OnSubmitEmail));
		uIInput.validator = UIUtils.TextValidatorForEmail;
		objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Input_Password");
		uIInput = Util.FindComponent<UIInput>(objectExactMatch3);
		UIInput uIInput3 = uIInput;
		uIInput3.onSubmit = (UIInput.OnSubmit)Delegate.Combine(uIInput3.onSubmit, new UIInput.OnSubmit(OnSubmitPassword));
		uIInput.validator = UIUtils.TextValidatorForPassword;
		objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Input_PasswordRepeat");
		uIInput = Util.FindComponent<UIInput>(objectExactMatch3);
		UIInput uIInput4 = uIInput;
		uIInput4.onSubmit = (UIInput.OnSubmit)Delegate.Combine(uIInput4.onSubmit, new UIInput.OnSubmit(OnSubmitPasswordRepeat));
		uIInput.validator = UIUtils.TextValidatorForPassword;
		UpdateLoginButton();
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (!string.IsNullOrEmpty(nextScreen))
		{
			ScreenManager.Instance.OpenPopup(nextScreen, true, true);
		}
	}

	private void OnLogin(GameObject caller)
	{
		EB.Debug.Log("> LoginCreateUI > OnLogin > sending info to server ...");
		Hub.Instance.LoginManager.CreateAccount(email, password, passwordRepeat, _loginData);
	}

	protected override void Close(GameObject go)
	{
		EB.Debug.Log("> LoginCreateUI > OnBack");
		nextScreen = "LoginSelectUI";
		base.Close(go);
	}

	private bool HasValidData()
	{
		return !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(passwordRepeat);
	}

	private void ReportLoginError(string msg)
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
		ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
		confirmationDialogUI.SetParameters(false, msg, "ID_UI_LOGIN_ERROR_TITLE", "ID_UI_COMMON_OK", string.Empty);
	}

	private void OnConfirmationDialogUIClosed(ConfirmationDialogUI.ButtonIds buttonPressed)
	{
	}

	private void OnSubmitEmail(string text)
	{
		email = text;
		UpdateLoginButton();
	}

	private void OnSubmitPassword(string text)
	{
		password = text;
		UpdateLoginButton();
	}

	private void OnSubmitPasswordRepeat(string text)
	{
		passwordRepeat = text;
		UpdateLoginButton();
	}

	private void UpdateLoginButton()
	{
		bool flag = HasValidData();
		loginButtonEnabled = flag;
		loginButtonBackground.collider.enabled = loginButtonEnabled;
		loginButtonBackground.color = ((!loginButtonEnabled) ? disabledButtonTint : defaultLoginButtonTint);
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
