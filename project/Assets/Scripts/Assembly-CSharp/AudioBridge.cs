using UnityEngine;

public class AudioBridge : MonoBehaviour
{
	private AudioEmitter _emitterTron;

	public static AudioBridge Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		_emitterTron = base.gameObject.AddComponent<AudioEmitter>();
	}

	public void Play(AudioEvent audioEvent)
	{
		_emitterTron.Play(audioEvent);
	}
}
