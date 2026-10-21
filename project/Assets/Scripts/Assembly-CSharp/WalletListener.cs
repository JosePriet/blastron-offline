using System.Collections;
using System.Collections.Generic;
using EB;
using EB.IAP;
using EB.Sparx;
using UnityEngine;

public class WalletListener : EB.Sparx.WalletListener
{
	private Dictionary<int, Action<bool>> _callbacks = new Dictionary<int, Action<bool>>();

	public static WalletListener Instance { get; set; }

	public WalletListener()
	{
		Instance = this;
	}

	public void PurchaseSoftWithHard(Item offer)
	{
		int key = Hub.Instance.WalletManager.Debit((int)offer.cost, "puchase soft " + offer.productId);
		_callbacks[key] = (bool result) =>
		{
			if (!result)
			{
				ScreenManager.Instance.BroadcastMessage("OnOfferPurchaseFailed", string.Empty);
			}
			else
			{
				Profile.Instance.AddSoftCurrency(offer.value);
				Profile.Instance.Save();
				ScreenManager.Instance.BroadcastMessage("OnOfferPurchaseSucceeded");
			}
		};
	}

	public void Debit(int value, string reason, Action<bool> cb)
	{
		value = Mathf.Max(0, value);
		if (value == 0)
		{
			cb(true);
		}
		else
		{
			Coroutines.Run(DoDebit(value, reason, cb));
		}
	}

	private IEnumerator DoDebit(int value, string reason, Action<bool> cb)
	{
		int id = Hub.Instance.WalletManager.Debit(value, reason);
		bool result = false;
		bool done = false;
		float wait = UnityEngine.Time.realtimeSinceStartup + 0.6f;
		int value2 = default(int);
		_callbacks[id] = (bool r) =>
		{
			result = r;
			done = true;
			if (r)
			{
				Profile.Instance.SpentHardCurrency(value2);
			}
		};
		while (!done || UnityEngine.Time.realtimeSinceStartup < wait)
		{
			yield return 1;
		}
		cb(result);
	}

	public void OnBalanceUpdated(int balance)
	{
	}

	public void OnDebitFailed(int id)
	{
		Action<bool> value;
		if (_callbacks.TryGetValue(id, out value))
		{
			_callbacks.Remove(id);
			value(false);
		}
	}

	public void OnDebitSuceeded(int id)
	{
		Action<bool> value;
		if (_callbacks.TryGetValue(id, out value))
		{
			_callbacks.Remove(id);
			value(true);
		}
	}

	public void OnCreditSuceeded(int id)
	{
	}

	public void OnOffersFetched()
	{
		ScreenManager.Instance.BroadcastMessage("OnOffersFetched", SendMessageOptions.DontRequireReceiver);
	}

	public void OnOfferPurchaseFailed(string error)
	{
		ScreenManager.Instance.BroadcastMessage("OnOfferPurchaseFailed", error);
	}

	public void OnOfferPurchaseSuceeded(Item item)
	{
		ScreenManager.Instance.BroadcastMessage("OnOfferPurchaseSucceeded");
	}

	public void OnOfferPurchaseCanceled()
	{
		ScreenManager.Instance.BroadcastMessage("OnOfferPurchaseCanceled");
	}
}
