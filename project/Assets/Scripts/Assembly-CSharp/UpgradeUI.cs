using System;
using System.Collections.Generic;
using System.Linq;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class UpgradeUI : PopupScript
{
	public AudioEvent _spinningAudioEvent;

	public AudioEvent _climaxAudioEvent;

	public AudioEvent _upgradeProgressAudioEvent;

	private MyRobotUI parentScreenMR;

	private bool _enableInput = true;

	private AudioEmitter _audioEmitter;

	private List<RobotAttribute> _attributes;

	private List<GameObject> _statContainers;

	private GameObject _attributeObj;

	private GameObject softButtonObj;

	private GameObject hardButtonObj;

	private GameObject freeButtonObj;

	private ButtonScript closeButtonScript;

	private ButtonScript softButtonScript;

	private List<ButtonScript> disableButtonsDuringAnimation;

	private int partHash;

	private CurrencyType purchaseType;

	private UpgradeProgressBar upgradeProgressBar;

	private CacGridItem upgradeCurrentItem;

	private CacGridItem upgradeToItem;

	private bool showUnlockPopupForTier;

	private CharacterDB.MetaData _md;

	private bool _canUpgrade = true;

	private bool _canExit = true;

	public List<RobotAttribute> attributes
	{
		get
		{
			if (_attributes == null && _md != null)
			{
				_attributes = PartManager.Instance.GetPartAttributes(_md.hash);
				if (_attributes == null)
				{
					_attributes = new List<RobotAttribute>();
				}
			}
			return _attributes;
		}
	}

	public List<GameObject> statContainers
	{
		get
		{
			if (_statContainers == null)
			{
				GameObject objectExactMatch = Util.GetObjectExactMatch(upgradeCurrentItem.gameObject, "ItemStats");
				GameObject[] objects = Util.GetObjects(objectExactMatch, "Stat");
				_statContainers = objects.ToList();
				_statContainers.Sort(StatContainerComparison);
				_statContainers.Remove(objectExactMatch);
			}
			return _statContainers;
		}
	}

	private bool CanUpgrade
	{
		get
		{
			return _canUpgrade;
		}
		set
		{
			_canUpgrade = value;
			foreach (ButtonScript item in disableButtonsDuringAnimation)
			{
				item.SetEnabled(value);
			}
		}
	}

	private bool CanExit
	{
		get
		{
			return _canExit;
		}
		set
		{
			_canExit = value;
			closeButtonScript.SetEnabled(value);
		}
	}

	public void SetParameters(CharacterDB.MetaData partMetadata)
	{
		_md = partMetadata;
		if (_md != null)
		{
			upgradeProgressBar.SetMetadata(partMetadata);
			partHash = _md.hash;
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Label_PartName");
			UILabel uILabel = Util.FindComponent<UILabel>(objectExactMatch);
			UIUtils.AssignTextAndResize(uILabel, _md.friendlyName, uILabel.transform.localScale);
			_attributeObj = Util.GetObjectExactMatch(base.gameObject, "Attribute");
			float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
			UpdateProgressBars(partUpgradeProgress);
			UpdateCosts(partUpgradeProgress);
			upgradeCurrentItem.metaData = _md;
			upgradeCurrentItem.UpdateArt();
			upgradeCurrentItem.Unload();
			upgradeCurrentItem.texturePath = "cac_ui_parts/" + _md.cacTexturePath;
			upgradeCurrentItem.Load();
			upgradeToItem.metaData = _md.nextTierItem;
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "UpgradeAnchors");
			if (upgradeToItem.metaData != null)
			{
				upgradeToItem.gameObject.SetActive(true);
				upgradeToItem.UpdateArt();
				upgradeToItem.Unload();
				upgradeToItem.texturePath = "cac_ui_parts/" + upgradeToItem.metaData.cacTexturePath;
				upgradeToItem.Load();
				upgradeCurrentItem.gameObject.transform.localPosition = Util.GetObjectExactMatch(objectExactMatch2, "Left").transform.localPosition;
				upgradeToItem.gameObject.transform.localPosition = Util.GetObjectExactMatch(objectExactMatch2, "Right").transform.localPosition;
				NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "Label_UpgradeHint"), true);
			}
			else
			{
				upgradeCurrentItem.gameObject.transform.localPosition = Util.GetObjectExactMatch(objectExactMatch2, "Center").transform.localPosition;
				upgradeToItem.gameObject.SetActive(false);
				NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "Label_UpgradeHint"), false);
			}
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		disableButtonsDuringAnimation = new List<ButtonScript>();
		parentScreenMR = UnityEngine.Object.FindObjectOfType(typeof(MyRobotUI)) as MyRobotUI;
		softButtonObj = Util.GetObjectExactMatch(base.gameObject, "ButtonSoftCurrency");
		hardButtonObj = Util.GetObjectExactMatch(base.gameObject, "ButtonHardCurrency");
		freeButtonObj = Util.GetObjectExactMatch(base.gameObject, "ButtonFree");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(softButtonObj);
		buttonScript.onButtonClick += DoUpgradeSoft;
		disableButtonsDuringAnimation.Add(buttonScript);
		softButtonScript = buttonScript;
		buttonScript = Util.FindComponent<ButtonScript>(hardButtonObj);
		buttonScript.onButtonClick += DoUpgradeHard;
		disableButtonsDuringAnimation.Add(buttonScript);
		buttonScript = Util.FindComponent<ButtonScript>(freeButtonObj);
		buttonScript.onButtonClick += DoUpgradeFree;
		disableButtonsDuringAnimation.Add(buttonScript);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		closeButtonScript = buttonScript;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ProgressBar");
		upgradeProgressBar = Util.FindComponent<UpgradeProgressBar>(objectExactMatch2);
		objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "UpgradeCurrentItem");
		upgradeCurrentItem = Util.FindComponent<CacGridItem>(objectExactMatch2);
		objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "UpgradeToItem");
		upgradeToItem = Util.FindComponent<CacGridItem>(objectExactMatch2);
		CanExit = true;
		_audioEmitter = GetComponent<AudioEmitter>();
	}

	protected override void OnTransitionInComplete()
	{
		base.OnTransitionInComplete();
		if (!Profile.Instance.IsTutorialDone("sequence_tutorial_upgradeparts"))
		{
			if (Profile.Instance.GetPartUpgradeProgress(partHash) > 0f)
			{
				Profile.Instance.MarkTutorialDone("sequence_tutorial_upgradeparts");
				return;
			}
			UnityEngine.Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_upgradeparts");
			UnityEngine.Object.Instantiate(original);
		}
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		string textureName = "cac_ui_parts/" + _md.cacTexturePath;
		TexturePoolManager.Instance.ReleaseTexture(textureName);
		parentScreenMR.RefreshData();
		EB.Sequence.Component.Activate(base.gameObject, "UpgradeUI", typeof(SequenceEvent_OnScreenClosed));
	}

	private void UpdateCosts(float progress)
	{
		EB.Debug.Log("UpdateCosts -> progress: " + progress);
		CanUpgrade = progress < 100f;
		int partUpgradeCostSoft = PartManager.Instance.GetPartUpgradeCostSoft(partHash, progress);
		if (progress == 0f && _md.previousTierItem == null)
		{
			EB.Debug.Log("UpdateCosts -> this upgrade is free.");
			freeButtonObj.SetActive(true);
			softButtonObj.SetActive(false);
			hardButtonObj.SetActive(false);
		}
		else
		{
			freeButtonObj.SetActive(false);
			if (partUpgradeCostSoft == 0 || partUpgradeCostSoft >= Tuning.Instance.HiddenSoftCostThreshold)
			{
				softButtonObj.SetActive(false);
				Vector3 localPosition = hardButtonObj.transform.localPosition;
				localPosition.x = 0f;
				hardButtonObj.transform.localPosition = localPosition;
			}
			else
			{
				softButtonObj.SetActive(true);
				UIUtils.SetLabelContents(softButtonObj, "LabelAmount", partUpgradeCostSoft);
			}
			hardButtonObj.SetActive(true);
			UIUtils.SetLabelContents(hardButtonObj, "LabelAmount", PartManager.Instance.GetPartUpgradeCostHard(partHash, progress));
		}
		CanUpgrade = CanUpgrade;
		if (partUpgradeCostSoft > Profile.Instance.SoftCurrency)
		{
			softButtonScript.SetEnabled(false);
		}
	}

	private void UpdateProgressBars(float progress)
	{
		EB.Debug.Log("> UpgradeUI > UpdateProgressBars > progress: " + progress);
		float value = progress / 100f;
		upgradeProgressBar.SetValue(value);
		float nextUpgrade = PartManager.Instance.GetPartUpgradeMaxResult(_md.hash) / 100f;
		upgradeProgressBar.SetNextUpgrade(nextUpgrade);
	}

	private void DoExit()
	{
		if (CanExit)
		{
			ScreenManager.Instance.ClosePopup("UpgradeUI");
		}
	}

	private void DoUpgradeDebugFull()
	{
		if (!_enableInput || !CanUpgrade)
		{
			return;
		}
		Profile.Instance.SetPartUpgradeProgress(partHash, 100f);
		Profile.Instance.Save();
		GameObject popup = ScreenManager.Instance.GetPopup("MyRobotUpgradeUI");
		if (popup != null)
		{
			MyRobotUpgradeUI myRobotUpgradeUI = Util.FindComponent<MyRobotUpgradeUI>(popup);
			if (myRobotUpgradeUI != null)
			{
				myRobotUpgradeUI.RefreshPartsData();
			}
		}
		UpdateAchievement();
		DisplayUpgrade(100f);
	}

	private void DoUpgradeFree(GameObject caller)
	{
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		EB.Debug.Log("UpgradeUI > DoUpgradeFree: " + (_enableInput && CanUpgrade) + " progress:" + partUpgradeProgress);
		if (_enableInput && CanUpgrade && partUpgradeProgress == 0f)
		{
			OnTransactionComplete(CurrencyType.Soft, 0, true);
		}
	}

	private void DoUpgradeSoft(GameObject caller)
	{
		EB.Debug.Log("UpgradeUI > DoUpgradeSoft: " + (_enableInput && CanUpgrade));
		if (_enableInput && CanUpgrade)
		{
			purchaseType = CurrencyType.Soft;
			float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
			int partUpgradeCostSoft = PartManager.Instance.GetPartUpgradeCostSoft(partHash, partUpgradeProgress);
			if (partUpgradeCostSoft <= Profile.Instance.SoftCurrency)
			{
				OnTransactionComplete(CurrencyType.Soft, partUpgradeCostSoft, Profile.Instance.RemoveSoftCurrency(partUpgradeCostSoft));
			}
		}
	}

	private void DoUpgradeHard(GameObject caller)
	{
		EB.Debug.Log("UpgradeUI > DoUpgradeHard: " + (_enableInput && CanUpgrade));
		if (_enableInput && CanUpgrade)
		{
			CanExit = false;
			CanUpgrade = false;
			purchaseType = CurrencyType.Hard;
			BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
			component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
			float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
			int cost = PartManager.Instance.GetPartUpgradeCostHard(partHash, partUpgradeProgress);
			WalletListener.Instance.Debit(cost, "upgrade: " + _md.name, (bool r) =>
			{
				OnTransactionComplete(CurrencyType.Hard, cost, r);
			});
		}
	}

	protected override void Close(GameObject caller)
	{
		if (CanExit && _enableInput)
		{
			base.Close(caller);
		}
	}

	private void OnTransactionComplete(CurrencyType type, int cost, bool result)
	{
		EB.Debug.Log("UpgradeUI > OnTransactionComplete > result: " + result);
		ScreenManager.Instance.ClosePopup("BlockingUI");
		if (result)
		{
			float upgradeRatio = 0f;
			float num = PartManager.Instance.Upgrade(partHash, out upgradeRatio);
			GameObject popup = ScreenManager.Instance.GetPopup("MyRobotUpgradeUI");
			if (popup != null)
			{
				MyRobotUpgradeUI myRobotUpgradeUI = Util.FindComponent<MyRobotUpgradeUI>(popup);
				if (myRobotUpgradeUI != null)
				{
					myRobotUpgradeUI.RefreshPartsData();
				}
			}
			if (cost > 0)
			{
				Telemetry.Event("upgrade_part");
				Telemetry.Event("upgrade/" + _md.name);
				if (type == CurrencyType.Hard)
				{
					Telemetry.Event("upgrade_hard/" + _md.name);
					Telemetry.Event("spend_hard", cost);
					Telemetry.Event("spend_hard_src/upgrade", cost);
				}
				else
				{
					Telemetry.Event("upgrade_soft/" + _md.name);
					Telemetry.Event("spend_soft", cost);
					Telemetry.Event("spend_soft_src/upgrade", cost);
				}
				Telemetry.Flush();
			}
			else
			{
				Telemetry.Event("upgrade_part_free");
			}
			UpdateAchievement();
			StatManager.Instance.IncrementStat(StatManager.Stat.Upgrades);
			DisplayUpgrade(upgradeRatio);
		}
		else
		{
			InsufficientCurrency(type, cost);
			CanUpgrade = true;
			CanExit = true;
		}
	}

	private void InsufficientCurrency(CurrencyType currencyType, int cost)
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true, true);
		CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(obj);
		currencyInsufficientUI.SetInfo(currencyType, cost);
	}

	private void UpdateUpgradeBars(float fillAmount)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(_attributeObj, "ProgressBar");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "upgradebar");
		UIFilledSprite component = objectExactMatch2.GetComponent<UIFilledSprite>();
		component.fillAmount = fillAmount;
	}

	private void UpdateAchievement()
	{
		Hub.Instance.GameCenterManager.ReportProgress("UPGRADE_1", 100.0);
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		float num = partUpgradeProgress / 100f;
		if (!(num >= 1f))
		{
			return;
		}
		Hub.Instance.GameCenterManager.ReportProgress("UPGRADE_FULL", 100.0);
		List<CharacterDB.MetaData> allPartsByTier = CharacterDB.GetAllPartsByTier(_md.category, _md.Tier + 1);
		bool flag = allPartsByTier.Count > 0;
		bool flag2 = allPartsByTier.Find((CharacterDB.MetaData item) => !item.IsLocked) == null;
		showUnlockPopupForTier = flag && flag2;
		CharacterDB.MetaData.CalculateUnlockStructure();
		CharacterDB.MetaData nextPart = _md.nextTierItem;
		if (nextPart == null)
		{
			return;
		}
		if (showUnlockPopupForTier)
		{
			showUnlockPopupForTier = false;
			GameObject obj = ScreenManager.Instance.OpenPopup("UpgradeTierUnlockedUI", true, true);
			UpgradeTierUnlockedUI upgradeTierUnlockedUI = Util.FindComponent<UpgradeTierUnlockedUI>(obj);
			upgradeTierUnlockedUI.SetParameters(nextPart.category, nextPart.Tier, _md);
			upgradeTierUnlockedUI.equippedPartNextTier = nextPart;
			return;
		}
		string desc = Localizer.Format("ID_UI_UPGRADE_BUY_NEXT_TIER", Localizer.GetString(_md.friendlyName), Localizer.GetString(nextPart.friendlyName));
		ScreenManager.Instance.Confirmation(false, "ID_UI_LEVELUP_CONGRATS", desc, "ID_UI_RESULTS_STORE", "ID_UI_COMMON_OK", (ConfirmationDialogUI.ButtonIds buttonId) =>
		{
			ScreenManager.Instance.ClosePopup("UpgradeUI");
			if (buttonId == ConfirmationDialogUI.ButtonIds.Left)
			{
				MyRobotUI component = ScreenManager.Instance.GetPopup("MyRobotUI").GetComponent<MyRobotUI>();
				component.ForceOwned(false);
				component.CenterOnNewUnlockedPart(nextPart);
				if (ScreenManager.Instance.GetPopup("MyRobotUpgradeUI") != null)
				{
					component.gameObject.SendMessage("OnSubscreenClose");
					ScreenManager.Instance.ClosePopup("MyRobotUpgradeUI");
				}
			}
		});
	}

	private void DisplayUpgrade(float upgradeBracketRatio)
	{
		CanExit = false;
		CanUpgrade = false;
		EB.Debug.Log("UpgradeUI > DisplayUpgrade");
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		float newVal = partUpgradeProgress / 100f;
		upgradeProgressBar.ShowUpgrade(newVal, SafeAction.Wrap(this, OnUpgradeDisplayComplete));
	}

	private void OnUpgradeDisplayComplete()
	{
		EB.Debug.Log("UpgradeUI > OnUpgradeDisplayComplete");
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		if (partUpgradeProgress < 100f)
		{
			CanUpgrade = true;
		}
		UpdateCosts(partUpgradeProgress);
		UpdateProgressBars(partUpgradeProgress);
		UpdateStatsInfo();
		CanExit = true;
		int partUpgradeXPReward = PartManager.Instance.GetPartUpgradeXPReward(partHash, partUpgradeProgress);
		SeXP.CreateAtVP(new Vector3(0.5f, 0.15f, 0f), partUpgradeXPReward);
		SequenceEvent_OnUpgradeComplete.PartEventInfo target = new SequenceEvent_OnUpgradeComplete.PartEventInfo(_md.category, _md.Tier);
		EB.Sequence.Component.Activate(base.gameObject, target, typeof(SequenceEvent_OnUpgradeComplete));
	}

	private void UpdateStatsInfo()
	{
		for (int i = 0; i < attributes.Count; i++)
		{
			int num = Convert.ToInt32(UIUtils.GetLabelContents(statContainers[i], "Value"));
			int num2 = Mathf.FloorToInt(PartManager.Instance.GetUpgradedPartAttributeValue(_md.hash, attributes[i].type));
			if (attributes[i].type != RobotAttribute.RobotAttributeType.Health)
			{
				num2 *= 10;
			}
			if (num < num2)
			{
				TintAnimation tintAnimation = Util.FindComponent<TintAnimation>(statContainers[i]);
				if (tintAnimation != null)
				{
					tintAnimation.Animate();
				}
				UIUtils.SetLabelContents(statContainers[i], "Value", num2);
			}
		}
	}

	private static int StatContainerComparison(GameObject g1, GameObject g2)
	{
		return g1.name.CompareTo(g2.name);
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void OnFETutorialFreeUpgrade()
	{
		OnTransactionComplete(CurrencyType.Soft, 0, true);
	}

	private void OnTutorialClose()
	{
		OnClose();
	}
}
