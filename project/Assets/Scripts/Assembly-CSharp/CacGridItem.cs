using System.Collections.Generic;
using System.Linq;
using EB;
using EB.Sparx;
using UnityEngine;

public class CacGridItem : GridItem
{
	public CharacterDB.MetaData metaData;

	public bool isHighlighted;

	private float statYDelta;

	private float statDefaultY;

	private static Dictionary<int, CacGridItem> _all = new Dictionary<int, CacGridItem>();

	private UISlicedSprite _spriteHighlight;

	private GameObject _itemCost;

	private GameObject _itemEquipped;

	private GameObject _itemOwned;

	private GameObject _itemTier;

	private GameObject _lockedContainer;

	private List<GameObject> _statContainers;

	private List<RobotAttribute> _attributes;

	private Vector3 defaultTopStatPosition;

	private Vector3 defaultBottomStatPosition;

	public UISlicedSprite spriteHighlight
	{
		get
		{
			return UIUtils.GetOrAssign(ref _spriteHighlight, base.gameObject, "SpriteHighlight");
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

	public GameObject itemEquipped
	{
		get
		{
			if (_itemEquipped == null)
			{
				_itemEquipped = Util.GetObjectExactMatch(base.gameObject, "ItemEquipped");
			}
			return _itemEquipped;
		}
	}

	public GameObject itemOwned
	{
		get
		{
			if (_itemOwned == null)
			{
				_itemOwned = Util.GetObjectExactMatch(base.gameObject, "ItemOwned");
			}
			return _itemOwned;
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
				if (metaData.IsFree || metaData.isDefault)
				{
					return true;
				}
				return Hub.Instance.InventoryManager.GetCount(metaData.name) > 0;
			}
			return false;
		}
	}

	public bool canBeUpgraded
	{
		get
		{
			return Profile.Instance.GetPartUpgradeProgress(metaData.hash) < 100f;
		}
	}

	public static CacGridItem GetItem(int hash)
	{
		CacGridItem value = null;
		_all.TryGetValue(hash, out value);
		return value;
	}

	public static void ClearItems()
	{
		_all.Clear();
	}

	public void UpdateArt()
	{
		if (metaData == null)
		{
			return;
		}
		bool flag = isEquipped;
		bool flag2 = isLocked;
		bool flag3 = isOwned;
		bool flag4 = canBeUpgraded;
		if (itemEquipped != null)
		{
			if (flag)
			{
				itemEquipped.SetActive(true);
				UIUtils.SetLabelContents(base.gameObject, "Label", "ID_UI_CAC_STATE_EQUIPPED");
			}
			else if (metaData.onSale)
			{
				itemEquipped.SetActive(true);
				UIUtils.SetLabelContents(base.gameObject, "Label", "ID_UI_CAC_STATE_ONSALE");
			}
			else
			{
				itemEquipped.SetActive(false);
			}
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
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ImageBackground");
		if (objectExactMatch != null)
		{
			UISprite component = objectExactMatch.GetComponent<UISprite>();
			if (component != null)
			{
				component.color = color;
			}
		}
		if (itemTier != null)
		{
			UIUtils.SetLabelContents(itemTier, "LabelTierTitle", (!metaData.isChampionshipBoss) ? "ID_UI_CAC_TIER" : "ID_UI_CAC_BOSS");
			UIUtils.SetLabelContents(itemTier, "LabelTierValue", tier.ToString());
			if (metaData.isChampionshipBoss)
			{
				color = Tuning.Instance.ColorTint_Championship;
			}
			else if (metaData.isEpic)
			{
				color = Tuning.Instance.ColorTint_Epic;
			}
			if (tier > 0)
			{
				UISprite[] componentsInChildren = itemTier.GetComponentsInChildren<UISprite>();
				UISprite[] array = componentsInChildren;
				foreach (UISprite uISprite in array)
				{
					uISprite.color = color;
				}
			}
		}
		if (itemOwned != null)
		{
			itemOwned.SetActive(false);
		}
		if (itemCost != null)
		{
			itemCost.SetActive(false);
		}
		if (lockedContainer != null)
		{
			lockedContainer.SetActive(flag2 && !flag3);
		}
		if (spriteHighlight != null)
		{
			spriteHighlight.enabled = isHighlighted;
		}
		BuildStatsInfo();
	}

	public void OnPreview(GameObject caller)
	{
		MyRobotUI myRobotUI = Util.FindComponentUpwards<MyRobotUI>(base.gameObject);
		myRobotUI.PreviewPart((CharacterDB.Category)metaData.category, metaData.hash);
	}

	public override void Load()
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
		if (metaData != null)
		{
			_all[metaData.hash] = this;
		}
	}

	public override void Unload()
	{
		if (Assets.Exists(texturePath))
		{
			Assets.Unload(texturePath);
		}
		else
		{
			base.Unload();
		}
		if (metaData != null)
		{
			_all.Remove(metaData.hash);
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		if (statContainers.Count >= 3)
		{
			defaultTopStatPosition = statContainers[0].transform.localPosition;
			defaultBottomStatPosition = statContainers[3].transform.localPosition;
		}
		float y = defaultTopStatPosition.y;
		float y2 = defaultBottomStatPosition.y;
		statYDelta = (y2 - y) / 4f;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemStats");
		if (objectExactMatch != null)
		{
			statDefaultY = Util.GetObjectExactMatch(base.gameObject, "ItemStats").transform.localPosition.y;
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ItemEquipped");
		UILabel uILabel = Util.FindComponent<UILabel>(objectExactMatch2);
		if (uILabel != null)
		{
			UIUtils.AssignTextAndResize(uILabel, uILabel.text, uILabel.transform.localScale);
		}
	}

	private void BuildStatsInfo()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemStats");
		if (objectExactMatch == null)
		{
			return;
		}
		bool flag = !isLocked || isOwned;
		for (int i = 0; i < statContainers.Count; i++)
		{
			statContainers[i].SetActive(flag && i < attributes.Count);
		}
		Transform transform = objectExactMatch.transform;
		Vector3 localPosition = transform.localPosition;
		localPosition.y = statDefaultY + statYDelta / 2f * (float)(statContainers.Count - attributes.Count);
		transform.localPosition = localPosition;
		for (int j = 0; j < attributes.Count; j++)
		{
			UISprite uISprite = Util.FindComponent<UISprite>(statContainers[j]);
			RobotAttribute.RobotAttributeType type = attributes[j].type;
			string text = type.ToString();
			float num = PartManager.Instance.GetUpgradedPartAttributeValue(metaData.hash, type);
			uISprite.spriteName = "attrib_" + text.ToLower();
			if (type != RobotAttribute.RobotAttributeType.Health)
			{
				num *= 10f;
			}
			UIUtils.SetLabelContents(statContainers[j], "Value", num.ToString("0"));
		}
	}

	private static int StatContainerComparison(GameObject g1, GameObject g2)
	{
		return g1.name.CompareTo(g2.name);
	}
}
