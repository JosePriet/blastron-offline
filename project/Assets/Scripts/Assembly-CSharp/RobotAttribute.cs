using System;
using EB;

[System.Serializable]
public class RobotAttribute
{
	public enum RobotAttributeType
	{
		Damage = 0,
		Armor = 1,
		Health = 2,
		Jump = 3,
		Speed = 4
	}

	public RobotAttributeType type;

	public int minValue;

	public int maxValue;

	public float GetCurrentValue(float upgradeProgress)
	{
		if (upgradeProgress < 0f || upgradeProgress > 100f)
		{
			Debug.LogWarning("Questionable upgrade progress of " + upgradeProgress + "!!!");
		}
		float num = upgradeProgress / 100f;
		return (float)(maxValue - minValue) * num + (float)minValue;
	}
}
