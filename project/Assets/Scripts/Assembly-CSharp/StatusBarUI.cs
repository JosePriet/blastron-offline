using System;
using EB;
using EB.Sparx;
using UnityEngine;

public class StatusBarUI : MonoBehaviour
{
	[NonSerialized]
	public bool checkSoftCurrency = true;

	[NonSerialized]
	public bool blockCurrencyStoreForUi;

	private UILabel _rivets;

	private int _rivetsValue = -1;

	private UILabel _gears;

	private int _gearsValue = -1;

	private UILabel _level;

	private int _levelValue = -1;

	private UILabel _xp;

	private UISlider _progress;

	private int _xpValue = -1;

	private UILabel _bgSaleTimer;

	private GameObject _bgSaleObj;

	private Vector3 _defaultGearsScale;

	private Vector3 _defaultRivetsScale;

	private Vector3 _defaultXpScale;

	private UILabel hofText;

	private string hofTexturePath;

	private GameObject _hofObject;

	private int _hofLevel = -1;

	private UITexture hofBadgeTexture;

	private bool InputEnabled { get; set; }

	private void Awake()
	{
		InputEnabled = true;
		GameObject gameObject = Util.GetObject(base.gameObject, "Rivets");
		_rivets = gameObject.GetComponentInChildren<UILabel>();
		gameObject = Util.GetObject(base.gameObject, "Gears");
		_gears = gameObject.GetComponentInChildren<UILabel>();
		gameObject = Util.GetObject(base.gameObject, "Label_level");
		_level = gameObject.GetComponentInChildren<UILabel>();
		gameObject = Util.GetObject(base.gameObject, "Label_xp");
		_xp = gameObject.GetComponentInChildren<UILabel>();
		gameObject = Util.GetObject(base.gameObject, "XP bar");
		_progress = gameObject.GetComponentInChildren<UISlider>();
		_bgSaleObj = Util.GetObject(base.gameObject, "BeginnerSale");
		gameObject = Util.GetObject(_bgSaleObj, "Timer");
		_bgSaleTimer = gameObject.GetComponent<UILabel>();
		gameObject = Util.GetObject(_bgSaleObj, "Sticker");
		UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
		componentInChildren.text = Localizer.GetString("ID_UI_BEGINNERS_SALE_STATUSBAR_SALE");
		ButtonScript component = Util.GetObjectExactMatch(base.gameObject, "GetMoreButton").GetComponent<ButtonScript>();
		component.onButtonClick += OnGetMorePressed;
		_defaultGearsScale = _gears.transform.localScale;
		_defaultRivetsScale = _rivets.transform.localScale;
		_defaultXpScale = _xp.transform.localScale;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "HoF");
		UIEventListener uIEventListener = UIEventListener.Get(objectExactMatch);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnHoF));
		_hofObject = Util.GetObjectExactMatch(base.gameObject, "HoF");
		hofBadgeTexture = Util.GetObjectExactMatch(_hofObject, "Badge").GetComponent<UITexture>();
		hofText = Util.FindComponent<UILabel>(_hofObject);
		Update();
	}

	private void OnDestroy()
	{
		ReleaseHofTexture();
	}

	private void OnOpenScreen()
	{
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnReady));
		EnableInput(true);
	}

	private void EnableInput(bool enable)
	{
		InputEnabled = enable;
	}

	private void OnReady()
	{
		UpdateHoFElements();
	}

	private void OnLoginStateChanged()
	{
		_xpValue = -1;
		_levelValue = -1;
	}

	private void Update()
	{
		int softCurrency = Profile.Instance.SoftCurrency;
		if (checkSoftCurrency && softCurrency != _rivetsValue)
		{
			_rivetsValue = softCurrency;
			UIUtils.AssignTextAndResize(_rivets, softCurrency.ToString(), _defaultRivetsScale);
		}
		softCurrency = Profile.Instance.HardCurrency;
		if (softCurrency != _gearsValue)
		{
			_gearsValue = softCurrency;
			UIUtils.AssignTextAndResize(_gears, softCurrency.ToString(), _defaultGearsScale);
		}
		softCurrency = Profile.Instance.XP;
		if (softCurrency != _xpValue)
		{
			string text = Localizer.Format("ID_UI_STATUS_BAR_XP", softCurrency, Profile.Instance.NextLevelInfo.XP);
			_progress.sliderValue = Profile.Instance.LevelProgress;
			_xpValue = softCurrency;
			UIUtils.AssignTextAndResize(_xp, text, _defaultXpScale);
		}
		softCurrency = Profile.Instance.Level;
		if (softCurrency != _levelValue)
		{
			_level.text = Localizer.Format("ID_UI_STATUS_BAR_LEVEL", softCurrency);
			_levelValue = softCurrency;
		}
		if (Profile.Instance.HasBeginnerSaleExpired() || !Profile.Instance.HasBeginnerSaleStarted())
		{
			_bgSaleObj.SetActive(false);
		}
		else
		{
			_bgSaleObj.SetActive(true);
			softCurrency = Profile.Instance.UpdateBeginnerSaleTimeRemaining();
			_bgSaleTimer.text = Localizer.FormatTime(softCurrency, true);
		}
		UpdateHoFElements();
	}

	private bool RequireLogin()
	{
		switch (Hub.Instance.State)
		{
		case HubState.Connected:
			return true;
		case HubState.Idle:
			Setup.Instance.Login(false);
			break;
		}
		return false;
	}

	public void OnGetMorePressed(GameObject button)
	{
		if (InputEnabled && !blockCurrencyStoreForUi && ScreenManager.Instance.GetPopup("StoreCurrencyUI") == null && ScreenManager.Instance.GetPopup("VaultRewardsUI") == null && RequireLogin())
		{
			GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", false, true);
			StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
			storeCurrencyUI.currencyType = CurrencyType.Hard;
		}
	}

	public void UpdateHoFElements(bool forceTextureLoad = false)
	{
		int num = Profile.Instance.HoFLevel();
		if (!_hofObject.activeSelf)
		{
			_hofObject.SetActive(true);
		}
		if (forceTextureLoad || _hofLevel != num)
		{
			_hofLevel = num;
			ReleaseHofTexture();
			hofTexturePath = "badges/hof_level_" + _hofLevel;
			hofBadgeTexture.enabled = true;
			TexturePoolManager.Instance.LoadTexture(hofTexturePath, OnTextureLoaded);
		}
		if (hofText.text != Profile.Instance.HoFPoints.ToString())
		{
			hofText.text = Profile.Instance.HoFPoints.ToString();
		}
	}

	private void ReleaseHofTexture()
	{
		if (!string.IsNullOrEmpty(hofTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(hofTexturePath);
			hofTexturePath = null;
		}
	}

	private void OnTextureLoaded(Texture2D t)
	{
		if (hofBadgeTexture != null && t != null)
		{
			hofBadgeTexture.mainTexture = t;
			hofBadgeTexture.color = Color.white;
			hofBadgeTexture.enabled = true;
		}
	}

	private void OnHoF(GameObject go)
	{
		if (!blockCurrencyStoreForUi && InputEnabled && Profile.Instance.HoFLevel() > 0)
		{
			HomeUI.OpenWebPage(HomeUI.PageType.HallOfFame);
		}
	}
}
