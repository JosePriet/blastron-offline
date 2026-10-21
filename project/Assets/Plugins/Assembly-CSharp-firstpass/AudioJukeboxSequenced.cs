using System;
using EB;
using UnityEngine;

public class AudioJukeboxSequenced : MonoBehaviour
{
	[System.Serializable]
	public class AudioJukeboxSequenceNode
	{
		public AudioEvent m_AudioEvent;

		public float m_Duration;
	}

	public AudioJukeboxSequenceNode[] m_AudioJukeboxSequenceNodes;

	public bool m_IsForMusic = true;

	private AudioEmitter m_AudioEmitter;

	private int m_CurrentAudioJukeboxSequenceNodeIndex = -1;

	private Timer m_Timer;

	public void Awake()
	{
		GlobalSettings.SetUpGlobalGameSettingsGameObject();
		m_AudioEmitter = base.gameObject.AddComponent<AudioEmitter>();
		m_AudioEmitter.m_PlayAtStart = false;
		m_AudioEmitter.m_PlayMode = AudioEmitter.PlayMode.Stack;
		m_Timer = new Timer(0f, true);
	}

	public void Start()
	{
		if (m_IsForMusic)
		{
			bool Argument = Options.sound == 0;
			Messenger<bool>.Broadcast(GlobalSettings.GetGlobalSettingsGameObject(), "GetIsMusicMuted", ref Argument);
			Mute(Argument);
		}
		Restart(false);
	}

	public void OnEnable()
	{
		Messenger.AddListener(base.gameObject, "StopMusic", StopMusic);
		Messenger<bool>.AddListener(base.gameObject, "Restart", OnRestart);
		Messenger.AddListener(base.gameObject, "AudioJukeboxAdvance", OnAudioJukeboxAdvance);
		Messenger<bool, bool>.AddListener(base.gameObject, "MuteAudioJukebox", OnMuteAudioJukebox);
	}

	public void OnDisable()
	{
		Messenger.RemoveListener(base.gameObject, "StopMusic", StopMusic);
		Messenger<bool>.RemoveListener(base.gameObject, "Restart", OnRestart);
		Messenger.RemoveListener(base.gameObject, "AudioJukeboxAdvance", OnAudioJukeboxAdvance);
		Messenger<bool, bool>.RemoveListener(base.gameObject, "MuteAudioJukebox", OnMuteAudioJukebox);
	}

	public void Update()
	{
		if (!m_Timer.GetIsElapsed())
		{
			m_Timer.Update();
			if (m_Timer.GetIsElapsed())
			{
				Advance();
			}
		}
		else if (m_CurrentAudioJukeboxSequenceNodeIndex < m_AudioJukeboxSequenceNodes.Length && !m_AudioEmitter.GetIsPlaying(m_AudioJukeboxSequenceNodes[m_CurrentAudioJukeboxSequenceNodeIndex].m_AudioEvent))
		{
			Advance();
		}
	}

	private void OnRestart(ref bool musicOnly)
	{
		Restart(musicOnly);
	}

	private void OnAudioJukeboxAdvance()
	{
		Advance();
	}

	private void StopMusic()
	{
		if (m_IsForMusic)
		{
			m_AudioEmitter.Stop(true, false);
		}
	}

	private void OnMuteAudioJukebox(ref bool DoMute, ref bool OnlyIfIsForMusic)
	{
		if (!OnlyIfIsForMusic || m_IsForMusic)
		{
			Mute(DoMute);
		}
	}

	private void Restart(bool musicOnly)
	{
		if (!musicOnly || m_IsForMusic)
		{
			int nextNodeIndex = GetNextNodeIndex(-1);
			if (nextNodeIndex != m_CurrentAudioJukeboxSequenceNodeIndex)
			{
				m_CurrentAudioJukeboxSequenceNodeIndex = -1;
				Advance();
			}
		}
	}

	private void Mute(bool DoMute)
	{
		m_AudioEmitter.Mute(DoMute);
	}

	private int GetNextNodeIndex(int CurrentNodeIndex)
	{
		do
		{
			CurrentNodeIndex++;
		}
		while (CurrentNodeIndex < m_AudioJukeboxSequenceNodes.Length && null == m_AudioJukeboxSequenceNodes[CurrentNodeIndex].m_AudioEvent);
		return CurrentNodeIndex;
	}

	private void Advance()
	{
		if (m_CurrentAudioJukeboxSequenceNodeIndex >= m_AudioJukeboxSequenceNodes.Length)
		{
			return;
		}
		m_CurrentAudioJukeboxSequenceNodeIndex = GetNextNodeIndex(m_CurrentAudioJukeboxSequenceNodeIndex);
		if (m_CurrentAudioJukeboxSequenceNodeIndex < m_AudioJukeboxSequenceNodes.Length)
		{
			m_AudioEmitter.m_AudioEvent = m_AudioJukeboxSequenceNodes[m_CurrentAudioJukeboxSequenceNodeIndex].m_AudioEvent;
			m_Timer.Elapse();
			if (m_AudioJukeboxSequenceNodes[m_CurrentAudioJukeboxSequenceNodeIndex].m_Duration > 0f)
			{
				m_Timer.SetLength(m_AudioJukeboxSequenceNodes[m_CurrentAudioJukeboxSequenceNodeIndex].m_Duration);
				m_Timer.Reset();
			}
			m_AudioEmitter.Stop(false, true);
			m_AudioEmitter.Play();
		}
	}
}
