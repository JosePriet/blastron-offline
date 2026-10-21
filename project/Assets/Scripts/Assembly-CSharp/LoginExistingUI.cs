using System;
using EB;
using EB.Sparx;
using UnityEngine;

public class LoginExistingUI : PopupScript
{
	public Color disabledButtonTint;

	private string nextScreen = string.Empty;

	private string email = string.Empty;

	private string password = string.Empty;

	private Color defaultLoginButtonTint;

	private Color defaultForgotPasswordButtonTint;

	private UISlicedSprite loginButtonBackground;

	private UISlicedSprite forgotPasswordButtonBackground;

	private bool loginButtonEnabled = true;

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
		EB.Debug.Log("> LoginExistingUI > canSkip > " + canSkip);
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
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonForgotPassword");
		forgotPasswordButtonBackground = Util.GetObjectExactMatch(objectExactMatch, "Background").GetComponent<UISlicedSprite>();
		defaultForgotPasswordButtonTint = forgotPasswordButtonBackground.color;
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnForgotPassword;
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
		EB.Debug.Log("> LoginExistingUI > OnLogin > sending info to server ...");
		Hub.Instance.LoginManager.LoginEmail(email, password);
	}

	private void OnForgotPassword(GameObject caller)
	{
		if (!string.IsNullOrEmpty(email))
		{
			Hub.Instance.LoginManager.ForgotPassword(email);
		}
	}

	protected override void Close(GameObject go)
	{
		EB.Debug.Log("> LoginExistingUI > OnBack");
		nextScreen = "LoginSelectUI";
		base.Close(go);
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

	private bool HasValidData()
	{
		return !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password);
	}

	private void UpdateLoginButton()
	{
		bool flag = HasValidData();
		loginButtonEnabled = flag;
		loginButtonBackground.collider.enabled = loginButtonEnabled;
		loginButtonBackground.color = ((!loginButtonEnabled) ? disabledButtonTint : defaultLoginButtonTint);
		forgotPasswordButtonBackground.color = (string.IsNullOrEmpty(email) ? disabledButtonTint : defaultForgotPasswordButtonTint);
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
