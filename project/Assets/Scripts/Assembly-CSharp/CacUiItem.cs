using System.Collections.Generic;
using System.Linq;
using EB;
using UnityEngine;

public class CacUiItem : GridItem
{
	public Color equippedTabColor;

	public Color ownedTabColor;

	public Color lockedTabColor;

	public bool isHighlighted;

	private CharacterDB.MetaData _metaData;

	private ButtonScript _equipButton;

	private GameObject _equippedButton;

	private GameObject _imageBackground;

	private ButtonScript _upgradeButton;

	private ButtonScript _buyButton;

	private UILabel _itemName;

	private UISlicedSprite _infoTab;

	private UILabel _infoTabLabel;

	private UISlicedSprite _spriteHighlight;

	private GameObject _lockIndicator;

	private GameObject _lockBlockingBg;

	private GameObject _itemCost;

	private GameObject _itemTier;

	private GameObject _upgradeBarContainer;

	private GameObject _upgradeComplete;

	private GameObject _lockedContainer;

	private List<GameObject> _statContainers;

	private List<RobotAttribute> _attributes;

	private UpgradeProgressBar _upgradeProgressBar;

	private bool _isShowingDescription;

	private Vector3 defaultTopLeftStatPosition;

	private Vector3 defaultBottomRightStatPosition;

	private Vector3 defaultItemNameSize;

	private Vector3 defaultInfoTabSize;

	private Vector3 statValueDefaultSize;

	public CharacterDB.MetaData metaData
	{
		get
		{
			return _metaData;
		}
		set
		{
			_attributes = null;
			if (_metaData == null || value == null || _metaData.hash != value.hash)
			{
				Unload();
				_metaData = value;
				if (_metaData != null)
				{
					texturePath = "cac_ui_parts/" + _metaData.cacTexturePath;
				}
				else
				{
					texturePath = null;
				}
				Load();
			}
		}
	}

	public ButtonScript equipButton
	{
		get
		{
			return UIUtils.GetOrAssign(ref _equipButton, base.gameObject, "EquipButton");
		}
	}

	public GameObject equippedButton
	{
		get
		{
			if (_equippedButton == null)
			{
				_equippedButton = Util.GetObjectExactMatch(base.gameObject, "EquippedButton");
			}
			return _equippedButton;
		}
	}

	public GameObject imageBackground
	{
		get
		{
			if (_imageBackground == null)
			{
				_imageBackground = Util.GetObjectExactMatch(base.gameObject, "ImageBackground");
			}
			return _imageBackground;
		}
	}

	public ButtonScript upgradeButton
	{
		get
		{
			return UIUtils.GetOrAssign(ref _upgradeButton, base.gameObject, "UpgradeButton");
		}
	}

	public ButtonScript buyButton
	{
		get
		{
			return UIUtils.GetOrAssign(ref _buyButton, base.gameObject, "BuyButton");
		}
	}

	public UILabel itemName
	{
		get
		{
			return UIUtils.GetOrAssign(ref _itemName, base.gameObject, "LabelItemName");
		}
	}

	public UISlicedSprite infoTab
	{
		get
		{
			return UIUtils.GetOrAssign(ref _infoTab, base.gameObject, "SpriteInfoTab");
		}
	}

	public UILabel infoTabLabel
	{
		get
		{
			return UIUtils.GetOrAssign(ref _infoTabLabel, base.gameObject, "LabelInfoTab");
		}
	}

	public UISlicedSprite spriteHighlight
	{
		get
		{
			return UIUtils.GetOrAssign(ref _spriteHighlight, base.gameObject, "SpriteHighlight");
		}
	}

	public GameObject lockIndicator
	{
		get
		{
			if (_lockIndicator == null)
			{
				_lockIndicator = Util.GetObjectExactMatch(base.gameObject, "LockIcon");
			}
			return _lockIndicator;
		}
	}

