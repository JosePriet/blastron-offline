using System.Collections;
using EB;
using UnityEngine;

public class StoreCurrencyInteractive : MonoBehaviour
{
	private StoreCurrencyUI.PurchaseOption _purchaseOption;

	private UITexture image;

	private string texturePath;

	public StoreCurrencyUI.PurchaseOption purchaseOption
	{
		get
		{
			return _purchaseOption;
		}
		set
		{
			_purchaseOption = value;
			Refresh();
		}
	}

	public void Refresh()
	{
		bool flag = purchaseOption.currency == CurrencyType.Hard;
		Util.GetObjectExactMatch(base.gameObject, "SpriteCurrencySoft").SetActive(!flag);
		Util.GetObjectExactMatch(base.gameObject, "ItemTexture").SetActive(flag);
		Util.GetObjectExactMatch(base.gameObject, "SpriteCurrencyHardForSoft").SetActive(!flag);
		UIUtils.SetLabelContents(base.gameObject, "LabelItemQuantity", Localizer.Format((!flag) ? "ID_UI_CURRENCY_STORE_SOFT_QUANTITY" : "ID_UI_CURRENCY_STORE_HARD_QUANTITY", purchaseOption.quantity));
		UIUtils.SetLabelContents(base.gameObject, "LabelItemDescription", Localizer.Format((!flag) ? "ID_UI_CURRENCY_STORE_SOFT_DESCRIPTION" : "ID_UI_CURRENCY_STORE_HARD_DESCRIPTION"));
		string labelText = string.Empty;
		ArrayList arrayList = Dot.Array("catalogItems", _purchaseOption.offer.info, new ArrayList());
		if (arrayList.Count > 0)
		{
			UnityEngine.Debug.LogWarning("have a catalog item! " + arrayList[0].ToString());
			Hashtable hashtable = arrayList[0] as Hashtable;
			int result = 0;
			int.TryParse(hashtable["itemId"].ToString(), out result);
			InventoryDB.MetaData metaDataFromServerId = InventoryDB.GetMetaDataFromServerId(result);
			string text = string.Empty;
			if (metaDataFromServerId != null)
			{
				text = metaDataFromServerId.friendlyName;
			}
			string s = hashtable["numOfItems"].ToString();
			int result2 = 0;
			int.TryParse(s, out result2);
			if (result2 > 0 && !string.IsNullOrEmpty(text))
			{
				string text2 = result2.ToString();
				if (result2 > 99999)
				{
					text2 = "infinite";
				}
				labelText = Localizer.Format("ID_UI_CURRENCY_BONUS_ITEM", text2, text);
			}
		}
		UIUtils.SetLabelContents(base.gameObject, "LabelItemBonusItems", labelText);
		string labelText2 = string.Empty;
		int num = Dot.Integer("igcBonus", _purchaseOption.offer.info, 0);
		if (num > 0)
		{
			labelText2 = Localizer.Format("ID_UI_CURRENCY_BONUS_HARD", num);
		}
		UIUtils.SetLabelContents(base.gameObject, "LabelItemBonusCurrency", labelText2);
		UIUtils.SetLabelContents(base.gameObject, "LabelItemCostCurrencyHard", (!flag) ? string.Empty : purchaseOption.price);
		UIUtils.SetLabelContents(base.gameObject, "LabelItemCostCurrencySoft", (!flag) ? purchaseOption.price : string.Empty);
		bool flag2 = purchaseOption.notes.Length > 0;
		UIUtils.SetLabelContents(base.gameObject, "LabelItemGeneralNote", purchaseOption.notes);
		Util.GetObjectExactMatch(base.gameObject, "BackgroundGeneralNote").SetActive(flag2);
		bool flag3 = !flag2 && purchaseOption.isBestOffer && !purchaseOption.isPopular;
		bool flag4 = !flag2 && purchaseOption.isPopular;
		Util.GetObjectExactMatch(base.gameObject, "LabelItemBestValue1").SetActive(flag4 || flag3);
		Util.GetObjectExactMatch(base.gameObject, "LabelItemBestValue2").SetActive(flag4 || flag3);
		Util.GetObjectExactMatch(base.gameObject, "BackgroundBestValue").SetActive(flag3);
		Util.GetObjectExactMatch(base.gameObject, "BackgroundPopular").SetActive(flag4);
		if (flag3)
		{
			UIUtils.SetLabelContents(base.gameObject, "LabelItemBestValue", "ID_UI_CURRENCY_STORE_BEST_VALUE");
		}
		else if (flag4)
		{
			UIUtils.SetLabelContents(base.gameObject, "LabelItemBestValue1", "ID_UI_CURRENCY_STORE_POPULAR1");
			UIUtils.SetLabelContents(base.gameObject, "LabelItemBestValue2", "ID_UI_CURRENCY_STORE_POPULAR2");
		}
		UIUtils.SetLabelContents(base.gameObject, "LabelName", _purchaseOption.name);
		texturePath = "currency_ui/UI_currency_hard_" + _purchaseOption.iconIndex;
		image = Util.GetObjectExactMatch(base.gameObject, "ItemTexture").GetComponent<UITexture>();
		TexturePoolManager.Instance.LoadTexture(texturePath, OnTextureLoaded);
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null)
		{
			image.mainTexture = t;
			image.color = Color.white;
			image.MakePixelPerfect();
		}
	}

	private void OnDestroy()
	{
		TexturePoolManager.Instance.ReleaseTexture(texturePath);
	}
}
