using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class PartManager : MonoBehaviour
{
	public const float maxUpgradeProgress = 100f;

	private static PartManager _this;

	private Dictionary<int, string> _mapping;

	public static PartManager Instance
	{
		get
		{
			return _this;
		}
	}

	private void Awake()
	{
		_this = this;
		_mapping = new Dictionary<int, string>();
	}

	public void AddPartMapping(int partHash, string path)
	{
		_mapping[partHash] = path;
	}

	private PartAttributes GetAttributes(int partHash)
	{
		string value = null;
		if (_mapping.TryGetValue(partHash, out value))
		{
			PartAttributes partAttributes = Assets.Load<PartAttributes>("attributes/" + value);
			if (partAttributes != null)
			{
				Assets.Unload(partAttributes.gameObject);
			}
			return partAttributes;
		}
		return null;
	}

	public List<RobotAttribute> GetPartAttributes(int hash)
	{
		PartAttributes attributes = GetAttributes(hash);
		if ((bool)attributes)
		{
			return attributes.BaseAttributes;
		}
		return null;
	}

	public int GetTier(int hash)
	{
		PartAttributes attributes = GetAttributes(hash);
		if ((bool)attributes)
		{
			return attributes.Tier;
		}
		return 0;
	}

	public float GetPartAttributeValue(int partHash, RobotAttribute.RobotAttributeType attributeType, float upgradeProgress)
	{
		float num = 0f;
		List<RobotAttribute> partAttributes = GetPartAttributes(partHash);
		if (partAttributes != null)
		{
			foreach (RobotAttribute item in partAttributes)
			{
				if (item != null && item.type == attributeType)
				{
					float num2 = item.minValue;
					num2 = item.GetCurrentValue(upgradeProgress);
					num += num2;
				}
			}
		}
		return num;
	}

	public float GetUpgradedPartAttributeValue(int partHash, RobotAttribute.RobotAttributeType attributeType)
	{
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		return GetPartAttributeValue(partHash, attributeType, partUpgradeProgress);
	}

	public float GetTotalAttributeValue(CharacterDB.Recipe recipe, RobotAttribute.RobotAttributeType attributeType)
	{
		float num = 0f;
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			int partHash = recipe.parts[i];
			float upgradeProgress = recipe.UpgradeProgress[i];
			num += Instance.GetPartAttributeValue(partHash, attributeType, upgradeProgress);
		}
		return num;
	}

	public float Upgrade(int partHash, out float upgradeRatio)
	{
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		PartAttributes.Bracket bracket = GetBracket(partHash, partUpgradeProgress);
		if (bracket != null)
		{
			float upgradeValue = bracket.GetUpgradeValue(out upgradeRatio);
			Profile.Instance.SetPartUpgradeProgress(partHash, Mathf.Min(partUpgradeProgress + upgradeValue, 100f));
			Profile.Instance.Save();
			return upgradeValue;
		}
		upgradeRatio = 0f;
		return 0f;
	}

	public int GetPartUpgradeCostSoft(int partHash, float upgradeProgress)
	{
		PartAttributes.Bracket bracket = GetBracket(partHash, upgradeProgress);
		if (bracket != null)
		{
			return bracket.priceRivets;
		}
		return 0;
	}

	public int GetPartUpgradeCostHard(int partHash, float upgradeProgress)
	{
		PartAttributes.Bracket bracket = GetBracket(partHash, upgradeProgress);
		if (bracket != null)
		{
			return bracket.priceGears;
		}
		return 0;
	}

	public int GetPartUpgradeXPReward(int partHash, float upgradeProgress)
	{
		PartAttributes.Bracket bracket = GetBracket(partHash, upgradeProgress);
		if (bracket != null)
		{
			return bracket.xp;
		}
		return 0;
	}

	public float GetPartUpgradeMaxResult(int partHash)
	{
		float partUpgradeProgress = Profile.Instance.GetPartUpgradeProgress(partHash);
		PartAttributes.Bracket bracket = GetBracket(partHash, partUpgradeProgress);
		float num = 0f;
		if (bracket != null)
		{
			if (bracket.dataType == PartAttributes.UpgradeDataType.Curve)
			{
				AnimationCurve upgradeValueCurve = bracket.upgradeValueCurve;
				if (upgradeValueCurve != null && upgradeValueCurve.keys.Length > 0)
				{
					num = upgradeValueCurve.keys[upgradeValueCurve.length - 1].value;
				}
			}
			else if (bracket.dataType == PartAttributes.UpgradeDataType.Table)
			{
				PartAttributes.UpgradeData[] upgrades = bracket.Upgrades;
				if (upgrades != null && upgrades.Length > 0)
				{
					num = upgrades[upgrades.Length - 1].value;
				}
			}
			else
			{
				EB.Debug.LogError("Unsupported upgrade data type!");
			}
		}
		return partUpgradeProgress + num;
	}

	private PartAttributes.Bracket GetBracket(int partHash, float upgradeProgress)
	{
		PartAttributes attributes = GetAttributes(partHash);
		if ((bool)attributes)
		{
			PartAttributes.Bracket[] array = null;
			if (attributes.ProgressBrackets != null && attributes.ProgressBrackets.Count > 0)
			{
				array = attributes.ProgressBrackets.ToArray();
			}
			else if (attributes.ProgressBracketsPrefab != null)
			{
				ApplyTuningForUpgradeData(attributes.ProgressBracketsPrefab);
				array = attributes.ProgressBracketsPrefab.ProgressBrackets;
			}
			if (array != null && array.Length > 0)
			{
				foreach (PartAttributes.Bracket bracket in array)
				{
					if (upgradeProgress >= (float)bracket.bracketMin && upgradeProgress <= (float)bracket.bracketMax)
					{
						return bracket;
					}
				}
			}
		}
		return null;
	}

	private void ApplyTuningForUpgradeData(UpgradeProgressData prefabData)
	{
		Hub.Instance.TuningManager.Apply(prefabData, "upgradeprogressdata." + prefabData.gameObject.name);
	}
}
