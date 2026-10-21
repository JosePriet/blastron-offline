using System.Collections.Generic;
using EB;
using EB.Collections;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class GachaUI : MonoBehaviour
{
	public GameObject gachaTableItemPrefab;

	public float gachaTableItemOffset = 122f;

	private MenuButtonScript spinBtn;

	private ButtonScript backBtn;

	private List<GachaTableItem> tableItems;

	private StatusBarUI statusBarUi;

	private GachaSpinner gachaSpinner;

	private UITutorialArrow spinTutorialArrow;

	private GachaAttract gachaAttract;

	private SafeInt xpRewardPending;

	private Collider spinBtnCollider;

	private bool isUiLocked;

	private bool tutorialInputLock;

	public void SetSelectedTable(GachaTableItem newSelection)
	{
		EB.Debug.Log("GachaUI > SetSelectedTable > newSelection: " + ((!(newSelection != null)) ? "<null>" : (newSelection.name + " - " + newSelection.Box.Name)));
		foreach (GachaTableItem tableItem in tableItems)
		{
			if (tableItem != newSelection)
			{
				tableItem.Selected = GachaTableItem.Selection.Unselected;
			}
		}
		if (newSelection == null)
		{
			gachaSpinner.Close();
		}
		else
		{
			gachaSpinner.Open();
		}
		UpdateInteractiveElements();
	}

	public void EnableInput(bool enable)
	{
		tutorialInputLock = !enable;
		UpdateInteractiveElements();
	}

	public void EnableButton(TutorialHook.Parameters parameters)
	{
		UnityEngine.Debug.Log("> Tutorial has requested activation of button: " + parameters.tutorialId);
		foreach (GachaTableItem tableItem in tableItems)
		{
			if (tableItem.name == parameters.tutorialId)
			{
				tableItem.SetInteractive(true);
			}
		}
		if (spinBtn.name == parameters.tutorialId)
		{
			SetSpinButtonActive(true);
		}
		if (backBtn.name == parameters.tutorialId)
		{
			backBtn.SetEnabled(true);
		}
	}

	public void DisableButton(TutorialHook.Parameters parameters)
	{
		UnityEngine.Debug.Log("> Tutorial has requested deactivation of button: " + parameters.tutorialId);
		foreach (GachaTableItem tableItem in tableItems)
		{
			if (tableItem.name == parameters.tutorialId)
			{
				tableItem.SetInteractive(false);
			}
		}
		if (spinBtn.name == parameters.tutorialId)
		{
			SetSpinButtonActive(false);
		}
		if (backBtn.name == parameters.tutorialId)
		{
			backBtn.SetEnabled(false);
		}
	}

	public GachaTableItem GetSelectedTable()
	{
		GachaTableItem gachaTableItem = tableItems.Find((GachaTableItem item) => item.Selected != GachaTableItem.Selection.Unselected);
		EB.Debug.Log("GachaUI > GetSelectedTable > selected: " + ((!(gachaTableItem != null)) ? "<null>" : (gachaTableItem.name + " - " + gachaTableItem.Box.Name)));
		return gachaTableItem;
	}

	private void Awake()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonSpin");
		spinBtn = Util.FindComponent<MenuButtonScript>(objectExactMatch);
		spinBtnCollider = Util.FindComponent<Collider>(objectExactMatch);
		spinTutorialArrow = Util.FindComponent<UITutorialArrow>(objectExactMatch);
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		backBtn = Util.FindComponent<ButtonScript>(objectExactMatch);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Spinner");
		gachaSpinner = Util.FindComponent<GachaSpinner>(objectExactMatch2);
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "GachaAttract");
		gachaAttract = Util.FindComponent<GachaAttract>(objectExactMatch3);
		statusBarUi = ScreenManager.Instance.GetPersistentScreen("StatusBar").GetComponent<StatusBarUI>();
		spinBtn.onButtonClick += OnSpinClick;
		backBtn.onButtonClick += OnBack;
		LockUi(true);
		SetSpinButtonActive(false);
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(persistentScreen);
		backgroundUI.ShowCliff(false);
	}

	private void Start()
	{
		ScreenManager.Instance.FadeOut(() =>
		{
			DataReadyCheck();
		});
	}

	private void OnDestroy()
	{
		if (statusBarUi != null)
		{
			statusBarUi.checkSoftCurrency = true;
			statusBarUi.blockCurrencyStoreForUi = false;
		}
	}

	private void OnTransitionInComplete()
	{
		LockUi(false);
		EB.Sequence.Component.Activate(base.gameObject, "GachaUI", typeof(SequenceEvent_OnScreenOpened));
	}

	private void OnTransitionOutComplete()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			if (!Profile.Instance.IsTutorialDone("vault"))
			{
				Profile.Instance.MarkTutorialDone("vault");
			}
			ScreenManager.Instance.ClosePopup();
		});
	}

	private void OnSpinClick(GameObject go)
	{
		EB.Debug.Log("GachaUI > OnSpinClick");
		LockUi(true);
		GachaManager.Instance.CheckGachaTimestamp(SafeAction.Wrap(this, (bool timestampsMatch) =>
		{
			if (timestampsMatch || !Profile.Instance.IsTutorialDone("mp"))
			{
				DoPurchase();
			}
			else
			{
				Restocking();
			}
		}));
	}

	public void Restocking()
	{
		ScreenManager.Instance.BlockingUI("ID_UI_GACHA_RESTOCKING", "ID_UI_LOGIN_BLOCKING_DESC");
		GachaManager.Instance.Restock((bool success) =>
		{
			GachaManager.Instance.Initialize();
			BuildTables();
			gachaAttract.SetData(GachaManager.Instance.GetAttractItems(GachaManager.GachaConfig.DefaultGroupName));
			CloseBlockingUi();
			LockUi(false);
		});
	}

	private void OnBack(GameObject go)
	{
		if (!isUiLocked)
		{
			LockUi(true);
			EB.Debug.Log("GachaUI > OnBack");
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionOutComplete));
		}
	}

	private void DataReadyCheck()
	{
		if (GachaManager.Instance.IsReady())
		{
			BuildTables();
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
			gachaAttract.SetData(GachaManager.Instance.GetAttractItems(GachaManager.GachaConfig.DefaultGroupName));
		}
		else
		{
			Coroutines.NextFrame(SafeAction.Wrap(this, DataReadyCheck));
		}
	}

	private void RemoveTables()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "GachaTableHolder");
		if (tableItems == null)
		{
			return;
		}
		foreach (GachaTableItem tableItem in tableItems)
		{
			Object.Destroy(tableItem.gameObject);
		}
		tableItems = null;
	}

	private void BuildTables()
	{
		RemoveTables();
		EB.Debug.Log("GachaUI > BuildTables");
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "GachaTableHolder");
		tableItems = new List<GachaTableItem>();
		string defaultGroupName = GachaManager.GachaConfig.DefaultGroupName;
		int numBoxes = GachaManager.Instance.GetNumBoxes(defaultGroupName);
		for (int i = 0; i < numBoxes; i++)
		{
			GachaBox box = GachaManager.Instance.GetBox(GachaManager.GachaConfig.DefaultGroupName, i);
			GameObject gameObject = NGUITools.AddChild(objectExactMatch, gachaTableItemPrefab);
			Vector3 localPosition = gameObject.transform.localPosition;
			localPosition.y += gachaTableItemOffset * (float)i;
			gameObject.transform.localPosition = localPosition;
			gameObject.name = gachaTableItemPrefab.name + " " + i;
			GachaTableItem gachaTableItem = Util.FindComponent<GachaTableItem>(gameObject);
			gachaTableItem.gachaUI = this;
			int freeSpins = 0;
			if (box != null)
			{
				freeSpins = GachaManager.Instance.GetFreeSpins(defaultGroupName, box.Index);
			}
			gachaTableItem.SetData(box, freeSpins);
			tableItems.Add(gachaTableItem);
		}
		UpdateInteractiveElements();
	}

	private void CloseBlockingUi()
	{
		EB.Collections.Stack<ScreenManager.PopupInfo> popupStack = ScreenManager.Instance.GetPopupStack();
		if (popupStack.Peek().name == "BlockingUI")
		{
			ScreenManager.Instance.ClosePopup();
		}
	}

	private void RequestServerPick(int boxId, int softPaid, int hardPaid, int tokensPaid)
	{
		statusBarUi.checkSoftCurrency = false;
		GachaTableItem tableItem = tableItems.Find((GachaTableItem item) => item.Box.Index == boxId);
		GachaBox box = GachaManager.Instance.GetBox(GachaManager.GachaConfig.DefaultGroupName, boxId);
		string text = ((!Profile.Instance.IsTutorialDone("vault")) ? GachaManager.GachaConfig.DefaultTutorialGroupName : GachaManager.GachaConfig.DefaultGroupName);
		string boxName = ((!Profile.Instance.IsTutorialDone("vault")) ? GachaManager.GachaConfig.DefaultTutorialGroupName : box.Name);
		string payment = ((tokensPaid <= 0) ? string.Empty : "token");
		GachaManager.Instance.DoGachaPick(text, boxName, hardPaid, softPaid, payment, (GachaManager.GachaResult result, int xpAwarded, List<GachaPossiblePrize> prespin) =>
		{
			CloseBlockingUi();
			Hub.Instance.WalletManager.Fetch();
			if (result != null)
			{
				if (tokensPaid > 0)
				{
					int paymentBalance = result.paymentBalance;
					tableItem.SetData(box, paymentBalance);
				}
				xpRewardPending = xpAwarded;
				gachaSpinner.SetSpinResult(result, prespin, tableItem, SafeAction.Wrap(this, OnSpinAnimationComplete));
			}
			else
			{
				OnSpinAnimationComplete();
				gachaSpinner.Exit();
				ScreenManager.Instance.ShowError("ID_SPARX_ERROR_UNKNOWN", () =>
				{
					SetSelectedTable(null);
				});
				Restocking();
			}
		});
	}

	private void OnSpinAnimationComplete()
	{
		statusBarUi.checkSoftCurrency = true;
		statusBarUi.blockCurrencyStoreForUi = false;
		GachaTableItem selectedTable = GetSelectedTable();
		if (selectedTable != null)
		{
			selectedTable.HandlePostSpin();
		}
		LockUi(false);
		if ((int)xpRewardPending > 0)
		{
			SeXP.CreateAtVP(new Vector3(0.75f, 0.15f, 0f), xpRewardPending);
			xpRewardPending = 0;
		}
		EB.Sequence.Component.Activate(base.gameObject, "GachaUI", typeof(SequenceEvent_OnLootinatorComplete));
	}

	private void LockUi(bool locked)
	{
		isUiLocked = locked;
		UpdateInteractiveElements();
	}

	private void UpdateInteractiveElements()
	{
		if (tableItems != null)
		{
			foreach (GachaTableItem tableItem in tableItems)
			{
				tableItem.SetInteractive(!isUiLocked && !tutorialInputLock);
				backBtn.SetEnabled(!isUiLocked && !tutorialInputLock);
			}
		}
		bool flag = !isUiLocked && GetSelectedTable() != null && !tutorialInputLock;
		if (flag)
		{
			GachaTableItem selectedTable = GetSelectedTable();
			if (selectedTable != null)
			{
				flag = selectedTable.IsFree || selectedTable.CanPayHard || (selectedTable.CanPaySoft && Profile.Instance.SoftCurrency >= selectedTable.Box.SoftCurrency.Cost);
			}
		}
		SetSpinButtonActive(flag);
		gachaAttract.EnableCycling(!isUiLocked);
	}

	private void DoPurchase()
	{
		GachaTableItem selected = GetSelectedTable();
		if (selected == null)
		{
			return;
		}
		statusBarUi.blockCurrencyStoreForUi = true;
		selected.PrepareForSpin();
		string groupName = (Profile.Instance.IsTutorialDone("vault") ? GachaManager.GachaConfig.DefaultGroupName : GachaManager.GachaConfig.DefaultTutorialGroupName);
		string text = "gachaspin_" + selected.Box.Index;
		if (selected.Selected == GachaTableItem.Selection.SelectedFree)
		{
			int freeSpins = GachaManager.Instance.GetFreeSpins(groupName, selected.Box.Index);
			if (freeSpins >= selected.Box.Tokens.Cost)
			{
				GachaManager.Instance.UseFreeSpin(groupName, selected.Box.Index, (string error, int newSpinCount) =>
				{
					if (string.IsNullOrEmpty(error))
					{
						if (newSpinCount >= selected.Box.Tokens.Cost)
						{
							selected.Selected = GachaTableItem.Selection.SelectedFree;
						}
						else
						{
							selected.Selected = ((!selected.CanPayHard) ? GachaTableItem.Selection.SelectedSoft : GachaTableItem.Selection.SelectedHard);
						}
						EB.Debug.Log("GachaUI > DoPurchase > UseFreeSpin completed with newSpinCount: " + newSpinCount);
						selected.PrepareForSpin();
						ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
						RequestServerPick(selected.Box.Index, 0, 0, selected.Box.Tokens.Cost);
					}
					else
					{
						EB.Debug.LogError("GachaUI > DoPurchase > UseFreeSpin return error: " + error);
						OnSpinAnimationComplete();
						gachaSpinner.Exit();
						ScreenManager.Instance.ShowError("ID_SPARX_ERROR_UNKNOWN", () =>
						{
							SetSelectedTable(null);
						});
					}
				});
			}
			else
			{
				HandleInsufficientCurrency(CurrencyType.Hard);
			}
		}
		else if (selected.Selected == GachaTableItem.Selection.SelectedSoft)
		{
			int cost = selected.Box.SoftCurrency.Cost;
			if (Profile.Instance.RemoveSoftCurrency(cost) && cost > 0)
			{
				ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
				Coroutines.NextFrame(SafeAction.Wrap(this, () =>
				{
					RequestServerPick(selected.Box.Index, cost, 0, 0);
				}));
			}
			else
			{
				HandleInsufficientCurrency(CurrencyType.Soft);
			}
		}
		else if (selected.Selected == GachaTableItem.Selection.SelectedHard)
		{
			int cost2 = selected.Box.HardCurrency.Cost;
			if (Profile.Instance.HardCurrency >= cost2 && cost2 > 0)
			{
				ScreenManager.Instance.BlockingUI("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
				RequestServerPick(selected.Box.Index, 0, cost2, 0);
			}
			else
			{
				HandleInsufficientCurrency(CurrencyType.Hard);
			}
		}
	}

	private void HandleInsufficientCurrency(CurrencyType currencyType)
	{
		statusBarUi.blockCurrencyStoreForUi = false;
		GachaTableItem selectedTable = GetSelectedTable();
		if (selectedTable != null)
		{
			selectedTable.HandlePostSpin();
		}
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", false, true);
		StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
		storeCurrencyUI.currencyType = currencyType;
		LockUi(false);
	}

	private void SetSpinButtonActive(bool active)
	{
		spinBtn.SetEnabled(active);
		spinBtn.SetGlareActive(active);
		spinBtnCollider.enabled = active;
		if (spinTutorialArrow != null)
		{
			if (active)
			{
				spinTutorialArrow.ShowArrow();
			}
			else
			{
				spinTutorialArrow.HideArrow();
			}
		}
	}
}
