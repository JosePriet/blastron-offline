using UnityEngine;

public class AudioMix : MonoBehaviour
{
	public bool m_ActivateAtStart = true;

	public AudioCategory[] m_AudioCategories;

	[HideInInspector]
	public float m_VolumeDB;

	[HideInInspector]
	public int m_Priority = 128;

	public float m_FadeInTime;

	public float m_FadeOutTime;

	private int m_ReferenceCount;

	public bool ActivateAtStart
	{
		get
		{
			return m_ActivateAtStart;
		}
		set
		{
			m_ActivateAtStart = value;
		}
	}

	public AudioCategory[] AudioCategories
	{
		get
		{
			return m_AudioCategories;
		}
		set
		{
			m_AudioCategories = value;
		}
	}

	public float VolumeDB
	{
		get
		{
			return m_VolumeDB;
		}
		set
		{
			m_VolumeDB = value;
		}
	}

	public int Priority
	{
		get
		{
			return m_Priority;
		}
		set
		{
			m_Priority = value;
		}
	}

	public float FadeInTime
	{
		get
		{
			return m_FadeInTime;
		}
		set
		{
			m_FadeInTime = value;
		}
	}

	public float FadeOutTime
	{
		get
		{
			return m_FadeOutTime;
		}
		set
		{
			m_FadeOutTime = value;
		}
	}

	public void Awake()
	{
		AudioMixer.SetUpAudioMixerGameObject();
		m_Priority = Mathf.Clamp(m_Priority, 0, 255);
		m_FadeInTime = Mathf.Max(m_FadeInTime, 0f, m_FadeInTime);
		m_FadeOutTime = Mathf.Max(m_FadeOutTime, 0f, m_FadeOutTime);
	}

	public void Start()
	{
		if (m_ActivateAtStart)
		{
			AudioMix Argument = this;
			Messenger<AudioMix>.Broadcast(AudioMixer.GetAudioMixerGameObject(), "ActivateAudioMix", ref Argument);
		}
	}

	public void OnDestroy()
	{
		if (AudioMixer.Exists())
		{
			AudioMix Argument = this;
			bool Argument2 = true;
			Messenger<AudioMix, bool>.Broadcast(AudioMixer.GetAudioMixerGameObject(), "DeactivateAudioMix", ref Argument, ref Argument2);
		}
	}

	public void Activate()
	{
		m_ReferenceCount++;
	}

	public void Deactivate(bool Force)
	{
		if (Force)
		{
			m_ReferenceCount = 1;
		}
		if (m_ReferenceCount > 0)
		{
			m_ReferenceCount--;
		}
	}

	public bool GetIsActive()
	{
		return m_ReferenceCount > 0;
	}
}
