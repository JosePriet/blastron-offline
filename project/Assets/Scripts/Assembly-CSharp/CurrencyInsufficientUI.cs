using EB;
using UnityEngine;

public class CurrencyInsufficientUI : PopupScript
{
	private const string TitlePrefix = "ID_UI_CURRENCY_INSUFFICIENT_TITLE_";

	private const string BodyPrefix = "ID_UI_CURRENCY_INSUFFICIENT_BODY_";

	private CurrencyType currencyType;

	protected override void OnStart()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonBuyMore");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnBuyMorePressed;
	}

	public void SetInfo(CurrencyType currencyType, int attemptedPurchaseCost)
	{
		this.currencyType = currencyType;
		UIUtils.SetLabelContents(base.gameObject, "LabelTitle", "ID_UI_CURRENCY_INSUFFICIENT_TITLE_" + currencyType.ToString().ToUpper());
		UIUtils.SetLabelContents(base.gameObject, "LabelBody", "ID_UI_CURRENCY_INSUFFICIENT_BODY_" + currencyType.ToString().ToUpper());
	}

	public void OnBuyMorePressed(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnTransitionOutToCurrencyStore);
	}

	private void OnTransitionOutToCurrencyStore()
	{
		base.OnTransitionOutComplete();
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", true, true);
		StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
		storeCurrencyUI.currencyType = currencyType;
	}
}
