using UnityEngine;

public class AudioLerper
{
	private float m_Value;

	private float m_TargetValue;

	private float m_Slope;

	public void Lerp(float TargetValue, float Duration)
	{
		if (GetIsLerping() || !Utilities.AreFloatsEqual(m_Value, TargetValue))
		{
			Duration = Mathf.Max(Duration, 0f);
			m_TargetValue = TargetValue;
			if (Utilities.AreFloatsEqual(0f, Duration))
			{
				m_Value = TargetValue;
			}
			else
			{
				m_Slope = (TargetValue - m_Value) / Duration;
			}
		}
	}

	public void Update(float TimeDelta)
	{
		if (GetIsLerping())
		{
			m_Value += m_Slope * TimeDelta;
			if (m_Slope > 0f)
			{
				m_Value = Mathf.Min(m_Value, m_TargetValue);
			}
			else
			{
				m_Value = Mathf.Max(m_Value, m_TargetValue);
			}
		}
	}

	public bool GetIsLerping()
	{
		if (!Utilities.AreFloatsEqual(m_Value, m_TargetValue))
		{
			return true;
		}
		return false;
	}

	public float GetValue()
	{
		return m_Value;
	}

	public float GetTargetValue()
	{
		return m_TargetValue;
	}
}
