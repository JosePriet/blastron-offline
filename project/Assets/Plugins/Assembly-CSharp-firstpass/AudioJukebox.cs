using UnityEngine;

public class AudioJukebox : MonoBehaviour
{
	public AudioEvent m_AudioEvent;

	public float m_FadeInTime = 1f;

	public float m_FadeOutTime = 1f;

	public float m_CycleTime = 60f;

	public bool m_IsForMusic = true;

	private AudioEvent m_CurrentAudioEvent;

	private AudioEmitter[] m_AudioEmitters = new AudioEmitter[2];

	private int m_CurrentAudioEmitterID;

	private Timer m_CycleTimer;

	public void Awake()
	{
		GlobalSettings.SetUpGlobalGameSettingsGameObject();
		m_CurrentAudioEvent = m_AudioEvent;
		m_AudioEmitters[0] = base.gameObject.AddComponent<AudioEmitter>();
		m_AudioEmitters[0].m_AudioEvent = m_AudioEvent;
		m_AudioEmitters[1] = base.gameObject.AddComponent<AudioEmitter>();
		m_AudioEmitters[1].m_AudioEvent = m_AudioEvent;
		m_CurrentAudioEmitterID = 0;
		m_CycleTimer = new Timer(m_CycleTime, true);
	}

	public void Start()
	{
		if (m_IsForMusic)
		{
			bool Argument = false;
			Messenger<bool>.Broadcast(GlobalSettings.GetGlobalSettingsGameObject(), "GetIsMusicMuted", ref Argument);
			Mute(Argument);
		}
	}

	public void OnEnable()
	{
		Messenger.AddListener(base.gameObject, "Restart", OnRestart);
		Messenger.AddListener(base.gameObject, "StopMusic", StopMusic);
		Messenger<bool, bool>.AddListener(base.gameObject, "MuteAudioJukebox", OnMuteAudioJukebox);
		Messenger<AudioEvent>.AddListener(base.gameObject, "SetAudioJukeboxAudioEvent", OnSetAudioJukeboxAudioEvent);
	}

	public void OnDisable()
	{
		Messenger.RemoveListener(base.gameObject, "Restart", OnRestart);
		Messenger.RemoveListener(base.gameObject, "StopMusic", StopMusic);
		Messenger<bool, bool>.RemoveListener(base.gameObject, "MuteAudioJukebox", OnMuteAudioJukebox);
		Messenger<AudioEvent>.RemoveListener(base.gameObject, "SetAudioJukeboxAudioEvent", OnSetAudioJukeboxAudioEvent);
	}

	public void Update()
	{
	}

	private void OnRestart()
	{
		SetAudioEvent(m_AudioEvent);
	}

	private void StopMusic()
	{
		Debug.Log("GotStop");
		if (m_IsForMusic)
		{
			Debug.Log("Stopping");
			m_AudioEmitters[0].Stop(true, true);
			m_AudioEmitters[1].Stop(true, true);
		}
	}

	private void OnMuteAudioJukebox(ref bool DoMute, ref bool OnlyIfIsForMusic)
	{
		if (!OnlyIfIsForMusic || m_IsForMusic)
		{
			Mute(DoMute);
		}
	}

	private void OnSetAudioJukeboxAudioEvent(ref AudioEvent AudioEvent)
	{
		SetAudioEvent(AudioEvent);
	}

	private void Mute(bool DoMute)
	{
		m_AudioEmitters[0].Mute(DoMute);
		m_AudioEmitters[1].Mute(DoMute);
	}

	private void SetAudioEvent(AudioEvent AudioEvent)
	{
		if (m_CurrentAudioEvent.GetInstanceID() != AudioEvent.GetInstanceID())
		{
			m_CurrentAudioEvent = AudioEvent;
			m_AudioEmitters[0].m_AudioEvent = AudioEvent;
			m_AudioEmitters[1].m_AudioEvent = AudioEvent;
			PlayNewAudioClip();
			m_CycleTimer.Reset();
		}
	}

	private void PlayNewAudioClip()
	{
		FadeOutCurrentAudioEmitter(m_FadeOutTime);
		m_CurrentAudioEmitterID = ((m_CurrentAudioEmitterID == 0) ? 1 : 0);
		m_AudioEmitters[m_CurrentAudioEmitterID].Play();
		m_AudioEmitters[m_CurrentAudioEmitterID].FadeIn(m_FadeInTime);
	}

	private void FadeOutCurrentAudioEmitter(float FadeTime)
	{
		if (m_AudioEmitters[m_CurrentAudioEmitterID].GetIsPlaying())
		{
			m_AudioEmitters[m_CurrentAudioEmitterID].FadeOut(FadeTime);
		}
	}
}
