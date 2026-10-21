using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class MyRobotUI : MonoBehaviour
{
	private const int UnselectedTabDepth = 0;

	private const int SelectedTabDepth = 1;

	private const string TabEnabledSprite = "UI_tab_ON";

	private const string TabDisabledSprite = "UI_tab_OFF";

	private const int MaxAttributesShown = 5;

	public Color statTintNoChange = Color.white;

	public Color statTintIncrease = new Color(0.2f, 1f, 0.2f, 1f);

	public Color statTintDecrease = new Color(1f, 0.2f, 0.2f, 1f);

	private UIImageButton previouslyHighlightedTab;

	private Dictionary<string, int> categoryTabNameToIndex = new Dictionary<string, int>();

	private CacItemFactory cacItemFactory;

	private int currentlyEquippedItemHash;

	private int previewPartHash = -1;

	private bool inSubScreen;

	private CacDisplay cacDisplay;

	private int upgradingPart;

	private UIScrollBar scrollBar;

	private CacUiItem detailedItem;

	private bool mScrollbarDragging;

	private bool refreshTable;

	private TableHelperUI tableHelper;

	public bool showOwned = true;

	private GameObject toggleOwnedButton;

	private GameObject arrowLeft;

	private GameObject arrowRight;

	private GameObject upgradeScreenButton;

	private List<UISprite> backgroundsToTint;

	private bool _enableInput = true;

	private Vector3 initialGridPosition = Vector3.zero;

	private Vector4 initialGridClipping = Vector4.zero;

	private bool _transitionedIn;

	private bool isPreviewingPart
	{
		get
		{
			return previewPartHash != -1;
		}
	}

	private void OnOpenPopup()
	{
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
	}

	private void OnTransitionInComplete()
	{
		if (showOwned && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts_storeintro") && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts_myrobotintro") && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts"))
		{
			UnityEngine.Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_parts_myrobotintro");
			UnityEngine.Object.Instantiate(original);
		}
		DoPartsTutorial();
		DoUpgradeTutorial();
		_transitionedIn = true;
	}

	private void DoPartsTutorial()
	{
		if (!showOwned && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts"))
		{
			if (PartsTutorialCheck())
			{
				Profile.Instance.MarkTutorialDone("sequence_tutorial_parts");
				return;
			}
			UnityEngine.Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_parts");
			UnityEngine.Object.Instantiate(original);
		}
	}

	private void DoUpgradeTutorial()
	{
		if (showOwned && Profile.Instance.IsTutorialDone("part_bought") && !Profile.Instance.IsTutorialDone("sequence_tutorial_myrobot"))
		{
			UnityEngine.Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_myrobot");
			UnityEngine.Object.Instantiate(original);
		}
	}

	private bool PartsTutorialCheck()
	{
		return Hub.Instance.InventoryManager.GetCount("cholotron_head_1") > 0 || Hub.Instance.InventoryManager.GetCount("lancebot_head_1") > 0 || Hub.Instance.InventoryManager.GetCount("steambot_head_1") > 0 || Hub.Instance.InventoryManager.GetCount("donbot_head_1") > 0 || Hub.Instance.InventoryManager.GetCount("bettybot_head_1") > 0 || Hub.Instance.InventoryManager.GetCount("samuroid_head_1") > 0;
	}

	private void Awake()
	{
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		arrowLeft = Util.GetObjectExactMatch(base.gameObject, "ArrowButtonLeft");
		arrowRight = Util.GetObjectExactMatch(base.gameObject, "ArrowButtonRight");
		backgroundsToTint = new List<UISprite>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackgroundTinted");
		InitBackgroundTint(objectExactMatch);
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemBackground");
		InitBackgroundTint(objectExactMatch);
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemNameBackground");
		InitBackgroundTint(objectExactMatch);
		GameObject[] objects = Util.GetObjects(base.gameObject, "CheckboxCategoryTab");
		GameObject[] array = objects;
		foreach (GameObject obj in array)
		{
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(obj, "Unselected");
			objectExactMatch = Util.GetObjectExactMatch(objectExactMatch2, "Background");
			InitBackgroundTint(objectExactMatch);
		}
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Grid");
		if (objectExactMatch3 != null)
		{
			initialGridPosition = objectExactMatch3.transform.localPosition;
			UIPanel component = objectExactMatch3.GetComponent<UIPanel>();
			if (component != null)
			{
				initialGridClipping = component.clipRange;
			}
		}
		InitializeStaticGui();
	}

	private void InitBackgroundTint(GameObject bgObj)
	{
		if (bgObj != null)
		{
			UISprite component = bgObj.GetComponent<UISprite>();
			if (component != null)
			{
				backgroundsToTint.Add(component);
			}
		}
	}

	private void Start()
	{
		detailedItem = Util.FindComponent<CacUiItem>(base.gameObject);
		detailedItem.metaData = null;
		detailedItem.UpdateArt();
		detailedItem.buyButton.onButtonClick += OnBuy;
		detailedItem.equipButton.onButtonClick += OnEquip;
		if (detailedItem.upgradeButton != null)
		{
			detailedItem.upgradeButton.onButtonClick += OnUpgrade;
		}
		cacItemFactory = Util.FindComponent<CacItemFactory>(base.gameObject);
		cacDisplay = Util.FindComponent<CacDisplay>(base.gameObject);
		cacDisplay.PlayBoredAnimations = false;
		cacDisplay.SwapFace = false;
		tableHelper.SetMaxVisibleItems(6);
		tableHelper.SetDelta(2);
		UpdateBackgroundTint();
		UIUtils.SetLabelContents(toggleOwnedButton, "Label", (!showOwned) ? "ID_UI_CAC_OWNED" : "ID_UI_CAC_STORE");
		GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
		backgroundUI.ShowCliff(false);
		ScreenManager.Instance.FadeOut(null);
	}

	private void OnInventoryUpdated()
	{
		UpdateArt(upgradingPart);
	}

	private void InitializeStaticGui()
	{
		List<string> list = new List<string>();
		UICheckbox[] array = Util.FindAllComponents<UICheckbox>(base.gameObject);
		foreach (UICheckbox uICheckbox in array)
		{
			if (uICheckbox.name.Contains("CheckboxCategoryTab"))
			{
				list.Add(uICheckbox.name);
				Util.FindComponent<TabButtonScript>(uICheckbox.gameObject).onTabSelected += OnChangeCategory;
			}
		}
		list.Sort();
		for (int j = 0; j < list.Count; j++)
		{
			categoryTabNameToIndex.Add(list[j], j);
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "MyStatsButton");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnMyStats;
		NGUITools.SetActive(objectExactMatch, false);
		toggleOwnedButton = Util.GetObjectExactMatch(base.gameObject, "ToggleOwnedButton");
		buttonScript = Util.FindComponent<ButtonScript>(toggleOwnedButton);
		buttonScript.onButtonClick += OnToggleOwned;
		upgradeScreenButton = Util.GetObjectExactMatch(base.gameObject, "UpgradeScreenButton");
		buttonScript = Util.FindComponent<ButtonScript>(upgradeScreenButton);
		buttonScript.onButtonClick += OnUpgradeScreen;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch2);
		buttonScript.onButtonClick += OnBack;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar");
		if (objectExactMatch3 != null)
		{
			scrollBar = Util.GetObjectExactMatch(base.gameObject, "Scroll Bar").GetComponent<UIScrollBar>();
			UIEventListener uIEventListener = UIEventListener.Get(scrollBar.background.gameObject);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnScrollDrag));
		}
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "ArrowButtonLeft");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch4);
		buttonScript.onButtonClick += OnArrowLeft;
		GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "ArrowButtonRight");
		buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch5);
		buttonScript.onButtonClick += OnArrowRight;
	}

	private void OnScrollDrag(GameObject caller, bool pressed)
	{
		if (_enableInput && pressed)
		{
			mScrollbarDragging = true;
		}
	}

	public void UpdateAttributes()
	{
		if (!isPreviewingPart)
		{
			UpdateEquippedAttributes();
		}
		else
		{
			UpdateComparedAttributes(previewPartHash);
		}
	}

	public void RefreshData()
	{
		UpdateAttributes();
		ResetCacDisplay();
		cacItemFactory.RefreshMetadataCache(showOwned);
		cacItemFactory.Reset();
		ResetTable();
		if (detailedItem != null)
		{
			detailedItem.UpdateArt();
			if (detailedItem.metaData != null)
			{
				int hash = detailedItem.metaData.hash;
				UpdateArt(hash);
			}
		}
	}

	private void UpdateArt(int hash)
	{
		CacGridItem item = CacGridItem.GetItem(hash);
		if ((bool)item)
		{
			item.UpdateArt();
		}
	}

	private void UpdateHighlighted(int hash, bool hl)
	{
		CacGridItem item = CacGridItem.GetItem(hash);
		if ((bool)item)
		{
			item.isHighlighted = hl;
			item.UpdateArt();
		}
	}

	public void EquipItem(CharacterDB.MetaData part)
	{
		if (currentlyEquippedItemHash != part.hash)
		{
			int num = previewPartHash;
			if (num == -1)
			{
				num = currentlyEquippedItemHash;
			}
			CharacterManager.Instance.LocalPlayer.recipe.SetPart(part, Profile.Instance.GetPartUpgradeProgress(part.hash));
			CharacterManager.Instance.UpdateLocalCharacter();
			cacDisplay.Recipe = null;
			cacDisplay.SpawnPlayer();
			UpdateArt(currentlyEquippedItemHash);
			currentlyEquippedItemHash = part.hash;
			UpdateArt(currentlyEquippedItemHash);
			UpdatePreviewInGrid(num, currentlyEquippedItemHash);
			previewPartHash = -1;
			UpdateAttributes();
		}
	}

	public void PreviewPart(CharacterDB.Category category, int hash)
	{
		int oldHash = previewPartHash;
		if (hash == currentlyEquippedItemHash)
		{
			previewPartHash = -1;
			CharacterDB.Recipe recipe = CharacterManager.Instance.LocalPlayer.recipe;
			cacDisplay.Recipe = recipe;
			cacDisplay.SpawnPlayer();
		}
		else
		{
			previewPartHash = hash;
			CharacterDB.Recipe recipe2 = new CharacterDB.Recipe();
			CharacterDB.Recipe recipe3 = CharacterManager.Instance.LocalPlayer.recipe;
			for (int i = 0; i < recipe3.parts.Length; i++)
			{
				recipe2.parts[i] = recipe3.parts[i];
			}
			for (int j = 0; j < recipe3.UpgradeProgress.Length; j++)
			{
				recipe2.UpgradeProgress[j] = recipe3.UpgradeProgress[j];
			}
			recipe2.parts[(int)category] = hash;
			recipe2.UpgradeProgress[(int)category] = 0f;
			cacDisplay.Recipe = recipe2;
			cacDisplay.SpawnPlayer();
		}
		UpdatePreviewInGrid(oldHash, previewPartHash);
		UpdateAttributes();
	}

	private void ResetCacDisplay()
	{
		if (isPreviewingPart)
		{
			int oldHash = previewPartHash;
			previewPartHash = -1;
			cacDisplay.Recipe = null;
			cacDisplay.SpawnPlayer();
			ResetTable();
			UpdatePreviewInGrid(oldHash, previewPartHash);
		}
	}

	private void UpdatePreviewInGrid(int oldHash, int newHash)
	{
		CharacterDB.Recipe recipe = CharacterManager.Instance.LocalPlayer.recipe;
		int num = recipe.parts[(int)cacItemFactory.cacCategory];
		if (oldHash == -1)
		{
			oldHash = num;
		}
		if (newHash == -1)
		{
			newHash = num;
		}
	}

	private void UpdateEquippedAttributes()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "AttributesPanel");
		int i = 0;
		foreach (int value in Enum.GetValues(typeof(RobotAttribute.RobotAttributeType)))
		{
			float robotAttributeValue = Profile.Instance.GetRobotAttributeValue((RobotAttribute.RobotAttributeType)value);
			float num = Mathf.Round(AttributesManager.instance.allStats[i]);
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Attribute" + i);
			objectExactMatch2.SetActive(true);
			UIUtils.SetLabelContents(objectExactMatch2, "LabelAttributeName", "ID_UI_ATTRIBUTE_" + ((RobotAttribute.RobotAttributeType)value).ToString().ToUpper());
			if (value == 2)
			{
				robotAttributeValue += AttributesManager.instance._currentHealth;
				string labelText = Localizer.Format("ID_UI_ATTRIBUTE_PLUS", robotAttributeValue.ToString("0"));
				UIUtils.SetLabelContents(objectExactMatch2, "LabelAttributeValue", labelText);
			}
			else
			{
				robotAttributeValue *= 10f;
				robotAttributeValue += num;
				string labelText2 = Localizer.Format("ID_UI_ATTRIBUTE_PLUS", robotAttributeValue.ToString("0"));
				UIUtils.SetLabelContents(objectExactMatch2, "LabelAttributeValue", labelText2);
			}
			UIUtils.SetLabelContents(objectExactMatch2, "LabelChangedAttributeValue", string.Empty);
			Util.GetObjectExactMatch(objectExactMatch2, "SpriteArrowTintable").SetActive(false);
			Util.GetObjectExactMatch(objectExactMatch2, "SpriteArrowOutline").SetActive(false);
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch2, "SpriteAttributes");
			Util.FindComponent<UISprite>(objectExactMatch3).spriteName = "attrib_" + ((RobotAttribute.RobotAttributeType)value).ToString().ToLower();
			i++;
		}
		for (; i < 5; i++)
		{
			GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch, "Attribute" + i);
			objectExactMatch4.SetActive(false);
		}
	}

	private void UpdateComparedAttributes(int compareToHash)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "AttributesPanel");
		int part = CharacterManager.Instance.LocalPlayer.recipe.GetPart(cacItemFactory.cacCategory);
		int i = 0;
		UIPanel uiPanel = null;
		foreach (int value in Enum.GetValues(typeof(RobotAttribute.RobotAttributeType)))
		{
			float robotAttributeValue = Profile.Instance.GetRobotAttributeValue((RobotAttribute.RobotAttributeType)value);
			float num = Mathf.Round(AttributesManager.instance.allStats[i]);
			float num2 = robotAttributeValue;
			num2 -= (float)(int)PartManager.Instance.GetUpgradedPartAttributeValue(part, (RobotAttribute.RobotAttributeType)value);
			num2 += (float)(int)PartManager.Instance.GetUpgradedPartAttributeValue(compareToHash, (RobotAttribute.RobotAttributeType)value);
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Attribute" + i);
			objectExactMatch2.SetActive(true);
			UIUtils.SetLabelContents(objectExactMatch2, "LabelAttributeName", "ID_UI_ATTRIBUTE_" + ((RobotAttribute.RobotAttributeType)value).ToString().ToUpper());
			if (value == 2)
			{
				robotAttributeValue += AttributesManager.instance._currentHealth;
				num2 += AttributesManager.instance._currentHealth;
				string labelText = Localizer.Format("ID_UI_ATTRIBUTE_PLUS", robotAttributeValue.ToString("0"));
				UIUtils.SetLabelContents(objectExactMatch2, "LabelAttributeValue", labelText);
				labelText = Localizer.Format("ID_UI_ATTRIBUTE_PLUS", num2.ToString("0"));
				UIUtils.SetLabelContents(objectExactMatch2, "LabelChangedAttributeValue", labelText);
			}
			else
			{
				robotAttributeValue *= 10f;
				num2 *= 10f;
				robotAttributeValue += num;
				num2 += num;
				string labelText2 = Localizer.Format("ID_UI_ATTRIBUTE_PLUS", robotAttributeValue.ToString("0"));
				UIUtils.SetLabelContents(objectExactMatch2, "LabelAttributeValue", labelText2);
				labelText2 = Localizer.Format("ID_UI_ATTRIBUTE_PLUS", num2.ToString("0"));
				UIUtils.SetLabelContents(objectExactMatch2, "LabelChangedAttributeValue", labelText2);
			}
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch2, "LabelChangedAttributeValue");
			UILabel uILabel = Util.FindComponent<UILabel>(objectExactMatch3);
			GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch2, "SpriteArrowTintable");
			GameObject objectExactMatch5 = Util.GetObjectExactMatch(objectExactMatch2, "SpriteArrowOutline");
			UISprite uISprite = Util.FindComponent<UISprite>(objectExactMatch4);
			if (num2 < robotAttributeValue)
			{
				uILabel.color = statTintDecrease;
				uISprite.color = statTintDecrease;
				objectExactMatch4.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
				objectExactMatch4.SetActive(true);
				objectExactMatch5.SetActive(true);
			}
			else if (num2 > robotAttributeValue)
			{
				uILabel.color = statTintIncrease;
				uISprite.color = statTintIncrease;
				objectExactMatch4.transform.localEulerAngles = Vector3.zero;
				objectExactMatch4.SetActive(true);
				objectExactMatch5.SetActive(true);
			}
			else
			{
				uILabel.color = statTintNoChange;
				uISprite.color = statTintNoChange;
				objectExactMatch4.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
				objectExactMatch4.SetActive(true);
				objectExactMatch5.SetActive(true);
			}
			objectExactMatch5.transform.localEulerAngles = objectExactMatch4.transform.localEulerAngles;
			uiPanel = uiPanel ?? uISprite.panel;
			i++;
		}
		for (; i < 5; i++)
		{
			GameObject objectExactMatch6 = Util.GetObjectExactMatch(objectExactMatch, "Attribute" + i);
			objectExactMatch6.SetActive(false);
		}
		if (uiPanel != null)
		{
			uiPanel.widgetsAreStatic = false;
			Coroutines.NextFrame(SafeAction.Wrap(this, () =>
			{
				uiPanel.widgetsAreStatic = true;
			}));
		}
	}

	private void OnChangeCategory(GameObject caller)
	{
		if (!_enableInput)
		{
			return;
		}
		string key = caller.transform.parent.gameObject.name;
		if (categoryTabNameToIndex.ContainsKey(key))
		{
			int num = categoryTabNameToIndex[key];
			if (num >= 0 && num < 3)
			{
				ResetCacDisplay();
				tableHelper.Clear();
				cacItemFactory.cacCategory = (CharacterDB.Category)num;
				cacItemFactory.RefreshMetadataCache(showOwned);
				cacItemFactory.Reset();
			}
		}
	}

	private void OnItemSelected(GameObject caller)
	{
		if (!_enableInput)
		{
			return;
		}
		EB.Debug.Log("MyRobotUI > OnItemSelected > " + caller.name);
		CacGridItem component = caller.GetComponent<CacGridItem>();
		if (!(component == null))
		{
			CharacterDB.MetaData metaData = component.metaData;
			int hash = -1;
			if (detailedItem.metaData != null)
			{
				hash = detailedItem.metaData.hash;
			}
			int hash2 = metaData.hash;
			UpdateHighlighted(hash, false);
			UpdateHighlighted(hash2, true);
			detailedItem.metaData = metaData;
			detailedItem.UpdateArt();
			if (metaData.IsOwned || !metaData.IsLocked)
			{
				PreviewPart((CharacterDB.Category)metaData.category, metaData.hash);
			}
		}
	}

	private void OnBack(GameObject caller)
	{
		if (_enableInput)
		{
			_enableInput = false;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnTransitionOutComplete);
		}
	}

	private void OnTransitionOutComplete()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			Coroutines.Run(CleanupAndClose());
		});
	}

	private static IEnumerator CleanupAndClose()
	{
		GameObject screen = ScreenManager.Instance.FindPopup("BackgroundUI");
		BackgroundUI background = Util.FindComponent<BackgroundUI>(screen);
		background.ShowCliff(true);
		ScreenManager.Instance.ClosePopup();
		CacGridItem.ClearItems();
		TexturePoolManager.Instance.ClearPool();
		yield return Assets.UnloadUnusedAssets();
		GameObject statusBarObj = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (null != statusBarObj)
		{
			statusBarObj.GetComponent<StatusBarUI>().UpdateHoFElements(true);
		}
		Profile.Instance.Save();
	}

	private void PrepareForSubscreen(bool state)
	{
		inSubScreen = state;
		Util.GetObjectExactMatch(base.gameObject, "MyStatsButton").SetActive(false);
		Util.GetObjectExactMatch(base.gameObject, "ToggleOwnedButton").SetActive(!state);
		Util.GetObjectExactMatch(base.gameObject, "UpgradeScreenButton").SetActive(!state);
		Util.GetObjectExactMatch(base.gameObject, "CheckboxContainer").SetActive(!state);
		Util.GetObjectExactMatch(base.gameObject, "Background").SetActive(!state);
		Util.GetObjectExactMatch(base.gameObject, "BackButton").SetActive(!state);
		EnableNavigation(!state && cacItemFactory.GetNumItems() > cacItemFactory.gridItemsHorizontal * cacItemFactory.gridItemsHorizontal);
		Util.GetObjectExactMatch(base.gameObject, "RobotPartsPanel").transform.Translate(0f, (!state) ? (-1000f) : 1000f, 0f);
		Util.GetObjectExactMatch(base.gameObject, "CacItem").transform.Translate(0f, (!state) ? (-1000f) : 1000f, 0f);
	}

	private void OnMyStats(GameObject caller)
	{
		if (_enableInput)
		{
			PrepareForSubscreen(true);
			GameObject obj = ScreenManager.Instance.OpenPopup("MyRobotStatsUI", true);
			Util.FindComponent<MyRobotStatsUI>(obj).SetParentScreen(this);
		}
	}

	private void OnUpgradeScreen(GameObject caller)
	{
		if (_enableInput)
		{
			PrepareForSubscreen(true);
			ResetCacDisplay();
			UpdateAttributes();
			GameObject obj = ScreenManager.Instance.OpenPopup("MyRobotUpgradeUI", true);
			Util.FindComponent<MyRobotUpgradeUI>(obj).SetParentScreen(this);
		}
	}

	public void EnableNavigation(bool enable)
	{
		NGUITools.SetActive(arrowLeft, enable);
		NGUITools.SetActive(arrowRight, enable);
	}

	public void ForceOwned(bool owned)
	{
		if (showOwned != owned)
		{
			OnToggleOwned(null);
		}
	}

	public void OnToggleOwned(GameObject caller)
	{
		if (_enableInput)
		{
			showOwned = !showOwned;
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "MainPanelTransition");
			EBUI_TransitionManager.Instance.RunOutroTransitions(objectExactMatch, OnTransitionOutToggleComplete);
		}
	}

	private void OnTransitionOutToggleComplete()
	{
		UnityEngine.Debug.Log("Transition out toggle complete");
		UpdateOwned(showOwned);
		DoPartsTutorial();
		DoUpgradeTutorial();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "MainPanelTransition");
		EBUI_TransitionManager.Instance.RunIntroTransitions(objectExactMatch, SafeAction.Wrap(this, OnTransitionInToggleComplete));
	}

	private void OnTransitionInToggleComplete()
	{
		UnityEngine.Debug.Log("Transition in toggle complete");
	}

	private void UpdateOwned(bool show)
	{
		EB.Debug.Log("MyRobotUI > UpdateOwned > is owned state: " + show);
		UIUtils.SetLabelContents(toggleOwnedButton, "Label", (!showOwned) ? "ID_UI_CAC_OWNED" : "ID_UI_CAC_STORE");
		ResetCacDisplay();
		cacItemFactory.RefreshMetadataCache(showOwned);
		cacItemFactory.Reset();
		ResetTable();
		refreshTable = true;
		UpdateBackgroundTint();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CheckboxCategoryTab" + (int)cacItemFactory.cacCategory);
		if (objectExactMatch != null)
		{
			TabButtonScript componentInChildren = objectExactMatch.GetComponentInChildren<TabButtonScript>();
			if (componentInChildren != null)
			{
				componentInChildren.SetSelected(true);
			}
		}
	}

	private void UpdateBackgroundTint()
	{
		foreach (UISprite item in backgroundsToTint)
		{
			item.color = ((!showOwned) ? Tuning.Instance.ColorTint_Store : Tuning.Instance.ColorTint_Owned);
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(toggleOwnedButton, "Background");
		objectExactMatch.GetComponent<UISprite>().color = ((!showOwned) ? Tuning.Instance.ColorTint_Owned : Tuning.Instance.ColorTint_Store);
	}

	public void OnSubscreenClose()
	{
		RefreshData();
		PrepareForSubscreen(false);
		if (!_transitionedIn)
		{
			OnOpenPopup();
		}
	}

	private void OnEquip(GameObject caller)
	{
		if (_enableInput)
		{
			CacUiItem cacUiItem = Util.FindComponentUpwards<CacUiItem>(caller);
			EquipItem(cacUiItem.metaData);
			cacUiItem.UpdateArt();
		}
	}

	private void OnUpgrade(GameObject caller)
	{
		CacUiItem cacUiItem = Util.FindComponentUpwards<CacUiItem>(caller);
		if (cacUiItem != null)
		{
			UITexture uITexture = Util.FindComponent<UITexture>(cacUiItem.gameObject);
			GameObject obj = ScreenManager.Instance.OpenPopup("UpgradeUI", true, true);
			UpgradeUI upgradeUI = Util.FindComponent<UpgradeUI>(obj);
			upgradeUI.SetParameters(cacUiItem.metaData);
			upgradeUI.parentScreen = base.gameObject;
			upgradingPart = cacUiItem.metaData.hash;
		}
	}

	private void OnBuy(GameObject caller)
	{
		if (_enableInput)
		{
			CacUiItem cacUiItem = Util.FindComponentUpwards<CacUiItem>(caller);
			CharacterDB.MetaData metaData = cacUiItem.metaData;
			if (metaData != null)
			{
				GameObject obj = ScreenManager.Instance.OpenPopup("MyRobotPartUI", true, true);
				MyRobotPartUI myRobotPartUI = Util.FindComponent<MyRobotPartUI>(obj);
				myRobotPartUI.SetParameters(metaData, this);
			}
		}
	}

	private void OnCacItemsReady(CharacterDB.Category category)
	{
		tableHelper.Clear();
		DummyGridItem[] array = Util.FindAllComponents<DummyGridItem>(base.gameObject);
		foreach (DummyGridItem dummyGridItem in array)
		{
			tableHelper.AddItem(dummyGridItem.gameObject);
		}
		tableHelper.CenterOnCurrentItem();
		if (cacItemFactory == null)
		{
			cacItemFactory = Util.FindComponent<CacItemFactory>(base.gameObject);
		}
		currentlyEquippedItemHash = CharacterManager.Instance.LocalPlayer.recipe.parts[(int)cacItemFactory.cacCategory];
		previewPartHash = -1;
		UpdatePreviewInGrid(0, -1);
		UpdateAttributes();
		ResetTable();
		refreshTable = true;
		if (detailedItem == null)
		{
			detailedItem = Util.FindComponent<CacUiItem>(base.gameObject);
			if (detailedItem != null)
			{
				detailedItem.metaData = null;
				detailedItem.UpdateArt();
				detailedItem.buyButton.onButtonClick += OnBuy;
				detailedItem.equipButton.onButtonClick += OnEquip;
			}
			else
			{
				EB.Debug.LogError("Detailed Item is still null!!! How?!?");
			}
		}
		detailedItem.metaData = null;
		detailedItem.UpdateArt();
	}

	private void OnArrowLeft(GameObject caller)
	{
		if (_enableInput)
		{
			tableHelper.OnScrollLeft();
		}
	}

	private void OnArrowRight(GameObject caller)
	{
		if (_enableInput)
		{
			tableHelper.OnScrollRight();
		}
	}

	private void Update()
	{
		if (refreshTable)
		{
			ResetTable();
			EnableNavigation(!inSubScreen && cacItemFactory.GetNumItems() > cacItemFactory.gridItemsVertical * cacItemFactory.gridItemsHorizontal);
			refreshTable = false;
		}
		if (mScrollbarDragging && !Input.GetMouseButton(0))
		{
			tableHelper.CenterOnCurrentItem();
			mScrollbarDragging = false;
		}
	}

	public void CenterOnNewUnlockedPart(CharacterDB.MetaData partData)
	{
		RefreshData();
		int partIndex = cacItemFactory.GetPartIndex(partData);
		if (partIndex != -1)
		{
			int hash = -1;
			if (detailedItem.metaData != null)
			{
				hash = detailedItem.metaData.hash;
			}
			int hash2 = partData.hash;
			UpdateHighlighted(hash, false);
			UpdateHighlighted(hash2, true);
			detailedItem.metaData = partData;
			detailedItem.UpdateArt();
			if (partData.IsOwned || !partData.IsLocked)
			{
				PreviewPart((CharacterDB.Category)partData.category, partData.hash);
			}
			GameObject item = tableHelper.GetItemList()[partIndex];
			tableHelper.CenterOnItem(item);
		}
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
		Util.FindComponent<UIDraggablePanel>(base.gameObject).enabled = enable;
	}

	private void OnFETutorialToggleOwned()
	{
		showOwned = !showOwned;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "MainPanelTransition");
		EBUI_TransitionManager.Instance.RunOutroTransitions(objectExactMatch, OnTransitionOutToggleComplete);
	}

	private void OnFETutorialUpgrade()
	{
		PrepareForSubscreen(true);
		GameObject obj = ScreenManager.Instance.OpenPopup("MyRobotUpgradeUI", true);
		Util.FindComponent<MyRobotUpgradeUI>(obj).SetParentScreen(this);
	}

	private void OnTutorialSelect(string partName)
	{
		CharacterDB.MetaData partFromString = GetPartFromString(partName);
		if (partFromString != null)
		{
			int hash = -1;
			if (detailedItem.metaData != null)
			{
				hash = detailedItem.metaData.hash;
			}
			int hash2 = partFromString.hash;
			UpdateHighlighted(hash, false);
			UpdateHighlighted(hash2, true);
			detailedItem.metaData = partFromString;
			detailedItem.UpdateArt();
			PreviewPart((CharacterDB.Category)partFromString.category, partFromString.hash);
		}
	}

	private void OnTutorialBuy(string partName)
	{
		CharacterDB.MetaData partFromString = GetPartFromString(partName);
		if (partFromString != null)
		{
			GameObject obj = ScreenManager.Instance.OpenPopup("MyRobotPartUI", true, true);
			MyRobotPartUI myRobotPartUI = Util.FindComponent<MyRobotPartUI>(obj);
			myRobotPartUI.SetParameters(partFromString, this);
		}
	}

	private CharacterDB.MetaData GetPartFromString(string partName)
	{
		foreach (CharacterDB.MetaData item in CharacterDB.GetCategory(CharacterDB.Category.kHead))
		{
			if (item.name == partName)
			{
				return item;
			}
		}
		return null;
	}

	private void ResetTable()
	{
		tableHelper.Refresh();
		GameObject gameObject = cacItemFactory.targetGrid.gameObject;
		if (gameObject != null)
		{
			gameObject.transform.localPosition = initialGridPosition;
			UIPanel component = gameObject.GetComponent<UIPanel>();
			if (component != null)
			{
				component.clipRange = initialGridClipping;
			}
		}
	}
}
