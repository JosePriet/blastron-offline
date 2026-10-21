using EB;
using UnityEngine;

public class CurrencySelectUI : PopupScript
{
	private void Awake()
	{
		Util.GetObjectExactMatch(base.gameObject, "HardButton").GetComponent<ButtonScript>().onButtonClick += OnHardCurrencyPressed;
		Util.GetObjectExactMatch(base.gameObject, "SoftButton").GetComponent<ButtonScript>().onButtonClick += OnSoftCurrencyPressed;
	}

	public void OnBackButtonPressed(GameObject caller)
	{
		ScreenManager.Instance.ClosePopup();
	}

	public void OnSoftCurrencyPressed(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoSoftCurrency);
	}

	private void GotoSoftCurrency()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", false, true);
		StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
		storeCurrencyUI.currencyType = CurrencyType.Soft;
	}

	public void OnHardCurrencyPressed(GameObject caller)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, GotoHardCurrency);
	}

	private void GotoHardCurrency()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", false, true);
		StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
		storeCurrencyUI.currencyType = CurrencyType.Hard;
	}
}
