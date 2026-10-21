using EB;
using UnityEngine;

public class AudioEvent : MonoBehaviour
{
	public enum SelectMode
	{
		Sequential = 0,
		Random = 1,
		RandomNoRepeat = 2
	}

	public AudioClip[] m_AudioClips;

	public SelectMode m_SelectMode = SelectMode.RandomNoRepeat;

	public AudioCategory m_AudioCategory;

	[HideInInspector]
	public int m_ChanceToNotPlay;

	[HideInInspector]
	public float m_VolumeDB;

	[HideInInspector]
	public float m_VolumeDBRandomDelta;

	[HideInInspector]
	public float m_FadeInTime;

	[HideInInspector]
	public float m_FadeOutTime;

	[HideInInspector]
	public float m_PitchST;

	[HideInInspector]
	public float m_PitchSTRandomDelta;

	public bool m_Loop;

	[HideInInspector]
	public int m_Priority = 128;

	[HideInInspector]
	public float m_DopplerLevel;

	[HideInInspector]
	public float m_MinDistance = 1f;

	[HideInInspector]
	public float m_MaxDistance = 10f;

	public AudioRolloffMode m_RolloffMode;

	private int m_LastPlayedAudioClipIndex = -1;

	public bool GetHasAudioClips()
	{
		return m_AudioClips != null && 0 != m_AudioClips.Length;
	}

	public bool PrimeAudioSource(AudioSource AudioSource)
	{
		if (!GetHasAudioClips())
		{
			EB.Debug.LogError(base.name + ": No AudioClips to play!");
			return false;
		}
		if (null == m_AudioCategory)
		{
			EB.Debug.LogError(base.name + ": No AudioCategory assigned!");
			return false;
		}
		AudioSource.clip = null;
		if (m_ChanceToNotPlay == 0 || Random.Range(1, 100) > m_ChanceToNotPlay)
		{
			int num = -1;
			switch (GetSelectMode())
			{
			case SelectMode.Sequential:
				num = (m_LastPlayedAudioClipIndex + 1) % m_AudioClips.Length;
				AudioSource.clip = m_AudioClips[num];
				break;
			case SelectMode.Random:
				num = Random.Range(0, m_AudioClips.Length);
				AudioSource.clip = m_AudioClips[num];
				break;
			case SelectMode.RandomNoRepeat:
			{
				num = m_LastPlayedAudioClipIndex;
				int num2 = 0;
				do
				{
					num = Random.Range(0, m_AudioClips.Length);
					num2++;
				}
				while (num == m_LastPlayedAudioClipIndex && num2 < 32);
				AudioSource.clip = m_AudioClips[num];
				break;
			}
			default:
				EB.Debug.LogError(string.Concat(base.name, ": Invalid SelectMode ", m_SelectMode, " !"));
				return false;
			}
			m_LastPlayedAudioClipIndex = num;
			AudioSource.volume = SelectVolumeScalar();
			AudioSource.pitch = SelectPitchScalar();
			AudioSource.loop = m_Loop;
			AudioSource.priority = m_Priority;
			AudioSource.dopplerLevel = m_DopplerLevel;
			AudioSource.rolloffMode = m_RolloffMode;
			AudioSource.minDistance = m_MinDistance;
			AudioSource.maxDistance = Mathf.Max(m_MaxDistance, m_MinDistance * 1.01f);
		}
		return true;
	}

	public float GetAudioCategoryVolumeScalar()
	{
		if (null == m_AudioCategory)
		{
			return 1f;
		}
		return m_AudioCategory.GetVolumeScalar();
	}

	private SelectMode GetSelectMode()
	{
		SelectMode result = SelectMode.Sequential;
		if (m_AudioClips.Length > 1)
		{
			result = m_SelectMode;
		}
		return result;
	}

	private float SelectVolumeScalar()
	{
		float num = Random.Range(0f - Mathf.Abs(m_VolumeDBRandomDelta), Mathf.Abs(m_VolumeDBRandomDelta));
		return Utilities.VolumeDecibelsToScalar(Mathf.Clamp(m_VolumeDB + num, -80f, 0f));
	}

	private float SelectPitchScalar()
	{
		float num = Random.Range(0f - Mathf.Abs(m_PitchSTRandomDelta), Mathf.Abs(m_PitchSTRandomDelta));
		return Utilities.PitchSemitonesToScalar(Mathf.Clamp(m_PitchST + num, -48f, 48f));
	}
}
