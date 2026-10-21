using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.IAP;
using EB.Sparx;
using UnityEngine;

public class StoreCurrencyUI : PopupScript
{
	[System.Serializable]
	public class PurchaseOption
	{
		public int quantity;

		public string price;

		public CurrencyType currency;

		public string notes;

		public bool isBestOffer;

		public bool isPopular;

		public string name;

		public int iconIndex;

		public Item offer;
	}

	private const string TitlePrefix = "ID_UI_CURRENCY_STORE_TITLE_";

	public GameObject itemPrefab;

	private PurchaseOption chosenPurchaseOption;

	private TableHelperUI tableHelper;

	private bool dragging;

	private UIGrid grid;

	private UILabel beginnerSaleTimer;

	private UILabel beginnerSaleEndsIn;

	private UILabel beginnerSaleExpired;

	private bool bShowBeginnerSaleBanner;

	private string chestTexturePath = "currency_ui/beginner_chest_2.png";

	public BeginnerSaleRewardsUI.beginnerSaleData begginerSalePopupData = new BeginnerSaleRewardsUI.beginnerSaleData();

	private bool _loading;

	private CurrencyType _currencyType;

	private bool reprocessResults;

	private bool Loading
	{
		get
		{
			return _loading;
		}
		set
		{
			if (value != _loading)
			{
				if (value)
				{
					ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_LOADING");
				}
				else if (ScreenManager.Instance.GetPopup("BlockingUI") != null)
				{
					ScreenManager.Instance.ClosePopup("BlockingUI");
				}
				_loading = value;
			}
		}
	}

	public CurrencyType currencyType
	{
		get
		{
			return _currencyType;
		}
		set
		{
			_currencyType = value;
			UIUtils.SetLabelContents(base.gameObject, "LabelTitle", "ID_UI_CURRENCY_STORE_TITLE_" + _currencyType.ToString().ToUpper());
		}
	}

	protected override void OnAwake()
	{
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		tableHelper.SetMaxVisibleItems(6);
		tableHelper.SetDelta(2);
		grid = Util.FindComponent<UIGrid>(base.gameObject);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BeginnerSaleBanner");
		if (Profile.Instance.HasBeginnerSaleStarted() && !Profile.Instance.HasBeginnerSaleExpired())
		{
			bShowBeginnerSaleBanner = true;
			objectExactMatch.SetActive(true);
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Note");
			beginnerSaleTimer = Util.GetObjectExactMatch(objectExactMatch2, "SaleTimer").GetComponent<UILabel>();
			beginnerSaleEndsIn = Util.GetObjectExactMatch(objectExactMatch2, "SaleEndsIn").GetComponent<UILabel>();
			beginnerSaleExpired = Util.GetObjectExactMatch(objectExactMatch2, "SaleExpired").GetComponent<UILabel>();
			beginnerSaleExpired.enabled = false;
			UITexture bannerChest = Util.FindComponent<UITexture>(objectExactMatch);
			TexturePoolManager.Instance.LoadTexture(chestTexturePath, SafeAction.Wrap(this, (Texture2D t) =>
			{
				bannerChest.mainTexture = t;
				bannerChest.MakePixelPerfect();
				Vector3 localScale = bannerChest.transform.localScale;
				localScale.x = 216f;
				localScale.y = 216f;
				bannerChest.transform.localScale = localScale;
			}));
		}
		else
		{
			objectExactMatch.SetActive(false);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (bShowBeginnerSaleBanner)
		{
			Vector3 localPosition = base.gameObject.transform.localPosition;
			localPosition.y -= 20f;
			base.gameObject.transform.localPosition = localPosition;
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CurrencyBar");
			localPosition = objectExactMatch.transform.localPosition;
			localPosition.y += 110f;
			objectExactMatch.transform.localPosition = localPosition;
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "PopupBackground");
			localPosition = objectExactMatch2.transform.localPosition;
			localPosition.y += 110f;
			localPosition.z += 20f;
			objectExactMatch2.transform.localPosition = localPosition;
			localPosition = objectExactMatch2.transform.localScale;
			localPosition.y += 110f;
			objectExactMatch2.transform.localScale = localPosition;
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
			localPosition = objectExactMatch3.transform.localPosition;
			localPosition.y += 110f;
			objectExactMatch3.transform.localPosition = localPosition;
		}
		Fetch();
	}

