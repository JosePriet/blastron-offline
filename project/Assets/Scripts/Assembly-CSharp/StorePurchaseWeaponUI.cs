using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using SmoothMoves;
using UnityEngine;

public class StorePurchaseWeaponUI : PopupScript
{
	private InventoryDB.MetaData itemMetaData;

	private CurrencyType purchaseType;

	private float discountPercentage;

	private ImpulseWeaponPurchaseUI.Mode saleMode = ImpulseWeaponPurchaseUI.Mode.kNone;

	private bool allowPurchase = true;

	private bool _enableInput = true;

	private BoneAnimation boneAnim;

	private ParticleSystem unlockFx;

	private GameObject bgHolder;

	private Dictionary<string, Vector3> bgOriginalScales = new Dictionary<string, Vector3>();

	private int _rivetsValue = -1;

	private int _gearsValue = -1;

	private bool IsItemLocked
	{
		get
		{
			return itemMetaData.IsLocked && saleMode != ImpulseWeaponPurchaseUI.Mode.kRevenge;
		}
	}

	protected override void OnAwake()
	{
		if (LevelData.Instance != null && LevelData.Instance.PauseGame)
		{
			LevelData.Instance.PauseGame = false;
		}
		base.OnAwake();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonHardCurrency");
		Util.FindComponent<ButtonScript>(objectExactMatch).onButtonClick += OnPurchaseWithHard;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonSoftCurrency");
		Util.FindComponent<ButtonScript>(objectExactMatch).onButtonClick += OnPurchaseWithSoft;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "padlock_item");
		boneAnim = objectExactMatch2.GetComponentInChildren<BoneAnimation>();
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "fx_padlock");
		unlockFx = objectExactMatch3.GetComponent<ParticleSystem>();
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonFree");
		Util.FindComponent<UILabel>(objectExactMatch).text = "ID_UI_PART_UPGRADE_FREE";
		NGUITools.SetActive(objectExactMatch, false);
		bgHolder = Util.GetObjectExactMatch(base.gameObject, "BackgroundTileHolder");
		UISprite[] componentsInChildren = bgHolder.GetComponentsInChildren<UISprite>();
		UISprite[] array = componentsInChildren;
		foreach (UISprite uISprite in array)
		{
			bgOriginalScales.Add(uISprite.gameObject.name, uISprite.transform.localScale);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		EB.Sequence.Component.Activate(base.gameObject, "StorePurchaseWeaponUI", typeof(SequenceEvent_OnScreenStarted));
	}

	public void Destroy()
	{
		ScreenManager.Instance.EnableBgFade(false);
	}

	public void SetParameters(InventoryDB.MetaData metadata, Texture2D itemTexture, bool allowSale, ImpulseWeaponPurchaseUI.Mode mode = ImpulseWeaponPurchaseUI.Mode.kNone, float discountPercentage = 0f)
	{
		itemMetaData = metadata;
		this.discountPercentage = discountPercentage;
		saleMode = mode;
		allowPurchase = allowSale;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemTexture");
		if (objectExactMatch != null)
		{
			UITexture componentInChildren = objectExactMatch.GetComponentInChildren<UITexture>();
			componentInChildren.mainTexture = itemTexture;
		}
		UpdateContents();
		SetupLock();
	}

	public void SetParameters(string itemName, InventoryItem.Category cat, bool allowSale, ImpulseWeaponPurchaseUI.Mode mode = ImpulseWeaponPurchaseUI.Mode.kNone, float discountPercentage = 0f)
	{
		InventoryItem inventoryItem = Arsenal.InventoryItemWithName(cat, itemName);
		InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(itemName));
		if (metaData != null)
		{
			TexturePoolManager.Instance.LoadTexture(inventoryItem.uiTexturePath, SafeAction.Wrap(this, (Texture2D texture) =>
			{
				SetParameters(metaData, texture, allowSale, mode, discountPercentage);
			}));
		}
	}

	protected override void OnTransitionInComplete()
	{
		base.OnTransitionInComplete();
		EB.Sequence.Component.Activate(base.gameObject, "StorePurchaseWeaponUI", typeof(SequenceEvent_OnScreenOpened));
	}

	protected override void OnTransitionOutComplete()
	{
		ScreenManager.Instance.EnableBgFade(false);
		base.OnTransitionOutComplete();
		EB.Sequence.Component.Activate(base.gameObject, "StorePurchaseWeaponUI", typeof(SequenceEvent_OnScreenClosed));
	}

	private int BaseSoftCost()
	{
		if (saleMode == ImpulseWeaponPurchaseUI.Mode.kRevenge)
		{
			return itemMetaData.revengeSaleSoftCost;
		}
		if (IsItemLocked)
		{
			return itemMetaData.unlockSoftCost;
		}
		return itemMetaData.softCost;
	}

	private int BaseHardCost()
	{
		if (saleMode == ImpulseWeaponPurchaseUI.Mode.kRevenge)
		{
			return itemMetaData.revengeSaleHardCost;
		}
		if (IsItemLocked)
		{
			return itemMetaData.unlockHardCost;
		}
		return itemMetaData.hardCost;
	}

	private int DiscountedHardCost()
	{
		return Mathf.CeilToInt((float)BaseHardCost() * (1f - discountPercentage));
	}

	private int DiscountedSoftCost()
	{
		return Mathf.CeilToInt((float)BaseSoftCost() * (1f - discountPercentage));
	}

	private void UpdateContents()
	{
		UIUtils.SetLabelContents(base.gameObject, "ItemName", itemMetaData.friendlyName);
		UIUtils.SetLabelContents(base.gameObject, "ItemDescription", itemMetaData.description);
		UIUtils.SetLabelContents(base.gameObject, "Count", Hub.Instance.InventoryManager.GetCount(itemMetaData.name).ToString());
		bool flag = true;
		if (allowPurchase)
		{
			if (IsItemLocked)
			{
				if (itemMetaData.isChampionshipTierReward)
				{
					Util.GetObjectExactMatch(base.gameObject, "ChampionshipPanel").SetActive(true);
					Util.GetObjectExactMatch(base.gameObject, "RegularPanel").SetActive(false);
					int numStars = ChampionshipData.RewardTiers.GetTierWithWeapon(itemMetaData.name).NumStars;
					UIUtils.SetLabelContents(base.gameObject, "ChampionshipPanel", Localizer.Format("ID_UI_PURCHASE_PREREQ_CHAMPIONSHIP_DESC", numStars));
				}
				else
				{
					Util.GetObjectExactMatch(base.gameObject, "ChampionshipPanel").SetActive(false);
					Util.GetObjectExactMatch(base.gameObject, "RegularPanel").SetActive(true);
					UIUtils.SetLabelContents(base.gameObject, "Action", (itemMetaData.ItemCategory != InventoryItem.Category.Weapon) ? "ID_UI_PURCHASE_UNLOCK_FREE_BOOST" : "ID_UI_PURCHASE_UNLOCK_FREE_BUNDLE");
					UIUtils.SetLabelContents(base.gameObject, "ButtonSoftCurrency", DiscountedSoftCost());
					UIUtils.SetLabelContents(base.gameObject, "ButtonHardCurrency", DiscountedHardCost());
					if (DiscountedSoftCost() > Profile.Instance.SoftCurrency)
					{
						flag = false;
					}
					UIUtils.SetLabelContents(base.gameObject, "LabelPrereq", string.Empty);
				}
			}
			else
			{
				Util.GetObjectExactMatch(base.gameObject, "ChampionshipPanel").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "RegularPanel").SetActive(true);
				UIUtils.SetLabelContents(base.gameObject, "Action", Localizer.Format("ID_UI_PURCHASE_BUY_N", itemMetaData.quantity));
				UIUtils.SetLabelContents(base.gameObject, "ButtonSoftCurrency", DiscountedSoftCost());
				UIUtils.SetLabelContents(base.gameObject, "ButtonHardCurrency", DiscountedHardCost());
				UIUtils.SetLabelContents(base.gameObject, "LabelPrereq", string.Empty);
				if (DiscountedSoftCost() > Profile.Instance.SoftCurrency)
				{
					flag = false;
				}
			}
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonHardCurrency");
			UIUtils.CenterSpriteAndLabel(objectExactMatch, "SpriteIcon", "LabelAmount");
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonSoftCurrency");
			if (DiscountedSoftCost() == 0 || DiscountedSoftCost() >= Tuning.Instance.HiddenSoftCostThreshold)
			{
				objectExactMatch2.SetActive(false);
				Vector3 localPosition = objectExactMatch.transform.localPosition;
				localPosition.x = 0f;
				objectExactMatch.transform.localPosition = localPosition;
				NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "ItemSeparator"), false);
				return;
			}
			UIUtils.CenterSpriteAndLabel(objectExactMatch2, "SpriteIcon", "LabelAmount");
			float alpha = 1f;
			if (!flag)
			{
				alpha = 0.3f;
			}
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch2, "Background");
			objectExactMatch3.GetComponent<UISprite>().alpha = alpha;
		}
		else
		{
			Util.GetObjectExactMatch(base.gameObject, "RegularPanel").SetActive(false);
			Util.GetObjectExactMatch(base.gameObject, "ChampionshipPanel").SetActive(false);
			Util.GetObjectExactMatch(base.gameObject, "SpriteLine").SetActive(false);
			Util.GetObjectExactMatch(base.gameObject, "CurrencyBar").SetActive(false);
			showCurrencyBar = false;
			UISprite[] componentsInChildren = bgHolder.GetComponentsInChildren<UISprite>();
			UISprite[] array = componentsInChildren;
			foreach (UISprite uISprite in array)
			{
				uISprite.transform.localScale = bgOriginalScales[uISprite.gameObject.name] - new Vector3(0f, 147f, 0f);
			}
		}
	}

	private void SetupLock()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Lock");
		NGUITools.SetActive(objectExactMatch, IsItemLocked);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "padlock_item");
		NGUITools.SetActive(objectExactMatch2, IsItemLocked);
	}

	private void OnPurchaseWithHard(GameObject caller)
	{
		if (_enableInput)
		{
			CheckAndUnpause();
			if (IsItemLocked)
			{
				UnlockItem(CurrencyType.Hard, DiscountedHardCost());
			}
			else
			{
				PurchaseItem(CurrencyType.Hard, DiscountedHardCost());
			}
		}
	}

	protected override void Close(GameObject caller)
	{
		if (_enableInput)
		{
			CheckAndUnpause();
			base.Close(caller);
			parentScreen.SendMessage("OnClosePurchaseWeaponScreen", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void CheckAndUnpause()
	{
		if (LevelData.Instance != null && LevelData.Instance.PauseGame)
		{
			LevelData.Instance.PauseGame = false;
		}
	}

	private void OnPurchaseWithSoft(GameObject caller)
	{
		if (!_enableInput)
		{
			return;
		}
		CheckAndUnpause();
		int num = DiscountedSoftCost();
		if (num <= Profile.Instance.SoftCurrency)
		{
			if (IsItemLocked)
			{
				UnlockItem(CurrencyType.Soft, num);
			}
			else
			{
				PurchaseItem(CurrencyType.Soft, num);
			}
		}
	}

	private void UnlockItem(CurrencyType currencyType, int cost)
	{
		EB.Debug.Log("Unlock " + itemMetaData.name + " for " + cost + " " + currencyType.ToString() + " currency ...");
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
		string src = ((itemMetaData.ItemCategory != InventoryItem.Category.Boost) ? "weapon" : "boost");
		purchaseType = currencyType;
		if (currencyType == CurrencyType.Hard)
		{
			WalletListener.Instance.Debit(cost, "unlock: " + itemMetaData.name, SafeAction.Wrap(this, (bool result) =>
			{
				if (result)
				{
					Telemetry.Event("spend_hard", cost);
					Telemetry.Event("spend_hard_src/unlock_" + src, cost);
					StartCoroutine("UnlockTransaction");
				}
				else
				{
					InsufficientCurrency(currencyType, cost);
				}
			}));
		}
		else if (Profile.Instance.RemoveSoftCurrency(cost))
		{
			Telemetry.Event("spend_soft", cost);
			Telemetry.Event("spend_soft_src/unlock_" + src, cost);
			StartCoroutine("UnlockTransaction");
		}
		else
		{
			InsufficientCurrency(currencyType, cost);
		}
	}

	private IEnumerator UnlockTransaction()
	{
		yield return UnlocksManager.Instance.Unlock(itemMetaData.name);
		string src = ((itemMetaData.ItemCategory != InventoryItem.Category.Boost) ? "weapon" : "boost");
		Telemetry.Event("unlock/" + itemMetaData.name, itemMetaData.quantity);
		Telemetry.Event("unlock_" + src);
		if (purchaseType == CurrencyType.Hard)
		{
			Telemetry.Event("unlock_hard/" + itemMetaData.name, itemMetaData.quantity);
		}
		else
		{
			Telemetry.Event("unlock_soft/" + itemMetaData.name, itemMetaData.quantity);
		}
		Telemetry.Flush();
		if (boneAnim != null)
		{
			boneAnim.Play("padlock_open_clamp");
			StartCoroutine("RemoveUnlockFader");
		}
		if (unlockFx != null)
		{
			unlockFx.Play();
		}
		ScreenManager.Instance.ClosePopup("BlockingUI");
		Hub.Instance.InventoryManager.AddItem(itemMetaData.name, itemMetaData.quantity);
		Profile.Instance.SetNextMaxUnlockLevel(itemMetaData.ItemCategory, Profile.Instance.GetMaxItemUnlockLevel(itemMetaData.ItemCategory));
		UpdateContents();
		if (parentScreen != null)
		{
			parentScreen.SendMessage("OnUnlockComplete");
		}
	}

	private void PurchaseItem(CurrencyType currencyType, int cost)
	{
		EB.Debug.Log("Purchase " + itemMetaData.name + " for " + cost + " " + currencyType.ToString() + " currency ...");
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
		string src = ((itemMetaData.ItemCategory != InventoryItem.Category.Boost) ? "weapons" : "boosts");
		purchaseType = currencyType;
		int num = ((saleMode != ImpulseWeaponPurchaseUI.Mode.kRevenge) ? itemMetaData.quantity : itemMetaData.revengeSaleQuantity);
		if (currencyType == CurrencyType.Hard)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(itemMetaData.name, num);
			InventoryListener.PurchaseHard(hashtable, src, cost, OnTransactionComplete);
		}
		else
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(itemMetaData.name, num);
			InventoryListener.PurchaseSoft(hashtable, src, cost, OnTransactionComplete);
		}
	}

	private void Update()
	{
		int softCurrency = Profile.Instance.SoftCurrency;
		if (softCurrency != _rivetsValue)
		{
			UIUtils.SetLabelContents(base.gameObject, "SoftCurrencyCount", Profile.Instance.SoftCurrency);
			_rivetsValue = softCurrency;
		}
		softCurrency = Profile.Instance.HardCurrency;
		if (softCurrency != _gearsValue)
		{
			UIUtils.SetLabelContents(base.gameObject, "HardCurrencyCount", Profile.Instance.HardCurrency);
			_gearsValue = softCurrency;
		}
	}

	private IEnumerator RemoveUnlockFader()
	{
		yield return new WaitForSeconds(Tuning.Instance.StoreUnlock_RemovePadlockFaderDelay);
		GameObject lockObj = Util.GetObjectExactMatch(base.gameObject, "Lock");
		NGUITools.SetActive(lockObj, false);
		yield return new WaitForSeconds(1.5f);
		lockObj = Util.GetObjectExactMatch(base.gameObject, "padlock_item");
		lockObj.SetActive(false);
	}

	private void OnTransactionComplete(InventoryListener.PurchaseResult result)
	{
		ScreenManager.Instance.ClosePopup();
		switch (result)
		{
		case InventoryListener.PurchaseResult.Ok:
		{
			if (Hub.Instance.GameManager.Game != null)
			{
				ScreenManager.Instance.EnableBgFade(false);
			}
			if (parentScreen != null)
			{
				parentScreen.SendMessage("OnPurchaseComplete", itemMetaData);
			}
			string text = ((itemMetaData.ItemCategory != InventoryItem.Category.Boost) ? "weapon" : "boost");
			Telemetry.Event("buy_" + text);
			break;
		}
		case InventoryListener.PurchaseResult.Failed:
		{
			int cost = ((purchaseType != CurrencyType.Hard) ? DiscountedSoftCost() : DiscountedHardCost());
			InsufficientCurrency(purchaseType, cost);
			break;
		}
		}
		UpdateContents();
		EB.Sequence.Component.Activate(base.gameObject, "StorePurchaseWeaponUI", typeof(SequenceEvent_OnPurchaseComplete));
		EB.Debug.Log("ON TRANSACTION COMPLETE WEAPON PURCHASE!");
	}

	private void InsufficientCurrency(CurrencyType currencyType, int cost)
	{
		ScreenManager.Instance.ClosePopup("BlockingUI");
		GameObject obj = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true, true);
		CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(obj);
		currencyInsufficientUI.SetInfo(currencyType, cost);
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void RunTutorial(bool run)
	{
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "ButtonFree"), run);
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "ButtonSoftCurrency"), !run);
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "ButtonHardCurrency"), !run);
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "ItemSeparator"), !run);
	}

	private void OnFETutorialBuy()
	{
		PurchaseItem(CurrencyType.Soft, 0);
	}
}