	public GameObject lockBlockingBg
	{
		get
		{
			if (_lockBlockingBg == null)
			{
				_lockBlockingBg = Util.GetObjectExactMatch(base.gameObject, "LockBlockingOverlay");
			}
			return _lockBlockingBg;
		}
	}

	public GameObject itemCost
	{
		get
		{
			if (_itemCost == null)
			{
				_itemCost = Util.GetObjectExactMatch(base.gameObject, "ItemCost");
			}
			return _itemCost;
		}
	}

	public GameObject itemTier
	{
		get
		{
			if (_itemTier == null)
			{
				_itemTier = Util.GetObjectExactMatch(base.gameObject, "ItemTier");
			}
			return _itemTier;
		}
	}

	public GameObject upgradeBarContainer
	{
		get
		{
			if (_upgradeBarContainer == null)
			{
				_upgradeBarContainer = Util.GetObjectExactMatch(base.gameObject, "UpgradeBarContainer");
			}
			return _upgradeBarContainer;
		}
	}

	public GameObject upgradeComplete
	{
		get
		{
			if (_upgradeComplete == null)
			{
				_upgradeComplete = Util.GetObjectExactMatch(base.gameObject, "UpgradeCompleteContainer");
			}
			return _upgradeComplete;
		}
	}

	public GameObject lockedContainer
	{
		get
		{
			if (_lockedContainer == null)
			{
				_lockedContainer = Util.GetObjectExactMatch(base.gameObject, "ItemLocked");
			}
			return _lockedContainer;
		}
	}

	public List<GameObject> statContainers
	{
		get
		{
			if (_statContainers == null)
			{
				GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemStats");
				GameObject[] objects = Util.GetObjects(objectExactMatch, "Stat");
				_statContainers = objects.ToList();
				_statContainers.Sort(StatContainerComparison);
				_statContainers.Remove(objectExactMatch);
			}
			return _statContainers;
		}
	}

	public List<RobotAttribute> attributes
	{
		get
		{
			if (_attributes == null && metaData != null)
			{
				_attributes = PartManager.Instance.GetPartAttributes(metaData.hash);
				if (_attributes == null)
				{
					_attributes = new List<RobotAttribute>();
				}
			}
			return _attributes;
		}
	}

	public bool isEquipped
	{
		get
		{
			if (metaData != null)
			{
				int num = CharacterManager.Instance.LocalPlayer.recipe.parts[metaData.category];
				return num == metaData.hash;
			}
			return false;
		}
	}

	public bool isLocked
	{
		get
		{
			if (metaData != null)
			{
				return metaData.IsLocked;
			}
			return true;
		}
	}

	public bool isOwned
	{
		get
		{
			if (metaData != null)
			{
				return metaData.IsOwned;
			}
			return false;
		}
	}

	public bool canBeUpgraded
	{
		get
		{
			if (PartManager.Instance.GetPartAttributes(metaData.hash) == null)
			{
				return false;
			}
			return Profile.Instance.GetPartUpgradeProgress(metaData.hash) < 100f;
		}
	}

	public UpgradeProgressBar upgradeProgressBar
	{
		get
		{
			if (_upgradeProgressBar == null)
			{
				_upgradeProgressBar = Util.FindComponent<UpgradeProgressBar>(base.gameObject);
			}
			return _upgradeProgressBar;
		}
	}

