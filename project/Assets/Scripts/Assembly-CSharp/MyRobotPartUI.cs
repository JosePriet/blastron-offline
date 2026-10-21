using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using UnityEngine;

public class MyRobotPartUI : PopupScript
{
	private CharacterDB.MetaData metadata;

	private float previewItemUpgradeProgress;

	private bool buyWithHard = true;

	private bool buyWithSoft;

	private int cost;

	private MyRobotUI parentScreenMR;

	private GameObject buySoft;

	private bool _enableInput = true;

	private bool _runningTutorial;

	private void Awake()
	{
		Util.GetObjectExactMatch(base.gameObject, "SoftCurrency").GetComponent<ButtonScript>().onButtonClick += OnBuyWithSoft;
		Util.GetObjectExactMatch(base.gameObject, "HardCurrency").GetComponent<ButtonScript>().onButtonClick += OnBuyWithHard;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "SoftCurrency");
		buySoft = Util.GetObjectExactMatch(objectExactMatch, "Background");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "FreeButton");
		Util.FindComponent<UILabel>(objectExactMatch2).text = "ID_UI_PART_UPGRADE_FREE";
		NGUITools.SetActive(objectExactMatch2, false);
	}

	public void SetParameters(CharacterDB.MetaData m, MyRobotUI p)
	{
		metadata = m;
		parentScreenMR = p;
		if (metadata.coins == 0 || metadata.coins >= Tuning.Instance.HiddenSoftCostThreshold)
		{
			Util.GetObjectExactMatch(base.gameObject, "SoftCurrency").SetActive(false);
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "HardCurrency");
			Vector3 localPosition = objectExactMatch.transform.localPosition;
			localPosition.x = 0f;
			objectExactMatch.transform.localPosition = localPosition;
		}
		else
		{
			Util.GetObjectExactMatch(base.gameObject, "SoftCurrency").GetComponent<ButtonScript>().SetText(metadata.coins.ToString());
		}
		Util.GetObjectExactMatch(base.gameObject, "HardCurrency").GetComponent<ButtonScript>().SetText(metadata.bucks.ToString());
		CacUiItem cacUiItem = Util.FindComponent<CacUiItem>(base.gameObject);
		cacUiItem.metaData = m;
		cacUiItem.texturePath = "cac_ui_parts/" + metadata.cacTexturePath;
		cacUiItem.UpdateArt();
		cacUiItem.Load();
		UpdateSoftCurrency();
	}

	protected override void HandleOpenPopup()
	{
		base.HandleOpenPopup();
		EB.Sequence.Component.Activate(base.gameObject, "MyRobotPartUI", typeof(SequenceEvent_OnScreenStarted));
	}

	protected override void OnTransitionInComplete()
	{
		EB.Sequence.Component.Activate(base.gameObject, "MyRobotPartUI", typeof(SequenceEvent_OnScreenOpened));
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		CacUiItem cacUiItem = Util.FindComponent<CacUiItem>(base.gameObject);
		cacUiItem.Unload();
		EB.Sequence.Component.Activate(base.gameObject, "MyRobotPartUI", typeof(SequenceEvent_OnScreenClosed));
	}

	private RobotAttribute GetAttributeByType(List<RobotAttribute> attributes, RobotAttribute.RobotAttributeType type)
	{
		RobotAttribute robotAttribute = attributes.Find((RobotAttribute item) => item.type == type);
		if (robotAttribute == null)
		{
			robotAttribute = new RobotAttribute();
			robotAttribute.type = type;
		}
		return robotAttribute;
	}

	private void OnBuyWithHard(GameObject caller)
	{
		if (_enableInput)
		{
			buyWithHard = true;
			buyWithSoft = false;
			cost = metadata.bucks;
			BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
			component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
			Hashtable hashtable = new Hashtable();
			hashtable.Add(metadata.name, 1);
			InventoryListener.PurchaseHard(hashtable, "parts", metadata.bucks, SafeAction.Wrap<InventoryListener.PurchaseResult>(this, OnTransactionComplete));
		}
	}

	private void OnBuyWithSoft(GameObject caller)
	{
		if (_enableInput && metadata.coins <= Profile.Instance.SoftCurrency)
		{
			buyWithHard = false;
			buyWithSoft = true;
			cost = metadata.coins;
			BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
			component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
			Hashtable hashtable = new Hashtable();
			hashtable.Add(metadata.name, 1);
			InventoryListener.PurchaseSoft(hashtable, "parts", metadata.coins, SafeAction.Wrap<InventoryListener.PurchaseResult>(this, OnTransactionComplete));
		}
	}

	protected override void Close(GameObject caller)
	{
		if (_enableInput)
		{
			base.Close(caller);
		}
	}

	private void OnTransactionComplete(InventoryListener.PurchaseResult result)
	{
		ScreenManager.Instance.ClosePopup();
		EB.Sequence.Component.Activate(base.gameObject, "MyRobotPartUI", typeof(SequenceEvent_OnPurchaseComplete));
		if (result == InventoryListener.PurchaseResult.Ok)
		{
			if (!_runningTutorial)
			{
				GameObject obj = ScreenManager.Instance.OpenPopup("ConfirmationDialogUI", true, true);
				ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
				string title = Localizer.Format("ID_UI_PART_PURCHASE_CELEB_TITLE", metadata.friendlyName);
				string description = Localizer.Format("ID_UI_PART_PURCHASE_CELEB_DESC", metadata.friendlyName);
				confirmationDialogUI.SetParameters(true, description, title, "ID_UI_PART_PURCHASE_FREE_UPGRADE", null);
				confirmationDialogUI.SetParentScreen(base.gameObject);
				parentScreenMR.EquipItem(metadata);
				parentScreenMR.RefreshData();
				SeXP.CreateAtVP(new Vector3(0.5f, 0.15f, 0f), metadata.xp);
			}
			else
			{
				parentScreenMR.EquipItem(metadata);
				parentScreenMR.RefreshData();
				OnClose();
			}
			Profile.Instance.MarkTutorialDone("part_bought");
		}
		else
		{
			GameObject obj2 = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true, true);
			CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(obj2);
			CurrencyType currencyType = CurrencyType.Hard;
			if (!buyWithHard)
			{
				currencyType = CurrencyType.Soft;
			}
			currencyInsufficientUI.SetInfo(currencyType, cost);
		}
		UpdateSoftCurrency();
	}

	private void OnUpgradeItem(GameObject caller)
	{
		if (_enableInput)
		{
			ScreenManager.Instance.ClosePopup();
			GameObject obj = ScreenManager.Instance.OpenPopup("UpgradeUI", true, true);
			Util.FindComponent<UpgradeUI>(obj).SetParameters(metadata);
		}
	}

	private void OnConfirmationDialogUIClosed(ConfirmationDialogUI.ButtonIds buttonID)
	{
		if (buttonID == ConfirmationDialogUI.ButtonIds.Back)
		{
			ClosePopup();
		}
		else
		{
			OnUpgradeItem(base.gameObject);
		}
	}

	private void UpdateSoftCurrency()
	{
		if (metadata != null)
		{
			float alpha = 1f;
			if (metadata.coins > Profile.Instance.SoftCurrency)
			{
				alpha = 0.3f;
			}
			buySoft.GetComponent<UISprite>().alpha = alpha;
		}
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void RunTutorial(bool run)
	{
		_runningTutorial = run;
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "FreeButton"), run);
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "SoftCurrency"), !run);
		NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "HardCurrency"), !run);
	}

	private void OnFETutorialBuy()
	{
		buyWithHard = false;
		buyWithSoft = true;
		cost = 0;
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
		Hashtable hashtable = new Hashtable();
		hashtable.Add(metadata.name, 1);
		InventoryListener.PurchaseHard(hashtable, "parts", 0, SafeAction.Wrap<InventoryListener.PurchaseResult>(this, OnTransactionComplete));
	}
}
