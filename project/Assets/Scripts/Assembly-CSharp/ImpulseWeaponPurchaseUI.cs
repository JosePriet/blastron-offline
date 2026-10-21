using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class ImpulseWeaponPurchaseUI : MonoBehaviour
{
	public enum Mode
	{
		kImpulse = 0,
		kRevenge = 1,
		kNone = 2
	}

	public float timerPulseDuration = 0.5f;

	private GameObject m_discountHolder;

	private GameObject m_backgroundHolder;

	private UILabel m_promptLabel;

	private UITexture m_itemTextureHolder;

	private GameObject m_lockFader;

	private GameObject m_padlock;

	private GameObject m_buttonSoft;

	private GameObject m_buttonHard;

	private TweenScale m_timerTweenScale;

	private UILabel m_timer;

	private UILabel m_discountLabel;

	private int m_hardCost;

	private int m_softCost;

	private string m_previousItemTexturePath;

	private InventoryDB.MetaData m_itemMetadata;

	private CurrencyType m_purchaseType;

	private string m_revengeTargetName;

	private float m_offerElapsedTime;

	private Mode m_mode;

	private Dictionary<GameObject, Vector3> m_originalPositions = new Dictionary<GameObject, Vector3>();

	private bool m_transactionInProgress;

	private static bool _isActive;

	private Vector3 defaultPromptScale;

	public static bool IsActive
	{
		get
		{
			return _isActive;
		}
	}

	private bool IsRevengeSale
	{
		get
		{
			return m_mode == Mode.kRevenge;
		}
	}

	private int availabilityTime
	{
		get
		{
			if (m_mode == Mode.kImpulse)
			{
				return Tuning.Instance.ImpulseWeaponPopupDurationSecs;
			}
			if (m_mode == Mode.kRevenge)
			{
				return Tuning.Instance.RevengeSalePopupDurationSecs;
			}
			return 0;
		}
	}

	private void Awake()
	{
		m_backgroundHolder = Util.GetObjectExactMatch(base.gameObject, "BackgroundHolder");
		m_discountHolder = Util.GetObjectExactMatch(base.gameObject, "Discount");
		m_promptLabel = Util.GetObjectExactMatch(base.gameObject, "Prompt").GetComponent<UILabel>();
		m_itemTextureHolder = Util.GetObjectExactMatch(base.gameObject, "ItemTexture").GetComponent<UITexture>();
		m_lockFader = Util.GetObjectExactMatch(base.gameObject, "LockFader");
		m_padlock = Util.GetObjectExactMatch(base.gameObject, "padlock_item");
		defaultPromptScale = m_promptLabel.transform.localScale;
		m_discountLabel = Util.GetObjectExactMatch(m_discountHolder, "DiscountLabel").GetComponent<UILabel>();
		m_buttonSoft = Util.GetObjectExactMatch(base.gameObject, "ButtonSoft");
		m_buttonSoft.GetComponent<ButtonScript>().onButtonClick += onBuyWithSoftCurrency;
		m_buttonHard = Util.GetObjectExactMatch(base.gameObject, "ButtonHard");
		m_buttonHard.GetComponent<ButtonScript>().onButtonClick += onBuyWithHardCurrency;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Timer");
		m_timer = objectExactMatch.GetComponent<UILabel>();
		m_timerTweenScale = objectExactMatch.AddComponent<TweenScale>();
		m_timerTweenScale.from = m_timer.transform.localScale;
		m_timerTweenScale.to = m_timer.transform.localScale;
		m_timerTweenScale.to.Scale(new Vector3(1.5f, 1.5f, 1f));
		m_timerTweenScale.style = UITweener.Style.PingPong;
		m_timerTweenScale.duration = timerPulseDuration;
		m_timerTweenScale.enabled = false;
		base.gameObject.transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2, base.gameObject.transform.localPosition.z);
		m_originalPositions.Add(m_backgroundHolder, m_backgroundHolder.GetComponentInChildren<UISlicedSprite>().transform.localScale);
		m_originalPositions.Add(m_buttonHard, m_buttonHard.transform.localPosition);
		m_originalPositions.Add(m_buttonSoft, m_buttonSoft.transform.localPosition);
	}

	private void Start()
	{
		m_offerElapsedTime = 0f;
		m_transactionInProgress = false;
		m_timerTweenScale.enabled = false;
	}

	public void Show()
	{
		m_offerElapsedTime = 0f;
		m_timerTweenScale.enabled = true;
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, null);
		_isActive = true;
		if (m_mode == Mode.kRevenge)
		{
			Profile.Instance.OnRevengeSaleShown();
		}
	}

	public void Hide()
	{
		m_offerElapsedTime = 0f;
		m_transactionInProgress = false;
		m_timerTweenScale.enabled = false;
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, null);
		_isActive = false;
	}

	private void Update()
	{
		if (_isActive)
		{
			m_offerElapsedTime += UnityEngine.Time.deltaTime;
			if (m_offerElapsedTime >= (float)availabilityTime)
			{
				m_timer.text = "ID_UI_BEGINNERS_SALE_EXPIRED";
				Hide();
				return;
			}
			int num = availabilityTime - Mathf.RoundToInt(m_offerElapsedTime);
			m_timer.text = Localizer.FormatTime(num, true);
			if (num <= 5)
			{
				m_timer.color = new Color(1f, 0f, 0f, 1f);
			}
			else
			{
				m_timer.color = new Color(1f, 1f, 1f, 1f);
			}
		}
		else
		{
			m_timer.text = string.Empty;
		}
	}

	private float Discount()
	{
		if (m_mode == Mode.kImpulse)
		{
			return (!m_itemMetadata.IsLocked) ? Tuning.Instance.ImpulseWeaponSalePurchaseDiscount : Tuning.Instance.ImpulseWeaponSaleUnlockDiscount;
		}
		if (m_mode == Mode.kRevenge)
		{
			return Tuning.Instance.RevengeSaleDiscount;
		}
		return 0f;
	}

	public void UpdateContents()
	{
		bool flag = Discount() > 0f;
		m_discountHolder.SetActive(flag);
		if (flag)
		{
			int num = Mathf.FloorToInt(Discount() * 100f);
			m_discountLabel.text = "-" + num + "%";
		}
		m_lockFader.SetActive(!IsRevengeSale && m_itemMetadata.IsLocked);
		m_padlock.SetActive(!IsRevengeSale && m_itemMetadata.IsLocked);
		string text;
		if (IsRevengeSale)
		{
			text = Localizer.Format("ID_UI_INGAME_REVENGE_PURCHASE_PROMPT", m_revengeTargetName, m_itemMetadata.friendlyName);
			m_hardCost = m_itemMetadata.hardCost;
			m_softCost = m_itemMetadata.softCost;
		}
		else if (m_itemMetadata.IsLocked)
		{
			text = Localizer.Format("ID_UI_INGAME_IMPULSE_UNLOCK_PROMPT", m_itemMetadata.friendlyName);
			m_hardCost = m_itemMetadata.unlockHardCost;
			m_softCost = m_itemMetadata.unlockSoftCost;
		}
		else
		{
			text = Localizer.Format("ID_UI_INGAME_IMPULSE_PURCHASE_PROMPT", m_itemMetadata.friendlyName, m_itemMetadata.quantity);
			m_hardCost = m_itemMetadata.hardCost;
			m_softCost = m_itemMetadata.softCost;
		}
		m_hardCost = Mathf.CeilToInt((float)m_hardCost * (1f - Discount()));
		m_softCost = Mathf.CeilToInt((float)m_softCost * (1f - Discount()));
		UIUtils.SetLabelContents(m_buttonHard, "LabelAmount", m_hardCost);
		UIUtils.SetLabelContents(m_buttonSoft, "LabelAmount", m_softCost);
		UIUtils.AssignTextAndResize(m_promptLabel, text, defaultPromptScale);
		ResizeBackground(m_originalPositions[m_backgroundHolder]);
		m_discountHolder.SetActive(flag);
		if (!flag)
		{
			ResizeBackground(m_originalPositions[m_backgroundHolder] - Vector3.right * 405f);
		}
		UIUtils.CenterSpriteAndLabel(m_buttonHard, "CurrencyIcon", "LabelAmount");
		UIUtils.CenterSpriteAndLabel(m_buttonSoft, "CurrencyIcon", "LabelAmount");
		if (m_softCost == 0 || m_softCost >= Tuning.Instance.HiddenSoftCostThreshold)
		{
			m_buttonSoft.SetActive(false);
			m_buttonHard.transform.localPosition = Vector3.zero;
			return;
		}
		m_buttonSoft.SetActive(true);
		m_buttonSoft.GetComponent<ButtonScript>().SetEnabled(CanPaySoft());
		m_buttonSoft.transform.localPosition = m_originalPositions[m_buttonSoft];
		m_buttonHard.transform.localPosition = m_originalPositions[m_buttonHard];
	}

	private void ResizeBackground(Vector3 newScale)
	{
		UISlicedSprite[] componentsInChildren = m_backgroundHolder.GetComponentsInChildren<UISlicedSprite>();
		foreach (UISlicedSprite uISlicedSprite in componentsInChildren)
		{
			uISlicedSprite.gameObject.transform.localScale = newScale;
		}
	}

	public void SetData(Mode mode, string itemName, string targetPlayerName, InventoryItem.Category itemCategory)
	{
		m_mode = mode;
		m_revengeTargetName = targetPlayerName;
		if (m_previousItemTexturePath != null)
		{
			TexturePoolManager.Instance.ReleaseTexture(m_previousItemTexturePath);
		}
		InventoryItem inventoryItem = Arsenal.InventoryItemWithName(itemCategory, itemName);
		InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(itemName));
		if (metaData != null)
		{
			m_previousItemTexturePath = inventoryItem.uiTexturePath;
			TexturePoolManager.Instance.LoadTexture(inventoryItem.uiTexturePath, SafeAction.Wrap(this, (Texture2D texture) =>
			{
				m_itemMetadata = metaData;
				m_itemTextureHolder.mainTexture = texture;
				UpdateContents();
			}));
		}
	}

	private void onBuyWithHardCurrency(GameObject caller)
	{
		if (m_transactionInProgress)
		{
			return;
		}
		int cost = ((!m_itemMetadata.IsLocked) ? m_itemMetadata.hardCost : m_itemMetadata.unlockHardCost);
		if (CanPayHard())
		{
			m_transactionInProgress = true;
			if (m_itemMetadata.IsLocked)
			{
				UnlockItem(CurrencyType.Hard, cost);
			}
			else
			{
				PurchaseItem(CurrencyType.Hard, cost);
			}
		}
		else
		{
			InsufficientCurrency(CurrencyType.Hard, cost);
		}
	}

	private void onBuyWithSoftCurrency(GameObject caller)
	{
		if (!m_transactionInProgress && CanPaySoft())
		{
			m_transactionInProgress = true;
			if (m_itemMetadata.IsLocked)
			{
				UnlockItem(CurrencyType.Soft, m_itemMetadata.unlockSoftCost);
			}
			else
			{
				PurchaseItem(CurrencyType.Soft, m_itemMetadata.softCost);
			}
		}
	}

	private bool CanPaySoft()
	{
		return m_softCost <= Profile.Instance.SoftCurrency;
	}

	private bool CanPayHard()
	{
		return m_hardCost <= Profile.Instance.HardCurrency;
	}

	private void PurchaseItem(CurrencyType currencyType, int cost)
	{
		EB.Debug.Log("Purchase " + m_itemMetadata.name + " for " + cost + " " + currencyType.ToString() + " currency ...");
		string src = ((m_mode != Mode.kImpulse) ? "revenge" : "impulse");
		m_purchaseType = currencyType;
		if (currencyType == CurrencyType.Hard)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(m_itemMetadata.name, m_itemMetadata.quantity);
			InventoryListener.PurchaseHard(hashtable, src, m_hardCost, OnPurchaseComplete);
		}
		else
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(m_itemMetadata.name, m_itemMetadata.quantity);
			InventoryListener.PurchaseSoft(hashtable, src, m_softCost, OnPurchaseComplete);
		}
	}

	private void InsufficientCurrency(CurrencyType currencyType, int cost)
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true);
		CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(obj);
		currencyInsufficientUI.SetInfo(currencyType, cost);
		currencyInsufficientUI.onClosed += onCurrencyInsufficientUIClosed;
	}

	private void onCurrencyInsufficientUIClosed(PopupScript ps)
	{
		if (CanPayHard())
		{
			if (m_itemMetadata.IsLocked)
			{
				UnlockItem(CurrencyType.Hard, m_hardCost);
			}
			else
			{
				PurchaseItem(CurrencyType.Hard, m_hardCost);
			}
		}
		else
		{
			Hide();
		}
	}

	private void OnPurchaseComplete(InventoryListener.PurchaseResult pr)
	{
		if (pr == InventoryListener.PurchaseResult.Ok)
		{
			EquipWeapon();
			Hide();
		}
	}

	private void UnlockItem(CurrencyType currencyType, int cost)
	{
		EB.Debug.Log("Unlock " + m_itemMetadata.name + " for " + cost + " " + currencyType.ToString() + " currency ...");
		string src = ((m_itemMetadata.ItemCategory != InventoryItem.Category.Boost) ? "weapon" : "boost");
		m_purchaseType = currencyType;
		if (currencyType == CurrencyType.Hard)
		{
			WalletListener.Instance.Debit(m_hardCost, "unlock: " + m_itemMetadata.name, SafeAction.Wrap(this, (bool result) =>
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

	private void EquipWeapon()
	{
		Player player = LevelData.Instance.GetPlayer();
		EB.Debug.Log("ImpulseWeaponPurchaseUI > EquipWeapon > " + m_itemMetadata.name);
		if (!(player != null))
		{
			return;
		}
		int num = Arsenal.NumItems(InventoryItem.Category.Weapon);
		for (int i = 0; i < num; i++)
		{
			if (Arsenal.ItemName(InventoryItem.Category.Weapon, i) == m_itemMetadata.name && player.CanUseWeapon(i))
			{
				player.SelectWeapon(i);
				break;
			}
		}
	}

	private IEnumerator UnlockTransaction()
	{
		yield return UnlocksManager.Instance.Unlock(m_itemMetadata.name);
		string src = ((m_itemMetadata.ItemCategory != InventoryItem.Category.Boost) ? "weapon" : "boost");
		Telemetry.Event("unlock/" + m_itemMetadata.name);
		Telemetry.Event("unlock_" + src);
		if (m_purchaseType == CurrencyType.Hard)
		{
			Telemetry.Event("unlock_hard/" + m_itemMetadata.name);
		}
		else
		{
			Telemetry.Event("unlock_soft/" + m_itemMetadata.name);
		}
		if (m_mode == Mode.kRevenge)
		{
			Telemetry.Event("item/revenge", m_itemMetadata.quantity, new Hashtable { { "name", m_itemMetadata.name } });
		}
		else
		{
			Telemetry.Event("item/impulse", m_itemMetadata.quantity, new Hashtable { { "name", m_itemMetadata.name } });
		}
		Telemetry.Flush();
		Hub.Instance.InventoryManager.AddItem(m_itemMetadata.name, m_itemMetadata.quantity);
		Profile.Instance.SetNextMaxUnlockLevel(m_itemMetadata.ItemCategory, Profile.Instance.GetMaxItemUnlockLevel(m_itemMetadata.ItemCategory) + 1);
		EquipWeapon();
		Hide();
	}
}
