using System.Collections.Generic;
using EB;
using UnityEngine;

public class AudioEmitter : MonoBehaviour
{
	public enum PlayMode
	{
		Interrupt = 0,
		Stack = 1,
		Breadcrumb = 2,
		Orphan = 3
	}

	public enum FadeState
	{
		None = 0,
		In = 1,
		Out = 2
	}

	private class AudioSourceTracker
	{
		public AudioEvent m_AudioEvent;

		public AudioSource m_AudioSource;

		public float m_NativeVolumeScalar = 1f;

		public float m_NativePitchScalar = 1f;

		public Timer m_FadeTimer = new Timer(0f, true);

		public FadeState m_FadeState;
	}

	private const string s_ClonedAudioSourceGameObjectNameSuffix = "_AudioEmitter";

	public bool m_PlayAtStart;

	public AudioEvent m_AudioEvent;

	public PlayMode m_PlayMode;

	[HideInInspector]
	public float m_InitialVolumeDB;

	[HideInInspector]
	public float m_InitialPitchST;

	private float m_VolumeScalar = 1f;

	private float m_PitchScalar = 1f;

	private List<AudioSourceTracker> m_AudioSourceTrackers = new List<AudioSourceTracker>();

	private Timer m_FadeTimer;

	private FadeState m_FadeState;

	private bool m_IsMuted;

	public void Awake()
	{
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.volume = 1f;
		AddAudioSourceTracker(audioSource);
		m_FadeTimer = new Timer(0f, true);
		m_VolumeScalar = Utilities.VolumeDecibelsToScalar(m_InitialVolumeDB);
		m_PitchScalar = Utilities.PitchSemitonesToScalar(m_InitialPitchST);
	}

	public void Start()
	{
		if (m_PlayAtStart)
		{
			Play();
		}
	}

	public void OnDestroy()
	{
		for (int i = 1; i < m_AudioSourceTrackers.Count; i++)
		{
			AudioSourceTracker audioSourceTracker = m_AudioSourceTrackers[i];
			if (null != audioSourceTracker.m_AudioSource)
			{
				Object.Destroy(audioSourceTracker.m_AudioSource.gameObject);
			}
		}
		m_AudioSourceTrackers.Clear();
	}

	public void Update()
	{
		UpdateAudioSources();
	}

	public void Update(float TimeDelta)
	{
		UpdateAudioSources(TimeDelta);
	}

	public void SetVolumeScalar(float VolumeScalar)
	{
		m_VolumeScalar = Mathf.Clamp(VolumeScalar, 0.0001f, 1f);
	}

	public void SetVolumeDB(float VolumeDB)
	{
		SetVolumeScalar(Utilities.VolumeDecibelsToScalar(VolumeDB));
	}

	public void SetPitchScalar(float PitchScalar)
	{
		m_PitchScalar = Mathf.Clamp(PitchScalar, 0.0625f, 16f);
	}

	public void SetPitchST(float PitchST)
	{
		SetPitchScalar(Utilities.PitchSemitonesToScalar(PitchST));
	}

	public bool Play()
	{
		return SelectAndPlayAudioClip();
	}

	public bool Play(AudioEvent NewAudioEvent)
	{
		m_AudioEvent = NewAudioEvent;
		return Play();
	}

	public bool Stop(bool OnlyLoops, bool AllowNativeFadeOut)
	{
		foreach (AudioSourceTracker audioSourceTracker in m_AudioSourceTrackers)
		{
			if (null != audioSourceTracker.m_AudioSource && (!OnlyLoops || audioSourceTracker.m_AudioSource.loop))
			{
				if (null != audioSourceTracker.m_AudioEvent && AllowNativeFadeOut && audioSourceTracker.m_AudioEvent.m_FadeOutTime > 0f)
				{
					SetFade(audioSourceTracker.m_FadeTimer, ref audioSourceTracker.m_FadeState, FadeState.Out, audioSourceTracker.m_AudioEvent.m_FadeOutTime);
				}
				else
				{
					audioSourceTracker.m_AudioSource.Stop();
				}
			}
		}
		return true;
	}

	public bool Stop(bool OnlyLoops)
	{
		return Stop(OnlyLoops, true);
	}

	public bool Stop()
	{
		return Stop(false, true);
	}

	public bool FadeIn(float FadeTime)
	{
		SetFade(m_FadeTimer, ref m_FadeState, FadeState.In, FadeTime);
		UpdateAudioSources();
		return true;
	}

	public bool FadeOut(float FadeTime)
	{
		SetFade(m_FadeTimer, ref m_FadeState, FadeState.Out, FadeTime);
		UpdateAudioSources();
		return true;
	}

