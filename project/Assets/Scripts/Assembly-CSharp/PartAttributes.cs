using System;
using System.Collections.Generic;
using EB;
using UnityEngine;

[System.Serializable]
public class PartAttributes : MonoBehaviour
{
	public enum UpgradeDataType
	{
		Curve = 0,
		Table = 1
	}

	[System.Serializable]
	[EB.Serializable(Tag = "pabk")]
	public class Bracket
	{
		[EB.Serializable(Tag = "pr")]
		public int priceRivets;

		[EB.Serializable(Tag = "pg")]
		public int priceGears;

		[EB.Serializable(Tag = "bmin")]
		public int bracketMin;

		[EB.Serializable(Tag = "bmax")]
		public int bracketMax;

		[EB.Serializable(Tag = "xp")]
		public int xp;

		[NoSerialize]
		public UpgradeDataType dataType;

		[EB.Serializable(Tag = "ups")]
		public UpgradeData[] Upgrades;

		[NoSerialize]
		public AnimationCurve upgradeValueCurve;

		private float totalWeight;

		public float GetUpgradeValue(out float upgradeRatio)
		{
			switch (dataType)
			{
			case UpgradeDataType.Curve:
				return GetUpgradeValueCurve(out upgradeRatio);
			case UpgradeDataType.Table:
				return GetUpgradeValueTable(out upgradeRatio);
			default:
				UnityEngine.Debug.LogError("Unsupported upgrade data type specification");
				upgradeRatio = 0f;
				return 0f;
			}
		}

		private float GetUpgradeValueCurve(out float upgradeRatio)
		{
			if (upgradeValueCurve != null && upgradeValueCurve.length > 1)
			{
				Keyframe keyframe = upgradeValueCurve.keys[0];
				Keyframe keyframe2 = upgradeValueCurve.keys[upgradeValueCurve.length - 1];
				float num = UnityEngine.Random.Range(keyframe.time, keyframe2.time);
				upgradeRatio = num / (keyframe2.time - keyframe.time);
				float num2 = upgradeValueCurve.Evaluate(num);
				return num2 * 10f;
			}
			UnityEngine.Debug.LogError("Missing curve data!");
			upgradeRatio = 0f;
			return 0f;
		}

		private float GetUpgradeValueTable(out float upgradeRatio)
		{
			float num = UnityEngine.Random.Range(0f, GetTotalWeight());
			upgradeRatio = num / GetTotalWeight();
			float num2 = 0f;
			for (int i = 0; i < Upgrades.Length; i++)
			{
				UpgradeData upgradeData = Upgrades[i];
				num2 += upgradeData.weight;
				if (num2 > num)
				{
					return upgradeData.value;
				}
			}
			EB.Debug.LogError("THIS SHOULD NOT HAPPEN!!! LOGIC ERROR!");
			return 0f;
		}

		private float GetTotalWeight()
		{
			if (totalWeight == 0f)
			{
				float num = 0f;
				if (Upgrades != null)
				{
					UpgradeData[] upgrades = Upgrades;
					foreach (UpgradeData upgradeData in upgrades)
					{
						num += upgradeData.weight;
					}
				}
				totalWeight = num;
				return num;
			}
			return totalWeight;
		}
	}

	[System.Serializable]
	[EB.Serializable(Tag = "ugd")]
	public class UpgradeData
	{
		[EB.Serializable(Tag = "v")]
		public float value;

		[EB.Serializable(Tag = "w")]
		public float weight;
	}

	public List<RobotAttribute> BaseAttributes;

	public int Tier;

	public List<Bracket> ProgressBrackets;

	public UpgradeProgressData ProgressBracketsPrefab;
}
