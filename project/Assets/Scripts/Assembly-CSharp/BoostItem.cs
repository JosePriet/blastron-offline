using EB;
using UnityEngine;

public class BoostItem : InventoryItem
{
	public enum Type
	{
		kDamage = 0,
		kHeal = 1,
		kBonusXp = 2,
		kShield = 3,
		kAim = 4,
		kJetpack = 5,
		kSpeed = 6,
		kMultipleShots = 7,
		kNum = 8
	}

	public enum SubType
	{
		kNone = 0,
		kJetpackNormal = 1,
		kJetpackHover = 2,
		kNum = 3
	}

	public enum EffectType
	{
		kNone = 0,
		kContinuous = 1,
		kOnce = 2
	}

	public Type _type;

	public SubType _subType;

	public float _value;

	public int _durationSecs;

	public int _cooldownSecs;

	public GameObject _effectPrefab;

	public string _effectBone;

	public Vector3 _effectOffset;

	public override Category ItemCategory
	{
		get
		{
			return Category.Boost;
		}
	}

	public static string GetHudIcon(Type type, bool isSmall = false)
	{
		string text = null;
		switch (type)
		{
		case Type.kAim:
			text = "icon_boost_aimassist";
			break;
		case Type.kDamage:
			text = "icon_boost_attack";
			break;
		case Type.kHeal:
			text = "icon_boost_health";
			break;
		case Type.kJetpack:
			text = "icon_boost_jetboots";
			break;
		case Type.kMultipleShots:
			text = "icon_boost_twoshot";
			break;
		case Type.kShield:
			text = "icon_boost_shield";
			break;
		case Type.kSpeed:
			text = "icon_boost_speed";
			break;
		case Type.kBonusXp:
			text = "icon_boost_xp";
			break;
		default:
			return null;
		}
		if (isSmall)
		{
			text += "_small";
		}
		return text;
	}

	public override bool IsValid()
	{
		return true;
	}

	public string GetDurationText()
	{
		return Localizer.Format("ID_UI_BOOSTS_DURATION", _durationSecs);
	}

	public string GetBoostText()
	{
		int num = Mathf.RoundToInt(_value);
		string empty = string.Empty;
		if (_type == Type.kHeal)
		{
			return "+" + num;
		}
		return "+" + num + "%";
	}

	public void Awake()
	{
		category = Category.Boost;
	}
}