	public bool GetIsPlaying(AudioEvent TestAudioEvent)
	{
		foreach (AudioSourceTracker audioSourceTracker in m_AudioSourceTrackers)
		{
			if (null != audioSourceTracker.m_AudioSource && (AudioListener.pause || audioSourceTracker.m_AudioSource.isPlaying) && (null == TestAudioEvent || (null != audioSourceTracker.m_AudioEvent && TestAudioEvent.GetInstanceID() == audioSourceTracker.m_AudioEvent.GetInstanceID())))
			{
				return true;
			}
		}
		return false;
	}

	public bool GetIsPlaying()
	{
		return GetIsPlaying(null);
	}

	public void Mute(bool DoMute)
	{
		foreach (AudioSourceTracker audioSourceTracker in m_AudioSourceTrackers)
		{
			audioSourceTracker.m_AudioSource.mute = DoMute;
		}
		m_IsMuted = DoMute;
	}

	private AudioSourceTracker AddAudioSourceTracker(AudioSource AudioSourceToTrack)
	{
		AudioSourceTracker audioSourceTracker = new AudioSourceTracker();
		audioSourceTracker.m_AudioSource = AudioSourceToTrack;
		audioSourceTracker.m_NativeVolumeScalar = AudioSourceToTrack.volume;
		audioSourceTracker.m_NativePitchScalar = AudioSourceToTrack.pitch;
		m_AudioSourceTrackers.Add(audioSourceTracker);
		return audioSourceTracker;
	}

	private void RemoveAudioSourceTracker(int AudioSourceTrackerIndex)
	{
		m_AudioSourceTrackers.RemoveAt(AudioSourceTrackerIndex);
	}

	private void SetFade(Timer FadeTimer, ref FadeState CurrentFadeState, FadeState NewFadeState, float FadeTime)
	{
		if (NewFadeState == FadeState.None)
		{
			FadeTimer.Elapse();
			CurrentFadeState = FadeState.None;
			return;
		}
		FadeTime = Mathf.Max(FadeTime, 0.0001f);
		float length = FadeTimer.GetLength();
		float elapsed = FadeTimer.GetElapsed();
		float remaining = FadeTimer.GetRemaining();
		FadeTimer.SetLength(FadeTime);
		switch (NewFadeState)
		{
		case FadeState.In:
			if (CurrentFadeState == FadeState.Out)
			{
				FadeTimer.SetRemaining(elapsed / length * FadeTime);
			}
			else if (CurrentFadeState == FadeState.In)
			{
				FadeTimer.SetRemaining(remaining / length * FadeTime);
			}
			else
			{
				FadeTimer.Reset();
			}
			CurrentFadeState = FadeState.In;
			break;
		case FadeState.Out:
			if (CurrentFadeState == FadeState.In)
			{
				FadeTimer.SetRemaining(elapsed / length * FadeTime);
			}
			else if (CurrentFadeState == FadeState.Out)
			{
				FadeTimer.SetRemaining(remaining / length * FadeTime);
			}
			else
			{
				FadeTimer.Reset();
			}
			CurrentFadeState = FadeState.Out;
			break;
		}
	}