	private void RemoveBeginnerSaleBanner()
	{
		Vector3 localPosition = base.gameObject.transform.localPosition;
		localPosition.y += 20f;
		base.gameObject.transform.localPosition = localPosition;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CurrencyBar");
		localPosition = objectExactMatch.transform.localPosition;
		localPosition.y -= 110f;
		objectExactMatch.transform.localPosition = localPosition;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "PopupBackground");
		localPosition = objectExactMatch2.transform.localPosition;
		localPosition.y -= 110f;
		localPosition.z -= 20f;
		objectExactMatch2.transform.localPosition = localPosition;
		localPosition = objectExactMatch2.transform.localScale;
		localPosition.y -= 110f;
		objectExactMatch2.transform.localScale = localPosition;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
		localPosition = objectExactMatch3.transform.localPosition;
		localPosition.y -= 110f;
		objectExactMatch3.transform.localPosition = localPosition;
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "BeginnerSaleBanner");
		objectExactMatch4.SetActive(false);
	}

	protected override void OnUpdate()
	{
		if (dragging && !Input.GetMouseButton(0))
		{
			tableHelper.CenterOnCurrentItem();
			dragging = false;
		}
		if (reprocessResults)
		{
			reprocessResults = false;
			OnOffersFetched();
		}
		if (bShowBeginnerSaleBanner)
		{
			int num = Profile.Instance.UpdateBeginnerSaleTimeRemaining();
			if (num == 0 || Profile.Instance.HasBeginnerSaleExpired())
			{
				beginnerSaleEndsIn.enabled = false;
				beginnerSaleTimer.enabled = false;
				beginnerSaleExpired.enabled = true;
				bShowBeginnerSaleBanner = false;
			}
			else
			{
				beginnerSaleTimer.text = Localizer.FormatTime(num, true);
			}
		}
	}

	private void Fetch(bool forceFetch = false)
	{
		EB.Debug.Log(string.Format("[{0}] Requesting offers from server ...", UnityEngine.Time.frameCount));
		Loading = true;
		Hub.Instance.WalletManager.FetchOffers(forceFetch);
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		TexturePoolManager.Instance.ReleaseTexture(chestTexturePath);
	}

	private void OnOffersFetched()
	{
		if (grid == null)
		{
			reprocessResults = true;
			return;
		}
		WalletManager walletManager = Hub.Instance.WalletManager;
		Item[] payouts = walletManager.Payouts;
		EB.Debug.Log(string.Format("[{0}] Got callback with {1} offers.", UnityEngine.Time.frameCount, payouts.Length));
		if (payouts == null || payouts.Length < 1)
		{
			EB.Debug.Log("Fetch failed ...");
			return;
		}
		Loading = false;
		List<PurchaseOption> list = new List<PurchaseOption>();
		tableHelper.Clear();
		Item[] array = payouts;
		foreach (Item item in array)
		{
			if (!item.show || (Dot.Integer("visible", item.info, 0) != 1 && _currencyType == CurrencyType.Hard))
			{
				continue;
			}
			switch (_currencyType)
			{
			case CurrencyType.Hard:
				if (item.type != ItemType.Consumable)
				{
					continue;
				}
				break;
			case CurrencyType.Soft:
				if (item.type != ItemType.Soft)
				{
					continue;
				}
				break;
			}
			PurchaseOption purchaseOption = new PurchaseOption();
			purchaseOption.price = item.localizedCost;
			purchaseOption.currency = _currencyType;
			purchaseOption.quantity = item.value;
			purchaseOption.notes = Dot.String("notes", item.info, string.Empty);
			purchaseOption.name = Dot.String("longName", item.info, string.Empty);
			purchaseOption.offer = item;
			purchaseOption.iconIndex = Dot.Integer("art", item.info, 1);
			ArrayList arrayList = Dot.Array("catalogItems", item.info, new ArrayList());
			if (Profile.Instance.HasBeginnerSaleStarted() && !Profile.Instance.HasBeginnerSaleExpired())
			{
				for (int j = 0; j < arrayList.Count; j++)
				{
					Hashtable hashtable = arrayList[j] as Hashtable;
					int result = 0;
					int result2 = 0;
					int.TryParse(hashtable["itemId"].ToString(), out result);
					int.TryParse(hashtable["numOfItems"].ToString(), out result2);
					string message = hashtable["title"].ToString();
					UnityEngine.Debug.LogError(message);
					InventoryDB.MetaData metaDataFromServerId = InventoryDB.GetMetaDataFromServerId(result);
					if (metaDataFromServerId != null)
					{
						UnityEngine.Debug.LogError(metaDataFromServerId.friendlyName);
						if (!begginerSalePopupData.ItemsMetadata.Contains(metaDataFromServerId))
						{
							begginerSalePopupData.ItemsMetadata.Add(metaDataFromServerId);
							begginerSalePopupData.beginnerSaleAmmoCount.Add(result2);
						}
						continue;
					}
					string text = hashtable["title"].ToString();
					if (text.Contains("@"))
					{
						string[] array2 = text.Split('@');
						result2 = int.Parse(array2[1]);
						string text2 = array2[0];
						if (text2.ToLower() == "rocket")
						{
							text2 = "Rocket";
						}
						else if (text2.ToLower().Contains("teleport"))
						{
							text2 = "Teleport Grenade";
						}
						metaDataFromServerId = InventoryDB.GetMetaData(Hash.StringHash(text2));
						if (metaDataFromServerId != null && !begginerSalePopupData.ItemsMetadata.Contains(metaDataFromServerId))
						{
							begginerSalePopupData.ItemsMetadata.Add(metaDataFromServerId);
							begginerSalePopupData.beginnerSaleAmmoCount.Add(result2);
						}
					}
				}
			}
			bool isBestOffer = Dot.Integer("value", item.info, 0) == 1;
			purchaseOption.isBestOffer = isBestOffer;
			bool isPopular = Dot.Integer("popular", item.info, 0) == 1;
			purchaseOption.isPopular = isPopular;
			list.Add(purchaseOption);
			GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
			gameObject.name = string.Format("[{0}] {1} - {2} for '{3}'", list.Count - 1, "CurrencyOffer", purchaseOption.quantity, purchaseOption.price);
			StoreCurrencyInteractive storeCurrencyInteractive = Util.FindComponent<StoreCurrencyInteractive>(gameObject);
			storeCurrencyInteractive.purchaseOption = purchaseOption;
			tableHelper.AddItem(gameObject);
			GameObject objectExactMatch = Util.GetObjectExactMatch(gameObject, "GetMoreButton");
			objectExactMatch.GetComponent<ButtonScript>().onButtonClick += OnItemSelected;
		}
		Util.GetObjectExactMatch(base.gameObject, "ArrowButtonLeft").SetActive(list.Count > 6);
		Util.GetObjectExactMatch(base.gameObject, "ArrowButtonRight").SetActive(list.Count > 6);
		tableHelper.Reset();
		SpringPanel component = grid.GetComponent<SpringPanel>();
		if (component != null)
		{
			component.target = Vector3.zero;
		}
		ScreenManager.Instance.FadeOut(null);
		ScreenManager.Instance.EnableBgFade(true);
	}

	public void OnItemSelected(GameObject selectedItem)
	{
		PurchaseOption purchaseOption = Util.FindComponent<StoreCurrencyInteractive>(selectedItem.transform.parent.gameObject).purchaseOption;
		Item offer = purchaseOption.offer;
		chosenPurchaseOption = purchaseOption;
		if (purchaseOption.currency == CurrencyType.Hard)
		{
			ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
			Hub.Instance.WalletManager.PurchaseOffer(offer);
			return;
		}
		string desc = Localizer.Format("ID_UI_CURRENCY_PURCHASE_CONFIRM_DESC", offer.value, (int)offer.cost);
		ScreenManager.Instance.Confirmation(true, "ID_UI_CURRENCY_PURCHASE_CONFIRM_TILE", desc, "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", SafeAction.Wrap(this, (ConfirmationDialogUI.ButtonIds id) =>
		{
			if (id == ConfirmationDialogUI.ButtonIds.Left)
			{
				PurchaseSoftWithHard(offer);
			}
		}));
	}

	private void PurchaseSoftWithHard(Item offer)
	{
		ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
		WalletListener.Instance.PurchaseSoftWithHard(offer);
	}

	private void OnOfferPurchaseFailed(string error)
	{
		if (chosenPurchaseOption != null)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
			int balance = Hub.Instance.WalletManager.Balance;
			Item offer = chosenPurchaseOption.offer;
			if (chosenPurchaseOption.currency == CurrencyType.Soft && (float)balance < chosenPurchaseOption.offer.cost)
			{
				GameObject obj = ScreenManager.Instance.OpenPopup("CurrencyInsufficientHardToBuySoftUI", true, true);
				CurrencyInsufficientHardToBuySoftUI currencyInsufficientHardToBuySoftUI = Util.FindComponent<CurrencyInsufficientHardToBuySoftUI>(obj);
				currencyInsufficientHardToBuySoftUI.SetParameters((int)offer.cost - balance, offer.value);
			}
			else
			{
				string text = error;
				text = text ?? "ID_UI_TRANSACTION_FAILED_UNKOWN";
				ScreenManager.Instance.Confirmation(false, "ID_UI_TRANSACTION_FAILED_TITLE", text, "ID_UI_COMMON_OK", string.Empty);
			}
			chosenPurchaseOption = null;
		}
	}

	private void OnOfferPurchaseSucceeded()
	{
		if (chosenPurchaseOption != null)
		{
			ScreenManager.Instance.ClosePopup("BlockingUI");
			Item offer = chosenPurchaseOption.offer;
			bool flag = chosenPurchaseOption.currency == CurrencyType.Soft;
			if (!flag && Profile.Instance.HasBeginnerSaleStarted() && !Profile.Instance.HasBeginnerSaleExpired())
			{
				GameObject gameObject = ScreenManager.Instance.OpenPopup("BeginnerSaleRewardsUI", true, true);
				gameObject.GetComponent<BeginnerSaleRewardsUI>().SetParams(begginerSalePopupData, chosenPurchaseOption);
			}
			else
			{
				string desc = Localizer.Format((!flag) ? "ID_UI_CURRENCY_BOUGHT_OFFER_HARD" : "ID_UI_CURRENCY_BOUGHT_OFFER_SOFT", offer.value);
				ScreenManager.Instance.Confirmation(false, "ID_UI_CURRENCY_BOUGHT_OFFER_TITLE", desc, "ID_UI_COMMON_OK", string.Empty);
			}
			if (!flag && bShowBeginnerSaleBanner)
			{
				Profile.Instance.ExpireBeginnerSale();
				RemoveBeginnerSaleBanner();
				bShowBeginnerSaleBanner = false;
				Fetch(true);
			}
			chosenPurchaseOption = null;
		}
	}

	public void OnDebugBeginnersSale()
	{
		GameObject obj = null;
		if (chosenPurchaseOption == null)
		{
			obj = Util.GetObjectExactMatch(base.gameObject, "Grid").transform.GetChild(0).gameObject;
		}
		PurchaseOption purchaseOption = Util.FindComponent<StoreCurrencyInteractive>(obj).purchaseOption;
		ScreenManager.Instance.ClosePopup("BlockingUI");
		int num = 0;
		bool flag = true;
		GameObject gameObject = ScreenManager.Instance.OpenPopup("BeginnerSaleRewardsUI", true, true);
		gameObject.GetComponent<BeginnerSaleRewardsUI>().SetParams(begginerSalePopupData, purchaseOption);
		Fetch(true);
		chosenPurchaseOption = null;
	}

	private void OnOfferPurchaseCanceled()
	{
		if (chosenPurchaseOption != null)
		{
			UnityEngine.Debug.LogError("cierrra popup");
			ScreenManager.Instance.ClosePopup("BlockingUI");
			ScreenManager.Instance.Confirmation(false, "ID_UI_TRANSACTION_CANCELED_TITLE", "ID_UI_TRANSACTION_CANCELED_BODY", "ID_UI_COMMON_OK", string.Empty);
			chosenPurchaseOption = null;
		}
	}

	private void OnClosePopup()
	{
		ScreenManager.Instance.EnableBgFade(false);
	}

	private void OnArrowLeft(GameObject caller)
	{
		tableHelper.OnScrollLeft();
	}

	private void OnArrowRight(GameObject caller)
	{
		tableHelper.OnScrollRight();
	}
}
