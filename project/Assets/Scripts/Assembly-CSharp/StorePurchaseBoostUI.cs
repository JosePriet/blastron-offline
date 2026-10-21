using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class StorePurchaseBoostUI : MonoBehaviour
{
	private InventoryDB.MetaData itemMetaData;

	public void SetParameters(InventoryDB.MetaData metadata)
	{
		itemMetaData = metadata;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemTexture");
		if (objectExactMatch != null)
		{
		}
		UIUtils.SetLabelContents(base.gameObject, "ItemName", metadata.name);
		UIUtils.SetLabelContents(base.gameObject, "Count", Hub.Instance.InventoryManager.GetCount(metadata.name).ToString());
		UIUtils.SetLabelContents(base.gameObject, "SoftCurrencyCount", "9999");
		UIUtils.SetLabelContents(base.gameObject, "HardCurrencyCount", "19");
		UIUtils.SetLabelContents(base.gameObject, "ButtonSoftCurrency", "9999");
		UIUtils.SetLabelContents(base.gameObject, "ButtonHardCurrency", "19");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonHardCurrency");
		UIUtils.CenterSpriteAndLabel(objectExactMatch2, "SpriteCurrencyIcon", "Label");
		objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonSoftCurrency");
		UIUtils.CenterSpriteAndLabel(objectExactMatch2, "SpriteCurrencyIcon", "Label");
	}

	private void OnBack()
	{
		ScreenManager.Instance.EnableBgFade(false);
		ScreenManager.Instance.ClosePopup();
	}

	private void OnPurchaseWithHard(GameObject caller)
	{
		EB.Debug.Log("Purchase " + itemMetaData.name + " for " + itemMetaData.hardCost + " hard currency ...");
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("PLEASE WAIT", "Transaction in progress ...");
		Hashtable hashtable = new Hashtable();
		hashtable.Add(itemMetaData.name, 1);
		InventoryListener.PurchaseHard(hashtable, "boost", itemMetaData.hardCost, OnTransactionComplete);
	}

	private void OnPurchaseWithSoft(GameObject caller)
	{
		EB.Debug.Log("Purchase " + itemMetaData.name + " for " + itemMetaData.softCost + " soft currency ...");
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("PLEASE WAIT", "Transaction in progress ...");
		Hashtable hashtable = new Hashtable();
		hashtable.Add(itemMetaData.name, 1);
		InventoryListener.PurchaseSoft(hashtable, "boost", itemMetaData.softCost, OnTransactionComplete);
	}

	private void OnTransactionComplete(InventoryListener.PurchaseResult result)
	{
		ScreenManager.Instance.EnableBgFade(false);
		ScreenManager.Instance.ClosePopup();
	}
}