	private bool SelectAndPlayAudioClip()
	{
		bool flag = false;
		if (null != m_AudioEvent && m_AudioEvent.GetHasAudioClips())
		{
			AudioSource audioSource = null;
			AudioSourceTracker audioSourceTracker = null;
			switch (m_PlayMode)
			{
			case PlayMode.Interrupt:
				audioSourceTracker = m_AudioSourceTrackers[0];
				audioSourceTracker.m_AudioSource.Stop();
				audioSourceTracker.m_FadeState = FadeState.None;
				audioSource = audioSourceTracker.m_AudioSource;
				if (!m_AudioEvent.PrimeAudioSource(audioSource))
				{
					EB.Debug.LogError(base.name + ":  Failed to prime AudioSource using AudioEvent " + m_AudioEvent.name);
				}
				else
				{
					flag = true;
				}
				audioSourceTracker.m_NativeVolumeScalar = audioSource.volume;
				audioSourceTracker.m_NativePitchScalar = audioSource.pitch;
				break;
			case PlayMode.Stack:
			case PlayMode.Breadcrumb:
			case PlayMode.Orphan:
			{
				GameObject gameObject = new GameObject(base.gameObject.name + "_AudioEmitter");
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.volume = 1f;
				if (!m_AudioEvent.PrimeAudioSource(audioSource))
				{
					EB.Debug.LogError(base.name + ":  Failed to prime AudioSource using AudioEvent " + m_AudioEvent.name);
					Object.Destroy(gameObject);
					break;
				}
				if (null == audioSource.clip)
				{
					Object.Destroy(gameObject);
					break;
				}
				if (m_PlayMode != PlayMode.Orphan)
				{
					audioSourceTracker = AddAudioSourceTracker(audioSource);
				}
				if (m_PlayMode == PlayMode.Stack)
				{
					gameObject.transform.parent = base.gameObject.transform;
					gameObject.transform.localPosition = Vector3.zero;
				}
				else
				{
					gameObject.transform.Translate(base.transform.position, Space.World);
				}
				flag = true;
				break;
			}
			default:
				EB.Debug.LogError(string.Concat("Invalid SourceMode ", m_PlayMode, " !"));
				break;
			}
			if (flag && null != audioSource.clip)
			{
				if (m_PlayMode == PlayMode.Orphan)
				{
					AudioSource audioSource2 = audioSource;
					float volume = audioSource2.volume;
					float num = (audioSource.volume = m_VolumeScalar);
					audioSource2.volume = volume * num;
					audioSource.Play();
					Object.Destroy(audioSource.gameObject, audioSource.clip.length);
				}
				else
				{
					audioSourceTracker.m_AudioEvent = m_AudioEvent;
					if (m_AudioEvent.m_FadeInTime > 0f)
					{
						SetFade(audioSourceTracker.m_FadeTimer, ref audioSourceTracker.m_FadeState, FadeState.In, m_AudioEvent.m_FadeInTime);
					}
					else
					{
						SetFade(audioSourceTracker.m_FadeTimer, ref audioSourceTracker.m_FadeState, FadeState.None, m_AudioEvent.m_FadeInTime);
					}
					float fadeVolumeScalar = GetFadeVolumeScalar(m_FadeState, m_FadeTimer);
					float fadeVolumeScalar2 = GetFadeVolumeScalar(audioSourceTracker.m_FadeState, audioSourceTracker.m_FadeTimer);
					audioSource.volume = m_VolumeScalar * audioSourceTracker.m_NativeVolumeScalar * audioSourceTracker.m_AudioEvent.GetAudioCategoryVolumeScalar() * fadeVolumeScalar * fadeVolumeScalar2;
					audioSource.pitch = m_PitchScalar * audioSourceTracker.m_NativePitchScalar;
					audioSource.mute = m_IsMuted;
					audioSource.Play();
				}
			}
		}
		return flag;
	}

	private static float GetFadeVolumeScalar(FadeState FadeState, Timer FadeTimer)
	{
		float result = 1f;
		switch (FadeState)
		{
		case FadeState.None:
			result = 1f;
			break;
		case FadeState.In:
			result = FadeTimer.GetElapsedRatio();
			break;
		case FadeState.Out:
			result = FadeTimer.GetRemainingRatio();
			break;
		}
		return result;
	}

	private void HousecleanAudioSources()
	{
		for (int num = m_AudioSourceTrackers.Count - 1; num > 0; num--)
		{
			AudioSourceTracker audioSourceTracker = m_AudioSourceTrackers[num];
			if (null != audioSourceTracker.m_AudioSource && !AudioListener.pause && !audioSourceTracker.m_AudioSource.isPlaying)
			{
				Object.Destroy(audioSourceTracker.m_AudioSource.gameObject);
				audioSourceTracker.m_AudioSource = null;
			}
			if (null == audioSourceTracker.m_AudioSource)
			{
				RemoveAudioSourceTracker(num);
			}
		}
	}

	private void UpdateAudioSources(float TimeDelta)
	{
		HousecleanAudioSources();
		m_FadeTimer.Update(TimeDelta);
		float fadeVolumeScalar = GetFadeVolumeScalar(m_FadeState, m_FadeTimer);
		foreach (AudioSourceTracker audioSourceTracker in m_AudioSourceTrackers)
		{
			audioSourceTracker.m_FadeTimer.Update(TimeDelta);
			float fadeVolumeScalar2 = GetFadeVolumeScalar(audioSourceTracker.m_FadeState, audioSourceTracker.m_FadeTimer);
			audioSourceTracker.m_AudioSource.volume = m_VolumeScalar * audioSourceTracker.m_NativeVolumeScalar * fadeVolumeScalar * fadeVolumeScalar2;
			if (null != audioSourceTracker.m_AudioEvent)
			{
				audioSourceTracker.m_AudioSource.volume *= audioSourceTracker.m_AudioEvent.GetAudioCategoryVolumeScalar();
			}
			audioSourceTracker.m_AudioSource.mute = m_IsMuted;
			if (audioSourceTracker.m_FadeState == FadeState.Out && audioSourceTracker.m_FadeTimer.GetIsElapsed())
			{
				audioSourceTracker.m_AudioSource.Stop();
				audioSourceTracker.m_FadeState = FadeState.None;
			}
		}
		if (m_FadeState == FadeState.Out && m_FadeTimer.GetIsElapsed())
		{
			Stop(false, false);
			m_FadeState = FadeState.None;
		}
	}

	private void UpdateAudioSources()
	{
		UpdateAudioSources(UnityEngine.Time.deltaTime);
	}

	private void OnClick()
	{
		Play();
	}
}
