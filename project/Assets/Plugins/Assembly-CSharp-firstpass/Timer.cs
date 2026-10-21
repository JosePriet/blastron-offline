using UnityEngine;

public class Timer
{
	private float m_Length;

	private float m_Remaining;

	public Timer(float Length, bool BeginElapsed)
	{
		m_Length = Mathf.Max(Length, 0f);
		if (BeginElapsed)
		{
			Elapse();
		}
		else
		{
			Reset();
		}
	}

	public float GetLength()
	{
		return m_Length;
	}

	public void SetLength(float Length)
	{
		m_Length = Mathf.Max(Length, 0f);
	}

	public float GetRemaining()
	{
		return m_Remaining;
	}

	public float GetRemainingRatio()
	{
		if (Utilities.AreFloatsEqual(0f, m_Length))
		{
			return float.PositiveInfinity;
		}
		return GetRemaining() / GetLength();
	}

	public float GetElapsed()
	{
		return m_Length - m_Remaining;
	}

	public float GetElapsedRatio()
	{
		if (Utilities.AreFloatsEqual(0f, m_Length))
		{
			return float.PositiveInfinity;
		}
		return GetElapsed() / GetLength();
	}

	public void SetRemaining(float Remaining)
	{
		m_Remaining = Mathf.Max(Remaining, 0f);
	}

	public bool GetIsElapsed()
	{
		return m_Remaining <= 0f;
	}

	public bool GetIsRunning()
	{
		return !GetIsElapsed();
	}

	public void Reset()
	{
		m_Remaining = m_Length;
	}

	public void Elapse()
	{
		m_Remaining = 0f;
	}

	public void Update(float Delta)
	{
		if (!GetIsElapsed())
		{
			m_Remaining = Mathf.Max(m_Remaining - Delta, 0f);
		}
	}

	public void Update()
	{
		Update(Time.deltaTime);
	}
}
