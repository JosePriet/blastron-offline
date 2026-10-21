using System;
using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class LoginMergeUI : PopupScript
{
	public Color disabledButtonTint;

	private string nextScreen = string.Empty;

	private string password = string.Empty;

	private Color defaultLoginButtonTint;

	private UISlicedSprite loginButtonBackground;

	private bool loginButtonEnabled = true;

	private string _email = string.Empty;

	private Hashtable _data;

	public void SetData(Hashtable data)
	{
		_data = data;
		_email = Dot.String("email", data, string.Empty);
		UIUtils.SetLabelContents(base.gameObject, "Input_Email", _email);
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
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnForgotPassword;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Input_Password");
		UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch3);
		uIInput.onSubmit = (UIInput.OnSubmit)Delegate.Combine(uIInput.onSubmit, new UIInput.OnSubmit(OnSubmitPassword));
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
		Hub.Instance.LoginManager.MergeAccount(_email, password, _data);
	}

	private void OnForgotPassword(GameObject caller)
	{
		Hub.Instance.LoginManager.ForgotPassword(_email);
	}

	protected override void Close(GameObject go)
	{
		EB.Debug.Log("> LoginExistingUI > OnBack");
		nextScreen = "LoginSelectUI";
		base.Close(go);
	}

	private void OnSubmitPassword(string text)
	{
		password = text;
		UpdateLoginButton();
	}

	private bool HasValidData()
	{
		return !string.IsNullOrEmpty(password);
	}

	private void UpdateLoginButton()
	{
		bool flag = HasValidData();
		if (flag != loginButtonEnabled)
		{
			loginButtonEnabled = flag;
			loginButtonBackground.color = ((!loginButtonEnabled) ? disabledButtonTint : defaultLoginButtonTint);
			loginButtonBackground.collider.enabled = loginButtonEnabled;
		}
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
}
