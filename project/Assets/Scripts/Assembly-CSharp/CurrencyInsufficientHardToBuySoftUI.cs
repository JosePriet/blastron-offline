using System.Collections.Generic;
using EB;
using EB.IAP;
using EB.Sparx;
using UnityEngine;

public class CurrencyInsufficientHardToBuySoftUI : PopupScript
{
	private int missingHardCurrency;

	private StoreCurrencyUI.PurchaseOption displayedOffer;

	private bool offerAccepted;

	private bool Loading
	{
		set
		{
			Util.GetObject(base.gameObject, "Loading", true).SetActive(value);
			Util.GetObject(base.gameObject, "DoneLoading", true).SetActive(!value);
		}
	}

	public void SetParameters(int missingHardCurrency, int purchaseRivetsQuantity)
	{
		this.missingHardCurrency = missingHardCurrency;
		string labelText = Localizer.Format("ID_UI_CURRENCY_INSUFFICIENT_HARD_TO_BUY_SOFT", missingHardCurrency, purchaseRivetsQuantity);
		UIUtils.SetLabelContents(base.gameObject, "LabelBody", labelText);
		FetchOffers();
	}

	private void FetchOffers()
	{
		Loading = true;
		Hub.Instance.WalletManager.FetchOffers();
	}

	public void OnOffersFetched()
	{
		Loading = false;
		List<StoreCurrencyUI.PurchaseOption> list = new List<StoreCurrencyUI.PurchaseOption>();
		Item[] payouts = Hub.Instance.WalletManager.Payouts;
		Item[] array = payouts;
		foreach (Item item in array)
		{
			if (item.type == ItemType.Consumable && item.value >= missingHardCurrency)
			{
				StoreCurrencyUI.PurchaseOption purchaseOption = new StoreCurrencyUI.PurchaseOption();
				purchaseOption.price = item.localizedCost;
				purchaseOption.currency = CurrencyType.Hard;
				purchaseOption.quantity = item.value;
				purchaseOption.notes = Dot.String("notes", item.info, string.Empty);
				purchaseOption.offer = item;
				list.Add(purchaseOption);
			}
		}
		list.Sort(CompareOffers);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Offer");
		StoreCurrencyInteractive storeCurrencyInteractive = Util.FindComponent<StoreCurrencyInteractive>(objectExactMatch);
		if (list.Count >= 1)
		{
			displayedOffer = list[0];
			objectExactMatch.SetActive(true);
			storeCurrencyInteractive.purchaseOption = displayedOffer;
		}
		else
		{
			objectExactMatch.SetActive(false);
		}
	}

	private static int CompareOffers(StoreCurrencyUI.PurchaseOption option1, StoreCurrencyUI.PurchaseOption option2)
	{
		return option1.quantity - option2.quantity;
	}

	public void OnBackButtonPressed(GameObject caller)
	{
		ScreenManager.Instance.ClosePopup();
	}

	public void OnAcceptOffer(GameObject caller)
	{
		offerAccepted = true;
		ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
		Hub.Instance.WalletManager.PurchaseOffer(displayedOffer.offer);
	}

	private void OnOfferPurchaseFailed(string error)
	{
		if (offerAccepted)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
			string text = error;
			text = text ?? "ID_UI_TRANSACTION_FAILED_UNKOWN";
			ScreenManager.Instance.Confirmation(false, "ID_UI_TRANSACTION_FAILED_TITLE", text, "ID_UI_COMMON_OK", string.Empty);
		}
	}

	private void OnOfferPurchaseSucceeded()
	{
		if (offerAccepted)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
			Item offer = displayedOffer.offer;
			string desc = Localizer.Format((displayedOffer.currency != CurrencyType.Soft) ? "ID_UI_CURRENCY_BOUGHT_OFFER_HARD" : "ID_UI_CURRENCY_BOUGHT_OFFER_SOFT", offer.value);
			ScreenManager.Instance.Confirmation(false, "ID_UI_CURRENCY_BOUGHT_OFFER_TITLE", desc, "ID_UI_COMMON_OK", string.Empty);
		}
	}

	private void OnOfferPurchaseCanceled()
	{
		if (offerAccepted)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
			ScreenManager.Instance.Confirmation(false, "ID_UI_TRANSACTION_CANCELED_TITLE", "ID_UI_TRANSACTION_CANCELED_BODY", "ID_UI_COMMON_OK", string.Empty);
		}
	}
}
