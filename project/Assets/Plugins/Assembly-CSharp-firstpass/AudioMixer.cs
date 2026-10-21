using System.Collections.Generic;
using UnityEngine;

public class AudioMixer : MonoBehaviour
{
	private const string s_GameObjectName = "AudioMixer";

	private static GameObject s_AudioMixerGameObject;

	private List<AudioMix> m_AudioMixes = new List<AudioMix>();

	private Dictionary<AudioCategory, AudioMix> m_AudioCategoryTrackers = new Dictionary<AudioCategory, AudioMix>();

	public static void SetUpAudioMixerGameObject()
	{
		if (null == s_AudioMixerGameObject)
		{
			s_AudioMixerGameObject = new GameObject("AudioMixer");
			s_AudioMixerGameObject.AddComponent<Persistent>();
			AudioMixer audioMixer = s_AudioMixerGameObject.AddComponent<AudioMixer>();
			audioMixer.RegisterMessageHandlers();
		}
	}

	public static GameObject GetAudioMixerGameObject()
	{
		if (null == s_AudioMixerGameObject)
		{
			SetUpAudioMixerGameObject();
		}
		return s_AudioMixerGameObject;
	}

	public static bool Exists()
	{
		return null != s_AudioMixerGameObject;
	}

	public void OnDestroy()
	{
		List<AudioMix> list = new List<AudioMix>(m_AudioMixes.Count);
		foreach (AudioMix audioMix in m_AudioMixes)
		{
			list.Add(audioMix);
		}
		foreach (AudioMix item in list)
		{
			DeactivateAudioMix(item, true, false);
		}
		UnregisterMessageHandlers();
	}

	public void Update()
	{
		UpdateAudioCategories();
	}

	public void ReapplyAudioMix(AudioMix AudioMixToReapply)
	{
		if (null != AudioMixToReapply)
		{
			DeactivateAudioMix(AudioMixToReapply, true, false);
			ActivateAudioMix(AudioMixToReapply, false);
		}
	}

	private void RegisterMessageHandlers()
	{
		Messenger<AudioMix>.AddListener(base.gameObject, "ActivateAudioMix", OnActivateAudioMix);
		Messenger<AudioMix, bool>.AddListener(base.gameObject, "DeactivateAudioMix", OnDeactivateAudioMix);
	}

	private void UnregisterMessageHandlers()
	{
		Messenger<AudioMix>.RemoveListener(base.gameObject, "ActivateAudioMix", OnActivateAudioMix);
		Messenger<AudioMix, bool>.RemoveListener(base.gameObject, "DeactivateAudioMix", OnDeactivateAudioMix);
	}

	private void OnActivateAudioMix(ref AudioMix AudioMixToActivate)
	{
		ActivateAudioMix(AudioMixToActivate, true);
	}

	private void OnDeactivateAudioMix(ref AudioMix AudioMixToDeactivate, ref bool Force)
	{
		DeactivateAudioMix(AudioMixToDeactivate, Force, true);
	}

	private void RegisterAudioCategory(AudioCategory AudioCategoryToRegister)
	{
		if (!m_AudioCategoryTrackers.ContainsKey(AudioCategoryToRegister))
		{
			m_AudioCategoryTrackers.Add(AudioCategoryToRegister, null);
		}
	}

	private void RegisterAudioMix(AudioMix AudioMixToRegister)
	{
		if (null != AudioMixToRegister && !m_AudioMixes.Contains(AudioMixToRegister))
		{
			m_AudioMixes.Add(AudioMixToRegister);
			AudioCategory[] audioCategories = AudioMixToRegister.m_AudioCategories;
			foreach (AudioCategory audioCategoryToRegister in audioCategories)
			{
				RegisterAudioCategory(audioCategoryToRegister);
			}
		}
	}

	private void UnregisterAudioMix(AudioMix AudioMixToUnregister)
	{
		if (null != AudioMixToUnregister)
		{
			m_AudioMixes.Remove(AudioMixToUnregister);
		}
	}