	public void UpdateArt()
	{
		if (metaData != null)
		{
			bool flag = isEquipped;
			bool flag2 = isLocked;
			bool flag3 = isOwned;
			bool flag4 = canBeUpgraded;
			if (equipButton != null)
			{
				equipButton.gameObject.SetActive(flag3 && !flag);
				equippedButton.gameObject.SetActive(flag3 && flag);
				buyButton.gameObject.SetActive(!flag3 && !flag2);
				itemCost.SetActive(!flag3 && !flag2);
				lockedContainer.SetActive(flag2 && !flag3);
				imageBackground.SetActive(true);
			}
			if (upgradeButton != null)
			{
				upgradeButton.gameObject.SetActive(flag3 && flag4);
			}
			if (upgradeBarContainer != null)
			{
				upgradeBarContainer.SetActive(flag3 && flag4);
				upgradeComplete.SetActive(flag3 && !flag4);
			}
			bool flag5 = metaData.coins == 0 || metaData.coins >= Tuning.Instance.HiddenSoftCostThreshold;
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemCost");
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ItemCostGearsOnly");
			if (objectExactMatch != null && objectExactMatch2 != null)
			{
				bool flag6 = equipButton == null || (!flag3 && !flag2);
				objectExactMatch.SetActive(flag6 && !flag5);
				objectExactMatch2.SetActive(flag6 && flag5);
				if (flag6)
				{
					GameObject container = ((!flag5) ? objectExactMatch : objectExactMatch2);
					UIUtils.SetLabelContents(container, "LabelHardCost", metaData.bucks);
					UIUtils.SetLabelContents(container, "LabelSoftCost", metaData.coins);
				}
			}
			else
			{
				UIUtils.SetLabelContents(base.gameObject, "LabelHardCost", metaData.bucks);
				UIUtils.SetLabelContents(base.gameObject, "LabelSoftCost", metaData.coins);
			}
			UIUtils.AssignTextAndResize(itemName, metaData.friendlyName, defaultItemNameSize);
			UIUtils.SetLabelContents(base.gameObject, "LabelItemDescription", metaData.description);
			string text = Localizer.GetString("ID_UI_UPGRADE_CATEGORY_" + ((CharacterDB.Category)metaData.category).ToString().ToUpper());
			if (metaData.isChampionshipBoss)
			{
				UIUtils.SetLabelContents(lockedContainer, "LabelUnlockRequirements", "ID_UI_PURCHASE_CHAMPIONSHIP");
			}
			else if (metaData.previousTierItem != null)
			{
				UIUtils.SetLabelContents(lockedContainer, "LabelUnlockRequirements", Localizer.Format("ID_UI_UPGRADE_UNLOCK_PREVIOUS_TIER", Localizer.GetString(metaData.previousTierItem.friendlyName)));
			}
			else
			{
				UIUtils.SetLabelContents(lockedContainer, "LabelUnlockRequirements", Localizer.Format("ID_UI_UPGRADE_UNLOCK_REQUIREMENTS", text, metaData.Tier));
			}
			Util.SetActiveRecursive(base.gameObject, "LabelItemName", true);
			Util.SetActiveRecursive(base.gameObject, "LabelItemDisabled", false);
			Util.SetActiveRecursive(base.gameObject, "ItemNameBackground", true);
			if (spriteHighlight != null)
			{
				spriteHighlight.enabled = true;
			}
			if (lockIndicator != null)
			{
				lockIndicator.SetActive(!isOwned && isLocked);
				lockBlockingBg.SetActive(!isOwned && isLocked);
			}
			if (infoTab != null)
			{
				infoTab.enabled = false;
				infoTabLabel.enabled = false;
			}
			int tier = PartManager.Instance.GetTier(metaData.hash);
			Color color = Tuning.Instance.GetTierColor(tier);
			if (metaData.isChampionshipBoss)
			{
				color = Tuning.Instance.ColorTint_Championship;
			}
			else if (metaData.isEpic)
			{
				color = Tuning.Instance.ColorTint_Epic;
			}
			GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "ImageBackground");
			if (objectExactMatch3 != null)
			{
				UISprite component = objectExactMatch3.GetComponent<UISprite>();
				if (component != null)
				{
					component.color = color;
				}
			}
			if (itemTier != null)
			{
				NGUITools.SetActive(itemTier, true);
				string labelText = ((!metaData.isChampionshipBoss) ? "ID_UI_CAC_TIER" : "ID_UI_CAC_BOSS");
				UIUtils.SetLabelContents(itemTier, "LabelTierTitle", labelText);
				UIUtils.SetLabelContents(itemTier, "LabelTierValue", tier.ToString());
				UISprite[] componentsInChildren = itemTier.GetComponentsInChildren<UISprite>();
				UISprite[] array = componentsInChildren;
				foreach (UISprite uISprite in array)
				{
					uISprite.color = color;
				}
			}
			SetUpgradeProgress(Profile.Instance.GetPartUpgradeProgress(metaData.hash), 100f);
			UpdateStatsAndDescription();
			BuildStatsInfo();
		}
		else
		{
			if (equipButton != null)
			{
				equipButton.gameObject.SetActive(false);
				equippedButton.gameObject.SetActive(false);
				buyButton.gameObject.SetActive(false);
				itemCost.SetActive(false);
				lockedContainer.SetActive(false);
				imageBackground.SetActive(false);
				spriteHighlight.enabled = false;
				Util.GetObjectExactMatch(base.gameObject, "LabelItemDescription").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "ItemStats").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "OpenDescriptionButton").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "CloseDescriptionButton").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "LabelItemName").SetActive(false);
				Util.GetObjectExactMatch(base.gameObject, "LabelItemDisabled").SetActive(true);
				Util.GetObjectExactMatch(base.gameObject, "ItemNameBackground").SetActive(false);
			}
			if (upgradeButton != null)
			{
				upgradeButton.gameObject.SetActive(false);
			}
			if (upgradeBarContainer != null)
			{
				upgradeBarContainer.SetActive(false);
				upgradeComplete.SetActive(false);
			}
			if (infoTab != null)
			{
				infoTab.enabled = false;
				UIUtils.SetLabelContents(base.gameObject, "LabelInfoTab", string.Empty);
			}
			if (itemTier != null)
			{
				NGUITools.SetActive(itemTier, false);
			}
			for (int j = 0; j < statContainers.Count; j++)
			{
				statContainers[j].SetActive(false);
			}
			if (lockIndicator != null)
			{
				lockIndicator.active = false;
				lockBlockingBg.active = false;
			}
		}
	}

	public void OnItemSelected()
	{
	}

	public void OnOpenDescription(GameObject caller)
	{
		_isShowingDescription = true;
		UpdateStatsAndDescription();
	}

	public void OnCloseDescription(GameObject caller)
	{
		_isShowingDescription = false;
		UpdateStatsAndDescription();
	}

	public void OnPreview(GameObject caller)
	{
		MyRobotUI myRobotUI = Util.FindComponentUpwards<MyRobotUI>(base.gameObject);
		if (myRobotUI != null)
		{
			myRobotUI.PreviewPart((CharacterDB.Category)metaData.category, metaData.hash);
		}
	}

	public override void Load()
	{
		if (!string.IsNullOrEmpty(texturePath))
		{
			if (Assets.Exists(texturePath))
			{
				Texture2D texture = Assets.Load<Texture2D>(texturePath);
				OnTextureLoaded(texture);
			}
			else
			{
				base.Load();
			}
		}
	}

	public override void Unload()
	{
		if (!string.IsNullOrEmpty(texturePath))
		{
			if (Assets.Exists(texturePath))
			{
				Assets.Unload(texturePath);
			}
			else
			{
				base.Unload();
			}
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Stat0");
		defaultTopLeftStatPosition = objectExactMatch.transform.localPosition;
		defaultBottomRightStatPosition = Util.GetObjectExactMatch(base.gameObject, "Stat3").transform.localPosition;
		statValueDefaultSize = Util.GetObjectExactMatch(objectExactMatch, "Value").transform.localScale;
		if (itemName != null)
		{
			defaultItemNameSize = itemName.transform.localScale;
		}
		if (infoTabLabel != null)
		{
			defaultInfoTabSize = infoTabLabel.transform.localScale;
		}
	}

	private void UpdateStatsAndDescription()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "LabelItemDescription");
		bool flag = metaData.IsOwned || !metaData.IsLocked;
		if (objectExactMatch != null)
		{
			Util.GetObjectExactMatch(base.gameObject, "LabelItemDescription").SetActive(_isShowingDescription && flag);
			Util.GetObjectExactMatch(base.gameObject, "ItemStats").SetActive(!_isShowingDescription && flag);
			Util.GetObjectExactMatch(base.gameObject, "OpenDescriptionButton").SetActive(!_isShowingDescription && flag);
			Util.GetObjectExactMatch(base.gameObject, "CloseDescriptionButton").SetActive(_isShowingDescription && flag);
		}
		if (!_isShowingDescription && flag)
		{
			for (int i = 0; i < statContainers.Count; i++)
			{
				statContainers[i].SetActive(i < attributes.Count);
			}
		}
	}

	private void BuildStatsInfo()
	{
		if (attributes.Count == 1)
		{
			Vector3 localPosition = (defaultTopLeftStatPosition + defaultBottomRightStatPosition) / 2f;
			Util.GetObjectExactMatch(base.gameObject, "Stat0").transform.localPosition = localPosition;
		}
		else if (attributes.Count == 2)
		{
			Vector3 vector = defaultTopLeftStatPosition;
			vector.y += defaultBottomRightStatPosition.y;
			vector.y /= 2f;
			Vector3 localPosition2 = vector;
			vector.x = defaultBottomRightStatPosition.x;
			Util.GetObjectExactMatch(base.gameObject, "Stat0").transform.localPosition = vector;
			Util.GetObjectExactMatch(base.gameObject, "Stat1").transform.localPosition = localPosition2;
		}
		else
		{
			Vector3 localPosition3 = defaultTopLeftStatPosition;
			localPosition3.x = defaultBottomRightStatPosition.x;
			Vector3 localPosition4 = defaultBottomRightStatPosition;
			localPosition4.x = defaultTopLeftStatPosition.x;
			Util.GetObjectExactMatch(base.gameObject, "Stat0").transform.localPosition = defaultTopLeftStatPosition;
			Util.GetObjectExactMatch(base.gameObject, "Stat1").transform.localPosition = localPosition3;
			Util.GetObjectExactMatch(base.gameObject, "Stat2").transform.localPosition = localPosition4;
			Util.GetObjectExactMatch(base.gameObject, "Stat3").transform.localPosition = defaultBottomRightStatPosition;
		}
		bool state = isOwned || (!isLocked && !_isShowingDescription);
		for (int i = 0; i < attributes.Count; i++)
		{
			UISprite uISprite = Util.FindComponent<UISprite>(statContainers[i]);
			RobotAttribute.RobotAttributeType type = attributes[i].type;
			string text = type.ToString();
			float num = PartManager.Instance.GetUpgradedPartAttributeValue(metaData.hash, type);
			uISprite.spriteName = "attrib_" + text.ToLower();
			GameObject objectExactMatch = Util.GetObjectExactMatch(statContainers[i], "Value");
			UILabel l = Util.FindComponent<UILabel>(objectExactMatch);
			if (type != RobotAttribute.RobotAttributeType.Health)
			{
				num *= 10f;
			}
			UIUtils.AssignTextAndResize(l, num.ToString("0"), statValueDefaultSize);
			UIUtils.SetLabelContents(statContainers[i], "Label", "ID_UI_ATTRIBUTE_" + text.ToUpper());
			NGUITools.SetActive(statContainers[i], state);
		}
	}

	private void SetUpgradeProgress(float progress, float max)
	{
		if (upgradeBarContainer != null && upgradeBarContainer.active)
		{
			upgradeProgressBar.SetMetadata(metaData);
			upgradeProgressBar.SetValue(progress / max);
		}
	}

	private static int StatContainerComparison(GameObject g1, GameObject g2)
	{
		return g1.name.CompareTo(g2.name);
	}
}
