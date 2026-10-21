using System;
using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class AttributesManager : MonoBehaviour, ISaveLoadInterface
{
	[System.Serializable]
	[EB.Serializable(Tag = "SkillPointData")]
	public class AttributesTunable
	{
		[EB.Serializable(Tag = "isp")]
		public int InitialSkillPoints = 5;

		[EB.Serializable(Tag = "spc")]
		public int SkillPointsLevelOneCost = 3;

		[EB.Serializable(Tag = "ml")]
		public int MaxAttributeLevel = 10;

		[EB.Serializable(Tag = "t")]
		public int[] ResetSkillPointsCost = new int[10];

		[EB.Serializable(Tag = "fv")]
		public IncrementStatFormulaValues[] SkillPointsFormula = new IncrementStatFormulaValues[Enum.GetValues(typeof(statsNames)).Length];

		[EB.Serializable(Tag = "ba")]
		public float baseAttackModifier = 20f;

		[EB.Serializable(Tag = "bh")]
		public float baseHealthModifier = 50f;

		[EB.Serializable(Tag = "br")]
		public float baseArmorModifier = 10f;

		[EB.Serializable(Tag = "bj")]
		public float baseJumpHeightModifier = 10f;

		[EB.Serializable(Tag = "bs")]
		public float baseSpeedModifier = 5f;

		[EB.Serializable(Tag = "ih")]
		public float InitialHealth;
	}

	public enum statsNames
	{
		statAttack = 0,
		statArmor = 1,
		statHealth = 2,
		statJumpHeight = 3,
		statSpeed = 4
	}

	[System.Serializable]
	[EB.Serializable(Tag = "SPFormulaValues")]
	public class IncrementStatFormulaValues
	{
		[EB.Serializable(Tag = "a")]
		public float AddedValue;

		[EB.Serializable(Tag = "m")]
		public float MultipliedValue;
	}

	private SafeFloat _currentAttackMod;

	private SafeFloat _currentHealthMod;

	private SafeFloat _currentArmorMod;

	private SafeFloat _currentJumpHeightMod;

	private SafeFloat _currentSpeedMod;

	private int timesReset;

	public SafeInt[] attributeLevels = new SafeInt[Enum.GetValues(typeof(statsNames)).Length];

	public SafeInt currentSkillPoints;

	private SafeInt spentSkillpoints;

	public static AttributesManager instance;

	private string lvlSufix = "lvl";

	private bool _saveRequired;

	internal bool DebugOn;

	public float _currentAttack
	{
		get
		{
			return (float)_currentAttackMod * 0.001f;
		}
	}

	public float _currentHealth
	{
		get
		{
			return SPData.InitialHealth + _currentHealthMod;
		}
	}

	public float _currentArmor
	{
		get
		{
			return (float)_currentArmorMod * 0.001f;
		}
	}

	public float _currentJumpHeight
	{
		get
		{
			return (float)_currentJumpHeightMod * 0.001f;
		}
	}

	public float _currentSpeed
	{
		get
		{
			return (float)_currentSpeedMod * 0.001f;
		}
	}

	public int _timesReset
	{
		get
		{
			if (timesReset < Tuning.Instance.SkillPointsData.ResetSkillPointsCost.Length)
			{
				return timesReset;
			}
			return Tuning.Instance.SkillPointsData.ResetSkillPointsCost.Length - 1;
		}
	}

	public float[] allStats
	{
		get
		{
			return new float[5] { _currentAttackMod, _currentArmorMod, _currentHealthMod, _currentJumpHeightMod, _currentSpeedMod };
		}
	}

	private AttributesTunable SPData
	{
		get
		{
			return Tuning.Instance.SkillPointsData;
		}
	}

	public string Key
	{
		get
		{
			return "Attr";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return _saveRequired;
		}
	}

	private void Awake()
	{
		instance = this;
		Hub.Instance.SaveLoadManager.Register(this);
	}

	public void IncrementStat(statsNames type)
	{
		bool CanAfford;
		int num = (int)Mathf.Round(GetSkillUpgradeCost(type, out CanAfford));
		switch (type)
		{
		case statsNames.statAttack:
			_currentAttackMod = (float)(_currentAttackMod + SPData.SkillPointsFormula[(int)type].AddedValue) * SPData.SkillPointsFormula[(int)type].MultipliedValue;
			break;
		case statsNames.statHealth:
			_currentHealthMod = (float)(_currentHealthMod + SPData.SkillPointsFormula[(int)type].AddedValue) * SPData.SkillPointsFormula[(int)type].MultipliedValue;
			break;
		case statsNames.statArmor:
			_currentArmorMod = (float)(_currentArmorMod + SPData.SkillPointsFormula[(int)type].AddedValue) * SPData.SkillPointsFormula[(int)type].MultipliedValue;
			break;
		case statsNames.statJumpHeight:
			_currentJumpHeightMod = (float)(_currentJumpHeightMod + SPData.SkillPointsFormula[(int)type].AddedValue) * SPData.SkillPointsFormula[(int)type].MultipliedValue;
			break;
		case statsNames.statSpeed:
			_currentSpeedMod = (float)(_currentSpeedMod + SPData.SkillPointsFormula[(int)type].AddedValue) * SPData.SkillPointsFormula[(int)type].MultipliedValue;
			break;
		}
		++attributeLevels[(int)type];
		currentSkillPoints -= (SafeInt)num;
		spentSkillpoints += (SafeInt)num;
		Hub.Instance.SaveLoadManager.Save(this);
	}

	public int NextLevelAttributeMod(statsNames stat)
	{
		float num = allStats[(int)stat];
		num = (num + SPData.SkillPointsFormula[(int)stat].AddedValue) * SPData.SkillPointsFormula[(int)stat].MultipliedValue;
		return (int)Mathf.Round(num);
	}

	public void AddSkillPoints(int Amount)
	{
		currentSkillPoints += (SafeInt)Amount;
		Hub.Instance.SaveLoadManager.Save(this);
	}

	public float GetSkillUpgradeCost(statsNames stat, out bool CanAfford)
	{
		int num = attributeLevels[(int)stat];
		float num2 = SPData.SkillPointsLevelOneCost;
		for (int i = 0; i < num - 1; i++)
		{
			num2 = num2 * 1.005f + 1f;
		}
		CanAfford = Mathf.RoundToInt(num2) <= (int)currentSkillPoints;
		return num2;
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		UnityEngine.Debug.Log(">>>>>Saving attributes");
		foreach (int i in Enum.GetValues(typeof(statsNames)))
		{
			data[(statsNames)i] = allStats[i];
			data[string.Concat((statsNames)i, lvlSufix)] = attributeLevels[i];
		}
		data["heldSkillPoints"] = currentSkillPoints;
		data["spentSkillPoints"] = spentSkillpoints;
		data["timesReset"] = _timesReset;
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		UnityEngine.Debug.Log("Loading attributes<<<<<<<");
		foreach (int i in Enum.GetValues(typeof(statsNames)))
		{
			statsNames stat = (statsNames)i;
			switch (stat)
			{
			case statsNames.statAttack:
				_currentAttackMod = Dot.Single(stat.ToString(), data, SPData.baseAttackModifier);
				attributeLevels[i] = Dot.Integer(stat.ToString() + lvlSufix, data, 1);
				break;
			case statsNames.statHealth:
				_currentHealthMod = Dot.Single(stat.ToString(), data, SPData.baseHealthModifier);
				attributeLevels[i] = Dot.Integer(stat.ToString() + lvlSufix, data, 1);
				break;
			case statsNames.statArmor:
				_currentArmorMod = Dot.Single(stat.ToString(), data, SPData.baseArmorModifier);
				attributeLevels[i] = Dot.Integer(stat.ToString() + lvlSufix, data, 1);
				break;
			case statsNames.statJumpHeight:
				_currentJumpHeightMod = Dot.Single(stat.ToString(), data, SPData.baseJumpHeightModifier);
				attributeLevels[i] = Dot.Integer(stat.ToString() + lvlSufix, data, 1);
				break;
			case statsNames.statSpeed:
				_currentSpeedMod = Dot.Single(stat.ToString(), data, SPData.baseSpeedModifier);
				attributeLevels[i] = Dot.Integer(stat.ToString() + lvlSufix, data, 1);
				break;
			default:
				UnityEngine.Debug.LogError("unknown stat type");
				break;
			}
		}
		currentSkillPoints = Dot.Integer("heldSkillPoints", data, -1);
		spentSkillpoints = Dot.Integer("spentSkillPoints", data, 0);
		timesReset = Dot.Integer("timesReset", data, 0);
		StartCoroutine(CheckSkillPoints());
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		yield break;
	}

	private IEnumerator CheckSkillPoints()
	{
		while ((int)currentSkillPoints != -1)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(0.5f);
		currentSkillPoints = SPData.InitialSkillPoints * Profile.Instance.Level;
	}

	public void debugSave()
	{
		Hub.Instance.SaveLoadManager.Save(this);
	}

	public void ResetAllSkills()
	{
		foreach (int value in Enum.GetValues(typeof(statsNames)))
		{
			switch ((statsNames)value)
			{
			case statsNames.statAttack:
				_currentAttackMod = SPData.baseAttackModifier;
				break;
			case statsNames.statHealth:
				_currentHealthMod = SPData.baseHealthModifier;
				break;
			case statsNames.statArmor:
				_currentArmorMod = SPData.baseArmorModifier;
				break;
			case statsNames.statJumpHeight:
				_currentJumpHeightMod = SPData.baseJumpHeightModifier;
				break;
			case statsNames.statSpeed:
				_currentSpeedMod = SPData.baseSpeedModifier;
				break;
			default:
				UnityEngine.Debug.LogError("unknown stat type");
				break;
			}
			attributeLevels[value] = 1;
		}
		AddSkillPoints(spentSkillpoints);
		spentSkillpoints = 0;
		timesReset++;
		Hub.Instance.SaveLoadManager.Save(this);
	}

	public void DebugMaxAttributes()
	{
		for (int i = 1; i < 99; i++)
		{
			foreach (int value in Enum.GetValues(typeof(statsNames)))
			{
				IncrementStat((statsNames)value);
			}
		}
	}

	private void OnGUI()
	{
		if (!DebugOn)
		{
			return;
		}
		GUILayout.BeginArea(new Rect(300f, 100f, 450f, 500f));
		GUILayout.BeginVertical("Box");
		GUILayout.Label("Current SkillPoints = " + currentSkillPoints);
		foreach (int value in Enum.GetValues(typeof(statsNames)))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(((statsNames)value).ToString() + " = " + Mathf.Round(allStats[value]) + " at lvl: " + attributeLevels[value]);
			bool CanAfford;
			int num2 = (int)Mathf.Round(GetSkillUpgradeCost((statsNames)value, out CanAfford));
			if (GUILayout.Button("UpgradeSkill = " + num2))
			{
				if (CanAfford)
				{
					IncrementStat((statsNames)value);
				}
				else
				{
					UnityEngine.Debug.LogError("not enough skill points");
				}
			}
			GUILayout.EndHorizontal();
		}
		if (GUILayout.Button("Reset Skills"))
		{
			ResetAllSkills();
		}
		GUILayout.Label("total Spent SkillPoints: " + spentSkillpoints);
		if (GUILayout.Button("DEBUG MAX ATTR"))
		{
			DebugMaxAttributes();
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