	private void ActivateAudioMix(AudioMix AudioMixToActivate, bool UseTransitionTime)
	{
		RegisterAudioMix(AudioMixToActivate);
		if (!(null != AudioMixToActivate))
		{
			return;
		}
		bool isActive = AudioMixToActivate.GetIsActive();
		AudioMixToActivate.Activate();
		if (isActive || !AudioMixToActivate.GetIsActive())
		{
			return;
		}
		float transitionTime = ((!UseTransitionTime) ? 0f : AudioMixToActivate.m_FadeInTime);
		AudioCategory[] audioCategories = AudioMixToActivate.m_AudioCategories;
		foreach (AudioCategory audioCategory in audioCategories)
		{
			AudioMix audioMix = m_AudioCategoryTrackers[audioCategory];
			if (null == audioMix || AudioMixToActivate.m_Priority < audioMix.m_Priority)
			{
				audioCategory.SetVolumeScalar(Utilities.VolumeDecibelsToScalar(AudioMixToActivate.m_VolumeDB), transitionTime);
				m_AudioCategoryTrackers[audioCategory] = AudioMixToActivate;
			}
		}
	}

	private void DeactivateAudioMix(AudioMix AudioMixToDeactivate, bool Force, bool UseTransitionTime)
	{
		if (!(null != AudioMixToDeactivate))
		{
			return;
		}
		bool isActive = AudioMixToDeactivate.GetIsActive();
		AudioMixToDeactivate.Deactivate(Force);
		if (!isActive || AudioMixToDeactivate.GetIsActive())
		{
			return;
		}
		float transitionTime = ((!UseTransitionTime) ? 0f : AudioMixToDeactivate.m_FadeOutTime);
		List<AudioCategory> list = new List<AudioCategory>();
		foreach (KeyValuePair<AudioCategory, AudioMix> audioCategoryTracker in m_AudioCategoryTrackers)
		{
			if (audioCategoryTracker.Value == AudioMixToDeactivate)
			{
				list.Add(audioCategoryTracker.Key);
			}
		}
		foreach (AudioCategory item in list)
		{
			RemoveAudioMixForAudioCategory(item, transitionTime);
		}
		UnregisterAudioMix(AudioMixToDeactivate);
	}

	private void RemoveAudioMixForAudioCategory(AudioCategory TargetAudioCategory, float TransitionTime)
	{
		AudioMix highestPriorityActiveAudioMixForAudioCategory = GetHighestPriorityActiveAudioMixForAudioCategory(TargetAudioCategory);
		if (null != highestPriorityActiveAudioMixForAudioCategory)
		{
			TargetAudioCategory.SetVolumeScalar(Utilities.VolumeDecibelsToScalar(highestPriorityActiveAudioMixForAudioCategory.m_VolumeDB), TransitionTime);
		}
		else
		{
			TargetAudioCategory.SetVolumeScalar(Utilities.VolumeDecibelsToScalar(TargetAudioCategory.m_VolumeDB), TransitionTime);
		}
		m_AudioCategoryTrackers[TargetAudioCategory] = highestPriorityActiveAudioMixForAudioCategory;
	}

	private AudioMix GetHighestPriorityActiveAudioMixForAudioCategory(AudioCategory TargetAudioCategory)
	{
		AudioMix audioMix = null;
		foreach (AudioMix audioMix2 in m_AudioMixes)
		{
			if (!audioMix2.GetIsActive())
			{
				continue;
			}
			AudioCategory[] audioCategories = audioMix2.m_AudioCategories;
			foreach (AudioCategory audioCategory in audioCategories)
			{
				if (audioCategory == TargetAudioCategory)
				{
					if (null == audioMix || audioMix2.m_Priority < audioMix.m_Priority)
					{
						audioMix = audioMix2;
					}
					break;
				}
			}
		}
		return audioMix;
	}

	private void UpdateAudioCategories()
	{
		foreach (KeyValuePair<AudioCategory, AudioMix> audioCategoryTracker in m_AudioCategoryTrackers)
		{
			audioCategoryTracker.Key.UpdateLerpers(Time.deltaTime);
		}
	}
}
