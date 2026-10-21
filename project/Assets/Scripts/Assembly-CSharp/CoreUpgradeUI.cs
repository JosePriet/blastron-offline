using System;
using EB;
using UnityEngine;

public class CoreUpgradeUI : MonoBehaviour
{
	private bool _enableInput;

	private BackgroundUI _bgUI;

	private CacDisplay Cac;

	public CharacterPrefab skeletronPrefab;

	private Color OrigButtonColor;

	private void OnOpenPopup()
	{
		Cac = GetComponentInChildren<CacDisplay>();
		Cac.Recipe = skeletronPrefab.ToRecipe();
		Cac.SpawnPlayer(() =>
		{
			ScreenManager.Instance.FadeOut(null);
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
		});
	}

	private void OnTransitionInComplete()
	{
		_enableInput = true;
	}

	private void Start()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnBack;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ResetButton");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnReset;
		OrigButtonColor = Util.GetObjectExactMatch(objectExactMatch, "Background").GetComponent<UISlicedSprite>().color;
		GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
		_bgUI = Util.FindComponent<BackgroundUI>(obj);
		_bgUI.ShowCliff(false);
		RefreshScreen();
	}

	private void SetSkeletalCac()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CacDisplayInstance");
	}

	private void RefreshScreen()
	{
		UpdateDisplayedAttributes();
		UpdateCurrentSkillPoints();
		UpdateResetCost();
	}

	private void UpdateDisplayedAttributes()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "AttributesPanel");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Attributes_Upgrades");
		int num = 0;
		foreach (int value in Enum.GetValues(typeof(AttributesManager.statsNames)))
		{
			float num2 = Mathf.Round(AttributesManager.instance.allStats[num]);
			string text = ((RobotAttribute.RobotAttributeType)num).ToString();
			string spriteName = "attrib_" + text.ToLower();
			float num3 = Profile.Instance.GetRobotAttributeValue((RobotAttribute.RobotAttributeType)num);
			if (num != 2)
			{
				num3 *= 10f;
			}
			num2 += num3;
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch, "Attribute" + num);
			objectExactMatch3.SetActive(true);
			UIUtils.SetLabelContents(objectExactMatch3, "LabelAttributeName", "ID_UI_ATTRIBUTE_" + text.ToUpper());
			string labelText = ((num == 2) ? Localizer.Format("ID_UI_ATTRIBUTE_PLUS", (AttributesManager.instance._currentHealth + num3).ToString("0")) : Localizer.Format("ID_UI_ATTRIBUTE_PLUS", num2.ToString("0")));
			UIUtils.SetLabelContents(objectExactMatch3, "LabelChangedAttributeValue", labelText);
			Util.GetObjectExactMatch(objectExactMatch3, "AttributesSprite").GetComponent<UISprite>().spriteName = spriteName;
			objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch2, "Attribute_" + num);
			GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch3, "CurrentLevel");
			GameObject objectExactMatch5 = Util.GetObjectExactMatch(objectExactMatch3, "NextLevel");
			UIUtils.SetLabelContents(objectExactMatch3, "AttributeName", "ID_UI_ATTRIBUTE_" + text.ToUpper());
			Util.GetObjectExactMatch(objectExactMatch3, "AttributesSprite").GetComponent<UISprite>().spriteName = spriteName;
			UIUtils.SetLabelContents(objectExactMatch4, "LevelLabel", Localizer.Format("ID_UI_ITEM_LEVEL_FORMAT", ":"));
			UIUtils.SetLabelContents(objectExactMatch4, "AttributeLevel", AttributesManager.instance.attributeLevels[num]);
			UIUtils.SetLabelContents(objectExactMatch4, "AttributeMod", Localizer.Format("ID_UI_ATTRIBUTE_PLUS", Mathf.Round(AttributesManager.instance.allStats[num])));
			bool flag = (int)AttributesManager.instance.attributeLevels[num] >= Tuning.Instance.SkillPointsData.MaxAttributeLevel;
			bool CanAfford;
			int num4 = Mathf.RoundToInt(AttributesManager.instance.GetSkillUpgradeCost((AttributesManager.statsNames)value, out CanAfford));
			if (!flag)
			{
				UIUtils.SetLabelContents(objectExactMatch5, "LevelLabel", Localizer.Format("ID_UI_ITEM_LEVEL_FORMAT", ":"));
				UIUtils.SetLabelContents(objectExactMatch5, "AttributeLevel", AttributesManager.instance.attributeLevels[num] + 1);
				UIUtils.SetLabelContents(objectExactMatch5, "AttributeMod", Localizer.Format("ID_UI_ATTRIBUTE_PLUS", AttributesManager.instance.NextLevelAttributeMod((AttributesManager.statsNames)value)));
				GameObject objectExactMatch6 = Util.GetObjectExactMatch(objectExactMatch3, "UpgradeButton");
				if (objectExactMatch6 == null)
				{
					objectExactMatch6 = Util.GetObjectExactMatch(objectExactMatch3, ((AttributesManager.statsNames)value).ToString());
				}
				UIUtils.SetLabelContents(objectExactMatch6, "SPCost", num4.ToString());
				objectExactMatch6.name = ((AttributesManager.statsNames)value).ToString();
				objectExactMatch6.GetComponent<ButtonScript>().onButtonClick += OnUpgrade;
				GameObject objectExactMatch7 = Util.GetObjectExactMatch(objectExactMatch6, "Background");
				objectExactMatch7.GetComponent<UISlicedSprite>().color = ((!CanAfford) ? Color.white : OrigButtonColor);
				objectExactMatch7.collider.enabled = CanAfford;
			}
			UISlicedSprite component = Util.GetObjectExactMatch(objectExactMatch3, "Arrow").GetComponent<UISlicedSprite>();
			Color color = component.color;
			color.a = ((!flag && CanAfford) ? 1f : 0.5f);
			component.color = color;
			Util.GetObjectExactMatch(objectExactMatch3, "MaxLevel").SetActive(flag);
			Util.GetObjectExactMatch(objectExactMatch3, "ValidLevel").SetActive(!flag);
			num++;
		}
	}

	private void UpdateCurrentSkillPoints()
	{
		UIUtils.SetLabelContents(base.gameObject, "SPCount", AttributesManager.instance.currentSkillPoints);
	}

	private void UpdateResetCost()
	{
		UIUtils.SetLabelContents(Util.GetObjectExactMatch(base.gameObject, "ResetButton"), Tuning.Instance.SkillPointsData.ResetSkillPointsCost[AttributesManager.instance._timesReset]);
	}

	private void OnUpgrade(GameObject caller)
	{
		AttributesManager.statsNames type = (AttributesManager.statsNames)(int)Enum.Parse(typeof(AttributesManager.statsNames), caller.name);
		AttributesManager.instance.IncrementStat(type);
		RefreshScreen();
	}

	private void OnBack(GameObject caller)
	{
		if (_enableInput)
		{
			_enableInput = false;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnTransitionOutComplete);
		}
	}

	private void OnReset(GameObject caller)
	{
		ScreenManager.Instance.Confirmation(true, "ID_UI_RESET_SKILLPOINTS", "ID_UI_RESET_SKILLPOINTS_CONFIRMATION", "ID_UI_COMMON_YES", "ID_UI_COMMON_NO", (ConfirmationDialogUI.ButtonIds button) =>
		{
			if (button == ConfirmationDialogUI.ButtonIds.Left)
			{
				ScreenManager.Instance.BlockingUI("ID_UI_LOGIN_BLOCKING_TITLE", "ID_UI_LOGIN_BLOCKING_DESC");
				WalletListener.Instance.Debit(Tuning.Instance.SkillPointsData.ResetSkillPointsCost[AttributesManager.instance._timesReset], "Reset Skill Points", SafeAction.Wrap(this, (bool result) =>
				{
					ScreenManager.Instance.ClosePopup("BlockingUI");
					if (result)
					{
						AttributesManager.instance.ResetAllSkills();
						RefreshScreen();
					}
					else
					{
						InsufficientCurrency(CurrencyType.Hard, Tuning.Instance.SkillPointsData.ResetSkillPointsCost[AttributesManager.instance._timesReset]);
					}
				}));
			}
		}, false);
	}

	private void InsufficientCurrency(CurrencyType currencyType, int cost)
	{
		GameObject gameObject = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true, true);
		if (gameObject != null)
		{
			CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(gameObject);
			currencyInsufficientUI.SetInfo(currencyType, cost);
		}
	}

	private void OnTransitionOutComplete()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			_bgUI.ShowCliff(true);
			ScreenManager.Instance.ClosePopup();
		});
	}
}
