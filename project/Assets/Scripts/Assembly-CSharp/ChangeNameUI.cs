using EB;
using EB.Sparx;
using UnityEngine;

public class ChangeNameUI : PopupScript
{
	public string Email;

	public string Password;

	private bool shouldPay;

	private string oldUserName;

	private string Name
	{
		get
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
			UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch);
			return uIInput.text;
		}
		set
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
			UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch);
			uIInput.text = value;
		}
	}

	public bool InputHandlingEnabled
	{
		set
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
			Collider collider = Util.FindComponent<Collider>(objectExactMatch);
			collider.enabled = value;
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonDone");
			collider = Util.FindComponent<Collider>(objectExactMatch2);
			collider.enabled = value;
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
			collider = Util.FindComponent<Collider>(objectExactMatch3);
			collider.enabled = value;
		}
	}

	private bool canPurchase
	{
		get
		{
			return Profile.Instance.HardCurrency >= Tuning.Instance.NameChangeCost;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
		UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch);
		uIInput.validator = UIUtils.TextValidatorForName;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonDone");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch2);
		buttonScript.onButtonClick += OnSelect;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "ButtonHardCurrency");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch3);
		UIUtils.SetLabelContents(objectExactMatch3, "LabelAmount", Tuning.Instance.NameChangeCost.ToString());
		buttonScript.onButtonClick += OnPurchase;
		oldUserName = Hub.Instance.LoginManager.LocalUser.Name;
		if (oldUserName == Localizer.GetString("ID_UI_HOME_DEFAULT_NAME") || string.IsNullOrEmpty(Hub.Instance.LoginManager.LocalUser.Name))
		{
			objectExactMatch3.SetActive(false);
		}
		else
		{
			objectExactMatch2.SetActive(false);
		}
		if (string.IsNullOrEmpty(Hub.Instance.LoginManager.LocalUser.Name))
		{
			objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
			objectExactMatch2.SetActive(false);
		}
		if (Hub.Instance.LoginManager.LocalUser != null && !string.IsNullOrEmpty(Hub.Instance.LoginManager.LocalUser.Name))
		{
			Name = ValidateName(Hub.Instance.LoginManager.LocalUser.Name);
		}
	}

	public void OnSelect(GameObject caller)
	{
		InputHandlingEnabled = false;
		Hub.Instance.LoginManager.SelectName(Name);
		Profile.Instance.recentRename = true;
	}

	public void OnPurchase(GameObject caller)
	{
		if (Name == oldUserName)
		{
			InputHandlingEnabled = false;
			Hub.Instance.LoginManager.SelectName(Name);
			Profile.Instance.recentRename = true;
		}
		else if (canPurchase)
		{
			shouldPay = true;
			InputHandlingEnabled = false;
			Hub.Instance.LoginManager.SelectName(Name);
			Profile.Instance.recentRename = true;
		}
		else if (!canPurchase)
		{
			InsufficientCurrency(CurrencyType.Hard, Tuning.Instance.NameChangeCost);
		}
	}

	private void InsufficientCurrency(CurrencyType currencyType, int cost)
	{
		ScreenManager.Instance.ClosePopup("BlockingUI");
		GameObject gameObject = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true, true);
		if (gameObject != null)
		{
			CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(gameObject);
			currencyInsufficientUI.SetInfo(currencyType, cost);
		}
	}

	public void OnUserClose(GameObject caller)
	{
		ClosePopup();
	}

	private void ReportLoginError(string msg)
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
		ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
		confirmationDialogUI.SetParameters(false, msg, "ID_UI_LOGIN_ERROR_TITLE", "ID_UI_COMMON_OK", string.Empty);
		confirmationDialogUI.callback = OnLoginErrorClosed;
		shouldPay = false;
	}

	protected override void OnClose()
	{
		if (shouldPay)
		{
			WalletListener.Instance.Debit(Tuning.Instance.NameChangeCost, "Increase Energy Limit", SafeAction.Wrap(this, (bool result) =>
			{
				if (result)
				{
					base.OnClose();
				}
				else
				{
					InsufficientCurrency(CurrencyType.Hard, Tuning.Instance.NameChangeCost);
				}
			}));
		}
		else
		{
			base.OnClose();
		}
	}

	private void OnLoginErrorClosed(ConfirmationDialogUI.ButtonIds b)
	{
		InputHandlingEnabled = true;
	}

	private string ValidateName(string text)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
		UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch);
		if (uIInput.validator != null)
		{
			string text2 = string.Empty;
			for (int i = 0; i < text.Length; i++)
			{
				char nextChar = text[i];
				nextChar = uIInput.validator(text2, nextChar);
				if (nextChar != '\0')
				{
					text2 += nextChar;
				}
			}
			return text2;
		}
		return text;
	}
}
